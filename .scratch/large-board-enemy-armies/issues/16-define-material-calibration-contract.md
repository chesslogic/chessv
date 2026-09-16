# Define the material calibration contract

Type: grilling  
Status: resolved  
Blocked by: 11, 12, 13, 15

## Question

What function and authored data transform resolved CPU formations into
ChecksMate material/chessmen expectations while preserving prior calibration
effort?

## Decision record

### Current resolution at Q35

The user accepted the released non-pawn reference bank and compact transfer.
They selected the smooth pawn curve and central-pressure model, but requested
an additional Rearguard premium.
They accepted the three-quarter Queen-to-checkmate gap for Regicide.
In round 13, they selected a Rearguard premium equal to one quarter of the
same-file main pawn's accessibility budget.

| Claim | Class | Authority | Excludes |
| --- | --- | --- | --- |
| The released analogue bank supplies non-pawn references, including Knight analogues for Lions | Author decision | Round 12, Q27 | The post-release Outer Attendant inheritance |
| Compact accessibility scales down without a positive-growth floor | Author decision | Round 12, Q27 proposal and selected bank | An unchanged base8 requirement on compact 6x8 |
| Pawns use the shape-preserving curve and the proposed central-pressure law | Author decision | Round 12, Q28: "Use the curve model but specify an additional Rearguard premium" | Straight-segment interpolation as the selected curve |
| Rearguard pawns receive an additional premium | Author decision | Round 12, Q28 | The zero-premium proposal |
| Rearguard adds `(main_pawn_baseline - 100) / 4` | Author decision | Round 13, Q30 | Zero premium, fixed 100/300, or 25% of the whole price |
| Regicide uses three quarters of the Queen-to-checkmate gap | Author decision | Round 12, Q29 | The earlier 4725 price and a full checkmate budget |
| Released inputs are present in the published world | Evidence | Release-provenance audit | Universal proof of gameplay calibration |

The provisional numeric model choices are settled.
The [individual-capture report](../research/individual-capture-model.md) and
[105-row CSV](../research/individual-capture-provisional.csv) contain the exact
individual and Regicide profiles.
The [cost-snapshot schema](../contracts/world-cost-snapshot-v1.md) supplies
correction storage and the exact-to-effective processing boundary.
The [shared-data specification](../contracts/shared-data-contract-v4.md)
supplies the remaining schema bindings and preserves tactic/castler
predicates within complete paths.
Q35 confirms the design and its initial calibration estimates.
This resolved record does not authorize implementation or play-test tuning.

The report's
[single-ceiling fixture](../research/individual-capture-model.md#single-ceiling-fixture)
uses intrinsic `361045/162`, difficulty `27/20`, and adjustment 240.
The correct uncapped result is 3249, not the 3250 from premature rounding.
A synthetic 3000 cap gives 3000 only after the correct final ceiling.

### 2026-09-13: Standard-family reference

The user chose the Standard (FIDE) army as the Location-calibration reference.
Each geometry uses its approved Standard-family formation, including its
specified augmentation.

Requirements do not vary with the player's Enemy Army dropdown choice and do
not take a conservative maximum across the four families. Other families can
make individual Locations easier or harder; that variation is an intentional
player choice and is outside the generator's Location-difficulty guarantee.

This preserves the free family selector. It does not remove the requirement
for valid gameplay arrays and rules for all four families.

### 2026-09-13: Midgame values

The user selected midgame catalog values, with 100 material units per pawn
unit. Endgame values and an unspecified blend are not the arithmetic basis.
Authored baselines and offsets remain part of the accepted hybrid method.

[Standard-army Location calibration](../../../docs/adr/0004-standard-army-location-calibration.md)
records these decisions. The remaining equation, adjustments, rounding,
monotonicity, and stage caps are still open.

### 2026-09-14: Reconstruct calibration from authored history

The user requested that the function come from existing Locations and Git
blame, rather than accepting a new weighting by assumption.

The proposed individual-capture rule was:

```text
authored baseline + 1:1 target-value delta + explicit positional offset
```

The user did not accept that equation or its exclusion of a formation term.
Their direction was to "derive a function based on the existing locations
and the git blame".

The research must recover quantitative relationships, comments, corrections,
and material/chessmen assumptions from primary source history.
It must distinguish recovered intent from a new fit or an unsupported guess.
New targets and remaining user choices follow that reconstruction.

The accepted role catalog and capture-series rules are in ticket 15.
They supply the target semantics, not evidence for a particular difficulty
function. This research does not authorize play tests or production changes.

### 2026-09-14: Source-history reconstruction completed

[Material calibration history](../research/material-calibration-history.md)
contains the source and commit evidence, residual comparisons, and runtime
order. These findings are evidence, not new author decisions.

The November 2023 individual-piece requirements exactly reconstruct as:

```text
historical_requirement =
    historical_piece_value
    + 400 accessibility material
    + 340 for a kingside counterpart
```

The historical Rules basis was 300 for a minor, 500 for a Rook, and 900
for Queen. These are not the current catalog's 325/500/950 values.
The 400-unit increment explicitly represented an extra piece and pawn.
Later bishop and Rook corrections add residuals that the original formula
does not explain.

All 52 current numbered-series chessmen requirements follow captures minus
one. Of Each requires `2*n - 1`. The history connects this allowance to the
player's starting King. It does not establish an automatic 325-material
discount or one discount per enemy King.

The series history explicitly reasons backward from the endgame.
It does not define the remaining gap as uncaptured enemy material.
The old 12-file checkmate increment is approximate: its comment budgets about
1000 per outer attendant, while the actual added inventory delta is 1300.

The recovered model separates resource budgets and authored accessibility
adjustments. It does not establish a universal curve, a zero formation
contribution, or a unique continuation for the new counts.

### 2026-09-14: Preserve the capture-goal chessmen heuristic

The user retained captures-needed minus one for capture goals, before
projection caps. This estimates the player's non-primary chessmen.
It does not change the actual capture counters from ticket 15.

```text
required_player_chessmen =
    required_capture_count - 1
```

For Of Each `n`, the required capture count is `2*n`.
For Capture Everything, it is the ending formation's normal full-clear count.
An individual capture or Regicide requires one capture and therefore has a
zero additional-chessmen estimate.

The allowance is for the player's starting King, not for each enemy King.
A player who completes a capture below the estimate still earns the check.
Material and actual goal conditions remain separate.

| Goal | Required captures | Player-chessmen estimate before caps |
| --- | ---: | ---: |
| Capture 19 Pieces | 19 | 18 |
| Capture 14 Of Each | 28 | 27 |
| Capture Everything on 12x10 | 33 | 32 |
| Regicide | 1 | 0 |

### 2026-09-14: Geometry-matched calibration profiles

The user selected explicit geometry-matched reference profiles.
The function derives intrinsic requirements from these profiles and retains
documented corrections. Difficulty, projection caps, and access across playable
boards remain separate steps.

The function must not take a blanket minimum that borrows a cheaper
requirement from an unavailable board. An explicitly transformed historical
reference is different from a playable-board access path.

This answer does not remove player piece/pawn configuration factors.
It also does not settle whether the later-stage resource bypass remains.
[Separate calibration and reachability](../../../docs/adr/0008-separate-calibration-from-reachability.md)
records these decisions.

### 2026-09-14: Provisional endgame budgets

The user selected the established 8x8 and 10x8 anchors with exact Standard
inventory deltas. `V` includes all starting CPU midgame values, including Kings.

```text
budget(target) =
    budget(reference) + V(target) - V(reference)
```

Use the 8x8 reference for 6x8/8x8 and the 10x8 reference for larger boards.
The reference pairs are 4020/4020 and 6020/6050 for checkmate/Everything.

| Geometry | CPU V | Checkmate | Capture Everything |
| --- | ---: | ---: | ---: |
| 6x8 | 2725 | 2370 | 2370 |
| 8x8 | 4375 | 4020 | 4020 |
| 10x8 | 6400 | 6020 | 6050 |
| 10x10 | 8400 | 8020 | 8050 |
| 12x10 | 11600 | 11220 | 11250 |

The user chose these provisional intrinsic values, before difficulty and caps,
with no additional new premium yet. The King upgrade and spare life already
contribute their catalog values. This does not apply the same delta wholesale
to individual captures.

The old twelve-file approximation contributes an extra 700 beyond its actual
inventory increment. The user did not retain that surcharge.
The rejected alternative gives 11920/11950 instead of 11220/11250.

[Provisional endgame budgets](../../../docs/adr/0009-anchor-provisional-endgame-budgets.md)
records the function and anchors. Later tuning still requires explicit authored
adjustments and does not rewrite this decision by implication.

### 2026-09-14: Retain the later-board resource bypass

The user retained the existing bypass for material and player-chessmen
estimates. It does not waive the actual goal condition, geometry eligibility,
or special tactic/castling conditions.

No later-board shortcut exists beyond the configured endpoint.
The new contract cannot use an unsupported 12x12 stage as a shortcut.

The user suggested a Regions refactor, then deferred it from 0.4.0.
The [Regions decision](20-decide-region-based-reachability.md) retains the
door-specific threshold intent without changing the graph structure.
The existing Location rule must evaluate complete per-geometry alternatives,
not combine separate conditions from different boards.

### 2026-09-14: Unit precision and explicit monotonicity

The user retained one-material-unit precision, equal to 0.01 pawn.
Apply difficulty and the applicable absolute adjustment before rounding upward
once. Apply the projected-stage maximum-material cap last.

```text
effective_material_requirement =
    min(stage_material_cap,
        ceil(intrinsic_material * difficulty + applicable_absolute_adjustment))
```

The existing absolute-adjustment condition remains explicit in the runtime
layer. Player-chessmen estimates receive their separate count cap.

Within each capture series, requirements must not decrease as the target count
increases. Conflicts require an explicit correction, not silent changes to
authored anchors. Different goal classes and different geometries do not
require one common numeric ordering.

### 2026-09-14: Accept endpoint-anchored series transfer

The [initial model comparison](../research/capture-series-model-candidates.md)
examines absolute-count tails, normal-clear progress, and count-deficit gaps.
Those candidates either leave required counts undefined or need further
continuation decisions. The user did not select those three candidates.

The user selected the
[endpoint-anchored model](../research/capture-series-endpoint-model.md)
as the provisional function for the five supported geometries.
It maps the interval from count 2 through each series ceiling onto the
corresponding reference interval, then scales by the clear-budget ratio.

This is an accepted pacing rule, not a recovered historical equation.
It preserves the authored 8x8/10x8 references by construction.
It does not make the later legacy12 additions mandatory anchors.
Its coverage and holdout comparisons are recorded in the report.

```text
x = 2 + (n - 2) * (reference_ceiling - 2) / (target_ceiling - 2)

intrinsic_series_material =
    interpolate(reference_curve, x)
    * target_clear_budget / reference_clear_budget
    + explicit_correction
```

The initial correction entries are zero. Later corrections require explicit
records and apply before runtime difficulty, absolute adjustment, rounding,
and caps. A correction must not silently break within-series monotonicity.

The reference is base8 for 6x8/8x8 and grand10 for larger boards.
The target ceiling is the target formation's local calibration coordinate,
not a replacement for the world's published series ceiling.
A one-King full-clear boundary prices a published Any 11/15/19/27 goal on
an earlier board at that board's clear budget.

[Transfer capture-series curves](../../../docs/adr/0010-transfer-capture-series-curves-by-endpoints.md)
records the accepted function and its limits.

The [series-only provisional table](../research/capture-series-provisional.csv)
contains 137 geometry/goal rows for 10x10 and 12x10.
It includes reference brackets, exact fractional intermediates, the formation
contribution, zero initial corrections, and uncapped player-chessmen estimates.
The Any 27 row is an earlier-board full-clear boundary, not a new endpoint
Location.

The CSV's `example_final_d1_r0_no_cap` column is illustrative.
Actual difficulty, absolute adjustment, and world-specific caps are not frozen
by that column. The raw fraction is the input to final processing.

The later individual-capture report supplies their separate equations and
105-row table. Series and individual captures do not share one equation.

### 2026-09-14: Retain task-specific budgets

The user selected the authored tactical, survival, and King-action reference
values, with no automatic army-wide increase.
Base8 supplies 6x8/8x8 defaults. Grand10 supplies the larger-board defaults.
True Royal Fork instead follows the target geometry's checkmate budget.

The table gives intrinsic material and authored player-chessmen estimates.
Difficulty, caps, tactic settings, and actual goal conditions remain separate.

| Goal | 6x8 | 8x8 | 10x8 | 10x10 | 12x10 | Chessmen before cap |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| King to E2/E7 Early | 0 | 0 | 0 | 0 | 0 | 0 |
| King to Center | 50 | 50 | 50 | 50 | 50 | 0 |
| King to A File | 0 | 0 | 150 | 150 | 150 | 0 |
| King Captures Anything | 150 | 150 | 350 | 350 | 350 | 0 |
| King to Back Rank | 2250 | 2250 | 5150 | 5150 | 5150 | 0 |
| Survive 3 Turns | 0 | 0 | 0 | 0 | 0 | 0 |
| Survive 5 Turns | 200 | 200 | 330 | 330 | 330 | 2 |
| Survive 10 Turns | 2500 | 2500 | 4500 | 4500 | 4500 | 9 |
| Survive 20 Turns | 3800 | 3800 | 5800 | 5800 | 5800 | 15 |
| Threaten Pawn | 0 | 0 | 0 | 0 | 0 | 0 |
| Threaten Minor | 200 | 200 | 400 | 400 | 400 | 0 |
| Threaten Major | 300 | 300 | 500 | 500 | 500 | 0 |
| Threaten Queen | 300 | 300 | 500 | 500 | 500 | 0 |
| Threaten King | 1000 | 1000 | 1800 | 1800 | 1800 | 0 |
| Fork, Sacrificial | 700 | 700 | 1100 | 1100 | 1100 | 6 |
| Fork, Sacrificial Triple | 3300 | 3300 | 2700 | 2700 | 2700 | 9 |
| Fork, Sacrificial Royal | 3600 | 3600 | 5200 | 5200 | 5200 | 12 |
| Fork, True | 3150 | 3150 | 4550 | 4550 | 4550 | 10 |
| Fork, True Triple | 3850 | 3850 | 5850 | 5850 | 5850 | 12 |
| Fork, True Royal | 2370 | 4020 | 6020 | 8020 | 11220 | 14 |
| O-O Castle | No path | 0 | 0 | 0 | 0 | 2 |
| O-O-O Castle | No path | 0 | 0 | 0 | 0 | 2 |

Survival rows retain their canonical `Current Objective: ` name prefix.
The abbreviated table labels do not rename those Locations.

The 6x8 profile disables castling for gameplay. No zero-cost reference value
can override that capability limit. A 6x8-only world has no qualifying
castling path.

Source values: sibling `worlds\checksmate\locations.py:186-192,295-317`.
Castling capability: `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:308-314`
and `ApmwChess.cs:413-452`.

[Task-specific calibration](../../../docs/adr/0011-preserve-task-specific-calibration.md)
records the selected policy.

### 2026-09-14: Increase the low-end accessibility buffer

The user did not accept the preceding individual-capture formula unchanged.
They clarified that individual captures become significantly harder on large
boards because other material obstructs access.

Sliders can travel farther and tend to enter play earlier.
Jumping or other non-sliding targets need a stronger accessibility buffer.
Earlier-board capture opportunities make this less important for many players,
but do not remove the need for credible large-board requirements.

The user considered many example budgets broadly plausible, but said that
the low end needs to start substantially higher.
No numeric buffer coefficient or complete individual-capture equation is
accepted by that feedback alone.

## Accepted non-pawn growth-buffer refinement

The user selected this refinement in the next interview answer.
It applies to non-pawn starting-role targets in the Standard reference army.
Pawn pricing, new-role reference selection, and Regicide's baseline remain
separate choices.

Let `A_ref = authored_reference_requirement - reference_target_MG`.
The earlier proportional proposal uses `A_ref * B_new / B_ref`.
Its growth is small for a Knight because that role's reference accessibility
budget is much smaller than a Rook's or Queen's.

For expanded targets with positive `other_growth`, the accepted refinement puts
a floor on incremental accessibility growth:

```text
other_growth =
    (new_starting_CPU_MG - new_target_MG)
    - (reference_starting_CPU_MG - reference_target_MG)

scaled_accessibility = A_ref * B_new / B_ref

accessibility =
    max(scaled_accessibility, A_ref + beta * other_growth)

proposed_requirement =
    new_target_MG + accessibility + explicit_position_correction
```

The coefficients are one-half for bounded-range non-pawns and
one-quarter for pieces with a board-spanning sliding component.
The user selected these new calibration coefficients. They are not recovered
historical constants. Slider/leaper hybrids belong to the sliding group.
Classification uses the Standard starting target, not the selected enemy
family or a promoted piece's current type.

The positive-growth floor is settled. The compact no-growth/shrinking transfer
shown in the earlier proposal still needs explicit reference treatment.

The buffer uses the starting formation. It does not shrink as pieces are
captured during play. The maximum selects one growth budget instead of
stacking both.

| 12x10 target | Earlier proportional example | Accepted growth floor |
| --- | ---: | ---: |
| Queen's Knight | 1953 | 3800 |
| King's Knight | 2324 | 4000 |
| Queen's Bishop | 1953 | 2500 |
| King's Bishop | 2324 | 2700 |
| Queen's Rook | 4870 | 4870 |
| Queen | 6808 | 6808 |

Examples use difficulty 1, absolute adjustment 0, no caps, and no additional
position correction. Final processing still follows the accepted precision
policy.

[Non-pawn accessibility growth](../../../docs/adr/0012-floor-non-pawn-accessibility-growth.md)
records the selected coefficients and scope.

## Reference provenance gate and latest feedback

### 2026-09-14: Audit every reference against 0.3.2

The user said: "We should check git blame for each role - some of these are
introduced after 0.3.2 and have never been tested".

The reference bank is not accepted. Each record needs its introduction,
material-value history, released-baseline status, and calibration evidence.
Source presence, an automated test of a constant, and gameplay calibration are
different evidence classes.

The audit must establish the actual release boundary and corresponding
producer revision where possible. Date proximity alone does not establish
cross-repository release membership. Pre-boundary presence also does not
prove that a value was play-tested.

This review includes the proposed Lion/Outer Attendant references, pawn curve
points, and newer series or task inputs. It must not promote an untested
addition into a trusted anchor merely because the entry already exists.

The selected mathematical policies remain provisional decisions.
The generated series table records their output for the supplied inputs,
not evidence that those inputs were released or play-tested.
Replacing an input requires an explicit decision and regeneration of the
affected derived values.

### Reference-provenance audit completed

The [per-reference audit](../research/calibration-reference-provenance.md)
compared the published Client 0.3.2 world asset with source history.
The client tag resolves to `6fe99970b574f11e35d52e080c03a5eefbdeedbb`.
The released world's 32 tracked files are source-equivalent to producer
`24c1a267f5c65362c9efb4e13e6f76e86564d699` after newline normalization.
That producer commit is titled `Bump ChecksMate client version to 0.3.2`.
Source equivalence does not prove a unique packaging commit.

The exact `v0.3.2` tag is absent from the sibling's local tags and the
`origin`/`apmw` tag references examined. Its bare `0.3.2` tag identifies an
unrelated upstream Archipelago release, not the ChecksMate calibration baseline.

| Reference set | Released 0.3.2 finding |
| --- | --- |
| Seven base8 and nine grand10 non-pawn references | Present at the current values |
| Base pawn A-H and grand pawn A-J controls | Present at the current values |
| All 74 selected base8/grand10 series points | Present, including the high-count grand10 endpoints |
| Retained task-specific base/grand budgets | Present at the current values |
| Outer Attendant 4110/4190 | Absent. Added after the release |
| Pawn K/L 970/1050 | Absent. Later continuation of the numeric run |
| Ten legacy12 series additions and old clear8050 | Post-release additions |

The released asset's SHA-256 is
`6c0f81ad139c71d3bf98ba4c521741b238b6b751920c38579dd6f37beaf2867d`.
The parent independently retrieved the asset and confirmed its digest,
representative released values, and the absent Outer Attendant/K/L entries.

The selected series bank is now confirmed as released data. This audit does
not require changing its inputs or regenerating its derived table.
Some historical changes mention feedback, but no located version-matched
gameplay study establishes every listed price's accuracy.

The Lion6035/6115 proposal cannot be justified as preservation of released
calibration. The released Knight1200/1400 pair is a possible replacement
analogue, not an automatically approved mapping.
The proposed pawn model uses released base8/grand10 control points, but its
curve and pressure law remain unaccepted.

### 2026-09-14: Pawn file costs need a curve

The user expects a curved relationship between starting file and capture cost.
This does not approve the proposed gamma coefficient, interpolation method,
or zero additional Rearguard premium.

The prior proposal separates a normalized file coordinate from an authored
cost curve, rather than using one straight cost ramp across the board.
Its exact curve shape and trustworthy control points remain open pending
the provenance audit.

### 2026-09-14: Regicide is a major feat relative to Queen capture

The user considers capturing even one King a significant feat relative to
capturing a Queen. The proposed 4725 budget is not accepted.
Nominal King material cannot justify treating Regicide as a cheap
Queen-capture alternative.

Its royal-capture premium or relative progression position needs a new
decision after the Queen-capture reference is established.
This does not change either-King completion, starting-setup eligibility, or
the accepted zero additional-player-chessmen estimate.
It also does not change the King's catalog value.

## Earlier reference and pawn proposals

These paragraphs preserve earlier proposals. The round 12 decisions that
follow select the reference bank, curve, and Regicide relationship.
The earlier Lion inheritance, straight-segment curve, and zero Rearguard
premium are not selected.

### Non-pawn reference bank

The audit confirms the base8/grand10 entries in the release, but not the later
Outer Attendant entries. This table retains the earlier proposal.
Its Lion row must not be treated as an established released reference.

| Target | Proposed reference |
| --- | --- |
| Existing home roles on 8x8 | Their authored base8 capture records |
| Existing home roles on 10x8 and larger | Their authored grand10 counterparts |
| Forward Bishop/Knight | The same-side home Bishop/Knight grand10 record |
| Basic Elephant | The same-side grand10 Knight record, with target MG 250 |
| Amazon | The grand10 Queen record, with target MG 1300 |
| Queen's/King's Lion | Their former legacy12 Outer Attendant records |
| Compact Center Rook | The base8 Queen's Rook record, as a calibration analogue only |

For Lions, the historical records are 4110/4190, target Nightrider value 550,
reference CPU material 7700, and reference clear budget 8050.
This reference context does not change the accepted new 12x10 clear budget of
11250. The selected floor gives proposed Lion requirements of 6035/6115.

A different movement analogue, the grand10 Knight, would instead give
3888/4088. Selecting either pair is a calibration decision, not a change to
the Lion capture roles.

For compact 6x8, the proposal scales the reference accessibility by 2370/4020
without a positive-growth floor. The Center Rook example becomes 1090.
No additional positional correction is proposed at this initial stage.

Examples use difficulty 1, adjustment 0, and no cap. They retain unrounded
values internally.

### Pawn pressure and starting-file shape

Unaccepted. The base8/grand10 controls are now confirmed in the release.
Curve shape, pressure transfer, and Rearguard treatment remain open.

The reference central-pawn values are 100 on 8x8 and 320 on grand10.
The corresponding Standard CPU midgame totals are 4375 and 6400.
These give a candidate common-pressure coefficient:

```text
gamma = (320 - 100) / (6400 - 4375) = 44/405

central_pawn_budget(g) =
    100 + gamma * max(0, CPU_starting_MG(g) - 4375)
```

This is a proposed extrapolation from the reference values, not a recovered
historical law.

Map each pawn's starting file between center and edge within its board half.
Interpolate the corresponding authored reference half without mirroring away
the queen-side/king-side asymmetry.
Use base8 for 6x8/8x8 and grand10 for larger boards.

```text
spatial_premium =
    interpolated_reference_pawn_requirement
    - reference_central_pawn_requirement

proposed_pawn_requirement =
    central_pawn_budget(g)
    + spatial_premium * clear_budget(g) / clear_budget(reference)
    + explicit_position_correction
```

The separation prevents scaling the reference central buffer and then adding
the same growth mechanism again. It preserves both reference pawn curves
exactly on their original boards.

For the eight noncentral grand10 files, a conditional base8-to-grand10
comparison has mean absolute error 68.7 material units and maximum error 156.2.
The two central values determine gamma and are not held-out observations.

The initial proposal gives front and Rearguard pawns on the same file the
same material baseline. Rearguard identity remains distinct. An additional
screen premium requires an explicit correction rather than an invented
coefficient.

| Proposed pawn example | 10x10 | 12x10 |
| --- | ---: | ---: |
| Queen's Rook Pawn, A2 | 1456 | 2168 |
| Queen-side main/rear pair, B3/B2 | 1190 | 1871 |
| Central Queen's/King's Pawns | 538 | 885 |
| King-side main/rear pair, I3/I2 or K3/K2 | 1190 | 1826 |

These examples use difficulty 1, adjustment 0, and no cap.
The 10x10 main B3 pawn is Queen's Knight Pawn. The 12x10 main B3 pawn is
Queen's Lion Pawn. Pricing from a starting file does not restore file-based
Location identity or reprice a pawn after movement.

### Regicide reference proposal

Not selected. The latest feedback requires a stronger royal-capture basis
relative to Queen capture. The following records the earlier proposal only.

No historical Regicide requirement exists.
The proposed reference combines the grand10 Threaten King material budget of
1800 with the reference King's nominal value of 325.
This makes 1800 an explicit approach-budget analogue, not a copied capture
requirement.

Apply the accepted bounded-range floor separately to each Standard starting
King witness, then take the cheaper qualifying witness because either King
completes Regicide. The ordinary-King result is 4725. The Mounted-King result
is 4913 after the illustrative final ceiling.

The resulting proposed Regicide baseline is 4725 before actual runtime
adjustments and caps. Its player-chessmen estimate remains zero.
This analogy does not require the Threaten King Location to be completed or
inherit its special access predicates.

## Round 12 decisions after the release audit

These are accepted provisional model choices. They are not evidence of
gameplay calibration. Round 13 supplies the additional Rearguard premium.

### Released-reference mapping

Use the released base8/grand10 banks for existing roles and the already
proposed forward-minor, Elephant, Amazon, and compact analogues.
Replace the proposed Lion inheritance with the released same-side Knight
references, rather than post-release Outer Attendant constants.

The user accepted this bank in Q27, including the compact downward transfer
and initial zero additional non-pawn position corrections.
[Released individual references](../../../docs/adr/0013-use-released-individual-capture-references.md)
contains the complete reference values and mapping rules.

The selected growth floor then gives 3888/4088 for the 12x10 Lion pair at
difficulty 1, adjustment 0, and without caps. The unrounded values are
7775/2 and 8175/2. The target remains Lion and keeps its own capture identity.

### Smooth pawn-cost curves

Keep the released base8 A-H and grand10 A-J controls.
Do not use the post-release K/L continuation as historical curve anchors.

The selected refinement uses shape-preserving piecewise cubic Hermite
interpolation independently on each board half. It passes through the
reference controls and does not overshoot their local intervals.
It preserves the king-side dip rather than imposing a symmetric global U.

The user selected the curve model in Q28, including the central-pressure
term and spatial-premium formula with `gamma = 44/405`.
They requested an additional Rearguard premium instead of the proposed zero.
Round 13 selected `(main_pawn_baseline - 100) / 4`.
Other initial additional pawn position corrections are zero.

| Selected 12x10 pawn baseline before the Rearguard premium | Smooth-curve example |
| --- | ---: |
| Queen's Rook Pawn, A2 | 2168 |
| Queen's Lion/Rearguard Pawn, B3/B2 | 1863 |
| Queen's/King's central Pawns | 885 |
| King's Lion/Rearguard Pawn, K3/K2 | 1803 |
| King's Rook Pawn, L2 | 1945 |

These examples apply the final ceiling at difficulty 1 without caps.
Exact fractions remain intact during interpolation and pressure scaling.

#### Exact curve definition

Each reference half has unit-spaced control coordinates from center to edge.
The controls preserve the released values:

| Reference half | Controls from center to edge |
| --- | --- |
| Base8, Queen side | 100, 220, 340, 490 |
| Base8, King side | 100, 320, 390, 490 |
| Grand10, Queen side | 320, 520, 660, 810, 1010 |
| Grand10, King side | 320, 620, 860, 810, 890 |

For zero-based file `f`, let `h = target_width / 2`.
The Queen-side distance is `h - 1 - f`.
The King-side distance is `f - h`.
Each distance maps to reference coordinate
`x = distance * (reference_half_length - 1) / (h - 1)`.

For control values `y[i]`, define `d[i] = y[i+1] - y[i]`.
An interior tangent is zero when adjacent differences have opposite signs
or either difference is zero.
Otherwise it is their harmonic mean:
`s[i] = 2*d[i-1]*d[i] / (d[i-1] + d[i])`.

The first endpoint starts with `s[0] = (3*d[0] - d[1]) / 2`.
If its sign differs from `d[0]`, or either is zero, its tangent is zero.
If `d[0]` and `d[1]` have opposite signs, its magnitude cannot exceed
`3*abs(d[0])`. The limiting tangent is `3*d[0]`.
The last endpoint uses the same rule with the last two differences reversed.

Within interval `i`, let `t = x - i`:

```text
curve(x) =
    (2*t^3 - 3*t^2 + 1) * y[i]
    + (t^3 - 2*t^2 + t) * s[i]
    + (-2*t^3 + 3*t^2) * y[i+1]
    + (t^3 - t^2) * s[i+1]

main_pawn_baseline(g, f) =
    100 + (44/405) * max(0, CPU_starting_MG(g) - 4375)
    + (curve(x) - reference_central_price)
      * clear_budget(g) / clear_budget(reference)

rearguard_requirement(g, f) =
    main_pawn_baseline(g, f) + (main_pawn_baseline(g, f) - 100) / 4
```

The Rearguard premium is intrinsic material, before runtime processing.
It does not require capture of the front pawn or change the one-capture
player-chessmen estimate of zero.
The starting role determines this premium after movement or promotion.

#### Round 13: Additional Rearguard premium

The user selected one quarter of the same-file main pawn's accessibility
budget, excluding its 100-unit target value.
The coefficient applies to intrinsic material, not an already rounded or
capped threshold.

| Geometry and side | Exact main baseline | Exact additional premium | Exact Rearguard requirement | Example final requirement |
| --- | --- | --- | --- | ---: |
| 10x10, either side | 11656010/9801 | 5337955/19602 | 28649975/19602 | 1462 |
| 12x10, Queen side | 127759090/68607 | 60449195/137214 | 315967375/137214 | 2303 |
| 12x10, King side | 146038/81 | 68969/162 | 361045/162 | 2229 |

The examples use difficulty 1, adjustment 0, and no cap.
The fixed 100/300 alternatives and zero-premium proposal are not selected.
[Pawn curves and Rearguard premiums](../../../docs/adr/0015-price-pawns-with-curves-and-rearguard-premiums.md)
records the combined decision.

### Regicide as a royal-achievement tier

The selected rule prices the goal relative to Queen capture and full
victory, not from the King's nominal value.
Let `Q` be the unrounded 12x10 Queen-capture requirement and `W` its checkmate
budget:

```text
Regicide = Q + royal_share * (W - Q)
```

With the released Queen reference and the selected growth model,
`Q = 823700/121` and `W = 11220`.

| Proposed royal_share | Example after ceiling at difficulty 1 |
| --- | ---: |
| 1/2 | 9014 |
| 3/4 | 10117 |
| 1 | 11220 |

The user selected 3/4 in Q29: substantially above Queen capture and near the
victory budget. This is a new pacing decision, not a historical formula.
It assumes `Q <= W`. A later correction that breaks that ordering needs an
explicit decision rather than a negative gap or silent clamp.

Either King still completes one Regicide Location.
Its zero additional-player-chessmen estimate and actual capture condition
remain unchanged.
[Regicide's progression tier](../../../docs/adr/0014-price-regicide-between-queen-capture-and-victory.md)
records the decision. Its initial exact value is `1224140/121`.

## Evidence-supported function candidates

These rows distinguish selected provisional functions from remaining work.

| Goal class | Candidate structure | Unresolved term |
| --- | --- | --- |
| Individual capture | Selected growth floors, released analogue bank, compact transfer, smooth pawn model, Rearguard premium, and Regicide tier | The 105-row CSV supplies the derived profiles. The snapshot draft specifies corrections. Capability cases remain integration work |
| Capture series | Accepted endpoint-anchored reference curve and clear-budget scaling | Future corrections remain explicit. The initial function and table are fixed |
| Checkmate/clear | Accepted 8x8/10x8 anchors plus exact inventory delta | Provisional budgets are fixed. Later tuning requires explicit authored adjustments |
| Tactical and movement goals | Accepted authored reference defaults, with True Royal Fork linked to checkmate | Capability and special-condition integration must remain correct |
| Required player chessmen for captures | Required capture count minus one | Accepted before projection caps, including the new series and Regicide |

Existing authored values and effective runtime requirements are different
inputs. Current profile selection can choose the smaller base/grand value.
Caps can reduce that value further, and later-stage unlocks can bypass resource
requirements.

The function specifies these stages separately. It must not fit only
the final capped values and claim to recover the authored calibration.
History supplies no universal rounding or repair rule. The later user decision
selects unit precision and explicit within-series monotonicity.

### Source context for the retained bypass

In normal generated progression, the next board's unlock item is locked at
the preceding board's victory Location. The current later-stage shortcut is
therefore connected to board completion, not an ordinary randomized item drop.

Source: sibling `worlds\checksmate\item_pool.py:582-604`.
Current resource bypass: `worlds\checksmate\rules.py:310-337`.
The user retained the shortcut. Its possible Regions representation remains
an independent decision.

The current difficulty factors for fairy pieces and pawns concern the
player's piece/pawn configuration. They are not the client's separate
`enemy_army` choice. The Standard CPU reference does not, by itself, remove
these player-configuration factors.

Source: sibling `worlds\checksmate\rules.py:115-159,212-246` and
`options.py:270-328,349-378`, plus
`ChessV.Games\MiscellaneousGames\ApmwChess.cs:391-392,487-493`.

## Candidate equation

The earlier candidate shape follows. Its terms, weights, and operation order
remain unaccepted after the source-history reconstruction:

```text
formation_delta(stage)
  = value(resolved CPU inventory) - value(legacy CPU inventory)

local_role_delta(location_role, stage)
  = value(resolved inventory relevant to location_role)
  - value(legacy inventory relevant to location_role)

provisional(location_role, stage)
  = legacy_threshold(location)
  + role_adjustment(local_role_delta)
  + formation_adjustment(formation_delta)
  + authored_offset

released_threshold
  = stage_cap(monotone_clamp(round(provisional)))
```

The inventories in this candidate use the approved Standard-family formations
and midgame values. The remaining terms and weights are placeholders. This
ticket must decide where a local-role delta is sufficient and where a
whole-formation delta is relevant. It must not silently apply the total
inventory change to every individual capture.

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
- the historical individual-piece formula adds 400 accessibility units. The
  later placement of its comment before the Pieces series does not authorize
  adding another 400 to every aggregate goal;
- `worlds\checksmate\rules.py:106-126` caps a threshold by projected stage maximum material;
- checkmate targets are 4020 for Minima, 6020 for Maxima/10x10, and 8020 for 12x10/12x12.

Checkmate 6x8 has no authored material value in current source. Its two
material fields are `None`, with calibration status `UNSUPPORTED`.
This does not remove 6x8 from the accepted geometry set. The new calibration
contract needs an explicit 6x8 target rather than a silent zero.

The current code has no semantic distinction between a forward pawn and a blocked rear pawn. File and stage asymmetry are evidence that accessibility matters, but do not identify which formation role causes a given premium.

Scope distinction: the 12x12 values and stage references above are current-source compatibility facts. Calibration targets cover the supported augmented geometries 10x10 and 12x10 within the formal set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`; this ticket must not create a 12x12 baseline or target.

## Correction trajectory

The research verified these changes against Git. Commit subjects alone do
not establish a numeric effect or a general invariant.

| Class | Commit | Date | Subject / recorded change |
|---|---|---|---|
| Formula origin | `e6bd565c12` | 2023-11-21 | Added 400 accessibility units to all seven individual-piece requirements |
| Calibration model | `a690076d30` | 2023-11-21 | Raised series values while explicitly reasoning backward from the endgame |
| Schema establishment | `31b9826` | 2023-11-25 | `Move required material into Locations` |
| Count model | `16ef70be2c` | 2023-12-11 | Moved explicit chessmen requirements into Locations |
| Schema establishment | `2144259` | 2024-06-09 | added more capture locations for pawn-heavy distributions |
| Calibration | `a5ffe61` | 2024-08-16 | `improve midgame pawn structure difficulty curve` |
| Calibration | `29c2629` | 2024-09-22 | Raised thresholds despite the subject `lower 4+ series difficulties`, including 4 Pawns from 2040 to 2240 |
| Comment only | `1a14cb9` | 2024-11-18 | Changed the highest-chessmen-requirement comment, not the numeric count model |
| Profile selection | `9c91903728` | 2024-12-22 | Changed mixed-size selection from maximum to minimum of base and grand, with exceptions |
| Calibration | `c1f53b7` | 2024-12-22 | Raised Rooks, grand pawn series, and Any series. It does not establish a universal Rook-above-two-pieces invariant |
| Calibration | `625d2df` | 2024-12-22 | raised queen/fork/series values |
| Geometry migration | `17df6ad` | 2026-07-19 | added APMW v2 geometry expansion and stage-aware grand expectations |
| Geometry migration | `108a5ca`, `61fec12` | 2026-07-20 | path/integration migration |

The research report gives exact historical paths, lines, and parent diffs.
The history supports the recovered structures described above, while leaving
explicit authored adjustments and extension choices.

## Rearguard-pawn hypothesis

The accepted Rearguard name identifies the rear member of a starting doubled
pair. That pawn can remain blocked or inaccessible longer, but the name does
not establish a difficulty premium. The forward pawn retains its counterpart
name and receives no automatic advance premium.

Current Locations commentary indirectly supports testing accessibility, defense duration, tempo, and deployment as pricing factors. Edge-pawn asymmetry, queen-side asymmetry, upper-end capture estimates, and stage-gated series are compatible with a harder rear capture, but none identifies a forward/rear pair or assigns its premium. No current code or retained history hit uses `rearguard`; that absence is negative evidence against treating the term as inherited vocabulary, not evidence against testing the model.

The role ticket fixes rearguard identity from the starting formation
relationship. This material ticket must compare a rearguard-local adjustment
against other local and whole-formation adjustments. No premium follows from
the role name alone. Round 13 selected the explicit quarter-accessibility premium.
That author decision, not the role name or source silence, supplies its
authority.

## Resolution requirements

- Calculate catalog deltas with the accepted midgame basis and 100 material
  units per pawn unit.
- Preserve the recovered historical model and its authored corrections as
  explicit evidence. Distinguish those inputs from current selection, caps,
  and later-stage resource bypass.
- Define the authored legacy baseline for every retained 10x10 and 12x10 Location and state which values are preserved unchanged.
- Derive Location requirements from the approved Standard-family formations.
  Do not vary them by the Enemy Army dropdown or aggregate across families.
- Define and separately calculate local-role deltas and whole-formation deltas.
- State which goal classes use only local-role deltas, only whole-formation deltas, a bounded combination, or no automatic delta: individual captures, aggregate captures, series, tactics, and checkmate.
- Prevent an unrelated inventory increase from being charged wholesale to every individual capture; any formation-wide contribution needs an explicit rationale and bound.
- Test the `rearguard pawn` hypothesis against forward-pawn and ordinary-pawn baselines without presuming that either pawn receives the premium.
- Define pawn-unit rounding and acceptable error bands.
- Supply an explicit 6x8 checkmate baseline. Current `None` values do not
  establish either a zero threshold or an already calibrated compact target.
- Preserve or intentionally replace each hand-authored `material_expectations_grand` and `chessmen_expectations` baseline; replacement requires an explicit old-to-new ledger rather than regeneration by implication.
- Decide whether 12x10 keeps the legacy 8020 checkmate target it currently shares with 12x12 or separates from that value, without treating 12x12 as supported or calibrated.
- Apply the projected-stage maximum-material cap from `rules.py:106-126`, and specify whether rounding and monotonic repair occur before or after that cap.
- State monotonicity constraints across capture counts, stage availability, projected stage material, and difficulty scaling. Any allowed plateau or exception must be explicit rather than an accidental clamp effect.
- Key individual-capture calibration to the role-aware Location identity resolved by the semantic-location ticket, including a rule that formation identity survives movement and current square does not reprice the Location.
- Trace authored baselines across role renames in an old-to-new semantic
  ledger. Numeric IDs can change in 0.4.0, and the new client rejects old
  contracts. This ledger does not require runtime aliases or migration.
- Specify where authored offsets and later play-test adjustments live and how they are reviewed.
- Complete provisional 10x10 and 12x10 tables for every goal class.
  The series CSV records its applicable reference/interpolation and formation
  terms. Individual-role tables must retain their own applicable derivation
  terms rather than pretend that the same equation covers every class.
- Keep exact raw values separate from difficulty, caps, and final thresholds.
  Do not run play tests in this ticket.

## Provenance ledger

| Evidence class | Claims carried into this ticket | Status |
|---|---|---|
| Current user decisions | White's 12x10 Queen is F2, Amazon F1, primary Mounted King G1, and additional King G2. Rearguard is a fixed identity. Round 13 supplies an explicit premium, not an implication from the name. | Arrays, vocabulary, and provisional material functions are selected. Integration acceptance remains open. |
| Current source | Authored pawn/series values, commentary about upper-end estimates and accessibility, `material_requirement()` selection, stage caps, and current checkmate targets. | Current compatibility evidence; not a complete calibration equation. |
| Verified Git history | Historical target-value/accessibility formula, captures-minus-one count relationship, endgame-oriented series reasoning, and later authored corrections. | Supports the documented structures, not a unique new equation or unrestricted extrapolation. |
| Rejected or absent historical evidence | Prior Amazon/Queen “around G1/G2” wording is superseded; current code/history has no forward/rearguard semantic split and no retained `rearguard` hit. | Must not be presented as source-backed historical vocabulary or settled occupancy. |

## Answer

Q35 confirms the selected calibration contract.
The released reference ledger, exact individual and series tables, endgame
budgets, and task table supply the initial requirements.
The [cost-snapshot specification](../contracts/world-cost-snapshot-v1.md)
defines corrections, exact arithmetic, the single ceiling, and world costs.

The values remain provisional calibration estimates, not unresolved formula
choices or proof of gameplay balance.
Later corrections require the specified provenance and review.
Implementation and play-test tuning remain outside this decision ticket.
