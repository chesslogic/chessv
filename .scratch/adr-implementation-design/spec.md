# ADR implementation development specification

Status: Design for later execution. No production implementation authorized by this document.
Baseline and source evidence: [assessment.md](assessment.md)
Assignment inventory: [fanout.json](fanout.json)

Current authority, 2026-10-25:
[ADR0024](../../docs/adr/0024-seeded-hierarchical-allocation.md) records the accepted guarantee-first behavior.
The [current frontier](engineering-closure.md#current-authority-and-frontier) controls supersession, evidence, and gate status.
Older scheduler, trial-driver, and PR instructions in this packet retain historical context only where that frontier supersedes them.
The [local allocation plan](plan/design-seeded-army-allocation-1.md) proposes engineering under that behavior, not an accepted executable algorithm.
Complete CV02/TX/TR/CS design/reference acceptance remains accepted. Native conformance and D03 acceptance remain separate.

## 1. Scope and authority

This specification maps finalized ADR requirements to bounded implementation work.
It selects engineering interfaces and file ownership, not new gameplay policy.
The accepted ADRs and their linked contracts remain authoritative.
The specialist reports explain current behavior and cite existing symbols.

The deliverable covers library, handler, search, board, and integration work.
Implementation must support exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`.
It preserves four CPU families and both human itemization modes.
It rejects old world contracts even when their requested geometry remains supported.

Ticket 24's D22 closure round settles the listed ADR 0022 product policy.
The [engineering closure register](engineering-closure.md) separates that policy from open proofs.
No worker can invent a default, omit an accepted capability, or treat reconciliation as execution approval.

### Claim ledger

| Exact claim | Class | Authority or basis | Excludes |
| --- | --- | --- | --- |
| Army data contains 20 explicit formations and one file-preserving coordinate system | Author decision | ADR 0001 and shared-data contract section 4 | Independent client/generator array synthesis |
| CPU royal policy follows surviving CPU Kings, independent of human upgrades | Author decision | ADR 0003 and royal fixtures | Selecting CPU policy from `foundKingPromotions` |
| CPU side and starting actor identity remain fixed after controller takeover | Engineering design derived from author decisions | ADRs 0003/0020 and local-play continuity | Reclassifying the CPU through current `IsHuman` |
| Current handler setup callbacks can precede a failed commit | Evidence | Handler probe, `Game.MakeMove` and `LocationHandler.SetupMove` | Treating setup notification as successful progress |
| `Game` publishes complete commit facts and the handler applies one atomic ledger entry | Temporary engineering assumption selected for this design | Existing move lifecycle seam and ADR 0019 | Generic event-framework replacement or handler-side move reconstruction |
| One owner journals earned progress and admits packets under the suppression gate | Author decision | Connection/reporting contract | Direct completion-helper calls or per-caller eligibility checks |
| Rules and slot data use the same frozen world-cost object | Author decision | ADR 0016 and snapshot contract | Recalculation in `fill_slot_data` or client-local prices |
| Core search algorithms remain unchanged | Evidence-based design | Search probe proves existing rule/make-unmake/hash seams | Speculative search rewrite |
| Named new files and DTO declarations can change only through S0 coordination | Temporary engineering assumption | Exclusive ownership and cross-assembly consumers | Silent local interface forks |
| ADR 0022 policy is selected but engineering proof is incomplete | Engineering gate | Ticket 24 closure round and engineering-closure.md | Re-grilling selected modes/targets or declaring a finalized algorithm from accepted status |
| Stock slot normalization precedes ChessV validation | Documented input boundary, not a blocker | User-directed scope correction and installed converter evidence, RI | Claiming raw-wire detection, weakening exact decoded-object checks, or requiring a new transport feature |

Temporary engineering assumptions can change when a concrete compile or fixture
failure disproves them. The integration owner records the replacement and updates
all consumers before parallel work resumes.
Product decisions require their original decision owner.

### Execution decisions, reconciled 2026-10-25

| Decision | Selection | Consequence |
| --- | --- | --- |
| Phase boundary | Close all open design work, including D22 and the settings matrix, before implementation | One orchestrated implementation pass covers every package, including A22 |
| D22 route | Consolidate the selected ticket 24 policy, then close named engineering gates | No broad new product interview. A narrow incompatibility returns to the coordinator |
| S0 acceptance | Explicit S0 authorization and accepted declarations remain required. Current instructions prohibit PR creation | Historical initial-PR wording does not mandate a PR. Later declaration changes go through the design owner. Behavior changes return to the user |
| Contract home | Existing projects: `ChessV.Base` and `ChessV.Games`, then `APMW.Client` | No new contracts project |
| Integration target | Ready branch commits, coordinator integration into ChessV `develop`, no PR | Dependency order still controls integration. This wave grants no integration authority in another repository |
| Capture role vocabulary | The capture role is the one domain identity. `unit_id` is an implementation field | `CONTEXT.md` records the term |

## 2. Target architecture and state ownership

```text
Pinned Army + Location profile + shared contract
                 |
      validated world context + original cost snapshot
          /                              \
 setup/CPU identity                  connection owner
          |                         journal + admission
 Game commit/abort/undo                      ^
          |                                 |
 reversible match progress -- EarnedMatchEvents
          |
 CPU rules use engine make/unmake, not reporting state

GUI reads a shared reporting snapshot.
Controller replacement suppresses reporting before changed-controller progress.
Transport loss changes connection state, not the board or match ledger.
```

| State | Owner | Lifetime and invariant |
| --- | --- | --- |
| A/L/C resources | Strict data readers | Immutable installed semantic set |
| Validated world context | C1 value types, N1 publication | Original AP generation/team/slot/game, W/C/S hashes, progression, and published sets |
| Starting CPU metadata | B1 | Immutable unit/role/class records and primary actor identity |
| Current piece-to-unit association | H1 progress ledger | Reversible transitions transfer identity through promotion replacement |
| CPU royal membership and castling rights | R1 | Reversible engine state, including speculative make/unmake |
| Commit/attempt IDs and complete board deltas | E1 | Game-local, monotonically assigned commit identity and explicit reset epoch |
| Current captures and history | H1 | One ledger entry per successful committed move, including zero captures |
| Earned Location set and final-goal marker | N1 | Monotonic eligible accomplishments, process memory, independent of Undo/window lifetime |
| Reporting eligibility | N1 | Permanent suppression per match, separate from connection availability |
| Effective allocation policy | A22 producer, C1 strict reader | One normalized recovered instruction set, no requested-policy overwrite |
| Prepared setup | U1 captures S0 K12, A22 realizes | Immutable inventory/settings/pockets/IgnoreCastlers and generation token. No mixed or stale adaptation |
| Generator costs | G2 | Freeze with effective policy and finalized obtainable counts before dependent fill/final core acceptance |

No transport record retains a game window or board after closure.
No search rule reads the current AP connection.
No semantic goal identity comes from a display name, current square, or current type.

## 3. Interface gate S0

After design closure and explicit authorization, S0 is a small implementation tranche before consumer fanout.
It materializes declarations, complete fixture inputs, and compile-level consumer
examples. It does not implement gameplay or networking.
The [shared contract inventory](contracts.md) lists every wire contract, seam,
and primitive, with writers, readers, and visibility.

S0 readiness first requires supported EP/NM/AL/PM/LC/TX/TR/CS closure.
Reconciliation does not supply that proof or authorize declarations.
The user then approves the initial S0 PR before consumer implementation.

### 3.1. Setup identity

The proposed public read-only contract lives in
`ChessV.Games\MiscellaneousGames\ApmwStartingArmyIdentity.cs`.
`APMW.Client` already references `ChessV.Games`.
No new project dependency is necessary.

The interface exposes stage/family identity, fixed CPU player, starting counts,
unit records, primary unit ID, and initial Piece-to-unit associations.
The unit record contains `unit_id`, `role_id`, and `starting_class`.
The backing collections must be immutable, not writable lists behind read-only types.

Cold start supplies formation metadata before type/rule initialization.
It establishes initial Piece bindings before root legal-move generation.
Internal reset changes the epoch and clears every dependent identity/rule/progress/projection cache.
CS must declare the precise `Initialize/postInitialize/LoadFEN/StartMatch` ordering.

The first protected Game placement hook binds CPU Pieces before PositionLoaded.
The second validates every board/pocket Piece after all PositionLoaded calls and before root trials.
Captured pocket/account values must work while Match is null.

The initial map is not the current position map.
E1 observations identify replacement/movement transitions.
H1 transfers the stable unit identity to the new piece and reverses that transfer
on Undo. Missing CPU identity is an explicit error, never a square/type fallback.

K11 declares effective policy in Games, without an APMW.Client dependency.
K12 declares immutable prepared-setup inputs and ordered pocket families in Games.
C1 validates policy bindings. U1 captures actual inventory, settings, seeds and local Ignore Castlers once.
A22 and the sidecar adapters consume that capture.
A stale generation refuses or regenerates the whole setup.
It never adapts response A with current settings B.

### 3.2. Committed observations

The proposed shared payload lives in `ChessV.Base\CommittedMoveObservation.cs`.
It contains game/epoch identity, commit ID, mover, primary move, captures, and
all piece transitions. Values describe the committed operation, not mutable
references that callers must inspect later.

E1 supplies these lifecycle outcomes:

| Outcome | Required observation |
| --- | --- |
| Successful committed move | Exactly one complete commit after acceptance |
| Failed committed attempt | Abort pending observation, no commit or ledger entry |
| Speculative make/unmake | No APMW commit or earned progress |
| Committed Undo | Exact commit ID, strict top-of-stack pairing |
| Internal accepted position replacement | New epoch and identity-aware reset through the accepted fixture/load path |
| Committed terminal result | Exactly one notification for every result source, including CPU extinction |

En passant, castling, promotion replacement, and Checkers multi-capture require
complete deltas. `MoveInfo` alone is not sufficient.
`Game` publishes facts. It does not classify Locations.
This design does not add user-facing save/resume support or promise arbitrary
FEN rollback beyond the accepted internal lifecycle fixtures.

TX distinguishes no-match rejection, fully applied late rejection and partial exceptions.
E1 restores the exact attempt at the committed transaction boundary once.
A minimal MoveList seam is conditional.
Global rollback for every MoveList false result is not the design.
Search/generation already unmake normal false results and must not double-unmake.
TR supplies the full terminal/controller source and dedup/order matrix.
Terminal facts do not broaden existing reward predicates.

The closure-r2 engine candidate adds absolute typed checkpoints because indexed unmake leaves measured scratch arrays changed.
S0 declares public opaque participant/checkpoint interfaces in the same Base K4 file.
The independently supported corrected K4 bridge supplies Base-owned registration/Prepare/InstallReversible/DiscardAfterAbort.
One validated reversible installation precedes acceptance.
ObserveAccepted delivers only.

E1 owns the committed-only receipt and absolute restore.
Enabled stateful rules/evaluations have explicit sole owners.
Only opted-in attached APMW composition requires strict participant registration.
Ordinary variants keep their existing paths.

Expanded ownership is a reviewed design prerequisite, not implementation authorization.

### 3.3. Progress-to-library interface

One shared declaration lives in proposed `APMW.Client\EarnedMatchEvents.cs`:

```csharp
interface IApmwEarnedEventSink
{
    void Record(EarnedMatchEvents earned);
}
```

`EarnedMatchEvents` contains the originating match ID, commit ID or explicit
terminal-result source, immutable Location IDs, and a separate goal-earned flag.
The terminal source covers a valid match-finished path without fabricating a move.
The sink records accomplishments, not server receipts.
There is no second `IApmwReporter` interface.

The progress module exposes the K4 participant factory and scoped Undo/identity-aware Reset.
Preparation computes the detached pure entry.
InstallReversible changes the owned ledger exactly once before acceptance.
ObserveAccepted delivers resulting intent without another ledger append.

No generic subscriber or caller can perform a second installation.
Unpublished thresholds do not reach the sink.

Unexpected commit pairing and unknown identities produce explicit failures.

### 3.4. World validation and reporting snapshot

C1 parses the exact A/L/C/S/W schemas from the accepted contracts.
It returns immutable typed data or an artifact-specific error.
Our raw artifact and fixture parsers reject duplicates before conversion.
Stock decoded slots retain full exact schema, semantic hash, binding and effective-policy validation.
RI documents that input boundary, not raw-wire duplicate detection or an S0 blocker.
It does not evaluate calibration formulas.

N1 combines that data with authenticated AP identity.
Its UI snapshot separates connection phase from match eligibility.
`NOT REPORTING` takes precedence over `AP DISCONNECTED`.
Neither state implies server acceptance.
U1 must not recompute eligibility from socket flags.

### 3.5. Gate completion

S0 must record the actual public declarations, assembly visibility, complete
fixture JSON, negative mutations, and consumer compile examples.
The integration owner accepts this seam packet before B1/E1/R1/C1/H1/N1 diverge.
Synthetic supported hashes belong only to isolated fixture harnesses.
No production parser accepts an arbitrary supplied pin.

## 4. Work packages

`NEW` identifies a proposed file.
`CM:` identifies the ChecksMate repository.
The manifest lists exact write sets and dependencies.
Read dependencies grant no write ownership.
Each owner also owns the listed focused tests.

### S0 - Shared declarations and fixture contracts

**Owner:** Integration architect. **Prerequisites:** D22 engineering closure and explicit S0 execution authorization.

Own NEW `CommittedMoveObservation.cs`, `ApmwStartingArmyIdentity.cs`,
`ApmwArmyData.cs`, `ApmwCanonicalJson.cs`, `ApmwWorldContext.cs`,
`EarnedMatchEvents.cs`, and `ApmwReportingModels.cs` in the assemblies
specified in the manifest.
Also own NEW `ApmwAllocationPolicy.cs` and `ApmwPreparedSetup.cs` in Games.
K11/K12 must have literal fields, immutable backing data, bindings and cross-assembly compile examples.
Define only the minimal interfaces in section 3 and the
[contract inventory](contracts.md#4-in-process-seams).
`ApmwCanonicalJson.cs` is the one public C# strict-JSON and semantic-hash
primitive in `ChessV.Games`. It passes the shared-data §10.4 fixtures.
Tactical target classes come from `L.target_classes` through the world context,
not from a Games-side catalog.
Materialize the P8/P12 schema fixtures from shared-data section 10.2.
Attach a compile example for every cross-assembly consumer.

**Exit:** All consumers can target one seam packet without guessing fields or
visibility. No behavior-changing decision disappears into a DTO.

### B1 - Army data, geometry, and board composition

**Owner:** Board/data specialist. **Prerequisites:** S0, E1.

Own `ApmwProfiles.cs`, `ApmwPieceCatalog.cs`, NEW `ApmwArmyReader.cs`,
the Army data file, the pure setup composer, and the focused geometry/setup tests.
The reader fills the S0 Army records and uses the S0 canonical JSON primitive.
It also supplies the one Army `type_id`-to-`PieceType` registry.
Author all 20 exact formations and role mappings from the accepted arrays.
Keep AP numeric IDs out of Army data.
Resolve selector fallback to Standard, but reject malformed artifact families.

Implement 2/1/5 and 3/1/6 CPU/neutral/human bands.
Use rank-only reflection.
Register the basic Elephant as 250/250 and Minor, distinct from War Elephant.
Expose CPU start/promotion types independently of human pools.
Provide explicit castling metadata with original actor IDs.

Consume E1's stable placement hooks and participant opt-in through serial transfer of `ApmwChess.cs`.
Bind Army metadata and initial CPU Pieces without waiting for StartMatch.
Transfer that file to U1 after B1 acceptance.

**Exit:** F01/F02/F03 pass at the pure data/composer seam and cold-start binding hooks.

The only ApmwChess writer order is E1 -> B1 -> U1.
Army publication tooling must run without a restored projector.

### E1 - Authoritative move and controller lifecycle

**Owner:** Engine transaction specialist. **Prerequisites:** S0.

Own the exact Base and pure-checkpoint Games files in the engine resource inventory.
MoveList changes are limited to a committed-only receipt/exception branch and root revision.
Existing bool APIs, search ordering and caller-owned speculative rollback remain unchanged.
Match stages accepted-prefix logs, adjudicator state and controller effects.

Do not move network or goal policy into these files.

Publish complete commit/abort/undo facts.
Publish every committed terminal result exactly once, including results from
`TestForWinLossDraw`. The current `MatchFinished` call covers only the
no-legal-moves branch (`Game.cs:1669-1680`).
Replace `lastMove` value-equality suppression with commit identity.
Scope notifications to the game instance.
Do not expose the old `NewMoveSetup` callback as successful progress.
Restore complete state after failed late committed attempts.
Do not use parameterless search-selected Unmake as an exact MoveInfo rollback.
Keep partial exception rollback distinct from normal late legality rejection.
Materialize TR's terminal claims, adjudication, Death, closure and history matrix.
Preserve the accepted stage-victory predicates instead of awarding victory for every terminal source.

Capture complete typed pre-state before Setup.
Restore absolute state once after late rejection, partial exception or staged-history/root failure.
Retain the original exception and entered stage.
H1 prepares a pure atomic entry before acceptance.

Publish Commit to H1, then Terminal, then external compatibility/UI events.
Quarantine failed restoration with original/restore diagnostics.
Opted-in Game speculative and genuine internal/view-only scopes suppress external events and restore residual trial state.
The accepted live GameForm toolbar continues through committed Undo/replay.

Preserve search make/unmake and result restoration.
Expose one ordered controller-change notification before the new controller
can produce progress. N1 performs suppression through that notification.

**Exit:** F04/F05 prove no phantom history, no speculative reports, exact Undo
pairing, one terminal-result notification for extinction and no-move results,
and takeover ordering. Search algorithms remain unchanged.

### Closure-r2 engine resource inventory

This inventory extends the old E1/R1 reservations as an engineering candidate.
Every listed change needs combined review and later execution authorization.
Pure checkpoint ownership grants no rule, coefficient or gameplay change.
The manifest carries each exact path and ordered handoff.

| Exact resource | Future sole writer/order | Permitted candidate scope |
| --- | --- | --- |
| Base `CommittedMoveObservation.cs` | S0 | Public K4/K5/checkpoint/progress types, preparation context, exact install/history receipts, typed failures and captured side-account declarations |
| Base `Game.cs`, `MoveList.cs`, `Board.cs`, `BoardMoveStack.cs` | E1 | Committed receipt, exact typed checkpoints, placement hooks and publication staging |
| Base `MoveCompletionDefaultRule.cs`, `GameAdjudicator.cs`, `Player.cs`, `Match.cs` | E1 | Completion/adjudicator/accepted-prefix state. Player.Gems/NewGame/AttachGame share the captured side account; spending/gain remains reversible |
| Games `Move50Rule.cs`, `RepetitionDrawRule.cs`, `EnPassantRule.cs`, `Apmw\MixedEnPassantRule.cs` | E1 | Absolute counter/hash/EP state and owned internal subscribers, no new rule policy |
| Games `Cards\CardDropRule.cs` | E1 | Same captured account for cold/attached generation, affordability/spend/unmake; actual generation side. R1 reviews unchanged range/reflection |
| Games `Abstract\GenericChess.cs` | E1 | Typed PawnStructureInfo content snapshot, no evaluation policy |
| Games `Evaluations\ColorbindingEvaluation.cs`, `OutpostEvaluation.cs`, `RookTypeEvaluation.cs` | E1 | Enabled scratch/immutable registration state, no coefficient change |
| Games `MiscellaneousGames\ApmwChess.cs` | E1 -> B1 -> U1 | E1 opt-in/participant/hook calls, B1 Army bindings, U1 final attached composition |
| Games `CastlingRule.cs`, `CheckmateRule.cs`, `Apmw\ApmwStalemateRule.cs`, `Apmw\ApmwFirstTurnRule.cs` | R1 | Rights/actor/royal authority and typed state under accepted policy |
| Games `Extinction\ExtinctionRule.cs`, `Extinction\CovenantRule.cs`, `BareKingRule.cs` | R1 | Immutable or explicit stateless participant classification if retained, no competing CPU authority |
| Games `BasicPromotionRule.cs`, `PromoteByReplacementRule.cs` | R1 | Immutable target/conditional-delegate participant and selected promotion policy |
| Games `Pieces\Apmw\Checkers.cs` | R1 | Existing rank-stop audit and delta fixtures, no Army coordinate change |
| Client `LocationHandler.cs` and H1 progress files | H1 | Public Base-interface factory, Client-internal participant/prepared values, one reversible installation and one delivery-only ObserveAccepted |

`PieceType.cs`, `HashKeys.cs`, `Rule.cs`, `Evaluation.cs` and `Search.cs` remain outside the candidate write set.
Game reserves existing per-game material-key indexes through NumSquaresExtended before root trials.
The geometry/type registry freezes before reservation.
Attempts allocate no new keys and assert stable counters.

No shared-key truncation, RNG rewrite or reflection hook applies.
Disabled APMW Development/PawnStructure/LowMaterial evaluations receive no write reservation.
A newly enabled participant requires a named owner and complete state inventory before opt-in.
This includes BerolinaEnPassant.

### R1 - CPU survival, castling, and promotion rules

**Owner:** Search/rules specialist. **Prerequisites:** S0.

Own APMW rule adapters and the explicit royal/promotion/stateless participants in the engine resource inventory.
Supply typed absolute snapshots or explicit immutable/stateless tokens.
Review Checkers/CardDrop rank hypotheses without changing Army coordinates or existing Range/gem costs.
Do not edit `Game.cs`, `Search.cs`, or `ApmwChess.cs`.

A missing core seam returns to E1 instead of producing a concurrent patch.

Audit Checkers' literal eight-rank pawn stop under R1.
Use separate Board/XBoard notation fixtures without changing Army coordinates.

One coordinator replaces the current competing CPU check/extinction/stalemate
authority. It fixes the CPU player at setup.
Multiple CPU Kings remove check obligation.
The last survivor uses checkmate legality.
Zero survivors produce CPU extinction defeat before no-move classification.
Multiple Kings or an unattacked last King with no moves produce CPU victory.

Bind CPU castling to original primary/castler actors and explicit route data.
Retain from/transit/to attack exclusions in both surviving phases.
Use a player-aware promotion adapter with B1's exact CPU list.
Retain human permissions separately.
Hash actor/right state only where board state and existing privilege hashes do
not already determine legal moves.

**Exit:** F03/F06 pass with both colors and human King upgrades 0/1/2.
Existing coefficients, ordinary games, and human rule behavior remain unchanged.

### C1 - Strict v4 client data and geometry entry

**Owner:** Library/data specialist. **Prerequisites:** S0, B1.

Own `ApmwContractV2.cs` migration, `Config.cs`, `ApmwGeometrySelection.cs`,
NEW snapshot/world/profile readers, and contract/selection tests.
The readers fill the S0 world-context types, including the target-class lookup
from `L.target_classes`. `ApmwContractV2.cs` delegates hashing to the S0 primitive.
Reject a match when `C.army` differs from the installed Army hash.
An internal type rename can occur within this package, with explicit consumer
updates through the agreed seam. A v2-named implementation must not imply old-world acceptance.
The new Location-profile reader uses `ApmwLocationProfileV2`, distinct from the
legacy `ApmwLocationProfile` in `CaptureLookup.cs`.
H1 removes that legacy interpretation after its consumers migrate.

Validate installed A/L/C against authenticated slot C/S/W before match creation.
Reject missing data, old contracts, unknown fields/algorithms, conflicting mirrors,
wrong bindings, unsupported stages, and incomplete published profiles.
Retain the original snapshot and exact rational provenance.
Do not price Locations locally.

**Exit:** F07/F08 parser pairs pass, including old 8x8-world rejection and
new-contract Legacy acceptance.
Concrete production hash selection remains I2 work.
After C1 acceptance, transfer `Config.cs` and its named allocation tests to A22.
C1 remains the strict-parser design authority through interface requests, not concurrent edits.

### H1 - Reversible match progress and handler migration

**Owner:** Handler specialist. **Prerequisites:** S0, B1, E1, C1.

Own `LocationHandler.cs`, `CaptureLookup.cs`, NEW `ApmwMatchProgress.cs`,
and capture/fork/progress tests.

Prepare a pure ledger entry before engine acceptance.
Keep current binding, counters, zero-capture entries and pending state reversible through a typed checkpoint.
Expose CreateCommittedProgressParticipant through LocationHandler after validated K2/K6/K9 attachment.
Implement the public Base protocol with Client-internal types.

Install the sole entry before acceptance and return its exact attempt/commit/native-history receipt.
Abort restores participant/native state and discards detached prepared values.
ObserveAccepted releases pending intent without another append or generic Commit subscriber.
Retain source-keyed unsent accepted intent for delivery-only retry, not move replay.

Never include N1's irreversible journal in engine restore.
Replace file/rank inference with stable starting identity.

Apply a complete capture batch once, then report every crossed published threshold.
Use current type only for tactical classification.
Classify through the Army type registry and the world-context target-class lookup.

Keep reversible captured-unit membership, counts, and current identity binding.
Count Kings according to the initial multi-King setup.
Capture Everything requires all starting non-Kings and spare King lives on
the configured endpoint.
Remove direct completion-helper calls and fire-and-forget reporting tasks.
Send immutable events through the S0 sink.

**Exit:** F04/F09/F10 pass. Undo changes local counts, not earned accomplishments.
No handler path reads replacement-session configuration.

### N1 - Connection owner, journals, and atomic admission

**Owner:** Library/lifecycle specialist. **Prerequisites:** S0, C1.

Own `Client.cs`, NEW `ApmwConnectionOwner.cs`, and connection/reporting tests.
Use stock MultiClient.Net 6.6.0.
The owner implements `IApmwEarnedEventSink`. `Record` is its only recording path.
Separate route lifetime, match lifetime, and reporting eligibility.
Retain eligible earned Locations and the goal marker after disconnect/window closure.
Do not retain board/window references.

Serialize suppression with the final predicate and direct captured
`Socket.SendPacketAsync` invocation.
Observe the returned task after releasing the gate.
Locations and goal packets require separate admissions.
All helper completion methods, including empty calls, remain prohibited.

Implement explicit route tracking, fresh helper state, generation-checked
callbacks, and positive retirement evidence.
Bare-host WSS-to-WS fallback follows the bounded connection contract.
A timeout alone leaves uncertain work `Stopping`.
No automatic reconnect, private-worker purge, or durable journal is added.
N1 alone owns DeathLink service creation, delegates and connection-time initialization.
Posted UI/DeathLink work repeats its generation check when it executes.
U1 controls read that owner rather than create a second service.

**Exit:** F05/F11 pass with deterministic barriers and actual pinned-library
acceptance evidence. Earlier admitted packets can finish after suppression.

### G1 - ChecksMate Army consumption and Location profile

**Owner:** Generator/data specialist. **Prerequisites:** S0, B1.

Own `CM:locations.py`, `geometry_progression.py`, `items.py`, NEW Army/profile
readers and profile artifact under `worlds\checksmate`, plus focused tests.
Consume exact Army bytes rather than reconstructing arrays.
Create the reviewed semantic-key/ID ledger, eligibility records, and complete
published capture series. Numeric ID changes require review, not universal renumbering.

Remove production 12x12 paths.
Set human maxima to Chessmen 71, Material 213, and Board Ranks 1.
Derive Location counts from the profile/world selection, not width constants.
Keep `PoolCapacity.for_world` as a consumer unless evidence requires a narrow change.

**Exit:** F01/F02/F09 profile invariants pass.
The independent Army/Location hashes can exist before final C.

### P1 - Settled projection geometry and semantic integration

**Owner:** Cross-language projection specialist. **Prerequisites:** S0, B1, C1, G1.

Own C# active-roster/semantic geometry consumers and Python canonical
contract/resource/placement/protocol modules listed in the manifest.
Migrate wrappers rather than implementing duplicate parsers.
Update the known schema, band formulas, resource loading, and runtime metadata.
Keep protocol 1 distinct from semantic contract 4.0.

Preserve human expected material, mode combinations, ordering, reserve semantics,
and conservative rule metrics where ADR 0022 does not revise them.
Use human depth rather than total ranks in placement, forwardness, and usage.
Keep exact roster metrics distinct from generator logic metrics.

**Exit:** F02/F07 projection fixtures agree across languages.
Outputs remain development fixtures until D22/A22 close the semantic set.
This package cannot publish the final production C or claim allocator compliance.

### G2D - Calibration draft and stable generator interfaces

**Owner:** Generator/rules specialist. **Prerequisites:** G1, P1.

Own calibration/cost modules, data and draft tests from the manifest.
Declare a stable threshold/lifecycle interface for A22.

Declare `build_candidate_requirements(DraftRequirements, CandidateMetricCaps) -> CandidateRequirements`.
Draft inputs include A/L/C, calibration binding, stage/profile set, exact costs/scaling/adjustment and complete path predicates.
The result is immutable post-cap S, integer caps, pure complete-path evaluator and content identity.
Derive rational intrinsic costs and draft post-cap requirements from supplied fixture inputs.

No all-inventory roster test replaces final filled-world core acceptance.
This package does not freeze final obtainable counts or claim final allocation/metric acceptance.
Transfer the named calibration files serially to G2 after draft acceptance.

**Exit:** Arithmetic and interface fixtures are stable for A22.
No final world S, C pin or G2 acceptance is claimed.

### G2 - Calibration, immutable world costs, and complete access paths

**Owner:** Generator/rules specialist. **Prerequisites:** G2D, A22.

Closure-r2 proposes a narrow G2 core-file reservation for `Main.py`, `worlds\AutoWorld.py` and `Fill.py`.
The initial generator packet supports barrier placement.
Retained-choice design and combined review must accept the exact seam before execution.
The barrier follows start-from-pool/item-link normalization and precedes early plando reachability/fill.

Planning includes retained exogenous receipts, not only maximum planned inventory.
Recovery and final inputs precede immutable policy/S/rule freeze.
The existing stage_pre_output route must reject failed real core accessibility.
This includes skip_output.
An existing pre_fill hook alone does not prove the barrier.

No post-fill repricing, sphere mutation or approximate replacement validator applies.
Declare only `prepare_generation_context(NormalizedInventoryCandidate)` as the new optional AutoWorld callback.
A separate static participation marker defaults false, not a user setting.
Its ordinary-world default is a no-op.

It cannot create, submit or remove items.
G2 owns `test\general\test_generation_context.py` for no-op identity/RNG/order regression.
Fill.py changes are conditional on finite variable-plando receipt planning.
Compare retained recipient receipts to the frozen candidate after plando/fill/balancing/finalization.

Retain literal target/clamp/sample/object/disposition/provenance and all ordinary RNG boundaries.
Candidate signatures include actual retained objects, not just receipt vectors.
General safe process reconstruction, finite coverage and occurrence-observer equivalence remain open.
An isolated driver entrypoint requires an accepted exact path before its manifest ownership changes.

Pre-barrier participant queries require explicit refusal or an earlier accepted seam.
No post-submission pool replan, post-fill repricing, or shallow MultiWorld rollback is permitted.
The existing [proof checklist](engineering-closure.md#33-remaining-proof-checklist-and-stopping-criteria) names consumers and stopping evidence.
Useful bounded certificates do not close full AL/PM/LC or D22.

Stage_pre_output uses the actual core on output, skip_output and spoiler_only.
False is failure even when the goal remains beatable.

Own `CM:rules.py`, `__init__.py`, NEW calibration/cost/world-binding modules,
calibration source artifacts, and targeted rule/lifecycle tests.
Use rational arithmetic through corrections, difficulty, and adjustment.
Apply one ceiling, then cap.
Use the accepted reference bank and exact curve definition in ticket 16.

Compile one complete access alternative per eligible stage.
Keep special predicates inside each alternative.
Freeze effective policy, obtainable counts, metrics, caps, rules and S as one state.
Account for upstream start-from-pool and item linking after initial create_items counts.
Complete recovery before dependent fill freezes.
Use the real core accessibility result for final acceptance, including skip_output.
Post-fill/finalize cannot reduce accessibility. pre_output cannot change spheres.
A failure follows the existing generation error path without lowering frozen costs.
`fill_slot_data` serializes the same frozen object.
Tracker reconstruction consumes original costs and cannot reprice them.
Update terminal item-pool budget consumers coherently without redesigning pool policy.

**Exit:** F08/F10/F12 pass.
Synthetic fixtures prove arithmetic and lifecycle, not a finalized production C.
Do not tune provisional costs based on convenience.

### U1 - Game and UI composition

**Owner:** ChessV integration specialist. **Prerequisites:** B1, E1, R1, C1, H1, N1, P1, A22.

Own `ApmwChess.cs`, APMW geometry game registrations, `GameForm.cs`,
`ApmwForm.cs` and related designer changes, necessary load-entry adapters,
and affected project files.
Also own `APMW.Client\Convenience.cs` for Q82's five successful connection details.
The list stores host/port/slot only and selection fills fields without auto-connect.
The [storage candidate](../large-board-enemy-armies/contracts/apmw-connection-reporting.md#recent-details-storage-candidate)
defines the exact disk record, admission source, MRU update and visible failure behavior.
Use N1's single DeathLink owner.
Apply Q79-Q81 timing/persistence without mutating active matches or frozen costs.
Integrate K11/K12 with preview, live ItemGeneration and sidecar adaptation.
Wire the agreed modules once.
Use the APMW base type rather than exact old subclass checks.

Connect setup identity, player-specific promotions, CPU rule authority, and
handler/sink attachment.
Wire all controller replacement paths to the pre-change suppression gate.
Preserve Undo/history/engine notifications.
Reject user-facing FEN continuation without disabling internal setup fixtures.
Show the shared reporting badge in the required game and connection surfaces.
Window close and transport cleanup have different effects.

The exact interaction routes are in the handler report's entry-route inventory.
`Match.SetPlayerToHuman` and `SetPlayerToInternalEngine` have no replacement event.
`HumanEnabled` is only turn/UI availability, not takeover evidence.
Computer Plays, Stop Thinking, and automatic human restoration need the same gate.

Current history controls perform committed Undo/replay on the live game.
They are not view-only merely because they show history.
This design retains the live ledger-aware path rather than adding a new viewer.
Any surface presented as view-only must leave continuation, ledger, and reporting unchanged.

FEN refusal must precede `LoadFENForm.btnOK_Click` clearing game state.
SGF loading through `MainForm`, `Manager.LoadGame`, and executable saved variables
must not bypass the same APMW continuation restriction.
Ordinary-game loading remains unchanged.

**Exit:** F03-F11 pass through actual game/UI entry points.
U1 registers source/resource additions after independent packages land.
SDK default inclusion is retained where sufficient.

U1 receives ApmwChess only after E1 -> B1.
Attach the selected participants, immutable capture, H1 preparation and N1 sink once.
Cold-start/reset tests consume complete internal records, not bare FEN.
The accepted live GameForm Undo/replay toolbar remains ledger-aware committed continuation.

Only genuine internal/view-only operations use no-publication snapshot scopes.
Local AI timing/capture follows Q79 without an invented Weakening formula.

### I1 - Publication and package tooling

**Owner:** Release integration specialist. **Prerequisites:** C1, G1, P1.

Own Army publication tooling, projector build/release tools, C# lock parser,
PowerShell restore script, and package parser tests.
Implement metadata schema 2, all three data descriptors, and minimum-client agreement.
Preserve provenance, archive integrity, extraction safety, and exclusive restore checks.

**Exit:** F07/F13 synthetic tooling cases pass.
Tooling acceptance can precede final A22 implementation within the authorized pass.
No placeholder production lock or fabricated release identity is allowed.

### D22 - Consolidate policy and close engineering design

**Owner:** Existing settings decision owner. **Prerequisites:** None.
**State:** Selected policy, open engineering closure.

Consolidate the closure round without reselecting its gameplay policy.
Targets are 11/5, 15/7, 19/9, 27/15, 33/19 on the prepared board.
Complete EP/NM/AL/PM/LC/TX/TR/CS in the closure register.
No mandatory new allocation-policy ID or world UUID is required.
A concrete incompatibility returns to the coordinator with evidence.

**Exit:** Literal schema, full independent fixture inputs and supported engineering proofs agree across the owners.
No implementation starts before this design gate and explicit authorization.

### A22 - Implement the approved allocator amendment

**Owner:** Cross-language allocation specialist. **Prerequisites:** D22, S0, C1, G1, P1, G2D.
**State:** Waiting for engineering closure and execution authorization.

Own Legacy funding/conversion, Fundamental recovery, generator pool planning and effective policy.
Also own live `ItemGeneration.cs`, normalized `Config.cs`, pockets and conservative metrics.
Own generator `options.py`, `item_pool.py`, `collection.py`, `item_utils.py` and planning modules.
The manifest records serial C1/P1 transfers, including sidecar snapshot/adaptation.
Use integrated outstanding obligations and ordered relaxation.
Hard-disabled actions, inventory, entitlements and capacity remain invariant.
Generator replanning does not grant extra runtime Chessmen from Material.
Implement the finite progress/exhaustion and exclusive ledger specified by AL.

Use forward affordable completion admission that cannot retract the proposed ordinary purchase.
Paid Pawn100->150->200 spends extra50/50 and refunds only released extra.
Atomic interfamily changes replace the current extra category, never debit both categories.
Captured selector/action eligibility still governs every target and recovery intermediate.

Preserve the raw-empty/Off four-gate default and explicit normalized twelve-action hard-disabled policy.
Publish effective instructions, not requested settings, traces or a hypothetical roster.

Consume G2D's stable requirement interface. G2 final acceptance then consumes A22 outputs.
Preserve grant provenance and prevent reserve double spending.
Apply prepared-board targets 11/15/19/27/33 and actual pocket membership as selected.
Retain the historical approximation rules, not an invented 100-point ceiling.
Compute pocket support and credit from captured actual families, not receipt counts.
Prove PM against actual inventory prefixes and every permitted final pocket distribution.

The paid-quality domain needs its own certificate.
The old interfamily-only corpus is insufficient.
Return proof-limit through the existing failure envelope/nonzero exit, not success or exhausted.
Canonical state excludes diagnostic/PRF/trial history and includes both historical one-use replacement marks.

**Exit:** The amended algorithm, logic metrics, input schema, and C agree.
The fully specified fixtures, including 785/485/500/285 accounting, pass.
No exact algorithm or new identifier is assigned by this assessment.

### T1 - Attached-match integration evidence

**Owner:** Integration test specialist. **Prerequisites:** U1, G2, I1, A22.

Own cross-domain integration tests, including `ApmwGameCharacterizationTests.cs`.
Use attached matches, both colors, independent board fixtures, and isolated
global catalog state.
Do not generate expected arrays or costs through the implementation under test.

**Exit:** All settled behavior fixtures pass through real module composition.
Source-only evidence does not satisfy the real-package gate.
Changed shared `Game` and rule paths also require existing ordinary-game regression coverage.

### I2 - Final pin, packaged interoperability, and release gate

**Owner:** Cross-repository integration owner.
**Prerequisites:** T1, A22, G2, I1 and explicit broader settings closure.

Freeze the final C only after the allocation amendment lands.
Publish Army bytes first, then vendor them in ChecksMate.
Build actual x86/x64 projector packages with the final A/L/C.
Derive the production lock from released metadata.
Restore and consume both real executables from the client package.
Rerun affected source fixtures against the final pin.

**Exit:** F07/F13 pass with actual packages and compatibility pairs.
No release, push, or publication occurs without its normal explicit authorization.

## 5. Dependency and conflict policy

The investigation's five domains are not five simultaneously writable folders.
The recommended sequence is:

| Wave | Candidate work |
| --- | --- |
| Design only | Consolidate D22 and close every pre-S0 engineering gate. No declaration implementation |
| 0 | S0 after explicit authorization, then initial S0 PR user approval |
| 1 | E1, R1 |
| 2 | B1 after E1's composition-hook transfer |
| 3 | C1 and G1 |
| 4 | H1, N1, P1 |
| 5 | G2D and I1 after their stable prerequisites |
| 6 | A22 after G2D, including serial C1/P1 transfers |
| 7 | Final G2 and U1 acceptance after A22, then T1 |
| 8 | I2 after all semantic and release gates |

These waves are a display aid. The manifest's exact dependency edges govern.
A worker can begin earlier with accepted fixture adapters only when its
declared inputs are stable. That does not mark an unfinished prerequisite complete.

| Shared file or concern | Exclusive write owner |
| --- | --- |
| `Game.cs`, conditional `MoveList.cs`, `Match.cs`, active `ApmwCore.cs` | E1 |
| CPU rule implementations and castling/promotion rule adapter | R1 |
| `ApmwProfiles.cs`, catalog, pure composer | B1 |
| `ApmwChess.cs` | E1 -> B1 -> U1, explicit ordered serial transfers |
| Game registration and shared project files | U1 |
| `LocationHandler.cs`, `CaptureLookup.cs` | H1 |
| `Client.cs`, connection owner | N1 |
| `Config.cs` | C1, then explicit serial transfer to A22 |
| Client v4 parser, geometry entry | C1 |
| C# and Python projection internals, snapshots/adapters | P1, then named serial transfers to A22 |
| Live ItemGeneration, generator options/pool/collection/item_utils/planning | A22 |
| `Convenience.cs` and recent-details UI | U1 |
| Python `locations.py`, progression, item maxima | G1 |
| Calibration/cost draft modules | G2D, then explicit serial transfer to G2 |
| Python `rules.py`, world lifecycle | G2, consuming A22 final policy/metrics |
| Release/restore tooling | I1 |
| Desktop release CI and final production lock | I2 |

Workers submit interface requests to the owner instead of editing its files.
One worker owns each existing test file.
New focused test files permit concurrent work without shared fixture churn.
Tests that mutate `ApmwCore` or catalog state retain serialization and cleanup.

## 6. Acceptance fixture register

The linked source fixtures contain full construction, mutation order, and
expected state. This register does not replace them with shorthand.
All contract paths are under `.scratch\large-board-enemy-armies\contracts`.

| ID | Fixture and exact required result | Complete authority |
| --- | --- | --- |
| F01 | 20 formations and 40 color-resolved boards. 10x10 has 12 Pawns/15 other non-Kings/1 King. 12x10 has 14/18/2 | Shared-data sections 4.1/10.3, tickets 12/13/15 |
| F02 | Human capacities: 6x8 29/11/24/18, 8x8 39/15/32/24, 10x8 49/19/40/30, 10x10 59/29/50/40, 12x10 71/35/60/48 | Shared-data section 6.1. Order: non-primary/non-Pawn/gross Pawn/forwardness |
| F03 | Exact CPU promotion sets across families/geometries, independent of human inventories. Basic Elephant is Minor only. Amazon is non-royal | ADR 0017, royal fixtures registration/isolation matrix |
| F04 | One successful commit/Undo, no speculative progress, failed late rejection leaves no history/report, branch continuation restores counts | Royal fixtures rollback matrix, interaction ticket 21 |
| F05 | Admission first permits completion. Suppression first makes zero sends. Controller restoration and Undo never restore eligibility | Connection contract atomic admission and deterministic fixtures |
| F06 | Additional/primary King capture, 2-to-1 transitions, 2-to-0 Checkers batch, repeated load 2-to-1-to-2, no castling inheritance | Full `royal-lifecycle-fixtures.md` legal-board constructions |
| F07 | Exact A/L/C/S/W and effective-policy validation; duplicates reject in our raw parsers; stock decoded objects receive full schema/hash/binding checks without a wire-detection claim; old-world rejection, protocol 1 vs runtime/semantic pins and positive/negative package pairs | Shared-data sections 3/10.1/10.2/10.4; RI input boundary |
| F08 | `361045/162 * 27/20 + 240` gives ceiling 3249, cap 3000 gives 3000. Synthetic +100 intrinsic gives 3384 | Full `world-cost-snapshot-v1.md` required-fixture table |
| F09 | Six Rookies Lions retain separate roles. Promoted Pawn retains its role/class. Initial multi-King eligibility never changes after capture | Ticket 15 and ADRs 0006/0007 |
| F10 | Earlier-board Any 15 can complete on 8x8. Capture Everything remains endpoint-only. No Any 33/34 or Pieces 20 Location is invented | Ticket 15, shared-data section 5, royal two-King extinction fixture |
| F11 | Same generation/team/slot/game and W/C/S permit journal replay, including after window closure. Port is not identity. Mismatch retains intent | Full connection contract deterministic fixture sequence |
| F12 | One complete geometry path must supply all resources and special predicates. Bypass cannot remove tactic/castler conditions | Snapshot availability section and shared-data section 10.2 |
| F13 | Real x86 and x64 archives restore and execute under identical A/L/C. Minimum-client and byte/semantic integrity checks remain strict | Shared-data sections 9/10.5 |
| F14 | Source credit 785, expected Major 485, concrete Rook debit 500 leaves 285 before signed netting. Exclusive transfers/reserves and visible historical overdraw | ADR 0022 Q51-Q55, closure register AL |
| F15 | None/total/both in both modes. Complete action/default/empty/all-disabled schema. Targets and R. Four receipts at limit one -> 2/1/1. Twelve -> 4/4/4. Thirteen+ remain received without credit | Ticket 24 matrix and Q61-Q68/Q73/Q75-Q78; closure EP/NM/PM |
| F16 | Forced unsuitable allocation recovers with ordered relaxation and seeded uniform smallest progress. Scarcity, hard-disabled actions, last-resort type overflow, seven protected units, finite exhaustion and conservation | Ticket 24 Q69-Q73/Q77/Q83; closure AL/PM |
| F17 | One immutable inventory/settings/seeds/IgnoreCastlers/pocket capture. Settings A->B refuses stale response or regenerates B. Actual-family support and all three pocket-material bounds exclude primary/pockets from B | Ticket 24 Q64/Q67/Q68/Q73/Q80; closure EP/CS/PM |
| F18 | Final obtainable counts include start transfer/links. Recovery precedes freeze. Rules/S/slot use same state. Real filled-world core acceptance includes skip_output and tracker restoration | Snapshot lifecycle; ticket 24 Q65/Q69; closure LC |
| F19 | Same-world local controls and world-switch defaults. AI adds/caps five next match only. One DeathLink owner initializes each reconnect. Five successful host/port/slot-only entries survive restart, no auto-connect | Ticket 24 Q79-Q82; closure NM/TR |

F14 alone does not specify an allocator.
The target pairs are selected, not illustrative.
F04 also requires complete-state equality after late committed rejection and partial exception cases.
F06 requires cold-start/reset cache evidence and separate Board/XBoard coordinate fixtures.
F07 distinguishes duplicate rejection in our raw parsers from strict stock decoded-object validation.
It does not require upstream wire-duplicate detection or a new library feature.

The proposed [engine design fixture](fixtures/engine-lifecycle-v3.json) contains revision3 literal lifecycle inputs.
The proposed [allocation design fixture](fixtures/allocation-contract-v4.json) contains 21 manifest-bound synthetic cases and independent expectation records.
They are two data owners in this effort, not runtime policy identities or copied model/probe source.
Full A/L/C/S/W fixture composition and native parser/handover acceptance remain separate.

The engine fixture root is version3 and the allocation wrapper root is version1.
Both contain design JSON, not production Army/W/K12 artifacts or executed native fixtures.
Symbolic lifecycle hashes/test identities are not production schema examples or pins.
F04/F06/TR inputs and F17/F19 lifecycle models have a separate evidence mapping in the royal fixtures.
Common/generator fixture revisions and native acceptance remain pending.

F15-F19 are not passing production tests.
F06's direct extinction case leaves a CPU Rook, so victory must not imply

Capture Everything.

## 7. Validation and completion rules

Each package runs its smallest existing focused suite plus its new acceptance cases.
The specialist reports contain commands and existing test names.
No new test framework is required.
After shared engine changes, the existing ordinary-game suites also run.
Builds must cover `ChessV.GUI` and both test projects.

The source and package gates remain separate.
`ProjectorRestoreValidation.ps1` currently uses a fake executable.
Its success cannot satisfy F13.
CI must restore first and then run a dedicated actual-package consumer for both
architectures. The release path cannot silently skip a missing production lock.

Each worker returns its baseline, changed files, exact fixture results, interface
deviations, remaining blockers, and producer/consumer artifacts.
Generated hashes and source commits come from actual bytes and revisions.
The integration owner validates the final cross-repository set.

## 8. Exclusions and stop conditions

This design does not authorize a search rewrite, tracker UI, Regions refactor,
ordinary-game Army migration, balance retuning, or transport fork.
It does not authorize user-facing APMW FEN resume, durable journals, automatic
reconnect, legacy-world conversion, or 12x12 support.

A worker stops at an unselected behavior-changing rule, a required interface
change outside its write set, or a contradictory authoritative fixture.
It records the narrow blocker and returns it to the named owner.
It does not remove the fixture or weaken validation to proceed.

Pre-S0 readiness remains blocked by the named engineering gates.
Reconciliation can complete without claiming readiness.
All implementation remains behind design closure, explicit authorization and initial S0 PR approval.
