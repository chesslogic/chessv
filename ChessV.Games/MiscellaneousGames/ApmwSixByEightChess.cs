namespace ChessV.Games
{
  [Game(ApmwProfiles.SixByEightGameName, typeof(Geometry.Rectangular), 6, 8, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwSixByEightChess : ApmwChessGame
  {
    public override ApmwGeometryProfile ApmwProfile
    {
      get { return ApmwProfiles.SixByEight; }
    }
  }
}
