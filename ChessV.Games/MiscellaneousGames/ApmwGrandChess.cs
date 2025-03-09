using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessV.Games
{
  [Game("Archipelago Multiworld Super-Sized", typeof(Geometry.Rectangular), 10, 8, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwGrandChess : ApmwChessGame
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

      string pawns = "pppppppppp";
      string pieces = "rnabqkbcnr";
      
      string enemyArmy = (string)GetCustomProperty("EnemyArmy");
      if (enemyArmy != null)
      {
        // Colourbound Clobberers (Betza)
        if (enemyArmy == "Colourbound Clobberers (Betza)")
        {
          pieces = "gxqeakecxg";
        }
        // Remarkable Rookies (Betza)
        else if (enemyArmy == "Remarkable Rookies (Betza)")
        {
          pieces = "staickiqts";
        }
        // Nutty Knights (Betza)
        else if (enemyArmy == "Nutty Knights (Betza)")
        {
          pieces = "hlamykmclh";
        }
      }

      if (humanPlayer == 0)
      {
        // TODO(chesslogic): CPU gets 1 piece per checkmate (as location?), Goal is to checkmate a "full" CPU army
        SetCustomProperty("BlackOuter", "10");
        SetCustomProperty("BlackPawns", pawns);
        SetCustomProperty("BlackPieces", pieces);
      }
      else
      {
        SetCustomProperty("WhiteOuter", "10");
        SetCustomProperty("WhitePawns", pawns.ToUpper());
        SetCustomProperty("WhitePieces", pieces.ToUpper());
      }

      // Add the army's attendant pieces to promotions (positions 2 and 7)
      string attendantPromotions = pieces.Substring(2, 1) + pieces.Substring(7, 1);
      PromotionTypes += attendantPromotions;
    }
  }
}
