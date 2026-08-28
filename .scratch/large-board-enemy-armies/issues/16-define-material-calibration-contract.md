# Define the material calibration contract

Type: grilling  
Status: open  
Blocked by: 11, 12, 13, 15

## Question

What exact equation and authored data transform resolved CPU formations into initial ChecksMate material/chessmen expectations while preserving calibrated legacy behavior?

A candidate shape to accept, amend, or reject is:

```text
formation_delta(stage, family)
  = value(resolved CPU inventory) - value(legacy CPU inventory)

local_role_delta(location_role, stage, family)
  = value(resolved inventory relevant to location_role)
  - value(legacy inventory relevant to location_role)

provisional(location_role, stage, family)
  = legacy_threshold(location)
  + role_adjustment(local_role_delta)
  + formation_adjustment(formation_delta)
  + authored_offset

released_threshold
  = stage_cap(monotone_clamp(round(provisional)))
```

The terms and weights above are placeholders. In particular, this ticket must decide where a local-role delta is sufficient and where a whole-formation delta is relevant; it must not silently apply the total inventory change to every individual capture.

## Observations from current Locations

`worlds\checksmate\locations.py:35-39` describes material expectations as generally upper-end estimates for individual captures, capture series, and fork/pin goals. The current values are authored compatibility data, not output from a documented universal equation.

### Individual pawn material

| File | Base | Grand |
|---|---:|---:|
| A | 490 | 1010 |
| B | 340 | 810 |
| C | 220 | 660 |
| D | 100 | 520 |
| E | 100 | 320 |
| F | 320 | 320 |
| G | 390 | 620 |
| H | 490 | 860 |
| I | — | 810 |
| J | — | 890 |
| K | — | 970 |
| L | — | 1050 |

`material_requirement()` at `locations.py:43-59` selects base or grand values from expansion/flags and otherwise uses `min(base, grand)`. The arrays therefore encode geometry-sensitive authored asymmetry rather than a simple file-independent pawn price.

### Pawn capture series

| Captures | Material | Availability note |
|---:|---:|---|
| 2 | 750 | ordinary series |
| 3 | 1450 | ordinary series |
| 4 | 2240 | ordinary series |
| 5 | 2620 | ordinary series |
| 6 | 2975 | ordinary series |
| 7 | 3255 | ordinary series |
| 8 | 3545 | ordinary series |
| 9 | 4645 | stage-gated |
| 10 | 5245 | stage-gated |
| 11 | 5845 | stage-gated |
| 12 | 6445 | stage-gated |

Other current commentary records:

- edge pawns are expected to remain defended longer (`locations.py:64,72`);
- queen-side captures have tempo/development asymmetry (`locations.py:66,75`);
- bishops and rooks may be less deployable (`locations.py:73`);
- some goals receive `+4` material because they should not be guaranteed early (`locations.py:96`);
- `worlds\checksmate\rules.py:106-126` caps a threshold by projected stage maximum material;
- checkmate targets are 4020 for Minima, 6020 for Maxima/10x10, and 8020 for 12x10/12x12.

The current code has no semantic distinction between a forward pawn and a blocked rear pawn. File and stage asymmetry are evidence that accessibility matters, but do not identify which formation role causes a given premium.

Scope distinction: the 12x12 values and stage references above are current-source compatibility facts. Calibration targets cover the supported augmented geometries 10x10 and 12x10 within the formal set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`; this ticket must not create a 12x12 baseline or target.

## Correction trajectory

These commits show repeated authored calibration and correction. The classifications describe their place in this ticket's evidence; they do not add motives beyond the recorded subjects/changes.

| Class | Commit | Date | Subject / recorded change |
|---|---|---|---|
| Schema establishment | `31b9826` | 2023-11-25 | `Move required material into Locations` |
| Schema establishment | `2144259` | 2024-06-09 | added more capture locations for pawn-heavy distributions |
| Calibration | `a5ffe61` | 2024-08-16 | `improve midgame pawn structure difficulty curve` |
| Correction | `29c2629` | 2024-09-22 | `lower 4+ series difficulties` |
| Correction | `1a14cb9` | 2024-11-18 | corrected outdated assumptions in chessmen expectation comments |
| Correction | `c1f53b7` | 2024-12-22 | corrected rook-vs-two-piece and series/cap difficulty assumptions |
| Calibration | `625d2df` | 2024-12-22 | raised queen/fork/series values |
| Geometry migration | `17df6ad` | 2026-07-19 | added APMW v2 geometry expansion and stage-aware grand expectations |
| Geometry migration | `108a5ca`, `61fec12` | 2026-07-20 | path/integration migration |

The trajectory argues for an explicit, reviewable calibration contract while retaining authored escape hatches. It does not prove one equation or explain the rationale for values beyond what the source and subjects state.

## Rearguard-pawn hypothesis

In a doubled/advanced pair, the pawn behind the forward pawn is blocked by that pawn and may remain defended or inaccessible longer. Current user intent therefore treats the blocked rear pawn as the likely distinctive harder capture and proposes `rearguard pawn` as its candidate semantic name. The forward pawn does not receive a unique premium or name merely because it is advanced.

Current Locations commentary indirectly supports testing accessibility, defense duration, tempo, and deployment as pricing factors. Edge-pawn asymmetry, queen-side asymmetry, upper-end capture estimates, and stage-gated series are compatible with a harder rear capture, but none identifies a forward/rear pair or assigns its premium. No current code or retained history hit uses `rearguard`; that absence is negative evidence against treating the term as inherited vocabulary, not evidence against testing the model.

The role ticket must decide whether rearguard identity is defined by the starting formation relationship. This material ticket must then compare an authored rearguard-local adjustment against any whole-formation adjustment. Both remain unresolved until exact arrays and role identity are settled.

## Resolution requirements

- Choose midgame, endgame, weighted, or other catalog value basis.
- Define the authored legacy baseline for every retained 10x10 and 12x10 Location and state which values are preserved unchanged.
- Decide whether thresholds vary by selected Enemy Army or conservatively aggregate across families.
- Define and separately calculate local-role deltas and whole-formation deltas.
- State which goal classes use only local-role deltas, only whole-formation deltas, a bounded combination, or no automatic delta: individual captures, aggregate captures, series, tactics, and checkmate.
- Prevent an unrelated inventory increase from being charged wholesale to every individual capture; any formation-wide contribution needs an explicit rationale and bound.
- Test the `rearguard pawn` hypothesis against forward-pawn and ordinary-pawn baselines without presuming that either pawn receives the premium.
- Define pawn-unit rounding and acceptable error bands.
- Preserve or intentionally replace each hand-authored `material_expectations_grand` and `chessmen_expectations` baseline; replacement requires an explicit old-to-new ledger rather than regeneration by implication.
- Decide whether 12x10 keeps the legacy 8020 checkmate target it currently shares with 12x12 or separates from that value, without treating 12x12 as supported or calibrated.
- Apply the projected-stage maximum-material cap from `rules.py:106-126`, and specify whether rounding and monotonic repair occur before or after that cap.
- State monotonicity constraints across capture counts, stage availability, projected stage material, and difficulty scaling. Any allowed plateau or exception must be explicit rather than an accidental clamp effect.
- Key individual-capture calibration to the role-aware Location identity resolved by the semantic-location ticket, including a rule that formation identity survives movement and current square does not reprice the Location.
- Define how role renames, aliases, or profile-version migrations preserve the authored baseline and numeric Location compatibility.
- Specify where authored offsets and later play-test adjustments live and how they are reviewed.
- Produce complete provisional 10x10 and 12x10 target tables with columns for authored baseline, local-role delta, whole-formation delta, authored offset, monotonic adjustment, stage cap, and released threshold. Do not run play tests in this ticket.

## Provenance ledger

| Evidence class | Claims carried into this ticket | Status |
|---|---|---|
| Current user intent | White's 12x10 Queen-role is around F1/F2 while the primary King-role is around G1; the blocked rear pawn is the stronger candidate for a distinctive premium and the term `rearguard pawn`. | Authoritative intent and strong working hypotheses; exact arrays, canonical vocabulary, and calibration remain open. |
| Current source | Authored pawn/series values, commentary about upper-end estimates and accessibility, `material_requirement()` selection, stage caps, and current checkmate targets. | Current compatibility evidence; not a complete calibration equation. |
| Git history | The listed schema, calibration, correction, and geometry-migration commits. | Evidence of trajectory only; do not infer unrecorded rationale. |
| Rejected or absent historical evidence | Prior Amazon/Queen “around G1/G2” wording is superseded; current code/history has no forward/rearguard semantic split and no retained `rearguard` hit. | Must not be presented as source-backed historical vocabulary or settled occupancy. |
