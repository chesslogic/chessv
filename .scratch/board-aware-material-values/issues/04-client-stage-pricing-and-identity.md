# 04 - Price client allocation by captured stage and expose profile identity

Status: needs-triage
Blocked by: 02

## Goal

Client allocation, previews, and the projection adapter price concrete types
through the pure profile lookup for the captured prepared stage, not through
the last-published `ApmwCore` instances. Expose the profile identity
`(profile_id, revision, sha256)` as additive metadata for later export.

## Scope

- `APMW.Client\OwnedRosterGeneration.cs`: `PiecesForFamily`, `RosterPiece`
  evaluated MG, `ConcreteEvaluationAdjustment`.
- `APMW.Client\ItemGeneration.cs`: `PieceMaterialAccounting`.
- `APMW.Client\ApmwProjectionBackend.cs`: stage-bound pricing.
- Family credits (`ItemGenerationValues`, contract `ExpectedMaterial`) stay
  fixed.

## Acceptance

- F4 in `../spec.md` §7 (prepared 8x8, final 10x10; generator side mocked).
- A preview computed after creating another geometry's game is unchanged.
- Neutral profile: existing allocation tests unchanged.

## Out of scope

The elimination procedure (spec I5). Contract export shape (spec I6, D7).
