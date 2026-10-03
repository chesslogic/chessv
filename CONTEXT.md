# ChessV domain language

These terms describe Archipelago objectives, player material policy, and the
CPU army's starting formation.

## Language

**Location**:
An Archipelago objective, distinct from a square on the chessboard.
_Avoid_: Board square (when referring to an Archipelago objective)

**Capture role**:
A piece's identity in its starting formation for individual capture
classification, independent of later movement or promotion.
Each starting piece holds exactly one capture role, and the role stays with
that piece through movement, promotion, and Undo.
Standard/FIDE roles provide stable names across army families.
_Avoid_: File identity, current piece kind (as substitutes for capture role);
starting unit (as a separate concept)

**Tactical target class**:
A group of current piece kinds used for threat and fork goals, independent
of a unit's starting capture role.
_Avoid_: Capture role, human item tier (as synonyms)

**Rearguard pawn**:
The rear member of a specified doubled pawn pair at setup, distinguished only
for capture classification.
_Avoid_: Special pawn type, dynamically blocked pawn

**Center Rook**:
The central Rook-role in the 6x8 Standard formation, distinct from the Queen
role on larger boards.
Other army families can fill this role with a different piece kind.
_Avoid_: Queen (for this capture role)

**Multi-King setup**:
A match whose CPU army starts with more than one King, even after only one
survives.
_Avoid_: Multi-King phase (as a substitute for the starting setup)

**Regicide**:
The Location for capturing a CPU King in a multi-King setup, distinct from
checkmate without a capture.
_Avoid_: Checkmate (as a synonym for Regicide)

**Capture Everything**:
The Location for clearing all non-King units and spare King lives in one
match on the world's configured ending geometry.
_Avoid_: Capturing every King, clearing whichever board is currently largest

**Chessmen estimate**:
The generator's estimate of the player's non-primary unit count for a goal,
separate from actual CPU capture counters.
_Avoid_: Captures already made, number of enemy Kings

**Intrinsic requirement**:
The generator's unscaled material or chessmen estimate for a goal on one
formation, before runtime difficulty and projection caps.
_Avoid_: Guaranteed physical minimum, final capped requirement

**Cost snapshot**:
The frozen Location cost definitions for one generated world, shared by the
generator and client.
_Avoid_: The latest price table, client-local calibration

**Pawn reservation**:
The minimum number of units that the Legacy pawn allocation must retain
before its remaining material can fund upgrades.
_Avoid_: Material grant, total active-army count (as synonyms)

**Composition minimums**:
Paired targets for total units and non-Pawn units, with no separate Pawn minimum
and no upper limit implied by either target.
_Avoid_: Fixed Pawn/non-Pawn split, Pawn quota, army-size ceiling

**Pawn family**:
Pawns and Pawn variants, such as Sergeants and Berolinas.
They count toward the total-unit minimum but not the non-Pawn minimum.
_Avoid_: Weak piece, pawn item (as synonyms)

**Fair Board Guarantee**:
The world setting that selects which composition minimums apply:
none, the total-unit minimum only, or both minimums.
_Avoid_: Material grant, Pawn-upgrade mode (as synonyms)

**Pawn-material conversion**:
Use of Legacy Pawn material and eligible Legacy surplus for fewer, stronger
units instead of one unit per Pawn item, subject to the pawn reservation.
_Avoid_: A new material grant, Fundamental slot consumption (as synonyms)

**Pocket contribution**:
The number of supported actual pocket families in ChessV's captured final
prepared setup.
One pocket piece can contribute for every two eligible non-primary chessmen
deployed on the board.
Non-Pawn pocket pieces contribute before Pawn-family pocket pieces.
Each supported piece counts once, including Pawns and Pawn variants.
An empty pocket contributes zero.
The prepared record identifies Pawn, Minor, Major, or Queen family, not a
concrete chessman such as Phoenix or Rook.
_Avoid_: Received Pocket-item count, pocket upgrade level, named pocket piece
(as synonyms)

**Non-reporting match**:
An APMW match that continues local play but cannot submit new Location
checks or final Archipelago goal completion.
Controller changes make this state permanent for that match.
Earlier submissions can still complete.
_Avoid_: Disconnected AP session, DeathLink event, lost match (as synonyms)

**Earned-Location set**:
The Location IDs achieved by one eligible match, retained for replay after
reconnection to its world and slot.
Undo changes current capture counts, not this record of accomplishments.
The set can outlive its game window in process memory, but has no durable
journal file.
_Avoid_: Current capture count, server acknowledgment list (as synonyms)

**Earned-goal marker**:
A record that an eligible match achieved the final Archipelago goal,
retained for the same replay as its earned Locations.
It is separate from the Location IDs and from server acceptance.
_Avoid_: Synthetic Location, saved chess position, server receipt (as synonyms)

**World/slot identity**:
The Archipelago generation name and authenticated team and slot that
identify the source of a match's earned progress.
The original world contract also constrains replay.
This identity does not distinguish server instances or generations with
identical identity and contract values.
_Avoid_: Server address, port, client UUID, unique room instance (as synonyms)

**Region (Archipelago)**:
A reachability node that can contain Locations and connect to other Regions
through Entrances.
_Avoid_: Board square, board geometry (as synonyms)

**Entrance (Archipelago)**:
A directed connection between Regions with its own access rule.
_Avoid_: A second parent for the same Location

**Material family**:
A nominal group of player piece types, such as Pawn, Minor, Major, Jack,
Queen, or Amazon, independent of each concrete type's exact evaluation.
_Avoid_: Exact piece value (as a synonym for material family)

**Enabled material palette**:
The set of material families permitted in the generated player roster.
It is distinct from pocket contents, CPU armies, and promotion permissions.
_Avoid_: Upgrade-edge list, promotion allowlist

**Cumulative material count**:
The number of roster units at or above a material-family threshold.
The same unit can contribute to several cumulative counts.
_Avoid_: Exclusive family count, count of distinct pieces in a bag

**Final family count**:
The number of generated roster units in one material family.
_Avoid_: Cumulative material count, guaranteed family quota

**Distribution preset**:
A named preference for player unit supply and higher-family allocation.
It is a bias, not a guaranteed final composition.
_Avoid_: Target roster, fixed piece counts

**Concrete-type variety**:
The diversity of concrete piece types within a material family, distinct
from the number of units allocated to that family.
_Avoid_: Family quantity limit, minimum piece count

**Castler protection**:
Castling support reserved in the generated player roster, distinct from
received Castler grants and actual legal castling moves.
_Avoid_: Castler ownership, guaranteed legal castling move

**Core roster**:
The player's grant-backed nonroyal units, distinct from separately granted
royals, pockets, and surplus Pawns.
_Avoid_: Active roster, all player pieces (as synonyms)

**Ordinary allocation remainder**:
Unspent material from the pooled Chessmen, Castler, and Material grants,
distinct from value attached to reserve pieces or outside-roster common credit.
_Avoid_: Total unallocated credit, reserve-piece value

**Surplus Pawn**:
An additional player Pawn paid from the ordinary allocation remainder,
not a received Chessman or Castler unit.
_Avoid_: Free Pawn, extra Chessman grant
