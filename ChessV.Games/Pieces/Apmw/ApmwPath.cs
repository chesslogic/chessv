using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Ribbon", "APMW Custom Pieces")]
  public class Ribbon : PieceType
  {
    public Ribbon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Butterfly") :
      base("Ribbon", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ferz.AddMoves(type);

      type.Step(new Direction(1, 1));
      type.Step(new Direction(1, -1));
      type.Step(new Direction(-1, 1));
      type.Step(new Direction(-1, -1));

      // Each location we step to allows us to move 1 or 2 squares in both perpendicular directions.

      // Positive X
      MoveCapability move = MoveCapability.Step(new Direction(2, 0));
      MovePathInfo movePath = new MovePathInfo();
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
    public Petal(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Bird") :
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
      type.Slide(new Direction(1, 1), maxSteps: 3);
      type.Slide(new Direction(1, -1), maxSteps: 3);
      type.Slide(new Direction(-1, 1), maxSteps: 3);
      type.Slide(new Direction(-1, -1), maxSteps: 3);

      // Positive X, Positive Y
      MoveCapability move = MoveCapability.Step(new Direction(3, 3));
      MovePathInfo movePath = new MovePathInfo();
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

      move = MoveCapability.Step(new Direction(3, 1));
      movePath = new MovePathInfo();
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

      // Shorter moves for positive X, negative Y
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

      // Shorter moves for negative X, negative Y
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

      // Shorter moves for negative X, positive Y
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
    }
  }
}
