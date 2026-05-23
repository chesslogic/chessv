using ChessV;

namespace ChessV.Test
{
  [TestClass]
  public class MoveHashPinningTests
  {
    private static uint MovementHash(int from, int to, int tag, int player, MoveType type)
    {
      return (uint)from
        + ((uint)to << 8)
        + ((uint)tag << 16)
        + ((((uint)player << 7) | (uint)type) << 24);
    }

    private static uint MoveInfoHash(int from, int to, int tag, int player, MoveType type)
    {
      return (uint)from
        + ((uint)to << 8)
        + ((uint)tag << 16)
        + ((uint)type << 24)
        + ((uint)player << 31);
    }

    [TestMethod]
    public void Movement_Hash_RoundTrip_PreservesFields()
    {
      var movement = new Movement(16, 254, 1, MoveType.EnPassant, 33);
      uint hash = movement.Hash;

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
      var movement = new Movement(255, 255, 1, maxSevenBitMoveType, 255);

      Assert.AreEqual(uint.MaxValue, movement.Hash);
      Assert.AreEqual(255, Movement.GetFromSquareFromHash(movement.Hash));
      Assert.AreEqual(255, Movement.GetToSquareFromHash(movement.Hash));
      Assert.AreEqual(255, Movement.GetTagFromHash(movement.Hash));
      Assert.AreEqual(1, Movement.GetPlayerFromHash(movement.Hash));
      Assert.AreEqual(maxSevenBitMoveType, Movement.GetMoveTypeFromHash(movement.Hash));
      Assert.AreEqual(movement.Hash, MovementHash(255, 255, 255, 1, maxSevenBitMoveType));

      var roundTrip = new Movement(movement.Hash);
      Assert.AreEqual(255, roundTrip.FromSquare);
      Assert.AreEqual(255, roundTrip.ToSquare);
      Assert.AreEqual(255, roundTrip.Tag);
      Assert.AreEqual(1, roundTrip.Player);
      Assert.AreEqual(maxSevenBitMoveType, roundTrip.MoveType);
      Assert.AreEqual(movement.Hash, roundTrip.Hash);
    }

    [TestMethod]
    public void Movement_HighPromotionOrTag_WithinByteRange()
    {
      var promotion = new Movement(12, 44, 0, MoveType.MoveWithPromotion, 254);
      var capturePromotion = new Movement(33, 77, 1, MoveType.CaptureWithPromotion, 255);

      var promotionRoundTrip = new Movement(promotion.Hash);
      var capturePromotionRoundTrip = new Movement(capturePromotion.Hash);

      Assert.AreEqual(MoveType.MoveWithPromotion, promotionRoundTrip.MoveType);
      Assert.AreEqual(254, promotionRoundTrip.Tag);
      Assert.AreEqual(promotion.Hash, promotionRoundTrip.Hash);
      Assert.AreEqual(254, Movement.GetTagFromHash(promotion.Hash));

      Assert.AreEqual(MoveType.CaptureWithPromotion, capturePromotionRoundTrip.MoveType);
      Assert.AreEqual(255, capturePromotionRoundTrip.Tag);
      Assert.AreEqual(capturePromotion.Hash, capturePromotionRoundTrip.Hash);
      Assert.AreEqual(255, Movement.GetTagFromHash(capturePromotion.Hash));
    }

    [TestMethod]
    public void MoveInfo_ImplicitConversion_ToMovement()
    {
      var moveInfo = new MoveInfo
      {
        FromSquare = 18,
        ToSquare = 52,
        Player = 1,
        MoveType = MoveType.CaptureWithPromotion,
        PromotionType = 7,
      };

      Movement movement = moveInfo;

      Assert.AreEqual(moveInfo.FromSquare, movement.FromSquare);
      Assert.AreEqual(moveInfo.ToSquare, movement.ToSquare);
      Assert.AreEqual(moveInfo.Player, movement.Player);
      Assert.AreEqual(moveInfo.MoveType, movement.MoveType);
      Assert.AreEqual(moveInfo.PromotionType, movement.Tag);
      Assert.AreEqual(moveInfo.Hash, movement.Hash);
    }

    [TestMethod]
    public void MoveInfo_Hash_Pins_Current32BitLayout()
    {
      var moveInfo = new MoveInfo
      {
        FromSquare = 0xA5,
        ToSquare = 0x5A,
        Player = 1,
        MoveType = MoveType.CaptureWithPromotion,
        Tag = 0xC3,
      };

      uint expectedHash = MoveInfoHash(0xA5, 0x5A, 0xC3, 1, MoveType.CaptureWithPromotion);

      Assert.AreEqual(expectedHash, moveInfo.Hash);
      Assert.AreEqual((uint)0xAAC35AA5, moveInfo.Hash);
      Assert.AreEqual(0xA5, Movement.GetFromSquareFromHash(moveInfo.Hash));
      Assert.AreEqual(0x5A, Movement.GetToSquareFromHash(moveInfo.Hash));
      Assert.AreEqual(0xC3, Movement.GetTagFromHash(moveInfo.Hash));
      Assert.AreEqual(1, Movement.GetPlayerFromHash(moveInfo.Hash));
      Assert.AreEqual(MoveType.CaptureWithPromotion, Movement.GetMoveTypeFromHash(moveInfo.Hash));
    }

    [TestMethod]
    public void TTHashEntry_MoveHash_StoresUInt32Verbatim()
    {
      var entry = new TTHashEntry();
      const uint moveHash = 0xFEDCBA98;

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

      const uint first = 0x01234567;
      const uint second = 0x89ABCDEF;
      const uint third = 0xFEDCBA98;
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
