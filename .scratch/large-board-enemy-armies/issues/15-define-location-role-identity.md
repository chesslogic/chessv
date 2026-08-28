# Define the semantic Location role identity

Type: grilling  
Status: open  
Blocked by: 12, 13, 14

## Question

What canonical role IDs, display names, and geometry mappings replace file-letter identity for individual CPU pawn and piece capture Locations?

Work this ticket with domain-modeling. The answer must distinguish stable identity from presentation and coordinate. It should test names such as `Capture King's Pawn` against all resolved arrays rather than adopting that example automatically.

For doubled/advanced pawn pairs, test `rearguard pawn` as the candidate semantic term for the blocked rear pawn behind the forward pawn. Current user intent strongly hypothesizes that this rear pawn, rather than the forward pawn merely because it is advanced, is the distinctive harder capture that may deserve a unique name or premium. This is a working hypothesis for the role and material decisions, not a historical source fact.

## Resolution requirements

- A canonical machine-role taxonomy for primary royal, queen, Amazon, bishops/minors, castlers, attendants, forward pieces, wedge pawns, candidate rearguard pawns, and any repeated role.
- Rules for side qualifiers, outer/inner qualifiers, ordinals, and multiple same-role pieces.
- A mapping from every old Location to a new role identity, including `Capture Pawn A`–`L` and current Queen's/King's piece names.
- A decision on whether `rearguard pawn` is canonical, display-only, or rejected, and whether its paired forward pawn needs any distinct identity at all.
- A migration rule for recognizing a rearguard pawn from its starting formation relationship to the forward pawn, without redefining its identity from whichever square either pawn occupies after movement.
- A decision on numeric Location IDs: preserve, alias, dual-emit, or replace.
- Human-facing display names and behavior when a selected family changes the literal piece type.
- Role-to-coordinate mappings, stage availability, and aggregate capture-count behavior for exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`.
- A general rule that derives a Location from starting formation identity even after the piece moves; current square is event state, not semantic identity.
- Compatibility behavior for clients/worlds that know only `apmw-location-profile-v1`, including identification of current/legacy 12x12 identities that must be rejected, hidden, or otherwise treated as unsupported rather than receiving a supported geometry mapping.

Do not create `CONTEXT.md` or an ADR until this ticket resolves a canonical vocabulary that needs repository-wide persistence.
