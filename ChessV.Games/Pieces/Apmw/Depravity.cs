﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Herald", "APMW Custom Pieces")]
  public class Herald : PieceType
  {
    public Herald(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Herald", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Cat" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Bishop.AddMoves(type);
      Camel.AddMoves(type);
      //Pawn move
      type.Step(new Direction(1, 0));
      //Tribbabah slide
      type.Slide(new Direction(0, 3));
      type.Slide(new Direction(0, -3));
      type.Slide(new Direction(3, 0));
      type.Slide(new Direction(-3, 0));
    }
  }

  [PieceType("Queennon", "APMW Custom Pieces")]
  public class Queennon : PieceType
  {
    public Queennon(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Queennon", name, notation, midgameValue, endgameValue, preferredImageName == null ? "Queennon" : preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ferz.AddMoves(type);
      Wazir.AddMoves(type);
      Cannon.AddMoves(type);
      Vao.AddMoves(type);
      // knight cannon moves
      type.CannonMove(new Direction(1, 2));
      type.CannonMove(new Direction(2, 1));
      type.CannonMove(new Direction(2, -1));
      type.CannonMove(new Direction(1, -2));
      type.CannonMove(new Direction(-1, -2));
      type.CannonMove(new Direction(-2, -1));
      type.CannonMove(new Direction(-2, 1));
      type.CannonMove(new Direction(-1, 2));
    }
  }
}
