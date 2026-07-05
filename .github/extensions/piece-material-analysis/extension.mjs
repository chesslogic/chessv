import { execFile } from "node:child_process";
import { access } from "node:fs/promises";
import { promisify } from "node:util";
import { joinSession } from "@github/copilot-sdk/extension";

const execFileAsync = promisify(execFile);

const REPO_ROOT = "C:\\GitHub\\chessv";
const PROJECT_NAME = "ChessV.PieceAnalysis";
const PROJECT_FILE = `${REPO_ROOT}\\${PROJECT_NAME}\\${PROJECT_NAME}.csproj`;
const TOOL_NAME = "assess_piece_material";
const TIER_VALUES = ["Weak", "Pawn", "Minor", "Major", "Jack", "Queen", "Amazon"];
const DEFAULT_TIMEOUT_MS = 180_000;

const session = await joinSession({
    tools: [
        {
            name: TOOL_NAME,
            description:
                "Assess ChessV piece material values via ChessV.PieceAnalysis and summarize tier fit, mobility, and warnings.",
            parameters: {
                type: "object",
                properties: {
                    pieces: {
                        type: "array",
                        description:
                            "Optional piece class names to assess, such as ShortRook, Queen, or Cannon.",
                        items: { type: "string" },
                    },
                    all: {
                        type: "boolean",
                        description:
                            "When true, assess every constructible piece in the engine. Slower than targeted piece analysis.",
                        default: false,
                    },
                    tier: {
                        type: "string",
                        description:
                            "Optional tier hint for candidate pieces not already wired into a known tier.",
                        enum: TIER_VALUES,
                    },
                    files: {
                        type: "integer",
                        description: "Board width in files.",
                        minimum: 1,
                        default: 8,
                    },
                    ranks: {
                        type: "integer",
                        description: "Board height in ranks.",
                        minimum: 1,
                        default: 8,
                    },
                },
                additionalProperties: false,
            },
            skipPermission: true,
            handler: async (args) => {
                await session.log("Running piece material analysis…", { ephemeral: true });

                const normalizedArgs = normalizeArgs(args);
                if (normalizedArgs.error) {
                    return failure(normalizedArgs.error);
                }

                if (!(await pathExists(PROJECT_FILE))) {
                    return failure(
                        `ChessV.PieceAnalysis project was not found at ${PROJECT_FILE}. ` +
                            "The analysis extension loaded correctly, but the console tool does not exist in this worktree yet.",
                    );
                }

                const analysisArgs = buildAnalysisArgs(normalizedArgs);
                const attempts = [
                    {
                        label: "no-build",
                        args: ["run", "--project", PROJECT_NAME, "--no-build", "--", ...analysisArgs],
                    },
                    {
                        label: "build-and-run",
                        args: ["run", "--project", PROJECT_NAME, "--", ...analysisArgs],
                    },
                ];

                const failures = [];
                for (const attempt of attempts) {
                    const result = await runDotnet(attempt.args);
                    if (result.ok) {
                        const parsed = tryParseJson(result.stdout);
                        if (!parsed.ok) {
                            return failure(
                                `ChessV.PieceAnalysis completed, but its stdout was not valid JSON. ` +
                                    `Output snippet: ${clip(result.stdout)}`,
                            );
                        }

                        return {
                            resultType: "success",
                            textResultForLlm: formatAnalysisSummary(parsed.value, normalizedArgs),
                        };
                    }

                    failures.push(
                        `${attempt.label}: ${result.message}${result.stderr ? ` | stderr: ${clip(result.stderr)}` : ""}`,
                    );
                }

                return failure(
                    `Piece material analysis failed after ${attempts.length} attempt(s). ${failures.join(" || ")}`,
                );
            },
        },
    ],
});

function normalizeArgs(args) {
    const raw = isPlainObject(args) ? args : {};
    const pieces = Array.isArray(raw.pieces)
        ? [...new Set(raw.pieces.map((piece) => `${piece ?? ""}`.trim()).filter(Boolean))]
        : [];
    const all = Boolean(raw.all);
    const tier = raw.tier == null ? undefined : `${raw.tier}`.trim();
    const files = normalizePositiveInteger(raw.files, 8);
    const ranks = normalizePositiveInteger(raw.ranks, 8);

    if (tier && !TIER_VALUES.includes(tier)) {
        return { error: `Invalid tier "${tier}". Expected one of: ${TIER_VALUES.join(", ")}.` };
    }

    return { pieces, all, tier, files, ranks };
}

function buildAnalysisArgs({ pieces, all, tier, files, ranks }) {
    const argv = [];

    for (const piece of pieces) {
        argv.push("--piece", piece);
    }

    if (all) {
        argv.push("--all");
    }

    if (tier) {
        argv.push("--tier", tier);
    }

    argv.push("--files", `${files}`, "--ranks", `${ranks}`);
    return argv;
}

async function runDotnet(args) {
    try {
        const { stdout, stderr } = await execFileAsync("dotnet", args, {
            cwd: REPO_ROOT,
            timeout: DEFAULT_TIMEOUT_MS,
            maxBuffer: 10 * 1024 * 1024,
            windowsHide: true,
        });
        return { ok: true, stdout, stderr };
    } catch (error) {
        return {
            ok: false,
            message: error?.message ?? "Unknown process error.",
            stderr: error?.stderr ?? "",
            stdout: error?.stdout ?? "",
        };
    }
}

function tryParseJson(stdout) {
    try {
        return { ok: true, value: JSON.parse(stdout.trim()) };
    } catch {
        return { ok: false };
    }
}

function formatAnalysisSummary(payload, requestedArgs) {
    const board = asObject(getAny(payload, "board"));
    const boardFiles = getNumber(board, "files") ?? requestedArgs.files ?? 8;
    const boardRanks = getNumber(board, "ranks") ?? requestedArgs.ranks ?? 8;
    const densityPercents = asArray(getAny(payload, "densityPercents", "densities", "density_percentages"))
        .map((value) => Number(value))
        .filter((value) => Number.isFinite(value));
    const pieces = asArray(getAny(payload, "pieces")).map((piece) => asObject(piece));
    const unresolved = asArray(getAny(payload, "unresolvedPieces", "unresolved", "failedPieces"));

    const warningCounts = countWarnings(pieces);
    const lines = [
        `Piece material analysis (${boardFiles}x${boardRanks})`,
        densityPercents.length > 0 ? `Density sweep: ${densityPercents.join(", ")}%` : null,
        `Pieces analyzed: ${pieces.length}`,
        warningCounts.total > 0
            ? `Warnings: ${warningCounts.critical} critical, ${warningCounts.warning} warning, ${warningCounts.info} info`
            : "Warnings: none",
        "",
    ].filter(Boolean);

    if (pieces.length === 0) {
        lines.push("No piece results were returned.");
    } else {
        for (const piece of pieces) {
            lines.push(...formatPieceSummary(piece, densityPercents), "");
        }
    }

    if (unresolved.length > 0) {
        lines.push("Unresolved pieces:");
        for (const entry of unresolved) {
            lines.push(`- ${formatUnresolved(entry)}`);
        }
    }

    return lines.join("\n").trim();
}

function formatPieceSummary(piece, densityPercents) {
    const name = getString(piece, "name") ?? "UnknownPiece";
    const notation = getString(piece, "notation");
    const tier = getString(piece, "tier") ?? "Unknown";
    const tierSource = getString(piece, "tierSource", "tier_source");
    const midgameValue = getNumber(piece, "midgameValue", "midgame_value");
    const endgameValue = getNumber(piece, "endgameValue", "endgame_value");
    const averageDirections = getNumber(
        piece,
        "averageDirectionsAttacked",
        "average_directions_attacked",
        "avgDirectionsAttacked",
    );
    const averageSafeChecks = getNumber(piece, "averageSafeChecks", "average_safe_checks", "avgSafeChecks");
    const mobility = asObject(getAny(piece, "mobilityByDensityPercent", "mobility", "mobility_by_density_percent"));
    const percentiles = asObject(getAny(piece, "percentiles"));
    const vsTierPeers = asObject(getAny(percentiles, "vsTierPeers", "vs_tier_peers", "tierPeers"));
    const vsAllPieces = asObject(getAny(percentiles, "vsAllPieces", "vs_all_pieces", "allPieces"));
    const constructionError = getString(piece, "constructionError", "construction_error");
    const warnings = normalizeWarnings(piece, constructionError);

    const lines = [
        `- ${name}${notation ? ` (${notation})` : ""} — tier ${tier}${tierSource ? ` [${tierSource}]` : ""}`,
        `  values: MG ${formatNumber(midgameValue)} | EG ${formatNumber(endgameValue)}`,
        `  reachability: directions ${formatNumber(averageDirections)} | safe checks ${formatNumber(averageSafeChecks)}`,
    ];

    const mobilitySummary = summarizeMobility(mobility, densityPercents);
    if (mobilitySummary) {
        lines.push(`  mobility: ${mobilitySummary}`);
    }

    const percentileSummary = summarizePercentiles(vsTierPeers, vsAllPieces);
    if (percentileSummary) {
        lines.push(`  percentiles: ${percentileSummary}`);
    }

    if (warnings.length > 0) {
        for (const warning of warnings) {
            lines.push(`  ${warning.prefix} ${warning.severity}: ${warning.message}`);
        }
    } else {
        lines.push("  warnings: none");
    }

    return lines;
}

function summarizeMobility(mobility, densityPercents) {
    const keys = densityPercents.length > 0 ? densityPercents : Object.keys(mobility);
    const samples = [];

    for (const key of keys) {
        const value = getNumber(mobility, `${key}`);
        if (value != null) {
            samples.push(`${key}% ${formatNumber(value)}`);
        }
    }

    if (samples.length === 0) {
        return null;
    }

    if (samples.length <= 3) {
        return samples.join(", ");
    }

    return `${samples[0]}, ${samples[Math.floor(samples.length / 2)]}, ${samples[samples.length - 1]}`;
}

function summarizePercentiles(vsTierPeers, vsAllPieces) {
    const parts = [];
    const tierSummary = formatPercentileGroup(vsTierPeers);
    const allSummary = formatPercentileGroup(vsAllPieces);

    if (tierSummary) {
        parts.push(`vs tier peers ${tierSummary}`);
    }

    if (allSummary) {
        parts.push(`vs all pieces ${allSummary}`);
    }

    return parts.length > 0 ? parts.join(" | ") : null;
}

function formatPercentileGroup(obj) {
    const entries = Object.entries(obj)
        .map(([key, value]) => ({ key, value: Number(value) }))
        .filter((entry) => Number.isFinite(entry.value));

    if (entries.length === 0) {
        return null;
    }

    const preferredOrder = [
        "averagemobilityatdensity10",
        "averagedirectionsattacked",
        "averagesafechecks",
    ];
    entries.sort((left, right) => {
        const leftKey = normalizeKey(left.key);
        const rightKey = normalizeKey(right.key);
        const leftIndex = preferredOrder.indexOf(leftKey);
        const rightIndex = preferredOrder.indexOf(rightKey);

        if (leftIndex !== -1 || rightIndex !== -1) {
            return (leftIndex === -1 ? Number.MAX_SAFE_INTEGER : leftIndex) -
                (rightIndex === -1 ? Number.MAX_SAFE_INTEGER : rightIndex);
        }

        const leftNumber = Number(left.key);
        const rightNumber = Number(right.key);
        if (Number.isFinite(leftNumber) && Number.isFinite(rightNumber)) {
            return leftNumber - rightNumber;
        }

        return left.key.localeCompare(right.key);
    });

    return entries
        .slice(0, 3)
        .map((entry) => `${formatPercentileLabel(entry.key)} ${entry.value.toFixed(1)}th`)
        .join(", ");
}

function formatPercentileLabel(key) {
    const normalized = normalizeKey(key);
    if (normalized === "averagemobilityatdensity10") {
        return "mob@10";
    }
    if (normalized === "averagedirectionsattacked") {
        return "directions";
    }
    if (normalized === "averagesafechecks") {
        return "safe-checks";
    }

    const density = Number(key);
    if (Number.isFinite(density)) {
        return `${density}%`;
    }

    return `${key}`;
}

function normalizeWarnings(piece, constructionError) {
    const warnings = asArray(getAny(piece, "warnings")).map((warning) => {
        if (typeof warning === "string") {
            return { severity: "warning", message: warning, prefix: "⚠" };
        }

        const warningObject = asObject(warning);
        const severity = (getString(warningObject, "severity") ?? "warning").toLowerCase();
        return {
            severity,
            message: getString(warningObject, "message") ?? "Warning emitted without a message.",
            prefix: severity === "critical" ? "❗" : severity === "info" ? "ℹ" : "⚠",
        };
    });

    if (constructionError) {
        warnings.push({
            severity: "critical",
            message: `Construction error: ${constructionError}`,
            prefix: "❗",
        });
    }

    return warnings;
}

function countWarnings(pieces) {
    const counts = { total: 0, critical: 0, warning: 0, info: 0 };

    for (const piece of pieces) {
        for (const warning of normalizeWarnings(piece, getString(piece, "constructionError", "construction_error"))) {
            counts.total += 1;
            if (warning.severity === "critical") {
                counts.critical += 1;
            } else if (warning.severity === "info") {
                counts.info += 1;
            } else {
                counts.warning += 1;
            }
        }
    }

    return counts;
}

function formatUnresolved(entry) {
    if (typeof entry === "string") {
        return entry;
    }

    const object = asObject(entry);
    const name = getString(object, "name", "piece") ?? "UnknownPiece";
    const reason = getString(object, "reason", "message", "error");
    return reason ? `${name}: ${reason}` : name;
}

function normalizePositiveInteger(value, fallback) {
    if (value == null || value === "") {
        return fallback;
    }

    const parsed = Number(value);
    return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function formatNumber(value) {
    if (value == null || Number.isNaN(value)) {
        return "n/a";
    }

    if (Number.isInteger(value)) {
        return `${value}`;
    }

    return value.toFixed(2).replace(/\.?0+$/, "");
}

function asArray(value) {
    return Array.isArray(value) ? value : [];
}

function asObject(value) {
    return isPlainObject(value) ? value : {};
}

function isPlainObject(value) {
    return value != null && typeof value === "object" && !Array.isArray(value);
}

function getString(object, ...names) {
    const value = getAny(object, ...names);
    return value == null ? null : `${value}`;
}

function getNumber(object, ...names) {
    const value = getAny(object, ...names);
    if (value == null || value === "") {
        return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
}

function getAny(object, ...names) {
    if (!isPlainObject(object)) {
        return undefined;
    }

    const normalizedMap = new Map(
        Object.entries(object).map(([key, value]) => [normalizeKey(key), value]),
    );

    for (const name of names) {
        const match = normalizedMap.get(normalizeKey(name));
        if (match !== undefined) {
            return match;
        }
    }

    return undefined;
}

function normalizeKey(value) {
    return `${value}`.replace(/[^a-zA-Z0-9]/g, "").toLowerCase();
}

function clip(value, max = 400) {
    const text = `${value ?? ""}`.trim().replace(/\s+/g, " ");
    if (text.length <= max) {
        return text;
    }

    return `${text.slice(0, max - 3)}...`;
}

async function pathExists(path) {
    try {
        await access(path);
        return true;
    } catch {
        return false;
    }
}

function failure(textResultForLlm) {
    return { resultType: "failure", textResultForLlm };
}
