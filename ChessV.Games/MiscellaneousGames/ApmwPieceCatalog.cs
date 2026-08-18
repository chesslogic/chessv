using ChessV.Base;
using ChessV.Games.Pieces.Apmw;
using ChessV.Games.Pieces.Berolina;
using ChessV.Games.Pieces.OdinsRune;
using System;
using System.Collections.Generic;

namespace ChessV.Games
{
  /// <summary>
  /// Owns a complete, internally consistent set of APMW piece types.
  /// Catalog instances are deliberately fresh because PieceType.Initialize binds
  /// each piece to a particular Game and Board.
  /// </summary>
  public sealed class ApmwPieceCatalog
  {
    private static readonly object publishLock = new object();

    private ApmwPieceCatalog()
    {
      King = new King("King", "K", 325, 325);
      MountedKing = new MountedKing("Mounted King", "W", 700, 700, preferredImageName: "Champion");
      HyperKing = new HyperKing("Hyper King", "Z", 1175, 1175, preferredImageName: "Frog");
      Pawn = new Pawn("Pawn", "P", 100, 125);
      Rook = new Rook("Rook", "R", 500, 550);
      Bishop = new Bishop("Bishop", "B", 325, 350);
      Knight = new Knight("Knight", "N", 325, 325);
      Queen = new Queen("Queen", "Q", 950, 1000);
      BerolinaPawn = new BerolinaPawn("Berolina Pawn", "Ŕ", 85, 120, preferredImageName: "Ferz");
      Checkers = new Checkers("Checkers", "Ç", 40, 95, preferredImageName: "CircleLittle");
      Sergeant = new Sergeant("Sergeant", "Ŝ", 200, 225, preferredImageName: "General");
      OdinPawn = new OdinPawn("Odin Pawn", "Ó", 150, 200, preferredImageName: "Wizard");
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
      Cannon = new Cannon("Cannon", "O", 400, 275);
      Vao = new Vao("Vao", "V", 300, 175);
      Herald = new Herald("Herald", "D", 1300, 1300);
      Amazon = new Amazon("Amazon", "Â", 1300, 1300);
      Paladin = new Paladin("Paladin", "Ṕ", 1300, 1350);
      Nightrider = new Nightrider("Nightrider", "J", 550, 550, "Knightsrider");
      Scout = new Scout("Scout", "U", 300, 300);
      Queennon = new Queennon("Queennon", "F", 1025, 720);
      Petal = new Petal("Petal", "Ă", 475, 575);
      Ribbon = new Ribbon("Ribbon", "Ŋ", 275, 375);
      Gardener = new Gardener("Gardener", "Ř", 250, 250);
      Miracle = new Miracle("Miracle", "Ħ", 960, 1050);
      AgileRook = new AgileRook("Agile Rook", "Ŗ", 700, 700);
      Mullah = new Mullah("Mullah", "Ŏ", 700, 700);
      Zealot = new Zealot("Zealot", "Ż", 700, 700);
      GreatCamel = new GreatCamel("Great Camel", "Č", 700, 700);
      DragonCannon = new DragonCannon("Dragon Cannon", "Ð", 700, 700);
      Mameluk = new Mameluk("Mameluk", "Ē", 700, 700);
      Grazer = new Grazer("Grazer", "Ŧ", 700, 700);

      Kings = new List<PieceType> { King, MountedKing, HyperKing };
      Pawns = new HashSet<PieceType> { Pawn, BerolinaPawn, Checkers };
      Sergeants = new HashSet<PieceType> { Sergeant, OdinPawn };
      Minors = new HashSet<PieceType>
      {
        Bishop, Knight, Phoenix, ShortRook, Tower, NarrowKnight,
        ChargingKnight, Vao, Cannon, Scout, Gardener, Ribbon,
      };
      Majors = new HashSet<PieceType>
      {
        Rook, WarElephant, Cleric, Lion, ChargingRook, Nightrider, Petal,
      };
      Jacks = new HashSet<PieceType>
      {
        AgileRook, Mullah, Zealot, GreatCamel, DragonCannon, Mameluk, Grazer,
      };
      Queens = new HashSet<PieceType>
      {
        Queen, Archbishop, Chancellor, Colonel, Queennon, Miracle,
      };
      Amazons = new HashSet<PieceType> { Amazon, Herald, Paladin };
      Colorbounds = new HashSet<PieceType> { Bishop, WarElephant, Cleric };
      Armies = new List<HashSet<PieceType>>
      {
        new HashSet<PieceType> { Bishop, Knight, Rook, Queen, AgileRook },
        new HashSet<PieceType> { WarElephant, Phoenix, Cleric, Archbishop, Mullah },
        new HashSet<PieceType> { Tower, ShortRook, Lion, Chancellor, Zealot },
        new HashSet<PieceType> { ChargingKnight, NarrowKnight, ChargingRook, Colonel, Mameluk },
        new HashSet<PieceType> { Vao, Cannon, Herald, Queennon, DragonCannon },
        new HashSet<PieceType> { Scout, Nightrider, Miracle, Colonel, Mameluk },
        new HashSet<PieceType> { Gardener, Ribbon, Petal, Miracle, Grazer },
      };
      PocketSets = new List<HashSet<PieceType>> { Pawns, Minors, Majors, Queens };
    }

    public PieceType King { get; }
    public PieceType MountedKing { get; }
    public PieceType HyperKing { get; }
    public PieceType Pawn { get; }
    public PieceType Rook { get; }
    public PieceType Bishop { get; }
    public PieceType Knight { get; }
    public PieceType Queen { get; }
    public PieceType BerolinaPawn { get; }
    public Checkers Checkers { get; }
    public PieceType Sergeant { get; }
    public PieceType OdinPawn { get; }
    public PieceType Archbishop { get; }
    public PieceType WarElephant { get; }
    public PieceType Phoenix { get; }
    public PieceType Cleric { get; }
    public PieceType ShortRook { get; }
    public PieceType Tower { get; }
    public PieceType Lion { get; }
    public PieceType Chancellor { get; }
    public PieceType ChargingRook { get; }
    public PieceType NarrowKnight { get; }
    public PieceType ChargingKnight { get; }
    public PieceType Colonel { get; }
    public PieceType Cannon { get; }
    public PieceType Vao { get; }
    public Queennon Queennon { get; }
    public Herald Herald { get; }
    public Amazon Amazon { get; }
    public Paladin Paladin { get; }
    public Nightrider Nightrider { get; }
    public Scout Scout { get; }
    public PieceType Gardener { get; }
    public PieceType Ribbon { get; }
    public PieceType Petal { get; }
    public PieceType Miracle { get; }
    public PieceType AgileRook { get; }
    public PieceType Mullah { get; }
    public PieceType Zealot { get; }
    public PieceType GreatCamel { get; }
    public PieceType DragonCannon { get; }
    public PieceType Mameluk { get; }
    public PieceType Grazer { get; }
    public List<PieceType> Kings { get; }
    public HashSet<PieceType> Pawns { get; }
    public HashSet<PieceType> Sergeants { get; }
    public HashSet<PieceType> Minors { get; }
    public HashSet<PieceType> Majors { get; }
    public HashSet<PieceType> Jacks { get; }
    public HashSet<PieceType> Queens { get; }
    public HashSet<PieceType> Amazons { get; }
    public HashSet<PieceType> Colorbounds { get; }
    public List<HashSet<PieceType>> Armies { get; }
    public List<HashSet<PieceType>> PocketSets { get; }

    public static ApmwPieceCatalog Create()
    {
      return new ApmwPieceCatalog();
    }

    public static void EnsurePublished()
    {
      ApmwCore core = ApmwCore.getInstance();
      if (IsComplete(core))
        return;

      lock (publishLock)
      {
        if (!IsComplete(core))
          Create().Publish(core);
      }
    }

    public void BindTo(ApmwChessGame game)
    {
      if (game == null)
        throw new ArgumentNullException(nameof(game));

      game.King = King;
      game.MountedKing = MountedKing;
      game.HyperKing = HyperKing;
      game.Pawn = Pawn;
      game.Rook = Rook;
      game.Bishop = Bishop;
      game.Knight = Knight;
      game.Queen = Queen;
      game.BerolinaPawn = BerolinaPawn;
      game.Checkers = Checkers;
      game.Sergeant = Sergeant;
      game.OdinPawn = OdinPawn;
      game.Archbishop = Archbishop;
      game.WarElephant = WarElephant;
      game.Phoenix = Phoenix;
      game.Cleric = Cleric;
      game.ShortRook = ShortRook;
      game.Tower = Tower;
      game.Lion = Lion;
      game.Chancellor = Chancellor;
      game.ChargingRook = ChargingRook;
      game.NarrowKnight = NarrowKnight;
      game.ChargingKnight = ChargingKnight;
      game.Colonel = Colonel;
      game.Cannon = Cannon;
      game.Vao = Vao;
      game.Queennon = Queennon;
      game.Herald = Herald;
      game.Amazon = Amazon;
      game.Paladin = Paladin;
      game.Nightrider = Nightrider;
      game.Scout = Scout;
      game.Gardener = Gardener;
      game.Ribbon = Ribbon;
      game.Petal = Petal;
      game.Miracle = Miracle;
      game.AgileRook = AgileRook;
      game.Mullah = Mullah;
      game.Zealot = Zealot;
      game.GreatCamel = GreatCamel;
      game.DragonCannon = DragonCannon;
      game.Mameluk = Mameluk;
      game.Grazer = Grazer;
      game.Kings = Kings;
      game.Pawns = Pawns;
      game.Sergeants = Sergeants;
      game.Minors = Minors;
      game.Majors = Majors;
      game.Jacks = Jacks;
      game.Queens = Queens;
      game.Amazons = Amazons;
      game.Colorbounds = Colorbounds;
      game.Armies = Armies;
      game.PocketSets = PocketSets;
    }

    public void Publish(ApmwCore core)
    {
      if (core == null)
        throw new ArgumentNullException(nameof(core));

      core.kings = Kings;
      core.pawns = Pawns;
      core.sergeants = Sergeants;
      core.minors = Minors;
      core.majors = Majors;
      core.jacks = Jacks;
      core.queens = Queens;
      core.amazons = Amazons;
      core.colorbound = Colorbounds;
      core.armies = Armies;
      core.pocketSets = PocketSets;
    }

    private static bool IsComplete(ApmwCore core)
    {
      return core.kings != null && core.kings.Count > 0 &&
        core.pawns != null && core.pawns.Count > 0 &&
        core.sergeants != null && core.sergeants.Count > 0 &&
        core.minors != null && core.minors.Count > 0 &&
        core.majors != null && core.majors.Count > 0 &&
        core.jacks != null && core.jacks.Count > 0 &&
        core.queens != null && core.queens.Count > 0 &&
        core.amazons != null && core.amazons.Count > 0 &&
        core.armies != null && core.armies.Count > 0 &&
        core.pocketSets != null && core.pocketSets.Count > 0;
    }
  }
}
