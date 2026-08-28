# Use catalog facts without assuming rule compatibility

Type: research  
Status: resolved

## Question

Which piece and royal-rule facts are safe inputs to layout and balance decisions, and which behaviors still require review?

## Answer

Safe catalog facts:

- Tower is Wazir + Dabbabah and is valued 325/325.
- Scout is Wazir + Tribbabah and is valued 300/300.
- Bishop is 325/350; Knight 325/325; Pawn 100/125.
- Ordinary King is 325/325; Mounted King is King + Knight and 700/700.
- Amazon is Queen + Knight and the APMW catalog values it 1300/1300.
- Gardener is 250/250, so “Tower/Gardener tier” is a design band, not one exact catalog value.

These values do not settle rule compatibility. Base `CheckmateRule` can track multiple royal piece types, but APMW replaces that rule with Extinction/Covenant behavior and adds `ApmwStalemateRule`, including special handling for promoted extra kings and asymmetric human/CPU stalemate outcomes. APMW castling assumes one profile king file and corner castlers. Rook-like evaluation bonuses are registered for a selected set of piece types, not automatically for every rook-like candidate.

Therefore every added King, Mounted King, Amazon, Tower, or family analogue needs an explicit decision about royalty, check, capture, castling eligibility, and evaluation treatment.

## Provenance

- `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs`
- `ChessV.Games\Pieces\CwDA.cs`
- `ChessV.Games\Pieces\MiscellaneousCompounds.cs`
- `ChessV.Games\Pieces\Apmw\MountedKing.cs`
- `ChessV.Games\Pieces\ChessMissingCompounds.cs`
- `ChessV.Games\Rules\CheckmateRule.cs`
- `ChessV.Games\Rules\Apmw\ApmwStalemateRule.cs`
- `ChessV.Games\MiscellaneousGames\ApmwChess.cs`
