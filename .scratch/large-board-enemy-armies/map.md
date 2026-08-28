# Geometry-driven large-board enemy armies

## Destination

An execution-ready cross-repository design for the formal geometry set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`, including exact per-family placement arrays for geometry-driven CPU army augmentation on 10x10 and 12x10, royal/check/castling semantics, stable role-based ChecksMate Location identity, and a calibrated material contract suitable for a later spec and ticket handoff.

## Notes

- This is a planning map, not production implementation.
- Formal support is exactly `[6,8,10]x8` plus `[10,12]x10`: `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`.
- A current or legacy 12x12 stage may be cited as source and compatibility evidence, and the later contract must define explicit rejected, hidden, or otherwise unsupported behavior for it. It is not a target geometry.
- Preserve the existing four-family Enemy Army selector. Geometry augments the selected family automatically; no new dropdown entries are planned.
- Source authority is: decisions supplied for this effort, then current source/docs/tests, then historical prompt lineage.
- Work array questions as concrete prototypes with the user. Work semantic Location naming with both grilling and domain-modeling.
- Treat `ChessV.Test\ApmwGeometryProfileTests.cs` and `APMW.Test\ApmwGameCharacterizationTests.cs` as primary ChessV characterization surfaces.
- The local sibling at `C:\GitHub\rft50-checksmate` is evidence only for this map. Do not edit or publish it from this effort.
- A map resolution may describe required cross-repository behavior and ownership, but production patches, migration execution, and calibration runs begin only after this map is complete and handed off.

## Decisions so far

- [Keep one family selector and make augmentation geometry-driven](issues/01-keep-one-family-selector.md) — the existing four Enemy Army families remain the only UI choices.
- [Use the existing CPU profile and setup seam](issues/02-use-existing-cpu-profile-seam.md) — geometry profiles, family resolution, FEN composition, and castling plans are the current integration boundary; its current 12x12 behavior is compatibility evidence, not support scope.
- [Respect the versioned geometry and projection contract](issues/03-respect-versioned-geometry-contract.md) — current stage capacities and CPU profile versions, including the legacy 12x12 record, are cross-repository compatibility data with frozen-hash coupling.
- [Use catalog facts without assuming rule compatibility](issues/04-use-catalog-facts-with-rule-review.md) — movement/value facts are known, but additional royals, castlers, and rook-like evaluation need explicit semantics.
- [Treat current ChecksMate targets as hand-authored compatibility data](issues/05-treat-checksmate-targets-as-hand-authored.md) — Locations, thresholds, counts, and legacy 12-file scaling are static authored series rather than a derived material model; they do not make 12x12 supported.
- [Rank current decisions above historical lineage](issues/06-rank-current-decisions-above-lineage.md) — prior Amazon and added-rank decisions constrain the discussion but do not authorize a CPU square.
- [Target a family-preserving 10x10 augmentation budget](issues/07-target-family-preserving-ten-by-ten-budget.md) — use Towers rather than Scouts and aim for roughly twenty pawn units without erasing family identity.
- [Carry forward the settled formation constraints](issues/08-carry-forward-formation-constraints.md) — the edge wedge, middle-out insertion, bishop-color protection, king alignment, and queen/Amazon roles constrain the unresolved 10x10 and 12x10 arrays.
- [Adopt hybrid material recalibration](issues/09-adopt-hybrid-material-recalibration.md) — catalog deltas seed new values, while authored offsets and calibration remain part of the contract.
- [Migrate file-based Locations to role identity](issues/10-migrate-locations-to-role-identity.md) — a square's strategic role, not its file letter, should determine Location identity.

## Not yet specified

- The final spec/ticket partition and implementation sequencing cannot be named until the live decision tickets settle the arrays, semantic identities, balancing equations, and version boundary.
- New fog discovered while resolving the frontier should be added here only when it cannot yet be phrased as a precise decision question.

## Out of scope

- Production implementation in ChessV, including source, tests, README, or release notes.
- Exact edits, commits, or pull requests in `chesslogic/Archipelago` / `C:\GitHub\rft50-checksmate`.
- PopTracker implementation or presentation changes.
- Running play-test calibration or tuning thresholds from play results; this map only defines the later calibration contract.
- Redesigning player itemization, including `major_to_amazon`, except where its existing contract constrains interoperability.
- Adding special Enemy Army dropdown entries.
- Formal support for 12x12, including 12x12 placement arrays, calibration targets, or an accepted contract stage. Current 12x12 source/contract facts remain compatibility evidence and may require explicit rejected, hidden, or otherwise unsupported behavior.
- Creating an ADR, `CONTEXT.md`, or `CONTEXT-MAP.md` before a canonical term or durable architectural decision is actually resolved.
