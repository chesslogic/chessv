using System;
using Archipelago.APChessV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ChessV.Test
{
    [TestClass]
    public class ClientVersionContractTests
    {
        [TestMethod]
        public void RequiredClientVersion_AcceptsStrictThreePartString()
        {
            Assert.IsTrue(ArchipelagoClient.TryParseRequiredClientVersion(
                new JValue("0.3.3"),
                out Version version,
                out string error));
            Assert.AreEqual(new Version(0, 3, 3), version);
            Assert.IsNull(error);
        }

        [DataTestMethod]
        [DataRow("not-a-version")]
        [DataRow("0.3")]
        [DataRow("0.3.3.0")]
        [DataRow(" 0.3.3")]
        [DataRow("")]
        public void RequiredClientVersion_RejectsMalformedStrings(string rawValue)
        {
            Assert.IsFalse(ArchipelagoClient.TryParseRequiredClientVersion(
                rawValue,
                out Version version,
                out string error));
            Assert.IsNull(version);
            StringAssert.Contains(error, "Invalid required_chess_client_version");
        }

        [TestMethod]
        public void RequiredClientVersion_RejectsNonStringValues()
        {
            Assert.IsFalse(ArchipelagoClient.TryParseRequiredClientVersion(
                303,
                out Version version,
                out string error));
            Assert.IsNull(version);
            StringAssert.Contains(error, "expected a semantic version string");
        }

        [TestMethod]
        public void ContractMinimum_MatchesReleasedClientVersion()
        {
            string fixture = File.ReadAllText(Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "ProjectionV2",
                "baseline.json"));
            ApmwContractV2 contract = ApmwContractV2Parser.Parse(fixture);

            Assert.AreEqual(contract.MinimumClientVersion, ApmwConstants.ClientVersion);
            Assert.IsTrue(ArchipelagoClient.IsClientVersionCompatible(
                new Version(contract.MinimumClientVersion)));
            Assert.IsFalse(ArchipelagoClient.IsClientVersionCompatible(new Version("0.4.1")));
        }
    }
}
