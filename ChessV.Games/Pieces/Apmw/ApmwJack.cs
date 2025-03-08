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
      base("Agile Rook (Too swole to fit in frame)", name, notation, midgameValue, endgameValue, preferredImageName == null ? "SwoleRook" : preferredImageName)
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

  [PieceType("Mullah", "APMW Custom Pieces")]
  public class Mullah : PieceType
  {
    public Mullah(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Mullah", name, notation, midgameValue, endgameValue, preferredImageName == null ? "CamelBishop" : preferredImageName)
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

  [PieceType("Zealot", "APMW Custom Pieces")]
  public class Zealot : PieceType
  {
    public Zealot(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Zealot", name, notation, midgameValue, endgameValue, preferredImageName == null ? "DragonHorse" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Rook moves
      Rook.AddMoves(type);

      // Non-leaping Camel moves (slide 3, then step 1 sideways)
      MoveCapability move;
      MovePathInfo movePath;

      // Vertical moves with side-steps
      move = MoveCapability.Step(new Direction(3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, 1) });
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

  [PieceType("Great Camel", "APMW Custom Pieces")]
  public class GreatCamel : PieceType
  {
    public GreatCamel(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Great Camel", name, notation, midgameValue, endgameValue, preferredImageName == null ? "KnightGeneral" : preferredImageName)
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

  [PieceType("Dragon Cannon", "APMW Custom Pieces")]
  public class DragonCannon : PieceType
  {
    public DragonCannon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Dragon Cannon", name, notation, midgameValue, endgameValue, preferredImageName == null ? "DragonHorse" : preferredImageName)
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
      base("Mameluk", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Wildebeest" : preferredImageName)
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
      Wazir.AddMoves(type);
      CloseRibbon.AddMoves(type);

      // Enhanced Ribbon moves - can move 1 further step before rotating
      type.Slide(new Direction(1, 1), maxSteps: 2);
      type.Slide(new Direction(1, -1), maxSteps: 2);
      type.Slide(new Direction(-1, 1), maxSteps: 2);
      type.Slide(new Direction(-1, -1), maxSteps: 2);

      // Add the bent moves like the regular Ribbon
      MoveCapability move;
      MovePathInfo movePath;

      // Positive X
      move = MoveCapability.Step(new Direction(4, 0));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 1), new Direction(1, 1), new Direction(1, -1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(1, -1), new Direction(1, -1), new Direction(1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative X
      move = MoveCapability.Step(new Direction(-4, 0));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 1), new Direction(-1, 1), new Direction(-1, -1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, -1), new Direction(-1, -1), new Direction(-1, 1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y
      move = MoveCapability.Step(new Direction(0, 4));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 1), new Direction(1, 1), new Direction(-1, 1), new Direction(-1, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 1), new Direction(-1, 1), new Direction(1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y
      move = MoveCapability.Step(new Direction(0, -4));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, -1), new Direction(1, -1), new Direction(-1, -1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, -1), new Direction(-1, -1), new Direction(1, -1), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Big Y, Positive X
      move = MoveCapability.Step(new Direction(3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(1, 1), new Direction(1, 1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(1, -1), new Direction(1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Big Y, Negative X
      move = MoveCapability.Step(new Direction(3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(1, 1), new Direction(1, -1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(1, -1), new Direction(1, -1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Tiny Y, Positive X 
      move = MoveCapability.Step(new Direction(-3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(-1, -1), new Direction(-1, 1), new Direction(-1, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 1), new Direction(-1, 1), new Direction(-1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
      
      // Tiny Y, Negative X
      move = MoveCapability.Step(new Direction(-3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(-1, 1), new Direction(-1, -1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, -1), new Direction(-1, -1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y, Big X
      move = MoveCapability.Step(new Direction(1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(-1, 1), new Direction(1, 1), new Direction(1, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 1), new Direction(1, 1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y, Tiny X
      move = MoveCapability.Step(new Direction(1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(-1, -1), new Direction(1, -1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(1, -1), new Direction(1, -1), new Direction(-1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y, Big X
      move = MoveCapability.Step(new Direction(-1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(1, 1), new Direction(-1, 1), new Direction(-1, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 1), new Direction(-1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y, Tiny X
      move = MoveCapability.Step(new Direction(-1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { 
        new Direction(1, -1), new Direction(-1, -1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, -1), new Direction(-1, -1), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }
}
