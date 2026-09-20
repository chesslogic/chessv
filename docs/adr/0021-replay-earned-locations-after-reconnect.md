# Replay earned Locations after reconnect

Status: accepted
Date: 2026-09-17

## Decision

Keep a set of Location IDs earned by each reporting-eligible APMW match.
After reconnecting to the same world and slot, resend that set.
Repeated reports are acceptable because the server deduplicates Location
checks.

At Q40, the user requested replay of all Locations earned by the game and
proposed accumulating them in a set.
This selects recovery after a connection interruption rather than requiring
a new match solely because transport was lost.

## Distinct state

The reversible move ledger stores current match progress.
The earned-Location set stores historical accomplishments.
Undo restores the former but does not remove entries from the latter.
This preserves the same accomplishment semantics whether delivery occurs
immediately or after reconnect.

Only successful committed gameplay contributes new accomplishments.
Read-only history, speculative moves, and rejected moves do not.
Use published Location IDs from the match's bound profile.
Do not reconstruct the set from the final board or latest counter totals.

A transport interruption preserves the match and its accounting state.
It is not equivalent to ending the match or starting a new one.
Keep earning checks during the interruption while the match remains
reporting-eligible, then resend the accumulated set after verified recovery.

## Reporting gates

Bind the set to its originating match and authenticated world/slot context.
A reconnect to another world or slot cannot flush it.
A compatible artifact schema alone does not establish the same world.

[ADR 0020](0020-stop-apmw-progress-after-controller-changes.md) still applies.
After a controller change, the match cannot submit its accumulated set again.
Restoring the connection, the original controllers, or an earlier board
position does not restore reporting eligibility.
Under Q45, reports admitted to the library before takeover can still finish.
This includes queued reports that have not reached the transport.
Application-held work must pass the match gate at actual library admission.
Earlier admission does not authorize a later journal replay.

The server's deduplication removes the need for an exactly-once delivery
protocol.
It does not remove client-side eligibility, identity, or published-ID
validation.

## Q41: Process-memory lifetime

Keep the earned-check journal in ChessV process memory.
Closing its game window does not discard the journal while the client
remains open.
Do not introduce a durable file format for it.
Restarting ChessV does not restore unsent journals from disk.
Checks already accepted by the server remain complete.

The user rejected file persistence because room identity is not equivalent
to an address or port.
Process-memory retention still requires verified world/slot binding.
Port reuse must not send an old journal to another room.
A port change alone does not prove that a world changed.

Retain each journal's match identity and reporting restriction after its
window closes.
A journal from a permanently non-reporting match cannot become eligible
merely because its window no longer exists.
Retaining earned checks does not save or resume the chess position.

## Q42: Standard AP identity

The user selected AP generation name, authenticated team/slot, and
frozen-contract checks without a new generated-world identifier.
Do not add a generation UUID or another slot-data field for journal identity.

Replay requires the original nonempty generation name, authenticated team
and slot, and game.
The validated World binding, shared contract, and cost snapshot must match
the match's retained originals.
This includes the original published Location set.
Missing identity evidence or a mismatch blocks replay and preserves the
journal.
Address, port, and client UUID do not replace these checks.

This decision accepts normal AP generation-name identity, not guaranteed
distinction between all independent generations or server instances.
If two generations reuse all compared identity and contract values, the
client cannot distinguish them.
Copies of the same generated world also remain indistinguishable.
The rejected additional identifier would strengthen generation identity,
but would not distinguish servers hosting copies of that world.

## Q43: Final-goal retention

The user selected retention of the final-goal marker with the Location
journal.
An eligible match records final AP goal completion separately from its
earned Location IDs.
The marker is not a synthetic Location.

The marker survives transport loss, session replacement, and game-window
closure while ChessV remains open.
It does not survive process exit or restart.
After verified recovery, resend the earned goal status through the same
identity and permanent-eligibility gates as Location checks.
Undo does not retract the accomplishment.
Controller takeover blocks new goal submissions, including delayed replay.
An earlier goal submission can still finish under Q45.

The examined server treats repeated goal status as idempotent after the
first completion.
The journal must not treat a completed send task as server acceptance or
discard the marker on that basis.
This is retained reporting intent, not a saved chess position.

## Evidence and related lifecycle work

The examined server calculates new checks as reported IDs minus previously
checked IDs, then intersects them with the slot's Locations.
Source: `C:\GitHub\rft50-checksmate\MultiServer.py:1170-1204`.
The goal-status handler preserves an already completed goal:
`MultiServer.py:2290-2303`.

This ADR does not select a full game-save format or permit FEN resume.

[Ticket 22](../../.scratch/large-board-enemy-armies/issues/22-specify-apmw-reporting-status.md)
owns the badge, journal lifetime, and final-goal delivery details.
[Ticket 23](../../.scratch/large-board-enemy-armies/issues/23-specify-connection-lifecycle.md)
owns the connection state machine and safe reattachment identity.
No production implementation is authorized here.
