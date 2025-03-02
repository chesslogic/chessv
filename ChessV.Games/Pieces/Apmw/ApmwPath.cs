using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Miracle", "APMW Custom Pieces")]
  public class Miracle : PieceType
  {
    public Miracle(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Butterfly") :
      base("Miracle", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ribbon.AddMoves(type);
      Petal.AddMoves(type);
    }
  }
  
  [PieceType("Unbound Ribbon", "APMW Custom Pieces")]
  public class UnboundRibbon : PieceType
  {
    public UnboundRibbon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "SquirrelGeneral") :
      base("Unbound Ribbon", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ribbon.AddMoves(type);
      Wazir.AddMoves(type);
    }
  }

  [PieceType("Gardener", "APMW Custom Pieces")]
  public class Gardener : PieceType
  {
    public Gardener(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "WateringCan") :
      base("Gardener", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Elephant.AddMoves(type);

      // Move 1 square inward from each elephant space
      MoveCapability move = MoveCapability.StepMoveOnly(new Direction(2, 1));
      MovePathInfo movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, 2), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(1, 2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, 2), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(1, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, 2), new Direction(-1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-2, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, 2), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-1, 2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, 2), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-1, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, 2), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(2, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, -2), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(1, -2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, -2), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(1, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(2, -2), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-2, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, -2), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-1, -2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, -2), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.StepMoveOnly(new Direction(-1, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-2, -2), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }

  public class CloseRibbon : PieceType
  {
    public CloseRibbon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "CloseRibbon") :
      base("Close Ribbon", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    /// <summary>
    /// Implements the first step of the "bent path" movement for Ribbon (minor) and for Grazer (jack).
    /// </summary>
    /// <param name="type"></param>
    public static new void AddMoves(PieceType type)
    {
      // Each location we step to allows us to move 1 or 2 squares in both perpendicular directions.

      MoveCapability move;
      MovePathInfo movePath;

      // Positive X
      move = MoveCapability.Step(new Direction(2, 0));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() { new Direction(1, -1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative X
      move = MoveCapability.Step(new Direction(-2, 0));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, 1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() { new Direction(-1, -1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y
      move = MoveCapability.Step(new Direction(0, 2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 1), new Direction(-1, 1) });
      movePath.AddPath(new List<Direction>() { new Direction(-1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y
      move = MoveCapability.Step(new Direction(0, -2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, -1), new Direction(-1, -1) });
      movePath.AddPath(new List<Direction>() { new Direction(-1, -1), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }

  [PieceType("Ribbon", "APMW Custom Pieces")]
  public class Ribbon : PieceType
  {
    public Ribbon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Ribbon") :
      base("Ribbon", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ferz.AddMoves(type);
      CloseRibbon.AddMoves(type);

      MoveCapability move;
      MovePathInfo movePath;

      // Big Y, Positive X
      move = MoveCapability.Step(new Direction(3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, -1), new Direction(1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Big Y, Negative X
      move = MoveCapability.Step(new Direction(3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 1), new Direction(1, -1), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Tiny Y, Positive X 
      move = MoveCapability.Step(new Direction(-3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, -1), new Direction(-1, 1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
      
      // Tiny Y, Negative X
      move = MoveCapability.Step(new Direction(-3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, 1), new Direction(-1, -1), new Direction(-1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y, Big X
      move = MoveCapability.Step(new Direction(1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, 1), new Direction(1, 1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive Y, Tiny X
      move = MoveCapability.Step(new Direction(1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(-1, -1), new Direction(1, -1), new Direction(1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y, Big X
      move = MoveCapability.Step(new Direction(-1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 1), new Direction(-1, 1), new Direction(-1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative Y, Tiny X
      move = MoveCapability.Step(new Direction(-1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, -1), new Direction(-1, -1), new Direction(-1, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }


  [PieceType("Petal", "APMW Custom Pieces")]
  public class Petal : PieceType
  {
    public Petal(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Flower") :
      base("Petal", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      // Moves horizontally and vertically 1, 2, and 3 squares, then rotates 90 degrees and moves 1, 2, or 3 squares.
      // It can't move or capture diagonally, including 1,1 - it has to go all the way down to 3,0 and then 3,1 or 3,2 or 3,3.
      // It also moves along these as paths, meaning it's not a leaper. It's like a bent rook.

      // A wazir moves 1 square, a dabbabah moves 2 squares, and a tribbabah moves 3 squares - but it's not a leaper
      type.Slide(new Direction(1, 0), maxSteps: 3);
      type.Slide(new Direction(-1, 0), maxSteps: 3);
      type.Slide(new Direction(0, 1), maxSteps: 3);
      type.Slide(new Direction(0, -1), maxSteps: 3);

      // First Y, Then X
      MoveCapability move = MoveCapability.Step(new Direction(3, 1));
      MovePathInfo movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(3, 2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, 1), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // First Y, Then -X
      move = MoveCapability.Step(new Direction(3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(3, -2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, -1), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // First -Y, Then -X
      move = MoveCapability.Step(new Direction(-3, -1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-3, -2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, -1), new Direction(0, -1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // First -Y, Then X
      move = MoveCapability.Step(new Direction(-3, 1));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-3, 2));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, 1), new Direction(0, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Second Y, First X
      move = MoveCapability.Step(new Direction(2, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(1, 0), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Second Y, First -X
      move = MoveCapability.Step(new Direction(2, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(1, 0), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Second -Y, First -X
      move = MoveCapability.Step(new Direction(-2, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(-1, 0), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-1, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Second -Y, First X
      move = MoveCapability.Step(new Direction(-2, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(-1, 0), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      move = MoveCapability.Step(new Direction(-1, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive X, Positive Y
      move = MoveCapability.Step(new Direction(3, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, 1), new Direction(0, 1), new Direction(0, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(1, 0), new Direction(1, 0), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Positive X, Negative Y
      move = MoveCapability.Step(new Direction(3, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(1, 0), new Direction(1, 0), new Direction(1, 0), new Direction(0, -1), new Direction(0, -1), new Direction(0, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(1, 0), new Direction(1, 0), new Direction(1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative X, Negative Y
      move = MoveCapability.Step(new Direction(-3, -3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, -1), new Direction(0, -1), new Direction(0, -1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(0, -1), new Direction(0, -1), new Direction(0, -1), new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);

      // Negative X, Positive Y
      move = MoveCapability.Step(new Direction(-3, 3));
      movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() {
        new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0), new Direction(0, 1), new Direction(0, 1), new Direction(0, 1) });
      movePath.AddPath(new List<Direction>() {
        new Direction(0, 1), new Direction(0, 1), new Direction(0, 1), new Direction(-1, 0), new Direction(-1, 0), new Direction(-1, 0) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }
}
