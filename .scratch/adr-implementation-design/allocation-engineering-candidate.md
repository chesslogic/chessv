# Guarantee-first allocation: candidate engineering contract

Status: Proposed engineering specification. Scheduler and pricing admission await authority resolution. No implementation authorization.
Date: 2026-10-25
Scope: E-SERIES, E-FUNDING, E-PREFIX, E-CASTLER, E-TAIL, and E-PLACEMENT.
Consumers: Existing A22/P1, PM/G2, CV01, and Common/D03 owners.

## 1. Decision boundary

This document supplies candidate defaults for the [local allocation plan](plan/design-seeded-army-allocation-1.md), especially TASK-001 through TASK-003.
It supplements the [contracts](contracts.md) and [engineering closure register](engineering-closure.md).
It does not change their acceptance statuses.

This candidate develops the October 21 successor, sections 2, 3, and 5-7.
Its exact source is `C:\Users\aaedi\.copilot\session-state\25c7b461-3d4d-419f-961a-888fe5699cef\files\common-guarantee-first-allocation-successor-v2.md`.
The inspected SHA256 is `8a10074c2dc46a1752dc27ebb8ef7e2b1a35efb3b9b5934f77f7c55a0e037a9e`.
The October 23 plan narrows construction to a local allocator, not an Archipelago solver or a general recovery graph.

**Authority hold:** [Board-material section 2](../board-aware-material-values/spec.md#2-confirmed-scope) also labels a different allocation scheduler accepted.
It stops the prefix at the first unaffordable draw, then remaps affordability-filtered choices and advances counters.
It also selects concrete MG spending, unlike the retained Fundamental funded-value branch.
The coordinator also reports recovered October 23 user authority from session `2181993b-955f-43a7-b1fb-228e6df19b38`.
That authority makes received Chessmen a minimum, not a total-unit maximum.
Post-prefix surplus can fund upgrades or additional Pawns by preference.
The coordinator is obtaining the user's decision about these rules' interaction with protected completion.
This child did not independently retrieve those original events.
The authority reconciliation owns their durable citation.

Sections 3-7 specify shared mechanisms and explicitly bounded alternatives.
They do not freeze the old slot-bound tail, a total-slot ceiling, or a first-failure policy.

**Successor-confirmed behavior:** Inclusive funding uses retained source laws. Preferred assignments precede affordability. The allocator promises the longest completable prefix.
It protects funding and capacity for mandatory completion, then makes one finite optional pass.
Natural Castlers count before shortfall completion. Optional pieces create no permanent entitlement.
The exact optional scheduler and surplus-birth integration now require reconciliation with the later authority.

**Candidate defaults:** Everything in sections 3-7 is proposed engineering, including names, ordering, witness selection, and the completion algorithm.
Requirements quoted from the successor retain their authority. Candidate defaults do not obtain authority through that quotation.

| Authority seam | Disposition for this candidate |
| --- | --- |
| ADR0022 retains Legacy, while ADR0024 retires it | Draft both mode-qualified paths. The coordinator's preliminary reconciliation retains both. Final authority mapping remains external to this document. |
| ADR0024 pools concrete base differences, while the successor protects bases | Do not release a funded100 base merely because its concrete Pawn costs40 or85. Do not import ADR0024's pooled equation. |
| Castler receipts, slots, and charges differ between records | Use the selected mode's actual token and source law. Do not adopt ADR0024's extra-unit grant or current code's preconstruction implicitly. |
| Board-material work can change prices | Require pinned pricing, selector, geometry, and source-law data. Illustrative amounts here do not freeze the next contract. |
| Later received-Chessmen minimum | A funded surplus Pawn has a new local actor identity, not another received Chessman token. The actual total can exceed received Chessmen. |
| Existing hierarchy namespaces and transition schedule | Retain the SHA256 primitive, not the old allocator schedule or suffix-search acceptance claims. |

The accepted CV02 design/reference gate stays closed.
Its receipt is `common-cv02-pres0-composition-acceptance-v1.md` beside the controlling successor.
Native conformance, useful access bounds, generation lifecycle, and D03 integration remain separate obligations.
No new public feature, mode, inventory grant, or old-world compatibility path follows.

## 2. Examined implementation seams

Source references use commit `1913e9f13992bd0ec950ffa1edbbcd36d572be35`.
They establish reusable mechanisms and differences, not successor conformance.

| Source | Relevant observation and candidate consequence |
| --- | --- |
| `APMW.Client\OwnedRosterGeneration.cs:248-285,318-333` | `CounterBasedSeedSeries` hashes a root, namespace, and counter. Reuse its byte convention, but reject missing frozen seeds instead of using the current zero fallback. |
| `APMW.Client\OwnedRosterGeneration.cs:100-200,431-568` | `RosterPiece` separates stable identity, source role, family, and granted/expected material. Current Fundamental generation selects locked Castlers before ordinary upgrades. That scheduling does not implement normal selection plus shortfall protection. |
| `APMW.Client\ActiveRosterProjection.cs:336-478,481-505` | Projection chooses active pieces after generation and favors material within roles. A later projection cannot evict a guaranteed assignment under the successor. |
| `APMW.Client\ActiveRosterProjection.cs:507-645` | Placement uses source roles, rank bands, and separate coordinate series. Reuse those concepts, not unchecked old capacity formulas or post-promise movement. |
| `APMW.Client\ItemGeneration.cs:9-23,106-139` | Expected values, concrete values, and residual accounting already differ. An aggregate `spareMaterial` is not sufficient custody evidence. |
| `APMW.Test\OwnedRosterGenerationTests.cs:417-429` | Existing tests cover counter isolation, not the new candidate order or slot-extension property. |
| [Shared-v4 sections 6.5-6.6](../large-board-enemy-armies/contracts/shared-data-contract-v4.md#66-fundamental-paid-funding-and-source-preserving-ledger) | Existing proposed `SourceKey`, `Part`, `Debit`, and paid-funding records supply the vocabulary. Preserve exact original affiliation and consumed-token state. |

The candidate seam is one pure `Allocate(capture) -> AllocationResult` module.
The result contains the roster, one coupled witness, or a classified diagnostic.
Internal `BuildPreferred`, `RequiredCompletion`, and `TryAcceptFinalEntry` operations remain implementation details.
Callers do not coordinate separate affordability, reservation, and activation decisions.

The capture contains effective instructions, mode, owned sources, seeds, catalog/pricing pins, geometry, rights, and actual ordered Pocket families.
It also contains world/slot binding, generation identity, Ignore Castlers, and captured presentation inputs.
The result binds to that exact capture. No mutable singleton supplies a missing input.

## 3. Exact candidate order

### 3.1 Root, primitive, and version

Candidate algorithm ID: `guarantee-first-candidate-v1`.
Candidate namespace prefix: `allocation.gf.v1`.
These are internal design identifiers, not new slot-data fields or accepted shared-contract versions.

```text
root = decimal(pocket_seed) + "|" + decimal(pawn_seed) + "|" +
       decimal(minor_seed) + "|" + decimal(major_seed) + "|" + decimal(queen_seed)
bytes = UTF8(root + LF + namespace + LF + decimal(counter))
value = unsigned-big-endian-64(SHA256(bytes)[0..8])
r53 = value >> 11
```

Seeds retain the shared-v4 integer domain `0..2147483647`.
Decimal encoding has no sign, padding, separator, whitespace, or locale formatting.
Counters start at zero and use checked nonnegative Int64.
Names use ASCII IDs and ordinal comparison. Composite IDs use length-prefixed UTF8 fields, not ambiguous concatenation.
An ordinary slot's literal ID is `ordinary.<decimal-ordinal>`.
Namespace substitutions for IDs use unpadded base64url of their UTF8 bytes.
The literal proposal ordinal remains invariant decimal.

The authenticated world/slot and frozen five-seed tuple bind the root.
The root contains neither current Material nor delivery order, geometry, local Chaos draws, or a fresh per-setup random value.
Different bindings can share the same tuple without sharing result-admission authority.
Catalog, policy, and price hashes belong in result binding, not in a count-dependent root.

### 3.2 Stable keys and weighted permutations

| Entity | Candidate identity and order |
| --- | --- |
| Source | Existing `(item_name, ordinal)` within effective canonical inventory. Item names use ordinal comparison, ordinals use numeric comparison. Raw/excess ownership remains separate. |
| Ordinary slot | `(ordinary, ordinal)`, ordinal `0..s-1`, for genuinely exchangeable owned slots. Special roles retain their own source-bound IDs. |
| Concrete member | Pinned catalog `type_id`, family ID, and funded-quality tag. A display name, notation, or runtime object hash is not identity. |
| Preferred entry | `(slot_id, member_id)`, visited by numeric ordinary ordinal after fixed dedicated obligations. |
| Optional proposal | `(proposal_ordinal, member_id, operation_kind)`. Operation kinds distinguish assignment to an existing slot from a permitted funded birth. The scheduler supplies the ordinal. |
| Placement | `(relative_rank, file)` in the captured human formation. The primary square and actual mandatory occupants are unavailable. |

Material sources never increase the received-slot count `s`.
Fundamental ordinary slots map to unique effective Chessmen tokens under the retained slot law.
Legacy slots require an explicit final-unit entitlement from the retained mode mapping.
Merged sources retain one beneficiary unit, not one unit per payer.
The compiler cannot invent received tokens by dividing gross material by a Pawn price.
Later lawful surplus births remain separate from this received-slot table.
Each birth requires an eligible funding debit, actual active capacity, and its own local actor identity.

For each preferred slot, construct a family permutation without replacement.
Start with ascending family IDs and the pinned positive integer weights.
Select the first cumulative weight `c` for which `r53 * totalWeight < c * 2^53`.
Remove that family and repeat, with one counter per emitted family.
Use exact integer products, not a logarithm, modulo shortcut, or platform RNG.
Rational weights require an agreed exact integer normalization before capture.

Within each family, construct the same permutation over ascending member IDs with unit weights.
The preferred namespace suffixes are `.preferred.<slot-id>.family` and `.preferred.<slot-id>.member.<family-id>`.
Reserve `.optional.<proposal-ordinal>.family` and `.optional.<proposal-ordinal>.member.<family-id>` for optional ordering.
Their counter-advancement and candidate-remapping rules remain pending, not implied by those names.
Each separate permutation starts its counter at zero.
Preferred-table construction does not depend on rejected affordability probes.
Optional failed-draw counter advancement remains part of the pending scheduler decision.

Hard-disabled choices never enter the candidate set.
Soft-zero and recovery eligibility come from the effective instruction contract, not an invented weight.
If an admitted recovery-only family has zero weight, append it in ordinal order after positive-weight families.
Its activation still requires the retained recovery condition.
The exact action-to-final-family weight compiler remains a fixture prerequisite, not an excuse to revive the old action scheduler.

Candidate type-variety default: stable-partition each member permutation into below-limit members, then remaining members.
Counts use earlier structurally preferred entries, never affordable-prefix results.
Optional variety counts remain pending with the scheduler.
The preferred-table default preserves budget-independent ordering but can differ from historical variety distributions.
It is an observable proposed default that needs acceptance.

### 3.3 Preferred assignments and qualified extension

`BuildPreferred` visits real slots in canonical order, without examining spendable balances.
For each slot, it takes the first member with a legal source derivation and an unused compatible square.
Square order uses `(rank-priority, value, relative_rank, file)`.
The value uses namespace `.square.<slot-id>.<member-id>` and counter `relative_rank * files + file`.
Rank priority preserves the retained role preference, with lower relative ranks first inside that preference.

This stage binds the member, source role, slot, and square.
It does not debit resources or construct a minimum roster.
Mandatory dedicated actors occupy their required squares before ordinary assignment.
Required Castler shortfall is not a dedicated actor placed during this stage.
Its protection belongs to `RequiredCompletion`.

If a slot has no structurally legal active assignment, it has no preferred entry.
It still retains its baseline/reserve obligation and source ownership.
It cannot become an active promise through a fictitious square.
The preferred list contains the successfully assigned entries in visitation order.
Affordability can shorten that list but cannot choose a different preferred member.

For unchanged slots, legal choices, and geometry, Material-only growth preserves this entire preferred table.
Appending an exchangeable ordinary slot preserves earlier choices and squares because earlier iterations do not inspect the final slot count.
This is conditional on unchanged mandatory occupants and per-slot source admissibility.

Canonical receipt ordinals are not delivery timestamps.
Appending a different Legacy source family can change admissibility or mandatory obligations.
It is not automatically the qualified ordinary-slot extension case.
The source compiler must prove exchangeability before replacing source-bound roles with ordinary ordinals.
New eligibility can change a member without changing its seed root.
Such changes require the separate certified-access proof.

## 4. Typed funding and custody

The following records refine existing shared-v4 candidates.
They describe local semantic data, not new public C# declarations.

```text
OwnedSlot { slot_id, token_keys[], birth_role, allowed_roles[], baseline, custody_id }
FundedBirth { actor_id, operation_kind, payer_parts[] }
ActorRef = OwnedSlotRef(slot_id) | FundedBirthRef(actor_id)
Source { key, mode_domain, received_credit, entitlement_state }
Part { source_key, owner_kind, owner_id, amount }
Funding { base_part_ids[], ordinary_extra_parts[], quality_extra_parts[],
          dedicated_parts[], signed_residuals[], historical_rights }
FinalEntry { actor_ref, member_id, final_role, funding, active_square | reserve }
Debit { debit_id, beneficiary_actor, cost_kind, amount, ordered_payer_parts[] }
Release { debit_id, original_part, amount, lawful_reason, next_owner }
CompletionWitness { binding, entries[], debits[], releases[], final_parts[],
                    active_ids[], reserve_ids[], composition, castlers, pockets }
```

Amounts use checked Int64.
Credits, Parts, debits, and releases are nonnegative.
Signed residuals have a separate tagged type and never enter a positive-balance field.
Parts coalesce only by identical source, owner kind, and owner ID.
Zero Parts disappear. Neither coalescing nor a release changes token entitlement.

Candidate payer order uses the law's eligibility tiers first, then source key, owner kind, and owner ID.
An existing prescribed payer order overrides this generic tie rule.
Each debit records actual allocations, not only its beneficiary.
A release restores only its recorded refundable allocations to their original lawful owners.
It cannot convert dedicated credit into ordinary Material or resurrect a consumed source.

| Mode-qualified branch | Required compiler output |
| --- | --- |
| Fundamental base | One protected funded100 baseline per real Chessman under the retained law. Concrete40/85/100 does not release the difference. |
| Fundamental paid quality | Extra50 or100 for funded150/200. Final-state debit counts the extra once. Ordinary extra and quality extra are mutually exclusive. |
| Fundamental ordinary family | Exact pinned extra liability above the protected base. Expected family material remains a different quantity from concrete BOARD material. |
| Castler/dedicated role | Actual compatible owned token, eligible payer Parts, role ceiling, and retained surcharge. Natural fulfillment cannot mint that token or refund an unrelated dedicated account. |
| Legacy direct/superseded sources | One final beneficiary with all source Parts and consumed-token affiliations. Positive and negative concrete residuals remain visible. |
| Legacy conversion | Net eligible non-Pawn residuals before the authorized zero floor. Protect the actual Pawn reservation. Exclude reserves, royals, primary, pockets, and dedicated accounts. |
| Legacy historical exception | Separate once-per-episode allowance and nonrenewable operation masks. An authorized overdraw is not ordinary `>=` affordability or another receipt. |

For the retained Fundamental candidate, `MaterialDebit = ordinary_extra + quality_extra + castler_extra`.
The setup compares available eligible Parts with the final debit, including equality.
An atomic transition uses `new MaterialDebit - old MaterialDebit`, with source-specific release for a negative delta.
It never sums successive full final-piece prices.

At the next setup, unpromised prior extras can release recorded lawful extra funding.
The previous active match remains unchanged.
Protected reserve or dedicated funding stays locked unless an explicit mode rule authorizes its transfer.
An entry's disappearance does not itself authorize a refund.

A surplus Pawn uses `FundedBirth`, not a fabricated `Source` or received token.
Its candidate ID is `(surplus-pawn, setup_binding, birth_ordinal)`.
Its attached value comes from actual payer Parts.
It cannot also leave those Parts available for an upgrade or protected completion.
The birth ordinal is local construction identity, not a permanent inventory entitlement.
The actor's `FinalEntry` records its member and square.
Its existence does not increase direct-entitlement count D or restore a consumed token.
The admitted birth catalog and exact charging rule await the scheduler/pricing resolution.

Funding validation precedes optional spending and follows every accepted trial.
For each source, final ownership and lawful spending reconcile to its original credit.
The Legacy allowance, deficit, and overdraw accounts reconcile separately under shared-v4 section 6.6.
A scalar total cannot hide duplicated Parts, lost custody, or the same reserve funding in two consumers.

## 5. One coupled completion witness

### 5.1 Obligations and result

`RequiredCompletion(capture, fixedEntries, target)` constructs the remaining entries without changing `fixedEntries`.
A successful result includes actual active squares, reserve ownership, and all funding allocations.
The result also includes actual composition counts, Castler applications, and Pocket support.
It is not a collection of independent minima.

The retained prepared targets are `(11,5)`, `(15,7)`, `(19,9)`, `(27,15)`, and `(33,19)` for the five supported geometries.
`none` imposes neither target. `standard_count` imposes T. `standard_and_pawns` imposes T and N.
Targets remain minima, not ceilings or a separate Pawn quota.
Missing inventory yields a recorded shortage, not an entitlement.

The input obligation record must distinguish the nominal target from its retained attainable requirement.
The allocator cannot lower that requirement after an expensive prefix or optional choice.
The exact shortage producer requires mode-specific acceptance. Independent maxima for T and N cannot establish a jointly attainable pair.

Let `n` be the actual active non-primary count and `j` its non-Pawn count.
Support at most `floor(n/2)` occupied Pockets, with non-Pawn families first and captured slot order inside each class.
Let `p` and `p_nonpawn` count those supported actual Pockets.
Composition counts are `n+p` and `j+p_nonpawn`.
The primary and reserve units contribute to neither count.

Legacy reservation also derives `D` from deployed original direct-unit entitlements, not payer count or current family.
The witness computes `R=min(availablePawnItems,max(0,T-D-p))` and verifies the retained source protection.
Conversion-funded non-Pawns can increase `j` without increasing `D`.
The reservation and final support must agree in the same witness.

Let `B` be concrete active non-primary BOARD material.
Let `S` be semantic material of the supported actual Pocket families and `C=p` their supported count.
The material bound remains `H=min(S,floor(B/2),300*C)`.
This bound differs from the count-support calculation.
Unsupported occupied Pockets add neither S nor C.
Neither H nor expected material funds a purchase.

### 5.2 Castler target and reservation

Derive the enabled cap from actual receipts, retained applicability, Ignore Castlers, and the existing maximum of two.
Try target two, one, then zero, bounded by that cap.
Each trial includes dedicated actors, actual source eligibility, the retained composition obligations, and compatible home-rank placement.
Select the greatest target with a complete witness before ordinary spending.
A failed heuristic cannot reduce the target.

For each prefix or optional trial, count qualifying naturally selected home-rank actors toward the target.
The remaining target is `max(0,target-natural_count)`.
Completion supplies only that shortfall through real compatible owned slots.
The retained role and rights predicates decide qualification, not the name "Rook" or a reserve entry.

The saved witness protects exact payer allocations and actual compatible capacity.
It is provisional: later normal choices can replace uncommitted completion entries with another valid witness.
It is not a second roster, an extra debit, or up-front construction of every Castler.
At finalization, only the last witness becomes the remaining roster.

### 5.3 Bounded completion subproblem: slot-layer dynamic programming

The proposed implementation uses a local dynamic program over remaining real slots.
It does not search move histories, Archipelago items, recovery cycles, or alternate worlds.
The first reference profile uses independently owned slots and exact final-state liabilities.
Its proposed proof covers only this subproblem, not a total-unit ceiling or the complete later surplus domain.
Already accepted surplus actors enter as fixed entries with their actual funding and square occupancy.
If mandatory completion itself requires new births, this initial profile is insufficient.

The row compiler enumerates each slot's finite permitted final members, baseline/reserve alternatives, and legal placement classes.
Each row carries its typed debit, role contributions, Castler predicate, and token effects.
No row retracts a fixed preferred entry or accepted extra.
Rows include retained fallback obligations rather than omitting inconvenient slots.

Placement classes group squares only when every remaining row has identical compatibility with those squares.
Primary occupancy, home-rank side restrictions, fixed assignments, and locked actors split classes where necessary.
For the simple rank-only profile, the classes are back, mixed, and Pawn-only capacity.
Forwardness or exact-square restrictions require finer classes.
Different source roles are not interchangeable merely because their concrete families match.

The initial state contains the fixed entries and their debits.
Process remaining slots in canonical order.
For each state and each row, add the row only if its token, class-capacity, and funding requirements remain legal.
Coalesce states only when all future legality predicates have identical inputs.

| State coordinate | Reason it cannot disappear |
| --- | --- |
| Placement-class occupancy | Counts alone do not protect home-rank or shared mixed capacity. |
| Actual n, non-Pawn contribution, and D | Actual n controls Pocket support. D controls Legacy reservation. Target-saturated counters are allowed only after equivalent future behavior is proved. |
| Castler count, saturated at the fixed target | Natural selection and remaining shortfall share one target. |
| Source-domain spending and remaining entitlement/mask state | Restricted sources, reserve locks, and nonrenewable rights constrain future rows. |
| Additional certified predicates | A future material or role guarantee can require extra state. It cannot enter after state coalescing. |

For one fungible extra-Material account, retain the least-spend predecessor for an otherwise identical state.
Equal-spend ties use the lexicographically smallest complete row-ID sequence.
Across restricted accounts, retain nondominated spend vectors only when every future eligible use is identical.
Do not collapse distinct original source ownership into a scalar.
Witness reconstruction retains the exact source allocations.

At terminal states, compute actual Pocket support, T/N satisfaction, Pawn reservation, and all dedicated obligations together.
Reject states that fail an obligation.
Choose the least-spend valid witness, then the canonical row-ID sequence.
With multiple account domains, use the lexicographic spend vector in canonical domain order after feasibility, not to prune incomparable states.

Expand each placement class to distinct squares in the saved seeded square order.
Identical class compatibility makes this expansion legal.
Record actual squares before returning success.
If a row depends on interactions between individual squares, class counts alone cannot certify it.
That case requires a finer state or remains unsupported by this profile.

This subproblem has a conditional completeness argument.
Every permitted row combination appears in one slot-layer path when the compiler is complete.
Class expansion preserves legality when the interchangeability premise holds.
Least-spend pruning preserves completion only under the stated future-equivalence premise.
These are obligations for a reference probe, not executed proof results.

### 5.4 Finite bounds and practical limits

Let `s` be the remaining slots, `v` the largest per-slot row count, and `L` the maximum retained labels per layer.
The dynamic program examines at most `s*v*L` transitions per completion call.
Two live layers require `O(L)` state storage, with predecessor storage for witness reconstruction.
Retained predecessor storage is at most `O(s*L)` records.
Class occupancies range from zero through their captured capacities.
Count coordinates range from zero through the real slot count.
The Castler coordinate has at most three values.

Let `t` be the global number of eligible final members and `q` the captured free-square count.
There are at most three initial Castler-target trials and `preferred_count+1` prefix trials in the successor alternative.
For the old slot-bound optional alternative, `s*t` occurrences with at most `s*q` bindings each give a finite bound.
That bound does not cover later surplus births or prove the newly clarified scheduler.
The final optional bound must include legal birth and replacement operations separately.

For shared fungible Material and a few rank classes, dominance can keep L small.
That is an engineering expectation, not a measured performance claim.
Independent restricted accounts, distinct square predicates, or shared Legacy source bundles can make L exponential.
This candidate does not authorize an unbounded exhaustive oracle as the production fallback.

The first reference probe must cover ordinary Fundamental funding and the smallest counterexamples for restricted funding and Legacy conversion.
Legacy signed releases, coupled source consumption, and historical masks require exact row-compiler and dominance proofs.
The candidate does not yet establish their practical complete profile.
If those proofs require broad source-subset enumeration, isolate the exact case and propose a narrower construction before implementation.

Birth finiteness needs a separate argument, not an assumed received-slot maximum.
With positive minimum lawful birth debit `d`, finite spendable funds `F`, and free active capacity `q`, at most `min(q,floor(F/d))` births fit.
This bound applies only when births cannot release more funds and no exception bypasses that debit.
Approximation rights require their explicit finite allowance and consumed-mask bounds.
The source compiler must reject an unproved zero-cost or repeatable-credit cycle, not impose an arbitrary sampling cutoff.

No arbitrary label cap turns a search failure into "unaffordable" or "exhausted."
Malformed inputs return a field-specific contract error.
Unproved profiles return a design/proof limitation, not a shorter claimed longest prefix.
An operational timeout or memory limit returns an explicit resource error with no admitted setup.
Normal supported runtime inputs cannot rely on these refusals as a substitute for the missing engineering proof.

## 6. Protected selection and final admission

The following is the October 21 successor candidate, not a decision against the later scheduler.
Steps 4-5 and 7-10 remain conditional.

1. Validate the immutable capture and the mode-qualified row compiler.
2. Establish dedicated obligations and construct the preferred table without affordability.
3. Derive the retained attainable composition requirement and feasible Castler target before optional spending.
4. Try preferred prefix lengths from longest through zero, each against the original captured ledger.
5. Select the first length with a complete coupled witness.
6. Freeze that prefix's member, slot, role, square, and debit associations.
7. Visit the old slot-bound alternative's finite family/member occurrences.
8. Try compatible unfilled slots in canonical order, then compatible free squares in the candidate square order.
9. Accept the first binding with an inclusive lawful debit and a replacement completion witness.
10. If every binding fails, skip the occurrence permanently.
11. Finalize the last saved witness after the optional pass.
12. Validate the capture binding and the complete roster before admission.

A tentative binding can change only uncommitted completion entries.
A rejected trial leaves the prior local state unchanged.
It cannot leave a debit, consume a historical right, advance a random counter, or free a reserve lock.
The saved witness is data in the pure module, not mutations to a live Game.

Descending prefix trials avoid an unproved first-failure assumption with typed releases.
If all trials fail because the declared attainable obligations contradict the actual capture, report the inconsistent obligation and its sources.
Do not silently replace the requirement with zero.
Legitimate retained shortages need an explicit valid shortage witness.

The old slot-bound candidate uses `s` occurrences per eligible final member.
That is sufficient for remaining owned slots but not the later funded-birth domain.
No unfilled received slot does not imply that optional allocation stops.
A permitted birth must still pay its actual charge, preserve the coupled witness, and occupy an actual compatible square.

| Scheduler seam | October 21 candidate | Later October 23 authority reported by coordinator |
| --- | --- | --- |
| Prefix stop | Longest fixed preferred prefix with a valid completion | First unaffordable concrete draw |
| Later order | One fixed finite candidate order, permanent skips | Affordable family/type filtering and remapping, advancing counters |
| Actors | Entries for existing owned slots | Existing slots plus lawfully funded additional Pawns |
| Replacement | No replacement of accepted extras in the pass | Surplus upgrades require exact replacement and funding semantics |

The coordinator must reconcile these rows before a single executable optional algorithm is frozen.
Both alternatives retain explicit source custody, real placement, and no invented receipts.
Neither supplies an optimal-packing promise.

Final activation consumes the witness's active/reserve partition.
It cannot rerun the old material-first activation selector.
Presentation can change coordinates only within an accepted legal equivalence that preserves promised placement predicates.
Forwardness and Chaos cannot move a promised Castler off its protected home rank or invalidate a Pocket-support certificate.
An unavailable presentation refinement leaves the admitted semantic placement intact, not a different piece.

## 7. Discriminating acceptance cases

These are specified expectations, not executed fixtures.
Synthetic amounts and tiny residual-board cells isolate mechanisms.
They do not replace the full paired ContractV4/ProjectionV4 capture fixtures.
Unless stated otherwise, dedicated obligations are already satisfied and no historical exception applies.

| ID | Input or construction | Required observation |
| --- | --- | --- |
| EQ | One funded100 slot, permitted Minor300, one legal square. Extra Material199,200,201 in three captures. | Minor debit200 fails at199 and qualifies at200/201. Equality still fails with no compatible square or wrong-source funds. |
| BASE | One funded100 slot selects ordinary Checker40. No extra Material. | Concrete B is40, funded base remains100, and spendable refund is0. A second slot or paid upgrade cannot use the difference60. |
| QUALITY | Prior paid Pawn200 has recorded extra100. Next setup permits Pawn150. | Release50 to its original eligible payer. Keep base100, one slot, and consumed-token state. Do not refund concrete evaluation or mutate the old match. |
| RESERVE | Ordinary free0, reserve-owned refundable extra200, dedicated100. Proposed ordinary debit200. | Reject while reserve ownership remains. With an independently authorized reserve release, transfer only the recorded extra200, then admit equality. Dedicated100 remains unavailable. |
| CROWD | One free home square, one free mixed square. One unmet Castler needs the home square. | Reject an ordinary binding to home that destroys completion. A tail binding to mixed can pass. Two free squares alone do not establish feasibility. |
| CAST | Target2, two compatible owned slots, synthetic charge q for each natural or shortfall application, eligible funds2q. | Zero natural selections require two completion applications. One natural application leaves only one protected q/slot. Two natural applications leave zero shortfall. No extra receipts appear. Real unequal charges need separate pinned rows. |
| IGNORE | CAST capture with Ignore Castlers enabled, then a capture with only one feasible application. | Enabled target follows Ignore's retained law. Otherwise complete target1 only after a full target2 infeasibility result, not a greedy miss. |
| POCKET | Four active non-primary units, j1, actual pockets `[Pawn,Minor,Queen]`. Synthetic residual targets T6/N3. | Support Minor and Queen first. Counts6/3 pass. With only three active units, support only Minor and counts4/2 fail. Reserve units cannot repair support. |
| MATERIAL | Three supported Minor pockets give S900/C3. Six active Pawns change from five Checker40 plus Pawn100 to six Checker40. | B changes300 to240 and H changes150 to120. Expected Pawn material remains600. Primary value and reserves cannot preserve the old150 claim. This is a proposed eligibility-change fixture, not an executed roster. |
| ELIGIBILITY | Same seeds and slots. A newly eligible earlier-ranked member replaces an old preferred member at the next setup. | Root stays unchanged. The change is lawful selection, not a reroll. Access acceptance remains blocked until every credited predicate survives the covered extension. |
| TAIL | October 21 alternative only: after the prefix, free7. Fixed optional order X:9, Y:3, Z:5, W:4. Two compatible slots. | Skip X, accept Y, skip Z, accept W. Remaining0. This expectation cannot validate the later remapping scheduler. |
| BIRTH | All received slots are filled. Lawful surplus80 remains. Two compatible squares and permitted birth debit40. | A minimum-not-maximum allocator can fund two additional Pawns if completion stays valid. No received-token count increases. A slot-bound tail cannot satisfy this case. |
| SLOT | Same legal choices, sources, geometry, and mandatory occupancy. Append one exchangeable ordinary token. | Existing preferred entries, priorities, and squares are identical. The new entry follows them. A changed source-role restriction does not satisfy this case's premise. |
| SIGN | Prefix1 requires unavailable eligible credit. Prefix2 includes a specifically lawful release that makes the combined final assignment fundable. | A complete typed transaction can accept prefix2. An illegal release cannot. Do not implement scalar first-failure or an unconditional zero clamp. |
| LEDGER | Legacy sources300 and485 fund concrete500. Another eligible residual is-200. | Preserve source identity and one beneficiary. Residual285 nets to85, not285. No reserve or dedicated account enters the net. |
| LIMIT | A completion profile lacks a valid interchangeability/source proof, or requires uncovered surplus births. | Return a proof limitation. Do not claim maximum prefix, impossible target, zero access, or semantic exhaustion. |
| GEOMETRY | One geometry meets material only. Another meets the required role only. | Neither passes the full access predicate. No cross-geometry witness joins their favorable coordinates. |

### Primitive reference vectors

The namespace for all four rows is `allocation.gf.v1.preferred.b3JkaW5hcnkuMA.family`.
Its encoded slot segment denotes `ordinary.0`.
The input is exactly `root + LF + namespace + LF + decimal(counter)`, without a final newline.
Raw64 uses hexadecimal. r53 uses decimal.
Root Z is `0|0|0|0|0`.
Root M is `2147483647|2147483647|2147483647|2147483647|2147483647`.

| Root | Counter | Raw64 | r53 |
| --- | ---: | --- | ---: |
| Z | 0 | `82ab2a90eaa12793` | 4597493285082148 |
| Z | 1 | `e5035a1dc7c13cb4` | 8057681906038823 |
| M | 0 | `5d83d6e0469fe58f` | 3290266467881980 |
| M | 1 | `ffce52ee7e3413e3` | 9000371830638210 |

These values came from a .NET SHA256 calculation during document preparation.
That calculation did not execute the allocator or establish cross-language conformance.
Independent expected fixtures must also cover a sole member, a weighted boundary, and equal-rank tie-breaking.
For weights `[1,1]`, synthetic r53=`2^52` selects the second member because the comparison is strict.
A sole member always wins, and still consumes its permutation counter.
Permutation fixtures must scramble input collection order and receipt delivery order without changing results.
The case authors must freeze expected vectors independently before either implementation generates its own expectations.

## 8. Handoff and remaining decisions

| Consumer | Required input from this work | Obligation that remains |
| --- | --- | --- |
| Authority/Common | Explicit candidate defaults and the section1 conflicts | Final mode, price, slot, seed, and old-clause mapping. This document does not edit the ADRs or closure register. |
| A22/P1 | Typed final rows, funded-birth separation, stable preferred order, and one witness | Reconcile the scheduler. Specify complete source/row compilers and surplus bounds. Prove practical completion for every admitted profile. |
| CV01/fixture owners | Section7 plus exact candidate namespaces and source revision | Supply independent full captures, pins, PRF vectors, custody traces, active squares, and negative/restoration pairs in both languages. |
| Conservative-access/PM | Exact witness resources, covered assumptions, and no optional entitlement | Prove useful universal lower bounds over covered receipts, eligibility, geometry, and actual Pocket choices. Prefix length is insufficient. |
| Generation lifecycle/G2 | Immutable input/result binding and classified non-success | Bind before the earliest dependent query. Preserve starts, links, copies, retained plando, frozen prices, and real upstream accessibility acceptance. |
| Later ChecksMate child | Exact namespace/encoding proposal, row semantics, and required fixture outputs | Investigate cross-repository implementations and lifecycle hooks. No such source investigation occurred here. |
| Coordinator/D03 | This candidate and the explicit remaining proof gates | Integrate accepted results only. Do not infer implementation, native, package, or release readiness. |

The scheduler/spending conflict in section1 requires the coordinator's pending user decision.
No additional product decision is proposed here.
The coordinator must obtain acceptance for observable engineering defaults before freezing them.
Those defaults include variety-count scope, cheapest-completion ties, and exact preferred-square selection.
If the local completion probe fails practical bounds, the next decision concerns a narrower exact mechanism, not weaker guarantees.

The source compiler still needs special attention for Legacy shared bundles and signed release cases.
The qualified extension proof must identify which source groups are genuinely exchangeable.
The actual price snapshot and Castler liability mapping remain pinned-input prerequisites.
These limits prevent this candidate from claiming engineering closure.

This deliverable changes documentation only.
Only the stated primitive-vector calculation ran.
No successor model, fixture suite, native allocator, generator, or benchmark ran as part of its preparation.
