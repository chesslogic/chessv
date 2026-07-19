using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ChessV.Test
{
  internal static class ApmwContractTestFixture
  {
    public static JObject Document()
    {
      return JObject.Parse(File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "ProjectionV2",
        "baseline.json")));
    }
  }
}
