# 01 - Add the APMW material profile and validator

Status: needs-triage
Blocked by: B1 v4 `type_id` (D8 in `../spec.md` §9)

## Goal

Add a pure, immutable `ApmwMaterialProfiles` in `ChessV.Games` (beside
`ApmwPieceCatalog.cs`) that stores complete MG/EG tables for `6x8`, `8x8`,
`10x8`, `10x10`, and `12x10`. Every stage equals today's 8x8 constants.
Nothing consumes it yet.

## Scope

- Data shape, keys, expansion, and hash per `../spec.md` §5.2-5.6.
- Type key is the v4 `type_id` (`../spec.md` §5.3). Do not key by board
  `Notation` or display `Name`.
- `Resolve(stageId, typeKey)` lookup with no `Game` or `ApmwCore` dependency.

## Acceptance

- F1 in `../spec.md` §7: missing type, extra stage `12x12`, unknown stage, and
  negative values fail at load.
- Golden test: expanded `8x8` equals the current `ApmwPieceCatalog`
  constructor values for all 43 types.
- Canonical sha256 is stable across runs.

## Out of scope

Changing any non-8x8 value. Reading the profile from any consumer.
