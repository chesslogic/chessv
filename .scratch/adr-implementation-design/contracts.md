# Shared contract inventory

Status: Identification complete. Corrections applied to the specification and manifest. Declarations are not yet written.
Date: 2026-09-27
Baselines: ChessV `78b604e43f684c8032d1af23955485d1e196954f`, ChecksMate `0772bac724ff483043b56979f0ef078b9c2d264a`

This document lists every contract that crosses a work-package boundary in the
[development specification](spec.md). A contract is shared when one package
writes it and a different package, assembly, language, or repository reads it.

The inventory has three tiers:

| Tier | Meaning | Work before fanout |
| --- | --- | --- |
| Wire | JSON artifacts, slot data, and package metadata exchanged by ChessV and ChecksMate | Implement strict readers and writers. The field sets are already normative |
| Seam | C# types that cross a package or assembly boundary inside ChessV | S0 declares them. They do not exist yet |
| Primitive | One algorithm that several producers and readers must run identically | Assign one implementation per language, with shared fixtures |

Package IDs refer to [spec.md section 4](spec.md#4-work-packages).
Line references describe the baselines above.

## 1. Assembly constraints

```text
ChessV.Base  <-  ChessV.Games  <-  APMW.Client  <-  ChessV.GUI
                                        ^
                                   APMW.Test (InternalsVisibleTo)
```

- `ChessV.Games` references only `ChessV.Base` (`ChessV.Games.csproj:35`).
- `APMW.Client` references Base and Games (`APMW.Client.csproj:33-34`).
- `ChessV.GUI` references all three (`ChessV.GUI.csproj:91-95`).
- `APMW.Client` grants internals only to `APMW.Test`
  (`APMW.Client\Properties\AssemblyInfo.cs:11`).

These facts set visibility:

| Seam location | Consumer | Required visibility |
| --- | --- | --- |
| Base or Games | APMW.Client | `public`, read-only |
| APMW.Client | Another APMW.Client package or APMW.Test | `internal` is sufficient |
| APMW.Client | ChessV.GUI | `public`, read-only |

A Games-side seam cannot depend on an APMW.Client type.
Data that Games needs must be in Games or Base.

## 2. Wire contracts

These contracts are normative in
[shared-data-contract-v4.md](../large-board-enemy-armies/contracts/shared-data-contract-v4.md)
and [world-cost-snapshot-v1.md](../large-board-enemy-armies/contracts/world-cost-snapshot-v1.md).
This inventory adds ownership and consumers. It does not change fields.

| ID | Contract | Schema and version | Writer | Readers | Authority |
| --- | --- | --- | --- | --- | --- |
| W1 | Army `A` | `chessv_armies` 1.0, layout `apmw-cpu-layout-v2` | B1 (data), I1 (publication) | G1, C1, U1 through B1 types, P1 | Shared data §4 |
| W2 | Army release descriptor | `chessv_armies_release` integer 1 | I1 | G1 vendoring | Shared data §9.1 |
| W3 | Location profile `L` | `apmw_location_profile` 2.0 | G1 | C1 (`ApmwLocationProfileV2`), H1 through C1, G2 | Shared data §5 |
| W4 | Shared contract `C` | `apmw_contract` 4.0, exact minor | P1 and G1 assemble in ChecksMate | C1, P1 (C# projection), I1 | Shared data §6 |
| W5 | Calibration manifest `K` | `apmw_calibration_manifest` 1.0 | G2 | G2 only. Reaches ChessV only as `S` provenance | Shared data §7.1 |
| W6 | Cost snapshot `S` | `apmw_location_costs` 1.0 | G2 | C1, N1 binding checks | Snapshot spec, all sections |
| W7 | World binding `W` and slot fields | `apmw_world_binding` 1.0 | G2 (`fill_slot_data`) | C1, N1 | Shared data §7.3 |
| W8 | Projector build, release, and lock metadata | Existing names, integer 2 | I1 (both repositories) | I1 C# lock parser and restore script, I2 | Shared data §9.2 |
| W9 | Projection envelope | Transport protocol 1, meaning under `C` | P1 (both languages) | P1 | Shared data §2, §9.4 |

Invariants that affect several packages:

- Protocol 1 is transport. Contract 4.0 is semantics. Runtime 0.2.0 is the executable.
  No package can substitute one for another.
- `A` and `L` never reference `C`. `S` never contains `W`. The hash graph has no cycle.
- Slot fields `apmw_contract`, `apmw_world_binding`, and `apmw_location_costs`
  are JSON objects, not encoded strings.
  Compatibility mirrors are assertions only. A conflict fails.
- Both Windows architectures package the same `A`, `L`, and `C` bytes.
- Real hashes and release pins are I2 outputs. No package writes a placeholder pin.

## 3. Shared primitives

| ID | Primitive | Current location | Required location | Readers |
| --- | --- | --- | --- | --- |
| X1 | Strict JSON parse and canonical semantic hash (§8) | `APMW.Client\ApmwContractV2.cs:638-700`, C#; ChecksMate contract module, Python | C#: S0 creates public `ChessV.Games\MiscellaneousGames\ApmwCanonicalJson.cs`. Python: one implementation in the canonical contract module | B1, C1, I1 (C#); G1, P1, G2 (Python) |
| X2 | Rank-only reflection | Reconstructed in several setup paths | One B1 function in the setup composer | U1, R1 castling squares, H1 fixtures |
| X3 | Exact rational arithmetic and single upward rounding | ChecksMate only | G2 | G2 writer. C1 reads decimal strings and never recomputes |

X1 is a gap. The Army reader in `ChessV.Games` needs strict duplicate-key
parsing and hash validation, but it cannot reach `APMW.Client`.
A second C# implementation would allow `A` to hash differently in two places.
The deterministic canonicalization fixtures in shared data §10.4 must run
against both the C# and Python implementations.

## 4. In-process seams

### Summary

| ID | Seam | Declared by | Writer | Readers | File (proposed) | Visibility |
| --- | --- | --- | --- | --- | --- | --- |
| K1 | Army typed model and type registry | S0 (records) | B1 (`ApmwArmyReader.cs`) | U1, R1, K2, C1 | `ChessV.Games\MiscellaneousGames\ApmwArmyData.cs` | public |
| K2 | Starting army identity | S0 | U1 (from K1) | H1, R1, T1 | `ChessV.Games\MiscellaneousGames\ApmwStartingArmyIdentity.cs` | public |
| K3 | CPU promotion and castling metadata | S0 | B1 | R1 | Part of K1 | public |
| K4 | Committed lifecycle observations | S0 | E1 | H1, T1 | `ChessV.Base\CommittedMoveObservation.cs` | public |
| K5 | Controller replacement notification | S0 | E1 | N1 (through U1 wiring), U1 | `ChessV.Base\Match.cs` event, payload in K4 file | public |
| K6 | Validated world context | S0 | C1 | N1, H1, U1 | `APMW.Client\ApmwWorldContext.cs` | public |
| K7 | Target-class lookup | S0 | C1 (from `L.target_classes`) | H1 | Part of K6 | internal |
| K8 | Earned events sink | S0 | H1 | N1 implements | `APMW.Client\EarnedMatchEvents.cs` | internal |
| K9 | Match binding and lifecycle | S0 | U1 calls | N1 owns | `APMW.Client\ApmwReportingModels.cs` | public |
| K10 | Connection and reporting snapshots | S0 | N1 | U1 (GUI) | `APMW.Client\ApmwReportingModels.cs` | public |

The following are package-internal and are not shared contracts:
CPU royal membership and phase (R1), the reversible capture ledger (H1),
route generations and retirement evidence (N1), and the earned journal (N1).
A second package that needs them must request a new seam.

### K1. Army typed model and type registry

Immutable C# records for W1: pieces, roles, royal policy, and 20 formations.
Each record keeps its Army `type_id`, `role_id`, and `unit_id` strings.

The registry maps each Army `type_id` to exactly one ChessV `PieceType`, and back.
It rejects an unknown `movement_id`. It does not match by notation, image, or display name.
This mapping is the only join between engine pieces and profile `type_id` values.

The model has no AP numeric IDs.
The fallback to Standard applies to selector input only, never to a malformed artifact.

### K2. Starting army identity

Per match, immutable after setup:

| Member | Content |
| --- | --- |
| `StageId`, `FamilyId` | Resolved formation |
| `CpuPlayer` | Fixed at setup. Never derived from the current controller or `IsHuman` |
| `Units` | `unit_id`, `role_id`, `starting_class`, starting `type_id` |
| `Counts` | `pawns`, `non_king_non_pawns`, `kings`, `total`, equal to the records |
| `PrimaryUnitId`, `KingUnitIds` | From `royal_units` |
| `ResolveInitialPiece(Piece)` | Initial binding. An unbound CPU piece is an explicit error |

The initial `Piece` binding is not the current binding.
H1 owns the reversible current `Piece`-to-`unit_id` association.
K4 transitions move that association to a replacement piece. Undo restores it.
An immutable `Piece` dictionary alone loses identity after replacement promotion.

R1 reads `CpuPlayer`, `PrimaryUnitId`, and `KingUnitIds` to seed its royal
state and castling actors. R1 does not read H1 state.

### K3. CPU promotion and castling metadata

Per formation, from Army §4.3:

- `promotion`: behavior ID, owner-relative promotion rank, ordered target `type_id` list.
- `castling`: ordered routes with actor unit IDs, owner-relative squares,
  empty and safety squares, and rights characters.

R1 receives this through K1 and resolves squares with X2.
Human promotion lists, pockets, and upgrades are separate inputs.
No R1 rule reconstructs a route or a promotion list from human state.

### K4. Committed lifecycle observations

`Game` publishes facts. It does not classify Locations.
Each notification belongs to one game instance and one reset epoch.

| Notification | Payload | Guarantee |
| --- | --- | --- |
| Commit | Game and epoch identity, monotonic `CommitId`, mover, primary move, all captures, all piece transitions | Exactly once, after the move is accepted |
| Abort | Pending attempt identity | A failed committed attempt. No commit ID is consumed by a ledger |
| Undo | The exact `CommitId` being removed | Strict top-of-stack pairing |
| Reset | New epoch and the accepted internal restoration record | Only for the accepted internal fixture/load scope |
| Terminal result | Commit ID or result source, result kind, winning side | Exactly once for every committed result |

Speculative make/unmake produces no notification.
Captures and transitions carry value snapshots: square, player, `PieceType`,
and the `Piece` reference as an identity token only.
En passant, castling, promotion replacement, and Checkers multi-capture use one
complete Commit. `MoveInfo` alone is insufficient:
it holds one `PieceCaptured` (`ChessV.Base\MoveInfo.cs:39-40`).

Current behavior that this seam replaces:

- `NewMoveSetup` runs before the move can fail (`ChessV.Base\Game.cs:1599-1603`).
- Duplicate suppression compares `move != lastMove` (`Game.cs:1596-1598`).
  `MoveInfo` equality ignores `Player` (`MoveInfo.cs:42-43`).
  Commit identity must replace value equality.
- `MatchFinished` fires only when no legal moves remain (`Game.cs:1669-1680`).
  A result from `TestForWinLossDraw`, such as CPU extinction, has no APMW notification.

The delivery channel should be scoped to the `Game` instance.
The present `ApmwCore` global handler lists (`ChessV.Base\ApmwCore.cs:65-77`)
make parallel tests share state.

### K5. Controller replacement notification

`Match.SetPlayerToHuman` and `SetPlayerToInternalEngine` replace controllers
with no event (`ChessV.Base\Match.cs:135-160`).
`HumanEnabled` is turn availability, not replacement evidence.

The seam is one synchronous notification raised before the replacement takes
effect. It carries the match, the side, and the old and new controller kinds.
N1 suppresses the match inside this notification, under its admission gate.
The new controller can produce progress only after the handler returns.
Computer Plays, Stop Thinking, and automatic human restoration all pass through it.

### K6. Validated world context

One immutable value for each authenticated slot:

- Parsed `A`, `L`, and `C` identities and semantic hashes.
- Parsed `S` and `W`, with original intrinsic provenance and effective integers.
- `published_location_ids` and `published_event_keys`.
- Progression stages, itemization, ordering, and tactics mode.
- AP generation name and authenticated team, slot, and game, attached by N1.

C1 produces the artifact part and never prices a Location.
N1 attaches authenticated identity and publishes the value.
The new type does not expose `ApmwContractV2` publicly.

The Army in the context is the Army the game setup uses.
C1 rejects a match when `C.army` differs from the installed Army hash.

### K7. Target-class lookup

`L.target_classes` is the authority for Pawn, Minor, Major, Queen, and King
tactical classes (shared data §5).
H1 maps the current `PieceType` through K1 to a `type_id`, then reads the class from K7.
Starting role and starting class stay separate from the current tactical class.

This replaces the global mutable sets in `ApmwCore.cs:49-61`,
which `ApmwChess.cs:143-150` and `ApmwPieceCatalog.cs:239-246` assign.
Human item generation reads those sets today. That use is outside this seam.

### K8. Earned events sink

```csharp
internal interface IApmwEarnedEventSink
{
    void Record(EarnedMatchEvents earned);
}
```

`EarnedMatchEvents` contains:

| Field | Content |
| --- | --- |
| `MatchId` | The originating match. Never the current connection |
| `Source` | Either a `CommitId` or an explicit terminal-result source |
| `LocationIds` | Immutable, all members of `published_location_ids` |
| `GoalEarned` | Separate flag |

`Record` means "this match earned these accomplishments."
It does not mean the server accepted them.
The connection owner implements this interface.
It exposes no second path such as `RecordCommittedLocations` or `RecordFinalGoal`.
Empty batches do not cross the seam.

### K9. Match binding and lifecycle

U1 starts a match with `BeginMatch(ApmwMatchBinding)` and ends it with `EndMatch(MatchId)`.
The binding holds the match ID, the K6 value, and K2 identity.
It holds no `Game`, board, or window reference.
Window closure calls `EndMatch`. Transport cleanup does not.

### K10. Connection and reporting snapshots

Two immutable values with one change event:

- Connection phase: `Disconnected`, `Connecting`, `Authenticating`,
  `Connected`, `Stopping`, `ValidationFailed`.
- Match reporting: `Eligible` or `NotReporting`.

The GUI composes them. `NOT REPORTING` takes precedence over `AP DISCONNECTED`.
Neither value means server acceptance.
The GUI does not recompute eligibility from socket state.

## 5. Corrections to the development specification

This pass found four differences from the earlier [spec.md](spec.md) and
[fanout.json](fanout.json). All four are now applied to both files.

| ID | Finding | Applied correction | Affected packages |
| --- | --- | --- | --- |
| CX1 | S0 created `ApmwTacticalClassCatalog.cs` in Games as a class authority. `L.target_classes` is already that authority | Catalog removed. K7 is declared in S0's `ApmwWorldContext.cs`, built by C1 from `L`, and joined through the K1 registry | S0, C1, H1 |
| CX2 | Strict JSON and canonical hashing exist only in `APMW.Client`. The Games Army reader cannot use them | S0 creates `ApmwCanonicalJson.cs` and the `ApmwArmyData.cs` records. B1 creates `ApmwArmyReader.cs`. C1 delegates `ApmwContractV2` hashing | S0, B1, C1 |
| CX3 | E1 did not list terminal-result publication. The current finish notification misses `TestForWinLossDraw` results | K4 and the E1 exit criteria require one notification per committed result | E1, H1, T1 |
| CX4 | The library report proposes `RecordCommittedLocations` and `RecordFinalGoal` beside the sink | N1 implements K8. `Record` is its only recording path | N1 |

S0 now also owns `ApmwWorldContext.cs`, which C1 previously created.
Shared declarations stay with S0. Readers request amendments through S0.

## 6. S0 gate checklist

S0 passes when every item below exists and the integration owner accepts it:

1. Public or internal declarations for K1-K10, with the visibility in section 1.
2. The X1 primitive, passing the §10.4 canonicalization fixtures in C#.
3. Complete synthetic A, L, C, S, and W fixture JSON, with one negative mutation per strict rule.
4. One compile example per cross-assembly reader: APMW.Client reading K1, K2, K4, K5;
   GUI reading K9 and K10.
5. A recorded owner for every K and W row, matching the manifest.
