using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archipelago.APChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwGameCharacterizationTests
    {
        private const string StandardArmy = "";
        private const string ColourboundClobberers = "Colourbound Clobberers (Betza)";
        private const string RemarkableRookies = "Remarkable Rookies (Betza)";
        private const string NuttyKnights = "Nutty Knights (Betza)";

        [TestCleanup]
        public void Cleanup()
        {
            ApmwCore._instance = null;
            ApmwConfig._instance = null;
        }

        [DataTestMethod]
        [DataRow(8, 0, StandardArmy, "rnbqkbnr")]
        [DataRow(8, 1, StandardArmy, "RNBQKBNR")]
        [DataRow(8, 0, ColourboundClobberers, "gxeakexg")]
        [DataRow(8, 1, ColourboundClobberers, "GXEAKEXG")]
        [DataRow(8, 0, RemarkableRookies, "stickits")]
        [DataRow(8, 1, RemarkableRookies, "STICKITS")]
        [DataRow(8, 0, NuttyKnights, "hlmykmlh")]
        [DataRow(8, 1, NuttyKnights, "HLMYKMLH")]
        [DataRow(10, 0, StandardArmy, "rnabqkbcnr")]
        [DataRow(10, 1, StandardArmy, "RNABQKBCNR")]
        [DataRow(10, 0, ColourboundClobberers, "gxqeakecxg")]
        [DataRow(10, 1, ColourboundClobberers, "GXQEAKECXG")]
        [DataRow(10, 0, RemarkableRookies, "staickiqts")]
        [DataRow(10, 1, RemarkableRookies, "STAICKIQTS")]
        [DataRow(10, 0, NuttyKnights, "hlamykmclh")]
        [DataRow(10, 1, NuttyKnights, "HLAMYKMCLH")]
        public void RegisteredVariant_SetupRowsPinCpuArmyAndBoardShape(
            int numFiles,
            int humanPlayer,
            string enemyArmy,
            string expectedCpuBackRank)
        {
            ApmwChessGame game = CreateGame(numFiles, humanPlayer, enemyArmy);
            string[] rows = FenRows(game);

            Assert.AreEqual(numFiles, game.Board.NumFiles);
            Assert.AreEqual(8, game.Board.NumRanks);
            Assert.AreEqual(numFiles * 8, game.Board.NumSquares);
            Assert.AreEqual(numFiles * 8 + 6, game.Board.NumSquaresExtended);
            Assert.AreEqual(numFiles == 8 ? typeof(ApmwChessGame) : typeof(ApmwGrandChess), game.GetType());
            Assert.AreEqual(8, rows.Length, "APMW setup must contain exactly eight ranks.");
            foreach (string row in rows)
                Assert.AreEqual(numFiles, ExpandedRowWidth(row), "Unexpected FEN row width: " + row);

            int cpuBackRankIndex = humanPlayer == 0 ? 0 : 7;
            int cpuPawnRankIndex = humanPlayer == 0 ? 1 : 6;
            Assert.AreEqual(expectedCpuBackRank, rows[cpuBackRankIndex]);
            Assert.AreEqual(
                new string(humanPlayer == 0 ? 'p' : 'P', numFiles),
                rows[cpuPawnRankIndex]);
            CollectionAssert.AreEqual(
                ExpectedSetupRows(numFiles, humanPlayer, expectedCpuBackRank),
                rows,
                "The expanded APMW setup rows changed.");
            Assert.IsFalse(game.FENStart.Contains("#{"), "FENStart should be fully expanded.");
        }

        [DataTestMethod]
        [DataRow(0, StandardArmy, "ac")]
        [DataRow(1, StandardArmy, "ac")]
        [DataRow(0, ColourboundClobberers, "qc")]
        [DataRow(1, ColourboundClobberers, "qc")]
        [DataRow(0, RemarkableRookies, "aq")]
        [DataRow(1, RemarkableRookies, "aq")]
        [DataRow(0, NuttyKnights, "ac")]
        [DataRow(1, NuttyKnights, "ac")]
        public void SuperSizedVariant_RegistersCurrentArmyAttendantsForPromotion(
            int humanPlayer,
            string enemyArmy,
            string expectedAttendants)
        {
            ApmwChessGame game = CreateGame(10, humanPlayer, enemyArmy);

            Assert.IsTrue(
                game.PromotionTypes.EndsWith(expectedAttendants, StringComparison.Ordinal),
                "PromotionTypes did not end with the current super-sized attendant pair. Actual: " +
                    game.PromotionTypes);
            Assert.AreEqual(
                expectedAttendants.Length,
                game.ParseTypeListFromString(expectedAttendants).Count,
                "Both attendant notations should resolve to registered promotion piece types.");
        }

        [DataTestMethod]
        [DataRow(8, 0, StandardArmy, "QRNBQRNB")]
        [DataRow(8, 1, StandardArmy, "QRNBqrnb")]
        [DataRow(8, 0, ColourboundClobberers, "QRNBQRNBgxea")]
        [DataRow(8, 1, ColourboundClobberers, "QRNBqrnbgxea")]
        [DataRow(8, 0, RemarkableRookies, "QRNBQRNBstic")]
        [DataRow(8, 1, RemarkableRookies, "QRNBqrnbstic")]
        [DataRow(8, 0, NuttyKnights, "QRNBQRNBhlmy")]
        [DataRow(8, 1, NuttyKnights, "QRNBqrnbhlmy")]
        [DataRow(10, 0, StandardArmy, "QRNBQRNBac")]
        [DataRow(10, 1, StandardArmy, "QRNBqrnbac")]
        [DataRow(10, 0, ColourboundClobberers, "QRNBQRNBgxeaqc")]
        [DataRow(10, 1, ColourboundClobberers, "QRNBqrnbgxeaqc")]
        [DataRow(10, 0, RemarkableRookies, "QRNBQRNBsticaq")]
        [DataRow(10, 1, RemarkableRookies, "QRNBqrnbsticaq")]
        [DataRow(10, 0, NuttyKnights, "QRNBQRNBhlmyac")]
        [DataRow(10, 1, NuttyKnights, "QRNBqrnbhlmyac")]
        public void RegisteredVariant_PromotionTypeStringsAreStable(
            int numFiles,
            int humanPlayer,
            string enemyArmy,
            string expectedPromotionTypes)
        {
            ApmwChessGame game = CreateGame(numFiles, humanPlayer, enemyArmy);

            Assert.AreEqual(expectedPromotionTypes, game.PromotionTypes);
        }

        [DataTestMethod]
        [DataRow(8, 0, "kqAH", "AHkq", "0:e1-c1:a1-d1:A|0:e1-g1:h1-f1:H|1:e8-g8:h8-f8:k|1:e8-c8:a8-d8:q")]
        [DataRow(8, 1, "KQah", "ahKQ", "0:e1-g1:h1-f1:K|0:e1-c1:a1-d1:Q|1:e8-c8:a8-d8:a|1:e8-g8:h8-f8:h")]
        [DataRow(10, 0, "kqAJ", "AJkq", "0:f1-d1:a1-e1:A|0:f1-h1:j1-g1:J|1:f8-h8:j8-g8:k|1:f8-d8:a8-e8:q")]
        [DataRow(10, 1, "KQaj", "ajKQ", "0:f1-h1:j1-g1:K|0:f1-d1:a1-e1:Q|1:f8-d8:a8-e8:a|1:f8-h8:j8-g8:j")]
        public void RegisteredVariant_CastlingRightsAndKingEndpointsAreStable(
            int numFiles,
            int humanPlayer,
            string expectedRights,
            string expectedSavedRights,
            string expectedMoveRegistrations)
        {
            ApmwChessGame game = CreateGame(numFiles, humanPlayer, StandardArmy);

            Assert.AreEqual(expectedRights, game.GetCustomProperty("CastleRooks"));
            Assert.AreEqual(expectedRights, game.FENStart.Split(' ')[3]);
            Assert.AreEqual(expectedSavedRights, game.FEN["castling"]);
            Assert.AreEqual(expectedMoveRegistrations, CastlingMoveRegistrationSignature(game));
        }

        [DataTestMethod]
        [DataRow(false, 0, "2,0:Checkers|2,2:Checkers|2,4:Berolina Pawn|2,6:Checkers|2,7:Checkers|3,0:Checkers|3,1:Pawn|3,2:Berolina Pawn|3,3:Pawn|3,4:Pawn|3,5:Berolina Pawn|3,6:Checkers|3,7:Checkers|4,0:Charging Knight|4,1:Colonel|4,3:Scout|4,4:King|4,5:Rook|4,6:Charging Knight|4,7:Bishop", "RGMBUY")]
        [DataRow(false, 1, "2,0:Checkers|2,2:Checkers|2,4:Berolina Pawn|2,6:Checkers|2,7:Checkers|3,0:Checkers|3,1:Pawn|3,2:Berolina Pawn|3,3:Pawn|3,4:Pawn|3,5:Berolina Pawn|3,6:Checkers|3,7:Checkers|4,0:Charging Knight|4,1:Colonel|4,3:Scout|4,4:King|4,5:Rook|4,6:Charging Knight|4,7:Bishop", "rgmbuy")]
        [DataRow(true, 0, "2,0:Pawn|2,3:Checkers|2,4:Berolina Pawn|2,5:Checkers|2,8:Checkers|3,0:Checkers|3,1:Pawn|3,2:Checkers|3,3:Pawn|3,4:Pawn|3,5:Berolina Pawn|3,6:Berolina Pawn|3,7:Checkers|3,8:Checkers|3,9:Berolina Pawn|4,0:Charging Knight|4,2:Colonel|4,4:Scout|4,5:King|4,7:Rook|4,8:Charging Knight|4,9:Bishop", "RGMBUY")]
        [DataRow(true, 1, "2,0:Pawn|2,3:Checkers|2,4:Berolina Pawn|2,5:Checkers|2,8:Checkers|3,0:Checkers|3,1:Pawn|3,2:Checkers|3,3:Pawn|3,4:Pawn|3,5:Berolina Pawn|3,6:Berolina Pawn|3,7:Checkers|3,8:Checkers|3,9:Berolina Pawn|4,0:Charging Knight|4,2:Colonel|4,4:Scout|4,5:King|4,7:Rook|4,8:Charging Knight|4,9:Bishop", "rgmbuy")]
        public void StableLocations_LegacyItemization_HasGoldenPieceOrdering(
            bool isSuperSized,
            int humanPlayer,
            string expectedSignature,
            string expectedPromotions)
        {
            ApmwFuzzCase fuzzCase = (isSuperSized
                ? ApmwFuzzCase.DefaultSuperSized()
                : ApmwFuzzCase.DefaultStandard()).With(builder =>
                {
                    builder.CaseName = "stable-legacy-ordering";
                    builder.PieceLocations = PieceLocations.Stable;
                    builder.ProgressionItemization = ProgressionItemization.Legacy;
                    builder.PlayAsWhiteCount = humanPlayer == 0 ? 1 : 0;
                });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                ApmwFuzzGenerationResult result = scope.RunItemHandlerGeneration();
                Assert.AreEqual(expectedSignature, PieceSetSignature(result.PlayerPieceSet));
                Assert.AreEqual(expectedPromotions, result.PromotionTypes);
            }
        }

        private static ApmwChessGame CreateGame(int numFiles, int humanPlayer, string enemyArmy)
        {
            ApmwCore._instance = new ApmwCore();
            ApmwConfig._instance = null;
            ApmwCore core = ApmwCore.getInstance();
            core.foundPockets = 0;
            core.foundPocketRange = 0;
            core.foundPocketGems = 0;
            core.foundPawns = 0;
            core.foundMinors = 0;
            core.foundMajors = 0;
            core.foundJacks = 0;
            core.foundQueens = 0;
            core.foundAmazons = 0;
            core.foundConsuls = 0;
            core.foundKingPromotions = 0;
            core.foundPawnForwardness = 0;
            core.foundChessmen = 0;
            core.foundMaterialBudget = 0;
            core.foundCastlers = 0;
            core.isGrand = numFiles == 10;
            core.GeriProvider = () => humanPlayer;
            core.EngineWeakeningProvider = () => 0;
            core.PlayerPocketPiecesProvider = () => new List<PieceType>();
            core.PlayerPieceSetProvider = files => BuildSimplePlayerSetup(files, humanPlayer);

            string gameName = numFiles == 8
                ? ApmwConstants.GameNameStandard
                : ApmwConstants.GameNameSuperSized;
            Dictionary<string, string> definitions = string.IsNullOrEmpty(enemyArmy)
                ? null
                : new Dictionary<string, string> { ["enemy_army"] = enemyArmy };
            return (ApmwChessGame)new ChessV.Manager.Manager().CreateGame(gameName, definitions);
        }

        private static (Dictionary<KeyValuePair<int, int>, PieceType>, string) BuildSimplePlayerSetup(
            int numFiles,
            int humanPlayer)
        {
            ApmwCore core = ApmwCore.getInstance();
            PieceType king = core.kings[0];
            PieceType rook = core.majors.Single(piece => piece.Name == "Rook");
            PieceType pawn = core.pawns.Single(piece => piece.Name == "Pawn");
            var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
            for (int file = 0; file < numFiles; file++)
                pieces[new KeyValuePair<int, int>(3, file)] = pawn;
            pieces[new KeyValuePair<int, int>(4, 0)] = rook;
            pieces[new KeyValuePair<int, int>(4, numFiles / 2)] = king;
            pieces[new KeyValuePair<int, int>(4, numFiles - 1)] = rook;
            return (pieces, humanPlayer == 0 ? "QRNB" : "qrnb");
        }

        private static string CastlingMoveRegistrationSignature(ApmwChessGame game)
        {
            CastlingRule rule = game.GetRules().OfType<CastlingRule>().Single();
            Type ruleType = typeof(CastlingRule);
            Array moves = (Array)ruleType
                .GetField("castlingMoves", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(rule);
            int[] moveCounts = (int[])ruleType
                .GetField("nCastlingMoves", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(rule);
            Type moveType = moves.GetType().GetElementType();
            FieldInfo kingFrom = moveType.GetField("KingFromSquare");
            FieldInfo kingTo = moveType.GetField("KingToSquare");
            FieldInfo rookFrom = moveType.GetField("OtherFromSquare");
            FieldInfo rookTo = moveType.GetField("OtherToSquare");
            FieldInfo privilege = moveType.GetField("PrivChar");
            var registrations = new List<string>();

            for (int player = 0; player < moveCounts.Length; player++)
                for (int index = 0; index < moveCounts[player]; index++)
                {
                    object move = moves.GetValue(player, index);
                    registrations.Add(
                        player + ":" +
                        game.GetSquareNotation((int)kingFrom.GetValue(move)) + "-" +
                        game.GetSquareNotation((int)kingTo.GetValue(move)) + ":" +
                        game.GetSquareNotation((int)rookFrom.GetValue(move)) + "-" +
                        game.GetSquareNotation((int)rookTo.GetValue(move)) + ":" +
                        privilege.GetValue(move));
                }

            return string.Join("|", registrations);
        }

        private static string[] FenRows(ApmwChessGame game)
        {
            return game.FENStart.Split(' ')[0].Split('/');
        }

        private static string[] ExpectedSetupRows(
            int numFiles,
            int humanPlayer,
            string cpuBackRank)
        {
            string empty = numFiles.ToString();
            string humanBackRank = numFiles == 8 ? "r3k2r" : "r4k3r";
            string humanPawns = new string('p', numFiles);
            string cpuPawns = new string(humanPlayer == 0 ? 'p' : 'P', numFiles);
            if (humanPlayer == 0)
            {
                humanBackRank = humanBackRank.ToUpperInvariant();
                humanPawns = humanPawns.ToUpperInvariant();
                return new[]
                {
                    cpuBackRank, cpuPawns, empty, empty, empty, empty, humanPawns, humanBackRank,
                };
            }

            return new[]
            {
                humanBackRank, humanPawns, empty, empty, empty, empty, cpuPawns, cpuBackRank,
            };
        }

        private static int ExpandedRowWidth(string row)
        {
            int width = 0;
            for (int index = 0; index < row.Length;)
            {
                if (!char.IsDigit(row[index]))
                {
                    width++;
                    index++;
                    continue;
                }

                int end = index + 1;
                while (end < row.Length && char.IsDigit(row[end]))
                    end++;
                width += int.Parse(row.Substring(index, end - index));
                index = end;
            }
            return width;
        }

        private static string PieceSetSignature(
            IReadOnlyDictionary<KeyValuePair<int, int>, PieceType> pieces)
        {
            return string.Join("|", pieces
                .OrderBy(item => item.Key.Key)
                .ThenBy(item => item.Key.Value)
                .Select(item => item.Key.Key + "," + item.Key.Value + ":" + item.Value.Name));
        }
    }
}
