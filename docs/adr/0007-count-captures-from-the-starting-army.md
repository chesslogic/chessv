# Count captures from the starting army

Status: accepted
Date: 2026-09-14

Capture counters use starting roles rather than current piece kinds or ranks.
CPU King captures count only when the starting army has multiple Kings.
This keeps promotion and survival-phase changes from changing counter
eligibility during a match.

## Regicide

Regicide is one Location for capturing either CPU King in a multi-King setup.
Checkmate without a King capture does not complete it. A one-King starting
setup neither offers Regicide nor counts its King in the capture series.

A Checkers move that captures both Kings completes Regicide once and adds two
non-pawn captures. Eligibility follows the starting setup, not an intermediate
or final surviving-King count.

## Capture Everything

Capture Everything requires every starting non-King unit and all spare King
lives. At most the last King can remain. A captured King cannot substitute
for an uncaptured pawn or other non-King unit.

The clear must occur within one match on the world's configured ending
geometry. Clearing an earlier board does not complete this Location.

Thus, 12x10 requires all fourteen pawns, all eighteen non-King pieces, and at
least one of its two Kings. Either King can be the survivor. Capturing both
Kings still requires every non-King unit to be gone.

## Consequences

The shared starting formation supplies pawn, non-King-piece, and King counts.
Board width alone cannot supply these counts for the augmented armies.

The series retain every integer threshold from 2 through the endpoint's
ceiling. The normal clear excludes only the last King. The Any series ends
one capture before that clear. The owning ticket contains the exact table.

A published count goal can complete on an earlier board that meets its count.
For example, Capture Any 15 can complete on 8x8 in a larger world.
Capture Everything still requires the configured ending geometry.

Counters can exceed the catalog ceilings during a two-King capture. The
client reports every crossed published threshold, not new unnamed thresholds.

## Source

[Semantic Location identity](../../.scratch/large-board-enemy-armies/issues/15-define-location-role-identity.md#decisions-in-progress).
The user selected these rules in the 2026-09-14 capture interview.
