# Specify the APMW connection lifecycle

Type: grilling
Status: resolved
Blocked by: 17, 21

## Question

How does the client allow safe repeated connection attempts without
overlapping sessions, stale callbacks, permanent reconnect lockout, or
cross-room journal replay?

## Author direction

The user described an old reconnect guard with a message resembling
"Reconnect prevented. Restart your client."
They asked to revisit its simple state machine using Git history and
MultiClient.Net usage in other projects.

The desired distinction is actual connection state:
disconnected, connecting, connected, or still cleaning up.
The user does not want a historical flag to require restart after the
client is truly disconnected and no connection attempt remains active.

The user offered 15 or more idle seconds as an example and explicitly
called that conservative.
This does not select a 15-second timer or permit clearing a live attempt
because it has produced no recent output.

The work remains design research.
There is no authorization to change production connection behavior.

## Settled dependencies

- The check journal exists only in process memory.
  It can outlive a game window but not the ChessV process.
- Q43 applies that same lifetime to the earned final-goal marker.
- A transport interruption must not erase an ongoing match's accounting
  state or the earned-check journal.
- Reconnect can replay eligible earned checks to the verified same world
  and slot.
- A controller change permanently stops that match's progress reporting.
  New session objects cannot clear that restriction.
- Q45 permits reports admitted to the library before takeover to finish.
  New application submissions and journal replay remain permanently blocked.
- Address and port are transport coordinates, not sufficient room identity.
- FEN-based APMW resume remains unsupported.

## Independent research

One source/history investigation owns the local guard, attempt lifecycle,
cleanup, callbacks, and introducing commits.
One primary-source web investigation owns MultiClient.Net 6.6.0 behavior,
maintained usage patterns, and available room identity.
Neither report is a runtime reproduction or an implementation fix.

### Local source and history result

The [local guard report](../research/reconnect-guard-history.md) is complete.
The same-server guard originated in commit
`59ce62f926315298253c7c0b4ae6eb5bcb2f4185`, dated 2023-10-26, with the subject
"prevent double taps".
The commit records a LocationHandler socket problem but explicitly leaves
its cause unknown.

The current guard blocks an incomplete connection task.
It also blocks a repeated endpoint/slot even after a login failure has
completed.
That failure path does not clear the endpoint latch, so a corrected-password
retry can be refused.
This is a source-proven outcome, not a reproduced race.

A handled close of the current session does clear the endpoint.
Its disposal also unloads match accounting, which conflicts with the
selected reconnect journal and ongoing-match lifetime.
`LocationHandler.Initialized` remains true and cannot serve as a live
reporting-availability test.

The proposed replacement is a single-flight state machine with owned
attempt generations and separate cleanup.
State names and synchronization are engineering details.
The companion library investigation supplies the cancellation and cleanup
constraints that follow.

### Primary-library research result

The [MultiClient.Net report](../research/multiclient-reconnect-patterns.md)
is complete.
It uses the 6.6.0 tag at
`063dfa5ad14bafbfe646bd67d54deb1a8fa2f036`, not current documentation or the
older XML cached in the local checkout.

Its important findings are:

- A login refusal can leave the transport open.
- A login timeout does not necessarily cancel the underlying connection.
- The public 6.6.0 interfaces have no caller cancellation token, `Abort`,
  or `Dispose`.
- Awaited disconnect of a known-open socket establishes orderly transport
  closure, not the absence of later callbacks.
- Fresh sessions isolate helper caches, but do not retire an old session.
- The Location helper can resend pending checks during item
  resynchronization. That path has no match identity or takeover state.
- AP exposes a generation name and authenticated team/slot, not a universal
  room-instance UUID. Generation names can be reused.

These are source-backed constraints, not runtime failure diagnoses.
Other clients supply useful patterns, but their retry timing and cleanup
implementations are not library guarantees.

## Proposed lifecycle contract

One coordinator owns connection state and a monotonically changing attempt
identity.
The candidate session is not the published authenticated connection.

| State | Event | Required transition |
| --- | --- | --- |
| Disconnected | Explicit Connect | Create one candidate and enter Connecting |
| Connecting | Duplicate request | Do not create a second attempt |
| Connecting | Successful login and identity/contract validation | Publish only the current attempt; enter Connected |
| Connecting | Failure, refusal, exception, or cancellation | Revoke the attempt's authority and enter Stopping |
| Connected | Request for the existing live connection | Keep the current connection without another login |
| Connected | Transport loss or explicit replacement | Block dispatch and enter Stopping; retain match records |
| Stopping | Another explicit Connect request | Start no transport until retirement is complete |
| Stopping | Attempt ended and transport retired | Enter Disconnected; a new explicit attempt is permitted |
| Any state | Callback from an obsolete owner | Do not publish, dispose the current owner, mutate another match, or report progress |

Whether duplicate explicit requests are coalesced or return an
already-connecting message is an engineering UI choice.
The essential property is single-flight ownership.
No automatic retry loop is selected.
The old same-endpoint latch is not part of the target design.

### Retirement boundary

A failed attempt can retry the same endpoint after cleanup, including with
corrected credentials.
No arbitrary idle delay or process restart is required for that settled
case.

Elapsed time, a wrapper timeout, or one false `Socket.Connected` sample does
not prove that an old attempt cannot finish later.
Stale-owner checks remain necessary after disconnect.

The implementation must distinguish actual operation settlement from a
timeout of its waiter.
A genuinely unresolved attempt remains in `Stopping`.
The selected policy does not promise bounded abort of every hung operation.
Extra abort capability, a package change, and overlapping replacement
transports are not authorized workarounds.

### Match and transport lifetimes

Connection cleanup removes transport subscriptions and session helpers.
It does not clear the ongoing match's role/capture ledger or process-owned
earned checks, and must not synthesize a resignation or DeathLink.

Every callback carries its originating attempt/session identity.
Progress additionally carries its originating match and world/slot binding.
The same gate supplies the badge and dispatch decision.
An active connection to another room is unavailable for this match's
reporting, even though the AP lobby itself is connected.

### Progress dispatch boundary

Application-owned journals remain the source of earned checks.
The library's pending-Location cache cannot become an unguarded second
outbox after takeover.

An engineering option is to send explicit Location-check packets through a
match-aware adapter without seeding unconfirmed checks into the library
helper.
The [connection/reporting contract](../contracts/apmw-connection-reporting.md)
specifies the stock adapter and its acceptance.
The adapter is not implemented.
Its fixtures cover internal item resync, ordinary sends, and journal replay.
Already sent or accepted packets are not recalled.
Q45 also permits packets admitted before takeover to finish from the
library's private queue.
Application-held work must pass the originating match's gate at library admission.
One owner serializes admission and permanent suppression.

### Selected-asset and queue findings

The delegated connection design examined the existing package resolution
for both `APMW.Client` and `ChessV.GUI`.
Both select MultiClient.Net 6.6.0's `lib/net6.0` asset under
`net7.0-windows7.0`.
This is inspection of existing assets, not a new restore.

The public send methods put packets into a private socket queue.
A private worker later removes, batches, serializes, and sends those packets.
The public API has no filter or cancellation point between admission and
the underlying send.
The `PacketsSent` event occurs too late to suppress that send.
Thus direct Location packets address the pending helper-cache problem, not
strict cancellation inside the private queue.
Q45 permits earlier queue admissions to finish, so that stricter capability
is not required.
The bounded contract now specifies synchronous queue admission under the
owner gate and containment of the helper's confirmed duplicates.

The factory has no socket-injection overload.
The session and socket constructors and underlying socket field are
internal.
The send methods are not overridable hooks.
A forwarding wrapper cannot intercept the socket already owned by the
library helpers.

Primary evidence is the pinned 6.6.0 source:
[socket queue](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/Helpers/BaseArchipelagoSocketHelper_system.net.websockets.cs#L233-L340),
[factory](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/063dfa5ad14bafbfe646bd67d54deb1a8fa2f036/Archipelago.MultiClient.Net/ArchipelagoSessionFactory.cs#L18-L60).

The full proposal and its fixtures remain in the child-session artifact:
`C:\Users\aaedi\.copilot\session-state\d5831281-9939-4587-89ea-3f2755590c92\files\reconnect-contract-seams.md`.
The child retains that artifact.
No runtime validation or dependency modification occurred.

### Stock retirement outcomes

The source-derived stock route directly tracks
`session.Socket.ConnectAsync()`, rather than only the session-level waiter.
It also tracks the full login invocation, which can block in a synchronous
send before it returns its task.

| Evidence | Result |
| --- | --- |
| Actual connect fails before opening, explicit URI, no socket replacement, no login or polling starts | `FailedBeforeOpen` permits another explicit attempt after owner revocation |
| Actual connect and login settle, and actual awaited disconnect succeeds | `OrderlyClosed` permits another attempt. The initial connectivity sample need not be true |
| An opened, unreplaced route has settled operations plus positive terminal evidence | `AlreadyTerminated` permits another attempt without a redundant close handshake |
| Only a waiter times out, or actual connect remains active | Remain in `Stopping`; no replacement transport |
| The connect helper faults but the failure phase is unknown | `UnknownFailurePhase`; report the missing evidence rather than asserting that a task is still active |

A helper task can fault after the socket opens, including through a
throwing `SocketOpened` handler.
Task status alone does not establish `FailedBeforeOpen`.
The future implementation must supply reliable phase evidence.

An already-closed route needs combined evidence, not a second successful handshake.
The bounded contract specifies the peer-close event path and a typed
terminal-state probe for an already-opened, settled route.
A false connectivity sample, generic error, or application cleanup no-op
does not supply that evidence.
Private worker and callback quiescence remain separate from retirement.

Unspecified-scheme fallback can discard an earlier socket without disposal.
An awaited pre-opening failure nevertheless retires that connect operation.
The earlier inference from retained allocation to live work was too strong.
The bounded contract instead uses fresh explicit WSS and WS routes.
Bare-host fallback requires positive pre-opening retirement and a still-active
user request.
It is not automatic reconnection.
These are source-derived requirements, not executed acceptance.

### Q45: Reporting cutoff

The user selected:

> Allow pre-takeover submissions to finish (Recommended)

This replaces ADR 0020's earlier strict wording for unsent queued reports.
The alternatives and outcome are:

| Alternative | Meaning and consequence |
| --- | --- |
| Library-admission cutoff, selected | Reports admitted before takeover can finish, including library-queued packets. No new application submission or journal replay from that match is permitted |
| Underlying-send cutoff, not selected | Even admitted or serialized packets remain suppressible until the real transport send begins. Stock 6.6.0 has no public mechanism for this |

Library admission does not mean that a packet is physically sent, in flight,
or accepted by the server.
Application-held work remains subject to its originating match's gate.
The adapter must serialize actual admission and takeover through one owner.
A wrapper scheduled before takeover does not establish earlier admission.
The owner must not hold its lock while awaiting network completion.

The stricter alternative prompted a proposed factory injection extension
and an application-owned transport.
Q45 does not select that extension or a dependency fork.
The stock-package design is the current engineering direction.

Guaranteed bounded abort of every hung attempt is not a separate user
requirement.
The selected lifecycle allows retry after genuine retirement.
A truly unresolved operation remains in `Stopping`.
The absence of a public abort method does not invalidate ordinary retries
after settled failures and completed closure.
No timer can manufacture retirement evidence.

Q45 is resolved.
The delegated design specifies the stock admission boundary, helper
duplicates, and transport retirement under that decision.
The bounded contract preserves those details and acceptance fixtures.

## Q42: Selected replay identity

The normal AP identity is the generation name plus authenticated team and
slot.
The client must also verify the game and the match's original contract,
cost snapshot, and published Location set.
Neither port nor client UUID is world identity.

Two independently generated worlds can reuse a generation name.
Identical visible metadata cannot prove that those worlds are different.
The protocol also cannot distinguish separate servers hosting copies of
one generated world as different live room instances.

The user selected:

> Use AP generation name, team/slot, and frozen-contract checks without a new identifier

This rejects the proposed additional generated-world marker.
The client uses normal AP identity with the documented collision limit.
It does not require new producer fields or alter the artifact hash graph.
[ADR 0021](../../../docs/adr/0021-replay-earned-locations-after-reconnect.md)
records the selected boundary.

| Evidence | Required comparison before replay |
| --- | --- |
| Generation name | Exact, nonempty match to the retained AP generation name |
| Team and slot | Exact match to authenticated coordinates, not request text alone |
| Game | Match the original game |
| World binding | Validate it and match its semantic hash to the retained binding |
| Shared contract and cost snapshot | Validate both and match their hashes to the retained originals |
| Published Location set | Remain the original set bound by the World binding; send no IDs outside it |

Missing evidence or a mismatch blocks replay.
It does not discard the journal or change permanent match eligibility.
Another valid connection can exist without permission to send this match's
progress.
Changed address or port alone neither rejects nor authorizes replay.

Two generations with equal compared values remain indistinguishable under
the selected policy.
The specification must not claim collision-proof world identity or unique
server-instance identity.

## Specified integration obligations

Define the connection states, valid transitions, attempt ownership, and
cleanup completion boundary.
Prevent duplicate attempts while permitting a later retry after a completed
failure or disconnection.
An obsolete attempt or session callback must not replace or dispose the
current one.

Define the verified identity needed before a retained journal can replay.
Do not equate same port with same room, or a different port with a different
world.
Do not invent a stable room identifier that the protocol does not provide.

Keep session teardown separate from game-window closure, match
disqualification, and application exit.
The reporting badge and dispatch gate must observe the same resulting state.

### Required lifecycle evidence

The later implementation must exercise actual attempt ownership through a
controllable session/transport seam.
The reports provide proposed inputs, not executed acceptance.

| Case | Required result |
| --- | --- |
| Repeated Connect from button, Enter, or GUI timing | At most one candidate can connect |
| Refused login followed by corrected credentials | Retry works after retirement without the historical endpoint latch |
| A wrapper timeout followed by late completion | No premature second connection or stale publication |
| Duplicate close/error callbacks | Cleanup is idempotent and affects only their owner |
| Old item, close, DeathLink, or UI callback after replacement | No mutation of the replacement session or another match |
| Connection loss during an ongoing match | Preserve the ledger and journal; no synthetic match termination |
| Controller takeover before application admission or journal replay | No new submission from that match enters the library |
| Controller takeover after library admission but before a physical send | The earlier submission can finish. The implementation does not claim recall or cancellation |
| Helper item resync after takeover | No application-unconfirmed journal entries enter a separate helper outbox. Classify confirmed duplicates explicitly |
| Same verified world on another port | Eligible journal can replay after validation |
| Changed generation name, authenticated team/slot, game, or frozen binding on the old port | Old journals remain retained but unsent |
| Different generation with identical compared metadata | The gate cannot distinguish it; document this accepted identity limit |
| Window closure while ChessV stays open | Retain eligible earned checks without retaining the board |

Automatic retry scheduling, new credential persistence, and a durable
journal format are not selected by this record.

## Answer

Q42 settles replay identity without a new producer identifier.
Q43 settles final-goal retention with the process-owned Location journal.
The user-facing lifecycle boundaries are selected.

The [bounded engineering contract](../contracts/apmw-connection-reporting.md)
completes ownership, stock library admission, helper containment, and
transport retirement.
It includes already-terminated routes, bounded bare-host fallback, and
genuinely unresolved operations.

The queue investigation rules out strict cancellation through stock public
APIs.
Q45 instead permits earlier admissions to finish.
No transport fork or hard-abort requirement follows.
The design is source-backed, not runtime-proven.
Implementation still owes the package/runtime fixtures and integration evidence.
This decision record is resolved for its bounded scope.
No execution handoff or production implementation is authorized.
They do not reopen the army, calibration, or shared-schema decisions.
