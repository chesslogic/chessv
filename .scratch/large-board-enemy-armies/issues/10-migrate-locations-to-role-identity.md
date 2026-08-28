# Migrate file-based Locations to role identity

Type: grilling  
Status: resolved

## Question

Should individual capture Locations continue to be identified by original file?

## Answer

No. Individual CPU capture Locations must derive identity from the piece or pawn's semantic formation role, with geometry mapping that role to a coordinate. Human-facing names should express the role where possible, such as `Capture King's Pawn`, rather than allowing `Capture Pawn D` to change meaning when inserted files move the royal and bishop files.

The exact role taxonomy, stable machine IDs, display names, multiplicity rules, legacy-name migration, and numeric Location compatibility remain open. Keep that vocabulary in the decision ticket until it is resolved; do not create an ADR or glossary entry pre-emptively.

Role mappings target exactly `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`. Current/legacy 12x12 identities may be compatibility inputs, but they must not receive a supported geometry mapping.

## Provenance

- Current user decision to include semantic, role-based Location migration.
- `APMW.Client\CaptureLookup.cs`
- `APMW.Client\LocationHandler.cs`
- `C:\GitHub\rft50-checksmate\worlds\checksmate\locations.py`
