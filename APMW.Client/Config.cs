using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.APChessV
{
  internal enum Goal
  {
    Single = 0, OrderedProgressive = 1, Progressive = 2, Super = 3
  }
  internal enum PieceLocations
  {
    Chaos = 0, Stable = 1, Ordered = 2,
  }
  /** Applies to Player and Enemy */
  internal enum PieceTypes
  {
    Chaos = 0, Stable = 1, Book = 2,
  }
  internal enum FairyTypes
  {
    Vanilla = 0, Full = 1, CwDA = 2, Cannon = 3, Eurasian = 4,
  }
  internal enum FairyArmy
  {
    Chaos = 0, Stable = 1, Limited = 2,
  }
  internal enum FairyPawns
  {
    Vanilla = 0, Mixed = 1, Berolina = 2,
  }

  internal class ApmwConfig
  {
    public static ApmwConfig _instance;
    public static ApmwConfig getInstance()
    {
      if (_instance == null)
      {
        lock (typeof(ApmwConfig))
        {
          if (_instance == null)
          {
            _instance = new ApmwConfig();
          }
        }
      }
      return _instance;
    }

    public Dictionary<string, object> SlotData { get; private set; }

    public int pocketSeed = -1;
    public List<int> pocketChoiceSeed { get; private set; }
    public int pawnSeed = -1;
    public int pawnLocSeed = -1;
    public int minorSeed = -1;
    public int minorLocSeed = -1;
    public int majorSeed = -1;
    public int majorLocSeed = -1;
    public int queenSeed = -1;
    public int queenLocSeed = -1;

    public int minorTypeLimit = -1;
    public int majorTypeLimit = -1;
    public int queenTypeLimit = -1;
    public int pocketLimit = -1;
    public List<int> Army = new List<int>();

    private Goal goal;
    public Goal Goal { get { return goal; } }
    public int GoalInt
    {
      set
      {
        goal = (Goal)value;
      }
    }
    private PieceLocations locs;
    public PieceLocations Locs { get { return locs; } }
    public int LocsInt
    {
      set
      {
        locs = (PieceLocations)value;
      }
    }
    private PieceTypes types;
    public PieceTypes Types { get { return types; } }
    public int TypesInt
    {
      set
      {
        types = (PieceTypes)value;
      }
    }
    private PieceTypes enemyTypes;
    public PieceTypes EnemyTypes { get { return enemyTypes; } }
    public int EnemyTypesInt
    {
      set
      {
        enemyTypes = (PieceTypes)value;
      }
    }
    private FairyArmy fairyArmy;
    /// <summary>
    /// Will be used to implement Stable vs Limited - Limited will randomize the Army value and ignore slotdata
    /// </summary>
    public FairyArmy FairyArmy { get { return fairyArmy; } }
    public int FairyArmyInt
    {
      set
      {
        fairyArmy = (FairyArmy)value;
      }
    }
    private FairyPawns pawns;
    public FairyPawns Pawns { get { return pawns; } }
    public int PawnsInt
    {
      set
      {
        pawns = (FairyPawns)value;
      }
    }

    public void Instantiate(Dictionary<string, object> slotData)
    {
      SlotData = slotData;

      // Implemented by ChecksMate protocol
      //SlotData["max_material"]
      //SlotData["min_material"]
      //SlotData["early_material"]
      //SlotData["queen_piece_limit"]
      //SlotData["max_pocket"]
      //SlotData["fairy_chess_pieces"]

      // Progressive Goal
      GoalInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "goal", Goal.Single));
      EnemyTypesInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "enemy_piece_types", PieceTypes.Book));

      // Chaotic Material Randomization
      // Non-Progressive Material
      LocsInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "piece_locations", PieceLocations.Stable));
      TypesInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "piece_types", PieceTypes.Stable));

      // Army-Constrained Material
      FairyArmyInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "fairy_chess_army", FairyArmy.Chaos));
      Army = ((JArray)SlotData.GetValueOrDefault(
        "army", new JArray())).ToObject<int[]>().ToList();

      // Non-Fairy Chess
      PawnsInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "fairy_chess_pawns", FairyPawns.Mixed));

      // Piece Limits
      minorTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "minor_piece_limit_by_type", 0));
      majorTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "major_piece_limit_by_type", 0));
      queenTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "queen_piece_limit_by_type", 0));
      pocketLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "pocket_limit_by_pocket", 4));
    }

    public void seed()
    {
      Random random = new Random();

      // Types
      if (this.Types != PieceTypes.Chaos)
      {
        pocketSeed = Convert.ToInt32(SlotData["pocket_seed"]);
        pawnSeed = Convert.ToInt32(SlotData["pawn_seed"]);
        minorSeed = Convert.ToInt32(SlotData["minor_seed"]);
        majorSeed = Convert.ToInt32(SlotData["major_seed"]);
        queenSeed = Convert.ToInt32(SlotData["queen_seed"]);
      }
      else
      {
        pocketSeed = random.Next();
        pawnSeed = random.Next();
        minorSeed = random.Next();
        majorSeed = random.Next();
        queenSeed = random.Next();
      }

      // Locations
      if (this.Locs != PieceLocations.Chaos)
      {
        // TODO(chesslogic): I thought about it for a moment, and I think this is fine
        // But maybe Mersenne twister is happier with some sort of offset
        pawnLocSeed = Convert.ToInt32(SlotData["pawn_seed"]);
        minorLocSeed = Convert.ToInt32(SlotData["minor_seed"]);
        majorLocSeed = Convert.ToInt32(SlotData["major_seed"]);
        queenLocSeed = Convert.ToInt32(SlotData["queen_seed"]);
      }
      else
      {
        pawnLocSeed = random.Next();
        minorLocSeed = random.Next();
        majorLocSeed = random.Next();
        queenLocSeed = random.Next();
      }
    }

    /** Possibly not stable - will generate a different pocket distribution as the player progresses through different foundPockets - but it is uniform */
    public List<int> generatePocketValues(int foundPockets)
    {
      if (foundPockets == 0) { return new List<int>() { 0, 0, 0 }; }
      if (pocketSeed == -1) { throw new InvalidOperationException("Please set Starter.pocket_seed"); }

      // preserve choices separate from values
      Random pocketRandom = new Random(pocketSeed);
      pocketChoiceSeed = new List<int>() { pocketRandom.Next(), pocketRandom.Next(), pocketRandom.Next() };

      // probably not uniform... but it's within range so it works for now. will break FEN later
      Random random = new Random(pocketSeed);
      var x = random.Next(
        Math.Max(0, foundPockets - pocketLimit * 2),
        Math.Min(foundPockets, pocketLimit + 1));
      if (x == foundPockets)
      {
        return new List<int>() { x, 0, 0 };
      }
      if (x == foundPockets - 1)
      {
        return new List<int>() { x, 1, 0 };
      }
      var y = random.Next(
        Math.Max(0, foundPockets - pocketLimit - x),
        Math.Min(foundPockets - x, pocketLimit + 1));
      var z = foundPockets - (y + x);
      if (z < 0)
      {
        (x, y, z) = (pocketLimit - x, pocketLimit - y, -z);
      }

      return new List<int>() { x, y, z };
    }

    /**
     * arg spaces should be TOTAL spaces not EMPTY spaces
     * 
     * vaguely inspired by this cacophanous suggestion:
     * https://stackoverflow.com/questions/28544808/random-distribution-of-items-in-list-with-exact-number-of-occurences
     */
    public static Dictionary<int, Item> distribute<Item>(List<Item> items, int spaces)
    {
      // Create list of items * z
      Dictionary<int, Item> allItems = new Dictionary<int, Item>();
      for (int i = 0; i < items.Count; i++)
        allItems.Add(i, items[i]);
      for (int i = items.Count; i < spaces; i++)
        allItems.Add(i, default);

      Random random = new Random();
      int n = allItems.Count;
      while (n > 1)
      {
        n--;
        int k = random.Next(n + 1);
        (allItems[k], allItems[n]) = (allItems[n], allItems[k]);
      }

      return allItems;
    }
  }
}
