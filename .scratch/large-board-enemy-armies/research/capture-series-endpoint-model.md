# Capture-series transfer: endpoint-anchored candidate

Status: accepted provisional function in the 2026-09-14 interview.

[ADR 0010](../../../docs/adr/0010-transfer-capture-series-curves-by-endpoints.md)
owns the decision. This report retains the analytical assumptions and limits.

**Reference-evidence review:** The user subsequently requested a per-role audit
against 0.3.2 because some later additions were never tested.
The [completed audit](calibration-reference-provenance.md) now confirms all 74
base8/grand10 series inputs in the published world asset.
This establishes release presence, not measured gameplay accuracy.
The later legacy12 additions remain comparison evidence rather than anchors.

The [initial comparison](capture-series-model-candidates.md) found incomplete
domains or weak tails in three candidates. Model D supplies a complete
bounded alternative without inventing count-0 or count-1 anchors.
It trades a workload interpretation for an explicit pacing assumption.

## Inputs and function

Use the accepted clear budgets and references from
[the endgame-budget decision](../../../docs/adr/0009-anchor-provisional-endgame-budgets.md).
Use the base8/grand10 authored curves listed in the initial comparison.
Those curves, not capped runtime values, supply the calibration inputs.

For series class `c`, let `m_g,c` be the target formation's normal local
ceiling, and `m_r,c` the reference ceiling. These are formation properties,
not the complete world's published upper limit.

```text
x = 2 + (n - 2) * (m_r,c - 2) / (m_g,c - 2)

F_g,c(n) = (clear_budget_g / clear_budget_r) * L_r,c(x)
```

`L` linearly interpolates adjacent authored count values with exact arithmetic.
The domain is `2 <= n <= m_g,c`.
All supported ceilings exceed 2, so the denominator is nonzero.

This maps count 2 to reference count 2 and each local series endpoint to its
reference endpoint. It preserves the reference curve's shape between them.
The ordinate scales by the accepted clear-budget ratio.

Equivalently:

```text
F = L + (L / clear_budget_r) * (clear_budget_g - clear_budget_r)
```

The formation contribution is weighted by the reference curve's relative cost,
not charged in full to every target. The supplied reference values are below
their clear budgets. This is a mathematical property of the proposal, not
proof that the transferred estimates match play.

## Explicit assumptions and exclusions

- Equivalent positions within a series retain equivalent relative difficulty.
- The whole authored curve, including its accessibility contribution, scales
  with the clear budget. History does not uniquely establish this scaling.
- Each maximum retains its reference endpoint's relative price. It does not
  mean the same number or kind of enemy units remain.
- In particular, Each 14 on 12x10 is not a full clear. Its relative endpoint
  price is a pacing choice, not an assertion that only one pawn remains.
- The later legacy12 additions remain comparison evidence. They do not become
  mandatory anchors or silent overrides in this candidate.
- This candidate prices numbered capture series only. It does not price
  individual capture roles, Regicide, tactics, or movement goals.

For a one-King geometry, also define `F_Any(N_g) = clear_budget_g`.
This covers a published Any 11/15/19/27 goal on an earlier board in a larger
world. Those captures clear every eligible non-King unit on that board.
The boundary creates no new Location.

No Any 33/34 or Pieces 20 Location is introduced for 12x10.
Two captured Kings still count normally in actual gameplay.
Capture Everything retains its separate starting-set condition.

## Coverage and retrospective comparison

The analysis probe evaluates all 236 feasible in-scope profile/count cases,
including the four earlier-board full-clear boundaries.
Every result is defined and nondecreasing within its series.
Counts 0/1 and infeasible or unpublished larger counts are rejected.

All 74 supplied base8/grand10 anchors reproduce exactly when reference and
target match. This is reproduction by construction, not predictive evidence.

Holdouts use the same source observations and conditional target budgets as
the initial comparison. Errors are intrinsic material units.

| Conditional comparison | Model D domain | Mean absolute error |
| --- | --- | ---: |
| Base8 to grand10 | All 42 points | 321.3 |
| Same, on Model B's defined domain | 36 points | 290.9 |
| Same, common A/B/C domain | 26 points | 264.5 |
| Grand10 to appended legacy12 values | All 10 points | 603.7 |
| Same, common A/B/C domain | 8 points | 698.2 |

Model B's corresponding common-domain errors were 258.9 and 683.5.
Model D therefore does not demonstrate a universal predictive improvement.
Its main advantage is complete coverage with explicit, bounded assumptions.
The old legacy12 budget of 8050 is a retrospective input, not the new target.

## Illustrative outputs

These are `ceil(F)` at difficulty 1, absolute adjustment 0, and without caps.
The function retains fractions internally. Production processing would apply
difficulty and the applicable adjustment before the single final ceiling.

| Geometry | Pawns maximum | Pieces maximum | Each maximum | Any maximum | Clear budget |
| --- | ---: | ---: | ---: | ---: | ---: |
| 6x8 | 2090 (6) | 2211 (5) | 2270 (5) | 2300 (10) | 2370 |
| 8x8 | 3545 (8) | 3750 (7) | 3850 (7) | 3900 (14) | 4020 |
| 10x8 | 5245 (10) | 5400 (9) | 5950 (9) | 6000 (18) | 6050 |
| 10x10 | 6979 (12) | 7186 (15) | 7917 (12) | 7984 (26) | 8050 |
| 12x10 | 9754 (14) | 10042 (19) | 11065 (14) | 11158 (32) | 11250 |

Additional 12x10 examples are Pawns 12/13 at 8576/9010, Pieces 12/15 at
8456/9309, Each 12 at 10817, and Any 22/26/27/28 at
9732/10637/10755/10854.

## Decision and limits

The user selected endpoint-relative pacing and clear-budget scaling, with
explicit correction records. Neither follows uniquely from the historical
rationale. Absolute-count and remaining-work alternatives were not selected.

The [generated series table](capture-series-provisional.csv) records the 137
10x10/12x10 profile rows with exact raw fractions.
Its default-condition example column is not a prematurely rounded input for
later difficulty scaling.

The candidate does not remove the need for later play-test calibration.
No production source, released thresholds, or capture semantics changed.

## Reproduction and sources

The session-only probe is
`C:\Users\aaedi\.copilot\session-state\715de4c1-9c73-42c5-849e-1c5cd8715f82\files\calibration-models\series_endpoint_probe.py`.
It uses Python's standard-library fractions and contains all reference inputs.

Historical curves: sibling `worlds\checksmate\Locations.py:65-108` at
`6fbf68628aa71f51b1803a49536ca63b89f8d02f`.
Legacy12 holdouts: the same path at
`17df6adc6d89af7b88bf2bcd3002c4e66defd535`, lines 115-125, 140-164,
and 187-201. The initial comparison documents the evidence boundaries.
