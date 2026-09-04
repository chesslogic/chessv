# Normalize exact 10x10 placement arrays

Type: prototype  
Status: open  
Blocked by: 11, 19

## Question

What exact collision-free white-side 10x10 coordinate array satisfies the settled wedge, defense, piece-budget, and family-correspondence constraints, and what is the mechanically mirrored black-side array?

The prototype must show all four families square by square, including empty squares. It must settle whether the two extra pawns are part of the rank-3 wedge, a second-rank pair, or another formation role.

The unresolved mirror collision is:

```text
files:       A  B  C  D  E  F  G  H  I  J
left wedge:     B3 C3 D3 E3 F3
right mirror:            E3 F3 G3 H3 I3
intersection:            E3 F3
```

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
- Attack/defense assertions for B1 and the intended forward pieces.
- Bishop color-complex assertions where a family uses colorbound correspondents.
- A catalog material total per family.
