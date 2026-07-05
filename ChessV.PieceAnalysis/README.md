# ChessV.PieceAnalysis

Headless console tool for running ChessV piece mobility analysis across the standard density sweep, then comparing results against the APMW tier roster and hand-tuned material values.

## CLI

```text
ChessV.PieceAnalysis.exe [options]
dotnet run --project ChessV.PieceAnalysis -- [options]
dotnet exec <path-to-dll> [options]
```

Options:

- `--piece <Name>` repeatable piece class name from `ChessV.Games` (example: `ShortRook`, `Queen`, `Cannon`)
- `--all` analyze every constructible `PieceType` subclass in `ChessV.Games`
- `--tier <TierName>` optional hint for unknown `--piece` entries: `Weak|Pawn|Minor|Major|Jack|Queen|Amazon`
- `--files <N>` board files, default `8`
- `--ranks <N>` board ranks, default `8`
- `--out <path>` write JSON to a file instead of stdout

Behavior:

- If neither `--piece` nor `--all` is supplied, the tool analyzes the curated APMW tier roster.
- Stdout is JSON only. Human diagnostics such as ignored tier hints are written to stderr.
- `--all` reports per-piece construction failures in `pieces[].constructionError` without aborting the run.
- Explicit `--piece` names that cannot be found or constructed are reported in `unresolvedPieces`.

## JSON schema

```json
{
  "board": { "files": 8, "ranks": 8 },
  "densityPercents": [50, 45, 40, 35, 30, 25, 20, 15, 10],
  "generatedAtUtc": "2026-07-05T21:08:00Z",
  "pieces": [
    {
      "name": "ShortRook",
      "notation": "S",
      "tier": "Minor",
      "tierSource": "ApmwChess",
      "midgameValue": 400,
      "endgameValue": 425,
      "averageDirectionsAttacked": 4.25,
      "averageSafeChecks": 2.25,
      "mobilityByDensityPercent": {
        "10": 5.6,
        "15": 5.2,
        "20": 4.9,
        "25": 4.6,
        "30": 4.4,
        "35": 4.1,
        "40": 3.9,
        "45": 3.7,
        "50": 3.5
      },
      "percentiles": {
        "vsTierPeers": {
          "averageMobilityAtDensity10": 95.8,
          "averageDirectionsAttacked": 91.7,
          "averageSafeChecks": 87.5
        },
        "vsAllPieces": {
          "averageMobilityAtDensity10": 72.0,
          "averageDirectionsAttacked": 78.0,
          "averageSafeChecks": 65.0
        }
      },
      "warnings": [
        {
          "type": "statistical_outlier",
          "severity": "warning",
          "message": "..."
        },
        {
          "type": "absolute_threshold",
          "severity": "warning",
          "message": "..."
        },
        {
          "type": "value_tier_mismatch",
          "severity": "info",
          "message": "..."
        }
      ],
      "constructionError": null
    }
  ],
  "unresolvedPieces": [
    {
      "name": "MissingPiece",
      "reason": "Piece type was not found in ChessV.Games."
    }
  ]
}
```

Notes:

- `mobilityByDensityPercent` keys are stringified density percentages.
- `percentiles.vsTierPeers` is empty/null-valued when a piece has no known tier cohort.
- `warnings` may be empty.
- Failed `--all` constructions keep the piece entry but set most analysis fields to `null` and populate `constructionError`.
