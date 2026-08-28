# Define the versioned cross-repository contract

Type: grilling  
Status: open  
Blocked by: 14, 15, 16

## Question

Which repository and artifact own exact CPU behavior for the formal geometry set `6x8`, `8x8`, `10x8`, `10x10`, and `12x10`, including augmented arrays, role identities, counts, and material tables, and how are ChessV and ChecksMate required to reject or migrate mismatched versions and the current/legacy 12x12 stage?

## Resolution requirements

- Decide whether the shared JSON manifest owns exact 10x10 and 12x10 coordinates and family correspondence or only versions/counts with ChessV as layout authority.
- Define the next CPU layout and Location profile version identifiers.
- Specify all contract fields needed for the five formal geometry records, including augmented stage arrays, role IDs, royal/castling metadata, CPU counts, and material-calibration metadata.
- Define canonical hashing and the required manifest-hash bump.
- Define compatibility behavior for old slot data, old clients, mixed layout/Location versions, and the current/legacy 12x12 stage. Choose whether 12x12 is rejected, hidden, or represented as explicitly unsupported; it must never be accepted as a formal geometry.
- Name the conceptual consumers in ChessV and `worlds\checksmate` without prescribing exact production diffs.
- Define which side validates exact behavior for the five formal geometries and which side owns numeric Location IDs and authored thresholds.
- State the required atomic release ordering or negotiation protocol so one repository cannot silently drift.

Exact external repository edits remain downstream; this ticket settles the contract they must implement.
