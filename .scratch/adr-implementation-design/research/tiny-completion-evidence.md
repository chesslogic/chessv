# Tiny Fundamental completion probe

Status: Bounded reference result. No production implementation or full-domain acceptance.
Date: 2026-10-26
Consumer: Allocation coordinator, A22/P1, PM/G2, and CV01.

## Result

The proposed slot-layer DP agrees with direct exhaustive combinations on all 6,840 comparisons in the declared domain.
Agreement covers feasibility, minimum extra spend, and membership of each returned concrete witness in the independent oracle's valid results.
The run includes 28 explicit expected cases, 6,804 grid cases, and eight before/after comparisons for four purchase scenarios.
No discrepancy occurred in the corrected DP.

A deliberate mutation that omits concrete BOARD material B from the state key fails on a one-actor example.
The corrected key includes B.
This specializes the candidate's existing "additional certified predicates" requirement for the newly selected B+H metric.
It is not a discovered failure of an implemented production allocator.

The probe is conclusive only for its tiny profile.
It does not close the full source compiler, scheduler, access monotonicity, performance, native integration, or release gates.

## Artifacts and reproduction

All files are in:
`C:\Users\aaedi\.copilot\session-state\e366c8ca-91b6-44a9-a874-add72ba14de6\files`

| Artifact | Purpose |
| --- | --- |
| `tiny-completion-expectations.json` | Initial 21 explicit expectations, authored before the probe implementation |
| `tiny-board-price-expectations.json` | Two synthetic stage profiles and seven additional expectations, authored before the stage-aware extension |
| `tiny_completion_probe.py` | DP, independent exhaustive oracle, purchase comparisons, finite grid, and deliberate B-key mutation |
| `tiny-completion-results.json` | Actual observations, complete explicit-case witnesses, purchase pairs, counts, and smallest mutant counterexample |
| `tiny-completion-evidence.md` | This bounded interpretation and integration handoff |

Run with Python 3.10 or later:

```powershell
python 'C:\Users\aaedi\.copilot\session-state\e366c8ca-91b6-44a9-a874-add72ba14de6\files\tiny_completion_probe.py'
```

The command uses only the Python standard library.
It writes the named results file after all assertions pass.
There are no dependency installs, production imports, repository changes, or background processes.

| Pinned input | SHA256 |
| --- | --- |
| Initial expectations | `fd37a204f3c2f53231c5eb63e59b8020ac305f7eb241b000bcfaf1507f7c2254` |
| Board expectations/profile input | `c5722373008548292a209b9ba1c5912a247bd46134ac1b1eda49ea6aae279a6e` |
| Probe | `c65538cd638650bb551787127577449f76659ba5a75fdd7ea31e6476992e6538` |

## Exact domain

The profiles are synthetic, not accepted calibration.
`tiny-A` and `tiny-B` are not supported game geometry IDs.
Names identify concrete synthetic members. They are not nominal spending-family labels.

| Concrete ID | tiny-A MG | tiny-B MG | Role and compatible class |
| --- | ---: | ---: | --- |
| `a-checker` | 40 | 40 | Pawn, mixed or Pawn-only |
| `b-pawn` | 100 | 100 | Pawn, mixed or Pawn-only |
| `c-better` | 150 | 175 | Improved Pawn, mixed or Pawn-only |
| `d-minor` | 120 | 180 | Non-Pawn, home or mixed |
| `e-castler` | 160 | 210 | Non-Pawn and Castler, home only |

An actor's member ID stays the same across stages.
Price lookup uses `(stage, member_id)` before debit and BOARD calculations.
Role and Castler eligibility do not derive from price.
The expensive-minor case cannot satisfy a Castler requirement.
No family weight or public palette is selected by this probe.

Each owned slot has a protected funded100 base.
The synthetic owned-slot extra charge is `max(0, concrete_price - 100)`.
This formula is a declared probe assumption, not adoption of all production liabilities.
A cheaper base does not release its difference.
A funded birth pays the full stage price and owns no received Chessman token.
Births use the same concrete IDs as owned Pawn actors.

The only spendable source is one nonnegative extra-Material account.
Every witness reconciles its extra spend and remaining balance.
Protected base remains `100 * received_slot_count`, with base refund zero.
The profile has no dedicated-account release, reserve transfer, surcharge, signed residual, refund, or Historical exception.

The grid covers:

| Dimension | Values |
| --- | --- |
| Stage profile | tiny-A, tiny-B |
| Actual free squares | H0/M0/P0, H0/H1/M0/P0, or M0/P0 |
| Owned slots | 0, 1, 2 |
| Additional birth positions | 0, 1 |
| Extra funds | 0, 20, 40, 50, 60, 80, 110 |
| `(total minimum, non-Pawn minimum, Castler target)` | `(0,0,0)`, `(2,1,0)`, `(3,1,1)` |
| Actual ordered Pockets | Empty, `[Minor]`, or `[Pawn,Minor]` |
| Concrete access threshold | 0, 100, 300 |

All owned slots must remain active in this profile.
Primary occupancy is outside the supplied free-square list.
There is no reserve alternative or missing-inventory fallback.
Explicit fixtures add precise square pins, boundary funds, and thresholds outside the grid.

Supported Pocket count is at most `floor(active_nonprimary_count/2)`.
Non-Pawn Pockets precede Pawn Pockets, with input order inside each class.
Supported semantic values remain Minor300 and Pawn100 on both stages.
The probe uses `H=min(S,floor(B/2),300*C)` and new Fundamental access `B+H`.
It does not change Legacy access or funding.

## Independence and constructive witness

The DP enumerates member/class rows in owned-slot layers, followed by optional birth layers.
A no-birth row consumes neither funds nor capacity.
Its equivalence key contains class occupancy, actual actor count, non-Pawn count, Castler count, and B.
For identical keys, it retains the least-spend label, with a deterministic row-order tie.
It expands classes to actual distinct squares after satisfying obligations.
Pinned squares remain reserved for their named actors during expansion.

The oracle instead enumerates concrete member/square products directly.
It rejects duplicate or incompatible squares and calculates spending, counts, support, and access independently.
It imports neither the DP's row compiler nor its metadata, feasibility predicates, state merging, or resource reducer.
Both implementations share only declared case inputs and the stage price table.
Frozen hand-authored expectations supply another check against shared-input mistakes.

Every DP witness must appear in the oracle's complete valid set with the same actor/member/square tuples, spend, B, H, and supported Pockets.
Comparison does not demand one identical arbitrary coordinate tie across the two constructions.
Both must achieve the same minimum extra spend.
This is local completion optimization, not a whole-army packing claim.

The probe performs 2,572,023 raw oracle combinations and 250,045 DP transitions.
The largest retained DP layer has 33 labels.
These are observed work counts, not runtime benchmarks or extrapolated feasibility limits.

## Discriminating observations

| Case | Actual result |
| --- | --- |
| Exact funding | Castler extra60 fails at59 and succeeds at60/61 on tiny-A |
| No cheap-base refund | Checker40 plus required Castler cannot use the protected60 difference when extra funds are zero |
| Individually affordable improvement | BetterPawn extra50 fits funds60 alone but fails when the same setup also requires Castler extra60 |
| Placement protection | Minor extra20 on the sole home square blocks required Castler completion despite funds80 |
| Compatible placement | The same Minor on mixed plus Castler on home succeeds at exact extra80 |
| All-prefix improvement | Pawn100 to BetterPawn150 preserves actor0, P0, count1, Pawn role, protected base100, and access floor100 while spending real extra50 |
| Required Castler role | A pinned promised Minor cannot replace the only required Castler merely because sufficient money remains |
| Natural Castler | One selected Castler fulfills target1 with one extra60 debit, not another shortfall charge |
| Funded surplus birth | One received Pawn plus one derived Checker uses extra40 and two real Pawn squares without another received token |
| Supported actual Pocket | Two active actors support the Minor before the Pawn, yielding the declared composition and H110 |
| Unsupported Pocket | One active actor cannot use an occupied Minor Pocket to reach total2 |
| Concrete access equality | Two Pawn100 actors plus one supported Minor give B200/H100, passing300 and failing301 |
| Stage-dependent identity | The same Minor needs extra20 on tiny-A and extra80 on tiny-B, with unchanged non-Pawn role |
| Stage-dependent Castler | The same Castler needs extra60 on tiny-A and extra110 on tiny-B, with unchanged home-only eligibility |

Purchase comparisons fix the proposed actor's post-purchase member and square.
The completion call cannot retract that purchase to justify admission.
The probe checks final-state guards, not a complete upgrade discovery algorithm or public BetterPawn configuration.
The improved-Pawn positive has Castler target zero.
Separate positive and negative Castler fixtures establish the required role check in this declared domain.

## Smallest deliberate mutant counterexample

The defective variant removes B from the DP equivalence key.
One owned slot has alternatives Checker40 and Pawn100, both with extra debit0.
There is one Pawn-only square, no Pocket, no birth, and no extra funds.
The access threshold is41.

Without B, the states collide.
The deterministic tie retains Checker40, then rejects it for access40.
The oracle accepts Pawn100 at access100 with zero extra debit.
The corrected DP retains both B states and returns Pawn100.

This is smallest only within the explicit search:
zero or one actor, one Pawn square, those two members, zero funds, no births/Pockets, and integer thresholds1 through100.
No zero-actor discrepancy occurs. Thresholds1 through40 do not discriminate.
This is not a global minimal-counterexample claim.

Engineering consequence: B belongs in state equivalence for B+H obligations.
Coalescing by counts, Castlers, and spend alone is unsound.
A stronger dominance reduction could replace exact B only after its future-equivalence proof.
The merged candidate already requires extra state for certified predicates.
This probe makes that requirement concrete without editing the merged specification.

## Concrete-specification feasibility handoff

The user's selected direction fits the seam: select a concrete identity, then resolve its prepared-board price from the shared profile.
Families can remain selection preferences without supplying spend amounts.
The probe's stage fixtures demonstrate this separation without a budget-target sequence or palette change.

Current repository evidence remains a real implementation gap:

| Source at `0cfc31394c9d7bad0fcc0e4b905f7fcd74f0fed5` | Observation |
| --- | --- |
| `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:19,26` | One constructor assigns Bishop325/350 |
| `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:155-157` | `Create()` accepts no stage and creates that catalog |
| `ChessV.Games\MiscellaneousGames\ApmwChess.cs:496-498` | Game setup calls the same parameterless catalog creation |

Production needs one accepted stage-aware catalog/profile binding shared by allocation and actual game values.
It also needs exact concrete IDs, profile identity, complete stage data, and rejection of missing or mismatched profile entries.
No price or family can silently substitute for an absent concrete member.
Actual values, calibration, primary/royal/Castler liabilities, and final contract fields remain with their owners.
The synthetic profiles select none of them.

## Unresolved domain and stop

The result excludes Legacy, source-restricted Parts, reserves, refund exceptions, consumed-right recovery, and real Castler surcharges.
It excludes seeded family/member ordering, elimination epochs, draw counters, type limits, and upgrade enumeration.
It excludes real five-board formation rules, arbitrary square interactions, more than two received actors, and more than one derived birth.
It excludes multiple-step upgrades, production profile validation, and stage-change access monotonicity.
No PM extension certificate, generator lifecycle, Archipelago accessibility, CV01 integration, or native result follows.

The conditional class-equivalence and least-spend argument remains necessary outside the tested domain.
The tiny positive result does not authorize an unbounded production search or a silent refusal for unsupported ordinary inputs.
No additional broad exploration is necessary for this bounded question.
The coordinator can consume the profile separation, explicit B key, frozen cases, and exact limits as the next design evidence.
