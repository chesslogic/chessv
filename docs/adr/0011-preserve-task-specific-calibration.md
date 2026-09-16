# Preserve task-specific budgets and the True Royal Fork endgame link

Status: accepted
Date: 2026-09-14

Existing tactical, survival, and King-action goals retain their authored
task-specific material and chessmen values as reference defaults.
They do not receive a blanket army-material increase.
True Royal Fork retains its specific relationship to the checkmate budget.

## Reference rule

Use base8 values for 6x8/8x8 and grand10 values for larger boards.
These are explicit target-profile defaults, not access through an unavailable
reference board.

For True Royal Fork, use the target geometry's accepted checkmate budget:

| Geometry | Intrinsic material |
| --- | ---: |
| 6x8 | 2370 |
| 8x8 | 4020 |
| 10x8 | 6020 |
| 10x10 | 8020 |
| 12x10 | 11220 |

Its authored player-chessmen estimate remains 14 before the projection cap.
Other task-specific chessmen estimates also remain unchanged before caps.
The captures-minus-one heuristic does not replace non-capture estimates.

## Consequences

Material values do not establish gameplay eligibility.
A zero castling price does not create a castling path on 6x8.
Existing target classification and special conditions remain separate.

The compact True Royal Fork budget is lower than some other authored fork
budgets. This follows the selected endgame link. The policy does not force
one numeric ordering across different goal classes.

Regicide has no historical capture-budget entry and remains a separate
calibration case. Individual starting-role captures also use their own model.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md).
The user selected retained task-specific values and the True Royal Fork link
in the 2026-09-14 interview.
