# Reconnect guard: local source and history

**E - Baseline:** `d14300c0b5e6091fa57dda2cf073ead9b5fc5ad0`, verified on 2026-09-18.
The production files examined match HEAD. Parent planning documents contain concurrent, uncommitted work.
This report uses local source, Git history, and cached API metadata. It contains no runtime reproduction.
No builds, tests, installs, real connections, or credential-file reads occurred. Only this report is an authorized repository write.

Labels: **D** = author decision, **E** = source evidence, **R** = engineering recommendation, **Q** = unresolved question.
Source paths are relative to `C:\GitHub\chessv`. Short citations expand as follows:
**C** = `APMW.Client\Client.cs`, **L** = `APMW.Client\LocationHandler.cs`,
**F** = `ChessV.GUI\Forms\ApmwForm.cs`, **CFG** = `APMW.Client\Config.cs`.

## Authority and limits

**D:** ADR 0018 rejects FEN-based APMW resume. ADR 0019 selects reliable Undo for 0.4.0.
ADR 0020 permits takeover but permanently suppresses that match's Location and final-goal reports, including pending reports.
ADR 0021 preserves match accounting across connection loss and replays the earned-Location set after verified same-world/slot recovery.
Sources: `docs\adr\0018-reject-fen-based-apmw-resume.md:8-15`, `docs\adr\0019-support-reliable-apmw-undo-in-zero-four.md:8-36`,
`docs\adr\0020-stop-apmw-progress-after-controller-changes.md:8-60`, `docs\adr\0021-replay-earned-locations-after-reconnect.md:8-51`.

**D:** The latest author statement rejects a journal file and permits retention after game-window closure while ChessV remains open.
ADR 0021 records that process-memory lifetime at lines 53-72. The journal is neither the capture ledger nor a saved chess position.
The author's "15+" seconds is an example, not a selected timer or proof that a quiet operation finished.
Automatic retry scheduling and new credential persistence remain unselected (`.scratch\large-board-enemy-armies\issues\23-specify-connection-lifecycle.md:72-73`).

## Exact guard and current state graph

**E:** `C:139-160` serializes calls to `Connect` with `lock (typeof(ArchipelagoClient))`.
Its first guard is `connectionTask != null && !connectionTask.IsCompleted && !connectionTask.IsFaulted`. It logs exactly `"Connection task currently processing"` and returns.
Its second guard is `(hostName, port) == lastServerUrl && slotName == lastSlotName`. It logs exactly:

> Reconnect attempt prevented. If you don't successfully connect, try restarting this client

**E:** The second guard does not inspect socket state, elapsed time, password changes, world identity, or the reason the previous attempt ended. These are historical fields, not a connection-state enum (`C:122-124,143-158`).

| E: Entry or outcome | Actual transition and supporting source |
| --- | --- |
| New permitted attempt | Calls `Dispose` first, records endpoint/slot, creates and publishes `Session`, captures a local `session`, then starts a task (`C:154-165,294-296`). |
| Attempt still incomplete | Rejects every new target through the task guard. There is no cancellation, timeout, or pending-intent queue (`C:139-173,294-296`). |
| Login failure result | Logs errors and returns. Does not clear `Session` or endpoint/slot. The close handler is not yet attached (`C:173-205`). |
| Same endpoint/slot after that failure | The completed task passes the first guard. The unchanged endpoint/slot fails the second guard, even with a corrected password (`C:143-186`). |
| Successful login | Records endpoint again, saves recent connection coordinates, initializes Locations, then attaches `SocketClosed` (`C:188-205`). |
| Post-login rejection | Invalid required version, insufficient version, invalid contract, insufficient contract version, or unsupported legacy 6x8 goal requests `DisconnectAsync` and returns (`C:213-267`). None awaits cleanup. |
| Validated connection | Instantiates config, creates the item handler, publishes geometry, enables optional DeathLink, then raises `OnConnect` (`C:258-294`). The task subsequently completes. |
| Exception | There is no encompassing `catch`/`finally`. Factory errors escape synchronously. Task-body errors fault the task without guaranteed cleanup (`C:157-296`). Endpoint recording precedes factory creation. |
| Connect while already connected | Same endpoint/slot is blocked. A different endpoint or slot passes after task completion and calls `Dispose` before replacement (`C:143-160`). |
| Current-session socket closes | Identity comparison succeeds, then `Dispose` runs. A subsequent same-target attempt can pass after the task finishes (`C:313-350`). |
| Old-session socket closes | A different session reference produces an `"A local session ended"` log without disposal (`C:343-351`). |
| Explicit `Dispose` during an attempt | Does not cancel or await the task. The incomplete-task guard remains active even after `Session` becomes null (`C:313-337`). |

**E:** Not every reconnect requires restart. The login-failure path proves a latch without assuming a race.
Handled current-session closure normally clears the endpoint guard. No idle timer releases either guard.
`IsFaulted` is redundant once `IsCompleted` is true. No code here requests task cancellation.

### Supporting state and teardown

**E:** `Dispose` requests socket disconnect only when `Session.Socket.Connected` is true.
It does not await that request or join the connection task (`C:313-320`). Exceptions can interrupt disposal before its resets.
It calls `UnloadMatch`, removes the item callback, calls `ItemHandler.Unhook`, then clears `ItemHandler` and `Session`.
It resets `lastServerUrl` to `("", -1)`, but leaves `lastSlotName` and `connectionTask` unchanged (`C:320-330`).
It resets geometry and config, then emits `OnClientDisconnect(0, ..., true)` regardless of the original close cause (`C:330-337`).

**E:** `CFG:381-386` resets only `SlotData`, `CurrentContract`, and `UsesCurrentContract`.
It does not reset every config field, the connection task, or `LocationHandler.Initialized`.
Geometry's `IsConnected` becomes true during geometry initialization, not from a live socket query (`APMW.Client\ApmwGeometrySelection.cs:168-175,193-198`, `C:439-449`).

**E:** `LocationHandler.Initialized` starts false and becomes true on initialization.
Neither `EndMatch` nor client disposal resets it. `TryValidatePlayingArchipelago` returns it (`L:29-39,65-80,137-161,820-827`).
That flag means historical initialization, not "safe to report now".

`EndMatch` clears capture counts and move diffs and removes the Undo subscription.
It retains its private match reference and original-square map. It can schedule `deathlink("resigned.")` when the match winner differs from the human (`L:137-175`).
Connection-only loss currently reaches this method through `C:300-320,343-350`. This conflicts with the accepted preservation requirement.

## Attempt callers, callbacks, and closure ownership

| E: Surface | Source-proven behavior |
| --- | --- |
| GUI attempt entries | Button click, Enter in either address/slot field, and `timer1_Tick` all call `button1_Click`. Text-change timer starts are commented out (`F:105-155`). |
| GUI throttle and validation | The button disables before parsing and re-enables through timer2 after 410 ms. Invalid addresses log and return. The throttle is independent of connection completion (`F:110-114,146-184`, `ApmwForm.Designer.cs:135-141` in the same directory). |
| GUI disconnect | There is no implemented Disconnect/Cancel action. The button TODO says players close the program. The sole production `Connect` caller is `F:184`. Client-owned disposal entries are replacement and current-session close (`F:148`, `C:155,350`). |
| Login completion | The task calls mutable `Session.TryConnectAndLogin`, but later uses captured `session`. It writes shared handlers/config without an attempt-generation check (`C:159-165,188-294`). |
| Socket close | The lambda captures the session reference. The callback compares it, but comparison and disposal do not share the Connect lock. No delegate is retained for removal (`C:205,313-351`). |
| Items | `ItemHandler` stores its item delegate and detaches it through `Unhook`. Its callback updates global providers. The client callback ignores `sender` and refreshes from its current handler (`APMW.Client\ItemHandler.cs:43-92,223-237`, `C:323-325,422-430`). |
| DeathLink | Client and GUI attach anonymous receive callbacks. They consult shared current-match state, not an originating generation. Client disposal does not remove them. GUI disconnect disables its checkbox and clears references (`C:272-291`, `F:257-324`). |
| GUI subscriptions | Constructor attaches four client events. Message-log replacement removes the previous delegate. Form disposal removes those events and the log delegate, but does not dispose the client (`F:41-48,185-194,412-420`). |
| Lobby closure | The form's own closing handler hides and cancels. MainForm additionally calls `apmwForm.Dispose` on that event. Neither path explicitly disconnects the singleton (`F:98-102`, `ChessV.GUI\Forms\MainForm.cs:354-367`). |
| Game-window closure | Aborts search and invokes `Game.Match.Finished()` with its default null argument. Client and lobby finish lambdas ignore the argument and call `UnloadMatch` (`ChessV.GUI\Forms\GameForm.cs:594-600`, `ChessV.Base\Match.cs:38`, `C:40-45`, `F:214-221`). |
| Other match endings | Engine `finish()` and `Death()` raise `Finished(this)`. LocationHandler also subscribes through core hooks and the match (`ChessV.Base\Match.cs:367-385,592-608`, `L:29-38,117-160`). |
| Pending progress | Location tasks resolve mutable `LocationCheckHelper` at execution. Victory captures a session but reads mutable match/config/helper state. No originating-match eligibility recheck exists (`L:65-72,374-375,761-788`). |

**E:** The code proves missing ownership checks, not a reproduced callback race.
Possible schedules include late success after disposal, closure during validation, and an old close callback passing its comparison before session replacement.
Old item, DeathLink, match-finish, and report callbacks also require fixtures. Library callback ordering and cancellation guarantees belong to the separate MultiClient.Net investigation.

## History provenance

**E:** Evidence came from `git blame`, focused `git log -L`, `git log -S`, and commit diffs. Dates are author dates. The guard history supports the author's recollection of early project work.

| Commit / date | Evidence and stated intent |
| --- | --- |
| `39bc805e`, 2023-10-24 | "resolve small connection state issues": makes `connectionTask` static and adds the unfinished-task guard. |
| `49bb56f4`, 2023-10-24 | "i broke connecting": puts the guard and Connect body under the type lock. Temporarily removes the task wrapper. |
| `ae649472`, 2023-10-24 | "disable submit on type": restores the task wrapper and comments out GUI text-change timer starts. |
| `06b0738f`, 2023-10-26 | "start working on some session reconnect issues": captures the local session for close callbacks and rejects old-session closes. Login still uses the shared `Session` property. |
| `ba46cd0d` / `116e10d8`, 2023-10-26 | First adds the singleton/Initialized flag, then "fix reconnect tracking issue" permits reinitialization and moves core hook registration into the constructor. |
| `59ce62f926315298253c7c0b4ae6eb5bcb2f4185`, 2023-10-26 19:07:56 -07:00 | Introduces the same-server guard and its current text. Subject: "prevent double taps". |
| `074f8943`, 2023-10-27 | "fix reconnect issues and add castling privileges": moves endpoint recording after disposal, repeats it after success, and clears it during disposal. |
| `b341c113`, 2023-11-05 | "support connecting to private servers": replaces URI comparison with `(hostName, port)` and uses the current reset sentinel. |
| `5bd68f46`, 2023-12-17 | "Allow swapping games without restarting client": adds `lastSlotName` to the guard and records the supplied slot. |
| `9b49f1f6` / `e6e80ae1`, 2024-12-21 | Move close subscription earlier within successful login, then add current/old-session close logging. Reference-based filtering remains. |
| `68ed411c`, 2025-07-19 | Adds slot-data client-version refusal. It requests asynchronous disconnect. |
| `10999c8c`, 2026-07-19 | Adds contract refusal paths, geometry/config reset, handler cleanup, and optional event invocation. Does not replace the reconnect guards. |
| `82b9ba3`, 2026-08-17 | Latest Client.cs change at this baseline. Adds the legacy 6x8-goal refusal path. |

**E:** The introducing commit body states:
> actually it destroys the locationhandler's socket whenever you connect to the same server again and I don't know why, but something I did today fixed that and I'm not looking a gift horse in the mouth

This records the author's observation and uncertainty in 2023. It does not establish a root cause, a library defect, or current reproducibility.

## Identity available locally

**E:** The declared dependency is MultiClient.Net 6.6.0 (`APMW.Client\APMW.Client.csproj:37`).
Metadata from the existing `ChessV.GUI\bin\Debug\net7.0-windows7.0\Archipelago.MultiClient.Net.dll` reports assembly version `6.6.0.0`.
Metadata inspection, without object creation, exposes `Session.RoomState.Seed` through `IRoomStateHelper`.
Other properties include `Session.ConnectionInfo.Game`, `.Team`, `.Slot`, `.Uuid`, and `LoginSuccessful.Team` / `.Slot`.

The local XML at `APMW.Client\bin\Debug\Archipelago.MultiClient.Net.xml` accompanies **5.0.6**, not 6.6.0.
It describes Seed as the generation name and Team/Slot as connection identity (`:483-516,972-1001`). These older descriptions do not prove 6.6.0 uniqueness or reconnect behavior.

**E:** ChessV reads authenticated numeric Slot for DeathLink naming (`L:802-807`).
The inspected client/GUI source does not read RoomState.Seed or ConnectionInfo.Team, or retain a world/slot binding. Neither do the packet fields at `C:112-115`.
Contract `ManifestSha256` describes the contract, not a unique generated room (`APMW.Client\ApmwContractV2.cs:1085-1114`).
`Convenience.success` writes recent endpoint/slot coordinates, not a journal or verified identity (`APMW.Client\Convenience.cs:38-55`). No actual convenience or credential file was read.

**R:** A candidate binding is the authenticated generation identity plus Team, numeric Slot, and game, attached to each originating match. Seed semantics and duplicate-generation edge cases require agreement with the separate library/protocol report.
Address/port select a transport destination only. Slot-name text and client UUID are not substitutes for authenticated world/slot identity.
The existing endpoint guard wrongly substitutes destination history for connection state. No current journal replay implementation misroutes by port.
Reusing this guard as journal identity introduces that error.

## Candidate bounded state machine

**R:** One coordinator owns `Idle`, `Connecting`, `Connected`, and `Disconnecting`. It permits one live attempt, one monotonic generation, and at most one pending explicit connect intent.
The attempt owns its candidate session, subscriptions, task, cancellation/cleanup handle, and immutable request. Its candidate session remains separate from the published, authenticated connection.

| R: State and event | Proposed result |
| --- | --- |
| Idle + Connect | Creates a new generation and candidate. Allows the same endpoint immediately after settled cleanup. |
| Connecting + duplicate intent | Coalesces the request. Does not create another transport or reset the existing attempt. |
| Connecting + changed intent or Cancel | Invalidates the old generation before cleanup. Retains the latest explicit replacement intent, or clears it for Cancel. |
| Connecting + validated success | Publishes only the current generation after identity/config validation. Then evaluates eligible journal replay. |
| Connecting + rejection, exception, or current close | Observes the result and enters owned cleanup. Failure cannot leave an endpoint latch. |
| Connected + same live target | Reports the existing state without reconnecting. Changed credentials alone do not require replacing a healthy connection. |
| Connected + replacement, disconnect, or transport loss | Disables transport dispatch before cleanup. Does not end the match or clear its accounting/journal. |
| Disconnecting + explicit Connect | Records one desired request. It starts only after the old transport cannot connect, publish, or mutate current state. |
| Cleanup settled | Returns to Idle, then services the retained explicit intent once. No automatic retry loop is implied. |
| Obsolete callback in any state | Cannot publish, dispose the current connection, alter match state, or dispatch reports. Releases only its owned resources. |

**R:** State transitions and ownership checks use one synchronization policy.
Every completion captures its generation/session. Every report additionally captures its originating match, binding, and permanent eligibility.
Cancellation requires transport cleanup, not merely cancellation of an outer task. Failures receive explicit diagnostics.
Cleanup observes asynchronous failures and removes retained delegates.

A cleanup deadline is an engineering safeguard, not proof of disconnection. The design cannot start another live attempt merely because 15 seconds passed without a log message.
The exact cancellation primitive and cleanup completion signal depend on the library investigation.

**R:** Match accounting, config/profile binding, and process-memory journals have lifetimes separate from the transport.
A closed game window releases game resources but retains its earned IDs, goal marker, identity, and reporting restriction.
A different slot cannot overwrite an ongoing old match's reporting context. The UI badge and report dispatch use the same authority. Takeover neither simulates DeathLink nor kills the AP session.

## Planned fixtures, not executed tests

**R:** A fake session factory, controllable login/close completions, and a deterministic callback scheduler form the proposed seam around `C:139-351`.
The fixture log records generation, session, match, binding, cleanup completion, report calls, and subscription counts. No fixture needs a server, real credentials, sleeping, or a production journal file.

| Fixture stimulus | Required observation and source seam |
| --- | --- |
| Double click and Enter before/after the 410 ms GUI throttle | At most one live candidate. Duplicate intent does not restart it (`F:105-184`, `C:143-147`). |
| Login failure, then same endpoint/slot with corrected password | Cleanup finishes, then the retry starts without restart or a waiting period. Include factory and task-body exceptions (`C:157-186`). |
| Valid connection, then same target or a changed slot | Same live target creates nothing. Replacement waits for cleanup and uses a new generation (`C:148-160`). |
| Invalid version/contract/legacy 6x8 goal, with delayed close | All five refusal branches settle once. No partially initialized handlers survive (`C:213-267`). |
| Cancel/dispose before login, during validation, and immediately before publication | Late success cannot revive the attempt. Cleanup is idempotent. Pending intent obeys cancellation (`C:159-296,313-337`). |
| Old close and completion after a newer generation | Neither disposes nor publishes over the new session. Include a pause between close identity comparison and disposal (`C:343-350`). |
| Old item, DeathLink, UI, match-finish, and report callbacks | No effect on the new generation or another match. No obsolete subscriptions remain. Use the ownership surfaces listed earlier. |
| Transport loss during an unfinished match | Retains counters, origin map, Undo ledger, and earned set. No synthetic resignation/DeathLink from transport cleanup (`L:137-160`). |
| Earn offline, Undo, then same-world/slot reconnect | Counters roll back, earned IDs do not. Eligible IDs and an earned final-goal marker replay through the same gate (`L:179-197,374-375,774-788`). |
| Takeover before queued dispatch or offline replay | Permanent suppression blocks Locations and final goal, including after window closure. Reconnection cannot restore eligibility (ADR 0020). |
| Close the game window while ChessV remains open | Retains the eligible journal without retaining/resuming the board. Old finish callbacks cannot unload another match (`ChessV.GUI\Forms\GameForm.cs:594-600`, `C:40-45`). |
| Reused endpoint, changed port, changed Team/Slot, or missing identity | Different world/slot sends nothing from the old journal. Verified same world/slot on a changed port remains eligible. Missing identity blocks replay. |
| Lobby disposal and ChessV restart | Lobby subscriptions detach independently of client ownership. Process exit loses unsent journals. No journal file recovery (ADR 0021). |
| Quiet but still-live operation beyond an example interval | Does not unlock based on silence. A settled failed operation permits another explicit attempt without an arbitrary cooldown. |

## Remaining decisions and blockers

**R:** State names, synchronization, generation identity, explicit error handling, callback ownership, and cleanup mechanics are engineering choices.
**Q - Engineering:** The exact 6.6.0 cancellation boundary and sufficient generated-world identity require the companion primary-source report.
**Q - User policy only if exposed:** Can a user replace the connected slot while an old match remains open, or must that action wait?
Either policy must preserve the old match's accounting and prevent cross-slot reporting.
No additional decision about file persistence, FEN resume, a fixed 15-second timer, or automatic backoff is required.
