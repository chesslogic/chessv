using ChessV;

namespace ChessV.Test
{
  [TestClass]
  public class MoveHashPinningTests
  {
    private static ulong MovementHash(int from, int to, int tag, int player, MoveType type)
    {
      return ((ulong)from & 0xFFUL)
        | (((ulong)to & 0xFFUL) << 8)
        | (((ulong)tag & 0xFFFFUL) << 16)
        | (((ulong)type & 0x7FUL) << 32)
        | (((ulong)player & 1UL) << 39);
    }

    private static ulong MoveInfoHash(int from, int to, int tag, int player, MoveType type)
    {
      return MovementHash(from, to, tag, player, type);
    }

    [TestMethod]
    public void Movement_Hash_RoundTrip_PreservesFields()
    {
      var movement = new Movement(16, 254, 1, MoveType.EnPassant, 33);
      ulong hash = movement.Hash;

      var roundTrip = new Movement(hash);

      Assert.AreEqual(16, roundTrip.FromSquare);
      Assert.AreEqual(254, roundTrip.ToSquare);
      Assert.AreEqual(1, roundTrip.Player);
      Assert.AreEqual(MoveType.EnPassant, roundTrip.MoveType);
      Assert.AreEqual(33, roundTrip.Tag);
      Assert.AreEqual(hash, roundTrip.Hash);
      Assert.AreEqual(movement, roundTrip);
    }

    [TestMethod]
    public void Movement_MaxFieldValues_PackAndUnpack()
    {
      MoveType maxSevenBitMoveType = (MoveType)127;
      var movement = new Movement(255, 255, 1, maxSevenBitMoveType, 65535);

      Assert.AreEqual(0x000000FFFFFFFFFFUL, movement.Hash);
      Assert.AreEqual(255, Movement.GetFromSquareFromHash(movement.Hash));
      Assert.AreEqual(255, Movement.GetToSquareFromHash(movement.Hash));
      Assert.AreEqual(65535, Movement.GetTagFromHash(movement.Hash));
      Assert.AreEqual(1, Movement.GetPlayerFromHash(movement.Hash));
      Assert.AreEqual(maxSevenBitMoveType, Movement.GetMoveTypeFromHash(movement.Hash));
      Assert.AreEqual(movement.Hash, MovementHash(255, 255, 65535, 1, maxSevenBitMoveType));

      var roundTrip = new Movement(movement.Hash);
      Assert.AreEqual(255, roundTrip.FromSquare);
      Assert.AreEqual(255, roundTrip.ToSquare);
      Assert.AreEqual(65535, roundTrip.Tag);
      Assert.AreEqual(1, roundTrip.Player);
      Assert.AreEqual(maxSevenBitMoveType, roundTrip.MoveType);
      Assert.AreEqual(movement.Hash, roundTrip.Hash);
    }

    [TestMethod]
    public void Movement_HighPromotionOrTag_WithinWordRange()
    {
      var promotion = new Movement(12, 44, 0, MoveType.MoveWithPromotion, 0xFEFE);
      var capturePromotion = new Movement(33, 77, 1, MoveType.CaptureWithPromotion, 0xFFFF);

      var promotionRoundTrip = new Movement(promotion.Hash);
      var capturePromotionRoundTrip = new Movement(capturePromotion.Hash);

      Assert.AreEqual(MoveType.MoveWithPromotion, promotionRoundTrip.MoveType);
      Assert.AreEqual(0xFEFE, promotionRoundTrip.Tag);
      Assert.AreEqual(promotion.Hash, promotionRoundTrip.Hash);
      Assert.AreEqual(0xFEFE, Movement.GetTagFromHash(promotion.Hash));

      Assert.AreEqual(MoveType.CaptureWithPromotion, capturePromotionRoundTrip.MoveType);
      Assert.AreEqual(0xFFFF, capturePromotionRoundTrip.Tag);
      Assert.AreEqual(capturePromotion.Hash, capturePromotionRoundTrip.Hash);
      Assert.AreEqual(0xFFFF, Movement.GetTagFromHash(capturePromotion.Hash));
    }

    [TestMethod]
    public void Movement_LegacyUInt32Constructor_ConvertsOldLayoutToCurrentLayout()
    {
      const uint legacyHash = 0xAA_C3_5A_A5U;

      var movement = new Movement(legacyHash);

      Assert.AreEqual(0xA5, movement.FromSquare);
      Assert.AreEqual(0x5A, movement.ToSquare);
      Assert.AreEqual(0xC3, movement.Tag);
      Assert.AreEqual(1, movement.Player);
      Assert.AreEqual(MoveType.CaptureWithPromotion, movement.MoveType);
      Assert.AreEqual(0x000000AA00C35AA5UL, movement.Hash);
      Assert.AreEqual(Movement.FromLegacyUInt32Hash(legacyHash), movement.Hash);
      Assert.IsTrue(movement == legacyHash);
    }

    [TestMethod]
    public void MoveInfo_ImplicitConversion_ToMovement()
    {
      int highPromotionType = Game.MAX_PIECE_TYPES - 1;
      Assert.IsTrue(highPromotionType > 24);
      var moveInfo = new MoveInfo
      {
        FromSquare = 18,
        ToSquare = 52,
        Player = 1,
        MoveType = MoveType.CaptureWithPromotion,
        PromotionType = highPromotionType,
      };

      Movement movement = moveInfo;

      Assert.AreEqual(moveInfo.FromSquare, movement.FromSquare);
      Assert.AreEqual(moveInfo.ToSquare, movement.ToSquare);
      Assert.AreEqual(moveInfo.Player, movement.Player);
      Assert.AreEqual(moveInfo.MoveType, movement.MoveType);
      Assert.AreEqual(moveInfo.PromotionType, movement.Tag);
      Assert.AreEqual(moveInfo.Hash, movement.Hash);
      Assert.AreEqual(highPromotionType, Movement.GetTagFromHash(moveInfo.Hash));
    }

    [TestMethod]
    public void MoveInfo_Hash_Pins_Current64BitLayout()
    {
      var moveInfo = new MoveInfo
      {
        FromSquare = 0xA5,
        ToSquare = 0x5A,
        Player = 1,
        MoveType = MoveType.CaptureWithPromotion,
        Tag = 0xC3,
      };

      ulong expectedHash = MoveInfoHash(0xA5, 0x5A, 0xC3, 1, MoveType.CaptureWithPromotion);

      Assert.AreEqual(expectedHash, moveInfo.Hash);
      Assert.AreEqual(0x000000AA00C35AA5UL, moveInfo.Hash);
      Assert.AreEqual(0xA5, Movement.GetFromSquareFromHash(moveInfo.Hash));
      Assert.AreEqual(0x5A, Movement.GetToSquareFromHash(moveInfo.Hash));
      Assert.AreEqual(0xC3, Movement.GetTagFromHash(moveInfo.Hash));
      Assert.AreEqual(1, Movement.GetPlayerFromHash(moveInfo.Hash));
      Assert.AreEqual(MoveType.CaptureWithPromotion, Movement.GetMoveTypeFromHash(moveInfo.Hash));
    }

    [TestMethod]
    public void TTHashEntry_MoveHash_StoresUInt64Verbatim()
    {
      var entry = new TTHashEntry();
      const ulong moveHash = 0x000000AAFFFF5AA5UL;

      entry.SetData(0x123456789ABCDEF0UL, moveHash, TTHashEntry.HashType.Exact, 42, -123, 17);

      Assert.AreEqual(moveHash, entry.MoveHash);
    }

    [TestMethod]
    public void PV_CopyAndStorage_RetainsMoveHashes()
    {
      var source = new PV();
      var target = new PV();
      source.Initialize();
      target.Initialize();

      const ulong first = 0x0000000101234567UL;
      const ulong second = 0x000000AA89ABCDEFUL;
      const ulong third = 0x000000FFFFFFFFFFUL;
      source[0] = first;
      source[1] = second;
      source[Game.MAX_PLY - 1] = third;

      source.CopyTo(target);

      Assert.AreEqual(first, source.MoveHashes[0]);
      Assert.AreEqual(second, source.MoveHashes[1]);
      Assert.AreEqual(third, source.MoveHashes[Game.MAX_PLY - 1]);
      Assert.AreEqual(first, target[0]);
      Assert.AreEqual(second, target[1]);
      Assert.AreEqual(third, target[Game.MAX_PLY - 1]);
    }
  }
}
