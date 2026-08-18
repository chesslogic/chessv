using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using static Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class ApmwGeometrySelectionTests
  {
    private static string Baseline
    {
      get
      {
        return File.ReadAllText(Path.Combine(
          AppContext.BaseDirectory,
          "Fixtures",
          "ProjectionV2",
          "baseline.json"));
      }
    }

    [TestCleanup]
    public void Cleanup()
    {
      ApmwConfig._instance = null;
      ApmwCore._instance = null;
    }

    [TestMethod]
    public void CurrentContract_ParsesSlotObjectStrictlyAndRejectsMalformedHashAndMajor()
    {
      ApmwContractV2 parsed = ApmwGeometryResolver.ParseCurrentContract(JObject.Parse(Baseline));
      Assert.AreEqual(
        "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15",
        parsed.ManifestSha256);

      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwGeometryResolver.ParseCurrentContract("{not-json")).Message,
        "invalid JSON");

      JObject badHash = JObject.Parse(Baseline);
      badHash["minimum_client_version"] = "0.4.1";
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwGeometryResolver.ParseCurrentContract(badHash)).Message,
        "manifest SHA-256 mismatch");

      JObject unsupported = JObject.Parse(Baseline);
      unsupported["version"]["major"] = 3;
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwGeometryResolver.ParseCurrentContract(unsupported)).Message,
        "unsupported contract major version 3");
    }

    [DataTestMethod]
    [DataRow(0, 0, "8x8")]
    [DataRow(1, 0, "8x8,10x8")]
    [DataRow(0, 1, "8x8")]
    [DataRow(1, 1, "8x8,10x8,10x10")]
    [DataRow(2, 1, "8x8,10x8,10x10,12x10")]
    [DataRow(2, 2, "8x8,10x8,10x10,12x10,12x12")]
    [DataRow(99, 99, "8x8,10x8,10x10,12x10,12x12")]
    public void CurrentUnlocks_AreIndependentClampedAndFilteredByValidPairWhitelist(
      int fileUnlocks,
      int rankUnlocks,
      string expectedStages)
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);

      IReadOnlyList<ApmwGeometryOption> options =
        ApmwGeometryResolver.ResolveCurrent(contract, fileUnlocks, rankUnlocks);

      Assert.AreEqual(
        expectedStages,
        string.Join(",", options.Select(option => option.StageId)));
    }

    [TestMethod]
    public void LegacyAdapter_AlwaysOffersEightByEightAndSuperAddsExactTenByEight()
    {
      CollectionAssert.AreEqual(
        new[] { "8x8" },
        ApmwGeometryResolver.ResolveLegacy(false).Select(option => option.StageId).ToArray());
      CollectionAssert.AreEqual(
        new[] { "8x8", "10x8" },
        ApmwGeometryResolver.ResolveLegacy(true).Select(option => option.StageId).ToArray());
    }

    [DataTestMethod]
    [DataRow(0, 0, "6x8")]
    [DataRow(1, 0, "6x8,8x8")]
    [DataRow(2, 0, "6x8,8x8,10x8")]
    [DataRow(2, 1, "6x8,8x8,10x8,10x10")]
    [DataRow(3, 1, "6x8,8x8,10x8,10x10,12x10")]
    [DataRow(3, 2, "6x8,8x8,10x8,10x10,12x10,12x12")]
    [DataRow(99, 99, "6x8,8x8,10x8,10x10,12x10,12x12")]
    public void OrderedProgressive6x8_PrependsCompactStageThenUsesExistingOrderedSequence(
      int fileUnlocks,
      int rankUnlocks,
      string expectedStages)
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);

      IReadOnlyList<ApmwGeometryOption> options =
        ApmwGeometryResolver.ResolveCurrent(
          contract,
          fileUnlocks,
          rankUnlocks,
          Goal.OrderedProgressive6x8);

      Assert.AreEqual(
        expectedStages,
        string.Join(",", options.Select(option => option.StageId)));
    }

    [TestMethod]
    public void LegacyOrderedProgressive_StillStartsAtEightByEight()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);

      IReadOnlyList<ApmwGeometryOption> options =
        ApmwGeometryResolver.ResolveCurrent(
          contract,
          0,
          0,
          Goal.OrderedProgressive);

      CollectionAssert.AreEqual(
        new[] { "8x8" },
        options.Select(option => option.StageId).ToArray());
    }

    [TestMethod]
    public void CurrentResolver_RejectsUnknownGoalInsteadOfUsingLegacyGeometry()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);

      Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        ApmwGeometryResolver.ResolveCurrent(
          contract,
          0,
          0,
          (Goal)5));
    }

    [TestMethod]
    public void OrderedProgressive6x8_SelectionStartsCompactThenExposesEightByEight()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);
      var model = new ApmwGeometrySelectionModel();

      model.Connect(ApmwGeometryResolver.ResolveCurrent(
        contract,
        0,
        0,
        Goal.OrderedProgressive6x8));
      Assert.AreEqual("6x8", model.SelectedOption.StageId);

      model.Refresh(ApmwGeometryResolver.ResolveCurrent(
        contract,
        1,
        0,
        Goal.OrderedProgressive6x8));
      CollectionAssert.AreEqual(
        new[] { "6x8", "8x8" },
        model.AvailableOptions.Select(option => option.StageId).ToArray());
      Assert.AreEqual("6x8", model.SelectedOption.StageId);
      Assert.IsTrue(model.Select("8x8"));
      Assert.AreEqual("8x8", model.SelectedOption.StageId);
    }

    [TestMethod]
    public void Selection_DefaultsLargestRetainsExplicitSmallerChoiceAndResetsDisconnected()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);
      var model = new ApmwGeometrySelectionModel();
      IReadOnlyList<ApmwGeometryOption> first =
        ApmwGeometryResolver.ResolveCurrent(contract, 1, 1);

      model.Connect(first);
      Assert.IsTrue(model.IsConnected);
      Assert.AreEqual("10x10", model.SelectedOption.StageId);

      Assert.IsTrue(model.Select("8x8"));
      model.Refresh(ApmwGeometryResolver.ResolveCurrent(contract, 2, 2));
      Assert.AreEqual("8x8", model.SelectedOption.StageId);
      Assert.IsFalse(model.Select("8x10"), "invalid Cartesian pairs are never selectable");

      model.Connect(ApmwGeometryResolver.ResolveCurrent(contract, 2, 1));
      Assert.AreEqual("12x10", model.SelectedOption.StageId);

      model.Reset();
      Assert.IsFalse(model.IsConnected);
      Assert.AreEqual("8x8", model.SelectedOption.StageId);
      CollectionAssert.AreEqual(
        new[] { "8x8" },
        model.AvailableOptions.Select(option => option.StageId).ToArray());
    }

    [TestMethod]
    public void StageMapping_UsesOnlyTheFiveStaticRegisteredGameNames()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);
      IReadOnlyList<ApmwGeometryOption> options =
        ApmwGeometryResolver.ResolveCurrent(contract, 2, 2);

      CollectionAssert.AreEqual(
        new[]
        {
          "8x8=Archipelago Multiworld",
          "10x8=Archipelago Multiworld Super-Sized",
          "10x10=Archipelago Multiworld 10x10",
          "12x10=Archipelago Multiworld 12x10",
          "12x12=Archipelago Multiworld 12x12",
        },
        options.Select(option => option.StageId + "=" + option.RegisteredGameName).ToArray());
      CollectionAssert.AreEqual(
        new[] { "8x8", "10x8", "10x10", "12x10", "12x12" },
        options.Select(option => option.DisplayLabel).ToArray());
    }

    [TestMethod]
    public void CompactStageMapping_UsesTheRegisteredSixByEightGameName()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(Baseline);

      ApmwGeometryOption option = ApmwGeometryResolver.ResolveCurrent(
        contract,
        0,
        0,
        Goal.OrderedProgressive6x8).Single();

      Assert.AreEqual("6x8", option.StageId);
      Assert.AreEqual(ApmwProfiles.SixByEightGameName, option.RegisteredGameName);
      Assert.AreEqual("6x8", option.DisplayLabel);
    }

    [TestMethod]
    public void ItemHandler_ClampsGeometryCountsRefreshesAndOwnsOneHelperSubscription()
    {
      var helper = new CountingReceivedItemsHelper()
        .Add(ApmwConstants.ProgressiveItems.BoardFiles, 7)
        .Add(ApmwConstants.ProgressiveItems.BoardRanks, 8)
        .Add(ApmwConstants.ProgressiveItems.SuperSizeMe);
      var handler = new ItemHandler(helper);
      int refreshes = 0;
      handler.ReceivedItemsChanged += (sender, args) => refreshes++;

      Assert.AreEqual(1, helper.SubscriptionCount);
      Assert.AreEqual(2, handler.GeometryUnlocks.BoardFileUnlockCount);
      Assert.AreEqual(2, handler.GeometryUnlocks.BoardRankUnlockCount);
      Assert.IsTrue(handler.GeometryUnlocks.LegacySuperSizeUnlocked);

      helper.RaiseItemReceived();
      Assert.AreEqual(1, refreshes);
      Assert.AreEqual(1, helper.SubscriptionCount);

      handler.Unhook();
      Assert.AreEqual(0, helper.SubscriptionCount);
    }

    [TestMethod]
    public void ItemHandler_AllowsTheExtraFileUnlockOnlyForGoalFour()
    {
      ApmwConfig.getInstance().Instantiate(new Dictionary<string, object>
      {
        ["goal"] = 4,
      });
      var helper = new CountingReceivedItemsHelper()
        .Add(ApmwConstants.ProgressiveItems.BoardFiles, 7);
      var handler = new ItemHandler(helper);

      Assert.AreEqual(3, handler.GeometryUnlocks.BoardFileUnlockCount);

      handler.Unhook();

      ApmwConfig._instance = new ApmwConfig();
      ApmwConfig.getInstance().Instantiate(new Dictionary<string, object>
      {
        ["goal"] = 1,
      });
      handler = new ItemHandler(helper);

      Assert.AreEqual(2, handler.GeometryUnlocks.BoardFileUnlockCount);

      handler.Unhook();
    }

    private sealed class CountingReceivedItemsHelper : IReceivedItemsHelper
    {
      private readonly Dictionary<string, long> itemIds = new Dictionary<string, long>();
      private readonly Dictionary<long, string> itemNames = new Dictionary<long, string>();
      private readonly List<ItemInfo> items = new List<ItemInfo>();
      private ItemReceivedHandler itemReceived;
      private long nextItemId = 900000;

      public int SubscriptionCount { get; private set; }

      public event ItemReceivedHandler ItemReceived
      {
        add
        {
          itemReceived += value;
          SubscriptionCount++;
        }
        remove
        {
          itemReceived -= value;
          SubscriptionCount--;
        }
      }

      public ReadOnlyCollection<ItemInfo> AllItemsReceived
      {
        get { return new ReadOnlyCollection<ItemInfo>(items); }
      }

      public int Index { get { return 0; } }

      public CountingReceivedItemsHelper Add(string name, int count = 1)
      {
        if (!itemIds.TryGetValue(name, out long itemId))
        {
          itemId = nextItemId++;
          itemIds.Add(name, itemId);
          itemNames.Add(itemId, name);
        }
        for (int index = 0; index < count; index++)
          items.Add(CreateItemInfo(itemId));
        return this;
      }

      public void RaiseItemReceived()
      {
        itemReceived?.Invoke(null);
      }

      public bool Any()
      {
        return items.Count > 0;
      }

      public ItemInfo DequeueItem()
      {
        if (items.Count == 0)
          return null;
        ItemInfo item = items[0];
        items.RemoveAt(0);
        return item;
      }

      public string GetItemName(long itemId, string game)
      {
        return itemNames.TryGetValue(itemId, out string name) ? name : null;
      }

      public ItemInfo PeekItem()
      {
        return items.Count == 0 ? null : items[0];
      }

      private static ItemInfo CreateItemInfo(long itemId)
      {
        return new ItemInfo(
          new NetworkItem
          {
            Item = itemId,
            Player = 1,
          },
          ApmwConstants.TrackerName,
          ApmwConstants.TrackerName,
          null,
          new PlayerInfo(
            0,
            1,
            "ApmwGeometrySelectionTests",
            "ApmwGeometrySelectionTests",
            ApmwConstants.TrackerName,
            Array.Empty<NetworkSlot>(),
            Array.Empty<int>()));
      }
    }
  }
}
