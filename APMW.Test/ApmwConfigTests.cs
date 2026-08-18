using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwConfigTests
    {
        [TestMethod]
        public void Goal_OrdinalsAreAppendOnlyAndValueFourParsesFromSlotData()
        {
            Assert.AreEqual(0, (int)Goal.Single);
            Assert.AreEqual(1, (int)Goal.OrderedProgressive);
            Assert.AreEqual(2, (int)Goal.Progressive);
            Assert.AreEqual(3, (int)Goal.Super);
            Assert.AreEqual(4, (int)Goal.OrderedProgressive6x8);

            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                ["goal"] = 4,
            });

            Assert.AreEqual(Goal.OrderedProgressive6x8, config.Goal);
            Assert.AreEqual(4, Convert.ToInt32(config.SlotData["goal"]));
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(5)]
        public void Goal_UnsupportedOrdinalIsRejectedInsteadOfFallingBack(int value)
        {
            var config = new ApmwConfig();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                config.Instantiate(new Dictionary<string, object>
                {
                    ["goal"] = value,
                }));
        }

        private sealed class UnexpectedFailureConvertible : IConvertible
        {
            public TypeCode GetTypeCode() { throw new ApplicationException("unexpected conversion failure"); }
            public bool ToBoolean(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public byte ToByte(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public char ToChar(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public DateTime ToDateTime(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public decimal ToDecimal(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public double ToDouble(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public short ToInt16(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public int ToInt32(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public long ToInt64(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public sbyte ToSByte(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public float ToSingle(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public string ToString(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public object ToType(Type conversionType, IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public ushort ToUInt16(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public uint ToUInt32(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
            public ulong ToUInt64(IFormatProvider provider) { throw new ApplicationException("unexpected conversion failure"); }
        }

        private sealed class AlwaysZeroRandom : Random
        {
            public override int Next(int maxValue)
            {
                return 0;
            }
        }

        [TestMethod]
        public void Seed_UsesDeterministicChaosSeedFromSlotData()
        {
            const int chaosSeed = 123456;
            var slotData = new Dictionary<string, object>
            {
                ["piece_types"] = (int)PieceTypes.Chaos,
                ["piece_locations"] = (int)PieceLocations.Chaos,
                [ApmwConfig.DeterministicChaosSeedSlotKeyForTest] = chaosSeed,
            };

            var config = new ApmwConfig();
            config.Instantiate(slotData);
            config.seed();

            var expected = new Random(chaosSeed);
            var firstStageSeeds = CaptureSeeds(config);
            CollectionAssert.AreEqual(
                new[]
                {
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                },
                firstStageSeeds);

            config.seed();
            CollectionAssert.AreEqual(firstStageSeeds, CaptureSeeds(config));
        }

        [TestMethod]
        public void Distribute_UsesProvidedRandom()
        {
            var result = ApmwConfig.distribute(
                new List<string> { "A", "B", "C" },
                3,
                new AlwaysZeroRandom());

            CollectionAssert.AreEqual(
                new[] { "B", "C", "A" },
                Enumerable.Range(0, 3).Select(index => result[index]).ToArray());
        }

        [DataTestMethod]
        [DataRow(FairyPawnUpgrades.Off, "new-pawn,more-pawn,better-pawn,major-to-queen,pool-pawn-upgrade,pawn-to-minor,pawn-to-major,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Pool, "new-pawn,pool-pawn-upgrade,more-pawn,better-pawn,major-to-queen,pawn-to-minor,pawn-to-major,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Max, "new-pawn,better-pawn,more-pawn,major-to-queen,pool-pawn-upgrade,pawn-to-minor,pawn-to-major,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.SuperMax, "new-pawn,better-pawn,more-pawn,major-to-queen,pool-pawn-upgrade,pawn-to-minor,pawn-to-major,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Configure, "new-pawn,more-pawn,better-pawn,major-to-queen,pool-pawn-upgrade,pawn-to-minor,pawn-to-major,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        public void Instantiate_DerivesPieceUpgradePreferencesFromLegacyMode(
            FairyPawnUpgrades legacyMode,
            string expectedCsv)
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)legacyMode,
            });

            CollectionAssert.AreEqual(
                expectedCsv.Split(','),
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_CurrentWorldShapeMapsThreeToConfigureAndReadsCanonicalRatio()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 3,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("more-pawn", "new-pawn"),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] = JObject.FromObject(new Dictionary<string, double>
                {
                    [ApmwConstants.PieceUpgradeActions.MorePawn] = 5,
                }),
            });

            Assert.AreEqual(FairyPawnUpgrades.Configure, config.PawnUpgrades);
            Assert.IsFalse(config.UsesSuperMaxPawnGuarantee);
            Assert.AreEqual(5.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.MorePawn].Proportion);
        }

        [TestMethod]
        public void Instantiate_CurrentConfigureWithoutOptionalRatioUsesResolvedPreferencesAsSchemaMarker()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 3,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("more-pawn", "new-pawn"),
            });

            Assert.AreEqual(FairyPawnUpgrades.Configure, config.PawnUpgrades);
            Assert.IsFalse(config.UsesSuperMaxPawnGuarantee);
            Assert.IsFalse(
                config.UsesCurrentContract,
                "A markerless preference list selects parsing compatibility, not the v2 item boundary.");
            CollectionAssert.AreEqual(
                new[]
                {
                    ApmwConstants.PieceUpgradeActions.MorePawn,
                    ApmwConstants.PieceUpgradeActions.NewPawn,
                    ApmwConstants.PieceUpgradeActions.BetterPawn,
                    ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
                    ApmwConstants.PieceUpgradeActions.PawnToMinor,
                    ApmwConstants.PieceUpgradeActions.PawnToMajor,
                    ApmwConstants.PieceUpgradeActions.MinorToMajor,
                    ApmwConstants.PieceUpgradeActions.MajorToJack,
                    ApmwConstants.PieceUpgradeActions.MinorToJack,
                    ApmwConstants.PieceUpgradeActions.MajorToQueen,
                    ApmwConstants.PieceUpgradeActions.JackToQueen,
                    ApmwConstants.PieceUpgradeActions.QueenToAmazon,
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_LegacyWorldShapeMapsThreeToSuperMaxAndReadsHistoricalProportion()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 3,
                [ApmwConstants.LegacySlotKeyPieceUpgradeProportion] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 4,
                    }),
            });

            Assert.AreEqual(FairyPawnUpgrades.SuperMax, config.PawnUpgrades);
            Assert.IsTrue(config.UsesSuperMaxPawnGuarantee);
            Assert.AreEqual(4.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.BetterPawn].Proportion);
        }

        [TestMethod]
        public void Instantiate_LegacyConfigureValueFourRemainsSupported()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 4,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("more-pawn"),
            });

            Assert.AreEqual(FairyPawnUpgrades.Configure, config.PawnUpgrades);
            Assert.IsFalse(config.UsesSuperMaxPawnGuarantee);
            Assert.IsTrue(config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.MorePawn));
        }

        [DataTestMethod]
        [DataRow("not-a-number")]
        [DataRow(long.MaxValue)]
        public void Instantiate_MalformedPawnUpgradeValueSafelyFallsBackToOff(object rawValue)
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = rawValue,
            });

            Assert.AreEqual(FairyPawnUpgrades.Off, config.PawnUpgrades);
        }

        [TestMethod]
        public void Instantiate_UnexpectedPawnUpgradeConversionFailureIsNotSwallowed()
        {
            var config = new ApmwConfig();
            Assert.ThrowsException<ApplicationException>(() => config.Instantiate(
                new Dictionary<string, object>
                {
                    [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = new UnexpectedFailureConvertible(),
                }));
        }

        [TestMethod]
        public void Instantiate_HistoricalProportionForcesLegacyWhenCurrentMarkersAreAlsoPresent()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 3,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("better-pawn"),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] = "malformed",
                [ApmwConstants.LegacySlotKeyPieceUpgradeProportion] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 9,
                    }),
            });

            Assert.AreEqual(FairyPawnUpgrades.SuperMax, config.PawnUpgrades);
            Assert.IsTrue(config.UsesSuperMaxPawnGuarantee);
            Assert.AreEqual(
                9.0,
                config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.BetterPawn].Proportion);
        }

        [TestMethod]
        public void Instantiate_ExplicitCurrentContractOverridesHistoricalProportionFallback()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                ["apmw_contract"] = ApmwContractTestFixture.Document(),
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = 3,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("better-pawn"),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 5,
                    }),
                [ApmwConstants.LegacySlotKeyPieceUpgradeProportion] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 9,
                    }),
            });

            Assert.IsTrue(config.UsesCurrentContract);
            Assert.AreEqual(FairyPawnUpgrades.Configure, config.PawnUpgrades);
            Assert.IsFalse(config.UsesSuperMaxPawnGuarantee);
            Assert.AreEqual(
                5.0,
                config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.BetterPawn].Proportion);
        }

        [TestMethod]
        public void Instantiate_UsesResolvedPieceUpgradePreferencesWhenPresent()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("better-pawn", "not-real", "new-pawn", "major-to-queen"),
            });

            CollectionAssert.AreEqual(
                new[]
                {
                    "better-pawn",
                    "new-pawn",
                    "major-to-queen",
                    "more-pawn",
                    "pool-pawn-upgrade",
                    "pawn-to-minor",
                    "pawn-to-major",
                    "minor-to-major",
                    "major-to-jack",
                    "minor-to-jack",
                    "jack-to-queen",
                    "queen-to-amazon",
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_DerivesLegacyPreferencesWhenResolvedListIsEmpty()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray(),
            });

            CollectionAssert.AreEqual(
                new[] { "new-pawn", "pool-pawn-upgrade", "more-pawn", "better-pawn", "major-to-queen", "pawn-to-minor", "pawn-to-major", "minor-to-major", "major-to-jack", "minor-to-jack", "jack-to-queen", "queen-to-amazon" },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_FallsBackToOffPreferencesWhenResolvedListHasNoValidNames()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("not-real", "also-not-real"),
            });

            CollectionAssert.AreEqual(
                new[] { "new-pawn", "more-pawn", "better-pawn", "major-to-queen", "pool-pawn-upgrade", "pawn-to-minor", "pawn-to-major", "minor-to-major", "major-to-jack", "minor-to-jack", "jack-to-queen", "queen-to-amazon" },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_AcceptsFuturePieceUpgradePreferenceActionsAndFiltersInvalidNames()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray(
                        "minor-to-major",
                        "not-real",
                        "major-to-jack",
                        "minor-to-jack",
                        "jack-to-queen",
                        "queen-to-amazon"),
            });

            CollectionAssert.AreEqual(
                new[]
                {
                    "minor-to-major",
                    "major-to-jack",
                    "minor-to-jack",
                    "jack-to-queen",
                    "queen-to-amazon",
                    "new-pawn",
                    "more-pawn",
                    "better-pawn",
                    "pool-pawn-upgrade",
                    "pawn-to-minor",
                    "pawn-to-major",
                    "major-to-queen",
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void PieceUpgradePreferenceHelpers_ReflectResolvedOrderAndMajorToQueen()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("new-pawn", "better-pawn", "more-pawn"),
            });

            Assert.IsTrue(config.IsPieceUpgradeActionPreferredBefore(
                ApmwConstants.PieceUpgradeActions.BetterPawn,
                ApmwConstants.PieceUpgradeActions.MorePawn));
            Assert.IsTrue(config.MajorToQueenUpgradeEnabled);
            Assert.AreEqual(0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.MajorToQueen].Priority);

            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("new-pawn", "major-to-queen"),
            });

            Assert.IsTrue(config.MajorToQueenUpgradeEnabled);
        }

        [TestMethod]
        public void Instantiate_ResolvesPieceUpgradePriorityMapWithNeutralOmissionsAndDisabledActions()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    JObject.FromObject(new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 2,
                        [ApmwConstants.PieceUpgradeActions.MajorToQueen] = -1,
                        ["not-real"] = 100,
                    }),
            });

            Assert.IsTrue(config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.NewPawn));
            Assert.IsFalse(config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.MajorToQueen));
            Assert.AreEqual(0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.NewPawn].Priority);
            Assert.AreEqual(2, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.BetterPawn].Priority);
            Assert.IsTrue(config.IsPieceUpgradeActionPreferredBefore(
                ApmwConstants.PieceUpgradeActions.BetterPawn,
                ApmwConstants.PieceUpgradeActions.NewPawn));
            Assert.IsFalse(config.PieceUpgradeActions.ContainsKey("not-real"));
            CollectionAssert.AreEqual(
                new[]
                {
                    ApmwConstants.PieceUpgradeActions.BetterPawn,
                    ApmwConstants.PieceUpgradeActions.NewPawn,
                    ApmwConstants.PieceUpgradeActions.MorePawn,
                    ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
                    ApmwConstants.PieceUpgradeActions.PawnToMinor,
                    ApmwConstants.PieceUpgradeActions.PawnToMajor,
                    ApmwConstants.PieceUpgradeActions.MinorToMajor,
                    ApmwConstants.PieceUpgradeActions.MajorToJack,
                    ApmwConstants.PieceUpgradeActions.MinorToJack,
                    ApmwConstants.PieceUpgradeActions.JackToQueen,
                    ApmwConstants.PieceUpgradeActions.QueenToAmazon,
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_ResolvesPieceUpgradeProportionsFromDictionaryAndDefaultsOmittedActionsToOne()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    JObject.FromObject(new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 1,
                        [ApmwConstants.PieceUpgradeActions.PawnToMajor] = 1,
                    }),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 3,
                        ["not-real"] = 100,
                    }),
            });

            Assert.AreEqual(3.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.PawnToMinor].Proportion);
            Assert.AreEqual(
                1.0,
                config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.PawnToMajor].Proportion,
                "an action absent from the proportion dictionary defaults to weight 1");
            Assert.IsFalse(config.PieceUpgradeActions.ContainsKey("not-real"));
        }

        [TestMethod]
        public void Instantiate_DefaultsAllProportionsToOneWhenProportionKeyIsAbsentEntirely()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("new-pawn", "pawn-to-minor", "pawn-to-major"),
            });

            foreach (var action in config.PieceUpgradeActions.Values)
                Assert.AreEqual(1.0, action.Proportion, "action " + action.ActionName);
        }

        [TestMethod]
        public void Instantiate_ClampsNonPositiveProportionToZero()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    JObject.FromObject(new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 1,
                        [ApmwConstants.PieceUpgradeActions.PawnToMajor] = 1,
                    }),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] =
                    JObject.FromObject(new Dictionary<string, double>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 0,
                        [ApmwConstants.PieceUpgradeActions.PawnToMajor] = -5,
                    }),
            });

            Assert.AreEqual(0.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.PawnToMinor].Proportion);
            Assert.AreEqual(0.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.PawnToMajor].Proportion);
        }

        [TestMethod]
        public void Instantiate_FallsBackToOneWhenProportionValueIsNotNumeric()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    JObject.FromObject(new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 1,
                    }),
                [ApmwConstants.SlotKeyPieceUpgradeRatio] =
                    JObject.FromObject(new Dictionary<string, string>
                    {
                        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = "not-a-number",
                    }),
            });

            Assert.AreEqual(1.0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.PawnToMinor].Proportion);
        }

        [TestMethod]
        public void Instantiate_DefaultsToLegacyProgressionItemization()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>());

            Assert.AreEqual(ProgressionItemization.Legacy, config.ProgressionItemization);
            Assert.IsFalse(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(ApmwConfig.DefaultMaterialItemValue, config.materialItemValue);
            Assert.AreEqual(ApmwConfig.DefaultCastlingLocationCount, config.castlingLocationCount);
        }

        [DataTestMethod]
        [DataRow("fundamental")]
        [DataRow("Fundamental")]
        [DataRow((int)ProgressionItemization.Fundamental)]
        public void Instantiate_ParsesFundamentalProgressionItemization(object rawItemization)
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyProgressionItemization] = rawItemization,
                [ApmwConstants.SlotKeyMaterialItemValue] = 500,
                [ApmwConstants.SlotKeyCastlingLocationCount] = 1,
            });

            Assert.AreEqual(ProgressionItemization.Fundamental, config.ProgressionItemization);
            Assert.IsTrue(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(500, config.materialItemValue);
            Assert.AreEqual(1, config.castlingLocationCount);
        }

        [TestMethod]
        public void Instantiate_InvalidProgressionItemizationFallsBackToLegacyAndClampsCounts()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyProgressionItemization] = "future-mode",
                [ApmwConstants.SlotKeyMaterialItemValue] = 0,
                [ApmwConstants.SlotKeyCastlingLocationCount] = -3,
            });

            Assert.AreEqual(ProgressionItemization.Legacy, config.ProgressionItemization);
            Assert.IsFalse(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(1, config.materialItemValue);
            Assert.AreEqual(0, config.castlingLocationCount);
        }

        private static int[] CaptureSeeds(ApmwConfig config)
        {
            return new[]
            {
                config.pocketSeed,
                config.pawnSeed,
                config.minorSeed,
                config.majorSeed,
                config.queenSeed,
                config.pawnLocSeed,
                config.minorLocSeed,
                config.majorLocSeed,
                config.queenLocSeed,
            };
        }
    }
}
