using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Archipelago.APChessV
{
  /// <summary>
  /// An immutable, offline description of a released APMW projector archive.
  /// No production lock is committed until its referenced GitHub release exists.
  /// </summary>
  public sealed class ApmwProjectorLock
  {
    internal ApmwProjectorLock(
      string runtimeSemanticVersion,
      int protocolVersion,
      string contractHash,
      string sourceRepository,
      string sourceCommit,
      string releaseTag,
      IReadOnlyDictionary<string, ApmwProjectorAsset> assets)
    {
      RuntimeSemanticVersion = runtimeSemanticVersion;
      ProtocolVersion = protocolVersion;
      ContractHash = contractHash;
      SourceRepository = sourceRepository;
      SourceCommit = sourceCommit;
      ReleaseTag = releaseTag;
      Assets = assets;
    }

    public string RuntimeSemanticVersion { get; }
    public int ProtocolVersion { get; }
    public string ContractHash { get; }
    public string SourceRepository { get; }
    public string SourceCommit { get; }
    public string ReleaseTag { get; }
    public IReadOnlyDictionary<string, ApmwProjectorAsset> Assets { get; }
  }

  public sealed class ApmwProjectorAsset
  {
    internal ApmwProjectorAsset(
      string platform,
      string filename,
      string sha256,
      long size,
      ApmwProjectorExecutable executable)
    {
      Platform = platform;
      Filename = filename;
      Sha256 = sha256;
      Size = size;
      Executable = executable;
    }

    public string Platform { get; }
    public string Filename { get; }
    public string Sha256 { get; }
    public long Size { get; }
    public ApmwProjectorExecutable Executable { get; }
  }

  public sealed class ApmwProjectorExecutable
  {
    internal ApmwProjectorExecutable(string relativePath, string sha256)
    {
      RelativePath = relativePath;
      Sha256 = sha256;
    }

    public string RelativePath { get; }
    public string Sha256 { get; }
  }

  /// <summary>Strict parser for the committed projector lock format, version 1.</summary>
  public static class ApmwProjectorLockParser
  {
    public const string Schema = "apmw_projector_lock";
    public const int Version = 1;
    public const string RuntimeSemanticVersion = "0.1.0";
    public const int ProtocolVersion = 1;
    public const string ContractHash =
      "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15";

    private static readonly Regex SemverPattern = new Regex(
      "^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)$",
      RegexOptions.CultureInvariant);
    private static readonly Regex HashPattern = new Regex(
      "^[0-9a-f]{64}$", RegexOptions.CultureInvariant);
    private static readonly Regex CommitPattern = new Regex(
      "^[0-9a-f]{40}$", RegexOptions.CultureInvariant);
    private static readonly Regex RepositoryPattern = new Regex(
      "^[A-Za-z0-9][A-Za-z0-9_.-]*/[A-Za-z0-9][A-Za-z0-9_.-]*$",
      RegexOptions.CultureInvariant);
    private static readonly Regex ZipNamePattern = new Regex(
      "^[A-Za-z0-9][A-Za-z0-9._-]*\\.zip$", RegexOptions.CultureInvariant);

    public static ApmwProjectorLock Parse(string json)
    {
      if (json == null)
        throw new ArgumentNullException(nameof(json));

      using JsonDocument document = ParseDocument(json);
      JsonElement root = document.RootElement;
      ExactFields(root, "$",
        "schema", "version", "runtime_semantic_version", "protocol_version", "contract_hash",
        "source_repository", "source_commit", "release_tag", "assets");

      RequireEqual(String(root, "schema", "$"), Schema, "$.schema");
      if (Integer(root, "version", "$") != Version)
        throw Error("$.version must be " + Version);

      string runtime = String(root, "runtime_semantic_version", "$");
      if (!SemverPattern.IsMatch(runtime))
        throw Error("$.runtime_semantic_version must be a three-part semantic version");
      RequireEqual(runtime, RuntimeSemanticVersion, "$.runtime_semantic_version");

      int protocol = Integer(root, "protocol_version", "$");
      if (protocol != ProtocolVersion)
        throw Error("unsupported projector protocol version " + protocol);

      string contract = String(root, "contract_hash", "$");
      if (!HashPattern.IsMatch(contract))
        throw Error("$.contract_hash must be 64 lowercase hexadecimal characters");
      RequireEqual(contract, ContractHash, "$.contract_hash");

      string repository = String(root, "source_repository", "$");
      if (!RepositoryPattern.IsMatch(repository))
        throw Error("$.source_repository must be an owner/repository identifier");
      string commit = String(root, "source_commit", "$");
      if (!CommitPattern.IsMatch(commit))
        throw Error("$.source_commit must be exactly 40 lowercase hexadecimal characters");
      string tag = String(root, "release_tag", "$");
      if (!tag.StartsWith("apmw-projector-v", StringComparison.Ordinal) ||
          !SemverPattern.IsMatch(tag.Substring("apmw-projector-v".Length)))
        throw Error("$.release_tag must be apmw-projector-v followed by a semantic version");
      if (!System.String.Equals(tag.Substring("apmw-projector-v".Length), runtime, StringComparison.Ordinal))
        throw Error("$.release_tag version must equal $.runtime_semantic_version");

      JsonElement assetsElement = Property(root, "assets", "$");
      if (assetsElement.ValueKind != JsonValueKind.Object)
        throw Error("$.assets must be an object keyed by platform");
      ExactFields(assetsElement, "$.assets", "windows-x86", "windows-x64");

      var assets = new Dictionary<string, ApmwProjectorAsset>(StringComparer.Ordinal);
      var zipNames = new HashSet<string>(StringComparer.Ordinal);
      foreach (string platform in new[] { "windows-x86", "windows-x64" })
      {
        string path = "$.assets." + platform;
        JsonElement entry = Property(assetsElement, platform, "$.assets");
        ExactFields(entry, path, "filename", "sha256", "size", "executable");

        string filename = String(entry, "filename", path);
        if (!ZipNamePattern.IsMatch(filename))
          throw Error(path + ".filename must be a plain .zip file name");
        if (!zipNames.Add(filename))
          throw Error("$.assets contains duplicate filename " + filename);

        string sha256 = Sha256(entry, "sha256", path);
        long size = Int64(entry, "size", path);
        if (size <= 0)
          throw Error(path + ".size must be positive");
        JsonElement executable = Property(entry, "executable", path);
        ExactFields(executable, path + ".executable", "relative_path", "sha256");
        string relativePath = String(executable, "relative_path", path + ".executable");
        ValidateRelativeArchivePath(relativePath, path + ".executable.relative_path");
        string executableSha256 = Sha256(executable, "sha256", path + ".executable");

        assets.Add(platform, new ApmwProjectorAsset(
          platform, filename, sha256, size,
          new ApmwProjectorExecutable(relativePath, executableSha256)));
      }

      return new ApmwProjectorLock(
        runtime,
        protocol,
        contract,
        repository,
        commit,
        tag,
        new ReadOnlyDictionary<string, ApmwProjectorAsset>(assets));
    }

    private static JsonDocument ParseDocument(string json)
    {
      try
      {
        return JsonDocument.Parse(json, new JsonDocumentOptions
        {
          AllowTrailingCommas = false,
          CommentHandling = JsonCommentHandling.Disallow,
        });
      }
      catch (JsonException exception)
      {
        throw Error("invalid JSON: " + exception.Message, exception);
      }
    }

    private static void ExactFields(JsonElement element, string path, params string[] expected)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw Error(path + " must be an object");
      var actual = new HashSet<string>(StringComparer.Ordinal);
      foreach (JsonProperty property in element.EnumerateObject())
      {
        if (!actual.Add(property.Name))
          throw Error(path + " contains duplicate JSON property " + property.Name);
      }
      var required = new HashSet<string>(expected, StringComparer.Ordinal);
      if (!actual.SetEquals(required))
      {
        string missing = string.Join(",", required.Except(actual).OrderBy(value => value, StringComparer.Ordinal));
        string unknown = string.Join(",", actual.Except(required).OrderBy(value => value, StringComparer.Ordinal));
        throw Error(path + " fields differ; missing=[" + missing + "], unknown=[" + unknown + "]");
      }
    }

    private static JsonElement Property(JsonElement element, string name, string path)
    {
      if (!element.TryGetProperty(name, out JsonElement value))
        throw Error(path + "." + name + " is required");
      return value;
    }

    private static string String(JsonElement element, string name, string path)
    {
      JsonElement value = Property(element, name, path);
      if (value.ValueKind != JsonValueKind.String)
        throw Error(path + "." + name + " must be a string");
      string result = value.GetString();
      if (string.IsNullOrEmpty(result) || result.Any(character => character < 0x21 || character > 0x7e))
        throw Error(path + "." + name + " must be non-empty printable ASCII");
      return result;
    }

    private static int Integer(JsonElement element, string name, string path)
    {
      JsonElement value = Property(element, name, path);
      if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int result))
        throw Error(path + "." + name + " must be a 32-bit integer");
      return result;
    }

    private static long Int64(JsonElement element, string name, string path)
    {
      JsonElement value = Property(element, name, path);
      if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out long result))
        throw Error(path + "." + name + " must be a 64-bit integer");
      return result;
    }

    private static string Sha256(JsonElement element, string name, string path)
    {
      string value = String(element, name, path);
      if (!HashPattern.IsMatch(value))
        throw Error(path + "." + name + " must be 64 lowercase hexadecimal characters");
      return value;
    }

    private static void ValidateRelativeArchivePath(string value, string path)
    {
      if (value.IndexOf('\\') >= 0 || value.StartsWith("/", StringComparison.Ordinal) ||
          value.IndexOf(':') >= 0)
        throw Error(path + " must be a slash-separated, non-rooted archive path");
      string[] segments = value.Split('/');
      if (segments.Length == 0 || segments.Any(segment =>
        System.String.IsNullOrEmpty(segment) || segment == "." || segment == ".."))
        throw Error(path + " must not contain empty, current, or parent path segments");
    }

    private static void RequireEqual(string actual, string expected, string path)
    {
      if (!System.String.Equals(actual, expected, StringComparison.Ordinal))
        throw Error(path + " must equal " + expected);
    }

    private static FormatException Error(string message, Exception innerException = null)
    {
      return innerException == null
        ? new FormatException("Invalid APMW projector lock: " + message)
        : new FormatException("Invalid APMW projector lock: " + message, innerException);
    }
  }
}
