using ChessV;
using ChessV.Games;

namespace ChessV.Test
{
  [TestClass]
  public class DefensiveGuardTests
  {
    [TestMethod]
    public void GetFileNotation_NegativeIndex_ThrowsInformative()
    {
      var board = new Board(8, 8);
      var ex = Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetFileNotation(-1));
      StringAssert.Contains(ex.Message, "GetFileNotation");
      StringAssert.Contains(ex.Message, "NumFiles=8");
    }

    [TestMethod]
    public void GetFileNotation_PastEnd_ThrowsInformative()
    {
      var board = new Board(8, 8);
      var ex = Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetFileNotation(8));
      StringAssert.Contains(ex.Message, "out of range");
    }

    [TestMethod]
    public void GetFileNotation_ValidIndex_ReturnsLetter()
    {
      var board = new Board(8, 8);
      Assert.AreEqual("a", board.GetFileNotation(0));
      Assert.AreEqual("h", board.GetFileNotation(7));
    }

    [TestMethod]
    public void GetRankNotation_NegativeIndex_ThrowsInformative()
    {
      var board = new Board(8, 8);
      var ex = Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetRankNotation(-1));
      StringAssert.Contains(ex.Message, "GetRankNotation");
      StringAssert.Contains(ex.Message, "NumRanks=8");
    }

    [TestMethod]
    public void GetRankNotation_PastEnd_ThrowsInformative()
    {
      var board = new Board(8, 8);
      var ex = Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetRankNotation(8));
      StringAssert.Contains(ex.Message, "out of range");
    }

    [TestMethod]
    public void GetFileNotation_LargerBoard_ReportsActualSize()
    {
      var board = new Board(10, 10);
      var ex = Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetFileNotation(15));
      StringAssert.Contains(ex.Message, "NumFiles=10");
    }

    [TestMethod]
    public void GetFileNotation_BetweenNumFilesAndMaxFiles_NowThrows()
    {
      // Regression: previously fileNotations is sized to MAX_FILES (16) so indices
      // [NumFiles, 16) silently returned '\0'. The guard now bounds against NumFiles.
      var board = new Board(8, 8);
      Assert.ThrowsException<System.IndexOutOfRangeException>(
        () => board.GetFileNotation(10));
    }
  }
}
