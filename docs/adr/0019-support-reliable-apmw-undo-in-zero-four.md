# Support reliable APMW Undo in 0.4.0

Status: accepted
Date: 2026-09-17

## Decision

Include reliable player-facing Undo support in 0.4.0.
Complete the existing reversible capture ledger and its integration rather
than disable backtracking by default.

The user selected at Q37:

> Include reliable Undo support in 0.4.0 (Recommended)

This selects release scope, not permission to start implementation.

## State and reporting

Each successful committed move has corresponding reversible match-local
state.
Undo restores capture counts, starting-role identity, royal phase, castling
rights, and other affected local state.
A multi-capture belongs to one move, not several invented turns.
Rejected or speculative attempts must not leave committed progress.

The current `MoveDiff` ledger already records capture deltas and prior
origin mappings.
Extend or adapt that mechanism instead of adding independent counter
histories for every capture threshold.
Its integration must cover real engine Undo, not only direct handler calls.

Undo does not retract Location checks already accepted by Archipelago.
For example, undoing a second pawn capture restores the local count to 1.
An awarded Capture 2 Pawns check remains complete.
The next continuation counts from the restored local state.
This decision does not introduce delayed reporting or server-side retraction.

## History and scope

Genuinely view-only history is acceptable.
It must not change the active continuation or generate gameplay reports.
The current toolbar's live committed undo/replay is not inherently
view-only.
This decision does not require a new general-purpose analysis UI.

Internal engine rollback remains required regardless of UI controls.
Existing finalization and match-lifecycle boundaries are not automatically
removed by selecting reliable Undo.
Player-control switching requires a separate support decision.
Ordinary-game controls are unchanged.

[ADR 0018](0018-reject-fen-based-apmw-resume.md) still rejects FEN-based APMW
resume.
A possible starting-FEN attachment in SGF does not change that boundary.

## Source

[Interaction-support record](../../.scratch/large-board-enemy-armies/issues/21-define-apmw-interaction-support.md)
contains the Q36 proposal, source evidence, and Q37 scope decision.
[Royal lifecycle fixtures](../../.scratch/large-board-enemy-armies/contracts/royal-lifecycle-fixtures.md)
define the existing reversible-state and failed-move requirements.
