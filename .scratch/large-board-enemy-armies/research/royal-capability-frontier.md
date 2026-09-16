# Royal lifecycle and goal-capability findings

Status: Source investigation complete. Q33 selects the explicit CPU
promotion lists.

The selected royal policy is not the current implementation.
This report separates reusable code from behavior that must change.
No production code or gameplay tests changed during this investigation.

## Royal membership and phase selection

`ApmwChessGame.AddRules` chooses Extinction or Covenant through the human's
King-upgrade count. It then installs an `ApmwStalemateRule` with that selection.
This does not independently define the CPU's new ordinary/Mounted King set.

Source: `ChessV.Games\MiscellaneousGames\ApmwChess.cs:181-202`.

`ApmwStalemateRule.MoveBeingMade` returns immediately for a human mover.
Its royal-removal branch instead handles a CPU capture of an opposing royal.
Thus, the existing branch does not establish correct CPU membership after
the human captures a CPU King.
Its unmake branch has the same mover distinction.

Source: `ChessV.Games\Rules\Apmw\ApmwStalemateRule.cs:28-55`.

The inherited `IllegalCheckMoves` checks every royal of the moving side.
It does not implement the selected no-check-obligation phase with multiple
CPU Kings.
`NoMovesResult` returns the undeclared literal zero for multiple royals.
Its one-royal lookup also cannot supply the required zero-King result.

Source: `ChessV.Games\Rules\CheckmateRule.cs:97-127`.

`CheckmateRule.PositionLoaded` adds pieces to existing royal sets without
first clearing them.
A repeated load therefore needs explicit reconstruction.
`ApmwStalemateRule` takes its starting-royal snapshot after that base call.

Sources: `ChessV.Games\Rules\CheckmateRule.cs:72-85`;
`ChessV.Games\Rules\Apmw\ApmwStalemateRule.cs:20-26`.

The required change is CPU-scoped behavior under the accepted policy.
It is not a global redesign of ordinary chess or human multi-King behavior.
CPU identity must not depend on which human King upgrades are present.

## Extinction and terminal order

`ExtinctionRule` loses on extinction of any configured type.
`CovenantRule` instead loses when all configured types are extinct.
The latter supplies a reusable all-types condition, not the entire CPU
phase contract.

Sources: `ChessV.Games\Rules\Extinction\ExtinctionRule.cs:58-68`;
`ChessV.Games\Rules\Extinction\CovenantRule.cs:14-26`.

The committed-move path evaluates win/loss before its no-move result.
`GameWon` identifies the current side, not invariably the CPU.
The implementation must preserve extinction precedence and use the correct
winner for either CPU color.

Source: `ChessV.Base\Game.cs:1634-1676`.

The committed path sends the APMW setup notification before
`moveLists[1].MakeMove`.
A false result then throws before the normal committed notifications.
A successful undo or an illegal search probe cannot prove that this failure
path restores all state.

Source: `ChessV.Base\Game.cs:1591-1604`.

## Castling

The existing castling component caches whether a checkmate rule exists.
Its attack-path restriction depends on that flag.
The new rule composition must preserve the selected attack restriction in
both CPU phases.

Sources: `ChessV.Games\Rules\CastlingRule.cs:92-97,254-282`.

The component records square-based rights and reconstructs them from FEN.
Those rights do not, by themselves, prove original-actor identity after a
substitution or malformed load.
The accepted primary/corner identities and the no-inheritance policy require
their own fixtures.

Sources: `ChessV.Games\Rules\CastlingRule.cs:98-115,128-153,285-298`;
[castling contract](../issues/14-set-royal-and-castling-semantics.md#2026-09-13-castling-keeps-normal-attack-restrictions).

The 6x8 capability remains disabled.
Neither a cost snapshot nor a zero material estimate creates a castling path.

## Piece registration and existing evaluation policy

Basic Elephant exists as a movement atom, but not in the examined APMW
catalog. War Elephant is a different existing catalog entry.
The new CPU Elephant requires an explicit catalog registration.

Sources: `ChessV.Games\Pieces\MovementAtoms.cs:63-79`;
`ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:20-63`.

The catalog already contains Lion, Amazon, and Mounted King.
Its Queen-family tactical set excludes Amazon.
The client already implements `Threaten Queen` and royal forks.
Those features are not absent merely because there are no concatenated
`ThreatenQueen` or `RoyalFork` method names.

Sources: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:21,39,47,80-86`;
`APMW.Client\LocationHandler.cs:633-646,704-728`.

Existing outpost registration includes Lion and Mounted King.
Its availability filter depends on the human-oriented promotion string.
CPU-required registrations must receive applicable existing evaluation
hooks without requiring human promotion entitlement.

Source: `ChessV.Games\MiscellaneousGames\ApmwChess.cs:257-301`.

The back-rank trapping bonus deliberately uses an ordinary King anchor.
The source explains that a Mounted King can escape the back rank.
Recognizing Mounted King for royal survival and fork Locations does not
require changing this separate evaluation policy.

Source: `ChessV.Games\Evaluations\RookTypeEvaluation.cs:164-175`.

The engineering default is no new evaluation tuning.
Existing Lion/Mounted King hooks remain applicable when those types are
registered. Basic Elephant and Amazon do not gain invented outpost or
rook bonuses from their new starting roles.
The existing generic material and piece-square behavior still applies.
Q35 confirms this default for the later implementation handoff.

Q34 selects Minor for the basic Elephant tactical class.
It grants no human random-pool or promotion permission.
The [royal fixture specification](../contracts/royal-lifecycle-fixtures.md#basic-elephant-threat-fixture)
contains the isolated threat case.
The new class does not change the starting capture role or ordinary fork
rules.

## Compact Queen tactics: classification versus availability

Standard 6x8 starts with `nbrkbn` and no Queen.
Its CPU profile declares `rnbq` promotions.
The other compact families can already start a Queen-family counterpart in
the Center Rook role.

Source: `ChessV.Games\MiscellaneousGames\ApmwProfiles.cs:362-375`.

The client classifies tactical Queen targets by their current concrete type.
It does not require a Queen starting role.
A CPU pawn promoted to a Queen can supply `Threaten Queen` and a royal-fork
target without creating an individual starting-Queen capture Location.

Source: `APMW.Client\LocationHandler.cs:620-646,704-728`.

But the profile's `rnbq` string alone does not guarantee the current promotion
path.
`SetOtherVariables` combines human, pocket, and CPU promotion strings.
It omits the main CPU fragment when the raw `EnemyArmy` property is null,
even though the family resolver selects Standard.
`GenericChess` starts `PromotionTypes` as an empty string.

Sources: `ChessV.Games\MiscellaneousGames\ApmwChess.cs:389-406`;
`ChessV.Games\Abstract\GenericChess.cs:100-102`.

Queen registration is a separate matter.
`AddPieceTypes` starts its registration filter with `KQRBNP`.
That makes Queen loadable but does not add Queen to the promotion list.

Source: `ChessV.Games\MiscellaneousGames\ApmwChess.cs:329-359`.

`GenericChess.AddRules` passes the resulting list into `BasicPromotionRule`.
That rule selects by the moving piece's type and relative destination rank.
It does not choose a different promotion list for the CPU and human.

Sources: `ChessV.Games\Abstract\GenericChess.cs:117-125`;
`ChessV.Games\Rules\BasicPromotionRule.cs:48-98`.

Thus, the current shared list can borrow human entitlements for the CPU or
CPU fragments for the human.
It also makes compact promotion capability depend on setup details outside
the selected CPU family.
A new CPU-only type must not enter that list merely to become loadable.

## Promotion decision selected in Q33

The selected rule is a CPU promotion list from its resolved family and
geometry, independent of human inventory and pocket contents.
Absent or unknown family input uses the same Standard list as explicit
Standard.

The selected lists retain family promotions and ten-file attendants.
They do not turn newly registered pieces into new promotion permissions.

| Family | 6x8 and 8x8 | Additions on 10x8, 10x10, and 12x10 |
| --- | --- | --- |
| Standard | Rook, Knight, Bishop, Queen | Archbishop, Chancellor |
| Colourbound Clobberers | Cleric, Phoenix, War Elephant, Archbishop | Queen, Chancellor |
| Remarkable Rookies | Short Rook, Tower, Lion, Chancellor | Archbishop, Queen |
| Nutty Knights | Charging Rook, Lancer, Charging Knight, Colonel | Archbishop, Chancellor |

The selected first-release lists exclude basic Elephant and the removed
legacy12 Nightrider extension.
Lion remains a Rookies promotion target through its existing family list.
The outer Lion pair does not add Lion promotion to other families.
The accepted Amazon and royal promotion exclusions remain unchanged.

Ticket 11 previously left basic Elephant promotion optional.
Q33 excludes it for the first release and accepts the listed sets.
[ADR 0017](../../../docs/adr/0017-separate-cpu-promotion-permissions.md)
records the decision.
Production changes still require the completed map and execution handoff.

## Independent compact tactic fixtures

These are logical acceptance positions, not completed gameplay tests.
All unlisted squares are empty. Pockets are empty.
Both castling and en-passant rights are absent.
The CPU is White and the human is Black on 6x8.

| Fixture | Initial pieces and state | Operation order | Expected output |
| --- | --- | --- | --- |
| CPU Queen promotion and threat | CPU King D1, Knight D8, Pawn B7 originating B2. Human King F8 and Rook A6. CPU to move, with the selected Standard promotion list | CPU plays B7-B8=Queen. Human plays A6-B6 | The human threatens the Queen on B8. That target retains its starting pawn identity. It does not become an individual starting-Queen capture |
| Royal fork of a promoted Queen | CPU King F3 and Queen B3 originating from the pawn at B2. Human King F8 and Knight F5. Human to move | Human plays F5-D4 | The Knight attacks F3 and B3. Both royal-fork Locations qualify under the existing true-fork test. No starting Queen role is required |

The first fixture holds a CPU Knight on D8 to block the promoted Queen's
line toward the human King.
Removing that blocker changes the legal position and is not the same case.
Promotion-list construction must have its own fixture before these positions
can establish an ordinary Standard 6x8 path.
