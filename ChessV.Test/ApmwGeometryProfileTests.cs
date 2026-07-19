using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class ApmwGeometryProfileTests
  {
    private static readonly string[] ArmyNames =
    {
      ApmwProfiles.StandardArmy,
      ApmwProfiles.ColourboundClobberers,
      ApmwProfiles.RemarkableRookies,
      ApmwProfiles.NuttyKnights,
    };

    [TestCleanup]
    public void Cleanup()
    {
      ApmwCore._instance = null;
    }

    [TestMethod]
    public void RegisteredVariants_MatchProfilesInContractStageOrder()
    {
      (ApmwChessGame Game, ApmwGeometryProfile Profile, Type Type)[] variants =
      {
        (new ApmwChessGame(), ApmwProfiles.Standard, typeof(ApmwChessGame)),
        (new ApmwGrandChess(), ApmwProfiles.Grand, typeof(ApmwGrandChess)),
        (new ApmwTenByTenChess(), ApmwProfiles.TenByTen, typeof(ApmwTenByTenChess)),
        (new ApmwTwelveByTenChess(), ApmwProfiles.TwelveByTen, typeof(ApmwTwelveByTenChess)),
        (new ApmwTwelveByTwelveChess(), ApmwProfiles.TwelveByTwelve, typeof(ApmwTwelveByTwelveChess)),
      };

      CollectionAssert.AreEqual(
        new[] { "8x8", "10x8", "10x10", "12x10", "12x12" },
        ApmwProfiles.Stages.Select(profile => profile.StageId).ToArray());
      CollectionAssert.AreEqual(
        variants.Select(variant => variant.Profile).ToArray(),
        ApmwProfiles.Stages.ToArray());

      foreach (var variant in variants)
      {
        Assert.AreEqual(variant.Type, variant.Game.GetType());
        Assert.AreSame(variant.Profile, variant.Game.ApmwProfile);
        GameAttribute attribute = RegistrationFor(variant.Type);
        variant.Profile.ValidateMetadata(attribute);
        Assert.AreEqual(variant.Profile.GameName, attribute.GameName);
        Assert.AreEqual(typeof(Geometry.Rectangular), attribute.GeometryType);
        CollectionAssert.AreEqual(
          attribute.GeometryParameters,
          variant.Profile.MetadataGeometryParameters.ToArray());
        Assert.AreEqual(3, variant.Profile.CardSlotsPerPlayer);
      }
    }

    [TestMethod]
    public void GeometryProfiles_ExposeImmutableMetadata()
    {
      Assert.ThrowsException<NotSupportedException>(
        () => ((IList<ApmwGeometryProfile>)ApmwProfiles.Stages)
          .Add(ApmwProfiles.Standard));

      foreach (ApmwGeometryProfile profile in ApmwProfiles.Stages)
      {
        Assert.IsFalse(
          typeof(ApmwGeometryProfile).GetProperty("GameName").SetMethod.IsPublic);
        Assert.IsFalse(
          typeof(ApmwGeometryProfile).GetProperty("Files").SetMethod.IsPublic);
        Assert.IsFalse(
          typeof(ApmwGeometryProfile).GetProperty("Ranks").SetMethod.IsPublic);
        Assert.ThrowsException<NotSupportedException>(
          () => ((IList<int>)profile.MetadataGeometryParameters)[0] = 99);
        Assert.ThrowsException<NotSupportedException>(
          () => ((IDictionary<string, ApmwCpuArmyProfile>)profile.CpuArmies)
            .Add("mutable", profile.ResolveCpuArmy(ApmwProfiles.StandardArmy)));
      }
    }

    [TestMethod]
    public void GeometryProfile_RejectsMismatchedMetadataBeforeAllocation()
    {
      var wrongGeometry = new GameAttribute(
        ApmwProfiles.StandardGameName,
        typeof(Geometry.Rectangular),
        10,
        8,
        3);
      var wrongGeometryType = new GameAttribute(
        ApmwProfiles.StandardGameName,
        typeof(object),
        8,
        8);

      Assert.ThrowsException<InvalidOperationException>(
        () => ApmwProfiles.Standard.ValidateMetadata(wrongGeometry));
      Assert.ThrowsException<InvalidOperationException>(
        () => ApmwProfiles.Standard.ValidateMetadata(wrongGeometryType));
    }

    [TestMethod]
    public void CpuArmyProfiles_PinAllBackRanksAndPromotionFragments()
    {
      var expected = new Dictionary<string, string[]>
      {
        ["8x8"] = new[]
        {
          "|rnbqkbnr|rnbq|",
          ApmwProfiles.ColourboundClobberers + "|gxeakexg|gxea|",
          ApmwProfiles.RemarkableRookies + "|stickits|stic|",
          ApmwProfiles.NuttyKnights + "|hlmykmlh|hlmy|",
        },
        ["10x8"] = new[]
        {
          "|rnabqkbcnr|rnbq|ac",
          ApmwProfiles.ColourboundClobberers + "|gxqeakecxg|gxea|qc",
          ApmwProfiles.RemarkableRookies + "|staickiqts|stic|aq",
          ApmwProfiles.NuttyKnights + "|hlamykmclh|hlmy|ac",
        },
        ["10x10"] = new[]
        {
          "|rnabqkbcnr|rnbq|ac",
          ApmwProfiles.ColourboundClobberers + "|gxqeakecxg|gxea|qc",
          ApmwProfiles.RemarkableRookies + "|staickiqts|stic|aq",
          ApmwProfiles.NuttyKnights + "|hlamykmclh|hlmy|ac",
        },
        ["12x10"] = new[]
        {
          "|rjnabqkbcnjr|rnbq|acj",
          ApmwProfiles.ColourboundClobberers + "|gjxqeakecxjg|gxea|qcj",
          ApmwProfiles.RemarkableRookies + "|sjtaickiqtjs|stic|aqj",
          ApmwProfiles.NuttyKnights + "|hjlamykmcljh|hlmy|acj",
        },
        ["12x12"] = new[]
        {
          "|rjnabqkbcnjr|rnbq|acj",
          ApmwProfiles.ColourboundClobberers + "|gjxqeakecxjg|gxea|qcj",
          ApmwProfiles.RemarkableRookies + "|sjtaickiqtjs|stic|aqj",
          ApmwProfiles.NuttyKnights + "|hjlamykmcljh|hlmy|acj",
        },
      };

      foreach (ApmwGeometryProfile profile in ApmwProfiles.Stages)
      {
        foreach (string record in expected[profile.StageId])
        {
          string[] fields = record.Split('|');
          ApmwCpuArmyProfile army = profile.ResolveCpuArmy(fields[0]);
          Assert.AreEqual(fields[1], army.BackRank, profile.StageId + " " + fields[0]);
          Assert.AreEqual(fields[1].ToUpperInvariant(), army.BackRankForPlayer(0));
          Assert.AreEqual(fields[1], army.BackRankForPlayer(1));
          Assert.AreEqual(fields[2], army.PromotionPieces);
          Assert.AreEqual(fields[3], army.AttendantPromotions);
          Assert.AreEqual(profile.Files, army.BackRank.Length);
          Assert.AreEqual(
            fields[0] == ApmwProfiles.ColourboundClobberers,
            army.CornerCastlersAreColorbound);

          if (profile.Files == 12)
          {
            Assert.AreEqual(
              1,
              army.AttendantPromotions.Count(notation => notation == 'j'),
              profile.StageId + " " + fields[0] + " must promote to Nightrider exactly once.");
          }
        }
      }
    }

    [TestMethod]
    public void EveryProfile_PlayerColorAndCpuArmy_HasValidFenAndPromotions()
    {
      foreach (ApmwGeometryProfile profile in ApmwProfiles.Stages)
      {
        foreach (int humanPlayer in new[] { 0, 1 })
        {
          foreach (string armyName in ArmyNames)
          {
            ApmwChessGame game = CreateGame(profile, humanPlayer, armyName);
            ApmwCpuArmyProfile army = profile.ResolveCpuArmy(armyName);
            string context = profile.StageId + " player " + humanPlayer + " army " + armyName;
            string[] rows = FenRows(game);

            Assert.AreEqual(profile.Files, game.Board.NumFiles, context);
            Assert.AreEqual(profile.Ranks, game.Board.NumRanks, context);
            Assert.AreEqual(profile.Files * profile.Ranks, game.Board.NumSquares, context);
            Assert.AreEqual(profile.Ranks, rows.Length, context);
            foreach (string row in rows)
              Assert.AreEqual(profile.Files, ExpandedRowWidth(row), context + " row " + row);
            CollectionAssert.AreEqual(ExpectedSetupRows(profile, humanPlayer, army), rows, context);
            Assert.IsFalse(game.FENStart.Contains("#{"), context);

            string expectedPromotionTypes =
              "QRNB" +
              (humanPlayer == 0 ? "QRNB" : "qrnb") +
              (armyName == ApmwProfiles.StandardArmy ? "" : army.PromotionPieces) +
              army.AttendantPromotions;
            Assert.AreEqual(expectedPromotionTypes, game.PromotionTypes, context);
            Assert.AreEqual(
              army.AttendantPromotions.Length,
              game.ParseTypeListFromString(army.AttendantPromotions).Count,
              context + " attendant promotion catalog");
          }
        }
      }
    }

    [TestMethod]
    public void EightByEightAndTenByEight_RetainExactLegacySetupAndCastling()
    {
      Assert.AreEqual(
        "#{BlackPieces}/#{BlackPawns}/#{BlackOuter}/#{BlackFourth}/" +
        "#{WhiteFourth}/#{WhiteOuter}/#{WhitePawns}/#{WhitePieces}",
        ApmwProfiles.Standard.FenArrayTemplate);
      Assert.AreEqual(ApmwProfiles.Standard.FenArrayTemplate, ApmwProfiles.Grand.FenArrayTemplate);

      AssertLegacyVariant(
        ApmwProfiles.Standard,
        0,
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/R3K2R",
        "kqAH",
        "AHkq",
        "0:e1-c1:a1-d1:A|0:e1-g1:h1-f1:H|1:e8-g8:h8-f8:k|1:e8-c8:a8-d8:q");
      AssertLegacyVariant(
        ApmwProfiles.Standard,
        1,
        "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR",
        "KQah",
        "ahKQ",
        "0:e1-g1:h1-f1:K|0:e1-c1:a1-d1:Q|1:e8-c8:a8-d8:a|1:e8-g8:h8-f8:h");
      AssertLegacyVariant(
        ApmwProfiles.Grand,
        0,
        "rnabqkbcnr/pppppppppp/10/10/10/10/PPPPPPPPPP/R4K3R",
        "kqAJ",
        "AJkq",
        "0:f1-d1:a1-e1:A|0:f1-h1:j1-g1:J|1:f8-h8:j8-g8:k|1:f8-d8:a8-e8:q");
      AssertLegacyVariant(
        ApmwProfiles.Grand,
        1,
        "r4k3r/pppppppppp/10/10/10/10/PPPPPPPPPP/RNABQKBCNR",
        "KQaj",
        "ajKQ",
        "0:f1-h1:j1-g1:K|0:f1-d1:a1-e1:Q|1:f8-d8:a8-e8:a|1:f8-h8:j8-g8:j");
    }

    [TestMethod]
    public void EveryProfile_PlayerColorAndCpuArmy_RegistersGeometryDrivenCastling()
    {
      foreach (ApmwGeometryProfile profile in ApmwProfiles.Stages)
      {
        foreach (int humanPlayer in new[] { 0, 1 })
        {
          foreach (string armyName in ArmyNames)
          {
            ApmwChessGame game = CreateGame(profile, humanPlayer, armyName);
            ApmwCpuArmyProfile army = profile.ResolveCpuArmy(armyName);
            string context = profile.StageId + " player " + humanPlayer + " army " + armyName;
            List<CastlingRegistration> expected = ExpectedCastlingRegistrations(
              profile,
              humanPlayer,
              army);
            List<CastlingRegistration> actual = CastlingRegistrations(game);

            Assert.AreEqual(expected.Count, actual.Count, context);
            for (int index = 0; index < expected.Count; index++)
              AssertCastlingRegistration(expected[index], actual[index], context + " #" + index);

            string humanRights = humanPlayer == 0
              ? "A" + (char)('A' + profile.Files - 1)
              : "a" + (char)('a' + profile.Files - 1);
            string cpuRights = humanPlayer == 0 ? "kq" : "KQ";
            Assert.AreEqual(cpuRights + humanRights, game.GetCustomProperty("CastleRooks"), context);
            Assert.AreEqual(cpuRights + humanRights, game.FENStart.Split(' ')[3], context);
            Assert.AreEqual(humanRights + cpuRights, game.FEN["castling"], context);

            ApmwCastlingPlan cpuQueenSide = profile.CreateCastlingPlan(
              profile.QueenSideCornerFile,
              army.CornerCastlersAreColorbound);
            ApmwCastlingPlan cpuKingSide = profile.CreateCastlingPlan(
              profile.KingSideCornerFile,
              army.CornerCastlersAreColorbound);
            if (army.CornerCastlersAreColorbound)
            {
              Assert.AreEqual(
                0,
                Math.Abs(cpuQueenSide.CastlerFromFile - cpuQueenSide.CastlerToFile) % 2,
                context + " queen-side color parity");
              Assert.AreEqual(
                0,
                Math.Abs(cpuKingSide.CastlerFromFile - cpuKingSide.CastlerToFile) % 2,
                context + " king-side color parity");
            }
          }
        }
      }
    }

    [TestMethod]
    public void TwelveFileColourboundBand_PreservesClericSquareColor()
    {
      foreach (ApmwGeometryProfile profile in new[]
      {
        ApmwProfiles.TwelveByTen,
        ApmwProfiles.TwelveByTwelve,
      })
      {
        ApmwCpuArmyProfile army = profile.ResolveCpuArmy(ApmwProfiles.ColourboundClobberers);
        Assert.AreEqual("gjxqeakecxjg", army.BackRank);

        ApmwCastlingPlan queenSide = profile.CreateCastlingPlan(0, true);
        Assert.AreEqual(6, queenSide.KingFromFile);
        Assert.AreEqual(4, queenSide.KingToFile);
        Assert.AreEqual(0, queenSide.CastlerFromFile);
        Assert.AreEqual(6, queenSide.CastlerToFile);

        ApmwCastlingPlan kingSide = profile.CreateCastlingPlan(11, true);
        Assert.AreEqual(6, kingSide.KingFromFile);
        Assert.AreEqual(8, kingSide.KingToFile);
        Assert.AreEqual(11, kingSide.CastlerFromFile);
        Assert.AreEqual(7, kingSide.CastlerToFile);
      }
    }

    [TestMethod]
    public void ExpandedVariants_InitializeAndGenerateInitialMoves()
    {
      foreach (ApmwGeometryProfile profile in new[]
      {
        ApmwProfiles.TenByTen,
        ApmwProfiles.TwelveByTen,
        ApmwProfiles.TwelveByTwelve,
      })
      {
        foreach (int humanPlayer in new[] { 0, 1 })
        {
          ApmwChessGame game = CreateGame(profile, humanPlayer, ApmwProfiles.StandardArmy);
          MoveList root = game.RootMoveListForTest;
          root.Reset();
          game.GenerateMovesForTest(game.CurrentSide);
          Assert.IsTrue(
            root.MoveCursor > 0,
            profile.StageId + " player " + humanPlayer + " generated no initial moves.");
          root.Reset();
          Assert.AreEqual(0, root.MoveCursor);
          Assert.AreEqual(0, root.PickupCursorForTest);
          Assert.AreEqual(0, root.DropCursorForTest);
        }
      }
    }

    [TestMethod]
    public void PreBoardGeometrySeam_UpdatesBothDimensionsBeforeAllocation()
    {
      var unchanged = new GeometryProbeGame(false);
      AssertProbeGeometry(unchanged, 8, 8);

      var reconfigured = new GeometryProbeGame(true);
      AssertProbeGeometry(reconfigured, 10, 6);
    }

    private static ApmwChessGame CreateGame(
      ApmwGeometryProfile profile,
      int humanPlayer,
      string enemyArmy)
    {
      ApmwCore._instance = new ApmwCore();
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
      core.foundPlayAsWhite = humanPlayer == 0 ? 1 : 0;
      core.isGrand = profile.Files > 8;
      core.foundArmy = null;
      core.GeriProvider = () => humanPlayer;
      core.EngineWeakeningProvider = () => 0;
      core.PlayerPocketPiecesProvider = () => new List<PieceType>();
      core.PlayerPieceSetProvider = files => BuildSimplePlayerSetup(
        core,
        profile,
        humanPlayer,
        files);

      Dictionary<string, string> definitions =
        enemyArmy == ApmwProfiles.StandardArmy
          ? null
          : new Dictionary<string, string> { ["enemy_army"] = enemyArmy };
      Game game = new ChessV.Manager.Manager().CreateGame(profile.GameName, definitions);
      Assert.IsNotNull(game, profile.GameName);
      Assert.AreEqual(ExpectedGameType(profile), game.GetType(), profile.GameName);
      return (ApmwChessGame)game;
    }

    private static (Dictionary<KeyValuePair<int, int>, PieceType>, string) BuildSimplePlayerSetup(
      ApmwCore core,
      ApmwGeometryProfile profile,
      int humanPlayer,
      int files)
    {
      Assert.AreEqual(profile.Files, files);
      PieceType king = core.kings[0];
      PieceType rook = core.majors.Single(piece => piece.Name == "Rook");
      PieceType pawn = core.pawns.Single(piece => piece.Name == "Pawn");
      var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int file = 0; file < profile.Files; file++)
        pieces[new KeyValuePair<int, int>(profile.PlayerPawnRank, file)] = pawn;
      pieces[new KeyValuePair<int, int>(profile.PlayerBackRank, 0)] = rook;
      pieces[new KeyValuePair<int, int>(profile.PlayerBackRank, profile.KingFile)] = king;
      pieces[new KeyValuePair<int, int>(profile.PlayerBackRank, profile.Files - 1)] = rook;
      return (pieces, humanPlayer == 0 ? "QRNB" : "qrnb");
    }

    private static Type ExpectedGameType(ApmwGeometryProfile profile)
    {
      if (profile == ApmwProfiles.Standard)
        return typeof(ApmwChessGame);
      if (profile == ApmwProfiles.Grand)
        return typeof(ApmwGrandChess);
      if (profile == ApmwProfiles.TenByTen)
        return typeof(ApmwTenByTenChess);
      if (profile == ApmwProfiles.TwelveByTen)
        return typeof(ApmwTwelveByTenChess);
      return typeof(ApmwTwelveByTwelveChess);
    }

    private static GameAttribute RegistrationFor(Type gameType)
    {
      return (GameAttribute)gameType
        .GetCustomAttributes(typeof(GameAttribute), false)
        .Single();
    }

    private static string[] ExpectedSetupRows(
      ApmwGeometryProfile profile,
      int humanPlayer,
      ApmwCpuArmyProfile army)
    {
      string[] humanRows = Enumerable.Repeat(
        profile.EmptyRow,
        profile.HumanFormationRanks).ToArray();
      humanRows[profile.PlayerPawnRank] =
        new string(humanPlayer == 0 ? 'P' : 'p', profile.Files);
      string humanBackRank =
        "r" +
        (profile.KingFile - 1) +
        "k" +
        (profile.Files - profile.KingFile - 2) +
        "r";
      humanRows[profile.PlayerBackRank] =
        humanPlayer == 0 ? humanBackRank.ToUpperInvariant() : humanBackRank;
      return profile.ComposeFenRows(humanPlayer, army, humanRows);
    }

    private static string[] FenRows(ApmwChessGame game)
    {
      return game.FENStart.Split(' ')[0].Split('/');
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

    private static void AssertLegacyVariant(
      ApmwGeometryProfile profile,
      int humanPlayer,
      string expectedArray,
      string expectedRights,
      string expectedSavedRights,
      string expectedCastlingSignature)
    {
      ApmwChessGame game = CreateGame(profile, humanPlayer, ApmwProfiles.StandardArmy);
      Assert.AreEqual(expectedArray, game.FENStart.Split(' ')[0]);
      Assert.AreEqual(expectedRights, game.GetCustomProperty("CastleRooks"));
      Assert.AreEqual(expectedRights, game.FENStart.Split(' ')[3]);
      Assert.AreEqual(expectedSavedRights, game.FEN["castling"]);
      Assert.AreEqual(expectedCastlingSignature, CastlingMoveRegistrationSignature(game));
    }

    private static List<CastlingRegistration> ExpectedCastlingRegistrations(
      ApmwGeometryProfile profile,
      int humanPlayer,
      ApmwCpuArmyProfile cpuArmy)
    {
      var registrations = new List<CastlingRegistration>();
      for (int player = 0; player < 2; player++)
      {
        bool human = player == humanPlayer;
        if (human)
        {
          AddExpectedCastling(
            registrations,
            profile,
            player,
            profile.QueenSideCornerFile,
            false,
            HumanPrivilege(player, profile.QueenSideCornerFile));
          AddExpectedCastling(
            registrations,
            profile,
            player,
            profile.KingSideCornerFile,
            false,
            HumanPrivilege(player, profile.KingSideCornerFile));
        }
        else
        {
          AddExpectedCastling(
            registrations,
            profile,
            player,
            profile.KingSideCornerFile,
            cpuArmy.CornerCastlersAreColorbound,
            player == 0 ? 'K' : 'k');
          AddExpectedCastling(
            registrations,
            profile,
            player,
            profile.QueenSideCornerFile,
            cpuArmy.CornerCastlersAreColorbound,
            player == 0 ? 'Q' : 'q');
        }
      }
      return registrations;
    }

    private static void AddExpectedCastling(
      List<CastlingRegistration> registrations,
      ApmwGeometryProfile profile,
      int player,
      int castlerSourceFile,
      bool colorbound,
      char privilege)
    {
      ApmwCastlingPlan plan = profile.CreateCastlingPlan(castlerSourceFile, colorbound);
      registrations.Add(new CastlingRegistration
      {
        Player = player,
        KingFrom = new Location(profile.HomeRank(player), plan.KingFromFile),
        KingTo = new Location(profile.HomeRank(player), plan.KingToFile),
        CastlerFrom = new Location(profile.HomeRank(player), plan.CastlerFromFile),
        CastlerTo = new Location(profile.HomeRank(player), plan.CastlerToFile),
        Privilege = privilege,
      });
    }

    private static char HumanPrivilege(int player, int sourceFile)
    {
      char privilege = (char)('a' + sourceFile);
      return player == 0 ? char.ToUpperInvariant(privilege) : privilege;
    }

    private static List<CastlingRegistration> CastlingRegistrations(ApmwChessGame game)
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
      FieldInfo castlerFrom = moveType.GetField("OtherFromSquare");
      FieldInfo castlerTo = moveType.GetField("OtherToSquare");
      FieldInfo privilege = moveType.GetField("PrivChar");
      var registrations = new List<CastlingRegistration>();

      for (int player = 0; player < moveCounts.Length; player++)
      {
        for (int index = 0; index < moveCounts[player]; index++)
        {
          object move = moves.GetValue(player, index);
          registrations.Add(new CastlingRegistration
          {
            Player = player,
            KingFrom = game.Board.SquareToLocation((int)kingFrom.GetValue(move)),
            KingTo = game.Board.SquareToLocation((int)kingTo.GetValue(move)),
            CastlerFrom = game.Board.SquareToLocation((int)castlerFrom.GetValue(move)),
            CastlerTo = game.Board.SquareToLocation((int)castlerTo.GetValue(move)),
            Privilege = (char)privilege.GetValue(move),
          });
        }
      }
      return registrations;
    }

    private static void AssertCastlingRegistration(
      CastlingRegistration expected,
      CastlingRegistration actual,
      string context)
    {
      Assert.AreEqual(expected.Player, actual.Player, context + " player");
      Assert.AreEqual(expected.KingFrom, actual.KingFrom, context + " king source");
      Assert.AreEqual(expected.KingTo, actual.KingTo, context + " king destination");
      Assert.AreEqual(expected.CastlerFrom, actual.CastlerFrom, context + " castler source");
      Assert.AreEqual(expected.CastlerTo, actual.CastlerTo, context + " castler destination");
      Assert.AreEqual(expected.Privilege, actual.Privilege, context + " privilege");
    }

    private static string CastlingMoveRegistrationSignature(ApmwChessGame game)
    {
      return string.Join(
        "|",
        CastlingRegistrations(game).Select(move =>
          move.Player + ":" +
          LocationNotation(move.KingFrom) + "-" + LocationNotation(move.KingTo) + ":" +
          LocationNotation(move.CastlerFrom) + "-" + LocationNotation(move.CastlerTo) + ":" +
          move.Privilege));
    }

    private static string LocationNotation(Location location)
    {
      return ((char)('a' + location.File)) + (location.Rank + 1).ToString();
    }

    private static void AssertProbeGeometry(
      GeometryProbeGame game,
      int expectedFiles,
      int expectedRanks)
    {
      var attribute = new GameAttribute(
        "Pre-board geometry probe",
        typeof(Geometry.Rectangular),
        8,
        8);
      Exceptions.GameInitializationException exception =
        Assert.ThrowsException<Exceptions.GameInitializationException>(
          () => game.Initialize(attribute, null, null));

      Assert.IsInstanceOfType(exception.InnerException, typeof(GeometryProbeException));
      Assert.AreEqual(expectedFiles, game.ObservedFiles);
      Assert.AreEqual(expectedRanks, game.ObservedRanks);
      Assert.AreEqual(expectedFiles, game.NumFiles);
      Assert.AreEqual(expectedRanks, game.NumRanks);
    }

    private sealed class CastlingRegistration
    {
      public int Player { get; set; }
      public Location KingFrom { get; set; }
      public Location KingTo { get; set; }
      public Location CastlerFrom { get; set; }
      public Location CastlerTo { get; set; }
      public char Privilege { get; set; }
    }

    private sealed class GeometryProbeGame : Game
    {
      private readonly bool reconfigure;

      public int ObservedFiles { get; private set; }
      public int ObservedRanks { get; private set; }

      public GeometryProbeGame(bool reconfigure)
        : base(2, 8, 8, new MirrorSymmetry())
      {
        this.reconfigure = reconfigure;
      }

      protected override void ConfigureBoardGeometry(ref int nFiles, ref int nRanks)
      {
        if (reconfigure)
        {
          nFiles = 10;
          nRanks = 6;
        }
      }

      public override Board CreateBoard(
        int nPlayers,
        int nFiles,
        int nRanks,
        Symmetry symmetry)
      {
        ObservedFiles = nFiles;
        ObservedRanks = nRanks;
        throw new GeometryProbeException();
      }
    }

    private sealed class GeometryProbeException : Exception
    {
    }
  }
}
