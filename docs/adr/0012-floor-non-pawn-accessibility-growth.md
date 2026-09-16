# Floor non-pawn accessibility growth by movement class

Status: accepted
Date: 2026-09-14

Large-board individual captures need an explicit access buffer for the added
surrounding material. Bounded-range targets receive a stronger minimum growth
buffer than sliders, which can travel farther and enter play earlier.
The estimator keeps the larger of proportional accessibility and this floor.

## Expanded-formation rule

`A_ref` is the authored reference requirement minus the reference target's
midgame value. `B` is the clear budget. All piece values and movement classes
come from the Standard starting formations.

```text
other_growth =
    (new_starting_CPU_MG - new_target_MG)
    - (reference_starting_CPU_MG - reference_target_MG)

scaled_accessibility = A_ref * B_new / B_ref

if other_growth > 0:
    accessibility =
        max(scaled_accessibility, A_ref + beta * other_growth)

expanded_capture_requirement =
    new_target_MG + accessibility + explicit_position_correction
```

| Target movement | beta |
| --- | ---: |
| Bounded-range non-pawn | 1/2 |
| Board-spanning slider, including slider/leaper hybrids | 1/4 |

Knight, basic Elephant, and Lion use the bounded group.
Bishop, Rook, Queen, Archbishop, Chancellor, and Amazon use the sliding group.
The selected alternative enemy family does not change this classification.

The buffer uses the starting formation, not current remaining material.
The maximum prevents double-charging the two growth estimates.
Unchanged reference formations retain their authored values.

## Scope and limits

The user selected these provisional coefficients. They are not constants
recovered from the historical calibration.
Pawns retain a separate positional model.
[ADR 0013](0013-use-released-individual-capture-references.md) supplies the
selected role mappings and compact transfer.
[ADR 0014](0014-price-regicide-between-queen-capture-and-victory.md) supplies
Regicide's separate royal-achievement tier.

With the grand10 references, 12x10 Queen's/King's Knight estimates become
3800/4000 at difficulty 1 without caps. The Bishop pair becomes 2500/2700.
The Queen's Rook and Queen examples remain 4870 and 6808 after that illustrative
final ceiling.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md).
The user selected half-growth and quarter-growth floors in the 2026-09-14
interview.
