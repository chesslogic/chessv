# CPU royal/search source probe

## Probe boundary and baseline

This is a source probe, not an implementation plan disguised as a patch. The
checkout was already at `78b604e43f684c8032d1af23955485d1e196954f`
(`78b604e`, `develop`), so no Git state was changed. The worktree had an
untracked `.scratch\adr-implementation-design\` directory before this file
was written. No production file or test file was changed.

The settled authority is ADR 0003 (`docs\adr\0003-cpu-royal-survival-phases.md`),
ADR 0017 (`docs\adr\0017-separate-cpu-promotion-permissions.md`), ticket 14,
the royal lifecycle fixtures, and ticket 18. The required attached-match,
reflection, origin-ledger, and non-reporting boundaries in
`contracts\royal-lifecycle-fixtures.md` are material: a null `Game.Match` is
not an equivalent fixture.

## Current source map

### Setup, type registration, profiles, and castling

* `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:11-116,181-286`
  creates fresh `PieceType` instances, classifies them into Kings/Pawns/
  Minors/Majors/Jacks/Queens/Amazons, and publishes the same collections into
  `ApmwChessGame` and `ApmwCore`. `Amazon` is currently in `Amazons`, not
  `Kings`; `MountedKing` is in `Kings`. This is the right existing seam for
  shared registration, but it has no CPU-side promotion/capability object.
* `ApmwProfiles.cs:49-217,280-425` already owns immutable geometry metadata,
  family resolution, exact CPU back-rank strings, `PromotionPieces`,
  `AttendantPromotions`, and geometry-derived castling plans. `ResolveCpuArmy`
  falls back for null/unknown names (`:112-117`). `CreateCastlingPlan`
  computes generic king ±2 and castler ±1 destinations with the CwDA
  color-bound fallback (`:147-205`). `SixByEight` has `SupportsCastling=false`
  (`:286-293`), while `Stages` still contains legacy `TwelveByTwelve`
  (`:323-340`), contrary to the accepted five-geometry boundary.
* `ApmwChess.cs:184-224` replaces the normal checkmate rule with
  `CovenantRule` plus `ApmwStalemateRule` when `foundKingPromotions > 0`, but
  uses a fixed string (`"K"` or `"KW"/"KZ"`) and fixed rule construction.
  `ApmwChess.cs:236-301` adds evaluation hooks and conditionally recognizes
  loaded piece notations. `MountedKing` gets the existing outpost treatment
  (`:292`); there is no new royal-specific evaluation coefficient.
* `ApmwChess.cs:322-375` registers `King` and optionally one promoted king,
  then filters other types through `loadableTypes` based on the human side.
  That filtering is a human registration policy and cannot by itself express
  CPU-only registered promotion targets.
* `ApmwChess.cs:378-480` composes the human formation, resolves the CPU
  family, and currently combines `basePromotions`, pocket promotions,
  `cpuArmyProfile.PromotionPieces` (only when `enemyArmy != null`), and
  `AttendantPromotions` into one `promotions` string. This is the central
  current-vs-required mismatch for ADR 0017: explicit Standard, absent, and
  unknown family input do not all necessarily take the same CPU list, and the
  shared string can leak CPU registration/promotion targets into human
  entitlements. The board child owns this setup/profile/FEN composition seam;
  the handler must not become its owner.
* `ApmwChess.cs:526-598` creates human back-rank castling and CPU castling
  from profile coordinates. `AddDefaultCastlingMove` (`:572-598`) registers
  CPU rights by geometry/family but does not bind a right to the original
  primary actor; `CastlingRule` later treats the registration as square-based.
  This is sufficient for coordinates, not for “only original Mounted King may
  castle; no inheritance”.
* `ChessV.Games\Rules\CastlingRule.cs:68-130,142-214,215-300` stores rights as
  bit masks, hashes them (`GetPositionHashCode`), restores them from FEN, erases
  rights when a registered source square moves, and generates special moves.
  Attack checks run only when any `CheckmateRule` exists (`:257-290`) and
  cover source through destination inclusive. The rule does not verify the
  occupant's original identity, and rights are not a board piece attribute.
  Any actor binding must therefore be a tightly coupled shared rule change or
  an APMW adapter around this registration/transaction seam; it cannot be
  solved by changing profile coordinates alone.

### Royal rules and terminal dispatch

* `ChessV.Games\Rules\CheckmateRule.cs:29-67` adds `RoyalAttribute` to configured
  types and creates `RoyalPieces` sets. `PositionLoaded` (`:70-84`) scans
  current pieces, but only adds and never clears the sets. `MoveBeingMade`
  (`:86-100`) rejects captured royals and applies compulsory check to every
  moving-side royal. `NoMovesResult` (`:102-118`) returns raw `0` for more than
  one surviving royal, then checks only the first survivor. `PositionalSearchExtension`
  has the same multi-royal early return (`:120-129`). These are exact defects
  against the accepted multiple/one/zero table.
* `ChessV.Games\Rules\Extinction\CovenantRule.cs:6-25` checks bitboards for
  extinction of all configured type numbers. It can classify zero Kings after
  one committed multi-capture, but it cannot express “CPU only, by surviving
  count, with multiple-royal no-move win” and is selected today from human
  King-upgrade state in `ApmwChess.AddRules`.
* `ChessV.Games\Rules\Apmw\ApmwStalemateRule.cs:8-61` snapshots starting
  royal objects on load; for attached matches it skips check legality for human
  moves, removes captured enemy royals only on CPU-side moves, and restores
  only those starting objects on unmake. Its no-move override converts
  stalemate to human loss/CPU win for CPU movers, but delegates first to the
  broken multi-royal `CheckmateRule.NoMovesResult`. `PositionLoaded` also
  accumulates stale objects on repeated loads because the base set is not
  cleared. The `Game.Match`/`HumanPlayer` branch (`:23-45`) is an actor-identity
  dependency and must remain unchanged for human-side policy.
* The accepted design must not install a second, competing CPU policy. The
  current APMW `CovenantRule + ApmwStalemateRule` path is the replacement
  target: one APMW CPU-survival coordinator becomes the sole authority for
  CPU royal membership, CPU check legality, CPU zero-King extinction, and CPU
  no-move classification. `CovenantRule` and `ApmwStalemateRule` must not also
  answer those CPU callbacks. The ordinary human-side check/stalemate policy
  remains preserved by delegation inside the coordinator (or by a retained
  base-rule adapter that is called only for the human side); it is not a
  second CPU authority. The smallest deep-module interface is a CPU royal
  lifecycle rule with: `PositionLoaded`, committed/speculative
  `MoveBeingMade`/`MoveBeingUnmade`, `TestForWinLossDraw`, `NoMovesResult`,
  and (only for the primary castling actor) an attack-aware castling
  predicate. Internally it should rebuild current royal membership from board
  state on load and transactionally record removed royal objects per move;
  callers should not manipulate `HashSet<Piece>`.
  Exact ordering is required: after board pickup/drop, before result dispatch;
  failed make must restore the rule snapshot; committed match notifications occur
  only after acceptance.

### Make/unmake, speculative search, hash, repetition, and result flow

* `ChessV.Base\Game.cs:1201-1216` generates all pieces for the side plus
  `GenerateSpecialMoves`; legal root generation is enabled at initialization
  (`Game.cs:650-724`, `moveLists[1].LegalMovesOnly=true`).
* `MoveList.cs:1053-1181` applies pickups/drops, calls
  `Game.MoveBeingMade`, and rolls back partial board mutation on exceptions.
  `MoveList.cs:1197-1248` calls `MoveBeingUnmade` before reversing drops/pickups.
  This is the existing search/speculation seam; no search rewrite is indicated.
  A rule that snapshots only its own lifecycle state and reverses it in these
  callbacks will cover legal generation, alpha-beta search, and failed
  generation.
* `Game.MoveBeingMade` (`Game.cs:1849-1875`) invokes all rules, evaluations,
  move completion, and `MoveMade` handlers even if one rule reports illegal;
  `MoveList.MakeMove` then unmake-checks the move. A late rejection fixture
  therefore must prove rule/event state is reverted, not merely that the board
  is restored.
* Committed `Game.MakeMove` (`Game.cs:1589-1707`) calls
  `ApmwCore.NewMoveSetup` before `moveLists[1].MakeMove`, enters
  `BoardMoveStack`/game history, tests terminal results, raises
  `MoveBeingPlayed` then `MovePlayed`, and only then emits
  `ApmwCore.NewMovePlayed`/`MatchFinished`. `NewMoveSetup` may observe a
  rejected committed attempt; no successful client report may be inferred from
  it.
* Speculative `Game.MakeMove` pushes only a `(gameHistoryCount, Result)`
  snapshot (`Game.cs:1689-1707,1710-1719`) and requires LIFO matching
  `UndoMove(..., Speculative)` (`:1736-1777`). It still executes rule/evaluation
  callbacks but defers APMW commit notification. This is adequate if the new
  rule's state is transactionally reversible; the snapshot does not include
  arbitrary rule-owned state.
* `Game.LoadFEN` (`Game.cs:~1790-1810`) clears history count, calls every
  rule's `PositionLoaded`, then regenerates moves. A royal rule must clear and
  reconstruct all current membership and phase state here. `Game.ClearGameState`
  also calls every rule's clear hook and clears speculative snapshots.
* `Game.GetPositionHashCode` (`Game.cs:2301-2310`) XORs `Board.HashCode` with
  rule contributions. `CastlingRule` contributes privilege state; no current
  royal phase/identity contribution exists. If royal membership is wholly
  derived from board piece type and square, board hash changes are enough for
  phase; if original actor identity or lifecycle ledger affects legal castling,
  that identity/rights state must contribute to the hash or be encoded in the
  castling rule hash.
* `RepetitionDrawRule.cs:25-88` records position hashes on
  `MoveBeingPlayed`, `MoveReverted`, and `MoveMade`. A royal phase/rights hash
  must be stable before these callbacks and restored on speculative/committed
  reversal; otherwise repetition and TT can conflate two legal-state variants.
* `ChessV.Base\Search.cs:381-451,488-560,1300-1323` performs root and PV
  make/search/unmake, checks terminal result before evaluation, uses
  `GetPositionHashCode` for TT lookup/store, and unmake-restores every explored
  move. Existing extension dispatch (`getExtension`, `:~1300`) can retain
  one-King check extension and return zero for multiple Kings; no core search
  rewrite is justified.

### Client/event and capture bookkeeping

* `APMW.Client\LocationHandler.cs:116-198` owns match attachment, counters,
  origin-square ledger, and committed undo deltas. `MoveTakenBackHandler` only
  reverses client capture/origin state; it must not be made responsible for
  engine royal membership or castling rights.
* `LocationHandler.cs:350-378,476-590` ignores CPU moves for Location emission,
  handles human capture bookkeeping, and treats Checkers multi-capture as one
  special path. The royal fixture's two-King Checkers batch must arrive as one
  committed result while preserving counters and Regicide exactly once.
* `LocationHandler.cs:761-789` reports human victory through the existing
  stage-victory boundary; `CaptureLookup.cs:61-94` supplies configured capture
  counts. The new extinction/checkmate classification must enter this existing
  result path without inventing an Any threshold or Capture Everything when a
  non-King remains.
* CPU side must be captured as immutable setup identity (`cpuPlayer`) when the
  attached APMW match is constructed, alongside the primary King/castling
  actor identity. It must not be recomputed from `HumanPlayer`, current
  `Match` controller types, or `Match.GetPlayer(...).IsHuman` after takeover:
  ADR 0020 permits local play to continue after controller changes. A
  controller takeover can disable reporting, but cannot change which side is
  CPU for royal legality or terminal classification. The handler owns
  event/capture bookkeeping; the game rule owns legality, royal membership,
  terminal classification, and transaction state. Any shared `ApmwCore`,
  `Game`, or rule edits are cross-surface changes and need one later
  integrator, not parallel ownership.

## Current versus required behavior

| Surface | Current characterization | Required characterization | Change type |
|---|---|---|---|
| CPU royal count | Fixed configured type set; stale on load; capture handling differs by attached actor | Current board membership exactly 2/1/0 for both colors; reload is replacement, not addition | Necessary rule change |
| Multiple CPU royals | `NoMovesResult` returns raw `0`; ordinary move check is bypassed only through APMW attached-match path | Legal moves may leave attacked non-primary royal; no legal moves = CPU win | Necessary rule change |
| Last CPU royal | APMW delegates to first surviving royal and can reject/accept based on stale set | Checkmate legality and terminal attack apply to whichever CPU royal survives | Necessary rule change |
| Zero CPU royals | Covenant can detect configured extinction after commit | Extinction takes precedence over no-move/attack, including one Checkers batch | Necessary dispatch change |
| Castling actor | Rights are square/privilege based; replacement King can use registered right | Only original primary may castle; captured/moved primary ends rights; additional King never inherits | Necessary shared rule or adapter change |
| Castling attacks | Existing `CastlingRule` checks source-through-destination when CheckmateRule exists | Same restriction in both CPU phases; human behavior unchanged | Existing seam sufficient after actor/phase gate |
| CPU promotion | Shared `promotions` string mixes base/pocket/army; null family differs from explicit Standard | Explicit CPU list per family/geometry, independent of human pockets/upgrade state | Necessary board/profile + promotion-rule wiring |
| Search/TT/repetition | Make/unmake and hash seams are already centralized | Reuse them; include any non-derived royal/castling state in rule hash | Investigation/targeted rule change |
| Evaluation | Existing rook/outpost/colorbound hooks, no new coefficients | Preserve coefficients; Mounted King recognition only where existing hooks apply; Amazon non-royal | Unchanged characterization, tests |
| APMW reports | Committed handler owns counters and Locations; setup can precede rejection | Failed committed/speculative attempts produce no successful report; undo keeps accepted checks | Necessary fixture/integration verification, handler edits only if evidence requires |

## Proposed transaction lifecycle and interface

Use one deep CPU-royal rule module at the existing `Rule` seam, replacing the
current APMW `CovenantRule + ApmwStalemateRule` CPU path rather than installing
alongside it. The coordinator is the sole CPU authority. It delegates the
preserved human-side check/stalemate behavior to the existing base-rule
implementation or adapter only when the moving/result side is human; no
retained human delegate may classify CPU moves or CPU terminal states.
Its caller-facing interface should be the inherited event surface, with one
small immutable internal state:

* `PositionLoaded`: clear all sets/ledgers, read immutable setup-side
  `cpuPlayer` and primary actor metadata, rebuild current CPU royal membership,
  and derive phase `Multiple`, `Last`, or `Extinct`. Do not infer CPU identity
  from the current controller after ADR 0020 takeover.
* `MoveBeingMade`: after board application, snapshot the prior phase, royal
  membership, actor rights, and any pending capture batch; update membership
  from captured objects and classify zero/one/multiple. For attached human
  moves, preserve human rules; for CPU last-royal moves, enforce attack safety.
  Return `IllegalMove` only for the one-King CPU phase or an invalid primary
  castle.
* `MoveBeingUnmade`: restore the exact per-move snapshot, including a move that
  removed two royals. This callback must be safe for search legality probes and
  failed late rejection.
* `TestForWinLossDraw`: first return CPU loss when CPU royal count is zero;
  otherwise leave ordinary move-result dispatch to `NoMovesResult`.
* `NoMovesResult`: multiple CPU royals => CPU win; one CPU royal attacked =>
  CPU loss; one un-attacked => preserve APMW stalemate CPU win. Human mover
  policy remains the current human-side behavior.
* `PositionalSearchExtension`: one surviving CPU royal uses the existing
  check extension; multiple/zero returns zero.
* Hash: use existing `GetPositionHashCode`; add only actor/rights state not
  represented by board or castling privileges. Do not hash transient reports.

The engine's `MoveList`/`Game` lifecycle already supplies the required
speculative transaction seam. Do not rewrite alpha-beta search, TT lookup,
move ordering, or Board hashing. A test-only late rejection Rule should be
inserted after setup participation to demonstrate rollback of rule/client
state as well as board/hash.

## Bounded work packages and ownership

Ownership below is intentionally non-overlapping. Shared APMW/Game edits need
one integrator after the bounded packages complete.

1. **Board child (ChessV.Games):** `ApmwProfiles.cs`, `ApmwChess.cs`,
   geometry child classes, `ApmwPieceCatalog.cs`. Own exact five-geometry
   setup/profile/FEN composition, CPU family metadata, type registration,
   castling metadata, and CPU promotion-list construction. Do not edit
   `LocationHandler`.
2. **Rules/engine integrator (shared ChessV.Base + ChessV.Games.Rules):**
   `CheckmateRule.cs`, `ApmwStalemateRule.cs`, `CovenantRule.cs`,
   `CastlingRule.cs`, and only if needed `Game.cs`/`Rule.cs`. Own the deep
   royal lifecycle rule, actor-bound castling, result precedence, and
   transaction/hash correctness. This is one shared package because rule
   callbacks, Game dispatch, and castling rights are tightly coupled.
3. **Handler child (APMW.Client):** `LocationHandler.cs`, related capture
   ledger tests only. Own committed event admission, multi-capture counter
   boundaries, undo deltas, victory/reporting and non-reporting state. Do not
   classify royal legality.
4. **Test/fixture integrator:** `ChessV.Test\ApmwGeometryProfileTests.cs`,
   `APMW.Test\ApmwGameCharacterizationTests.cs`, and new test files marked
   `NEW`. Cross package tests must cover both colors, human King upgrades
   0/1/2, and human pocket/promotion contexts.

## Concrete fixture inputs, outputs, and test surfaces

The authoritative inputs/outputs are in
`.scratch\large-board-enemy-armies\contracts\royal-lifecycle-fixtures.md`:

* Two-King base: Standard 12x10, CPU White Mounted King G1, ordinary King G2,
  Rook A1, human Black King L10/Rook G8, candidate A1-A2. Expected legal while
  G2 is attacked.
* Last-primary and last-additional variants: same board with one prior King
  capture. Expected A1-A2 illegal; additional King never inherits castling.
* Additional-King capture G8-G2 with CPU `Q` right: expected one-King phase,
  check on G1, Regicide once, one non-pawn capture; undo restores exact actors,
  rights, phase, counters, and hash.
* Checkers D7-F5-H3 capturing CPU Kings E6/G4 in one committed batch:
  expected zero phase, CPU defeat by extinction, Pawn 14, Piece 19, Any 33,
  Regicide once, no invented Any 33 Location, Capture Everything incomplete;
  one undo restores both royals and prior counters.
* Last-King A1 fixtures with human King C3 and Queen B2/C2: checkmate versus
  preserved stalemate CPU win; Mounted King adds B3/C2 escape coverage.
* Castling clear/source/transit/destination attack fixtures for all five
  geometries, reflected colors, both CPU phases, moved-away-and-back primary,
  missing/substituted actor restoration, and 6x8 disabled capability.
* Rollback matrix: successful committed undo, successful speculative undo,
  failed speculative late rejection, failed committed late rejection,
  repeated load `2 -> 1 -> 2`, and terminal load followed by fresh two-King
  position. Equality includes board, hash, side, counters, identities, rights,
  result, capture lineage, and pending reporting state.
* Registration matrix: Standard/Colourbound/Rookies/Nutty x five geometries x
  both colors x human King upgrades 0/1/2 x human promotion/pocket contexts.
  CPU promotion lists must remain fixed; basic Elephant is Minor tactical only,
  no Amazon royal, no basic Elephant/legacy12 Nightrider CPU promotion.

Existing source test surfaces already provide useful characterization:

* `ChessV.Test\ApmwGeometryProfileTests.cs:14-84,360-700` asserts profile
  metadata, setup rows, family back-ranks, promotion fragments, and castling
  registrations (via reflection).
* `APMW.Test\ApmwGameCharacterizationTests.cs:14-150` asserts both colors,
  current family rows, promotion strings, and castling endpoints.
* Add royal/search lifecycle tests under `APMW.Test` or `ChessV.Test` as
  `NEW`; use attached `Match` and explicit players, not bare FEN-only setup.
  A test-only rejection Rule is also `NEW` if no existing fixture helper can
  inject it.

Smallest validation commands after implementation (from repository root):

```powershell
dotnet test ChessV.Test\ChessV.Test.csproj --filter "FullyQualifiedName~ApmwGeometryProfileTests"
dotnet test APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~ApmwGameCharacterizationTests"
dotnet test APMW.Test\APMW.Test.csproj --filter "FullyQualifiedName~Royal|FullyQualifiedName~Rollback|FullyQualifiedName~Promotion"
```

The first two commands characterize existing setup/profile behavior; the
third is the smallest intended selector for the new lifecycle evidence.
Package/architecture interoperability and play-test calibration remain
outside this source probe and ticket 18's downstream completion boundary.

## Blockers and unresolved integration questions

* The accepted five-geometry scope conflicts with current
  `ApmwProfiles.Stages` including 12x12 and `ApmwGeometrySelection` exposing
  it. Removing or rejecting 12x12 is a shared profile/selector decision for
  the later integrator, not safe to infer in this probe.
* CPU primary actor identity is not represented in current `CastlingRule`
  privileges; exact restoration semantics need a chosen representation
  (piece-origin metadata, rule-owned actor token, or an APMW-specific castling
  adapter) before implementation can be split further.
* `Game.MoveBeingMade` deliberately calls rule/evaluation completion hooks even
  when a rule later rejects the move. The integrator must define whether the
  new rule snapshots before all callbacks or whether a general rollback hook
  is required; the fixtures require no stale phase/report state either way.
* The current APMW promotion path mixes human and CPU lists in a single string
  and `AddBasicPromotionRule` has one list per pawn type. A CPU-side conditional
  promotion adapter is required unless the existing promotion rule can safely
  select by `Piece.Player`; this choice affects shared rule wiring and must not
  be guessed by the board child.
* `ApmwCore` is process-global and `ApmwPieceCatalog.Publish` shares mutable
  collection references. Parallel test fixtures must retain the existing
  `DoNotParallelize`/cleanup discipline; changing that global contract is out
  of scope.
* The source confirms no need for a core search rewrite. It does not prove
  package restoration, x86/x64 interoperability, user-facing FEN resume, or
  play-test calibration; ticket 18 explicitly leaves those downstream.
