# CPU royal and castling acceptance fixtures

Status: Accepted engineering fixture specification, confirmed at Q35
Scope: Future implementation acceptance, not executed gameplay tests

[ADR 0003](../../../docs/adr/0003-cpu-royal-survival-phases.md) owns the CPU
survival policy.
[Ticket 14](../issues/14-set-royal-and-castling-semantics.md) supplies the exact
castling destinations.
[Ticket 15](../issues/15-define-location-role-identity.md) owns capture identity
and counting.
These fixtures preserve those decisions.

## Common fixture context

Every integration fixture uses an attached match with explicit human and CPU
players. A null `Game.Match` is a different source path and is insufficient.
The CPU is White unless the fixture specifies its reflected form.
All unlisted squares and pockets are empty.
En-passant rights are absent.
Castling rights are absent unless explicitly listed.

The initialized game uses the approved geometry and starting-role catalog.
The listed position represents a later state in that match.
Piece identity includes its original role, not just its current square.
Previous captures and movements must agree with that role ledger.

Reflection keeps files fixed and maps rank `r` to `height + 1 - r`.
It also swaps piece colors and the human/CPU assignments.
CPU `K/Q` castling rights become `k/q`.
Winner assertions identify the CPU or human, not a fixed color.

Rule components remain installed throughout all transitions.
No fixture permits replacement of the rule set when one King disappears.

Undo restores match-local state.
It does not retract Location checks already accepted by Archipelago.
The current undo handler restores counters and lineage, not server checks.
Source: `APMW.Client\LocationHandler.cs:178-198`.
Failed attempts must produce no successful check in the first place.

These undo/load fixtures specify internal state correctness, not support for
player-facing backtracking or arbitrary position import.
[ADR 0018](../../../docs/adr/0018-reject-fen-based-apmw-resume.md)
rejects FEN-based APMW resume.
[Ticket 21](../issues/21-define-apmw-interaction-support.md)
records player-command availability.
Q37 and [ADR 0019](../../../docs/adr/0019-support-reliable-apmw-undo-in-zero-four.md)
include reliable player-facing Undo in 0.4.0.
Its additional acceptance cases exercise the real engine notification path
and repeated or alternate continuations.
Disabling such a command does not waive speculative or failed-move rollback.
The supported restoration fixtures require their stated origin ledger;
a bare FEN or an executable `FENStart` field does not supply it.
After a controller change, Q39 permanently disables that match's progress
reporting.
That one-way state is separate from the reversible move ledger.
Undo must not restore reporting eligibility.
The terminal reporting fixtures below assume a reporting-eligible match;
the non-reporting counterparts are in ticket 21.

## Check obligation by surviving count

The base position is Standard 12x10 with White to move:

| Side | Pieces |
| --- | --- |
| CPU White | Mounted King G1, ordinary King G2, Rook A1 |
| Human Black | King L10, Rook G8 |

G1 and G2 retain the original primary and additional King identities.
The Rook on G8 attacks the King on G2.
The candidate CPU move is A1-A2.

| Variant | Change from the base position | Expected result |
| --- | --- | --- |
| Two Kings | None | A1-A2 is legal despite the attacked ordinary King |
| Last ordinary King | The primary at G1 was captured earlier | A1-A2 is illegal because G2 remains attacked |
| Last Mounted King | The additional King at G2 was captured earlier | A1-A2 is illegal because the open G-file attacks G1 |

The one-King variants retain initial multi-King capture eligibility.
They do not become single-King starting setups.
The reflected fixtures have the same legality outcomes.

## Capture, transition, and undo

### Capture the additional King

The base position is the preceding two-King position, with Black to move.
CPU Queen-side castling right `Q` is present.
The primary King and Rook A1 retain their original unmoved identities.
The King's-side right is absent because its corner castler is absent.

The human commits G8-G2, capturing the additional King.
The last Mounted King on G1 is then in check.
The match continues under one-King legality.
The unrelated CPU move A1-A2 is illegal.

The King capture completes Regicide once and contributes one eligible
non-pawn capture.
The primary's Queen-side castling right is not inherited or newly created.
It remains associated with the original primary, subject to the current
attack restriction.

Undo restores the Rook to G8 and the additional King to G2.
The phase returns to two Kings and A1-A2 is legal again.
The board, original roles, rights, counters, and result return to the
pre-move state.

### Capture the primary and occupy its former square

The starting position is Standard 12x10, with Black to move:

| Side | Pieces |
| --- | --- |
| CPU White | Original Mounted King G1, additional ordinary King G2, original Rook A1 |
| Human Black | King L10, Rook H1 |

The only CPU castling right is `Q`.
The human commits H1-G1 and captures the primary.
The additional King becomes the last King and is in check.
All CPU castling rights end.

The CPU then plays G2-G1 and captures the attacking Rook.
It occupies the original primary square but still has no castling rights.
The corner Rook's continued presence does not permit inheritance.

Undoing only G2-G1 restores the checked additional King on G2, without rights.
Undoing H1-G1 then restores the original Mounted King and its prior `Q` right.
The two undo boundaries must remain distinct.

## Two-King extinction in one committed move

The position is Standard 12x10, with Black to move:

| Side | Pieces |
| --- | --- |
| CPU White | Mounted King E6 originating G1, ordinary King G4 originating G2, Rook A1 |
| Human Black | Checkers D7, King L10 |

The prior capture ledger contains all fourteen starting CPU pawns and
seventeen of its eighteen non-King pieces.
Neither King was captured.
The Rook at A1 is the uncaptured non-King.

The human commits the Checkers path D7-F5-H3.
Its two captured squares are E6 and G4.
The complete committed batch removes both Kings.

| Output | Required value |
| --- | --- |
| CPU survival phase | Zero Kings |
| Match result | CPU defeat by extinction |
| Pawn counter | 14 |
| Piece counter | 19 |
| Any counter | 33 |
| Regicide | Complete once |
| Capture Everything | Incomplete because the Rook remains |
| Reported Any threshold | No invented Any 33 Location |

The result must not require an intervening one-King turn.
The surviving Rook and its legal moves do not prevent extinction defeat.
Undo restores both Kings in one operation and restores the prior counters.

## Last-King terminal positions

The CPU has only its surviving King at A1.
The human has an ordinary King at C3.
The CPU is to move.
All other state follows the common fixture context.

| Human's other piece | CPU King type | Expected moves and result |
| --- | --- | --- |
| Queen B2 | Ordinary King | No legal move and A1 is attacked. CPU loses by checkmate |
| Queen C2 | Ordinary King | No legal move and A1 is not attacked. CPU wins by stalemate |
| Queen B2 | Mounted King | No legal King-step or Knight-leap escape. CPU loses by checkmate |
| Queen C2 | Mounted King | No legal King-step or Knight-leap escape. A1 is not attacked. CPU wins by stalemate |

The Mounted King's extra destinations from A1 are B3 and C2.
The human King and Queen cover those destinations in both positions.
The ordinary-King cases apply to all five geometries.
Mounted-King cases apply to the surviving primary on 12x10.

The zero-King precedence case removes the CPU King but retains CPU Rook F1.
The same human King/Queen placement then produces CPU extinction defeat.
The dispatcher must not first request a nonexistent last King.

### Multiple-King no-move classifier

This is a terminal-policy unit fixture, not a claim that a constructed board
has no legal moves.
Its explicit inputs are CPU side, surviving count 2, and legal-move count 0.
The required output is CPU victory.
Attack status does not change that output.

The dispatcher returns the appropriate side-relative win/loss response.
An undeclared zero enum value is not an accepted result.
The separate integration positions establish the reachable one-King and
extinction branches.

### Victory reporting boundary

Both last-King checkmate and zero-King extinction enter the existing human
victory reporting path.
The published stage-victory goals through the match stage complete.
Only victory on the configured ending geometry emits the client-goal status.
Neither CPU stalemate victory nor the multiple-King no-move result awards a
human stage victory.

The direct-extinction fixture leaves a CPU Rook on the board.
It awards the stage victory but not Capture Everything.
The surviving Rook therefore distinguishes the two goal conditions.
Earlier-stage victory must not report an unpublished or later-stage goal.

These expectations preserve the existing victory boundary, with the accepted
CPU outcomes and new World binding.
Sources: `APMW.Client\LocationHandler.cs:761-789` and
`APMW.Client\CaptureLookup.cs:79-94`.

## Rollback and load matrix

The operation target is the additional-King capture G8-G2.
Its starting position and rights are defined in that fixture.

| Operation | Harness input and order | Required boundary |
| --- | --- | --- |
| Successful committed capture and undo | Commit G8-G2, inspect its one-King state, then undo | Restore all pre-move state |
| Successful speculative move and undo | Perform the same move speculatively, then undo with the same mode | Restore all pre-move state and emit no committed APMW capture |
| Failed speculative move | Enumerate the otherwise legal move, then activate a test-only rejection rule before execution | Restore state despite rejection. No capture or Regicide notification |
| Failed committed move | Enumerate the move, then activate the same late rejection rule and attempt a committed move | Restore state after the setup notification and rejected make. No successful committed report |
| Repeated load | Load the two-King position, then its post-capture position, then the two-King position again | Royal membership is exactly 2, 1, 2 with no stale objects |
| Terminal load followed by fresh position | Load the zero-King result, then the two-King position | No terminal result or empty royal cache survives into the fresh state |

The rejection rule is an explicit fixture input, not production behavior.
It returns `IllegalMove` after stateful move handlers participate.
The fixture must not substitute rejection before move setup.

State equality includes board contents and hash, side to move, move counters,
royal identities, castling rights, game result, capture lineage, capture
counters, and pending reporting state.
The raw setup observer can run for the rejected committed attempt.
It must not leave a pending successful capture or suppress a later real move.
The ledger must align with committed moves, including non-capturing moves.
One multi-capture commits or reverses as one complete batch.
View-only traversal must not produce committed progress or reports.

## Castling fixture construction

Each castling-capable geometry uses its original primary at the home square,
both original corner castlers, and CPU rights `KQ`.
The human has only an ordinary King at file D on its home rank.
The CPU is to move.

On 12x10, the additional ordinary King is at C3 with its original G2 identity.
That location keeps the primary's castling routes and attack files clear.
All other squares follow the common empty-board rule.

The expected primary and castler destinations are literal values from
ticket 14, not results obtained from the production castling composer.
Standard corner castlers are Rooks.
The Colourbound cases substitute Clerics and their specified parity
destinations.

| Variation | Input | Required result |
| --- | --- | --- |
| Clear legal route | The base position | The registered castling move uses the exact primary and corner actors and destinations |
| Attacked source | Add a human Rook on the primary's source file at rank `height - 2` | No castle from that attacked source |
| Attacked transit | Put the human Rook on the intermediate King file instead | No castle through that attacked square |
| Attacked destination | Put the human Rook on the destination King file instead | No castle onto that attacked square |
| Last primary | Remove only the additional King on 12x10 | Apply the same legal-route and attack fixtures |
| Last additional | Remove the primary and retain the additional King | No castling rights or move, regardless of square occupancy |
| Moved primary returns | Move the primary away and back through an otherwise legal sequence | No regenerated rights |
| Supported save/load | Save an allowed position with its role ledger and rights, then reconstruct it | Preserve the same actors and rights, without inferring new rights |
| Inconsistent restored actors | Supply rights but omit or substitute their required original actor | Reject the inconsistent APMW restoration rather than permit inheritance |
| 6x8 | Use the compact geometry | No castling rule or qualifying cost profile |

The attack fixtures apply with both CPU Kings and with the original primary
alone.
Reflection supplies the opposite CPU color.
Human Major/Jack source-file rights remain a separate preserved baseline.
The added forward pieces and the outer Lions receive no CPU castling rights.

## Registration and isolation matrix

The later suite crosses the following axes:

| Axis | Required values |
| --- | --- |
| Geometry | All five supported geometries |
| CPU family | Standard, Colourbound, Rookies, Nutty |
| CPU color | White and Black |
| Human King-upgrade state | 0, 1, and 2 |
| Human promotion context | No optional target, an entitled target, and a pocket-only target |

CPU setup and promotion sets remain equal to their approved per-family
definitions for every human-context variation.
Mounted King availability and CPU royal membership do not depend on human
King upgrades.
Basic Elephant is registered for the added setup but remains outside the
selected CPU promotion lists.

Applicable existing evaluation hooks follow registered types.
The fixtures do not authorize new balance coefficients.
Amazon remains outside the Queen-family tactical set.
Both ordinary and Mounted Kings receive the selected royal-fork treatment.

Q34 selects Minor for the basic Elephant tactical class.
This classification must not make it a human random-pool or promotion
candidate.

### Basic Elephant threat fixture

The attached Standard 10x10 match has White CPU King F1 and basic Elephant C2.
The Elephant retains its original Queen's Elephant capture role.
Black has human King J10 and Rook A5.
All other squares are empty, and Black moves A5-C5.

The resulting C-file attack qualifies for Threaten Minor.
It does not qualify for Threaten Major, Threaten Queen, or Threaten King.
This fixture changes no capture counter and grants no promotion entitlement.
It uses the common reflection and attached-match requirements.

## Completion boundary

These are specifications for the downstream suites.
The logical positions and explicit fault inputs are independent acceptance
data, not evidence that the current engine already satisfies the policy.
The implementation must include its executed results before release.
