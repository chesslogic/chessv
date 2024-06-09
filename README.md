## ChecksMate Client

This project implements a co-op Roguelite meta-progression layer for the best semi-3d on-rails platformer since Crash Bandicoot 2.

Your opponent begins with a set of 16 chessmen. You begin with a King and the ability to Try Again. Your objective is to checkmate the King.

In addition to the 6 ordinary chessmen, you may find yourself in control of fairy chess pieces. These include the Berolina Pawn, various pieces from Ralph Betza's Chess With Different Armies, as well as Xiang Qi's Cannon (and an invented Vao piece, which is to the Bishop as the Cannon is to the Rook).

As you complete the following objectives, you will gain access to additional material.

 - Capture individual enemy pieces and pawns (e.g. capture pawn E, the pawn that begins on the E file)
 - Capture multiple enemy pieces and pawns in 1 match (e.g. capture any 2 pawns), including sequences of pairs (e.g. both 2 pieces and 2 pawns)
 - Attack any opposing pawn, minor piece, major piece, or queen
 - Attack multiple opposing pieces with a single piece: two pieces, three pieces, and the King and Queen
   - Sacrificial forks merely require such an attack, but a True fork additionally requires that the piece will live to attack, and that each target is: not defended, worth more, or is the king.
 - Move your King each of: forward one space; to the A file; to the center 4 squares; to the opposing home rank; and to capture a piece
 - Short/Long "Castle" where you castle.

Unlike ordinary Chess, the victory condition in this client is King extinction, not checkmate. This means when a player has had their last King captured, they lose. (A player ordinarily has 1 King piece.) This was chosen in order to enable the player to simplify whether various objectives are accessible.

This client implements the ChecksMate protocol for ArchipelagoMW by modifying the ChessV 2.2 client by Greg Strong.

### Supported Options

 - Pocket Pieces. Inspired by Bughouse and Pocket Knights, you may drop a piece from outside the board onto an open square on your home row instead of making a normal move.
   - Players have 3 pockets, which can be empty, or hold a pawn, minor piece, major piece, or queen. Collected pocket items are distributed randomly to the 3 pockets, improving them in the above order.
   - You may only drop a piece by spending Gems equal to its material value. Gems are collected at a rate of 1/turn, and you start a match with your collected Pocket Gems. The Black player starts with 1 extra Gem.
   - Pocket Range allows the player to deploy pocket items one rank further from the home row, but not the opponent's home row
 - Non-Fairy Chess. Your major pieces will always be Rooks, your minor pieces will always be Bishops and Knights, and your queens will always slay. Also, no more dumb Berolina Pawns. Who even thought mixing those was a good idea?
 - Chaotic Material Randomization. Every game, you get new pieces in new places! Who needs an opening book?
 - Army-Constrained Material. The material you get will always be related to each other (in that they belong in the same army): If you find a Bishop you won't find a Cannon; if you find a Cleric you won't find a Lion.
   - It may be inconvenient to exclude certain pieces under this mode...
 - Piece Limits. Under some mindsets, it can be taxing to find 6 minor pieces and no Queen. By adding certain rails to the experience, one can have a more personalized approach to a Chess randomizer, where one's army bears some resemblance to a traditional game.
 - Extra Kings. The player loses when their King is extinct, and the AI loses by checkmate. But what if you had a backup King?

### Strategic notes

This is not Chess. It's an asymmetric, multi-round experience involving the rules of Chess. You only need to checkmate once - and your opponent is too shortsighted to stop you from coming back stronger.

Don't hesitate to put your King into checkmate to capture a new piece - you can just play again, now with another item.

Choose one specific location each round. Invest all your tools toward that task alone.

## ChessV

ChessV is a free, open-source universal chess program with a graphical user interface, sophisticated AI engine, and other features of traditional Chess programs. As a "universal" chess program, it not only plays orthodox Chess, it is also capable of playing games reasonbly similar to Chess. It currently plays over 100 different chess variants, and can be programmed to play additional variants.

Features
 - Plays over 100 different Chess variants, including some that are quite exotic.
 - Has a fully-featured graphical user interface, but the engine can also be used separately under another GUI (such as WinBoard) and other compliant engines can be used with ChessV's GUI.
 - Has a scripting language to allow configuration of new variants. It supports combining existing pieces and rules, and even defining new pieces, but creation of new rules is not supported.
 - Plays with a fairly high level of skill. The engine can also be configured to weaken its skill level.
 - ChessV is a .NET application so it can be run under Linux or MacOS using Mono.

http://www.chessv.org/

## ArchipelagoMW

Archipelago provides a generic framework for developing multiworld capability for game randomizers. In all cases, presently, Archipelago is also the randomizer itself.

https://github.com/ArchipelagoMW/Archipelago

https://archipelago.gg/

## Not implemented yet (TODO)

Bugs:

 - Generation rarely (5%?) fails due to lack of items in item pool for goal. (Should be fixed in existing commits, but not released at time of writing.)
 - Engine Elo reduction item is incorrectly named. (It's a person's name, not an acronym.)
 - Draw by repetition happens in 2 moves, not 3. (Honestly, I think this is a problem in the base ChessV client. If true, I'm inclined to believe that fixing it would be quite difficult.)

Locations:

 - "Discovered Attack" where a piece which was not under attack becomes under attack but not by the piece you moved
 - "Pin" and "Skewer" where a piece would be under attack if not for another piece on the same side. If the higher value piece is attacked, it's a skewer, otheerwise it's a pin
 - A location for which one performs the French move

Randomizer options and features:

 - Simplified Fairy Pieces option: only standard Chess pieces / all fairy armies / customize armies. The last option would lead to the current behaviour. This is only necessary because OptionSets don't show up on the settings menu on the Archipelago website, unless you navigate to the advanced Weighted Settings page.
 - Progressive Goal option. Your enemy's pieces are also scattered across the multiworld! (The current design can make progression too easy.)
 - Non-Progressive Material option. Pieces will not be selected progressively from a set, but instead placed with specific names in your world. This means you would find a Bishop or Cleric rather than a Progressive Minor Piece or Progressive Major Piece. (They are unlikely to come with pre-determined locations.)
 - Internal "difficulty" acknowledgement in the sphere generation based on certain settings. (Mixed pawns makes not only your pawn items much weaker, but also your pieces which must navigate past them. Likewise, Stable positions are easier to study but can leave the player stuck with an awkward layout.)

Client features:

 - The word "CHECK" in large red text if a King the player controls is threatened.
 - Reconnect or warn user of disconnect when the computer goes to sleep.
 - Maybe it's possible to change the seed? I think you can modify slot data during a game... this would be another alternative to the "Stable Stuck" seed problem where a player must play out a weak position.
