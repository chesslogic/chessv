# Capture-series material: bounded candidate comparison

**Finding:** History supports retained authored curves and an endgame-oriented interpretation. It does not uniquely identify a transfer function. Three simple candidates expose different assumptions and missing boundaries. None is an approved equation or a released threshold.

**Later decision:** The user selected the separate
[endpoint-anchored model](capture-series-endpoint-model.md) as the provisional
function. This report remains the comparison of the initial three alternatives.

**Classification:** **E** = source evidence or accepted decision. **I** = mathematical inference from stated inputs. **P** = proposed function or recommendation. **O** = open assumption or unsupported interpretation.

**E - Local snapshot:** ChecksMate HEAD is `0772bac724ff483043b56979f0ef078b9c2d264a`, with a clean worktree. ChessV HEAD is `888710f99cf007b982ea1f6a8a64c1e8058be196`. Local `git rev-parse HEAD` and `git status --short` supplied this snapshot. Ticket 16 is modified. ADRs 0008/0009 and the earlier history report are untracked interview records, not release evidence.

## Evidence and reproducible inputs

**E:** Commit `a690076d30` describes series calibration as "thinking of them in reverse from endgame state". It supplies no remaining-resource equation. The accessibility comment about an extra piece and pawn concerns individual pieces, not a series generator.[^rationale]

**E:** The table separates current base8 anchors from the historical grand10 anchors. The latter already exist before the twelve-file expansion. December 2024 corrections explain the final grand pawn/Any and revised Each values.[^current][^grand][^corrections]

| Series | Base8: values for consecutive n, starting at 2 | Grand10: values for consecutive n, starting at 2 |
| --- | --- | --- |
| Pawns | 750,1450,2240,2620,2975,3255,3545 | 1650,2450,3240,3620,3975,4255,4545,4645,5245 |
| Pieces | 1450,2100,2770,2950,3300,3750 | 3000,3400,3750,4150,4500,4900,5200,5400 |
| Each | 2250,2650,2950,3200,3500,3850 | 4150,4550,4900,5200,5450,5650,5850,5950 |
| Any | 750,1450,2240,2500,2700,2850,3000,3150,3300,3450,3600,3750,3900 | 1650,2450,3240,3500,3700,3850,4000,4150,4350,4650,5000,5350,5600,5750,5850,5950,6000 |

**E:** Commit `17df6adc6d` appends these ten numbered-series observations for twelve files and changes Everything from 6050 to 8050. Its unchanged grand-column prefix is not independent intrinsic12 data.[^grand][^append]

| Explicit legacy12 additions | Authored values |
| --- | --- |
| Pawns 11,12 | 5845,6445 |
| Pieces 10,11 | 6100,6800 |
| Each 10,11 | 6900,7850 |
| Any 19,20,21,22 | 6400,6900,7450,8000 |

**I:** The appended steps are +600 per Pawn, +700 per Piece, and +950 per Each count. Grand10's preceding steps are +600, +200, and +100 respectively. The pawn continuation repeats its last slope, but the other two use larger steps. Grand10 Any18 and legacy12 Any22 each have a 50-unit clear gap. Base8 Any14 instead has a 120-unit gap.[^grand][^append]

**E:** `b_g` is the approved clear budget, not the checkmate budget. `V` includes every starting Standard CPU King. The references are base8 for 6x8/8x8 and grand10 for larger geometries. Budgets use `b_g=b_r+V_g-V_r`, with no additional premium.[^budgets]

| Geometry g | P/M/K | V_g | b_g | Normal clear N_g | Published Pawns/Pieces/Each/Any ceilings |
| --- | --- | ---: | ---: | ---: | --- |
| 6x8 | 6/5/1 | 2725 | 2370 | 11 | 6/5/5/10 |
| 8x8 | 8/7/1 | 4375 | 4020 | 15 | 8/7/7/14 |
| 10x8 | 10/9/1 | 6400 | 6050 | 19 | 10/9/9/18 |
| 10x10 | 12/15/1 | 8400 | 8050 | 27 | 12/15/12/26 |
| 12x10 | 14/18/2 | 11600 | 11250 | 33 | 14/19/14/32 |

**E:** `N_g=P+M+K-1`. For Each, `q(n)=2n` and `k=2`. Other numbered series use `q(n)=n` and `k=1`. Player chessmen remain `q(n)-1` before their separate cap. Every integer from 2 through the endpoint ceiling remains published.[^counts][^separation]

## Three candidate functions

**P - Shared interpolation:** Let `f_r,c(j)` be an authored reference value for class `c`, and `m_r,c` its last authored count. `L_r,c(x)` returns the anchor at integer `x`. Between adjacent anchors, it returns `f(j)+(x-j)*(f(j+1)-f(j))`, where `j=floor(x)`. Arithmetic remains exact until final processing.

**P/E - Domain:** The functions price feasible published counts and the one-King full-clear boundary on the declared target geometry. Reference reuse supplies no playable-board access and takes no blanket base/grand minimum.[^separation]

**P - Boundary convention:** For the one-King reference profiles only, extend `L_Any` with the virtual point `(N_r,b_r)`. This prices a non-King full clear on that geometry. It does not create a Location. Other classes receive no additional reference anchor. Outside the resulting interpolation domain, `L` is undefined. In particular, there is no invented count-0 or count-1 value.[^counts]

### A. Absolute-count anchors with a controlled tail

**P:** A preserves `F_A(n)=L_r,c(n)` through `m_r,c`. Afterwards, it uses `F_A(n)=f(m)+(n-m)*s`, where `s=f(m)-f(m-1)`. The grand10 tail slopes are Pawns600, Pieces200, Each100, and Any50. These are reference differences, not fitted new coefficients.

**P:** One-King Any instead connects its retained prefix to the target clear boundary. Let `a` be the largest authored reference count less than `N_g`. A retains all anchors through `a`. For `a<n<=N_g`, it uses `F_A(n)=f(a)+(b_g-f(a))*(n-a)/(N_g-a)`. If `b_g<f(a)`, the profile is invalid. Multi-King Any retains the last-slope tail, without a target clear anchor.

**O:** A assumes that equal absolute counts preserve difficulty despite formation changes. Its ordinary tails ignore the inventory delta. Only the one-King Any bridge uses the new clear budget. History supports retained anchors and finite linear runs, not unlimited continuation of their slopes.[^grand][^append]

### B. Stretch normal-clear progress and scale the budget

**P:** `x=n*N_r/N_g`, then `F_B(n)=(b_g/b_r)*L_r,c(x)`. This preserves the shape at equal fractions of normal-clear capture workload. It scales every ordinate, including authored accessibility residuals, by the clear-budget ratio.

**O:** Neither proportional progress nor proportional residual scaling appears as a recovered historical equation. This candidate uses the approved budget as a scale, not as evidence for those assumptions. A changed pawn/piece mixture can place `x` outside the reference class domain.

### C. Transfer an absolute endgame gap by count deficit

**P:** Let `d=N_g-q(n)` and `x=(N_r-d)/k = n-(N_g-N_r)/k`. Then `F_C(n)=b_g-[b_r-L_r,c(x)]`. This preserves the reference gap at an equal normal-clear count deficit. It changes both the count coordinate and the budget, rather than adding a blanket delta at unchanged `n`.

**O:** The historical backward-from-endgame rationale motivates this candidate, but does not establish this particular deficit coordinate.[^rationale] A gap is not necessarily the value of uncaptured enemy material. Neither a capture order nor an expected trade inventory is supplied by the series table.[^grand]

**P/O - Multi-King scope:** B and C use `N_g` as a workload scale despite the extra eligible King capture. If extended to Any33, both algebraically give `b_g`. That numerical equality has no justification as an Everything identity. The proposed multi-King Any domain stops at 32, subject to reference-domain limits. All candidates exclude Any33/34 and create no such anchors or Locations.

## Reproduction versus conditional prediction

**I:** Each candidate exactly reproduces all 74 supplied base8/grand10 anchors when its target equals its reference. This is interpolation by construction, not predictive evidence.

**I - Method:** The comparisons use Python standard-library `ast`, `fractions.Fraction`, and arithmetic without game imports. Holdout values never enter the reference interpolator or determine a tail coefficient. Target clear budgets are supplied inputs: 6050 for grand10 and historical 8050 for legacy12. The legacy12 workload is 23 captures, from its old one-King formation.[^grand][^append][^old-army] These are retrospective, conditional comparisons of authored values, not blind experiments or play outcomes.

| Reference -> held-out points | Candidate | Defined / total | Raw MAE | MAE on common defined points |
| --- | --- | ---: | ---: | ---: |
| Base8 -> all 42 grand10 points | A | 42/42 | 1214.5 | 1239.6 |
| Same | B | 36/42 | 364.0 | 258.9 |
| Same | C | 28/42 | 402.5 | 414.6 |
| Grand10 -> ten appended legacy12 points | A | 10/10 | 472.0 | 252.5 |
| Same | B | 8/10 | 683.5 | 683.5 |
| Same | C | 10/10 | 486.0 | 582.5 |

**I:** The common domains contain 26 and eight points respectively. MAE means mean absolute error. Presentation uses one decimal place, but calculations retain exact fractions. The historical legacy12 budget of 8050 is conditional evidence only. It supplies neither an extra 700 nor a replacement for the approved new12 budget of 11250.[^budgets]

| Held-out goal | Authored | A error | B error | C error |
| --- | ---: | ---: | ---: | ---: |
| Grand10 Pawns10 | 5245 | -1120 | +44.2 | -240 |
| Grand10 Each9 | 5950 | -1400 | undefined | -70 |
| Legacy12 Pawns12 | 6445 | 0 | +464.5 | +100 |
| Legacy12 Pieces11 | 6800 | -1000 | undefined | +100 |
| Legacy12 Each11 | 7850 | -1700 | undefined | +100 |
| Legacy12 Any19 | 6400 | +10 | +1343.4 | +1350 |
| Legacy12 Any22 | 8000 | -360 | -5.0 | 0 |

**I:** Error is prediction minus authored value. A reproduces both appended pawn points, but misses the new Pieces/Each slopes. C preserves the grand terminal 50-unit gap, but substantially overpredicts early appended Any points. B transfers base8 to grand10 better on the common domain, but its legacy12 errors and undefined endpoints prevent a universal conclusion.

## Representative target outputs

**I:** Entries are `ceil(F)` at **D=1, R=0, without caps**. They are illustrations, not proposed release thresholds. Undefined means the reference domain cannot supply the required coordinate. Infeasible geometry/count pairs are omitted.

| Geometry | Goal | A | B | C |
| --- | --- | ---: | ---: | ---: |
| 8x8 | Any15, earlier-board full clear | 4020 | 4020 | 4020 |
| 10x8 | Any19, earlier-board full clear | 6050 | 6050 | 6050 |
| 10x10 | Pawns12 | 6445 | 6107 | 5240 |
| 10x10 | Pieces12 | 6000 | 7038 | 5750 |
| 10x10 | Pieces15 | 6600 | undefined | 6900 |
| 10x10 | Each12 | 6250 | 7844 | 7850 |
| 10x10 | Any22 | 6912 | 7715 | 7600 |
| 10x10 | Any26 | 7823 | 8004 | 8000 |
| 10x10 | Any27, earlier-board full clear | 8050 | 8050 | 8050 |
| 12x10 | Pawns12 | 6445 | 7865 | undefined |
| 12x10 | Pawns13 | 7045 | 8174 | undefined |
| 12x10 | Pawns14 | 7645 | 8463 | undefined |
| 12x10 | Pieces12 | 6000 | 9044 | undefined |
| 12x10 | Pieces15 | 6600 | 9907 | undefined |
| 12x10 | Pieces19 | 7400 | undefined | 9350 |
| 12x10 | Each12 | 6250 | 10473 | 10400 |
| 12x10 | Each14 | 6450 | 10890 | 10850 |
| 12x10 | Any22 | 6200 | 9732 | 9200 |
| 12x10 | Any26 | 6400 | 10684 | 10200 |
| 12x10 | Any27 | 6450 | 10794 | 10550 |
| 12x10 | Any28 | 6500 | 10901 | 10800 |
| 12x10 | Any32 | 6700 | 11197 | 11200 |

**I:** For example, B's 10x10 Pawns12 interpolant has `x=76/9` and raw value `604555/99`. Only the final ceiling gives 6107. Any15/19/27 full-clear rows require an existing published Location in the larger world. Their virtual curve boundaries create no new names.[^counts]

## Failure boundaries and the next interview

**I:** B lacks count 2 on 10x10 and counts 2/3 on 12x10. It also lacks Pieces13-15 on 10x10 and Pieces16-19 on 12x10. C lacks every 12x10 Pawn point and counts 2-15 for its Pieces/Any series. C's 12x10 Each domain starts at 9. Inventing lower anchors or extrapolating past the reference piece inventory hides these missing decisions.

**I:** Compact geometry also exposes limitations. A preserves base8 Any10=3300, but its virtual Any11=2370 violates within-series monotonicity. Its compact Any profile is invalid, not silently repaired. B cannot supply compact Pawns6. C cannot supply compact Pawns5/6 or Pieces4/5. Elsewhere, nonnegative reference slopes and positive affine transformations preserve order wherever the functions are defined.

**I:** New10 Each12 needs 24 captures, three short of normal clear 27. New12 Each14 needs 28, five short of normal clear 33. C maps them to grand10 Each8 and Each7, not to Each9. New12 Each14 can leave five non-King pieces with one King retained, or six after both Kings are captured. Count deficit is not always the remaining Everything workload.[^counts]

**E/I:** Two King captures can produce Any33 while a Rook remains. Everything still fails because its condition names the required starting sets. Any33/34 are not published.[^counts] Neither a near-terminal number nor a virtual anchor overrides this condition. Different classes and geometries need not share one numeric ordering.[^separation]

**E:** The accepted processing order is `min(stage_cap, ceil(F*D+R_applicable))`, with no early interpolant rounding. Player configuration difficulty, chessmen caps, actual goal conditions, and geometry access remain separate. The later-board resource bypass remains intact. These proposals do not change those decisions or address Regions.[^separation][^precision]

**P - Recommendation:** Retain the two explicit authored reference curves and document every transfer residual. Do not describe any candidate as recovered historical intent. The smallest remaining material choices are:

1. **Transfer invariant:** Absolute count, fractional normal-clear progress, or absolute count deficit? Historical rationale supports a gap model, but not C's exact coordinate.
2. **Missing boundaries:** Which explicit anchors or bounded continuation rule cover the selected model's undefined domains and compact conflicts? No count0/1 assumption is implicit.
3. **Residual authority:** Which legacy12 slope changes remain applicable to a changed formation? A count-only transfer does not establish a resource-composition model or a new multi-King premium.

## Primary local references

[^rationale]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Rules.py:104-132` at `a690076d30fe977b67a16e922acde2621ea64948` (2023-11-21), plus its commit body.
[^current]: `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py:194-248,259-296` at ChecksMate HEAD. Current blame confirms the historical uppercase path and later formatting/stage changes.
[^grand]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:65-108` at `6fbf68628aa71f51b1803a49536ca63b89f8d02f`, parent of `17df6adc6d`. This contains the complete pre-expansion base8/grand10 series and clear4020/6050.
[^corrections]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:65-73,92-108` at `c1f53b70fc155bdba5b48090e45d611d91a727cc`, and `:83-90` at `625d2dff110bef7aed37403469d851d0f671aaef` (both 2024-12-22).
[^append]: `C:\GitHub\rft50-checksmate\worlds\checksmate\Locations.py:115-125,140-164,187-201` at `17df6adc6d89af7b88bf2bcd3002c4e66defd535` (2026-07-19), compared with its parent.
[^old-army]: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:394-414` at ChessV HEAD. Historical series ceilings and capture-count evidence also appear in `Locations.py:121-164,190-201` at `17df6adc6d`.
[^budgets]: `C:\GitHub\chessv\docs\adr\0009-anchor-provisional-endgame-budgets.md:13-44` (working-tree accepted interview decision).
[^counts]: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\15-define-location-role-identity.md:155-172,222-286,403-408` (working tree), including starting-set counters, ceilings, and earlier-board fixtures.
[^separation]: `C:\GitHub\chessv\docs\adr\0008-separate-calibration-from-reachability.md:6-38` (working-tree accepted interview decision).
[^precision]: `C:\GitHub\chessv\.scratch\large-board-enemy-armies\issues\16-define-material-calibration-contract.md:190-206` (working tree).
