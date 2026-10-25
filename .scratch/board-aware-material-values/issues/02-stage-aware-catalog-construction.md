# 02 - Build APMW piece types from the stage profile

Status: needs-triage
Blocked by: 01

## Goal

`ApmwPieceCatalog.Create(stageId)` constructs fresh `PieceType` instances with
values resolved from the material profile, using the board type's
`ApmwProfile.StageId`. Values are final before `BindTo`, `Publish`,
`AddPieceTypes`, and board accounting. `ApmwCore` publishes family membership
only.

## Scope

- `ApmwPieceCatalog.cs`: remove hard-coded MG/EG pairs; take a stage.
- `ApmwChess.cs` `earlyPopulatePieceTypes()`: pass the stage.
- All five APMW board types; 12x12 entry keeps its ADR 0002 rejection.
- No writes to `MidgameValue`/`EndgameValue` after construction.

## Acceptance

- F2 in `../spec.md` §7 (8x8 -> 10x10 test profile -> 8x8 isolation).
- Full APMW and engine suites unchanged with the neutral profile.

## Out of scope

Client pricing (issue 04). Ordinary variants (unchanged, `../spec.md` §5.9).
