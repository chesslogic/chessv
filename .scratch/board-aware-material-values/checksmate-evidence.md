# ChecksMate board-material integration: evidence and iterative plan

## Executive summary

ChecksMate already owns the Python projector, the item-pool generator, and stage-local reachability rules. It does not own concrete piece prices.
Its production contract is v3, not v2. That contract still uses fixed family expectations and fixed item credits.
Concrete affordability requires candidate identity and eligibility inside the shared allocation contract. A price table alone cannot close this gap.

The simplest proposed integration extends the existing exported contract with explicit geometry profiles and the minimum candidate metadata.
ChessV remains the author of the material profiles. ChecksMate consumes a generated, hashed export, not a second hand-maintained price table.
New worlds freeze this contract and their funding/Location data. Existing worlds retain their original snapshots and allocation semantics.

This report proposes implementation iterations. It does not authorize implementation or select new prices.
Fundamental is the only allocation target. Legacy receives only necessary compatibility work.

## Consequential gaps and decisions

| Gap | Evidence boundary and effect | Deciding owner or evidence |
| --- | --- | --- |
| Actual opponent composition | Python stores army IDs and geometry counts, but no concrete opponent roster or per-piece values. Counts cannot determine final-board cost. | ChessV owner: export the deterministic opponent recipe, or provide matching concrete-roster fixtures and a small resolver. |
| Concrete candidate eligibility | Projector input contains no type IDs, army selection, pawn restrictions, or per-type limits. Those fields affect affordable pools. | Joint allocator contract. Agree what data crosses the semantic/presentation boundary. |
| Upgrade/refund and grant accounting | Current funding uses Chessmen base100, Material400, and Castler delta400. Concrete prices cannot replace these credit denominations. | User/coordinator: approve debit, retained slot credit, release/refund, locked-Castler, and royal rules. |
| Reported extra +70 | No explicit +70 pool provision appears in the examined current world sources or documents. | Request the originating revision, formula, or source from the user. Do not infer an allowance or overdraft. |
| Location calibration | Existing small/grand thresholds are authored estimates, not opponent-cost calculations. Compact checkmate has no calibrated threshold. | Product/calibration owner: approve per-board estimates, including 6x8. |
| Legacy world interpretation | Slot data embeds a contract, but Python tracker construction still selects the production contract. No material-profile snapshot schema exists. | Joint compatibility design. Old-world replay must select old data and semantics explicitly. |
| Monotonic reachability | The current large-state floor assumes every active slot is worth at least Pawn100. New calibration does not establish that invariant. | Generator owner: derive and prove a new conservative bound from eligible concrete types, or use a bounded exact method. |
| Numeric profiles | Accepted analysis is not a calibrated price table. | ChessV calibration owner. All current 8x8 MG/EG entries remain exact. |

These are different gaps. Missing evidence for +70 is not permission to add +70.
Missing opponent composition is a cross-repository data dependency, not a request for a general generator redesign.

## Scope, revisions, and source access

Investigation date: 2026-10-23, local time.
Primary reader task: a holistic integration assessment with an actionable, dependency-ordered plan.

| Source unit | Revision or access | Coverage |
| --- | --- | --- |
| This child worktree | `055d492db8efb7b20008691e60c302b46a1e942a`, main-derived | Contains no `worlds\checksmate`. Clean at inspection. |
| Actual ChecksMate implementation | `checks_mate` and `origin/checks_mate`, both `0772bac724ff483043b56979f0ef078b9c2d264a` | Local commit dated 2026-09-12. No network freshness claim. |
| Immutable source snapshot | `files\source-0772bac\worlds\checksmate` under this session | Extracted with `git archive`. No other checkout reads or writes. |
| Coordinator authority | `C:\Users\aaedi\.copilot\session-state\2181993b-955f-43a7-b1fb-228e6df19b38\plan.md`, lines 1-440 | Read confirmed decisions, frozen fixture, integration gates, and current execution notes. |
| ChessV source | Outside this child's permitted repository scope | Not inspected. Coordinator and registry child own current C# evidence. |
| Historical generator investigation | Session `d8a061d2-4cb2-4329-bbbe-fab9cea86e38` | Not consulted or changed. |

All source citations below refer to revision `0772bac724ff483043b56979f0ef078b9c2d264a`.
Paths are relative to that revision, not this main-derived checkout.
The snapshot preserves the same paths and line numbers.

The finite investigation covered world lifecycle, funding, Locations, projection, contract resources, protocol, packaging entry points, and directly relevant tests.
Searches for +70, concrete values, export provenance, and snapshots covered the extracted world Python, JSON, and Markdown files.
No repository `AGENTS.md` or `CONTEXT.md` appeared in the inspected tree.
No new upstream, remote branch, ordinary game world, or historical implementation investigation formed part of this report.

## Evidence map

| Surface | Current implementation | Citation |
| --- | --- | --- |
| Geometry identity | Explicit stages -1 through 3 represent the five target boards. Stage4 is legacy 12x12. | `worlds\checksmate\geometry_progression.py:6-12,68-78,84-177` |
| Opponent metadata | Each geometry stores only CPU pawn/non-king counts. For example, 6x8 stores 6/5 and 12x10 stores 12/11. | `geometry_progression.py:35-45,84-159` |
| Army selection | `generate_early` resolves allowed army IDs, with one chosen ID or the configured chaos set. | `worlds\checksmate\__init__.py:126-173` |
| Item credit | Chessmen grants100, Material grants400, Castler grants0, and Play as White grants50. These are inventory denominations. | `worlds\checksmate\items.py:47-75` |
| Material calculation | Pool requirements call fixed minimum/maximum formulas and subtract50 for forced Play as White. | `worlds\checksmate\material_model.py:49-62` |
| Funding target | Minimum41 and maximum46 scale by the terminal checkmate threshold divided by4020. Difficulty and relaxation also apply. | `worlds\checksmate\rules.py:155-210` |
| Terminal estimates | 8x8=4020, 10x8=6020, 10x10=6020, 12x10=8020. The 8020 comment estimates two outer attendants. | `worlds\checksmate\locations.py:49,141-165` |
| Compact gap | 6x8 has `None` thresholds. Pool calculations substitute the 8x8 baseline, explicitly without a new calibration. | `locations.py:132-138`, `rules.py:193-210` |
| Pool minimum | `create_progression_items` accepts `min_material`, but its loop/removal decisions use `max_material`, capacity, and prerequisites. No minimum postcondition appears there. | `worlds\checksmate\item_pool.py:685-751` |
| Pool capacity | Available item slots derive from the actual stage-local Location set minus reserved Locations. | `worlds\checksmate\pool_state.py:45-88`, `item_pool.py:495-565` |
| Pool prerequisites | Mandatory Fundamental Castlers require existing Chessmen and enough Material items. Starting inventory also affects this plan. | `item_pool.py:310-387`, `worlds\checksmate\item_utils.py:150-180` |
| Pool removal | Removal compares generated credit plus candidate credit plus locked credit against the maximum. It reserves room for required Chessmen. | `worlds\checksmate\item_removal.py:31-73,149-168,229-254` |
| Location data | Each Location has small/grand material estimates, not a five-geometry material map. | `locations.py:52-103,113-240` |
| Rule application | Rules resolve a stage, apply difficulty/relaxation, and clamp material targets to obtainable logic material. A later board bypasses an earlier material rule. | `rules.py:227-247,280-344` |
| Reachability | Small Fundamental count spaces use exact projection and suffix minima. Larger spaces use the active-slot floor. | `worlds\checksmate\logic_projection.py:49-58,296-359` |
| Fixed floor | The large-state bound is active Chessmen times the contract's Pawn100 expectation. King promotion adds425 separately. | `logic_projection.py:177-182,319-357` |
| Cache context | Tables use `(stage,maxima)`. Active-count caches use `(stage,counts)`. Per-world context currently lacks profile/type restrictions. | `logic_projection.py:87-97,296-304,360-410` |
| Semantic input | Input includes counts, seeds, upgrades, unlocks, and a geometry baseline. No concrete type or candidate eligibility exists. | `worlds\checksmate\apmw_projection\models.py:91-118` |
| Allocation timing | The projector selects geometry, but does not pass it into Fundamental generation. | `worlds\checksmate\apmw_projection\semantic.py:155-199` |
| Family-only spending | Fundamental starts slots as Pawn100, spends Material400 units through fixed family upgrades, and retains spare credit. | `worlds\checksmate\apmw_projection\fundamental.py:66-123,179-219,255-344` |
| Authored allocation | Priority groups use eligible source counts times authored proportions. Tie counters advance for multiple viable actions. | `fundamental.py:301-369` |
| Stable ownership | Existing slot IDs and source roles are explicit. Results separately report active, reserve, dormant, and unallocated ledgers. | `models.py:131-146,193-230`, `fundamental.py:74-88` |
| Common grants | Primary royal starts with zero ordinary material. King promotion contributes425 each, Consul325, White50, and Pocket110. | `worlds\checksmate\apmw_projection\planning.py:87-150` |
| Current resource | Production selects `data/apmw_contract_v3.json`. v2 remains a separate compatibility resource. | `worlds\checksmate\apmw_projection\resource.py:17-40`, `test\test_contract_runtime_consistency.py:80-140` |
| Strict parser | Supported contracts are2.0/3.0. The parser fixes algorithms, field sets, and family expectations, including Pawn100 and Material400. | `worlds\checksmate\apmw_projection\contract.py:21-23,74-95,484-510,847-848` |
| Sidecar identity | Protocol1/runtime0.1.0 requires the exact frozen contract hash. It accepts only counts, seeds, mode, and optional upgrades. | `worlds\checksmate\apmw_projection\protocol.py:13-38,72-117,156-203` |
| World snapshot | `fill_slot_data` embeds the contract, its version, seeds, army IDs, options, and obtainable counts. | `worlds\checksmate\__init__.py:175-230` |
| Tracker gap | Python builds `WorldLogicProjection` without a snapshot contract. Overrides restore obtainable counts, not material profiles or Location estimates. | `__init__.py:330-389`, `logic_projection.py:60-73` |
| Export provenance | v2 fixture comments name `..\chessv\APMW.Test\Fixtures\ProjectionV2` and an explicit `Copy-Item` regeneration command. | `worlds\checksmate\test\test_semantic_projection_v2.py:1-9,50-69,135-175` |
| Generated copies | Production v3 JSON and `test\fixtures\projection-v3\baseline.json` must match byte-for-byte. v2 retains independently frozen hashes. | `test\test_contract_runtime_consistency.py:80-140` |
| Packaging | Sidecar build includes the v3 JSON and exports runtime/protocol/contract metadata. Release manifests reject inconsistent build metadata. | `worlds\checksmate\tools\build_apmw_projector.py:21-23,80-109,125-148`, `tools\create_apmw_projector_release_manifest.py:50-106` |

Full paths in the first citation establish the `worlds\checksmate` prefix for shorter citations.
The inspected world contains no concrete MG/EG registration, catalog export, or per-geometry opponent-cost table.
Family expectations in `contract.py` duplicate frozen semantic constants intentionally. They are not concrete piece prices.

## Why these boundaries matter

### Three different quantities

An item grant is fixed inventory credit. A concrete MG value is an integer price in a selected board profile.
A pawn-relative ratio is an analysis or display quantity. It does not define the integer unit or item denomination.
Pawn100 on every geometry is not an accepted calibration rule.

Existing code conflates the Pawn100 family expectation with both grant credit and a safe active-material floor.
The new contract must separate these meanings. Changing only `expected_material["pawn"]` changes grants and breaks fixed-denomination requirements.
The v2/v3 parser also rejects a changed expectation table.

### Funding versus spending

World funding uses the final board. A prepared match spends the same received inventory against its selected board.
Board previews must not regenerate items, change received credit, or change stable ownership IDs.
Selected types, active placement, reserves, and unspent credit can differ by geometry under the new explicit contract.

The current pool budget is a scaled historical threshold. It does not inspect selected army IDs or the concrete opponent roster.
The new budget needs an opponent-cost record before pool construction. It also needs a separate ledger for other grants and reserved item slots.

A proposed accounting shape is:

```text
opponent_cost = sum(concrete MG price for each chargeable final-board opponent piece)
target = approved funding policy(opponent_cost, difficulty, relaxation, explicit provision)
roster_credit = 100 * effective Chessmen + 400 * effective Material
common_credit = approved treatment of royals, White, pockets, and other existing grants
Material deficit = target minus approved non-Material contributions
```

This shape is a proposal, not an approved formula.
The owner must settle chargeable royal material, common-credit offsets, rounding, and the existing provision before implementation.
Item denominations remain fixed. A positive deficit can require a ceiling to whole Material items and an explicit residual.
Locked Castlers reclassify existing slots. Their funding must not enter the ledger twice.

The generator must distinguish required funding from a maximum allowance.
Merely raising the existing maximum does not prove that the random pool supplies the final-board target.
Existing capacity and item restrictions can prevent that target. Such cases require explicit diagnostics, not silent underfunding.

### Concrete allocation and conservative logic

The accepted prefix-to-elimination procedure differs from the current fixed-family upgrade waves.
The shared contract must define canonical family/type order, counter events, type limits, source roles, and affordable upgrade deltas.
The Python projector must receive enough candidate data to make the same decision as ChessV.

The exact allocation result need not increase monotonically with more Material.
AP reachability still requires conservative monotonic metrics. A suffix envelope can preserve this property.
Its cheap fallback needs a new proof against the eligible concrete prices.
It cannot inherit Pawn100 merely because a Chessman grants100.

Higher opponent prices also do not mechanically determine every tactical Location threshold.
Current thresholds include development and task difficulty estimates. The calibration owner must approve the relationship for each board.
Existing later-board bypass and target-clamp behavior require explicit acceptance. Otherwise they can conceal an insufficient funding or calibration result.

## Proposed shared contract

The following names describe proposed fields, not existing APIs.
The contract belongs in the existing ChessV export path and Python frozen-resource package.
No new service, package, runtime estimator, or parallel hand-maintained table is necessary.

| Contract element | Minimum proposed content | Consumer |
| --- | --- | --- |
| Material profile set | Schema/version, stable profile ID, canonical hash, source revision, unit definition | Both repositories, snapshots, caches |
| Piece catalog | Stable piece ID, existing family/army eligibility, deterministic ordering metadata | Both concrete allocators and opponent resolver |
| Geometry profiles | Explicit `{piece_id: {mg, eg}}` for exactly 6x8,8x8,10x8,10x10,12x10 | ChessV catalog, Python prices |
| Allocation identity | New algorithm identity with candidate filtering, order, counters, upgrade/refund rules | Local and sidecar parity |
| Eligibility context | Selected armies, pawn rules, relevant type limits, authored priorities/proportions, deterministic type seeds | Projection input and world logic |
| Opponent record | Deterministic per-board roster or canonical resolver inputs, chargeable roles, total MG, profile identity | Final-board funding and calibration |
| World snapshot | Full resolved price data or an embedded immutable profile export, contract hash, allocation identity, per-board Location expectations, funding ledger | Client, generator, tracker replay |

A standalone hashed profile payload is possible, but direct inclusion in the next contract is simpler for distribution.
Its content hash can still identify the profile independently.
The schema must keep inventory-credit constants separate from concrete MG/EG prices.
It must reject missing pieces, unknown geometries, invalid integers, stale hashes, and unsupported semantic combinations.
Old contract resources remain immutable.

No concrete 12x12 profile is part of this work.
Old 12x12 snapshots remain a compatibility concern, not an extension of the new price contract.

## Iterations, dependencies, and acceptance

| ID | Bounded action and likely components | Depends on | Acceptance and stop gate |
| --- | --- | --- | --- |
| G0 | Freeze joint spending/funding decisions and the data seam. Produce shared fixtures before production activation. | This report and ChessV registry plan | Resolve upgrade/refund accounting, counters, chargeable royals, +70 evidence, and opponent composition. No numeric repricing. |
| G1 | Extend the ChessV export and Python contract/resource parser in parallel. Add a complete profile set and candidate identities. | G0 data shape, accepted ChessV calibration | Exact 8x8 equality. Five explicit geometry profiles. One authoring source. Matching canonical hashes. Old v2/v3 fixtures unchanged. |
| G2 | Add concrete allocation under a new identity in `models.py`, `fundamental.py`, `semantic.py`, and `protocol.py`. Connect ChessV local/presentation code to the same result. | G0 allocator rules and G1 | Same concrete selection, counters, ledgers, reserves, and errors in both implementations. Material creates no Chessmen. No Legacy policy change. |
| G3 | Add final-board opponent valuation and funding reconciliation in `__init__.py`, `material_model.py`, `item_pool.py`, `item_removal.py`, and Castler helpers. | G0 funding decisions, G1 profiles, deterministic opponent recipe | Actual final-board MG total drives the approved target. Required Material reaches the target with fixed denominations. Capacity errors are explicit. No double credit. |
| G4 | Add board-specific Location expectations and concrete conservative logic in `locations.py`, `rules.py`, `logic_projection.py`. | G1, G2, G3 and accepted per-board calibration | All five boards, including compact, have approved behavior. Logic never exceeds actual material. Every single item increment preserves logic monotonicity. |
| G5 | Add world-bound profile/Location snapshots and old-world routing in `fill_slot_data`, tracker reconstruction, and ChessV readers. | G0 compatibility policy and G1 | Saved old worlds retain prices and semantic results. Unknown identities fail explicitly. New previews never change grants or world funding. |
| G6 | Activate the complete vertical path and release matching sidecar assets. Update directly affected docs and fixture provenance. | G2, G3, G4, G5 | Generator, local client, sidecar, tracker, and package manifests agree. No partial activation. Ordinary variants and Legacy pass regression smoke tests. |

G2 and G3 can proceed independently after their prerequisites.
G5 can proceed alongside them. G4 consumes both concrete semantics and the approved funding/calibration result.
Each iteration can use provisional test-only profiles before numeric approval.
Production activation remains blocked until the accepted full profile set and compatibility gates pass.

## Required shared fixtures

The allocation fixture retains the exact accepted illustrative values:

| Step | Required result |
| --- | --- |
| Start | 300 material, one available owned slot, no Gardener restriction. |
| First draw | Family3 selects Knight325. Reject it and consume the draw position. |
| Filter | Remove family5 because every eligible member exceeds300. Keep family3 because Gardener225 fits. |
| Next draw | Remap future seed values onto surviving family/type pools. Do not scan an unchanged family stream. |
| Emit | Emit Gardener225 and retain75. |
| Stop | No eligible candidate costs75 or less. Checkers40 is excluded from this fixture. |

These prices are illustrative, not a new catalog proposal.
The fixture must include concrete seed values, ordered candidate pools, and exact before/after counters.
Current Python contains neither Knight nor Gardener prices, so the fixture requires the new candidate contract.
No optimal-knapsack claim or unchanged fallback-tail claim applies.

Additional acceptance fixtures cover:

- A cheaper eligible member keeps its family available.
- Failed draws consume their positions without counter resets.
- Authored restrictions and per-type limits survive filtering.
- Priority/proportion rules remain explicit rather than uniform replacements.
- Exact-price purchases, empty pools, finite slot/upgrade progress, and budget-increasing refunds have defined results.
- Chessmen100 credit remains fixed even when the selected Pawn price differs.
- Locked Castlers consume existing slots and reconcile approved concrete debits without double funding.
- Royal and King-promotion evaluation stays separate from ordinary grant credit.
- Same inventory and seeds produce deterministic results per board.
- Different selected boards preserve inventory credit and stable ownership identities.
- Different opponent compositions at the same geometry produce the approved different funding targets.
- Different final boards produce whole-item Material targets and explicit residuals.
- Start inventory, start-from-pool, locked items, and reserved Locations enter the funding ledger once.
- Insufficient pool capacity or restrictive options produce explicit errors.
- Per-board reachability remains conservative for exact and large-state logic paths.
- Later-board bypasses and maximum clamps cannot silently substitute for funding acceptance.
- Old world snapshots retain their original price, Location, and allocation identities.
- Local and sidecar outputs match for every new profile and reject stale contracts.
- Build assets contain the exact contract/profile data named by release metadata.

## Existing test hooks and validation evidence

The existing focused suites are the natural owners:

| Concern | Existing suites |
| --- | --- |
| Schema, constants, copies | `test_apmw_contract_v2.py`, `test_apmw_contract_v3.py`, `test_contract_resource.py`, `test_contract_runtime_consistency.py` |
| Concrete semantic parity | `test_semantic_projection_v2.py`, new versioned cases beside `test\fixtures\projection-v2` |
| Sidecar and distribution | `test_apmw_projection_protocol.py`, `test_apmw_projector_build.py`, `test_apmw_projector_release_manifest.py` |
| Funding | `test_material_model.py`, `test_item_pool.py`, `test_item_pool_size.py`, `test_v2_itemization.py`, `test_castle_accessibility.py` |
| Location and logic | `test_geometry_location_profiles.py`, `test_rules_thresholds.py`, `test_logic_projection.py`, `test_location_logic.py` |
| World lifecycle | `test_board_series_integration.py`, `test_world_generation.py`, `test_world_state_pool.py`, `test_test_state_isolation.py` |

Two standalone source characterizations ran against the immutable extracted revision:

```powershell
$T = 'C:\Users\aaedi\.copilot\session-state\64a5e91f-a58b-4957-873c-19864a36415a\files\source-0772bac\worlds\checksmate\test'
python -m unittest discover -s $T -p test_semantic_projection_v2.py -q
# Passed: 21 tests.
python "$T\test_apmw_projection_protocol.py" -q
# Passed: 9 tests.
```

These results establish existing fixture/protocol behavior only.
They do not validate an unimplemented concrete-price path.
The full Archipelago world suites did not run in this main-derived child checkout.

## Delivery and remaining ownership

The coordinator can combine this plan with the ChessV registry plan.
The next deciding evidence is the opponent recipe and the unresolved accounting/counter contract.
The user/coordinator remains the authority for product questions and numeric acceptance.
The ChessV owner remains the authority for current C# exporter and registration paths.

No production file changed. No commit, push, PR, merge, or historical-session change occurred.
This child stops after delivery.
