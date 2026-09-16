# Twelve-file piece inspiration

Current choice: the user superseded Champion with the existing APMW Lion at
500/500 for B1/K1. The other approved pieces remain unchanged.
[The array ticket](../issues/13-normalize-twelve-file-arrays.md) owns that
decision. The precedent survey below remains historical evidence, not an
instruction to restore Champion.

## Exact question

> "On the home rank, E should have a bishop, D should have a knight, and B should have ... something. Maybe we can look at other 12-file games and take inspiration from one of them, without using any of the really outlandish pieces."

Which existing, approachable pieces offer precedents for B1/K1 in the Standard 12x10 CPU array?
This report records precedents, not a piece selection or an approved array.

## Fixed inputs and exclusions

Authority for these inputs is the current user brief, not the precedent games:

- The target is 12 files by 10 ranks. Other twelve-file games supply inspiration only. The target does not support 12x12.
- E1 contains a Bishop and D1 contains a Knight. These instructions supersede the prior, unapproved twelve-file home row.
- The approved pawn footprint contains 14 pawns: A2, B2, K2, L2, and B3 through K3.
- The army has one Amazon, a forward family Queen, a primary Mounted King, and an ordinary King extraLife. No Nightrider pair.
- F1 Amazon / F2 Queen / G1 Mounted King / G2 ordinary King remains a proposal, not blanket approval.
- The resolved 10x10 candidate C remains `RNArBQKBChNR`, then `P P El N B B N El P P`, then `. P P P P P P P P .`.
- The four families retain their fixed Bishop/Knight correspondents. This research does not revise those mappings or assume B1/K1 adds rather than replaces inventory.

The geometry boundary also appears in `C:\GitHub\chessv\docs\adr\0002-reject-unsupported-twelve-by-twelve.md:1-16`.

## Source-backed variant table

Rows show White's actual placement, from file A through L. A dot means an empty square.
Symbols belong to each precedent game, not the proposed CPU array.
The reasons to consider a piece are research judgments, not material assessments.

| Variant and actual width | Home pieces and relevant placements | Candidate and reason to consider it | Local primary sources |
| --- | --- | --- | --- |
| **Janus Kamil Chess: 12x10** | Rank 1: `C R J N B Q K B N J R C`. Camels occupy A1/L1. Janus pieces occupy C1/J1. | **Camel** supplies a single, easy-to-describe leap. Janus is an Archbishop, not a new movement type. This precedent already places Knights at D1/I1 and Bishops at E1/H1. | `C:\GitHub\chessv\ChessV.Games\12x10\JanusKamilChess.cs:23-27`, `C:\GitHub\chessv\ChessV.Games\12x10\JanusKamilChess.cs:49-63`. |
| **ArchCourier Chess: 12x8** | Rank 1: `R H A B D Q K S B C H R`. H = Knight. A = ArchCourier at C1. D = Duke at E1. S = Squirrel at H1. C = Crowned Rook at J1. | **DragonHorse** (ArchCourier) and **DragonKing** (Crowned Rook) each combine a familiar slider with one-square steps. The Duke is a Centaur, another simple compound. | `C:\GitHub\chessv\ChessV.Games\12x8\ArchCourierChess.cs:23-27`, `C:\GitHub\chessv\ChessV.Games\12x8\ArchCourierChess.cs:48-71`. |
| **Gross Chess: 12x12** | Rank 1: `M A V W C . . C W V A M`. Rank 2: `. R S N B Q K B N S R .`. Wizards occupy D1/I1. Champions occupy C2/J2, **not the home rank**. M = Marshall, A = Archbishop, V = Vao, C = Cannon, S = Champion. | **Wizard** and **Champion** have genuine twelve-file precedent independent of Omega Chess. Their ordinary step/leap movement can stand apart from Gross Chess's other pieces and promotion rules. | `C:\GitHub\chessv\ChessV.Games\12x12\GrossChess.cs:25-30`, `C:\GitHub\chessv\ChessV.Games\12x12\GrossChess.cs:53-75`. |
| **Compound Courier Custom Chess: 12x8** | Rank 1: `C R N B G Q K M B N R C`. General occupies E1, Marshal H1, and Couriers A1/L1. Rank 2 is empty. | **General** supplies king-like steps without a new move pattern. Marshal is a Chancellor and Courier is an Archbishop. This script supplies precedent for familiar compounds rather than another exotic piece. | `C:\GitHub\chessv\Include\Compound Courier Custom Chess.cvc:2-10`, `C:\GitHub\chessv\Include\Compound Courier Custom Chess.cvc:18-23`. |
| **King's Court: 12x8** | Rank 1: `R J C N B Q K B N C J R`. Jesters occupy **B1/K1**, Chancellors C1/J1, Knights D1/I1, and Bishops E1/H1. | The placement closely matches the user's question, but **Jester fails the simple-movement filter**. Its companion Chancellor has simple, range-limited movement. Here that name does **not** mean the usual Rook+Knight compound. | `C:\GitHub\chessv\ChessV.Games\12x8\KingsCourt.cs:23-28`, `C:\GitHub\chessv\ChessV.Games\12x8\KingsCourt.cs:47-78`. |

For the script, `Generic12x8` explicitly fixes twelve files:
`C:\GitHub\chessv\ChessV.Games\Abstract\Generic12x8.cs:36-53`.

## Candidate shortlist

These five pieces require no exotic capture mechanic, cylinder, teleport, or multi-path move.
They are alternatives for discussion, not a ranked list or an approved pair.
The movement definitions establish simplicity, not opening mobility or material fit.

| Candidate | Plain movement description | Defining classes |
| --- | --- | --- |
| **Camel** | Leaps three squares along one axis and one along the other, in all eight orientations. It jumps intervening pieces. | `Camel`: `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:125-146`. |
| **Wizard** | Takes one diagonal step, or makes a Camel leap. | `Wizard`: `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:133-147`. `Ferz` and `Camel`: `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:45-60`, `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:125-146`. |
| **Champion** | Steps one square orthogonally, or leaps exactly two squares orthogonally or diagonally. The two-square moves jump intervening pieces. | `Champion`: `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:151-166`. `Wazir`, `Dabbabah`, and `Elephant`: `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:25-40`, `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:65-100`. |
| **Dragon Horse / ArchCourier** | Moves as a Bishop, or steps one square orthogonally. | `DragonHorse`: `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:226-240`. |
| **Dragon King / Crowned Rook** | Moves as a Rook, or steps one square diagonally. | `DragonKing`: `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:208-222`. |

Further familiar alternatives exist in the table, but they do not require a larger shortlist:

- **Archbishop / Janus / Courier** = Bishop+Knight. **Chancellor / Marshal** = Rook+Knight. Definitions: `C:\GitHub\chessv\ChessV.Games\Pieces\ChessMissingCompounds.cs:25-57`.
- **General** = king-like steps. **Centaur / Duke** = king-like steps plus Knight leaps. Definitions: `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:25-38`, `C:\GitHub\chessv\ChessV.Games\Pieces\MiscellaneousCompounds.cs:170-185`. The step definition is `C:\GitHub\chessv\ChessV.Games\Pieces\Chess.cs:148-152`. Movement alone does not confer the CPU's royal or extraLife roles.
- **King's Court Chancellor** = Knight leaps plus Queen slides limited to two squares. The game constructs an `Amazon` and limits its slides. It also adds a separate King's Flight rule, which this research does not propose to import. Sources: `C:\GitHub\chessv\ChessV.Games\12x8\KingsCourt.cs:63-78` and `C:\GitHub\chessv\ChessV.Games\Pieces\ChessMissingCompounds.cs:61-75`.

## Rejected choices and remaining source coverage

The local survey covered twelve-file registrations in `ChessV.Games` and declarations in `Include\*.cvc`.
The five table entries are the focused examples. The following records cover exclusions and other discovered variants:

- **Jester / FreePadwar:** Excluded despite its exact B1/K1 precedent. Its two-square orthogonal destinations use alternative diagonal paths, not plain leaps. Source: `C:\GitHub\chessv\ChessV.Games\Pieces\MultiPath.cs:170-216`.
- **Nightrider / Speedy Knight:** Excluded by the user. Chess and a Half is genuinely 12x12, but its Speedy Knight is a Nightrider promotion. Its Cats also receive an overtake-capture rule. Sources: `C:\GitHub\chessv\ChessV.Games\12x12\ChessAndAHalf.cs:25-29`, `C:\GitHub\chessv\ChessV.Games\12x12\ChessAndAHalf.cs:70-85`, `C:\GitHub\chessv\ChessV.Games\12x12\ChessAndAHalf.cs:94-100`. Neither mechanic enters the shortlist.
- **Cannon and Vao:** Excluded from the Gross Chess suggestions because they use cannon capture moves. Definitions: `C:\GitHub\chessv\ChessV.Games\Pieces\Miscellaneous.cs:50-56`, `C:\GitHub\chessv\ChessV.Games\Pieces\Miscellaneous.cs:70-76`.
- **Odyssey, 12x12:** Supplies further DragonKing, DragonHorse, and Camel precedents. White has Dragon Kings at A1/L1, Camels at B2/K2, and Dragon Horses at C2/J2. Its Assassin has rifle captures and a trade restriction, so that piece is excluded. Sources: `C:\GitHub\chessv\ChessV.Games\12x12\Odyssey.cs:27-32`, `C:\GitHub\chessv\ChessV.Games\12x12\Odyssey.cs:64-93`, `C:\GitHub\chessv\ChessV.Games\12x12\Odyssey.cs:99-104`.
- **Cagliostro's Chess, 12x8:** Rank 1 is `R N B A C Q K G A B N R`. A = Archbishop, C = Chancellor, G = Amazon. It confirms familiar compounds but does not justify another Amazon. Sources: `C:\GitHub\chessv\ChessV.Games\12x8\CagliostrosChess.cs:23-27`, `C:\GitHub\chessv\ChessV.Games\12x8\CagliostrosChess.cs:50-66`.
- **Courier Chess and Courier Chess Moderno, both 12x8:** Both offer a Mann with king-like steps and a Schleich with one-square orthogonal steps. Courier Chess also has Bischofs that leap two squares diagonally. Moderno's Elephant instead has Silver General movement plus special unmoved-piece moves, so the two Elephants are not interchangeable. Sources: `C:\GitHub\chessv\ChessV.Games\12x8\CourierChess.cs:23-30`, `C:\GitHub\chessv\ChessV.Games\12x8\CourierChess.cs:54-67`, `C:\GitHub\chessv\ChessV.Games\12x8\CourierChessModerno.cs:25-31`, `C:\GitHub\chessv\ChessV.Games\12x8\CourierChessModerno.cs:71-90`, `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:25-40`, `C:\GitHub\chessv\ChessV.Games\Pieces\MovementAtoms.cs:65-80`.
- **Chess on a 12 by 12 Board:** Genuinely 12x12, but its orthodox pieces start on C3 through J3. It supplies no new piece candidate. Sources: `C:\GitHub\chessv\ChessV.Games\12x12\ChessOnA12x12Board.cs:25-30`, `C:\GitHub\chessv\ChessV.Games\12x12\ChessOnA12x12Board.cs:46-62`.
- **Viking Chess, 12x7:** An additional genuine twelve-file discovery outside the three requested example sizes. It uses ordinary chess pieces and supplies no new candidate. Sources: `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\VikingChess.cs:22-26`, `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\VikingChess.cs:38-42`, `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\VikingChess.cs:62-66`.
- **Omega Chess:** Not a genuine twelve-file rectangular game. Its playable center is 10x10, with four corner squares and an internal 12x12 representation. Gross Chess, not Omega's directory or base class, establishes the twelve-file precedent for Wizard and Champion. Sources: `C:\GitHub\chessv\ChessV.Games\12x12\OmegaChess.cs:26-32`, `C:\GitHub\chessv\ChessV.Games\12x12\OmegaChess.cs:49-55`, `C:\GitHub\chessv\ChessV.Games\Rules\Omega\OmegaChessBorderRule.cs:24-35`.
- **Width traps and target code:** `Generic__x12` specifies twelve **ranks**, not twelve files. The APMW 12x10/12x12 registrations belong to the current system, not independent inspiration. Sources: `C:\GitHub\chessv\ChessV.Games\Abstract\Generic__x12.cs:29-32`, `C:\GitHub\chessv\ChessV.Games\Abstract\Generic__x12.cs:52-56`, `C:\GitHub\chessv\ChessV.Games\MiscellaneousGames\ApmwExpandedChess.cs:18-42`. Duke of Rutlands Chess is fourteen files, so it is not a twelve-file precedent: `C:\GitHub\chessv\Include\Duke of Rutlands Chess.cvc:2-5`.

These findings describe the local implementations. No external designer pages were necessary or consulted.
The initial variant survey did not assign new APMW values or import any
precedent game's special rules. The consolidated inspection below adds source
prices and a bounded fit assessment.

## Consolidated source values and fit

The follow-up inspection adds these source values. They are the values in the
precedent games, not approved new APMW prices or measured 12x10 strengths.

| Candidate | Source MG / EG | Binding | Fit with the approved pawn screen |
| --- | ---: | --- | --- |
| Camel | 250 / 250 | Colorbound | From B1 it can leap to A4/C4, beyond both pawn ranks |
| Wizard | 600 / 550 | Colorbound | The same Camel exits, plus a one-square diagonal step |
| Champion | 600 / 600 | Not colorbound | Straightforward short steps and leaps, but the dense starting screen occupies many nearby destinations |
| Dragon Horse | 500 / 550 | Not colorbound | Familiar Bishop movement with orthogonal flexibility; initial development depends on the still-open C2 and C1 occupancy |
| Dragon King | 700 / 700 | Not colorbound | Familiar Rook movement plus diagonal steps, but a substantially stronger source price |

Values: `ChessV.Games\12x10\JanusKamilChess.cs:61-63`,
`ChessV.Games\12x12\GrossChess.cs:74-75`,
`ChessV.Games\12x8\ArchCourierChess.cs:69-71`.

Camel's B1 exits follow directly from its `(3,1)` leap and the required empty
neutral rank 4. A paired Camel on K1 has the corresponding J4/L4 exits.
B1 and K1 have opposite square colors, so the pair covers both complexes.

The current APMW catalog has none of these five exact piece registrations.
It has a distinct `GreatCamel` entry at 700/700; that is not permission to
substitute it for the plain Camel or transfer its price.
Source: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:21-64`.

Existing APMW alternatives include Short Rook (400/425), Tower (325/325),
Lancer (325/325), Knight (325/325), and Cleric (450/500). They avoid a new
piece registration when reused as-is, but the source survey did not establish
the same direct twelve-file precedent for that exact B1/K1 role.
Sources: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:25-42`;
`ChessV.Games\Pieces\CwDA.cs:81-176`.

The fit assessment does not ban non-royal king-moving compounds. Such a piece
would need a separate concrete registration so it cannot accidentally enter
the accepted King-life pool. The user's constraint concerns understandable
pieces, not an automatic prohibition on every compound.

The consolidator recommends the plain Camel as the first option to discuss:
Janus Kamil is already 12x10, shares the requested D/E Knight/Bishop positions,
and supplies a restrained 250/250 source value. This remains a recommendation.
The user has not chosen the piece, its APMW price, or whether it adds inventory.

## Unanswered user decisions

1. Which existing movement type does the user want at B1/K1?
2. Does that choice replace another planned piece, relocate existing inventory, or add inventory?
3. Which remaining home-row and central placements receive approval, including the proposed F1/F2/G1/G2 arrangement?

Those decisions remain open. The precedent research is complete.

## Earlier user choice (superseded)

The user selected Champion for B1/K1 and chose an added pair, retaining the
other agreed pieces and fourteen pawns. The Camel recommendation was not
adopted.

The subsequent user answer initially approved the complete family arrays and
Champion at 475/475. The Mounted King's `Champion` image name would have
required image separation for that proposal. The later Lion choice removes
this Champion-specific work.

Champion has no single existing repository-wide price. Omega uses 375/375,
TenCubed uses 475/475, and Gross uses 600/600.
Sources: `ChessV.Games\12x12\OmegaChess.cs:88`,
`ChessV.Games\10x10\TenCubedChess.cs:60`,
`ChessV.Games\12x12\GrossChess.cs:75`.
These prices apply to the same movement in different games. The user made
the explicit 475/475 choice rather than silently inheriting Gross's price.

## Final piece correction

The user then chose Lion instead of Champion. Reuse the existing APMW Lion
at 500/500: a diagonal step or a two- or three-square orthogonal leap.
Only the universal B1/K1 pair changes. The Rookies' other Lion
correspondents remain.

Sources: `ChessV.Games\MiscellaneousGames\ApmwPieceCatalog.cs:40`;
`ChessV.Games\Pieces\CwDA.cs:25-41`; the later user correction in the owning
array ticket. This is an author decision, not a claim that the original
precedent shortlist selected Lion.
