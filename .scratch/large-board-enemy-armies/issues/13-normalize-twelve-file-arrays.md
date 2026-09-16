# Normalize exact 12x10 placement arrays

Type: prototype  
Status: resolved  
Blocked by: 11, 12

## Question

What exact per-family coordinate arrays should 12x10 use after middle-out file insertion, and does the added file width change the CPU inventory or only its spacing?

## Answer

### 2026-09-13: Fourteen-pawn footprint

The user adopted this exact footprint:

```text
files:   A B C D E F G H I J K L
rank 3:  - P P P P P P P P P P -
rank 2:  P P - - - - - - - - P P
```

There are fourteen pawns: `A2`, `B2`, `K2`, `L2`, and `B3` through `K3`.
`F3/G3` each contain one pawn, not duplicate occupants from overlapping
six-pawn wedge descriptions. The rear members are `B2/K2`, behind `B3/K3`.
The opposite color preserves files and reflects zero-based rank `r` to `9-r`.
Black's pawn squares are `A9`, `B9`, `K9`, `L9`, and `B8` through `K8`.

### 2026-09-13: No Nightrider pair

The user omitted the two Nightriders from the new twelve-file inventory.
Widening does not add those pieces, despite their presence in the current
runtime's twelve-file profile.

The subsequent layout proposal retained the approved 10x10 pieces, moved the
existing family Queen forward, upgraded the primary King to Mounted King,
and added one Amazon, one additional ordinary King, and two pawns.
Under those assumptions, it had 32 pieces and Standard raw midgame material
of 10600. Those counts describe the proposal, not a final approved inventory.

The user then revised its home row as recorded below. The final inventory,
Queen count, and catalog totals must follow the approved replacement array.
No total here sets a Location threshold.

### 2026-09-13: Revise the home row and research B1

The user required Bishop-role at `E1` and Knight-role at `D1`, then left `B1`
open for a piece inspired by other twelve-file games without "really
outlandish pieces."

For Standard, these are a Bishop and Knight. The settled family correspondence
still applies to other families. Under the paired wing structure, their
opposite-wing counterparts are `H1` and `I1`; the unresolved pair is `B1/K1`.

The prior proposal with home Bishop-role on `D1/I1` and gaps at `E1/H1` is not
approved. Nor does the correction approve the unmentioned center squares or
the complete remainder of that proposal.

The research must distinguish inspiration from a new piece decision.
The rejected Nightrider pair stays excluded. Whether the replacement B1/K1
choice adds inventory or replaces other pieces remains explicit and open.

[Twelve-file piece inspiration](../research/twelve-file-piece-inspiration.md)
records the source-backed candidates and their movement and material facts.
Janus Kamil Chess supplies a close 12x10 precedent for the requested D/E roles
and plain Camels. King's Court matches the B/D/E placement but uses a
multi-path Jester. Neither precedent selects the new B1/K1 piece.

With Bishop-role at `E1/H1`, the forward B/N order must be reconsidered to
retain the intended color-complex coordination. Exact forward squares remain
open.

### 2026-09-13: Add a Champion pair (superseded)

The user selected Champion for `B1/K1` and chose to add the pair while
retaining the other agreed pieces and fourteen pawns.

Champion moves one square orthogonally, or leaps exactly two squares
orthogonally or diagonally. It is the `Champion` movement from Gross Chess,
not the existing Mounted King's movement or royal role.

Existing prices differ: Omega uses 375/375, TenCubed uses 475/475, and Gross
uses 600/600. The user initially adopted 475/475 and one universal Champion
pair for all four families. The later Lion decision below supersedes that
piece and price choice.

The Champion proposal had an icon collision: the current APMW
Mounted King requests the image named `Champion`. The two concrete piece
types needed unambiguous notation and images. Replacing the new pair with
existing Lions removes that Champion-specific registration and image work.
The Mounted King's image does not need to change for this reason.

Sources: `ChessV.Games\Pieces\MiscellaneousCompounds.cs:151-166`;
`ChessV.Games\12x12\GrossChess.cs:74-75`;
`ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:21-23`.

### 2026-09-13: Replace the pair with existing Lions

The user changed the B1/K1 choice from Champion to Lion:

> I changed my mind about the Champion piece, I really like the design of the
> Lion, let's use that instead.

Use the existing APMW Lion at 500/500 for the universal pair. Its movement is
a one-square diagonal step or a two- or three-square orthogonal leap.
All other approved squares and pieces remain unchanged.

The Rookies retain their four Lion Bishop-correspondents in addition to this
outer pair. That family therefore has six Lions, not two. Starting roles
distinguish their capture identities.

Source: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:40`;
`ChessV.Games\Pieces\CwDA.cs:25-41`.

### Final four-family array

The exact role arrangement is:

```text
files:   A  B  C  D  E  F  G  H  I  J  K  L
rank 3:  .  P  P  P  P  P  P  P  P  P  P  .
rank 2:  P  P El  B  N  Q  K  N  B El  P  P
rank 1:  R Li At  N  B Am MK  B  N At Li  R
```

`B/N/R` are family correspondence roles. `At` denotes the existing home
attendant type specified in the concrete tables, not a new piece type.
`Q` is the displaced family Queen. `Li` is Lion, `El` is basic Elephant,
`Am` is literal Amazon, and `MK` is the primary Mounted King.
These labels do not select new FEN notation.

Use the home table on White rank 1 or Black rank 10. Use the forward table
on White rank 2 or Black rank 9. File columns never reverse.

#### Exact home row

| Family | A | B | C | D | E | F | G | H | I | J | K | L |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Standard (FIDE) | Rook | Lion | Archbishop | Knight | Bishop | Amazon | Mounted King | Bishop | Knight | Chancellor | Lion | Rook |
| Colourbound Clobberers | Cleric | Lion | Queen | Phoenix | War Elephant | Amazon | Mounted King | War Elephant | Phoenix | Chancellor | Lion | Cleric |
| Remarkable Rookies | Short Rook | Lion | Archbishop | Tower | Lion | Amazon | Mounted King | Lion | Tower | Queen | Lion | Short Rook |
| Nutty Knights | Charging Rook | Lion | Archbishop | Lancer | Charging Knight | Amazon | Mounted King | Charging Knight | Lancer | Chancellor | Lion | Charging Rook |

#### Exact forward row

| Family | A | B | C | D | E | F | G | H | I | J | K | L |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Standard (FIDE) | Pawn | Pawn | Basic Elephant | Bishop | Knight | Queen | King | Knight | Bishop | Basic Elephant | Pawn | Pawn |
| Colourbound Clobberers | Pawn | Pawn | Basic Elephant | War Elephant | Phoenix | Archbishop | King | Phoenix | War Elephant | Basic Elephant | Pawn | Pawn |
| Remarkable Rookies | Pawn | Pawn | Basic Elephant | Lion | Tower | Chancellor | King | Tower | Lion | Basic Elephant | Pawn | Pawn |
| Nutty Knights | Pawn | Pawn | Basic Elephant | Charging Knight | Lancer | Colonel | King | Lancer | Charging Knight | Basic Elephant | Pawn | Pawn |

`Lancer` is `NarrowKnight`. Each basic Elephant is 250/250.
Every Lion is 500/500. No Champion or Nightrider appears in the array.

#### Exact third row and empty squares

All families use this row on White rank 3 or Black rank 8:

| A | B | C | D | E | F | G | H | I | J | K | L |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Empty | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Pawn | Empty |

The only empty squares within the CPU band are White `A3/L3` or Black
`A8/L8`. The CPU layer is also empty outside its three ranks. That does not
declare the human formation empty. The neutral rank remains empty at setup.

#### Center and Queen identity

White has Amazon `F1`, the existing family Queen `F2`, primary Mounted King
`G1`, and additional ordinary King `G2`. Black uses `F10`, `F9`, `G10`, and
`G9`, respectively.

The existing family Queen moves forward; it is not duplicated. There is one
forward Queen-role piece and one Amazon in each family.

Literal Queen counts differ from that role count:

| Family | Literal Queen count and square for White |
| --- | --- |
| Standard (FIDE) | One, F2 |
| Colourbound Clobberers | One, C1; the forward Queen-role piece is Archbishop |
| Remarkable Rookies | One, J1; the forward Queen-role piece is Chancellor |
| Nutty Knights | Zero; the forward Queen-role piece is Colonel |

The two Kings use the accepted extinction/checkmate phases. Lion and
Amazon do not contribute King lives. Only the original primary Mounted King
owns the CPU castling role.

#### Counts and midgame material

Every family has 34 pieces: 14 pawns, two Kings, and 18 other pieces.
The non-King count is 32. There is one occupancy per listed square.

| Family | Approved 10x10 MG including King | Approved 12x10 MG including both Kings | 12x10 MG excluding both Kings | Delta from approved 10x10 |
| --- | ---: | ---: | ---: | ---: |
| Standard (FIDE) | 8400 | 11600 | 10575 | 3200 |
| Colourbound Clobberers | 8860 | 12060 | 11035 | 3200 |
| Remarkable Rookies | 8900 | 12100 | 11075 | 3200 |
| Nutty Knights | 8550 | 11750 | 10725 | 3200 |

The delta is two pawns (200), two Lions (1000), one Amazon (1300), the
primary King upgrade (375), and the additional ordinary King (325).
Moving the existing family Queen does not add another Queen's value.
These are raw catalog totals, not released Location thresholds.

#### Color and defense assertions

- Home Bishop-role `E1/H1` and forward Bishop-role `D2/I2` occupy matching
  color complexes. Each pair covers opposite square colors.
- Standard Bishops, War Elephants, and Lions support the adjacent diagonal
  connections `E1 <-> D2` and `H1 <-> I2`.
- Forward Charging Knights can defend the corresponding home pieces by
  backward diagonal steps. Mutual defense is not claimed for that family.
- Basic Elephants on `C2/J2` and Lions on `B1/K1` each start on opposite
  square colors. Lion is not colorbound.
- `B2/B3` and `K2/K3` remain pawn screens. Rear-pawn identity is capture-map
  metadata, not a new movement property.
- Opposite-side conversion reflects zero-based rank `r` to `9-r` and
  preserves files. It preserves the paired color relationships and aligns
  both primary Kings on file G.

This is a CPU setup decision. It does not add human progression-pool entries
or promotion permissions merely because these concrete types must be
registered for deployment.

## Earlier center hypotheses

The prototype must resolve the current center conflict rather than infer a square from prose:

```text
12 files: A B C D E F G H I J K L
current standard profile: Queen file F, King file G
working hypotheses to normalize:
  Amazon role        -> normal queen/home-rank role around F1
  forward Queen role -> displaced/forward role around F2
  primary King role  -> aligned on the king file around G1
```

This corrects and supersedes the prior Amazon/Queen “around G1/G2” wording. These coordinates express the intended role regions, not final occupancy: the prototype must test them against inventory, collision, defense, color-complex, and royal constraints before declaring exact squares.

The final array above resolves these hypotheses and the forward B/N order.

## Resolution requirements

- Exact white and black arrays for 12x10, for all four families.
- Exactly one primary King square, an explicit Amazon square, and an explicit forward-Queen square with no collision.
- A decision confirming or revising the F1 Amazon-role, F2 forward-Queen-role, and G1 primary-King-role hypotheses, with the reason visible in the prototype.
- Exact Queen count and whether Amazon replaces, moves, or supplements an existing queen-tier piece.
- Exact pawn/non-king/royal counts and catalog totals per family for the 12x10 stage.
- Color-complex, mutual-defense, and center-insertion assertions.

## Comments

### 2026-09-13: Capture the corrected Lion prototype

The approved interactive prototype is preserved on the local throwaway branch
`prototype-large-board-armies-715de4c1`, commit
`777e2c778ed7d26feae525da9415d62af3d5c469`, at
`.scratch\large-board-enemy-armies\twelve-by-ten-armies.prototype.html`.

It uses the corrected Lion pair and existing 500/500 values. The branch also
contains the earlier approved 10x10 prototype. Neither prototype is production
implementation.
