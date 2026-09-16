# Transfer capture-series curves through their endpoints

Status: accepted
Date: 2026-09-14

The provisional numbered-series estimator retains the authored base8 and
grand10 curves. It maps each target series from count 2 through its normal
local ceiling onto the corresponding reference interval, then scales material
by the clear-budget ratio.

This preserves the reference curves while defining the new counts without
unsupported extrapolation. It selects relative pacing within each series,
not a claim that equal fractions leave equivalent enemy armies.

## Function

Let `m_g,c` and `m_r,c` be the target and reference local ceilings for class
`c`. Let `L_r,c` interpolate adjacent authored reference points.

```text
x = 2 + (n - 2) * (m_r,c - 2) / (m_g,c - 2)

F_g,c(n) = L_r,c(x) * clear_budget_g / clear_budget_r
```

Use base8 for 6x8/8x8 and grand10 for larger boards.
The clear budgets come from [the endgame-anchor decision](0009-anchor-provisional-endgame-budgets.md).
Interpolation and scaling retain exact fractions until final processing.

The local ceiling supplies a calibration coordinate. It does not replace the
configured world's published Location set.
For a one-King board, a published Any goal equal to its full non-King count
uses that board's clear budget. This covers Any 11/15/19/27 on earlier boards.

No Any 33/34 or Pieces 20 Location is added for 12x10.
Capture Everything retains its separate starting-set condition.

## Consequences

Each maximum keeps its reference endpoint's relative price. Each 14 on 12x10
is therefore a late check, but it is not equivalent to Capture Everything.

The later legacy12 additions remain comparison evidence, not mandatory anchors
for the changed army. Explicit corrections can change derived intrinsic
values without silently changing the historical reference curves.
The initial correction entries are zero.

Apply corrections before player difficulty and absolute adjustment, then round
upward once and cap last. Within-series monotonicity must hold.
A conflicting correction requires explicit resolution, not automatic repair.

This function covers numbered capture series only. Individual captures,
Regicide, tactical goals, and movement goals have separate material estimates.
The function is provisional calibration, not proof of play-tested balance.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md).
The user selected the endpoint-anchored function in the 2026-09-14 interview.
The [model report](../../.scratch/large-board-enemy-armies/research/capture-series-endpoint-model.md)
records the assumptions, reference curves, coverage, and retrospective errors.
