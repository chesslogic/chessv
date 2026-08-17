using Archipelago.APChessV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace ChessV.Test
{
  [TestClass]
  public class ProjectionV2FixtureParityTests
  {
    private const string CasesSha256 =
      "8d644bee7d7565f45b572d1e93c8f6e544a1efdb8b1a8f198172c8f528e8e98c";

    private static string FixturePath(string name)
    {
      return Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", name);
    }

    //[TestMethod]
    // Skipped since the Python fixture is not available to the remote GitHub Action.
    public void CasesFixture_IsByteIdenticalToCorrectedPythonFixture()
    {
      byte[] bytes = File.ReadAllBytes(FixturePath("cases.json"));
      string hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
      Assert.AreEqual(CasesSha256, hash);
    }

    [TestMethod]
    public void CasesFixture_AllInputsAndErrorsMatchPureCSharpEngine()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(
        File.ReadAllText(FixturePath("baseline.json")));
      using JsonDocument document = JsonDocument.Parse(
        File.ReadAllText(FixturePath("cases.json")));

      foreach (JsonElement fixtureCase in document.RootElement.GetProperty("cases").EnumerateArray())
      {
        string id = fixtureCase.GetProperty("id").GetString();
        try
        {
          string actualJson = ProjectionFixtureSerializer.Serialize(
            contract,
            fixtureCase.GetProperty("input"));
          if (fixtureCase.TryGetProperty("error", out JsonElement expectedError))
            Assert.Fail(id + " expected error: " + expectedError.GetString());
          using JsonDocument actualDocument = JsonDocument.Parse(actualJson);
          AssertJsonEqual(
            fixtureCase.GetProperty("output"),
            actualDocument.RootElement,
            id + ".output");
        }
        catch (ProjectionV2Exception exception)
        {
          if (!fixtureCase.TryGetProperty("error", out JsonElement expectedError))
            Assert.Fail(id + " unexpected error: " + exception.Message);
          Assert.AreEqual(expectedError.GetString(), exception.Message, id);
        }
      }
    }

    [TestMethod]
    public void CasesFixture_PrfAndFundamentalVectorsMatch()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(
        File.ReadAllText(FixturePath("baseline.json")));
      using JsonDocument document = JsonDocument.Parse(
        File.ReadAllText(FixturePath("cases.json")));
      JsonElement root = document.RootElement;

      foreach (JsonElement vector in root.GetProperty("series_vectors").EnumerateArray())
      {
        var series = new CounterBasedSeedSeries(
          vector.GetProperty("root").GetString(),
          vector.GetProperty("series_id").GetString());
        long counter = vector.GetProperty("counter").GetInt64();
        Assert.AreEqual(vector.GetProperty("value").GetString(), series.Value(counter).ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(vector.GetProperty("index").GetInt32(), series.Index(counter, vector.GetProperty("count").GetInt32()));
        Assert.AreEqual(
          vector.GetProperty("unit").GetString(),
          series.Unit(counter).ToString("G17", CultureInfo.InvariantCulture));
      }

      foreach (JsonElement vector in root.GetProperty("fundamental_plan_vectors").EnumerateArray())
      {
        ProjectionV2SemanticEngine.FundamentalPlan plan =
          ProjectionV2SemanticEngine.CharacterizeFundamental(
            contract,
            vector.GetProperty("chessmen").GetInt32(),
            vector.GetProperty("material_budget").GetInt32(),
            vector.GetProperty("castlers").GetInt32(),
            vector.GetProperty("seeds"),
            vector.GetProperty("upgrade_preferences"));
        Assert.AreEqual(vector.GetProperty("spare_material").GetInt32(), plan.Spare, vector.GetProperty("id").GetString());
        foreach (JsonProperty tier in vector.GetProperty("tier_counts").EnumerateObject())
          Assert.AreEqual(tier.Value.GetInt32(), plan.Tiers[tier.Name], vector.GetProperty("id").GetString() + ":" + tier.Name);
      }
    }

    private static void AssertJsonEqual(JsonElement expected, JsonElement actual, string path)
    {
      Assert.AreEqual(expected.ValueKind, actual.ValueKind, path + " kind");
      switch (expected.ValueKind)
      {
        case JsonValueKind.Object:
          JsonProperty[] expectedProperties = expected.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal).ToArray();
          JsonProperty[] actualProperties = actual.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal).ToArray();
          CollectionAssert.AreEqual(
            expectedProperties.Select(item => item.Name).ToArray(),
            actualProperties.Select(item => item.Name).ToArray(),
            path + " fields");
          foreach (JsonProperty property in expectedProperties)
            AssertJsonEqual(property.Value, actual.GetProperty(property.Name), path + "." + property.Name);
          break;
        case JsonValueKind.Array:
          JsonElement.ArrayEnumerator expectedItems = expected.EnumerateArray();
          JsonElement.ArrayEnumerator actualItems = actual.EnumerateArray();
          Assert.AreEqual(expected.GetArrayLength(), actual.GetArrayLength(), path + " length");
          int index = 0;
          while (expectedItems.MoveNext() && actualItems.MoveNext())
            AssertJsonEqual(expectedItems.Current, actualItems.Current, path + "[" + index++ + "]");
          break;
        case JsonValueKind.String:
          Assert.AreEqual(expected.GetString(), actual.GetString(), path);
          break;
        case JsonValueKind.Number:
          Assert.AreEqual(expected.GetInt64(), actual.GetInt64(), path);
          break;
        case JsonValueKind.True:
        case JsonValueKind.False:
          Assert.AreEqual(expected.GetBoolean(), actual.GetBoolean(), path);
          break;
        case JsonValueKind.Null:
          break;
        default:
          Assert.Fail(path + " unsupported JSON kind " + expected.ValueKind);
          break;
      }
    }
  }
}
