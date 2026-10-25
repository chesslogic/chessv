---
goal: Keep Archipelago fill upstream and implement deterministic local army allocation
version: 1
date_created: 2026-10-23
last_updated: 2026-10-23
owner: Existing Common, A22, P1, and G2 owners
status: Planned
tags: [design, architecture, allocation, archipelago]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Authority update, 2026-10-25:
[ADR0024](../../../docs/adr/0024-seeded-hierarchical-allocation.md) and the
[current frontier](../engineering-closure.md#current-authority-and-frontier) record the current clause-level authority.
Later Fundamental answers permit preference-ordered post-prefix upgrades or additional Pawns and select affordability-filtered pool elimination.
This plan predates those answers.
Its exact tail, fixed-slot, and completion proposals need reconciliation before acceptance.
The original task rows remain planning records, not evidence of implementation.
Current delivery uses branch commits and coordinator integration, not PRs.
Historical initial-PR wording retains the substantive S0 authorization/declaration gate but does not require a PR.

Archipelago owns item placement and its accessibility algorithm. ChecksMate supplies truthful items, access rules, and generation inputs. ChessV constructs a legal army from those inputs.

This plan treats coupled completion as a local setup obligation, not a second multiworld solver. The local allocator protects mandatory funding and board capacity. It then consumes the seeded piece order.

The user requested an engineering plan, not production implementation. This document supplements the existing design packet. It changes no existing task status, package owner, or execution permission.

The October 21 confirmed successor supplies the behavior. The October 23 direction narrows the proposed implementation architecture.

## 1. Requirements & Constraints

### Ownership boundary

| Owner | Responsibility | Exclusion |
|---|---|---|
| Archipelago core | Item fill, locality, plando, collection traversal, and core accessibility evaluation | A replacement search, fill, or sphere algorithm in ChessV |
| ChecksMate generator adapter | Effective instructions, captured seeds, sound access rules, frozen world costs, and correct hook timing | Assuming that upstream accessibility repairs an incorrect local access rule |
| Shared local allocation semantics | Source-qualified funding, deterministic selection, active placement, mandatory completion, and useful guaranteed contributions | A general solver over the multiworld |
| ChessV client | Interpret the captured instructions and actual owned inventory through those semantics | New world-item placement, local repricing, or client-specific rerolls |

An incoming seed specifies choices. It is not a received piece, a funding source, or a placement witness.

### Required behavior

- **REQ-001**: Use the confirmed guarantee-first behavior in both runtime allocation and generator access estimates.
- **REQ-002**: Interpret each preferred entry as one final piece for an actual owned slot.
- **REQ-003**: Apply inclusive affordability, `>=`, through the retained mode-specific funding laws.
- **REQ-004**: Preserve stable world/player priorities independently of delivery chronology.
- **REQ-005**: Keep priorities unchanged after a Material-only increase under otherwise unchanged legal inputs.
- **REQ-006**: Extend ordinary-slot assignments canonically under unchanged legal choices.
- **REQ-007**: Select the longest affordable preferred prefix that permits the retained mandatory completion.
- **REQ-008**: Place ordinary promised pieces on the active board. Do not credit their presence in reserve.
- **REQ-009**: Select Castlers normally while protecting funding and compatible capacity for the feasible enabled target.
- **REQ-010**: Count natural Castler selections before the allocator constructs the remaining shortfall.
- **REQ-011**: Cover finite eligible tail alternatives once, in deterministic order.
- **REQ-012**: Bind tail pieces only to compatible unfilled slots. Keep guaranteed assignments and accepted tail pieces unchanged.
- **REQ-013**: Preserve the applicable Pawn baseline, source protections, dedicated actors, reserve obligations, and composition minimums.
- **REQ-014**: Permit lawful optional-piece changes only between setups. Keep active matches unchanged.
- **REQ-015**: Credit only certified access contributions. Do not use a favorable optional result as a guaranteed metric.
- **REQ-016**: Keep actual late inventory and plando rewards distinct from optional allocation results.
- **REQ-017**: Preserve the accepted Intelligence, post-terminal, and complete CV02 design/reference results.
- **REQ-018**: Obey effective hard bans, member eligibility, locked-role restrictions, and paid-quality laws.

### Implementation constraints

- **CON-001**: Do not replace `distribute_items_restrictive`, `flood_items`, or `MultiWorld.fulfills_accessibility`.
- **CON-002**: Do not introduce a general recovery graph, historical comparator, or multiworld trial driver for this allocator.
- **CON-003**: Keep source eligibility and lawful refunds explicit. A gross material total cannot substitute for typed funding.
- **CON-004**: Do not serialize hypothetical future rosters or concrete board coordinates into generated slot data.
- **CON-005**: Keep Legacy and Fundamental support across 6x8, 8x8, 10x8, 10x10, and 12x10.
- **CON-006**: Keep frozen costs, offers, and world bindings unchanged after fill.
- **CON-007**: Report contract errors explicitly. Do not add silent fallbacks, arbitrary retry caps, or normal seed-dependent withdrawal of promises.
- **CON-008**: Retain separate S0 authorization and initial-PR approval before consumer implementation.
- **CON-009**: Treat proposed helper names in this plan as internal implementation names, not existing public declarations.
- **CON-010**: Start with existing world hooks. Change a core lifecycle seam only for a demonstrated missing boundary under existing approval rules.
- **PAT-001**: Reuse `CounterBasedSeedSeries` and shared fixture vectors instead of platform-specific random shuffles.
- **PAT-002**: Keep one interpretation of allocation instructions across the C# engine, Python projector, sidecar, and live client.
- **PAT-003**: Use a concrete legal completion as the local safety witness. Do not use independent scalar minima as that witness.

### Proposed local construction

This sequence is an engineering proposal under the confirmed behavior. Phase 1 must supply its exact contract before implementation.

| Step | Local operation | Required result |
|---|---|---|
| Capture | Capture the effective instructions, seeds, inventory, legal rights, geometry, and actual Pocket inputs once | One immutable setup input, separate from mutable session globals |
| Assign | Derive stable priorities and legal preferred final-piece assignments before affordability | One finite preferred series with real slots and sources |
| Protect | Derive mandatory actors, composition requirements, and the feasible enabled Castler target | Typed protected funds and compatible placement capacity |
| Prefix | Evaluate preferred prefixes against actual funding and local completion | The longest accepted prefix, with unchanged preferred assignments |
| Tail | Visit each eligible later alternative once | Affordable legal additions that preserve the protected completion |
| Finish | Construct only the remaining mandatory shortfall and applicable fallback pieces | One legal active/reserve partition with complete accounting |
| Project | Derive placement, supported Pockets, and certified contributions from that setup | No reserve-only or cross-geometry false access |

The prefix evaluation can inspect the finite prefix lengths from longest to shortest. This is a local scan, not an Archipelago search.

Typed releases can invalidate scalar first-failure assumptions. The prefix scan must not stop at an unproved arithmetic shortcut.

The protection helper starts with count and rank-band constraints. Castler target enumeration has at most two applications under the retained cap.

The helper must also respect actual source restrictions and dedicated actors. A material reservation alone does not prove square compatibility.

Successful construction supplies a safety witness. A failed greedy construction does not prove that every legal completion fails.

Phase 1 must establish completeness for the proposed local helper. If a retained case defeats the simple rule, record that exact case. Add only the necessary local correction.

## 2. Implementation Steps

All tasks remain unstarted. A dependency authorizes ordering, not execution.

Paths beginning with `CM:` resolve under `C:\GitHub\rft50-checksmate`. Other code paths resolve under `C:\GitHub\chessv`.

### Implementation Phase 1

- **GOAL-001**: Specify the small local algorithm and demonstrate that its reservation guard covers the retained setup obligations.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Common with A22/P1: amend the series, slot, source, and cost cells in `.scratch\large-board-enemy-armies\contracts\shared-data-contract-v4.md`. Specify the exact root, series identifiers, canonical keys, counters, final-member enumeration, and tie rules. Reuse the existing SHA256 counter primitive. Preserve semantic and presentation separation. Specify chronology independence and qualified slot extension through explicit vectors. Mark this output as a proposed engineering contract until acceptance. | No | Not started |
| TASK-002 | A22/P1: specify internal `RequiredCompletion` and `TryAcceptFinalEntry` operations for Common's integration into `.scratch\adr-implementation-design\contracts.md`. Define their immutable inputs, typed debits, active placements, protected funds, and rollback result. Cover T/N, royals, protected Pawn sources, Castler shortfall, and actual Pocket support. Derive the feasible Castler target before optional spending. Show that natural Castlers reduce both the remaining target and its reservation. Prove the local guard complete for prefix acceptance, or isolate the exact unsupported case. Do not substitute an unspecified solver. | No | Not started |
| TASK-003 | P1 and the fixture owners: define independent expected cases in both planned ProjectionV4 `cases.json` files. Include exact seed vectors, source/debit traces, required completion, and active/reserve placement. Include positive and negative cases from Section 6. Freeze these expectations before runtime implementations consume them. | No | Not started |

Completion criteria: The series has exact reproducible vectors. The local guard has a bounded construction and a stated completeness argument. Independent fixtures contain concrete expected results. Unsupported cells remain explicit blockers.

### Implementation Phase 2

- **GOAL-002**: Close the generator design and existing pre-S0 gates before production implementation.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | G2 with A22/P1: specify useful guaranteed quantities for `CM:worlds\checksmate\logic_projection.py:WorldLogicProjection.metrics_from_counts` and `rules.py:set_rules`. Prove their covered-extension access guarantees from the local construction. Map the first dependent query, starts, links, plando, copies, freeze, and final acceptance in `CMWorld`. Define the existing stage-hook integration and exact conditional early seam. Join these results to the existing PM/LC design obligations. Do not edit production code. | No | Not started |
| TASK-005 | Common: compose TASK-001 through TASK-004 with the remaining existing D03 inputs. Update the allocation design in `spec.md`, `contracts.md`, and `engineering-closure.md` under `.scratch\adr-implementation-design`. Reuse accepted CV02. Obtain the existing complete pre-S0 design acceptance, separate S0 authorization, and initial-PR approval at their required boundaries. Do not promote this plan into those approvals. | No | Not started |

Completion criteria: The exact local algorithm, PM/LC design, and independent fixture definitions satisfy their existing gates. D03 acceptance precedes production implementation. Required S0 and consumer approvals remain separate.

### Implementation Phase 3

- **GOAL-003**: Implement matching local semantics in Python and the client after the required approvals.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | A22: implement the accepted local algorithm in `CM:worlds\checksmate\apmw_projection\planning.py`, `legacy.py`, and `fundamental.py`. Update `generate_legacy` and `generate_fundamental` through their existing owner interfaces. Preserve actual sources, payer allocations, lawful refunds, and mode-specific liabilities. Implement the finite prefix scan, shortfall reservation, tail pass, and completion. Use the independent fixtures. | No | Not started |
| TASK-007 | A22: update `APMW.Client\OwnedRosterGeneration.cs:GenerateLegacy` and `GenerateFundamental` to consume the same captured instructions and series. Reuse `ApmwSeedSeries` and `CounterBasedSeedSeries`. Bind actual `RosterPiece` sources and debits to the accepted local result. Remove conflicting old scheduling from the successor path only. Use the same independent fixtures. | No | Not started |
| TASK-008 | P1 then A22: integrate the result through Python `semantic.py:project_semantic_roster` and `placement.py:project_roster`. Route both exact-only Python metric entrypoints through the same accepted local construction. Update C# `ProjectionV2SemanticEngine.Project`, `GeneratedRosterProjector.Project`, and `ApmwProjectionBackend.cs` consumers. Preserve serial ownership. Prevent later activation from removing a promised active piece. Bind sidecar adaptation to the captured geometry, not an assumed first result. Make preview and live setup consume the same capture. Refuse stale results explicitly. | No | Not started |

Completion criteria: Both languages match the independent fixtures. Candidate rejection leaves the prior local state unchanged. The tail visits each admitted alternative once. Projection preserves promised active placements.

### Implementation Phase 4

- **GOAL-004**: Supply sound local rules and stable inputs to Archipelago's existing generation algorithm.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | G2 with A22/P1: implement the accepted TASK-004 metric design in `CM:worlds\checksmate\logic_projection.py:WorldLogicProjection.metrics_from_counts` and `rules.py:set_rules`. Exclude unproved optional benefits. Keep each geometry's material, role, Castler, and Pocket predicates together. Preserve the accepted coverage. Do not equate exact favorable output with a conservative metric. | No | Not started |
| TASK-010 | G2: update `CM:worlds\checksmate\__init__.py:CMWorld.create_items`, `set_rules`, and `fill_slot_data` through the existing lifecycle design. Bind effective instructions and seeds before their first dependent query. Account for start inventory, item links, retained plando, and late rewards at their actual lifecycle boundaries. Serialize the same frozen world object. Do not treat initial `create_items` counts as final by assumption. | No | Not started |
| TASK-011 | G2: add `CMWorld.stage_pre_output` through the existing AutoWorld stage-hook dispatch. Call the real `MultiWorld.fulfills_accessibility` and propagate false through the existing generation error path. Cover normal, no-output, spoiler, and mixed-world routes. Retain the upstream fill and output checks. If an earlier required barrier is absent, submit only that exact seam through the existing conditional-core ownership route. | No | Not started |

Completion criteria: The real upstream fill consumes the local rules without replacement. Invalid local guarantees fail focused tests. No-output generation cannot bypass final core acceptance. Publication does not redraw seeds or reprice costs.

### Implementation Phase 5

- **GOAL-005**: Validate implementation conformance within the existing package acceptance sequence.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | A22/P1: extend `OwnedRosterGenerationTests.cs`, `ActiveRosterProjectionTests.cs`, `ApmwProjectionBackendTests.cs`, and the fixture parity tests. Wire the planned ContractV4/ProjectionV4 resources into `APMW.Test.csproj`. Compare ordered pieces, sources, debits, reservations, active squares, and residual funds. Join the Python metric and real generation-route tests. | No | Not started |
| TASK-013 | Existing integration owners: validate the implementations against the accepted CV01 design inputs and required native handoffs. Include strict binding, stale-capture, restoration, and world-lifecycle cases. Reuse complete CV02 design acceptance while preserving its later native obligations. Do not replace native evidence with a model or path audit. | No | Not started |
| TASK-014 | Common: record implementation results and exact remaining gaps in the existing package acceptance records. Preserve AL/PM/LC/CV01/D03 ownership and serial transfers. Keep library, package, and release acceptance in their existing later packages. Do not create another roadmap or revive parked Historical work. | No | Not started |

Completion criteria: Implementations conform to the previously accepted design. Earlier CV02 results remain accepted. Every later implementation or release step retains its explicit permission boundary.

## 3. Alternatives

- **ALT-001**: Replace Archipelago fill with a ChessV solver. Reject this approach because upstream owns the multiworld algorithm.
- **ALT-002**: Build a general allocation/recovery graph first. Reject this prerequisite because the selected behavior requires a finite local sequence.
- **ALT-003**: Reserve only a material scalar for Castlers. Reject this approach because source eligibility, compatible slots, and home-rank capacity also constrain completion.
- **ALT-004**: Construct all Castlers before normal selection. Reject this approach because natural selections must count before shortfall completion.
- **ALT-005**: Credit one seeded favorable roster in access rules. Reject this approach because later legal inputs can change its optional contributions.
- **ALT-006**: Freeze one concrete roster in slot data. Reject this approach because runtime inventory changes and the contract carries instructions.
- **ALT-007**: Preserve every old scheduler operation. Reject this requirement because the confirmed successor replaces conflicting selection procedures.

## 4. Dependencies

| Task | Prerequisites | Parallel-work rule |
|---|---|---|
| `TASK-001` | Confirmed successor and existing schema ownership | Common owns the shared contract edits |
| `TASK-002` | Confirmed successor and retained typed-funding laws | Develop with TASK-001, using a separate design surface |
| `TASK-003` | TASK-001, TASK-002 | Freeze independent expectations before either runtime consumes them |
| `TASK-004` | TASK-001, TASK-002, TASK-003 and existing PM/LC inputs | G2 owns the generator design surface |
| `TASK-005` | TASK-004 and all other existing D03 prerequisites | Preserve the complete design-before-implementation gate |
| `TASK-006` | TASK-005, required S0 implementation, and consumer approval | A22 owns Python allocation files |
| `TASK-007` | TASK-005, required S0 implementation, and consumer approval | Can proceed beside TASK-006 on separate files |
| `TASK-008` | TASK-006, TASK-007 and the P1 handoff | Serialize shared C# and projection writes |
| `TASK-009` | TASK-004, TASK-005, TASK-006 and consumer approval | Can proceed beside client-only work |
| `TASK-010` | TASK-009 and existing lifecycle inputs | G2 owns world/rule integration |
| `TASK-011` | TASK-010 and applicable core-seam approval | No automatic core write permission |
| `TASK-012` | TASK-008, TASK-009, TASK-011 | Run against the previously accepted paired expectations |
| `TASK-013` | TASK-012 and existing native handoff prerequisites | Distinguish implementation conformance from pre-S0 design acceptance |
| `TASK-014` | TASK-013 | Common records results without expanding acceptance |

- **DEP-001**: The confirmed successor v2 controls allocation behavior. This plan proposes an implementation architecture, not a new gameplay policy.
- **DEP-002**: The final shared contract must supply exact series and typed-funding records before either language implements them.
- **DEP-003**: Existing Army, Location, calibration, and contract work supplies complete fixture inputs. This plan does not reimplement those packages.
- **DEP-004**: Existing AL/PM/LC/CV01 dependencies remain. Archipelago ownership does not waive the correctness of local rules or hook timing.
- **DEP-005**: Existing S0 authorization and initial-PR approval remain implementation prerequisites.

## 5. Files

Existing symbols describe the inspected source. New symbols and fixture directories are explicitly marked as proposed.

- **FILE-001**: `.scratch\large-board-enemy-armies\contracts\shared-data-contract-v4.md`: existing instructions, source, cost, and fixture contract.
- **FILE-002**: `APMW.Client\OwnedRosterGeneration.cs`: existing `CounterBasedSeedSeries`, `ApmwSeedSeries`, `GenerateLegacy`, and `GenerateFundamental`.
- **FILE-003**: `APMW.Client\ItemGeneration.cs`: existing funding constants and historical generation helpers. Change only coupled successor consumers.
- **FILE-004**: `APMW.Client\ActiveRosterProjection.cs`: existing `GeneratedRosterProjector.Project` and active/reserve admission.
- **FILE-005**: `APMW.Client\ProjectionV2SemanticEngine.cs`: existing pure C# semantic projection. Its historical filename does not define successor compatibility.
- **FILE-006**: `APMW.Client\ApmwProjectionBackend.cs`: existing live and sidecar backend integration. Existing sidecar capture/adapter files remain in their assigned scope.
- **FILE-007**: `APMW.Test\OwnedRosterGenerationTests.cs`, `ActiveRosterProjectionTests.cs`, `ApmwProjectionBackendTests.cs`, `ApmwPawnDistributionTests.cs`, and `ProjectionV2FixtureParityTests.cs`: existing test surfaces.
- **FILE-008**: `APMW.Test\APMW.Test.csproj`: existing fixture-copy declarations. `Fixtures\ContractV4\p8.json`, `p12.json`, and `Fixtures\ProjectionV4\cases.json` are planned additions.
- **FILE-009**: `CM:worlds\checksmate\apmw_projection\planning.py`, `legacy.py`, `fundamental.py`, and `semantic.py`: existing local allocation functions.
- **FILE-010**: `CM:worlds\checksmate\apmw_projection\models.py`, `contract.py`, and `placement.py`: existing input, seed, result, and placement types.
- **FILE-011**: `CM:worlds\checksmate\logic_projection.py`, `rules.py`, and `__init__.py`: existing local metrics and world hooks.
- **FILE-012**: `CM:worlds\checksmate\test\test_semantic_projection_v2.py`, `test_logic_projection.py`, `test_world_generation.py`, `test_location_logic.py`, and `test_lifecycle_capacity.py`: existing Python test surfaces. `fixtures\projection-v4\cases.json` is planned.
- **FILE-013**: `.scratch\adr-implementation-design\spec.md`, `contracts.md`, and `engineering-closure.md`: existing design acceptance and owner records.
- **FILE-014**: `CM:Main.py`, `Fill.py`, `worlds\AutoWorld.py`, and `BaseClasses.py`: existing upstream seams. Inspection and integration tests are the default scope. Changes remain conditional.

## 6. Testing

### Independent acceptance cases

- **TEST-001**: Compare exact cross-language PRF and series vectors. Include a tie and canonical ordering independent of collection enumeration.
- **TEST-002**: Replay identical captures and reordered deliveries. Require identical output without additional draws.
- **TEST-003**: Add only Material. Require unchanged priorities and preserved certified access.
- **TEST-004**: Add an ordinary slot under unchanged legal choices. Require canonical extension without a new inventory grant.
- **TEST-005**: Exercise funding immediately before, at, and after an entry's lawful cost. Exact funding must qualify.
- **TEST-006**: Reject apparent affordability from protected bases, dedicated funds, reserve locks, or Pocket reachability credit.
- **TEST-007**: Preserve the Legacy 785 source, 500 concrete debit, 485 expected value, and 285 residual distinction.
- **TEST-008**: Cover Castler targets zero, one, and two. Cover natural fulfillment, partial fulfillment, Ignore Castlers, scarce funding, and incompatible capacity.
- **TEST-009**: Make a normal candidate compete with an unmet Castler or composition obligation. Require preservation of the legal completion.
- **TEST-010**: Use an expensive skipped entry followed by a smaller legal entry. Require one tail visit without revisiting or replacing accepted extras.
- **TEST-011**: Force a funded piece into reserve in the old projector. Require the successor to preserve promised active placement or reject that prefix.
- **TEST-012**: Cover both modes and all five geometries, including minimum-attaining and shortage cases. Do not create missing slots.
- **TEST-013**: Replace a next-setup optional piece lawfully. Require preserved certified access and unchanged active-match state.
- **TEST-014**: Exercise BOARD-dependent Pocket support and mixed-geometry false positives. Credit only a complete geometry-local rule.
- **TEST-015**: Exercise starts, links, retained plando, late rewards, copied state, and stale captures without duplicate sources or repricing.
- **TEST-016**: Run real upstream fill and core accessibility on normal, no-output, spoiler, and mixed-world routes.
- **TEST-017**: Reject a deliberately false local rule promise through the acceptance harness. Successful upstream fill alone must not certify that promise.
- **TEST-018**: Compare successor outputs against independent paired expectations. Keep legacy suites at their original evidence scope.
- **TEST-019**: Compare exact-only metric entrypoints with the full local construction. Bind every sidecar result to its captured geometry.

### Commands for authorized implementation

These commands are future validation steps. Planning does not authorize their execution.

Run the focused client command from `C:\GitHub\chessv`:

```powershell
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~OwnedRosterGenerationTests|FullyQualifiedName~ActiveRosterProjectionTests|FullyQualifiedName~ApmwProjectionBackendTests|FullyQualifiedName~ApmwPawnDistributionTests|FullyQualifiedName~ProjectionV2FixtureParityTests"
```

Run the focused generator command from `C:\GitHub\rft50-checksmate`:

```powershell
$env:AP_TEST_WORLDS = 'checksmate'
python -m pytest -q worlds\checksmate\test\test_semantic_projection_v2.py worlds\checksmate\test\test_logic_projection.py worlds\checksmate\test\test_world_generation.py worlds\checksmate\test\test_location_logic.py worlds\checksmate\test\test_lifecycle_capacity.py
```

TASK-011 must exercise the actual generation entrypoint in the existing world tests. Calling only the fill function does not cover lifecycle returns.

## 7. Risks & Assumptions

- **RISK-001**: A naive reservation can protect sufficient money but leave no legal Castler square.
- **RISK-002**: A greedy completion can miss a legal alternative. Its rejection cannot certify the longest feasible prefix without a completeness argument.
- **RISK-003**: A later projection can silently remove a promised piece through old activation priority.
- **RISK-004**: A raw inventory or expected-material sum can spend locked funds or lose original Legacy credit.
- **RISK-005**: A favorable optional result can create an unsound access rule.
- **RISK-006**: An upstream early return can bypass final accessibility. Existing source places `skip_output` before its ordinary output-stage accessibility task.
- **RISK-007**: Historical filenames and tests can imply old semantics. Successor acceptance requires explicit independent expectations.
- **ASSUMPTION-001**: The inspected Archipelago fill remains the upstream algorithm. This plan does not change its search strategy.
- **ASSUMPTION-002**: The simple local reservation approach is a proposal. Phase 1 must establish its coverage before implementation.
- **ASSUMPTION-003**: The confirmed October 21 behavior remains authoritative except for a later explicit user change.
- **ASSUMPTION-004**: Native engine, reporting, package, and release acceptance retain their existing separate scope.

## 8. Related Specifications / Further Reading

| Source | Relevant content |
|---|---|
| `C:\Users\aaedi\.copilot\session-state\25c7b461-3d4d-419f-961a-888fe5699cef\files\common-guarantee-first-allocation-successor-v2.md`, sections 2-7 | Confirmed behavior, open engineering, and preserved acceptance |
| `C:\Users\aaedi\.copilot\session-state\25c7b461-3d4d-419f-961a-888fe5699cef\files\common-cv02-pres0-composition-acceptance-v1.md`, sections 3-5 | Accepted lifecycle design/reference scope and later native obligations |
| `.scratch\adr-implementation-design\spec.md`, P1, A22, G2D, G2, and U1 | Existing packages and serial ownership |
| `.scratch\large-board-enemy-armies\contracts\shared-data-contract-v4.md`, sections 6 and 10 | Shared allocation semantics and complete fixture requirements |
| `docs\adr\0022-retain-legacy-pawn-material-conversion.md` | Retained funding and mode history, subject to the confirmed successor |
| ChessV source `c389ffac8a605e93fa336ffac4c379816a15d717` | Inspected C# source baseline |
| ChecksMate source `0772bac724ff483043b56979f0ef078b9c2d264a` | Inspected upstream and generator baseline |
| `CM:Main.py:109-239` | World creation, rules, starts, links, plando, fill, pre-output, and early returns |
| `CM:worlds\AutoWorld.py:202-225` | Existing stage-hook dispatch |
| `APMW.Client\OwnedRosterGeneration.cs:246-568` | Existing seed primitive and live mode dispatch |
| `APMW.Client\ActiveRosterProjection.cs:335-493` | Existing placement and material-conservation boundary |
| `CM:worlds\checksmate\apmw_projection\semantic.py:96-126` | Existing local semantic projection entrypoint |

This document is a planned engineering supplement. It is not a passing result, a production contract, or implementation authorization.
The current authority links take precedence over conflicting historical task wording.
