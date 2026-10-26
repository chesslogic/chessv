# Guarantee-first allocation: candidate engineering contract

Status: Proposed engineering specification under reconciled purchase guarantees. Pricing and exact mechanics remain open. No implementation authorization.
Date: 2026-10-25
Reconciliation: 2026-10-25, coordinator relay `call_FMqPTlTVzppQi3hbAotueDGW`.
Prefix-upgrade decision: 2026-10-26, coordinator relay `call_lqd1YtSNa0r8nAs3wbi6dbA2`.
Scope: E-SERIES, E-FUNDING, E-PREFIX, E-CASTLER, E-TAIL, and E-PLACEMENT.
Consumers: Existing A22/P1, PM/G2, CV01, and Common/D03 owners.

## 1. Decision boundary

This document supplies candidate defaults for the [local allocation plan](plan/design-seeded-army-allocation-1.md), especially TASK-001 through TASK-003.
It supplements the [contracts](contracts.md) and [engineering closure register](engineering-closure.md).
It does not change their acceptance statuses.

The authority baseline is commit `6158507a0b8f0095fb69ff3d0d85779f3b1ee153`.
Its [ADR0024](../../docs/adr/0024-seeded-hierarchical-allocation.md#current-decision) and [current frontier](engineering-closure.md#current-authority-and-frontier) record the user decisions and original evidence.
This child read those documents from the commit objects without changing their files.
The October 23 plan keeps construction local, not an Archipelago solver or a general recovery graph.

**Resolved guarantee reconciliation:** Every later purchase preserves the promised prefix and feasible mandatory completion, including Castler funding and placement.
The later elimination, surplus-upgrade, and additional-Pawn behavior remains selected.
Standalone affordability cannot consume funding or capacity necessary for mandatory completion.
For Fundamental, this candidate does not reinstate the old fixed tail, descending-prefix scan, or total-slot ceiling.

Sections 3-7 propose engineering under this resolved rule.
Inclusive lawful funding, stable underlying priorities, actual placement, and the protected prefix remain.
Natural Castlers reduce the shortfall. Optional actors create no received inventory or permanent entitlement.
Exact refund, counter, and pricing details remain open.
**Resolved prefix upgrades:** The user permits an otherwise permitted funded improvement of a promised actor while preserving the promised guarantees.
The answer explicitly adds Castlers needed for a Location to funding, counts, roles, legal placement, and certified-access preservation.
The exact answer was: "The only constraint not mentioned in the recommended option is castlers, which are needed for a location. Otherwise yes".
There is no exact-piece freeze, blanket family/price dominance, or new refund right.
The authority writer owns the durable decision record. Exact predicate encoding and upgrade enumeration remain engineering work.

**Candidate defaults:** Everything in sections 3-7 is proposed engineering, including names, ordering, witness selection, and the completion algorithm.
Retained requirements apply through the reconciliation just stated.
Candidate defaults do not obtain authority through that reconciliation.

| Authority seam | Disposition for this candidate |
| --- | --- |
| Retained modes | Both Legacy and Fundamental remain. Later board-material amendments apply to Fundamental, not a new Legacy redesign. |
| Historical ADR0024 pooled concrete base differences | Current authority preserves funded100 for ordinary Pawn40/85/100. Do not import the superseded pooled equation. |
| Castler receipts, slots, and charges differ between historical records | Use the selected mode's actual token and source law. Do not adopt the superseded extra-unit grant or current code's preconstruction implicitly. |
| Board-material work can change prices | Require pinned pricing, selector, geometry, and source-law data. Illustrative amounts here do not freeze the next contract. |
| Later received-Chessmen minimum | A funded surplus Pawn has a new local actor identity, not another received Chessman token. The actual total can exceed received Chessmen. |
| Existing hierarchy namespaces and transition schedule | Retain the SHA256 primitive, not the old allocator schedule or suffix-search acceptance claims. |

The accepted CV02 design/reference gate stays closed.
The current frontier links its exact accepted receipt and pin.
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
| Optional proposal | `(proposal_ordinal, member_id, operation_kind)`. Operations distinguish final assignment, a permitted actor upgrade, and funded Pawn birth. Source-actor identity distinguishes upgrade bindings. |
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
Section 6 proposes their counter-advancement and candidate-remapping rules.
Each separate permutation starts its counter at zero.
Preferred-table construction does not depend on rejected affordability probes.
An actual failed draw consumes its position under the later authority.
That does not authorize completion probes to consume draw positions.

Hard-disabled choices never enter the candidate set.
Soft-zero and recovery eligibility come from the effective instruction contract, not an invented weight.
If an admitted recovery-only family has zero weight, append it in ordinal order after positive-weight families.
Its activation still requires the retained recovery condition.
The exact action-to-final-family weight compiler remains a fixture prerequisite, not an excuse to revive the old action scheduler.

Candidate type-variety default: stable-partition each member permutation into below-limit members, then remaining members.
Counts use earlier structurally preferred entries, never affordable-prefix results.
Optional variety-count scope remains an engineering choice.
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
The admitted birth catalog and exact charging rule await the pricing/source-law specification.

An optional upgrade identifies one eligible source actor and its proposed final entry.
Promised actors are eligible for otherwise permitted improvements that preserve their promises, including required Castler support for its Location.
The proposed transaction preserves its birth affiliation and records the old debit, authorized releases, and new payer allocations.
The release cannot double-count the old value or unlock protected base/reserve funding.
Concrete price differences alone do not establish a lawful refund.
The guarantee reconciliation authorizes guarded purchases, not a new downgrade or refund rule.

Funding validation precedes optional spending and follows every accepted trial.
For each source, final ownership and lawful spending reconcile to its original credit.
The Legacy allowance, deficit, and overdraw accounts reconcile separately under shared-v4 section 6.6.
A scalar total cannot hide duplicated Parts, lost custody, or the same reserve funding in two consumers.

## 5. One coupled completion witness

### 5.1 Obligations and result

`RequiredCompletion(capture, postPurchaseEntries, target)` constructs the remaining entries without undoing the proposed purchase.
A transaction first replaces its permitted upgrade source with the proposed final entry.
Every other already accepted entry remains fixed during that completion call.
If the source actor has a promise, admission must establish that the proposed result still satisfies that promise.
Admission preserves funding, counts, credited roles, legal placement, certified access, and required Castler support for its Location.
A higher price, family label, or evaluated strength is not a substitute for those predicates.
The exact machine-readable predicates remain to specify, not the permission to improve a promised actor.
The completion helper cannot refund or retract the new purchase to justify its admission.
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
Already accepted surplus actors enter with their actual funding and square occupancy.
The selected upgrade target enters in its post-purchase form, not its old form.
If mandatory completion itself requires new births, the independent-slot profile alone is insufficient.
The bounded birth extension described next is a proposed correction, not a proved full-domain implementation.

The row compiler enumerates each slot's finite permitted final members, baseline/reserve alternatives, and legal placement classes.
Each row carries its typed debit, role contributions, Castler predicate, and token effects.
No row retracts another accepted actor or the proposed purchase.
Every row must preserve the accepted promises under the eventual explicit interpretation.
Rows include retained fallback obligations rather than omitting inconvenient slots.

For a positive-debit birth profile, append at most `b` virtual birth positions after the real-slot layers.
Here `b` is the lawful birth bound from section 5.4, not a received-token count.
Each position offers one row per permitted Pawn member and an explicit no-birth row.
Real birth rows pay their exact source-qualified charge and consume actual placement capacity.
A no-birth row grants nothing and changes neither actor count nor funding.
Reconstruction assigns consecutive local birth ordinals to the real rows.
These positions enumerate possible funded actors, not fictitious owned slots.
Reserved completion births remain uncommitted until the final witness becomes the roster.

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
| Concrete BOARD material B | B+H obligations require B in state equivalence, or a proved equivalent dominance reduction. Counts, Castlers, and spend alone are insufficient. |
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
For the birth extension, let `b` be its bounded birth positions and include their rows in `v`.
The dynamic program examines at most `(s+b)*v*L` transitions per completion call.
Two live layers require `O(L)` state storage, with predecessor storage for witness reconstruction.
Retained predecessor storage is at most `O((s+b)*L)` records.
Class occupancies range from zero through their captured capacities.
Count coordinates include fixed actors, remaining real slots, and at most `b` new actors.
The Castler coordinate has at most three values.

There are at most three initial Castler-target trials.
The Fundamental preferred walk examines each entry once until the first inadmissible draw.
Section 6 bounds the optional loop by actual purchases and finite state-local candidates.
No bound assumes that total actors equal received Chessmen.

For shared fungible Material and a few rank classes, dominance can keep L small.
That is an engineering expectation, not a measured performance claim.
Independent restricted accounts, distinct square predicates, or shared Legacy source bundles can make L exponential.
This candidate does not authorize an unbounded exhaustive oracle as the production fallback.

The first reference probe must cover ordinary Fundamental funding, funded births, and a guarded upgrade of an optional actor.
Restricted funding and Legacy conversion need their smallest discriminating counterexamples.
Legacy signed releases, coupled source consumption, and historical masks require exact row-compiler and dominance proofs.
The candidate does not yet establish their practical complete profile.
If those proofs require broad source-subset enumeration, isolate the exact case and propose a narrower construction before implementation.

Birth finiteness needs a separate argument, not an assumed received-slot maximum.
With positive minimum lawful birth debit `d`, finite spendable funds `F`, and free active capacity `q`, at most `min(q,floor(F/d))` births fit.
This bound applies only when births cannot release more funds and no exception bypasses that debit.
Approximation rights require their explicit finite allowance and consumed-mask bounds.
An unproved zero-cost or repeatable-credit cycle blocks acceptance of that compiler profile.
It is not permission for a normal runtime refusal or an arbitrary sampling cutoff.

No arbitrary label cap turns a search failure into "unaffordable" or "exhausted."
Malformed inputs return a field-specific contract error.
Unproved profiles return a design/proof limitation, not a false prefix-stop or affordability result.
An operational timeout or memory limit returns an explicit resource error with no admitted setup.
Normal supported runtime inputs cannot rely on these refusals as a substitute for the missing engineering proof.

## 6. Protected selection and final admission

The phase algorithm in this section is the Fundamental candidate under the later amendment.
Typed custody, placement, and completion interfaces remain shared.
It does not export Fundamental surplus-Pawn or elimination rules into Legacy.
Legacy retains the October 21 mode-qualified prefix and finite-pass requirements until its own exact compiler and scheduler receive acceptance.
Its signed-release domain cannot use Fundamental's first-failure shortcut as proof of a longest affordable prefix.
This document does not claim one executable schedule for both modes.

### 6.1 Reconciled construction

1. Validate the immutable capture and the mode-qualified row compiler.
2. Establish dedicated obligations and construct the preferred table without affordability.
3. Derive the retained attainable composition requirement and feasible Castler target before optional spending.
4. Visit preferred entries in order, with inclusive source-qualified affordability and a coupled completion guard.
5. Accept an entry only with a valid post-purchase witness.
6. At the first inadmissible draw, consume its position and freeze the already accepted prefix.
7. Construct the optional operation pool for the current state.
8. Filter and draw through the later elimination/remapping scheduler.
9. Accept a purchase only with a valid post-purchase witness that preserves the promised prefix.
10. Repeat while the finite state-local pool contains an admissible purchase.
11. Finalize the last saved completion witness.
12. Validate the capture binding and the complete roster before admission.

Step 6 treats a completion-blocked preferred draw like an unaffordable draw.
This is a candidate phase-transition default, not another user selection.
It preserves the prefix without spending mandatory resources.
It does not skip that entry and call later preferred entries a longer prefix.

The algorithm does not search longer prefixes for a later release that repairs the first failure.
Signed release remains legal only inside an admitted atomic transaction under its actual source law.
The former descending-prefix scan is historical, not this candidate's executable rule.

If the initial declared attainable obligations contradict the capture, report the inconsistent obligation and its sources.
Do not silently replace the requirement with zero.
Legitimate retained shortages need an explicit valid shortage witness.

### 6.2 Candidate elimination and binding defaults

The operation pool contains permitted existing-slot assignments, eligible actor upgrades, and funded Pawn births.
A filled received-slot table does not stop the latter two operations.
Upgrades can replace accepted optional pieces when the retained operation law permits that replacement.
Otherwise permitted improvements can also replace promised actors' concrete members while preserving their promises.
Do not treat prefix membership as an inherited hard ban.
Do not treat a larger family or price as inherited permission.

Candidate representation: retain the original promise separately from the current actor entry.
The promise records its beneficiary and every credited role, composition, placement, resource, and certified-access predicate.
It explicitly carries required Castler support and the Location predicate that consumes that support.
Concrete-member identity is not an unconditional preservation predicate.
An accepted upgrade preserves the original promise rather than weakening or recreating it.
The witness must satisfy all credited predicates in the same realized setup.
Ordinary material or a higher family cannot compensate for lost required Castler support.
Exact upgrade-source enumeration and predicate encoding remain incomplete engineering, not another user-permission question.

For each destination member, enumerate allowed operation kinds, canonical source-actor IDs, and compatible squares.
Candidate operation-kind tie order is existing-slot assignment, optional upgrade, then Pawn birth.
This tie order resolves multiple bindings for the same member, not family preference weights.
Try the bindings in that order.
Retain the first binding with lawful funding and a successful coupled completion witness.
This ordering needs fixture acceptance, especially when an upgrade competes with a birth.

Remove a concrete member from the current state's draw pool only after every permitted binding fails.
Remove its family only when no eligible member has a valid binding.
Thus a role-blocked Rook cannot remove a family that still contains a feasible alternative.
Standalone affordability is necessary but not sufficient for a surviving member.
A proof/resource limitation aborts construction explicitly. It is not a failed binding or an elimination.

Draw the family and member from the surviving pools in canonical order with their retained preferences.
Use the exact integer weighted-choice primitive from section 3, remapped to those pools.
Do not consume old fixed-tail occurrences or keep their permanent-skip set.
Uncommitted completion entries can change to a different witness after each purchase.
They cannot become funds for the same purchase twice.

Candidate pruning scope is one accepted-state epoch.
After a purchase, rebuild eligibility because actual funds, actor roles, and available capacity changed.
A previously blocked alternative can then become eligible.
Without a purchase or another admitted state change, do not rebuild or retry the same empty pool.
This is deterministic reevaluation after progress, not a retry loop or a seed reset.
Permanent elimination versus state-local elimination remains an unaccepted engineering default.

### 6.3 Counters and atomicity

The later authority requires failed draws to consume their positions and counters never to reset.
Candidate implementation: one monotonically increasing `proposal_ordinal` spans preferred and optional actual draws.
Each optional ordinal owns separate family/member namespaces.
Each actual draw increments the ordinal once, including a guard-rejected draw and a sole choice.
Optional remapping uses the next ordinal, not zero.
Deterministic pool enumeration, completion trials, binding alternatives, and witness reconstruction consume no draw ordinal.

The precise family/type draw alignment is still proposed.
The primitive's local counter zero inside a fresh ordinal namespace is not a restart of the global draw sequence.
There is no random retry to obtain a different result for an unchanged state.

A rejected purchase restores all funding Parts, actors, squares, and historical rights.
Its actual draw ordinal remains consumed.
A rejected feasibility probe consumes neither the ordinal nor those resources.
Admission records distinguish these events so rollback cannot accidentally redraw a failed choice.
The saved witness is data in the pure module, not mutations to a live Game.

### 6.4 Finite stopping and unresolved operation law

Let `b` bound lawful births and `u` bound permitted upgrades per eligible actor.
Let `a` count initial eligible actors, including `s` actors that still need their one initial assignment.
Promised actors enter this domain when the permitted improvement preserves every required predicate, including Castler support.
With no actor deletion and an acyclic upgrade relation, at most `s+b+(a+b)*u` optional purchases occur.
This is a conditional proof, not permission to invent an upgrade relation.
Paid improvements must have an accepted strict progress measure or an exact nonrenewable right.
Equal-price substitutions and downgrade/refund cycles cannot enter that proof silently.

For one accepted-state epoch, let `r` be its finite operation/member/source/square bindings.
The candidate enumerates at most `r` guarded bindings before a draw or a terminal empty pool.
The total is bounded by the purchase bound plus one terminal epoch, each with its own finite `r`.
The bound includes actual square capacity and funded-birth bounds, not just received-slot count.

Stop normally only when the complete supported state-local pool is empty.
Publish its reason classes: source restriction, price, incompatible square, protected prefix, or failed mandatory completion.
Do not claim an optimal roster, maximum material use, or longest jointly affordable prefix.
Legacy historical exceptions and any permitted cyclic replacement relation still need their own finite argument.
Missing proofs remain engineering gaps, not additional refusal policy.

### 6.5 Final admission

Final activation consumes the witness's active/reserve partition.
It cannot rerun the old material-first activation selector.
Presentation can change coordinates only within an accepted legal equivalence that preserves promised placement predicates.
Forwardness and Chaos cannot move a promised Castler off its protected home rank or invalidate a Pocket-support certificate.
An unavailable presentation refinement leaves the admitted semantic placement intact, not a different piece.

### 6.6 Conservative routes and exact-state caching

The user selected a conservative guarantee across permitted routes and lawful Castler cases.
The [current authority record](engineering-closure.md#conservative-route-intent-and-open-proof) preserves the exact answer and its limits.
This selects neither a mutable stepping pointer nor full-domain enumeration.
The following mechanism and evidence are attributed to the existing allocation and generator children through the coordinator's October 26 handoff.
They are not a new source investigation or additions to the frozen reports.

**Proposed exact cache:** `A[I,g,z,m,k]` must equal fresh deterministic allocation for that complete normalized capture.

| Key | Meaning |
| --- | --- |
| `I` | Mode, algorithm, source law, profile, catalog, seeds, policy, and world identity |
| `g` | Prepared geometry |
| `z` | All non-Material effective inputs and actual ordered Pocket choices |
| `m` | Effective Material count |
| `k` | A genuinely lawful, feasible Castler case, consistent with the other captured inputs |

Queries use state counts, not a world-global mutable cursor or receipt chronology.
Raw excess changes that leave effective inputs unchanged must leave projection unchanged.
Removal and copied-state queries must equal fresh allocation.
The generator child reports that CollectionState copies share the world projector.
Direct count helpers and temporary precollection during copy make hook-only cache invalidation unsafe.
Cache equivalence across these paths remains an engineering obligation.

**Reported stepping counterexample:** synthetic tiny-B has one ordinary funded100 actor and preferred Minor180 with extra cost80.
Under an illustrative improvement-before-birth order, extra40 gives Pawn100 plus Checker40.
Extra75 instead gives BetterPawn175 alone.
Both captures have preferred prefix length0, but retaining the earlier birth blocks the later upgrade.
Prefix-only stepping therefore cannot establish exact replay.
Optional tail/elimination must recompute from a canonical checkpoint, or use an exact-key full snapshot.
Reported DP/oracle checks of separate lawful Castler0/1/2 captures do not cover all feasible cases for one inventory.

**Conservative target specification:**

```text
F(w) = B + min(S, floor(B/2), 300*C)
D_I(x,l) = permitted future normalized captures extending x,
           with lawful protection cases and relevant board/Pocket outcomes for Location l
L_M(x,l) = min { F(w) : w in D_I(x,l) }
```

This defines the target, not an instruction to enumerate its whole domain.
Constructive conservative floors remain permitted.
Sound domain coverage and nested domains `D_I(y,l) subset D_I(x,l)` under extensions establish the monotonicity argument.
A Material suffix with a minimum over `k` covers only those axes.
Other inputs require preservation proof or explicit domain coverage.
Castler cases are not unconditionally `0..2`.
Excluding an infeasible case requires complete proof, not a timeout or failed heuristic.
Castler Locations still require separate actual capability, rights, and compatible placement.
A material minimum cannot grant them.
Only boards relevant to the Location enter its domain, not an unconditional minimum across every board.

**Reported current-v3 counterexample, not successor evidence:** ChecksMate `0772bac`, seed7, Fundamental8x8, Chessmen20, Material30:

| Consul count | Family material at Castler0/1/2 | Minimum |
| ---: | --- | ---: |
| 0 | `[11840,11425,11425]` | 11425 |
| 1 | `[11680,11265,10850]` | 10850 |

The minimum drops by 575 with unchanged Material after one Consul.
This shows why omitting the Consul placement axis needs proof.
It does not show the same loss for successor concrete `B+H`.
The existing Fundamental suffix already covers four axes.
Archipelago explores hypothetical inventories, but does not repair a nonmonotone local allocation rule.
Receipt-history simulation supplies no substitute for the required domain proof.
Supported-domain coverage, useful bounds, cache equivalence, and practical costs remain open.

## 7. Discriminating acceptance cases

These are specified expectations, not executed fixtures.
Synthetic amounts and tiny residual-board cells isolate mechanisms.
They do not replace the full paired ContractV4/ProjectionV4 capture fixtures.
Unless stated otherwise, cases use the Fundamental candidate with dedicated obligations already satisfied and no historical exception.
LEDGER is a Legacy funding case. POCKET and GEOMETRY express shared resource laws.
EQ, BASE, and QUALITY use the retained funded-value profile explicitly.
They do not settle its mapping to the later concrete-MG price profile.

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
| ELIMINATE | Surplus7. Family A has only member X:9. Family B has Y:3 and Z:5. Both B members have a guarded binding. | Remove X and A. Remap the next seeded family/type draw to B and its surviving members. Do not assert the old fixed-tail order or redraw the failed position. |
| BIRTH | All received slots are filled. Lawful surplus80 remains. Two compatible squares and permitted birth debit40. | A minimum-not-maximum allocator can fund two additional Pawns if completion stays valid. No received-token count increases. A slot-bound tail cannot satisfy this case. |
| BIRTH-GUARD | The same birth opportunities, but funds120 must preserve a mandatory debit80. | Admit one Pawn40 and retain80. Reject a second Pawn40 despite standalone affordability. If a birth consumes mandatory placement, reject it even before this monetary limit. |
| UPGRADE | An unpromised actor has recorded refundable debit100. A lawful replacement needs150. Funds80 include a mandatory40 obligation. | Atomic net debit50 is standalone affordable but fails completion. A permitted alternative with net debit40 can pass equality. These are synthetic charges, not a new refund rule. |
| PREFIX-UPGRADE | Every received actor belongs to the prefix. A permitted BetterPawn improvement uses genuine free Material. Funding, counts, credited roles, placement, certified access, and required Castler support remain satisfied. | Admit the improvement with its coupled witness. Preserve the original promise and actor affiliation while changing its concrete member. Do not require an unpromised actor or exact-piece identity. |
| PREFIX-CASTLER | A promised actor supplies the only required Castler support for a Location. A proposed higher-value upgrade loses that role or compatible placement without a lawful replacement witness. | Reject despite standalone affordability or increased material. Preserve the Location's Castler requirement explicitly. A companion positive fixture must show every required predicate and actual compatible support, not only a family label. |
| CASTLER-UPGRADE | A naturally selected optional Castler fulfills target1. A proposed upgrade removes its qualifying role or home placement. | Accept only if the same post-purchase witness lawfully restores the target elsewhere. Otherwise reject, even with sufficient ordinary funds. Do not retract the upgrade inside its witness. |
| COUNTER | An actual preferred draw at ordinal k fails. Completion binding probes run before the next optional draw. | The optional draw uses k+1. Probes consume no ordinal. Failed purchase rollback restores resources, not k. Exact family/type stream alignment still needs vectors. |
| SLOT | Same legal choices, sources, geometry, and mandatory occupancy. Append one exchangeable ordinary token. | Existing preferred entries, priorities, and squares are identical. The new entry follows them. A changed source-role restriction does not satisfy this case's premise. |
| SIGN | The next preferred entry fails. A later entry could release credit through an independently lawful operation. | Stop the prefix at the failed draw. Do not accept a longer prefix by searching the future release. Optional operations still use lawful signed transactions, not a universal zero clamp. |
| LEDGER | Legacy sources300 and485 fund concrete500. Another eligible residual is-200. | Preserve source identity and one beneficiary. Residual285 nets to85, not285. No reserve or dedicated account enters the net. |
| LIMIT | A completion profile lacks a valid interchangeability/source proof, or requires uncovered surplus births. | Return a proof limitation. Do not prune a family or claim an impossible target, zero access, or semantic exhaustion. |
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
| Authority/Common | The relayed purchase-guarantee decision and explicit candidate defaults | Integrate the durable authority receipt. Keep price/refund mapping and unaccepted engineering defaults distinct. This document does not edit the ADRs or closure register. |
| A22/P1 | Typed final rows, funded births/upgrades, stable preferred order, and one witness | Specify exact remapping/counters and complete source/row compilers. Prove practical completion and finite operation bounds for every admitted profile. |
| CV01/fixture owners | Section7 plus exact candidate namespaces and source revision | Supply independent full captures, pins, PRF vectors, custody traces, active squares, and negative/restoration pairs in both languages. |
| Conservative-access/PM | Exact witness resources, covered assumptions, and no optional entitlement | Prove useful universal lower bounds over covered receipts, eligibility, geometry, and actual Pocket choices. Prefix length is insufficient. |
| Generation lifecycle/G2 | Immutable input/result binding and classified non-success | Bind before the earliest dependent query. Preserve starts, links, copies, retained plando, frozen prices, and real upstream accessibility acceptance. |
| ChecksMate consumer | Exact namespace/encoding proposal, row semantics, and required fixture outputs | Join the bounded generator report with accepted allocator predicates. This child did not investigate cross-repository source. |
| Coordinator/D03 | This candidate and the explicit remaining proof gates | Integrate accepted results only. Do not infer implementation, native, package, or release readiness. |

The user-level guarantee reconciliation is resolved.
It is not a remaining scheduler-versus-guarantees interview.
Permission for improvements to promised actors is also resolved, with explicit Castler protection for its Location.
No further user decision is required on exact-piece freezing.
The exact upgrade-source domain and predicate encoding still need engineering acceptance.
The coordinator must obtain acceptance for observable engineering defaults before freezing them.
Those defaults include variety-count scope, completion ties, preferred squares, guard-failure phase transition, state-local elimination, and exact draw alignment.
If the local completion probe fails practical bounds, the next decision concerns a narrower exact mechanism, not weaker guarantees.

The [generator consumer report](research/generator-guarantee-closure.md) preserves its source revisions and bounded evidence.
Its bounded metric and lifecycle seams remain inputs, not duplicated proof here.
PM must quantify over all permitted promise-preserving upgrades, not only the original concrete members.
It must keep semantic E, concrete BOARD B, and supported-Pocket credit distinct.
The report records resolved prefix-upgrade permission and the selected Fundamental `B + H` material domain.
Useful conservative bounds and extension-closure proofs remain open.
G2 must bind the access contract before its first dependent query, separately from the later final supply census.

The [tiny completion evidence](research/tiny-completion-evidence.md) records 6,840 DP/oracle comparisons with no discrepancy in its corrected tiny profile.
It covers synthetic stage prices, prefix upgrades, and funded births, not the complete supported compiler or a production allocator.
Omitting B from state equivalence fails with one actor, Checker40/Pawn100 alternatives, and access threshold41.
The corrected key retains B. A replacement dominance rule requires its own future-equivalence proof.
Full-domain practicality, PM extension certification, and LC/integration acceptance remain open.
The [current frontier](engineering-closure.md#next-design-steps) gives the remaining design order without creating production tickets.

### Concrete refund disposition

This candidate requires no new refund exception.
Its default is to retain existing source-qualified release rules and refuse unselected releases.
For example, a funded100 Checker40 does not expose60 for a surplus Pawn purchase.
An optional actor with a recorded refundable extra50 can release only that50 under an already authorized operation.
Neither its protected base nor a dedicated Castler account becomes ordinary funds.

If later concrete pricing requires the first behavior, the deciding case is explicit:
"Can the protected60 difference from funded100/Checker40 fund a new Pawn40?"
The current recommendation is no, unless the user explicitly changes the base-protection law.
This is a potential policy exception, not a prerequisite for guarded surplus births from genuine free Material.
Do not ask for a generic refund policy when no concrete operation requires an exception.

The source compiler still needs special attention for Legacy shared bundles and signed release cases.
The qualified extension proof must identify which source groups are genuinely exchangeable.
The actual price snapshot and Castler liability mapping remain pinned-input prerequisites.
These limits prevent this candidate from claiming engineering closure.

This deliverable changes documentation only.
Only the stated primitive-vector calculation ran.
No successor model, fixture suite, native allocator, generator, or benchmark ran as part of its preparation.
