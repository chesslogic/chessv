# ChessV domain language

These terms describe the Archipelago objectives and the CPU army's starting
formation.

## Language

**Location**:
An Archipelago objective, distinct from a square on the chessboard.
_Avoid_: Board square (when referring to an Archipelago objective)

**Capture role**:
A piece's identity in its starting formation for individual capture
classification, independent of later movement or promotion.
Standard/FIDE roles provide stable names across army families.
_Avoid_: File identity, current piece kind (as substitutes for capture role)

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

**Region (Archipelago)**:
A reachability node that can contain Locations and connect to other Regions
through Entrances.
_Avoid_: Board square, board geometry (as synonyms)

**Entrance (Archipelago)**:
A directed connection between Regions with its own access rule.
_Avoid_: A second parent for the same Location
