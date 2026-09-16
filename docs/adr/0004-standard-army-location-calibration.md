# Calibrate Locations against the Standard army

Status: accepted
Date: 2026-09-13

Location difficulty uses the approved Standard (FIDE) army for each geometry.
It does not depend on the Enemy Army dropdown or take a conservative maximum
across families. Players can deliberately choose another family that makes
particular Locations easier or harder.

Catalog deltas use midgame values, with 100 material units per pawn unit.
Authored baselines and offsets remain part of the hybrid calibration method.

## Consequences

The generator does not need a locked family choice or four family-dependent
threshold schedules. Valid arrays and gameplay rules remain required for all
four families.

The Standard reference includes the approved augmentation for the selected
geometry. It does not mean an unchanged orthodox 8x8 army on every board.

This decision does not select the complete equation, role adjustments,
rounding, monotonicity rules, or stage caps.

## Source

[Define the material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md#decisions-in-progress).
The user selected the Standard-family reference and midgame values during the
2026-09-13 grill-with-docs rounds.
