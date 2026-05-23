using System;
using System.Collections.Generic;
using System.Linq;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    internal static class ApmwFuzzInvariants
    {
        private const int BoardRanks = 8;
        private const int PocketSquaresPerGame = 6;
        private const int PocketSlotsPerPlayer = 3;

        public static void AssertGenerationInvariants(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzGenerationResult result,
            string stageName)
        {
            Assert.IsNotNull(result, StageMessage(fuzzCase, stageName, "generation result was null."));
            Assert.AreEqual(
                PocketSlotsPerPlayer,
                result.PocketPieces.Count,
                StageMessage(fuzzCase, stageName, "generated pocket piece slot count did not match the APMW board."));

            AssertNoUnresolvedVariables(fuzzCase, stageName, result.PromotionTypes, "generated promotion type string");

            for (int index = 0; index < result.PocketPieces.Count; index++)
            {
                var pieceType = result.PocketPieces[index];
                if (pieceType == null)
                    continue;

                Assert.IsNotNull(
                    pieceType.Notation,
                    StageMessage(fuzzCase, stageName, "generated pocket piece notation was null at slot " + index + "."));
                Assert.IsTrue(
                    pieceType.Notation.Length >= 2,
                    StageMessage(fuzzCase, stageName, "generated pocket piece notation did not include both players at slot " + index + "."));
                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(pieceType.Notation[0]),
                    StageMessage(fuzzCase, stageName, "generated pocket piece white notation was empty at slot " + index + "."));
                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(pieceType.Notation[1]),
                    StageMessage(fuzzCase, stageName, "generated pocket piece black notation was empty at slot " + index + "."));
            }
        }

        public static void AssertCreatedGameInvariants(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStartupResult startup,
            string stageName)
        {
            Assert.IsNotNull(startup, StageMessage(fuzzCase, stageName, "startup result was null."));
            Assert.IsNotNull(startup.Game, StageMessage(fuzzCase, stageName, "created game was null."));

            var game = startup.Game;
            AssertBoardDimensions(fuzzCase, startup, stageName);
            AssertGameName(fuzzCase, startup, stageName);
            AssertFenExpansion(fuzzCase, game, stageName);
            AssertStartingPieces(fuzzCase, game, stageName);
            AssertActualPieceSquares(fuzzCase, game, stageName);
            AssertPromotionNotation(fuzzCase, game, stageName);
        }

        public static void AssertInitialMoveGeneration(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStartupResult startup,
            string stageName)
        {
            Assert.IsNotNull(startup, StageMessage(fuzzCase, stageName, "startup result was null."));
            Assert.IsNotNull(startup.Game, StageMessage(fuzzCase, stageName, "created game was null."));

            var game = startup.Game;
            var root = game.RootMoveListForTest;
            Assert.IsNotNull(root, StageMessage(fuzzCase, stageName, "root move list was null."));

            root.Reset();
            AssertMoveListReset(fuzzCase, root, stageName);

            game.GenerateMovesForTest(game.CurrentSide);
            AssertMoveListCoherent(fuzzCase, game, root, stageName);

            root.Reset();
            AssertMoveListReset(fuzzCase, root, stageName);
        }

        private static void AssertBoardDimensions(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStartupResult startup,
            string stageName)
        {
            int expectedFiles = fuzzCase.IsSuperSized ? 10 : 8;
            int expectedPlayableSquares = expectedFiles * BoardRanks;
            int expectedExtendedSquares = expectedPlayableSquares + PocketSquaresPerGame;
            var game = startup.Game;

            Assert.AreEqual(
                expectedFiles,
                startup.ExpectedNumFiles,
                StageMessage(fuzzCase, stageName, "startup expected file count did not match the case."));
            Assert.AreEqual(
                expectedFiles,
                game.NumFiles,
                StageMessage(fuzzCase, stageName, "created game file count did not match the case."));
            Assert.AreEqual(
                BoardRanks,
                game.NumRanks,
                StageMessage(fuzzCase, stageName, "created game rank count did not match APMW."));
            Assert.IsNotNull(game.Board, StageMessage(fuzzCase, stageName, "created game board was null."));
            Assert.AreEqual(
                expectedFiles,
                game.Board.NumFiles,
                StageMessage(fuzzCase, stageName, "created board file count did not match the case."));
            Assert.AreEqual(
                BoardRanks,
                game.Board.NumRanks,
                StageMessage(fuzzCase, stageName, "created board rank count did not match APMW."));
            Assert.AreEqual(
                expectedPlayableSquares,
                game.Board.NumSquares,
                StageMessage(fuzzCase, stageName, "created board playable square count did not match APMW."));
            Assert.AreEqual(
                expectedExtendedSquares,
                game.Board.NumSquaresExtended,
                StageMessage(fuzzCase, stageName, "created board extended square count did not include APMW pockets."));
        }

        private static void AssertGameName(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStartupResult startup,
            string stageName)
        {
            var game = startup.Game;
            Assert.AreEqual(
                fuzzCase.GameName,
                startup.GameName,
                StageMessage(fuzzCase, stageName, "startup game name did not match the case."));
            Assert.IsNotNull(
                game.GameAttribute,
                StageMessage(fuzzCase, stageName, "created game attribute was null."));
            Assert.AreEqual(
                fuzzCase.GameName,
                game.GameAttribute.GameName,
                StageMessage(fuzzCase, stageName, "created game attribute name did not match the case."));
            Assert.AreEqual(
                fuzzCase.GameName,
                game.Name,
                StageMessage(fuzzCase, stageName, "created game variable name did not match the case."));
        }

        private static void AssertFenExpansion(ApmwFuzzCase fuzzCase, Game game, string stageName)
        {
            AssertNoUnresolvedVariables(fuzzCase, stageName, game.FENStart, "expanded FENStart");
            AssertNoUnresolvedVariables(fuzzCase, stageName, FirstFenPart(game.FENStart), "expanded FENStart array");

            var currentFen = game.FEN;
            AssertNoUnresolvedVariables(fuzzCase, stageName, currentFen.ToString(), "current FEN");
            AssertNoUnresolvedVariables(fuzzCase, stageName, currentFen["array"], "current FEN array");
            AssertNoUnresolvedVariables(fuzzCase, stageName, currentFen["pieces in hand"], "current FEN pocket string");

            // Game.Array remains the public template for APMW and intentionally
            // contains variables. FENStart/current FEN are the safely accessible
            // expanded values, so avoid reflection-heavy custom-property checks.
        }

        private static void AssertStartingPieces(ApmwFuzzCase fuzzCase, Game game, string stageName)
        {
            Assert.IsNotNull(
                game.StartingPieces,
                StageMessage(fuzzCase, stageName, "starting piece map was null."));
            Assert.IsNotNull(
                game.StartingPieceSquares,
                StageMessage(fuzzCase, stageName, "starting piece square map was null."));
            Assert.IsNotNull(
                game.StartingPieceCount,
                StageMessage(fuzzCase, stageName, "starting piece count map was null."));
            Assert.AreEqual(
                game.NumPlayers,
                game.StartingPieceSquares.GetLength(0),
                StageMessage(fuzzCase, stageName, "starting piece square player dimension did not match the game."));
            Assert.AreEqual(
                game.Board.NumSquaresExtended,
                game.StartingPieceSquares.GetLength(1),
                StageMessage(fuzzCase, stageName, "starting piece square board dimension did not match the board."));
            Assert.AreEqual(
                game.NumPlayers,
                game.StartingPieceCount.Length,
                StageMessage(fuzzCase, stageName, "starting piece count player dimension did not match the game."));
            Assert.AreEqual(
                game.Board.NumSquares,
                game.StartingPieces.Count,
                StageMessage(fuzzCase, stageName, "starting piece map did not cover every playable square."));

            var seenSquares = new HashSet<int>();
            var countedPieces = new int[game.NumPlayers];

            foreach (var pair in game.StartingPieces)
            {
                int square = SquareFromNotation(fuzzCase, game, stageName, pair.Key);
                AssertPlayableSquare(fuzzCase, game, stageName, square, "starting square " + pair.Key);
                Assert.IsTrue(
                    seenSquares.Add(square),
                    StageMessage(fuzzCase, stageName, "starting square appeared more than once: " + pair.Key + "."));

                if (pair.Value == null)
                {
                    AssertNoStartingPieceFlags(fuzzCase, game, stageName, square, pair.Key);
                    continue;
                }

                AssertValidPlayer(fuzzCase, game, stageName, pair.Value.Player, "starting piece player at " + pair.Key);
                Assert.IsNotNull(
                    pair.Value.PieceType,
                    StageMessage(fuzzCase, stageName, "starting piece type was null at " + pair.Key + "."));

                countedPieces[pair.Value.Player]++;
                Assert.AreEqual(
                    1,
                    game.StartingPieceSquares[pair.Value.Player, square],
                    StageMessage(fuzzCase, stageName, "starting piece square flag was not set at " + pair.Key + "."));

                var boardPiece = game.Board[square];
                Assert.IsNotNull(
                    boardPiece,
                    StageMessage(fuzzCase, stageName, "board did not contain the starting piece at " + pair.Key + "."));
                Assert.AreEqual(
                    pair.Value.Player,
                    boardPiece.Player,
                    StageMessage(fuzzCase, stageName, "board piece player did not match starting map at " + pair.Key + "."));
                Assert.AreSame(
                    pair.Value.PieceType,
                    boardPiece.PieceType,
                    StageMessage(fuzzCase, stageName, "board piece type did not match starting map at " + pair.Key + "."));
            }

            for (int player = 0; player < game.NumPlayers; player++)
            {
                Assert.AreEqual(
                    countedPieces[player],
                    game.StartingPieceCount[player],
                    StageMessage(fuzzCase, stageName, "starting piece count did not match occupied starting squares for player " + player + "."));
                for (int square = game.Board.NumSquares; square < game.Board.NumSquaresExtended; square++)
                {
                    Assert.AreEqual(
                        0,
                        game.StartingPieceSquares[player, square],
                        StageMessage(fuzzCase, stageName, "extended pocket square was marked as a starting board square for player " + player + "."));
                }
            }
        }

        private static void AssertActualPieceSquares(ApmwFuzzCase fuzzCase, Game game, string stageName)
        {
            var seenSquares = new HashSet<int>();
            foreach (var piece in game.GetPieceList())
            {
                Assert.IsNotNull(piece, StageMessage(fuzzCase, stageName, "piece list contained a null piece."));
                AssertValidPlayer(fuzzCase, game, stageName, piece.Player, "active piece player");
                Assert.IsNotNull(
                    piece.PieceType,
                    StageMessage(fuzzCase, stageName, "active piece type was null."));
                AssertExtendedSquare(fuzzCase, game, stageName, piece.Square, "active piece square");
                Assert.IsTrue(
                    seenSquares.Add(piece.Square),
                    StageMessage(fuzzCase, stageName, "more than one active piece used square " + game.Board.GetDefaultSquareNotation(piece.Square) + "."));
                Assert.AreSame(
                    piece,
                    game.Board[piece.Square],
                    StageMessage(fuzzCase, stageName, "board contents did not match the active piece list at " + game.Board.GetDefaultSquareNotation(piece.Square) + "."));
            }
        }

        private static void AssertPromotionNotation(ApmwFuzzCase fuzzCase, Game game, string stageName)
        {
            var apmwGame = game as ApmwChessGame;
            if (apmwGame == null)
                return;

            AssertNoUnresolvedVariables(fuzzCase, stageName, apmwGame.PromotionTypes, "created game promotion type string");

            try
            {
                game.ParseTypeListFromString(apmwGame.PromotionTypes ?? string.Empty);
            }
            catch (Exception ex)
            {
                Assert.Fail(StageMessage(
                    fuzzCase,
                    stageName,
                    "created game promotion type string was not parseable: " + ex.Message));
            }
        }

        private static void AssertMoveListReset(ApmwFuzzCase fuzzCase, MoveList root, string stageName)
        {
            Assert.AreEqual(0, root.MoveCursor, StageMessage(fuzzCase, stageName, "reset move cursor was not zero."));
            Assert.AreEqual(0, root.Count, StageMessage(fuzzCase, stageName, "reset move count was not zero."));
            Assert.AreEqual(0, root.PickupCursorForTest, StageMessage(fuzzCase, stageName, "reset pickup cursor was not zero."));
            Assert.AreEqual(0, root.DropCursorForTest, StageMessage(fuzzCase, stageName, "reset drop cursor was not zero."));
        }

        private static void AssertMoveListCoherent(
            ApmwFuzzCase fuzzCase,
            Game game,
            MoveList root,
            string stageName)
        {
            AssertCursorInRange(fuzzCase, stageName, root.MoveCursor, MoveList.MAX_MOVES, "move cursor");
            Assert.AreEqual(
                root.MoveCursor,
                root.Count,
                StageMessage(fuzzCase, stageName, "move count did not match move cursor."));
            AssertCursorInRange(fuzzCase, stageName, root.PickupCursorForTest, MoveList.MAX_MOVES, "pickup cursor");
            AssertCursorInRange(fuzzCase, stageName, root.DropCursorForTest, MoveList.MAX_MOVES, "drop cursor");

            int previousPickupCursor = 0;
            int previousDropCursor = 0;
            for (int index = 0; index < root.MoveCursor; index++)
            {
                var move = root.GetMoveForTest(index);
                Assert.AreNotEqual(
                    MoveType.Invalid,
                    move.MoveType,
                    StageMessage(fuzzCase, stageName, "generated move " + index + " had an invalid move type."));
                AssertValidPlayer(fuzzCase, game, stageName, move.Player, "generated move player");
                AssertExtendedSquare(fuzzCase, game, stageName, move.FromSquare, "generated move " + index + " from-square");
                AssertExtendedSquare(fuzzCase, game, stageName, move.ToSquare, "generated move " + index + " to-square");
                Assert.IsTrue(
                    move.PickupCursor >= previousPickupCursor && move.PickupCursor <= root.PickupCursorForTest,
                    StageMessage(fuzzCase, stageName, "generated move " + index + " pickup cursor was not monotonic/coherent."));
                Assert.IsTrue(
                    move.DropCursor >= previousDropCursor && move.DropCursor <= root.DropCursorForTest,
                    StageMessage(fuzzCase, stageName, "generated move " + index + " drop cursor was not monotonic/coherent."));

                if (move.PieceMoved != null)
                    Assert.AreEqual(
                        move.Player,
                        move.PieceMoved.Player,
                        StageMessage(fuzzCase, stageName, "generated move " + index + " player did not match moved piece."));

                previousPickupCursor = move.PickupCursor;
                previousDropCursor = move.DropCursor;
            }
        }

        private static int SquareFromNotation(ApmwFuzzCase fuzzCase, Game game, string stageName, string notation)
        {
            try
            {
                return game.NotationToSquare(notation);
            }
            catch (Exception ex)
            {
                Assert.Fail(StageMessage(fuzzCase, stageName, "starting square notation was not parseable: " + notation + " (" + ex.Message + ")."));
                return -1;
            }
        }

        private static void AssertNoStartingPieceFlags(ApmwFuzzCase fuzzCase, Game game, string stageName, int square, string notation)
        {
            for (int player = 0; player < game.NumPlayers; player++)
            {
                Assert.AreEqual(
                    0,
                    game.StartingPieceSquares[player, square],
                    StageMessage(fuzzCase, stageName, "empty starting square was marked occupied for player " + player + " at " + notation + "."));
            }
        }

        private static void AssertCursorInRange(
            ApmwFuzzCase fuzzCase,
            string stageName,
            int cursor,
            int maxExclusive,
            string cursorName)
        {
            Assert.IsTrue(
                cursor >= 0 && cursor <= maxExclusive,
                StageMessage(fuzzCase, stageName, cursorName + " was outside the move-list storage range: " + cursor + "."));
        }

        private static void AssertValidPlayer(ApmwFuzzCase fuzzCase, Game game, string stageName, int player, string label)
        {
            Assert.IsTrue(
                player >= 0 && player < game.NumPlayers,
                StageMessage(fuzzCase, stageName, label + " was outside the player range: " + player + "."));
        }

        private static void AssertPlayableSquare(ApmwFuzzCase fuzzCase, Game game, string stageName, int square, string label)
        {
            Assert.IsTrue(
                square >= 0 && square < game.Board.NumSquares,
                StageMessage(fuzzCase, stageName, label + " was outside the playable board: " + square + "."));
        }

        private static void AssertExtendedSquare(ApmwFuzzCase fuzzCase, Game game, string stageName, int square, string label)
        {
            Assert.IsTrue(
                square >= 0 && square < game.Board.NumSquaresExtended,
                StageMessage(fuzzCase, stageName, label + " was outside the extended board: " + square + "."));
        }

        private static void AssertNoUnresolvedVariables(
            ApmwFuzzCase fuzzCase,
            string stageName,
            string value,
            string label)
        {
            Assert.IsNotNull(value, StageMessage(fuzzCase, stageName, label + " was null."));
            Assert.IsFalse(
                value.Contains("#{"),
                StageMessage(fuzzCase, stageName, label + " still contained an unresolved FEN variable: " + value));
        }

        private static string FirstFenPart(string fen)
        {
            if (fen == null)
                return null;

            int separator = fen.IndexOf(' ');
            return separator < 0 ? fen : fen.Substring(0, separator);
        }

        private static string StageMessage(ApmwFuzzCase fuzzCase, string stageName, string message)
        {
            return stageName + " failed for " + fuzzCase.ToDiagnosticString() + ": " + message;
        }
    }
}
