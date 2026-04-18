/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG

This file is part of ChessV.  ChessV is free software; you can redistribute
it and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation, either version 3 of the License,
or (at your option) any later version.

ChessV is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
more details; the file 'COPYING' contains the License text, but if for
some reason you need a copy, please visit <http://www.gnu.org/licenses/>.

****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace ChessV
{
  /// <summary>
  /// Thread-local stack of move-generation context frames. Each frame
  /// captures the rule, ply and cursor state at the point a move began
  /// being constructed (BeginMoveAdd / AddMove / AddCapture). When a
  /// crash escapes from PerformPickup or Board.ClearSquare the formatted
  /// stack is appended to the exception message so the failing call site
  /// can be diagnosed without a custom build.
  ///
  /// Design notes:
  ///   - Storage is [ThreadStatic] so each search/eval thread gets its
  ///     own stack and there is no cross-thread interference.
  ///   - Frames are value types stored in a per-thread List; push/pop is
  ///     amortized O(1) and allocates only when the list grows.
  ///   - Rule name is set by Game.GenerateSpecialMoves around each rule
  ///     invocation (no Reflection-based stack walking).
  ///   - Always on; no conditional compilation. The hot path in the
  ///     no-throw case is a List.Add and a List.RemoveAt.
  /// </summary>
  public static class MoveGenerationContext
  {
    public struct Frame
    {
      public string RuleName;
      public int Ply;
      public MoveType MoveType;
      public int FromSquare;
      public int ToSquare;
      public int PickupCursor;
      public int DropCursor;
      public int MoveCursor;
      public ulong BoardHash;
    }

    [ThreadStatic] private static List<Frame> _stack;
    [ThreadStatic] private static string _currentRule;

    private static List<Frame> EnsureStack()
    {
      if (_stack == null)
        _stack = new List<Frame>(16);
      return _stack;
    }

    /// <summary>
    /// Set by Game.GenerateSpecialMoves on entry to each Rule. Cleared on
    /// exit. Frames pushed while this is non-null inherit it as their
    /// rule label unless an explicit override is provided.
    /// </summary>
    public static void SetCurrentRule(string ruleName)
    {
      _currentRule = ruleName;
    }

    public static string CurrentRule { get { return _currentRule; } }

    public static int Depth { get { return _stack == null ? 0 : _stack.Count; } }

    /// <summary>
    /// Push a frame describing a move-generation operation that is about
    /// to run a Make/Unmake cycle. Returns the new depth so callers can
    /// assert/restore in unusual control flow.
    /// </summary>
    public static int Push(
      string ruleNameOverride,
      int ply,
      MoveType moveType,
      int fromSquare,
      int toSquare,
      int pickupCursor,
      int dropCursor,
      int moveCursor,
      ulong boardHash)
    {
      var s = EnsureStack();
      Frame f;
      f.RuleName = ruleNameOverride ?? _currentRule ?? "(root)";
      f.Ply = ply;
      f.MoveType = moveType;
      f.FromSquare = fromSquare;
      f.ToSquare = toSquare;
      f.PickupCursor = pickupCursor;
      f.DropCursor = dropCursor;
      f.MoveCursor = moveCursor;
      f.BoardHash = boardHash;
      s.Add(f);
      return s.Count;
    }

    public static void Pop()
    {
      if (_stack != null && _stack.Count > 0)
        _stack.RemoveAt(_stack.Count - 1);
    }

    /// <summary>
    /// Test/recovery helper. Drops every frame and clears the current
    /// rule pointer. Called from tests; should not be needed by product
    /// code because every Push has a matching Pop in a finally block.
    /// </summary>
    public static void Reset()
    {
      if (_stack != null)
        _stack.Clear();
      _currentRule = null;
    }

    /// <summary>
    /// Format the entire stack for inclusion in an exception message.
    /// Returns the empty string when the stack is empty so callers can
    /// safely concatenate without producing trailing whitespace.
    /// </summary>
    public static string FormatContextStack()
    {
      if (_stack == null || _stack.Count == 0)
        return string.Empty;

      var sb = new StringBuilder();
      sb.AppendLine();
      sb.AppendLine("=== Move Generation Context Stack ===");
      sb.AppendLine($"Depth: {_stack.Count}   CurrentRule: {_currentRule ?? "<none>"}");
      // Innermost frame first so the most-relevant context is read at
      // the top of the dump (matches the existing _refs report style).
      for (int i = _stack.Count - 1; i >= 0; i--)
      {
        var f = _stack[i];
        sb.AppendLine(
          $"  [{i}] rule={f.RuleName} ply={f.Ply} type={f.MoveType} " +
          $"from={f.FromSquare} to={f.ToSquare} " +
          $"pickupCursor={f.PickupCursor} dropCursor={f.DropCursor} moveCursor={f.MoveCursor} " +
          $"boardHash=0x{f.BoardHash:X16}");
      }
      sb.AppendLine("=== End Move Generation Context Stack ===");
      return sb.ToString();
    }
  }
}
