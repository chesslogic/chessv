# Pre-S0 engineering closure register

Status: Allocation/access engineering open. CV02 design/reference accepted. No S0 readiness or production execution approval.
Reconciliation date: 2026-10-26

Revision: current authority projection over the preserved closure-r2-20261001 proposal.
Authority: ADRs 0001-0024 under the clause-level reconciliation in ADR0024.

This register belongs to the existing implementation-design effort.
It keeps proof obligations out of the product decision record.
It does not duplicate the settings matrix or select new gameplay policy.
The [assessment](assessment.md) owns source evidence.
The [specification](spec.md) owns implementation packages.
The [contract inventory](contracts.md) owns declarations and visibility.

## Current authority and frontier

This section controls the current status and applicability of the older sections.
It records accepted behavior, bounded evidence, and unresolved work separately.
It does not approve a new algorithm or close integrated engineering acceptance.

### Decision authority

The October 21 user confirmation selects the guarantee-first contract.
The original Q19 tool result records: "User selected: Confirm this understanding; consolidate the design only".
The proposal was assistant-authored and explicitly accepted, not an unselected recommendation.

| Source | Stable revision and scope |
| --- | --- |
| Original Q19 | Session `83a50a4c-571c-4935-8202-13c0e56c1090`, `events.jsonl`, call `call_L9SRn9gxxIHHqMkifV43CYQx`. Answer event `6b8527a0-2b57-4218-892b-d9234cc41043`, 2026-10-21T07:06:10.882Z |
| Consolidated successor | Session `25c7b461-3d4d-419f-961a-888fe5699cef`, `files\common-guarantee-first-allocation-successor-v2.md`, sections 2-9. SHA256 `8a10074c2dc46a1752dc27ebb8ef7e2b1a35efb3b9b5934f77f7c55a0e037a9e` |
| Complete CV02 acceptance | Same session, `files\common-cv02-pres0-composition-acceptance-v1.md`, sections 1-5. SHA256 `a4a59bdd0d6cef943c7eb73fe28edfc87aea04c6854520c548b86866af9f4364` |
| Later local architecture | [Seeded allocation plan](plan/design-seeded-army-allocation-1.md), commit `12562dc`. Confirmed behavior remains distinct from proposed helper construction |
| Later Fundamental decisions | [Board-value plan](../board-aware-material-values/spec.md), commit `f3e9f3d`, D3/D5/D6. Original user events in session `2181993b-955f-43a7-b1fb-228e6df19b38` establish the bounded amendments |
| Guarantee preservation | User decision relayed on 2026-10-25 by coordinator session `78444ebb-4f91-4995-acfc-d233bf1fcfb6`, call `call_HaAceB0tYWrxtcDBRmkFu8Qj`. Later behavior retains the promised prefix and feasible mandatory completion |
| Prefix upgrades | User clarification relayed on 2026-10-26 by the same coordinator, call `call_HKDCNcrr28MGXL0jGEM3QChY`. Permitted surplus-funded prefix upgrades preserve every guarantee, explicitly including Castlers needed for their Location |
| Fundamental access valuation | User decision relayed on 2026-10-26 by the same coordinator, call `call_eVkiWLKJDtMiWP3zuBjWVAhD`. New Fundamental Location access uses concrete board-specific player values |
| Concrete selection and pricing | User decision relayed on 2026-10-26 by the same coordinator, call `call_5fTtHRu8gB18gUxfcRxGvWzC`. Resolve concrete identities before prepared-board price lookup. Family labels are selection preferences, not spending prices |

Session artifacts resolve under `C:\Users\aaedi\.copilot\session-state`.
The current decisions and gates are recorded here and in ADR0024.
The source paths supply provenance, not an obligation to reconstruct history before implementation.
Original artifacts and their historical permissions remain unchanged.
Their old PR wording does not override the current no-PR delivery instruction.

### Retained and superseded rules

| Rule | Current disposition | Consumer |
| --- | --- | --- |
| Runtime and generator | Both consume the guaranteed final-piece prefix with retained Legacy and Fundamental funding. Later Fundamental surplus can add derived Pawns without receipts | A22/P1/G2 |
| Affordability | Inclusive `>=`, subject to actual sources, lawful releases, obligations, and placement | AL01/Common |
| Stable series | Chronology-independent world/player priorities. Material-only changes preserve order under unchanged legal inputs. Ordinary-slot growth extends assignments under unchanged legal choices | AL01-choice-series |
| Prefix and tail | Later Fundamental elimination, upgrades, and surplus remain permitted. Every purchase preserves the promised prefix and feasible mandatory completion. Standalone affordability is insufficient | AL01-04/P1 |
| Prefix identity | Permitted surplus-funded upgrades can change a promised actor's concrete type. Required funding, counts, roles, placement, certified access, and Location-required Castlers must survive | AL01-04/P1/PM |
| Fundamental material metric | For new worlds, `L_M` must be sound against concrete `B + H`, not nominal `E + H`. Pocket support/credit, protected funding, and Legacy are unchanged | PM/G2/P1 |
| Castlers | Natural selections count. Protect eligible funds and compatible capacity for the feasible shortfall, not a forced Castler-first phase | AL02-04 |
| Composition and reserves | Retain supported T/N guarantees, Pawn-source protection, dedicated actors, and active/reserve ownership. Ordinary promises must be active | AL02/CV01 |
| Later changes | Optional pieces can change lawfully between setups. No active-match mutation or loss of certified access | PM/LC/P1 |
| Old scheduler | Child-tail replacement rounds, general recovery graphs, Historical comparators, and multiworld trial drivers are not successor prerequisites | All allocation owners |
| Old exact defaults | September namespaces, pooled grant equations, caps, presets, and suffix-envelope implementation are not silently adopted by the successor | EP/NM/Common |
| Independent safeguards | Strict bindings, frozen costs, source integrity, supported geometry, CPU policy, and reporting boundaries remain | Existing owners |

Grant-backed Fundamental ordinary units require unique effective Chessmen tokens.
The later surplus decision permits derived Pawns without extra received tokens.
Additional royals require their distinct Consul tokens.
Material and a Castler application cannot replace those tokens.
The retained [shared contract](../large-board-enemy-armies/contracts/shared-data-contract-v4.md#66-fundamental-paid-funding-and-source-preserving-ledger)
does not grant a free 500-point Castler unit.

Funded base100 remains protected for ordinary concrete Pawn40/85/100 choices.
The price difference is not spendable surplus.
Paid quality releases only its funded extra under its real eligibility rules.
Legacy expected values, concrete debits, signed surplus, and historical allowances remain separate.
Pocket credit is not spendable allocation funding.

The shared contract labels its exact ledger equations candidate invariants.
Its locked-Castler decomposition preserves a 500 initial Material charge and a later 200 Jack increment, including the separate 100 surcharge.
This is not ADR0024's historical `N=S+K` or free 500-total grant model.
The successor still needs exact slot/application, payer, release, and placement records.
Natural selection does not waive those liabilities or double-charge an already funded application.

### Current engineering gates

| Gate | Status and remaining result | Owner and consumer |
| --- | --- | --- |
| EP/NM and series | Open successor mapping. Exact roots, counters, canonical members/slots, policy fields, defaults, caps, and shared vectors | Common with AL01-choice-series, C1/A22 |
| AL01 / typed funding | Open. Mode-qualified Sources, Parts, debits, releases, tokens, protected bases, dedicated accounts, and Castler applications | A22/Common, P1 |
| AL02-04 / coupled completion | Open. Constructive funding, T/N, Castler shortfall, actual active placement, reserves, and supported Pockets in one realization | A22/P1, PM/G2 |
| PM01/02 | Open. Complete covered domain and useful conservative quantities across permitted extensions. Sampled extras or a zero-only bound do not suffice | A22/P1 with G2 acceptance |
| LC / LC08 | Open. Inputs bound before the first dependent query, starts/links/plando/copies, freeze, original-world restoration, and actual upstream accessibility acceptance | G2/Common |
| CV01 | Open successor-specific integration. Independent expected fixtures, coupled realization, strict bindings, and owner handoffs | Common/A22/P1/G2 |
| CV02 / TX / TR / CS | Accepted at the complete pre-S0 design/reference threshold. Native private-state, reset/history, GUI/library, and separate-assembly conformance remain later | E1/R1/H1/U1/N1/T1 |
| D03 | Open integrated acceptance. Consume AL04, PM02, LC08, CV01, accepted CV02, and existing D01/D02 results | Common/coordinator |
| RI | Retained nonblocking boundary. Raw parsers reject duplicate keys. Stock decoded objects receive exact resulting-object validation | C1/N1/I1 |
| S0 and native/release | Later explicit authorization, declarations, consumer acceptance, actual packages, and release evidence remain separate | Coordinator and existing package owners |

Accepted Intelligence, post-terminal, and CV02 results are not reopened by absent allocation proof.
The CV02 receipt covers the complete combined criterion, not only six corrections or a count of green methods.
Its native exclusions remain mandatory later work.
The October 1 table in section 1 is historical and does not re-block that accepted design gate.

Archipelago retains fill, locality, plando, collection traversal, and core accessibility.
ChessV/ChecksMate must supply sound local rules and correctly timed immutable inputs.
An unspecified completion oracle or failed greedy construction does not prove completeness.
The October 23 helper proposal needs its own bounded completeness argument and independent expected fixtures.

The [allocation engineering candidate](allocation-engineering-candidate.md) is **PROPOSED engineering**.
Its exact records, algorithms, and fixtures are not an accepted algorithm or completed proof.
It consumes the resolved prefix-upgrade authority without closing the remaining metric, extension, or integration gates.

### Durable generator evidence

The [generator report](research/generator-guarantee-closure.md) is research and proposed engineering acceptance, not production conformance or a full proof.
Its [probe](research/generator-guarantee-probe.py) and [recorded results](research/generator-guarantee-probe-results.json) retain their original evidence scope.
The report records resolved prefix-upgrade permission and the selected Fundamental `B + H` domain.
The finite arithmetic example gives 1762 for `B + H`, versus the semantic comparison 1987.
Its nine witnesses assume funding, placement, and future preservation. They do not prove those obligations or useful general bounds.
Extension coverage, metric proof, and lifecycle acceptance remain open.

These files are exact copies from session `c2cee554-0e0d-4a73-8425-eced3c4a7225`, under `files`.
Source and destination byte counts and SHA256 values matched at persistence.
Scoped Git attributes preserve the three artifacts' exact bytes, including their original line endings.
The later concrete-selection clarification is recorded separately, without modifying this frozen report.

| Artifact | Bytes | SHA256 |
| --- | ---: | --- |
| `generator-guarantee-closure.md` | 43673 | `047a607780d20a5c5fb45195159f6a12f1d1c608e5c29391695cb04a8b43885a` |
| `generator-guarantee-probe.py` | 3782 | `6d2420ee3566e6f8acaf4c31e07e0e6d2f27fe383efa1469eb5c2553cdc42d65` |
| `generator-guarantee-probe-results.json` | 827 | `e1b1297f7f382203ea28afd2666dad01b5ad4af860a7ace3b3f0ff800dc7047a` |

### Later authority and resolved preservation: board-aware prices

The board-value work scopes its redesign to Fundamental, not Legacy.
Its `needs-triage` status does not erase later direct author answers.
The following original events are in session `2181993b-955f-43a7-b1fb-228e6df19b38`, `events.jsonl`.
October 24 UTC timestamps correspond to October 23 local dates in the plan.

| Source | Direct decision | Disposition |
| --- | --- | --- |
| Call `call_6HTPBD4EwkKMYlY094OJfbCf`, 2026-10-21T08:04:41.408Z | Keep earned credits fixed and vary piece costs by board | Concrete pricing direction, not permission to pool protected funds |
| User event `0bfeca7b-9e7e-4957-aa4a-f83392a7e918`, 2026-10-24T00:25:43.726Z | Switch from exhaustive prefix to pool elimination after the first unaffordable concrete draw | Later Fundamental selection direction. Exact completion/counter composition remains open |
| Calls `call_qzRlcWJoFQuLzIXvFtrk1dkr` and `call_vKM0YNNFao6Mz1IlUCLsVI5b`, 2026-10-24T00:35-00:39Z | Select affordable types and filter families as well as types | Explicit answers, not inferred defaults |
| User event `6f6b540d-bb3a-4cf4-9bcf-286b2fe36329`, 2026-10-24T06:16:39.912Z | Chessmen are a minimum, not a maximum. Satisfy the prefix, then use remaining material for upgrades or additional Pawns in the player's preference order | Replaces a global Fundamental slot ceiling. Derived Pawns are not received Chessmen |
| Same event | Spending is mostly one-way, but the user does not understand the proposed refunds | Refund exceptions remain unanswered. No blanket prohibition or new refund right |
| User event `920a4991-28a1-47eb-96d8-0ade40977d9b`, 2026-10-24T06:05:27.099Z | A prefix acquired on one board need not remain indefinitely | Consistent with geometry-specific reconstruction. Access proof remains required |

The later surplus decision does not change token ownership for grant-backed units.
It does not authorize ADR0024's old free Castler grant or refundable cheap-base equations.
The exact use of ordinary versus dedicated funds still needs a typed contract.

The coordinator relayed the user's October 25 answer to the guarantee-preservation question:

> Yes—later allocation behavior must preserve those guarantees (Recommended)

This is a user decision reported by the coordinator, not an independently retrieved original event in this reconciliation.
The question covered every purchase, the promised prefix, and feasible mandatory completion, including Castler funding and placement.
Later Fundamental elimination, upgrades, and surplus Pawns remain permitted.
Chessmen remain a minimum, not an overall army-size maximum.
The allocator rejects a purchase that makes mandatory completion impossible, even when its standalone price is affordable.
The earlier no-replacement wording does not ban the accepted surplus upgrades outright.
Those upgrades still cannot break the promised prefix or mandatory obligations.

On October 26, the coordinator relayed the user's clarification about a permitted BetterPawn upgrade of a promised prefix actor:

> The only constraint not mentioned in the recommended option is castlers, which are needed for a location. Otherwise yes

The proposed upgrade uses genuine surplus and preserves required funding, counts, roles, placement, and certified access.
The user explicitly adds Castlers needed for their Location.
Thus prefix preservation is not an exact concrete-type freeze.
Permitted prefix-piece upgrades remain subject to all those guarantees.
Higher price or family rank alone is not evidence of Castler capability or access preservation.
The clarification creates no new refund right, price rule, family-dominance relation, or active-match mutation permission.
This records the coordinator's attributed user answer, not an independently retrieved original event.

The behavior-precedence conflict is resolved.
Exact finite enumeration, counter events, upgrade procedures, typed prices/liabilities, and constructive completion proofs remain engineering work.
Lawful refund exceptions remain unanswered.
This answer selects no blanket refund prohibition, new release right, price table, or Castler funding equation.
AL/PM/LC/CV01/D03 remain open, and accepted CV02 design/reference results remain accepted.

### Resolved Fundamental material domain

On October 26, the coordinator relayed this explicit user selection:

> Use concrete board-specific player values for Fundamental Location access (Recommended)

For new Fundamental worlds, the player material domain is `B + H`, not nominal family `E + H`.
`B` is actual active non-primary BOARD material at the prepared board's concrete values.
`H = min(S, floor(B/2), 300*C)` remains unchanged.
`S` remains supported semantic Pocket material, and `C` remains the supported Pocket count.
Pocket support/credit rules, protected funding, and Legacy remain unchanged.

The conservative bound `L_M` must be sound against `B + H` over the covered realization and extension domain.
Actual current `B + H` is not thereby proved monotone.
The decision closes metric-domain policy only.
Useful conservative bounds, extension coverage, profile numeric acceptance, and PM integration remain open.
This records the coordinator's attributed user selection, not an independently retrieved original event.

### Concrete selection and prepared-board prices

The coordinator later relayed this user selection:

> Yes—concrete pieces with board-specific prices, retaining families only for selection preferences (Recommended)

The seeded sequence resolves concrete piece identities before price lookup for the prepared board.
One shared `(profile identity, stage_id, type_id) -> MG/EG` lookup uses the same versioned profile as the game.
Affordability uses concrete MG, not family labels such as 3 or 5 or prescribed per-entry budgets.
Families retain existing classification, semantic Pocket, and accounting roles outside this selection question.
Roles and Castler capability remain identity-backed, not numeric.
No numeric price table, new palette, family reclassification, or refund rule follows.
The user's 300/525 examples are illustrative, not selected prices.
Exact mapping and proof remain engineering obligations.

### Re-entry and delivery

Ready engineering proposals can proceed within their bounded design assignments.
They must distinguish proposed records and algorithms from accepted behavior.
Complete independent fixtures and owner acceptance precede implementation claims.
Documentation reconciliation alone closes none of AL/PM/LC/CV01/D03.

Current delivery uses branch commits, no PR creation, and coordinator integration into `develop` when ready.
Explicit S0 authorization and accepted declarations remain substantive gates.
Historical initial-PR approval language supplies neither a current PR requirement nor permission to bypass those gates.

## 1. Historical October 1 status and re-entry

Sections 1-6 preserve closure-r2 evidence and proposed mechanisms.
The current frontier controls their status and applicability.

The latest reviews demonstrate no new general product-policy blocker.
They reject immediate implementation under the unamended packet.
RI records the stock decode boundary, not a pre-S0 blocker.
The accepted all-design-before-implementation hold remains in force.

Each engineering gate is open until its evidence exists and the design owner accepts it.
The RI row records input scope, not an additional feature or blocking proof gate.
A prose fixture declaration is not a materialized fixture or a passing result.
Closure of documentation does not close an engineering gate.

| Gate | Deciding question and owner | Blocks | Required re-entry evidence |
| --- | --- | --- | --- |
| EP | Which literal effective-policy fields and normalized projection records implement the accepted instructions? D22 design owner, S0/C1/P1/A22 consumers | Consolidated S0 design acceptance and later strict consumers | Final CM1 supplies exact records, retained gem source, bindings, complete supplied synthetic cases and visibility. Final independent recheck/combined acceptance pending. Full C fixture composition remains separate |
| NM | Which existing defaults, aliases, inherited settings and seeds survive normalization? D22/C1/A22, U1 for local settings | Consolidated S0 design acceptance and settings closure | 74-row matrix, twelve actions, enum/default/all-disabled/mirror cases supplied. Final independent recheck/combined acceptance pending; no unselected product-default branch claimed |
| AL | Does the practical finite allocator cover every selected ledger/action predicate? A22 with common owner | S0 readiness, A22 implementation | General OPEN. Bounded models and corrected original-token D do not cover Legacy/coupled reserve/role/cost/quality branches. Legal complete oracle, progress/coverage and stopping evidence in section 3.3 |
| PM | Does the lower-envelope domain cover every permitted realization/prefix with useful certified caps? A22/P1 with G2 acceptance | S0 readiness, final G2/U1/T1 acceptance, C pin | General OPEN. Exact bounded recurrence/certificates cover declared cells only. Full masks/guards/selectors/roles/reserves/Material41..213 and all rule quantities require section 3.3 evidence |
| LC | Does the normalized candidate remain identical through every early query/fill and final core route? G2/A22 integration owner | S0 readiness, G2/T1 acceptance | General OPEN. Literal choice/RNG/provenance contract and bounded real Fill/core cases supplied. Safe isolated trial driver, complete candidate coverage, observer equivalence and early-query/no-op proof remain |
| TX | Does the typed checkpoint and committed-only receipt cover every opted-in participant and failure stage? E1 with S0/R1/H1 | Consolidated S0 design acceptance; later E1/H1 implementation acceptance | Corrected engine independently supported at design threshold. Consolidated ownership review remains; native complete equality, restoration failure and measured cost remain later acceptance |
| TR | Does each accepted source have an exact identity, order and unchanged reward predicate? E1/U1/H1/N1 | Consolidated S0 design acceptance; later attached-match acceptance | Corrected engine independently supported at design threshold. Live toolbar/source predicates preserved. Native Match/H1/N1 admission/history delivery remains later acceptance |
| CS | Do placement hooks, capture admission and the complete reset inventory cover the selected composition? S0/B1/R1/U1/H1 | Consolidated S0 design acceptance; later setup/rule acceptance | Corrected engine independently supported at design threshold. Same captured A account and literal nonempty fixture supplied. Native complete cache/account/reset acceptance follows implementation |
| RI (boundary) | Raw-file parsers reject duplicate keys; stock decoded slot objects receive full exact validation. C1/N1/I1 maintain this scope | Not an S0 blocker | F07 distinguishes raw parser fixtures from decoded-object schema/hash/binding/effective-policy fixtures. No claim of upstream wire-duplicate detection |
| PK | Do released x86/x64 packages and the installed library/runtime satisfy their separate gates? I1/I2/N1 | Release only, not product reselection | Actual package restore AND execution, matching A/L/C and minimum client, actual admission/retirement evidence. Fake executables do not count |

## 2. Funding and allocation proof

The supersession example has three different values.
Minor and Major input credits total 785.
The final Major family's expected value is 485.
A concrete Rook debit of 500 leaves 285 before signed netting.
Combining pieces reduces the count.
It does not erase the consumed source's funding or award the final family's value again.

AL must name exclusive owners for these ledger entries:

| Entry | Required invariant |
| --- | --- |
| Received source credit | Every eligible input grant enters once, including superseded sources |
| Placement or upgrade debit | Each concrete cost belongs to one realization. Expected family material is not another grant |
| Signed Legacy surplus | Net eligible sources before `max(0, netSurplus)`. Negative net does not consume Pawn-item credit |
| Transfer | Credit leaves its old owner before the pawn allocator can spend it |
| Reserve | A grant in reserve cannot also fund an active conversion. Later activation reverses the correct transfer |
| Unallocated credit | The ledger shows residual funds, not silently discarded inventory |
| Historical approximation | Show the fixed 45 allowance, positive-remainder emission and replacement overdraw separately from received funding |
| Pocket material | Reachability credit only. Never a source for the conversion budget |

The accepted rules do not choose a single 100-point excess ceiling.
AL must specify refunds, replacement debits and allowance scope without selecting that rejected ceiling.
The guarantee uses prepared-board targets, not the world endpoint for every stage.
`R = min(availablePawnItems, max(0,T-D-P))` protects 100 per reserved unit.
The shortage remains visible.

D depends on actual deployment.

D counts units attributable to direct non-Pawn item entitlements, not their funding bundle.
Surplus-funded Pawn-conversion emissions never enter D, regardless of their final family.
For the 300+485 bundle, one entitled Major and residual285 leave D=1 after surplus-funded conversion.
P depends on supported final pocket families.

Conversion can change deployment and pocket support.
Therefore receipt counting or a one-pass subtraction is not a proved allocation algorithm.
AL must define bounded evaluation/re-evaluation or candidate search with explicit acceptance and exhaustion.

Fundamental runtime slots come only from owned Chessmen.
Generator pool replanning can replace a planned allocation with another obtainable Chessman.
A runtime downgrade or split cannot create extra Chessmen from Material.
Every candidate preserves hard-disabled actions, actual inventory, entitlements and physical capacity.

Ordinary generation retains its weighted distribution.
Recovery relaxes quantity targets, then weights/order, then preferred families.
Type-limit overflow remains last resort after permitted ordinary alternatives fail.
Recovery selects uniformly with the world seed among progressive permitted realized-piece transformations.
Each transformation is the smallest one that makes the declared progress.
AL must define candidate adjacency, progress, visited-state bounds and exhaustion.
The existing upward upgrade DAG alone does not prove finite downgrade/backtrack recovery.
A failed candidate cannot retry unchanged forever.

F16 must include the historical seven-protected/gets-six source counterexample.
Every accepted ordinary draw must preserve feasible completion of seven protected units.
This is not permission to restore the retired Pool preset.
The source trace is not an executed generation failure.

### Closure-r2 quality authority and pending proof

The user selected these changes during closure-r2.
Priority `0` is inactive in ordinary allocation and recoverable during weights/order relaxation.
Priority `-1` is a hard ban.
Fundamental better-pawn/pool-pawn-upgrade spends Material above the currently funded Pawn value.

The initial funded value is 100.
Pawn150 costs 50 extra, and a later Pawn200 costs another 50.
Downgrade returns only released extra Material funding.
These actions create no extra Chessmen slots.

The coordinator's source-preservation instruction is not another user selection.
The current raw-empty/Off Fundamental defaults retain four priority-1/unit-weight gates and disabled paid quality.
Base entitlement stays separate.
Raw-empty default does not mean normalized twelve-action all-`-1`.

No raw-presence extension or automatic quality enablement follows.
The final common proposal records that source-preserved default set and exact received-gem input.
Its independent recheck and combined acceptance remain pending.

The common owner's derived accounting keeps base100 attached to each unlocked owned Chessman.
Ordinary-family extra and paid Pawn-quality extra share the Material debit.
Pawn150 -> Minor300 releases 50 and charges 200, for a net debit of 150.
The reverse transition refunds 150.

Ordinary40/85/100 cannot refund base100.
These are candidate derived invariants, not additional user wording.
Shared data sections 6.5-6.6 define the final proposed records and ledger.
Independent literal/fixture acceptance remains pending.

General practical AL/PM/LC proof is still open.

The generator amendment separates expected family material from concrete/funded value.
A paid Pawn still has expected family value 100.
Its small-domain model is not a general PM proof or native acceptance.
The three-Chessmen/one-Material guarantee-None counterexample remains explicit.

No-quality attains two non-Pawns with expected material 700.
Paid quality can instead attain one non-Pawn with expected material 500.
The feasibility-witness guard must preserve two attainable non-Pawns where the accepted guarantee requires them.
Final common/generator fingerprints and combined review must settle the revised domain and proof limits.

### Closure-r2 finite recovery candidate

The earlier generator packet supports a bounded reference candidate.
The corrected R2 packet narrows its claim to demonstrated domains.
It does not establish full client allocation or native cross-language acceptance.
Unsubmitted generator plan credit differs from actual received runtime funding.

Plan replacement returns its old planning debit before charging the new planned allocation.
Runtime never gains a receipt or Chessman from that refund.

Canonical state includes the prepared stage, effective phase, plan multiplicities/capacity and outstanding literal obligations.
It also includes source/entitlement cohorts, current families, reduced legal paths, placement classes and exclusive ledger ownership.
Final ordered pockets, historical pass marks and immutable post-cap requirements complete the state.
Diagnostic traces and PRF counters are not visited-state coordinates.

Canonical ledger Parts coalesce by source, owner and category.
The inherited castler transition adds a path symbol, not a thirteenth settings action.

Supersession joins disjoint source sets and preserves both credit and entitlement provenance.
A Legacy inverse restores the entire pre-acceptance unit/token/ledger checkpoint.
It cannot retain the merged funded output or resurrect a token through a live refund.
Weak-to-Best and delayed-Sergeant replacement passes each visit a concrete unit once.

The fixed 45 allowance occurs once per invocation, not per retry, candidate or replacement.
Deficits, allowance and explicit overdraw remain separate from received funds.

Ordinary admission requires a forward affordable completion witness after the proposed purchase.
That witness cannot retract the purchase through a recovery refund.
Recovery uses a separate checkpoint oracle.
Paid200 can recover to admitted150 even after direct100->200.

If the selector excludes150, recovery cannot invent that intermediate.
Neither route refunds base100 or creates a slot.

The finite recovery graph excludes every hard-invariant violation.
Reverse traversal computes shortest distance to a spend-complete feasible goal.
A progressive adjacent edge strictly decreases that distance.
The selector chooses uniformly among eligible realized-piece/plan groups, then their smallest progressive edge.

Canonical seeded ties do not reuse ordinary weights.
Bounded rejection sampling preserves exact uniform-word preimages.
Its draw limit returns `proof_limit`, never modulo fallback or false exhaustion.

| Phase | Candidate soft change | Hard boundary |
| --- | --- | --- |
| 0 | Ordinary requested quantities/weights/order/families/type limits | Bans, inventory, entitlements, physical capacity and attainable selected obligations |
| 1 | Quantity preferences/soft ceilings | Literal locks and immutable item maxima |
| 2 | Weights/order variants and soft-zero promotion to positive effective priority | No -1 activation. Capture/world policy remain immutable |
| 3 | Preferred families expand within legal human catalogs | Selector/catalog legality and hard bans |
| 4 | Minimum necessary type overflow after earlier exhaustion | No runtime inventory discard |

Each phase has finite quantity sentinels, at most 12! orders and at most 2^Z soft-zero subsets.
Finite receipt/plan/cohort/catalog domains bound the state graph.
Every canonical state expands once per phase.
Every phase transition occurs once in order.

Only complete exploration can report `exhausted`.
Resource/time limits use the existing protocol error envelope with code `proof_limit` and a nonzero exit.
They cannot produce a success or an exhausted SemanticPlan.

Local runtime recovery belongs in `SemanticPlan.runtime_actions/recovery_phase`.
It does not mutate frozen capture or policy-hash bytes.
Final common literal records and combined review must establish that seam.
Generator recovery publishes recovered effective instructions, not requested mirrors.

## 3. Effective policy, setup capture and metric proof

The generator publishes recovered effective instructions, not requested settings.
Diagnostics keep the request, reasons and rejected draws generator-local.
Slot data contains no hypothetical roster, concrete ChessV piece names or board coordinates.
No mandatory allocation-policy ID or generated-world UUID is selected.

EP defines the semantic C binding, world W binding, projection request and response checks.
A hash of source settings is not enough unless the contract defines their normalized meaning.
Old requested mirrors cannot override the effective object.
A response must identify the same captured input and generation.
EP specifies canonical integers, maps, orders, disabled actions, seeds and enabled armies.

K12 captures one immutable prepared setup before projection or live generation begins.
It includes actual inventory, geometry, effective settings, seeds, local Ignore Castlers and final ordered pocket families.
It also includes a generation token and world/contract binding reference.
The Games-side record cannot depend on APMW.Client types.
The client associates authenticated identity through K6/K9.

Adapters, previews and live ItemGeneration consume the same capture.
A settings or inventory change invalidates all dependent caches and responses.
A stale response refuses adaptation or regenerates the whole capture.
It never combines projection A with current local settings B.
EP/CS must declare the capture moment, cache keys and refusal diagnostics.

For each candidate setup, composition uses current families.
The primary royal provides no composition unit or pocket support.
A global `floor(nonprimaryBoardCount/2)` bounds supported actual pockets.
Non-Pawn pockets take support first, then Pawn-family pockets.
Canonical final pocket order resolves ties.
Unsupported pockets remain present but earn no composition or material credit.

Pocket levels increment the lowest level, with suggested-slot-order ties.
Ordinary limits can overflow until all three pockets reach level four.
Four receipts at ordinary limit one produce levels 2/1/1.
Twelve effective receipts produce 4/4/4.
Further receipts remain received but provide no setup unit or material credit.

Pocket-material credit is `min(S, floor(B/2), 300*C)`.
S is supported semantic pocket material.
B is exact active non-primary BOARD material after allocation.
C is the supported count.
B excludes the primary and every pocket.
This credit is not spendable funding or an exact roster total.

PM must prove each rule-facing metric is no greater than the runtime contribution it represents.
The proof covers actual received-count prefixes, not only maximum obtainable inventory.
It covers every permitted final pocket distribution, not only the suggestion.
It distinguishes semantic expected material, concrete allocation debits, and CPU catalog values.
No CPU material registry supplies the human funding ledger.

The current grant-to-unit floor and unconditional occupied-pocket count lack this proof after conversion.
Primary upgrades must not inflate B.
Small enumeration can support a declared finite domain.
Larger spaces require a stated conservative bound, not an unexplained fallback.
Monotonicity must survive collection, recovery, reserves and geometry changes.
A zero pocket lower bound alone does not prove the remaining board metrics.

### Closure-r2 lower-envelope candidate

For a terminal realization, `n/v` count non-primary board units/current non-Pawns.
`E` is their semantic family material.
`B` is their exact concrete BOARD material.
Supported pocket count/non-Pawn count/material are `C/J/S`.

Pocket credit is `H = min(S,floor(B/2),300*C)`.
Thus total units are `n+C`, total non-Pawns are `v+J`, and rule material includes `E+H`.
The primary enters none of these terms.
An independent primary contribution needs its own certificate.

The bounded generator model uses zero primary contribution.

Fix effective policy `p`, finalized upper inventory `U`, actual prefix `x` and prepared geometry `g`.
Let `Omega(p,y,h)` contain every permitted terminal realization at prefix `y` and available later geometry `h`.
The candidate defines each logic quantity as:

```text
Lq(p,x,g) = min(q(r) for x <= y <= U,
               h available at or after g,
               r in Omega(p,y,h))
```

The set includes every permitted outcome at `x/g`, so each minimum is conservative.
Increasing inventory/geometry shrinks that set, so each minimum is monotone.
Componentwise minima can come from different realizations.
They are bounds, not a synthetic roster.

Complete stage alternatives never mix quantities or special predicates across geometries.

The domain includes reserves, signed Legacy branches, historical approximation, paid quality and every permitted final pocket record.
Within one fixed funded allocation, a permitted concrete-cost floor bounds `B/H`.
That floor cannot replace cost branches before allocation.
Paid quality can change subsequent family allocation even while its Pawn expected value stays 100.

The older interfamily-only corpus cannot certify that domain.

Small domains use exhaustive finite states and suffix minima.
Larger domains require conservative cohort dynamic programming or branch-and-bound certificates.
Each certificate names policy/catalog binding, prefix box, stages and represented quantity.
It must cover the whole upper suffix.

Unexplored states do not use an unproved grant-to-unit floor.
An explicit zero lower bound is mathematically safe but does not establish useful-cap acceptance.

The separate paid corpus covers420 prefixes,8590 eleven-quantity comparisons and8096 concrete comparisons.
Its independent fully enabled unlocked oracle agrees across30 cells/1294 feasible cohorts/135 terminal states.
Full source-role placement, locked castlers, Consuls, captured selectors, Legacy debit predicates and large-domain certificates remain excluded.
These are open PM obligations with A22/P1/G2 owners, not native parity or blanket proof.

The paid corpus owns at most five Chessmen and does not exercise the smallest selected target of eleven.
Its stage loop does not exercise deployment/reserve boundaries.
The corrected R2 recurrence now supplies useful certificates for its declared domain.
A lower-bound overapproximation cannot supply a legal goal-completion witness.

General practical proof and complete domain coverage remain open.

### 3.1. Corrected bounded certificate

R2 uses eight funded tiers for unlocked Fundamental Chessmen.
Its domain is 0..71 owned slots and 0..40 Material items.
All eight interfamily actions and both quality actions are positive and available.
It excludes selective masks, guard-induced early termination, type ceilings, Castlers, Consuls, and exact full-role reserve transitions.

Reserved extras remain exclusively allocated before active selection.
These preconditions require validation at consumption.

| Funded tier | Extra | Expected E | Concrete B floor | Least forward delta |
| --- | ---: | ---: | ---: | ---: |
| Pawn100 | 0 | 100 | 40 | 50 |
| Pawn150 | 50 | 100 | 150 | 50 |
| Pawn200 | 100 | 100 | 200 | 100 |
| Minor300 | 200 | 300 | 250 | 185 |
| Major485 | 385 | 485 | 450 | 215 |
| Jack700 | 600 | 700 | 700 | 200 |
| Queen900 | 800 | 900 | 875 | 400 |
| Amazon1300 | 1200 | 1300 | 1300 | No edge |

Terminality requires the remaining purse to be smaller than every present tier's least permitted forward delta.
The recurrence partitions that purse into seven exact intervals.
Within each interval, admitted terminal tiers stay constant.
Bitsets cover every summed extra for each exact cohort size.

Nonnegative extras justify pruning only amounts beyond the complete budget ceiling.
Pawn/non-Pawn bitset convolution tests each quantity branch exactly.

```text
R_A[0] = bit0
R_A[i+1] = OR_t (R_A[i] << extra(t)/5)
D_q[0,0] = 0
D_q[i+1,s+extra(t)/5] = min(existing, D_q[i,s]+q(t))
```

The exact scalar recurrence refines no-reserve cells separately for non-Pawn count, E, and B.
Component minima do not publish a synthetic jointly realized roster.
For reserves, the certificate relaxes actual role-priority selection to every active subset of the required sizes.
The actual selected subset lies inside that superset under the declared preconditions.

Four active/reserve Pawn/non-Pawn groups use verified integer interval inequalities.
Their rational slopes use denominator4 and numerators `-8,-4,-2,-1,0,1,2,4,8`.
The verifier recomputes coverage, inequalities, scalar minima, and inventory/geometry suffix minima.
It uses no favorable sample roster, floating-point optimizer, or guessed dominance.

The proof covers three final upper boxes: N11/M12, N33/M33, and N71/M40.
It closes inventory axes and the ordered geometry suffix separately.
Pocket bounds minimize across permitted final distributions after board-only bounds.
Unsupported policy/domain/binding returns visible `proof_limit`/unsupported-proof failure.

It cannot reuse these caps, infer a nonzero floor, or claim exhaustive failure.

| Zero-pocket upper cell | Board count | Non-Pawns | E | B floor |
| --- | ---: | ---: | ---: | ---: |
| N11/M12, 6x8 | 11 | 4 | 5285 | 5375 |
| N33/M33, 12x10 | 33 | 9 | 14100 | 15025 |
| N71/M40, 6x8 | 11 | 9 | 3300 | 2750 |
| N71/M40, 10x10 | 29 | 9 | 8700 | 7250 |
| N71/M40, 12x10 | 35 | 9 | 10500 | 8750 |

These cells do not automatically satisfy selected non-Pawn targets5/19.
Named paid150/200 forward constructions demonstrate semantic feasibility without retracting the paid prefix.
They do not prove concrete type/seed witnesses or whole-world acceptance.
A legal witness algorithm must preserve every obligation, not retain a single lower-bound scalar.

### 3.2. Corrected retained-choice and trial contract

G2 retains immutable source/runtime/option/seed bytes and hashes, the accepted unsubmitted plan, and its RNG boundary.
Object keys include creator, hook/stage, and construction ordinal.
Origin distinguishes starts, pools, locks, prefill, link-original/transport/replacement, and plando fabrication/nonpool items.
Raw blocks preserve requesting player, original block/occurrence indexes, count bounds, force, pool mode, target worlds, and literal Location selectors.

Resolved choices preserve initial-sweep reachability, ordered Location keys, pre/post clamps, target draw, zero-block removal, shuffle, and stable execution order.
They also preserve sampled occurrence indexes, chosen pool/fabricated identities, compatible-Location shuffles, placement attempts, and retained locks.
Disposition distinguishes retained, returned pool, dropped nonpool, dropped event, and removed mixed-event block.
MultiWorld and each local RNG state are retained before/after resolution and after each block.

Recovery PRF state remains separate.
Hashes never replace bytes/state required for reconstruction.

The source can query reachability before it draws target counts.
Exact post-plando counts at the barrier are candidate assumptions, not observations.
Signature candidates include retained objects and provenance, not just count vectors.
Sampled/fabricated requests alone do not become receipts.

Returned non-event pool objects count, dropped objects do not.
Mixed-event early continuation cannot silently become a successful return.

Each trial uses a fresh isolated process and the same accepted pre-submission plan.
All participants bind policy/counts/metrics/rules/S before the first dependent query.
The real early resolver and distributor consume natural ordinary RNG.
Actual provenance/signatures must match the assumption.

Mismatch rejects the trial without repricing or item resubmission.
Final replay reproduces accepted RNG/fill signatures and passes the same real core gate.
Only typed mismatch/infeasibility follows retry. Unexpected errors retain their traceback/cause.

The driver defines candidate, operation, per-trial-time, and total-time limits.
Incomplete coverage or a timeout yields `proof_limit`, not exhaustion.
Process isolation must cover module/class caches, options, rules, RNG, handlers, temporary files, and output workers.
External effects require pinned/trial-safe capability.

Fresh MultiWorld objects or shallow rollback do not prove that isolation.
Pre-barrier queries from entrance/generate_basic hooks require explicit refusal or an earlier accepted seam.
The driver and complete compatibility proof remain open.
No unassigned general entrypoint is silently added to the write set.

Ordinary nonparticipants execute no candidate/trial/observer path.
A static participation marker defaults false and is not a user setting.
Mixed-world nonparticipant callbacks are no-op.
Occurrence observation must preserve exact duplicate-occurrence indexes and ordinary RNG advancement.

Actual output, skip_output, and spoiler_only all consume real accessibility without an all-inventory state.
False fails even if beatability succeeds.
This remains a proposed Main/AutoWorld/conditional Fill integration, not an existing ChecksMate override.

### 3.3. Remaining proof checklist and stopping criteria

These obligations stay in the existing register/specification.
Partial delivery creates no new ticket, closes neither D22 nor S0, and does not lift the design-first hold.

| Owned obligation | Consumers and decisive observation | Stop/refusal criterion |
| --- | --- | --- |
| A22/common practical legal allocator | Live ItemGeneration, semantic projector, P1 adapters. Exact admissible adjacency and witness covers Legacy signed ledger/two passes, tokens, coupled D/P/R, paid quality, reserve/role/selector/capacity predicates | Unsupported branch or incomplete legal-domain exploration remains explicit. No false exhaustion or invented grant |
| A22/P1 complete metric certificates, G2 acceptance | Every rule-facing quantity/cap at actual prefixes/later stages/final pockets, including default/selective masks, guarantees, Material41..213, Castlers/Consuls and reserves | Reject incompatible preconditions/bindings or incomplete coverage. Zero-cap fallback and useful bounded cells do not close PM |
| G2 safe retained-plando driver | Main/AutoWorld/conditional Fill, ordinary/mixed worlds, tracker and T1. Finite signature coverage, occurrence/RNG equivalence, isolated replay, earliest-query ordering and strict core routes | Reject unsafe/nonreproducible participants, limits, mismatch, or unexpected errors. No later replan/reprice/shallow rollback |
| C1/P1/I2 full fixture composition | Synthetic A/L/C/S/W admission, exact C descriptor and cross-assembly examples after authorization | Templates/context hashes and the C fragment do not prove complete artifact admission |
| Consolidated design owner | Exact input seals, owner/write-set/DAG/transfers and final independent combined review | Remain artifact-only until exact reviewed application permission. S0 remains not ready while general AL/PM/LC is open |

## 4. Committed attempt and lifecycle proof

At ChessV `ace9362`, MoveList applies pickups/drops before `Game.MoveBeingMade` returns legality.
Normal late `false` can leave state that requires exact unmake.
`Game.MakeMove` throws without that exact rollback.
An early no-match `false` performs no mutation.
Partial exceptions already use bounded pickup/drop rollback with different rule-state uncertainty.

Debug throw mode can skip that rollback.
A drop changes type and MoveCount before its final board operation can throw.
Completed primitive spans alone do not prove restoration.

### 4.1. Evidence and candidate boundary

The closure-r2 engine probe compiles unchanged Base/Games source at `ace9362` into session artifacts.
Its source manifest records the compiled inputs and their hashes.
The probe targets and runs installed .NET/WindowsDesktop 7.0.20 without restore or installation.
The artifact-only reflection checkpoint measures selected fields, not the entire graph or the future APMW composition.

The assessment owns the durable [evidence digest](assessment.md#closure-r2-engine-evidence).

| Characterization | Result and limit |
| --- | --- |
| Exact-index unmake and transient BoardMoveStack entry | Restore board/side/Ply, but Move50 counter arrays and repetition scratch hashes still differ |
| Stateful-rule exception, debug off/on | Debug off restores board but leaves changed rule state. Debug on can leave board and Piece state applied |
| Entered drop onto an occupied square | Debug off restores occupancy/hash but leaves MoveCount changed. Debug on leaves partial application |
| Artifact reflection restore | Restores enumerated ordinary-Chess fields only. No native typed participant, H1, R1, N1 or complete APMW proof |
| Rejected move then real repetition | Later identical real move succeeds. Attempt identity must not use MoveInfo equality |
| Bare repeated LoadFEN | Occupied-square exception and retained prior Win. Not a reset mechanism |
| Repetition draw | Played callbacks occur without MatchFinished. Existing finish event is not terminal authority |
| Material-key reservation | One type/player/slice preserves old indexes through the finite count bound. Not proof for all selected participants |

The coordinator accepted typed checkpoint/accessor ownership as an engineering candidate only.
The corrected engine core has independent TX/TR/CS design support.
Final source-gem alignment and combined ownership acceptance remain separate.
No native gate closes by assertion.

No probe or model result substitutes for future native APMW acceptance.

### 4.2. Exact attempt and absolute restore candidate

Attempt identity is `(GameId, ResetEpoch, AttemptId)`.
Commit identity is `(GameId, ResetEpoch, CommitId)`.
Every committed call advances AttemptId.
Only accepted gameplay advances CommitId.

Undo rewinds neither sequence.

The receipt resolves the root-list revision and exact generated index before mutation.
It also matches Player, Piece identity, source square, current type and descriptor.
It snapshots every pickup/drop value before the list can reset.
The public parameterless Unmake uses a search-selected index that `MakeMove(MoveInfo)` does not establish.

Exact-index unmake identifies a move but does not restore the measured scratch arrays.

| Transition | Candidate contract |
| --- | --- |
| Created -> NoMatch | Abort only. No primitive, unmake, commit, ledger entry or earned event |
| Created -> Resolved | Capture complete typed pre-state before Setup and mutable hooks |
| Resolved -> Applying/Hooks | Record entered primitive/hook and completed spans, including both current MoveMade passes |
| Applying/Hooks -> AppliedRejected | Normal late false after completion/Ply. Restore the exact checkpoint once |
| Applying/Hooks -> PartialException | Preserve original exception and entered stage. Restore absolute fields, not inverse hooks |
| AppliedLegal -> Staged | Stage native history, second MoveMade, rule result and root generation |
| Staged -> Prepared | Freeze complete snapshots/prospective CommitKey/native token. Registered Base participant prepares pure local progress |
| Prepared -> Installed -> Accepted | Install one reversible H1 entry, validate exact receipt, then accept native entry/CommitId once under the same barrier |
| Accepted -> Published | H1 ObserveAccepted delivers only, Terminal second, compatibility/UI observers last |
| Accepted -> CommittedUndo | Reverse the exact top CommitId. Earned journals remain monotonic |

The minimal committed-only MoveList candidate records outcome, stage, exact index and original exception.
Its exception path does not run the old partial rollback.
Game owns one absolute restore.
Existing bool MakeMove overloads and low-level search/generation retain their current caller-owned rollback.

There is no global false-result auto-unmake or search rewrite.

S0 declares public opaque participant/checkpoint interfaces in the existing Base K4 file.
Implementations keep typed state private.
RestoreOnce belongs to one game/epoch/attempt and rejects stale or repeated use.
Restoration assigns absolute fields without rule callbacks or public board mutators.

It restores Pieces, Board, native lists, rules/evaluations/completion, then staged H1/Match state.
Content snapshots include nested arrays, maps, sets and scratch/history entries.
The corrected Base-owned bridge appears in [K4](contracts.md#k4-committed-lifecycle-observations).
H1's public factory returns its internal adapter after complete attachment validation.

Registration binds one Game/epoch and includes H1 in the checkpoint domain.
Prepare does no mutation or I/O.
InstallReversible is the one ledger write before acceptance.
E1 validates Attempt/Commit/NativeHistory before releasing the barrier.

Preacceptance failure restores complete native/H1 state and discards detached prepared values.
Discard neither inverses the ledger nor emits public Undo.
Postacceptance ObserveAccepted publishes pending intent without another installation.
H1 registers no second generic Commit subscriber.

A keyed publication failure retains the accepted prefix.
Its retry repeats pending delivery only.
N1 deduplicates source/Location admission.
Restore/discard failure quarantines uncertain state.

No durable crash recovery follows.
The [state inventory](../large-board-enemy-armies/contracts/royal-lifecycle-fixtures.md#closure-r2-state-and-reset-inventory)
specifies capture, rebuild and preservation.

Only explicitly opted-in attached APMW compositions use strict participant registration.
Every enabled stateful participant must register.
Known stateless participants supply an explicit stateless token.
Missing registration rejects that APMW setup.

Ordinary variants retain their existing setup and move paths.
Neither Rule nor Evaluation gains a silent default checkpoint.

E1 reserves material-hash indexes through `Board.NumSquaresExtended` before root trials.
The reservation uses the existing per-game `PieceType.GetMaterialHashKey` route for each registered type/player/slice.
Frozen geometry/types bound every possible occupancy count.
The bound includes pockets.
Attempts assert unchanged allocation counters.

Reset retains the reserved indexes.
No HashKeys/PieceType/RNG rewrite, shared-key truncation or production reflection hook is selected.
The proposed expanded [writer inventory](spec.md#closure-r2-engine-resource-inventory) must pass review before dispatch.

A failed restore quarantines the match and reports original plus restore errors.
Uncertain state permits no new progress or attempt.
An exception after accepted publication cannot retract admitted work or relabel the move as aborted.
It requires explicit observer diagnostics.

### 4.3. Terminal, controller and continuation matrix

One Game-scoped publisher owns terminal facts.
The accepted source key, not `(result type, winner)` equality, identifies a terminal transition.
A move carries its CommitId.
Claims/adjudication/Death carry explicit operation IDs without fabricated commits.

Undo followed by a new real continuation can create a new terminal source.
History restoration reuses original identities and earns nothing.

| Source | Adapter, order and deduplication | Existing reward predicate |
| --- | --- | --- |
| Rule result, including CPU extinction | Stage during Game.MakeMove. Commit before Terminal. Does not wait for root no-moves | Accepted Win with fixed human winner and reporting eligibility |
| No legal moves | Post-move root -> NoMovesResult, same accepted move source. Extinction takes precedence | Last-King human checkmate qualifies. CPU stalemate/multi-King win does not |
| Controller claim | Match.onResultClaim accepted branch only, explicit claim ID. Retain invalid-claim result kind | Accepted Win with fixed human winner. Other result kinds receive no new award |
| Speculative adjudication | Save provisional result/counters, finish trials, restore, then accept after the relevant real commits | No award from a speculative result |
| Accepted adjudication | Explicit adjudication operation, ordered after its accepted commits | Adjudication is not Win, even with a human winner |
| Received DeathLink | N1 callback rechecks route generation, MatchId and enabled choice when it executes | No stage victory, Capture Everything or goal |
| Transport cleanup | No terminal operation. Retain active Game, H1 and earned intent | None |
| Match Stop/finish/Finished | Consume accepted result source. Local early return or null closure arguments cannot suppress or duplicate it | No reward inferred from Finished |
| Window closure | EndMatch with WindowClosed, release references/subscriptions, no synthetic resignation | None |
| Committed Undo | Exact top CommitId, restore active result/source, retain earned journals | Undo earns nothing |
| Read-only history/result restore | Explicit checkpoint scope, original IDs, complete exit restoration, no earned publication | None |
| Existing live GameForm Undo/replay | Accepted committed continuation, exact H1 pairing and source identities, monotonic N1 journal | Existing earned predicates, not a new viewer |
| Controller replacement/restoration | K5 fires before assignment/AttachGame/subscriptions. N1 permanently suppresses under admission gate | No DeathLink or revived eligibility |
| Initial setup controllers | Fixed sides/binding precede Match.Start. Initial SetPlayer is not takeover | Fresh eligible match |
| Search-only cancellation | No K5 unless the action actually replaces a controller | None |

H1 preserves published victory thresholds through the current stage.
The final goal additionally requires the world endpoint.
Capture Everything independently requires all starting non-Kings and spare King lives captured on that endpoint.
Regicide requires a King capture in an initially multi-King setup.

Neither generic terminal facts nor a remaining Rook can satisfy those capture predicates.

Match currently logs Moves/PGN and notifies the opponent before actual commits.
E1 stages those effects.
It publishes only the accepted prefix of a controller Movement list.
A later failed move restores only its own attempt, not earlier published gameplay.

The rejected suffix is neither logged nor sent.
Provisional adjudicator state restores if the relevant list fails.
This design introduces no new adjudication policy or forced validation mode.

Opted-in public speculative and genuine read-only scopes suppress external UI/compatibility publication.
They retain required internal rule bookkeeping and restore residual scratch state after their paired trial Undo.
An unsuccessful trial emits no successful Undo.
This does not alter low-level search/generation conventions.

GameForm's accepted live Undo/replay toolbar remains ledger-aware committed continuation.
It does not become read-only merely because it displays history.
Corrected engine fixtures must separate that toolbar from genuine internal/view-only operations.

### 4.4. Cold start, captured setup and internal reset

| Phase | Required data/order |
| --- | --- |
| U1 capture | Immutable K12 before preview, projector, live generation or Manager.CreateGame |
| Game variables/starting map | Resolve K1 formation and fixed sides before AddPieceTypes/AddRules |
| Rule/type initialization | Formation/actor metadata exists. Live Piece bindings do not yet exist |
| Board/type/evaluation/postInitialize | Initialize geometry/types/dispatch and BoardMoveStack without requiring Match |
| After piece placement | Protected Game hook binds every CPU Piece before PositionLoaded |
| PositionLoaded | Seed rights, royals and completion. Transfer captured PocketGems before roots, not live singleton replacements |
| After all PositionLoaded calls | Second protected hook validates board/pocket Pieces and freezes complete K2 |
| Root trials | Bindings and bounded hash reservation complete before the first legal generation |
| StartMatch/setup controllers/Started | Associate K6/K9/H1 and next-match AI. Transfer the same captured card account into attached Player/CardDrop before play |

K12 content equality alone cannot admit an old response.
Generation, input hash, stage and every C/W/policy echo must match the current capture.
A-B-A invalidation rejects the first A even when content hashes match.
Cache clearing/cancellation does not revoke a pending waiter by itself.

The adapter receives the accepted capture and realization, never newer singleton settings.
Common-owner literal schema revisions remain pending.
This section does not freeze its earlier draft spellings or allocation oracle.
The corrected packet includes nonempty captured base A4 versus newer live base B9.

The same Game-owned side account supplies roots, Player.Gems/NewGame/AttachGame and CardDrop spending/unmake.
At human side1/turn1/spent0/gained0, A gives5 gems while a separately prepared B gives10.
After a five-gem drop and CPU reply, A gives1 and rejects the second Rook.
Undo restores 5.

Internal post-reply/cold origins restore 1/5 while live received inventory stays 9.
CardDrop uses the actual generation side.
Ordinary paths retain their source behavior.
The source-card probe characterizes the missed live read and cold-side bug, not future native handover/reset acceptance.

Internal reset requires complete formation, fixed sides, bindings, Piece state, rights, capture origin, ledger, result/source and continuation history.
A bare FEN or omitted counters cannot provide that record.
Validate before mutation, revoke old setup work and quiesce search.
Allocate one new ResetEpoch, rebuild the listed state, bind new Pieces, then generate roots.

Emit one Reset and no earned event.
Preserve MatchId, K6/K9, permanent suppression, earned Locations and earned goal.
Only separate BeginMatch creates fresh eligibility.

R1 reviews the ten-rank Checkers stop with both colors and the unchanged eight-rank case.
The opponent-relative-rank-one interpretation is a geometry hypothesis, not a new selected gameplay rule.
Board/StandardAlgebraic G1/G10 and ten-rank XBoard g0/g9 remain separate domains.
CardDrop's `7 - rank` reflection also needs R1 review without changing Range/gem policy.

Army coordinates remain unchanged.

## 5. Freeze, core acceptance and dependency order

Initial `create_items` counts precede upstream start-from-pool and item links.
Recovery and count finalization must account for those later inputs before dependent fill freezes.
Draft policy changes invalidate draft metrics, caps, rules and S together.
The frozen state cannot change after acceptance.
`fill_slot_data` is a pure serializer of that state.
Tracker reconstruction restores it and the original S without newer-code recovery.

The stable generator packet proposes `prepare_generation_context(NormalizedInventoryCandidate)`.
AutoWorld's default callback is a no-op.
Main invokes it after start-from-pool removal/link_items and before early-location resolution, planned blocks and pre_fill.
It cannot create, submit or remove an AP item.

G2 owns the declared Main/AutoWorld seam and conditional Fill receipt-planning seam.
Nonparticipants retain item identity, RNG advancement, plando order and outputs.
Ordinary-world no-op regression is a required new G2 fixture.

The collector counts unique recipient item objects in pools, locks, starts and stable pre-fill accessors.
Group transport items do not duplicate their locked recipient representation.
Raw copies and semantic effective counts remain distinct.
Missing from-pool removal uses actual core-created/precollected objects, not guessed subtraction.

Literal locked-item minima stay unsubmitted planning obligations.

Variable plando requests do not determine retained receipts.
The finite candidate includes a possible retained exogenous receipt vector.
It freezes policy/counts/metrics/caps/rules/S before that candidate's first reachability query.
Actual retained receipts must match exactly after plando and later fill/balancing/finalization.

A mismatch fails without repricing.
Another attempt requires a fresh isolated MultiWorld draft, not shallow rollback of a live filled world.
Exact optional-plando materialization and whole-world regression remain open LC obligations.

Post-fill/finalize hooks cannot reduce accessibility.
`pre_output` cannot change spheres.
An all-inventory roster test is not the filled-world core validator.
The real `MultiWorld.fulfills_accessibility` result belongs to final acceptance.

Existing stage_pre_output precedes Main's output/skip_output/spoiler_only route split.
The candidate calls real `fulfills_accessibility()` there without a supplied all-inventory state.
Existing FillError details propagate.
False fails even if beatability succeeds.

The current output-path check can remain as a second validation.
Failure uses the existing generation failure route, not repricing or a substitute validator.

Stable rule delegates reject resource queries before candidate initialization.
All-state/indirect caches invalidate at freeze.
Serialization and tracker restoration use the original bound object without recovery or newer calibration.

D22 closes engineering design before S0 implementation authorization.
S0 then needs the user's initial declaration PR approval before consumers.
G2D drafts calibration and stable threshold/lifecycle interfaces without final allocator acceptance.
A22 implements the policy/metric/recovery contract against those interfaces.
G2 then integrates final rules, freeze and core acceptance with A22 outputs.
U1 and T1 final acceptance also depend on A22.
This separates an interface prerequisite from final acceptance and prevents a dependency cycle.

## 6. Stock decode boundary and scoped validation

### Authority and interpretation

User concern, 2026-10-01 11:04 PDT, exact quote:

> I don't see the circumstances where this would be a concern. I mean, I didn't know what the attack vector would even be. Like you said, it doesn't get preserved, it just overwrites.

Coordinator interpretation, recorded separately from the user's words:
No exploit or cross-consumer mismatch from duplicate keys alone was demonstrated in the stock normalized slot path.
The earlier raw-wire boilerplate overstated that path as a readiness blocker.
This is not a general security assurance or a fictitious selected menu option.

The user-directed documentation correction scopes the validation boundary below.
No planned generated slot payload needs duplicate keys.
Normal generator dictionaries contain unique keys.
RI is not an allocator/gameplay issue or an S0 blocker.

### Required input scopes

Raw Army, Location, contract, calibration, snapshot, package files and controlled fixture JSON enter our raw parsers.
Those parsers reject duplicate properties at every depth before dictionary conversion.
Strict raw syntax, exact schema, semantic hash, binding and byte/provenance checks remain required.

Stock decoded slot objects enter after MultiClient.Net's normalization.
ChessV validates every resulting field, type, semantic hash, binding and effective-policy record before setup.
It rejects missing, unknown, conflicting or invalid values without a fallback.
It does not claim detection of raw-wire duplicates or syntax already lost upstream.
Canonical hashes use the fully validated resulting objects.
This boundary does not weaken raw artifact or decoded-object validation.

### Evidence and limits

An isolated canonical parser proves only its own raw input path.
Installed NuGet 6.6.0 identifies source commit
`75d4c5e6a52bb0c8bb1d4bc368652613509c7acb`.
The socket helper privately parses the full message before public `PacketReceived`.
`ArchipelagoPacketConverter.ReadJson` calls `JObject.Load` with default duplicate replacement.
A live in-memory call to the installed net6.0 converter replaces `probe:1,probe:2` with `probe:2`.
No error preserves the original duplicate evidence.

The public socket interface offers parsed packets and errors, not a raw event.
The session factory constructs a fixed stock graph without a public session constructor.
Dictionary or parsed-packet checks therefore cannot detect original wire duplicates.
No new raw route is required by this scoped contract.
A transport fork, reflection hook or second raw connection is not authorized.

F07 keeps separate raw parser and normalized slot-object acceptance cases.
No dependency fork, raw hook, reflection, network change or new mandatory feature is authorized.
The evidence describes the stock boundary; it is not a new blocking dependency conflict.

## 7. Completion boundary

Reconciliation can finish with these gates explicitly open.
S0 readiness requires the current integrated design gates, including EP/NM/AL/PM/LC/CV01/D03.
CV02/TX/TR/CS design/reference acceptance remains accepted with its separate native obligations.
S0 implementation also requires explicit execution authorization.
Consumer implementation requires accepted S0 declarations under the current no-PR delivery instruction.
I2 retains final pins, actual packages and publication authorization as separate later gates.
This document claims no materialized declarations, executed gameplay tests, release hashes or ready status.
