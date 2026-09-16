# Name capture Locations by their Standard formation roles

Status: accepted
Date: 2026-09-14

Individual capture Locations use stable Standard/FIDE-role names across army
families. Their identity follows the starting capture role, not the current
piece kind or square. This keeps the Location contract independent of the
player's Enemy Army choice.

## Consequences

The Rookies' six Lions occupy different capture roles. A home Bishop-role, a
forward Bishop-role, and an outer Lion-role do not collapse into one Lion
Location. The UI can show the actual piece name as supplementary information.

Movement and promotion do not rename a starting capture role. Numeric IDs and
display names remain separate parts of the Location mapping.

The 6x8 Center Rook and the Queen on larger boards have distinct Locations.
A world that spans both geometries can contain both checks. Capturing the
Center Rook does not complete the Queen Location. A 6x8-only world has the
Center Rook Location instead of Queen.

Every starting non-King unit has one individual capture Location. Existing
roles keep one shared Location across stages, not a copy for every board.
The compact Center Rook does not also complete Queen's Rook.

Main pawns use counterpart names, with separate Queen's/King's Rearguard Pawn
names for rear members of doubled pairs. Forward non-pawn blockers do not
rename those pawns. The owning ticket contains the full coordinate catalog.

[Starting-army capture counts](0007-count-captures-from-the-starting-army.md)
defines Regicide and the aggregate clear condition separately from individual
non-King coverage.

## Source

[Semantic Location identity](../../.scratch/large-board-enemy-armies/issues/15-define-location-role-identity.md#decisions-in-progress).
The user selected stable role names and distinct Center Rook and Queen checks
in the 2026-09-14 interview.
