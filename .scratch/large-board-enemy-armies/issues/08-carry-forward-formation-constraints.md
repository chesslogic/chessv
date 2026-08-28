# Carry forward the settled formation constraints

Type: grilling  
Status: resolved

## Question

Which spatial constraints are settled even though the exact supported 10x10 and 12x10 arrays are not?

## Answer

For the white-side 10x10 concept:

- A1's rook has a pawn on A2.
- B1's knight is defended by B2 and B3.
- A pawn wedge continues inward through C3, D3, E3, and F3, mirrored from the other edge.
- The spaces created forward of the home rank hold family-corresponding Tower/Knight/Bishop-role pieces.

The mirror instruction is intentionally not normalized here: mirroring `{B3,C3,D3,E3,F3}` on ten files yields `{E3,F3,G3,H3,I3}`, overlapping at E3/F3.

For the supported 12x10 geometry:

- added files are treated as insertion from the middle outward;
- the forward Knight/Bishop order swaps where needed so same-color Bishops protect one another;
- the current standard profile places White's Queen on file F and King on file G;
- opposing primary Kings align on the chosen king file, so the White primary-King hypothesis is around G1;
- a single Amazon in the normal queen/home-rank role is hypothesized around F1;
- a displaced/forward Queen is hypothesized around F2;
- the exact six-pawn wedge is unresolved.

The user's file correction supersedes the earlier “around G1/G2” wording for the Amazon/Queen roles: the queen-role file is F, while the primary-King file is G. F1 Amazon-role, F2 forward-Queen-role, and G1 primary-King-role are still hypotheses rather than settled occupancy; the exact 12x10 arrays must resolve them. The earlier two-King rank-9 exploration is superseded. These constraints do not establish 12x12 support.

## Provenance

- Current user formation decisions and refinements supplied for this map.
- `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs` currently computes the twelve-file king file as file G; the current standard profile has the Queen on file F.
