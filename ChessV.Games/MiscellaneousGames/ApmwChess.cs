
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG

This file is part of ChessV.  ChessV is free software; you can redistribute
it and/or modify it under the terms of the GNU General Public License as 
published by the Free Software Foundation, either version 3 of the License, 
or (at your option) any later version.

ChessV is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or 
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
more details; the file 'COPYING' contains the License text, but if for
some reason you need a copy, please visit <http://www.gnu.org/licenses/>.

****************************************************************************/

using ChessV.Base;
using ChessV.Evaluations;
using ChessV.Games.Pieces.Apmw;
using ChessV.Games.Pieces.Berolina;
using ChessV.Games.Rules.Apmw;
using ChessV.Games.Rules.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessV.Games
{
  //**********************************************************************
  //
  //                        Archipelago ChecksMate
  //
  //    Play against an AI with your friends! Get new pieces! Be confused!
  //
  //**********************************************************************

  [Game("Archipelago Multiworld", typeof(Geometry.Rectangular), 8, 8, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwChessGame : Chess, IMultipawnGame
  {
    // *** PIECE TYPES *** //

    //  Berolina
    public PieceType BerolinaPawn;

    //	Colorbound Clobberers
    public PieceType Archbishop;
    public PieceType WarElephant;
    public PieceType Phoenix;
    public PieceType Cleric;

    //	Remarkable Rookies
    public PieceType ShortRook;
    public PieceType Tower;
    public PieceType Lion;
    public PieceType Chancellor;

    //	Nutty Knights
    public PieceType ChargingRook;
    public PieceType NarrowKnight;
    public PieceType ChargingKnight;
    public PieceType Colonel;

    //  Eurasian
    public PieceType Cannon;
    public PieceType Vao;
    public Queennon Queennon;

    //  Misc
    public Herald Herald;
    public Nightrider Nightrider;
    public Scout Scout;

    //  Fairy Kings
    public PieceType MountedKing;
    public PieceType HyperKing;

    public List<PieceType> Kings;
    public HashSet<PieceType> Pawns { get; set; }
    public HashSet<PieceType> Minors;
    public HashSet<PieceType> Majors;
    public HashSet<PieceType> Queens;
    public HashSet<PieceType> Colorbounds;
    public List<HashSet<PieceType>> Armies;
    public List<HashSet<PieceType>> PocketSets;


    protected Dictionary<KeyValuePair<int, int>, PieceType> startingPosition;
    protected string promotions;
    public int HumanPlayer;

    public ApmwChessGame()
    {
      Kings = new List<PieceType>();
      Pawns = new HashSet<PieceType>();
      Minors = new HashSet<PieceType>();
      Majors = new HashSet<PieceType>();
      Queens = new HashSet<PieceType>();
      Colorbounds = new HashSet<PieceType>();
      Armies = new List<HashSet<PieceType>>();
      PocketSets = new List<HashSet<PieceType>>() { Pawns, Minors, Majors, Queens };

      ApmwCore apmwCore = ApmwCore.getInstance();
      apmwCore.kings = Kings;
      apmwCore.pawns = Pawns;
      apmwCore.minors = Minors;
      apmwCore.majors = Majors;
      apmwCore.queens = Queens;
      apmwCore.colorbound = Colorbounds;
      apmwCore.armies = Armies;
      apmwCore.pocketSets = PocketSets;
    }

    // *** INITIALIZATION *** //

    #region CreateBoard
    //	We override the CreateBoard function so the game uses a board of 
    //	type BoardWithCards instead of Board.  This is enough to trigger the 
    //	board with cards architecture and proper rendering to the display.
    public override Board CreateBoard(int nPlayers, int nFiles, int nRanks, Symmetry symmetry)
    { return new Boards.BoardWithCards(nFiles, nRanks, 3); }
    #endregion

    #region AddRules
    public override void AddRules()
    {
      base.AddRules();
      AddRule(new CardDropRule(ApmwCore.getInstance().foundPocketRange));
      var kingPromotions = ApmwCore.getInstance().foundKingPromotions;
      if (kingPromotions > 0)
      {
        ReplaceRule(FindRule(typeof(Rules.CheckmateRule), true), new Rules.Extinction.CovenantRule("KW"));
        AddRule(new ApmwStalemateRule(new PieceType[] { King, Kings[kingPromotions] }));
      }
      else
      {
        ReplaceRule(FindRule(typeof(Rules.CheckmateRule), true), new Rules.Extinction.ExtinctionRule("K"));
        AddRule(new ApmwStalemateRule(new PieceType[] { King }));
      }
      //AddRule(new ApmwMoveCompletionRule());
      // TODO(chesslogic): conditionally remove en passant for one player only. (the Apmw provider probably knows whether the player can en passant at this point)

      // *** BEROLINA PAWN PROMOTION *** //
      List<PieceType> availablePromotionTypes = ParseTypeListFromString(PromotionTypes);
      AddBasicPromotionRule(BerolinaPawn, availablePromotionTypes, (loc) => loc.Rank == Board.NumRanks - 1);

      // *** BEROLINA PAWN DOUBLE MOVE *** //
      if (PawnDoubleMove && BerolinaPawn.Enabled)
      {
        MoveCapability doubleMoveNE = new MoveCapability();
        doubleMoveNE.MinSteps = 2;
        doubleMoveNE.MaxSteps = 2;
        doubleMoveNE.MustCapture = false;
        doubleMoveNE.CanCapture = false;
        doubleMoveNE.Direction = new Direction(1, 1);
        doubleMoveNE.Condition = location => location.Rank <= 1;
        BerolinaPawn.AddMoveCapability(doubleMoveNE);

        MoveCapability doubleMoveNW = new MoveCapability();
        doubleMoveNW.MinSteps = 2;
        doubleMoveNW.MaxSteps = 2;
        doubleMoveNW.MustCapture = false;
        doubleMoveNW.CanCapture = false;
        doubleMoveNW.Direction = new Direction(1, -1);
        doubleMoveNW.Condition = location => location.Rank <= 1;
        BerolinaPawn.AddMoveCapability(doubleMoveNW);
      }

      // *** BEROLINA PAWN EN PASSANT *** //

      // We want all kinds of pawns to be able to en passant. Add the MixedEnPassantRule, which gives the pawns
      // additional capture moves corresponding to the others' type.
      Rule enPassantRule = FindRule(typeof(Rules.EnPassantRule));
      if (enPassantRule != null)
        ReplaceRule(enPassantRule, new MixedEnPassantRule((Rules.EnPassantRule)enPassantRule));

      // *** 480 CASTLING NO SCOPE ***
      AddCastlingRule();
      Dictionary<string, string> majorsFromAndTo = new Dictionary<string, string>();
      int humanPlayer = ApmwCore.getInstance().GeriProvider();
      int rank = humanPlayer * 7;
      // TODO(chesslogic): the starting position dict chesslogic made uses rank=4 for back line. why? u ever heard front to back?
      int positionRank = 4;
      Location kingFrom = new Location(rank, 4);
      for (int i = 0; i < NumFiles; i++)
      {
        var rookFromPair = new KeyValuePair<int, int>(positionRank, i);
        if (!startingPosition.ContainsKey(rookFromPair))
          continue;
        PieceType rookPiece = startingPosition[rookFromPair];
        if (Majors.Contains(rookPiece))
        {
          var kingMoveAmt = i < (NumFiles / 2) ? -2 : 2;
          Location kingTo = new Location(rank, kingFrom.File + kingMoveAmt);
          Location rookFrom = new Location(rank, i);
          Location rookTo = new Location(rank, kingFrom.File + (kingMoveAmt / 2));
          if (Colorbounds.Contains(rookPiece))
          {
            int parity = rookFrom.File % 2;
            if (parity != rookTo.File % 2)
              rookTo = new Location(rookTo.Rank, kingFrom.File);
          }
          if (kingTo == rookTo)
          {
            throw new InvalidOperationException(
              string.Format("chesslogic fucked up some castling math: {0}, {1}, {2}, {3}, {4}",
                  humanPlayer, kingFrom, kingTo, rookFrom, rookTo));
          }
          char privChar = (char)('a' + i);
          if (humanPlayer == 0)
            privChar = Char.ToUpper(privChar);
          castlingMove(humanPlayer,
            Board.LocationToSquare(kingFrom),
            Board.LocationToSquare(kingTo),
            Board.LocationToSquare(rookFrom),
            Board.LocationToSquare(rookTo),
            privChar);
        }
      }
      // TODO(chesslogic): support CPU different armies
      // computer is black
      if (humanPlayer == 0)
      {
        CastlingMove(1, "e8", "g8", "h8", "f8", 'k');
        CastlingMove(1, "e8", "c8", "a8", "d8", 'q');
      }
      // computer is white
      else
      {
        CastlingMove(0, "e1", "g1", "h1", "f1", 'K');
        CastlingMove(0, "e1", "c1", "a1", "d1", 'Q');
      }
    }
    #endregion

    #region AddEvaluations
    public override void AddEvaluations()
    {
      // Do NOT add pawn structure evaluation - berolina pawns are confusing and line 444 crashes the game
      // AddEvaluation(new PawnStructureEvaluation());

      // Do NOT add development evaluation - the AI persists unknown misleading information between undos
      // AddEvaluation(new DevelopmentEvaluation());

      // Do NOT add low material evaluation - instant draw does not account for fairy pieces
      // AddEvaluation(new LowMaterialEvaluation());

      //	Check for colorbound pieces
      bool colorboundPieces = false;
      for (int x = 0; x < nPieceTypes; x++)
        if (pieceTypes[x].NumSlices == 2)
          colorboundPieces = true;
      if (colorboundPieces)
        AddEvaluation(new ColorbindingEvaluation());
      
      // Prepare for other specific types
      string loadableTypes = "KQRBNP" + promotions;
      if (HumanPlayer == 0)
        loadableTypes = loadableTypes.ToUpper();
      else
        loadableTypes = loadableTypes.ToLower();

      // Update strategies of rook types
      if (RookTypeEval == null)
      {
        RookTypeEval = new RookTypeEvaluation();
        RookTypeEval.AddOpenFileBonus(Rook);
        RookTypeEval.AddRookOn7thBonus(Rook, King);
        RookTypeEval.AddRookOn7thBonus(Queen, King, 2, 8);
        AddEvaluation(RookTypeEval);
      }
      if (loadableTypes.Contains(ChargingRook.Notation[HumanPlayer]))
        RookTypeEval.AddRookOn7thBonus(ChargingRook, King);
      if (loadableTypes.Contains(ShortRook.Notation[HumanPlayer]))
      {
        RookTypeEval.AddOpenFileBonus(ShortRook);
        RookTypeEval.AddRookOn7thBonus(ShortRook, King);
      }
      if (loadableTypes.Contains(Colonel.Notation[HumanPlayer]))
        RookTypeEval.AddRookOn7thBonus(Colonel, King, 2, 8);
      if (loadableTypes.Contains(Chancellor.Notation[HumanPlayer]))
        RookTypeEval.AddRookOn7thBonus(Chancellor, King, 2, 8);

      // Update strategies of leapers
      if (OutpostEval == null)
      {
        OutpostEval = new OutpostEvaluation();
        OutpostEval.AddOutpostBonus(Knight);
        OutpostEval.AddOutpostBonus(Bishop, 10, 2, 5, 5);
        AddEvaluation(OutpostEval);
      }
      if (loadableTypes.Contains(ChargingKnight.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(ChargingKnight);
      if (loadableTypes.Contains(NarrowKnight.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(NarrowKnight);
      if (loadableTypes.Contains(Phoenix.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Phoenix);
      if (loadableTypes.Contains(MountedKing.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(MountedKing);
      if (loadableTypes.Contains(Nightrider.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Nightrider);
      if (loadableTypes.Contains(WarElephant.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(WarElephant, 10, 2, 5, 5);
      if (loadableTypes.Contains(Cleric.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Cleric, 10, 2, 5, 5);
      if (loadableTypes.Contains(Lion.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Lion, 10, 2, 5, 5);
      if (loadableTypes.Contains(Bishop.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Bishop, 10, 2, 5, 5);
      if (loadableTypes.Contains(Tower.Notation[HumanPlayer]))
        OutpostEval.AddOutpostBonus(Tower, 10, 2, 5, 5);
    }
    #endregion

    // *** INITIALIZATION *** //

    #region SetGameVariables
    public override void SetGameVariables()
    {
      base.SetGameVariables();
      FENFormat = "{array} {current player} {pieces in hand} {castling} {en-passant} {half-move clock} {turn number}";
      FENStart = "#{Array} w #{PocketPieces} #{CastleRooks} - 0 1";
      Array = "#{BlackPieces}/#{BlackPawns}/#{BlackOuter}/#{BlackFourth}/#{WhiteFourth}/#{WhiteOuter}/#{WhitePawns}/#{WhitePieces}";
      Castling.RemoveChoice("Flexible");
      PawnDoubleMove = true;
      EnPassant = true;
      Castling.AddChoice("CwDA", "Standard castling with the extra exception to prevent color-bound pieces from changing square colors");
      Castling.Value = "CwDA";
    }
    #endregion

    #region AddPieceTypes
    public override void AddPieceTypes()
    {
      CastlingType = King;

      string loadableTypes = "KQRBNP" + promotions;
      if (HumanPlayer == 0)
        loadableTypes = loadableTypes.ToUpper();
      else
        loadableTypes = loadableTypes.ToLower();

      AddPieceType(King);
      if (ApmwCore.getInstance().foundKingPromotions > 0)
        AddPieceType(Kings[ApmwCore.getInstance().foundKingPromotions]);
      foreach (PieceType piece in Pawns)
        AddPieceType(piece);
      foreach (PieceType piece in Minors)
        if (loadableTypes.Contains(piece.Notation[HumanPlayer]))
          AddPieceType(piece);
      foreach (PieceType piece in Majors)
        if (loadableTypes.Contains(piece.Notation[HumanPlayer]))
          AddPieceType(piece);
      foreach (PieceType piece in Queens)
        if (loadableTypes.Contains(piece.Notation[HumanPlayer]))
          AddPieceType(piece);


      //	Army adjustment
      //if ((WhiteArmy.Value == "Fabulous FIDEs" && BlackArmy.Value == "Remarkable Rookies") ||
      //  (BlackArmy.Value == "Fabulous FIDEs" && WhiteArmy.Value == "Remarkable Rookies"))
      //{
      //  //	increase the value of the bishops since the Rookies have no piece that moves that way
      //  Bishop.MidgameValue += 35;
      //  Bishop.EndgameValue += 35;
      //}
    }
    #endregion

    #region SetOtherVariables
    public override void SetOtherVariables()
    {
      base.SetOtherVariables();

      ApmwCore starter = ApmwCore.getInstance();
      int humanPlayer = starter.GeriProvider();

      earlyPopulatePieceTypes();
      (Dictionary<KeyValuePair<int, int>, PieceType>, string) pieceSet =
        starter.PlayerPieceSetProvider();
      startingPosition = pieceSet.Item1;
      promotions = pieceSet.Item2;
      List<PieceType> pocketPieces = ApmwCore.getInstance().PlayerPocketPiecesProvider();
      promotions += string.Join("", pocketPieces
        .Select(p => p != null ? p.Notation[humanPlayer] : "")
        .Where(p => !promotions.Contains(p) && !Pawns.Select(pn => pn.Notation[humanPlayer]).Contains(p)));
      PromotionTypes += promotions;

      string humanPrefix = "Black";
      string cpuPrefix = "White";
      if (humanPlayer == 0)
      {
        (humanPrefix, cpuPrefix) = (cpuPrefix, humanPrefix);

        // TODO(chesslogic): CPU gets 1 piece per checkmate (as location?), Goal is to checkmate a "full" CPU army
        // TODO(chesslogic): CPU different armies
        SetCustomProperty("BlackOuter", "8");
        SetCustomProperty("BlackPawns", "pppppppp");
        SetCustomProperty("BlackPieces", "rnbqkbnr");
        PromotionTypes += "qrbn";
      }
      else
      {
        SetCustomProperty("WhiteOuter", "8");
        SetCustomProperty("WhitePawns", "PPPPPPPP");
        SetCustomProperty("WhitePieces", "RNBQKBNR");
        PromotionTypes += "QRBN";
      }

      //	determine player's board
      // TODO(chesslogic): incorporate ApmwCore
      Dictionary<int, string> notations = new Dictionary<int, string>();
      StringBuilder CastleRooks = new StringBuilder();
      if (humanPlayer == 0)
      {
        CastleRooks.Append("kq");
      }
      else
      {
        CastleRooks.Append("KQ");
      }

      // Set castling pieces
      for (int rank = 0; rank < this.NumRanks; rank++)
      {
        notations[rank] = "";
        int emptySpaceCount = 0;
        for (int file = 0; file < this.NumFiles; file++)
        {
          var place = new KeyValuePair<int, int>(rank, file);
          if (startingPosition.ContainsKey(place))
          {
            if (emptySpaceCount > 0)
            {
              notations[rank] += Convert.ToChar('0' + emptySpaceCount);
              emptySpaceCount = 0;
            }

            PieceType pieceType = startingPosition[place];
            notations[rank] += pieceType.Notation[humanPlayer];
            if (rank == 4 && Majors.Contains(pieceType))
            {
              var newCastlingRook = (char)('a' + file);
              if (humanPlayer == 0)
              {
                newCastlingRook = Char.ToUpper(newCastlingRook);
              }
              CastleRooks.Append(newCastlingRook);
            }
          }
          else
          {
            emptySpaceCount++;
          }
        }
        if (emptySpaceCount > 0)
        {
          if (emptySpaceCount > 9)
          {
            emptySpaceCount -= 10;
            notations[rank] += "1";
          }
          notations[rank] += Convert.ToChar('0' + emptySpaceCount);
          emptySpaceCount = 0;
        }
      }
      SetCustomProperty("CastleRooks", CastleRooks.ToString());

      // determine pockets
      SetCustomProperty("PocketPieces",
        (humanPlayer == 1 ? "3" : "") +
        pocketPieces
          .Select((PieceType piece) => piece == null ? "1" : piece.Notation[humanPlayer])
          .Aggregate("", (string piece, string notation) => notation + piece)
          + (humanPlayer == 0 ? "3" : ""));
     
      if (humanPlayer == 0)
      {
        SetCustomProperty("BlackFourth", notations[0]);
        SetCustomProperty("WhiteFourth", notations[1]);
      }
      else
      {
        SetCustomProperty("WhiteFourth", notations[0]);
        SetCustomProperty("BlackFourth", notations[1]);
      }
      SetCustomProperty(humanPrefix + "Outer", notations[2]);
      SetCustomProperty(humanPrefix + "Pawns", notations[3]);
      SetCustomProperty(humanPrefix + "Pieces", notations[4]);
    }
    #endregion

    public void earlyPopulatePieceTypes()
    {
      King = new King("King", "K", 325, 325);
      MountedKing = new MountedKing("Mounted King", "W", 700, 700, preferredImageName: "Champion");
      HyperKing = new HyperKing("Hyper King", "W", 1175, 1175, preferredImageName: "Frog");
      Pawn = new Pawn("Pawn", "P", 100, 125);
      Rook = new Rook("Rook", "R", 500, 550);
      Bishop = new Bishop("Bishop", "B", 325, 350);
      Knight = new Knight("Knight", "N", 325, 325);
      Queen = new Queen("Queen", "Q", 950, 1000);
      // Berolina
      BerolinaPawn = new BerolinaPawn("Berolina Pawn", "Z", 100, 125, preferredImageName: "Ferz");
      // Cwda
      //AddPieceType(Queen = new Queen("Queen", "Q", 950, 1000));
      //AddPieceType(Rook = new Rook("Rook", "R", 500, 550));
      //AddPieceType(Bishop = new Bishop("Bishop", "B", 325, 350));
      //AddPieceType(Knight = new Knight("Knight", "N", 325, 325));
      Archbishop = new Archbishop("Archbishop", "A", 875, 875);
      WarElephant = new WarElephant("War Elephant", "E", 475, 475);
      Phoenix = new Phoenix("Phoenix", "X", 315, 315);
      Cleric = new Cleric("Cleric", "G", 450, 500);
      Chancellor = new Chancellor("Chancellor", "C", 950, 950);
      ShortRook = new ShortRook("Short Rook", "S", 400, 425);
      Tower = new Tower("Tower", "T", 325, 325);
      Lion = new Lion("Lion", "I", 500, 500);
      ChargingRook = new ChargingRook("Charging Rook", "H", 495, 530);
      NarrowKnight = new NarrowKnight("Lancer", "L", 325, 325);
      ChargingKnight = new ChargingKnight("Charging Knight", "M", 365, 365);
      Colonel = new Colonel("Colonel", "Y", 950, 950);
      // Eurasian
      Cannon = new Cannon("Cannon", "O", 400, 275);
      Vao = new Vao("Vao", "V", 300, 175);
      // Misc
      Herald = new Herald("Herald", "D", 890, 900);
      Nightrider = new Nightrider("Nightrider", "J", 550, 550, "Knightsrider");
      Scout = new Scout("Scout", "U", 300, 300);
      Queennon = new Queennon("Queennon", "F", 1025, 720);

      Kings.Add(King);
      Kings.Add(MountedKing);
      Kings.Add(HyperKing);

      Pawns.Add(Pawn);
      Pawns.Add(BerolinaPawn);

      Minors.Add(Bishop);
      Minors.Add(Knight);
      Minors.Add(Phoenix); // awkward
      Minors.Add(ShortRook); // unusually powerful
      Minors.Add(Tower);
      Minors.Add(NarrowKnight);
      Minors.Add(ChargingKnight);
      Minors.Add(Vao); // very weak
      Minors.Add(Cannon); // unusually powerful
      Minors.Add(Scout); // slightly weak

      Majors.Add(Rook);
      Majors.Add(WarElephant);
      Majors.Add(Cleric);
      Majors.Add(Lion);
      Majors.Add(ChargingRook);
      Majors.Add(Nightrider);

      Queens.Add(Queen);
      Queens.Add(Archbishop);
      Queens.Add(Chancellor);
      Queens.Add(Colonel);
      Queens.Add(Queennon); // hilarious comedy option
      Queens.Add(Herald); // hilarious comedy option

      Colorbounds.Add(Bishop);
      Colorbounds.Add(WarElephant);
      Colorbounds.Add(Cleric);

      Armies.AddRange(new HashSet<PieceType>[] {
        new HashSet<PieceType>() { Bishop, Knight, Rook, Queen },
        new HashSet<PieceType>() { WarElephant, Phoenix, Cleric, Archbishop }, // has 2 major pieces and 1 minor piece
        new HashSet<PieceType>() { Tower, ShortRook, Lion, Chancellor },
        new HashSet<PieceType>() { ChargingKnight, NarrowKnight, ChargingRook, Colonel },
        // TODO(chesslogic): Decide breadth of Eurasian army
        // new HashSet<PieceType>() { Bishop, Cannon, Rook, Colonel });
        // new HashSet<PieceType>() { Vao, Cannon, Rook, Colonel });
        new HashSet<PieceType>() { Vao, Cannon, Rook, Queennon },
        new HashSet<PieceType>() { NarrowKnight, Nightrider, Scout, Herald },
      });
    }
  }
}
