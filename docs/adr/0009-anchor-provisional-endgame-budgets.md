# Anchor provisional endgame budgets to the established small and grand profiles

Status: accepted
Date: 2026-09-14

Provisional checkmate and Capture Everything budgets retain the established
8x8 and 10x8 authored anchors. Other supported formations add the exact
Standard CPU inventory delta, using current midgame values.
This replaces the old twelve-file approximation rather than preserving its
extra 700 material units.

## Function and anchors

`V` is the sum of all starting CPU midgame values, including Kings.

```text
budget(target) =
    budget(reference) + V(target) - V(reference)
```

Use the 8x8 reference for 6x8 and 8x8. Use the 10x8 reference for 10x8,
10x10, and 12x10.

| Reference | V | Checkmate anchor | Capture Everything anchor |
| --- | ---: | ---: | ---: |
| 8x8 | 4375 | 4020 | 4020 |
| 10x8 | 6400 | 6020 | 6050 |

## Provisional intrinsic budgets

| Geometry | V | Checkmate | Capture Everything |
| --- | ---: | ---: | ---: |
| 6x8 | 2725 | 2370 | 2370 |
| 8x8 | 4375 | 4020 | 4020 |
| 10x8 | 6400 | 6020 | 6050 |
| 10x10 | 8400 | 8020 | 8050 |
| 12x10 | 11600 | 11220 | 11250 |

The Mounted King upgrade and spare King already contribute through `V`.
No additional new premium applies at this initial stage.

These are intrinsic endgame budgets, before player difficulty and projection
caps. They are not released, play-tested thresholds or a blanket increment
for individual captures. Later adjustments require explicit authored entries.

## Source and excluded alternative

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md#decision-record).
The user selected these anchors and provisional values in the 2026-09-14
interview.

The old twelve-file comment adds 2000 for an actual 1300 inventory increment.
Retaining that approximation would produce 11920/11950 for the new 12x10.
The user chose 11220/11250 instead.
