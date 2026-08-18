using Archipelago.APChessV;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class ApmwGeometryAwareProviderIntegrationTests
  {
    [TestMethod]
    public void GeometryPreviewBeforeGameCreationInitializesArmyFilteredPieceCatalog()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.ArmyIndexes = new[] { 1, 6 };
      builder.PawnCount = 4;
      builder.MinorPieceCount = 1;
      builder.JackCount = 2;

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(builder.Build()))
      {
        ApmwCore core = ApmwCore.getInstance();
        Assert.IsNotNull(core.armies);
        Assert.AreEqual(7, core.armies.Count);
        Assert.IsTrue(core.kings.All(piece => piece.Game == null),
          "Preview catalog pieces must not be bound to a Game.");

        ApmwGeometryPreview preview = scope.Handler.GetGeometryPreview(8, 8);
        ActiveRosterProjection projection = scope.Handler.ProjectOwnedRoster(
          ProjectionGeometry.For(8, 8));

        Assert.IsNotNull(preview);
        Assert.AreEqual("8x8", preview.StageId);
        Assert.AreEqual(projection.ActivePieces.Count + 1, preview.ActiveCount);
        Assert.IsNotNull(projection.PrimaryKing);
        Assert.IsTrue(projection.ActivePieces.All(piece => piece.ConcretePieceType != null));
      }
    }

    [TestMethod]
    public void OrderedProgressive6x8_CurrentProjectionLaunchesCompactRegisteredGame()
    {
      ApmwFuzzCase fuzzCase = RichCurrentContractCase(0).With(builder =>
      {
        builder.Goal = Goal.OrderedProgressive6x8;
        builder.IsSuperSized = false;
        builder.SuperSizeMeCount = 0;
      });

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(fuzzCase))
      {
        ApmwGeometryOption compact = ApmwGeometryResolver.ResolveCurrent(
          LoadContract(),
          0,
          0,
          Goal.OrderedProgressive6x8).Single();
        ApmwChessGame game = (ApmwChessGame)new ChessV.Manager.Manager()
          .CreateGame(compact.RegisteredGameName);
        ActiveRosterProjection projection = scope.Handler.ProjectOwnedRoster(
          ProjectionGeometry.For(6, 8));

        Assert.AreEqual(ApmwProfiles.SixByEightGameName, compact.RegisteredGameName);
        Assert.IsInstanceOfType(game, typeof(ApmwSixByEightChess));
        Assert.AreEqual(Goal.OrderedProgressive6x8, ApmwConfig.getInstance().Goal);
        Assert.AreEqual(6, game.Board.NumFiles);
        Assert.AreEqual(8, game.Board.NumRanks);
        Assert.AreEqual(
          projection.ActivePieces.Count + 1,
          Enumerable.Range(0, game.Board.NumSquares)
            .Count(square => game.Board[square] != null &&
              game.Board[square].Player == 0));
        CollectionAssert.AreEqual(
          ExpectedHumanBoardSignature(projection, 0, 8),
          ActualHumanBoardSignature(game, 0));
      }
    }

    [DataTestMethod]
    [DataRow(ApmwProfiles.TenByTenGameName, 10, 10, 0, 70, 11)]
    [DataRow(ApmwProfiles.TenByTenGameName, 10, 10, 1, 70, 11)]
    [DataRow(ApmwProfiles.TwelveByTenGameName, 12, 10, 0, 81, 0)]
    [DataRow(ApmwProfiles.TwelveByTenGameName, 12, 10, 1, 81, 0)]
    [DataRow(ApmwProfiles.TwelveByTwelveGameName, 12, 12, 0, 81, 0)]
    [DataRow(ApmwProfiles.TwelveByTwelveGameName, 12, 12, 1, 81, 0)]
    public void CurrentContract_ExpandedStartupUsesProjectedActiveRoster(
      string gameName,
      int files,
      int ranks,
      int humanPlayer,
      int expectedActiveCount,
      int expectedReserveCount)
    {
      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(
        RichCurrentContractCase(humanPlayer)))
      {
        Assert.IsTrue(ApmwConfig.getInstance().UsesCurrentContract);
        Assert.IsNotNull(ApmwCore.getInstance().GeometryAwarePlayerPieceSetProvider);

        ApmwChessGame game = (ApmwChessGame)new ChessV.Manager.Manager()
          .CreateGame(gameName);
        ActiveRosterProjection projection = scope.Handler.ProjectOwnedRoster(
          ProjectionGeometry.For(files, ranks));
        ApmwGeometryPreview preview = scope.Handler.GetGeometryPreview(files, ranks);
        ActiveRosterProjection withoutForwardness = GeneratedRosterProjector.Project(
          scope.Handler.GenerateOwnedRoster(),
          ProjectionGeometry.For(files, ranks),
          0,
          ApmwConfig.getInstance());

        Assert.AreEqual(files, game.Board.NumFiles);
        Assert.AreEqual(ranks, game.Board.NumRanks);
        Assert.AreEqual(expectedActiveCount, preview.ActiveCount);
        Assert.AreEqual(expectedReserveCount, preview.ReserveCount);
        Assert.AreEqual(projection.ActivePieces.Count + 1, preview.ActiveCount);
        Assert.AreEqual(projection.ReservePieces.Count, preview.ReserveCount);
        Assert.AreEqual(projection.UnspentForwardness, preview.UnspentForwardness);
        Assert.AreEqual(13, ApmwCore.getInstance().foundPawnForwardness);
        if (projection.UnspentForwardness < 13)
        {
          Assert.AreNotEqual(
            PawnPlacementSignature(withoutForwardness),
            PawnPlacementSignature(projection),
            "Applied Pawn Forwardness must change the launched setup.");
        }
        else
        {
          Assert.AreEqual(
            PawnPlacementSignature(withoutForwardness),
            PawnPlacementSignature(projection),
            "Fully blocked Pawn Forwardness must remain unspent.");
        }

        CollectionAssert.AreEqual(
          ExpectedHumanBoardSignature(projection, humanPlayer, ranks),
          ActualHumanBoardSignature(game, humanPlayer));
        Assert.AreEqual(
          expectedActiveCount,
          Enumerable.Range(0, game.Board.NumSquares)
            .Count(square => game.Board[square] != null &&
              game.Board[square].Player == humanPlayer));

        Piece primaryKing = Enumerable.Range(0, game.Board.NumSquares)
          .Select(square => game.Board[square])
          .Single(piece => piece != null &&
            piece.Player == humanPlayer &&
            piece.PieceType == projection.PrimaryKing);
        Assert.AreEqual(
          humanPlayer == 0 ? 0 : ranks - 1,
          primaryKing.Location.Rank,
          "The projected primary King must occupy the human home rank.");
        Assert.AreEqual(projection.Geometry.PrimaryKingFile, primaryKing.Location.File);

        int[] projectedPawnRanks = projection.ActivePieces
          .Where(piece => piece.SourcePlacementRole == SourcePlacementRole.PawnSlot)
          .Select(piece => piece.Placement.Coordinate.RelativeRank)
          .OrderBy(rank => rank)
          .ToArray();
        int[] boardPawnRanks = Enumerable.Range(0, game.Board.NumSquares)
          .Select(square => game.Board[square])
          .Where(piece => piece != null &&
            piece.Player == humanPlayer &&
            (game.Pawns.Contains(piece.PieceType) ||
              game.Sergeants.Contains(piece.PieceType)))
          .Select(piece => humanPlayer == 0
            ? piece.Location.Rank
            : ranks - 1 - piece.Location.Rank)
          .OrderBy(rank => rank)
          .ToArray();
        CollectionAssert.AreEqual(projectedPawnRanks, boardPawnRanks);
        Assert.IsTrue(
          boardPawnRanks.Any(rank => rank > 4),
          "Expanded projection must use formation ranks beyond the legacy five-row envelope.");

        int[] projectedCastlerFiles = projection.CastlingRights
          .Select(right => right.Coordinate.File)
          .OrderBy(file => file)
          .ToArray();
        string allRights = (string)game.GetCustomProperty("CastleRooks");
        int[] registeredHumanCastlerFiles = allRights.Substring(2)
          .Select(privilege => char.ToLowerInvariant(privilege) - 'a')
          .OrderBy(file => file)
          .ToArray();
        CollectionAssert.AreEqual(projectedCastlerFiles, registeredHumanCastlerFiles);

        foreach (string promotion in projection.ActivePromotionCatalog)
          StringAssert.Contains(game.PromotionTypes, promotion);
      }
    }

    [TestMethod]
    public void LegacyContract_LeavesGeometryAwareProviderUnset()
    {
      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(
        ApmwFuzzCase.DefaultSuperSized()))
      {
        Assert.IsFalse(ApmwConfig.getInstance().UsesCurrentContract);
        Assert.IsNull(ApmwCore.getInstance().GeometryAwarePlayerPieceSetProvider);

        ApmwGrandChess game = (ApmwGrandChess)new ChessV.Manager.Manager()
          .CreateGame(ApmwProfiles.GrandGameName);
        Assert.AreEqual(10, game.Board.NumFiles);
        Assert.AreEqual(8, game.Board.NumRanks);
      }
    }

    [TestMethod]
    public void CurrentContract_UnhookRestoresGeometryAwareProvider()
    {
      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(
        RichCurrentContractCase(0)))
      {
        ApmwCore core = ApmwCore.getInstance();
        Assert.IsNotNull(core.GeometryAwarePlayerPieceSetProvider);

        scope.Handler.Unhook();

        Assert.IsNull(core.GeometryAwarePlayerPieceSetProvider);
      }
    }

    private static ApmwFuzzCase RichCurrentContractCase(int humanPlayer)
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultSuperSized().ToBuilder();
      builder.CaseName = "geometry-aware-provider-player-" + humanPlayer;
      builder.PlayerPieceTypes = PieceTypes.Stable;
      builder.PieceLocations = PieceLocations.Stable;
      builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Configure;
      builder.PieceUpgradePreferenceProfile =
        ApmwPieceUpgradePreferenceProfile.TiedGraduationWithProportions;
      builder.PawnCount = 60;
      builder.MinorPieceCount = 15;
      builder.MajorPieceCount = 11;
      builder.JackCount = 9;
      builder.MajorToQueenCount = 0;
      builder.PawnForwardnessCount = 13;
      builder.PlayAsWhiteCount = humanPlayer == 0 ? 1 : 0;
      return builder.Build();
    }

    private static ApmwContractV2 LoadContract()
    {
      return ApmwContractV2Parser.Parse(File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "ProjectionV2",
        "baseline.json")));
    }

    private static string[] ExpectedHumanBoardSignature(
      ActiveRosterProjection projection,
      int humanPlayer,
      int ranks)
    {
      var expected = new List<string>
      {
        PieceSignature(
          projection.PrimaryKingPlacement.Coordinate,
          projection.PrimaryKing,
          humanPlayer,
          ranks),
      };
      expected.AddRange(projection.ActivePieces.Select(piece =>
        PieceSignature(
          piece.Placement.Coordinate,
          piece.ConcretePieceType,
          humanPlayer,
          ranks)));
      return expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
    }

    private static string[] ActualHumanBoardSignature(
      ApmwChessGame game,
      int humanPlayer)
    {
      return Enumerable.Range(0, game.Board.NumSquares)
        .Select(square => game.Board[square])
        .Where(piece => piece != null && piece.Player == humanPlayer)
        .Select(piece =>
          piece.Location.Rank + "," +
          piece.Location.File + ":" +
          piece.PieceType.Name)
        .OrderBy(value => value, StringComparer.Ordinal)
        .ToArray();
    }

    private static string PieceSignature(
      ProjectionCoordinate coordinate,
      PieceType pieceType,
      int humanPlayer,
      int ranks)
    {
      int boardRank = humanPlayer == 0
        ? coordinate.RelativeRank
        : ranks - 1 - coordinate.RelativeRank;
      return boardRank + "," + coordinate.File + ":" + pieceType.Name;
    }

    private static string PawnPlacementSignature(ActiveRosterProjection projection)
    {
      return string.Join(
        "|",
        projection.ActivePieces
          .Where(piece => piece.SourcePlacementRole == SourcePlacementRole.PawnSlot)
          .OrderBy(piece => piece.StableId, StringComparer.Ordinal)
          .Select(piece =>
            piece.StableId + ":" +
            piece.Placement.Coordinate.RelativeRank + "," +
            piece.Placement.Coordinate.File));
    }
  }
}
