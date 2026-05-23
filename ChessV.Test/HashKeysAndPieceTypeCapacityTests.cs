using System;
using ChessV;
using ChessV.Games;

namespace ChessV.Test
{
  [Game("High Piece Type Capacity Test Variant",
      typeof(Geometry.Rectangular), 8, 8,
      Template = true)]
  public class HighPieceTypeCapacityTestGame : Chess
  {
    public override void SetGameVariables()
    {
      base.SetGameVariables();
      Castling.Value = "None";
      PromotionRule.Value = "None";
      PromotionTypes = "";
      EnPassant = false;
      PawnDoubleMove = false;
      Array = "8/8/8/8/8/8/8/8";
      FENStart = "8/8/8/8/8/8/8/8 w - - 0 1";
    }

    public override void AddPieceTypes()
    {
      base.AddPieceTypes();
      int suffix = 0;
      while (NPieceTypes < Game.MAX_PIECE_TYPES)
        AddPieceType(new Rook($"Extra Type {suffix}", $"_x{suffix++}", 100, 100, "Rook"));
    }
  }

  [TestClass]
  public class HashKeysAndPieceTypeCapacityTests
  {
    private const int OriginalStaticKeyCount = 8704;
    private const UInt64 DynamicKeySeed = 0x6A09E667F3BCC909UL;
    private const UInt64 SplitMixSilverRatioGamma = 0x9E3779B97F4A7C15UL;
    private const UInt64 SplitMixFirstScrambler = 0xBF58476D1CE4E5B9UL;
    private const UInt64 SplitMixSecondScrambler = 0x94D049BB133111EBUL;

    [TestMethod]
    public void TakeKeys_GrowsDeterministicallyBeyondStaticTable()
    {
      Assert.IsTrue(HashKeys.Keys.Length >= OriginalStaticKeyCount);
      UInt64[] before = SnapshotHashKeys();

      var hashKeys = new HashKeys();
      hashKeys.TakeKeys(before.Length - 256);
      int generatedStart = hashKeys.TakeKeys(3);

      Assert.AreEqual(before.Length, generatedStart);
      Assert.IsTrue(HashKeys.Keys.Length >= before.Length + 3);
      AssertPrefixPreserved(before);
      AssertGeneratedKey(before.Length);
      AssertGeneratedKey(before.Length + 1);
      AssertGeneratedKey(before.Length + 2);
    }

    [TestMethod]
    public void TakeMaterialKeys_GrowsDeterministicallyBeyondStaticTable()
    {
      Assert.IsTrue(HashKeys.Keys.Length >= OriginalStaticKeyCount);
      UInt64[] before = SnapshotHashKeys();

      var hashKeys = new HashKeys();
      hashKeys.TakeMaterialKeys(before.Length - 256);
      int generatedStart = hashKeys.TakeMaterialKeys(2);

      Assert.AreEqual(before.Length, generatedStart);
      Assert.IsTrue(HashKeys.Keys.Length >= before.Length + 2);
      AssertPrefixPreserved(before);
      AssertGeneratedKey(before.Length);
      AssertGeneratedKey(before.Length + 1);
    }

    [TestMethod]
    public void TestGame_InitializesAtRaisedPieceTypeCap()
    {
      Assert.AreEqual(64, Game.MAX_PIECE_TYPES);
      var game = new HighPieceTypeCapacityTestGame();
      object[] attrs = typeof(HighPieceTypeCapacityTestGame).GetCustomAttributes(typeof(GameAttribute), inherit: false);

      game.Initialize((GameAttribute)attrs[0], null, null);

      Assert.AreEqual(Game.MAX_PIECE_TYPES, game.NPieceTypes);
      PieceType[] pieceTypes;
      Assert.AreEqual(Game.MAX_PIECE_TYPES, game.GetPieceTypes(out pieceTypes));
      Assert.IsNotNull(pieceTypes[Game.MAX_PIECE_TYPES - 1]);
      Assert.AreEqual(Game.MAX_PIECE_TYPES - 1, game.GetPieceTypeNumber(pieceTypes[Game.MAX_PIECE_TYPES - 1]));
    }

    private static UInt64[] SnapshotHashKeys()
    {
      var snapshot = new UInt64[HashKeys.Keys.Length];
      Array.Copy(HashKeys.Keys, snapshot, snapshot.Length);
      return snapshot;
    }

    private static void AssertPrefixPreserved(UInt64[] before)
    {
      for (int index = 0; index < before.Length; index++)
        Assert.AreEqual(before[index], HashKeys.Keys[index], $"Hash key {index} changed during growth.");
    }

    private static void AssertGeneratedKey(int index)
    {
      Assert.AreEqual(ExpectedDynamicKey(index), HashKeys.Keys[index]);
      Assert.AreNotEqual(0UL, HashKeys.Keys[index]);
    }

    private static UInt64 ExpectedDynamicKey(int index)
    {
      UInt64 value = unchecked(DynamicKeySeed + ((UInt64)index * SplitMixSilverRatioGamma));
      value = unchecked((value ^ (value >> 30)) * SplitMixFirstScrambler);
      value = unchecked((value ^ (value >> 27)) * SplitMixSecondScrambler);
      value ^= value >> 31;
      return value == 0UL ? DynamicKeySeed : value;
    }
  }
}
