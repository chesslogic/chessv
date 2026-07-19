using ChessV.Base;

namespace ChessV.Games
{
  [Game(ApmwProfiles.GrandGameName, typeof(Geometry.Rectangular), 10, 8, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwGrandChess : ApmwChessGame
  {
    public override ApmwGeometryProfile ApmwProfile
    {
      get { return ApmwProfiles.Grand; }
    }
  }
}
