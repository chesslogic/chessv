using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.APChessV
{
  internal static class ItemGenerationValues
  {
    public const int Major = 485;
    public const int Minor = 300;
    public const int Pawn = 100;
    public const int Weak = 75;
    public const int Jack = 700;
    public const int Queen = 900;
    public const int Amazon = 1300;
    public const int Castler = 500;
    public const int CastlerMaximum = 2;
    public const int Consul = 325;
    public const int KingPromotion = 425;
    public const int PlayAsWhite = 50;
    public const int Pocket = 110;
  }

  internal readonly struct BoardCoordinate
  {
    public BoardCoordinate(int rank, int file)
    {
      Rank = rank;
      File = file;
    }

    public int Rank { get; }
    public int File { get; }

    public KeyValuePair<int, int> ToKeyValuePair()
    {
      return new KeyValuePair<int, int>(Rank, File);
    }
  }

  internal sealed class PieceSetLayout
  {
    private PieceSetLayout(
      List<PieceType> leftBackRank,
      PieceType centerBackRank,
      List<PieceType> rightBackRank,
      List<PieceType> outerRank)
    {
      LeftBackRank = leftBackRank;
      CenterBackRank = centerBackRank;
      RightBackRank = rightBackRank;
      OuterRank = outerRank;
    }

    public List<PieceType> LeftBackRank { get; }
    public PieceType CenterBackRank { get; set; }
    public List<PieceType> RightBackRank { get; }
    public List<PieceType> OuterRank { get; }

    public int BackRankPieceCount
    {
      get { return LeftBackRank.Count(piece => piece != null) + RightBackRank.Count(piece => piece != null); }
    }

    public int AvailableBackRankSpaces
    {
      get { return LeftBackRank.Count(piece => piece == null) + RightBackRank.Count(piece => piece == null); }
    }

    public int AvailableOuterRankSpaces
    {
      get { return OuterRank.Count(piece => piece == null); }
    }

    public static PieceSetLayout Empty(int numFiles, PieceType centerBackRank)
    {
      return new PieceSetLayout(
        Enumerable.Repeat<PieceType>(null, numFiles / 2).ToList(),
        centerBackRank,
        Enumerable.Repeat<PieceType>(null, numFiles / 2 - 1).ToList(),
        Enumerable.Repeat<PieceType>(null, numFiles).ToList());
    }

    public static PieceSetLayout FromPieceList(int numFiles, List<PieceType> pieces)
    {
      return new PieceSetLayout(
        pieces.Take(numFiles / 2).ToList(),
        pieces[numFiles / 2],
        pieces.Skip(numFiles / 2 + 1).Take(numFiles / 2 - 1).ToList(),
        pieces.Skip(numFiles).Take(numFiles).ToList());
    }

    public List<PieceType> ToPieceList()
    {
      List<PieceType> output = new List<PieceType>();
      output.AddRange(LeftBackRank);
      output.Add(CenterBackRank);
      output.AddRange(RightBackRank);
      output.AddRange(OuterRank);
      return output;
    }
  }

  internal static class PieceMaterialAccounting
  {
    public static void RecordPromotion(
      HashSet<string> promotionPieces,
      PieceType piece,
      int player,
      int expectedMaterial,
      ref int spareMaterial)
    {
      if (piece == null)
        return;

      promotionPieces.Add(piece.Notation[player]);
      spareMaterial += expectedMaterial - piece.MidgameValue;
    }

    public static void RecordSubstitutionPromotion(
      HashSet<string> promotionPieces,
      PieceType sourcePiece,
      PieceType targetPiece,
      int player,
      int expectedMaterial,
      ref int spareMaterial)
    {
      if (sourcePiece != null)
        spareMaterial += sourcePiece.MidgameValue;
      RecordPromotion(promotionPieces, targetPiece, player, expectedMaterial, ref spareMaterial);
    }

    public static void RecordUnusedUpgradeCredit(int unusedUpgrades, int expectedMaterial, ref int spareMaterial)
    {
      spareMaterial += Math.Max(0, unusedUpgrades) * expectedMaterial;
    }
  }

  internal sealed class NonPawnFamilySubstitutionRequest
  {
    public NonPawnFamilySubstitutionRequest(
      List<PieceType> pieces,
      List<string> promotions,
      IEnumerable<PieceType> sourceFamily,
      IEnumerable<int> preferredSourceIndices,
      bool allowEmptySourceSlots,
      IEnumerable<PieceType> targetPieces,
      int requestedUpgrades,
      int seed,
      int typeLimit,
      int expectedMaterial)
    {
      Pieces = pieces;
      Promotions = promotions;
      SourceFamily = sourceFamily ?? Enumerable.Empty<PieceType>();
      PreferredSourceIndices = preferredSourceIndices;
      AllowEmptySourceSlots = allowEmptySourceSlots;
      TargetPieces = targetPieces;
      RequestedUpgrades = Math.Max(0, requestedUpgrades);
      Seed = seed;
      TypeLimit = typeLimit;
      ExpectedMaterial = expectedMaterial;
    }

    public List<PieceType> Pieces { get; private set; }
    public List<string> Promotions { get; private set; }
    public IEnumerable<PieceType> SourceFamily { get; private set; }
    public IEnumerable<int> PreferredSourceIndices { get; private set; }
    public bool AllowEmptySourceSlots { get; private set; }
    public IEnumerable<PieceType> TargetPieces { get; private set; }
    public int RequestedUpgrades { get; private set; }
    public int Seed { get; private set; }
    public int TypeLimit { get; private set; }
    public int ExpectedMaterial { get; private set; }
  }

  internal static class NonPawnFamilySubstitution
  {
    public static List<PieceType> Substitute(NonPawnFamilySubstitutionRequest request, ref int spareMaterial)
    {
      HashSet<string> promotionPieces = new HashSet<string>();
      List<int> sourceIndices = FindSourceIndices(request);
      List<PieceType> targetPieces = ArmyPieceFilter.Filter(request.TargetPieces ?? Enumerable.Empty<PieceType>());
      int replacementsToApply = Math.Min(request.RequestedUpgrades, sourceIndices.Count);
      int replacementsApplied = 0;

      if (targetPieces.Count > 0)
      {
        Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
        Random random = new Random(request.Seed);
        int player = ApmwCore.getInstance().GeriProvider();

        for (int i = 0; i < replacementsToApply; i++)
        {
          int sourceIndex = sourceIndices[i];
          PieceType sourcePiece = request.Pieces[sourceIndex];
          PieceType targetPiece = PieceChoice.Choose(ref targetPieces, random, chosenPieces, request.TypeLimit);
          PieceMaterialAccounting.RecordSubstitutionPromotion(
            promotionPieces,
            sourcePiece,
            targetPiece,
            player,
            request.ExpectedMaterial,
            ref spareMaterial);
          request.Pieces[sourceIndex] = targetPiece;
          replacementsApplied++;
        }
      }

      PieceMaterialAccounting.RecordUnusedUpgradeCredit(
        request.RequestedUpgrades - replacementsApplied,
        request.ExpectedMaterial,
        ref spareMaterial);
      request.Promotions.Add(string.Join("", promotionPieces));
      return request.Pieces;
    }

    private static List<int> FindSourceIndices(NonPawnFamilySubstitutionRequest request)
    {
      HashSet<PieceType> sourceFamily = new HashSet<PieceType>(request.SourceFamily);
      IEnumerable<int> indices = request.PreferredSourceIndices ??
        Enumerable.Range(0, request.Pieces.Count);
      return indices
        .Where(index => index >= 0 && index < request.Pieces.Count)
        .Where(index => IsSourceSlot(request.Pieces[index], sourceFamily, request.AllowEmptySourceSlots))
        .ToList();
    }

    private static bool IsSourceSlot(
      PieceType piece,
      HashSet<PieceType> sourceFamily,
      bool allowEmptySourceSlots)
    {
      if (piece == null)
        return allowEmptySourceSlots;
      return sourceFamily.Contains(piece);
    }
  }

  internal enum NonPawnPieceFamily
  {
    Minor = 0,
    Major = 1,
    Jack = 2,
    Queen = 3,
    Amazon = 4
  }

  internal sealed class NonPawnUpgradeActionMetadata
  {
    public NonPawnUpgradeActionMetadata(
      string actionName,
      NonPawnPieceFamily sourceFamily,
      NonPawnPieceFamily targetFamily,
      int upgradeMaterialCredit)
    {
      ActionName = actionName;
      SourceFamily = sourceFamily;
      TargetFamily = targetFamily;
      UpgradeMaterialCredit = upgradeMaterialCredit;
    }

    public string ActionName { get; private set; }
    public NonPawnPieceFamily SourceFamily { get; private set; }
    public NonPawnPieceFamily TargetFamily { get; private set; }
    public int UpgradeMaterialCredit { get; private set; }
  }

  internal sealed class PlannedNonPawnUpgradeAction
  {
    public PlannedNonPawnUpgradeAction(NonPawnUpgradeActionMetadata metadata, int requestedUpgrades)
    {
      Metadata = metadata;
      RequestedUpgrades = Math.Max(0, requestedUpgrades);
    }

    public NonPawnUpgradeActionMetadata Metadata { get; private set; }
    public int RequestedUpgrades { get; private set; }
  }

  internal sealed class NonPawnGenerationPlan
  {
    public NonPawnGenerationPlan(
      Dictionary<NonPawnPieceFamily, int> directCounts,
      List<PlannedNonPawnUpgradeAction> upgradeActions,
      Dictionary<NonPawnPieceFamily, int> unusedUpgradeCounts,
      int lockedMajorCount)
    {
      DirectCounts = directCounts;
      UpgradeActions = upgradeActions;
      UnusedUpgradeCounts = unusedUpgradeCounts;
      LockedMajorCount = Math.Max(0, lockedMajorCount);
    }

    public Dictionary<NonPawnPieceFamily, int> DirectCounts { get; private set; }
    public List<PlannedNonPawnUpgradeAction> UpgradeActions { get; private set; }
    public Dictionary<NonPawnPieceFamily, int> UnusedUpgradeCounts { get; private set; }
    public int LockedMajorCount { get; private set; }

    public int DirectCount(NonPawnPieceFamily family)
    {
      int count;
      return DirectCounts.TryGetValue(family, out count) ? count : 0;
    }
  }

  internal sealed class PieceGenerationAllocation
  {
    private readonly Dictionary<NonPawnPieceFamily, int> nonPawnCounts;

    private PieceGenerationAllocation(
      int pawnSlots,
      Dictionary<NonPawnPieceFamily, int> nonPawnCounts,
      int initialSpareMaterial,
      int? nonKingPieceSlotLimit,
      int lockedMajorCount,
      NonPawnGenerationPlan precomputedNonPawnPlan,
      Dictionary<string, int> appliedGraduationCounts)
    {
      PawnSlots = Math.Max(0, pawnSlots);
      this.nonPawnCounts = nonPawnCounts ?? new Dictionary<NonPawnPieceFamily, int>();
      InitialSpareMaterial = Math.Max(0, initialSpareMaterial);
      NonKingPieceSlotLimit = nonKingPieceSlotLimit;
      LockedMajorCount = Math.Max(0, lockedMajorCount);
      PrecomputedNonPawnPlan = precomputedNonPawnPlan;
      AppliedGraduationCounts = appliedGraduationCounts ??
        new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public int PawnSlots { get; private set; }
    public int InitialSpareMaterial { get; private set; }
    public int? NonKingPieceSlotLimit { get; private set; }
    public bool LimitsNonKingPieceSlots { get { return NonKingPieceSlotLimit.HasValue; } }
    public int LockedMajorCount { get; private set; }

    /// <summary>
    /// When set (Fundamental mode), this exact plan is used instead of re-deriving one via
    /// <see cref="NonPawnUpgradeGeneration.Plan(PieceGenerationAllocation, ApmwConfig)"/>. The
    /// slot graduation simulation already knows precisely which upgrade actions fired and how
    /// many times, so re-netting from gross counts is unnecessary (and, for per-slot randomized
    /// graduation, can be lossy -- see FundamentalSlotGraduationPlanner). Null for Legacy, which
    /// continues to net gross found-item counts exactly as before.
    /// </summary>
    public NonPawnGenerationPlan PrecomputedNonPawnPlan { get; private set; }
    public IReadOnlyDictionary<string, int> AppliedGraduationCounts { get; private set; }

    public int AppliedGraduationCount(string actionName)
    {
      int count;
      return AppliedGraduationCounts.TryGetValue(actionName, out count) ? count : 0;
    }

    public int NonPawnCount(NonPawnPieceFamily family)
    {
      int count;
      return nonPawnCounts.TryGetValue(family, out count) ? count : 0;
    }

    public int RemainingPawnSlotsAfterNonKingPieces(List<PieceType> pieces)
    {
      if (!LimitsNonKingPieceSlots)
        return PawnSlots;

      int occupiedNonKingSlots = pieces.Count(IsNonKingPiece);
      return Math.Max(0, NonKingPieceSlotLimit.Value - occupiedNonKingSlots);
    }

    public static PieceGenerationAllocation FromCore(ApmwCore core, ApmwConfig config, int numFiles)
    {
      if (config.UsesFundamentalProgressionItemization)
        return FundamentalSlotGraduationPlanner.Plan(core, config, numFiles);

      return new PieceGenerationAllocation(
        core.foundPawns,
        new Dictionary<NonPawnPieceFamily, int>
        {
          [NonPawnPieceFamily.Minor] = core.foundMinors,
          [NonPawnPieceFamily.Major] = core.foundMajors,
          [NonPawnPieceFamily.Jack] = core.foundJacks,
          [NonPawnPieceFamily.Queen] = core.foundQueens,
          [NonPawnPieceFamily.Amazon] = core.foundAmazons,
        },
        0,
        null,
        0,
        null,
        null);
    }

    internal static PieceGenerationAllocation Fundamental(
      int pawnSlots,
      Dictionary<NonPawnPieceFamily, int> nonPawnCounts,
      int initialSpareMaterial,
      int nonKingPieceSlotLimit,
      int lockedMajorCount,
      NonPawnGenerationPlan precomputedNonPawnPlan,
      Dictionary<string, int> appliedGraduationCounts)
    {
      return new PieceGenerationAllocation(
        pawnSlots,
        nonPawnCounts,
        initialSpareMaterial,
        Math.Max(0, nonKingPieceSlotLimit),
        lockedMajorCount,
        precomputedNonPawnPlan,
        appliedGraduationCounts);
    }

    private static bool IsNonKingPiece(PieceType piece)
    {
      if (piece == null)
        return false;

      var kings = ApmwCore.getInstance().kings;
      return kings == null || !kings.Contains(piece);
    }
  }

  internal enum ChessmanTier
  {
    Pawn = 0,
    Minor = 1,
    Major = 2,
    Jack = 3,
    Queen = 4,
    Amazon = 5
  }

  /// <summary>
  /// Shared per-unit weighted tie-break used by both graduation/upgrade consumers
  /// (FundamentalSlotGraduationPlanner.Simulate and NonPawnUpgradeGeneration.Plan) so their
  /// tie-break behavior can never independently drift apart. Weight is always
  /// eligibleCount * proportion, decided fresh for each single unit applied -- callers
  /// recompute the candidate list and weights every time something might have changed
  /// eligibility (i.e. after every applied unit), never once per whole batch.
  /// </summary>
  internal static class WeightedTieBreak
  {
    public static int ChooseIndex(IReadOnlyList<double> weights, Random random)
    {
      double totalWeight = 0;
      for (int i = 0; i < weights.Count; i++)
        totalWeight += weights[i];

      if (totalWeight <= 0)
        return random.Next(weights.Count); // Every tied candidate has zero weight (e.g.
                                            // proportion explicitly configured to 0 for all of
                                            // them) -- fall back to a uniform pick rather than
                                            // always favoring the first.

      double r = random.NextDouble() * totalWeight;
      double cumulative = 0;
      for (int i = 0; i < weights.Count; i++)
      {
        cumulative += weights[i];
        if (r < cumulative)
          return i;
      }

      return weights.Count - 1; // Defensive fallback; unreachable since r < totalWeight.
    }

    public static int ChooseIndex(
      IReadOnlyList<double> weights,
      CounterBasedSeedSeries series,
      long counter)
    {
      double totalWeight = 0;
      for (int i = 0; i < weights.Count; i++)
        totalWeight += weights[i];
      if (totalWeight <= 0)
        return series.Index(counter, weights.Count);

      double draw = series.Unit(counter) * totalWeight;
      double cumulative = 0;
      for (int i = 0; i < weights.Count; i++)
      {
        cumulative += weights[i];
        if (draw < cumulative)
          return i;
      }
      return weights.Count - 1;
    }
  }

  /// <summary>
  /// Fundamental-mode piece-count planner. Every one of the player's Chessmen slots starts as a
  /// Pawn; a single seeded simulation graduates slots up through tiers (Pawn -&gt; Minor/Major/
  /// Jack -&gt; Queen -&gt; Amazon), one real incremental step at a time, spending from a single
  /// running material counter, until nothing more is affordable. This mirrors the "legacy
  /// placement algorithm" it feeds -- kept completely unmodified downstream: which specific
  /// piece type fills a slot and which board square it lands on are still decided entirely by
  /// NonPawnUpgradeGeneration.ApplyUpgrades / NonPawnFamilySubstitution / MajorPieceGeneration /
  /// PiecePlacement using their own existing seeds. This planner only ever decides tier counts.
  /// </summary>
  // TODO(chesslogic): A future monotonic slot-prefix planner could preserve individual earlier
  // slots. The characterized shared-wave planner intentionally remains for v2; with fixed
  // Material, adding Chessmen can redistribute and downgrade earlier tiers
  // (FundamentalSlotGraduationPlannerTests.Plan_GrowingChessmenWithFixedMaterial_*).
  internal static class FundamentalSlotGraduationPlanner
  {
    private const int CastlerMaterialCost = ItemGenerationValues.Castler;
    private const int TierCount = 6;

    private static readonly int[] TierMaterialValues =
    {
      ItemGenerationValues.Pawn,
      ItemGenerationValues.Minor,
      ItemGenerationValues.Major,
      ItemGenerationValues.Jack,
      ItemGenerationValues.Queen,
      ItemGenerationValues.Amazon,
    };

    private sealed class GraduationAction
    {
      public GraduationAction(string key, ChessmanTier fromTier, ChessmanTier toTier, int expectedMaterial, int priority, double proportion)
      {
        Key = key;
        FromTier = fromTier;
        ToTier = toTier;
        ExpectedMaterial = expectedMaterial;
        Priority = priority;
        Proportion = proportion;
      }

      public string Key { get; private set; }
      public ChessmanTier FromTier { get; private set; }
      public ChessmanTier ToTier { get; private set; }
      public int ExpectedMaterial { get; private set; }
      public int Priority { get; private set; }

      // Per-draw relative weight used only to arbitrate among actions tied at the same
      // Priority -- see ApmwConfig.PieceUpgradeActionResolution.Proportion. Two actions tied at
      // the same priority AND the same FromTier (e.g. pawn-to-minor/pawn-to-major, both
      // Pawn-sourced) previously collapsed to "silently keep the cheaper one"; now every tied
      // action is genuinely competed for via weighted draw, exactly like today's existing
      // cross-FromTier ties (e.g. major-to-jack vs minor-to-jack).
      public double Proportion { get; private set; }

      // Unlike the old aggregate "relative-to-Pawn" recipe cost, this is the true marginal cost
      // of this one step. A multi-step journey (e.g. Pawn->Minor then Minor->Major) sums its
      // incremental costs to exactly the old aggregate recipe cost for the equivalent full chain.
      public int IncrementalCost
      {
        get { return Math.Max(0, ExpectedMaterial - TierMaterialValues[(int)FromTier]); }
      }
    }

    public static PieceGenerationAllocation Plan(ApmwCore core, ApmwConfig config, int numFiles)
    {
      return Plan(core, config, numFiles, true);
    }

    public static PieceGenerationAllocation PlanOwnedRoster(ApmwCore core, ApmwConfig config)
    {
      return Plan(core, config, 0, false);
    }

    private static PieceGenerationAllocation Plan(
      ApmwCore core,
      ApmwConfig config,
      int numFiles,
      bool applyGeometryCaps)
    {
      int requestedSlots = Math.Max(0, core.foundChessmen);
      int placeableSlots = applyGeometryCaps
        ? Math.Min(requestedSlots, MaxGeneratedNonKingPieces(numFiles))
        : requestedSlots;
      int nonPawnSlotCapacity = applyGeometryCaps
        ? Math.Min(placeableSlots, MaxNonPawnPipelineSlots(numFiles))
        : requestedSlots;
      int spareMaterial = Math.Max(0, core.foundMaterialBudget);

      // Castler-locked majors are pre-seeded directly and permanently excluded from
      // graduation: they must remain eligible to castle, so they can never be substituted
      // away by a later upgrade. Locking is just as much a "spend" as any other graduation,
      // so it's debited from the same unified spareMaterial counter, not carved out specially.
      int lockedMajorCount = ActiveCastlerCount(
        core,
        spareMaterial,
        nonPawnSlotCapacity,
        applyGeometryCaps ? MaxCastlingMajorSlots(numFiles) : int.MaxValue);
      spareMaterial -= lockedMajorCount * CastlerMaterialCost;

      int[] tierCounts;
      Dictionary<string, int> appliedCounts = Simulate(
        placeableSlots,
        lockedMajorCount,
        nonPawnSlotCapacity,
        config,
        ref spareMaterial,
        out tierCounts);

      NonPawnGenerationPlan nonPawnPlan = BuildNonPawnPlan(tierCounts, appliedCounts, lockedMajorCount);
      Dictionary<NonPawnPieceFamily, int> nonPawnCounts = new Dictionary<NonPawnPieceFamily, int>
      {
        [NonPawnPieceFamily.Minor] = tierCounts[(int)ChessmanTier.Minor],
        [NonPawnPieceFamily.Major] = tierCounts[(int)ChessmanTier.Major],
        [NonPawnPieceFamily.Jack] = tierCounts[(int)ChessmanTier.Jack],
        [NonPawnPieceFamily.Queen] = tierCounts[(int)ChessmanTier.Queen],
        [NonPawnPieceFamily.Amazon] = tierCounts[(int)ChessmanTier.Amazon],
      };

      return PieceGenerationAllocation.Fundamental(
        tierCounts[(int)ChessmanTier.Pawn],
        nonPawnCounts,
        spareMaterial,
        placeableSlots,
        lockedMajorCount,
        nonPawnPlan,
        appliedCounts);
    }

    /// <summary>
    /// Runs the graduation simulation using pure per-tier counts (no per-slot bookkeeping):
    /// every slot at a given tier is completely interchangeable (same available actions, same
    /// cost), so tracking "how many" is all correctness ever requires -- individual slot
    /// identity never affects the final counts. This keeps the simulation trivially seed/prefix
    /// -stable: with no configured ties, the sequence of "highest-priority currently-affordable
    /// action" is fully deterministic and simply runs longer as Chessmen/Material grow (a
    /// smaller budget's run is always an exact prefix of a larger budget's run using the same
    /// seed). The seeded Random is consulted only on a genuine tie -- two or more distinct
    /// configured actions sharing one priority value, including two that also share the same
    /// FromTier (e.g. pawn-to-minor/pawn-to-major, both gateways out of Pawn) -- weighted by
    /// eligible-count-at-that-tier times each action's configured proportion.
    /// </summary>
    private static Dictionary<string, int> Simulate(
      int placeableSlots,
      int lockedMajorCount,
      int nonPawnSlotCapacity,
      ApmwConfig config,
      ref int spareMaterial,
      out int[] tierCounts)
    {
      List<GraduationAction> actions = BuildActions(config);

      // Every action sharing a priority level is a genuine competitor for that level's single
      // per-iteration draw -- including two actions that share both priority and FromTier
      // (e.g. pawn-to-minor/pawn-to-major). There is no more "keep the cheaper one" collision
      // -avoidance: ties are the point of the proportion mechanism, not an error case to route
      // around.
      Dictionary<int, List<GraduationAction>> actionsByPriority = actions
        .GroupBy(action => action.Priority)
        .ToDictionary(group => group.Key, group => group.ToList());

      // Priority levels -- and which actions exist at each -- are fixed once the action set is
      // built; only the per-tier counts change as slots graduate.
      List<int> priorityLevelsDescending = actionsByPriority.Keys.OrderByDescending(priority => priority).ToList();

      tierCounts = new int[TierCount];
      tierCounts[(int)ChessmanTier.Pawn] = Math.Max(0, placeableSlots - lockedMajorCount);
      tierCounts[(int)ChessmanTier.Major] = lockedMajorCount;

      Dictionary<string, int> appliedCounts = new Dictionary<string, int>(StringComparer.Ordinal);
      Dictionary<int, long> tieCounters = new Dictionary<int, long>();
      List<GraduationAction> viable = new List<GraduationAction>();
      List<double> viableWeights = new List<double>();

      while (true)
      {
        bool applied = false;
        foreach (int priority in priorityLevelsDescending)
        {
          viable.Clear();
          viableWeights.Clear();
          foreach (GraduationAction action in actionsByPriority[priority])
          {
            int eligible = EligibleCount(tierCounts, lockedMajorCount, action.FromTier);
            if (eligible <= 0)
              continue;

            if (action.FromTier == ChessmanTier.Pawn &&
              placeableSlots - tierCounts[(int)ChessmanTier.Pawn] >= nonPawnSlotCapacity)
              continue;

            if (action.IncrementalCost > spareMaterial)
              continue;

            viable.Add(action);
            viableWeights.Add(eligible * action.Proportion);
          }

          if (viable.Count == 0)
            continue; // Nothing usable at this level right now -- drop to the next-lower one.

          GraduationAction chosen = viable.Count == 1
            ? viable[0]
            : ChooseWeighted(
              viable,
              viableWeights,
              ApmwSeedSeries.Semantic(config, "fundamental.wave.tie." + priority),
              tieCounters.TryGetValue(priority, out long counter) ? counter : 0);
          if (viable.Count > 1)
            tieCounters[priority] = tieCounters.TryGetValue(priority, out long current) ? current + 1 : 1;

          spareMaterial -= chosen.IncrementalCost;
          tierCounts[(int)chosen.FromTier]--;
          tierCounts[(int)chosen.ToTier]++;

          int currentCount;
          appliedCounts[chosen.Key] = appliedCounts.TryGetValue(chosen.Key, out currentCount) ? currentCount + 1 : 1;

          applied = true;
          break; // Reset the scan to the top priority: the globally most-preferred still-
                 // affordable action should always be tried first.
        }

        if (!applied)
          break;
      }

      return appliedCounts;
    }

    // Locked Castler majors are baked into tierCounts[Major] but are permanently excluded from
    // candidacy (they must remain castle-eligible), so they're subtracted back out here.
    private static int EligibleCount(int[] tierCounts, int lockedMajorCount, ChessmanTier tier)
    {
      int count = tierCounts[(int)tier];
      if (tier == ChessmanTier.Major)
        count -= lockedMajorCount;
      return count;
    }

    private static GraduationAction ChooseWeighted(
      List<GraduationAction> actions,
      List<double> weights,
      CounterBasedSeedSeries series,
      long counter)
    {
      return actions[WeightedTieBreak.ChooseIndex(weights, series, counter)];
    }

    private static List<GraduationAction> BuildActions(ApmwConfig config)
    {
      List<GraduationAction> actions = new List<GraduationAction>();

      // Every tier transition -- including the two gateways out of Pawn -- is now a normal,
      // symmetric, opt-in configured action (config.PieceUpgradeActions), exactly like Legacy's
      // MinorToMajor/MajorToQueen/etc: none of these are unconditionally available. A slot can
      // reach Major either by first passing through Minor (PawnToMinor then MinorToMajor) or
      // directly (PawnToMajor); Jack is still only reachable via Minor/Major. If nothing is
      // configured at all, every slot simply remains a Pawn -- a well-defined, deliberate
      // outcome, not a special case.
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.PawnToMinor, ChessmanTier.Pawn, ChessmanTier.Minor, ItemGenerationValues.Minor);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.PawnToMajor, ChessmanTier.Pawn, ChessmanTier.Major, ItemGenerationValues.Major);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.MinorToMajor, ChessmanTier.Minor, ChessmanTier.Major, ItemGenerationValues.Major);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.MajorToJack, ChessmanTier.Major, ChessmanTier.Jack, ItemGenerationValues.Jack);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.MinorToJack, ChessmanTier.Minor, ChessmanTier.Jack, ItemGenerationValues.Jack);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.MajorToQueen, ChessmanTier.Major, ChessmanTier.Queen, ItemGenerationValues.Queen);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.JackToQueen, ChessmanTier.Jack, ChessmanTier.Queen, ItemGenerationValues.Queen);
      AddConfiguredAction(actions, config, ApmwConstants.PieceUpgradeActions.QueenToAmazon, ChessmanTier.Queen, ChessmanTier.Amazon, ItemGenerationValues.Amazon);

      return actions;
    }

    private static void AddConfiguredAction(
      List<GraduationAction> actions,
      ApmwConfig config,
      string actionName,
      ChessmanTier fromTier,
      ChessmanTier toTier,
      int expectedMaterial)
    {
      ApmwConfig.PieceUpgradeActionResolution action;
      if (!config.PieceUpgradeActions.TryGetValue(actionName, out action) || !action.IsEnabled || action.Priority <= 0)
        return;

      actions.Add(new GraduationAction(actionName, fromTier, toTier, expectedMaterial, action.Priority, action.Proportion));
    }

    private static NonPawnGenerationPlan BuildNonPawnPlan(
      int[] finalTierCounts,
      Dictionary<string, int> appliedCounts,
      int lockedMajorCount)
    {
      // PawnToMinor and PawnToMajor are the two gateways out of Pawn. Minor is still the entry
      // point for the Minor->Major/->Jack/->Queen/->Amazon branch of the tier graph: every
      // non-locked slot that leaves Pawn via PawnToMinor gets exactly one placeholder piece,
      // placed once as Minor, and every later tier change for that same slot happens via
      // in-place substitution in ApplyUpgrades below, never a second fresh placeholder. So only
      // directCounts[Minor] needs "pass-through" additions for slots that continued beyond Minor
      // (AppliedCount(MinorToMajor) / AppliedCount(MinorToJack)); any slot that went on to
      // Major/Jack/Queen/Amazon from there already has its physical square reserved by that
      // Minor placeholder. PawnToMajor is a second, direct gateway straight to Major -- those
      // slots never have a Minor placeholder at all, so they're seeded directly into
      // directCounts[Major] instead, exactly like the castler-locked Major pool (which is also
      // pre-seeded directly and never passes through Minor). Both direct-Major populations are
      // still fully eligible for further substitution (MajorToJack/MajorToQueen) except the
      // locked ones, which PreferredSourceIndices protects by majorOrder position, not by count.
      Dictionary<NonPawnPieceFamily, int> directCounts = new Dictionary<NonPawnPieceFamily, int>
      {
        [NonPawnPieceFamily.Minor] = finalTierCounts[(int)ChessmanTier.Minor]
          + AppliedCount(appliedCounts, ApmwConstants.PieceUpgradeActions.MinorToMajor)
          + AppliedCount(appliedCounts, ApmwConstants.PieceUpgradeActions.MinorToJack),
        [NonPawnPieceFamily.Major] = lockedMajorCount
          + AppliedCount(appliedCounts, ApmwConstants.PieceUpgradeActions.PawnToMajor),
        [NonPawnPieceFamily.Jack] = 0,
        [NonPawnPieceFamily.Queen] = 0,
        [NonPawnPieceFamily.Amazon] = 0,
      };

      // Fixed topological order over the (small, hardcoded) upgrade DAG -- every action's
      // target tier is strictly "later" than its source tier -- so ApplyUpgrades always finds
      // the placeholder pieces it needs already on the board when it substitutes them onward.
      // (A slot can only ever reach Amazon by actually visiting Queen as a real simulation step
      // first, so MajorToQueen/JackToQueen entries always precede QueenToAmazon here whenever
      // it fired -- no special-cased compound recipe is needed, unlike the old recipe planner.)
      List<PlannedNonPawnUpgradeAction> upgradeActions = new List<PlannedNonPawnUpgradeAction>();
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.MinorToMajor);
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.MinorToJack);
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.MajorToJack);
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.MajorToQueen);
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.JackToQueen);
      AddPlannedAction(upgradeActions, appliedCounts, ApmwConstants.PieceUpgradeActions.QueenToAmazon);

      Dictionary<NonPawnPieceFamily, int> unusedUpgradeCounts = new Dictionary<NonPawnPieceFamily, int>
      {
        [NonPawnPieceFamily.Queen] = 0,
        [NonPawnPieceFamily.Amazon] = 0,
      };

      return new NonPawnGenerationPlan(directCounts, upgradeActions, unusedUpgradeCounts, lockedMajorCount);
    }

    private static void AddPlannedAction(
      List<PlannedNonPawnUpgradeAction> upgradeActions,
      Dictionary<string, int> appliedCounts,
      string actionName)
    {
      int count = AppliedCount(appliedCounts, actionName);
      if (count <= 0)
        return;

      NonPawnUpgradeActionMetadata metadata = NonPawnUpgradeGeneration.MetadataFor(actionName);
      if (metadata == null)
        return;

      upgradeActions.Add(new PlannedNonPawnUpgradeAction(metadata, count));
    }

    private static int AppliedCount(Dictionary<string, int> appliedCounts, string key)
    {
      int count;
      return appliedCounts.TryGetValue(key, out count) ? count : 0;
    }

    private static int ActiveCastlerCount(
      ApmwCore core,
      int materialBudget,
      int nonPawnSlotCapacity,
      int castlingSlotCapacity)
    {
      return Math.Min(
        Math.Min(ItemGenerationValues.CastlerMaximum, Math.Max(0, core.EffectiveFoundCastlers)),
        Math.Min(
          Math.Min(nonPawnSlotCapacity, materialBudget / CastlerMaterialCost),
          Math.Max(0, castlingSlotCapacity)));
    }

    private static int MaxGeneratedNonKingPieces(int numFiles)
    {
      return Math.Max(0, 5 * numFiles - 1);
    }

    private static int MaxNonPawnPipelineSlots(int numFiles)
    {
      return Math.Max(0, 2 * numFiles - 1);
    }

    private static int MaxCastlingMajorSlots(int numFiles)
    {
      return Math.Max(0, numFiles - 1);
    }
  }

  internal static class NonPawnUpgradeGeneration
  {
    private const int FamilyCount = 5;

    private static readonly NonPawnUpgradeActionMetadata[] UpgradeActions =
    {
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.MinorToMajor,
        NonPawnPieceFamily.Minor,
        NonPawnPieceFamily.Major,
        ItemGenerationValues.Major - ItemGenerationValues.Minor),
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.MajorToJack,
        NonPawnPieceFamily.Major,
        NonPawnPieceFamily.Jack,
        ItemGenerationValues.Jack - ItemGenerationValues.Major),
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.MinorToJack,
        NonPawnPieceFamily.Minor,
        NonPawnPieceFamily.Jack,
        ItemGenerationValues.Jack - ItemGenerationValues.Minor),
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.MajorToQueen,
        NonPawnPieceFamily.Major,
        NonPawnPieceFamily.Queen,
        ItemGenerationValues.Queen - ItemGenerationValues.Major),
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.JackToQueen,
        NonPawnPieceFamily.Jack,
        NonPawnPieceFamily.Queen,
        ItemGenerationValues.Queen - ItemGenerationValues.Jack),
      new NonPawnUpgradeActionMetadata(
        ApmwConstants.PieceUpgradeActions.QueenToAmazon,
        NonPawnPieceFamily.Queen,
        NonPawnPieceFamily.Amazon,
        ItemGenerationValues.Amazon - ItemGenerationValues.Queen),
    };

    public static NonPawnGenerationPlan Plan(ApmwCore core, ApmwConfig config)
    {
      int[] foundCounts =
      {
        Math.Max(0, core.foundMinors),
        Math.Max(0, core.foundMajors),
        Math.Max(0, core.foundJacks),
        Math.Max(0, core.foundQueens),
        Math.Max(0, core.foundAmazons),
      };

      return Plan(foundCounts, config, 0);
    }

    public static NonPawnGenerationPlan Plan(PieceGenerationAllocation allocation, ApmwConfig config)
    {
      int[] foundCounts =
      {
        Math.Max(0, allocation.NonPawnCount(NonPawnPieceFamily.Minor)),
        Math.Max(0, allocation.NonPawnCount(NonPawnPieceFamily.Major)),
        Math.Max(0, allocation.NonPawnCount(NonPawnPieceFamily.Jack)),
        Math.Max(0, allocation.NonPawnCount(NonPawnPieceFamily.Queen)),
        Math.Max(0, allocation.NonPawnCount(NonPawnPieceFamily.Amazon)),
      };

      return Plan(foundCounts, config, allocation.LockedMajorCount);
    }

    private static NonPawnGenerationPlan Plan(int[] foundCounts, ApmwConfig config, int lockedMajorCount)
    {
      foundCounts = (int[])foundCounts.Clone();
      for (int index = 0; index < foundCounts.Length; index++)
        foundCounts[index] = Math.Max(0, foundCounts[index]);
      lockedMajorCount = Math.Min(Math.Max(0, lockedMajorCount), foundCounts[(int)NonPawnPieceFamily.Major]);

      int[,] currentByFamilyAndOrigin = new int[FamilyCount, FamilyCount];
      currentByFamilyAndOrigin[(int)NonPawnPieceFamily.Minor, (int)NonPawnPieceFamily.Minor] =
        foundCounts[(int)NonPawnPieceFamily.Minor];
      currentByFamilyAndOrigin[(int)NonPawnPieceFamily.Major, (int)NonPawnPieceFamily.Major] =
        Math.Max(0, foundCounts[(int)NonPawnPieceFamily.Major] - lockedMajorCount);
      currentByFamilyAndOrigin[(int)NonPawnPieceFamily.Jack, (int)NonPawnPieceFamily.Jack] =
        foundCounts[(int)NonPawnPieceFamily.Jack];

      int[] remainingTargetBudgets = (int[])foundCounts.Clone();
      Dictionary<NonPawnPieceFamily, int> directCounts = new Dictionary<NonPawnPieceFamily, int>
      {
        [NonPawnPieceFamily.Minor] = foundCounts[(int)NonPawnPieceFamily.Minor],
        [NonPawnPieceFamily.Major] = foundCounts[(int)NonPawnPieceFamily.Major],
        [NonPawnPieceFamily.Jack] = foundCounts[(int)NonPawnPieceFamily.Jack],
        [NonPawnPieceFamily.Queen] = 0,
        [NonPawnPieceFamily.Amazon] = 0,
      };

      // Grouped once by current priority, descending -- config doesn't change mid-plan -- and,
      // within a group, in the same fixed declaration order as UpgradeActions/
      // ValidPieceUpgradeActions. Mirrors FundamentalSlotGraduationPlanner.Simulate: a genuine
      // tie (two actions sharing both priority AND SourceFamily, e.g. minor-to-major/
      // minor-to-jack, both Minor-sourced) is now resolved by the same weighted-per-unit-draw
      // discipline instead of silently handing the whole shared budget to whichever action
      // happens to sit first in this fixed order.
      List<List<NonPawnUpgradeActionMetadata>> actionsByPriorityDescending = UpgradeActions
        .Where(metadata => config.IsPieceUpgradeActionEnabled(metadata.ActionName)
          && config.PieceUpgradeActions[metadata.ActionName].Priority > 0)
        .GroupBy(metadata => config.PieceUpgradeActions[metadata.ActionName].Priority)
        .OrderByDescending(group => group.Key)
        .Select(group => group.ToList())
        .ToList();

      Dictionary<string, int> appliedCounts = new Dictionary<string, int>(StringComparer.Ordinal);
      Dictionary<string, long> sourceSeriesCounters = new Dictionary<string, long>(StringComparer.Ordinal);
      List<NonPawnUpgradeActionMetadata> viable = new List<NonPawnUpgradeActionMetadata>();
      List<double> viableWeights = new List<double>();

      while (true)
      {
        bool applied = false;
        foreach (List<NonPawnUpgradeActionMetadata> actionsAtPriority in actionsByPriorityDescending)
        {
          viable.Clear();
          viableWeights.Clear();
          foreach (NonPawnUpgradeActionMetadata metadata in actionsAtPriority)
          {
            int targetFamily = (int)metadata.TargetFamily;
            bool hasTargetBudget = IsUpgradeOnlyFamily(metadata.TargetFamily)
              ? remainingTargetBudgets[targetFamily] > 0
              : remainingTargetBudgets[targetFamily] > 0 && currentByFamilyAndOrigin[targetFamily, targetFamily] > 0;
            if (!hasTargetBudget)
              continue;

            // An upgrade can only actually happen for as many pieces as the source family
            // currently has available. Without this check, a target-side-only budget (e.g.
            // minor-to-major with 0 minors on the board) would still "reserve"
            // directCounts/remainingTargetBudgets/currentByFamilyAndOrigin for a piece that
            // MovePlannedSources below can never actually supply -- silently vanishing a piece
            // that no action ever really touched, instead of just leaving the action a no-op.
            int sourceEligible = CurrentFamilyCount(currentByFamilyAndOrigin, (int)metadata.SourceFamily);
            if (sourceEligible <= 0)
              continue;

            viable.Add(metadata);
            viableWeights.Add(sourceEligible * config.PieceUpgradeActions[metadata.ActionName].Proportion);
          }

          if (viable.Count == 0)
            continue; // Nothing usable at this level right now -- drop to the next-lower one.

          NonPawnUpgradeActionMetadata chosen = viable.Count == 1
            ? viable[0]
            : ChooseWeightedByUpgradeSourceSeries(
              viable,
              viableWeights,
              config,
              sourceSeriesCounters);
          if (viable.Count > 1)
          {
            foreach (NonPawnUpgradeActionMetadata candidate in viable)
            {
              long counter = sourceSeriesCounters.TryGetValue(candidate.ActionName, out long current)
                ? current
                : 0;
              sourceSeriesCounters[candidate.ActionName] = counter + 1;
            }
          }

          int chosenTargetFamily = (int)chosen.TargetFamily;
          remainingTargetBudgets[chosenTargetFamily] -= 1;
          if (directCounts.ContainsKey(chosen.TargetFamily))
            directCounts[chosen.TargetFamily] = Math.Max(0, directCounts[chosen.TargetFamily] - 1);
          if (!IsUpgradeOnlyFamily(chosen.TargetFamily))
            currentByFamilyAndOrigin[chosenTargetFamily, chosenTargetFamily] =
              Math.Max(0, currentByFamilyAndOrigin[chosenTargetFamily, chosenTargetFamily] - 1);

          MovePlannedSources(currentByFamilyAndOrigin, (int)chosen.SourceFamily, chosenTargetFamily, 1);

          int currentCount;
          appliedCounts[chosen.ActionName] = appliedCounts.TryGetValue(chosen.ActionName, out currentCount) ? currentCount + 1 : 1;

          applied = true;
          break; // Reset the scan to the top priority: the globally most-preferred still-
                 // viable action should always be tried first for the next unit too.
        }

        if (!applied)
          break;
      }

      // Same priority-descending, declaration-order-tiebreak sequence as the actions were
      // drawn in, now flattened to one aggregated entry per action name (regardless of how many
      // individual per-unit draws contributed to its total, or how they were interleaved with
      // any tied competitor).
      List<PlannedNonPawnUpgradeAction> plannedActions = actionsByPriorityDescending
        .SelectMany(actionsAtPriority => actionsAtPriority)
        .Where(metadata => appliedCounts.ContainsKey(metadata.ActionName))
        .Select(metadata => new PlannedNonPawnUpgradeAction(metadata, appliedCounts[metadata.ActionName]))
        .ToList();

      Dictionary<NonPawnPieceFamily, int> unusedUpgradeCounts = new Dictionary<NonPawnPieceFamily, int>
      {
        [NonPawnPieceFamily.Queen] = remainingTargetBudgets[(int)NonPawnPieceFamily.Queen],
        [NonPawnPieceFamily.Amazon] = remainingTargetBudgets[(int)NonPawnPieceFamily.Amazon],
      };

      return new NonPawnGenerationPlan(directCounts, plannedActions, unusedUpgradeCounts, lockedMajorCount);
    }

    private static NonPawnUpgradeActionMetadata ChooseWeightedByUpgradeSourceSeries(
      List<NonPawnUpgradeActionMetadata> actions,
      List<double> weights,
      ApmwConfig config,
      Dictionary<string, long> counters)
    {
      bool hasPositiveWeight = weights.Any(weight => weight > 0);
      int chosenIndex = 0;
      double chosenScore = double.PositiveInfinity;
      ulong chosenUniform = ulong.MaxValue;
      for (int index = 0; index < actions.Count; index++)
      {
        string actionName = actions[index].ActionName;
        long counter = counters.TryGetValue(actionName, out long current) ? current : 0;
        CounterBasedSeedSeries series = ApmwSeedSeries.UpgradeSource(config, actionName);
        if (hasPositiveWeight)
        {
          if (weights[index] <= 0)
            continue;
          double unit = Math.Max(double.Epsilon, series.Unit(counter));
          double score = -Math.Log(unit) / weights[index];
          if (score < chosenScore)
          {
            chosenScore = score;
            chosenIndex = index;
          }
        }
        else
        {
          ulong uniform = series.Value(counter);
          if (uniform < chosenUniform)
          {
            chosenUniform = uniform;
            chosenIndex = index;
          }
        }
      }
      return actions[chosenIndex];
    }

    public static List<PieceType> ApplyUpgrades(
      int numFiles,
      List<PieceType> pieces,
      List<int> majorOrder,
      NonPawnGenerationPlan plan,
      List<string> promotions,
      ref int spareMaterial)
    {
      foreach (PlannedNonPawnUpgradeAction plannedAction in plan.UpgradeActions)
      {
        NonPawnUpgradeActionMetadata metadata = plannedAction.Metadata;
        pieces = NonPawnFamilySubstitution.Substitute(
          new NonPawnFamilySubstitutionRequest(
            pieces,
            promotions,
            PiecesForFamily(metadata.SourceFamily),
            PreferredSourceIndices(metadata, numFiles, majorOrder, pieces.Count, plan.LockedMajorCount),
            false,
            PiecesForFamily(metadata.TargetFamily),
            plannedAction.RequestedUpgrades,
            SeedForFamily(metadata.TargetFamily),
            TypeLimitForFamily(metadata.TargetFamily),
            metadata.UpgradeMaterialCredit),
          ref spareMaterial);
      }

      RecordUnusedUpgradeOnlyCounts(plan, ref spareMaterial);

      return pieces;
    }

    private static bool IsUpgradeOnlyFamily(NonPawnPieceFamily family)
    {
      return family == NonPawnPieceFamily.Queen || family == NonPawnPieceFamily.Amazon;
    }

    private static void RecordUnusedUpgradeOnlyCounts(NonPawnGenerationPlan plan, ref int spareMaterial)
    {
      foreach (var unusedUpgradeCount in plan.UnusedUpgradeCounts)
      {
        int expectedMaterial =
          unusedUpgradeCount.Key == NonPawnPieceFamily.Queen
            ? ItemGenerationValues.Queen - ItemGenerationValues.Major
            : ItemGenerationValues.Amazon - ItemGenerationValues.Queen;
        PieceMaterialAccounting.RecordUnusedUpgradeCredit(
          unusedUpgradeCount.Value,
          expectedMaterial,
          ref spareMaterial);
      }
    }

    internal static NonPawnUpgradeActionMetadata MetadataFor(string actionName)
    {
      return UpgradeActions.FirstOrDefault(action => action.ActionName == actionName);
    }

    private static int CurrentFamilyCount(int[,] currentByFamilyAndOrigin, int family)
    {
      int count = 0;
      for (int origin = 0; origin < FamilyCount; origin++)
        count += currentByFamilyAndOrigin[family, origin];
      return count;
    }

    private static void MovePlannedSources(
      int[,] currentByFamilyAndOrigin,
      int sourceFamily,
      int targetFamily,
      int replacementsToPlan)
    {
      foreach (int origin in PreferredOrigins(sourceFamily))
      {
        if (replacementsToPlan <= 0)
          break;

        int moved = Math.Min(replacementsToPlan, currentByFamilyAndOrigin[sourceFamily, origin]);
        currentByFamilyAndOrigin[sourceFamily, origin] -= moved;
        currentByFamilyAndOrigin[targetFamily, origin] += moved;
        replacementsToPlan -= moved;
      }
    }

    private static IEnumerable<int> PreferredOrigins(int sourceFamily)
    {
      yield return sourceFamily;
      for (int origin = 0; origin < FamilyCount; origin++)
        if (origin != sourceFamily)
          yield return origin;
    }

    private static IEnumerable<PieceType> PiecesForFamily(NonPawnPieceFamily family)
    {
      var core = ApmwCore.getInstance();
      switch (family)
      {
        case NonPawnPieceFamily.Minor:
          return core.minors;
        case NonPawnPieceFamily.Major:
          return core.majors;
        case NonPawnPieceFamily.Jack:
          return core.jacks;
        case NonPawnPieceFamily.Queen:
          return core.queens;
        case NonPawnPieceFamily.Amazon:
          return core.amazons;
        default:
          return Enumerable.Empty<PieceType>();
      }
    }

    private static int SeedForFamily(NonPawnPieceFamily family)
    {
      var config = ApmwConfig.getInstance();
      switch (family)
      {
        case NonPawnPieceFamily.Minor:
          return config.minorSeed;
        case NonPawnPieceFamily.Major:
        case NonPawnPieceFamily.Jack:
          return config.majorSeed;
        case NonPawnPieceFamily.Queen:
        case NonPawnPieceFamily.Amazon:
          return config.queenSeed;
        default:
          return config.majorSeed;
      }
    }

    private static int TypeLimitForFamily(NonPawnPieceFamily family)
    {
      var config = ApmwConfig.getInstance();
      switch (family)
      {
        case NonPawnPieceFamily.Minor:
          return config.minorTypeLimit;
        case NonPawnPieceFamily.Major:
        case NonPawnPieceFamily.Jack:
          return config.majorTypeLimit;
        case NonPawnPieceFamily.Queen:
        case NonPawnPieceFamily.Amazon:
          return config.queenTypeLimit;
        default:
          return -1;
      }
    }

    private static IEnumerable<int> PreferredSourceIndices(
      NonPawnUpgradeActionMetadata metadata,
      int numFiles,
      List<int> majorOrder,
      int pieceSetCount,
      int lockedMajorCount)
    {
      if (metadata.SourceFamily != NonPawnPieceFamily.Major &&
        metadata.SourceFamily != NonPawnPieceFamily.Jack)
        return null;

      List<int> preferredIndices = new List<int>();
      HashSet<int> lockedPieceSetIndices = new HashSet<int>();
      int lockedOrderCount = 0;
      if (metadata.SourceFamily == NonPawnPieceFamily.Major)
      {
        lockedOrderCount = Math.Min(Math.Max(0, lockedMajorCount), majorOrder.Count);
        foreach (int lockedOrderIndex in majorOrder.Take(lockedOrderCount))
          lockedPieceSetIndices.Add(MajorUpgradeSubstitution.MajorOrderIndexToPieceSetIndex(
            numFiles,
            lockedOrderIndex,
            pieceSetCount));
      }

      foreach (int orderIndex in majorOrder.Skip(lockedOrderCount).Reverse())
      {
        int pieceSetIndex = MajorUpgradeSubstitution.MajorOrderIndexToPieceSetIndex(
          numFiles,
          orderIndex,
          pieceSetCount);
        if (!lockedPieceSetIndices.Contains(pieceSetIndex) && !preferredIndices.Contains(pieceSetIndex))
          preferredIndices.Add(pieceSetIndex);
      }

      preferredIndices.AddRange(
        Enumerable.Range(0, pieceSetCount)
          .Where(index => !lockedPieceSetIndices.Contains(index) && !preferredIndices.Contains(index)));
      return preferredIndices;
    }
  }

  internal static class PlayerPieceSetGeneration
  {
    public static (Dictionary<KeyValuePair<int, int>, PieceType>, string) Generate(int numFiles)
    {
      ApmwConfig.getInstance().seed();
      List<string> promotions = new List<string>();
      List<int> order;
      var core = ApmwCore.getInstance();
      var config = ApmwConfig.getInstance();
      PieceGenerationAllocation allocation = PieceGenerationAllocation.FromCore(core, config, numFiles);
      int spareMaterial = allocation.InitialSpareMaterial;
      NonPawnGenerationPlan nonPawnPlan = allocation.PrecomputedNonPawnPlan ?? NonPawnUpgradeGeneration.Plan(allocation, config);
      // Generate direct pieces only from target budgets that were not reserved by upgrade actions.
      List<PieceType> withMajors = MajorPieceGeneration.GenerateDirect(
        numFiles,
        nonPawnPlan.DirectCount(NonPawnPieceFamily.Major),
        nonPawnPlan.DirectCount(NonPawnPieceFamily.Jack),
        nonPawnPlan.LockedMajorCount,
        out order,
        promotions,
        ref spareMaterial);
      List<PieceType> withMinors = MinorPieceGeneration.GenerateDirect(
        numFiles,
        withMajors,
        nonPawnPlan.DirectCount(NonPawnPieceFamily.Minor),
        promotions,
        ref spareMaterial);
      List<PieceType> withUpgrades = NonPawnUpgradeGeneration.ApplyUpgrades(
        numFiles,
        withMinors,
        order,
        nonPawnPlan,
        promotions,
        ref spareMaterial);
      List<PieceType> withPawns;
      if (allocation.LimitsNonKingPieceSlots)
      {
        int remainingPawnSlots = allocation.RemainingPawnSlotsAfterNonKingPieces(withUpgrades);
        withPawns = PawnGeneration.GeneratePawns(
          numFiles,
          withUpgrades,
          spareMaterial,
          allocation.PawnSlots,
          remainingPawnSlots,
          remainingPawnSlots);
      }
      else
      {
        withPawns = PawnGeneration.GeneratePawns(numFiles, withUpgrades, spareMaterial);
      }

      Dictionary<KeyValuePair<int, int>, PieceType> pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int rankIndex = 0; rankIndex < 5; rankIndex++)
        for (int fileIndex = 0; fileIndex < numFiles; fileIndex++)
        {
          PieceType piece = withPawns[rankIndex * numFiles + fileIndex];
          if (piece != null)
          {
            var coordinate = new BoardCoordinate(4 - rankIndex, fileIndex);
            pieces.Add(coordinate.ToKeyValuePair(), piece);
          }
        }

      return (pieces, string.Join("", promotions));
    }
  }

  internal static class PawnGeneration
  {
    private enum PawnUpgrade
    {
      Core,
      Min,
      Best,
      Sergeant
    }

    public static List<PieceType> SetupPawnOptions()
    {
      var config = ApmwConfig.getInstance();
      var standardPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("P"));
      var berolinaPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("Ŕ"));
      var checkersPawn = ApmwCore.getInstance().pawns.First(item => item.Name.Equals("Checkers"));

      switch (config.Pawns)
      {
        case FairyPawns.Mixed:
          return ApmwCore.getInstance().pawns.ToList();
        case FairyPawns.AnyPawn:
          return new List<PieceType>() { standardPawn, berolinaPawn };
        case FairyPawns.AnyFairy:
          return new List<PieceType>() { berolinaPawn, checkersPawn };
        case FairyPawns.AnyClassical:
          return new List<PieceType>() { standardPawn, checkersPawn };
        case FairyPawns.Vanilla:
          return new List<PieceType>() { standardPawn };
        case FairyPawns.Berolina:
          return new List<PieceType>() { berolinaPawn };
        case FairyPawns.Checkers:
          return new List<PieceType>() { checkersPawn };
        default:
          return new List<PieceType>() { standardPawn };
      }
    }

    public static PieceType GetNextPawn(Random randomSource, List<PieceType> options)
    {
      return GetNextPawn(randomSource, options, PawnUpgrade.Core);
    }

    private static PieceType GetNextPawn(Random randomSource, List<PieceType> options, PawnUpgrade upgrade)
    {
      List<PieceType> limited;
      switch (upgrade)
      {
        case PawnUpgrade.Sergeant:
          if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
            return ApmwCore.getInstance().sergeants.First(s => s.Name == "Sergeant");
          else
            return ApmwCore.getInstance().sergeants.ElementAt(
              randomSource.Next(
                ApmwCore.getInstance().sergeants.Count));
        case PawnUpgrade.Best:
          limited = options.Where(item => item.MidgameValue >= ItemGenerationValues.Weak).ToList();
          if (limited.Count == 0)
            return GetNextPawn(randomSource, options, PawnUpgrade.Sergeant);
          return limited.ElementAt(randomSource.Next(limited.Count));
        case PawnUpgrade.Min:
          limited = options.Where(item => item.MidgameValue <= options.Min(item => item.MidgameValue)).ToList();
          return limited.ElementAt(randomSource.Next(limited.Count));
        case PawnUpgrade.Core:
        default:
          return options.ElementAt(randomSource.Next(options.Count));
      }
    }

    private static void FillPawnRank(List<PieceType> targetRank, int numFiles, int startIndex, Queue<PieceType> adjustedPawns,
        Random randomLocations)
    {
      for (int i = startIndex;
        i < numFiles * (startIndex / numFiles + 1)
          && adjustedPawns.Count > 0
          && targetRank.Count(item => item == null) > 0;
        i++)
      {
        var piece = adjustedPawns.Dequeue();
        PiecePlacement.ChooseIndexAndPlace(targetRank, randomLocations, piece);
      }
    }

    public static List<PieceType> PickPawns(Random randomPieces, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      return PickPawns(randomPieces, SetupPawnOptions(), adjustedPawnValues, remainingPawnSpaces, foundPawns);
    }

    public static List<PieceType> PickPawns(Random randomPieces, List<PieceType> pawnOptions, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      var config = ApmwConfig.getInstance();
      var sergeants = ApmwCore.getInstance().sergeants.ToList();
      List<PieceType> workingPawns = new List<PieceType>();
      while (workingPawns.Count < remainingPawnSpaces && adjustedPawnValues > 0)
      {
        PieceType picked = PickPawnUsingUpgradePreferences(
          randomPieces, pawnOptions, sergeants, adjustedPawnValues, foundPawns, workingPawns.Count, config);
        if (picked == null) break;
        workingPawns.Add(picked);
        adjustedPawnValues -= picked.MidgameValue;
      }
      // TODO(chesslogic): Add an Option not to upgrade pawns. For now, we'll always maximize material value.
      UpgradePawns(randomPieces, adjustedPawnValues, pawnOptions, workingPawns, config);
      return workingPawns;
    }

    private static PieceType PickPawnUsingUpgradePreferences(
        Random randomPieces, List<PieceType> pawnOptions, List<PieceType> sergeants,
        int budget, int foundPawns, int currentCount, ApmwConfig config)
    {
      if (ShouldApplyPoolPawnUpgradeAction(config))
        return ApplyPoolPawnUpgradeAction(randomPieces, pawnOptions, sergeants, budget, foundPawns, currentCount);

      if (ShouldApplyBetterPawnActionBeforeMorePawn(config))
      {
        var upgradedPawn = ApplyBetterPawnAction(randomPieces, pawnOptions, budget, foundPawns, currentCount);
        if (upgradedPawn != null)
          return upgradedPawn;
      }

      return PickNewOrMorePawnAction(randomPieces, pawnOptions, budget, foundPawns, currentCount, config);
    }

    private static bool ShouldApplyPoolPawnUpgradeAction(ApmwConfig config)
    {
      return config.IsPieceUpgradeActionPreferredBefore(
          ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
          ApmwConstants.PieceUpgradeActions.BetterPawn)
        && config.IsPieceUpgradeActionPreferredBefore(
          ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
          ApmwConstants.PieceUpgradeActions.MorePawn);
    }

    private static bool ShouldApplyBetterPawnActionBeforeMorePawn(ApmwConfig config)
    {
      return config.IsPieceUpgradeActionPreferredBefore(
        ApmwConstants.PieceUpgradeActions.BetterPawn,
        ApmwConstants.PieceUpgradeActions.MorePawn);
    }

    private static PieceType PickNewOrMorePawnAction(
        Random randomPieces, List<PieceType> pawnOptions,
        int budget, int foundPawns, int currentCount, ApmwConfig config)
    {
      if (currentCount < foundPawns)
        return ApplyNewPawnAction(randomPieces, pawnOptions, budget, foundPawns, currentCount);

      if (!config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.MorePawn))
        return null;

      return ApplyMorePawnAction(randomPieces, pawnOptions, budget, foundPawns, currentCount);
    }

    private static PieceType ApplyNewPawnAction(
        Random randomPieces, List<PieceType> pawnOptions,
        int budget, int foundPawns, int currentCount)
    {
      return GetNextPawn(randomPieces, pawnOptions, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    private static PieceType ApplyMorePawnAction(
        Random randomPieces, List<PieceType> pawnOptions,
        int budget, int foundPawns, int currentCount)
    {
      return GetNextPawn(randomPieces, pawnOptions, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    private static PieceType ApplyPoolPawnUpgradeAction(Random randomPieces, List<PieceType> pawnOptions, List<PieceType> sergeants,
        int budget, int foundPawns, int currentCount)
    {
      var augmented = pawnOptions.Concat(sergeants).ToList();
      if (augmented.Count == 0) return null;
      var candidate = augmented[randomPieces.Next(augmented.Count)];
      if (!sergeants.Contains(candidate)) return candidate;
      bool allowed = currentCount >= foundPawns
        ? budget >= candidate.MidgameValue
        : PigeonholeAllowsSergeant(budget, foundPawns, currentCount, candidate.MidgameValue, pawnOptions);
      if (allowed) return candidate;
      // Sergeant rejected; re-pick uniformly from non-sergeant options.
      var nonSerg = pawnOptions.Where(p => !sergeants.Contains(p)).ToList();
      if (nonSerg.Count == 0) return null;
      return GetNextPawn(randomPieces, nonSerg, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    private static PieceType ApplyBetterPawnAction(Random randomPieces, List<PieceType> pawnOptions,
        int budget, int foundPawns, int currentCount)
    {
      var sergeant = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
      bool allowed = currentCount >= foundPawns
        ? budget >= sergeant.MidgameValue
        : PigeonholeAllowsSergeant(budget, foundPawns, currentCount, sergeant.MidgameValue, pawnOptions);
      if (allowed) return sergeant;
      return null;
    }

    // Slot-aware fallback selector: if remaining budget per still-required slot cannot
    // afford a pawn-value piece, force the cheapest option so the count guarantee holds.
    private static PawnUpgrade FallbackUpgrade(int budget, int foundPawns, int currentCount)
    {
      if (budget <= ItemGenerationValues.Pawn) return PawnUpgrade.Min;
      int slotsLeft = Math.Max(1, foundPawns - currentCount);
      if (currentCount < foundPawns && budget / slotsLeft <= ItemGenerationValues.Pawn) return PawnUpgrade.Min;
      return PawnUpgrade.Core;
    }

    private static bool PigeonholeAllowsSergeant(
        int budget, int foundPawns, int currentCount, int sergeantCost,
        List<PieceType> pawnOptions)
    {
      var sergeants = ApmwCore.getInstance().sergeants;
      int slotsStillNeededAfter = Math.Max(0, foundPawns - currentCount - 1);
      if (slotsStillNeededAfter == 0) return budget >= sergeantCost;
      var nonSergeantOptions = pawnOptions.Where(p => !sergeants.Contains(p)).ToList();
      if (nonSergeantOptions.Count == 0) return false;
      int cheapestNonSergeant = nonSergeantOptions.Min(p => p.MidgameValue);
      return (budget - sergeantCost) >= slotsStillNeededAfter * cheapestNonSergeant;
    }

    private static void UpgradePawns(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions,
        List<PieceType> workingPawns, ApmwConfig config)
    {
      if (config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.BetterPawn))
        UpgradeWeakPawnsToBetterPawns(randomPieces, ref adjustedPawnValues, pawnOptions, workingPawns);

      if (!ShouldApplyDelayedBetterPawnAction(config)) return;
      UpgradeRemainingPawnsToSergeants(randomPieces, adjustedPawnValues, pawnOptions, workingPawns);
    }

    private static void UpgradeWeakPawnsToBetterPawns(Random randomPieces, ref int adjustedPawnValues,
        List<PieceType> pawnOptions, List<PieceType> workingPawns)
    {
      var miniIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .Where(item => item.Piece.MidgameValue < ItemGenerationValues.Weak)
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && miniIndexes.Count > 0)
      {
        var index = miniIndexes.Dequeue();
        adjustedPawnValues += workingPawns[index].MidgameValue;
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Best);
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
    }

    private static bool ShouldApplyDelayedBetterPawnAction(ApmwConfig config)
    {
      return config.IsPieceUpgradeActionPreferredBefore(
          ApmwConstants.PieceUpgradeActions.MorePawn,
          ApmwConstants.PieceUpgradeActions.BetterPawn)
        && !ShouldApplyPoolPawnUpgradeAction(config);
    }

    private static void UpgradeRemainingPawnsToSergeants(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions, List<PieceType> workingPawns)
    {
      var sergeantIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && sergeantIndexes.Count > 0)
      {
        var index = sergeantIndexes.Dequeue();
        adjustedPawnValues += workingPawns[index].MidgameValue;
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
    }

    internal static int SuperMaxPawnGuarantee(int numFiles, int foundPawns, int foundConsuls, int foundJacks, int foundMajors, int foundMinors)
    {
      int boardLocationNeeds = (numFiles == 10 ? 19 : 15) - foundConsuls - foundJacks - foundMajors - foundMinors;
      return Math.Min(foundPawns, Math.Max(0, boardLocationNeeds));
    }

    public static List<PieceType> GeneratePawns(int numFiles, List<PieceType> minors, int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      int foundPawnMaterialCount = core.foundPawns;
      int pawnGuarantee = core.foundPawns;
      if (ApmwConfig.getInstance().UsesSuperMaxPawnGuarantee)
        pawnGuarantee = SuperMaxPawnGuarantee(numFiles, core.foundPawns, core.foundConsuls, core.foundJacks, core.foundMajors, core.foundMinors);

      return GeneratePawns(
        numFiles,
        minors,
        spareMaterial,
        foundPawnMaterialCount,
        pawnGuarantee,
        -1);
    }

    public static List<PieceType> GeneratePawns(
      int numFiles,
      List<PieceType> minors,
      int spareMaterial,
      int foundPawnMaterialCount,
      int pawnGuarantee,
      int maxPawnPieces)
    {
      var core = ApmwCore.getInstance();
      List<PieceType> thirdRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> fourthRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> finalRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> pawnRank = minors.Skip(numFiles).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().pawnSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().pawnLocSeed);
      int startingPieces = pawnRank.Count((item) => item != null);
      int remainingPawnSpaces = 4 * numFiles - startingPieces;
      int pawnSpaceLimit = remainingPawnSpaces;
      if (maxPawnPieces >= 0)
        pawnSpaceLimit = Math.Min(remainingPawnSpaces, Math.Max(0, maxPawnPieces));

      int adjustedPawnValues = Math.Max(
        foundPawnMaterialCount * ItemGenerationValues.Pawn,
        foundPawnMaterialCount * ItemGenerationValues.Pawn + spareMaterial + 45);

      if (maxPawnPieces >= 0)
        pawnGuarantee = Math.Min(Math.Max(0, pawnGuarantee), pawnSpaceLimit);

      List<PieceType> workingPawns = PickPawns(randomPieces, adjustedPawnValues, pawnSpaceLimit, pawnGuarantee);

      Queue<PieceType> adjustedPawns = new Queue<PieceType>(workingPawns);
      // Fill each rank
      FillPawnRank(pawnRank, numFiles, startingPieces, adjustedPawns, randomLocations);
      FillPawnRank(thirdRank, numFiles, numFiles, adjustedPawns, randomLocations);
      FillPawnRank(fourthRank, numFiles, numFiles * 2, adjustedPawns, randomLocations);
      FillPawnRank(finalRank, numFiles, numFiles * 3, adjustedPawns, randomLocations);

      int remainingForwardness = core.foundPawnForwardness;
      foreach (var rankAdvance in new List<(List<PieceType> SourceRank, List<PieceType> TargetRank)> {
        (pawnRank, thirdRank), (thirdRank, fourthRank), (fourthRank, finalRank),
        (pawnRank, thirdRank), (thirdRank, fourthRank),
        (pawnRank, thirdRank)
      })
      {
        List<int> possibleForwardPawnPositions = new List<int>();
        for (int i = 0; i < rankAdvance.TargetRank.Count; i++)
          if (rankAdvance.TargetRank[i] == null && rankAdvance.SourceRank[i] != null && rankAdvance.SourceRank[i].IsPawn)
            possibleForwardPawnPositions.Add(i);
        for (
          int i = randomLocations.Next(possibleForwardPawnPositions.Count);
          remainingForwardness-- > 0 && possibleForwardPawnPositions.Count > 0;
          i = randomLocations.Next(possibleForwardPawnPositions.Count))
        {
          // swap backward with forward
          rankAdvance.TargetRank[possibleForwardPawnPositions[i]] = rankAdvance.SourceRank[possibleForwardPawnPositions[i]];
          rankAdvance.SourceRank[possibleForwardPawnPositions[i]] = null;
          possibleForwardPawnPositions.RemoveAt(i);
        }
      }

      List<PieceType> output = new List<PieceType>();
      output.AddRange(minors.Take(numFiles));
      output.AddRange(pawnRank);
      output.AddRange(thirdRank);
      output.AddRange(fourthRank);
      output.AddRange(finalRank);

      return output;
    }
  }

  internal static class MinorPieceGeneration
  {
    public static List<PieceType> Generate(int numFiles, List<PieceType> queens, List<string> promotions, ref int spareMaterial)
    {
      return GenerateDirect(
        numFiles,
        queens,
        Math.Max(0, ApmwCore.getInstance().foundMinors),
        promotions,
        ref spareMaterial);
    }

    public static List<PieceType> GenerateDirect(
      int numFiles,
      List<PieceType> queens,
      int directMinorCount,
      List<string> promotions,
      ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> minors = core.minors.ToList();
      minors = ArmyPieceFilter.Filter(minors);
      PieceSetLayout layout = PieceSetLayout.FromPieceList(numFiles, queens);

      Random randomPieces = new Random(ApmwConfig.getInstance().minorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().minorLocSeed);

      int limit = ApmwConfig.getInstance().minorTypeLimit;
      int player = core.GeriProvider();
      int parity = layout.LeftBackRank.Count((piece) => piece != null) - layout.RightBackRank.Count((piece) => piece != null);
      int backRankPieces = layout.BackRankPieceCount;
      int availableBackRankSpaces = layout.AvailableBackRankSpaces;
      int availableOuterSpaces = layout.AvailableOuterRankSpaces;
      int minorsToPlace = Math.Min(Math.Max(0, directMinorCount), availableBackRankSpaces + availableOuterSpaces);
      int backRankMinorsToPlace = Math.Min(availableBackRankSpaces, minorsToPlace);

      for (int i = 0; i < backRankMinorsToPlace; i++)
      {
        var piece = PieceChoice.Choose(ref minors, randomPieces, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, ItemGenerationValues.Minor, ref spareMaterial);
        parity = PiecePlacement.PlaceOnBackRank(new List<int>(), layout, randomLocations, parity, backRankPieces + i, piece);
      }
      for (int i = backRankMinorsToPlace; i < minorsToPlace; i++)
      {
        var piece = PieceChoice.Choose(ref minors, randomPieces, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, ItemGenerationValues.Minor, ref spareMaterial);
        PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece);
      }
      spareMaterial += Math.Max(0, directMinorCount - minorsToPlace) * ItemGenerationValues.Minor;

      promotions.Add(string.Join("", promotionPieces));

      return layout.ToPieceList();
    }
  }

  internal static class DirectPieceFamilyGeneration
  {
    public static List<PieceType> Generate(
      int numFiles,
      List<PieceType> pieces,
      IEnumerable<PieceType> pieceFamily,
      int directCount,
      int expectedMaterial,
      int pieceSeed,
      int locationSeed,
      int typeLimit,
      List<string> promotions,
      ref int spareMaterial)
    {
      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> options = ArmyPieceFilter.Filter(pieceFamily ?? Enumerable.Empty<PieceType>());
      PieceSetLayout layout = PieceSetLayout.FromPieceList(numFiles, pieces);
      Random randomPieces = new Random(pieceSeed);
      Random randomLocations = new Random(locationSeed);
      int player = ApmwCore.getInstance().GeriProvider();
      int parity = layout.LeftBackRank.Count(piece => piece != null) - layout.RightBackRank.Count(piece => piece != null);
      int backRankPieces = layout.BackRankPieceCount;
      int availableBackRankSpaces = layout.AvailableBackRankSpaces;
      int availableOuterSpaces = layout.AvailableOuterRankSpaces;
      int piecesToPlace = Math.Min(Math.Max(0, directCount), availableBackRankSpaces + availableOuterSpaces);
      int backRankPiecesToPlace = Math.Min(availableBackRankSpaces, piecesToPlace);

      if (options.Count > 0)
      {
        for (int i = 0; i < backRankPiecesToPlace; i++)
        {
          var piece = PieceChoice.Choose(ref options, randomPieces, chosenPieces, typeLimit);
          PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, expectedMaterial, ref spareMaterial);
          parity = PiecePlacement.PlaceOnBackRank(new List<int>(), layout, randomLocations, parity, backRankPieces + i, piece);
        }

        for (int i = backRankPiecesToPlace; i < piecesToPlace; i++)
        {
          var piece = PieceChoice.Choose(ref options, randomPieces, chosenPieces, typeLimit);
          PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, expectedMaterial, ref spareMaterial);
          PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece);
        }
      }
      else
      {
        piecesToPlace = 0;
      }

      spareMaterial += Math.Max(0, directCount - piecesToPlace) * expectedMaterial;
      promotions.Add(string.Join("", promotionPieces));

      return layout.ToPieceList();
    }
  }

  internal static class QueenGeneration
  {
    public static List<PieceType> Substitute(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      return MajorUpgradeSubstitution.Substitute(
        numFiles,
        majors,
        order,
        promotions,
        core.queens,
        Math.Max(0, core.foundQueens),
        0,
        ApmwConfig.getInstance().queenSeed,
        ApmwConfig.getInstance().queenTypeLimit,
        ItemGenerationValues.Queen,
        ref spareMaterial);
    }
  }

  internal static class AmazonGeneration
  {
    public static List<PieceType> Substitute(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      return MajorUpgradeSubstitution.Substitute(
        numFiles,
        majors,
        order,
        promotions,
        core.amazons,
        Math.Max(0, core.foundAmazons),
        Math.Max(0, core.foundQueens),
        ApmwConfig.getInstance().queenSeed,
        ApmwConfig.getInstance().queenTypeLimit,
        ItemGenerationValues.Amazon,
        ref spareMaterial);
    }
  }

  internal static class MajorUpgradeSubstitution
  {
    public static List<PieceType> Substitute(
      int numFiles,
      List<PieceType> majors,
      List<int> order,
      List<string> promotions,
      IEnumerable<PieceType> upgradePieces,
      int upgradesToSubstitute,
      int reservedAfter,
      int seed,
      int limit,
      int expectedMaterial,
      ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();

      int majorSlotStart = Math.Min(order.Count, Math.Max(0, core.foundJacks));
      int endExclusive = Math.Min(order.Count, Math.Max(majorSlotStart, order.Count - Math.Max(0, reservedAfter)));
      int startInclusive = Math.Max(majorSlotStart, endExclusive - upgradesToSubstitute);
      List<int> sourceIndices = new List<int>();
      for (int i = endExclusive - 1; i >= startInclusive; i--)
        sourceIndices.Add(MajorOrderIndexToPieceSetIndex(numFiles, order[i], majors.Count));

      return NonPawnFamilySubstitution.Substitute(
        new NonPawnFamilySubstitutionRequest(
          majors,
          promotions,
          Enumerable.Empty<PieceType>(),
          sourceIndices,
          true,
          upgradePieces,
          upgradesToSubstitute,
          seed,
          limit,
          expectedMaterial),
        ref spareMaterial);
    }

    public static int MajorOrderIndexToPieceSetIndex(int numFiles, int orderIndex, int pieceSetCount)
    {
      int kingIndex = numFiles / 2;
      int pieceSetIndex =
        orderIndex < kingIndex ? orderIndex :
        orderIndex < numFiles - 1 ? orderIndex + 1 :
        orderIndex;
      if (pieceSetIndex < 0 || pieceSetIndex >= pieceSetCount)
        throw new InvalidOperationException(
          "Major order index " + orderIndex + " mapped outside generated piece set of size " + pieceSetCount + ".");
      return pieceSetIndex;
    }
  }

  internal static class MajorPieceGeneration
  {
    public static List<PieceType> Generate(int numFiles, out List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      int majorUpgradesToBe = Math.Max(0, core.foundQueens) + Math.Max(0, core.foundAmazons);
      return GenerateWithCounts(
        numFiles,
        Math.Max(0, core.foundMajors),
        Math.Max(0, core.foundJacks),
        majorUpgradesToBe,
        0,
        out order,
        promotions,
        ref spareMaterial);
    }

    public static List<PieceType> GenerateDirect(
      int numFiles,
      int directMajorCount,
      int directJackCount,
      out List<int> order,
      List<string> promotions,
      ref int spareMaterial)
    {
      return GenerateDirect(
        numFiles,
        directMajorCount,
        directJackCount,
        0,
        out order,
        promotions,
        ref spareMaterial);
    }

    public static List<PieceType> GenerateDirect(
      int numFiles,
      int directMajorCount,
      int directJackCount,
      int lockedMajorCount,
      out List<int> order,
      List<string> promotions,
      ref int spareMaterial)
    {
      return GenerateWithCounts(
        numFiles,
        Math.Max(0, directMajorCount),
        Math.Max(0, directJackCount),
        0,
        Math.Max(0, lockedMajorCount),
        out order,
        promotions,
        ref spareMaterial);
    }

    private static List<PieceType> GenerateWithCounts(
      int numFiles,
      int majorSlotCount,
      int jackCount,
      int majorUpgradesToBe,
      int lockedMajorCount,
      out List<int> order,
      List<string> promotions,
      ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      order = new List<int>();
      List<PieceType> majors = core.majors.ToList();
      List<PieceType> jacks = core.jacks.ToList();
      majors = ArmyPieceFilter.Filter(majors);
      jacks = ArmyPieceFilter.Filter(jacks);
      PieceSetLayout layout = PieceSetLayout.Empty(numFiles, null);

      Random randomPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomJackPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().majorLocSeed);
      int limit = ApmwConfig.getInstance().majorTypeLimit;
      int player = core.GeriProvider();
      int parity = 0;

      int numKings = core.foundConsuls;
      if (numKings > 0)
      {
        List<PieceType> kings = core.kings;
        // Center the king on D file for 8x8 or E file for 10x8
        int centerFile = (numFiles / 2) - 1;
        layout.LeftBackRank[centerFile] = kings[0];
        if (numKings > 1)
        {
          // Place second king on E file for 8x8 or F file for 10x8
          layout.RightBackRank[0] = kings[0];
        }
      }

      int numJacks = jackCount;
      int reservedUpgradeSlots = Math.Min(Math.Max(0, majorUpgradesToBe), Math.Max(0, majorSlotCount));
      int numDirectMajors = Math.Max(0, majorSlotCount - reservedUpgradeSlots);
      int numLockedMajors = Math.Min(Math.Max(0, lockedMajorCount), numDirectMajors);
      int numNonMinorPieces = numDirectMajors + reservedUpgradeSlots + numKings + numJacks;
      int backRankCapacity = numFiles - 1;
      int outerRankCapacity = numFiles;
      int placementCapacity = backRankCapacity + outerRankCapacity;
      var piecePicker = new MajorPiecePicker(
        majors,
        jacks,
        randomPieces,
        randomJackPieces,
        chosenPieces,
        limit,
        player,
        promotionPieces,
        numKings,
        numLockedMajors,
        numJacks,
        numNonMinorPieces,
        reservedUpgradeSlots);

      for (int placementIndex = numKings; placementIndex < Math.Min(placementCapacity, numNonMinorPieces); placementIndex++)
      {
        PieceType piece = piecePicker.Pick(placementIndex, ref spareMaterial);
        if (placementIndex < backRankCapacity)
          parity = PiecePlacement.PlaceOnBackRank(order, layout, randomLocations, parity, placementIndex, piece);
        else
          PlaceOnOuterRank(order, layout, randomLocations, piece, numFiles);
      }
      RecordUnplacedDirectMajorMaterial(
        numKings,
        numLockedMajors,
        numJacks,
        numDirectMajors,
        reservedUpgradeSlots,
        placementCapacity,
        ref spareMaterial);

      layout.CenterBackRank = core.kings[core.foundKingPromotions];
      promotions.Add(string.Join("", promotionPieces));

      return layout.ToPieceList();
    }

    private static void RecordUnplacedDirectMajorMaterial(
      int numKings,
      int numLockedMajors,
      int numJacks,
      int numDirectMajors,
      int reservedUpgradeSlots,
      int placementCapacity,
      ref int spareMaterial)
    {
      int remainingCapacity = Math.Max(0, placementCapacity - numKings);
      int placedLockedMajors = Math.Min(numLockedMajors, remainingCapacity);
      remainingCapacity -= placedLockedMajors;
      int placedJacks = Math.Min(numJacks, remainingCapacity);
      remainingCapacity -= placedJacks;
      int nonLockedDirectMajors = Math.Max(0, numDirectMajors - numLockedMajors);
      int placedMajors = Math.Min(nonLockedDirectMajors, remainingCapacity);
      remainingCapacity -= placedMajors;

      spareMaterial += Math.Max(0, numLockedMajors - placedLockedMajors) * ItemGenerationValues.Major;
      spareMaterial += Math.Max(0, numJacks - placedJacks) * ItemGenerationValues.Jack;
      spareMaterial += Math.Max(0, nonLockedDirectMajors - placedMajors) * ItemGenerationValues.Major;

      // Legacy reserved queen/amazon major slots are accounted by their substitution step.
    }

    private static void PlaceOnOuterRank(
      List<int> order,
      PieceSetLayout layout,
      Random randomLocations,
      PieceType piece,
      int numFiles)
    {
      bool innerSpillSpaceAvailable = layout.OuterRank.Skip(1).Take(numFiles - 2).Any(item => item == null);
      int placedIndex = innerSpillSpaceAvailable
        ? PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece, 1, numFiles - 1)
        : PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece);
      order.Add(placedIndex + numFiles);
    }

    private sealed class MajorPiecePicker
    {
      private readonly Random randomPieces;
      private readonly Random randomJackPieces;
      private readonly Dictionary<PieceType, int> chosenPieces;
      private readonly int limit;
      private readonly int player;
      private readonly HashSet<string> promotionPieces;
      private readonly int numKings;
      private readonly int numLockedMajors;
      private readonly int numJacks;
      private readonly int numNonMinorPieces;
      private readonly int majorUpgradesToBe;
      private List<PieceType> majors;
      private List<PieceType> jacks;

      public MajorPiecePicker(
        List<PieceType> majors,
        List<PieceType> jacks,
        Random randomPieces,
        Random randomJackPieces,
        Dictionary<PieceType, int> chosenPieces,
        int limit,
        int player,
        HashSet<string> promotionPieces,
        int numKings,
        int numLockedMajors,
        int numJacks,
        int numNonMinorPieces,
        int majorUpgradesToBe)
      {
        this.majors = majors;
        this.jacks = jacks;
        this.randomPieces = randomPieces;
        this.randomJackPieces = randomJackPieces;
        this.chosenPieces = chosenPieces;
        this.limit = limit;
        this.player = player;
        this.promotionPieces = promotionPieces;
        this.numKings = numKings;
        this.numLockedMajors = numLockedMajors;
        this.numJacks = numJacks;
        this.numNonMinorPieces = numNonMinorPieces;
        this.majorUpgradesToBe = majorUpgradesToBe;
      }

      public PieceType Pick(int placementIndex, ref int spareMaterial)
      {
        if (placementIndex < numKings + numLockedMajors)
          return PickPromotion(ref majors, randomPieces, ItemGenerationValues.Major, ref spareMaterial);
        if (placementIndex < numJacks + numKings + numLockedMajors)
          return PickPromotion(ref jacks, randomJackPieces, ItemGenerationValues.Jack, ref spareMaterial);
        if (placementIndex < numNonMinorPieces - majorUpgradesToBe)
          return PickPromotion(ref majors, randomPieces, ItemGenerationValues.Major, ref spareMaterial);

        randomPieces.Next();
        return null;
      }

      private PieceType PickPromotion(
        ref List<PieceType> pieces,
        Random random,
        int expectedMaterial,
        ref int spareMaterial)
      {
        PieceType piece = PieceChoice.Choose(ref pieces, random, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, expectedMaterial, ref spareMaterial);
        return piece;
      }
    }
  }

  internal static class PocketItemGeneration
  {
    public static List<PieceType> Generate()
    {
      int foundPockets = ApmwCore.getInstance().foundPockets;
      var pockets = ApmwConfig.getInstance().generatePocketValues(foundPockets);
      List<PieceType> pocketPieces = new List<PieceType>();
      List<PieceType> pawnOptions = PawnGeneration.SetupPawnOptions();
      for (int i = 0; i < 3; i++)
      {
        Random randomPieces = new Random(ApmwConfig.getInstance().pocketChoiceSeed[i]);
        if (pockets[i] == 0)
          pocketPieces.Add(null);
        else if (pockets[i] == 1)
          pocketPieces.Add(PawnGeneration.GetNextPawn(randomPieces, pawnOptions));
        else
        {
          // TODO(chesslogic): Try to remove very low material pieces like Gardener, unless it leaves set empty
          HashSet<PieceType> pieceSet = ApmwCore.getInstance().pocketSets[pockets[i] - 1];
          List<PieceType> pocketOptions = ArmyPieceFilter.Filter(pieceSet);
          int index = randomPieces.Next(pocketOptions.Count);
          pocketPieces.Add(pocketOptions[index]);
        }
      }
      return pocketPieces;
    }
  }

  internal static class PieceChoice
  {
    public static PieceType Choose(ref List<PieceType> pieces, Random randomPieces, Dictionary<PieceType, int> chosenPieces, int limit)
    {
      if (limit <= 0)
        return pieces[randomPieces.Next(pieces.Count)];
      int index = randomPieces.Next(pieces.Count);
      PieceType piece = pieces[index];
      if (!chosenPieces.ContainsKey(pieces[index]))
        chosenPieces[pieces[index]] = 0;
      if (++chosenPieces[pieces[index]] >= limit && pieces.Count > 1)
        pieces.RemoveAt(index);
      return piece;
    }
  }

  internal static class PiecePlacement
  {
    public static int PlaceOnBackRank(
      List<int> order,
      PieceSetLayout layout,
      Random random,
      int parity,
      int placementIndex,
      PieceType piece)
    {
      return PlaceOnBackRank(order, layout.LeftBackRank, layout.RightBackRank, random, parity, placementIndex, piece);
    }

    public static int PlaceOnBackRank(
      List<int> order,
      List<PieceType> left,
      List<PieceType> right,
      Random random,
      int parity,
      int placementIndex,
      PieceType piece)
    {
      int side;
      // The left back-rank side has one more slot than the right side.
      if (placementIndex >= right.Count * 2 || placementIndex >= left.Count * 2)
      {
        side = right.Count(item => item == null) - left.Count(item => item == null);
        parity = 0;
      }
      // if we need to choose a side, it should be random
      else if (parity == 0)
      {
        parity = random.Next(2) * 2 - 1;
        side = -parity;
      }
      // we chose the other side last time, let's go somewhere new
      else
      {
        side = parity;
        parity = 0;
      }

      if (side <= 0)
      {
        order.Add(ChooseIndexAndPlace(left, random, piece));
      }
      else
      {
        order.Add(ChooseIndexAndPlace(right, random, piece) + left.Count);
      }

      return parity;
    }

    public static int ChooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece)
    {
      return ChooseIndexAndPlace(items, random, piece, 0, items.Count);
    }

    public static int ChooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece, int startIndex, int endIndex)
    {
      if (items == null)
        throw new ArgumentNullException(nameof(items));
      if (random == null)
        throw new ArgumentNullException(nameof(random));
      if (startIndex < 0 || endIndex > items.Count || startIndex >= endIndex)
        throw new ArgumentOutOfRangeException(nameof(startIndex), "Placement range must be within the target list.");

      int emptyCount = items.Skip(startIndex).Take(endIndex - startIndex).Count(item => item == null);
      if (emptyCount <= 0)
        throw new InvalidOperationException(
          "No space to place piece in " + string.Join(", ", items.Select(item => item?.Notation[0])));

      var skips = random.Next(emptyCount);
      for (int index = startIndex; index < endIndex; index++)
      {
        if (items[index] != null)
          continue;
        if (skips-- > 0)
          continue;
        items[index] = piece;
        return index;
      }

      throw new InvalidOperationException(
        "No space to place piece in " + string.Join(", ", items.Select(item => item?.Notation[0])));
    }
  }

  internal static class ArmyPieceFilter
  {
    public static List<PieceType> Filter(IEnumerable<PieceType> pieces)
    {
      List<PieceType> originalPieces = pieces.ToList();
      List<int> army = ApmwConfig.getInstance().Army;
      if (army.Count == 0)
        return originalPieces;
      HashSet<PieceType> armiesPieces = new HashSet<PieceType>();
      for (int i = 0; i < army.Count; i++)
        armiesPieces = armiesPieces.Concat(ApmwCore.getInstance().armies[army[i]]).ToHashSet();
      List<PieceType> newPieces = new List<PieceType>();
      foreach (var piece in originalPieces)
        if (armiesPieces.Contains(piece))
          newPieces.Add(piece);
      return newPieces.Count > 0 ? newPieces : originalPieces;
    }
  }
}
