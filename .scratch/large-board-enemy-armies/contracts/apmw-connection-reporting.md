# APMW connection and reporting contract

Status: bounded engineering specification.
Date: 2026-09-18.
This document does not authorize implementation or claim runtime acceptance.

## Authority and scope

[ADR 0020](../../../docs/adr/0020-stop-apmw-progress-after-controller-changes.md)
selects permanent match suppression with the Q45 library-admission cutoff.
[ADR 0021](../../../docs/adr/0021-replay-earned-locations-after-reconnect.md)
selects process-memory journals, final-goal retention, and Q42 replay identity.
[Ticket 23](../issues/23-specify-connection-lifecycle.md) owns the lifecycle decision.

The design uses stock MultiClient.Net 6.6.0 and its selected `lib/net6.0`
asset in the .NET 7 Windows application.
It requires no transport fork, dependency extension, durable journal, or
guaranteed abort of every hung operation.
Automatic reconnection is outside this scope.

## Ownership and retained state

One application owner controls connection publication, retirement, reporting
admission, and permanent match suppression.
The exact class name is an implementation detail.

| Owned record | Required contents and lifetime |
| --- | --- |
| Connect request | One explicit user action and its bounded protocol route plan |
| Route | One fresh session, generation, actual connect task, tracked login invocation, event delegates, and retirement evidence |
| Published connection | Only the current authenticated session with validated identity and contract |
| Match record | Original match ID, frozen binding, earned Location set, separate goal marker, and irreversible eligibility |
| Report envelope | Original match, intended connection generation, immutable payload, and frozen binding |
| Reporting snapshot | The same authority that decides whether new admission is permitted |

The reversible move ledger remains separate.
Transport cleanup cannot clear it, end a match, resign, or fabricate DeathLink.
An eligible disconnected match continues to earn progress against its frozen
profile and costs.
Undo changes current counters, not historical earned IDs or the goal marker.

Window closure retains the reporting record without a board or window reference.
Process exit loses unsent intent.
A suppressed match never regains eligibility through Undo, controller restoration,
reconnection, or window closure.
A fresh normal match has its own eligibility.

Every application callback carries its route generation.
Posted callbacks repeat the ownership check when they execute.
Delegate removal alone does not cancel a callback already captured elsewhere.
Old callbacks cannot publish state, dispose a replacement, or affect another match.

## Atomic library admission

The final admission predicate requires:

- The originating match remains eligible.
- Its intended generation is the published, authenticated connection.
- That connection is not stopping.
- Its nonempty AP generation name matches the retained name exactly.
- Its authenticated team, slot, and game match the retained values.
- Its validated World binding, shared contract, and cost-snapshot hashes match.
- Its published Location set remains the original set.
- Every outgoing Location belongs to that set.

A mismatch blocks admission and retains the journal.
Address, port, and client UUID cannot replace this predicate.
Equal compared metadata from different worlds remains indistinguishable under Q42.

The owner prepares an immutable packet before the final gate.
Under the gate, it evaluates the predicate and directly invokes the captured
session's `Socket.SendPacketAsync(packet)`.
In the pinned asset, that method synchronously adds the packet to its queue
before returning a task.
That queue addition is the admission boundary.
The owner observes the returned task outside the gate.

No await, deferred task, notification, or synchronous send wait separates the
final predicate from that invocation.
Logging and external callbacks occur after gate release.
The payload belongs exclusively to that admission and cannot change afterward.

Under the same gate, takeover permanently suppresses the originating match.
That transition precedes progress from the changed controller.
Thus only two orders are possible:

| Order | Result |
| --- | --- |
| Library admission precedes suppression | That packet can finish, including from the private queue |
| Suppression precedes attempted admission | The application makes no send call for that match |

An earlier application task, envelope, or replay snapshot supplies no exception.
Each Location packet and each separate goal packet passes the gate again.
One Location packet can contain several IDs from one match.
The application cannot batch different matches into an unqualified report.
The library can batch packets after their individual admissions.

Admission is not a physical send, local completion, or server acceptance.
Some stock queue errors can leave send tasks incomplete.
Neither task age nor local completion discards retained intent.
After suppression, even a failed earlier admission cannot authorize a retry.
Admission failures and uncertain outcomes require explicit diagnostics.

## Helper containment

All application-earned Locations use direct `LocationChecksPacket` admissions.
The final goal uses a separately guarded `StatusUpdatePacket` with `ClientGoal`.
Application code never calls either helper completion method, including an
empty `CompleteLocationChecks` call.
It never invokes unrestricted goal reporting for a match.

| Current production surface | Required integration |
| --- | --- |
| `LocationHandler.cs:375` | Record committed IDs for the original match and request guarded admission |
| `LocationHandler.cs:783` | Route victory Locations through the same reporter |
| `LocationHandler.cs:788` | Retain the separate goal marker and request its own guarded admission |
| Read-only Location lookup | Use the match's original bound profile, not replacement-session global state |
| Reconnect and closed-window replay | Repeat the same predicate for every packet |

Every candidate has fresh helpers.
A helper with legacy locally unconfirmed IDs cannot be adopted.
Stock APIs provide no supported purge or filter for that private outbox.
The healthy AP connection and item reception stay active after takeover.

Without application completion calls, the helper obtains IDs only from
server `Connected` and `RoomUpdate` packets.
Normal item-index resynchronization therefore has no application-unconfirmed
Location outbox to replay.
The factory's other examined helpers do not subscribe to `CheckedLocationsUpdated`.

`RoomUpdate` raises the check-update event before its internal confirmation union.
Application observers must copy read-only data and post UI work without
reentrant helper calls or synchronous UI waits.
Observer exceptions receive diagnostics and cannot interrupt that union.
The journal records committed gameplay, not helper notifications.

An adverse callback can leave server-acknowledged IDs temporarily absent from
the helper's confirmation set.
Internal resynchronization can then send those acknowledged IDs again.
They award no new checks against unchanged authoritative server state.
This limited duplicate case does not excuse new application calls after
suppression or a helper seeded with unconfirmed local progress.
No unavailable raw-packet filter is assumed.

## Connection sequence and retirement

The owner installs startup observers before exposing a candidate.
Its first `SocketOpened` observer is a minimal, nonthrowing phase recorder.
The examined factory installs no earlier observer for that event.
The implementation must preserve this order.

The owner calls `session.Socket.ConnectAsync()` once for each explicit route.
It retains that actual task rather than only a session-level timeout waiter.
It separately observes RoomInfo.
The session constructor already installs its RoomInfo handler.
Only the current route can start login after both prerequisites complete.

`LoginAsync` can block in a synchronous send before returning its task.
The owner tracks the entire invocation and its returned task as one operation.
Neither the UI nor a receive callback waits synchronously for that operation.
Application completion sources use asynchronous continuations.
Successful authentication and contract validation precede publication.

A timeout revokes authority but does not retire underlying work.
The retired session is never reused.
Retirement establishes that the route cannot connect later.
It does not prove that every private worker or captured callback stopped.

| Evidence | Retirement result |
| --- | --- |
| No transport operation started | `NotStarted` |
| Actual explicit-route connect faults before opening, with reliable phase evidence and no login | `FailedBeforeOpen` |
| Actual connect and login settle, then actual awaited disconnect succeeds | `OrderlyClosed` |
| An opened, unreplaced route satisfies the terminal evidence below | `AlreadyTerminated` |
| Actual connect, login, or close remains pending | `Stopping`, identifying that operation |
| Completed work lacks sufficient phase or terminal evidence | `Stopping`, identifying the missing evidence rather than inventing active work |

An opened route needs settled connect/login operations and no pending
application close before classification as `AlreadyTerminated`.
All raw application disconnect calls belong to this owner.
One of these additional evidence paths must apply:

- Its captured helper close event follows the completed peer-response close
  path, and `Connected` is false.
- A terminal abort is positively established, not inferred from a generic error.
- A single actual close probe produces the typed terminal-state rejection
  described next.

On the examined .NET 7 implementation, close accepts `Open`, `CloseReceived`,
and `CloseSent`.
Prior observed opening and no socket replacement exclude `None` and `Connecting`.
Under those preconditions, `WebSocketException` with `WebSocketError.InvalidState`
identifies `Closed` or `Aborted`.
The classifier records this expected terminal rejection, not a successful handshake.
It cannot classify arbitrary exceptions or localized message text this way.
The supported runtime must establish this behavior during implementation acceptance.

Stock `DisconnectAsync` does not have a `Connected` guard.
It invokes the lower close operation even when that property is false.
An application no-op adds no evidence and is permitted only after retirement
is already established.
An actual pending close remains pending because false can mean `CloseSent`.
A handler error with true connectivity does not certify closure.

Private send workers can remain blocked without a public join interface.
Report-completion tasks are not the sole retirement gate.
Retirement still requires settled application-owned connect/login work and
positive transport evidence.
No idle timer can replace those facts.

## Bare-host convenience

| User input | Bounded route plan |
| --- | --- |
| Explicit WSS URI | One WSS route, without implicit downgrade |
| Explicit WS URI | One WS route |
| Bare host | At most WSS followed by WS, using fresh sessions and explicit URIs |

The owner preserves existing host, port, and supported endpoint parsing.
Only positive pre-opening WSS retirement permits the WS route.
The original request must still be active.
The same gate retires one route and reserves the next without an idle gap.

No fallback follows abandonment, an opened route, login refusal, invalid
contract data, or a hung connect.
A late failure after timeout cannot start WS.
Two settled failures end the request without an endpoint-history latch.
A later attempt requires another explicit user action.

The native unspecified-scheme path can discard a failed socket without disposal.
An awaited pre-opening failure nevertheless retires its connect operation.
Retained allocation does not imply live work.
Explicit routes provide the application-controlled boundary before fallback.

## Shared badge

Permanent suppression shows `NOT REPORTING` and takes precedence over connection loss.
Its details state that earlier submissions can still finish.
An eligible match without its validated connection shows `AP DISCONNECTED`.
The badge uses the admission authority, not an independent UI flag.
It is not a delivery receipt.
It coexists with `YOU ARE IN CHECK`.
Ordinary games do not acquire an APMW warning.

## Deterministic acceptance fixtures

These fixtures are requirements, not executed results.
Use barriers and controlled completions rather than timing sleeps.
The validated fixture binding has generation `fixture-seed`, game `fixture-game`,
team 1, slot 2, and published IDs exactly `{71001, 71002}`.
Its immutable validated hash tokens are `WHash`, `CHash`, and `CostHash`.
Matches A and B start eligible with that binding.
The endpoint is `wss://fixture.invalid:38281`; the port-only alternative uses 38282.

| Case and operation order | Required result |
| --- | --- |
| Prepare A/71001 before the gate, suppress A, then release the task | Zero admissions |
| Admit A/71001, pause before dequeue, suppress, then release | The earlier packet can finish. Its trace remains unsent until the lower send |
| Pause an admitted packet after serialization, then suppress | The same pre-admission exception applies |
| Begin the lower send before suppression | No recall promise |
| Admit a Location, pause goal admission, then suppress | Only the Location can finish |
| Admit both Location and goal before suppression | Both can finish |
| Fail an admitted packet after suppression, then reconnect | No retry or replay from A |
| Capture a two-packet replay snapshot, admit one, then suppress | The second packet is denied |
| Directly admit unacknowledged 71001, suppress, then trigger item-index mismatch | Sync remains available. No helper-local 71001 outbox exists |
| Acknowledge 71001 through RoomUpdate, attempt suppressed application reporting, then process item resync | Application admission is denied. Normal confirmation leaves no pending helper check |
| A throwing test observer interrupts confirmation after the server acknowledgment | Internal resend contains only that acknowledged ID. Diagnostics identify the observer failure |
| Reenter empty helper completion from a test observer | Demonstrates prohibited application behavior, not a supported bypass |
| Seed unconfirmed 71002 through the old helper path, suppress, then resync | Negative case: it can escape. Such a helper violates the fresh/no-seeding contract |
| Earn 71002 offline on a committed second capture, then Undo to count 1 | The earned ID remains. Speculative or rejected captures add no intent |
| Earn both IDs and goal offline, close the eligible window, then reconnect correctly | Replay needs no retained board or window |
| Suppress, restore controllers, Undo, close, then reconnect | No new admissions from A |
| Suppress A, then B independently earns the same ID | B can report. There is no ID-wide suppression or transfer of A's intent |
| Change only port, then separately change each identity/hash field | Port alone permits validated replay. Each binding mismatch blocks and retains the journal |
| Restart ChessV | No unsent journal is restored |
| Duplicate Connect while the actual first connect remains pending | No second request or route starts |
| Actual WSS fails before opening, then the same endpoint is requested | Positive retirement permits a fresh attempt |
| Opening recorder runs, then a later startup observer throws | This is not pre-opening failure |
| Bare-host WSS fails before opening; the active request then uses WS | Routes do not overlap. Helpers are fresh |
| Revoke the request after WSS failure but before WS reservation | No WS route starts |
| Hold WSS indefinitely and expire only its waiter | Remain `Stopping`. No fallback or replacement |
| Let that revoked WSS route open later | No login or publication. Complete its retirement |
| Both explicit routes fail before opening | End the request. A fresh user request can retry immediately |
| Refuse login, complete retirement, then submit corrected credentials | No historical endpoint lockout |
| Settle connect/login, complete peer closure, observe its captured close event and false connectivity | `AlreadyTerminated` permits retry without another handshake |
| Abort the settled opened route, then obtain typed terminal rejection from the single close probe | `AlreadyTerminated`, subject to runtime proof |
| Return a handler error while still open, an arbitrary close exception, or false during a pending close | None independently certifies retirement |
| Open successfully but never receive RoomInfo | No login or WS fallback. Retire the opened route |
| Hold login's synchronous send or the actual close task | Name the outstanding operation and remain `Stopping` |
| Deliver old item, close, DeathLink, UI, and reporting callbacks after replacement | No wrong-owner effects, match termination, or ledger loss |

## Source and remaining proof

MultiClient.Net source is pinned at
`063dfa5ad14bafbfe646bd67d54deb1a8fa2f036`.
Relevant files are `ArchipelagoSession.cs`, the system WebSocket helpers,
`LocationCheckHelper.cs`, and `ReceivedItemsHelper.cs`.
The terminal-state classifier additionally uses .NET runtime reference
source at `v7.0.20`, not a claim about an inspected installed runtime.

The retained detailed report, citations, and source copies are at:
`C:\Users\aaedi\.copilot\session-state\d5831281-9939-4587-89ea-3f2755590c92\files\reconnect-contract-seams.md`.

Implementation acceptance must prove the actual package/runtime behavior,
opening-observer order, atomic admission, helper containment, retirement
classification, and these fixtures.
No further product choice blocks this bounded design.
The broader settings plan remains open, and no execution handoff is published.
