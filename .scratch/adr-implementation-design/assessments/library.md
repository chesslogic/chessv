# Library / connection / reporting architectural assessment

Baseline: `78b604e43f684c8032d1af23955485d1e196954f`  
Scope: Archipelago library adapter, connection lifecycle, reporting admission and
journals, strict client-contract/cost-snapshot validation, and shared UI
connection/reporting state. This is a source probe only. It does not authorize
implementation and does not decide the unspecified policy in ADR 0022.

## Executive assessment

The current adapter is not a safe seam for the accepted 0.4.0 behavior. A
process-wide `ArchipelagoClient` singleton owns a replaceable
`ArchipelagoSession`, a static `connectionTask`, mutable global `ApmwConfig`,
and a singleton `LocationHandler`. Connection replacement and reporting are
therefore coordinated by convention rather than by one owner with a
generation/match gate.

The highest-leverage change is one deep application owner behind a small
interface: it owns route generations, the published validated connection,
match reporting records, connection retirement, atomic library admission, and
the UI-facing reporting snapshot. `LocationHandler` should remain the owner of
capture-role and victory *detection*, but should call that reporter instead of
calling MultiClient.Net helpers directly. The board, search, and generator
domains should not acquire knowledge of sockets, journals, or UI flags.

## Existing source map and behavior

### Adapter and lifecycle

* `APMW.Client\Client.cs:88-136` defines the singleton client surface,
  mutable `Session`, `OnConnect`, `OnClientDisconnect`,
  `GeometryStateChanged`, and `MatchStateChanged`. The public interface has no
  connection-generation, validated-world binding, reporting state, or
  retirement result.
* `APMW.Client\Client.cs:139-161` rejects only an incomplete static
  `connectionTask` and repeats the last `(host, port, slot)` request. It calls
  `Dispose()` before creating a replacement session. This is not duplicate
  request serialization for an actual connect/login operation and the
  address/slot latch is not an identity or retirement proof.
* `APMW.Client\Client.cs:162-198` runs synchronous
  `TryConnectAndLogin` inside a `Task`, reads `Session` instead of the captured
  route for login, and returns on login failure without a route retirement
  record. The invocation and its actual task are not tracked as one operation.
* `APMW.Client\Client.cs:199-294` initializes the singleton `LocationHandler`,
  installs `SocketClosed`, validates a minimum client version and
  `apmw_contract`, initializes global `ApmwConfig`, installs `ItemHandler`, and
  publishes `OnConnect`. Validation is partial: `apmw_contract` is optional,
  there is no world generation/team/slot/game binding object, no cost snapshot
  check, no published Location-set check, and no old-world/new-contract
  admission boundary matching ADR 0005.
* `APMW.Client\Client.cs:295-340` unloads the match, calls
  `DisconnectAsync()` without awaiting or classifying retirement, clears
  `Session`, resets global config/geometry, and raises disconnect UI state.
  It can dispose a replacement while callbacks from the old session remain
  captured. `Session_SocketClosed` at `:343-355` calls `Dispose()` recursively
  for the current session and has no route-generation check.
* `APMW.Client\Client.cs:399-456` publishes geometry state from mutable
  `ItemHandler`/global config. It is useful UI state but is not an authenticated
  connection publication and must not be used as a reporting gate.
* `APMW.Client\Config.cs:104-181, 299-382` is a singleton mutable slot-data
  projection. `Instantiate` parses both legacy and current forms, and
  `ResetConnectionState` clears only `SlotData`, `CurrentContract`, and
  `UsesCurrentContract`. It does not freeze a world binding, published IDs,
  cost snapshot, or per-match copy. Legacy itemization remains a valid mode,
  but legacy *world contracts* must not remain accepted in the new client
  (ADR 0005).
* `APMW.Client\ApmwContractV2.cs:1-90, 140-340, 970-1090` is a strong strict
  parser for the v2 manifest: exact fields, versions, canonical hash,
  geometry, roles, algorithms, and cost-related profile metadata are parsed
  into immutable value records. It does not currently expose the complete
  world binding/cost-snapshot admission object required by ADR 0016 and ADR
  0021; the caller does not retain the parser's hash as a frozen match
  binding.
* `APMW.Client\ApmwGeometrySelection.cs:31-112` maps validated geometry to
  registered game names and handles the 6x8 opening. It should remain a
  selector/projection, not become a connection or reporting authority.

### Location and final-goal routes

* `APMW.Client\LocationHandler.cs:29-80` is a singleton wired to global
  `ApmwCore` move events. `Initialize` captures a session in a closure:
  `victory = () => new Task(() => Victory(session)).Start()` and
  `deathlink = ...`. There is no match identity or route generation in those
  callbacks.
* `LocationHandler.cs:108-160` starts and ends a match, resets reversible
  capture state, and subscribes `Match.Finished`. `EndMatch` can send a
  resignation DeathLink and clears the ledger, but it does not retain a
  reporting journal when a window closes.
* `LocationHandler.cs:177-197` correctly restores current capture counters and
  original-square mappings from the `MoveDiff` stack. This is the reversible
  ledger required by ADR 0019; the earned-Location journal must be separate
  and must not be reconstructed from this stack or from the final board.
* `LocationHandler.cs:351-377` detects committed human-move Locations, then
  fire-and-forget calls `LocationCheckHelper.CompleteLocationChecks`. This is
  the current Location send route. It has no originating match envelope, no
  admission gate, no published-ID validation, and no replay journal.
* `LocationHandler.cs:383-391` (the `CheckVictoryAndDeathlink` path) invokes
  the captured `victory` callback after a win. A result callback can outlive
  the match/session that created it.
* `LocationHandler.cs:768-789` is the final route: `Victory` derives all
  checkmate IDs through `CaptureLookup`/`ApmwLocationProfile`, calls
  `CompleteLocationChecks` synchronously, then directly sends
  `StatusUpdatePacket(ClientGoal)` through the captured session. These must
  become two separately guarded admissions: one Location packet and one goal
  packet. Empty completion helpers and unrestricted goal helpers must not be
  used.
* `APMW.Client\CaptureLookup.cs:11-95, 165+` owns the current hard-coded
  geometry-to-Location-name mapping. It is a lookup/detection dependency, not
  the reporting identity source. Read-only lookup must use the match's frozen
  profile, not replacement-session global config.
* `LocationHandler.cs:820-885` validates “currently playing APMW” through
  global initialization and game attributes. This is not a reporting
  eligibility check and must not be widened into one.

### UI publication

* `ChessV.GUI\Forms\ApmwForm.cs:35-47` subscribes directly to client events.
* `ApmwForm.cs:160-190` parses a user-entered URL into a forced `wss` host/port
  and immediately calls `Connect`; there is no explicit URI/WS route plan or
  bare-host WSS-then-WS fallback.
* `ApmwForm.cs:257-307` initializes DeathLink and updates controls from
  `OnConnect`/slot data. The form has no shared connection/reporting snapshot,
  no `NOT REPORTING` state, and no distinction between transport loss and
  permanent match suppression.
* `ApmwForm.cs:308-321` clears controls on every client disconnect. This is
  incorrect for a retained eligible match whose journal must survive transport
  interruption and for a suppressed match whose badge must take precedence.
* `ApmwForm.cs:348-401` uses `GeometrySelection.IsConnected` and match-active
  state for launch controls. Those controls can remain, but connection badge
  text must come from the reporting authority rather than an independent UI
  flag.
* `ChessV.GUI\ChessV.GUI.csproj:91` references `APMW.Client`; the GUI does not
  directly reference MultiClient.Net. `APMW.Client\APMW.Client.csproj:35-38`
  pins `Archipelago.MultiClient.Net` 6.6.0. `APMW.Client\APMW.Client.csproj:43`
  excludes the root `ApmwManager.cs`, so that file is not the active adapter.

### Tests and fixtures already present

* `APMW.Test\ApmwContractV2Tests.cs:10-120` and
  `APMW.Test\Fixtures\ProjectionV2\baseline.json` verify the strict v2 parser
  and frozen manifest hash.
* `APMW.Test\LocationHandlerUnitTests.cs:1-220` verifies capture-role
  mapping, promotion lineage, and geometry goal sequences through 12x12.
  These are detection/ledger tests, not library-admission tests.
* `APMW.Test\LocationHandlerForkTests.cs` and
  `LocationHandlerForkBoardTests.cs` are the existing rollback/fork seams to
  preserve. They must not be made dependent on a live socket.
* `APMW.Test\ApmwConfigTests.cs` covers slot-data projection and should remain
  the fixture for legacy itemization behavior, not legacy world-contract
  acceptance.

## ADR gap matrix

| Accepted decision | Current behavior | Required gap |
|---|---|---|
| ADR 0001 exact shared deployment data | Client uses its own `CaptureLookup`/profiles; contract parser has exact geometry data but no published binding consumed by reporting | Freeze the validated contract/army/location/cost binding in one world record; do not make generator or board code reconstruct it here |
| ADR 0005 new-contract-only at 0.4.0 | `Client.cs:232-254` accepts absent `apmw_contract` and `Config.cs` retains legacy parsing | Reject absent/old world contracts before publication; retain Legacy only as itemization |
| ADR 0016 world-bound cost snapshot | No client-side cost snapshot is extracted, hashed, or checked at connection or admission | Parse and freeze calibration version/hash plus per-geometry paths; compare every admission to the match binding; never recalculate or use a scalar minimum |
| ADR 0018 no FEN resume | No library surface currently resumes from FEN; `FENStart` remains ordinary internal/game data | Keep the adapter free of FEN resume/import; do not turn journal or reconnect into board restoration |
| ADR 0019 reliable Undo | MoveDiff is present and restores counters/original squares | Keep this ledger separate from earned IDs; ensure reporter records only successful committed moves and does not retract IDs on undo |
| ADR 0020 permanent suppression after takeover | No match reporting eligibility record or controller-change gate; all sends remain possible | Add one-way match suppression before changed-controller progress; guard Locations and goal; allow library-admitted pre-takeover packets to finish |
| ADR 0021 replay earned Locations/goal | No process-memory journal; `EndMatch` clears local state and reconnect creates a new session without replay | Retain match record after window close, bind to world/slot/contract/cost/published IDs, replay after verified same-world recovery, retain on mismatch, lose on process exit |

## Proposed deep module and interface

### Seam and ownership

**Proposed NEW file:** `APMW.Client\ApmwReportingModels.cs`  
Define immutable records/enums only:

* `ApmwWorldBinding`: nonempty generation name, authenticated team/slot/game,
  validated immutable v4 contract identity/hash, published Location-ID set,
  and cost-snapshot version/hash plus per-geometry paths. The new world object
  must not publicly depend on `ApmwContractV2`.
* `ApmwMatchId` and `ApmwMatchRecord`: original match identity, frozen
  binding, earned `HashSet<long>` (private mutation), separate final-goal
  marker, and irreversible `CanReport`/suppressed state.
* `ApmwReportEnvelope`: originating match, intended connection generation,
  immutable packet payload, and frozen binding.
* `ApmwConnectionSnapshot` and `ApmwReportingSnapshot`: two related
  UI-readable values. Connection phases (`Disconnected`, `Connecting`,
  `Authenticating`, `Connected`, `Stopping`, and validation failure) are
  distinct from match reporting eligibility (`Eligible`, `NotReporting`).
  The UI composes them: `NOT REPORTING` takes precedence over connection loss
  and includes the fact that earlier admissions may still finish. Do not model
  these as one flat mutually-exclusive enum.

**Proposed NEW file:** `APMW.Client\ApmwConnectionOwner.cs`  
One owner controls route creation/retirement, current publication, match
records, and admission. The external interface should be small:

* `Connect(ApmwConnectRequest)` — one explicit request; explicit URI is one
  route, bare host is bounded WSS then WS.
* `Disconnect()` / `SuppressMatch(ApmwMatchId)` — serialized with admission.
* `BeginMatch(ApmwMatchBinding)` and `EndMatch(ApmwMatchId)` — no board/window
  reference retained by the journal.
* `RecordCommittedLocations(matchId, ids)` and
  `RecordFinalGoal(matchId)` — record first, then request guarded admission.
* `GetSnapshot()` and `SnapshotChanged`.

The owner is deep because callers do not know socket state, queue admission,
identity comparison, retirement evidence, replay ordering, or lock rules.
Internally it may have private route, parser, journal, and packet adapters, but
those are not UI or handler contracts.

### Atomic gate and data lifetime

Use one private owner lock (or one serialized owner queue) for:

1. current published route/generation and stopping state;
2. match eligibility/suppression;
3. world/slot/contract/cost/published-ID comparisons;
4. final packet preparation and the direct
   `capturedSession.Socket.SendPacketAsync(packet)` call.

The final predicate is: originating match eligible; intended generation is the
published authenticated route; route is not stopping; nonempty generation
name, authenticated team/slot/game, validated world binding, contract hash,
cost hash, and published IDs match; all outgoing IDs are members of the
original set. A mismatch blocks admission and retains the journal.

Prepare an immutable packet before taking the gate. Under the gate, evaluate
the predicate and invoke the captured session's direct socket send. In the
pinned 6.6.0 asset, queue insertion occurs synchronously before the returned
task, so that invocation is the Q45 admission boundary. Release the lock
before observing the task. No await, callback, or completion helper may occur
between predicate and invocation. Diagnostics happen after release.

Takeover uses the same gate and sets suppression before controller-changed
progress. Admission before suppression may finish, including private queued
packets; suppression before attempted admission makes no socket call. A
failed admitted task does not authorize retry after suppression.

The journal is process-memory only. It survives transport loss and game-window
closure, but not process restart. It retains the original match and binding
even after the board/window is gone. Undo changes the reversible ledger, never
the earned set or goal marker. Replay creates fresh envelopes and repeats the
same predicate for each Location packet and the separate goal packet.

### Connection route and retirement

Each route owns a fresh session, generation, actual connect task, tracked
login invocation/task, event delegates, and retirement evidence. Install the
minimal nonthrowing first `SocketOpened` recorder before exposing the route.
Track `ConnectAsync` once per route and the entire synchronous-plus-task
`LoginAsync` invocation. Never reuse a retired session.

Timeout revokes route authority but does not claim retirement. Only positive
pre-opening failure, settled connect/login plus awaited disconnect, captured
peer-close evidence, positive abort, or the typed terminal close probe may
classify retirement. A pending connect/login/close remains `Stopping`; no
fallback follows it. Bare-host WSS may reserve WS only after positive
pre-opening WSS retirement and while the original request remains active.
Old callbacks repeat the generation check before publishing, disposing a
replacement, or changing match state.

### Error ownership

The owner emits explicit diagnostics for malformed contract, binding mismatch,
admission denial, queue/send failure, and uncertain retirement. It must not
turn timeout into “disconnected,” arbitrary close exceptions into “terminated,”
or task completion into server acceptance. The stock library remains the
transport adapter; no transport fork, purge, durable journal, or automatic
reconnect is proposed.

## Files that must change vs inspect-only

### Must change in a later implementation

* `APMW.Client\Client.cs` — delegate public connect/disconnect/match/reporting
  operations to the owner; stop publishing raw session as authority; route old
  callbacks by generation; expose the shared snapshot.
* `APMW.Client\Config.cs` — retain the parsed immutable contract/cost binding
  for the current route, reject absent/legacy world contracts at the new
  release boundary, while preserving Legacy itemization.
* `APMW.Client\LocationHandler.cs` — accept a match-scoped reporter; record
  committed IDs and goal marker; remove direct completion-helper and direct
  goal-send routes; preserve MoveDiff and capture-role detection.
* `ChessV.GUI\Forms\ApmwForm.cs` — subscribe to the shared snapshot, display
  `NOT REPORTING` ahead of `AP DISCONNECTED`, and preserve journal semantics
  across connection loss/window closure.
* `APMW.Client\APMW.Client.csproj` — include proposed files if SDK defaults are
  insufficient and retain the pinned 6.6.0 dependency.
* `APMW.Test\APMW.Test.csproj` — include proposed tests/fixtures if they are
  not discovered by SDK defaults.

### Inspect-only / do not broaden

* `APMW.Client\CaptureLookup.cs`, `ApmwGeometrySelection.cs`,
  `ItemHandler.cs`, and `ApmwContractV2.cs` are lookup, geometry, item, and
  parser dependencies. Change only if the implementation proves a missing
  immutable contract/cost field; do not move reporting policy into them.
* `ChessV.Games\MiscellaneousGames\Apmw*.cs`,
  `ChessV.Games\Rules\Apmw\*.cs`, and board setup sources remain board/setup
  owners. They should not reference connection or journals.
* Search/engine controller sources and `Game.MoveTakenBack` remain search and
  reversible-ledger owners. Controller takeover must notify the owner through
  one narrow match event, not add socket logic to search.
* `APMW.Client\ApmwManager.cs`, root `ApmwManager.cs`, and `APMW.Client\Config.cs`
  legacy parsing paths are not parallel alternative adapters. Confirm active
  registration before editing; the client project explicitly removes its local
  `ApmwManager.cs`.

## Bounded implementation packages and exclusive ownership

| Package | Exclusive files | Shared-file conflict to resolve |
|---|---|---|
| Library owner and models | **NEW** `ApmwReportingModels.cs`, **NEW** `ApmwConnectionOwner.cs`, **NEW** `ApmwRoute.cs` if needed | `Client.cs` is the only integration seam; no other package edits owner internals |
| Strict binding/config | `Config.cs`, **NEW** `ApmwWorldBindingParser.cs` only if parser extraction is necessary | `ApmwContractV2.cs` is inspect-first; coordinate any new cost fields with parent generator contract |
| Handler/reporting bridge | `LocationHandler.cs`, **NEW** `IApmwReporter.cs` | Must not edit owner files; parent must approve the reporter interface before handler fanout |
| UI state | `ChessV.GUI\Forms\ApmwForm.cs` and, if required, **NEW** `ApmwReportingStatusPresenter.cs` | Consumes only `ApmwReportingSnapshot`; no direct session or journal reads |
| Tests | **NEW** `APMW.Test\ApmwConnectionOwnerTests.cs`, **NEW** `APMW.Test\ApmwReportingAdmissionTests.cs`, **NEW** `APMW.Test\ApmwConnectionLifecycleTests.cs` | Existing LocationHandler and contract tests remain owned by their current domain packages |
| Project registration | `APMW.Client.csproj`, `APMW.Test.csproj` only | One owner must make project-file edits; do not have UI/handler agents edit project files concurrently |

The parent owns external generator/package integration. The exact schemas are
already authoritative in `contracts\shared-data-contract-v4.md` sections 2,
7.2, and 7.3, plus `world-cost-snapshot-v1.md`; implementation work must
materialize fixtures from those schemas rather than reopen their field
decisions. This probe does not invent missing ADR 0022 policy.

## Dependencies on other domains

* **Handler:** must provide the committed Location IDs and final-goal event
  through `IApmwReporter`; it owns capture-role, multi-capture, victory
  detection, and the reversible MoveDiff ledger. It must not decide admission.
* **Board/setup:** supplies the actual match geometry and starting deployment.
  The reporter consumes the match's frozen profile; it must not infer
  starting-role lineage from FEN or a later board.
* **Search/engine:** supplies the controller-change/takeover transition and
  real Undo event. The transition must be emitted before changed-controller
  progress and must be irreversible for reporting only.
* **External generator/package (parent):** supplies the exact v4 contract,
  world-bound cost snapshot, published Location set, generation name, and
  authenticated slot semantics. A parser can validate only fields actually
  present in the agreed package.
* **UI:** consumes one snapshot. It must not equate connection loss with
  non-reporting, show a delivery receipt, or clear retained journals on window
  closure.

## Concrete tests and smallest validation commands

Add deterministic barrier-based tests; do not use timing sleeps.

* `ApmwReportingAdmissionTests.AdmitLocationThenSuppress_AllowsQueuedPacketToFinish`
* `ApmwReportingAdmissionTests.SuppressBeforeAdmission_MakesZeroSocketCalls`
* `ApmwReportingAdmissionTests.LocationAndGoalHaveIndependentAdmissionGates`
* `ApmwReportingAdmissionTests.FailedAdmittedPacket_IsNotRetriedAfterSuppression`
* `ApmwReportingAdmissionTests.ReplayRequiresWorldSlotContractAndCostBinding`
* `ApmwReportingAdmissionTests.PortOnlyChange_DoesNotBlockValidatedReplay`
* `ApmwReportingAdmissionTests.PublishedLocationMismatch_RetainsJournal`
* `ApmwReportingAdmissionTests.UndoDoesNotRemoveEarnedLocationOrGoalMarker`
* `ApmwReportingAdmissionTests.ClosedWindowReplayNeedsNoBoardReference`
* `ApmwReportingAdmissionTests.RestartDoesNotRestoreProcessMemoryJournal`
* `ApmwConnectionLifecycleTests.DuplicateConnectWhileActualConnectPending_StartsOneRoute`
* `ApmwConnectionLifecycleTests.BareHostFallsBackOnlyAfterPositivePreOpenWssRetirement`
* `ApmwConnectionLifecycleTests.OpenedRouteNeverFallsBackToWs`
* `ApmwConnectionLifecycleTests.HungConnectRemainsStopping`
* `ApmwConnectionLifecycleTests.OldCallbacksCannotPublishReplacementOrMutateMatch`
* `ApmwConnectionLifecycleTests.LoginAndCloseOperationsAreTrackedUntilSettled`
* `ApmwContractV2Tests` existing baseline/hash and rejection tests, extended
  only with the agreed cost-snapshot fixture.
* `LocationHandlerUnitTests` existing promotion, capture-role, and goal-stage
  tests, plus a reporter mock verifying only committed IDs reach the seam.

Fixture binding from `.scratch\large-board-enemy-armies\contracts\apmw-connection-reporting.md`:
generation `fixture-seed`, game `fixture-game`, team 1, slot 2, published IDs
`{71001, 71002}`, hashes `WHash`, `CHash`, `CostHash`, endpoint
`wss://fixture.invalid:38281` and port-only alternative 38282. The contract's
listed negative and ordering cases should become the acceptance matrix, not
timed integration tests.

Smallest validation commands after implementation:

```powershell
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwConnection"
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwReporting"
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~LocationHandler"
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwContractV2"
dotnet build .\ChessV.GUI\ChessV.GUI.csproj --no-restore
```

No command was executed for this source probe.

## Uncertainties and non-goals

* The exact 0.4.0 world contract schema for cost snapshots and published
  Location IDs is authoritative in
  `contracts\shared-data-contract-v4.md` sections 2, 7.2, and 7.3, plus
  `world-cost-snapshot-v1.md`. The remaining work is implementation fixture
  materialization and validation; do not silently accept a partial binding.
* ADR 0022 is partially specified; this assessment makes no decision about
  legacy pawn-material conversion policy.
* Automatic reconnection, durable journal files, process restart recovery,
  guaranteed cancellation of private MultiClient.Net workers, generation UUIDs,
  server-instance identity, and exactly-once delivery are non-goals.
* FEN import/resume, game-save restoration, and synthetic Locations are
  non-goals and remain rejected by ADR 0018/0021.
* The stock helper's private queue and `Socket.SendPacketAsync` admission
  behavior must be proved against the pinned 6.6.0 asset and supported .NET 7
  runtime during implementation acceptance; this report treats the contract
  document's evidence as the design input, not as an executed build result.
* UI labels and concrete class names remain implementation choices. The
  semantic precedence (`NOT REPORTING` over `AP DISCONNECTED`) is not.

## Top blockers for parent

1. Materialize implementation fixtures from the already authoritative schemas:
   `contracts\shared-data-contract-v4.md` sections 2, 7.2, and 7.3, plus
   `world-cost-snapshot-v1.md`. Use those exact fields for generation name,
   authenticated team/slot/game, contract identity, cost-snapshot version/hash,
   per-geometry paths, and the original published Location set.
2. Approve the `IApmwReporter`/snapshot seam before handler and UI fanout so
   no child package calls MultiClient.Net directly.
3. Add a pinned v4 world-binding/cost-snapshot fixture materialized from those
   contracts. The existing `ProjectionV2\baseline.json` proves parser
   mechanics but not replay admission.
4. Confirm the engine's controller-change event ordering relative to committed
   move publication; the owner must suppress before changed-controller progress.

## Producer integration clarification

The related ChecksMate producer baseline is clean at
`0772bac724ff483043b56979f0ef078b9c2d264a`. The parent owns
`tools\Restore-ApmwProjector.ps1`, the real package/CI workflow, and Python
generator integration; this library probe does not assign those files to a
client package.

The producer is currently frozen at semantic producer v3, runtime pin
`0.1.0`, and build manifest 1, including `minimum_client_version`. The restore
v1 artifact omits `minimum_client_version` and pins the old semantic contract
v2. The transport protocol is 1 on both sides. These are different concepts:
the transport protocol and old semantic contract are not the supported APMW
semantic/runtime compatibility pin. The client adapter must validate the
supported semantic/runtime contract and complete world binding; it must not
treat transport protocol 1 or semantic contract v2 as sufficient evidence.

ADR 0022 blocks the final shared semantic pin and release decision. It does
not block pure schema/parser work or exact army-data work that can be validated
without selecting the missing semantic policy. The new world object should not
publicly depend on `ApmwContractV2`; use a v4 immutable type materialized from
the authoritative shared-data and world-cost schemas.
