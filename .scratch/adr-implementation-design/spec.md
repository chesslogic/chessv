# ADR implementation development specification

Status: Design for later execution. No production implementation authorized by this document.
Baseline and source evidence: [assessment.md](assessment.md)
Assignment inventory: [fanout.json](fanout.json)

## 1. Scope and authority

This specification maps finalized ADR requirements to bounded implementation work.
It selects engineering interfaces and file ownership, not new gameplay policy.
The accepted ADRs and their linked contracts remain authoritative.
The specialist reports explain current behavior and cite existing symbols.

The deliverable covers library, handler, search, board, and integration work.
Implementation must support exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`.
It preserves four CPU families and both human itemization modes.
It rejects old world contracts even when their requested geometry remains supported.

ADR 0022 is only partly specified.
The dependency graph exposes its unresolved path.
No worker can substitute an invented reservation rule or silently omit the
accepted conversion capability from a claimed complete release.

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
| ADR 0022 lacks a complete allocator and portable contract | Open question | ADR 0022, ticket 24 | A finalized algorithm inferred from accepted status |

Temporary engineering assumptions can change when a concrete compile or fixture
failure disproves them. The integration owner records the replacement and updates
all consumers before parallel work resumes.
Product decisions require their original decision owner.

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
| Generator costs | G2 | Freeze after finalized obtainable counts, before final access evaluation |

No transport record retains a game window or board after closure.
No search rule reads the current AP connection.
No semantic goal identity comes from a display name, current square, or current type.

## 3. Interface gate S0

S0 is a small implementation tranche before consumer fanout.
It materializes declarations, complete fixture inputs, and compile-level consumer
examples. It does not implement gameplay or networking.

### 3.1. Setup identity

The proposed public read-only contract lives in
`ChessV.Games\MiscellaneousGames\ApmwStartingArmyIdentity.cs`.
`APMW.Client` already references `ChessV.Games`.
No new project dependency is necessary.

The interface exposes stage/family identity, fixed CPU player, starting counts,
unit records, primary unit ID, and initial Piece-to-unit associations.
The unit record contains `unit_id`, `role_id`, and `starting_class`.
The backing collections must be immutable, not writable lists behind read-only types.

The initial map is not the current position map.
E1 observations identify replacement/movement transitions.
H1 transfers the stable unit identity to the new piece and reverses that transfer
on Undo. Missing CPU identity is an explicit error, never a square/type fallback.

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

En passant, castling, promotion replacement, and Checkers multi-capture require
complete deltas. `MoveInfo` alone is not sufficient.
`Game` publishes facts. It does not classify Locations.
This design does not add user-facing save/resume support or promise arbitrary
FEN rollback beyond the accepted internal lifecycle fixtures.

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

The progress module exposes `Apply`, `Undo`, and identity-aware `Reset`.
`Apply` produces an immutable earned batch without I/O.
Unpublished thresholds do not reach the sink.
Unexpected commit pairing and unknown identities produce explicit failures.

### 3.4. World validation and reporting snapshot

C1 parses the exact A/L/C/S/W schemas from the accepted contracts.
It returns immutable typed data or an artifact-specific error.
Duplicate keys fail before lossy JSON conversion.
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

**Owner:** Integration architect. **Prerequisites:** None after execution approval.

Own NEW `CommittedMoveObservation.cs`, `ApmwStartingArmyIdentity.cs`,
`ApmwTacticalClassCatalog.cs`, `EarnedMatchEvents.cs`, and
`ApmwReportingModels.cs` in the assemblies specified in the manifest.
Define only the minimal interfaces in section 3.
Materialize the P8/P12 schema fixtures from shared-data section 10.2.
Attach a compile example for every cross-assembly consumer.

**Exit:** All consumers can target one seam packet without guessing fields or
visibility. No behavior-changing decision disappears into a DTO.

### B1 - Army data, geometry, and board composition

**Owner:** Board/data specialist. **Prerequisites:** S0.

Own `ApmwProfiles.cs`, `ApmwPieceCatalog.cs`, NEW Army reader/data and pure setup
composer, and the focused geometry/setup tests.
Author all 20 exact formations and role mappings from the accepted arrays.
Keep AP numeric IDs out of Army data.
Resolve selector fallback to Standard, but reject malformed artifact families.

Implement 2/1/5 and 3/1/6 CPU/neutral/human bands.
Use rank-only reflection.
Register the basic Elephant as 250/250 and Minor, distinct from War Elephant.
Expose CPU start/promotion types independently of human pools.
Provide explicit castling metadata with original actor IDs.

**Exit:** F01/F02/F03 pass at the pure data/composer seam.
U1 remains the sole `ApmwChess.cs` integration owner.
Army publication tooling must run without a restored projector.

### E1 - Authoritative move and controller lifecycle

**Owner:** Engine transaction specialist. **Prerequisites:** S0.

Own `ChessV.Base\Game.cs`, `ApmwCore.cs`, `Match.cs`, and notification tests.
The `Match.cs` change is conditional on the needed pre-controller-change hook.
Do not move network or goal policy into these files.

Publish complete commit/abort/undo facts.
Do not expose the old `NewMoveSetup` callback as successful progress.
Preserve search make/unmake and result restoration.
Expose one ordered controller-change notification before the new controller
can produce progress. N1 performs suppression through that notification.

**Exit:** F04/F05 prove no phantom history, no speculative reports, exact Undo
pairing, and takeover ordering. Search algorithms remain unchanged.

### R1 - CPU survival, castling, and promotion rules

**Owner:** Search/rules specialist. **Prerequisites:** S0.

Own APMW rule adapters and tightly coupled changes to `CastlingRule.cs`,
`CheckmateRule.cs`, and `BasicPromotionRule.cs` only where needed.
Do not edit `Game.cs`, `Search.cs`, or `ApmwChess.cs`.
A missing core seam returns to E1 instead of producing a concurrent patch.

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

### H1 - Reversible match progress and handler migration

**Owner:** Handler specialist. **Prerequisites:** S0, B1, E1, C1.

Own `LocationHandler.cs`, `CaptureLookup.cs`, NEW `ApmwMatchProgress.cs`,
and capture/fork/progress tests.
Replace file/rank inference with stable starting identity.
Apply a complete capture batch once, then report every crossed published threshold.
Use current type only for tactical classification.

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

### G2 - Calibration, immutable world costs, and complete access paths

**Owner:** Generator/rules specialist. **Prerequisites:** G1, P1.

Own `CM:rules.py`, `__init__.py`, NEW calibration/cost/world-binding modules,
calibration source artifacts, and targeted rule/lifecycle tests.
Use rational arithmetic through corrections, difficulty, and adjustment.
Apply one ceiling, then cap.
Use the accepted reference bank and exact curve definition in ticket 16.

Compile one complete access alternative per eligible stage.
Keep special predicates inside each alternative.
Freeze the snapshot after obtainable counts settle.
`fill_slot_data` serializes the same frozen object.
Tracker reconstruction consumes original costs and cannot reprice them.
Update terminal item-pool budget consumers coherently without redesigning pool policy.

**Exit:** F08/F10/F12 pass.
Synthetic fixtures prove arithmetic and lifecycle, not a finalized production C.
Do not tune provisional costs based on convenience.

### U1 - Game and UI composition

**Owner:** ChessV integration specialist. **Prerequisites:** B1, E1, R1, C1, H1, N1, P1.

Own `ApmwChess.cs`, APMW geometry game registrations, `GameForm.cs`,
`ApmwForm.cs` and related designer changes, necessary load-entry adapters,
and affected project files.
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

### I1 - Publication and package tooling

**Owner:** Release integration specialist. **Prerequisites:** C1, G1, P1.

Own Army publication tooling, projector build/release tools, C# lock parser,
PowerShell restore script, and package parser tests.
Implement metadata schema 2, all three data descriptors, and minimum-client agreement.
Preserve provenance, archive integrity, extraction safety, and exclusive restore checks.

**Exit:** F07/F13 synthetic tooling cases pass.
Tooling can be ready before policy closure.
No placeholder production lock or fabricated release identity is allowed.

### D22 - Complete the remaining allocation decision

**Owner:** Existing settings decision owner. **Prerequisites:** None.
**State:** Blocked on explicit decisions, not implementation.

Resolve reservation, composition targets/membership, viable fallback, transfer
accounting, algorithm order, and portable inputs/identifiers in ticket 24.
Preserve every accepted ADR 0022 claim.
The 15-total/7-non-Pawn example is illustrative, not a selected target table.
Off does not revoke Fundamental owned Chessmen entitlements.

**Exit:** A versioned contract amendment and full allocator fixtures exist.
No coding worker can mark this gate complete by choosing plausible defaults.

### A22 - Implement the approved allocator amendment

**Owner:** Cross-language allocation specialist. **Prerequisites:** D22, P1.
**State:** Blocked.

After the decision closes, own Legacy allocation code and its parity fixtures.
The manifest records serial transfer of P1's semantic consumer files.
Preserve grant provenance and prevent reserve double spending.
Apply prepared-board targets 11/15/19/27/33 and actual pocket membership as selected.
Retain the historical approximation rules, not an invented 100-point ceiling.

**Exit:** The amended algorithm, logic metrics, input schema, and C agree.
The fully specified fixtures, including 785/485/500/285 accounting, pass.
No exact algorithm or new identifier is assigned by this assessment.

### T1 - Attached-match integration evidence

**Owner:** Integration test specialist. **Prerequisites:** U1, G2, I1.

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
| 0 | S0 and the independent D22 decision process |
| 1 | B1, E1, R1 |
| 2 | C1 and G1 |
| 3 | H1, N1, P1 |
| 4 | G2, U1, I1 once each package's prerequisites pass |
| 5 | T1 and A22 after its decision gate |
| 6 | I2 after all semantic and release gates |

These waves are a display aid. The manifest's exact dependency edges govern.
A worker can begin earlier with accepted fixture adapters only when its
declared inputs are stable. That does not mark an unfinished prerequisite complete.

| Shared file or concern | Exclusive write owner |
| --- | --- |
| `Game.cs`, `Match.cs`, active `ApmwCore.cs` | E1 |
| CPU rule implementations and castling/promotion rule adapter | R1 |
| `ApmwProfiles.cs`, catalog, pure composer | B1 |
| `ApmwChess.cs`, game registration, shared project files | U1 |
| `LocationHandler.cs`, `CaptureLookup.cs` | H1 |
| `Client.cs`, connection owner | N1 |
| `Config.cs`, client v4 parser, geometry entry | C1 |
| C# and Python projection internals | P1, then named serial transfers to A22 |
| Python `locations.py`, progression, item maxima | G1 |
| Python `rules.py`, world lifecycle | G2 |
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
| F07 | Exact A/L/C/S/W bindings, duplicate-key rejection, old 8x8-world rejection, protocol 1 vs runtime/semantic pins, positive and negative package pairs | Shared-data sections 10.1/10.2/10.4 |
| F08 | `361045/162 * 27/20 + 240` gives ceiling 3249, cap 3000 gives 3000. Synthetic +100 intrinsic gives 3384 | Full `world-cost-snapshot-v1.md` required-fixture table |
| F09 | Six Rookies Lions retain separate roles. Promoted Pawn retains its role/class. Initial multi-King eligibility never changes after capture | Ticket 15 and ADRs 0006/0007 |
| F10 | Earlier-board Any 15 can complete on 8x8. Capture Everything remains endpoint-only. No Any 33/34 or Pieces 20 Location is invented | Ticket 15, shared-data section 5, royal two-King extinction fixture |
| F11 | Same generation/team/slot/game and W/C/S permit journal replay, including after window closure. Port is not identity. Mismatch retains intent | Full connection contract deterministic fixture sequence |
| F12 | One complete geometry path must supply all resources and special predicates. Bypass cannot remove tactic/castler conditions | Snapshot availability section and shared-data section 10.2 |
| F13 | Real x86 and x64 archives restore and execute under identical A/L/C. Minimum-client and byte/semantic integrity checks remain strict | Shared-data sections 9/10.5 |
| F14 | Legacy input grants 785, Major expected value 485, concrete Rook 500 leaves 285 before netting. No double-counted reserve credit | ADR 0022 Q51-Q55, plus future D22 complete allocator fixtures |

F14 alone does not specify an allocator.
The illustrative composition example does not become an additional fixture
target table without a decision.
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

Full 0.4.0 readiness remains blocked by D22 and broader settings closure.
The settled modules remain assignable through the declared dependency gates.
