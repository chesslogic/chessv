using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.APChessV;
using Newtonsoft.Json.Linq;

namespace ChessV.Test
{
    internal sealed class ApmwFuzzCase
    {
        internal const string VictoryItemName = "Victory";

        private static readonly int[] AllArmyIndexes = { 0, 1, 2, 3, 4, 5, 6 };

        public string CaseName { get; }
        public string Label { get { return TargetStage + "/" + Category + "/" + CaseName; } }
        public int CaseIndex { get; }
        public string MasterSeed { get; }
        public string TargetStage { get; }
        public string Category { get; }
        public bool IsSuperSized { get; }
        public string GameName { get { return IsSuperSized ? ApmwConstants.GameNameGrand : ApmwConstants.GameNameStandard; } }

        public Goal Goal { get; }
        public PieceTypes EnemyPieceTypes { get; }
        public PieceLocations PieceLocations { get; }
        public PieceTypes PlayerPieceTypes { get; }
        public FairyArmy FairyChessArmy { get; }
        public IReadOnlyList<int> ArmyIndexes { get; }
        public FairyPawns FairyChessPawns { get; }
        public FairyPawnUpgrades FairyChessPawnUpgrades { get; }
        public int MinorPieceLimitByType { get; }
        public int MajorPieceLimitByType { get; }
        public int QueenPieceLimitByType { get; }
        public int PocketLimitByPocket { get; }
        public bool DeathLink { get; }

        public int PocketSeed { get; }
        public int PawnSeed { get; }
        public int MinorSeed { get; }
        public int MajorSeed { get; }
        public int QueenSeed { get; }
        public int? DeterministicChaosSeed { get; }

        public int PocketCount { get; }
        public int PocketRangeCount { get; }
        public int PocketGemCount { get; }
        public int AIIntelligenceMalusCount { get; }
        public int PawnCount { get; }
        public int MinorPieceCount { get; }
        public int MajorPieceCount { get; }
        public int JackCount { get; }
        public int MajorToQueenCount { get; }
        public int AmazonCount { get; }
        public int PawnForwardnessCount { get; }
        public int ConsulCount { get; }
        public int KingPromotionCount { get; }
        public int SuperSizeMeCount { get; }
        public int PlayAsWhiteCount { get; }
        public int VictoryCount { get; }

        private ApmwFuzzCase(Builder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            CaseName = string.IsNullOrWhiteSpace(builder.CaseName)
                ? "apmw-fuzz-case-" + builder.CaseIndex
                : builder.CaseName;
            CaseIndex = builder.CaseIndex;
            MasterSeed = builder.MasterSeed ?? string.Empty;
            TargetStage = string.IsNullOrWhiteSpace(builder.TargetStage)
                ? TargetStages.ItemHandlerGeneration
                : builder.TargetStage;
            Category = string.IsNullOrWhiteSpace(builder.Category)
                ? "uncategorized"
                : builder.Category;
            IsSuperSized = builder.IsSuperSized;

            Goal = builder.Goal;
            EnemyPieceTypes = builder.EnemyPieceTypes;
            PieceLocations = builder.PieceLocations;
            PlayerPieceTypes = builder.PlayerPieceTypes;
            FairyChessArmy = builder.FairyChessArmy;
            ArmyIndexes = new ReadOnlyCollection<int>((builder.ArmyIndexes ?? Enumerable.Empty<int>()).ToList());
            FairyChessPawns = builder.FairyChessPawns;
            FairyChessPawnUpgrades = builder.FairyChessPawnUpgrades;
            MinorPieceLimitByType = builder.MinorPieceLimitByType;
            MajorPieceLimitByType = builder.MajorPieceLimitByType;
            QueenPieceLimitByType = builder.QueenPieceLimitByType;
            PocketLimitByPocket = builder.PocketLimitByPocket;
            DeathLink = builder.DeathLink;

            PocketSeed = builder.PocketSeed;
            PawnSeed = builder.PawnSeed;
            MinorSeed = builder.MinorSeed;
            MajorSeed = builder.MajorSeed;
            QueenSeed = builder.QueenSeed;
            DeterministicChaosSeed = builder.DeterministicChaosSeed;

            PocketCount = builder.PocketCount;
            PocketRangeCount = builder.PocketRangeCount;
            PocketGemCount = builder.PocketGemCount;
            AIIntelligenceMalusCount = builder.AIIntelligenceMalusCount;
            PawnCount = builder.PawnCount;
            MinorPieceCount = builder.MinorPieceCount;
            MajorPieceCount = builder.MajorPieceCount;
            JackCount = builder.JackCount;
            MajorToQueenCount = builder.MajorToQueenCount;
            AmazonCount = builder.AmazonCount;
            PawnForwardnessCount = builder.PawnForwardnessCount;
            ConsulCount = builder.ConsulCount;
            KingPromotionCount = builder.KingPromotionCount;
            SuperSizeMeCount = builder.SuperSizeMeCount;
            PlayAsWhiteCount = builder.PlayAsWhiteCount;
            VictoryCount = builder.VictoryCount;
        }

        public static ApmwFuzzCase DefaultStandard()
        {
            return new Builder().Build();
        }

        public static ApmwFuzzCase DefaultSuperSized()
        {
            return new Builder
            {
                CaseName = "default-super-sized",
                IsSuperSized = true,
                PawnCount = 10,
                SuperSizeMeCount = 1,
            }.Build();
        }

        public ApmwFuzzCase With(Action<Builder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = ToBuilder();
            configure(builder);
            return builder.Build();
        }

        public Builder ToBuilder()
        {
            return new Builder
            {
                CaseName = CaseName,
                CaseIndex = CaseIndex,
                MasterSeed = MasterSeed,
                TargetStage = TargetStage,
                Category = Category,
                IsSuperSized = IsSuperSized,

                Goal = Goal,
                EnemyPieceTypes = EnemyPieceTypes,
                PieceLocations = PieceLocations,
                PlayerPieceTypes = PlayerPieceTypes,
                FairyChessArmy = FairyChessArmy,
                ArmyIndexes = ArmyIndexes.ToArray(),
                FairyChessPawns = FairyChessPawns,
                FairyChessPawnUpgrades = FairyChessPawnUpgrades,
                MinorPieceLimitByType = MinorPieceLimitByType,
                MajorPieceLimitByType = MajorPieceLimitByType,
                QueenPieceLimitByType = QueenPieceLimitByType,
                PocketLimitByPocket = PocketLimitByPocket,
                DeathLink = DeathLink,

                PocketSeed = PocketSeed,
                PawnSeed = PawnSeed,
                MinorSeed = MinorSeed,
                MajorSeed = MajorSeed,
                QueenSeed = QueenSeed,
                DeterministicChaosSeed = DeterministicChaosSeed,

                PocketCount = PocketCount,
                PocketRangeCount = PocketRangeCount,
                PocketGemCount = PocketGemCount,
                AIIntelligenceMalusCount = AIIntelligenceMalusCount,
                PawnCount = PawnCount,
                MinorPieceCount = MinorPieceCount,
                MajorPieceCount = MajorPieceCount,
                JackCount = JackCount,
                MajorToQueenCount = MajorToQueenCount,
                AmazonCount = AmazonCount,
                PawnForwardnessCount = PawnForwardnessCount,
                ConsulCount = ConsulCount,
                KingPromotionCount = KingPromotionCount,
                SuperSizeMeCount = SuperSizeMeCount,
                PlayAsWhiteCount = PlayAsWhiteCount,
                VictoryCount = VictoryCount,
            };
        }

        public Dictionary<string, object> BuildSlotData()
        {
            var slotData = new Dictionary<string, object>
            {
                ["goal"] = (int)Goal,
                ["enemy_piece_types"] = (int)EnemyPieceTypes,
                ["piece_locations"] = (int)PieceLocations,
                ["piece_types"] = (int)PlayerPieceTypes,
                ["fairy_chess_army"] = (int)FairyChessArmy,
                ["army"] = BuildArmyJArray(),
                ["fairy_chess_pawns"] = (int)FairyChessPawns,
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyChessPawnUpgrades,
                ["minor_piece_limit_by_type"] = MinorPieceLimitByType,
                ["major_piece_limit_by_type"] = MajorPieceLimitByType,
                ["queen_piece_limit_by_type"] = QueenPieceLimitByType,
                ["pocket_limit_by_pocket"] = PocketLimitByPocket,
                ["death_link"] = DeathLink ? 1 : 0,
                ["pocket_seed"] = PocketSeed,
                ["pawn_seed"] = PawnSeed,
                ["minor_seed"] = MinorSeed,
                ["major_seed"] = MajorSeed,
                ["queen_seed"] = QueenSeed,
                ["required_chess_client_version"] = ApmwConstants.ClientVersion,
            };

            if (DeterministicChaosSeed.HasValue)
                slotData["deterministic_chaos_seed"] = DeterministicChaosSeed.Value;

            return slotData;
        }

        public IEnumerable<(string ItemName, int Count)> BuildItemCounts()
        {
            yield return (ApmwConstants.ProgressiveItems.Pocket, PocketCount);
            yield return (ApmwConstants.ProgressiveItems.PocketRange, PocketRangeCount);
            yield return (ApmwConstants.ProgressiveItems.PocketGems, PocketGemCount);
            yield return (ApmwConstants.ProgressiveItems.AIIntelligenceMalus, AIIntelligenceMalusCount);
            yield return (ApmwConstants.ProgressiveItems.Pawn, PawnCount);
            yield return (ApmwConstants.ProgressiveItems.MinorPiece, MinorPieceCount);
            yield return (ApmwConstants.ProgressiveItems.MajorPiece, MajorPieceCount);
            yield return (ApmwConstants.ProgressiveItems.Jack, JackCount);
            yield return (ApmwConstants.ProgressiveItems.MajorToQueen, MajorToQueenCount);
            yield return (ApmwConstants.ProgressiveItems.Amazon, AmazonCount);
            yield return (ApmwConstants.ProgressiveItems.PawnForwardness, PawnForwardnessCount);
            yield return (ApmwConstants.ProgressiveItems.Consul, ConsulCount);
            yield return (ApmwConstants.ProgressiveItems.KingPromotion, KingPromotionCount);
            yield return (ApmwConstants.ProgressiveItems.SuperSizeMe, SuperSizeMeCount);
            yield return (ApmwConstants.ProgressiveItems.PlayAsWhite, PlayAsWhiteCount);
            yield return (VictoryItemName, VictoryCount);
        }

        public IEnumerable<(string ItemName, int Count)> BuildPositiveItemCounts()
        {
            return BuildItemCounts().Where(item => item.Count > 0);
        }

        public Dictionary<string, int> BuildItemCountMap()
        {
            return BuildItemCounts().ToDictionary(item => item.ItemName, item => item.Count);
        }

        public string CanonicalOptionKey
        {
            get { return ApmwFuzzCaseOptionVector.BuildCanonicalKey(this); }
        }

        public string ToDiagnosticString()
        {
            return string.Format(
                "ApmwFuzzCase(CaseName=\"{0}\", Label=\"{1}\", CaseIndex={2}, MasterSeed=\"{3}\", TargetStage=\"{4}\", Category=\"{5}\", IsSuperSized={6}, GameName=\"{7}\", " +
                "Slots=[goal={8}, enemy_piece_types={9}, piece_locations={10}, piece_types={11}, fairy_chess_army={12}, army=[{13}], fairy_chess_pawns={14}, fairy_chess_pawn_upgrades={15}, minor_limit={16}, major_limit={17}, queen_limit={18}, pocket_limit={19}, death_link={20}], " +
                "Seeds=[pocket={21}, pawn={22}, minor={23}, major={24}, queen={25}, chaos={26}], " +
                "Items=[pockets={27}, pocket_range={28}, pocket_gems={29}, ai_malus={30}, pawns={31}, minors={32}, majors={33}, jacks={34}, major_to_queen={35}, amazons={36}, pawn_forwardness={37}, consuls={38}, king_promotions={39}, super_size={40}, play_as_white={41}, victory={42}])",
                Escape(CaseName),
                Escape(Label),
                CaseIndex,
                Escape(MasterSeed),
                Escape(TargetStage),
                Escape(Category),
                IsSuperSized,
                Escape(GameName),
                (int)Goal,
                (int)EnemyPieceTypes,
                (int)PieceLocations,
                (int)PlayerPieceTypes,
                (int)FairyChessArmy,
                string.Join(",", ArmyIndexes),
                (int)FairyChessPawns,
                (int)FairyChessPawnUpgrades,
                MinorPieceLimitByType,
                MajorPieceLimitByType,
                QueenPieceLimitByType,
                PocketLimitByPocket,
                DeathLink ? 1 : 0,
                PocketSeed,
                PawnSeed,
                MinorSeed,
                MajorSeed,
                QueenSeed,
                DeterministicChaosSeed.HasValue ? DeterministicChaosSeed.Value.ToString() : "null",
                PocketCount,
                PocketRangeCount,
                PocketGemCount,
                AIIntelligenceMalusCount,
                PawnCount,
                MinorPieceCount,
                MajorPieceCount,
                JackCount,
                MajorToQueenCount,
                AmazonCount,
                PawnForwardnessCount,
                ConsulCount,
                KingPromotionCount,
                SuperSizeMeCount,
                PlayAsWhiteCount,
                VictoryCount);
        }

        public override string ToString()
        {
            return ToDiagnosticString();
        }

        private JArray BuildArmyJArray()
        {
            var army = new JArray();
            foreach (int armyIndex in ArmyIndexes)
                army.Add(armyIndex);
            return army;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        internal static class TargetStages
        {
            public const string Configuration = "configuration";
            public const string ItemHandlerGeneration = "item-handler-generation";
            public const string StandardBoardStartup = "standard-board-startup";
            public const string SuperBoardStartup = "super-board-startup";
            public const string MoveGeneration = "move-generation";
            public const string InvariantValidation = "invariant-validation";

            public static string BoardStartup(bool isSuperSized)
            {
                return isSuperSized ? SuperBoardStartup : StandardBoardStartup;
            }

            public static string FromStage(ApmwFuzzStage stage)
            {
                switch (stage)
                {
                    case ApmwFuzzStage.Configuration:
                        return Configuration;
                    case ApmwFuzzStage.ItemHandlerGeneration:
                        return ItemHandlerGeneration;
                    case ApmwFuzzStage.StandardBoardStartup:
                        return StandardBoardStartup;
                    case ApmwFuzzStage.SuperBoardStartup:
                        return SuperBoardStartup;
                    case ApmwFuzzStage.MoveGeneration:
                        return MoveGeneration;
                    case ApmwFuzzStage.InvariantValidation:
                        return InvariantValidation;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stage), stage, "Unknown APMW fuzz stage.");
                }
            }
        }

        internal sealed class Builder
        {
            public string CaseName { get; set; } = "default-standard";
            public int CaseIndex { get; set; }
            public string MasterSeed { get; set; } = "0";
            public string TargetStage { get; set; } = TargetStages.ItemHandlerGeneration;
            public string Category { get; set; } = "generation";
            public bool IsSuperSized { get; set; }

            public Goal Goal { get; set; } = Goal.Single;
            public PieceTypes EnemyPieceTypes { get; set; } = PieceTypes.Book;
            public PieceLocations PieceLocations { get; set; } = PieceLocations.Stable;
            public PieceTypes PlayerPieceTypes { get; set; } = PieceTypes.Stable;
            public FairyArmy FairyChessArmy { get; set; } = FairyArmy.Chaos;
            public IEnumerable<int> ArmyIndexes { get; set; } = AllArmyIndexes;
            public FairyPawns FairyChessPawns { get; set; } = FairyPawns.Mixed;
            public FairyPawnUpgrades FairyChessPawnUpgrades { get; set; } = FairyPawnUpgrades.Off;
            public int MinorPieceLimitByType { get; set; }
            public int MajorPieceLimitByType { get; set; }
            public int QueenPieceLimitByType { get; set; }
            public int PocketLimitByPocket { get; set; } = 4;
            public bool DeathLink { get; set; }

            public int PocketSeed { get; set; } = 101;
            public int PawnSeed { get; set; } = 102;
            public int MinorSeed { get; set; } = 103;
            public int MajorSeed { get; set; } = 104;
            public int QueenSeed { get; set; } = 105;
            public int? DeterministicChaosSeed { get; set; }

            public int PocketCount { get; set; }
            public int PocketRangeCount { get; set; }
            public int PocketGemCount { get; set; }
            public int AIIntelligenceMalusCount { get; set; }
            public int PawnCount { get; set; } = 8;
            public int MinorPieceCount { get; set; } = 4;
            public int MajorPieceCount { get; set; } = 2;
            public int JackCount { get; set; }
            public int MajorToQueenCount { get; set; } = 1;
            public int AmazonCount { get; set; }
            public int PawnForwardnessCount { get; set; }
            public int ConsulCount { get; set; }
            public int KingPromotionCount { get; set; }
            public int SuperSizeMeCount { get; set; }
            public int PlayAsWhiteCount { get; set; } = 1;
            public int VictoryCount { get; set; }

            public ApmwFuzzCase Build()
            {
                return new ApmwFuzzCase(this);
            }
        }
    }
}
