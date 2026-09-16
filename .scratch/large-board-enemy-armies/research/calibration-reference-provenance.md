# Calibration-reference provenance against Client 0.3.2

## Findings

**Confirmed release presence:** The published 0.3.2 world contains all 74 proposed base8/grand10 series points, including the high-count grand10 endpoints. It also contains the seven base8 non-pawn references, nine grand10 non-pawn references, base pawns A-H, and grand pawns A-J. Their material values match current source. These are reproducible released baselines, not automatically play-tested prices.[^artifact]

**Confirmed later additions:** Outer Attendants 4110/4190, pawns K/L 970/1050, and the ten legacy12 series additions first appear in `17df6adc6d`. They are absent from the released world. No role-specific gameplay-calibration record for these additions was found in the inspected sources.[^later][^evidence]

**Policy boundary:** This audit does not change ADRs 0008-0012 or select prices. The endpoint transfer rule and growth floors do not establish the provenance of their inputs. Lion references 6035/6115, the pawn curve, and Regicide 4725 remain unaccepted.[^policy]

## 1. What 0.3.2 actually identifies

| Evidence | Result | Limit |
| --- | --- | --- |
| ChessV local `v0.3.2` | Lightweight tag resolving to `6fe99970b574f11e35d52e080c03a5eefbdeedbb` | Not an annotated tag. `^{commit}` resolves to the same object |
| First-party GitHub tag metadata | Also resolves to that commit | Confirms the local tag against the published repository |
| Release metadata | **Client 0.3.2**, published 2026-06-17 07:44:11 UTC | The `targetCommitish` field says `release`, a branch name, not a producer SHA |
| Tagged client source | `ClientVersion = "0.3.2"` | Identifies the client handshake version, not the upstream Archipelago version |
| Sibling local `0.3.2` | Lightweight tag at `5d3b4c8efd413c3b808d6d5b979ea3736da0a078`, a 2022 Meritous change | Upstream Archipelago 0.3.2. No ChecksMate world exists there. It is the wrong calibration boundary |
| Sibling exact `v0.3.2`, checked after the user's tag hint | Absent locally and from the `origin` and `apmw` tag references examined | No tags were fetched or changed |
| Sibling version-bump commit | `24c1a267f5c65362c9efb4e13e6f76e86564d699`, titled `Bump ChecksMate client version to 0.3.2` | This is the source-equivalent snapshot confirmed from the published asset, not a discovered producer tag |
| Published `checksmate.apworld` | Asset ID **449986468**, 243280 bytes | This is direct released producer evidence, unlike a nearby commit date |

Tag types and peeling came from `git cat-file`, `git rev-parse`, and first-party tag metadata. Client source is `C:\GitHub\chessv\APMW.Client\ApmwConstants.cs:19-20` at the ChessV tag. The unrelated upstream version is `C:\GitHub\rft50-checksmate\Utils.py:32-33` at its `0.3.2` tag.[^release]

The released world has SHA-256 `6c0f81ad139c71d3bf98ba4c521741b238b6b751920c38579dd6f37beaf2867d`. All **32 tracked world files** match producer commit `24c1a267f5c65362c9efb4e13e6f76e86564d699` after CRLF-to-LF normalization: 29 Python files and three documentation files. No tracked file is missing or substantively different. The archive also contains bytecode, which was not executed or used as calibration evidence.[^artifact]

This establishes a **source-equivalent producer snapshot**, designated **P** below. Its `__init__.py:39` requires client 0.3.2. Its Locations Git blob is `8882b66178e4861c93aa23251bf5dfda0c972a73`. Its Rules blob is `1867c0a45dbd9c6290ed36cf6ae13c409b3ddff0`. The archive contains no examined build manifest proving a unique packaging commit. Source equality, rather than timestamp proximity, establishes the calibration boundary.[^artifact]

`P` is an ancestor of current ChecksMate HEAD and of `17df6adc6d`. The local disabled `worlds_disabled\checksmate.apworld` has a different archive hash and was not substituted for the release. No client ZIP download was necessary.

The parent independently downloaded the small published world asset in memory
and confirmed the SHA-256. AST inspection confirmed representative released
Attendant, pawn I/J, and grand-series endpoint values, with Outer Attendant and
pawn K/L entries absent. No archive code was executed.

**Snapshot:** ChecksMate HEAD is `0772bac724ff483043b56979f0ef078b9c2d264a` (**H**), with a clean worktree. ChessV HEAD is `888710f99cf007b982ea1f6a8a64c1e8058be196` (**V**). Ticket 16 and other interview tickets are modified. The research directory and ADR directory are untracked interview work. These observations came from local `git status` and `git rev-parse`, not release records.

## 2. Ledger conventions and evidence limits

The investigation used actual blame, `log --follow`, introducing diffs, and per-column numeric comparisons across 129 relevant source-history commits. Rename-only, formatting-only, ID-only, and stage-only edits do not count as new material calibration.

The compact citations below expand to these exact local paths. Each citation gives **commit:alias:line(s)**. Ten-character commit prefixes identify local Git objects, not estimated dates.

| Alias | Absolute source path |
| --- | --- |
| `L` | `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py` |
| `R` | `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py` |
| `oL` | `C:\GitHub\rft50-checksmate\worlds\checks_mate\Locations.py` |
| `oR` | `C:\GitHub\rft50-checksmate\worlds\checks_mate\Rules.py` |
| `l` | `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py` |

**R/U** means released presence is proved, but version-matched gameplay calibration is not established. **R/F** adds an explicit feedback-related change, without a test protocol or measured outcomes. **N/U** means absent from 0.3.2, introduced later, with no located role-specific gameplay-calibration evidence. These labels do not claim that a person never tested a value.

**Important distinction:** Commit `a49040a4ab` explicitly says "incorporate feedback involving relaxed locations". Its diff changes pawn and base-bishop values. That is documented calibration effort, but not proof of a particular number of games or validation of every final value. Commit `86e670b9c5` reports that multiple-pawn captures were harder than expected. Its numbers precede several later corrections.[^evidence]

The released `TestLocationLogic.py:10-48` constructs collection state and evaluates resource accessibility. It does not measure chess-match success. Current `H:l:71-77` returns `MaterialCalibration.CALIBRATED` whenever a material field is present. That enum label is not empirical calibration evidence.[^tests]

Released source presence also does not prove that gameplay exercised a particular field. `P:R:150-157` selects `min(base,grand)` in some expanded configurations. A calibration record must identify the configuration and resource path before it establishes evidence for an authored grand price.

### Role and condition continuity

The initial stub `4fec0e5586:oL:25-31` contains Rook/Queen predecessors and swapped Bishop/Knight labels. The unambiguous seven home roles appear as Piece A/B/C/D/F/G/H in `2977e4558d:oL:26-32` and `oR:59-65`. The Rook and Queen predecessors are noted separately below.

`15b04a5d20:L:38-49` introduces the grand profile and descriptive home-role names. `b09aea136a:L:38-56` adds the `Capture Piece` prefix without changing those numbers. `31b982671d:L:22-86` moves earlier material constants out of Rules. None of these naming/storage changes proves fresh play calibration.

The released client maps original files to named home roles and uses the captured unit's starting square. Its grand formation is **10x8**, with home array `rnabqkbcnr`, despite stale comments that say 10x10.[^client] A familiar Nightrider, Lion, or Bishop does not prove that a new formation role was tested.

## 3. Individual non-pawn reference bank

Current entries are `H:l:128-131,169-184`. `Q` and `K` in role labels mean Queen's side and King's side. Introduction means introduction of that role/profile, not introduction of its current price.

| Profile / role | Current | Released 0.3.2 | Introduction | Last material-value change | Status |
| --- | ---: | --- | --- | --- | --- |
| Base8 Q Rook | 1500 | 1500, `P:L:45` | Rook predecessor `4fec0e5586:oL:25`, canonical `2977e4558d:oL:26` | `c1f53b70fc:L:45` | R/U |
| Base8 Q Knight | 700 | 700, `P:L:46` | `2977e4558d:oL:27` | `e6bd565c12:R:105` | R/U |
| Base8 Q Bishop | 1040 | 1040, `P:L:47` | `2977e4558d:oL:28` | `a49040a4ab:L:47` | R/F |
| Base8 Queen | 1300 | 1300, `P:L:48` | Queen predecessor `4fec0e5586:oL:31`, canonical `2977e4558d:oL:29` | `e6bd565c12:R:107` | R/U |
| Base8 K Bishop | 1140 | 1140, `P:L:52` | `2977e4558d:oL:30` | `a49040a4ab:L:52` | R/F |
| Base8 K Knight | 1040 | 1040, `P:L:53` | `2977e4558d:oL:31` | `e6bd565c12:R:110` | R/U |
| Base8 K Rook | 1900 | 1900, `P:L:54` | Rook predecessor `4fec0e5586:oL:26`, canonical `2977e4558d:oL:32` | `c1f53b70fc:L:54` | R/U |
| Grand10 Q Rook | 2850 | 2850, `P:L:45` | `15b04a5d20:L:38` | `c1f53b70fc:L:45` | R/U |
| Grand10 Q Knight | 1200 | 1200, `P:L:46` | `15b04a5d20:L:39` | Same commit/line | R/U |
| Grand10 Q Bishop | 1200 | 1200, `P:L:47` | `15b04a5d20:L:40` | Same commit/line | R/U |
| Grand10 Queen | 4100 | 4100, `P:L:48` | `15b04a5d20:L:41` | `625d2dff11:L:48` | R/U |
| Grand10 K Bishop | 1400 | 1400, `P:L:52` | `15b04a5d20:L:45` | Same commit/line | R/U |
| Grand10 K Knight | 1400 | 1400, `P:L:53` | `15b04a5d20:L:46` | Same commit/line | R/U |
| Grand10 K Rook | 3250 | 3250, `P:L:54` | `15b04a5d20:L:47` | `c1f53b70fc:L:54` | R/U |
| Grand10 Q Attendant | 3950 | 3950, `P:L:55` | `15b04a5d20:L:48` | `625d2dff11:L:55` | R/U |
| Grand10 K Attendant | 4030 | 4030, `P:L:56` | `15b04a5d20:L:49` | `625d2dff11:L:56` | R/U |
| Legacy12 Q Outer Attendant | 4110 | Absent | `17df6adc6d:L:94-95` | Same commit/lines | N/U |
| Legacy12 K Outer Attendant | 4190 | Absent | `17df6adc6d:L:97-98` | Same commit/lines | N/U |

The November 2023 `+400` accessibility increment and kingside rationale are explicit in `e6bd565c12:R:103-111`. The later Rook correction says capturing Rooks is not easier than capturing two pieces. Neither rationale supplies a play-test dataset for the final bank (`c1f53b70fc:L:45,54`, commit subject).

The Outer Attendant values are not old grand10 values with renamed roles. The release has only the two inner Attendants. Their proposed later Lion mapping also changes the formation and target movement. Current Nightrider MG550 and the old twelve-file inventory are current catalog/profile evidence, not proof that the 4110/4190 constants were calibrated against them.[^catalog]

## 4. Pawn control points

Each A-H row identifies both base8 and grand10 values. Current entries are `H:l:112-126`. Their original Location identities begin in `4fec0e5586:oL:17-24`. Explicit base material100 first appears in `283a10772c:oR:70-77`, in A-H order.

| File | Current base / grand | Released base / grand | Grand-profile introduction | Last base change | Last grand change | Status |
| --- | --- | --- | --- | --- | --- | --- |
| A | 490 / 1010 | Same, `P:L:32` | `15b04a5d20:L:26` | `a49040a4ab:L:32` | `625d2dff11:L:32` | R/F |
| B | 340 / 810 | Same, `P:L:33` | `15b04a5d20:L:27` | `a49040a4ab:L:33` | `625d2dff11:L:33` | R/F |
| C | 220 / 660 | Same, `P:L:35` | `15b04a5d20:L:28` | `a49040a4ab:L:35` | `625d2dff11:L:35` | R/F |
| D | 100 / 520 | Same, `P:L:36` | `15b04a5d20:L:29` | `283a10772c:oR:73` | `625d2dff11:L:36` | R/U base, R/F grand |
| E | 100 / 320 | Same, `P:L:37` | `15b04a5d20:L:30` | `283a10772c:oR:74` | `625d2dff11:L:37` | R/U |
| F | 320 / 320 | Same, `P:L:38` | `15b04a5d20:L:32` | `625d2dff11:L:38` | `625d2dff11:L:38` | R/F base, R/U grand |
| G | 390 / 620 | Same, `P:L:39` | `15b04a5d20:L:33` | `a49040a4ab:L:39` | `625d2dff11:L:39` | R/F |
| H | 490 / 860 | Same, `P:L:41` | `15b04a5d20:L:35` | `a49040a4ab:L:41` | `625d2dff11:L:41` | R/F |
| I | Absent / 810 | Same, `P:L:42` | `15b04a5d20:L:36` | Not applicable | `a49040a4ab:L:42` | R/F |
| J | Absent / 890 | Same, `P:L:43` | `15b04a5d20:L:37` | Not applicable | `a49040a4ab:L:43` | R/F |
| K | Absent / 970 | Absent | `17df6adc6d:L:64` | Not applicable | Same commit/line | N/U |
| L | Absent / 1050 | Absent | `17df6adc6d:L:65` | Not applicable | Same commit/line | N/U |

For grand A-D/G/H and base F, the feedback marker concerns a predecessor value. December 2024 changes follow it. Physical-line blame attributes all A-H rows to December because the grand column changed. It does not mean base D/E100 was introduced then.

**Arithmetic, not recovered intent:** I/J/K/L form the 810,890,970,1050 run. I/J were already released and have an explicit feedback-related correction. K/L extend the +80 run only in the later expansion. This arithmetic does not establish a trusted four-point curve. Released A-J values also retain center/edge and side asymmetry, rather than one global linear file ramp (`P:L:31-43`, `17df6adc6d:L:62-65`).

The central100/320 inputs behind the proposed pawn-pressure coefficient are released values. Their provenance does not approve that coefficient, normalized interpolation, or equal front/Rearguard pricing. Those remain separate author choices.[^policy]

## 5. The 74-point series bank

Every value in the next two tables equals the value in the published 0.3.2 world. Current counterparts are `H:l:194-242,259-283`. Lists correspond to consecutive counts in the displayed range. All rows have **R/U** status. The earlier multiple-pawn observations support historical effort, not proof of the final curve's measured accuracy.[^evidence]

| Profile / counts | Current = 0.3.2 values | Introduction | Last material-value change | Released source |
| --- | --- | --- | --- | --- |
| Base Pawns2 | 750 | `2977e4558d:oL:40` | `c518afee64:L:64` | `P:L:65` |
| Base Pawns3 | 1450 | `2977e4558d:oL:41` | `a5ffe61a46:L:66` | `P:L:66` |
| Base Pawns4-8 | 2240,2620,2975,3255,3545 | `2977e4558d:oL:42-46` | `29c262935a:L:67-71` | `P:L:67-71` |
| Grand Pawns2-8 | 1650,2450,3240,3620,3975,4255,4545 | `15b04a5d20:L:58-64` | `c1f53b70fc:L:65-71` | `P:L:65-71` |
| Base Pieces2-7 | 1450,2100,2770,2950,3300,3750 | `2977e4558d:oL:47-52` | `29c262935a:L:75-80` | `P:L:75-80` |
| Grand Pieces2-7 | 3000,3400,3750,4150,4500,4900 | `15b04a5d20:L:68-73` | `a5ffe61a46:L:75-80` | `P:L:75-80` |
| Base Each2-5 | 2250,2650,2950,3200 | `283a10772c:oL:54-57` | `625d2dff11:L:83-86` | `P:L:83-86` |
| Base Each6 | 3500 | `283a10772c:oL:58` | `6c7e26c091:L:68` | `P:L:87` |
| Base Each7 | 3850 | `283a10772c:oL:59` | `6201bbd4bd:R:109` | `P:L:88` |
| Grand Each2-6 | 4150,4550,4900,5200,5450 | `15b04a5d20:L:76-80` | `625d2dff11:L:83-87` | `P:L:83-87` |
| Grand Each7 | 5650 | `15b04a5d20:L:81` | `1f3eeff32a:L:88` | `P:L:88` |
| Base Any2 | 750 | `2144259e8c:L:71` | `c518afee64:L:91` | `P:L:92` |
| Base Any3-14 | 1450,2240,2500,2700,2850,3000,3150,3300,3450,3600,3750,3900 | `2144259e8c:L:72-83` | `c1f53b70fc:L:93-104` | `P:L:93-104` |
| Grand Any2-10 | 1650,2450,3240,3500,3700,3850,4000,4150,4350 | `15b04a5d20:L:85-93` | `c1f53b70fc:L:92-100` | `P:L:92-100` |
| Grand Any11 | 4650 | `15b04a5d20:L:94` | Same commit/line | `P:L:101` |
| Grand Any12-14 | 5000,5350,5600 | `15b04a5d20:L:95-97` | `c1f53b70fc:L:102-104` | `P:L:102-104` |

### Explicit high-count grand10 audit

These ten records were not introduced by the twelve-file expansion. Each originates in the June 2024 grand-profile commit. The released client also has the corresponding capture-count reporting paths.[^client]

| Goal | Current = 0.3.2 | Introduction | Last material-value change | Released source |
| --- | ---: | --- | --- | --- |
| Pawns9 | 4645 | `15b04a5d20:L:65` | `c518afee64:L:71` | `P:L:72` |
| Pawns10 | 5245 | `15b04a5d20:L:66` | `c518afee64:L:72` | `P:L:73` |
| Pieces8 | 5200 | `15b04a5d20:L:74` | `a5ffe61a46:L:81` | `P:L:81` |
| Pieces9 | 5400 | `15b04a5d20:L:75` | `c518afee64:L:81` | `P:L:82` |
| Each8 | 5850 | `15b04a5d20:L:82` | `1f3eeff32a:L:89` | `P:L:89` |
| Each9 | 5950 | `15b04a5d20:L:83` | `c518afee64:L:89` | `P:L:90` |
| Any15 | 5750 | `15b04a5d20:L:98` | `c1f53b70fc:L:105` | `P:L:105` |
| Any16 | 5850 | `15b04a5d20:L:99` | `c1f53b70fc:L:106` | `P:L:106` |
| Any17 | 5950 | `15b04a5d20:L:100` | `c1f53b70fc:L:107` | `P:L:107` |
| Any18 | 6000 | `15b04a5d20:L:101` | `c1f53b70fc:L:108` | `P:L:108` |

The introducing commit reused some IDs. Corrections appear in `e339f3b31b:L:65-66,82-83,98-101` and `b64b8a3b2c:L:74-75,98-101`. The later `17df6adc6d:L:115-119,134-137,152-155,178-188` adds multiline formatting and stage flags around these old endpoints. Blame on those lines alone incorrectly dates their numeric introduction to 2026.

### Later additions and endgame anchors

| Reference | Current value | 0.3.2 value/presence | Introduction / last material change | Status |
| --- | --- | --- | --- | --- |
| Legacy12 Pawns11/12 | 5845/6445 | Absent | Both `17df6adc6d:L:121-125` | N/U |
| Legacy12 Pieces10/11 | 6100/6800 | Absent | Both `17df6adc6d:L:140-144` | N/U |
| Legacy12 Each10/11 | 6900/7850 | Absent | Both `17df6adc6d:L:158-162` | N/U |
| Legacy12 Any19-22 | 6400,6900,7450,8000 | Absent | Both `17df6adc6d:L:190-200` | N/U |
| Base full-clear budget | 4020 | 4020, `P:L:91` | Goal `283a10772c:oL:60`, numeric4020 `4bf9f4ed23:L:70` | R/U |
| Historical grand10 clear reference | 6050 | 6050, `P:L:91` | `15b04a5d20:L:84` | R/U |
| Current shared grand Everything field | 8050 | Field was6050 | Numeric change `17df6adc6d:L:164`, current `H:l:250-258` | N/U for8050 |
| Standard full-army mate reference | 4020 | Minima4020, `P:L:49` | Numeric4020 `4bf9f4ed23:L:37`, renamed/reassigned `15b04a5d20:L:42` | R/U |
| Grand10 mate reference | 6020 | Maxima6020, `P:L:50` | `15b04a5d20:L:43` | R/U |
| Added 10x10 / 12x10 mate fields | 6020 / 8020 | Absent | `17df6adc6d:L:33-34,75-79`, unchanged numeric basis | N/U |

Before grand chess, the standard full-army mate was named Maxima. The grand-profile introduction uses Minima for4020 and Maxima for6020. Those are semantic predecessors, not interchangeable names for the same geometry. Base Everything4020 is temporarily disabled by the grand introduction and restored in `b09aea136a:L:84`. Its numeric amount remains the earlier4020.

Only the shared Everything grand amount differs between current and released common material records. The approved new10x10 clear8050 is an independently selected inventory-delta budget. Its equality to the later legacy12 field does not make that field a released anchor or revoke the new budget.[^policy]

## 6. Task-specific records retained by ADR 0011

All pairs below are **current base/grand = released base/grand**, with **R/U** status. Current source is `H:l:188-192,298-317`. The table separates introduction from the last numeric change in each column.

| Goal | Base / grand | Role/profile introduction | Last base / grand material change | Released source |
| --- | --- | --- | --- | --- |
| King to E2/E7 Early | 0 / 0 | Bongcloud Once `2977e4558d:oL:34`, grand `15b04a5d20:L:52` | `31b982671d:L:43` / `15b04a5d20:L:52` | `P:L:59` |
| King to Center | 50 / 50 | Bongcloud Center `2977e4558d:oL:35`, grand `15b04a5d20:L:53` | `6cf26bd573:R:146` / `15b04a5d20:L:53` | `P:L:60` |
| King to A File | 0 / 150 | Bongcloud A File `2977e4558d:oL:36`, grand `15b04a5d20:L:54` | `939d62dac8:L:45` / `625d2dff11:L:61` | `P:L:61` |
| King Captures Anything | 150 / 350 | Bongcloud Capture `2977e4558d:oL:37`, grand `15b04a5d20:L:55` | `e1a78b70d1:L:46` / `c518afee64:L:61` | `P:L:62` |
| King to Back Rank | 2250 / 5150 | Bongcloud Promotion `2977e4558d:oL:38`, grand `15b04a5d20:L:56` | Both `c518afee64:L:62` | `P:L:63` |
| Survive3 | 0 / 0 | `c518afee64:L:108` | Same commit/line, both columns | `P:L:109` |
| Survive5 | 200 / 330 | `c518afee64:L:109` | Both `3bcf48813a:L:109` | `P:L:110` |
| Survive10 | 2500 / 4500 | `c518afee64:L:110` | Both `3bcf48813a:L:110` | `P:L:111` |
| Survive20 | 3800 / 5800 | `c518afee64:L:111` | Same commit/line, both columns | `P:L:112` |
| Threaten Pawn | 0 / 0 | Pawn Threat `283a10772c:oL:61`, grand `15b04a5d20:L:103` | `31b982671d:L:70` / `15b04a5d20:L:103` | `P:L:114` |
| Threaten Minor | 200 / 400 | Minor Threat `7af33303fc:oL:62`, grand `15b04a5d20:L:104` | `b75b578af1:L:71` / `15b04a5d20:L:104` | `P:L:115` |
| Threaten Major | 300 / 500 | Major Threat `7af33303fc:oL:63`, grand `15b04a5d20:L:105` | Both `15b04a5d20:L:105` | `P:L:116` |
| Threaten Queen | 300 / 500 | Queen Threat `7af33303fc:oL:64`, grand `15b04a5d20:L:106` | `64caa4f20a:oR:119` / `15b04a5d20:L:106` | `P:L:117` |
| Threaten King | 1000 / 1800 | King Threat `283a10772c:oL:63`, grand `15b04a5d20:L:107` | `e1a78b70d1:L:74` / `15b04a5d20:L:107` | `P:L:118` |
| Fork, Sacrificial | 700 / 1100 | Fork `2977e4558d:oL:56`, grand `15b04a5d20:L:111` | `c8f4a9da7a:oR:105` / `81e12a7d86:L:111` | `P:L:122` |
| Fork, Sacrificial Triple | 3300 / 2700 | Triple Fork `4aec531ae7:oL:71`, grand `15b04a5d20:L:112` | `625d2dff11:L:123` / `15b04a5d20:L:112` | `P:L:123` |
| Fork, Sacrificial Royal | 3600 / 5200 | Royal Fork `2977e4558d:oL:57`, grand `15b04a5d20:L:114` | `625d2dff11:L:125` / `15b04a5d20:L:114` | `P:L:125` |
| Fork, True | 3150 / 4550 | `3e69a5ebcb:oL:73`, grand `15b04a5d20:L:115` | `625d2dff11:L:126` / `15b04a5d20:L:115` | `P:L:126` |
| Fork, True Triple | 3850 / 5850 | `3e69a5ebcb:oL:74`, grand `15b04a5d20:L:116` | Both `625d2dff11:L:127` | `P:L:127` |
| Fork, True Royal | 4020 / 6020 | `3e69a5ebcb:oL:75`, grand `15b04a5d20:L:118` | `4bf9f4ed23:L:84` / `15b04a5d20:L:118` | `P:L:129` |
| O-O Castle | 0 / 0 | Early `00 Castle` `2977e4558d:oL:54`, re-enabled `d1e4d22c75:L:77` | `31b982671d:L:85` / `15b04a5d20:L:120` | `P:L:130` |
| O-O-O Castle | 0 / 0 | Early `000 Castle` `2977e4558d:oL:55`, re-enabled `d1e4d22c75:L:78` | `31b982671d:L:86` / `15b04a5d20:L:121` | `P:L:131` |

Survival names retain `Current Objective: Survive n Turns`. `939d62dac8:L:43-47` renames the Bongcloud actions. `ce81d45814:oL:61-65` renames the threat categories. `3e69a5ebcb:oL:70-75` retains the older fork conditions under Sacrificial names and adds the True variants.

The released client explicitly emits these fork categories. The Royal condition includes King and Queen threats, not capture of a King. `King Captures Anything` examines the **moving player's King**. It is not a predecessor for Regicide. The old Bongcloud Promotion name becomes a back-rank movement goal, not promotion of a King.[^conditions]

True Royal Fork's released4020/6020 pair supports its historical endgame link. Its new geometry-specific budgets remain the accepted ADR 0011 decision, not historical play-test observations.[^policy]

## 7. Consequences for the pending reference choices

| Proposed input or output | Provenance assessment | Consequence |
| --- | --- | --- |
| Base8 seven and grand10 nine non-pawn references | Released at the listed values | Safe to identify as released historical baselines. Not proof of measured balance |
| Forward Bishop/Knight, Elephant, Amazon, compact Center Rook analogues | Source references can be released, but the new role/formation mappings are not thereby validated | Reference mapping remains an explicit author decision |
| Lion6035/6115 from Outer4110/4190 | Depends on post-release Outer constants and later legacy12 context | Provisional, not a trusted released Lion anchor |
| Grand Knight as an alternative Lion analogue | Knight1200/1400 is released | Does not automatically approve its transfer to a Lion |
| Pawn A-H base and A-J grand control points | Released, with the feedback qualifications above | A defensible historical input set, without approving the proposed curve |
| Pawn K/L970/1050 | Later +80 continuation, absent from the release | Cannot be promoted to trusted curve controls solely because they exist in source |
| All74 series points | Exact released source values | No release-presence defect requires replacement of this bank |
| Legacy12 series extensions and clear8050 | Post-release source additions | Comparison evidence, not mandatory anchors for the accepted endpoint model |
| Regicide4725 | No matching released capture-goal budget | Remains unaccepted. Queen4100 grand is released, but supplies no royal-capture premium |

The Lion proposals use current Nightrider550, old inventory7700, and legacy12 clear8050. These inputs are not the released grand10 profile. Their dependency on later constants survives application of the accepted half-growth floor. This audit neither replaces them with another pair nor changes the approved new12 clear11250.[^catalog][^policy]

**Corrections to possible earlier interpretations:** "Pre-twelve-file", "authored", "calibrated" in an enum, and "reproduced by interpolation" are not testing claims. The earlier 74-point reproduction result alone proved no release membership. This audit now proves membership from the asset, including all ten disputed high-count endpoints. It still does not prove their gameplay calibration.

**Smallest remaining factual gap:** Located comments and commit messages document judgment and some feedback, but not per-reference match records. Such records could strengthen evidence for released values or later additions. Absence from this audit is not proof that no testing occurred.

**Smallest author decisions:** Decide whether to permit later uncorroborated constants as references for Lions or pawn control points. Then settle the remaining role analogues, pawn curve, and Regicide's progression position relative to the released Queen reference. No numerical choice is made here. Replacing any selected input requires an explicit decision and regeneration of its dependent outputs.[^policy]

## Primary source details

[^release]: First-party release metadata: <https://github.com/chesslogic/chessv/releases/tag/v0.3.2>, read with `gh release view`. Tag object: <https://api.github.com/repos/chesslogic/chessv/git/ref/tags/v0.3.2>. Local tag peeling confirms the same ChessV commit. Upstream sibling tag: `5d3b4c8efd413c3b808d6d5b979ea3736da0a078`, `C:\GitHub\rft50-checksmate\Utils.py:32-33`, commit metadata and empty ChecksMate tree.
[^artifact]: First-party asset <https://github.com/chesslogic/chessv/releases/download/v0.3.2/checksmate.apworld>, API asset449986468. Read in memory only. Source comparison used `24c1a267f5c65362c9efb4e13e6f76e86564d699` and its complete `worlds\checksmate` tree. Archive `checksmate\Locations.py:29-139` matches `P:L:29-139` after newline normalization. Archive and producer `__init__.py:39` require client0.3.2. Asset creation/update metadata is 2026-06-17 07:14:07/08 UTC. Those times are not used to infer a build revision.
[^later]: `17df6adc6d89af7b88bf2bcd3002c4e66defd535:L:60-98,121-164,190-200`, its parent diff, and absence from the release's complete `P:L:29-139` table. Ancestry was established with `git merge-base --is-ancestor`, not dates.
[^evidence]: `a49040a4ab67d64b3335e0e6f6755b9087108fb8:L:31-56` and commit body. Earlier observation: `86e670b9c512fae044f76968ce5bef70e41d74da:R:91-97` and commit body. Further qualitative correction: `6c7e26c0911cefa4929feb3aae28d95f47e95a4c:L:50-71`. Scope included relevant source histories, their comments/messages, world documentation, and static test definitions. External chat and private match logs were not available as evidence.
[^tests]: `C:\GitHub\rft50-checksmate\worlds\checksmate\test\TestLocationLogic.py:10-48` at P. Current `H:l:71-77` is the enum assignment, not a gameplay result. The release notes discuss move-generation, crash, and contract tests, not a capture-price calibration study.
[^client]: At ChessV tag commit `6fe99970b574f11e35d52e080c03a5eefbdeedbb`: `C:\GitHub\chessv\APMW.Client\CaptureLookup.cs:15-57`, `APMW.Client\LocationHandler.cs:432-490`, and `ChessV.Games\MiscellaneousGames\ApmwGrandChess.cs:10-36`. The latter declares 10 files and 8 ranks.
[^conditions]: At the same ChessV tag: `C:\GitHub\chessv\APMW.Client\LocationHandler.cs:329-357,407-416,608-632`. These are condition implementations, not evidence of calibration quality.
[^catalog]: At V: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:38-50` and `ApmwProfiles.cs:394-414`. Current twelve-file home array `rjnabqkbcnjr` contains the two Nightriders. Their MG550 does not establish the derivation of later producer constants.
[^policy]: Working-tree accepted decisions: `C:\GitHub\chessv\docs\adr\0008-separate-calibration-from-reachability.md:6-38`, `0009-anchor-provisional-endgame-budgets.md:13-44`, `0010-transfer-capture-series-curves-by-endpoints.md:6-53`, `0011-preserve-task-specific-calibration.md:6-42`, and `0012-floor-non-pawn-accessibility-growth.md:12-56`. Pending reference decisions and user feedback: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\16-define-material-calibration-contract.md:385-530`.
