using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessV.Games.MiscellaneousGames
{
  [Game("Archipelago Multiworld Super-Sized", typeof(Geometry.Rectangular), 10, 8, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  internal class ApmwGrandChess : ApmwChessGame
  {
    #region CreateBoard
    //	We override the CreateBoard function so the game uses a board of 
    //	type BoardWithCards instead of Board.  This is enough to trigger the 
    //	board with cards architecture and proper rendering to the display.
    public override Board CreateBoard(int nPlayers, int nFiles, int nRanks, Symmetry symmetry)
    {
      this.NumFiles = 10;
      return base.CreateBoard(nPlayers, 10, nRanks, symmetry);
    }
    #endregion

    public override void SetOtherVariables()
    {
      base.SetOtherVariables();

      ApmwCore starter = ApmwCore.getInstance();
      int humanPlayer = starter.GeriProvider();

      List<PieceType> pocketPieces = ApmwCore.getInstance().PlayerPocketPiecesProvider();


      string humanPrefix = "Black";
      string cpuPrefix = "White";
      if (humanPlayer == 0)
      {
        (humanPrefix, cpuPrefix) = (cpuPrefix, humanPrefix);

        // TODO(chesslogic): CPU gets 1 piece per checkmate (as location?), Goal is to checkmate a "full" CPU army
        // TODO(chesslogic): CPU different armies
        SetCustomProperty("BlackOuter", "10");
        SetCustomProperty("BlackPawns", "pppppppppp");
        SetCustomProperty("BlackPieces", "rnabqkbcnr");
        PromotionTypes += "ac";
        promotions += "ac";
      }
      else
      {
        SetCustomProperty("WhiteOuter", "10");
        SetCustomProperty("WhitePawns", "PPPPPPPPPP");
        SetCustomProperty("WhitePieces", "RNABQKBCNR");
        PromotionTypes += "AC";
        promotions += "AC";
      }
    }
  }
}
