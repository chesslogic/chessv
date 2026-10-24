using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ChessV.PieceAnalysis.Test
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void Main_MissingPieceValueReturnsExitCodeOne()
        {
            var result = ExecuteProgram("--piece");

            Assert.AreEqual(1, result.ExitCode);
            StringAssert.Contains(result.StdErr, "Missing value for --piece.");
            Assert.AreEqual(string.Empty, result.StdOut);
        }

        [TestMethod]
        public void Main_MissingFilesValueReturnsExitCodeOne()
        {
            var result = ExecuteProgram("--files");

            Assert.AreEqual(1, result.ExitCode);
            StringAssert.Contains(result.StdErr, "Missing value for --files.");
            Assert.AreEqual(string.Empty, result.StdOut);
        }

        [TestMethod]
        public void Main_UnknownArgumentReturnsExitCodeOne()
        {
            var result = ExecuteProgram("--bogus");

            Assert.AreEqual(1, result.ExitCode);
            StringAssert.Contains(result.StdErr, "Unknown argument: --bogus");
            Assert.AreEqual(string.Empty, result.StdOut);
        }

        [TestMethod]
        public void Main_InvalidTierReturnsExitCodeOne()
        {
            var result = ExecuteProgram("--piece", "Knight", "--tier", "NotATier");

            Assert.AreEqual(1, result.ExitCode);
            StringAssert.Contains(result.StdErr, "Invalid --tier value.");
        }

        [TestMethod]
        public void Main_OmitsBoardArgumentsByDefaultUsingEightByEightBoard()
        {
            var result = ExecuteProgram("--piece", "Knight");

            Assert.AreEqual(0, result.ExitCode);
            using var document = JsonDocument.Parse(result.StdOut);
            Assert.AreEqual(8, document.RootElement.GetProperty("board").GetProperty("files").GetInt32());
            Assert.AreEqual(8, document.RootElement.GetProperty("board").GetProperty("ranks").GetInt32());
            Assert.AreEqual(1, document.RootElement.GetProperty("pieces").GetArrayLength());
        }

        [TestMethod]
        public void Main_AllModeIncludesCanonicalKingAndPawnWithoutNotationCollisions()
        {
            var result = ExecuteProgram("--all");

            Assert.AreEqual(0, result.ExitCode);
            using var document = JsonDocument.Parse(result.StdOut);
            var pieces = document.RootElement.GetProperty("pieces").EnumerateArray().ToList();
            var unresolvedPieces = document.RootElement.GetProperty("unresolvedPieces");
            var king = pieces.Single(piece => piece.GetProperty("name").GetString() == "King");
            var pawn = pieces.Single(piece => piece.GetProperty("name").GetString() == "Pawn");

            Assert.IsTrue(pieces.Count > 80, "Expected the engine-wide audit to discover many constructible piece types.");
            Assert.AreEqual(0, unresolvedPieces.GetArrayLength());
            Assert.AreEqual(JsonValueKind.Null, king.GetProperty("constructionError").ValueKind);
            Assert.AreEqual(JsonValueKind.Null, pawn.GetProperty("constructionError").ValueKind);
            Assert.AreEqual(325, king.GetProperty("midgameValue").GetInt32());
            Assert.AreEqual(100, pawn.GetProperty("midgameValue").GetInt32());
        }

        [DataTestMethod]
        [DataRow(6, 8)]
        [DataRow(8, 8)]
        [DataRow(10, 8)]
        [DataRow(10, 10)]
        [DataRow(12, 10)]
        public void Main_ApmwModeIncludesEveryCatalogPieceWithCanonicalValues(int files, int ranks)
        {
            var result = ExecuteProgram("--apmw", "--files", files.ToString(), "--ranks", ranks.ToString(), "--compare-8x8");

            Assert.AreEqual(0, result.ExitCode, result.StdErr);
            using var document = JsonDocument.Parse(result.StdOut);
            var pieces = document.RootElement.GetProperty("pieces").EnumerateArray().ToList();
            Assert.AreEqual(TierReferenceData.Entries.Count, pieces.Count);
            Assert.AreEqual(0, document.RootElement.GetProperty("unresolvedPieces").GetArrayLength());
            foreach (var piece in pieces)
            {
                var reference = TierReferenceData.Entries[piece.GetProperty("name").GetString()];
                Assert.AreEqual(reference.MidgameValue, piece.GetProperty("midgameValue").GetInt32());
                Assert.AreEqual(reference.EndgameValue, piece.GetProperty("endgameValue").GetInt32());
                Assert.AreEqual("ApmwChess", piece.GetProperty("materialValueSource").GetString());
                Assert.AreEqual(JsonValueKind.Null, piece.GetProperty("constructionError").ValueKind);
                Assert.AreEqual(8, piece.GetProperty("comparison").GetProperty("baselineBoard").GetProperty("files").GetInt32());
                Assert.AreEqual(8, piece.GetProperty("comparison").GetProperty("baselineBoard").GetProperty("ranks").GetInt32());
            }
        }

        [TestMethod]
        public void Main_GeometryComparisonReportsBaselineWithoutChangingMaterial()
        {
            var result = ExecuteProgram("--piece", "Knight", "--files", "12", "--ranks", "10", "--compare-8x8");

            Assert.AreEqual(0, result.ExitCode, result.StdErr);
            using var document = JsonDocument.Parse(result.StdOut);
            Assert.IsFalse(document.RootElement.GetProperty("eightByEightThresholdsApplied").GetBoolean());
            var piece = document.RootElement.GetProperty("pieces")[0];
            var comparison = piece.GetProperty("comparison");
            Assert.AreEqual(325, piece.GetProperty("midgameValue").GetInt32());
            Assert.AreEqual(8, comparison.GetProperty("baselineBoard").GetProperty("files").GetInt32());
            Assert.AreEqual(8, comparison.GetProperty("baselineBoard").GetProperty("ranks").GetInt32());
            Assert.AreEqual(5.25, comparison.GetProperty("baselineMobilityByDensityPercent").GetProperty("10").GetDouble(), 0.0000001);
            Assert.AreEqual(
                (89.0 / 15.0) / 5.25,
                comparison.GetProperty("mobilityRatioByDensityPercent").GetProperty("10").GetDouble(),
                0.0000001);
            Assert.IsTrue(piece.GetProperty("emptyBoardTravel").GetProperty("averageMovesHomeToFarRank").GetDouble() >
                comparison.GetProperty("baselineEmptyBoardTravel").GetProperty("averageMovesHomeToFarRank").GetDouble());
        }

        [TestMethod]
        public void Main_CustomMovePieceReportsLimitationsAndUndefinedMobilityRatio()
        {
            var result = ExecuteProgram("--piece", "Checkers", "--files", "6", "--ranks", "8", "--compare-8x8");

            Assert.AreEqual(0, result.ExitCode, result.StdErr);
            using var document = JsonDocument.Parse(result.StdOut);
            var piece = document.RootElement.GetProperty("pieces")[0];
            Assert.AreEqual(40, piece.GetProperty("midgameValue").GetInt32());
            Assert.IsTrue(piece.GetProperty("analysisLimitations").GetArrayLength() > 0);
            Assert.AreEqual(JsonValueKind.String, piece.GetProperty("emptyBoardTravel").GetProperty("unsupportedReason").ValueKind);
            Assert.AreEqual(JsonValueKind.Null,
                piece.GetProperty("comparison").GetProperty("mobilityRatioByDensityPercent").GetProperty("10").ValueKind);
        }

        [DataTestMethod]
        [DataRow("--all")]
        [DataRow("--piece")]
        public void Main_ApmwModeRejectsConflictingPieceSelection(string option)
        {
            var result = option == "--piece"
                ? ExecuteProgram("--apmw", option, "Knight")
                : ExecuteProgram("--apmw", option);

            Assert.AreEqual(1, result.ExitCode);
            StringAssert.Contains(result.StdErr, "--apmw cannot be combined");
            Assert.AreEqual(string.Empty, result.StdOut);
        }

        private static ProgramExecutionResult ExecuteProgram(params string[] args)
        {
            var originalOut = Console.Out;
            var originalErr = Console.Error;
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            try
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
                var exitCode = Program.Main(args);
                return new ProgramExecutionResult(exitCode, stdout.ToString(), stderr.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }
        }

        private sealed class ProgramExecutionResult
        {
            public ProgramExecutionResult(int exitCode, string stdOut, string stdErr)
            {
                ExitCode = exitCode;
                StdOut = stdOut;
                StdErr = stdErr;
            }

            public int ExitCode { get; private set; }
            public string StdOut { get; private set; }
            public string StdErr { get; private set; }
        }
    }
}
