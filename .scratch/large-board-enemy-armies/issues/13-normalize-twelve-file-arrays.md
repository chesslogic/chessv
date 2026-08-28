# Normalize exact 12x10 placement arrays

Type: prototype  
Status: open  
Blocked by: 11, 12

## Question

What exact per-family coordinate arrays should 12x10 use after middle-out file insertion, and does the added file width change the CPU inventory or only its spacing?

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

It must also provide the exact six-pawn wedge and the forward Knight/Bishop order that lets same-color Bishops protect one another.

## Resolution requirements

- Exact white and black arrays for 12x10, for all four families.
- Exactly one primary King square, an explicit Amazon square, and an explicit forward-Queen square with no collision.
- A decision confirming or revising the F1 Amazon-role, F2 forward-Queen-role, and G1 primary-King-role hypotheses, with the reason visible in the prototype.
- Exact Queen count and whether Amazon replaces, moves, or supplements an existing queen-tier piece.
- Exact pawn/non-king/royal counts and catalog totals per family for the 12x10 stage.
- Color-complex, mutual-defense, and center-insertion assertions.
