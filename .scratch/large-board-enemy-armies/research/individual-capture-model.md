# Provisional individual-capture profiles

Status: Derived from the selected round 12 and round 13 decisions

The [CSV](individual-capture-provisional.csv) contains exact intrinsic
requirements for every individual non-King capture on all five geometries.
It also contains the single 12x10 Regicide profile.
These are provisional estimates, not measured gameplay difficulty.

## Coverage

| Geometry | Non-pawn non-King rows | Pawn rows | Regicide rows | Total |
| --- | ---: | ---: | ---: | ---: |
| 6x8 | 5 | 6 | 0 | 11 |
| 8x8 | 7 | 8 | 0 | 15 |
| 10x8 | 9 | 10 | 0 | 19 |
| 10x10 | 15 | 12 | 0 | 27 |
| 12x10 | 18 | 14 | 1 | 33 |
| Total | 54 | 50 | 1 | 105 |

The data contains 34 distinct individual non-King role keys and Regicide.
Several geometries can supply a profile for one shared Location.
The 105 profile rows do not imply 105 distinct Locations.

The [role catalog](../issues/15-define-location-role-identity.md#canonical-role-and-coordinate-catalog)
supplies the role keys, names, and White origins.
Black keeps the file and uses rank `height + 1 - white_rank`.
The Regicide origin field uses `|` to mean either starting King, not both.

## Input authority

| Data or rule | Authority |
| --- | --- |
| Standard inventory, midgame values, and geometry clear budgets | [Material record](../issues/16-define-material-calibration-contract.md) and [ADR 0009](../../../docs/adr/0009-anchor-provisional-endgame-budgets.md) |
| Non-pawn reference requirements and new-role analogues | [ADR 0013](../../../docs/adr/0013-use-released-individual-capture-references.md) |
| Movement-dependent accessibility floor | [ADR 0012](../../../docs/adr/0012-floor-non-pawn-accessibility-growth.md) |
| Pawn controls, interpolation, pressure term, and Rearguard premium | [ADR 0015](../../../docs/adr/0015-price-pawns-with-curves-and-rearguard-premiums.md) and the material record's exact curve definition |
| Queen-to-checkmate Regicide relationship | [ADR 0014](../../../docs/adr/0014-price-regicide-between-queen-capture-and-victory.md) |
| Released versus later reference inputs | [Release-provenance audit](calibration-reference-provenance.md) |

The released banks contain sixteen non-pawn values and eighteen pawn values.
Their corresponding 8x8/10x8 profiles reproduce those values exactly.
This is reproduction by construction, not predictive validation.
The later Outer Attendant and pawn K/L constants are not inputs.

The CSV retains applicable intermediate terms.
For non-pawns, these include reference accessibility, other-material growth,
the proportional estimate, and the growth floor.
For pawns, these include the reference coordinate, curve value, pressure
budget, spatial premium, main baseline, and Rearguard premium.
Regicide retains its Queen and checkmate inputs.

An empty term means that the selected equation does not use that term.
Zero additional corrections are explicit.
They do not erase historical adjustments already present in released inputs.

## Selected 12x10 examples

| Goal | Exact intrinsic material | Example final value |
| --- | --- | ---: |
| Queen | 823700/121 | 6808 |
| Amazon | 866050/121 | 7158 |
| Queen's Elephant | 7525/2 | 3763 |
| King's Elephant | 7925/2 | 3963 |
| Queen's Lion | 7775/2 | 3888 |
| King's Lion | 8175/2 | 4088 |
| Queen's Rearguard Pawn | 315967375/137214 | 2303 |
| King's Rearguard Pawn | 361045/162 | 2229 |
| Regicide | 1224140/121 | 10117 |

The example column applies difficulty 1, adjustment 0, and no cap.
The exact fraction, not the example, supplies the runtime input.

```text
effective_material =
    min(stage_material_cap,
        ceil(intrinsic_material * difficulty + applicable_absolute_adjustment))
```

Every row has a zero additional-player-chessmen estimate because each goal
requires one capture.
The Rearguard premium does not add a front-pawn capture prerequisite.
Regicide does not require both Kings.

## Arithmetic and coverage evidence

The bounded arithmetic probe checks all sixteen non-pawn reference values.
Its pawn companion checks all eighteen released pawn values.
It also samples 101 coordinates per curve interval for interval bounds.
These operations do not play games or tune the selected coefficients.

The table builder checks the role count, profile count, coordinate bounds,
unique starting squares, and total Standard inventory material.
It compares selected Black origins with explicit reflected-square fixtures.
It compares the six selected Lion/Rearguard outputs with their exact
decision fixtures. It also compares the Queen and Regicide fractions.

The probes persist in the current session's `files\calibration-models`
directory as `individual_capture_probe.py` and `pawn_curve_probe.py`.
The authoritative design inputs remain the linked role catalog and ADRs.
The CSV is a generated planning artifact, not a selected production schema.

## Single-ceiling fixture

This fixture isolates final processing from gameplay and projection
calculation. Its explicit cap is a synthetic arithmetic input, not a
published 12x10 maximum.

| Field | Exact value |
| --- | --- |
| Goal | 12x10 King's Rearguard Pawn |
| Intrinsic material | 361045/162 |
| Difficulty | 27/20 |
| Applicable absolute adjustment | 240 |
| Value before the ceiling | 77969/24 |
| Result without a cap | 3249 |
| Supplied cap | 3000 |
| Result with that cap | 3000 |
| Additional-player-chessmen estimate | 0 |

Rounding the intrinsic value first gives 3250 before the cap.
That result is incorrect under the selected operation order.
The cap does not authorize an earlier ceiling, even when both paths finally
give 3000.

## Remaining integration boundary

The [series table](capture-series-provisional.csv) supplies the enlarged-board
numbered goals. The material record supplies the five-geometry endgame and
task-specific tables.
Together these artifacts cover the selected goal classes.

Generator access still needs complete per-geometry alternatives.
An unavailable role or unsupported capability has no path, not a zero
requirement. Material, chessmen, and special conditions must qualify together.
The retained resource bypass does not waive the actual achievement condition.

Correction storage, package identifiers, pinned data, and real packaged
integration remain part of the contract and acceptance work.
No production implementation or gameplay calibration occurred.
