using ChessV.Base;

namespace ChessV.Games
{
  [Game(ApmwProfiles.TenByTenGameName, typeof(Geometry.Rectangular), 10, 10, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwTenByTenChess : ApmwChessGame
  {
    public override ApmwGeometryProfile ApmwProfile
    {
      get { return ApmwProfiles.TenByTen; }
    }
  }

  [Game(ApmwProfiles.TwelveByTenGameName, typeof(Geometry.Rectangular), 12, 10, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwTwelveByTenChess : ApmwChessGame
  {
    public override ApmwGeometryProfile ApmwProfile
    {
      get { return ApmwProfiles.TwelveByTen; }
    }
  }

  [Game(ApmwProfiles.TwelveByTwelveGameName, typeof(Geometry.Rectangular), 12, 12, 3,
      Invented = "2019",
      InventedBy = "Berserker",
      Tags = "Chess Variant,Multiple Boards,Popular,Different Armies")]
  [Appearance(ColorScheme = "Sublimation")]
  public class ApmwTwelveByTwelveChess : ApmwChessGame
  {
    public override ApmwGeometryProfile ApmwProfile
    {
      get { return ApmwProfiles.TwelveByTwelve; }
    }
  }
}
