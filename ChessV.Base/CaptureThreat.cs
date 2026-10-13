using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessV
{
  public sealed class CapturePath
  {
    public IReadOnlyList<int> Squares { get; }
    public int ScreenSquare { get; }
    public bool IsLeap { get; }
    public bool WrapsFiles { get; }

    public CapturePath(IEnumerable<int> squares, int screenSquare = -1,
      bool isLeap = false, bool wrapsFiles = false)
    {
      Squares = Array.AsReadOnly(squares.ToArray());
      ScreenSquare = screenSquare;
      IsLeap = isLeap;
      WrapsFiles = wrapsFiles;
    }
  }

  public sealed class CaptureThreat
  {
    public Piece Attacker { get; }
    public int SourceSquare { get; }
    public int TargetSquare { get; }
    public CapturePath Path { get; }

    public CaptureThreat(Piece attacker, int targetSquare, CapturePath path)
    {
      Attacker = attacker;
      SourceSquare = attacker.Square;
      TargetSquare = targetSquare;
      Path = path;
    }
  }
}
