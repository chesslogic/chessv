# Define the semantic Location role identity

Type: grilling  
Status: resolved  
Blocked by: 12, 13, 14

## Question

What canonical role IDs, display names, and geometry mappings replace file-letter identity for individual CPU pawn and piece capture Locations?

## Answer

Use the role catalog, capture arithmetic, and series contract in this record.
ADRs 0006 and 0007 record the accepted policies. The tables cover all five
geometries and both colors through the stated reflection.

ChecksMate assigns numeric IDs during contract implementation. Calibration
remains in ticket 16, and publication/schema work remains in ticket 17.
Resolving this decision does not authorize production changes.

For doubled/advanced pawn pairs, `rearguard` identifies the rear pawn in the
starting capture map. It does not define a special pawn type or movement rule.
The role's final display wording and any Location-difficulty adjustment remain
separate decisions.

## Decisions in progress

### 2026-09-13: Rearguard is capture metadata only

The user clarified: "The only reason we need to maintain this 'rearguard'
property is for LocationHandler to be able to emit proper capture data."

The approved 10x10 footprint has rear pawns at `B2/I2`, behind `B3/I3`.
Their starting-map identity lets `LocationHandler` emit the correct capture
data after movement. Rearguard identity does not add a movement, protection,
survival, promotion, or other special gameplay property.

The current square remains event state. The starting-position map supplies
the capture classification. No dynamic blocked/unblocked-pawn classification
is required.

This answer does not select a new piece type, a canonical display name, a
numeric Location ID, or a material premium. The complete role taxonomy remains
open.

### 2026-09-13: King threats and royal forks

The user chose both ordinary and Mounted Kings as King targets for
`Threaten King` and King-plus-Queen royal-fork identity. The existing automatic
King bonus in the true-fork test remains active even when multiple Kings
survive. It is not limited to the last King.

This Location policy is intentionally distinct from compulsory check
avoidance. A King target can receive the existing true-fork treatment while
the CPU is still in its extinction phase.

The implementation must not recognize only `kings[0]` and miss the Mounted
King. This answer leaves the existing Queen-family target classification
unchanged. Amazon remains non-royal.

Source baseline: `APMW.Client\LocationHandler.cs:631-642,697-723`.

### 2026-09-14: Location IDs can change for 0.4.0

The user plans 0.4.0 as a breaking release and has no particular requirement
either to preserve or to replace existing numeric Location IDs.

ID allocation is an implementation choice under the semantic Location
contract. Preserve numbers where convenient or assign new ones where needed.
This is permission for breaking changes, not a requirement to renumber every
Location.

The meaning and numeric mapping must remain coherent within each published
contract. The subsequent compatibility answer selects new-contract-only
support in 0.4.0. Older worlds use matching older clients. Numeric continuity
alone does not prove semantic compatibility.

### 2026-09-14: Stable role names across families

The user selected stable Standard/FIDE-role names. The actual piece name can
appear as supplementary UI information, but it does not replace the canonical
role name.

For the Rookies, a Queen's Bishop, Queen's Forward Bishop, and Queen's Lion
can all be concrete Lions. Their different starting roles still identify
different captures. Movement and promotion do not rename those roles.

This settles the naming model, not the complete spelling table or which new
roles receive individual Locations.

### 2026-09-14: Center Rook is distinct from Queen

The user selected distinct Center Rook and Queen Locations. In a world that
progresses from 6x8 to 8x8, capturing the Center Rook and the later Queen can
award separate checks. Neither capture completes the other's Location.

A 6x8-only world contains Center Rook instead of Queen. Queen becomes
available on larger boards. The relationship between Center Rook and the
old 6x8 Queen's Rook classification still needs an explicit role mapping.

[Starting-role capture names](../../../docs/adr/0006-name-capture-locations-by-starting-role.md)
records these decisions. The root glossary defines the resolved terms without
claiming that the complete taxonomy is settled.

### 2026-09-14: Every starting non-King unit has an individual check

The user selected one individual capture Location per starting non-King unit.
This includes all pawns, forward minors, Elephants, outer Lions, and Amazon.
An existing role keeps one shared Location across stages, not a separate
Location for every board.

On 6x8, the sole Rook completes Center Rook only. It does not also complete
Queen's Rook. Queen's Rook remains a separate target on larger boards.

| Geometry | Starting pawns | Starting non-pawns excluding Kings | Individual non-King targets |
| --- | ---: | ---: | ---: |
| 6x8 | 6 | 5 | 11 |
| 8x8 | 8 | 7 | 15 |
| 10x8 | 10 | 9 | 19 |
| 10x10 | 12 | 15 | 27 |
| 12x10 | 14 | 18 | 32 |

These counts are per formation. They are not the sum of distinct Locations
across a world's progression.

### 2026-09-14: Regicide and King capture counts

The user supplied `Regicide` as the King-capture Location name. The next
round must make its exact trigger and geometry availability explicit.
The answer does not authorize separate primary-King and additional-King
Locations.

For aggregate counters, the user chose: "Count Kings, but only in the
multi-king setup". This rejects unconditional exclusion of Kings.
The next round must distinguish the initial setup from the current survival
phase and specify the outcome for a two-King capture in one move.

King contributions to capture totals do not yet settle whether Capture
Everything requires a spare King as well as every non-King unit. That
remaining choice must not disappear inside an arithmetic threshold.

### 2026-09-14: Capture Everything uses the configured ending board

The user selected the world's configured ending geometry, within one match.
An 8x8-to-12x10 world requires the clear on 12x10. A 6x8-only world requires
the clear on 6x8.

The largest geometry currently unlocked is not the eligibility rule.
Captures from separate matches do not accumulate toward this clear.
The required King/non-King composition remains the separate open question
described above.

### 2026-09-14: Exact Regicide, King-count, and clear rules

The user approved the starting-setup interpretation. Regicide is one Location
for capturing either CPU King when the starting army has multiple Kings.
Checkmate without a King capture does not complete it.

A Checkers move that captures both Kings completes Regicide once and adds two
non-pawn captures. A one-King starting setup neither offers Regicide nor counts
its King in capture series. The current survival phase does not change that
eligibility.

Capture Everything requires all starting non-Kings and the spare King lives.
At most the last King can remain. On 12x10, that means all fourteen pawns,
all eighteen non-King pieces, and at least one of the two Kings.
Either King can survive. A captured King never substitutes for an uncaptured
non-King unit.

The earlier ending-geometry and single-match requirements remain fixed.
[Starting-army capture counts](../../../docs/adr/0007-count-captures-from-the-starting-army.md)
records these rules.

### 2026-09-14: Pawn counterpart names

The user approved counterpart-based pawn names. The main pawn keeps the name
of its corresponding Standard formation role. Forward non-pawn blockers do
not rename it.

The rear pawn in each doubled pair has a separate Queen's/King's Rearguard
Pawn identity. On 10x10, White B3 is Queen's Knight Pawn and B2 is Queen's
Rearguard Pawn. On 12x10, B3 is Queen's Lion Pawn and B2 keeps the Rearguard
name.

On 12x10, F3 is Queen's Pawn despite Amazon on F1 and Queen on F2.
On 6x8, C2 is Center Rook Pawn. Other pawn names follow their Rook, Knight,
Bishop, Attendant, or King counterparts. Display names start with `Capture`.
The King's-side names follow the same rule.

Rearguard remains capture identity only. Its name does not imply a material
premium.

### 2026-09-14: Series ceilings and earlier-board count goals

The user retained every integer threshold from 2 through the configured
endpoint's ceilings. The ceilings never require capturing the last King.
The Any series stops one short of the normal full-clear count.

The user also approved count goals on earlier boards when the current match
meets their counts. For example, Capture Any 15 can complete on 8x8 in a
world that contains that Location. Capture Everything remains endpoint-only.
The client reports only Locations actually present in that world.

## Claim ledger for the 2026-09-14 round

| Exact claim | Class | Authority | Excludes |
| --- | --- | --- | --- |
| Numeric Location IDs can change for 0.4.0 without requiring universal renumbering | Author decision | User's release clarification | Mandatory preservation or replacement of every ID |
| Canonical capture names follow Standard/FIDE starting roles across army families | Author decision | User's Q2 answer | Renaming the canonical Location from the current concrete piece kind |
| Center Rook and Queen are distinct checks in a world spanning their geometries | Author decision | User's Q3 answer | Capturing either one completing a shared Location |
| Every starting non-King unit has one individual capture Location | Author decision | User's Q4 answer | Leaving new non-King roles without an individual check |
| Center Rook does not also complete Queen's Rook on 6x8 | Author decision | User's Q4 answer | Double-counting one compact Rook as two individual roles |
| The King-capture Location is named Regicide | Author decision | User's Q5 answer | Treating the example Capture a King wording as the selected name |
| Kings contribute to aggregate captures only in the multi-King setup | Author decision | User's Q6 answer | Unconditional exclusion or unconditional inclusion on every geometry |
| The initial setup, rather than live surviving count, controls King eligibility | Author decision | User's Q8 answer | A counter-policy switch when one King remains |
| Capture Everything uses the configured ending geometry within one match | Author decision | User's Q7 answer | A smaller-board clear or captures accumulated across matches |
| Capture Everything requires every non-King and at least one King on the two-King setup | Author decision | User's Q9 answer | Two captured Kings substituting for an uncaptured ordinary unit |
| Main pawns use counterpart names and rear pawns use separate Rearguard names | Author decision | User's Q10 answer | Renaming a pawn from a forward non-pawn blocker or its current square |
| Series include every integer from 2 through the endpoint ceilings | Author decision | User's Q11 answer | Sparse milestones or thresholds that require the last King's capture |
| A published count goal can complete on an earlier board that meets its count | Author decision | User's Q12 answer | Retaining an arbitrary later-board gate on a satisfied count |

## Derived capture arithmetic

`P`, `M`, and `K` are the starting counts of pawns, non-pawn non-Kings, and
Kings. `cP`, `cM`, and `cK` count distinct captured starting units in the
current match. Promotion does not change a unit's starting class.

```text
pawn_captures  = cP
piece_captures = cM + (K > 1 ? cK : 0)
any_captures   = pawn_captures + piece_captures
of_each       = min(pawn_captures, piece_captures)

regicide =
    K > 1
    and cK >= 1

capture_everything =
    match_geometry == configured_ending_geometry
    and cP == P
    and cM == M
    and K - cK <= 1
```

These equations describe goal conditions. A counter can exceed a published
ceiling without creating a new Location. The runtime must report only
Locations present in the world.

## Capture-series contract

The configured ending formation determines the published ceilings.
With the starting counts defined above:

```text
normal_piece_clear = M + max(0, K - 1)
normal_full_clear  = P + normal_piece_clear

Pawns ceiling   = P
Pieces ceiling  = normal_piece_clear
Of Each ceiling = min(P, normal_piece_clear)
Any ceiling     = normal_full_clear - 1
```

Each series contains every integer from 2 through its ceiling.

| Configured ending geometry | Pawns | Pieces | Of Each | Any | Normal full clear |
| --- | ---: | ---: | ---: | ---: | ---: |
| 6x8 | 6 | 5 | 5 | 10 | 11 |
| 8x8 | 8 | 7 | 7 | 14 | 15 |
| 10x8 | 10 | 9 | 9 | 18 | 19 |
| 10x10 | 12 | 15 | 12 | 26 | 27 |
| 12x10 | 14 | 19 | 14 | 32 | 33 |

Published ceilings depend on the endpoint, not the current match's geometry.
A published threshold completes when its count is met within the match.
Thus, a full 8x8 clear can complete Capture Any 15 in a larger world.
An 8x8-only world has no Capture Any 15 Location to report.

Generator availability follows the same count semantics. It cannot retain a
later-board gate merely because the old table assigned one. Material
requirements remain a separate calibration constraint.

A multi-capture completes every newly crossed published threshold.
It does not create Capture Any 33 or 34, or Capture 20 Pieces, in the
12x10 catalog. Both King captures still contribute to the actual counters.

## Canonical role and coordinate catalog

This catalog applies the accepted names and arrays. Symbolic machine keys use
`snake_case` as a specification convention. They are not numeric AP IDs.
Each non-King row has one individual Location whose display name is `Capture `
plus the role label. Primary and additional Kings instead feed Regicide and
the accepted royal rules.

Coordinates show White CPU starting squares. For Black, keep the file and
replace rank `r` with `height + 1 - r`. A dash means the role is absent.
Every family uses the same roles at these squares, with the approved piece
substitutions.

| Machine role | Role label | 6x8 | 8x8 | 10x8 | 10x10 | 12x10 |
| --- | --- | --- | --- | --- | --- | --- |
| `center_rook` | Center Rook | C1 | - | - | - | - |
| `queen_side_rook` | Queen's Rook | - | A1 | A1 | A1 | A1 |
| `queen_side_knight` | Queen's Knight | A1 | B1 | B1 | B1 | D1 |
| `queen_side_bishop` | Queen's Bishop | B1 | C1 | D1 | D1 | E1 |
| `queen_side_attendant` | Queen's Attendant | - | - | C1 | C1 | C1 |
| `queen` | Queen | - | D1 | E1 | E1 | F2 |
| `king_side_attendant` | King's Attendant | - | - | H1 | H1 | J1 |
| `king_side_bishop` | King's Bishop | E1 | F1 | G1 | G1 | H1 |
| `king_side_knight` | King's Knight | F1 | G1 | I1 | I1 | I1 |
| `king_side_rook` | King's Rook | - | H1 | J1 | J1 | L1 |
| `queen_side_forward_bishop` | Queen's Forward Bishop | - | - | - | E2 | D2 |
| `king_side_forward_bishop` | King's Forward Bishop | - | - | - | F2 | I2 |
| `queen_side_forward_knight` | Queen's Forward Knight | - | - | - | D2 | E2 |
| `king_side_forward_knight` | King's Forward Knight | - | - | - | G2 | H2 |
| `queen_side_elephant` | Queen's Elephant | - | - | - | C2 | C2 |
| `king_side_elephant` | King's Elephant | - | - | - | H2 | J2 |
| `queen_side_lion` | Queen's Lion | - | - | - | - | B1 |
| `king_side_lion` | King's Lion | - | - | - | - | K1 |
| `amazon` | Amazon | - | - | - | - | F1 |
| `primary_king` | Primary King (no individual check) | D1 | E1 | F1 | F1 | G1 |
| `additional_king` | Additional King (no individual check) | - | - | - | - | G2 |

Attendant retains the existing generic role name. Outer Attendant becomes
Lion in the new catalog. These names do not require legacy aliases.
Elephant means the approved basic Elephant, not War Elephant.

| Machine role | Pawn label | 6x8 | 8x8 | 10x8 | 10x10 | 12x10 |
| --- | --- | --- | --- | --- | --- | --- |
| `center_rook_pawn` | Center Rook Pawn | C2 | - | - | - | - |
| `queen_side_rook_pawn` | Queen's Rook Pawn | - | A2 | A2 | A2 | A2 |
| `queen_side_knight_pawn` | Queen's Knight Pawn | A2 | B2 | B2 | B3 | D3 |
| `queen_side_bishop_pawn` | Queen's Bishop Pawn | B2 | C2 | D2 | D3 | E3 |
| `queen_side_attendant_pawn` | Queen's Attendant Pawn | - | - | C2 | C3 | C3 |
| `queen_pawn` | Queen's Pawn | - | D2 | E2 | E3 | F3 |
| `king_pawn` | King's Pawn | D2 | E2 | F2 | F3 | G3 |
| `king_side_attendant_pawn` | King's Attendant Pawn | - | - | H2 | H3 | J3 |
| `king_side_bishop_pawn` | King's Bishop Pawn | E2 | F2 | G2 | G3 | H3 |
| `king_side_knight_pawn` | King's Knight Pawn | F2 | G2 | I2 | I3 | I3 |
| `king_side_rook_pawn` | King's Rook Pawn | - | H2 | J2 | J2 | L2 |
| `queen_side_lion_pawn` | Queen's Lion Pawn | - | - | - | - | B3 |
| `king_side_lion_pawn` | King's Lion Pawn | - | - | - | - | K3 |
| `queen_side_rearguard_pawn` | Queen's Rearguard Pawn | - | - | - | B2 | B2 |
| `king_side_rearguard_pawn` | King's Rearguard Pawn | - | - | - | I2 | K2 |

An individual Location exists when its role appears in at least one geometry
in the world's configured progression. Its capture requires that role in the
current match. A Center Rook check does not persist as a Queen's Rook alias
after progression.

The five-stage catalog contains nineteen non-King non-pawn roles and fifteen
pawn roles. Thus, a world spanning 6x8 through 12x10 has 34 distinct
individual non-King Locations, plus Regicide. Series, tactics, and victory
Locations are additional.

## Old-to-new semantic review ledger

This ledger compares contract meanings. It does not transfer old completion
state or select which authored material baseline a new role inherits.
Ticket 16 owns that calibration lineage.

| Legacy name or classification | New contract treatment |
| --- | --- |
| Capture Pawn A-L | Replace file names with the pawn catalog. The catalog supplies the exact role and coordinate for each geometry |
| One file-based check for a doubled pawn file | Separate main-pawn and Rearguard Locations. Neither role aliases the other |
| Capture Piece Queen's Rook on 6x8 | Replace this compact classification with Center Rook only |
| Queen's/King's Rook, Knight, and Bishop on larger boards | Keep their semantic roles and use the new catalog coordinates. Individual labels omit the old Piece prefix |
| Capture Piece Queen | Keep Queen-role identity. On 12x10 it follows Queen to F2, not Amazon at F1 |
| Queen's/King's Attendant | Keep the generic role labels and approved counterpart coordinates |
| Queen's/King's Outer Attendant | Use Queen's/King's Lion in the new 12x10 catalog. The former Nightrider occupant does not survive this change |
| Non-home-rank means pawn | Remove this classification rule. Forward minors, Elephants, Queen, and the additional King have their own starting roles |
| King-file checkmate placeholder in the piece capture map | Do not emit it as an individual capture. Use Regicide and the royal result rules separately |
| Pawn/Piece/Of Each/Any series | Keep the series names and apply the explicit starting-class counts, new ceilings, and count-based availability |
| Capture Everything | Clear all non-Kings and spare King lives on the configured ending geometry within one match |
| Legacy 12x12 identities | Exclude from the new contract. Do not map them to a supported geometry |

Numeric continuity, where retained, does not override this ledger.

## Semantic fixtures from this round

These are classification fixtures, not legal-move or full-match fixtures.
The role catalog fixes display names. Numeric IDs remain downstream.
The approved army tables supply the initial CPU formation.

| Fixture | Initial state and inputs | Operation order | Expected output |
| --- | --- | --- | --- |
| Distinct Rookies Lions | White CPU, approved Rookies 12x10 array. B1, E1, and D2 are concrete Lions | Classify each origin. Apply the isolated lineage event B1 to B4, then capture the piece at B4 | B1 is the queen-side outer Lion-role, E1 is the home Queen's Bishop-role, and D2 is the Queen's Forward Bishop-role. The moved B1 piece completes only its outer Lion individual check, not either Bishop check |
| Center then Queen | Standard family, White CPU, world from 6x8 through 8x8. Neither subject Location is complete | Capture the piece originating at C1 on 6x8. Advance the fixture to 8x8 without erasing completed Locations. Capture the piece originating at D1 | The first capture completes Center Rook, not Queen. After the second capture, both are complete |
| Compact-only availability | Standard family, world starts and ends at 6x8 | Build the supported individual-capture Location set | Center Rook is present. Queen and Queen's Rook are absent |

These fixtures hold geometry, family, and origin fixed. Alternative-family
piece substitutions cannot change the accepted role identity.
Aggregate captures and unrelated Locations are outside these fixture outputs.

## Counter and clear fixtures

These fixtures hold the accepted starting army fixed. The counters describe
distinct starting units captured within one match, not their current types.
The world ends at 12x10 unless a row states otherwise.

| Starting setup and captured units | Expected counters | Regicide | Capture Everything |
| --- | --- | --- | --- |
| 12x10, all 14 pawns and 18 non-King pieces captured, neither King captured | Pawns 14, Pieces 18, Any 32 | Incomplete | Incomplete: both King lives remain |
| Same setup, then capture either King | Pawns 14, Pieces 19, Any 33 | Complete once | Complete |
| Same setup, both Kings captured in one committed move | Pawns 14, Pieces 20, Any 34 | Complete once | Complete |
| 12x10, all 14 pawns, only 17 non-King pieces, and both Kings captured | Pawns 14, Pieces 19, Any 33 | Complete once | Incomplete: one non-King remains |
| 12x10, only 13 pawns, all 18 non-King pieces, and both Kings captured | Pawns 13, Pieces 20, Any 33 | Complete once | Incomplete: one pawn remains |
| 8x8, all 8 pawns and 7 non-King pieces captured, King remains, world ends at 12x10 | Pawns 8, Pieces 7, Any 15. Capture Any 15 completes | Not offered | Incomplete: wrong geometry |
| Same 8x8 capture state, world starts and ends at 8x8 | Pawns 8, Pieces 7, Any 15 | Not offered | Complete |
| 12x10, only the Queen's Pawn captured after it promoted to Queen | Pawns 1, Pieces 0, Any 1 | Incomplete | Incomplete |

For the two-King multi-capture fixture, the White CPU primary King originates
at G1 and its additional King originates at G2. Their current squares can be
E6 and G4, with a Black Checkers piece at D7 and empty landing squares F5
and H3. The capture batch is D7-F5-H3, taking both Kings.
This distinguishes current capture squares from starting-role bindings and
does not substitute the opposite-colored initial King squares as one diagonal
capture path.

One complete logical board for the non-substitution case contains only those
two CPU Kings, the CPU Queen's Rook at A1, the Black Checkers piece at D7,
and the Black King at L10. All other squares are empty.
The prior captured-role set contains fourteen pawns and seventeen non-King
pieces. After the capture batch, the CPU loses by extinction, but the Rook
still prevents Capture Everything.

Series counters above their published ceilings remain valid counts.
In the two-King case starting from Pieces 17 and Any 31, emit newly crossed
Pieces 18, Pieces 19, and Any 32. Do not emit an absent Any 33.

## Current-source comparison

These observations describe existing code, not new author decisions.

| Observation | Source |
| --- | --- |
| The 6x8 Standard home row is `N B R K B N`. The existing capture map calls its C-file Rook Queen's Rook | `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:362-366`; `APMW.Client\CaptureLookup.cs:110-120` |
| Existing larger-board checks include Queen's/King's Attendant and Queen's/King's Outer Attendant | `APMW.Client\CaptureLookup.cs:135-164`; sibling `worlds\checksmate\locations.py:174-185` |
| Capture goals use shared global names, not per-stage copies. Stage rules control availability | Sibling `worlds\checksmate\locations.py:345-429` |
| Existing series cover 2-12 Pawns, 2-11 Pieces, 2-11 Of Each, and Any 2-22, plus Capture Everything | Sibling `worlds\checksmate\locations.py:194-293` |
| The counter path classifies a capture by its original rank. It has no separate King exclusion and misclassifies new forward non-pawns as pawns | `APMW.Client\LocationHandler.cs:512-568` |
| Despite its name, `CpuNonKingCount = Files - 1` means the old non-pawn, non-King home-row count, not the total non-King army count | `APMW.Client\CaptureLookup.cs:58-60` |
| Current Capture Everything eligibility uses the goal mode and selected legacy geometry conditions, not one consistent configured-endpoint rule | `APMW.Client\LocationHandler.cs:861-883` |

The unchanged Standard home rows are `R N B Q K B N R` on 8x8 and
`R N Archbishop B Q K B Chancellor N R` on 10x8.
Each has one full pawn row. The approved augmented arrays remain in tickets
12 and 13, not in the current production composer.

Source: `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:378-409`.

## Implementation and handoff requirements

- Bind every starting CPU unit to the catalog role, including the two King
  roles. Preserve that identity through movement, promotion, and restoration.
- Derive counts from the exact formation, not board width or home-rank tests.
- Generate the configured world's individual and series Locations from the
  role-presence and endpoint rules. Report only members of that set.
- Complete every crossed threshold once for a committed multi-capture.
  Rejected moves must not advance counters or publish checks.
- Preserve counter and role-lineage state through supported undo/load paths.
  Ticket 18 must carry the explicit capture fixtures into acceptance.
- Assign deterministic numeric IDs in ChecksMate and freeze them in the
  published contract. The 0.4.0 boundary requires neither legacy aliases nor
  universal renumbering.
- Reject legacy Location profiles under ADR 0005. Include exactly the five
  supported geometries and no 12x12 mapping.
- Keep authored calibration lineage separate from numeric-ID reuse. The
  unresolved function and baseline choices remain in ticket 16.
