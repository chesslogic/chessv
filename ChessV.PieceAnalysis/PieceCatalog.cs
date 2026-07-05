using ChessV;
using ChessV.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChessV.PieceAnalysis
{
  public sealed class ConstructedPiece
  {
    public ConstructedPiece(
      string requestedName,
      Type pieceType,
      PieceType piece,
      PieceTier? tier,
      string tierSource,
      int midgameValue,
      int endgameValue,
      string notation)
    {
      RequestedName = requestedName;
      PieceType = pieceType;
      Piece = piece;
      Tier = tier;
      TierSource = tierSource;
      MidgameValue = midgameValue;
      EndgameValue = endgameValue;
      Notation = notation;
    }

    public string RequestedName { get; private set; }
    public Type PieceType { get; private set; }
    public PieceType Piece { get; private set; }
    public PieceTier? Tier { get; private set; }
    public string TierSource { get; private set; }
    public int MidgameValue { get; private set; }
    public int EndgameValue { get; private set; }
    public string Notation { get; private set; }
  }

  public sealed class PieceConstructionFailure
  {
    public PieceConstructionFailure(string requestedName, Type pieceType, string reason)
    {
      RequestedName = requestedName;
      PieceType = pieceType;
      Reason = reason;
    }

    public string RequestedName { get; private set; }
    public Type PieceType { get; private set; }
    public string Reason { get; private set; }
  }

  public sealed class PieceCatalog
  {
    private readonly Assembly gamesAssembly;
    private readonly IReadOnlyDictionary<string, TierReferenceEntry> referenceEntries;
    private readonly IReadOnlyDictionary<string, Type> constructibleTypesByName;

    public PieceCatalog(IReadOnlyDictionary<string, TierReferenceEntry> referenceEntries)
    {
      this.referenceEntries = referenceEntries;
      gamesAssembly = typeof(Chess).Assembly;
      constructibleTypesByName = DiscoverConstructiblePieceTypes()
        .ToDictionary(type => type.Name, type => type, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetDefaultRosterPieceNames()
    {
      return referenceEntries.Keys.OrderBy(name => name, StringComparer.Ordinal).ToList();
    }

    public IReadOnlyList<Type> GetAllConstructiblePieceTypes()
    {
      return constructibleTypesByName.Values.OrderBy(type => type.Name, StringComparer.Ordinal).ToList();
    }

    public bool TryResolvePieceType(string name, out Type pieceType)
    {
      return constructibleTypesByName.TryGetValue(name, out pieceType);
    }

    public ConstructedPiece ConstructPiece(Type pieceType, PieceTier? explicitTierHint, out string ignoredTierHintMessage)
    {
      ignoredTierHintMessage = null;

      TierReferenceEntry entry;
      bool hasReferenceEntry = referenceEntries.TryGetValue(pieceType.Name, out entry);
      PieceTier? tier = null;
      string tierSource = null;
      int midgameValue = 0;
      int endgameValue = 0;
      string notation = CreateFallbackNotation(pieceType);

      if (hasReferenceEntry)
      {
        tier = entry.Tier;
        tierSource = entry.TierSource;
        midgameValue = entry.MidgameValue;
        endgameValue = entry.EndgameValue;
        notation = entry.Notation;
        if (explicitTierHint.HasValue)
          ignoredTierHintMessage = string.Format("Ignoring tier hint '{0}' for known piece '{1}'.", explicitTierHint.Value, pieceType.Name);
      }
      else if (explicitTierHint.HasValue)
      {
        tier = explicitTierHint.Value;
        tierSource = "CliHint";
      }

      PieceType piece = InstantiatePiece(pieceType, midgameValue, endgameValue, notation);
      string outputNotation = piece.Notation != null && piece.Notation.Length > 0 ? piece.Notation[0] : notation;
      return new ConstructedPiece(pieceType.Name, pieceType, piece, tier, tierSource, midgameValue, endgameValue, outputNotation);
    }

    private IEnumerable<Type> DiscoverConstructiblePieceTypes()
    {
      return gamesAssembly
        .GetTypes()
        .Where(type =>
          typeof(PieceType).IsAssignableFrom(type) &&
          type.IsClass &&
          !type.IsAbstract &&
          HasSupportedConstructor(type));
    }

    private static bool HasSupportedConstructor(Type pieceType)
    {
      return FindSupportedConstructor(pieceType) != null;
    }

    private static ConstructorInfo FindSupportedConstructor(Type pieceType)
    {
      ConstructorInfo[] constructors = pieceType.GetConstructors();
      ConstructorInfo constructor =
        constructors.FirstOrDefault(ci => MatchesParameterTypes(ci, typeof(string), typeof(string), typeof(int), typeof(int)));
      if (constructor != null)
        return constructor;

      return constructors.FirstOrDefault(ci => MatchesParameterTypes(ci, typeof(string), typeof(string), typeof(int), typeof(int), typeof(string)));
    }

    private static bool MatchesParameterTypes(ConstructorInfo constructor, params Type[] expectedTypes)
    {
      ParameterInfo[] parameters = constructor.GetParameters();
      if (parameters.Length != expectedTypes.Length)
        return false;

      for (int index = 0; index < parameters.Length; index++)
      {
        if (parameters[index].ParameterType != expectedTypes[index])
          return false;
      }

      return true;
    }

    private static PieceType InstantiatePiece(Type pieceType, int midgameValue, int endgameValue, string notation)
    {
      ConstructorInfo constructor = FindSupportedConstructor(pieceType);
      if (constructor == null)
        throw new InvalidOperationException(string.Format("No supported constructor was found for {0}.", pieceType.FullName));

      object[] arguments;
      if (constructor.GetParameters().Length == 4)
      {
        arguments = new object[] { pieceType.Name, notation, midgameValue, endgameValue };
      }
      else
      {
        arguments = new object[] { pieceType.Name, notation, midgameValue, endgameValue, string.Empty };
      }

      return (PieceType)constructor.Invoke(arguments);
    }

    private static string CreateFallbackNotation(Type pieceType)
    {
      return string.IsNullOrEmpty(pieceType.Name) ? "?" : pieceType.Name.Substring(0, 1).ToUpperInvariant();
    }
  }
}
