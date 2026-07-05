using ChessV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChessV.PieceAnalysis.Test
{
    internal static class PieceAnalysisTestHelpers
    {
        private static readonly PieceCatalog Catalog = new PieceCatalog(TierReferenceData.Entries);

        public static ConstructedPiece ConstructKnownPiece(string pieceName)
        {
            Assert.IsTrue(Catalog.TryResolvePieceType(pieceName, out var pieceType), $"Could not resolve piece '{pieceName}'.");
            string ignoredTierHintMessage;
            return Catalog.ConstructPiece(pieceType, null, out ignoredTierHintMessage);
        }

        public static IReadOnlyList<MobilityStatistics> AnalyzeReferencePieces(int files = 8, int ranks = 8)
        {
            var analyzer = new MobilityAnalyzer();
            return analyzer.Analyze(
                TierReferenceData.Entries.Values
                    .OrderBy(entry => entry.PieceTypeName, StringComparer.Ordinal)
                    .Select(entry => ConstructKnownPiece(entry.PieceTypeName))
                    .ToList(),
                files,
                ranks,
                MobilityAnalyzer.DefaultDensityPercents);
        }

        public static MobilityStatistics CreateStatistics(
            string requestedName,
            PieceTier? tier,
            double mobilityAtAllDensities,
            double averageDirectionsAttacked,
            double averageSafeChecks,
            int midgameValue = 300,
            int endgameValue = 300,
            string pieceTypeName = "Knight")
        {
            return CreateStatistics(
                requestedName,
                tier,
                MobilityAnalyzer.DefaultDensityPercents.ToDictionary(percent => percent, _ => mobilityAtAllDensities),
                averageDirectionsAttacked,
                averageSafeChecks,
                midgameValue,
                endgameValue,
                pieceTypeName);
        }

        public static MobilityStatistics CreateStatistics(
            string requestedName,
            PieceTier? tier,
            IReadOnlyDictionary<int, double> mobilityByDensityPercent,
            double averageDirectionsAttacked,
            double averageSafeChecks,
            int midgameValue = 300,
            int endgameValue = 300,
            string pieceTypeName = "Knight")
        {
            var constructedPiece = CreateConstructedPiece(
                requestedName,
                tier,
                midgameValue,
                endgameValue,
                pieceTypeName,
                "T");

            return new MobilityStatistics(
                constructedPiece,
                averageDirectionsAttacked,
                averageSafeChecks,
                new Dictionary<int, double>(mobilityByDensityPercent));
        }

        private static ConstructedPiece CreateConstructedPiece(
            string requestedName,
            PieceTier? tier,
            int midgameValue,
            int endgameValue,
            string pieceTypeName,
            string notation)
        {
            Assert.IsTrue(Catalog.TryResolvePieceType(pieceTypeName, out var pieceType), $"Could not resolve piece type '{pieceTypeName}'.");

            var constructor = pieceType.GetConstructors()
                .FirstOrDefault(ci => MatchesParameterTypes(ci, typeof(string), typeof(string), typeof(int), typeof(int)))
                ?? pieceType.GetConstructors()
                    .FirstOrDefault(ci => MatchesParameterTypes(ci, typeof(string), typeof(string), typeof(int), typeof(int), typeof(string)));

            Assert.IsNotNull(constructor, $"Could not find a supported constructor for '{pieceTypeName}'.");

            object[] arguments = constructor.GetParameters().Length == 4
                ? new object[] { requestedName, notation, midgameValue, endgameValue }
                : new object[] { requestedName, notation, midgameValue, endgameValue, string.Empty };

            var piece = (PieceType)constructor.Invoke(arguments);
            return new ConstructedPiece(requestedName, pieceType, piece, tier, "Test", midgameValue, endgameValue, notation);
        }

        private static bool MatchesParameterTypes(ConstructorInfo constructor, params Type[] expectedTypes)
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != expectedTypes.Length)
                return false;

            for (var index = 0; index < parameters.Length; index++)
            {
                if (parameters[index].ParameterType != expectedTypes[index])
                    return false;
            }

            return true;
        }
    }
}
