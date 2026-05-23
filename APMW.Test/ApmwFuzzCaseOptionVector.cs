using System;
using System.Globalization;

namespace ChessV.Test
{
    internal static class ApmwFuzzCaseOptionVector
    {
        private static readonly Lazy<ApmwFuzzOptionSpace> DefaultSpace =
            new Lazy<ApmwFuzzOptionSpace>(ApmwFuzzOptionSpace.CreateDefault);

        public static ApmwFuzzOptionSpace DefaultOptionSpace
        {
            get { return DefaultSpace.Value; }
        }

        public static string BuildCanonicalKey(ApmwFuzzCase fuzzCase)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            return DefaultOptionSpace.BuildCanonicalKey(fuzzCase) +
                "|pocket_seed=" + IntKey(fuzzCase.PocketSeed) +
                "|pawn_seed=" + IntKey(fuzzCase.PawnSeed) +
                "|minor_seed=" + IntKey(fuzzCase.MinorSeed) +
                "|major_seed=" + IntKey(fuzzCase.MajorSeed) +
                "|queen_seed=" + IntKey(fuzzCase.QueenSeed) +
                "|deterministic_chaos_seed=" + NullableIntKey(fuzzCase.DeterministicChaosSeed);
        }

        private static string IntKey(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string NullableIntKey(int? value)
        {
            return value.HasValue ? IntKey(value.Value) : "null";
        }
    }
}
