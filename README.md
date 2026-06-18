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

Unlike ordinary Chess, the main match target in this client is King extinction, not ordinary checkmate. This means when a player has no relevant King piece left, they lose. (A player ordinarily has 1 King piece.) This was chosen in order to make it clearer whether various objectives are accessible.

This client implements the ChecksMate protocol for ArchipelagoMW by modifying the ChessV 2.2 client by Greg Strong.

### Gameplay expectations and rules FAQ

Each Try Again is a fresh match. Think of a match as an attempt to claim one or more Archipelago locations, not as a board state you are supposed to preserve forever. Losing after grabbing a useful location is normal progression: pick a target, spend the position to get it, and come back stronger. Undo is not supported.

Extra Kings act as backup royal pieces for extinction; losing one King is not necessarily the end if another King-type piece remains. Castling is still for the main King only; extra Kings and consuls do not castle.

Checkers are their own weird pawn variant. Their normal non-capturing moves are one step forward diagonally. Their captures are jump chains, and during those capture chains they may hop across the left/right board edge. That is Checkers-specific behavior, not a general "the whole board is cylindrical" rule.

Fairy pieces and pawn variants may not move like their icons suggest. If a piece surprises you, right-click it and choose Properties to see its info and movement diagram.

Location wording is literal. `Capture Any N` counts total captures in one match. `Capture N Of Each` means N pawns and N back-rank pieces in the same match. Fork locations are attack locations: Sacrificial forks require one piece attacking multiple counted non-pawn targets; True forks additionally require the attacker to live and the counted targets to be king, undefended, or valuable enough that recapturing still loses material.

### Supported Options

 - Pocket Pieces. Inspired by Bughouse and Pocket Knights, you may drop a piece from outside the board onto an open square on your home row instead of making a normal move.
   - Players have 3 pockets, which can be empty, or hold a pawn, minor piece, major piece, or queen. Collected pocket items are distributed randomly to the 3 pockets, improving them in the above order.
   - You may only drop a piece by spending Gems equal to its material value. Gems are collected at a rate of 1/turn, and you start a match with your collected Pocket Gems. The Black player starts with 1 extra Gem.
   - Pocket Range extends pocket drops away from your home row, normally stopping before the opponent's home row.
 - Fairy Chess Pieces and Fairy Chess Army. You can keep material close to orthodox Chess, open the Betza/FIDE/fairy pools, customize the enabled set, or constrain generated player material by army.
 - Chaotic Material Randomization. Every game, you get new pieces in new places! Who needs an opening book?
 - Piece Limits. Under some mindsets, it can be taxing to find 6 minor pieces and no Queen. By adding certain rails to the experience, one can have a more personalized approach to a Chess randomizer, where one's army bears some resemblance to a traditional game.
 - Extra Kings. What if you had a backup King?
 - Difficulty, AI Intelligence malus, enemy army, Super Mode, DeathLink, pawn families, and Jacks all have generation or client-side wrinkles. See the notes below before assuming a dropdown changes the current match.

#### Option reference notes

 - DeathLink is chosen at generation time. If your slot has DeathLink, the client enables the DeathLink checkbox after connecting and starts it checked; if not, the checkbox stays unavailable. The checkbox is a local participation toggle, not a way to add DeathLink to a non-DeathLink seed. With the toggle enabled, losing a match or resigning sends a DeathLink, and receiving one kills the active match immediately. No undo, no review, no "wait, I had a tactic."
 - Difficulty and AI Intelligence are not the same knob. YAML `difficulty` changes generation logic: which checks are expected, how much material the logic assumes, and how relaxed later objectives are. `Maximum Engine Penalties` controls how many `Progressive AI Intelligence Malus` items can appear. The client's "Reduce AI Intelligence" dropdown is local and per-match; it stacks with collected AI malus for search limits, but it does not change the generated world or logic.
 - Enemy army is a client match setting. The "Change Enemy Army" dropdown can give the opponent Standard/FIDE, Colourbound Clobberers, Remarkable Rookies, or Nutty Knights pieces for the next match, and those pieces are added to the promotion set. Pick it before starting the match.
 - Super Mode uses the larger Super-Sized variant. Goal `Super` starts there immediately. `Progressive` puts `Super-Size Me` in the pool. `Ordered Progressive` awards `Super-Size Me` at Checkmate Minima. After you have `Super-Size Me`, the Super checkbox starts the larger-board match.
 - Fairy Chess Pieces is the simple collection selector. FIDE, Betza, and Full override the custom Configure set. If you want `fairy_chess_pieces_configure` to matter, set Fairy Chess Pieces to Configure first.
 - Fairy Chess Army constrains generated player material to the enabled army or armies. If the selected army filter would leave no legal choices for a piece class, the client falls back to the unfiltered pool rather than drawing from nothing.
 - `Asymmetric Trades: Jacks` adds `Progressive Jack`. Jacks are custom roughly 7-material pieces such as Agile Rook, Mullah, Zealot, Great Camel, Dragon Cannon, Mameluk, and Grazer. They are generated before ordinary major pieces and can participate in castling.
 - `fairy_chess_pawns` chooses the pawn family: standard pawns, Berolina, Checkers, or one of the mixed pools. `fairy_chess_pawn_upgrades` controls stronger pawn-family upgrades drawn from the pawn budget: Off keeps the legacy post-selection upgrade pass, Pool adds upgrades as random pool options while guarding pawn count, and Max prefers upgrades when the budget can still reach your earned pawn count.
 - A drop is not a pawn move. If a nonstandard setup lets a pocket pawn be dropped directly onto a promotion rank, do not expect it to promote as part of that drop.

### Strategic notes

This is not Chess. It's an asymmetric, multi-round experience involving the rules of Chess. You only need to land the win once - and your opponent is too shortsighted to stop you from coming back stronger.

Don't hesitate to lose a match to capture a new piece - you can just play again, now with another item.

Choose one specific location each round. Invest all your tools toward that task alone.

### Versions, trackers, and troubleshooting

Use the ChecksMate client and `checksmate.apworld` from the same release unless you know why you're mixing them. The generator writes a `required_chess_client_version` into slot data, and the client compares it with its built-in version. Newer-or-equal clients may continue; too-old clients disconnect with an update message. If version parsing fails, the client warns and lets you continue. That's "dangerous wizard mode", not a compatibility promise.

The Archipelago game/tracker name is `ChecksMate`. A PopTracker pack exists at https://github.com/checkerslogic/checksmate-poptracker/releases/. Universal Tracker is not promised here yet; if you try it with the release `checksmate.apworld`, treat it as best-effort until the workflow is verified.

Known rough edges:
 - If the AI appears to think forever or stops moving, start a fresh game/client. Recent releases hardened move generation, make/unmake, diagnostics, and move hashes, but weird boards can still be weird.
 - Checkers pawns are spicy. They can multi-capture and wrap captures over the board edge. Recent builds added guard rails around Checkers/Cannon move generation, but Checkers-heavy seeds are still a reasonable place to expect instability. For a calmer seed, avoid or reduce Checkers in `fairy_chess_pawns`.
 - Reconnect/disconnect guarantees, Stable Stuck behavior, exact Consul/King Promotion limits, and Play as White details are intentionally not promised here yet.

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

## ArchipelagoMW

Archipelago provides a generic framework for developing multiworld capability for game randomizers. In all cases, presently, Archipelago is also the randomizer itself.

https://github.com/ArchipelagoMW/Archipelago

https://archipelago.gg/

## Not implemented yet (TODO)

Bugs:

 - Infinite generation time if player excludes almost every possible item on a very low difficulty. (This is close to a suggested preset, and therefore a higher risk.)
 - Engine Elo reduction item is incorrectly named. (It's a person's name, not an acronym.)
 - Draw by repetition happens in 2 moves, not 3. (I think this is a problem in the base ChessV client. If true, I'm inclined to believe that fixing it would be quite difficult.)

Locations:

 - "Discovered Attack" where a piece which was not under attack becomes under attack but not by the piece you moved
 - "Pin" and "Skewer" where a piece would be under attack if not for another piece on the same side. If the higher value piece is attacked, it's a skewer, otheerwise it's a pin
 - A location for which one performs the French move

Randomizer options and features:

 - Silly vs Serious Location and Item Names. Some people have laughed at some of the names I've come up with, and others are rather utilitarian in wanting to know the specific requirements of locations and benefits of items. With some work to update the location and item trackers, as well as an expanded redundant set of items from Items.py, a player should be able to use either set of names.
 - Progressive Goal option. Your enemy's pieces are also scattered across the multiworld! (The current design can make progression too easy.)
 - Non-Progressive Material option. Pieces will not be selected progressively from a set, but instead placed with specific names in your world. This means you would find a Bishop or Cleric rather than a Progressive Minor Piece or Progressive Major Piece. (They are unlikely to come with pre-determined locations.)
 

Client features:

 - Reconnect or warn user of disconnect when the computer goes to sleep.
 - Maybe it's possible to change the seed? I think you can modify slot data during a game... this would be another alternative to the "Stable Stuck" seed problem where a player must play out a weak position.
