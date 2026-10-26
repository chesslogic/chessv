# Guarantee-first seeded allocation for 0.4.0

Status: accepted behavior and preservation rule; exact engineering and funding questions remain open
Original decision: 2026-09-21
Successor decision: 2026-10-21
Fundamental refinement: 2026-10-23 (local date)
Authority reconciliation: 2026-10-26
Guarantee-preservation decision: 2026-10-25
Prefix-upgrade clarification: 2026-10-26
Fundamental access valuation: 2026-10-26

## Current decision

The October 21 successor selects final pieces for actual owned slots in both runtime allocation and generator access estimates.
It replaces conflicting selection procedures, not the retained mode-specific funding laws.
The purpose is a stable guaranteed prefix with lawful optional additions, not an optimal packing or a permanent optional-piece entitlement.
The later Fundamental amendments that follow qualify its slot ceiling and optional-selection rules.
Neither revision alone is the complete executable contract.

| Matter | Current rule |
| --- | --- |
| Funding | Exact funding qualifies (`>=`). Eligible sources, lawful releases, protected bases, reserves, and dedicated accounts remain distinct. |
| Priorities | World/player-seeded priorities do not depend on delivery chronology. Material alone does not reorder them under unchanged legal inputs. |
| Slot extension | Added ordinary slots extend earlier assignments when legal choices remain unchanged. This is not chronological receipt appending. |
| Preferred prefix | Fix legal preferred assignments before affordability. Select the longest affordable prefix that permits all retained mandatory completion. |
| Prefix upgrades | A permitted upgrade can change a promised actor's concrete type when genuine surplus pays and every guarantee survives, including Castlers needed for their Location. |
| Active placement | Ordinary promised pieces must occupy compatible active-board positions. Reserve pieces cannot satisfy that promise. |
| Castlers | Select normally while protecting eligible funding and compatible capacity for the feasible enabled target. Natural selections count. Complete only the shortfall. |
| Optional pass | The October 21 baseline uses a finite forward pass. Later Fundamental elimination, upgrades, and surplus Pawns remain permitted under the preservation rule that follows. |
| Completion | Every accepted candidate preserves mandatory completion. Applicable Pawn baselines, composition minimums, reserves, and dedicated actors remain. |
| Later setups | Lawful reallocation can change optional pieces. New eligibility or geometry can change final pieces without a reroll. Certified access remains protected. |
| Match boundary | No active-match mutation. Actual later inventory and plando bonuses retain their separate obligations. |

### Later Fundamental amendments

The board-material interview supplies later direct author decisions, not only a proposed engineering plan.
Its scope prioritizes Fundamental and does not redesign Legacy.

| Matter | Later author decision | Boundary still to specify |
| --- | --- | --- |
| Board prices | Keep earned credits fixed and vary concrete costs by prepared board. Keep the 8x8 values unchanged | Typed funding, protected bases, royal/Castler liabilities, and accepted profile values |
| Optional selection | After the first unaffordable concrete draw, filter affordable families and types and advance the seed. Every purchase preserves the guarantees | Exact finite enumeration, counters, and constructive completion guard |
| Surplus | Chessmen are a minimum, not an army-size maximum. After satisfying the prefix, use remaining material for upgrades or additional Pawns in the player's preference order | Source eligibility, derived-unit identity, capacity, and exact upgrade procedure |
| Later boards | A prefix on one board need not survive on another board. Allocate under that board's prices | Conservative access across geometry changes |
| Refunds | Spending is mostly one-way. The user did not settle refund exceptions | No blanket refund prohibition or new refund right |

These amendments replace the unconditional Fundamental army-size ceiling.
They do not mint received Chessmen or free funds.
They do not select ADR0024's old child-tail scheduler, pooled Castler grant, or refundable base model.
On October 25, the user resolved the preservation question:

> Yes—later allocation behavior must preserve those guarantees (Recommended)

Later elimination, upgrades, and surplus purchases must preserve the promised prefix and feasible mandatory completion, including Castler funding and compatible placement.
The allocator rejects a purchase that makes mandatory completion impossible, even when its standalone price is affordable.
This retains the later behavior without restoring an overall Chessmen ceiling.
The earlier no-replacement wording cannot prohibit those accepted upgrades outright.
It also cannot authorize an upgrade that breaks the promised prefix.

On October 26, the user clarified the permitted upgrade of a promised prefix actor:

> The only constraint not mentioned in the recommended option is castlers, which are needed for a location. Otherwise yes

The question concerned a permitted BetterPawn upgrade paid from genuine surplus.
Prefix preservation protects required funding, counts, roles, placement, and certified access, not an exact concrete-type freeze.
A permitted prefix-piece upgrade must also preserve the Castlers needed for their Location.
Higher material value or a higher family alone does not prove that capability survives.
This clarification grants no new refund, price rule, or family-dominance rule.
It does not permit active-match mutation.

This closes the behavior-precedence question, not the exact algorithm or its proof.
Refund exceptions, exact prices and liabilities, counter events, and constructive completion remain unresolved.
The [board-value plan](../../.scratch/board-aware-material-values/spec.md#9-open-decisions)
records D3/D5/D6 and the other bounded refinements.

The exact series, typed funding records, completion algorithm, and useful conservative metrics remain engineering obligations.
Prefix monotonicity alone does not prove access monotonicity.
One favorable optional roster does not establish a guarantee.

### Fundamental Location-access material

On October 26, the user selected:

> Use concrete board-specific player values for Fundamental Location access (Recommended)

For new Fundamental worlds, the material contribution is `B + H`, not nominal family material `E + H`.
`B` is actual active non-primary BOARD material at the prepared board's concrete values.
Pocket credit remains `H = min(S, floor(B/2), 300*C)`.
`S` remains supported semantic Pocket material, and `C` remains the supported Pocket count.
Pocket support and credit rules, protected funding, and Legacy remain unchanged.

The conservative Location material bound `L_M` must be sound against `B + H` throughout its covered domain.
This decision does not prove that actual `B + H` is monotone across receipts, upgrades, or geometry changes.
It selects neither numeric profile values nor a completed PM proof.

The [current engineering frontier](../../.scratch/adr-implementation-design/engineering-closure.md#current-authority-and-frontier)
owns the evidence, open decisions, acceptance gates, and implementation owners.

## Funding and clause-level supersession

[ADR0022](0022-retain-legacy-pawn-material-conversion.md) retains the Legacy and Fundamental funding boundaries.
Grant-backed Fundamental units require actual owned Chessmen tokens.
Later surplus Pawns remain derived units, not additional received tokens.
A Castler application is not an additional Chessman token or a free 500-point grant.
Natural Castler selection changes construction order, not source ownership.

Ordinary concrete Pawns valued at 40, 85, or 100 retain their funded base of 100.
A cheaper concrete Pawn does not release 60 or 15 spendable Material.
Paid quality can release only its lawfully funded extra.
The [retained funding contract](../../.scratch/large-board-enemy-armies/contracts/shared-data-contract-v4.md#66-fundamental-paid-funding-and-source-preserving-ledger)
distinguishes selected economics from candidate ledger equations.
Exact Castler applications, payers, releases, and compatible slots still need the successor's typed mapping.

| Earlier clause | Disposition for the successor |
| --- | --- |
| ADR0023 cumulative W | Remains superseded. Its vectors do not define the successor. |
| September section 1: Fundamental-only worlds and Legacy retirement | Superseded by the retained two-mode scope. Old-world contract rejection remains separate and retained. |
| September sections 2/4: `N=S+K`, pooled free Castler credit, and refundable cheap bases | Not the retained funding model. No new grant, token, or refund follows from those equations. |
| September sections 3-6: child tails, replacement priorities, and accepted-round schedules | Replaced where they conflict with final-slot prefix selection and the finite optional pass. No obligation to reproduce that scheduler. |
| September section 6: natural Castlers count and only the deficit needs completion | Retained behavior. Its old reserve formula and closure order are not automatically the successor algorithm. |
| September sections 7-8: concrete-price-only accounting and additive new Pawn slots | No automatic reuse of their equations or mechanism. Later Fundamental surplus permits additional Pawns without new receipts. Funding and placement still need reconciliation. |
| September section 9: full suffix-envelope implementation default | Historical mechanism, not a mandatory successor prerequisite. Useful conservative access and full-world acceptance remain required. |
| September sections 10-11: fixtures, namespaces, wire IDs, caps, and defaults | Historical specification of that algorithm. Reuse requires explicit successor mapping and independent acceptance, not an inherited `E` label. |
| Separate CPU roles/promotion, truthful accounting, strict bindings, and legal placement | Retained safeguards. This reconciliation grants no new behavior in those domains. |

The successor does not ratify every September palette, preset, cap, or grant change.
Those exact policy and schema mappings remain with the engineering owners.
The later board-value decisions require an exact scheduler and typed funding under the accepted preservation rule.
Neither a newer filename nor a proposed helper supplies that engineering result.

## Consequences and acceptance

Both languages and every projection consumer need one accepted interpretation of the selected behavior.
The [local allocation plan](../../.scratch/adr-implementation-design/plan/design-seeded-army-allocation-1.md)
keeps Archipelago fill upstream and proposes a bounded local construction.
Its helper names and algorithm choices remain proposals until their existing acceptance gates close.
General recovery graphs, Historical comparators, and multiworld trial drivers are not prerequisites for this successor.
The October 23 architecture plan predates the later board-material answers and cannot cancel them.

Accepted CV02/TX/TR/CS design/reference results remain accepted.
Native conformance, complete allocation/access integration, S0 authorization, and release acceptance remain separate.
Current Git instructions prohibit PR creation and require branch commits for coordinator integration.
Historical PR-permission text is not a current delivery instruction.

## Historical September contract

The remaining sections preserve the September 21 decision and its fixtures for provenance.
Their `A` and `E` labels describe that revision only.
The current decision and clause map control applicability.

The allocator uses seeded child prefixes, concrete material prices, and explicit transition rules.
A finite search supplies an affordable permitted action or a complete local stopping certificate.
This replaces cumulative allocator W without promising permanent piece identities across snapshots.
This decision records the implementation contract, not a claim of implemented or released behavior.

**Authority:** Sections marked **A** retain author decisions. Sections marked **E** specify accepted initial engineering defaults.
An E default is required implementation behavior, not a quotation from the user.
The coordinator approved these defaults and the policy mapping on 2026-09-21.

## 1. Lifecycle and retained release scope [A]

This historical decision superseded [ADR0023](0023-enabled-material-families.md).
Its historical body and fixtures remain unchanged.
This successor restates retained requirements and replaces the incompatible clauses explicitly.

The replacements are W, normalized allocation prices, protected-first funding, the fixed Major-to-Jack debit, and the exclusion of Sergeant support.
They also replace post-allocation type selection and the old 100-per-slot logic floor.
Old W measurements and its PRF vectors remain historical evidence, not validation of this algorithm.
The cumulative interpretation `[15,7,3,1] -> [8,4,2,1]` remains mathematically correct, but does not define this allocator's state.

New worlds use Fundamental itemization and the newly declared contract only.
Older worlds use matching older clients, as [ADR0005](0005-break-contract-compatibility-at-zero-four.md) requires.
Retire live Legacy itemization selection, family tickets/cascades, action dictionaries/ratios, and old-contract runtime dispatch.
Replace their live consumers before removing imports. Preserve historical source and vectors where useful.
Retain common-item quotas, capacity/rejection guards, army/fairy filters, reconstruction, pockets, royals, and unrelated standalone chess modes.

No new stable/chaos allocator selector, public Sergeant percentage, numeric distribution override, or overall quantity limit enters this release.
Modal concrete-type variety and new overall quantity limits remain a follow-up ADR.
Their public modes, metrics, count scope, and defaults remain outside this contract.
Existing presentation controls remain separate. They cannot change allocation prices or turn an already selected concrete type into another type.
Coordinate-only presentation behavior remains subject to the existing geometry and legal-placement constraints.

## 2. Configuration, supply, and grants [A]

Enabled labels name families: Pawn1, Minor3, Major5, Jack7, Queen9, Amazon13.
Every valid palette contains 1, 3, and at least one of 5/7. Labels 9/13 are optional.
There are 12 valid palettes. Default: `[1,3,5,9]`, preset `classic`.
This rule guarantees availability, not a minimum generated count.
It applies with or without castling objectives.

Producer input accepts a numeric collection. Connected JSON requires a numeric array.
Normalize integral numeric values by deduplication and ascending order.
Reject booleans, strings, nulls, fractional/nonfinite values, unknown labels, duplicate JSON properties, and invalid palettes explicitly.
Diagnostics must identify the offending field, its value, and the required condition.
Do not fall back to another palette, preset, policy, or an empty roster.
For example, `[9,1,5,5,3.0]` normalizes to `[1,3,5,9]`.

`[1]`, `[1,7]`, `[1,9]`, `[1,5,9]`, and an empty set are invalid.
No automatic Material mask or Pawn-only world branch exists.

| Preset ID | Pawn: AP Chessmen | Minor | Major | Jack | Queen | Amazon |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `classic` | 8 | 8 | 4 | 1 | 3 | 1 |
| `royals` | 6 | 1 | 1 | 12 | 12 | 12 |
| `welterweight` | 4 | 24 | 6 | 1 | 1 | 1 |
| `horde` | 36 | 12 | 144 | 1 | 1 | 1 |

Pawn is the total AP Chessmen proposal weight, not additional tickets and not a runtime action.
Material retains weight3 and denomination400. Eligible Castler retains weight1. Pocket retains weight3 and its existing common rules.
Successful AP proposals sample with replacement. Rejection can prune an occurrence.
Neither final item shares nor common-item counts are guaranteed unchanged.
Material eligibility retains productive temporarily blocked grants, starting/locked grants, and validated tracker totals.
No new suppression rule removes Castlers merely because objectives are disabled.

The five non-Pawn columns are destination-family preferences in Section 5, not the old W transition frequencies.
Absent families are masked without weight redistribution. No empirical equivalence to W is claimed.
At fixed grants, configuration, and seeds, Pawn supply weight cannot change allocation or filler.
The presets remain soft preferences. No preset guarantees an exclusive count or leaves an otherwise permitted affordable action unused.

`S` and `K` count effective ordinary Chessmen and Castler grants.
Lowercase `m` counts whole Material items. Uppercase `M` measures additional material points.
Receipt states use `M=400*m`. Synthetic affordability checkpoints can specify `M` directly without defining a new receipt denomination.

```text
M = 400*m                       additional material points from whole receipts
N = S + K                       grant-backed nonroyal units
G = 100*S + 500*K + M           pooled core material
S <= 107, K <= 2, m <= 321      separate effective caps
```

A Castler supplies one new unit and500 total, including its base. It does not reserve an existing Chessman.
Keep reversible raw/excess handling and the precedence of validated tracker totals.
There is no combined107-unit cap. `S=107,K=2` owns109 core units before geometry selects active pieces.
Royals, Consuls, King Promotion, Pocket, and Play as White keep separate accounting.

Use the resolved preset in generation and supported reconstruction.
Preserve the full quota catalogue independently of proposal tickets.
Update composite Castler unit/material effects in pool budgets, support minima, removal guards, and obtainable totals.
Do not synthesize duplicate parent Chessmen or Material grants.
Preserve occupied-Pocket credit in the unit guard and validated-tracker precedence.
Use Material400 consistently in pool, starting/locked grants, projection, ledgers, slot export, and tracker accounting.

## 3. Internal hierarchy and concrete identity [E, constrained by A]

Each family has semantic groups and price-homogeneous child sequences.
The initial grouping uses the current catalogue and its declared integer midgame values:

| Family/group | Child prices | Current membership |
| --- | --- | --- |
| Pawn/base-checker | 40 | Checkers |
| Pawn/base-foot | 85,100 | BerolinaPawn, Pawn |
| Pawn/strengthened | 150,200 | OdinPawn, Sergeant |
| Minor | 250,275,300,315,325,365,400 | Catalogue Minors, grouped by declared price |
| Major | 450,475,495,500,550 | Catalogue Majors, grouped by declared price |
| Jack | 700 | Catalogue Jacks |
| Queen | 875,950,960,1025 | Catalogue Queens |
| Amazon | 1300 | Catalogue Amazons |

One child contains all remaining concrete types at its price within that semantic group.
For example, Minor325 contains Bishop, Knight, Tower, and NarrowKnight before army filtering.
The hierarchy is internal. It creates neither public value-2 configuration nor an independently spendable inventory for each price.
Price grouping makes the current head representative of that child's affordability.
Semantic rules still depend on group/family IDs, never on a positive price inequality alone.

Apply the retained army filter to each non-Pawn family before grouping.
An empty army intersection retains the original family, as the current filter does.
Base Pawn eligibility uses the existing `FairyPawns` mapping in `SetupPawnOptions`.
Strengthened eligibility retains the existing helper's distinction: Vanilla selects Sergeant, other Pawn configurations permit OdinPawn and Sergeant.
A missing catalogue or a genuinely empty required family is an explicit error.
Prices come from canonical exported metadata, not independent C#/Python tables or engine revaluation.

Each child has an immutable sequence `type(child,index)` for fixed seeds and resolved configuration.
An owned count `c` means exactly indices `[0,c)`. The next target is index `c`, and the source is index `c-1`.
No operation skips an owned index or edits an interior entry.
The child selector uses equal concrete-type weights and the seeded choice rule in Section 5.
Unlimited child choice samples with replacement. It has no cycle quota.
The parent child weight equals the number of eligible concrete types in that child.
Thus the unconditioned child/type hierarchy starts with equal per-type preference, not equal per-price preference.
These are initial internal weights, not a new family preset.

Retain `minor_piece_limit_by_type`1..15, `major_piece_limit_by_type`1..11, and `queen_piece_limit_by_type`1..9.
Each retains default0/no-limit. Major/Jack share the configured threshold but not choice counters. Queen/Amazon do likewise.
Positive values remain soft variety preferences. They cannot delete a unit or make the family unavailable.
No limited bag or hard cap enters this contract.

**Exact bookkeeping adaptation:** To generate child entry `i`, count earlier entries `[0,i)` of that child by concrete type.
Prefer types below the configured family threshold. After exhaustion, reuse the complete child list.
Types belong to only one child, so this preserves their per-type counter scope.
Before choosing among currently feasible children, prefer heads whose type occurs below the threshold in the current family prefixes.
After exhaustion of that feasible preferred set, reuse all feasible children.
Protection and ordinary allocation use the same prefixes and preference counts.
Consumed tails do not alter earlier child identities. Revisiting an index restores its same type.
This defines the new prefix-aware chooser precisely. It does not claim identical old chooser draw order or historical counters.

## 4. Core state, source order, and hard rules [A/E]

**E:** Initialize exactly `N` units through a separate seeded base-parent iterator over the permitted base children.
Its child weights are their concrete-type counts. Append each selected child's next entry.
All current base prices are at most100.
Set `B = G - sum(initial concrete values)`.
For example, a Checker40 base leaves60 of its100 grant unallocated.
This is initial allocation, not a downgrade or a refund from an existing piece.

State consists of child counts, budget `B`, the global accepted-core-replacement ordinal, protection marks, and transient append-order stamps.
Initial stamps follow base creation order. Each accepted append receives the next stamp.
Stamps and marks are rebuilt during replay. They are not saved receipt history or persistent piece identities.

For each target, consider only current source tails.
Within the Pawn parent, consider the most recently appended permitted tail first.
For late non-Pawn sources, consider the highest lower enabled family first, then the most recently appended tail.
Stable child ID breaks ties.
Choose the first source that passes all action guards.
A blocked tail does not expose an earlier entry in that child.
A sole ordinary entry remains consumable. There is no first-piece immunity or minimum-one family floor.

The following table defines the initial schedule. Every action uses a positive concrete price gap.

| Priority | Source | Destination | Initial rule |
| ---: | --- | --- | --- |
| 1 | Any Pawn child, including strengthened | Any enabled non-Pawn family | Primary destination request, with direct catch-up |
| 2 | Unprotected Minor/Major/Jack/Queen tail | Any higher enabled non-Pawn family | Late replacement, after no permitted priority1 action |
| 3 | Base Checker/foot tail | Strengthened Pawn child | Lower-grade improvement, after higher opportunities |
| 4 | Checker tail | Permitted base-foot child | Low-priority cleanup |
| 5 | Protected Major tail | Jack | Optional protected quality improvement, only with 5 and7 enabled |
| Terminal | No source | Base Pawn child | Additive filler under Section 8, after core termination |

Unlisted transitions are forbidden. Affordability does not permit a within-Minor change.
Other same-family substitutions, including Odin-to-Sergeant and Berolina-to-Pawn, have no initial permitted edge.
Priority5 is the only permitted replacement of a protected entry and preserves its protection.
Castler closure uses its separate constrained rule in Section 6.
No source operation can remove an interior protected entry by bypassing its child tail.

**A:** A lower-value target is forbidden regardless of family, priority, or closure.
The rejection leaves source count, target count, roster, and budget unchanged.
Discarding the potential credit does not make the replacement legal.
An accepted replacement atomically subtracts `target.midgame - source.midgame`.
It never exposes the full source value as spendable credit.
**E:** Equal-value replacements remain rejected, matching the bounded prototype. This is not a new author decision about all equal-cost mechanics.

The final generated core and filler obey the family allowlist.
Pockets, CPU armies, CPU capture roles, and royal/common grants remain separate.
Promotion candidates receive no additional enabled-family filter.

## 5. Seeded preference and finite eligible search [E]

The root is the existing invariant decimal seeds in order: `pocket|pawn|minor|major|queen`.
Connected seeds retain their nonnegative31-bit domain.
No budget, receipt history, wall clock, current counts, or presentation RNG state enters the root.

Use the existing SHA-256 counter primitive:
hash UTF-8 `root + LF + namespace + LF + decimal(counter)`.
Read the first8 bytes as unsigned big-endian64.
Set `u=(value>>11)/2^53`, using the existing binary64 behavior.
Weighted choice selects the first ordinal-ID-ordered entry whose cumulative weight is strictly greater than `u*totalWeight`.

`round` is the one global accepted-core-replacement ordinal, starting0 after base initialization.
Ordinary and closure namespaces use this same ordinal, not separate counters.
Each accepted ordinary replacement or closure replacement increments it exactly once.
Base creation, protection marks, scans, rejected proposals, and hypothetical feasibility probes do not increment it.
Filler uses its separate accepted-purchase ordinal `purchase`, starting0 for each geometry projection.
Only a completed filler purchase increments `purchase`.

Namespaces start with `fundamental.hierarchy.v1`.
Ordinary suffixes are `.base`, `.child.<childId>`, `.family.<priority>.<round>`, and `.branch.<priority>.<round>.<familyId>`.
Closure suffixes are `.closure.<round>.family` and `.closure.<round>.branch.<familyId>`.
Surplus uses `.surplus.<purchase>.branch`, `.surplus.child.<childId>`, and `.surplus.coordinate.<bandId>`.
Child IDs are ASCII `<family>.<group>.mg<price>`, with lowercase family/group names and invariant decimal prices.
Use group `pieces` for non-Pawns and `checker`, `foot`, or `strengthened` for Pawns.
Base/child generation uses its sequence index as the counter.
Permutation generation starts at counter0 and increments once per emitted entry, including a sole entry.

Each surplus child has its own prefix count, initially0 for each geometry projection.
That count is the index of its next type under `.surplus.child.<childId>`.
An accepted purchase increments only the selected surplus child's count.
Surplus counts do not change the core child sequences or counts.
Section 8 specifies the coordinate namespace and its counter use.

For each accepted-action round, generate a weighted permutation of the enabled non-Pawn families.
Select by weight, remove the selected family from that temporary ordering list, and continue until the list is empty.
For each family, generate its child permutation the same way with the internal child weights.
This finite list is a **preference row**, not an inventory bag or a prescribed cycle.
Every new round starts a new seeded row. Usually only the first feasible branch is used.
With unchanged complete eligibility, the first choice targets the specified average proportions.
No exact cycle quota or final-board proportion follows.

Enumerate the current finite frontier: each permitted source child tail paired with each permitted destination child's next entry.
Evaluate semantic permission, positive gap, budget, protected-tail restrictions, and Section 6 feasibility.
Apply source order and soft variety preference before using the seeded destination ordering.
Process the highest priority with a feasible action. Choose its first feasible family/child in the row.
Lower-grade priorities use their single Pawn parent and the same weighted child ordering.
Priority5 uses Jack only.

Rejected proposals change no counts, budget, child index, or accepted-action round.
The row scan index advances while searching. Acceptance increments the round once and rebuilds the entire frontier.
A sole feasible action still consumes an accepted round.
Future snapshots restart base generation and round0.
Deferred child heads remain unchanged and receive consideration in each later row.

**This deliberately replaces a flattened infinite proposal stream.**
A row offers every current branch once, then the next accepted action starts a new row.
It is not claimed equivalent to skipping an arbitrary number of failed independent draws.
The scripted `[325,250,325]` prototype remains illustrative evidence, not this production iterator.
Caching cannot omit a currently legal head. Recompute gaps after source, count, budget, or protection changes.

At termination, every declared frontier action fails a named guard and every funded protection obligation is satisfied.
There is no unsuccessful-sample cutoff and no search through unseen entries.
All types within a child have the same price. Later child entries are not selectable before its current head.
With fixed core units and these acyclic semantic edges, each unit can change grade only finitely often.
The initial grades are Checker, foot, strengthened, Minor, Major, Jack, Queen, and Amazon.
They permit at most `7*N` accepted core replacements, including closure and protected improvement.
The finite frontier bounds work per round, and increasing grade bounds successful rounds.
This proves local termination, not global material optimality, minimum churn, or AP reachability.

## 6. Castler value reservation and closure [A/E]

**A:** Eligible protection targets are enabled Major/Jack pieces, never Queen/Amazon.
Existing eligible pieces count toward the requirement. Closure creates only the deficit.
Protection does not depend on which grant originally supplied a unit.
The Ignore Castler protection control removes protection but preserves every unit and500-total grant.
AP uses protection enabled. The local Boolean remains `ignore_castler_protection` in input and cache identity.
Ordinary legal castling remains possible without a protection mark.

**E:** Let `eligible(s)` count current Major/Jack entries.
At initialization, choose the largest feasible protected count `P<=K`.
Feasibility includes budget, units, permitted tails, and concrete targets. It is not just `floor(material/500)`.
If `P<K`, retain the unachievable claims only as unfunded counts. All their units remain ordinary.
There are no waiting-piece identities, free protected pieces, or missing-target fallback.

Define `reserve(s,P)` as the minimum additional material needed to reach at least `P` eligible entries.
Existing eligible entries cost zero toward this target.
For a deficit, enumerate direct permitted conversions of noneligible lower tails into current Major/Jack child heads.
Each closing action must increase the eligible count by one.
Recursively enumerate at most two deficit-closing actions because `K<=2`.
After each hypothetical action, update counts, heads, and tails exactly.
Reject decreasing, equal-cost, unaffordable, or protected-source moves.
The minimum total gap is the reserve. No feasible completion means infinity.
This oracle ignores ordinary priorities and proposal preference, but never ignores hard permissions or prefix rules.
Intermediate upgrades cannot reduce a direct gap, and no child hides a different-priced later target.

As eligible entries appear, mark the earliest append stamps until `P` entries are protected.
Marks stay on those entries for the current replay.
The current marked entry can improve only through priority5. That move transfers its mark.
Ordinary candidates must preserve a finite feasible completion after their hypothetical change.
Their gap must also fit `B-reserve(s,P)` before the change.
Thus ordinary proposals do not spend the current earmark.
This restriction also applies to an ordinary proposal that satisfies a protection claim.
This is an explicit conservative scheduling default, not a separate material grant.

When no discretionary frontier action passes these guards and a funded deficit remains, run closure.
Order eligible target families by a seeded permutation using their retained family weights.
Order their children by internal weights under the same closure round.
Within each target, use the normal tail ordering across permitted lower sources.
Choose the first action whose residual state can complete every remaining funded claim.
If an earlier preferred target destroys joint feasibility, skip it.
Debit its positive gap, mark its target, increment the accepted round, and recompute the reserve.
Resume the ordinary priority schedule after closure. Closure is not a final stop.

The reserve is an earmark within `B`, not an additional balance.
A decrease in the required reserve releases a constraint, not a source-piece refund.
At core termination, the funded deficit and its earmark are zero.
Unfunded claims confer neither protected placement nor AP castling access.
Actual castling still requires active eligible pieces, legal home-rank placement, and retained game rules.

## 7. Final core, presentation, and promotion [A/E]

Materialize only final child-prefix entries. Candidate probes use IDs, prices, indices, and arithmetic, not game objects.
Concrete values replace normalized family values in active/reserve material, affordability, and projection.
Keep the separate grant ledger. Do not add a second concrete-value adjustment for already concrete core values.
At core completion, `sum(core concrete values)+B=G`.
Reserve pieces retain their allocated value. Their value never becomes filler cash.

Use one `ordinary-nonpawn` placement category, independent of source family or transition path.
Keep royals and protected Castlers special. Keep existing rank/capacity/coordinate algorithms.
Final concrete price replaces the old normalized expected-material sort key.
Retain descending value, descending allocated credit, source ordinal, then ordinal stable ID ordering.
**E:** Enumerate final ordinary non-Pawns by descending family rank, child ID, then prefix index.
Assign six-digit `ordinary-nonpawn:` ordinals across that enumeration.
Enumerate protected entries by append stamp, with six-digit `castler-protected:` ordinals.
Enumerate Pawn entries by child ID/index, with six-digit `pawn-slot:` ordinals.
These IDs describe the current result, not persistent upgrade lineage.
The sidecar adapter must use each selected concrete ID rather than draw another type from its family.

Human promotion rights derive only from final active concrete types, with retained game-legal exclusions.
Reserve types form the separate reserve catalogue.
No intermediate type, previous-snapshot union, or source-family history adds a right.
CPU promotion remains governed by [ADR0017](0017-separate-cpu-promotion-permissions.md).
CPU placement and capture roles do not change.

## 8. Terminal additive Pawn fill [A scope, E mechanism]

Include this bounded addition with the planned projection/accounting rewrite.
If implementation demonstrates substantial extra scope, the author permits explicit release deferral.
Deferral requires a recorded integration reason and removal of filler from that release's declared exact-output policy.
It is not permission to silently omit filler now or to ship client-only hidden pieces.

After final core activation, placement, and Forwardness, consider only legal vacant Pawn cells.
Filler cannot move, replace, or deactivate a core piece, including a reserve.
It cannot alter protected castling eligibility or consume another Forwardness grant.
Process permitted Pawn ranks in the retained order.
For the selected Pawn type, use the first rank with legal vacant cells and sort its vacant files numerically ascending.
Use the existing `FormationBandId(relativeRank)` as `bandId`.

The exact coordinate namespace is `fundamental.hierarchy.v1.surplus.coordinate.<bandId>`, with the Section 5 root and counter `purchase`.
Select vacant-file index `floor(u*vacantFileCount)` with the Section 5 PRF value `u`.
A rejected candidate or cell scan does not increment `purchase`.
The completed purchase debits its price, occupies that cell, increments its surplus child count, and increments `purchase` once.
Each geometry projection starts with `purchase=0` and empty surplus prefixes.
Core placement and child sequences remain unchanged.

Buy a base Pawn at its full declared concrete price using only the ordinary spendable remainder `B`.
Never use common outside-roster credit, dormant credit, reserve-piece value, or a Castler earmark.
The aggregate `UnallocatedMaterial` field is not a spendable balance.
Keep the separate ordinary remainder explicit in C#, Python, transport, and ledgers.
There is no old45 subsidy, new receipt, or increase to `S`.

Use a separate surplus child sequence over the same permitted base Pawn catalogue.
Use equal per-type preference and the finite row rule to find an affordable head.
An empty legal cell and an affordable head permit one purchase. Repeat until no such pair exists.
Positive prices and finite legal cells bound this pass.
No filler becomes a core upgrade source, receives protection, or creates an allocator loop.
Filler has normal movement and in-game promotion under the retained legal rules.
Horde has no special priority override.

Report fillers explicitly as derived slots with IDs `surplus-pawn:` plus six-digit ordinals.
They appear in owned/active slots, coordinates, Pawn counts, region usage, concrete material, and final-active-type rights.
They never appear as received grants or reserve candidates.
The sidecar adapter must resolve every active filler ID to its corresponding owned slot.
For each geometry, `sum(core values)+sum(filler values)+ordinary remainder=G`.
Common credits and royal accounting remain separate and unchanged.
Fresh replay can remove extras because core allocation runs first. No prior filler claim survives the replay.

## 9. AP conservative core versus exact final output [A/E]

Keep both conservative lower bounds and target maxima core-only.
Filler adds no pool quota, minimum count, item supply, or access-rule kind.
The generator cannot require filler to satisfy an objective.
The exact final projector still includes all actual fillers. A core-only metric must not masquerade as exact final output.
Split these paths explicitly within the existing backend/package architecture.

Keep the capped future-count envelope on Chessmen, Material items, Castler, and Consul.
For fixed policy, seeds, stage, and complete obtainable bounds, take componentwise suffix minima of exact **core** metrics.
Obtainable bounds include pool, starting, locked, and supported item-link grants.
Reject effective counts beyond frozen bounds. Do not silently widen bounds or change semantic modes after receipts.
Keep King Promotion and occupied-Pocket contributions separate without duplicate credit.
Protected support uses actual funded/protected core counts and the retained safe home-rank bound.

**E:** Remove the old large-space `100*slotCount` fallback.
Checker40 makes that constant unsound.
Use the same exact capped suffix envelope for large rectangles, with tiled evaluation and memoization.
The former10,000-cell threshold is the initial in-memory tile size, not a semantic switch to a weaker floor.
This is a concrete correctness-first engineering default, not a claim of measured production performance.
An implementation can add a proven price-aware pruning bound without changing the returned suffix minima.
It cannot substitute `40*count` and declare supported worlds infeasible because that shortcut is too weak.

At every supported state, the core metric must not exceed exact core output.
Additive fill must preserve core active values/counts, placement legality, Castler eligibility, and independent bonuses.
These properties keep conservative metrics no greater than exact final output.
Later disappearance of extras does not invalidate this relationship.
They do not promise identical tactical ease.

All four presets must pass normal generation, fill, accessibility, collection-sphere, and goal-completion gates.
Use supported standard/all-six configurations and paired seeds `1,7,42,4242`, plus multiplayer/item-link regressions.
Cover small and large rectangles, adjacent-count monotonicity, `conservative<=core<=final`, legal deployment, and bounded runtime.
Performance or a weak conservative result is an engineering failure to resolve, not authority to retune weights or narrow supported normal worlds.
Actual infeasibility needs a diagnostic with policy, stage, counts, exact capacity, obligations, and bounds.

## 10. Concrete acceptance fixtures [A cases, E completed defaults]

These are isolated checkpoints unless grant counts are stated.
Core cases exclude royals, pockets, common credit, filler, and placement effects.
Filler cases specify their own geometry and credit exceptions.
Unmentioned source counts are zero. Explicit target lists pin candidate identity/order, not public configuration.
`M` means additional material points. Lowercase `m` means whole400-point Material receipts.
A listed action result does not assert termination before the remaining finite frontier is examined.

| ID | Inputs and operation | Required result |
| --- | --- | --- |
| H1 | K0, Pawn100, B190. Minor325 head Bishop and Minor250 head Gardener. Preference row tries325 then250. | Reject gap225, accept gap150. Pawn0, Minor325 count0, Minor250 count1, B40. Child sequences unchanged. |
| H2 | K0, Gardener250, B75. Request Bishop325. | Reject affordable gap75 as within-Minor forbidden. Counts and B75 unchanged. |
| H3 | K0, Bishop325 count1, Gardener child count0, B0. Request Gardener250. | Reject negative75 before semantic priority. Source1, target0, B0. No refund and no downgrade with discarded credit. |
| H4 | K0, Checker40, B60. Only selected higher target Gardener250 is unaffordable. Foot target Pawn100 is permitted. | Priority4 spends60 and leaves Pawn100/B0. With B250, priority1 Gardener gap210 precedes cleanup and leaves B40. |
| H5 | Same original base and child sequences as H1, now B225. Row tries325 then250. | Bishop325 succeeds for225, B0. This fresh replay differs from H1 without executing Gardener-to-Bishop. |
| H6 | K0, no Pawn source. Minor325 prefix Bishop/Knight, Minor250 prefix Gardener. Append stamps B0/G1/N2. Rook500 head, B175. | Consume Knight at the325 tail. Retain Bishop/Gardener, append Rook, B0. No interior substitution. |
| H7 | K0, sole Rook500, Queen950 head, B450. | Late replacement consumes the sole Rook and leaves Queen950/B0. No first-entry floor. |
| H8 | Pawn100, Queen950 head. Compare B800 with B850 for this request. | Gap850 fails at800 and succeeds at850. Never charge800 or use historical Queen900 output. |
| C1 | `[1,3,5]`, S0/K1/M0. Pawn-only bases, Rook500 only eligible protection target. | N1/G500, P1. Reserve400, close to Rook500, B0. Not normalized Major485/B15. |
| C2 | `[1,3,7]`, S0/K1/M0. Pawn-only base, Bishop325 only Minor, Jack700 target. | P0. Primary Bishop costs225. Final Bishop325/B175, no protected claim or castling entitlement. |
| C3 | Same palette/catalogue as C2, S0/K1/M200 synthetic threshold checkpoint. Not reachable from whole400-point Material receipts. | G700, P1. Reserve600, close to Jack700/B0. The200 threshold is not a new item denomination. |
| C3R | Same palette/catalogue as C2, S0/K1/m1, so M400. One real Material receipt. | G900, P1. One protected Jack700 with core B200 before filler. |
| C4 | `[1,3,5]`, S0/K2/M0, Pawn-only bases, P2 feasible. Only protection types Nightrider550/Rook500, preferred in that order. | Reject550 first because450+400 exceeds B800. Close two Rooks for400 each. B0, exactly two protected pieces. |
| C5 | `[1,3,5]`, already Rook500/Pawn100, B400, funded P1. Mark existing Rook. Only next ordinary target Rook500. | Existing piece satisfies the claim. Ordinary Pawn-to-Rook costs400. Two Rooks/B0, only one protected. No extra dedicated Castler. |
| C6 | Protected Rook500, B200, both5/7 enabled, no higher-priority feasible action. Jack700 head. | Priority5 spends200, transfers protection, B0. No fixed215 debit. |
| C7 | Pawn100/Pawn100, B900, P1, cheapest closing gap400. Request Queen950 from a Pawn. | Gap850 is affordable but exceeds discretionary500. Reject without mutation because it spends the earmark. |
| F1 | Core terminated, ordinary remainder100, base Pawn100 eligible, one legal vacant Pawn cell. | One derived Pawn100, remainder0. Core unchanged. No new Chessman grant or upgrade source. |
| F2 | Same geometry, ordinary remainder0, aggregate outside-roster credit100. | No filler. Pocket/Play-as-White credit remains outside the purchase. |
| F3 | Ordinary remainder100, no legal vacant Pawn cell. | No filler, remainder100. No displacement or reserve-credit reuse. |

A forced-closure attempt at a lower-value target must fail H3's invariant, regardless of another rule's permission label.
An equal-gap probe retains the prototype's rejection behavior.
The old positive-cost ceiling counterexample remains valid: `100->325` costs225, then `500->525` costs25.
It contains no downgrade and supplies no cached-ceiling proof for this different tail model.

## 11. Integration and delivery gates [E]

Keep the C# default backend, existing sidecar selection, Python package, transport, and release architecture.
No mandatory new service, runtime, or downloader is required.
The canonical metadata binds concrete type IDs/prices, groups, transition table, priorities, weights, seed namespaces, and consumption rules.
It also binds grants, caps, protection, filler policy, role semantics, and final-type rights.
Equivalent normalized palettes produce identical canonical inputs and cache keys.

The slot fields remain `material_policy_version`, `enabled_material_values`, and `material_distribution`.
Producer omissions use declared defaults. Invalid or missing connected fields fail explicitly.
Connected diagnostics identify the offending field, value, and required condition.
They must not substitute another policy or an empty roster.
Reject retired action keys, Legacy itemization, incompatible contracts, and unsupported policy/preset IDs.
Keep offline/preconnection previews separate from connected validation.

Reserve the unshipped v4.0/policy1/protocol2/runtime0.2.0 identifiers from ADR0023 for this reconciled contract.
If another release consumes an identifier, coordinate the next free identifier without changing policy.
Canonical hashes, price exports, source pins, and minimum compatible build are implementation outputs, not additional pricing decisions.

Delivery requires shared C#/Python vectors for seeds, preference rows, child identities, tail order, budget, protection, and full per-geometry output.
Cover all12 palettes, invalid inputs, zero/positive/exhausted type preferences, caps/removal,109 core units, and independent Pawn supply weight.
Cover underfunded protection, two-claim feasibility, Ignore behavior, same-count receipt-order independence, and exact filler/core separation.
Compare final concrete IDs, active/reserve/filler slots, coordinates, ledgers, region usage, promotions, and castling metadata.
Retain historical fixtures unchanged and label their obsolete pricing/algorithm explicitly.
No implementation is complete until parity and the full-world gate pass.

## Review and publication record

The 2026-09-21 coordinator review accepted the complete policy mapping and engineering defaults.
Publication adds explicit material units, total seeded counters, producer integration duties, and connected-input diagnostics.
The exact large-envelope default retains runtime and full-world delivery gates, without a claim of measured feasibility.
Publication establishes this contract only. Implementation handoff remains a separate consuming action.
No gameplay implementation, runtime dependency, or calibration accompanies this decision.

## Optional evidence

The normative contract and fixtures do not depend on session-local research or prototypes.
The [ADR0023 evidence appendix](../planning/material-policy-design-evidence.md) retains historical W measurements and superseded phase outputs.
They do not validate this successor's distributions.

- [Concrete catalogue](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/ChessV.Games/MiscellaneousGames/ApmwPieceCatalog.cs#L24-L85): declared prices and family membership.
- [Existing chooser and ledger](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L371-L565): soft variety and separate ordinary remainder.
- [Outside-roster credits and Pawn choices](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/OwnedRosterGeneration.cs#L753-L785): common credit is not filler cash.
- [Client projection boundary](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ActiveRosterProjection.cs#L339-L485) and [sidecar slot lookup](https://github.com/chesslogic/chessv/blob/d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0/APMW.Client/ApmwSidecarPresentationAdapter.cs#L178-L213): explicit final-output integration.
- [AP lower bounds and target maxima](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/logic_projection.py#L159-L229): conservative metrics remain separate from optional filler.
- [AP envelope and old floor](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/logic_projection.py#L296-L357): the old Pawn100 assumption does not establish a concrete-price floor.
- [Shared exact preparation/output](https://github.com/chesslogic/Archipelago/blob/0772bac724ff483043b56979f0ef078b9c2d264a/worlds/checksmate/apmw_projection/semantic.py#L100-L195): core metrics and exact final output require an explicit split.
