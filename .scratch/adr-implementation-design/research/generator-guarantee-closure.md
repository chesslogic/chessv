# ChecksMate generator guarantees: consumer contract and acceptance proposal

Status: Research and proposed engineering acceptance. No production implementation or activation approval.
Date: 2026-10-25, local session date.
Authority update: 2026-10-26, prefix-upgrade clarification and selected Fundamental access metric.
Consumer: ChessV engineering-closure coordinator, PM/G2, A22/P1, CV01, and Common/D03.

## Executive summary

ChecksMate has a working v3 family projector and stage-local access rules.
It does not implement the proposed guarantee-first concrete allocator.
Its existing tests establish useful v3 behavior, not successor conformance.

The successor needs one immutable access contract before the first dependent rule query.
That contract differs from the final inventory census.
Archipelago applies starting-pool depletion, item links, and plando after `set_rules`.
Plando itself queries rules before `pre_fill`.
A first freeze in `pre_fill` is too late.

Useful bounds can come from guaranteed, funded, active witness resources.
The user selected concrete board-specific player values for new Fundamental Location access: certified `L_M <= B+H`.
Pocket support and credit, protected funding, and Legacy remain unchanged.
This closes P1 policy mapping, not the conservative-bound proof.
The proof needs separate lower bounds for semantic material, concrete BOARD material, composition, Castlers, and supported Pockets.
A sampled optional roster, nominal grant balance, or prefix length alone supplies none of these guarantees.
The proposal below does not require a generic multiworld solver, recovery graph, comparator, or trial driver.

## Consequential gaps

| Gap and kind | Effect | Deciding evidence or owner |
| --- | --- | --- |
| Exact allocator laws: engineering | No complete successor proof for counters, pruning, refunds, source eligibility, or mandatory completion | A22/P1 and Common accept mode-qualified rows and a complete bounded construction |
| Prefix upgrades: engineering proof | User permits funded improvements that preserve all promised predicates, including Location Castler requirements. Exact identity can change | A22/P1 supplies the predicate-preservation witness. Higher material alone is insufficient |
| Fundamental material bound: engineering proof | P1 policy is closed: new Fundamental access uses certified `L_M <= B+H`. The useful bound remains unproved | PM02 proves the bound over the covered domain. `H` is unchanged, `S` remains semantic, and `E` stays separate |
| Future inventory and eligibility: proof | A guarantee for one current capture need not survive a later receipt or a changed legal catalog | PM02 proves extension closure over the declared domain |
| Pocket distribution: proof | Suggested order cannot represent all permitted final Pocket choices | PM02 consumes actual ordered families, or proves a minimum over all permitted choices |
| Frozen world restoration: implementation | Tracker restores some counts and seeds but constructs the production contract, not the saved contract | G2/Common define explicit old/new identity routing |
| Final-board funding and calibration: data/policy | Current pool generation does not establish an opponent-cost minimum postcondition | Existing board-value owners supply profiles, opponent records, target policy, and compact-board expectations |
| Integrated AP lifecycle: acceptance | Existing test helpers omit core starts, links, and plando phases | LC08 uses the real upstream orchestration order and accessibility checks |

These gaps do not reopen CV02/TX/TR/CS design/reference acceptance.
Native conformance remains a separate later obligation.

## Scope, authority, and revisions

The primary reader task is a holistic integration assessment.
The investigation follows one local producer-to-consumer path.
Its finite scope covers the ChecksMate world, projector, pool, collection adapter, related tests, and relevant AP lifecycle functions.
Validation covers seven existing test modules and one small artifact-only probe.
No remote freshness, full-repository audit, benchmark, or production proof is claimed.

| Source | Revision or fingerprint | Treatment |
| --- | --- | --- |
| ChecksMate and local AP core | `0772bac724ff483043b56979f0ef078b9c2d264a` | Current worktree HEAD. Clean before and after investigation |
| ChessV ADR0024 and closure register | `6158507a0b8f0095fb69ff3d0d85779f3b1ee153` | Read with `git show` through the authorized sibling worktree |
| Candidate engineering contract | File revision `ebda82865651b18b18a52cd1eedb27b0ff6ea307` | Proposal, not accepted law. This supersedes the kickoff's older `4e2b0e6` pointer |
| Candidate file SHA256 | `11d7d8d403562204984897c06e7d78ddcb11d1e0698956498ef8028fa604a654` | Exact observed proposal |
| Board-value `spec.md` SHA256 | `e2bdf4e450f211a6caa0dc1386eb9e3a9c42bb5e42644ece1d03449a1beb33ea` | Authorized read of current reconciled plan |
| Prior `checksmate-evidence.md` SHA256 | `48b8c637b63e3423cf36a23b1efb7a6dfca11b61843c2580d5aaade5e275a25a7` | Prior evidence at the same ChecksMate revision, checked against current source |

ChecksMate citations below use the `worlds\checksmate\` prefix unless they explicitly name an AP core file.
ChessV citations use repository-relative paths at the stated revision.
No `AGENTS.md` exists in the searched worktree.
The documented test-scoping command appears in `docs\tests.md:136-158`.
Only session artifacts changed. No sibling document, repository source, commit, push, or PR changed.

The accepted boundary comes from `docs\adr\0024-seeded-hierarchical-allocation.md:10-65`.
It requires inclusive affordability, stable priorities, active promises, protected completion, and lawful later Fundamental purchases.
The closure frontier retains protected funded100 bases and separate Pocket credit.
See `.scratch\adr-implementation-design\engineering-closure.md:44-99`.
The candidate's exact series, dynamic program, and state pruning remain proposals.
See `allocation-engineering-candidate.md:75-180,295-418,421-490`.

The coordinator relayed a later user clarification on 2026-10-26:

> The only constraint not mentioned in the recommended option is castlers, which are needed for a location. Otherwise yes

Source: coordinator session `78444ebb-4f91-4995-acfc-d233bf1fcfb6`, message source call `call_NStUM43H4qadz3X9R4e8LCLW`.
This report records the relay, not an independent retrieval of the original user event.
Permitted improvements of prefix actors require genuine funding and preservation of all promised predicates.
Those predicates explicitly include Castler requirements for the Location.
Exact concrete identity need not remain. Higher material alone does not prove preservation.
This clarification closes the policy question, not the allocator proof or integrated acceptance gates.

The coordinator relayed the user's metric selection on 2026-10-26:

> Use concrete board-specific player values for Fundamental Location access (Recommended)

Source: coordinator session `78444ebb-4f91-4995-acfc-d233bf1fcfb6`, message source call `call_b1Nivxu7ln2a9cI4P9bgcJfh`.
The coordinator reports that the exact question left Pocket support/credit, protected funding, and Legacy unchanged.
This report records that relay, not an independent retrieval of the original answer.
New Fundamental access requires certified `L_M <= B+H`, with unchanged `H` and semantic supported-Pocket value `S`.
`E` remains separate for retained accounting and Legacy.
The selected domain does not establish a useful monotone bound or close PM02.

## Current implementation: actual consumer paths

| Surface | Current behavior | Consequence and source |
| --- | --- | --- |
| World preparation | Normalizes geometry, fixes five seeds, chooses army IDs, validates pool requests | `__init__.py:126-173,466-490`. No concrete price or candidate capture |
| Pool creation | Places Victory, creates pool, then counts returned items, ordinary starts, and local locked items | `__init__.py:269-283`. The count snapshot precedes links and plando |
| Pool minimum plan | Reserves mode-specific Castler prerequisites and enough nominal Chessmen under nonminimal accessibility | `item_pool.py:310-419`. Uses raw occupied-Pocket credit for the Chessmen minimum |
| Budget loop | Receives `min_material`, but decisions use maximum material, prerequisites, and capacity | `item_pool.py:685-751`. No final minimum-material assertion |
| Funding estimates | Scales historical 41/46 targets by terminal Location estimates | `rules.py:155-210`, `material_model.py:49-62`. No concrete opposing-roster valuation |
| Projection context | Stores seeds, preferences, mode, contract, geometry baseline, and obtainable count caps | `logic_projection.py:60-143`. Keeps a mutable `options` reference |
| Small Fundamental material | Computes all cells of a bounded count rectangle and takes componentwise suffix minima | `logic_projection.py:296-343`. Rectangle limit: 10,000 cells |
| Large Fundamental and Legacy material | Uses active-count floor times fixed Pawn100 | `logic_projection.py:246-294,319-359`. Not a concrete-price proof |
| King promotions | Adds425 per normalized promotion outside the envelope | `logic_projection.py:177-182`. This is not non-primary BOARD material |
| Castlers | Fundamental uses minimum of receipts, Chessmen, and Material capacity. Legacy subtracts frozen obtainable Queen upgrades | `logic_projection.py:184-217`, `item_utils.py:150-180` |
| Pocket count | Adds `min(3,ceil(receipts/limit))` without board support or family inspection | `logic_projection.py:145-168`, `item_utils.py:33-36` |
| Material rule | Compares projected material against a dynamic clamp to `maximum_material(stage)` | `rules.py:227-247` |
| Chessmen rule | Captures a clamped threshold during `set_rules` | `rules.py:331-344`. Later projection changes can affect material and count thresholds differently |
| Earlier-board access | A later unlocked stage bypasses earlier material and Chessmen rules | `rules.py:102-113,320-344`. This is a rule certificate, not a reallocation proof |
| Tactics | Fundamental threat/fork rules use count, not a concrete tactical witness | `rules.py:346-386`. Scope of these authored heuristics remains distinct |
| Geometry | Five selectable boards and legacy 12x12. Starting geometry contributes initial unlock counts, not precollected event items | `geometry_progression.py:6-12,68-177`, `__init__.py:590-620` |
| Semantic input | Counts, seeds, upgrades, unlocks, and baseline only | `apmw_projection\models.py:106-118`. No concrete type catalog, actual Pocket families, or typed source Parts |
| Current allocation | Constructs fixed-family slots and locked Castlers before ordinary upgrades | `apmw_projection\fundamental.py:59-125`. Geometry affects later active selection, not generation |
| Result resources | Separates active/reserve slots, placement, ledgers, and material totals | `apmw_projection\models.py:193-231`. Useful structure, not successor semantics |
| Common credit | Pocket/White credit stays outside roster projection | `apmw_projection\planning.py:87-150`. No supported-Pocket material calculation |
| Effective collection | Caps real counts and records removable excess copies | `collection_state.py:1-47`, `__init__.py:305-316`. Mode-incompatible items provide no effective contribution |
| Slot output | Emits contract, seeds, geometry, army IDs, preferences, and frozen obtainable counts | `__init__.py:175-230`. Repeated output does not consume RNG |
| Tracker | Restores counts and complete seed tuple. Rejects malformed values and inconsistent geometry | `__init__.py:330-470,494-605` |
| Tracker contract | Builds projection without passing the embedded saved contract | `__init__.py:330-344`, `logic_projection.py:60-73`. Snapshot output alone does not establish replay |
| Resource freeze | Package loader freezes v3 JSON and returns fresh output dictionaries | `apmw_projection\resource.py:17-99`. Package immutability is not a world-input freeze |

Current family expectations, concrete costs, received credits, and rule thresholds are different quantities.
For example, the current exact projector sums `final_expected_material`.
It does not sum concrete MG prices.
See `apmw_projection\semantic.py:119-126`.

## Proposed local consumer contract

### Separate the immutable domain from actual supply

The proposed `AccessContract` binds world/slot identity, mode, algorithm identity, five seeds, geometry series, effective restrictions, and source laws.
It also binds catalog/profile hashes, threshold domains, Pocket policy, normalization caps, and the permitted extension domain.
Each query uses an immutable capture of effective counts and relevant setup inputs.
No query reads an unbound live singleton or replaces a missing field with zero.

The proposed `SupplyInventory` records actual generated and retained resources after core transformations.
It distinguishes ordinary starts, starting-pool relocation, direct pool items, fixed events, links, replacements, and successful plando additions.
A supply census is not proof that every counted item is reachable.
Core AP retains that responsibility.

The simplest sound domain covers all supported effective counts and permitted restrictions, independently of the provisional generated quantity.
This is a proof domain, not a request to enumerate a global state rectangle in production.
A tighter world-specific domain is valid only with a conservative pre-query upper bound on every later transformation.
The current local API has no dedicated hook between item links and plando.
Therefore a precise post-link snapshot cannot be the sole pre-plando safety mechanism.

### Result obligations

The allocator supplies one coupled witness for the actual capture.
Each active actor has a source identity or explicit funded-birth identity, concrete member, current family, and compatible square.
Its funding names eligible Parts, exact debits, lawful releases, retained reserves, and consumed token state.
The same witness supplies composition, Castler fulfillment, and actual supported Pocket families.

The access consumer extracts certified metrics from that witness contract.
It never turns an unallocated balance, protected base, dedicated reserve, or Pocket credit into purchase funding.
It never treats a derived Pawn as a received Chessman.
Natural Castlers reduce the required shortfall without a free token or duplicate credit.
These are accepted requirements, not choices of the proposed dynamic program.

For each rule-facing metric `L`, acceptance requires both properties:

```text
Soundness:   L(capture) <= the matching contribution of every admitted covered realization.
Monotonicity: L(capture) <= L(permitted extension of capture).
```

The second property concerns permitted inventory, eligibility, Pocket, and geometry changes.
Stable seeds alone do not prove it.
The first property concerns the metric's actual meaning, not an unrelated greater total.
All components must refer to compatible realizations.

The user permits improvements of promised actors under the clarified preservation rule.
The proof must cover permitted replacements, including their funding, placement, and Location Castler requirements.
It cannot assume exact-member immutability or infer preservation from higher material alone.
Exact-member minima require a proof over every permitted replacement, not a frozen original identity.

## Useful constructive bounds

### Resource definitions

The retained closure gives the following definitions at `engineering-closure.md:357-410`.
Its historical construction is not adopted here.
The resource distinctions remain, but the later user decision replaces the player-access domain for new Fundamental worlds.

| Symbol | Meaning |
| --- | --- |
| `n`, `v` | Actual active non-primary actors and their current non-Pawn count |
| `E` | Semantic family material of those actors |
| `B` | Concrete active non-primary BOARD material. No primary, Pocket, reserve, or unspent credit |
| `C`, `J`, `S` | Supported actual Pocket count, non-Pawn count, and semantic material |
| `H` | `min(S, floor(B/2), 300*C)` |
| Composition | `(n+C, v+J)` |
| Rule material | New Fundamental uses certified `L_M <= B+H`. The earlier `E+H` candidate does not control this domain. Legacy is unchanged |

At most `floor(n/2)` actual occupied Pockets receive support.
Non-Pawn families receive support first, then Pawn families.
Captured final order resolves ties.
Unsupported occupied Pockets remain present but contribute no count or material credit.

The producer must preserve a useful guaranteed core across its declared extension domain.
That core can include mandatory completion actors, not only preferred-prefix actors.
The consumer needs proved lower bounds `n0,v0,E0,B0` and supported-Pocket bounds `C0,J0,S0`.
Then it can use:

```text
T0 = n0 + C0
N0 = v0 + J0
H0 = min(S0, floor(B0/2), 300*C0)
M0 = B0 + H0             # selected new Fundamental rule domain
E0 + H0                  # separate semantic comparison, not this access metric
```

The monotone `min` formula makes `H0` sound once all three inputs are sound.
It does not prove those inputs.
The selected `M0` is sound only if its component bounds are sound.
Its extension monotonicity remains a separate proof obligation.
`E0` remains separate and supplies no additional credit to `M0`.

### How the producer can establish the inputs

A finite guaranteed role set can provide per-role minima over every permitted final member.
The minimum must use actual eligible types and the pinned profile.
Disjoint guaranteed actors prevent double counting.
Funding and placement determine whether each actor belongs to the guaranteed set.
An active actor that can move into reserve does not supply an unconditional active minimum.

The mandatory completion construction can certify a larger core than the preferred prefix.
Its proof must cover every final completion that the production selector can return.
One low-cost completion witness proves feasibility, not a lower bound on all other completions.
The proposed row compiler can carry material and role coordinates where its pruning proof requires them.
It cannot merge states that differ in a future certified predicate.

For a fixed actual Pocket capture, support from `n0` selects only justified families.
For unrestricted local redistribution, the producer needs the minimum over all permitted distributions.
Using the suggested order silently narrows that domain.
Such a minimum is a bounded local calculation when the Pocket state space is finite.
It is not a multiworld search.

### Nonzero arithmetic example

This is a synthetic fixture, not an approved price table or a production allocation.
Assume the funding/placement proof guarantees five Pawn actors at concrete40 and three non-Pawn actors at concrete325.
Assume semantic values100 and300, respectively.
Three actual Pockets contain two non-Pawn families at semantic300 and one Pawn family at semantic100.

```text
n0 = 8, v0 = 3
B0 = 5*40 + 3*325 = 1175
E0 = 5*100 + 3*300 = 1400
C0 = 3, J0 = 2, S0 = 700
T0 = 11, N0 = 5
H0 = min(700, floor(1175/2), 900) = 587
M0 = 1175 + 587 = 1762
Separate semantic comparison E0+H0 = 1400 + 587 = 1987
```

This result uses useful board and Pocket resources without optional bonuses.
The artifact probe checks nine nonnegative optional extensions of this arithmetic model.
It does not establish that the allocator can fund or place the assumed core.
The exact gap is the producer's guarantee over its full accepted source and extension domain.

### Geometry and receipt extensions

A material-only extension preserves preferred order under unchanged legal inputs.
It still requires retained material, role, and placement predicates for a monotone access claim.
A new slot, new eligibility, or a new board can change the preferred table.
Those changes need separate certificate-preservation cases.

The current later-board bypass can remain only as an explicitly supported access certificate.
A larger board can have different costs and a shorter affordable prefix.
Geometry size alone supplies no proof.
The successor can use a per-Location preserved certificate or a proved cross-stage bound.
Neither choice requires preservation of identical pieces on every board.

## Correct AP lifecycle use

| Phase | Current AP behavior | Proposed contract obligation |
| --- | --- | --- |
| `generate_early` | Resolves world inputs before starting items | Capture immutable profiles, mode laws, seeds, geometry, and restrictions before their first use |
| Core starts | Adds both start forms to precollected inventory | Ordinary starts are additive. Starting-pool inventory is a relocation claim, not an extra copy |
| `create_items` | Creates pool and fixed placements | Construct funding plan and nominal supply. Do not call it final post-transform supply |
| `set_rules` | Installs rules before links and plando | Bind certified domain and threshold policy before dependent queries |
| Starting-pool depletion | Removes matching pool copies and adds repeatable filler | Preserve total pool size. Count the surviving start copy once |
| `link_items` | Replaces common copies with group items and recipient event locations | Count recipient entitlements once. Do not count group receipt and recipient event as separate owned sources |
| Plando | Moves or creates items, obtains all-state, and invokes restrictive fill | Existing rules must already be sound for permitted additions and omissions |
| `pre_fill` | Runs after plando | Reconcile actual local supply and report unsupported inconsistencies before main fill. Do not invent a first freeze here |
| Main fill and `post_fill` | Places remaining items before progression balancing | Preserve the query contract. Keep AP ownership of locality and accessibility |
| `finalize_multiworld` | Last logical changes, increase accessibility only | Validate seals and final supply. No threshold tightening or certificate revocation |
| `pre_output` | No sphere-affecting changes permitted | Freeze output data, with no logical recomputation that changes rules |
| `fill_slot_data` | Can overlap output generation | Serialize defensive copies of the already bound world data. No new RNG, costs, or inventory mutation |
| Tracker/client setup | Restores original-world inputs | Route exact algorithm/profile identity. Current production defaults cannot replace saved world data |

Source: `Main.py:76-118,142-213,239,365-370`.
Core constraints: `worlds\AutoWorld.py:417-468`.
Starts and links: `Main.py:81-99,147-185`, `BaseClasses.py:295-378`.
Plando: `Fill.py:1112-1190`.

`get_all_state` is not a safe way to discover the initial certification domain.
It collects precollected, pool, and pre-fill items, then sweeps placements through rules.
That creates a circular dependency if those rules await the resulting census.
See `BaseClasses.py:427-461`.
A structural inventory ledger can avoid this dependency.
It still cannot assert reachability.

A structural census must distinguish link recipient events from the group token that unlocks them.
It must also avoid duplicate object counts across pool and placement surfaces during partial fill.
Only successfully retained plando items count as actual supply.
The declaration alone is insufficient.
Core can create a missing `from_pool` plando item after a warning.
See `Fill.py:1142-1156`.

Current `fill_slot_data` does not reconcile this later supply.
Its material threshold clamp also does not prove adequate funding.
A weak bound can lower a target until an underfunded world appears reachable.
The successor needs an independently approved funding minimum and calibrated threshold policy.
If a per-world clamp remains, its value must bind before queries and remain semantically justified.

## Discriminating counterexamples

| ID | Exact case | Result and implication |
| --- | --- | --- |
| CE1: stale Queen domain | Frozen obtainable Queens0. Current inventory contains Majors2 and Queen upgrades1 | Executed: logic Castlers2, exact semantic eligible Castlers1. Current-count normalization does not repair the frozen Legacy subtraction |
| CE2: unsupported Pocket | No non-primary board actors, one Pocket receipt | Executed: current count metric1, material0. Successor support cap is0, so raw occupancy cannot satisfy composition |
| CE3: late supply | Freeze obtainable Queens0, append a Queen upgrade to the pool, then serialize again | Executed: exported obtainable counts remain unchanged. This isolates missing refresh, not a full plando run |
| CE4: protected cheap base | One Chessman funds100 but selects concrete Pawn40 | Spendable base difference is0, not60. Concrete cheapness does not release protected funds |
| CE5: competing obligations | One free compatible home square. Both a nonqualifying optional actor and required Castler need it | Each fits separately. Their joint completion does not fit. Admission needs one coupled witness |
| CE6: protected funding | Mandatory completion needs all eligible100. Optional purchase costs100 | Standalone equality holds, but completion fails. Inclusive affordability does not waive the guard |
| CE7: unsupported family sum | Two actual Pockets each semantic300, `n=2`, `B=800` | Only one Pocket has support. Correct `H=min(300,400,300)=300`, not400 |
| CE8: duplicate primary credit | Non-primary `B=0`, primary promotion contributes425, one Pocket | Correct support and `H` remain0. Primary material cannot fund Pocket support |
| CE9: birth identity | One real Chessman plus one lawful funded Pawn birth | Active count2, received Chessmen1. The birth has no reusable token or second credit |
| CE10: family pruning | One family contains an unaffordable member and an affordable permitted member | Rejecting the first member cannot remove the entire family |
| CE11: domain expansion | A future eligible member has lower concrete/semantic contribution than the previously chosen member | Unchanged seed and ordinal do not preserve material. The proof needs minima over the extension domain |
| CE12: incompatible maxima | Feasible rosters have composition pairs `(11,4)` and `(10,5)` only | Independent maxima claim `(11,5)`, but no witness meets both |
| CE13: bound used as target | Authored threshold1000, available certified material400 | `min(1000,400)` allows access at400. This is a policy change, not proof of1000 |

CE1-CE3 execute current code through an artifact-only probe.
CE4-CE13 are logical discriminators under stated inputs.
They do not claim existing production defects in unimplemented successor behavior.
CE1 is a real local soundness mismatch outside the frozen obtainable domain.
The full late-plando path remains an integration fixture obligation.

## Independent successor acceptance matrix

Expected vectors must come from explicit arithmetic, accepted fixture data, or an independent finite reference calculation.
Neither implementation can generate its own expected results.
Matching Python and C# outputs alone does not prove the contract.

| Fixture | Input and independent expectation | Gate and current evidence |
| --- | --- | --- |
| F01: equality and reserve | Eligible100, mandatory100, optional100. Mandatory fits exactly, optional fails the guard | AL/PM. CE6 defines expected result |
| F02: concrete base custody | Chessman100 selects Pawn40. Funded100 remains protected, free released amount0 | AL. Accepted source law, no successor executable fixture |
| F03: useful lower bound and selected domain | Synthetic eight-actor case, plus the P1 two-Knight discriminator | New Fundamental `(T,N,B,E,H,M)=(11,5,1175,1400,587,1762)`. In P1's singleton case,1050 passes1025. Separate semantic1000 fails. Neither fixture proves the full bound |
| F04: support edges | `n=0,1,2,3,4`, at least three actual occupied Pockets | Supported counts `0,0,1,1,2`. Primary and reserves do not change them |
| F05: final Pocket choices | Same receipts, different actual family orders/distributions | Use each actual support set. Universal bound cannot exceed the least covered contribution |
| F06: natural Castler | Enabled target2, one naturally qualifying active actor | Completion supplies exactly one additional application, with actual eligible charge and home placement |
| F07: placement collision | CE5's single compatible square | Reject optional purchase. Preserve the feasible Castler target |
| F08: incompatible target maxima | Feasible pairs `(11,4)` and `(10,5)` | No successful `(11,5)` witness |
| F09: guarded upgrade | Upgrade optional actor, then a promised actor. Include a higher-material replacement that loses a required Castler contribution | Permit genuine funded improvements that preserve every promise and Location Castler requirement. Reject the Castler-losing case without a valid coupled witness that preserves the requirement. Exact identity can change |
| F10: surplus birth | Received count1, sufficient lawful residual and square for one birth | Received count stays1, active count2. Ledger consumes the residual once |
| F11: remapping/counters | Same family contains one failed member and one valid member | Family survives. Exact next draw vector waits for accepted counter and canonical-order laws |
| F12: collect/copy/remove | Effective cap plus two excess copies, copy state, remove copies in one branch | Source state unchanged. Excess disappears before effective count. Metrics return to original values |
| F13: ordinary start | Pool has Material `m`, ordinary start has1 below the effective cap | Total ownership `m+1`. No pool removal |
| F14: starting-pool item | Pool has Material `m`, starting-pool request1 | Start1 plus remaining pool `m-1`, total `m`. One replacement filler, unchanged pool size |
| F15: linked supply | Two players each link one common Material | Each obtains one effective entitlement through the group. Neither obtains two from token plus event |
| F16: linked replacement | A link replacement is a progression item, including a Queen upgrade | Capture covers the effective increment before plando queries. No stale Queen bound |
| F17: plando move | Successful `from_pool` placement | One retained item, no duplicated source. Same total entitlement |
| F18: plando bonus/missing source | `from_pool=false`, then missing `from_pool=true` with core warning | Count successful actual additions. CE1 must not produce a two-Castler certificate |
| F19: failed optional plando | Core cannot place a nonrequired item | No phantom supply from the declaration |
| F20: first query timing | Instrument first access query during core plando | Full access binding already exists before `pre_fill` |
| F21: immutable thresholds | Query, reconcile supply, fill, finalize, serialize twice | No post-query tightening. Identical serialized semantic data and unchanged RNG |
| F22: stage extension | Each supported geometry transition, same received inventory, distinct board prices | Every previously certified Location stays justified. Identical pieces are not required |
| F23: world restoration | Saved world A, installed profile B, same current production version selector | Tracker and client use A. Unknown hash/identity returns an explicit diagnostic |
| F24: stale result | Start capture A, change inventory/profile/geometry to B, receive A's response | No result admission into B. No mixed ledgers or active-match mutation |
| F25: minimum funding | Accepted opponent target cannot fit local capacity or item restrictions | Explicit generation diagnostic, not threshold reduction or silent underfunding |
| F26: upstream acceptance | Real orchestration, both modes, starts/links/plando, geometry events, applicable accessibility levels | AP fill succeeds, game is beatable, and requested accessibility succeeds |
| F27: old-world isolation | Frozen v2/v3 resources and old fixture inputs | Original semantics remain unchanged. No new 12x12 price profile |

F26 uses actual `fulfills_accessibility` and `can_beat_game`, not only nonempty spheres.
Current AP raises for unbeatable games but warns for unmet accessibility in a beatable game.
See `Main.py:365-370`.
Acceptance must record both results.
A `skip_output` path returns before the output-thread accessibility task.
Such a fixture must invoke the core checks explicitly.
See `Main.py:218-239`.

## Executed evidence and limits

The selected test command follows the repository's documented world-scoping mechanism:

```powershell
$env:AP_TEST_WORLDS='checksmate'
py -3.12 -m pytest -q worlds\checksmate\test\test_logic_projection.py worlds\checksmate\test\test_generation_options.py worlds\checksmate\test\test_lifecycle_capacity.py worlds\checksmate\test\test_board_series_integration.py worlds\checksmate\test\test_castle_accessibility.py worlds\checksmate\test\test_collection_state.py worlds\checksmate\test\test_world_generation.py
```

Result: **213 passed, 3553 subtests passed**, one `_speedups` fallback warning.
Elapsed test time: 38.40 seconds. This is not a performance benchmark.
The initial `python -m pytest` attempt used Python3.10 and failed collection because `typing.Self` was unavailable.
The installed Python3.12 resolved that environment mismatch.
No dependencies or manifests changed.

The existing tests cover finite count-space monotonicity, representative exact bounds, stage transitions, inventory requests, slot-data repeatability, tracker overrides, and excess removal.
They do not establish the successor's concrete funding, placement, source preservation, or optional-purchase laws.
`test\general\__init__.py:8-16,34-68` calls world hooks directly.
It omits Main's precollection, starting-pool depletion, link processing, and plando orchestration.
The inventory test manually demonstrates depletion rather than running the whole core path.
See `test\test_generation_options.py:68-160`.

The retained probe is `generator-guarantee-probe.py` beside this report.
Its recorded output is `generator-guarantee-probe-results.json`.
It runs from the ChecksMate worktree with Python3.12 and `AP_TEST_WORLDS=checksmate`.
Its private projection-input call is evidence instrumentation, not a proposed public interface.
Its synthetic arithmetic does not implement the candidate dynamic program.
The probe and results remain byte-for-byte unchanged by the P1 policy update.
Their `combined` value1762 represents the selected concrete-plus-Pocket domain.
Their `semantic_plus_pocket` value1987 remains a separate comparison, not new Fundamental access credit.

Covered proof domain: current v3 tests, three local counterexamples, and nine synthetic nonnegative arithmetic extensions.
Uncovered: complete successor rows, dominance, exact counters, all legal refunds, all Pocket distributions, cross-stage certificate preservation, and full AP transformation integration.
C# native behavior and CV02 native conformance remain outside this source investigation.

## Proposed next acceptance steps

These are bounded proposals, not implementation dispatch.

| Inquiry | Inputs, method, and budget | Consumer and stop |
| --- | --- | --- |
| P1: exact metric join | Completed authority read and subsequent user selection on 2026-10-26 | Policy closed: new Fundamental certified `L_M <= B+H`. Useful-bound proof remains with PM02 |
| P2: useful guarantee core | Accepted source rows and proposed completion witness. Independent finite enumeration of one small Fundamental profile, including birth and optional upgrade | A22/PM02. Stop at matched literal vectors and declared extension proof, or smallest counterexample |
| P3: AP timing fixtures | Current `Main`, `Fill`, `BaseClasses`, and F13-F21. One two-player link case plus three plando forms and starts | LC08/G2. Stop at correct pre-query binding and reconciled supply without modified AP traversal |
| P4: restored-world fixture | One frozen old world and one successor world with distinct profile identities | G2/Common. Stop at original-world replay, stale-response refusal, and identical repeated serialization |
| P5: integrated acceptance | Consume P1-P4, AL04/PM02, CV01, and already accepted CV02 design/reference evidence | D03. Stop with covered/uncovered gates, not S0 or release approval |

Effects for these proposals remain fixture/design work until the coordinator obtains the relevant execution approval.
No broad interview or new user-policy question is necessary from this investigation.
The prefix-upgrade policy question is resolved by the 2026-10-26 clarification.
Predicate encoding, independent upgrade fixtures, and the full preservation proof remain engineering obligations.

## P1 result: exact metric join

Status: Policy mapping selected by the user. Conservative-bound and integrated acceptance proofs remain open.
Date: 2026-10-26.
Question: Do later board-value decisions select concrete player material for Location access, or only spending and threshold calibration?

### Exact decision evidence

The source is `events.jsonl` in session `2181993b-955f-43a7-b1fb-228e6df19b38`.
The retrieval examined relevant user messages and `ask_user` results, not implementation output as policy.
Cross-session reports in that history are not direct user decisions.

| Evidence | Exact decision and boundary |
| --- | --- |
| User event `0bfeca7b-9e7e-4957-aa4a-f83392a7e918`, `2026-10-24T00:25:43.726Z` | The user ties the final opponent cost to supplied Material and "a higher material value expectation for each Location on that board." The Knight325/Gardener225 example specifies concrete purchases, not the access comparator |
| User event `4da59559-7a33-4093-b773-cbcf4bbbdb1c`, `2026-10-24T06:05:27.099Z`, answer9 | "Opposing armies are strictly defined in the client - I imagine after these adjustments we would sum the values for the vanilla army and assign that as the new top-end for each checkmate on each board and then might scale the scoring for locations on that board to suit." |
| User event `6f6b540d-bb3a-4cf4-9bcf-286b2fe36329`, `2026-10-24T06:16:39.912Z`, answer9 | "Everything on a given side should be added together using the estimated material value, minus a single unmounted King (the player's starting King isn't accounted for, so neither should be the enemy army's)." |
| `ask_user` call `call_yqlmsu69WjVKx9spuHw4J0cA`, answer event `7f6fa189-4165-4021-9f78-12171a6b4d1f` | User selected geometry-aware values consistently for Gem costs and True Fork comparisons. The question named those two consumers, not aggregate Location access |
| Board-value plan `spec.md:334-344,419-424` | D9 records the concrete opponent MG total and King exclusion. D11 retains a concrete-price lower-bound obligation. Neither specifies the complete player comparator |
| Accepted closure baseline at `6158507`, `engineering-closure.md:383-410` | Keeps semantic `E`, concrete `B`, and supported-Pocket `H` distinct. The retained lower-envelope candidate names `E+H` |

The answer9 phrase "Everything on a given side" strongly favors symmetric concrete board valuation.
Its question context concerns the opponent export and King exclusion.
It does not explicitly replace `E` in the player-access expression.
The Gem/True Fork answer cannot supply that missing choice because its question names different consumers.

Before the later selection, these statements favored concrete valuation but did not unambiguously override the retained `E+H` mapping.
Common units do not settle this alone.
Both semantic expectations and concrete MG values use Pawn100 units but assign different values to the same actor.
Using `E+H` against a concrete target is mathematically defined, though it introduces that valuation mismatch.

The subsequent explicit selection resolves this narrow authority gap.
Its relay and scope exclusions appear in the authority section of this report.

### Selected mapping

Selected rule: use a certified lower bound on `B+H` for new Fundamental Location material access.
Keep semantic Pocket material inside `H`.
Keep received funding, purchase debits, and semantic board expectations separate.
This selection does not change Legacy or old-world semantics.

| Consumer | Quantity | Authority status |
| --- | --- | --- |
| Fundamental purchase | Prepared-board concrete MG liability under the typed funding law | Selected. Protected bases, surcharges, and release rules still constrain debits |
| Opponent top end | Exported sum of vanilla opposing pieces at that board's MG values, excluding one unmounted King | Selected by D9. This is a per-board target, not player inventory credit |
| Player board contribution | `B`: actual active non-primary concrete MG sum under the same captured profile | Selected metric domain. No reserve, unspent credit, or primary contribution |
| Pocket contribution | `H=min(S,floor(B/2),300*C)` for actual supported Pockets | Retained. `S` remains semantic Pocket material. No conversion into purchase funding |
| Player Location access | Certified monotone lower bound `L_M <= B+H` over every covered realization | Selected replacement of `E+H` for new Fundamental worlds. Bound proof remains open |
| Semantic board material | `E` remains available for retained accounting, Legacy, and unchanged consumers | Not added to `B` and not an extra funding source |
| Threshold scaling | Approved board-specific Location calibration based on D9's top end | Selected direction. This inquiry does not invent scaling, difficulty, or rounding formulas |

This mapping does not claim that actual `B+H` is monotone after every receipt.
PM02 must certify a monotone lower bound, not return a favorable current allocation.
It must also preserve supported-Pocket credit rather than remove Pockets to simplify the proof.

### One ordinary pass/fail discriminator

Assume one valid 8x8 witness contains two Knights325 and one Pawn100.
It also has one supported Minor-family Pocket with semantic material300.
The primary King contributes nothing to these quantities.
This example assumes fixed admission and placement, so only the metric choice varies.

```text
n = 3, C = 1, S = 300
B = 325 + 325 + 100 = 750
E = 300 + 300 + 100 = 700
H = min(300, floor(750/2), 300) = 300
Concrete player quantity B+H = 1050
Semantic player quantity E+H = 1000
Illustrative already-calibrated Location threshold = 1025
```

The selected concrete domain passes. The separate semantic comparison fails.
The threshold1025 is a discriminator, not a claim about an existing authored Location.
In this singleton fixture, the certified bound equals the displayed quantity.
A broader extension domain can require a smaller bound.

No metric-policy question remains.
The user selected concrete board MG plus retained supported-Pocket credit.
It aligns the player board valuation with the opponent-derived target and the prepared-board price profile.
It neither reopens concrete spending nor changes the Pocket-support rule.
The example proves only the arithmetic discriminator under its stated singleton assumptions.
The complete useful-bound proof, fixture integration, and gate acceptance remain open.
No production source or probe changed for this policy update.
