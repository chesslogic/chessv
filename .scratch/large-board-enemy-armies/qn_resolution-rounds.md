# Large-board decision-resolution question network

Status: Follow-up audit complete. Additional settings decisions remain open
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

## Pass 4: Follow-up breadth audit, 2026-09-16

The user requested another audit before the later handoff.
The subjects are architecture, algorithms, data structures, item-flag
portability, Options, and historical behavior.
Q35 remains the authority for the bounded army and calibration design.
This pass does not reopen its choices or authorize production work.

| Branch | Question | Why it matters and cases | Resolver | Edges | Status |
| --- | --- | --- | --- | --- | --- |
| Q11 | Which architectural interfaces or algorithm contracts still need specification? | Stable identity, committed events, reconstruction, determinism, and independent implementation ownership | Later engineering specification | Most behavior is settled. Bare-FEN ingress feeds Q14 | Investigated: bounded engineering work |
| Q12 | Is Fundamental itemization portable end to end? | Canonical mode values, authoritative world data, backend parity, reconnect, and world switches | User clarified the referent. Engineering owns the remaining integration | Settings normalization and the local Castlers override feed Q13 | Meaning and representation settled; integration incomplete |
| Q13 | Is there one supported Options matrix across generation, slot data, client, and GUI? | Defaults, supported combinations, invalid values, and world versus local authority | One proposed settings decision record, with user choices separated from engineering work | Constrains Q14 and the release contract | No complete end-to-end matrix yet |
| Q14 | Which historical behaviors are retained, removed, or rejected in 0.4.0? | Old worlds and 12x12 versus retained Legacy itemization, public deprecated controls, and bare-FEN editing | Existing ADRs for settled removals. User for additional retirement | Depends on Q13 and the Q11 ingress case | Partial: selected removals are clear; additional choices remain |

These cases are investigation prompts, not new acceptance requirements.
The reports must distinguish missing policy from ordinary implementation
choices and already specified behavior.
Concrete containers do not need an ADR merely because code uses them.
No report can treat the major-version boundary as permission to remove
Legacy itemization.

### Q11: Architecture and algorithm coverage

The accepted packet already specifies ordering, hashes, exact arithmetic,
algorithm versions, and the logic-envelope cutoff.
It does not need an interview about each collection type or a new
architecture framework.
The later engineering specification needs three clear interfaces:

| Interface | Required property | Current evidence |
| --- | --- | --- |
| Validated contract and setup context | One immutable context supplies geometry, settings, projection adaptation, and composition. Callers do not reconstruct bands or combine different settings snapshots | `ApmwProfiles.cs:74-77,209-241`, `ActiveRosterProjection.cs:44-98`, `ApmwSidecarPresentationAdapter.cs:19-25,79-124,178-188` |
| Match identity and reversible state | Fixed unit IDs remain distinct from live occupancy. Promotion, capture, load, and undo retain lineage | `LocationHandler.cs:91-113,178-198,733-758`, `Piece.cs:116-133` |
| Committed move effects | A complete successful move supplies its captures and relocations before external reports. Rejected moves discard pending effects | `Game.cs:1590-1607,1850-1875`, `BoardMoveStack.cs:47-53`, `LocationHandler.cs:225-379` |

`Piece` uses reference equality but a hash from mutable position, type, and
move count.
Stable IDs or an explicit reference comparer are therefore relevant
implementation details.
They are not new gameplay policy.

The follow-up exposed an unsupported edited-FEN path in an attached match.
`LoadFENForm.cs:68-75` replaces the position without the starting-role ledger.
Different histories of identical Lions can produce the same FEN.
The accepted identity policy prohibits guessing their capture roles.

The user reaffirmed that FEN-based APMW resume is unsupported.
[ADR 0018](../../docs/adr/0018-reject-fen-based-apmw-resume.md)
records that boundary.
No analysis-only import feature is selected.
Internal lineage-preserving restoration remains a correctness requirement,
not a promise of player-facing resume.
Ordinary-game FEN behavior is unchanged.

### Q12: Fundamental itemization portability

The user clarified that the field selects Fundamental itemization:
`progression_itemization`.
This is a mode selector, not an Archipelago item-classification bit or an
unseen-item marker.

| Step | Current representation or target authority |
| --- | --- |
| ChecksMate option | `legacy=0`, `fundamental=1` |
| Existing slot field | `progression_itemization` contains `"legacy"` or `"fundamental"` |
| C# configuration and GUI | The resolved mode selects grants and mode-specific controls |
| Python projector input | `itemization` contains the same canonical string |
| Accepted v4 authority | `apmw_world_binding.itemization` supplies the supported mode. Conflicting mirrors fail |

The string representation is portable without a new AP flag or matching
language-specific enum ordinals.
Sources: sibling `options.py:129-135`, `items.py:22-31`,
`__init__.py:211-214`, and client `Config.cs:67-71,323-325`,
`ApmwSidecarSnapshot.cs:333-353`.

End-to-end behavior is not complete in current code.
`Config.cs:450-484` silently substitutes Legacy for malformed or unknown
mode input.
The producer-v3/client-v2 mismatch is already recorded implementation work.
Neither condition is proof that another itemization policy needs selection.

The recommended v4 mapping derives the runtime mode from the World binding.
A retained `progression_itemization` field acts only as a canonical assertion.
Its omission cannot override a valid Fundamental binding.
Explicit invalid or conflicting values fail instead of selecting Legacy.

The adjacent Castlers control needs backend parity.
The C# allocation uses `EffectiveFoundCastlers`.
The sidecar input uses raw `foundCastlers` without the local override, so
the checkbox does not change that input or its cache identity.
Sources: `ApmwCore.cs:39-46`, `ItemGeneration.cs:852-861`,
`ApmwSidecarSnapshot.cs:349-353,499-505`.
Preservation must not falsify received inventory or generator logic.

### Q13: Remaining settings questions

These branches separate product choices from ordinary engineering work.
Recommendations are not decisions.

| Branch | Missing specification | Consequence and representative case | Resolver and recommendation |
| --- | --- | --- | --- |
| Q13-a | Semantic ordering versus type/location presentation | The default uses stable types and chaotic locations. Fundamental projector semantics still select Stable | Engineering can preserve that distinction. User selection is necessary before removing chaotic presentation |
| Q13-b | Deprecated pawn presets and advertised guarantees | Pool/Max remain public values. Q44 retains Legacy pawn-material conversion, but its new guarantee control lacks an end-to-end consumer | Define the selected conversion rule and settle the remaining controls. No silent no-op |
| Q13-c | Limit scope and exhaustion | Per-type limits can relax. Pocket allocation can exceed the parsed limit. The total Queen limit constrains a Legacy item, not Fundamental graduation | User confirms soft preference, generated-pool limit, or hard deployment limit. Preserve current behavior until then |
| Q13-d | Authority, timing, and persistence of local controls | CPU family, Ignore Castlers, AI reduction, and DeathLink have different consumers and lifetimes | User settles meaningful timing changes. Engineering captures one setup context and defines reconnect/world-switch behavior |
| Q13-e | Required fields, defaults, and assertions | Omitted pawn and location fields have different producer/client defaults. Explicitly disabled upgrades can become default upgrades | Engineering specifies normalization once, with distinct absent, empty, disabled, invalid, and conflicting inputs |

The semantic combinations remain Legacy/Stable, Legacy/Chaos, and
Fundamental/Stable.
That statement alone does not prohibit chaotic concrete placement under
stable Fundamental semantics.
Sources: sibling `options.py:107-134`, client
`ApmwSidecarSnapshot.cs:333-338`, and
`ApmwSidecarSnapshotTests.cs:155-169`.

Pool and Max are reproducible preference presets in sibling
`options.py:523-554`.
Their deprecation text is not a removal decision.
`fair_board_guarantee` is declared at `options.py:562-581`, but the examined
client/game code has no reader for that key.

The disabled-action case is concrete.
An all-`-1` priority map becomes an empty preference sequence in
`logic_projection.py:423-458`.
`apmw_projection\planning.py:34-54` then substitutes default actions.
The support contract must preserve the difference between absent preferences
and an explicitly disabled action set.

Limit evidence is in client `OwnedRosterGeneration.cs:371-404`,
`ItemGeneration.cs:2333-2376`, and `Config.cs:920-934`.
Default and input-normalization evidence is in `Config.cs:317-384,450-626`.
The local-control table must cover both client and GUI consumers.
Duplicate DeathLink receiver ownership alone does not prove that server-side
tag disabling fails.
Its opt-out and reconnect behavior need one explicit lifecycle contract.

### Proposed cohesive Options record

One new owning record can define the 0.4.0 settings support and retirement
matrix.
Issue 17 continues to own the shared wire contract.
The proposed record does not replace it or create another compatibility
mechanism.

Each setting needs its canonical key, owner, lifetime, supported modes,
default, normalization rule, generator effect, wire field, GUI surface,
invalid-input behavior, retirement status, and acceptance cases.
The same normalized world settings must feed costs, slot data, and client
setup.

| Coverage group | Controls that the record must classify |
| --- | --- |
| Board progression | Minimum/maximum geometry, fixed transitions, endpoint victory, and old `goal` inputs |
| Itemization and presentation | Fundamental/Legacy, type ordering, placement ordering, human collections, and presets |
| Pawn and upgrade behavior | Pawn modes, Pool/Max/Configure, priorities, ratios, guarantees, and all-disabled actions |
| Limits and inventory | Per-type limits, Queen-item limits, pockets, human Kings, Early Material, and excess external inventory |
| Costs and capability | Difficulty, derived modifiers, tactics, obtainable counts, and frozen cost inputs |
| Local controls | CPU family, Castlers override, AI reduction, DeathLink, preview/match timing, and saved state |

The existing producer permits starts at 6x8, 8x8, or 10x8.
With the five endpoints, that gives 12 valid contiguous start/end pairs and
three reversed pairs to reject.
Starting at 10x10 or 12x10 is not implied by support for those endpoints.
Source: sibling `options.py:11-50`.
The client must derive endpoint behavior from the World binding, not the old
goal switch in `LocationHandler.cs:844-883`.

### Q14: Retirement boundary

| Disposition | Behavior |
| --- | --- |
| Already selected for removal or rejection | Old-world execution paths, missing-contract fallback, 12x12, old layout guesses, obsolete width-based Location counts, shared CPU/human promotion leakage, and FEN-based APMW resume |
| Explicitly retained | Legacy itemization under the new contract, the four CPU families, human entitlements, selected projection semantics, and original world costs |
| Requires further selection or reachability evidence | Public Pool/Max controls, nonzero guarantees, soft-limit fallbacks, local gameplay-control timing, and obsolete outer-envelope keys |
| Outside this retirement scope | Ordinary-game rules and saves, generic appearance/engine preferences, tracker UI, and the Regions refactor |

The recommended implementation ledger lists each retired input or path,
its authority, replacement or rejection message, and affected consumers.
A historical symbol name does not authorize deleting a helper still used
by supported Legacy itemization.

### Follow-up disposition

The three audits are complete.
The clarified Fundamental selector does not need another meaning decision.
Architecture needs bounded engineering interfaces, not a general redesign.
Settings and retirement need the additional owning record before treating
the broader 0.4.0 release plan as complete.
Q35 remains valid for the original 20 decisions.
The later FEN support clarification is recorded below.
No production implementation is approved here.

The next user-owned discussion concerns Q13-b, Q13-c, and Q13-d.
Ticket 21 resolves the separate FEN, Undo, and controller-change boundary.
Q13-a can preserve current presentation behavior unless the user wants a
change.
Q12 integration and Q13-e normalization remain engineering-owned work.
[Ticket 24](issues/24-specify-settings-support-and-retirement.md) now owns
the settings matrix and its remaining user decisions.

## FEN clarification and interaction frontier

On 2026-09-16, the user reaffirmed the historical absence of FEN resume
support in APMW.
They suggested a starting-FEN attachment in SGF as a possible recording
feature, not as permission to restore APMW play.
[Ticket 21](issues/21-define-apmw-interaction-support.md)
owns this additional support record.

The suggestion to disable Undo before 1.0.0 remains conditional.
At Q36, the user did not select the proposed blanket 0.4.0 restriction.
They accepted genuinely view-only traversal and proposed reversing capture
counter increments by turn.
At Q37, they selected reliable Undo support for 0.4.0.
[ADR 0019](../../docs/adr/0019-support-reliable-apmw-undo-in-zero-four.md)
records that release commitment without authorizing implementation.

| Branch | Case | Status |
| --- | --- | --- |
| FEN resume | Direct import or the same executable starting FEN inside SGF | Unsupported; ADR 0018 |
| SGF recording | Attach the generated starting FEN as informational data | Optional idea, not an implementation commitment |
| Take-back and history | Reliable Undo through the reversible ledger, plus genuinely view-only traversal | Q37 selects 0.4.0 scope; no blanket UI restriction |
| Internal rollback | Search, speculative moves, rejected moves, and known-state reconstruction | Existing correctness requirements remain |
| Computer takeover | Controller changes permanently stop new Location and final-goal submissions | Q38/Q39 settled. Q45 permits earlier library submissions to finish. AP session remains connected |

Restoring local counters does not retract checks already accepted by
Archipelago.
Any future player-backtracking design must account for that distinction.
The current menu inventory does not authorize disabling read-only views,
ordinary-game features, or normal new matches.

The source already contains the basic proposed ledger:
`LocationHandler.cs:98-103,178-198,584-597`.
It stores move deltas, subtracts capture counts on take-back, and restores
origin mappings.
The remaining work is integration correctness and extending that state to
the new role/King model, not inventing a second history structure.
Ticket 21 supplies the additional integration acceptance for the selected
0.4.0 scope.

Q38 rejects the proposed controller lock.
Computer takeover is permitted, with Location reporting disconnected for
the affected window.
This does not require disconnecting the AP session or pretending a
DeathLink occurred.
Q39 selects a permanent non-reporting state for that match.
Restored controllers and Undo do not re-enable reporting.
A fresh normal match can report again.
This includes final AP goal completion, not only individual Locations.
[ADR 0020](../../docs/adr/0020-stop-apmw-progress-after-controller-changes.md)
records the decision and queued-report boundary.
Ticket 21 is resolved; the other Options questions remain open.

## Reporting-status badge frontier

The user proposed a badge like the check indicator when the game window
cannot report Locations.
[Ticket 22](issues/22-specify-apmw-reporting-status.md)
owns the presentation and recovery contract.

The proposed badge distinguishes `NOT REPORTING` after a controller change
from `AP DISCONNECTED` when transport is unavailable.
Both derive from the same reporting gate used by the client.
They must coexist with the check warning.

Q39 already prevents reconnection or Undo from clearing takeover's permanent
non-reporting state.
Q40 selects an earned-Location set per match, replayed after reconnect.
The server deduplicates repeated IDs.
Undo changes the current counters, not the set of historical accomplishments.
Permanent takeover suppression still blocks replay.
The implementation must preserve state that the current disconnect path
clears.

[ADR 0021](../../docs/adr/0021-replay-earned-locations-after-reconnect.md)
records this choice.
Q41 selects process-memory retention after game-window closure, with no
durable journal file or restart recovery.
The journal is not owned by the window or by one socket session.
It retains match eligibility and world/slot identity.
This does not save or resume chess games from FEN.

### Connection lifecycle research

The user asked to revisit the old restart-required reconnect guard.
[Ticket 23](issues/23-specify-connection-lifecycle.md)
owns this extension.
Parallel research covers the local source/history and primary
MultiClient.Net behavior and usage.

The user wants actual disconnected/connecting/connected state to determine
whether another attempt is safe.
The illustrative 15-second interval is not a selected timeout.
Port reuse and changes reinforce the need for verified room identity.
Automatic retry scheduling is not yet selected.

The [local guard history](research/reconnect-guard-history.md) confirms its
2023-10-26 origin and a failure-path latch.
A completed login failure can still block a corrected-password retry to
the same endpoint/slot.
Handled socket closure clears that guard but also clears match accounting.
The [primary-library report](research/multiclient-reconnect-patterns.md)
is also complete.
Its pinned 6.6.0 source shows that a timeout does not establish transport
retirement, and helper resynchronization can resend cached checks.
The proposed lifecycle uses Disconnected, Connecting, Connected, and
Stopping with attempt-owned callbacks.
The [bounded contract](contracts/apmw-connection-reporting.md) completes
the stock design, including already-terminated routes and bare-host fallback.
Genuinely unresolved operations remain in Stopping.
Runtime retirement classification remains an implementation proof obligation.
Tickets 22 and 23 are resolved as decision records.

Q42 selects AP generation name, authenticated team/slot, and frozen-contract
checks without a new identifier.
The World binding schema and hash graph remain unchanged.
This accepts normal AP identity with its collision limit.
Identical compared values cannot distinguish separate generations or server
instances.
Q43 gives the earned final-goal marker the same retention and replay rules
as the Location set, including process-memory retention after window
closure.

| Branch | Remaining decision or work | Resolver |
| --- | --- | --- |
| Q42 | Normal AP identity plus frozen-contract checks; no additional generated-world marker | Resolved |
| Q43: Final-goal journal | Retain the earned final-goal marker after window closure alongside the Location set | Resolved |
| Connection transitions | Positive retirement permits retry. Genuinely unresolved work remains Stopping | Resolved design in ticket 23 and its bounded contract. Runtime proof remains future work |
| Q45: Reporting cutoff | Permit pre-takeover library submissions to finish. Block new application submissions and journal replay after takeover | Resolved. The stock adapter specification is complete |
| Automatic retry | Not required by the current manual-reconnect request | Deferred unless requested |
| Q44: Fair board guarantee | Retain pawn-material conversion for Legacy in 0.4.0 | Resolved capability choice, ADR 0022 |
| Q46: Reservation count basis | Actual deployable contribution plus pocket contents, not received items or undeployed board reserves | Resolved principle, ADR 0022 |

## Pass 5: Conversion-rule prerequisites, 2026-09-18

Q44 retains Legacy pawn-material conversion.
Q46 selects actual deployment plus pocket contents as its count basis.
The user's tentative capacity estimate exposed a placement question.
Q47 resolves it by retaining the Q35 rank rules.
Ten-rank boards have one more mixed human rank than eight-rank boards.
This pass keeps the remaining prerequisites separate from the conversion
equation.

| Branch | Question and consequence | Representative case | Resolver | Edges and status |
| --- | --- | --- | --- | --- |
| Q47 | Keep the current human rank limits and shared Minor placement | One mixed rank on eight-rank boards; two on ten-rank boards | Settled in tickets 19/24 | Resolved; no capacity revision |
| G1 / Q48 | The board being prepared supplies the reservation target | Prepare 6x8 in a world that ends at 12x10: use the 6x8 target | Settled in ADR 0022 | Resolved board context; numeric target remains separate |
| G1 / Q49 | Use CPU starting count minus its primary royal as the target | Targets 11/15/19/27/33 in stage order | Settled in ADR 0022 | Resolved; no CPU price or promotion-policy transfer |
| G2 / Q50 | Clarify the actual count-reducing path before another policy choice | Fundamental upgrades preserve owned slots. The historical Legacy guard would protect all eight units in the proposed example | Source clarification in ticket 24 | Hypothetical policy choice withdrawn; no new conversion or Fundamental behavior selected |
| G2 | Which conversion actions and accounting implement the retained capability? | Material and count constraints must both hold | User semantics and engineering specification | Open; must update the shared algorithm/input contract and compatible logic metrics |
| G3 | Which pieces qualify in each named guarantee mode? | Board pawns, a pawn in a pocket, additional royals, and board reserves | User decision where the mode meaning differs | Partial: Q54 includes every actual pocket piece, including Pawns. Named modes and the full equation remain open |
| G4 | How do both repositories derive actual pocket contribution? | Two Pocket items with limit four do not determine one versus two actual occupied pockets | Engineering contract after membership selection | Q54's total count requires exact occupancy. The composition branch can also require Pawn classification. Portable input/allocation remains to specify |
| G5 | Does restored allocation retain funding from other Legacy surplus? | Historical concrete-value differences and unused credits versus a Pawn-only budget | Q51 in ticket 24 | Resolved: retain eligible Legacy surplus, with explicit transfers and no reserve double spending |
| G6 | Which approximation rules augment funded credit? | The old 45 allowance and replacement can produce 200 extra centipawns in the Checkers trace | Q52/Q53 in ticket 24 | Resolved: floor net surplus at zero, then retain historical approximation. The proposed shared 100-centipawn cap is rejected |
| G7 | Does surplus funding retain credit removed by Legacy merge normalization? | Minor 300 plus Major 485 currently becomes one 485 grant. A concrete Rook at 500 leaves zero transferable surplus versus 285 from the full received base | Q55 in ticket 24 | Resolved: preserve all involved eligible Legacy credit. The minimal custom fixture needs no crowding or difficulty input, but does not establish default-path frequency |
| G8 | Does every candidate preserve the required unit reservation? | Mixed Pool can emit two Sergeants and four Pawns from budget 745, then stop with only six of seven required units | Engineering, after the reservation policy is fixed | Source counterexample recorded. Preserve approximation without preserving this count-guard gap |
| G9 / Q56 | What did the two named guarantee modes intend, including for Fundamental? | A full-count roster can have no Pawns; owned Chessmen can also exceed active capacity | Prompt-intent recovery, then author decisions only for remaining details | Original July 4 answer recovers best-effort Pawn composition. Producer text drifted to counting found Pawns. One-mode simplification is unselected; exact algorithms remain open |
| Q45 | Does takeover suppress reports already admitted to the library queue? | A queued report waits when the user changes control | User decision in ticket 23 | Resolved: earlier library submissions can finish. New submissions and replay remain blocked |
| Q13-b/c/d/e | Presets, limits, local-control timing, and normalization remain visible | Pool/Max, surplus pockets, AI reduction, world switching, all-disabled actions | Ticket 24 | Open; not resolved by the new guarantee decisions |

Q47 explicitly retains the existing band allocation.
No new CPU placement, neutral-rank change, automatic reconnect, transport
fork, or production implementation follows from this pass.

## Pass 6: Guarantee enhancement and instrumentation, 2026-09-19

Status: Open decision work.
Origin: The user requested enhancement and instrumentation after the recovered composition intent.
Destination: Specified setting behavior and reproducible explanations for a later implementation handoff.
The [settings decision record](issues/24-specify-settings-support-and-retirement.md#enhancement-and-instrumentation-frontier-2026-09-19)
owns the answers and exact first fixture.
This network remains navigation, not gameplay authority.

### Landed baseline and boundary

Fundamental keeps its owned Chessmen slots.
Legacy keeps the selected conversion, funding, and approximation policies.
Selected Legacy reservation targets remain 11/15/19/27/33.
Their role in Fundamental requires explicit mode semantics.
Every actual pocket piece counts toward the selected total contribution.
That decision does not yet settle pocket membership in the Pawn-composition target.
The original eight-Pawn example supplies intent, not a complete quota table.

The user reports a recent audience concern.
Its exact external source remains unavailable in the bounded session-history search.
No participant, quotation, or more specific requirement is inferred.
This evidence gap does not block decisions about the requested setting.

### Breadth map and waysigns

| ID | Question and consequence | Cases | Resolver and next route | Edges | Status |
| --- | --- | --- | --- | --- | --- |
| G10 / Q57 | Does the illustrative eight-Pawn target override cross-family upgrades? | E1, E6 | Q59 supersedes this Pawn-specific question. General preference precedence belongs to G20 | Does not block the selected total/non-Pawn interpretation | Superseded: Q59 selects no separate Pawn minimum |
| G11 / Q59 | Which minimums does composition protect: total units plus non-Pawns, or Pawns plus non-Pawns? These predicates differ even without a ceiling | E2, E6, E7 | Q59 in the settings decision record | Precedes numeric targets and preference precedence | Settled: total units plus non-Pawns. No Pawn minimum or ceiling |
| G11-a / Q60 | Which prepared-board target pairs apply? Derivation from CPU counts must be explicit, including the second 12x10 King | E2, E7 | User through the next interview question | Depends on Q59 and Q49. Feeds shortage handling | Open. Proposed pairs: 11/5, 15/7, 19/9, 27/15, 33/19 |
| G12 | Which units satisfy composition, and where? Pocket Pawns can contribute to total count without forming an on-board Pawn contingent | E3 | User, then catalog/source analysis | Feeds G11, G14, and the actual-pocket contract | Open |
| G13 | How does best effort handle shortages, excess inventory, Castler locks, and capacity conflicts? An unsatisfied target is not permission to invent units | E2, E4 | User for precedence, engineering for feasible allocation | Depends on G10-G12. Must preserve owned slots and selected Legacy accounting | Open |
| G14 | Who controls the setting, when does it take effect, and what survives world changes? Preview and live-match behavior must not mix snapshots | E3, E5 | User through the existing settings-lifecycle branch | Depends on mode semantics and constrains normalization | Open |
| G15 | Which explanations serve players and which records serve diagnosis? An unmet target needs an accurate reason, not a silent fallback | E1-E5 | User for surface scope, engineering for records and error handling | Depends on G10-G14 for final meanings | Open. Local-only preview and reproducible diagnostics proposed |
| G16 | How do both repositories agree on allocation, pockets, and logic bounds? Exact rosters cannot silently replace conservative reachability estimates | E3-E5 | Engineering specification and bounded source analysis | Depends on G10-G15. Requires explicit semantic/input version changes | Open |
| G17 | Which modes, defaults, names, and deprecated presets ship? Enhancing composition does not silently redefine `none` or retire Pool/Max | E1, E2, E5 | User through the settings support matrix | Depends on semantics and explanation scope | Open |
| G18 / Q58 | Do conflicting settings require generation rejection or a declared allocation fallback? This defines whether preferences are strict | E6 | Q58 in the settings decision record | Precedes G10 and G20 | Selected direction: best-effort preferences, shared fallback, and generator validation. Detailed criterion remains open |
| G19 | What proves configuration viability? Current inventory, maximum inventory, exact material, conservative logic, and capped requirements are different quantities | E1, E6 | Source analysis for existing comparisons, user for the acceptance criterion | Feeds G18 implementation, G15 explanations, and G16 logic | Open. Existing projection caps can conceal a low allocation ceiling |
| G20 | If fallback is selected, what can it relax, in what order, and when does it stop? Disabled actions are not automatically ordinary preferences | E6 | User after G18 | Depends on G18. Affects G10-G13 and versioned semantics | Open. Meeting a goal budget and spending all usable material differ |
| G21 | Which cap is relevant: type preference, physical space, or unit entitlement? Absence of a Pawn type limit does not remove the other constraints | More than eight Pawns, an exhausted type pool, and Material without additional Chessmen | Source facts in the settings decision record. User only for later semantic changes | Feeds G13, G15, and G20 | Current client has no Pawn-type repetition cap. Placement and ownership limits remain |
| G22 | What does `locked_items` guarantee, and which parts inform the new allocation policy? Pool resources and prepared composition remain distinct | A future pool guarantee, partial receipts, an upgraded Minor, and a Pawn slot that graduates | Completed source analysis. Further policy uses the corrected G23 process analogy | Feeds G15-G16 and the remaining target/relaxation decisions | Evidence complete. The proposed baseline-boundary interview is withdrawn, not answered |
| G23 | How does the generator retain its original distribution while satisfying locks, then broaden the mix as material increases? Count obligations alone do not explain the process | Outstanding locks, sampled lock matches, capped candidates, and additional material | Completed ChecksMate source report, section 10 | Refines G22. Feeds G15-G16 and G20 | Evidence complete: ordinary weighted draws satisfy reserved minima. More budget permits a wider mix but guarantees neither diversity nor fixed ratios |

### Representative cases

E1 and E6 are exact allocation fixtures in the owning record.
E7 is a predicate comparison, not an allocation fixture.
E2-E5 are investigation prompts, not executable fixtures or selected outcomes.
They require fixed inputs before implementation or a prototype.

| Case | Distinction to preserve |
| --- | --- |
| E1: Feasible composition versus spending | 15 Fundamental slots and 1600 upgrade material. Seven 200-cost transitions leave 8 Pawns, 7 Minors, and 200 unspent. The eighth transition is the decision |
| E2: Too few or more than enough units | Five owned Chessmen cannot provide eight Pawns plus seven non-pawns. More than fifteen owned units must not disappear because an example used fifteen |
| E3: A Pawn in a pocket | Total contribution includes the occupied pocket. Whether it satisfies Pawn composition remains open |
| E4: Capacity and entitlements | A family graduation can place an owned unit in reserve. Castler locks and extra royals also compete with a desired composition |
| E5: Reproducible explanation | The same inventory, geometry, settings, seeds, and versions must explain the same prepared result. A later receipt or local change is a different snapshot |
| E6: Terminal inventory and viability | Maximum inventory is 15 Chessmen plus 8 Material, with only 200-cost Pawn-to-Minor transitions. The 8-Pawn target limits expected material to 2900. Relaxation can reach the stipulated 3900 with 3 Pawns and 12 Minors, or 4500 with 15 Minors and 200 unspent |
| E7: Minimums do not imply a Pawn minimum | Fix an active roster of 15 Minors, zero Pawns, no pockets or additional royals, and a separate primary King. Compare total >= 15 plus non-Pawns >= 7 against Pawns >= 8 plus non-Pawns >= 7. Only the first predicate is satisfied without fallback |

### Q57 response and reordered frontier

The user rejected the premise that composition precedence can be selected without goal viability.
They did not select either E1 outcome.
E1 is only a current inventory snapshot, so it cannot establish a world's final reachability.
The user proposed generation rejection or progressive preference relaxation as possible remedies.
Neither mechanism is selected.
G18-G20 now precede the original composition question.

The retained projection cap is a material interaction, not a reason to dismiss the concern.
It can lower the effective requirement when the configured allocation has a low maximum.
The new viability criterion must state whether it compares before that cap.
The user's illustrative 3900 is not a replacement for the accepted intrinsic cost table.
E6 holds its threshold explicit to isolate the decision.

### Q58 response and minimum-count interpretation

The user then accepted best effort.
This selects the shared fallback and generation-validation direction in the owning record.
The exact criterion, relaxable preferences, order, and stopping rule remain open.
This supersedes the preceding statement that neither mechanism was selected.

The user also proposed minimums for total chessmen and non-Pawn pieces.
That can permit an all-non-Pawn army without a fallback.
It differs from preserving minimums for both Pawns and non-Pawns.
Q59 isolates that distinction before selecting a numeric quota or relaxation order.
The earlier Pawn-specific target remains an example, not an accepted universal requirement.

### Q59 answer and source correction

The user selected total-unit and non-Pawn minimums, with no separate Pawn minimum.
E7's all-Minor army therefore satisfies the illustrative targets without fallback.
Neither target is a maximum.
The earlier eight-Pawn ceiling in E6 is not a constraint of the selected interpretation.
Its fixed arithmetic remains useful only as the rejected-policy comparison.

The owning record and ADR 0022 contain the current decision.
The glossary names this concept "composition minimums."
Historical research receives a current-authority note rather than a rewritten quotation.
Q60 now proposes prepared-board target pairs.
Membership, shortage handling, preferences, and viability remain open.

The subsequent Pawn-cap question distinguishes limits rather than selecting a new policy.
Current Pawn selection has no per-type repetition cap.
An 8x8 board has gross Pawn capacity 32, reduced by non-Pawns beyond the optional back rank.
Fundamental Material still upgrades owned Chessmen rather than creating additional slots.
The owning record supplies source references and the historical Legacy distinction.
This does not answer Q60 or authorize an unlimited allocation.

### Orchestrated baseline investigation

The user's latest example names `locked_items`, at least seven Minor units and eight Pawns, followed by surplus allocation.
A pool guarantee, a baseline allocation, and a persistent composition floor are different claims.
Q59 currently selects no separate Pawn floor.
The new statement might concern an earlier allocation phase or might revise that choice.
Neither interpretation is selected on the user's behalf.

The existing ChecksMate research session traces the producer declaration and all material consumers.
The parent traces only the client-side receipt, owned-slot, and projection boundaries.
The child writes a session artifact, not repository changes.
Source findings will precede the next deciding question.
Target-pair confirmation is deferred while this prerequisite is open.

The producer report is complete at HEAD `0772bac724ff483043b56979f0ef078b9c2d264a`.
The settings record links the artifact and captures its decisive findings.
`locked_items` guarantees literal randomized-pool copies, not an initial or permanent army composition.
Starting inventory does not silently consume the locked obligation.
Fundamental rejects the Legacy lock dictionary rather than converting it.
Its current sole-gateway example with 15 Chessmen and four Material produces eight Minors and seven Pawns.
Thus, rounded input funding alone does not implement an exact 7/8 baseline.

The proposed runtime enhancement can establish a baseline phase before surplus allocation.
That proposal still needs user selection.
Pool-only minima and a final Pawn-retention rule remain distinct alternatives.
The latter explicitly reopens Q59.
The investigation authorizes neither implementation nor a new inventory grant.

The user did not accept the proposed intermediate-baseline interpretation.
They instead described a more complex distribution-preserving process in `locked_items`.
G23 now investigates how locks interact with the ordinary distribution as material increases.
The earlier pool-minimum findings remain valid but do not resolve this process question.
No fixed seeding phase or full generator-algorithm port is selected for the army allocator.

Section 10 of the producer report completes this explanation.
Ordinary accepted draws first satisfy matching outstanding obligations instead of adding another copy.
Outstanding obligations reserve material and AP slots without defining the base weights.
Candidate elimination changes the available distribution, and it does not reset when locks are fulfilled.
A terminal append supplies remaining obligations.
The source thus supports the user's distinction from a seed-first algorithm.

The owning record retains the controlled 600-versus-1100 budget comparison and its full-source pointer.
It also records the current extra-headroom check before a matching lock is decremented.
These are source mechanics, not automatically selected rules for the new allocator.
The next policy work concerns minimums, ordinary preferences, and permitted relaxation, not a forced baseline phase.

### Proposed observability and closure criteria

The proposed record separates requested composition from achieved composition.
It exposes owned, active, pocket, and reserve counts without merging their meanings.
It records protected actions, retained material, and specific reasons for unmet targets.
An allocator's failure to find a target is not proof of global infeasibility.
Execution errors remain errors, not a best-effort success status.
A fallback explanation also needs the requested preferences, effective relaxations, and resulting material.
An ordinary early shortage must remain distinct from a terminal configuration ceiling.
The preview must not describe a capped requirement as proof that the original target was achievable.

The current geometry preview counts the primary King in its active total.
The guarantee targets exclude that King.
Instrumentation must label and derive the correct contribution explicitly.
Existing material summaries are prior art, not proof of guarantee instrumentation.

A later closure review requires selected mode semantics, complete boundary fixtures, a portable setup result, and an explanation contract.
The exact and conservative metrics remain separate.
No branch is ready for production handoff yet.
External telemetry, new analytics infrastructure, and a broad tracker UI are outside the proposed scope.

## Source update map

| Source | Currentness |
| --- | --- |
| Owning map, issues, glossary, and twenty-two ADRs | Working-tree documents include the current user answers. Tickets 21-23 are resolved. The bounded connection/reporting contract specifies future runtime acceptance. Ticket 24 owns settings and the remaining Legacy conversion rule |
| Earlier interview source baseline | `888710f99cf007b982ea1f6a8a64c1e8058be196`. The follow-up audit uses the newer baseline recorded below |
| Approved 10x10 prototype | Local branch `prototype-ten-by-ten-715de4c1`, immutable commit `285d59741917bde4f9ed143ca85e61fe7d451c05` |
| Approved 12x10 Lion prototype | Local branch `prototype-large-board-armies-715de4c1`, immutable commit `777e2c778ed7d26feae525da9415d62af3d5c469` |
| Original session planning packet | Historical snapshot, superseded where the later interview records a decision |
| Twelve-file inspiration report | Current local source survey and fit assessment; source examples do not choose the new piece |
| Generator graph source | One Menu Region at sibling `worlds\checksmate\__init__.py:281-295`. Single-parent and unique-name constraints at `BaseClasses.py:1295-1299,1481-1514` |
| Decision-to-execution route | Generic Wayfinder compatibility passed. The repository adapter remains absent, so automatic projection/implementation routes remain disabled |
| Enhancement refresh, 2026-09-19 | Re-read ADR 0022, the map, ticket 24, the glossary, and the original July 4 prompt answer. Current `OwnedRosterGeneration.cs:478-565` retains Fundamental slots. `ActiveRosterProjection.cs:237-301` supplies family/material summaries. `ApmwGeometryPreview.cs:39-51` includes the primary King in `ActiveCount`. No runtime execution or new external feedback source |
| Follow-up audit baseline | Clean working tree at `cc83ca9`, after Q35 documentation commit `739eda8`. The new real-board fork characterization is current evidence |
