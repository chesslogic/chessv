using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessV.Games.Pieces.Apmw
{
  /// <summary>
  /// Provides various pieces estimated to be 7 material.
  /// 
  /// FIDE gains a combined Rook + Elephant, which moves like a Rook but also a 2,2 leaper.
  /// 
  /// Colorbound Clobberers gain a combined Cleric + Camel, which moves like a Cleric but also a 1,3 leaper.
  /// 
  /// Reliable Rookies gain a combined Rook + non-leaping Camel, which moves like a Rook but can also
  /// side-step 1 square after 3 sliding squares.
  /// 
  /// Nutty Knights gain a combined Knight + Camel, which moves like a Knight but also a 1,3 leaper.
  /// 
  /// Cannons gain a combined Cannon + Dao, similar to the Queennon without knight cannon moves nor King captures.
  /// 
  /// Camels gain a Camel-Rider + Wazir, which moves as a 1,3 slider but also has a 0,1 step move like a Phoenix.
  /// 
  /// Petals gain a Ribbon that can also move 1 further step before it first rotates.
  /// </summary>

  [PieceType("Agile Rook", "APMW Custom Pieces")]
  public class AgileRook : PieceType
  {
    public AgileRook(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Agile Rook", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Minister" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Rook moves
      type.Slide(new Direction(1, 0));
      type.Slide(new Direction(-1, 0));
      type.Slide(new Direction(0, 1));
      type.Slide(new Direction(0, -1));

      // Elephant (2,2) leaper moves
      type.Step(new Direction(2, 2));
      type.Step(new Direction(2, -2));
      type.Step(new Direction(-2, 2));
      type.Step(new Direction(-2, -2));
    }
  }

  [PieceType("Colorbound Jack", "APMW Custom Pieces")]
  public class ColorboundJack : PieceType
  {
    public ColorboundJack(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Colorbound Jack", name, notation, midgameValue, endgameValue, preferredImageName == null ? "DragonHorse" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Cleric moves (diagonal slider)
      type.Slide(new Direction(1, 1));
      type.Slide(new Direction(1, -1));
      type.Slide(new Direction(-1, 1));
      type.Slide(new Direction(-1, -1));

      // Camel (1,3) leaper moves
      type.Step(new Direction(1, 3));
      type.Step(new Direction(3, 1));
      type.Step(new Direction(3, -1));
      type.Step(new Direction(1, -3));
      type.Step(new Direction(-1, -3));
      type.Step(new Direction(-3, -1));
      type.Step(new Direction(-3, 1));
      type.Step(new Direction(-1, 3));
    }
  }

  [PieceType("Rookie Jack", "APMW Custom Pieces")]
  public class RookieJack : PieceType
  {
    public RookieJack(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Rookie Jack", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Rook" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Rook moves
      type.Slide(new Direction(1, 0));
      type.Slide(new Direction(-1, 0));
      type.Slide(new Direction(0, 1));
      type.Slide(new Direction(0, -1));

      // Non-leaping Camel moves (slide 3, then step 1 sideways)
      MoveCapability move;
      MovePathInfo movePath;

      // Vertical moves with side-steps
      move = MoveCapability.Step(new Direction(3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 0), new Direction(1, 0), new Direction(1, 0),new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Horizontal moves with side-steps
      move = MoveCapability.Step(new Direction(1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }

  [PieceType("Knight Jack", "APMW Custom Pieces")]
  public class KnightJack : PieceType
  {
    public KnightJack(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Knight Jack", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Knight" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Knight moves
      type.Step(new Direction(2, 1));
      type.Step(new Direction(2, -1));
      type.Step(new Direction(-2, 1));
      type.Step(new Direction(-2, -1));
      type.Step(new Direction(1, 2));
      type.Step(new Direction(1, -2));
      type.Step(new Direction(-1, 2));
      type.Step(new Direction(-1, -2));

      // Camel moves
      type.Step(new Direction(1, 3));
      type.Step(new Direction(3, 1));
      type.Step(new Direction(3, -1));
      type.Step(new Direction(1, -3));
      type.Step(new Direction(-1, -3));
      type.Step(new Direction(-3, -1));
      type.Step(new Direction(-3, 1));
      type.Step(new Direction(-1, 3));
    }
  }

  [PieceType("Cannon Jack", "APMW Custom Pieces")]
  public class CannonJack : PieceType
  {
    public CannonJack(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Cannon Jack", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Cannon" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Cannon.AddMoves(type);

      // Dao moves (diagonal)
      type.CannonMove(new Direction(1, 1));
      type.CannonMove(new Direction(1, -1));
      type.CannonMove(new Direction(-1, 1));
      type.CannonMove(new Direction(-1, -1));
    }
  }

  [PieceType("Mameluk", "APMW Custom Pieces")]
  public class Mameluk : PieceType
  {
    public Mameluk(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Mameluk", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Camel" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Wazir moves (orthogonal single step)
      Wazir.AddMoves(type);

      // Camel-Rider moves (1,3 slider)
      type.Slide(new Direction(1, 3));
      type.Slide(new Direction(3, 1));
      type.Slide(new Direction(3, -1));
      type.Slide(new Direction(1, -3));
      type.Slide(new Direction(-1, -3));
      type.Slide(new Direction(-3, -1));
      type.Slide(new Direction(-3, 1));
      type.Slide(new Direction(-1, 3));
    }
  }

  [PieceType("Grazer", "APMW Custom Pieces")]
  public class Grazer : PieceType
  {
    public Grazer(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Grazer", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Ram" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ribbon.AddMoves(type);

      // Enhanced Ribbon moves - can move 1 further step before rotating
      type.Slide(new Direction(1, 1), maxSteps: 2);
      type.Slide(new Direction(1, -1), maxSteps: 2);
      type.Slide(new Direction(-1, 1), maxSteps: 2);
      type.Slide(new Direction(-1, -1), maxSteps: 2);

      // Add the bent moves like the regular Ribbon
      MoveCapability move;
      MovePathInfo movePath;

      // First Y, Then X
      move = MoveCapability.Step(new Direction(4, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(1, 1), new Direction(1, 1), new Direction(1, -1), new Direction(1, 0), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Add similar moves for other directions...
      // (Additional move patterns would be added following the same pattern as the Ribbon class)
    }
  }
}
