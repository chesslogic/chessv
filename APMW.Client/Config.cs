﻿using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  internal enum Goal
  {
    Single = 0, Progressive = 1, OrderedProgressive = 2,
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
    Chaos = 0, Limited = 1, Fair = 2,
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
    private FairyTypes fairy;
    public FairyTypes Fairy { get { return fairy; } }
    public int FairyInt
    {
      set
      {
        fairy = (FairyTypes)value;
      }
    }
    private FairyArmy army;
    public FairyArmy Army { get { return army; } }
    public int ArmyInt
    {
      set
      {
        army = (FairyArmy)value;
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

      seed();

      // Implemented by ChecksMate protocol
      //SlotData["max_material"]
      //SlotData["min_material"]
      //SlotData["early_material"]
      //SlotData["queen_piece_limit"]

      // Progressive Goal
      GoalInt = Convert.ToInt32(SlotData["goal"]);
      EnemyTypesInt = Convert.ToInt32(SlotData["enemy_piece_types"]);

      // Chaotic Material Randomization
      // Non-Progressive Material
      LocsInt = Convert.ToInt32(SlotData["piece_locations"]);
      TypesInt = Convert.ToInt32(SlotData["piece_types"]);

      // Army-Constrained Material
      ArmyInt = Convert.ToInt32(SlotData["fairy_chess_army"]);

      // Non-Fairy Chess
      FairyInt = Convert.ToInt32(SlotData["fairy_chess_pieces"]);
      PawnsInt = Convert.ToInt32(SlotData["fairy_chess_pawns"]);

      // Piece Limits
      minorTypeLimit = Convert.ToInt32(SlotData["minor_piece_limit_by_type"]);
      majorTypeLimit = Convert.ToInt32(SlotData["major_piece_limit_by_type"]);
      queenTypeLimit = Convert.ToInt32(SlotData["queen_piece_limit_by_type"]);
    }

    public void seed()
    {
      Random random = new Random();
      int[] seeds = new int[] {
          Convert.ToInt32(SlotData["pocketSeed"]),
          Convert.ToInt32(SlotData["pawnSeed"]),
          Convert.ToInt32(SlotData["minorSeed"]),
          Convert.ToInt32(SlotData["majorSeed"]),
          Convert.ToInt32(SlotData["queenSeed"]),
        };
      pocketSeed = seeds[0];
      pawnSeed = seeds[1];
      pawnLocSeed = seeds[1];
      minorSeed = seeds[2];
      minorLocSeed = seeds[2];
      majorSeed = seeds[3];
      majorLocSeed = seeds[3];
      queenSeed = seeds[4];
      queenLocSeed = seeds[4];
      // TODO(chesslogic): I thought about it for a moment, and I think this is fine
      // But maybe Mersenne twister is happier with some sort of offset
      
      if (this.Types == PieceTypes.Chaos)
      {
        pocketSeed = random.Next();
        pawnSeed = random.Next();
        minorSeed = random.Next();
        majorSeed = random.Next();
        queenSeed = random.Next();
      }
      if (this.Locs == PieceLocations.Chaos)
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
      if (pocketSeed == -1) { throw new InvalidOperationException("Please set Starter.pocketSeed"); }

      // preserve choices separate from values
      Random pocketRandom = new Random(pocketSeed);
      pocketChoiceSeed = new List<int>() { pocketRandom.Next(), pocketRandom.Next(), pocketRandom.Next() };

      // probably not uniform... but it's within range so it works for now. will break FEN later
      Random random = new Random(pocketSeed);
      var x = random.Next(Math.Max(0, foundPockets - 8), Math.Min(foundPockets, 5));
      if (x == foundPockets)
      {
        return new List<int>() { x, 0, 0 };
      }
      var y = random.Next(Math.Max(0, foundPockets - 4 - x), Math.Min(foundPockets, 5));
      var z = foundPockets - (y + x);
      if (z < 0)
      {
        (x, y, z) = (4 - x, 4 - y, -z);
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
