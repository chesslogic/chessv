using ChessV;
using ChessV.Games;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.PieceAnalysis
{
  public enum PieceTier
  {
    Weak,
    Pawn,
    Minor,
    Major,
    Jack,
    Queen,
    Amazon
  }

  public sealed class TierReferenceEntry
  {
    public TierReferenceEntry(string pieceTypeName, string notation, int midgameValue, int endgameValue, PieceTier tier, string tierSource)
    {
      PieceTypeName = pieceTypeName;
      Notation = notation;
      MidgameValue = midgameValue;
      EndgameValue = endgameValue;
      Tier = tier;
      TierSource = tierSource;
    }

    public string PieceTypeName { get; private set; }
    public string Notation { get; private set; }
    public int MidgameValue { get; private set; }
    public int EndgameValue { get; private set; }
    public PieceTier Tier { get; private set; }
    public string TierSource { get; private set; }
  }

  public static class TierReferenceData
  {
    // Mirrors APMW.Client\ItemGeneration.cs ItemGenerationValues.
    private static readonly IReadOnlyDictionary<PieceTier, int> tierMaterialValues =
      new Dictionary<PieceTier, int>
      {
        { PieceTier.Weak, 75 },
        { PieceTier.Pawn, 100 },
        { PieceTier.Minor, 300 },
        { PieceTier.Major, 485 },
        { PieceTier.Jack, 700 },
        { PieceTier.Queen, 900 },
        { PieceTier.Amazon, 1300 }
      };

    private static readonly Lazy<IReadOnlyDictionary<string, TierReferenceEntry>> entries =
      new Lazy<IReadOnlyDictionary<string, TierReferenceEntry>>(BuildEntries);

    public static IReadOnlyDictionary<string, TierReferenceEntry> Entries
    {
      get { return entries.Value; }
    }

    public static IReadOnlyDictionary<PieceTier, int> TierMaterialValues
    {
      get { return tierMaterialValues; }
    }

    public static bool TryGetEntry(string pieceTypeName, out TierReferenceEntry entry)
    {
      return Entries.TryGetValue(pieceTypeName, out entry);
    }

    public static bool TryParseTier(string rawTier, out PieceTier tier)
    {
      return Enum.TryParse(rawTier, true, out tier);
    }

    public static IReadOnlyList<TierReferenceEntry> GetEntriesForTier(PieceTier tier)
    {
      return Entries.Values
        .Where(entry => entry.Tier == tier)
        .OrderBy(entry => entry.PieceTypeName, StringComparer.Ordinal)
        .ToList();
    }

    private static IReadOnlyDictionary<string, TierReferenceEntry> BuildEntries()
    {
      var game = new ApmwChessGame();
      game.earlyPopulatePieceTypes();

      var result = new Dictionary<string, TierReferenceEntry>(StringComparer.OrdinalIgnoreCase);
      AddEntries(result, game.Minors, PieceTier.Minor);
      AddEntries(result, game.Majors, PieceTier.Major);
      AddEntries(result, game.Jacks, PieceTier.Jack);
      AddEntries(result, game.Queens, PieceTier.Queen);
      AddEntries(result, game.Amazons, PieceTier.Amazon);
      return result;
    }

    private static void AddEntries(
      IDictionary<string, TierReferenceEntry> result,
      IEnumerable<PieceType> pieces,
      PieceTier tier)
    {
      foreach (PieceType piece in pieces)
      {
        string pieceTypeName = piece.GetType().Name;
        result[pieceTypeName] = new TierReferenceEntry(
          pieceTypeName,
          piece.Notation != null && piece.Notation.Length > 0 ? piece.Notation[0] : string.Empty,
          piece.MidgameValue,
          piece.EndgameValue,
          tier,
          "ApmwChess");
      }
    }
  }
}
