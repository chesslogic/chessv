# Use released references for individual non-pawn captures

Status: accepted

The release audit separates established inputs from later additions without
gameplay evidence. The user chose released matching-role references and
explicit analogues for new roles. Lions use Knight references, not the later
Outer Attendant constants.

## Reference bank

The base8 bank supplies 6x8 and 8x8. The grand10 bank supplies larger boards.
The corresponding reference clear budgets are 4020 and 6050.
The corresponding Standard CPU material totals are 4375 and 6400.

| Reference role | Target MG | Base8 requirement | Grand10 requirement |
| --- | ---: | ---: | ---: |
| Queen's Rook | 500 | 1500 | 2850 |
| Queen's Knight | 325 | 700 | 1200 |
| Queen's Bishop | 325 | 1040 | 1200 |
| Queen | 950 | 1300 | 4100 |
| King's Bishop | 325 | 1140 | 1400 |
| King's Knight | 325 | 1040 | 1400 |
| King's Rook | 500 | 1900 | 3250 |
| Queen's Attendant (Archbishop) | 875 | Not present | 3950 |
| King's Attendant (Chancellor) | 950 | Not present | 4030 |

These requirements occur in the published Client 0.3.2 world.
Release presence does not prove that every requirement underwent gameplay
calibration.

| Target role | Selected reference |
| --- | --- |
| Existing home role | Matching role in the geometry's reference bank |
| Forward Bishop or Knight | Same-side home counterpart in grand10 |
| Basic Elephant | Same-side grand10 Knight, with target MG 250 |
| Lion | Same-side grand10 Knight, with target MG 500 |
| Amazon | Grand10 Queen, with target MG 1300 |
| Compact Center Rook | Base8 Queen's Rook, with target MG 500 |

These analogues change calibration inputs, not capture identities.
The compact Center Rook remains separate from Queen's Rook and Queen.
No automatic reference exists for an unknown role.

## Transfer and corrections

[ADR 0012](0012-floor-non-pawn-accessibility-growth.md) supplies the positive
growth rule. Without positive growth in other starting material, accessibility
uses only the reference accessibility multiplied by the clear-budget ratio.
Thus compact 6x8 uses the ratio 2370/4020, without an added growth floor.

Initial additional position corrections are zero. The released requirements
already contain their historical authored adjustments.
Later corrections require explicit records.

At difficulty 1 without adjustment or caps, the 12x10 Lion pair gives
3888/4088 after the final ceiling. Its intrinsic values are 7775/2 and 8175/2.
The compact Center Rook gives 1090 after the final ceiling.
No intermediate rounded example becomes a runtime input.

## Source

[Material calibration contract](../../.scratch/large-board-enemy-armies/issues/16-define-material-calibration-contract.md),
round 12, Q27: "Adopt the released analogue bank, including Knight references
for Lions".

[Release-provenance audit](../../.scratch/large-board-enemy-armies/research/calibration-reference-provenance.md).
