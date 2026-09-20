# Define APMW interaction support

Type: grilling
Status: resolved
Blocked by: 14, 15, 18

## Question

Which player-facing position replacement, take-back, history, and
player-control operations are supported in APMW, and at which release?

This record supplements the resolved Q35 design.
It does not reopen the royal policy, capture identities, or internal
rollback requirements.
It does not own every world Option or a general anti-cheat system.

## Author decisions and limits

| Claim | Authority | Excludes |
| --- | --- | --- |
| FEN-based APMW resume is unsupported | User statement on 2026-09-16 and ADR 0018 | Treating an edited FEN as a supported continuation |
| Starting FEN in SGF is a possible recording feature | "It would be neat to attach a starting FEN to a SGF file" | A committed implementation requirement or permission to resume |
| Reliable Undo is in 0.4.0 scope | Q37: "Include reliable Undo support in 0.4.0 (Recommended)" | Blanket disabling as the selected plan, or deferral until 1.0.0 |
| Genuinely view-only traversal is acceptable | Q36 | Calling live committed undo/replay view-only merely because its UI shows history |
| Computer takeover is permitted, with Location reporting disconnected for that window | Q38 author response | A blanket prohibition on computer takeover, or continued Location reporting during takeover |
| A controller change permanently stops all progress reporting for that match | Q39 selected proposal | Re-enabling reporting after Undo or restored controllers, reporting final goal completion, or disconnecting the whole AP session |
| Reports admitted to the library before takeover can finish | Q45 selected proposal | New application submissions or journal replay after takeover, or treating queue admission as a completed send |
| Engine rollback and complete state restoration remain required | Tickets 14/15/18 and royal lifecycle fixtures | Disabling engine make/unmake because a menu command is disabled |

[ADR 0018](../../../docs/adr/0018-reject-fen-based-apmw-resume.md)
records the settled FEN boundary.
[ADR 0019](../../../docs/adr/0019-support-reliable-apmw-undo-in-zero-four.md)
records the selected Undo release scope.
[ADR 0020](../../../docs/adr/0020-stop-apmw-progress-after-controller-changes.md)
records the one-way reporting boundary after controller changes.

## Current command surfaces

These are source observations at `d14300c`, not new support decisions.
Paths are relative to the ChessV repository.

| Surface | Current operation | Evidence |
| --- | --- | --- |
| Take Back Move / Take Back All Moves | One or repeated `Game.UndoMove(true)` calls, with generic engine/review guards | `ChessV.GUI\Forms\GameForm.cs:446-478,511-516,612-620` |
| First / Previous / Next / Last / Stop history controls | Undo and replay on the same live Game. Replay uses committed moves | `ChessV.GUI\Forms\GameForm.cs:699-881`; `ChessV.Base\Game.cs:1590-1600,1736-1738` |
| Get or Set Position FEN | Apply can clear game state, load FEN, and replace the starting FEN | `ChessV.GUI\Forms\LoadFENForm.cs:68-77`; `ChessV.GUI\Forms\GameForm.cs:653-684` |
| SGF load | Create a game from saved variables, then replay saved moves | `ChessV.Manager\Manager.cs:233-257`; `ChessV.Base\Game.cs:729-744,2278-2287` |
| Computer Plays / Stop Thinking | Convert the player between human and internal engine control. Stop Thinking calls those conversion handlers | `ChessV.GUI\Forms\GameForm.cs:224-282,688-694` |
| Normal new APMW match | Generate a new match under connection and active-match guards | `ChessV.GUI\Forms\ApmwForm.cs:197-221,392-399` |

Hiding only Take Back Move leaves the all-moves and live history paths.
History has viewing intent, but its current implementation mutates the live
game and can invoke APMW callbacks.
A genuinely read-only viewer is a different implementation.

The SGF writer can already serialize a changed `FENStart`.
The reader applies that value as executable setup input.
This is not equivalent to inert starting-position metadata.
Sources: `ChessV.Base\Game.cs:216,729-744,2278-2287` and
`ChessV.Manager\SavedGameReader.cs:75-111`.

## Boundaries to preserve

- Internal search make/unmake, rejected-move rollback, and state restoration
  remain available and correct.
- A fixture that reconstructs a known role ledger does not authorize
  arbitrary player imports.
- Read-only move lists, board rotation, piece information, and export are
  not live take-back operations.
- Normal new matches remain distinct from resuming an edited position.
- Ordinary games, their save files, and their controls remain outside this
  APMW restriction.
- Disabling a button alone is insufficient if another command invokes the
  same unsupported operation.

## Q36 and Q37: Undo design and release scope

Q36 asked about the release policy for user-initiated take-back and the
current live history traversal.
The user did not select the proposed blanket 0.4.0 restriction.
Their response favors a smaller reversible-state change:

> If view-only backtracking is supported without really using the Undo Move tool, that's fine.

> The only change we would need to add to support Undo Move, I think, would be to remember the turns that we incremented certain capture counters, like Capture N Pawns, and decrement those if the user undoes those turns.

They suggested that this could be less invasive than changing the UI.
Genuinely view-only traversal is acceptable.
That statement does not classify the current live undo/replay toolbar as
view-only or commit to implementing a replacement viewer.

### Existing capture ledger

The proposed basic counter mechanism already exists in current source.

| Mechanism | Source |
| --- | --- |
| One `MoveDiff` stack entry stores pawn/piece capture deltas and previous origin mappings | `APMW.Client\LocationHandler.cs:91-113,244-248` |
| Captures increment both the total and active move delta | `APMW.Client\LocationHandler.cs:584-597` |
| A take-back pops the delta, subtracts both counters, and restores origin mappings | `APMW.Client\LocationHandler.cs:178-198` |
| Committed engine undo emits the handler's notification | `ChessV.Base\Game.cs:1772-1776` |
| Existing source fixtures cover capture, undo, and recapture without reaching Capture 2 | `APMW.Test\LocationHandlerUnitTests.cs:484-517` |

Those fixtures invoke the handler directly for take-back.
They do not establish the correctness of every live command, failed move,
multi-capture, terminal event, or new two-King transition.
This is source evidence, not a new claim of executed acceptance.

The next engineering step would extend and verify this ledger, not add a
second independent counter history.
Its entries must correspond to successful committed moves.
The new capture-role and King-life state must reverse with the same move.
Counter totals can derive Pieces, Any, and Of Each progress; each named
threshold does not need an independent mutable counter.

At Q37, the user selected:
"Include reliable Undo support in 0.4.0 (Recommended)".
The release includes the focused ledger and integration work.
The previous disable-by-default recommendation is not the selected policy.
No production implementation is authorized by this scope decision.

A later backtracking design must address retained Archipelago checks across
alternate continuations, not only restoring local counters.
The existing fixtures explicitly do not retract server-accepted checks.
This record does not choose delayed reporting or server-side retraction.

For example, undoing the second pawn capture restores the local count to 1.
An already accepted Capture 2 Pawns check remains complete.
A new continuation starts counting from 1, not from the abandoned branch's
total.
This follows the existing local-state/server-check distinction.

### Q38: Permit takeover without Location reporting

The user rejected the proposed fixed-controller restriction:

> I've seen people have some fun by having a computer take over, and I think that's quite justified (and it's probably kind of funny, although I haven't tried it), but we should disconnect the window from emitting locations (similar to a deathlink event).

Computer takeover is supported.
The affected game window must stop reporting Locations.
This is a match-local reporting boundary, not a requirement to disconnect
the Archipelago session or stop receiving items.
The DeathLink comparison describes reporting suppression; takeover does not
by itself assert that a DeathLink event occurred.

The new engineering state should represent match reporting eligibility,
not overload the meaning of a received DeathLink.
The existing `DeathlinkedMatches` guard is a source precedent, not a
complete implementation of this new policy.

At Q39, the user selected:
"Permanently disable progress reporting for that match (Recommended)".
The window emits neither new Location checks nor final AP goal completion.
Reporting stays disabled if the original controllers return.
A fresh normal match can report again while the AP session remains
connected.

Undo restores local gameplay state, but cannot reverse the reporting
disconnection.
Controller-changing commands, including the current Stop Thinking handler,
cross the same boundary.
An ordinary internal search cancellation does not inherently change control.

Already accepted checks are not retracted.
An event committed before takeover is distinct from an event generated
after the window loses reporting eligibility.
The later implementation must make that transition explicit rather than
race queued callbacks against a GUI checkbox.
An application-held batch rechecks its originating match at library admission.
It cannot report through a newly current match.

### Q45: Permit earlier library submissions to finish

The user selected:

> Allow pre-takeover submissions to finish (Recommended)

Reports admitted before takeover can finish, including packets still in the
library's private queue.
This replaces the earlier strict suppression requirement for all unsent
packets.
Library admission does not establish transport completion or server acceptance.
New submissions and journal replay remain permanently disabled for that match.
Takeover and library admission must pass through the same owner.
[ADR 0020](../../../docs/adr/0020-stop-apmw-progress-after-controller-changes.md)
records this distinction.

## Deferred recording idea

Starting-FEN metadata in SGF remains an optional later idea.
It does not follow automatically from a decision about Undo or takeover.
Its deferral does not block the selected interaction policy.

## Required Undo and history acceptance

These cases supplement the existing royal lifecycle matrix.

| Case | Required result |
| --- | --- |
| Capture one pawn, invoke engine Undo, and recapture it | Local pawn total is 1, not 2. No new Capture 2 or Any 2 completion from repetition |
| Capture a piece, undo an intervening non-capture move, then undo the capture | The zero-capture move has its own history boundary. Only undoing the capture removes its contribution |
| One committed move captures multiple units | One ledger entry reverses the entire capture batch, including eligible King captures |
| Undo, then make a different legal continuation | Counts and identities follow the new branch. Abandoned deltas do not carry forward |
| Reject a move after setup observers run | No committed ledger entry or successful report survives. The next Undo still corresponds to the previous successful move |
| Undo a promoted starting pawn or either CPU King capture | Restore starting identity, counts, royal phase, and original castling actors together |
| Repeat Undo through the supported take-back commands | Restore each successful move exactly once, including moves with no captures |
| Traverse a path presented as view-only, then return | Active continuation, ledger, and gameplay reporting are unchanged |
| Undo after a capture Location has been accepted | Restore local state without retracting that accepted check |

Finalized-match restrictions remain separate from the supported live Undo
path.
No fixture requires reopening a finalized match or retracting DeathLink.
Read-only history must not masquerade as newly committed gameplay.

## Required takeover acceptance

| Case | Required result |
| --- | --- |
| Computer takes over after a normally initialized APMW match starts | Local play continues. New Location and final-goal submissions stop before the changed controller plays |
| Original controllers return | Reporting remains disabled |
| Undo before or after takeover, then play another continuation | The capture ledger is correct, but reporting remains disabled |
| Current Stop Thinking changes a controller | Apply the same reporting transition; the command label cannot bypass it |
| A non-reporting match reaches a new capture threshold or wins | Neither accomplishment creates a Location or final-goal submission |
| Application-held work remains pending when reporting is disabled | Its originating match prevents library admission |
| A report entered the library before the transition but remains queued there | It can finish under Q45. No cancellation or recall is promised |
| A report was sent or accepted before the transition | Do not retract it or claim that it was recalled |
| Takeover races library admission | One owner orders both operations. No admission follows permanent suppression |
| Start a fresh normal match while the session remains connected | New match reporting is enabled; old callbacks retain the old match identity |
| Controller changes in an ordinary game | No APMW reporting state is required |

The match's non-reporting status must be visible without indicating an AP
session disconnect, DeathLink event, or automatic loss.

## Required acceptance for the settled FEN boundary

An attempted FEN-based APMW continuation is refused without replacing the
active position, origin ledger, or pending reporting state.
Placing the same FEN inside SGF cannot bypass that boundary.
Internal generated setups and ordinary-game FEN use continue to work.

These are future acceptance requirements, not executed results.
The final command matrix must distinguish retained operations from disabled
operations and any separately specified read-only implementation.

## Answer

FEN-based APMW resume is unsupported.
Reliable Undo is part of 0.4.0 through the existing reversible ledger and
complete integration acceptance.
Genuinely view-only history must not mutate the active continuation or
produce reports.
Computer takeover remains available, but permanently stops new Location and
final-goal submissions from that match.
Q45 permits earlier library submissions to finish.

These decisions preserve internal engine rollback, ordinary-game behavior,
and the distinction between local state and accepted server checks.
They do not select the deferred SGF recording feature, resolve the other
world Options questions, or authorize production changes.

The later badge proposal is owned by
[ticket 22](22-specify-apmw-reporting-status.md).
Its connection-only recovery question does not reopen the permanent
controller-change reporting boundary.
