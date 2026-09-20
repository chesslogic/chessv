# Specify APMW reporting status and reconnect

Type: grilling
Status: resolved
Blocked by: 21, 23

## Question

How does an APMW game window show that it cannot report Locations, and
what recovery does each reporting state permit?

## User direction

The user proposed a badge similar to the existing check indicator when a
window or server connection is disconnected and will not send Locations.
This record specifies that presentation without authorizing production UI
changes.

## Existing source

The existing `label_check` is a top-right label with a Firebrick background
and light text.
Its literal text is `YOU ARE IN CHECK`.
Sources: `ChessV.GUI\Forms\GameForm.Designer.cs:769-780` and
`ChessV.GUI\Forms\GameForm.cs:194-205`.

Transport loss and permanent match disqualification are different facts.
Current socket closure calls `Dispose`, which unloads the match, removes
the item handler, and resets connection state.
Source: `APMW.Client\Client.cs:298-351`.
`LocationHandler.EndMatch` also clears its capture ledger.
Current code therefore does not establish automatic same-match recovery.

`TryValidatePlayingArchipelago` returns the historical `Initialized` flag.
It is not a complete live connection or match-reporting gate.
Source: `APMW.Client\LocationHandler.cs:820-827`.
The new badge must not infer reporting availability from that flag alone.

## Proposed badge contract

The badge is persistent while the APMW window cannot report progress.
It follows the same authoritative state used to permit or suppress reports.
The UI must not maintain an independent eligibility flag.

| Condition | Proposed text | Meaning |
| --- | --- | --- |
| Reporting-eligible match with a valid active connection | No warning badge | Progress reporting is available |
| Controller change permanently disabled match reporting | `NOT REPORTING` | New submissions from this match are disabled. Earlier library submissions can still finish |
| No validated connection for this match, without prior permanent disqualification | `AP DISCONNECTED` | Checks accumulate for replay after verified same-world/slot reconnection |
| Permanent disqualification and connection loss together | `NOT REPORTING`, with both causes available in details | Reconnection cannot remove the permanent match restriction |

The labels are engineering proposals, not a new wire enum.
Use the existing indicator style, but do not replace or obscure the check
warning.
Both warnings can be visible together.
Cause and recovery text must be available without relying on color alone.
Ordinary-game windows do not acquire an APMW warning merely because no AP
session exists.

The badge is not a delivery receipt.
It cannot promise that an in-flight report has been accepted by the server.
Already accepted checks remain complete.
Under Q45, earlier submissions can also finish after the warning appears.
Suggested detail text: "New reports from this match are disabled. Reports
submitted earlier can still complete."
If the lobby is connected to a different world, the badge details must
explain that this particular match lacks its original connection.

## Q40: Reconnect with the earned-Location set

Q39 permanently disables reporting after controller changes.
Reconnection must never clear that state.

The user requested replay of all Locations earned by the game:

> it's basically just adding everything this game to a set each time

[ADR 0021](../../../docs/adr/0021-replay-earned-locations-after-reconnect.md)
records the selected recovery model.
This is replay of earned IDs, not replay of chess moves.
No FEN restoration is involved.

The server already deduplicates received Location IDs.
`MultiServer.py:1170-1204` subtracts checked IDs and filters to the slot's
published Locations before granting new items.
The client still must bind its journal to the correct match, world, and
slot.

### Small data model

| State | Behavior |
| --- | --- |
| Reversible move ledger | Tracks the current continuation's counters and original-unit mappings. Undo restores it |
| Earned Location ID set | Accumulates accomplishments from successful committed gameplay. Undo does not remove earned IDs |
| Earned-goal marker | Records achieved final AP goal completion separately. It has the same retention and replay gates as the earned Location set |
| Permanent non-reporting state | Set by controller changes. Prevents new submissions or replay, but permits earlier library submissions to finish |
| Connection/world binding | Determines whether the eligible match can send now and where its journal belongs |

Transport loss must not clear the move ledger or earned set.
An eligible match can continue earning while disconnected.
On verified same-world/slot reconnect, send its earned set, including IDs
already delivered.
No independent acknowledgment ledger is required for correctness.
Do not clear earned IDs merely because one send attempt returned.

Goal completion uses a separate status message in current code:
`APMW.Client\LocationHandler.cs:774-788`.
Q43 selects an earned-goal marker beside the Location set
and replay through the same eligibility and world-binding gates.
Do not invent a synthetic Location ID for that marker.
The server does not undo or re-award a completed goal on repeated status:
`MultiServer.py:2290-2303`.

The badge must distinguish retained local accomplishments from server
acceptance.
`AP DISCONNECTED` means the client cannot currently send; it does not mean
that local progress is discarded.

## Q41: Journal lifetime

The user rejected a journal file format and selected retention after a game
window closes while the ChessV client remains open.
The journal therefore belongs to process-level reporting state, not the
window or network session object.

| Event | Journal behavior |
| --- | --- |
| Transport disconnect or replacement session object | Retain the journal and its original binding |
| Game window closes while ChessV remains open | Retain earned checks for later delivery |
| Reconnect to the verified same world and slot | Replay an eligible journal, including a closed window's journal |
| Reconnect to another room or slot | Keep the old journal separate; do not send it there |
| A permanently non-reporting window closes | Preserve its ineligibility; do not make its journal reportable |
| ChessV process exits or restarts | No journal-file recovery. Server-accepted checks remain complete |

No chess position is saved or resumed by retaining earned checks.
The set must not be merged into a process-global unqualified bag of IDs.
It retains its originating match and authenticated world/slot context.
Q42 selects AP generation name and authenticated team/slot plus
frozen-contract checks, without a new identifier.
[Ticket 23](23-specify-connection-lifecycle.md) supplies the exact comparison
and the accepted identity limit.
Equal compared metadata cannot distinguish independent generations or
separate server instances.

### Q43: Final-goal retention

The user selected:

> Retain the final-goal marker with the Location journal (Recommended)

The marker survives game-window closure in process memory, just like the
Location set.
It does not survive process exit or restart.
It retains the originating match, world binding, and permanent reporting
restriction.
The library's goal action does not retain that intent for the application.
The process-owned journal must retain it.

## Required acceptance

| Case | Required result |
| --- | --- |
| Earn checks, disconnect, earn more, and reconnect to the same world/slot | Preserve match accounting and resend the union of earned IDs |
| Reconnect repeatedly or send the same set twice | Each Location is granted once by the server |
| Undo a capture that earned a threshold while disconnected | Restore current counters but retain the earned threshold in the journal |
| Take over while disconnected, then reconnect | Do not flush that permanently non-reporting match's journal |
| Connect with different AP identity or frozen-contract values | Do not send the old match's journal there |
| A speculative, rejected, or view-only move resembles a new accomplishment | Do not add a new earned ID or goal marker |
| Reach the final goal offline in an eligible match, then reconnect | Replay the earned status through the same verified reporting gates |
| Show check and reporting warnings together | Neither badge obscures the other; details explain cause and recovery |
| A connection returns after permanent takeover | Continue showing the non-reporting state |
| A pre-takeover submission finishes after the warning appears | Keep the warning. Neither its text nor details promise cancellation of earlier submissions |
| Close a game window with pending earned checks, keep ChessV open, then reconnect correctly | Deliver the retained eligible journal without reconstructing the game |
| Earn the final goal offline in an eligible match, close its window, keep ChessV open, and reconnect correctly | Send the retained goal status and earned Location set without reconstructing the game |
| Replay that closed match's goal status twice against unchanged server state | Final AP goal completion remains complete without repeated goal side effects |
| A permanently non-reporting match wins offline, closes, and reconnects | Send neither its Location checks nor a final-goal status |
| Reuse the previous port for a different room | Do not replay the old journal based on endpoint equality |
| Reconnect to the same verified world on a changed port | Port inequality alone does not reject the journal's actual world identity |
| Restart ChessV | Do not read a journal file or claim recovery of unsent process-memory state |

These are planned acceptance cases, not executed results.

## Answer

Q40-Q43 settle replay, process-memory lifetime, normal AP identity, and
final-goal retention.
Q45 permits earlier library submissions to finish, without permitting new
submissions or replay after takeover.
The badge uses the same match-aware reporting gate as dispatch.
The proposed labels are engineering UI choices, not protocol values.

The product decisions and bounded engineering specification are complete.
The [connection/reporting contract](../contracts/apmw-connection-reporting.md)
supplies the shared admission authority and stock retirement boundaries.
This decision record is resolved.
No runtime behavior or implementation acceptance is claimed.
