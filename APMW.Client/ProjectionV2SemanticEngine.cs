using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Archipelago.APChessV
{
  internal sealed class ProjectionV2Exception : Exception
  {
    public ProjectionV2Exception(string message) : base(message) { }
  }

  internal static class ProjectionV2SemanticEngine
  {
    private const string PrimaryRoyal = "primary-royal";
    private const string AdditionalRoyal = "additional-royal";
    private const string LockedCastler = "locked-castler";
    private const string JackSlot = "jack-slot";
    private const string MajorSlot = "major-slot";
    private const string MinorSlot = "minor-slot";
    private const string PawnSlot = "pawn-slot";
    private const string Royal = "royal";
    private const string Pawn = "pawn";
    private const string Minor = "minor";
    private const string Major = "major";
    private const string Jack = "jack";
    private const string Queen = "queen";
    private const string Amazon = "amazon";

    private static readonly string[] RoleOrder =
    {
      PrimaryRoyal, AdditionalRoyal, LockedCastler, JackSlot, MajorSlot, MinorSlot, PawnSlot,
    };
    private static readonly string[] FamilyOrder =
    {
      Royal, Pawn, Minor, Major, Jack, Queen, Amazon,
    };
    private static readonly string[] NonPawnRoles =
    {
      AdditionalRoyal, LockedCastler, JackSlot, MajorSlot, MinorSlot,
    };
    private static readonly string[] OrdinaryNonPawnRoles = { JackSlot, MajorSlot, MinorSlot };
    private static readonly string[] LegacyActionOrder =
    {
      "minor-to-major", "major-to-jack", "minor-to-jack",
      "major-to-queen", "jack-to-queen", "queen-to-amazon",
    };
    private static readonly string[] FundamentalActionOrder =
    {
      "pawn-to-minor", "pawn-to-major", "minor-to-major", "major-to-jack",
      "minor-to-jack", "major-to-queen", "jack-to-queen", "queen-to-amazon",
    };

    private sealed class Input
    {
      public string Itemization;
      public string Ordering;
      public Seeds Seeds;
      public Dictionary<string, int> Items;
      public Dictionary<string, int> Unlocks;
      public List<Preference> Preferences;
    }

    private sealed class Seeds
    {
      public string Pocket = "0";
      public string Pawn = "0";
      public string Minor = "0";
      public string Major = "0";
      public string Queen = "0";
      public string StableRoot { get { return string.Join("|", Pocket, Pawn, Minor, Major, Queen); } }
    }

    private sealed class Preference
    {
      public string Action;
      public int Priority;
      public int Numerator;
      public int Denominator;
      public double Proportion
      {
        get
        {
          if (Denominator <= 0)
            throw new ProjectionV2Exception("upgrade proportion denominator must be positive");
          if (Numerator < 0)
            throw new ProjectionV2Exception("upgrade proportion numerator must not be negative");
          return (double)Numerator / Denominator;
        }
      }
    }

    private sealed class Effective
    {
      public List<(string Name, int Count)> Items = new List<(string, int)>();
      public List<(string Name, int Count)> Overcounts = new List<(string, int)>();
      public List<(string Role, int Count)> Unlocks = new List<(string, int)>();
      public List<(string Role, int Count)> UnlockOvercounts = new List<(string, int)>();
      public int Count(string name)
      {
        return Items.FirstOrDefault(item => item.Name == name).Count;
      }
    }

    private sealed class ActionPlan
    {
      public string Name;
      public string From;
      public string To;
      public string RoleRule;
      public int Priority;
      public double Proportion;
    }

    private sealed class Piece
    {
      public string Id;
      public string Role;
      public int Ordinal;
      public string OriginAction;
      public string OriginFamily;
      public string Family;
      public List<string> Path = new List<string>();
      public bool Locked;
      public int Granted;
      public int Expected;
      public List<string> Entitlements = new List<string>();
    }

    private sealed class Ledger
    {
      public string Id;
      public string Source;
      public int Amount;
      public string SlotId;
      public string Reason;
    }

    private sealed class LegacyPlan
    {
      public Dictionary<string, int> Direct;
      public List<(ActionPlan Action, int Count)> Actions;
      public Dictionary<string, int> Unused;
    }

    internal sealed class FundamentalPlan
    {
      public Dictionary<string, int> Tiers;
      public Dictionary<string, int> Applied;
      public int Spare;
      public int Locked;
    }

    public static JsonObject Project(ApmwContractV2 contract, JsonElement inputElement)
    {
      Input input = ParseInput(inputElement);
      ValidateMode(contract, input);
      Effective effective = Normalize(contract, input);
      GeometryStage stage = SelectGeometry(contract, effective);
      List<ActionPlan> actions = ResolveActions(contract, input);
      List<Piece> pieces;
      List<Ledger> dormant;
      List<Ledger> unallocated;
      int normalized;
      if (input.Itemization == "legacy")
        GenerateLegacy(contract, effective, actions, input.Seeds.StableRoot, out pieces, out dormant, out unallocated, out normalized);
      else
        GenerateFundamental(contract, effective, actions, input.Seeds.StableRoot, out pieces, out dormant, out unallocated, out normalized);
      return ProjectSlots(contract, input, effective, stage, pieces, dormant, unallocated, normalized);
    }

    public static FundamentalPlan CharacterizeFundamental(
      ApmwContractV2 contract,
      int chessmen,
      int materialBudget,
      int castlers,
      JsonElement seedsElement,
      JsonElement preferencesElement)
    {
      var input = new Input
      {
        Itemization = "fundamental",
        Ordering = "stable",
        Seeds = ParseSeeds(seedsElement),
        Items = new Dictionary<string, int>(),
        Unlocks = new Dictionary<string, int>(),
        Preferences = ParsePreferences(preferencesElement),
      };
      List<ActionPlan> actions = ResolveActions(contract, input);
      return PlanFundamental(
        contract,
        chessmen,
        materialBudget,
        Math.Min(castlers, contract.Castler.Maximum),
        actions,
        input.Seeds.StableRoot);
    }

    private static Input ParseInput(JsonElement value)
    {
      return new Input
      {
        Itemization = value.GetProperty("itemization").GetString(),
        Ordering = value.GetProperty("ordering").GetString(),
        Seeds = value.TryGetProperty("seeds", out JsonElement seeds) ? ParseSeeds(seeds) : new Seeds(),
        Items = ParseIntMap(value, "item_counts"),
        Unlocks = ParseIntMap(value, "unlock_counts"),
        Preferences = value.TryGetProperty("upgrade_preferences", out JsonElement preferences)
          ? ParsePreferences(preferences)
          : new List<Preference>(),
      };
    }

    private static Seeds ParseSeeds(JsonElement value)
    {
      string Read(string name)
      {
        return value.TryGetProperty(name, out JsonElement item) ? item.ToString() : "0";
      }
      return new Seeds
      {
        Pocket = Read("pocket_seed"),
        Pawn = Read("pawn_seed"),
        Minor = Read("minor_seed"),
        Major = Read("major_seed"),
        Queen = Read("queen_seed"),
      };
    }

    private static Dictionary<string, int> ParseIntMap(JsonElement parent, string property)
    {
      var output = new Dictionary<string, int>(StringComparer.Ordinal);
      if (!parent.TryGetProperty(property, out JsonElement value))
        return output;
      foreach (JsonProperty item in value.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal))
        output[item.Name] = item.Value.GetInt32();
      return output;
    }

    private static List<Preference> ParsePreferences(JsonElement value)
    {
      return value.EnumerateArray().Select(item => new Preference
      {
        Action = item.GetProperty("action").GetString(),
        Priority = item.GetProperty("priority").GetInt32(),
        Numerator = item.TryGetProperty("proportion_numerator", out JsonElement numerator) ? numerator.GetInt32() : 1,
        Denominator = item.TryGetProperty("proportion_denominator", out JsonElement denominator) ? denominator.GetInt32() : 1,
      }).ToList();
    }

    private static void ValidateMode(ApmwContractV2 contract, Input input)
    {
      if (!contract.ModeCombinations.Any(mode =>
        mode.Itemization == input.Itemization && mode.Ordering == input.Ordering))
      {
        throw new ProjectionV2Exception(
          "unsupported itemization/ordering combination: " + input.Itemization + "/" + input.Ordering);
      }
    }

    private static Effective Normalize(ApmwContractV2 contract, Input input)
    {
      foreach (var item in input.Items)
        if (item.Value < 0)
          throw new ProjectionV2Exception("item count for " + item.Key + " must not be negative");
      var maxima = new Dictionary<string, int>(contract.EffectiveItemMaxima["common"], StringComparer.Ordinal);
      foreach (var item in contract.EffectiveItemMaxima[input.Itemization])
        maxima[item.Key] = item.Value;
      var effective = new Effective();
      foreach (string name in maxima.Keys.OrderBy(name => name, StringComparer.Ordinal))
      {
        int raw = input.Items.TryGetValue(name, out int count) ? count : 0;
        int accepted = Math.Min(raw, maxima[name]);
        if (accepted != 0)
          effective.Items.Add((name, accepted));
        if (raw > accepted)
          effective.Overcounts.Add((name, raw - accepted));
      }

      Dictionary<string, GeometryUnlockRole> roles = contract.GeometryUnlocks.Roles
        .ToDictionary(role => role.RoleId, StringComparer.Ordinal);
      foreach (var unlock in input.Unlocks)
      {
        if (!roles.ContainsKey(unlock.Key))
          throw new ProjectionV2Exception("unknown geometry unlock role: " + unlock.Key);
        if (unlock.Value < 0)
          throw new ProjectionV2Exception("unlock count for " + unlock.Key + " must not be negative");
      }
      foreach (string roleId in roles.Keys.OrderBy(value => value, StringComparer.Ordinal))
      {
        GeometryUnlockRole role = roles[roleId];
        int maximumSteps = (role.Maximum - role.Base) / role.Increment;
        int raw = input.Unlocks.TryGetValue(roleId, out int count) ? count : 0;
        int accepted = Math.Min(raw, maximumSteps);
        effective.Unlocks.Add((roleId, accepted));
        if (raw > accepted)
          effective.UnlockOvercounts.Add((roleId, raw - accepted));
      }
      return effective;
    }

    private static GeometryStage SelectGeometry(ApmwContractV2 contract, Effective effective)
    {
      Dictionary<string, int> counts = effective.Unlocks.ToDictionary(item => item.Role, item => item.Count);
      GeometryUnlockRole fileRole = contract.GeometryUnlocks.Roles.Single(role => role.RoleId == "board-file-unlock");
      GeometryUnlockRole rankRole = contract.GeometryUnlocks.Roles.Single(role => role.RoleId == "board-rank-unlock");
      int files = Math.Min(fileRole.Maximum, fileRole.Base + fileRole.Increment * counts[fileRole.RoleId]);
      int ranks = Math.Min(rankRole.Maximum, rankRole.Base + rankRole.Increment * counts[rankRole.RoleId]);
      Dictionary<string, int> order = contract.StageOrder.Select((id, index) => (id, index))
        .ToDictionary(item => item.id, item => item.index, StringComparer.Ordinal);
      GeometryStage selected = contract.Stages
        .Where(stage => stage.Files <= files && stage.Ranks <= ranks)
        .OrderByDescending(stage => order[stage.StageId])
        .FirstOrDefault();
      if (selected == null)
        throw new ProjectionV2Exception("no valid geometry is unlocked by " + files + "x" + ranks);
      return selected;
    }

    private static List<ActionPlan> ResolveActions(ApmwContractV2 contract, Input input)
    {
      List<Preference> preferences = input.Preferences;
      if (preferences.Count == 0)
      {
        preferences = input.Itemization == "legacy"
          ? new List<Preference> { new Preference { Action = "major-to-queen", Priority = 1, Numerator = 1, Denominator = 1 } }
          : new List<Preference>
          {
            new Preference { Action = "pawn-to-minor", Priority = 1, Numerator = 1, Denominator = 1 },
            new Preference { Action = "minor-to-major", Priority = 1, Numerator = 1, Denominator = 1 },
            new Preference { Action = "pawn-to-major", Priority = 1, Numerator = 1, Denominator = 1 },
            new Preference { Action = "major-to-queen", Priority = 1, Numerator = 1, Denominator = 1 },
          };
      }
      var byName = new Dictionary<string, Preference>(StringComparer.Ordinal);
      foreach (Preference preference in preferences)
      {
        if (byName.ContainsKey(preference.Action))
          throw new ProjectionV2Exception("duplicate upgrade preference: " + preference.Action);
        _ = preference.Proportion;
        byName.Add(preference.Action, preference);
      }
      HashSet<string> transitions = contract.UpgradeDag.Transitions.Select(item => item.Action).ToHashSet(StringComparer.Ordinal);
      HashSet<string> validActions = transitions
        .Concat(contract.UpgradeDag.PawnCreationActions)
        .ToHashSet(StringComparer.Ordinal);
      string[] unknown = byName.Keys.Where(name => !validActions.Contains(name)).OrderBy(name => name, StringComparer.Ordinal).ToArray();
      if (unknown.Length > 0)
        throw new ProjectionV2Exception("unknown upgrade actions: ['" + string.Join("', '", unknown) + "']");
      var actions = new List<ActionPlan>();
      foreach (UpgradeTransition transition in contract.UpgradeDag.Transitions)
      {
        if (!byName.TryGetValue(transition.Action, out Preference preference) || preference.Priority <= 0)
          continue;
        actions.Add(new ActionPlan
        {
          Name = transition.Action,
          From = transition.FromFamily,
          To = transition.ToFamily,
          RoleRule = transition.SourceRoleRule,
          Priority = preference.Priority,
          Proportion = preference.Proportion,
        });
      }
      return actions;
    }

    private static void PrimaryAndCommon(
      ApmwContractV2 contract,
      Effective effective,
      out List<Piece> pieces,
      out List<Ledger> unallocated,
      out int commonTotal)
    {
      IReadOnlyDictionary<string, int> expected = contract.ExpectedMaterial;
      int promotions = effective.Count("Progressive King Promotion");
      int primaryMaterial = promotions * expected["king_promotion"];
      pieces = new List<Piece>
      {
        new Piece
        {
          Id = "primary-royal:000000", Role = PrimaryRoyal, Ordinal = 0,
          OriginAction = "primary-royal", OriginFamily = Royal, Family = Royal,
          Path = Enumerable.Repeat("king-promotion", promotions).ToList(),
          Granted = primaryMaterial, Expected = primaryMaterial,
          Entitlements = promotions > 0 ? new List<string> { Royal } : new List<string>(),
        },
      };
      for (int ordinal = 0; ordinal < effective.Count("Progressive Consul"); ordinal++)
      {
        pieces.Add(new Piece
        {
          Id = StableId(AdditionalRoyal, ordinal), Role = AdditionalRoyal, Ordinal = ordinal,
          OriginAction = "consul", OriginFamily = Royal, Family = Royal,
          Granted = expected["consul"], Expected = expected["consul"],
          Entitlements = new List<string> { Royal },
        });
      }
      unallocated = new List<Ledger>();
      commonTotal = primaryMaterial + pieces.Where(piece => piece.Role == AdditionalRoyal).Sum(piece => piece.Granted);
      foreach (var pair in new[] { ("Play as White", "play_as_white"), ("Progressive Pocket", "pocket") })
      {
        int amount = effective.Count(pair.Item1) * expected[pair.Item2];
        if (amount == 0)
          continue;
        commonTotal += amount;
        unallocated.Add(new Ledger
        {
          Id = "unallocated:" + pair.Item1, Source = pair.Item1, Amount = amount,
          Reason = "outside-roster-projection",
        });
      }
    }

    private static void GenerateLegacy(
      ApmwContractV2 contract,
      Effective effective,
      List<ActionPlan> actions,
      string root,
      out List<Piece> pieces,
      out List<Ledger> dormant,
      out List<Ledger> unallocated,
      out int normalized)
    {
      PrimaryAndCommon(contract, effective, out pieces, out unallocated, out int common);
      LegacyPlan plan = PlanLegacy(effective, actions, root);
      foreach (var familyRole in new[] { (Minor, MinorSlot), (Major, MajorSlot), (Jack, JackSlot) })
      {
        for (int ordinal = 0; ordinal < plan.Direct[familyRole.Item1]; ordinal++)
        {
          pieces.Add(new Piece
          {
            Id = StableId(familyRole.Item2, ordinal), Role = familyRole.Item2, Ordinal = ordinal,
            OriginAction = "direct-" + familyRole.Item1, OriginFamily = familyRole.Item1,
            Family = familyRole.Item1, Granted = contract.ExpectedMaterial[familyRole.Item1],
            Expected = contract.ExpectedMaterial[familyRole.Item1],
            Entitlements = new List<string> { familyRole.Item1 },
          });
        }
      }
      foreach (var planned in plan.Actions)
      {
        List<Piece> candidates = pieces.Where(piece => !piece.Locked && piece.Family == planned.Action.From).ToList();
        var series = new CounterBasedSeedSeries(root, "upgrade-source." + planned.Action.Name);
        int replacementCount = Math.Min(planned.Count, candidates.Count);
        for (int index = 0; index < replacementCount; index++)
        {
          int selected = series.Index(index, candidates.Count);
          Piece piece = candidates[selected];
          candidates.RemoveAt(selected);
          Upgrade(contract, piece, planned.Action);
        }
      }
      int pawns = effective.Count("Progressive Pawn");
      for (int ordinal = 0; ordinal < pawns; ordinal++)
      {
        pieces.Add(new Piece
        {
          Id = StableId(PawnSlot, ordinal), Role = PawnSlot, Ordinal = ordinal,
          OriginAction = "new-pawn", OriginFamily = Pawn, Family = Pawn,
          Granted = contract.ExpectedMaterial[Pawn], Expected = contract.ExpectedMaterial[Pawn],
          Entitlements = new List<string> { Pawn },
        });
      }
      dormant = new List<Ledger>();
      int unusedQueen = plan.Unused[Queen];
      if (unusedQueen != 0)
      {
        dormant.Add(new Ledger
        {
          Id = "dormant:Progressive Major To Queen",
          Source = "Progressive Major To Queen",
          Amount = unusedQueen * (contract.ExpectedMaterial[Queen] - contract.ExpectedMaterial[Major]),
          Reason = "parentless-upgrade",
        });
      }
      normalized = common +
        pawns * contract.ExpectedMaterial[Pawn] +
        new[] { Minor, Major, Jack }.Sum(family => plan.Direct[family] * contract.ExpectedMaterial[family]) +
        plan.Actions.Sum(item => item.Count * UpgradeCredit(contract, item.Action)) +
        dormant.Sum(item => item.Amount);
    }

    private static LegacyPlan PlanLegacy(Effective effective, List<ActionPlan> actions, string root)
    {
      string[] families = { Minor, Major, Jack, Queen, Amazon };
      var found = new Dictionary<string, int>
      {
        [Minor] = effective.Count("Progressive Minor Piece"),
        [Major] = effective.Count("Progressive Major Piece"),
        [Jack] = effective.Count("Progressive Jack"),
        [Queen] = effective.Count("Progressive Major To Queen"),
        [Amazon] = 0,
      };
      var current = new Dictionary<(string Family, string Origin), int>();
      foreach (string family in families)
        foreach (string origin in families)
          current[(family, origin)] = 0;
      foreach (string family in new[] { Minor, Major, Jack })
        current[(family, family)] = found[family];
      var remaining = new Dictionary<string, int>(found);
      var direct = new Dictionary<string, int>
      {
        [Minor] = found[Minor], [Major] = found[Major], [Jack] = found[Jack], [Queen] = 0, [Amazon] = 0,
      };
      Dictionary<string, ActionPlan> byName = actions.ToDictionary(action => action.Name, StringComparer.Ordinal);
      List<ActionPlan> ordered = LegacyActionOrder.Where(byName.ContainsKey).Select(name => byName[name]).ToList();
      List<List<ActionPlan>> groups = ordered.GroupBy(action => action.Priority)
        .OrderByDescending(group => group.Key).Select(group => group.ToList()).ToList();
      var applied = new Dictionary<string, int>(StringComparer.Ordinal);
      var counters = new Dictionary<string, long>(StringComparer.Ordinal);
      while (true)
      {
        bool didApply = false;
        foreach (List<ActionPlan> group in groups)
        {
          var viable = new List<ActionPlan>();
          var weights = new List<double>();
          foreach (ActionPlan action in group)
          {
            bool target = remaining[action.To] > 0;
            if (action.To != Queen && action.To != Amazon)
              target = target && current[(action.To, action.To)] > 0;
            int eligible = families.Sum(origin => current[(action.From, origin)]);
            if (!target || eligible <= 0)
              continue;
            viable.Add(action);
            weights.Add(eligible * action.Proportion);
          }
          if (viable.Count == 0)
            continue;
          ActionPlan chosen = viable.Count == 1 ? viable[0] : LegacyChoice(root, viable, weights, counters);
          if (viable.Count > 1)
            foreach (ActionPlan candidate in viable)
              counters[candidate.Name] = counters.TryGetValue(candidate.Name, out long count) ? count + 1 : 1;
          remaining[chosen.To]--;
          direct[chosen.To] = Math.Max(0, direct[chosen.To] - 1);
          if (chosen.To != Queen && chosen.To != Amazon)
            current[(chosen.To, chosen.To)] = Math.Max(0, current[(chosen.To, chosen.To)] - 1);
          foreach (string origin in new[] { chosen.From }.Concat(families.Where(family => family != chosen.From)))
          {
            if (current[(chosen.From, origin)] <= 0)
              continue;
            current[(chosen.From, origin)]--;
            current[(chosen.To, origin)]++;
            break;
          }
          applied[chosen.Name] = applied.TryGetValue(chosen.Name, out int countApplied) ? countApplied + 1 : 1;
          didApply = true;
          break;
        }
        if (!didApply)
          break;
      }
      return new LegacyPlan
      {
        Direct = direct,
        Actions = groups.SelectMany(group => group)
          .Where(action => applied.ContainsKey(action.Name))
          .Select(action => (action, applied[action.Name])).ToList(),
        Unused = new Dictionary<string, int> { [Queen] = remaining[Queen], [Amazon] = remaining[Amazon] },
      };
    }

    private static ActionPlan LegacyChoice(
      string root,
      List<ActionPlan> actions,
      List<double> weights,
      Dictionary<string, long> counters)
    {
      bool positive = weights.Any(weight => weight > 0);
      int selected = 0;
      double score = double.PositiveInfinity;
      ulong uniform = ulong.MaxValue;
      for (int index = 0; index < actions.Count; index++)
      {
        ActionPlan action = actions[index];
        long counter = counters.TryGetValue(action.Name, out long count) ? count : 0;
        var series = new CounterBasedSeedSeries(root, "upgrade-source." + action.Name);
        if (positive)
        {
          if (weights[index] <= 0)
            continue;
          double candidate = -Math.Log(Math.Max(double.Epsilon, series.Unit(counter))) / weights[index];
          if (candidate < score)
          {
            score = candidate;
            selected = index;
          }
        }
        else
        {
          ulong candidate = series.Value(counter);
          if (candidate < uniform)
          {
            uniform = candidate;
            selected = index;
          }
        }
      }
      return actions[selected];
    }

    private static void GenerateFundamental(
      ApmwContractV2 contract,
      Effective effective,
      List<ActionPlan> actions,
      string root,
      out List<Piece> pieces,
      out List<Ledger> dormant,
      out List<Ledger> unallocated,
      out int normalized)
    {
      PrimaryAndCommon(contract, effective, out pieces, out unallocated, out int common);
      int chessmen = effective.Count("Chessmen");
      FundamentalPlan plan = PlanFundamental(
        contract,
        chessmen,
        effective.Count("Material") * contract.ExpectedMaterial["material_item"],
        effective.Count("Castler"),
        actions,
        root);
      var chessmenPieces = new List<Piece>();
      for (int ordinal = 0; ordinal < chessmen; ordinal++)
      {
        chessmenPieces.Add(new Piece
        {
          Id = "chessman:" + ordinal.ToString("D6", CultureInfo.InvariantCulture),
          Role = PawnSlot, Ordinal = ordinal, OriginAction = "chessman",
          OriginFamily = Pawn, Family = Pawn, Granted = contract.ExpectedMaterial[Pawn],
          Expected = contract.ExpectedMaterial[Pawn],
        });
      }
      pieces.AddRange(chessmenPieces);
      List<Piece> castlerCandidates = chessmenPieces.ToList();
      var gateway = new CounterBasedSeedSeries(root, "fundamental.gateway.role");
      for (int ordinal = 0; ordinal < plan.Locked; ordinal++)
      {
        int selected = gateway.Index(ordinal, castlerCandidates.Count);
        Piece piece = castlerCandidates[selected];
        castlerCandidates.RemoveAt(selected);
        piece.Role = LockedCastler;
        piece.Ordinal = ordinal;
        piece.OriginAction = "castler";
        piece.Family = Major;
        piece.Path.Add("castler");
        AddEntitlement(piece, Major);
        piece.Locked = true;
        piece.Granted += contract.Castler.NormalizedCost;
        piece.Expected = contract.ExpectedMaterial[Major];
      }
      ApplyGateway(contract, chessmenPieces, root, plan, "pawn-to-minor", MinorSlot, Minor);
      ApplyGateway(contract, chessmenPieces, root, plan, "pawn-to-major", MajorSlot, Major);
      Dictionary<string, ActionPlan> byName = actions.ToDictionary(action => action.Name, StringComparer.Ordinal);
      foreach (string actionName in new[]
      {
        "minor-to-major", "minor-to-jack", "major-to-jack",
        "major-to-queen", "jack-to-queen", "queen-to-amazon",
      })
      {
        int count = plan.Applied.TryGetValue(actionName, out int applied) ? applied : 0;
        if (count <= 0)
          continue;
        ActionPlan action = byName[actionName];
        List<Piece> candidates = chessmenPieces.Where(piece => !piece.Locked && piece.Family == action.From).ToList();
        var series = new CounterBasedSeedSeries(root, "upgrade-source." + actionName);
        int replacementCount = Math.Min(count, candidates.Count);
        for (int index = 0; index < replacementCount; index++)
        {
          int selected = series.Index(index, candidates.Count);
          Piece piece = candidates[selected];
          candidates.RemoveAt(selected);
          Upgrade(contract, piece, action);
        }
      }
      int spare = plan.Spare;
      if (byName.ContainsKey("major-to-jack"))
      {
        int credit = contract.ExpectedMaterial[Jack] - contract.ExpectedMaterial["castler"];
        List<Piece> candidates = chessmenPieces.Where(piece => piece.Locked && piece.Family == Major).ToList();
        int count = Math.Min(candidates.Count, spare / credit);
        var series = new CounterBasedSeedSeries(root, "upgrade-source.major-to-jack");
        for (int index = 0; index < count; index++)
        {
          int selected = series.Index(index, candidates.Count);
          Piece piece = candidates[selected];
          candidates.RemoveAt(selected);
          piece.Path.Add("major-to-jack");
          piece.Family = Jack;
          AddEntitlement(piece, Jack);
          piece.Granted += credit;
          piece.Expected = contract.ExpectedMaterial[Jack];
        }
        spare -= count * credit;
      }
      if (spare != 0)
      {
        unallocated.Add(new Ledger
        {
          Id = "unallocated:Material", Source = "Material", Amount = spare,
          Reason = "unspent-fundamental-material",
        });
      }
      foreach (Piece pawn in chessmenPieces.Where(piece => piece.Family == Pawn))
        AddEntitlement(pawn, Pawn);
      dormant = new List<Ledger>();
      normalized = common + chessmen * contract.ExpectedMaterial[Pawn] +
        effective.Count("Material") * contract.ExpectedMaterial["material_item"];
    }

    private static FundamentalPlan PlanFundamental(
      ApmwContractV2 contract,
      int chessmen,
      int material,
      int castlers,
      List<ActionPlan> actions,
      string root)
    {
      int spare = material;
      int locked = Math.Min(
        Math.Min(castlers, contract.Castler.Maximum),
        Math.Min(chessmen, spare / contract.Castler.NormalizedCost));
      spare -= locked * contract.Castler.NormalizedCost;
      var tiers = FamilyOrder.Skip(1).ToDictionary(family => family, family => 0, StringComparer.Ordinal);
      tiers[Pawn] = chessmen - locked;
      tiers[Major] = locked;
      Dictionary<string, ActionPlan> byName = actions.ToDictionary(action => action.Name, StringComparer.Ordinal);
      List<ActionPlan> ordered = FundamentalActionOrder.Where(byName.ContainsKey).Select(name => byName[name]).ToList();
      int[] priorities = ordered.Select(action => action.Priority).Distinct().OrderByDescending(value => value).ToArray();
      Dictionary<int, List<ActionPlan>> groups = priorities.ToDictionary(
        priority => priority,
        priority => ordered.Where(action => action.Priority == priority).ToList());
      var applied = new Dictionary<string, int>(StringComparer.Ordinal);
      var counters = new Dictionary<int, long>();
      while (true)
      {
        bool didApply = false;
        foreach (int priority in priorities)
        {
          var viable = new List<ActionPlan>();
          var weights = new List<double>();
          foreach (ActionPlan action in groups[priority])
          {
            int eligible = tiers[action.From] - (action.From == Major ? locked : 0);
            int credit = UpgradeCredit(contract, action);
            if (eligible <= 0 || credit > spare)
              continue;
            viable.Add(action);
            weights.Add(eligible * action.Proportion);
          }
          if (viable.Count == 0)
            continue;
          ActionPlan chosen;
          if (viable.Count == 1)
          {
            chosen = viable[0];
          }
          else
          {
            long counter = counters.TryGetValue(priority, out long value) ? value : 0;
            var series = new CounterBasedSeedSeries(root, "fundamental.wave.tie." + priority);
            int index = WeightedTieBreak.ChooseIndex(weights, series, counter);
            chosen = viable[index];
            counters[priority] = counter + 1;
          }
          int chosenCredit = UpgradeCredit(contract, chosen);
          spare -= chosenCredit;
          tiers[chosen.From]--;
          tiers[chosen.To]++;
          applied[chosen.Name] = applied.TryGetValue(chosen.Name, out int count) ? count + 1 : 1;
          didApply = true;
          break;
        }
        if (!didApply)
          break;
      }
      return new FundamentalPlan { Tiers = tiers, Applied = applied, Spare = spare, Locked = locked };
    }

    private static void ApplyGateway(
      ApmwContractV2 contract,
      List<Piece> pieces,
      string root,
      FundamentalPlan plan,
      string action,
      string role,
      string family)
    {
      int count = plan.Applied.TryGetValue(action, out int applied) ? applied : 0;
      List<Piece> candidates = pieces.Where(piece => !piece.Locked && piece.Family == Pawn).ToList();
      var series = new CounterBasedSeedSeries(root, "upgrade-source." + action);
      int roleOrdinal = 0;
      int replacementCount = Math.Min(count, candidates.Count);
      for (int index = 0; index < replacementCount; index++)
      {
        int selected = series.Index(index, candidates.Count);
        Piece piece = candidates[selected];
        candidates.RemoveAt(selected);
        piece.Role = role;
        piece.Ordinal = roleOrdinal++;
        piece.OriginAction = action;
        piece.Path.Add(action);
        piece.Family = family;
        AddEntitlement(piece, family);
        piece.Granted += contract.ExpectedMaterial[family] - contract.ExpectedMaterial[Pawn];
        piece.Expected = contract.ExpectedMaterial[family];
      }
    }

    private static void Upgrade(ApmwContractV2 contract, Piece piece, ActionPlan action)
    {
      if (action.RoleRule == "establish-minor-slot")
      {
        piece.Role = MinorSlot;
        piece.OriginAction = action.Name;
      }
      else if (action.RoleRule == "establish-major-slot")
      {
        piece.Role = MajorSlot;
        piece.OriginAction = action.Name;
      }
      else if (action.RoleRule != "preserve")
      {
        throw new ProjectionV2Exception("unsupported source role rule: " + action.RoleRule);
      }
      piece.Granted += UpgradeCredit(contract, action);
      piece.Family = action.To;
      AddEntitlement(piece, action.To);
      piece.Expected = contract.ExpectedMaterial[action.To];
      piece.Path.Add(action.Name);
    }

    private static int UpgradeCredit(ApmwContractV2 contract, ActionPlan action)
    {
      return Math.Max(0, contract.ExpectedMaterial[action.To] - contract.ExpectedMaterial[action.From]);
    }

    private static void AddEntitlement(Piece piece, string family)
    {
      if (!piece.Entitlements.Contains(family))
        piece.Entitlements.Add(family);
    }

    private static JsonObject ProjectSlots(
      ApmwContractV2 contract,
      Input input,
      Effective effective,
      GeometryStage stage,
      List<Piece> pieces,
      List<Ledger> dormant,
      List<Ledger> unallocated,
      int normalized)
    {
      List<Piece> frozen = pieces.ToList();
      Dictionary<string, List<Piece>> byRole = RoleOrder.ToDictionary(
        role => role,
        role => frozen.Where(piece => piece.Role == role).ToList(),
        StringComparer.Ordinal);
      var activeIds = byRole[PrimaryRoyal].Select(piece => piece.Id).ToHashSet(StringComparer.Ordinal);
      int backRemaining = stage.Files - 1;
      foreach (string role in new[] { AdditionalRoyal, LockedCastler })
      {
        List<Piece> selected = ActivationOrder(byRole[role]).Take(backRemaining).ToList();
        activeIds.UnionWith(selected.Select(piece => piece.Id));
        backRemaining -= selected.Count;
      }
      int activeNonPawns = frozen.Count(piece => activeIds.Contains(piece.Id) && NonPawnRoles.Contains(piece.Role));
      int remainingNonPawn = Math.Max(0, stage.NonPawnCapacity - activeNonPawns);
      foreach (string role in OrdinaryNonPawnRoles)
      {
        List<Piece> selected = ActivationOrder(byRole[role]).Take(remainingNonPawn).ToList();
        activeIds.UnionWith(selected.Select(piece => piece.Id));
        remainingNonPawn -= selected.Count;
      }
      activeNonPawns = frozen.Count(piece => activeIds.Contains(piece.Id) && NonPawnRoles.Contains(piece.Role));
      int activePawnCapacity = stage.GrossPawnCapacity - Math.Max(0, activeNonPawns - (stage.Files - 1));
      activeIds.UnionWith(ActivationOrder(byRole[PawnSlot]).Take(activePawnCapacity).Select(piece => piece.Id));
      List<Piece> active = frozen.Where(piece => activeIds.Contains(piece.Id)).OrderBy(SlotOutputKey).ToList();
      List<Piece> reserve = frozen.Where(piece => !activeIds.Contains(piece.Id)).OrderBy(ReserveKey).ToList();
      Dictionary<string, (int File, int Rank)> coordinates = Place(stage, active, input.Seeds);
      int requestedForwardness = effective.Count("Progressive Pawn Forwardness");
      int unspent = ApplyForwardness(stage, active, coordinates, requestedForwardness, input.Seeds);
      int appliedForwardness = requestedForwardness - unspent;
      JsonObject region = RegionUsageJson(stage, active, coordinates);
      HashSet<string> home = active.Where(piece => coordinates[piece.Id].Rank == 0)
        .Select(piece => piece.Id).ToHashSet(StringComparer.Ordinal);
      List<Ledger> activeLedger = active.Where(piece => piece.Granted != 0).Select(piece => new Ledger
      {
        Id = "active:" + piece.Id, Source = piece.OriginAction, Amount = piece.Granted,
        SlotId = piece.Id, Reason = "active-slot",
      }).ToList();
      List<Ledger> reserveLedger = reserve.Where(piece => piece.Granted != 0).Select(piece => new Ledger
      {
        Id = "reserve:" + piece.Id, Source = piece.OriginAction, Amount = piece.Granted,
        SlotId = piece.Id, Reason = "reserve-slot",
      }).ToList();
      int activeGranted = activeLedger.Sum(entry => entry.Amount);
      int missing = reserveLedger.Sum(entry => entry.Amount);
      int dormantMaterial = dormant.Sum(entry => entry.Amount);
      int unallocatedMaterial = unallocated.Sum(entry => entry.Amount);
      int accounted = activeGranted + missing + dormantMaterial + unallocatedMaterial;
      if (accounted != normalized)
        throw new ProjectionV2Exception(
          "normalized grant material is not conserved: expected " + normalized + ", accounted " + accounted);
      HashSet<string> activeEntitlements = active.SelectMany(piece => piece.Entitlements).ToHashSet(StringComparer.Ordinal);
      HashSet<string> reserveEntitlements = reserve.SelectMany(piece => piece.Entitlements).ToHashSet(StringComparer.Ordinal);

      return new JsonObject
      {
        ["contract_hash"] = contract.ManifestSha256,
        ["itemization"] = input.Itemization,
        ["ordering"] = input.Ordering,
        ["geometry_stage"] = stage.StageId,
        ["files"] = stage.Files,
        ["ranks"] = stage.Ranks,
        ["effective_counts"] = EffectiveJson(effective),
        ["owned_slots"] = SlotsJson(frozen.OrderBy(SlotOutputKey)),
        ["active_slots"] = SlotsJson(active),
        ["reserve_slots"] = SlotsJson(reserve),
        ["active_placements"] = new JsonArray(active.Select(piece =>
        {
          var coordinate = coordinates[piece.Id];
          return (JsonNode)new JsonObject
          {
            ["slot_id"] = piece.Id, ["file"] = coordinate.File, ["relative_rank"] = coordinate.Rank,
            ["formation_band"] = FormationBand(stage, coordinate.Rank),
          };
        }).ToArray()),
        ["active_counts"] = CountsJson(active),
        ["reserve_counts"] = CountsJson(reserve),
        ["active_material_ledger"] = LedgerJson(activeLedger),
        ["reserve_material_ledger"] = LedgerJson(reserveLedger),
        ["dormant_material_ledger"] = LedgerJson(dormant),
        ["unallocated_material_ledger"] = LedgerJson(unallocated),
        ["owned_expected_material"] = frozen.Where(piece => piece.Role != PrimaryRoyal).Sum(piece => piece.Expected),
        ["exact_active_material"] = active.Sum(piece => piece.Expected),
        ["active_granted_material"] = activeGranted,
        ["missing_material"] = missing,
        ["dormant_material"] = dormantMaterial,
        ["unallocated_material"] = unallocatedMaterial,
        ["normalized_grant_material"] = normalized,
        ["total_accounted_material"] = accounted,
        ["active_castlers"] = StringArray(active.Where(piece => piece.Locked).Select(piece => piece.Id)),
        ["reserve_castlers"] = StringArray(reserve.Where(piece => piece.Locked).Select(piece => piece.Id)),
        ["castling_eligible_slots"] = StringArray(active.Where(piece =>
          home.Contains(piece.Id) && (piece.Family == Major || piece.Family == Jack)).Select(piece => piece.Id)),
        ["available_promotion_families"] = StringArray(FamilyOrder.Where(activeEntitlements.Contains)),
        ["reserve_promotion_families"] = StringArray(FamilyOrder.Where(reserveEntitlements.Contains)),
        ["region_usage"] = region,
        ["applied_forwardness"] = appliedForwardness,
        ["unspent_forwardness"] = unspent,
      };
    }

    private static IEnumerable<Piece> ActivationOrder(IEnumerable<Piece> pieces)
    {
      return pieces.OrderByDescending(piece => piece.Expected)
        .ThenByDescending(piece => piece.Granted)
        .ThenBy(piece => piece.Ordinal)
        .ThenBy(piece => piece.Id, StringComparer.Ordinal);
    }

    private static (int Role, int Ordinal, string Id) SlotOutputKey(Piece piece)
    {
      return (Array.IndexOf(RoleOrder, piece.Role), piece.Ordinal, piece.Id);
    }

    private static (int Role, int Expected, int Granted, int NegativeOrdinal, string Id) ReserveKey(Piece piece)
    {
      return (Array.IndexOf(RoleOrder, piece.Role), piece.Expected, piece.Granted, -piece.Ordinal, piece.Id);
    }

    private static Dictionary<string, (int File, int Rank)> Place(
      GeometryStage stage,
      List<Piece> active,
      Seeds seeds)
    {
      var coordinates = active.Where(piece => piece.Role == PrimaryRoyal)
        .ToDictionary(piece => piece.Id, piece => (stage.Files / 2, 0), StringComparer.Ordinal);
      Dictionary<int, List<int>> available = Enumerable.Range(0, stage.Ranks - 3)
        .ToDictionary(rank => rank, rank => Enumerable.Range(0, stage.Files).ToList());
      available[0].Remove(stage.Files / 2);
      int mixed = stage.Ranks - 7;
      foreach (string role in new[] { AdditionalRoyal, LockedCastler, JackSlot, MajorSlot, MinorSlot })
      {
        List<Piece> rolePieces = active.Where(piece => piece.Role == role)
          .OrderBy(piece => piece.Ordinal).ThenBy(piece => piece.Id, StringComparer.Ordinal).ToList();
        IEnumerable<int> ranks = role == AdditionalRoyal || role == LockedCastler
          ? new[] { 0 }
          : Enumerable.Range(0, mixed + 1);
        PlaceAcross(stage, rolePieces, ranks, available, seeds, coordinates);
      }
      List<Piece> pawns = active.Where(piece => piece.Role == PawnSlot)
        .OrderBy(piece => piece.Ordinal).ThenBy(piece => piece.Id, StringComparer.Ordinal).ToList();
      PlaceAcross(stage, pawns, Enumerable.Range(1, stage.Ranks - 4), available, seeds, coordinates);
      if (coordinates.Count != active.Count)
        throw new ProjectionV2Exception("active projection selected slots that could not be placed");
      return coordinates;
    }

    private static void PlaceAcross(
      GeometryStage stage,
      List<Piece> pieces,
      IEnumerable<int> ranks,
      Dictionary<int, List<int>> available,
      Seeds seeds,
      Dictionary<string, (int File, int Rank)> coordinates)
    {
      List<Piece> remaining = pieces.ToList();
      foreach (int rank in ranks)
      {
        List<int> files = available[rank];
        if (remaining.Count == 0)
          break;
        if (files.Count == 0)
          continue;
        string role = remaining[0].Role;
        string band = FormationBand(stage, rank);
        string id = "placement." + role + "." + band;
        var series = new CounterBasedSeedSeries(PresentationRoot(seeds, id), id);
        int count = Math.Min(remaining.Count, files.Count);
        for (int index = 0; index < count; index++)
        {
          Piece piece = remaining[0];
          remaining.RemoveAt(0);
          int fileIndex = series.Index(index, files.Count);
          int file = files[fileIndex];
          files.RemoveAt(fileIndex);
          coordinates[piece.Id] = (file, rank);
        }
      }
      if (remaining.Count > 0)
        throw new ProjectionV2Exception("source role exceeded its assigned placement region");
    }

    private static int ApplyForwardness(
      GeometryStage stage,
      List<Piece> active,
      Dictionary<string, (int File, int Rank)> coordinates,
      int requested,
      Seeds seeds)
    {
      int remaining = Math.Max(0, requested);
      int edges = stage.Ranks - 5;
      for (int wave = edges; wave >= 1; wave--)
      {
        for (int edge = 0; edge < wave; edge++)
        {
          if (remaining <= 0)
            return 0;
          int source = edge + 1;
          int targetRank = source + 1;
          HashSet<(int File, int Rank)> occupied = coordinates.Values.ToHashSet();
          List<Piece> movable = active.Where(piece =>
            piece.Role == PawnSlot &&
            coordinates[piece.Id].Rank == source &&
            !occupied.Contains((coordinates[piece.Id].File, targetRank)))
            .OrderBy(piece => piece.Id, StringComparer.Ordinal).ToList();
          string id = "placement.pawn-slot.forwardness-" + source + "-" + targetRank;
          var series = new CounterBasedSeedSeries(PresentationRoot(seeds, id), id);
          int counter = 0;
          while (remaining > 0 && movable.Count > 0)
          {
            int selected = series.Index(counter++, movable.Count);
            Piece piece = movable[selected];
            movable.RemoveAt(selected);
            var current = coordinates[piece.Id];
            var target = (current.File, targetRank);
            if (coordinates.Values.Contains(target))
              continue;
            coordinates[piece.Id] = target;
            remaining--;
          }
        }
      }
      return remaining;
    }

    private static JsonObject RegionUsageJson(
      GeometryStage stage,
      List<Piece> active,
      Dictionary<string, (int File, int Rank)> coordinates)
    {
      int mixedCount = stage.Ranks - 7;
      var ranks = new JsonArray();
      int backNonPawns = 0;
      int mixedNonPawns = 0;
      int mixedPawns = 0;
      int pawnOnlyPawns = 0;
      for (int rank = 0; rank < stage.Ranks - 3; rank++)
      {
        List<Piece> rankPieces = active.Where(piece => coordinates[piece.Id].Rank == rank).ToList();
        int nonPawns = rankPieces.Count(piece => piece.Role != PawnSlot);
        int pawns = rankPieces.Count - nonPawns;
        string region = rank == 0 ? "back" : rank <= mixedCount ? "mixed" : "pawn-only";
        ranks.Add(new JsonObject
        {
          ["relative_rank"] = rank, ["region"] = region, ["capacity"] = stage.Files,
          ["non_pawns"] = nonPawns, ["pawns"] = pawns, ["empty"] = stage.Files - nonPawns - pawns,
        });
        if (rank == 0)
          backNonPawns = nonPawns - 1;
        else if (region == "mixed")
        {
          mixedNonPawns += nonPawns;
          mixedPawns += pawns;
        }
        else
        {
          pawnOnlyPawns += pawns;
        }
      }
      int activeNonPawns = active.Count(piece => NonPawnRoles.Contains(piece.Role));
      int activePawns = active.Count(piece => piece.Role == PawnSlot);
      int activePawnCapacity = stage.GrossPawnCapacity - Math.Max(0, activeNonPawns - (stage.Files - 1));
      return new JsonObject
      {
        ["back_optional_capacity"] = stage.Files - 1,
        ["mixed_capacity"] = stage.Files * mixedCount,
        ["pawn_only_capacity"] = stage.Files * 3,
        ["non_pawn_capacity"] = stage.NonPawnCapacity,
        ["gross_pawn_capacity"] = stage.GrossPawnCapacity,
        ["combined_non_primary_capacity"] = stage.CombinedNonPrimaryCapacity,
        ["active_pawn_capacity"] = activePawnCapacity,
        ["back_non_primary"] = backNonPawns,
        ["mixed_non_pawns"] = mixedNonPawns,
        ["mixed_pawns"] = mixedPawns,
        ["pawn_only_pawns"] = pawnOnlyPawns,
        ["unused_non_pawn_capacity"] = stage.NonPawnCapacity - activeNonPawns,
        ["unused_pawn_capacity"] = activePawnCapacity - activePawns,
        ["ranks"] = ranks,
      };
    }

    private static JsonObject EffectiveJson(Effective effective)
    {
      return new JsonObject
      {
        ["items"] = CountArray(effective.Items.Select(item => (item.Name, item.Count)), "name"),
        ["overcounts"] = CountArray(effective.Overcounts.Select(item => (item.Name, item.Count)), "name"),
        ["unlocks"] = CountArray(effective.Unlocks.Select(item => (item.Role, item.Count)), "role_id"),
        ["unlock_overcounts"] = CountArray(effective.UnlockOvercounts.Select(item => (item.Role, item.Count)), "role_id"),
      };
    }

    private static JsonArray CountArray(IEnumerable<(string Key, int Count)> values, string keyName)
    {
      return new JsonArray(values.Select(value => (JsonNode)new JsonObject
      {
        [keyName] = value.Key, ["count"] = value.Count,
      }).ToArray());
    }

    private static JsonArray SlotsJson(IEnumerable<Piece> pieces)
    {
      return new JsonArray(pieces.Select(piece => (JsonNode)new JsonObject
      {
        ["slot_id"] = piece.Id,
        ["source_role"] = piece.Role,
        ["source_ordinal"] = piece.Ordinal,
        ["role_origin_action"] = piece.OriginAction,
        ["final_family"] = piece.Family,
        ["upgrade_path"] = StringArray(piece.Path),
        ["locked_castler"] = piece.Locked,
        ["granted_material"] = piece.Granted,
        ["final_expected_material"] = piece.Expected,
        ["promotion_entitlement_families"] = StringArray(piece.Entitlements),
      }).ToArray());
    }

    private static JsonArray CountsJson(IEnumerable<Piece> pieces)
    {
      var counts = pieces.GroupBy(piece => (piece.Role, piece.Family))
        .ToDictionary(group => group.Key, group => group.Count());
      var output = new JsonArray();
      foreach (string role in RoleOrder)
        foreach (string family in FamilyOrder)
          if (counts.TryGetValue((role, family), out int count))
            output.Add(new JsonObject { ["source_role"] = role, ["final_family"] = family, ["count"] = count });
      return output;
    }

    private static JsonArray LedgerJson(IEnumerable<Ledger> entries)
    {
      return new JsonArray(entries.Select(entry => (JsonNode)new JsonObject
      {
        ["entry_id"] = entry.Id,
        ["source"] = entry.Source,
        ["amount"] = entry.Amount,
        ["slot_id"] = entry.SlotId == null ? null : JsonValue.Create(entry.SlotId),
        ["reason"] = entry.Reason,
      }).ToArray());
    }

    private static JsonArray StringArray(IEnumerable<string> values)
    {
      return new JsonArray(values.Select(value => (JsonNode)JsonValue.Create(value)).ToArray());
    }

    private static string FormationBand(GeometryStage stage, int rank)
    {
      if (rank == 0)
        return "back-rank";
      int mixed = stage.Ranks - 7;
      return rank <= mixed ? "mixed-" + rank : "pawn-only-" + (rank - mixed);
    }

    private static string PresentationRoot(Seeds seeds, string seriesId)
    {
      if (seriesId.Contains("pawn", StringComparison.Ordinal))
        return seeds.Pawn;
      if (seriesId.Contains("minor", StringComparison.Ordinal))
        return seeds.Minor;
      if (seriesId.Contains("queen", StringComparison.Ordinal) || seriesId.Contains("amazon", StringComparison.Ordinal))
        return seeds.Queen;
      if (seriesId.Contains("major", StringComparison.Ordinal) ||
          seriesId.Contains("jack", StringComparison.Ordinal) ||
          seriesId.Contains("castler", StringComparison.Ordinal))
        return seeds.Major;
      return seeds.StableRoot;
    }

    private static string StableId(string role, int ordinal)
    {
      return role + ":" + ordinal.ToString("D6", CultureInfo.InvariantCulture);
    }
  }

  internal static class ProjectionFixtureSerializer
  {
    public static string Serialize(ApmwContractV2 contract, JsonElement input)
    {
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));
      return ProjectionV2SemanticEngine.Project(contract, input).ToJsonString();
    }
  }
}
