# Archipelago.MultiClient.Net reconnect and lifecycle research

Research date: 2026-09-18. Status: research complete, implementation not authorized.
The evidence supports manual reconnect with serialized ownership and a separate process-memory journal. It does not support a permanent latch or timer-based retirement. [S2] [S3] [C1]

Decision follow-up: Q42 selects normal AP generation-name/team/slot identity
plus frozen-contract checks, without a new identifier.
[Ticket 23](../issues/23-specify-connection-lifecycle.md#q42-selected-replay-identity)
records that decision and its accepted collision limit.
The product choices listed in this report reflect the research handoff,
before Q42.

## Authority and scope

The research brief supplies the ChessV dependency version: `Archipelago.MultiClient.Net` 6.6.0. This report does not duplicate the other agent's ChessV source or history investigation.
Local authority reads were limited to `CONTEXT.md` and ADRs 0018-0021. [A1] [A2] [A3] [A4] [A5]
The following decisions come from the user brief, not from external client examples:

- The historical latch prevented concurrent attempts and connections while already active.
- A truly disconnected client can reconnect after the previous attempt ends.
- The 15+ second idle example is deliberately conservative, not a selected timeout.
- Each match retains its earned Location IDs in ChessV process memory after its window closes.
- Process restart loses that record. No durable journal file is authorized.
- Replay requires the original world and slot. The original cost snapshot and contract remain fixed.
- Controller takeover permanently suppresses that match's Location and goal reports, including pending replay.

ADRs 0019-0021 separate reversible move state, permanent reporting eligibility, and earned accomplishments. [A3] [A4] [A5]
The user brief settles window-close and process-restart storage scope that ADR 0021 previously left open. [A5]
This research changes no production code, dependencies, connection state, or Git history.

## Pinned 6.6.0 facts

The official `v6.6.0` tag resolves to commit `063dfa5ad14bafbfe646bd67d54deb1a8fa2f036`. The release date is 2025-02-22. Both the source and release are available. [S0] [S1]
The package targets `net35`, `net40`, `net45`, `netstandard2.0`, and `net6.0`. The first two targets use websocket-sharp. The other targets use `ClientWebSocket`. [S1] [S4] [S5]
The ChessV source owner must identify the actual selected package asset before applying target-specific conclusions.
The local source/history follow-up identifies local XML as 5.0.6, not 6.6.0. This report uses pinned source semantics, not that XML. [Local report](reconnect-guard-history.md)

### Creation, login, and connection state

`ArchipelagoSessionFactory.CreateSession(Uri)` and `CreateSession(string hostname, int port = 38281)` construct a session and its helpers. Construction does not log in. Each call creates new helpers. [S6]
`TryConnectAndLogin(game, name, itemsHandlingFlags, version, tags, uuid, password, requestSlotData)` returns `LoginResult`. Success provides `LoginSuccessful`. Refusal or timeout normally provides `LoginFailure`, but callers must also handle transport exceptions. [S2] [S3]
For non-`NET35` targets, the asynchronous sequence is `ConnectAsync()` followed by `LoginAsync(...)`. There is no `TryConnectAndLoginAsync`. Neither method accepts a caller cancellation token. [S2]
`version` describes AP protocol compatibility, not the NuGet package version. Its default packet value is `0.6.0` in this tag. [S3]

`Socket.Connected` describes the socket's believed transport state, not authenticated slot ownership or a successful ping.
The modern transport counts `Open` and `CloseReceived` as connected. websocket-sharp counts `Open` and `Closing`. [S9] [S5]
`SocketOpened` does not mean that login succeeded. AP sends `RoomInfo` before it accepts the `Connect` login packet. [P8]
`ConnectionRefused` leaves the transport open for another login. Login failure alone does not establish disconnection. [P8]
`SocketClosed` is an event with a reason string, not a Boolean state property.
Its delegate has no sender argument, so application handlers need an explicit captured session owner. [S7]
`ConnectionInfo.Team` and `.Slot` update on `Connected`. Socket closure does not reset them in this helper.
Old identity fields alone cannot authorize a new send. [S8]

### Timeout, cancellation, and cleanup limits

The session source uses a four-second constant, despite XML comments that describe five seconds. That constant is not a reconnect delay or a reliable bound for all underlying work. [S2] [S3]
`ConnectAsync()` starts socket work and waits internally. Its cancellation completes the RoomInfo task, not the underlying socket operation.
If socket connection finishes without `RoomInfo`, the asynchronous RoomInfo wait can remain unresolved. `TryConnectAndLogin` separately times out its RoomInfo wait without disconnecting on non-`NET35` targets. [S3]
The modern socket uses `CancellationToken.None` for connect, send, receive, and close operations. [S4] [S9] [S10]
Overlapping calls on one session replace shared login and RoomInfo task-completion fields. A timed-out wrapper or abandoned `Task.Run` does not prove attempt retirement. [S3]

The public cleanup API is `session.Socket.DisconnectAsync()` on non-`NET35` targets. `NET35` also exposes synchronous `Disconnect()`, while its asynchronous disconnect returns `void`.
Neither the session nor socket interface supplies `Dispose`, `Abort`, or caller-controlled cancellation. [S2] [S7]
The modern disconnect method awaits `CloseAsync` and raises `SocketClosed`. It does not join the background send and receive loops or drain queued work. [S9] [S10]
For a known-open socket, successful awaited disconnect provides orderly transport closure, not callback quiescence. Generation checks remain necessary after that close. [S9] [S10]
Receive errors call `ErrorReceived`. That error path does not itself guarantee a `SocketClosed` event.
Close notification also has multiple call sites without an exactly-once guard. Retirement must tolerate duplicate and missing notifications. [S9] [S10]

**Planning boundary:** 6.6.0 does not provide a proven, bounded hard-abort operation through these public interfaces.
The connection owner must observe connection-attempt termination and completed transport closure before it declares strict physical single-flight retirement.
An application timeout can revoke reporting authority, but cannot prove that the socket stopped.
Unresolved retirement needs a visible error/state, not silent success or a new overlapping attempt. [S3] [S7] [S9]

### Callback threading and session replacement

The modern implementation starts polling and sending with `Task.Run`. Packet handlers execute directly from the polling path, without UI-context dispatch.
Other helper events can run on the caller's thread, including locally added Location checks. Handlers must marshal UI changes and serialize lifecycle mutations. [S9] [S10] [S18]
A synchronous connect/login call on the UI thread also blocks that thread. [S3]

Fresh sessions are the conservative default for reconnect candidates, not a requirement to upgrade the package.
The modern socket normally retains its constructor-created `ClientWebSocket`. It only replaces that socket during the unspecified-scheme WSS-to-WS fallback, not through a general reconnect reset.
websocket-sharp creates a socket during connection, so reuse behavior differs by target. [S4] [S5]
Fresh sessions prevent old helper state from crossing world boundaries, but do not cancel the previous session.
Subscriptions, item-processing state, and caches do not automatically migrate to a replacement session. [S6] [S8] [S13]

## Location and goal delivery

The Location helper owns instance fields for all Locations, locally checked IDs, and server-confirmed IDs. `Connected` loads checked/missing IDs. `RoomUpdate` adds server-confirmed checks. [S11]
`Connected` unions IDs into existing sets instead of clearing them. Reuse across worlds risks stale checked and confirmed state. [S11]
`CompleteLocationChecks(ids)` records known IDs before sending. It sends all locally checked IDs except server-confirmed IDs, not merely the new arguments. [S12]
An empty-argument call can resend unconfirmed checks. Unknown IDs do not enter the checked set. [S12] [S18]
The pinned tests explicitly cover suppression of confirmed IDs and resend of unconfirmed IDs. [S14]
`AllLocationsChecked` includes local accomplishments before server confirmation. It is not an acknowledgment list. [S11] [S12]
This state survives only while that helper instance survives. It is not a process-wide journal or durable storage. [S6] [S11]

There is no unconditional Location resend on `Connected`. An item-index mismatch calls `CompleteLocationChecks()` internally, after a `Sync` packet.
Thus automatic resend exists on a specific resynchronization path, not as a complete reconnect policy. [S11] [S13]
This internal path also lacks match identity and takeover eligibility.
Pending match-owned checks must not rely on the helper cache as their only reporting gate. [S12] [S13] [A4]

The modern `CompleteLocationChecksAsync` returns `Task.Factory.StartNew(async ...)` without `Unwrap`. Its returned outer task can finish before the nested send finishes.
The socket send queue also has failure paths that do not complete the associated task.
Neither task completion nor `PacketsSent` proves server acceptance. [S12] [S10] [S7]
These are pinned source observations, not results from runtime tests.

The server subtracts previously checked IDs, intersects the remainder with the slot's Locations, and awards only that remainder.
Repeated Location reports are idempotent for the same server world/team/slot state. [P2]
Deduplication does not protect another world with overlapping Location IDs.
It also does not enforce ChessV's match eligibility or original cost contract. [P2] [A4] [A5]

`SetGoalAchieved()` sends `StatusUpdate(ClientGoal)`. The session action stores no pending goal record. [S15]
The inspected server ignores further status changes after `CLIENT_GOAL`, including another identical goal report.
The first goal invokes goal side effects. Replays do not invoke them again while that server state remains intact. [P3]
This verifies goal idempotence for the cited server implementation, not every custom server or a restored/reset server state.
Goal replay still requires the same identity and permanent eligibility gates as Location replay. [P3] [A4]

## What identifies the original world?

The protocol calls `RoomInfo.seed_name` the uniquely identifying name of a generation.
Version fields describe software versions, tags describe features, and `time` is the current server timestamp. None identifies a room. Standard RoomInfo contains no room UUID. [P1] [P4]
In 6.6.0, `session.RoomState.Seed` exposes `seed_name`. `ConnectionInfo.Team` and `.Slot` expose the authenticated coordinates from `Connected`. [S16] [S8]
`Connect.uuid` identifies the client/player, not the world. The library generates a UUID when the caller omits it. [P5] [S8]
Datapackage checksums identify game mapping data, not a generated world or its cost snapshot. [P1] [P6]

There is a uniqueness limit: generation assigns `seed_name` from an optional name or the numeric seed. `Main.py` supplies the requested output name. [P7] [P9]
The name is not a cryptographic world digest or a universally enforced room-instance identifier. [P7]
Two generated worlds can reuse a name. Two room instances can also serve copies of the same generated world.
The standard fields cannot prove that such instances are the same server run. [P4] [P7]

**Recommended identity evidence:** exact nonempty seed name, authenticated team and slot, game, and original slot name.
The frozen ChessV contract/cost snapshot and published Location set also need comparison before replay.
This combines protocol identity with the user's fixed-contract rule. Equal schemas or equal prices alone do not prove world identity. [P1] [P7] [A5]
The server address and port are connection routes, not journal keys. Changed routes can lead to the same world. Reused routes can lead to another world. [P1] [P4]
Under normal AP seed-name uniqueness, these comparisons support replay across a port change.
They do not establish absolute identity against deliberate or accidental seed-name reuse with otherwise identical visible data. [P7]
For that stronger guarantee, an authoritative generation marker must already exist or become an explicit world/client contract.
This report does not invent such a field. Missing or conflicting required identity evidence blocks replay, while the process retains the set.

## Maintained usage: patterns, not guarantees

**StardewArchipelago and its utility library:** the game calls `APUpdate()` from `OnUpdateTicked`. Its manual retry dialog also accepts a replacement server address. [C2] [C9]
The utility client creates fresh sessions, removes event subscriptions during cleanup, and implements delayed automatic retries. [C3] [C10]
The game exposes reconnect failure/success feedback and a configured response to repeated failures. [C4]
Those retry intervals and game responses are product policy, not MultiClient.Net behavior.
The inspected utility cleanup does not await disconnect, and its retry method has no explicit in-flight mutex.
It is useful lifecycle precedent, not proof of race-free serialization. [C3]
Version caveat: Stardew declares MultiClient.Net 6.7.1 and utility 3.1.15. The inspected utility head declares 3.1.16 and MultiClient.Net 6.6.0. This is not an exact packaged-source match. [C5] [C11]

**TUNIC:** the normal connect path disconnects, creates a fresh session, and calls `TryConnectAndLogin`.
A separate silent-reconnect path reuses the session. Cleanup calls `DisconnectAsync` without awaiting it. These paths show application policy, not interchangeable safe recipes. [C6]
The inspected client declares MultiClient.Net 6.7.1, so these examples do not redefine 6.6.0 semantics. [C12]

**Official AP CommonClient:** this Python client is not a .NET API example.
Its manual connect awaits disconnect, which closes the socket and awaits the previous server task. It cancels pending autoreconnect work. Automatic retry guards the server task. [C1] [C7] [C13]
On `Connected`, it compares `(seed_name, team, slot)` before replay. A changed identity clears local progress. A matching identity permits Location and goal replay. [C8]
ChessV instead retains separately bound per-match sets in memory, as the user requested.
The useful precedent is identity-gated replay and explicit task ownership, not CommonClient's clearing or persistence policy. [C8] [A5]

## Recommended lifecycle invariants

These are planning recommendations, not claims that the library already enforces them:

1. **One owner:** serialize connect, login, disconnect, and reconnect through one coordinator. Each candidate owns a generation token, session, subscriptions, and observed transport work. [S3] [S7] [C1]
2. **Explicit states:** a four-state model can use `Disconnected`, `Connecting`, `Connected`, and `Stopping`. `Connecting` includes authentication and identity verification. `Stopping` includes unresolved retirement. Manual retry starts after retirement, without a permanent latch. [S3] [C1]
3. **No timer proof:** elapsed idle time never establishes retirement. `Socket.Connected == false`, a wrapper timeout, or a close callback alone is insufficient evidence. [S3] [S9]
4. **Stale-owner rejection:** callbacks capture their originating session and token. Old completion, error, item, and close callbacks cannot publish or retire the current owner. [S3] [S7] [S13]
5. **Two gates:** authenticated connection identity and match reporting eligibility are independent.
   Replay starts only after identity/contract verification, and rechecks the originating match at dispatch. [A4] [A5] [P1]
6. **Process-owned journal:** retain match ID, identity evidence, fixed contract, earned IDs, and irreversible eligibility outside windows and sessions.
   Window closure preserves the record. Undo preserves earned IDs. Process exit loses it. [A3] [A5] [user brief](#authority-and-scope)
7. **No hidden bypass:** takeover suppresses pending progress through every send and resend path.
   A proposed adapter can send explicit `LocationChecksPacket` snapshots without adding pending IDs to the helper cache.
   That adapter requires proof against internal resync and queued-dispatch races, while item reception remains active. [S12] [S13] [A4]
8. **Replay, not reconstruction:** resend eligible earned IDs for the verified binding.
   Keep accomplishments despite uncertain delivery. Do not recalculate them from the current board or a replacement cost table. [A3] [A5] [P2]

## Proposed tests, not run

These fixtures derive from the cited API limits and authority decisions:

| Fixture | Required observation |
| --- | --- |
| Two simultaneous reconnect requests | Exactly one candidate reaches transport connect. [S3] |
| Timeout followed by late login success | The stale candidate gains no reporting authority or current-session ownership. [S3] |
| Open socket without RoomInfo | The UI remains responsive. A wrapper deadline does not permit physical overlap. [S3] [S9] |
| Close/error duplication and reversed order | Retirement is idempotent. No event retires a replacement owner. [S9] [S10] |
| Old callback after successful replacement | No stale UI mutation, progress send, or disconnect of the current owner. [S7] |
| Genuine disconnect, then manual retry | Reconnect succeeds without process restart or a fixed idle delay. [C1] |
| Same world/slot at a changed port | Replay uses the original earned set and cost contract after verification. [A5] [P1] |
| Reused port with another seed/team/slot/contract | No Location or goal replay. The original record remains in process memory. [A4] [A5] [P7] |
| Identical visible identity from a different generation | The selected stronger-identity policy applies. The test cannot assume a universal AP UUID. [P7] |
| Window close, fresh session, process restart | The first two preserve the application record. Restart loses it. [user brief](#authority-and-scope) |
| Earn, undo, disconnect, replay twice | Earned IDs remain. The server awards each Location once. [A3] [P2] |
| Takeover before replay or internal item resync | No pending progress from that match escapes. Items can still arrive. [A4] [S13] |
| Async helper completes before nested send | No false delivery acknowledgment or journal deletion occurs. [S12] |
| Earn goal, lose delivery, replay goal twice | Goal side effects occur once for unchanged server state, subject to eligibility. [P3] |

## Remaining product choices and confidence

Manual reconnect is sufficient for this design. Automatic retry, retry limits, backoff, and user cancellation behavior remain product choices. [C3] [C7]
Disk persistence is not an open choice here. The user selected process-memory retention and loss on restart.
Goal-intent retention after window closure still needs an explicit decision separate from Location-set retention.
The library supplies an idempotent goal command, but not a pending-goal journal. [S15] [P3] [A5]
Identity policy must choose whether normal AP seed identity plus the frozen contract is sufficient.
A stronger collision-resistant guarantee requires an authoritative generation marker, not another timeout or the server port. [P7]
Confidence is high for the pinned source facts and cited server behavior, but this research does not establish runtime cleanup guarantees.
No package upgrade is required by this report. Current generated docs include newer APIs such as `Hints`, absent from the pinned session interface. [S2] [S17]

## Primary sources and local authority

[A1]: ..\..\..\CONTEXT.md
[A2]: ..\..\..\docs\adr\0018-reject-fen-based-apmw-resume.md
[A3]: ..\..\..\docs\adr\0019-support-reliable-apmw-undo-in-zero-four.md
[A4]: ..\..\..\docs\adr\0020-stop-apmw-progress-after-controller-changes.md
[A5]: ..\..\..\docs\adr\0021-replay-earned-locations-after-reconnect.md
[S0]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/releases/tag/v6.6.0
[S1]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Archipelago.MultiClient.Net.csproj#L1-L20
[S2]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/ArchipelagoSession.cs#L61-L126
[S3]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/ArchipelagoSession.cs#L248-L399
[S4]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/ArchipelagoSocketHelper_system.net.websockets.cs#L22-L98
[S5]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/ArchipelagoSocketHelper_websocket-sharp.cs#L39-L139
[S6]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/ArchipelagoSessionFactory.cs#L18-L45
[S7]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/IArchipelagoSocketHelper.cs#L18-L91
[S8]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/ConnectionInfoHelper.cs#L75-L118
[S9]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/BaseArchipelagoSocketHelper_system.net.websockets.cs#L50-L155
[S10]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/BaseArchipelagoSocketHelper_system.net.websockets.cs#L233-L340
[S11]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/LocationCheckHelper.cs#L245-L297
[S12]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/LocationCheckHelper.cs#L354-L412
[S13]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/ReceivedItemsHelper.cs#L144-L207
[S14]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net.Tests/LocationCheckHelperFixture.cs#L366-L426
[S15]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/ArchipelagoSessionActions.cs#L18-L40
[S16]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/RoomStateHelper.cs#L158-L168
[S17]: https://archipelagomw.github.io/Archipelago.MultiClient.Net/api/Archipelago.MultiClient.Net.IArchipelagoSession.html
[S18]: https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/LocationCheckHelper.cs#L496-L516
[P1]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/docs/network%20protocol.md#L73-L143
[P2]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/MultiServer.py#L1169-L1215
[P3]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/MultiServer.py#L2290-L2304
[P4]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/MultiServer.py#L956-L975
[P5]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/docs/network%20protocol.md#L291-L309
[P6]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/docs/network%20protocol.md#L720-L739
[P7]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/BaseClasses.py#L212-L219
[P8]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/docs/network%20protocol.md#L1-L14
[P9]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/Main.py#L35-L39
[C1]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/CommonClient.py#L497-L505
[C2]: https://github.com/agilbert1412/StardewArchipelago/blob/2fc71324de7f8d0b2aba84dd3a0cec964ab9c997/StardewArchipelago/ModEntry.cs#L416-L433
[C3]: https://github.com/agilbert1412/ArchipelagoUtilities/blob/a7135cf6a91208bd6658ec6b99e699ebb8943e50/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net/Client/ArchipelagoClient.cs#L909-L1000
[C4]: https://github.com/agilbert1412/StardewArchipelago/blob/2fc71324de7f8d0b2aba84dd3a0cec964ab9c997/StardewArchipelago/Archipelago/StardewArchipelagoClient.cs#L235-L259
[C5]: https://github.com/agilbert1412/StardewArchipelago/blob/2fc71324de7f8d0b2aba84dd3a0cec964ab9c997/StardewArchipelago/StardewArchipelago.csproj#L28-L35
[C6]: https://github.com/silent-destroyer/tunic-randomizer/blob/b5403bb986bfe0b7f45df35b17c1c2429def6e8d/src/Archipelago/ArchipelagoIntegration.cs#L79-L208
[C7]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/CommonClient.py#L863-L941
[C8]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/CommonClient.py#L1028-L1064
[C9]: https://github.com/agilbert1412/StardewArchipelago/blob/2fc71324de7f8d0b2aba84dd3a0cec964ab9c997/StardewArchipelago/ModEntry.cs#L547-L551
[C10]: https://github.com/agilbert1412/ArchipelagoUtilities/blob/a7135cf6a91208bd6658ec6b99e699ebb8943e50/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net/Client/ArchipelagoClient.cs#L202-L206
[C11]: https://github.com/agilbert1412/ArchipelagoUtilities/blob/a7135cf6a91208bd6658ec6b99e699ebb8943e50/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net.csproj#L3-L43
[C12]: https://github.com/silent-destroyer/tunic-randomizer/blob/b5403bb986bfe0b7f45df35b17c1c2429def6e8d/packages.config#L1-L5
[C13]: https://github.com/ArchipelagoMW/Archipelago/blob/e0b6083384cd37019f4c41da0970dff1d99f1325/CommonClient.py#L567-L575
