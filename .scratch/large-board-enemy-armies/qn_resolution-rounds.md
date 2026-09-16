# Large-board decision-resolution question network

Status: Design closure confirmed at Q35. No open decision frontier
Authority: Navigation only. The linked tickets and ADRs own the decisions.

## Purpose and boundary

The [map](map.md) seeks a specified cross-repository army, Location, and material
contract. The current work resolves its questions, not production behavior.
The formal geometry set excludes 12x12.

## Landed baseline

The earlier family and formation decisions remain fixed.
The interview added these decisions:

| Decision | Owning record |
| --- | --- |
| Exact 10x10 candidate C for all four families and both colors | [Exact 10x10 arrays](issues/12-normalize-ten-by-ten-arrays.md) |
| Exact 12x10 arrays, fourteen pawns, and a universal 500/500 Lion pair on B1/K1 | [Exact 12x10 arrays](issues/13-normalize-twelve-file-arrays.md) |
| Extinction with multiple CPU Kings, checkmate with one, explicit terminal table, no castling inheritance, non-royal Amazon | [CPU royal survival phases](../../docs/adr/0003-cpu-royal-survival-phases.md) |
| Rearguard is starting capture-map metadata, not a pawn behavior | [Location identity](issues/15-define-location-role-identity.md) |
| Standard-family Location calibration and midgame values | [Standard-army calibration](../../docs/adr/0004-standard-army-location-calibration.md) |
| ChessV publishes army data; ChecksMate owns AP mappings and authored difficulty | [Shared deployment data](../../docs/adr/0001-shared-exact-deployment-data.md) |
| Reject 12x12 entry without a legacy support path | [Unsupported geometry boundary](../../docs/adr/0002-reject-unsupported-twelve-by-twelve.md) |
| The 0.4.0 client supports the new contract; older worlds use matching older clients | [Breaking-release boundary](../../docs/adr/0005-break-contract-compatibility-at-zero-four.md) |
| Stable Standard/FIDE capture-role names and separate Center Rook and Queen Locations | [Starting-role names](../../docs/adr/0006-name-capture-locations-by-starting-role.md) |
| One individual check per starting non-King unit, with no Queen's Rook alias for the compact Center Rook | [Starting-role names](../../docs/adr/0006-name-capture-locations-by-starting-role.md) |
| Capture Everything uses the configured ending geometry within one match | [Capture rules](issues/15-define-location-role-identity.md) |
| Regicide and King counts follow the initial multi-King setup; Everything clears all non-Kings and spare King lives | [Starting-army capture counts](../../docs/adr/0007-count-captures-from-the-starting-army.md) |
| Pawn counterpart names and separate Rearguard names | [Role catalog](issues/15-define-location-role-identity.md#canonical-role-and-coordinate-catalog) |
| Every integer capture-series threshold through the endpoint ceilings, with earlier-board completion when counts are met | [Capture-series contract](issues/15-define-location-role-identity.md#capture-series-contract) |
| Captures-minus-one player-chessmen estimates and separate geometry-matched calibration profiles | [Calibration and reachability](../../docs/adr/0008-separate-calibration-from-reachability.md) |
| Provisional endgame budgets from established 8x8/10x8 anchors and exact inventory deltas | [Endgame anchors](../../docs/adr/0009-anchor-provisional-endgame-budgets.md) |
| Retained later-board resource bypass, unit precision, and explicit within-series monotonicity | [Calibration and reachability](../../docs/adr/0008-separate-calibration-from-reachability.md) |
| Endpoint-anchored capture-series transfer, with explicit correction records | [Series transfer](../../docs/adr/0010-transfer-capture-series-curves-by-endpoints.md) |
| Retained task-specific budgets and the True Royal Fork checkmate link | [Task-specific calibration](../../docs/adr/0011-preserve-task-specific-calibration.md) |
| Half-growth access floors for bounded non-pawns and quarter-growth floors for sliders | [Accessibility growth](../../docs/adr/0012-floor-non-pawn-accessibility-growth.md) |
| Released non-pawn reference bank, Knight analogues for Lions, and compact downward transfer | [Individual references](../../docs/adr/0013-use-released-individual-capture-references.md) |
| Smooth, side-specific pawn curves, the central-pressure law, and a quarter-accessibility Rearguard premium | [Pawn model](../../docs/adr/0015-price-pawns-with-curves-and-rearguard-premiums.md) |
| Regicide at three quarters of the Queen-to-checkmate gap | [Regicide tier](../../docs/adr/0014-price-regicide-between-queen-capture-and-victory.md) |

## Breadth map

These IDs retain the branches from the earlier session question network.

| ID | Question | Resolver | Edges and effect | Status |
| --- | --- | --- | --- | --- |
| Q1 | Which statements are decisions rather than evidence or proposals? | Source analysis and user answers | Owns classification across the packet | Settled for the recorded rounds |
| Q2 | What are the exact family arrays? | User with prototype and primary-source inspiration | Both enlarged arrays are resolved. The later Lion correction changes only the 12x10 B1/K1 pair | Settled |
| Q3 | What are the royal and castling semantics? | Source analysis and user decisions | Royal, castling, promotion, and Minor Elephant policies have concrete fixtures. Q35 confirms the preserved evaluation defaults | Resolved |
| Q4 | What capture roles, display names, and mappings are required? | User and ChecksMate data owner | The semantic Location record resolves the catalog, Regicide, counters, clear condition, series ceilings, and count-based availability | Settled |
| Q5 | What exact material equation and authored data define requirements? | User and ChecksMate data owner | Q35 confirms the selected functions, tables, correction schema, and exact-to-effective fixtures | Resolved |
| Q6 | What is the complete cross-repository contract? | ChessV and ChecksMate owners with user decisions | Q35 confirms shared schemas, cost snapshots, hash direction, strict gates, and publication order. Real pins are later build outputs | Resolved |
| Q7 | What ordinary-game exposure is intended? | User through a separate scope decision | Does not block the APMW-only array decision | Deferred from this frontier |
| Q8 | Which implementation slices can run in parallel? | Integration owner | Existing plan supplies candidate file ownership. Fresh execution handoffs still required | Planned, not authorized |
| Q9 | What permits publication and implementation handoff? | Map owner and execution workflow owner | Q35 confirms shared understanding. A separate handoff and execution authorization remain necessary | Resolved for design |
| Q10 | Should Regions express board and shared-goal reachability? | User | The user deferred graph refactoring. Board-specific thresholds remain in scope through complete rule alternatives | Deferred from 0.4.0 |

## Representative cases

| Case | Exact point that must survive later work |
| --- | --- |
| 10x10 overlap | E3/F3 hold one pawn each. The approved total is twelve pawns and 28 CPU pieces |
| 10x10 candidate C | C2/H2 are basic Elephants; D2/G2 are Knight-role; E2/F2 are Bishop-role |
| Color reflection | Files stay fixed. Zero-based rank `r` becomes `9-r` |
| Multiple-King phase | No check obligation while multiple CPU Kings survive |
| Last-King phase | The surviving King uses checkmate behavior, regardless of which King survived |
| Checkers multi-capture | Capturing both Kings in one turn reaches extinction defeat directly |
| CPU no moves | Multiple Kings or an unattacked last King produce CPU victory. An attacked last King produces checkmate defeat |
| Rear-pawn capture | Starting-map identity determines the Location after movement; no new pawn behavior exists |
| Other enemy family | Its easier or harder Locations do not change the Standard-based generator requirements |
| 12x10 revision | E1/H1 are Bishop-role, D1/I1 are Knight-role, and B1/K1 are Lions. The old gaps and Champion proposal are superseded |
| 6x8 to 8x8 | Center Rook and Queen are separate checks, not two ways to complete one Location |
| Six Rookies Lions | Home Bishop, forward Bishop, and outer Lion roles retain distinct identities despite the same concrete piece kind |
| Mixed release | An old contract requesting 8x8 remains incompatible with the 0.4.0 client |
| Shared Queen goal | Several board-specific access paths must lead to one Location, not duplicate checks |
| Mixed-path failure | Material on one board and castlers on another do not combine into a qualifying path |
| Endgame reference | New 12x10 intrinsic budgets are 11220/11250, not the rejected 11920/11950 continuation |

## Pass 2 questions

This pass widens the map after the source-history work and Regions suggestion.
These branches remain visible together. Numeric calibration does not block
the graph's scope decision.

| Branch | Question and why it matters | Representative cases | Resolver and route | Edges | Status |
| --- | --- | --- | --- | --- | --- |
| Q5-a | How does individual accessibility transfer to a changed formation? This sets the remaining individual thresholds | Low-end Knights versus sliders, curved pawn file costs, Rearguard Pawn, Regicide versus Queen | User through the material-calibration interview | Reference bank, compact transfer, curve, Rearguard premium, and Regicide tier are selected | Settled provisionally |
| Q5-b | How do endgame-oriented series curves continue to the new counts? This controls progression pacing | 14 Pawns, 19 Pieces, 14 Of Each, Any 32 | [Series-transfer decision](../../docs/adr/0010-transfer-capture-series-curves-by-endpoints.md) | Accepted function supplies the series table. Corrections remain explicit | Settled provisionally |
| Q5-c | Which tactical and movement estimates change with the new formations? This prevents an unjustified blanket delta | Threaten King with two targets, Royal Fork, late King movement, no castling on 6x8 | [Task-specific calibration](../../docs/adr/0011-preserve-task-specific-calibration.md) | Budgets are fixed. Capability and special-condition fixtures feed acceptance | Settled provisionally |
| Q5-d | Which reference roles and values belong to the released or tested baseline? This prevents untested additions from becoming authority | Outer Attendants, pawn I/J/K/L, grand series tails, task entries | [Reference-provenance report](research/calibration-reference-provenance.md) | The full selected base8/grand10 bank is released. Outer Attendants and K/L are later; release presence is not play-test proof | Completed |
| Q10 | Is a Regions refactor in scope, and what graph preserves the interface? This changes generator structure, not capture identity | Shared Queen, start-at-10x8, endpoint Everything, mixed-path failure | [Regions decision](issues/20-decide-region-based-reachability.md) | Refactor deferred. Per-board rules and mixed-path prevention still feed acceptance | Deferred |
| Q6 | How are the data and pins distributed and validated? This joins the two consumers | Matching contract, old-world rejection, real packaged projector | Shared-data specification confirmed at Q35 | Required source inputs and the Regions deferral are settled | Resolved |

## Current questions and waysigns

The user approved the complete 12x10 array, then replaced its Champion pair
with the existing 500/500 Lions on B1/K1. The two source probes are complete.
[Twelve-file piece inspiration](research/twelve-file-piece-inspiration.md)
contains the precedents and the consolidated movement/catalog assessment.

The Lion pair reuses an existing APMW piece and image. Champion-specific
registration and image work is no longer part of the new army.
The capture contract is resolved. The
[material history report](research/material-calibration-history.md) supplies
the source evidence for the next function choices.
The local-only equation remains unaccepted.
The rule fixtures and shared-contract specification are complete for the
closure review.
They do not reopen the arrays or release boundary.

The [series comparison](research/capture-series-model-candidates.md) led to the
accepted [endpoint-anchored model](research/capture-series-endpoint-model.md).
The [0.3.2 reference audit](research/calibration-reference-provenance.md) is
complete. It confirms the core released references and identifies later
Outer Attendant and K/L entries. Round 12 selected the released analogue bank,
including Knight references for Lions and compact downward transfer.
It also selected the smooth pawn curve and central-pressure law.
Round 13 selected a Rearguard premium of one quarter of the same-file main
pawn's accessibility budget, excluding its 100-unit pawn value.
Regicide uses three quarters of the Queen-to-checkmate gap, not the earlier
4725 proposal.

The Regions suggestion has its own decision record. The user retained the
bypass but deferred graph refactoring. Per-board access costs remain part of
the accepted design and do not require multiple parent Regions.

Exact Location naming follows those arrays. Numerical calibration follows the
role contract. Contract assembly and final acceptance depend on those complete
inputs, not just their accepted high-level policies.

## Pass 3: After numeric model selection

This pass restores breadth after the calibration interview.
The [individual-capture report](research/individual-capture-model.md) supplies
105 exact profiles. No remaining branch can silently replace the selected
reference bank, Rearguard coefficient, or Regicide tier.

| Branch | Remaining question | Why it matters and representative case | Resolver and next route | Blocking effect | Status |
| --- | --- | --- | --- | --- | --- |
| Q3-a | What lifecycle and actor-identity fixtures encode the selected royal rules? | Undo after primary-King capture, repeated load, failed committed move, and direct two-King extinction | [Royal lifecycle fixtures](contracts/royal-lifecycle-fixtures.md) | Exact integration positions and fault inputs are specified. The multiple-King no-move case is explicitly a policy-unit fixture | Specified for implementation |
| Q3-b | Which existing evaluations and tactics must recognize each new starting type? | Mounted King in threats/forks, non-royal Amazon, and CPU-only Elephant registration | Source report, Q34, and Q35 | Q34 selects Minor Elephant without human pool or promotion permissions. Q35 confirms the preserved evaluation coefficients | Resolved |
| Q3-c | What are the exact CPU promotion permissions? | Explicit and absent Standard inputs, human pocket entitlements, basic Elephant, and the removed legacy12 Nightrider extension | [CPU promotion ADR](../../docs/adr/0017-separate-cpu-promotion-permissions.md) | Q33 selects explicit family/geometry lists, independent of human entitlements, with no additional targets | Settled |
| Q5-e | How are corrections and exact intrinsic profiles stored and reviewed? | A rational Regicide value, a geometry-specific offset, and a monotonicity conflict | [Cost-snapshot schema](contracts/world-cost-snapshot-v1.md) | Exact and effective fields, correction records, reconstruction fixtures, and literal shared-contract bindings are specified | Specified |
| Q5-f | Which complete geometry paths qualify under capabilities and projection limits? | No 6x8 castle, Queen-dependent tactics without a starting Queen, and no mixed-board prerequisites | Royal source report, Q33, and acceptance fixtures | Selected promotions provide the Standard 6x8 Queen-tactic path. No starting Queen capture or 6x8 castling path is invented | Policy and logical fixtures specified |
| Q6-a | How do pinned army data, AP mappings, and runtime packages move between repositories? | A vendored army artifact, incompatible layout/Location versions, and the real restored sidecar | [Shared-data specification](contracts/shared-data-contract-v4.md) | Q35 confirms the reviewed schemas and publication order | Resolved |
| Q6-b | Which explicit compatibility fields reject unsupported pairs? | Old 8x8 world, new world with an old client, and matching 0.4.0 labels with different data | Shared-data specification and its paired wire fixtures | Strict gates cover the v2/v3 and build-manifest mismatches without weakened validation | Specified |
| Q6-c | How and when do both consumers bind to synchronized costs? | The planned built-in tracker reads the same per-world costs as the generator, without an independently compiled price table | [Cost-snapshot ADR](../../docs/adr/0016-publish-world-bound-location-cost-snapshots.md) | Q32 selects the snapshot in 0.4.0 and defers tracker UI | Settled |
| Q9 | When is the map complete enough for execution handoff? | Every branch has authority, fixtures, ownership, and a resolved prerequisite | Q35 design sign-off | Ready for a later handoff, not authorized for production work | Resolved for design |

The completed contract probe was read-only. It did not choose distribution
policy or repair the absent execution adapter.
Arrays, role identities, Region deferral, and the release boundary remain
settled.
The planned built-in tracker adds a future consumer of Location costs.
Its cost-data dependency belongs in this contract discussion.
Its UI implementation does not become part of this effort by implication.

## Q34 and Q35 answers

The user selected: "Minor tactical target, without human pool or promotion
permissions".
Ticket 14 owns this decision.
The target-class profile and isolated Elephant threat fixture agree.

At Q35, the user selected: "Confirm the design; mark the map ready for a
later handoff".
This confirms the preserved evaluation defaults, reviewed shared schemas,
publication sequence, acceptance ownership, and unchanged scope exclusions.
No decision frontier remains open.
The sign-off does not authorize production implementation or publish a
handoff.

## Deferred and out of scope

Production patches, threshold tuning from play tests, generator edits, and
release publication remain outside this interview.
Ordinary-game UI and rule symmetry need their own bounded decision.
The Regions refactor is deferred from 0.4.0. A future design can use several
entrances into a shared goal Region. Direct multi-parent registration is not
required for the user's door-cost model.
Nightriders remain excluded from the proposed new 12x10 pair.
Twelve-file games on other heights can supply inspiration without becoming
supported target geometries.

## Source update map

| Source | Currentness |
| --- | --- |
| Owning map, issues, glossary, and seventeen ADRs | Working-tree documents include the current user answers |
| C# source and movement definitions | Observed code baseline `888710f99cf007b982ea1f6a8a64c1e8058be196`; no production edits |
| Approved 10x10 prototype | Local branch `prototype-ten-by-ten-715de4c1`, immutable commit `285d59741917bde4f9ed143ca85e61fe7d451c05` |
| Approved 12x10 Lion prototype | Local branch `prototype-large-board-armies-715de4c1`, immutable commit `777e2c778ed7d26feae525da9415d62af3d5c469` |
| Original session planning packet | Historical snapshot, superseded where the later interview records a decision |
| Twelve-file inspiration report | Current local source survey and fit assessment; source examples do not choose the new piece |
| Generator graph source | One Menu Region at sibling `worlds\checksmate\__init__.py:281-295`. Single-parent and unique-name constraints at `BaseClasses.py:1295-1299,1481-1514` |
| Decision-to-execution route | Generic Wayfinder compatibility passed. The repository adapter remains absent, so automatic projection/implementation routes remain disabled |
