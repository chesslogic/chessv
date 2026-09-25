# Handler and committed-move source probe

Status: bounded architecture assessment; no runtime implementation  
Baseline: `78b604e43f684c8032d1af23955485d1e196954f`  
Scope owner: handler/event/capture/history and client-side allocation probe

## Outcome

The current handler cannot satisfy the accepted capture and Undo contract by
incremental edits to `LocationHandler` alone. Two upstream facts must become
explicit:

1. `ApmwChessGame` must publish an immutable starting-army identity snapshot
   from the exact resolved formation. `LocationHandler` must not infer identity
   from current type, file, rank, or board width.
2. `Game` must publish an unambiguous committed/aborted/undone lifecycle.
   `LocationHandler` must not push reversible history from the current
   pre-mutation `NewMoveSetup` callback.

With those seams in place, the handler can be reduced to a deep match-progress
module: apply one committed observation, update one reversible ledger, classify
all crossed goals, and return one immutable batch of earned events. The library
child owns journal retention, reporting eligibility, reconnect replay, and
atomic network admission. It must receive earned events; it must not receive
mutable handler counters or callbacks that re-read the current singleton.

ADR 0022 does not currently authorize a Legacy allocator implementation. The
existing roster/projection code can be characterized and instrumented, but
reservation, composition target pairs, fallback, and the wire/algorithm
revision remain unresolved policy.

## Current source map

### Move lifecycle and handler state

| Path and symbol | Current behavior | Assessment |
| --- | --- | --- |
| `ChessV.Base\Game.cs:1589-1684`, `Game.MakeMove` | For a committed move, invokes `ApmwCore.NewMoveSetup` before `MoveList.MakeMove`; invokes `NewMovePlayed` only after board/rule/result mutation succeeds | Commit success is observable, but failure has no matching abort notification. Setup is not a history boundary. |
| `ChessV.Base\Game.cs:1736-1783`, `Game.UndoMove` | Speculative Undo restores its result snapshot and emits `MoveReverted(Speculative)`; committed Undo emits `MoveTakenBack` and `MoveReverted(Committed)` | Suitable distinction exists. Committed Undo lacks the reverted move/commit identity, so consumers can only assume stack alignment. |
| `ChessV.Base\Game.cs:1103-1123`, `Game.LoadFEN` | Replaces board/rule position and calls `Rule.PositionLoaded` | No generic client notification resets or replaces APMW identity/history. ADR 0018 forbids player-facing APMW FEN resume. Only the already accepted internal fixture/load scope needs an explicit reset path; this probe does not add a save or resume format. |
| `ChessV.Base\ApmwCore.cs:75-77` | Global lists carry `MoveInfo` setup and played callbacks | This is the active event surface. It has no match ID, attempt ID, execution mode, abort, position-reset, or complete multi-capture delta. |
| `APMW.Core\ApmwEvents.cs:8-32` | Contains an isolated `Starter` singleton with only startup/provider lists | This project is not referenced by the active solution paths found in the probe. It is not the active `ChessV.Base.ApmwCore` bus and must not become a second lifecycle authority. |
| root `ApmwEvents.cs:1-20` | Malformed legacy source outside the active project | Inspect-only historical debris; do not build the new seam here. |
| `APMW.Client\LocationHandler.cs:98-114`, nested `MoveDiff` | Records pawn/piece deltas and previous square-origin mappings | Good reversible-ledger seed, but too weak for stable unit IDs, starting classes, King lives, and commit validation. |
| `LocationHandler.cs:117-160`, `StartMatch` / `EndMatch` | Resets counters, origin map, and diff stack at match boundaries | Suitable reset intent. State is held on a process singleton and is not keyed by match. |
| `LocationHandler.cs:224-249`, `SetupMove` / `StartMoveDiff` | Pushes a diff and snapshots the board before commit is known | A failed committed attempt leaves a phantom stack entry. Every successful non-capture does create a boundary, which is correct, but only accidentally. |
| `LocationHandler.cs:349-378`, `HandleMove` | Classifies and starts an unobserved `Task` that calls `CompleteLocationChecks`; then updates lineage | Reporting and reversible mutation are interleaved. Errors are unobserved. The handler bypasses the required library admission owner. |
| `LocationHandler.cs:179-202`, `MoveTakenBackHandler` | Pops one diff, subtracts counters, restores overwritten destination mappings | Suitable behavior for direct tests, but assumes one setup callback exactly equals one successful committed move. |

### Identity, capture arithmetic, and tactics

| Path and symbol | Current behavior | Required change |
| --- | --- | --- |
| `APMW.Client\CaptureLookup.cs:13-107`, `ApmwLocationProfile` | Derives pawn/non-King ceilings from width; recognizes legacy `12x12`; marks only `12x12` final | Replace count and endpoint semantics with the frozen world/formation profile. Exactly five geometries are supported. |
| `CaptureLookup.cs:109-190`, `CaptureLookup` | Maps a starting file to a legacy display name | Replace with role-ID-to-published-Location lookup. Center Rook, Queen, forward pieces, Lions, Elephants, and Rearguard pawns cannot be represented by a file-only map. |
| `LocationHandler.cs:476-582`, `RecordCaptureLocations` | Follows an origin-square map, then classifies home-rank as piece and every other rank as pawn | Violates ADRs 0006/0007 for forward non-pawns, promoted starting pawns, Kings, and doubled Rearguards. Classify the captured starting unit's immutable `starting_class` and `role_id`. |
| `LocationHandler.cs:251-346`, `HandleCheckersMultiCapture` | Diffs board occupancy and recursively calls `HandleMove` for each inferred capture | One committed move is recursively presented as several handler moves. Location dispatch can occur per subcapture, threshold crossing depends on inferred order, and state updates are not one atomic ledger entry. |
| `LocationHandler.cs:584-598`, `RecordCapture` | Increments aggregate pawn/piece counters and active diff | Preserve this concept inside one ledger application, but count distinct starting unit IDs and include eligible King captures only when the immutable starting setup has multiple Kings. |
| `LocationHandler.cs:600-731`, `RecordThreatLocations` / `RecordForksForAttacker` | Uses the active game's global `ApmwCore` type sets to classify current tactical targets | Current type, not starting role, is correct for tactics. Move the taxonomy to an immutable tactical-class catalog from the resolved game/profile. Add basic Elephant as Minor only; keep Amazon outside Queen; both King types qualify as King. |
| `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:65-97` | Publishes global Pawn/Minor/Major/Jack/Queen/Amazon sets. Basic Elephant is absent; Amazon is already separate from Queens. | Preserve Amazon separation. Add the accepted basic Elephant registration/classification in board/catalog work, then expose a match-local read-only tactical catalog rather than singleton sets. |
| `LocationHandler.cs:733-759`, `UpdateMoveState` | Transfers origin square for the primary mover and detects a castler through board reference comparison | Promotion can retain origin only because the square map ignores current type. Exact secondary movement should come from the committed board delta, not a castling-specific heuristic in the handler. |
| `LocationHandler.cs:829-883`, stage helpers | Uses live config/legacy goal rules to choose goal and Capture Everything stages | Use the match's frozen world binding, published Location set, and configured ending geometry. Do not query replacement-session globals. |

### Setup and roster/projection surfaces

| Path and symbol | Existing suitable behavior | Gap or ownership |
| --- | --- | --- |
| `ChessV.Games\MiscellaneousGames\ApmwChess.cs:379-469`, `SetOtherVariables` | Resolves CPU family and composes the internal generated setup | This is the correct setup-side seam to bind exact starting unit IDs/role IDs. The board child owns exact arrays and shared army consumption. |
| `ApmwChess.cs:128-152` and `ApmwPieceCatalog.cs:234-262` | Publishes current type categories to the global `ApmwCore` singleton | Suitable only as legacy compatibility. New capture identity must be match-local and immutable. |
| `APMW.Client\OwnedRosterGeneration.cs:13-61` | Has explicit placement roles and material-ledger entries | Useful accounting vocabulary for human roster generation; these are not CPU capture roles. Do not reuse `SourcePlacementRole` as starting-army capture identity. |
| `OwnedRosterGeneration.cs:431-475`, `GenerateLegacy` | Applies non-pawn planning/upgrades, then creates one Pawn slot per effective Pawn item | This is exactly the ADR 0022 gap: it does not retain Legacy pawn-material conversion. It is not safe to replace until policy is complete. |
| `APMW.Client\ActiveRosterProjection.cs:44-104`, `ProjectionGeometry` | Makes capacities and formation bands explicit | Existing `ValidStageIds` includes `12x12`; final contract must reject it. Board child owns exact geometry/layout changes. |
| `APMW.Client\ProjectionV2SemanticEngine.cs:16-160` | Has deterministic semantic roles, action order, and material records for parity fixtures | Algorithm is pinned to the old semantic contract. It cannot independently invent the ADR 0022 allocator or wire revision. |
| `APMW.Client\ItemHandler.cs:145-221`, `ItemProgressSnapshot` | Separates Legacy family items from Fundamental Chessmen/Material/Castlers | Preserve this mode split. ADR 0022 explicitly does not merge Fundamental entitlement with Legacy funding. |

### Controller replacement and player-facing position/history entry routes

This table closes the bounded ADR 0018/0020 entry-route inventory. Parent
assigns generic `Match`/`Game` facts to **E1** and GUI/Manager policy wiring to
**U1**.

| Owner | Exact path and symbol | Current route and event availability | Required boundary |
| --- | --- | --- | --- |
| E1 | `ChessV.Base\Match.cs:135-146`, `Match.SetPlayerToHuman(int)` | Replaces an `InternalEngine` with a new `HumanPlayer`, attaches it, then assigns `m_player[side]`. It raises no controller-replaced event. | Publish one authoritative replacement fact before the new controller can produce progress, or provide an E1-owned callback invoked atomically with replacement. |
| E1 | `ChessV.Base\Match.cs:148-160`, `Match.SetPlayerToInternalEngine(int)` | Replaces a `HumanPlayer` with a new `InternalEngine`, attaches it, then assigns `m_player[side]`. It raises no controller-replaced event. | Same one-way APMW suppression trigger as the reverse replacement. Fresh-match initialization through `SetPlayer` is not takeover. |
| E1 | `ChessV.Base\Match.cs:33,73,482,583`, `HumanEnabledEventHandler` / `Match.HumanEnabled` | Existing event carries only `bool humanEnabled`; `startTurn` raises it according to the controller whose turn begins. It is also used during match start/stop. It does not identify side, old/new controller, cause, or whether replacement occurred. | Do not use `HumanEnabled` as the ADR 0020 takeover event. It is a turn/UI availability signal, not controller-change evidence. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:226-253`, `menuitem_ComputerPlays0_Click` | Toggles `Game.ComputerControlled[0]`, changes label/name, then calls `Match.SetPlayerToInternalEngine(0)` or `SetPlayerToHuman(0)`. | Route through the E1 replacement fact; suppression must precede starting the replacement engine. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:256-283`, `menuitem_ComputerPlays1_Click` | Same replacement path for side 1. | Same ordering and permanent match suppression. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:168-190`, `GameForm.MovePlayed` | If `ComputerControlled[sideOnClock]` is false but the current controller is not human, automatically calls `SetPlayerToHuman` after a move. | This indirect replacement crosses the same ADR 0020 boundary; do not guard only menu clicks. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:687-695`, `menuitem_StopThinking_Click` | Invokes both Computer Plays handlers for computer-controlled sides, then calls `StopThinking`. Thus it can replace controllers as a side effect. | Must trigger the same permanent suppression through the shared E1 replacement route. A pure search cancellation that does not replace a controller remains distinct. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:448-482`, `menu_Game_DropDownOpening` | Enables Computer Plays and take-back operations for Human/InternalEngine pairs outside Review Mode; disables controller toggles during Review Mode. | UI availability is not the reporting gate. Library admission uses the match's one-way state. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:510-517`, `menuitem_TakeBackMove_Click` | Calls `Game.UndoMove(true)` on the active game. | This is live committed Undo and must traverse the reversible ledger. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:611-621`, `menuitem_TakeBackAllMoves_Click` | Repeats `Game.UndoMove(true)` until the active move stack is empty. | Also live committed Undo; each committed move reverses exactly once. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:703-754`, `pictPrevious_Click`; `GameForm.cs:796-834`, `pictFirst_Click` | Enters Review Mode, stores historical moves, and calls default `Game.UndoMove()`. Previous can then call default `Game.MakeMove(...)` to restore highlighting. | Despite the label, this mutates the live game through committed-mode defaults and emits committed APMW Undo/replay callbacks. It is not accepted view-only history. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:757-789`, `pictStop_Click`; `GameForm.cs:837-883`, `pictNext_Click` / `pictLast_Click` | Replays queued moves with default `Game.MakeMove(...)` to return or move forward in Review Mode. | Also live committed replay, not view-only traversal. A future view-only history module must not call these mutation paths or produce gameplay reports. |
| U1 | `ChessV.GUI\Forms\GameForm.Designer.cs:636-641`, `menuitem_LoadPositionByFEN` | Registers the user-visible `Get or Set Position FEN ...` command. | Disable/refuse the set/resume operation for APMW while preserving ordinary-game behavior and any permitted read-only display. |
| U1 | `ChessV.GUI\Forms\GameForm.cs:652-684`, `menuitem_LoadPositionByFEN_Click` | Opens `LoadFENForm`; after success, clears move/UI state, resets engines, and may resume thinking on the replacement position. | This is the active player-facing FEN continuation route that ADR 0018 must refuse before mutation. |
| U1 | `ChessV.GUI\Forms\LoadFENForm.cs:68-78`, `btnOK_Click` | If edited, calls `Game.ClearGameState()`, `Game.LoadFEN(...)`, then sets executable `Game.FENStart`. | APMW refusal must occur before `ClearGameState`; a failed/refused operation leaves board, identity ledger, and pending reporting state unchanged. |
| U1 | `ChessV.GUI\Forms\MainForm.cs:386-407`, `btnLoadGame_Click` | User-visible SGF file picker calls `Manager.LoadGame(reader)` and opens the resulting game. | An SGF containing APMW variables/FEN cannot bypass ADR 0018. Ordinary-game SGF load remains supported. |
| U1 | `ChessV.Manager\Manager.cs:233-257`, `Manager.LoadGame` | `SavedGameReader` supplies game variables; `CreateGame` initializes from them; then `PlayMoves` replays saved moves. | Gate APMW saved-game continuation before creating/resuming a playable APMW match unless a later accepted format explicitly authorizes it. This assessment authorizes none. |
| U1 | `ChessV.Manager\SavedGameReader.cs:75-111`, `SavedGameReader.matchVariable` | Treats arbitrary saved variables, including `FENStart`, as executable game definitions before the `MOVES` section. | Parsing is not authorization. U1 must distinguish/refuse APMW continuation rather than treating embedded FEN as inert metadata. |

`GameForm` subscribes to `Match.HumanEnabled` at
`ChessV.GUI\Forms\GameForm.cs:81-85`, and its handler at
`GameForm.cs:1004-1020` only toggles thinking/history UI. That confirms the
existing event is unsuitable for permanent reporting suppression.

## Required move traces

### Successful committed move

1. `Game.MakeMove` allocates an attempt identity only for
   `MoveExecutionMode.Committed`.
2. A pre-mutation observation captures the authoritative before-state needed to
   identify every affected piece. It does not push handler history.
3. Board/rule/result mutation succeeds.
4. `Game` publishes exactly one `CommittedMoveObservation` after success. The
   observation identifies the originating game/match, attempt, primary move,
   all captures, and all piece transitions, including promotion replacement,
   en passant, castling, and Checkers multi-capture.
5. The reversible ledger applies the complete batch atomically and pushes one
   entry, including for a zero-capture move.
6. Classification returns all newly crossed *published* Location IDs and any
   earned-goal marker.
7. The handler hands one immutable `EarnedMatchEvents` value to the library
   owner. It neither starts a task nor calls an Archipelago helper.

### Speculative move

`MoveExecutionMode.Speculative` does not open an APMW attempt and does not call
the handler. `Game` continues to restore engine state through its existing
speculative snapshot and `MoveReverted(Speculative)` path
(`Game.cs:1589-1599,1669-1684,1736-1778`). Search make/unmake therefore cannot
create ledger entries, earned events, or transport work.

### Failed committed move

The current setup callback can run before `MoveList.MakeMove` returns false or
throws (`Game.cs:1597-1603`). The new lifecycle must close that attempt with an
abort in a `catch`/failure path. The observer discards its pending before-state.
It produces no committed ledger entry and no earned event. A later successful
move must not inherit the failed attempt's snapshot. This is required even if
the underlying move list has already restored board/rule state.

### Committed Undo and branch change

Committed Undo identifies the exact reverted commit. The ledger checks that ID
against its top entry, restores counters, captured-unit membership, role
lineage, and any identity transfers, then pops once. Earned Locations already
handed to the library are not removed. A different continuation applies to the
restored local state. Reporting eligibility is not in this ledger and cannot be
restored by Undo.

### Position reconstruction/load reset

Player-facing FEN continuation is rejected by ADR 0018. The already accepted
internal fixture/load reconstruction scope must supply both board state and its
starting-unit identity artifact. On successful replacement within that bounded
scope, the setup owner supplies a new immutable `StartingArmyIdentity`; the
progress module atomically resets/rebuilds its ledger from the fixture's
restoration record. Bare FEN cannot synthesize role identity. A failed internal
replacement leaves the old snapshot and ledger intact. This is acceptance
harness support, not a newly supported APMW save or resume format.

### Promotion and multi-capture

- Promotion changes current tactical type but not `StartingUnitId`,
  `CaptureRoleId`, or starting class.
- One Checkers move supplies one observation containing every capture. The
  ledger records one entry, updates all captured unit IDs, then reports every
  newly crossed published threshold once.
- Both Kings captured in that batch can increment the piece count twice in an
  initial multi-King setup and complete Regicide once. No unpublished Any 33/34
  or Pieces 20 Location is invented.

## Proposed small interfaces

Names are design-level and may be adjusted by the implementation owner. The
shape and ownership are the requirement.

### Immutable starting identity

**NEW:** `ChessV.Games\MiscellaneousGames\ApmwStartingArmyIdentity.cs`

```csharp
public interface IApmwStartingArmyIdentity
{
  IReadOnlyList<ApmwStartingUnit> Units { get; }
  ApmwStartingUnit ResolveInitialPiece(Piece piece);
  ApmwStartingCounts Counts { get; }
  string StageId { get; }
}

public sealed record ApmwStartingUnit(
  string UnitId,
  string CaptureRoleId,
  ApmwStartingClass StartingClass);
```

`APMW.Client\APMW.Client.csproj:33-34` references both `ChessV.Base` and
`ChessV.Games`, and no `InternalsVisibleTo` seam was found. Therefore this
cross-assembly interface and all types exposed by it must be `public` with
read-only members (or parent synthesis must deliberately relocate the contract
to a lower shared assembly). An `internal` Games interface cannot be consumed
directly by Client and is not a valid design.

The concrete metadata snapshot is immutable after setup. It is created by
`ApmwChessGame` from exact shared-formation records and contains stable unit,
role, class, and count metadata. It may also establish the initial
piece-instance bindings, hiding coordinates, family substitution, reflection,
and artifact parsing. `ResolveInitialPiece` fails explicitly for an unbound CPU
piece instead of guessing from square/type.

The immutable snapshot is not, by itself, the complete runtime binding. Some
promotion implementations replace the `Piece` object. `ApmwMatchProgress`
therefore owns a reversible current `Piece`-to-`UnitId` binding initialized from
this snapshot. Each authoritative `PieceTransition` transfers the stable unit
binding from the old piece instance to the replacement; the move diff records
both sides so Undo restores the prior binding. Starting metadata never changes,
but the current object associated with that metadata can change. A design that
only stores an immutable `Dictionary<Piece, ApmwStartingUnit>` will lose
identity across replacement promotion.

### Committed observation and reversible ledger

**NEW:** `ChessV.Base\CommittedMoveObservation.cs`  
**NEW:** `APMW.Client\ApmwMatchProgress.cs`

```csharp
public sealed record CommittedMoveObservation(
  long CommitId,
  MoveInfo Move,
  IReadOnlyList<PieceTransition> Transitions,
  IReadOnlyList<CapturedPiece> Captures);

internal interface IApmwMoveLedger
{
  EarnedMatchEvents Apply(CommittedMoveObservation move);
  void Undo(long commitId);
  void Reset(ApmwProgressRestoration restoration);
}
```

`Game` owns the truth that an observation committed. The board/rule
implementation owns complete transitions/captures. The APMW ledger owns
classification and reversal. `Apply` is atomic and returns results rather than
performing I/O. `Undo` validates stack pairing; it does not silently ignore an
unexpected commit. `Reset` exists only for the accepted internal fixture/load
scope and requires its identity-aware restoration record, not a FEN or a new
player-facing resume format.

This is deeper than exposing `BeginCapture`, `IncrementPawn`,
`RestoreOriginalSquare`, and per-threshold methods: one interface operation
exercises the entire commit policy and is also the natural test surface.

### Earned events handed to the library owner

**NEW (shared with library child):** `APMW.Client\EarnedMatchEvents.cs`

```csharp
internal sealed record EarnedMatchEvents(
  ApmwMatchId MatchId,
  long CommitId,
  IReadOnlyCollection<long> LocationIds,
  bool GoalEarned);

internal interface IApmwEarnedEventSink
{
  void Record(EarnedMatchEvents earned);
}
```

This is the one library-owned reporting contract selected by parent synthesis.
Do not add a second `IApmwReporter` abstraction beside it.

The sink call means "the match earned these immutable accomplishments," not
"the server accepted them." The library adapter records the process-memory
journal, applies the permanent match gate, and attempts admission. Empty batches
need not cross the seam. Match/world/slot binding is retained by the library
match record; the handler supplies the originating `MatchId`, never the current
singleton connection.

## Shared ownership; do not assign these files exclusively to the handler child

### `ChessV.Base\Game.cs`

The search child and handler child both depend on this file. Search owns
speculative make/unmake and result correctness. Handler work needs only the
generic committed-attempt lifecycle and exact Undo identity. Parent synthesis
must assign one integration owner or sequence patches:

1. introduce generic committed/aborted/undone observations and tests;
2. preserve existing speculative behavior;
3. then connect the APMW adapter.

Do not add APMW capture rules to `Game`; it should publish facts, not classify
Locations.

### `ChessV.Games\MiscellaneousGames\ApmwChess.cs`

The board child owns exact formation consumption, CPU promotions, and setup.
The handler child needs the resulting identity snapshot and tactical catalog.
Board/setup work should publish those immutable artifacts; handler work should
only consume them. Do not duplicate the role coordinate catalog in
`LocationHandler`.

### Project files

New source registration touches `ChessV.Base\ChessV.Base.csproj`,
`ChessV.Games\ChessV.Games.csproj`, `APMW.Client\APMW.Client.csproj`, and
`APMW.Test\APMW.Test.csproj` as applicable. Assign one project-registration
owner after the parallel source packages land.

## File-level development design

### Must change for settled handler work

| Path | Required development |
| --- | --- |
| `ChessV.Base\Game.cs` | Publish committed success, committed abort, exact committed Undo, and successful position-replacement observations without changing speculative semantics. |
| `ChessV.Base\ApmwCore.cs` | Retire or adapt `NewMoveSetup`/`NewMovePlayed` after consumers move to the generic lifecycle. Do not expand it into a second network owner. |
| `ChessV.Games\MiscellaneousGames\ApmwChess.cs` | Expose the setup-owned immutable starting identity and match-local tactical catalog produced by board/contract work. |
| `APMW.Client\LocationHandler.cs` | Consume committed observations; delegate reversible state to `ApmwMatchProgress`; return earned events to the sink; remove direct `Task`/helper reporting and file/rank identity inference. |
| `APMW.Client\CaptureLookup.cs` | Replace width/file lookup and legacy stage counts with semantic role and frozen published-profile lookup; reject `12x12`. |
| `APMW.Test\LocationHandlerUnitTests.cs` | Replace direct setup/play pairing tests with commit/undo/reset tests across the ledger interface; retain focused classification tests. |
| `APMW.Test\GameMoveNotificationTests.cs` | Prove one commit, one abort, no speculative APMW event, and exact Undo identity. |
| `APMW.Test\LocationHandlerForkTests.cs` and `LocationHandlerForkBoardTests.cs` | Retarget tactical tests to the immutable tactical catalog and committed event seam. |

### Proposed new files

| Path | Purpose |
| --- | --- |
| `ChessV.Base\CommittedMoveObservation.cs` | Generic immutable commit/abort/undo payload records. |
| `ChessV.Games\MiscellaneousGames\ApmwStartingArmyIdentity.cs` | Public read-only cross-assembly starting metadata contract plus setup-owned initial piece bindings; runtime replacement bindings belong to the ledger. |
| `ChessV.Games\MiscellaneousGames\ApmwTacticalClassCatalog.cs` | Match-local current-type classification for Pawn/Minor/Major/Queen/King. |
| `APMW.Client\ApmwMatchProgress.cs` | Deep reversible-ledger and classification module. |
| `APMW.Client\EarnedMatchEvents.cs` | Immutable handler-to-library seam. |
| `APMW.Test\ApmwMatchProgressTests.cs` | Pure commit/undo/threshold/identity acceptance tests. |
| `APMW.Test\ApmwStartingIdentityIntegrationTests.cs` | Setup-to-piece identity, replacement-promotion transfer/Undo, reflection, and accepted internal reconstruction tests. |

### Inspect-only for this package

| Path | Reason |
| --- | --- |
| `APMW.Core\ApmwEvents.cs` and root `ApmwEvents.cs` | Orphan/legacy surfaces, not the active bus. Removal/retirement is separate cleanup. |
| `APMW.Client\ItemHandler.cs` | Its Legacy/Fundamental input split is suitable; no handler-event change required. |
| `APMW.Client\ItemGeneration.cs` | Historical allocation helpers are evidence only; do not revive them as ADR 0022 policy. |
| `APMW.Client\OwnedRosterGeneration.cs` | Safe for characterization and later allocator work, but blocked for behavioral ADR 0022 edits. |
| `APMW.Client\ActiveRosterProjection.cs` | Board/projection owner changes geometry support; handler consumes outputs. |
| `APMW.Client\ProjectionV2SemanticEngine.cs` | External integration owner coordinates any new semantic/wire algorithm. |
| `APMW.Client\Client.cs` and library connection/reporting files | Library child owns transport, journals, admission, replay, and status reporting. |
| `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs` and exact army data | Board child owns arrays, role coordinates, counts, and promotion lists. |

## Bounded exclusive implementation packages

These packages are separable only after the listed prerequisites.

| Package | Exclusive production files | Prerequisites | Deliverable |
| --- | --- | --- | --- |
| H1: generic committed lifecycle | `ChessV.Base\Game.cs`, **NEW** `CommittedMoveObservation.cs`; corresponding base/APMW notification tests | Parent arbitration with search child | Commit/abort/undo/reset facts; no APMW classification |
| H2: immutable setup identity | **NEW** `ApmwStartingArmyIdentity.cs`, **NEW** `ApmwTacticalClassCatalog.cs`; integration points in `ApmwChess.cs` | Board child exact shared formation/role artifact | Match-local immutable identity and tactical taxonomy |
| H3: reversible progress core | **NEW** `ApmwMatchProgress.cs`, **NEW** `EarnedMatchEvents.cs`, **NEW** progress tests | Frozen Location profile + H1 payload contract + H2 identity interface | Pure atomic apply/undo/reset and earned-event batches |
| H4: handler adapter | `LocationHandler.cs`, `CaptureLookup.cs`, handler/fork tests | H1-H3 and library sink interface | Existing handler converted to commit consumer; no direct networking |
| H5: project registration/integration | affected `.csproj` files | H1-H4 source complete | One conflict-free registration/build patch |

H1 and H2 can proceed in parallel after parent fixes the payload contracts.
H3 can use fakes for both while they land. H4 is the integration package and
must not start by editing shared files speculatively.

## Prerequisites from other probes

- **Board child:** exact five-geometry formation records, stable unit/role IDs,
  reflected setup binding, basic Elephant registration, CPU promotion lists,
  and explicit rejection of `12x12`.
- **Search child:** complete authoritative move deltas for multi-capture,
  en-passant, castling, and promotion; royal phase/castling state must already
  reverse in engine make/unmake.
- **Library child:** `IApmwEarnedEventSink` adapter and match ID semantics;
  process-memory journal, permanent eligibility, world/slot binding, and atomic
  admission remain entirely on that side.
- **Parent/integration:** v4 shared contract and frozen Location profile,
  published Location membership, exact ending geometry, package restoration,
  and project-file arbitration.

## Acceptance cases and test surfaces

### Engine/event lifecycle

Primary surface: `APMW.Test\GameMoveNotificationTests.cs`.

- Successful committed move produces one commit observation.
- Repeated speculative make/unmake produces none.
- A late failed committed move produces one abort and no commit.
- A following successful move uses a fresh attempt.
- Committed Undo identifies and reverts exactly the latest commit.
- Zero-capture committed moves have ledger boundaries.
- Successful identity-aware reconstruction in the accepted internal
  fixture/load scope emits one reset; failed internal load emits none and leaves
  the prior match state intact. No case implies a player-facing resume format.

### Starting identity and captures

Primary surfaces: **NEW** `ApmwStartingIdentityIntegrationTests.cs`,
**NEW** `ApmwMatchProgressTests.cs`, existing
`APMW.Test\LocationHandlerUnitTests.cs`.

- Distinct Rookies Lions at B1, E1, and D2 retain three role IDs.
- 6x8 C1 is Center Rook, not Queen's Rook.
- A starting pawn promoted to Queen still increments Pawn and completes only
  its pawn-role Location.
- Forward minors, Elephants, and the 12x10 Queen are non-pawns despite rank.
- Main and Rearguard pawns on one file remain distinct.
- One-King setup excludes King capture; initial two-King setup includes either
  King even after one survives.
- Two-King Checkers capture is one commit/one Undo, completes Regicide once,
  increments two piece captures, and reports every crossed published threshold.
- Capture Everything requires all starting non-Kings and all spare King lives
  on the frozen endpoint; captured Kings cannot replace an uncaptured Rook.
- 8x8 in a larger world can complete published Any 15; it cannot complete
  endpoint-only Capture Everything.
- No unpublished threshold is handed to the sink.
- Undo restores local counts/identity but does not retract a previously earned
  Location from the fake sink.
- Undo followed by a different branch uses restored counts.

### Tactics

Primary surfaces: `LocationHandlerForkTests.cs`,
`LocationHandlerForkBoardTests.cs`.

- Basic Elephant is Threaten Minor only.
- Amazon is not Threaten Queen.
- Mounted King and ordinary King are Threaten King and royal-fork targets.
- Promotion changes tactical current type while `PieceTransition` transfers the
  stable unit binding to any replacement `Piece`; Undo restores the prior
  binding and capture role.
- Tactical scans run only after a successful committed move.

### Cross-owner integration

Primary surface: `APMW.Test\ApmwGameCharacterizationTests.cs` plus the royal
lifecycle fixtures.

- Additional/primary King capture and Undo restore board, royal phase,
  castling actors, starting identities, counters, and result together.
- Failed speculative and failed committed cases leave no pending report.
- Repeated accepted internal fixture/load reconstruction produces royal
  membership 2, 1, 2 without stale piece bindings; it does not establish save
  or resume support.
- Controller takeover does not alter ledger behavior, but the library sink
  refuses new admission permanently.
- Reconnect replay comes only from the library earned set, never recomputed
  from current counters.

Suggested commands after implementation:

```powershell
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~GameMoveNotificationTests|FullyQualifiedName~ApmwMatchProgressTests|FullyQualifiedName~LocationHandler"
dotnet test .\APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwGameCharacterizationTests|FullyQualifiedName~ApmwStartingIdentityIntegrationTests"
dotnet test .\APMW.Test\APMW.Test.csproj --no-restore --verbosity minimal
dotnet test .\ChessV.Test\ChessV.Test.csproj --no-restore --verbosity minimal
```

The targeted filters are development feedback. Both full project suites are
required because `Game.cs` and setup/catalog changes are shared.

## ADR 0022: settled safe work versus blocked allocator work

### Safe and settled now

- Keep Legacy and Fundamental as distinct itemization modes.
- Characterize the current one-Pawn-per-item Legacy path without calling it
  compliant.
- Preserve material provenance fields already present in
  `OwnedMaterialLedger`.
- Preserve the selected Legacy funding facts: Pawn credit, eligible positive
  Legacy surplus, zero floor for negative surplus, historical 45-point and
  positive-remainder approximation behavior, and superseded-placement grant
  provenance.
- Count actual prepared pocket pieces once each when a future rule consumes
  that input.
- Use the prepared board as the context and the selected total-unit targets
  11/15/19/27/33 where ADR 0022 explicitly settled them.
- Add diagnostics/test scaffolding that records requested versus effective
  values without selecting fallback behavior.

### Blocked; do not implement

- **Pawn reservation equation:** how deployable and pocket contributions protect
  units before conversion is still unspecified.
- **Composition target pairs:** Q59 selects total/non-Pawn predicates, but Q60's
  numeric non-Pawn pairs and additional-King treatment remain proposals.
- **Fallback:** relaxable preferences, order, viability proof, rejection versus
  fallback, stopping rule, and whether disabled actions may be enabled remain
  open.
- **Wire revision:** no final setting keys/schema, semantic algorithm ID,
  projector package revision, or compatibility pair has been selected.
- **Allocator:** no portable ordering of reservation, funding transfers,
  conversion actions, approximation, type limits, placement, and terminal
  emission has been accepted.

Therefore H1-H4 are safe settled handler work. Any behavioral edit to
`OwnedRosterGeneration.GenerateLegacy`,
`ProjectionV2SemanticEngine.GenerateLegacy`, pawn generation, or the v4
projection wire is blocked on the parent decision owner.

## Main risks

1. **Phantom history from failed commits.** Keeping `SetupMove` as a push point
   will make reliable Undo impossible even if role names are corrected.
2. **Two lifecycle authorities.** Extending orphan `APMW.Core` while active code
   uses `ChessV.Base.ApmwCore` would create divergent event buses.
3. **Identity inferred after the fact.** File/rank/type inference cannot
   distinguish Rearguards, forward roles, promoted pawns, or substituted army
   families. The exact setup artifact must author identity.
4. **Recursive multi-capture dispatch.** Current recursion can emit partial
   progress and does not represent one reversible move. Complete board deltas
   are a hard prerequisite.
5. **Direct helper calls.** The current fire-and-forget tasks bypass journal,
   match eligibility, world binding, and atomic admission. Handler integration
   must wait for the library sink.
6. **Shared-file collisions.** `Game.cs`, `ApmwChess.cs`, and project files are
   shared with search/board/integration work and require explicit sequencing.
7. **ADR 0022 status ambiguity.** Its accepted status settles capabilities and
   several inputs, not the allocator. Implementing a plausible rule would
   silently choose unresolved policy.

## Handoff boundary

The handler child can exclusively own H3 and H4 after the parent freezes the
three interfaces above. H1 belongs to a shared `Game.cs` integration owner; H2
belongs to board/setup. The library receives immutable earned events and owns
everything after that seam. Search/rules own the correctness of the committed
board delta and reversible engine state. External generator/package integration
owns contract/wire versions and packaged parity.

No production behavior was changed by this probe.
