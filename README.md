## ChecksMate Client

This project implements a co-op Roguelite meta-progression layer for the best semi-3d on-rails platformer since Crash Bandicoot 2.

Your opponent begins with a set of 16 chessmen. You begin with a King and the ability to Try Again. Your objective is to win a match by eliminating the opposing King pieces.

In addition to the 6 ordinary chessmen, you may find yourself in control of fairy chess pieces. These include the Berolina Pawn, various pieces from Ralph Betza's Chess With Different Armies, as well as Xiang Qi's Cannon (and an invented Vao piece, which is to the Bishop as the Cannon is to the Rook).

As you complete the following objectives, you will gain access to additional material.

 - Capture individual enemy pieces and pawns (e.g. capture pawn E, the pawn that begins on the E file)
 - Capture multiple enemy pieces and pawns in 1 match (e.g. capture any 2 pawns), including sequences of pairs (e.g. both 2 pieces and 2 pawns)
 - Attack any opposing pawn, minor piece, major piece, or queen
 - Attack multiple opposing pieces with a single piece: two pieces, three pieces, and the King and Queen
   - Sacrificial forks merely require such an attack, but a True fork additionally requires that the piece will live to attack, and that each target is: not defended, worth more, or is the king.
 - Move your King each of: forward one space; to the A file; to the center 4 squares; to the opposing home rank; and to capture a piece
 - Short/Long "Castle" where you castle.

Unlike ordinary Chess, the main match target in this client is King extinction, not ordinary checkmate. This means when a player has no relevant King piece left, they lose. (A player ordinarily has 1 King piece.) This was chosen in order to make it clearer whether various objectives are accessible. Archipelago refers to the above objectives as "Locations."

This client implements the ChecksMate protocol for ArchipelagoMW by modifying the ChessV 2.2 client by Greg Strong.

### Gameplay expectations and rules FAQ

Each Try Again is a fresh match. In each match, you should attempt to claim one or more Archipelago locations. Losing after grabbing a useful location is normal progression: pick a target, spend the position, come back stronger.

Right-click a piece and choose Properties to see its info and movement diagram. Fairy pieces and pawn variants may not move like their icons suggest.

Extra Kings act as backup royal pieces for extinction; losing one King is not necessarily the end if another King-type piece remains. Castling is still for the main King only; extra Kings (called Consuls in your Item Tracker) do not castle.

The first two turns have a special "Scholar's Mate Minimum" rule, which can look surprising: If a King is threatened very early, the defender may emergency capture against the attacking piece, so that a generated starting position does not immediately decide the match. This triggers on ANY King attack, not only on mate.

Checkers are a pawn variant. The default options preclude all pawn variants, so don't worry. Their normal non-capturing moves are one step forward diagonally. Their captures are jump chains, and during those capture chains they may hop across the left/right board edge. That is Checkers-specific behavior, not a general "the whole board is cylindrical" rule.

Location wording is literal. `Capture Any N` counts total captures in one match. `Capture N Of Each` means N pawns and N back-rank chessmen in the same match. Fork locations are attack locations: Sacrificial forks require one piece attacking multiple counted non-pawn targets; True forks additionally require the attacker to live and the counted targets to be king, undefended, or valuable enough that recapturing still loses material.

Undo is not supported. The analysis tools will fail you. There are various other ways to cheat, the behaviour of which is undefined. You have been warned.

A recommended PopTracker pack is available at https://github.com/checkerslogic/checksmate-poptracker/releases/.

### Supported Options

 - Pocket Pieces. Inspired by Bughouse and Pocket Knights, you may drop a piece from outside the board onto an open square on your home row instead of making a normal move.
   - Players have 3 pockets, which can be empty, or hold a pawn, minor piece, major piece, or queen. Collected pocket items are distributed randomly to the 3 pockets, improving them in the above order.
   - You may only drop a piece by spending Gems equal to its material value. Gems are collected at a rate of 1/turn, and you start a match with your collected Pocket Gems. The Black player starts with 1 extra Gem.
   - Pocket Range extends pocket drops away from your home row, normally stopping before the opponent's home row.
 - Fairy Chess Pieces and Fairy Chess Army. While the default is close to orthodox Chess, support for Ralph Betza's Different Armies and other fairy pieces allow you to customize the enabled set, constraining generated player material by army.
 - Chaotic Material Randomization. Every game, you get new pieces in new places! Who needs an opening book?
 - Piece Limits. Under some mindsets, it can be taxing to find 6 minor pieces and no Queen. By adding certain rails to the experience, one can have a more personalized approach to a Chess randomizer, where one's army bears some resemblance to a traditional game.
 - Extra Kings. What if you had a backup King?
 - Difficulty, AI Intelligence malus, enemy army, Super Mode, DeathLink, fairy pawns, and Jacks! 

#### Option reference notes

 - DeathLink is only available when enabled at generation time. If the local toggle is also enabled, losing a match or resigning sends a DeathLink, and receiving one kills the active match immediately. DeathLink cannot be enabled in a non-DeathLink seed.
 - Difficulty and AI Intelligence differ: YAML `difficulty` changes generation logic, lowering expectations at any given material value, while `Maximum Engine Penalties` controls how many `Progressive AI Intelligence Malus` items can appear. The client's "Reduce AI Intelligence" dropdown is additive to collected AI malus. AI malus causes the heuristic engine to act without thinking.
 - The "Change Enemy Army" dropdown normally affords the opponent Standard/FIDE pieces, which can be replaced by Ralph Betza's Different Armies (Colourbound Clobberers, Remarkable Rookies, or Nutty Knights) for the next match. Those pieces are added to the promotion set.
 - FUN SPOILERS: Super Mode uses the larger Super-Sized board variant. Goal `Super` starts there immediately. `Progressive` puts `Super-Size Me` in the pool. `Ordered Progressive`, the default, awards `Super-Size Me` at Checkmate Minima. After you have `Super-Size Me`, the Super checkbox starts a super match.
 - Fairy Chess Pieces allows further replayability by replacing the player pieces with modern innovations by Ralph Betza and other authors. FIDE, Betza, and Full override the custom Configure set. If you want to use `fairy_chess_pieces_configure` to choose your own subset, set Fairy Chess Pieces to Configure first.
 - Fairy Chess Army constrains generated player material to a single army among enabled armies.
 - `Asymmetric Trades: Jacks` adds `Progressive Jack`. Jacks are custom (by the author!) roughly 7-material pieces such as Agile Rook, Mullah, Zealot, Great Camel, Dragon Cannon, Mameluk, and Grazer. They can participate in castling like major pieces and unlike queens.
 - `fairy_chess_pawns` includes standard pawns, Berolina, Checkers, or one of the mixed pools. `fairy_chess_pawn_upgrades` controls stronger pawn upgrades drawn from the pawn budget: Off keeps the legacy post-selection upgrade pass, Pool adds upgrades as random pool options while guarding pawn count, Max prefers upgrades when the budget can still reach your earned pawn count, and SuperMax is an inline pawn-upgrade/Sergeant option, not a separate `pawn_count_guarantee` setting. SuperMax behaves like Max, but when the board-location pawn requirement is lower than your collected `Progressive Pawn` count, it keeps the full collected pawn material budget and can convert excess pawn material into Sergeant/Odin Pawn upgrades. The board-location guarantee counts 16 chessmen including the base King on standard boards (15 non-base-king slots), or 20 including the base King on Super-Sized boards (19 non-base-king slots); known Consuls, jacks, majors, and minors reduce how many pawn slots still need to be guaranteed.

### Strategic notes

This is not Chess. It's an asymmetric, multi-round experience involving the rules of Chess. You only need to land the win once - and your opponent is too shortsighted to stop you from coming back stronger.

Don't hesitate to lose a match to capture a new piece - you can just play again, now with another item.

Choose one specific location each round. Invest all your tools toward that task alone.

### Versions, trackers, and troubleshooting

Use the ChecksMate client and `checksmate.apworld` from the same release unless you know why you're mixing them. The generator writes a `required_chess_client_version` into slot data, and the client compares it with its built-in version. Newer-or-equal clients may continue; too-old clients disconnect with an update message. If version parsing fails, the client warns and lets you continue. That's "dangerous wizard mode", not a compatibility promise.

Known rough edges:
 - If the AI appears to think forever or stops moving, start a fresh game/client. Recent releases hardened move generation, make/unmake, diagnostics, and move hashes, but weird boards can still be weird.
 - Checkers pawns are spicy. They can multi-capture and wrap captures over the board edge. Recent builds added guard rails around Checkers/Cannon move generation, but Checkers-heavy seeds are still a reasonable place to expect instability. For a calmer seed, avoid or reduce Checkers in `fairy_chess_pawns`.
 - Reconnect/disconnect guarantees, Stable Stuck behavior, exact Consul/King Promotion limits, and Play as White details are intentionally not promised here yet.

### Army PieceTypes reference

This list covers army-selected non-pawn back-rank PieceTypes and promotion pools. Right-click Properties in the client to see piece info and movement diagrams. Camel and Petal include unique material partly designed by the ChecksMate author, which is why their definitions are documented here. Jacks listed below are army-specific; Great Camel is a global Jack pool piece, not a Camel or Petal army assignment.

#### Movement notation

`(r,f)` is the rank/file offset from the moving piece; N = `(+1,0)`, E = `(0,+1)`. `s,t in {+1,-1}` choose mirrored signs. For directional pieces, positive rank is forward for the moving side; the opposite side mirrors forward/backward.

`Step` or leap moves directly to the listed offset; blockers between source and destination are ignored. `Slide` or rider repeats a vector until the board edge or blockage: empty squares are legal, the first enemy can be captured, and any occupied square stops the ray. `Path` is a single-target move requiring one listed sequence of pre-target route squares to be empty; the target uses normal move/capture rules unless marked move-only. `CannonMove` uses the screen/capture rules described in Cannon pieces.

#### FIDE

 - **Minor:** Bishop/Knight.
 - **Major:** Rook.
 - **Jack:** Agile Rook.
 - **Queen:** Queen.

#### Colourbound

 - **Minor:** Phoenix.
 - **Major:** War Elephant/Cleric.
 - **Jack:** Mullah.
 - **Queen:** Archbishop.

#### Remarkable

 - **Minor:** Tower/Short Rook.
 - **Major:** Lion.
 - **Jack:** Zealot.
 - **Queen:** Chancellor.

#### Nutty

 - **Minor:** Charging Knight/Lancer.
 - **Major:** Charging Rook.
 - **Jack:** Mameluk.
 - **Queen:** Colonel.

#### Eurasian

 - **Minor:** Cannon/Vao.
 - **Major:** No army-specific major, so generation falls back to the broader major pool if filtering finds none.
 - **Jack:** Dragon Cannon.
 - **Queen:** Herald/Queennon.

#### Camel

 - **Minor:** Scout.
 - **Major:** Nightrider.
 - **Jack:** Mameluk.
 - **Queen:** Miracle/Colonel.

#### Petal

 - **Minor:** Gardener/Ribbon.
 - **Major:** Petal.
 - **Jack:** Grazer.
 - **Queen:** Miracle.

#### Movement definitions

All offsets below use the notation above. In `via` clauses, only pre-target route squares are listed; the described offset is the target square and is not repeated in the route list.

##### Orthodox baseline and atoms

| Piece/atom | Movement definition |
| --- | --- |
| Rook | Slides `(s,0)` or `(0,s)`. |
| Bishop | Slides `(s,t)`. |
| Knight | Leaps to `(s,2t)` or `(2s,t)`; jumps blockers. |
| Queen | Rook + Bishop. |
| Wazir | Steps `(s,0)` or `(0,s)`. |
| Ferz | Steps `(s,t)`. |
| Elephant | Leaps `(2s,2t)`. |
| Dabbabah | Leaps `(2s,0)` or `(0,2s)`. |
| Tribbabah | Leaps `(3s,0)` or `(0,3s)`. |
| Camel | Leaps `(s,3t)` or `(3s,t)`. |

##### Path pieces

| Piece | Movement definition |
| --- | --- |
| Ribbon | Ferz step to `(s,t)`. Close targets: `(2s,0)` via `(s,+1)` or `(s,-1)`; `(0,2t)` via `(+1,t)` or `(-1,t)`. Long targets: `(3s,t)` via `(s,-t) -> (2s,0)`; `(s,3t)` via `(-s,t) -> (0,2t)`. Ribbon is not a Knight, not a rider, never leaps to Knight squares, and has no longer repeated paths. Intervening route squares must be empty. |
| Petal | Orthogonal limited rider targets `(ks,0)` and `(0,kt)` for `k=1..3`, straight path clear. Bent rook paths only after exactly 3 orthogonal squares: `(3s,t)` via `(s,0) -> (2s,0) -> (3s,0)`; `(3s,2t)` via `(s,0) -> (2s,0) -> (3s,0) -> (3s,t)`; `(s,3t)` via `(0,t) -> (0,2t) -> (0,3t)`; `(2s,3t)` via `(0,t) -> (0,2t) -> (0,3t) -> (s,3t)`; `(3s,3t)` via rank-first `(s,0) -> (2s,0) -> (3s,0) -> (3s,t) -> (3s,2t)` or file-first `(0,t) -> (0,2t) -> (0,3t) -> (s,3t) -> (2s,3t)`. Petal has no short bends or diagonal targets such as `(s,t)`, `(2s,2t)`, `(2s,t)`, or `(s,2t)`. |
| Gardener | Elephant leaps `(2s,2t)`. Also has move-only inward paths to `(2s,t)`, `(s,2t)`, and `(s,t)`, each via `(2s,2t)`; both the route square and the target must be empty. |
| Miracle | Ribbon + Petal. If both define the same target, either legal path is sufficient. |
| Grazer | Wazir steps `(s,0),(0,t)`. CloseRibbon targets: `(2s,0)` via `(s,+1)` or `(s,-1)`; `(0,2t)` via `(+1,t)` or `(-1,t)`. Diagonal limited rider targets `(s,t)` and `(2s,2t)`, with `(s,t)` empty for length 2. Cardinal four-step targets: `(4s,0)` via `(s,t) -> (2s,2t) -> (3s,t)` and `(0,4t)` via `(s,t) -> (2s,2t) -> (s,3t)`. Enhanced Ribbon targets: `(3s,t)` via `(s,-t) -> (2s,0)` or `(s,t) -> (2s,2t)`; `(s,3t)` via `(-s,t) -> (0,2t)` or `(s,t) -> (2s,2t)`. |
| Zealot | Rook slide plus forward-only path targets `(3,t)` via `(1,0) -> (2,0) -> (3,0)` and `(1,3t)` via `(0,t) -> (0,2t) -> (0,3t)`. No backward path counterparts. |

##### Cannon pieces

`CannonMove` per vector: empty squares before the first occupied square are quiet moves. The first occupied square of either color is the screen and is not captured. Empty squares beyond the screen are illegal destinations. The next occupied square stops the ray and is capturable only if enemy. No screen means no capture; pure cannon pieces therefore have no adjacent captures.

| Piece | Movement definition |
| --- | --- |
| Cannon | `CannonMove` on `(s,0)` and `(0,s)`. |
| Vao | `CannonMove` on `(s,t)`. |
| Dragon Cannon | Cannon + Vao. |
| Queennon | Dragon Cannon plus adjacent capture-only king steps in all 8 directions and knight-vector `CannonMove`s `(s,2t)` and `(2s,t)` as repeated-vector cannon riders. |

##### Other compound pieces

| Piece | Movement definition |
| --- | --- |
| Agile Rook | Rook slides + Elephant leaps. |
| Phoenix | Wazir steps + Elephant leaps. |
| War Elephant | Ferz steps + Elephant leaps + Dabbabah leaps. |
| Cleric | Bishop slides + Dabbabah leaps. |
| Mullah | Bishop-like diagonal slides `(s,t)` + Camel leaps `(s,3t),(3s,t)`. |
| Archbishop | Bishop slides + Knight leaps. |
| Tower | Wazir steps + Dabbabah leaps. |
| Short Rook | Bounded orthogonal slide up to 4 squares: targets `(ks,0)` and `(0,ks)` for `k=1..4`, straight path clear. |
| Lion | Ferz steps + Dabbabah leaps + Tribbabah leaps `(3s,0),(0,3s)`. |
| Chancellor | Rook slides + Knight leaps. |
| Charging Knight | Leaps `(1,+/-2),(2,+/-1)` plus steps `(-1,-1),(-1,0),(-1,1),(0,+/-1)`. Directional piece. |
| Lancer/NarrowKnight | User-facing Lancer; Ferz steps `(s,t)` + narrow knight leaps `(2s,t)` only. |
| Charging Rook | Slides `(1,0),(0,+/-1)` plus steps `(-1,0),(-1,+/-1)`. Directional piece. |
| Mameluk | Wazir steps + camel-rider lines along `(s,3t),(3s,t)`. |
| Colonel | Leaps `(1,+/-2),(2,+/-1)`, steps `(+/-1,+/-1),(-1,0)`, slides `(1,0),(0,+/-1)`. Directional piece. |
| Scout | Wazir steps + Tribbabah leaps `(3s,0),(0,3s)`. |
| Nightrider | Rider along Knight vectors `(s,2t),(2s,t)`. |
| Herald | Camel leaps + Tribbabah leaps + diagonal limited rider up to 3 squares on `(s,t)` + forward step `(1,0)`. Directional piece. |
| Great Camel | Global Jack pool piece, not a Camel/Petal army assignment; Knight leaps + Camel leaps. |

#### Pawn families

Pawn families are separate from back-rank armies. In CwDA, `WhiteArmy` and `BlackArmy` change back ranks and promotion pools while every army keeps normal pawns. In ChecksMate generation, `fairy_chess_pawns` chooses Pawn/Berolina/Checkers pools; `fairy_chess_pawn_upgrades` can add Sergeant/Odin Pawn, and army filtering applies to non-pawn piece sets.

 - Standard Pawns move forward, capture diagonally, and use generic promotion/en passant.
 - Berolina Pawns move diagonally, capture forward, and use diagonal double-step/en passant.
 - Checkers move diagonally without capture, capture by jump chains that may wrap over the left/right edge, and promote on the final rank.
 - Sergeant mixes normal and Berolina pawn movement; Odin Pawn is an upgrade with diagonal/Ferz-like movement plus a forward zig-zag option.

Diamond Pawn is Diamond Chess-only; do not treat it as a ChecksMate army pawn.

## ChessV

ChessV is a free, open-source universal chess program with a graphical user interface, sophisticated AI engine, and other features of traditional Chess programs. As a "universal" chess program, it not only plays orthodox Chess, it is also capable of playing games reasonably similar to Chess. It currently plays over 100 different chess variants, and can be programmed to play additional variants.

Features
 - Plays over 100 different Chess variants, including some that are quite exotic.
 - Has a fully-featured graphical user interface, but the engine can also be used separately under another GUI (such as WinBoard) and other compliant engines can be used with ChessV's GUI.
 - Has a scripting language to allow configuration of new variants. It supports combining existing pieces and rules, and even defining new pieces, but creation of new rules is not supported.
 - Plays with a fairly high level of skill. The engine can also be configured to weaken its skill level.
 - ChessV is a .NET application so it can be run under Linux or MacOS using Mono.

http://www.chessv.org/

### Enhancements

 - Fixes to some extremely minor and/or niche bugs.
 - The AI considering a pocket knight drop will write "Pkt1" in its node exploration log.
 - Displays "YOU ARE IN CHECK" in large red text if a King the player controls is threatened.
 - MoveInfo is now 64-bit, allowing a wider range of representation of PieceTypes and other info.

## ArchipelagoMW

Archipelago provides a generic framework for developing multiworld capability for game randomizers. In all cases, presently, Archipelago is also the randomizer itself.

https://github.com/ArchipelagoMW/Archipelago

https://archipelago.gg/

## Not implemented yet (TODO)

Bugs:

 - Infinite generation time if player excludes almost every possible item on a very low difficulty. (This is close to a suggested preset, and therefore a higher risk.)
 - Engine Elo reduction item is incorrectly named. (It's a person's name, not an acronym.)
 - Draw by repetition happens in 2 moves, not 3. (I think this is a problem in the base ChessV client. If true, I'm inclined to believe that fixing it would be quite difficult.)
 - A drop is not a pawn move. If a nonstandard setup lets a pocket pawn be dropped directly onto a promotion rank, do not expect it to promote as part of that drop. There has been some work to completely prevent having a Pocket Range value above +6.

Locations:

 - "Discovered Attack" where a piece which was not under attack becomes under attack but not by the piece you moved
 - "Pin" and "Skewer" where a piece would be under attack if not for another piece on the same side. If the higher value piece is attacked, it's a skewer, otheerwise it's a pin
 - A location for which one performs the French move

Randomizer options and features:

 - Silly vs Serious Location and Item Names. Some people have laughed at some of the names I've come up with, and others are rather utilitarian in wanting to know the specific requirements of locations and benefits of items. With some work to update the location and item trackers, as well as an expanded redundant set of items from Items.py, a player should be able to use either set of names.
 - Progressive Goal option. Your enemy's pieces are also scattered across the multiworld! (The current design can make progression too easy or flush with filler items.)
 - Non-Progressive Material option. Pieces will not be selected progressively from a set, but instead placed with specific names in your world. This means you would find a Bishop or Cleric rather than a Progressive Minor Piece or Progressive Major Piece. (They are unlikely to come with pre-determined locations.)
 

Client features:

 - Reconnect or warn user of disconnect when the computer goes to sleep.
 - Maybe it's possible to change the seed? I think you can modify slot data during a game... this would be another alternative to the "Stable Stuck" seed problem where a player must play out a weak position.
