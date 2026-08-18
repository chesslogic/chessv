using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  internal sealed class ApmwSidecarRequest
  {
    private static readonly Regex ContractHashPattern = new Regex(
      "^[0-9a-f]{64}$",
      RegexOptions.CultureInvariant);

    public const int ProtocolVersion = 1;
    public const string DefaultExpectedRuntimeSemanticVersion = "0.1.0";

    public ApmwSidecarRequest(
      string requestId,
      string contractHash,
      JsonElement input,
      IEnumerable<string> geometries,
      string expectedRuntimeSemanticVersion = DefaultExpectedRuntimeSemanticVersion)
    {
      if (string.IsNullOrEmpty(requestId))
        throw new ArgumentException("request ID must be nonempty", nameof(requestId));
      if (string.IsNullOrEmpty(contractHash) || !ContractHashPattern.IsMatch(contractHash))
        throw new ArgumentException("contract hash must be a lowercase SHA-256 hash", nameof(contractHash));
      if (input.ValueKind != JsonValueKind.Object)
        throw new ArgumentException("sidecar input must be an object", nameof(input));
      if (!ApmwSidecarProtocol.IsValidSemanticVersion(expectedRuntimeSemanticVersion))
        throw new ArgumentException(
          "expected runtime semantic version must be a semantic version",
          nameof(expectedRuntimeSemanticVersion));

      ApmwSidecarProtocol.ValidateNoDuplicateProperties(input, "$.input");
      string[] geometryArray = geometries == null ? null : geometries.ToArray();
      if (geometryArray == null || geometryArray.Length == 0)
        throw new ArgumentException("at least one geometry is required", nameof(geometries));
      if (geometryArray.Any(geometry => string.IsNullOrEmpty(geometry)))
        throw new ArgumentException("geometries must be nonempty strings", nameof(geometries));
      if (geometryArray.Any(geometry =>
          !ApmwSidecarInputSnapshot.OrderedProgressive6x8GeometryStages.Contains(
            geometry,
            StringComparer.Ordinal)))
      {
        throw new ArgumentException(
          "geometries must use supported APMW stages",
          nameof(geometries));
      }
      if (geometryArray.Distinct(StringComparer.Ordinal).Count() != geometryArray.Length)
        throw new ArgumentException("geometries must be unique", nameof(geometries));

      RequestId = requestId;
      ContractHash = contractHash;
      Input = input.Clone();
      Geometries = Array.AsReadOnly(geometryArray);
      ExpectedRuntimeSemanticVersion = expectedRuntimeSemanticVersion;
    }

    public string RequestId { get; }
    public string ContractHash { get; }
    public JsonElement Input { get; }
    public IReadOnlyList<string> Geometries { get; }
    public string ExpectedRuntimeSemanticVersion { get; }
  }

  internal sealed class ApmwSidecarProjectionResult
  {
    internal ApmwSidecarProjectionResult(string geometryStage, JsonElement projection)
    {
      GeometryStage = geometryStage;
      Projection = projection.Clone();
    }

    public string GeometryStage { get; }
    public JsonElement Projection { get; }
  }

  internal sealed class ApmwSidecarSuccessResponse
  {
    internal ApmwSidecarSuccessResponse(
      string requestId,
      string contractHash,
      string runtimeSemanticVersion,
      IReadOnlyList<ApmwSidecarProjectionResult> results)
    {
      RequestId = requestId;
      ContractHash = contractHash;
      RuntimeSemanticVersion = runtimeSemanticVersion;
      Results = results;
    }

    public string RequestId { get; }
    public string ContractHash { get; }
    public string RuntimeSemanticVersion { get; }
    public IReadOnlyList<ApmwSidecarProjectionResult> Results { get; }
  }

  internal sealed class ApmwSidecarErrorResponse
  {
    internal ApmwSidecarErrorResponse(string code, string message)
    {
      Code = code;
      Message = message;
    }

    public string Code { get; }
    public string Message { get; }
  }

  internal sealed class ApmwSidecarResponse
  {
    private ApmwSidecarResponse(
      ApmwSidecarSuccessResponse success,
      ApmwSidecarErrorResponse error)
    {
      Success = success;
      Error = error;
    }

    public ApmwSidecarSuccessResponse Success { get; }
    public ApmwSidecarErrorResponse Error { get; }
    public bool IsError { get { return Error != null; } }

    internal static ApmwSidecarResponse FromSuccess(ApmwSidecarSuccessResponse success)
    {
      return new ApmwSidecarResponse(success, null);
    }

    internal static ApmwSidecarResponse FromError(ApmwSidecarErrorResponse error)
    {
      return new ApmwSidecarResponse(null, error);
    }
  }

  internal sealed class ApmwSidecarProtocolException : Exception
  {
    public ApmwSidecarProtocolException(string message) : base(message) { }
  }

  internal static class ApmwSidecarProtocol
  {
    private static readonly Regex SemanticVersionPattern = new Regex(
      "^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)$",
      RegexOptions.CultureInvariant);

    private static readonly string[] ProjectionFields =
    {
      "contract_hash", "itemization", "ordering", "geometry_stage", "files", "ranks",
      "effective_counts", "owned_slots", "active_slots", "reserve_slots", "active_placements",
      "active_counts", "reserve_counts", "active_material_ledger", "reserve_material_ledger",
      "dormant_material_ledger", "unallocated_material_ledger", "owned_expected_material",
      "exact_active_material", "active_granted_material", "missing_material", "dormant_material",
      "unallocated_material", "normalized_grant_material", "total_accounted_material",
      "active_castlers", "reserve_castlers", "castling_eligible_slots",
      "available_promotion_families", "reserve_promotion_families", "region_usage",
      "applied_forwardness", "unspent_forwardness",
    };

    public static string SerializeRequest(ApmwSidecarRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      return JsonSerializer.Serialize(new RequestWire
      {
        ProtocolVersion = ApmwSidecarRequest.ProtocolVersion,
        RequestId = request.RequestId,
        ContractHash = request.ContractHash,
        Input = request.Input,
        Geometries = request.Geometries,
      });
    }

    public static ApmwSidecarResponse ParseResponse(string json, ApmwSidecarRequest request)
    {
      if (json == null)
        throw new ArgumentNullException(nameof(json));
      if (request == null)
        throw new ArgumentNullException(nameof(request));

      try
      {
        using (JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
          AllowTrailingCommas = false,
          CommentHandling = JsonCommentHandling.Disallow,
        }))
        {
          JsonElement root = document.RootElement;
          if (root.ValueKind != JsonValueKind.Object)
            throw new ApmwSidecarProtocolException("$ must be an object");
          ValidateNoDuplicateProperties(root, "$");
          return root.TryGetProperty("error", out JsonElement error)
            ? ParseError(root, error)
            : ParseSuccess(root, request);
        }
      }
      catch (JsonException exception)
      {
        throw new ApmwSidecarProtocolException("sidecar response is not valid JSON: " + exception.Message);
      }
    }

    internal static void ValidateNoDuplicateProperties(JsonElement element, string path)
    {
      if (element.ValueKind == JsonValueKind.Object)
      {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
          if (!names.Add(property.Name))
            throw new ApmwSidecarProtocolException(path + " contains duplicate property '" + property.Name + "'");
          ValidateNoDuplicateProperties(property.Value, path + "." + property.Name);
        }
      }
      else if (element.ValueKind == JsonValueKind.Array)
      {
        int index = 0;
        foreach (JsonElement item in element.EnumerateArray())
          ValidateNoDuplicateProperties(item, path + "[" + index++ + "]");
      }
    }

    private static ApmwSidecarResponse ParseError(JsonElement root, JsonElement error)
    {
      ExactObject(root, "$", "error");
      ExactObject(error, "$.error", "code", "message");
      ValidateNoDuplicateProperties(error, "$.error");
      return ApmwSidecarResponse.FromError(new ApmwSidecarErrorResponse(
        RequiredNonemptyString(error, "code", "$.error"),
        RequiredNonemptyString(error, "message", "$.error")));
    }

    private static ApmwSidecarResponse ParseSuccess(JsonElement root, ApmwSidecarRequest request)
    {
      ExactObject(root, "$", "protocol_version", "request_id", "contract_hash",
        "runtime_semantic_version", "results");
      if (RequiredInteger(root, "protocol_version", "$") != ApmwSidecarRequest.ProtocolVersion)
        throw new ApmwSidecarProtocolException("sidecar response has an unsupported protocol version");
      if (!string.Equals(RequiredString(root, "request_id", "$"), request.RequestId, StringComparison.Ordinal))
        throw new ApmwSidecarProtocolException("sidecar response request ID does not match the request");
      if (!string.Equals(RequiredString(root, "contract_hash", "$"), request.ContractHash, StringComparison.Ordinal))
        throw new ApmwSidecarProtocolException("sidecar response contract hash does not match the request");

      string semanticVersion = RequiredString(root, "runtime_semantic_version", "$");
      if (!IsValidSemanticVersion(semanticVersion))
        throw new ApmwSidecarProtocolException("sidecar response has an invalid runtime semantic version");
      if (!string.Equals(
        semanticVersion,
        request.ExpectedRuntimeSemanticVersion,
        StringComparison.Ordinal))
        throw new ApmwSidecarProtocolException(
          "sidecar response runtime semantic version does not match the requested projector version");

      JsonElement results = RequiredProperty(root, "results", "$");
      if (results.ValueKind != JsonValueKind.Array)
        throw new ApmwSidecarProtocolException("$.results must be an array");
      if (results.GetArrayLength() != request.Geometries.Count)
        throw new ApmwSidecarProtocolException("$.results does not contain every requested geometry exactly once");

      var parsed = new List<ApmwSidecarProjectionResult>();
      int index = 0;
      foreach (JsonElement result in results.EnumerateArray())
      {
        string path = "$.results[" + index + "]";
        ExactObject(result, path, "geometry_stage", "projection");
        ValidateNoDuplicateProperties(result, path);
        string geometryStage = RequiredString(result, "geometry_stage", path);
        if (!string.Equals(geometryStage, request.Geometries[index], StringComparison.Ordinal))
          throw new ApmwSidecarProtocolException(path + " is missing or out of order for requested geometry '" +
            request.Geometries[index] + "'");

        JsonElement projection = RequiredProperty(result, "projection", path);
        ValidateProjection(projection, path + ".projection", geometryStage, request);
        parsed.Add(new ApmwSidecarProjectionResult(geometryStage, projection));
        index++;
      }

      return ApmwSidecarResponse.FromSuccess(new ApmwSidecarSuccessResponse(
        request.RequestId,
        request.ContractHash,
        semanticVersion,
        parsed.AsReadOnly()));
    }

    private static void ValidateProjection(
      JsonElement projection,
      string path,
      string geometryStage,
      ApmwSidecarRequest request)
    {
      ExactObject(projection, path, ProjectionFields);
      if (!string.Equals(
        RequiredString(projection, "contract_hash", path),
        request.ContractHash,
        StringComparison.Ordinal))
      {
        throw new ApmwSidecarProtocolException(path + ".contract_hash does not match the request");
      }
      if (!string.Equals(
        RequiredString(projection, "geometry_stage", path),
        geometryStage,
        StringComparison.Ordinal))
      {
        throw new ApmwSidecarProtocolException(path + ".geometry_stage does not match its result");
      }

      string itemization = RequiredString(projection, "itemization", path);
      string ordering = RequiredString(projection, "ordering", path);
      if ((itemization != "legacy" && itemization != "fundamental") ||
          (ordering != "stable" && ordering != "chaos"))
      {
        throw new ApmwSidecarProtocolException(path + " has an invalid semantic mode");
      }
      ValidateProjectionGeometry(projection, path, geometryStage);
      ValidateEffectiveCounts(RequiredProperty(projection, "effective_counts", path), path + ".effective_counts");
      ValidateArray(RequiredProperty(projection, "owned_slots", path), path + ".owned_slots", ValidateOwnedSlot);
      ValidateArray(RequiredProperty(projection, "active_slots", path), path + ".active_slots", ValidateOwnedSlot);
      ValidateArray(RequiredProperty(projection, "reserve_slots", path), path + ".reserve_slots", ValidateOwnedSlot);
      ValidateArray(
        RequiredProperty(projection, "active_placements", path),
        path + ".active_placements",
        ValidatePlacement);
      ValidateArray(RequiredProperty(projection, "active_counts", path), path + ".active_counts", ValidateRoleFamilyCount);
      ValidateArray(RequiredProperty(projection, "reserve_counts", path), path + ".reserve_counts", ValidateRoleFamilyCount);
      foreach (string name in new[]
      {
        "active_material_ledger", "reserve_material_ledger", "dormant_material_ledger",
        "unallocated_material_ledger",
      })
      {
        ValidateArray(RequiredProperty(projection, name, path), path + "." + name, ValidateLedgerEntry);
      }
      foreach (string name in new[]
      {
        "owned_expected_material", "exact_active_material", "active_granted_material",
        "missing_material", "dormant_material", "unallocated_material",
        "normalized_grant_material", "total_accounted_material", "applied_forwardness",
        "unspent_forwardness",
      })
      {
        RequiredInteger(projection, name, path);
      }
      foreach (string name in new[]
      {
        "active_castlers", "reserve_castlers", "castling_eligible_slots",
        "available_promotion_families", "reserve_promotion_families",
      })
      {
        ValidateStringArray(RequiredProperty(projection, name, path), path + "." + name);
      }
      ValidateRegionUsage(RequiredProperty(projection, "region_usage", path), path + ".region_usage");
    }

    private static void ValidateProjectionGeometry(JsonElement projection, string path, string stage)
    {
      int expectedFiles;
      int expectedRanks;
      switch (stage)
      {
        case "6x8": expectedFiles = 6; expectedRanks = 8; break;
        case "8x8": expectedFiles = 8; expectedRanks = 8; break;
        case "10x8": expectedFiles = 10; expectedRanks = 8; break;
        case "10x10": expectedFiles = 10; expectedRanks = 10; break;
        case "12x10": expectedFiles = 12; expectedRanks = 10; break;
        case "12x12": expectedFiles = 12; expectedRanks = 12; break;
        default:
          throw new ApmwSidecarProtocolException(path + " has an unknown geometry stage");
      }
      if (RequiredInteger(projection, "files", path) != expectedFiles ||
          RequiredInteger(projection, "ranks", path) != expectedRanks)
      {
        throw new ApmwSidecarProtocolException(path + " dimensions do not match its geometry stage");
      }
    }

    private static void ValidateEffectiveCounts(JsonElement value, string path)
    {
      ExactObject(value, path, "items", "overcounts", "unlocks", "unlock_overcounts");
      ValidateArray(RequiredProperty(value, "items", path), path + ".items", ValidateItemCount);
      ValidateArray(RequiredProperty(value, "overcounts", path), path + ".overcounts", ValidateItemCount);
      ValidateArray(RequiredProperty(value, "unlocks", path), path + ".unlocks", ValidateUnlockCount);
      ValidateArray(
        RequiredProperty(value, "unlock_overcounts", path),
        path + ".unlock_overcounts",
        ValidateUnlockCount);
    }

    private static void ValidateItemCount(JsonElement value, string path)
    {
      ExactObject(value, path, "name", "count");
      RequiredString(value, "name", path);
      RequiredInteger(value, "count", path);
    }

    private static void ValidateUnlockCount(JsonElement value, string path)
    {
      ExactObject(value, path, "role_id", "count");
      RequiredString(value, "role_id", path);
      RequiredInteger(value, "count", path);
    }

    private static void ValidateOwnedSlot(JsonElement value, string path)
    {
      ExactObject(
        value,
        path,
        "slot_id", "source_role", "source_ordinal", "role_origin_action", "final_family",
        "upgrade_path", "locked_castler", "granted_material", "final_expected_material",
        "promotion_entitlement_families");
      RequiredString(value, "slot_id", path);
      RequiredString(value, "source_role", path);
      RequiredInteger(value, "source_ordinal", path);
      RequiredString(value, "role_origin_action", path);
      RequiredString(value, "final_family", path);
      ValidateStringArray(RequiredProperty(value, "upgrade_path", path), path + ".upgrade_path");
      RequiredBoolean(value, "locked_castler", path);
      RequiredInteger(value, "granted_material", path);
      RequiredInteger(value, "final_expected_material", path);
      ValidateStringArray(
        RequiredProperty(value, "promotion_entitlement_families", path),
        path + ".promotion_entitlement_families");
    }

    private static void ValidatePlacement(JsonElement value, string path)
    {
      ExactObject(value, path, "slot_id", "file", "relative_rank", "formation_band");
      RequiredString(value, "slot_id", path);
      RequiredInteger(value, "file", path);
      RequiredInteger(value, "relative_rank", path);
      RequiredString(value, "formation_band", path);
    }

    private static void ValidateRoleFamilyCount(JsonElement value, string path)
    {
      ExactObject(value, path, "source_role", "final_family", "count");
      RequiredString(value, "source_role", path);
      RequiredString(value, "final_family", path);
      RequiredInteger(value, "count", path);
    }

    private static void ValidateLedgerEntry(JsonElement value, string path)
    {
      ExactObject(value, path, "entry_id", "source", "amount", "slot_id", "reason");
      RequiredString(value, "entry_id", path);
      RequiredString(value, "source", path);
      RequiredInteger(value, "amount", path);
      JsonElement slotId = RequiredProperty(value, "slot_id", path);
      if (slotId.ValueKind != JsonValueKind.Null && slotId.ValueKind != JsonValueKind.String)
        throw new ApmwSidecarProtocolException(path + ".slot_id must be a string or null");
      RequiredString(value, "reason", path);
    }

    private static void ValidateRegionUsage(JsonElement value, string path)
    {
      ExactObject(
        value,
        path,
        "back_optional_capacity", "mixed_capacity", "pawn_only_capacity", "non_pawn_capacity",
        "gross_pawn_capacity", "combined_non_primary_capacity", "active_pawn_capacity",
        "back_non_primary", "mixed_non_pawns", "mixed_pawns", "pawn_only_pawns",
        "unused_non_pawn_capacity", "unused_pawn_capacity", "ranks");
      foreach (string name in new[]
      {
        "back_optional_capacity", "mixed_capacity", "pawn_only_capacity", "non_pawn_capacity",
        "gross_pawn_capacity", "combined_non_primary_capacity", "active_pawn_capacity",
        "back_non_primary", "mixed_non_pawns", "mixed_pawns", "pawn_only_pawns",
        "unused_non_pawn_capacity", "unused_pawn_capacity",
      })
      {
        RequiredInteger(value, name, path);
      }
      ValidateArray(RequiredProperty(value, "ranks", path), path + ".ranks", ValidateFormationRank);
    }

    private static void ValidateFormationRank(JsonElement value, string path)
    {
      ExactObject(value, path, "relative_rank", "region", "capacity", "non_pawns", "pawns", "empty");
      RequiredInteger(value, "relative_rank", path);
      RequiredString(value, "region", path);
      RequiredInteger(value, "capacity", path);
      RequiredInteger(value, "non_pawns", path);
      RequiredInteger(value, "pawns", path);
      RequiredInteger(value, "empty", path);
    }

    private static void ValidateArray(
      JsonElement value,
      string path,
      Action<JsonElement, string> validateItem)
    {
      if (value.ValueKind != JsonValueKind.Array)
        throw new ApmwSidecarProtocolException(path + " must be an array");
      int index = 0;
      foreach (JsonElement item in value.EnumerateArray())
        validateItem(item, path + "[" + index++ + "]");
    }

    private static void ValidateStringArray(JsonElement value, string path)
    {
      ValidateArray(value, path, (item, itemPath) =>
      {
        if (item.ValueKind != JsonValueKind.String)
          throw new ApmwSidecarProtocolException(itemPath + " must be a string");
      });
    }

    private static void ExactObject(JsonElement element, string path, params string[] expected)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw new ApmwSidecarProtocolException(path + " must be an object");

      var actual = new HashSet<string>(StringComparer.Ordinal);
      foreach (JsonProperty property in element.EnumerateObject())
      {
        if (!actual.Add(property.Name))
          throw new ApmwSidecarProtocolException(path + " contains duplicate property '" + property.Name + "'");
      }

      var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);
      if (!actual.SetEquals(expectedSet))
        throw new ApmwSidecarProtocolException(path + " has unknown or missing fields");
    }

    private static JsonElement RequiredProperty(JsonElement element, string name, string path)
    {
      JsonElement value;
      if (!element.TryGetProperty(name, out value))
        throw new ApmwSidecarProtocolException(path + " is missing required field '" + name + "'");
      return value;
    }

    private static string RequiredString(JsonElement element, string name, string path)
    {
      JsonElement value = RequiredProperty(element, name, path);
      if (value.ValueKind != JsonValueKind.String)
        throw new ApmwSidecarProtocolException(path + "." + name + " must be a string");
      return value.GetString();
    }

    private static string RequiredNonemptyString(JsonElement element, string name, string path)
    {
      string value = RequiredString(element, name, path);
      if (value.Length == 0)
        throw new ApmwSidecarProtocolException(path + "." + name + " must be nonempty");
      return value;
    }

    internal static bool IsValidSemanticVersion(string value)
    {
      return !string.IsNullOrEmpty(value) && SemanticVersionPattern.IsMatch(value);
    }

    private static int RequiredInteger(JsonElement element, string name, string path)
    {
      JsonElement value = RequiredProperty(element, name, path);
      int number;
      if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out number))
        throw new ApmwSidecarProtocolException(path + "." + name + " must be an integer");
      return number;
    }

    private static bool RequiredBoolean(JsonElement element, string name, string path)
    {
      JsonElement value = RequiredProperty(element, name, path);
      if (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False)
        throw new ApmwSidecarProtocolException(path + "." + name + " must be a boolean");
      return value.GetBoolean();
    }

    private sealed class RequestWire
    {
      [JsonPropertyName("protocol_version")]
      public int ProtocolVersion { get; set; }

      [JsonPropertyName("request_id")]
      public string RequestId { get; set; }

      [JsonPropertyName("contract_hash")]
      public string ContractHash { get; set; }

      [JsonPropertyName("input")]
      public JsonElement Input { get; set; }

      [JsonPropertyName("geometries")]
      public IReadOnlyList<string> Geometries { get; set; }
    }
  }

  internal interface IApmwSidecarRunner
  {
    Task<ApmwSidecarSuccessResponse> RunAsync(
      ApmwSidecarRequest request,
      CancellationToken cancellationToken);
  }

  internal abstract class ApmwSidecarExecutionException : Exception
  {
    protected ApmwSidecarExecutionException(string message, int? exitCode, string standardError, Exception innerException)
      : base(message, innerException)
    {
      ExitCode = exitCode;
      StandardError = standardError;
    }

    public int? ExitCode { get; }
    public string StandardError { get; }
  }

  internal sealed class ApmwSidecarRemoteException : ApmwSidecarExecutionException
  {
    internal ApmwSidecarRemoteException(ApmwSidecarErrorResponse error, int exitCode, string standardError)
      : base("sidecar error " + error.Code + ": " + error.Message, exitCode, standardError, null)
    {
      Code = error.Code;
      RemoteMessage = error.Message;
    }

    public string Code { get; }
    public string RemoteMessage { get; }
  }

  internal sealed class ApmwSidecarProcessException : ApmwSidecarExecutionException
  {
    internal ApmwSidecarProcessException(
      string message,
      int? exitCode,
      string standardError,
      Exception innerException = null)
      : base(message, exitCode, standardError, innerException) { }
  }

  internal sealed class ApmwSidecarTimeoutException : ApmwSidecarExecutionException
  {
    internal ApmwSidecarTimeoutException(
      TimeSpan timeout,
      string standardError,
      string cleanupDiagnostic = null)
      : base(
        "sidecar did not exit within " + timeout +
        (string.IsNullOrEmpty(cleanupDiagnostic) ? string.Empty : "; cleanup: " + cleanupDiagnostic),
        null,
        standardError,
        null)
    {
      Timeout = timeout;
      CleanupDiagnostic = cleanupDiagnostic;
    }

    public TimeSpan Timeout { get; }
    public string CleanupDiagnostic { get; }
  }

  internal sealed class ApmwSidecarProcessRunner : IApmwSidecarRunner
  {
    private static readonly TimeSpan TerminationConfirmationTimeout = TimeSpan.FromSeconds(5);

    private readonly string executablePath;
    private readonly TimeSpan timeout;

    public ApmwSidecarProcessRunner(string executablePath, TimeSpan timeout)
    {
      if (string.IsNullOrEmpty(executablePath) || !Path.IsPathRooted(executablePath))
        throw new ArgumentException("sidecar executable path must be explicit and absolute", nameof(executablePath));
      if (timeout <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(nameof(timeout));

      this.executablePath = executablePath;
      this.timeout = timeout;
    }

    public async Task<ApmwSidecarSuccessResponse> RunAsync(
      ApmwSidecarRequest request,
      CancellationToken cancellationToken)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request));
      cancellationToken.ThrowIfCancellationRequested();
      if (!File.Exists(executablePath))
        throw new ApmwSidecarProcessException(
          "sidecar executable does not exist or is not a file: " + executablePath,
          null,
          string.Empty);

      var startInfo = new ProcessStartInfo
      {
        FileName = executablePath,
        UseShellExecute = false,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
      };

      using (var process = new Process { StartInfo = startInfo })
      {
        try
        {
          if (!process.Start())
            throw new ApmwSidecarProcessException(
              "sidecar process did not start: " + executablePath,
              null,
              string.Empty);
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
          throw new ApmwSidecarProcessException(
            "could not start sidecar executable '" + executablePath + "': " + exception.Message,
            null,
            string.Empty,
            exception);
        }
        catch (InvalidOperationException exception)
        {
          throw new ApmwSidecarProcessException(
            "could not start sidecar executable '" + executablePath + "': " + exception.Message,
            null,
            string.Empty,
            exception);
        }

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        Task writeRequest = WriteRequestAndCloseAsync(process, ApmwSidecarProtocol.SerializeRequest(request));
        Task exited = process.WaitForExitAsync();
        Task timeoutTask = Task.Delay(timeout);
        Task cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        Task completed = await Task.WhenAny(exited, timeoutTask, cancellationTask).ConfigureAwait(false);

        if (completed != exited)
        {
          TerminationResult termination = await TerminateAndConfirmAsync(process, exited).ConfigureAwait(false);
          if (termination.Confirmed)
          {
            await Task.WhenAll(standardOutput, standardError).ConfigureAwait(false);
            await ObserveTerminatedWriterAsync(writeRequest).ConfigureAwait(false);
          }
          else
          {
            _ = ObserveTerminatedTask(standardOutput);
            _ = ObserveTerminatedTask(standardError);
            _ = ObserveTerminatedTask(writeRequest);
          }
          if (completed == cancellationTask)
            throw new OperationCanceledException(
              CancellationMessage(termination.CleanupDiagnostic),
              cancellationToken);
          throw new ApmwSidecarTimeoutException(
            timeout,
            termination.Confirmed ? standardError.Result : string.Empty,
            termination.CleanupDiagnostic);
        }

        await Task.WhenAll(standardOutput, standardError).ConfigureAwait(false);
        int exitCode = process.ExitCode;
        try
        {
          await writeRequest.ConfigureAwait(false);
        }
        catch (IOException exception)
        {
          throw new ApmwSidecarProcessException(
            "could not write sidecar request",
            exitCode,
            standardError.Result,
            exception);
        }
        catch (ObjectDisposedException exception)
        {
          throw new ApmwSidecarProcessException(
            "could not write sidecar request",
            exitCode,
            standardError.Result,
            exception);
        }
        ApmwSidecarResponse response;
        try
        {
          response = ApmwSidecarProtocol.ParseResponse(standardOutput.Result, request);
        }
        catch (ApmwSidecarProtocolException exception)
        {
          throw new ApmwSidecarProcessException(
            "sidecar produced an invalid protocol response",
            exitCode,
            standardError.Result,
            exception);
        }

        if (response.IsError)
        {
          if (exitCode == 0)
            throw new ApmwSidecarProcessException(
              "sidecar returned an error envelope but exited successfully",
              exitCode,
              standardError.Result);
          throw new ApmwSidecarRemoteException(response.Error, exitCode, standardError.Result);
        }
        if (exitCode != 0)
          throw new ApmwSidecarProcessException(
            "sidecar exited with code " + exitCode + " after a success response",
            exitCode,
            standardError.Result);

        return response.Success;
      }
    }

    private static async Task WriteRequestAndCloseAsync(Process process, string requestJson)
    {
      await process.StandardInput.WriteAsync(requestJson).ConfigureAwait(false);
      await process.StandardInput.FlushAsync().ConfigureAwait(false);
      process.StandardInput.Close();
    }

    private static async Task<TerminationResult> TerminateAndConfirmAsync(Process process, Task exited)
    {
      string diagnostic = null;
      if (!HasExited(process, ref diagnostic))
      {
        string treeKillFailure = TryKill(process, true);
        if (!string.IsNullOrEmpty(treeKillFailure))
          diagnostic = AppendDiagnostic(diagnostic, treeKillFailure);
        if (!HasExited(process, ref diagnostic))
        {
          string rootKillFailure = TryKill(process, false);
          if (!string.IsNullOrEmpty(rootKillFailure))
            diagnostic = AppendDiagnostic(diagnostic, rootKillFailure);
        }
      }

      if (HasExited(process, ref diagnostic))
      {
        await exited.ConfigureAwait(false);
        return new TerminationResult(true, diagnostic);
      }

      Task confirmationDelay = Task.Delay(TerminationConfirmationTimeout);
      if (await Task.WhenAny(exited, confirmationDelay).ConfigureAwait(false) == exited)
      {
        await exited.ConfigureAwait(false);
        return new TerminationResult(true, diagnostic);
      }

      return new TerminationResult(
        false,
        AppendDiagnostic(
          diagnostic,
          "could not confirm sidecar process exit after termination attempt"));
    }

    private static bool HasExited(Process process, ref string diagnostic)
    {
      try
      {
        return process.HasExited;
      }
      catch (InvalidOperationException exception)
      {
        diagnostic = AppendDiagnostic(diagnostic, "could not query sidecar process exit: " + exception.Message);
        return false;
      }
    }

    private static string TryKill(Process process, bool entireProcessTree)
    {
      try
      {
        process.Kill(entireProcessTree);
        return null;
      }
      catch (InvalidOperationException)
      {
        return "sidecar process exited while termination was requested";
      }
      catch (System.ComponentModel.Win32Exception exception)
      {
        return "sidecar termination failed: " + exception.Message;
      }
      catch (NotSupportedException exception)
      {
        return "sidecar tree termination is unsupported: " + exception.Message;
      }
    }

    private static string AppendDiagnostic(string current, string addition)
    {
      if (string.IsNullOrEmpty(addition))
        return current;
      return string.IsNullOrEmpty(current) ? addition : current + "; " + addition;
    }

    private static string CancellationMessage(string cleanupDiagnostic)
    {
      return string.IsNullOrEmpty(cleanupDiagnostic)
        ? "sidecar execution was cancelled"
        : "sidecar execution was cancelled; cleanup: " + cleanupDiagnostic;
    }

    private static Task ObserveTerminatedWriterAsync(Task writeRequest)
    {
      return ObserveTerminatedTask(writeRequest);
    }

    private static Task ObserveTerminatedTask(Task task)
    {
      return task.ContinueWith(
        completed =>
        {
          // Killing the process can concurrently break its stdin pipe; timeout/cancellation remains primary.
          if (completed.IsFaulted)
            _ = completed.Exception;
        },
        CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default);
    }

    private sealed class TerminationResult
    {
      public TerminationResult(bool confirmed, string cleanupDiagnostic)
      {
        Confirmed = confirmed;
        CleanupDiagnostic = cleanupDiagnostic;
      }

      public bool Confirmed { get; }
      public string CleanupDiagnostic { get; }
    }
  }
}
