# Stop APMW progress reporting after controller changes

Status: accepted
Date: 2026-09-17

## Decision

Allow computer takeover in APMW.
A user-initiated change of player control during a match permanently
disables new Archipelago progress submissions from that match.
The game window can continue local play.

At Q38, the user requested computer takeover while disconnecting that
window from emitting Locations.
At Q39, the user selected:

> Permanently disable progress reporting for that match (Recommended)

The selected proposal includes individual Locations and final Archipelago
goal completion.
Restoring the original controllers does not restore reporting.

## Boundaries

The reporting state belongs to the match, not to the AP session.
The session stays connected and can continue receiving items.
A fresh normally initialized APMW match can report progress.
Undo, history traversal, or changing controllers again does not turn the
existing non-reporting match into a fresh match.

Already awarded checks remain complete.
Reports admitted to the network library before takeover can still finish.
The transition does not retract those reports or require their cancellation.
Events from the non-reporting match must not be attributed to a later match.

Takeover does not itself send or simulate DeathLink.
It does not end the local game.
The similarity to DeathLink concerns reporting suppression, not the event's
meaning.
Represent the reporting state separately from a record of actual DeathLink
events.

Commands that indirectly change control, including the current Stop
Thinking implementation, cross the same boundary.
A search cancellation that does not change control is a different operation.
Normal controller assignment during fresh-match setup is not takeover.

## Q45: Library-admission cutoff

The user selected:

> Allow pre-takeover submissions to finish (Recommended)

This includes reports still pending inside the library's private queue.
Library admission is not a completed network send or server acceptance.
Packets in that queue are not necessarily in flight.
The selected cutoff replaces the earlier requirement to suppress every
unsent report until the underlying transport send begins.

Application-held work remains subject to permanent match suppression.
After takeover, neither a delayed callback nor journal replay can make a
new submission for that match.
The same boundary applies to Locations and final-goal status.
No dependency fork or transport extension is selected for this cutoff.

## Implementation requirements

The state is a one-way transition from reporting to non-reporting within
the match lifetime.
It is not part of the reversible capture ledger.
Apply the transition before the changed controller can produce progress.
Guard all progress paths, including terminal victory and queued dispatch.

Capture the originating match with application-held reports.
Recheck its eligibility at library admission, not a later singleton's current match.
Serialize that admission and permanent suppression through the same owner.
A delayed wrapper invocation is not proof of library admission.
Do not hold the owner lock while awaiting network completion.
Reports already admitted before suppression can finish through the library.

The UI must make the match's non-reporting state visible.
Do not present takeover as an AP connection failure or a lost match.
The warning must not promise cancellation of earlier submissions.
Exact labels and internal data structures remain engineering choices.

## Source

[Interaction-support record](../../.scratch/large-board-enemy-armies/issues/21-define-apmw-interaction-support.md)
records Q38 and Q39.
[Connection-lifecycle record](../../.scratch/large-board-enemy-armies/issues/23-specify-connection-lifecycle.md#q45-reporting-cutoff)
records the selected Q45 cutoff and its library constraints.
[ADR 0019](0019-support-reliable-apmw-undo-in-zero-four.md)
separately specifies reversible local Undo.
This design does not authorize production implementation.
