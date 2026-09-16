# Normalize exact 10x10 placement arrays

Type: prototype  
Status: resolved  
Blocked by: 11, 19

## Question

What exact collision-free white-side 10x10 coordinate array satisfies the settled wedge, pawn-screen, piece-budget, and family-correspondence constraints, and what is the mechanically mirrored black-side array?

The approved arrays below cover all four families and both colors.

The original overlapping wedge descriptions were:

```text
files:       A  B  C  D  E  F  G  H  I  J
left wedge:     B3 C3 D3 E3 F3
right mirror:            E3 F3 G3 H3 I3
intersection:            E3 F3
```

## Answer

### 2026-09-13: Twelve-pawn footprint

The user adopted this exact footprint for all four families:

```text
files:   A B C D E F G H I J
rank 3:  - P P P P P P P P -
rank 2:  P P - - - - - - P P
```

Here `-` means no pawn, not an approved empty square in the final array.
There are exactly twelve pawns: `A2`, `B2`, `I2`, `J2`, and `B3` through `I3`.
The overlap at `E3/F3` contributes one pawn per square. `B2/I2` form the rear
pair behind `B3/I3`; canonical Location names remain open.

The opposite color preserves files and reflects zero-based rank `r` to `9-r`.
Its board-coordinate pawn squares are `A9`, `B9`, `I9`, `J9`, and `B8` through
`I8`.

The subsequent candidate-C decision fixes the remaining non-pawns and retains
the current ten-file home ranks.

### 2026-09-13: Candidate C for all four families

The user adopted candidate C, including unchanged home ranks.
On `C2-H2`, the added roles are:

```text
Basic Elephant, Knight-role, Bishop-role, Bishop-role, Knight-role, Basic Elephant
```

Every `Basic Elephant` below is the existing Elephant movement atom at
`250/250`, not War Elephant. These are semantic piece names, not new FEN
notation assignments.

The tables apply to White on home rank 1 and forward rank 2. For Black, use
home rank 10 and forward rank 9 without reversing the file columns.

#### Exact home row

| Family | A | B | C | D | E | F | G | H | I | J |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Standard (FIDE) | Rook | Knight | Archbishop | Bishop | Queen | King | Bishop | Chancellor | Knight | Rook |
| Colourbound Clobberers | Cleric | Phoenix | Queen | War Elephant | Archbishop | King | War Elephant | Chancellor | Phoenix | Cleric |
| Remarkable Rookies | Short Rook | Tower | Archbishop | Lion | Chancellor | King | Lion | Queen | Tower | Short Rook |
| Nutty Knights | Charging Rook | Lancer | Archbishop | Charging Knight | Colonel | King | Charging Knight | Chancellor | Lancer | Charging Rook |

`Lancer` is the APMW display name for `NarrowKnight`.

#### Exact forward row

| Family | A | B | C | D | E | F | G | H | I | J |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Standard (FIDE) | Pawn | Pawn | Basic Elephant | Knight | Bishop | Bishop | Knight | Basic Elephant | Pawn | Pawn |
| Colourbound Clobberers | Pawn | Pawn | Basic Elephant | Phoenix | War Elephant | War Elephant | Phoenix | Basic Elephant | Pawn | Pawn |
| Remarkable Rookies | Pawn | Pawn | Basic Elephant | Tower | Lion | Lion | Tower | Basic Elephant | Pawn | Pawn |
| Nutty Knights | Pawn | Pawn | Basic Elephant | Lancer | Charging Knight | Charging Knight | Lancer | Basic Elephant | Pawn | Pawn |

#### Exact third row and empty squares

All families use this row on White rank 3 or Black rank 8:

| A | B | C | D | E | F | G | H | I | J |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Empty | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Empty |

Within the three CPU ranks, the only empty White squares are `A3/J3`.
For Black, they are `A8/J8`.

The CPU layer has no pieces outside its three ranks. White's CPU-layer empty
set also includes every square on ranks 4-10. Black's includes ranks 1-7.
Those layer blanks do not declare the human formation empty. The neutral rank
remains empty at setup under the separate formation contract.

#### Counts and catalog material

Each family has 28 pieces: 12 pawns, 15 non-King non-pawns, and one ordinary
King. The non-King count is 27. The primary King is on `F1` for White and
`F10` for Black. The footprint contains no collisions.

| Family | Prior army MG, including King | Approved army MG, including King | Approved army MG, excluding King | Augmentation delta MG |
| --- | ---: | ---: | ---: | ---: |
| Standard (FIDE) | 6400 | 8400 | 8075 | 2000 |
| Colourbound Clobberers | 6580 | 8860 | 8535 | 2280 |
| Remarkable Rookies | 6550 | 8900 | 8575 | 2350 |
| Nutty Knights | 6470 | 8550 | 8225 | 2080 |

The prior army is the unchanged home row plus ten pawns. The augmentation
delta is six added non-pawns plus two added pawns, not all twelve pawns.
MG denotes catalog midgame material. The ordinary King contributes 325.
These are inventory totals, not released Location thresholds.

#### Screen, defense, and color assertions

- The pawns on `B2/B3` screen the home Knight-role on `B1`. The mirror is
  `I2/I3` screening `I1`. The screen is not direct pawn defense of rank 1.
- The added Bishop-role pair is on `E2/F2`, on opposite square colors.
  `E2` shares the color of home `D1`; `F2` shares the color of home `G1`.
- Standard Bishops, War Elephants, and Lions support the adjacent diagonal
  connections `D1 <-> E2` and `G1 <-> F2`.
- A forward Charging Knight can defend the corresponding home piece by a
  backward diagonal step. The reverse connection is not a Charging Knight
  move; mutual defense is not claimed for that non-colorbound family.
- The basic Elephants on `C2/H2` occupy opposite square colors.
- Standard Knights on `D2/G2` defend wedge pawns `B3/F3` and `E3/I3`,
  respectively. Other correspondents retain their own literal movement.
- File-preserving rank reflection retains each pair's color relationship for
  Black. It reverses each member's absolute square color on a ten-rank board.

The current runtime cannot yet compose this three-rank CPU layout. Basic
Elephant also needs a collision-free APMW notation and icon. Those are
implementation obligations, not reasons to alter the approved semantic array.

Source facts: `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:394-418`;
`ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:21-46`;
`ChessV.Games\Pieces\CwDA.cs:25-224`.

Minimum pinned facts:

```text
A1 = home rook-role piece
A2 = pawn
B1 = home knight-role piece
B2 = pawn
B3 = pawn
```

## Resolution requirements

- Exact ordered coordinates and piece types for each family.
- Exactly one occupancy per square and a declared empty-square set.
- Exact pawn and non-king counts.
- Symmetry transform for the opposite side.
- Pawn-screen assertions for B1. B2 and B3 do not directly defend B1.
- Attack/defense assertions for the intended forward pieces.
- Bishop color-complex assertions where a family uses colorbound correspondents.
- A catalog material total per family.

## Comments

### 2026-09-13: Claimed for the layout prototype

The user confirmed the B1/B2/B3 relationship as a pawn screen, not direct
defense. The same session approved the twelve-pawn footprint and then
candidate C for all four families, including the unchanged home ranks.

### 2026-09-13: Prototype capture

The interactive primary-source prototype is preserved on the local throwaway
branch `prototype-ten-by-ten-715de4c1`, commit
`285d59741917bde4f9ed143ca85e61fe7d451c05`, at
`.scratch\large-board-enemy-armies\ten-by-ten-armies.prototype.html`.

Its initial state is approved candidate C. Candidates A and B remain marked
as unapproved comparison cases. The prototype branch contains no production
implementation.
