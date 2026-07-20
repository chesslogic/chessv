using System;
using System.IO;
using Archipelago.APChessV;

namespace APMW.Test
{
  [TestClass]
  public class ApmwProjectorLockTests
  {
    private static string Fixture
    {
      get
      {
        return File.ReadAllText(Path.Combine(
          AppContext.BaseDirectory, "Fixtures", "ProjectorLock", "synthetic-lock.json"));
      }
    }

    [TestMethod]
    public void Parse_AcceptsStrictSyntheticFixture()
    {
      ApmwProjectorLock lockFile = ApmwProjectorLockParser.Parse(Fixture);

      Assert.AreEqual("0.1.0", lockFile.RuntimeSemanticVersion);
      Assert.AreEqual(1, lockFile.ProtocolVersion);
      Assert.AreEqual("chesslogic/Archipelago", lockFile.SourceRepository);
      Assert.AreEqual(2, lockFile.Assets.Count);
      Assert.AreEqual("ApmwProjector.exe", lockFile.Assets["windows-x64"].Executable.RelativePath);
    }

    [DataTestMethod]
    [DataRow("\"version\": 1", "\"version\": 2", "version")]
    [DataRow("\"protocol_version\": 1", "\"protocol_version\": 2", "unsupported projector protocol")]
    [DataRow("\"source_commit\": \"0123456789abcdef0123456789abcdef01234567\"",
      "\"source_commit\": \"0123456789abcdef0123456789abcdef0123456\"",
      "source_commit")]
    [DataRow("\"relative_path\": \"ApmwProjector.exe\"",
      "\"relative_path\": \"../ApmwProjector.exe\"", "relative_path")]
    public void Parse_RejectsInvalidRequiredValues(string find, string replace, string expectedError)
    {
      FormatException exception = Assert.ThrowsException<FormatException>(
        () => ApmwProjectorLockParser.Parse(Fixture.Replace(find, replace)));

      StringAssert.Contains(exception.Message, expectedError);
    }

    [TestMethod]
    public void Parse_RejectsUnknownAndDuplicateFields()
    {
      string unknown = Fixture.Replace(
        "\"version\": 1,",
        "\"version\": 1,\n  \"unlocked\": true,");
      StringAssert.Contains(
        Assert.ThrowsException<FormatException>(() => ApmwProjectorLockParser.Parse(unknown)).Message,
        "unknown=[unlocked]");

      string duplicate = Fixture.Replace(
        "\"version\": 1,",
        "\"version\": 1,\n  \"version\": 1,");
      StringAssert.Contains(
        Assert.ThrowsException<FormatException>(() => ApmwProjectorLockParser.Parse(duplicate)).Message,
        "duplicate JSON property version");
    }

    [TestMethod]
    public void Parse_RejectsMissingPlatformDuplicateAssetAndAssetIdentityFields()
    {
      string missingPlatform = Fixture.Replace(
        "    \"windows-x86\": {",
        "    \"windows-x86-removed\": {");
      StringAssert.Contains(
        Assert.ThrowsException<FormatException>(() => ApmwProjectorLockParser.Parse(missingPlatform)).Message,
        "missing=[windows-x86]");

      string duplicateZip = Fixture.Replace(
        "apmw-projector-windows-x64.zip",
        "apmw-projector-windows-x86.zip");
      StringAssert.Contains(
        Assert.ThrowsException<FormatException>(() => ApmwProjectorLockParser.Parse(duplicateZip)).Message,
        "duplicate filename");

      string repeatedIdentity = Fixture.Replace(
        "      \"filename\": \"apmw-projector-windows-x64.zip\",",
        "      \"runtime_semantic_version\": \"0.1.0\",\n      \"filename\": \"apmw-projector-windows-x64.zip\",");
      StringAssert.Contains(
        Assert.ThrowsException<FormatException>(() => ApmwProjectorLockParser.Parse(repeatedIdentity)).Message,
        "unknown=[runtime_semantic_version]");
    }
  }
}
