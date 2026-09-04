# Specify the family correspondence matrix

Type: grilling  
Status: resolved
Blocked by: 02, 04, 07, 08

## Question

For every added 10x10 and 12x10 formation role, which exact piece type represents that role in Standard, Colourbound Clobberers, Remarkable Rookies, and Nutty Knights, and what value/movement tolerance makes two pieces valid correspondents?

The answer must provide a complete matrix for:

- each added minor-role slot;
- each Tower/Gardener-role slot;
- Queen and Amazon roles;
- any primary or additional royal role;
- any castler-like role;
- promotion fragments if the new starting piece must also become a CPU promotion option.

It must also decide whether “10x10 uses Towers” means a literal Tower in Standard only, a shared cross-family role anchored to Tower value/movement, or a literal Tower in every family despite the family-preservation rule.

## Resolution requirements

- Name exact catalog piece types, not tiers alone.
- State the comparison basis: midgame value, endgame value, weighted value, movement role, colorbinding, or an explicit combination.
- Identify any family slot that has no acceptable existing analogue and choose whether to reuse, relax the tolerance, or reject the candidate formation.
- Keep player `major_to_amazon` lineage separate from CPU correspondence authority.

## Answer

Correspondence follows the selected CPU army's established home-rank roles before
it follows exact material equality. Each added pair uses two instances of the same
piece type, so the pair has identical material values. Where the pair is
colorbound, the placement arrays must put one member on each color complex and
align each forward member with the home colorbound piece it is intended to
coordinate with.

| Added role | Standard | Colourbound Clobberers | Remarkable Rookies | Nutty Knights |
|---|---|---|---|---|
| Bishop-role pair | Bishop | War Elephant | Lion | Charging Knight |
| Knight-role pair | Knight | Phoenix | Tower | Lancer |
| Forward Queen role | Queen | Archbishop | Chancellor | Colonel |

The third 10x10 pair is not family-specific in the first release. Every family
uses two basic Elephants, valued at exactly 250/250 each. The Elephants start on
opposite square colors. This is the first-release interpretation of the earlier
weaker-Tower/weaker-Phoenix concept and replaces the rejected 315-325
family-specific light-piece matrix.

For 12x10, every family uses:

- one literal Amazon in the home-rank Amazon role;
- its family-specific Queen correspondent from the table in the displaced
  forward Queen role;
- one primary Mounted King; and
- one additional ordinary King.

The later rules ticket decides which King instances are royal and how extinction,
check, stalemate, and castling interact. This ticket fixes their piece identities
only.

The existing two CPU corner slots remain the only CPU castler roles. A forward
Tower, Elephant, Lion, additional King, or other augmented piece does not acquire
castling rights from its piece type. Preserve the existing player rule separately:
a player back-rank Major or Jack may be a castler.

Amazon is never a promotion option for either side. The basic Elephant may be
added as a promotion option; existing non-royal family promotion fragments remain
valid. Mounted and ordinary Kings remain outside ordinary pawn promotion.

Any piece registration added to APMW must use notation and an icon not already
assigned in the game. The possible future family-specific 250-material set is
outside the first-release handoff.

## Comparison basis

1. Preserve the selected army's established movement role and identity.
2. Preserve pair equality and color-complex intent within an army.
3. Use catalog material as a calibration input, not as permission to substitute a
   differently behaving family piece.
4. Treat the universal Elephant pair, Amazon, Mounted King, and ordinary King as
   explicit geometry-augmentation exceptions.

## Provenance

- Current user decisions in the resolution session.
- `ChessV.Games\MiscellaneousGames\ApmwChess.cs` for catalog values, family sets,
  promotion sets, and player Major/Jack castling eligibility.
- `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs` for current CPU family
  placement and corner castlers.
- `ChessV.Games\Pieces\MovementAtoms.cs` for Elephant movement.
- `ChessV.Games\Pieces\Apmw\MountedKing.cs` for Mounted King movement.
- ChessV PieceAnalysis results for the 10x10 candidate comparison.
