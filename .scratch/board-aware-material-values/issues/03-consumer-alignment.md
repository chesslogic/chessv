# 03 - Prove consumers read the prepared game's values

Status: needs-triage
Blocked by: 02

## Goal

Show with a test-only profile that evaluation, SEE, board material, pocket
Gems, True Fork, and Properties all read the prepared game's `PieceType`
values. Change code only where a consumer reads a global or stale instance.

## Scope

- `CardDropRule.cs:94-143`: keep `MidgameValue / 100` integer division; no new
  rounding or minimum.
- `APMW.Client\LocationHandler.cs:693`: True Fork uses prepared-game MG.
- `ChessV.GUI\Controls\PieceInfoControl.cs:53-54`: shows prepared-game MG/EG.
- `Board.cs`, `Game.cs` SEE, `MoveList.cs`: confirm through tests.

## Acceptance

F3 in `../spec.md` §7. Production profile remains neutral.
