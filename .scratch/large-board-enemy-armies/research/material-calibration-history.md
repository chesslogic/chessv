# ChecksMate material calibration: source-history reconstruction

Research date: 2026-09-14. Scope: local planning evidence, not implementation or play-test calibration.

**Classification:** **E** = source evidence, including explicitly identified user decisions. **I** = mathematical inference from cited evidence. **O** = open or unsupported.

**Finding [I]:** The history supports a resource-budget model with authored accessibility adjustments, not one universal equation. The strongest recoveries are the original individual-piece formula, capture-count requirements, and several bounded arithmetic relationships. Later calibration edits leave substantial residuals.[^origin][^counts][^series][^corrections]

**Authority [E]:** The user did not approve `authored baseline + 1:1 target-value delta + positional offset`, or its exclusion of formation effects. The accepted hybrid method retains authored baselines/offsets and starts from Standard/FIDE catalog deltas. Midgame values use 100 units per pawn. The exact equation remains open.[^intent]

## 1. Evidence snapshot and limits

**E:** These are local revisions, not evidence of released behavior. Git metadata came from `rev-parse HEAD`, `branch --show-current`, `status --short`, and `log -1`. Source history came from actual `blame -w`, `log --follow`, and `show`, including parent diffs.

| Repository | Branch | Exact HEAD | HEAD author/committer timestamp |
| --- | --- | --- | --- |
| `C:\GitHub\rft50-checksmate` | `checks_mate` | `0772bac724ff483043b56979f0ef078b9c2d264a` | `2026-09-12T10:53:46-07:00` |
| `C:\GitHub\chessv` | `develop` | `888710f99cf007b982ea1f6a8a64c1e8058be196` | `2026-09-04T16:18:39-07:00` |

**E:** ChecksMate had a clean working tree. ChessV's catalog and CPU profiles matched HEAD. ChessV already had these unrelated working-tree changes:

```text
Modified under C:\GitHub\chessv\.scratch\large-board-enemy-armies\:
  issues\08-carry-forward-formation-constraints.md
  issues\12-normalize-ten-by-ten-arrays.md
  issues\13-normalize-twelve-file-arrays.md
  issues\14-set-royal-and-castling-semantics.md
  issues\15-define-location-role-identity.md
  issues\16-define-material-calibration-contract.md
  issues\17-define-versioned-cross-repo-contract.md
  issues\18-define-handoff-acceptance-contract.md
  map.md
Untracked, as reported by Git:
  C:\GitHub\chessv\.scratch\large-board-enemy-armies\qn_resolution-rounds.md
  C:\GitHub\chessv\.scratch\large-board-enemy-armies\research\
  C:\GitHub\chessv\CONTEXT.md
  C:\GitHub\chessv\docs\adr\
```

**E:** The research changed only this report. The sibling repository remained read-only. Arithmetic used Python's standard library and AST parsing of Git blobs, without importing game code. No games, play tests, installations, network research, threshold changes, or publication formed part of this research.

**E:** The historical path was `worlds\checks_mate\Rules.py` before the package rename in `e9d2581cae` on 2023-11-06. Locations moved from `Locations.py` to `locations.py` in the R100 rename `108a5ca8e1` on 2026-07-20. The material helper also passed through `MaterialModel.py`. These paths were followed, not treated as separate implementations.

## 2. What the calibration effort actually changed

**E:** The following dates and effects come from Git, not the earlier ticket's chronology. Dates are author dates. A commit subject is not proof of its numeric effect.

| Commit / date | Verified quantitative change or rationale |
| --- | --- |
| `c6b30c8ff5`, 2023-11-12 | Edge-pawn estimates rise from 100 to 190, adjacent pawns to 140. Kingside pieces gain 90 for queen-development tempo. Adds the player-King/chessmen assumption.[^early] |
| `6cf26bd573`, 2023-11-15 | Kingside piece premiums rise from 90 to 340. Pawn F/G/H become 140/240/290.[^tempo] |
| `e6bd565c12`, 2023-11-21 | Adds exactly 400 to each individual non-pawn target. The comment calls this one extra piece plus one pawn for accessibility.[^origin] |
| `a690076d30`, 2023-11-21 | Raises capture series. The commit body explicitly reasons backward from the endgame for smoother mid/late-game costs.[^endgame] |
| `31b982671d`, 2023-11-25 | Moves existing requirements from Rules into Locations. It does not introduce a generator. `b75b578af1` then calls them generally upper-end estimates.[^schema] |
| `16ef70be2c`, 2023-12-11 | Moves explicit chessmen counts into Locations. `4bf9f4ed23` sets checkmate, Everything, and True Royal Fork to 4020.[^counts][^mate-history] |
| `6c7e26c091`, 2024-02-04 | Raises midgame series, including 8 Pawns from 2575 to 3275. Subject: the AI takes midgame Locations seriously.[^midgame] |
| `2144259e8c`, 2024-06-09 | Adds Any 2-14 to support pawn-heavy item distributions. `15b04a5d20`, 2024-06-25, adds a separate grand column and 6020 grand checkmate.[^any-origin][^grand-origin] |
| `a49040a4ab`, 2024-08-10 | Feedback-driven pawn increases and bishop deployment premiums. `a5ffe61a46`, 2024-08-16, then increases midgame pawn/piece series.[^access][^pawn-curve] |
| `1f3eeff32a`, 2024-08-25 | Raises Of Each estimates, especially grand values. `29c262935a`, 2024-09-22, also **raises** values despite "lower 4+ series difficulties." Base 4 Pawns changes 2040 to 2240.[^each-history][^september] |
| `1a14cb983b`, 2024-11-18 | Changes a comment about the highest chessmen requirement. It does **not** recalibrate counts or correct a numeric trade model.[^comment-only] |
| `9c91903728`, 2024-12-22 | Changes mixed-size selection from `max(base, grand)` to `min(base, grand)`, preserving grand-only/always-grand exceptions.[^minimum-history] |
| `c1f53b70fc`, 2024-12-22 | Raises rooks, grand pawn series, and Any series. `625d2dff11` then raises grand Queen 1900 to 4100 and attendants by 2000, plus forks/Of Each.[^corrections] |
| `17df6adc6d`, 2026-07-19 | Adds twelve-file estimates, stage gates, and projection caps. The 8020 checkmate comment explicitly adds two approximately 1000-valued outer attendants.[^expansion] |
| `61fec128a4`, 2026-07-20 | Makes profile selection explicit and gives Everything 14/22 chessmen plus its expanded-stage override. `bd59772c66`, 2026-08-25, introduces configurable board series and chessmen caps.[^selection][^runtime][^availability] |

**E:** Rules also received a real execution correction: `028eb6579a`, 2025-02-03, binds each Location's rule list into its lambda. Previously, late binding reused the last list. Thus, authored values cannot establish what every historical runtime actually enforced.[^binding]

## 3. Reconstructed material and count relationships

### Individual non-pawn captures

**E:** Before the accessibility increment, the historical Rules values were Rook 500, minor 300, Queen 900, plus 340 for kingside counterparts. The November 2023 diff adds 400 to all seven individual-piece requirements.[^origin][^tempo]

**I:** This exactly reconstructs those seven values:

```text
H_2023(role) = V_rules(role) + 400 + 340 * is_kingside_counterpart(role)
V_rules: Rook = 500, Knight/Bishop = 300, Queen = 900
```

The side term applies to the King's Rook, Knight, and Bishop. It does not apply to the Queen. The formula matches that historical snapshot, not all later calibration.[^origin]

**I:** Residual means current authored base minus the reconstructed 2023 prediction. This comparison excludes profile selection, difficulty, and caps.[^origin][^individual-current]

| Role | 2023 prediction | Current base | Residual | Current grand |
| --- | ---: | ---: | ---: | ---: |
| Queen's Rook | 900 | 1500 | +600 | 2850 |
| Queen's Knight | 700 | 700 | 0 | 1200 |
| Queen's Bishop | 700 | 1040 | +340 | 1200 |
| Queen | 1300 | 1300 | 0 | 4100 |
| King's Bishop | 1040 | 1140 | +100 | 1400 |
| King's Knight | 1040 | 1040 | 0 | 1400 |
| King's Rook | 1240 | 1900 | +660 | 3250 |

**E/I:** The bishop residuals come from the deployment edit. Rook residuals come from the later rook correction. But the latter's subject does not establish an invariant: grand Queen's Rook remains 2850, below grand 2 Pieces at 3000.[^access][^corrections]

**E/I:** The `+4 material` comment now sits before the Pieces series. Its introducing diff places it before **individual pieces**. Applying another automatic 400 to every aggregate goal misreads that history.[^origin][^schema][^series]

### Pawn captures

**E:** Pawn estimates reflect defense duration, edge position, queen-development tempo, and later feedback. The sources do not supply a distance function or defense-count algorithm.[^early][^access][^individual-current]

**I:** Every reference pawn has value 100. Thus, `H = 100 + A(file, profile)` is a transparent residual ledger. It is not a recovered generator for `A`.[^catalog][^individual-current]

| File | Base | Grand | Base residual above 100 | Grand residual above 100 |
| --- | ---: | ---: | ---: | ---: |
| A | 490 | 1010 | 390 | 910 |
| B | 340 | 810 | 240 | 710 |
| C | 220 | 660 | 120 | 560 |
| D | 100 | 520 | 0 | 420 |
| E | 100 | 320 | 0 | 220 |
| F | 320 | 320 | 220 | 220 |
| G | 390 | 620 | 290 | 520 |
| H | 490 | 860 | 390 | 760 |
| I | unavailable | 810 | - | 710 |
| J | unavailable | 890 | - | 790 |
| K | unavailable | 970 | - | 870 |
| L | unavailable | 1050 | - | 950 |

**I:** A target-value-only rule cannot explain these differences. Equal edge distance also fails: base B/G differ by 50, and D/E/F are 100/100/320. The I-L sequence is exactly `810 + 80*(file_index-9)` for one-based indices. It is a bounded pattern, not evidence for arbitrary widths or rearguard premiums.[^individual-current][^expansion]

### Capture series: counts are much more recoverable than material

**E:** Historical Rules say the AI avoids trades unless it wins material or secures victory. A separate comment requires the player to own enough chessmen, in addition to a King. These are distinct requirements, not two names for material.[^trade]

**I:** Every current ordinary capture-series count matches:

```text
C(Pawns n) = C(Pieces n) = C(Any n) = n - 1
C(Of Each n) = 2*n - 1
C(Everything) = starting non-Kings - 1  # historical one-King formations
```

There are 52 exact matches across the four numbered series. Historical Everything gives 14 for 15 non-Kings, then 18 for 19, and currently 22 for 23.[^counts][^trade][^series][^selection]

**I:** This is consistent with one capture contribution from the player's free starting King, and one owned chessman per remaining capture. It does **not** subtract 325 material, promise profitable trades, or grant one discount per enemy King. The comments and counts do not establish those stronger claims.[^trade][^catalog]

**E/I:** Material follows a different, repeatedly adjusted curve. The endgame-oriented commit supports a model of progress toward a full-clear budget. It does not identify which pieces are traded, which captures are free, or an equation for the remaining budget.[^endgame][^midgame][^pawn-curve][^september]

The following candidate failures use current authored **base** values. Residual is actual minus prediction.[^series][^individual-current]

| Candidate | Goal | Prediction | Actual | Residual |
| --- | --- | ---: | ---: | ---: |
| Sum the cheapest individual pawn requirements | 2 Pawns | 200 | 750 | +550 |
| Same | 4 Pawns | 740 | 2240 | +1500 |
| Same | 8 Pawns | 2450 | 3545 | +1095 |
| Add the Pawns and Pieces requirements | 2 Of Each | 2200 | 2250 | +50 |
| Same | 4 Of Each | 5010 | 2950 | -2060 |
| Same | 7 Of Each | 7005 | 3850 | -3155 |
| Extend the exact Any 6-14 line, `150*n + 1800` | Any 5 | 2550 | 2500 | -50 |
| Same | Any 4 | 2400 | 2240 | -160 |

**I:** These bounded relationships are reproducible, but their coefficients remain authored:[^series][^corrections][^expansion]

| Relationship | Exact domain | Counterexample or limit |
| --- | --- | --- |
| Any equals Pawns | Counts 2-4, both columns | Base Any 5 = 2500, Pawns 5 = 2620 |
| Grand Pawns = base + 1000 | Counts 3-8 | Count 2 adds 900 |
| Base Any = `150*n + 1800` | Counts 6-14 | Fails before count 6. There is no base Any 15 |
| Grand Pawns = `5245 + 600*(n-10)` | Counts 10-12 | The preceding 8-to-9 step is only 100 |
| Grand Pieces = `5400 + 700*(n-9)` | Counts 9-11 | The preceding step is 200 |
| Grand Of Each = `5950 + 950*(n-9)` | Counts 9-11 | The preceding step is 100 |
| Any terminal is 50 below Everything | Grand Any 18/Everything in 2024, 22/Everything in 2026 | Not a universal gap: base Any 14 is 120 below Everything |

**O:** None of these finite-domain identities authorizes continuation to the new endpoints. No high-degree curve was fitted. The history does not uniquely determine new intermediate requirements.

### Tactics, threats, and King movement

**E:** These also use authored pairs, with no separate material generator. The six current fork pairs and chessmen counts are:[^tactics]

| Fork | Base / grand material | Chessmen |
| --- | ---: | ---: |
| Sacrificial | 700 / 1100 | 6 |
| Sacrificial Triple | 3300 / 2700 | 9 |
| Sacrificial Royal | 3600 / 5200 | 12 |
| True | 3150 / 4550 | 10 |
| True Triple | 3850 / 5850 | 12 |
| True Royal | 4020 / 6020 | 14 |

**E:** `d7d175f7dd`, 2024-06-26, adds those chessmen counts without a generating equation. `625d2dff11` raises several base fork values but leaves grand Sacrificial Triple at 2700. Thus grand is not necessarily harder. True Royal inherits the checkmate-sized 4020/6020 pair.[^tactic-history][^mate-history]

**E:** Survive 3/5/10/20 Turns uses material `0/0`, `200/330`, `2500/4500`, `3800/5800`, with counts `0,2,9,15`. The 5- and 10-turn estimates increase immediately after introduction. No constant material-per-turn rule explains them.[^turn-history][^tactics]

**E:** Threaten Pawn/Minor/Major/Queen/King uses `0/0`, `200/400`, `300/500`, `300/500`, `1000/1800`. King to Back Rank uses `2250/5150`, explicitly for a late-game state. Early King movement includes zero-material goals. These are task/accessibility estimates, not prices of threatened or moving pieces.[^tactics][^king-moves]

### Checkmate and Everything

**E/I:** An early checkmate estimate is 3900. This equals `8*100 + 4*300 + 2*500 + 900`, excluding the King under the historical Rules basis. `6201bbd4bd` changes it to 3920. `4bf9f4ed23` changes it to 4020. The extra 20 and 100 are authored changes, not a recovered formula.[^mate-history][^origin]

**E:** Grand checkmate enters as 6020 in `15b04a5d20`. The 2026 expansion reuses 6020 for 10x10 and sets 8020 for both twelve-file heights. Only the last step has an explicit quantitative comment: 6020 plus two outer attendants at about 1000 each.[^grand-origin][^expansion]

**I:** Approximate orthodox values can explain `4020 + 2*900 + 2*100 = 6020`. That explanation is compatible with the numbers, but the introducing diff does not state it. It is not exact catalog arithmetic.[^grand-origin][^catalog]

**I:** Applying exact current inventory deltas from the 8x8 reference does not recover every authored checkmate requirement:[^catalog][^profiles][^mate-current]

| Existing profile | Raw material including King | `4020 + (raw - 4375)` | Authored checkmate | Residual |
| --- | ---: | ---: | ---: | ---: |
| 8x8 | 4375 | 4020 | 4020 | 0 |
| 10x8 | 6400 | 6045 | 6020 | -25 |
| Old 10x10 | 6400 | 6045 | 6020 | -25 |
| Old 12x10 | 7700 | 7345 | 8020 | +675 |

**E/I:** The old twelve-file Standard profile adds two Nightriders, not two 1000-valued pieces. Their 1100 plus two pawns' 200 gives a 1300 inventory increment, versus the authored 2000 increment. This leaves a 700 step residual. The comment is approximate calibration evidence, not an exact inventory description.[^profiles][^catalog][^expansion]

**E:** Everything changes from 3950 to 4050 and then 4020 in December 2023. Grand Everything enters as 6050 and becomes 8050 in the twelve-file expansion. Its current expanded chessmen count is 22. Checkmate 6x8 remains explicitly unsupported, with both material fields `None`.[^mate-history][^grand-origin][^expansion][^selection][^mate-current]

## 4. Runtime: selection, scaling, capacity, and units

### Actual order

**E:** The current runtime follows this order. These are historical mechanics, not a proposed future policy.[^selection][^availability][^runtime]

1. **Publish eligible Locations.** Endpoint stage and tactics mode select the set. Compact exclusions and progression-start filtering also apply.
2. **Select the profile.** Most Locations use `expanded = endpoint > 8x8`. Everything uses its explicit endpoint threshold of 12x10.
3. **Select the rule stage.** Use the expanded stage override where applicable, otherwise the declared stage. Raise it to the start stage, except compact remapping.
4. **Select authored material/count.** Non-expanded uses base. Expanded uses grand for grand-only/flagged goals, otherwise `min(base, grand)`. A missing grand value falls back to base.
5. **Apply resource rules.** Scale material, add the conditional absolute amount, then cap by projected stage material. Independently cap chessmen by projected stage chessmen.
6. **Compose gates.** Required geometry remains a gate. A later unlocked stage in the configured series bypasses both resource rules. Special tactic/castling conditions remain separate.

**E:** In normal generated progression, each next-board unlock is locked at
the preceding stage's victory Location. This supplies context for the
later-stage shortcut. The resource rules still test unlock state, not a
separate completed-victory predicate.[^unlock-placement][^runtime]

**E/I:** `force_grand` exists on `material_requirement()`, but current `set_rules()` does not pass it. Even a 10x8-start world normally uses the smaller base/grand value. Queen selects 1300, not 4100. Sacrificial Triple selects 2700, not 3300, in expanded worlds.[^selection][^runtime][^individual-current][^tactics]

**E:** Geometry unlocks equal the start-stage pair plus received Board Files/Ranks items. A received legacy Super-Size Me adds one file unlock. Required pairs are `6x8=(0,0)`, `8x8=(1,0)`, `10x8=(2,0)`, `10x10=(2,1)`, and `12x10=(3,1)`. These are unlock counts, not material units.[^availability][^runtime]

For positive selected material `H`, the effective material threshold is:

```text
T_material = min(H * D + (R if H > 90 else 0), M_max(rule_stage))
T_chessmen = min(C_selected, C_max(rule_stage))

resource_access =
    later_stage_unlocked
    OR (projected_material >= T_material AND projected_chessmen >= T_chessmen)
```

**E:** `None` and zero material do not add a material rule. The `H > 90` condition uses the unscaled authored value. Difficulty does not scale chessmen. The `CALIBRATED` property means that at least one material field exists, not that play tests established its accuracy.[^runtime][^selection]

**E:** The multiplicative difficulty `D` combines stable placement `1.05`, pawn-mode factors, `0.99 + 0.01*family_count`, and difficulty mode. Pawn factors are `mixed=1.16`, `any_pawn/any_fairy/any_classical=1.12`, `berolina/checkers=1.06`, otherwise `1`. Family count is FIDE 1, Betza 4, full 6, otherwise the configured list length. Daily/bullet/relaxed factors are `1.1/1.2/1.35`, otherwise `1`.[^difficulty]

**E:** `R` adds 120 for bullet or 240 for relaxed. It also adds 120 for pawn modes `checkers`, `any_fairy`, or `any_classical`.[^difficulty]

**E:** These fairy-piece and pawn factors use the player's world configuration.
They do not read the client's separate `enemy_army` choice.
The accepted Standard CPU reference does not itself remove these
player-configuration factors.[^player-options][^intent]

**E/I:** There is no Location-threshold rounding call or monotonic repair. `490 * 1.05 = 514.5` remains a float. Integer material must meet that value, mathematically equivalent to at least 515 before attainable-state granularity. A 100-unit state grid can make the first attainable value 600. Neither effect changes the authored 490.[^runtime][^projection]

### What the cap actually measures

**E:** Projection counts come from generated items, start inventory, and locked items, with tracker overrides where present. They are normalized against contract maxima. Thus, the cap is world-specific, not the CPU army's catalog total.[^obtainable][^projection]

**E:** Current projection has two paths:[^projection]

| Path | Material metric before King-promotion increments |
| --- | --- |
| Fundamental with at most 10,000 envelope cells | Minimum exact active semantic material over every componentwise future count state within the envelope |
| Legacy, or larger Fundamental spaces | `100 * safe_active_non_primary_chessmen` |

**E:** Fundamental safe chessmen are `min(Chessmen + Consuls, stage.non_pawn_capacity)`. Legacy limits non-pawns by capacity, then adds pawns within the remaining pawn capacity. King promotions add 425 each. Owned pockets add at most three chessmen through `ceil(pocket_items / limit_per_pocket)`, but `maximum_chessmen()` excludes pockets.[^projection][^pockets]

**E:** "Exact active material" here sums semantic expected values, not the ChessV midgame catalog. Contract values include minor 300, major 485, queen 900, pawn 100, and material item 400.[^semantic][^expected]

**I:** An existing full-maxima Fundamental fixture gives these caps. These are fixture results from source arithmetic, not a claim about every generated world:[^cap-fixture]

| Stage | Safe chessmen | Material with two King promotions |
| --- | ---: | ---: |
| 6x8 | 11 | 1950 |
| 8x8 | 15 | 2350 |
| 10x8 | 19 | 2750 |
| 10x10 | 39 | 4750 |
| 12x10 | 47 | 5550 |

**I:** Consequently, the 8x8 authored 4020 checkmate requirement can become 2350 in this fixture at `D=1, R=0`. Many distinct late-game baselines can collapse to the same cap. Effective reachability alone hides those authored distinctions.[^runtime][^cap-fixture][^mate-current]

### MaterialModel is pool accounting, not a Location generator

**E:** Let `q = terminal_checkmate_material / 4020`. The minimum/maximum pool targets returned by MaterialModel are:[^pool]

```text
B_min = (4100*q + 50)*D + R - 50
B_max = (4600*q + 50)*D + R - 50
```

**E:** The 41/46 inputs are pawn units, converted to hundredths by multiplication by 100. The White bonus is difficulty-scaled before the separate unscaled subtraction. A 6x8-only endpoint uses `q=1` without claiming a calibrated 6x8 checkmate value.[^pool]

**E:** The dynamic ratio moved into Rules in `5abb85d21a`, 2025-02-21. `bd59772c66` later makes it depend on the configured endpoint instead of the largest legacy goal. `c1886028db` changes pool accounting, including quantity caps and Pocket exclusion for explicit item lists. None generates Location constants.[^pool-history][^pool]

**E:** Current `create_progression_items()` accepts `min_material` but does not use it in its loop. Thus the returned pair is not proof that a minimum army value is enforced. This is a source limitation, not a change made by this research.[^pool-consumer]

## 5. Catalog basis and implications for the new formations

**E:** Current catalog midgame values include Pawn 100, Rook 500, Knight/Bishop/King 325, and Queen/Chancellor 950. Other reference values are Archbishop 875, Lion 500, Nightrider 550, Amazon 1300, and Mounted King 700. `82b9ba37cf`, 2026-08-17, creates the catalog file. Relevant ordinary values already appear in the older engine code. `77471e9b31`, 2023-11-29, changes the King's engine value from zero to 325 for extinction evaluation.[^catalog][^engine-history]

**I:** These facts prevent retroactively treating the Rules' 300/900 assumptions as exact catalog values. Substituting modern values into the original individual-piece formula predicts Knight 725 and Queen 1350, not authored 700 and 1300. Those differences require residuals, not a fabricated historical catalog conversion.[^origin][^catalog]

**E, user decisions:** The approved Standard 10x10 and 12x10 inventories total 8400 and 11600, including Kings. Basic Elephant contributes 250 by decision but is not in the current APMW catalog. The approved arrays are not the old profiles examined here.[^new-arrays][^catalog]

**I:** The old-to-new inventory deltas are `8400-6400=2000` and `11600-7700=3900`. They are inputs for a candidate function, not Location thresholds. Applying either wholesale to every individual capture has no support in the recovered history.[^new-arrays][^profiles][^catalog][^individual-current]

**E, user decisions:** Starting-role identity, Regicide, spare King lives, all integer series goals, earlier-board count completion, and endpoint-only Everything are fixed. Rearguard identity adds no automatic premium. The new contract supports 6x8, 8x8, 10x8, 10x10, and 12x10, not 12x12. Old-source 12x12 rows are historical evidence only.[^capture-decisions][^intent]

**I:** Current stage gating does not implement all these fixed semantics. For example, current Any 15 requires 10x8, and Everything uses only its legacy base/expanded profiles. The new contract must distinguish publication, count completion, and endpoint-only clearing. Historical behavior is not a policy default.[^series][^availability][^capture-decisions]

## 6. Candidate functions and the smallest remaining choices

**I/O:** The following are evidence-supported ways to expose the model. They are not approved equations:

| Candidate | What evidence supports | What remains unknown |
| --- | --- | --- |
| `H_individual = V_catalog(target) + A(role, stage, formation)` | A unit-slope value component exists in the 2023 ancestor. Later diffs explicitly change accessibility costs.[^origin][^access] | How `A` transfers to a changed formation. It can include formation effects. Zero formation effect is not established |
| `H_new = H_old + delta(V_catalog) + delta(A)` | Preserves a baseline and exposes rather than hides calibration residuals.[^origin][^catalog] | Which old role/profile supplies the baseline, and how to calculate the changed residual |
| `C = captures_needed - 1` | Exact ordinary-series count relationship and the player-King comment.[^counts][^trade][^series] | Whether the same resource heuristic remains suitable for the fixed new multi-King goals. Capture-counter semantics themselves are settled |
| `H_series(n) = H_checkmate - G_series(n)` | The commit body explicitly reasons backward from the endgame.[^endgame] | `G` is an authored gap, not identified uncaptured-piece material. Expressing it this way does not generate new counts |
| `H_mate,new = H_mate,old + delta(V_inventory) + delta(A_mate)` | Approximate added-piece budgeting and exact catalog deltas can be recorded separately.[^expansion][^profiles][^catalog] | Historical rounding/approximation and royal/formation residuals. The old twelve-file counterexample forbids claiming exact recovery |

**O:** Four consequential choices remain for the interview:

1. **Residual transfer.** Which matched intrinsic profile is the anchor, and which accessibility residuals persist when placement or surrounding material changes? This includes new roles without a historical baseline.
2. **Series and tactical continuation.** Which endgame gaps and resource-count assumptions remain authored, and what explicit rule extends them to new counts? History alone cannot price the new intervals.
3. **Projection and profile policy.** Does the new contract preserve base/grand minimum selection, world-dependent caps, and later-stage resource bypass? Fixed capture availability does not answer these difficulty questions.
4. **Numeric presentation and constraints.** What rounding and monotonicity rules apply, and at what point relative to caps? Current code supplies no universal rounding or repair rule.

**I:** The next decision concerns how an explicit resource term and an authored residual transfer between formations. The history supports that separation. It does not uniquely determine either the new residuals or the exclusion of formation effects.[^origin][^endgame][^corrections][^runtime]

## Source references

Current ChecksMate citations use HEAD `0772bac724ff483043b56979f0ef078b9c2d264a`. Current ChessV citations use HEAD `888710f99cf007b982ea1f6a8a64c1e8058be196`. Explicit historical citations use the path and line numbers at that commit. Ticket citations identify dirty interview records, not executable or release evidence.

[^intent]: User's 2026-09-14 research directive, especially LATEST USER DIRECTION and FIXED DECISIONS. Recorded equation status: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\16-define-material-calibration-contract.md:11-55` (working tree).
[^early]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:78-101,137-193` at `c6b30c8ff54c68dd9b29a39e3368b884dbc6072e`, including its parent diff.
[^tempo]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:99-121` at `6cf26bd5732a846426bdb2da182a258d8b2567d9`, parent diff.
[^origin]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:95-111` at `e6bd565c121278317877b94f5b859c0c195aaae8`, parent diff. This is the introducing placement of the 400-unit accessibility comment.
[^endgame]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:104-138` at `a690076d30fe977b67a16e922acde2621ea64948`, parent diff and commit body.
[^schema]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:10-84` and `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:92-100` at `31b982671d58abf8e44e15517026686efe83b1f4`, parent diff. Estimate disclaimer: the same historical Locations path, lines 10-16, at `b75b578af15ca6b3f05a5ebfe34b2a227339a18b`.
[^counts]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:50-70` at `16ef70be2c0fd5d4fb468c170edcb59756b56431`, parent diff.
[^midgame]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:50-71` at `6c7e26c0911cefa4929feb3aae28d95f47e95a4c`, parent diff and subject.
[^any-origin]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:71-83` at `2144259e8c5b2523e6729f7f619bb3ec1b6d93c1`, parent diff and subject.
[^grand-origin]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:10-49,57-104` at `15b04a5d202bf795b4dc34751ddb125f15e2f108`, parent diff.
[^access]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:29-55` at `a49040a4ab67d64b3335e0e6f6755b9087108fb8`, parent diff and subject.
[^pawn-curve]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:65-82` at `a5ffe61a46106803119f787c1681bf14d471212d`, parent diff.
[^each-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:83-90` at `1f3eeff32a9bd45522f20798a6aabb8eca3f69e6`, parent diff.
[^september]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:65-103` at `29c262935ae4440dd2e097dec13d6e46a135547f`, parent diff.
[^comment-only]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:150-155` at `1a14cb983b02a7b9961883fd9491622c1bce156c`, parent diff. Its Rules edit only removes an old TODO.
[^minimum-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:109-116` at `9c9190372832fa76bb9b30597b7b5ac971c3042d`, parent diff.
[^corrections]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:35-75,92-108` at `c1f53b70fc155bdba5b48090e45d611d91a727cc`, and lines 29-56,83-89,122-129 at `625d2dff110bef7aed37403469d851d0f671aaef`, parent diffs.
[^binding]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:138-141` at `028eb6579a69b0e42241f7de0c98619f404b697e`, parent diff.
[^expansion]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:33-34,60-101,113-164,177-201` at `17df6adc6d89af7b88bf2bcd3002c4e66defd535`, parent diff. Current comment: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:49-49`.
[^selection]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:52-107,250-258` at current HEAD. Introducing methods/Everything profile: the same path, lines 62-92,210-218, at `61fec128a4736943e40065a7d28b6fe2111ae3f5`.
[^individual-current]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:111-131,168-185` at current HEAD.
[^series]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:193-297` at current HEAD.
[^trade]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:123-159` at `b09aea136a0546119edf344e41589ced849081ed`. Earlier King wording: the same historical path, line 138, at `c6b30c8ff54c68dd9b29a39e3368b884dbc6072e`.
[^tactics]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:298-330` at current HEAD.
[^tactic-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:111-118` at `d7d175f7ddcbc7b36a3d69dfc385937767e92089`, lines 109-118 at `81e12a7d86cee209e5a6aa3cf2988f7d2186c629`, and lines 122-129 at `625d2dff110bef7aed37403469d851d0f671aaef`, parent diffs.
[^turn-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:108-111` at `c518afee6457fca086a5d083a785b88960fc01ed` and `3bcf48813a460d815ed51035a27aad586301bcda`, parent diffs.
[^king-moves]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:186-192` at current HEAD. Back-rank increase: historical `Locations.py:57-63` at `c518afee6457fca086a5d083a785b88960fc01ed`.
[^mate-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:75-75` at `6201bbd4bdfa7517e6f74550c5a6888e35a5845a`, parent diff (3900 to 3920). `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:65-70` at `fda83edd9807756cce5e59c06a7fdfe585731c7f` changes Everything, despite its checkmate subject. The same Locations path, lines 37-37,70-70,84-84, at `4bf9f4ed238c79f1ad38de6cae9a1c3710ec5d8b` sets the three 4020 values.
[^mate-current]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:132-167` at current HEAD.
[^availability]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:344-417` and `C:\GitHub\rft50-checksmate\worlds\checksmate\geometry_progression.py:67-161,203-226` at current HEAD. Board-series changes: `bd59772c6640b31865705cf7251fcfe9e5d31a8a`, parent diffs.
[^unlock-placement]: `C:\GitHub\rft50-checksmate\worlds\checksmate\item_pool.py:582-604` at current HEAD. Each progression transition locks its Board Files or Board Ranks item at `transition.source.victory.location_name`.
[^runtime]: `C:\GitHub\rft50-checksmate\worlds\checksmate\rules.py:65-113,227-246,271-393` at current HEAD. Material-cap introduction: `17df6adc6d89af7b88bf2bcd3002c4e66defd535`. Chessmen-cap and no-force-grand changes: `bd59772c6640b31865705cf7251fcfe9e5d31a8a`.
[^difficulty]: `C:\GitHub\rft50-checksmate\worlds\checksmate\rules.py:115-159,212-224` at current HEAD. Blame attributes pawn factors to `5cf9f8b1dc`, placement-option correction to `dc17605a3a`, and difficulty-mode factors to `8a8afbd9f7`.
[^player-options]: `C:\GitHub\rft50-checksmate\worlds\checksmate\options.py:270-328,349-378` and `rules.py:115-159,212-224` at current HEAD. Separate CPU selection: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwChess.cs:391-392,487-493` at current HEAD.
[^projection]: `C:\GitHub\rft50-checksmate\worlds\checksmate\logic_projection.py:30-43,98-110,134-189,221-229,257-357,418-420` at current HEAD.
[^pockets]: `C:\GitHub\rft50-checksmate\worlds\checksmate\item_utils.py:33-36` and `C:\GitHub\rft50-checksmate\worlds\checksmate\logic_projection.py:148-156,228-229` at current HEAD.
[^semantic]: `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\semantic.py:119-138` at current HEAD.
[^expected]: `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\contract.py:81-95,847-848` and `C:\GitHub\rft50-checksmate\worlds\checksmate\apmw_projection\resource.py:17-17,26-41` at current HEAD.
[^obtainable]: `C:\GitHub\rft50-checksmate\worlds\checksmate\__init__.py:268-279,331-344,364-389` at current HEAD.
[^cap-fixture]: `C:\GitHub\rft50-checksmate\worlds\checksmate\test\test_logic_projection.py:179-198` at current HEAD. Blame identifies `17df6adc6d` for the fixture and `bd59772c66` for adding compact capacity.
[^pool]: `C:\GitHub\rft50-checksmate\worlds\checksmate\material_model.py:20-64` and `C:\GitHub\rft50-checksmate\worlds\checksmate\rules.py:155-224` at current HEAD.
[^pool-history]: `C:\GitHub\rft50-checksmate\worlds\checksmate\MaterialModel.py:26-38` and `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:71-91` at `5abb85d21a53dd68b8500640ed121f535614449c`, parent diffs. Accounting: `C:\GitHub\rft50-checksmate\worlds\checksmate\material_model.py:20-62` at `c1886028db21e706d1531de56ceb8ac560d77248`. Endpoint ratio: current Rules lines 180-209, introduced by `bd59772c66`. White subtraction: historical `MaterialModel.py:38-43` at `e8f99e18b5c407ac44c5f8301c99b0000ceed430`.
[^pool-consumer]: `C:\GitHub\rft50-checksmate\worlds\checksmate\item_pool.py:685-744` at current HEAD.
[^catalog]: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:21-63` at current HEAD, blamed to `82b9ba37cfc6ea440a3ba6f878b60ff4f21093e3`. Constructor parameter order: `C:\GitHub\chessv\ChessV.Games\Pieces\Chess.cs:100-101,160-161`.
[^engine-history]: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwChess.cs:486-520` at `77471e9b316786414c661c5bf64fefd53c603ec1`, including the King-value parent diff. Pre-extraction values: the same path, lines 482-521, at `82b9ba37cfc6ea440a3ba6f878b60ff4f21093e3^`.
[^profiles]: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:129-132,316-348,362-365,378-381,394-414` at current HEAD. Ten/twelve-file profiles originate in `10999c8c`, 2026-07-19. Compact profile enters in `82b9ba37cf`, 2026-08-17.
[^new-arrays]: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\12-normalize-ten-by-ten-arrays.md:48-113` and `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\13-normalize-twelve-file-arrays.md:135-203` (working tree, current user decisions).
[^capture-decisions]: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\15-define-location-role-identity.md:153-202,264-277,401-410` (working tree), and the user's fixed decisions in the 2026-09-14 research directive.
