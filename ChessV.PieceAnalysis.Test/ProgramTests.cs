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
        public void Main_AllModeReportsKnownConstructionFailuresWithoutAbortingRun()
        {
            var result = ExecuteProgram("--all");

            Assert.AreEqual(0, result.ExitCode);
            using var document = JsonDocument.Parse(result.StdOut);
            var pieces = document.RootElement.GetProperty("pieces").EnumerateArray().ToList();
            var unresolvedPieces = document.RootElement.GetProperty("unresolvedPieces");
            var failedPieces = pieces
                .Where(piece => piece.GetProperty("constructionError").ValueKind != JsonValueKind.Null)
                .ToList();
            var king = pieces.Single(piece => piece.GetProperty("name").GetString() == "King");
            var pawn = pieces.Single(piece => piece.GetProperty("name").GetString() == "Pawn");

            Assert.IsTrue(pieces.Count > 80, "Expected the engine-wide audit to discover many constructible piece types.");
            Assert.AreEqual(0, unresolvedPieces.GetArrayLength());
            Assert.IsTrue(failedPieces.Count >= 2);
            Assert.AreEqual(JsonValueKind.String, king.GetProperty("constructionError").ValueKind);
            Assert.AreEqual(JsonValueKind.String, pawn.GetProperty("constructionError").ValueKind);
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
