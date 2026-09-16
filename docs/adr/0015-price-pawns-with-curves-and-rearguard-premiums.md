# Price pawns with positional curves and a Rearguard premium

Status: accepted

Pawn access varies by file, side, and starting formation.
The user selected separate shape-preserving cubic curves through the released
pawn controls. Rearguard targets add one quarter of the corresponding main
pawn's accessibility budget.

## Baseline

Base8 supplies 6x8 and 8x8. Grand10 supplies larger boards.
Each half maps its center-to-edge file distance onto the corresponding
reference half. The curve preserves the released control values, including
the King-side dip, without interval overshoot.

```text
central_budget =
    100 + (44/405) * max(0, starting_CPU_MG - 4375)

main_pawn_baseline =
    central_budget
    + (reference_curve(file_coordinate) - reference_central_price)
      * target_clear_budget / reference_clear_budget

rearguard_premium = (main_pawn_baseline - 100) / 4

rearguard_requirement = main_pawn_baseline + rearguard_premium
```

The 100-unit pawn value remains separate from accessibility.
The central-pressure coefficient derives from the released 100/320 central
prices and the 2025-unit difference in reference army material.
Its transfer to other formations is a provisional author decision.
The Rearguard coefficient is also an author decision, not a historical value.

## Scope and processing

The extra premium applies only to the specified starting Rearguard roles.
It does not apply to a main pawn because another piece blocks it.
The original role continues after movement or promotion.
Capturing the front pawn is not a prerequisite for the Rearguard Location.

Each individual pawn capture retains a zero additional-player-chessmen
estimate. Other initial additional pawn position corrections are zero.
Future corrections require explicit records.

All calculations retain fractions.
Difficulty and applicable absolute adjustment precede the single final
ceiling. The projection cap applies last.

| Formation | Main pawn baseline after example ceiling | Rearguard requirement after example ceiling |
| --- | ---: | ---: |
| 10x10, Queen or King side | 1190 | 1462 |
| 12x10, Queen side | 1863 | 2303 |
| 12x10, King side | 1803 | 2229 |

These examples use difficulty 1, no adjustment, and no cap.
The [material record](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md#exact-curve-definition)
defines the exact controls, coordinates, tangents, and interpolation.
The post-release pawn K/L prices are not reference controls.

## Source

Round 12, Q28 selected the curve model with an additional Rearguard premium.
Round 13, Q30 selected: "Add 25% of the same-file main pawn's accessibility
budget".
The fixed 100/300 premiums and zero-premium proposal are not selected.
