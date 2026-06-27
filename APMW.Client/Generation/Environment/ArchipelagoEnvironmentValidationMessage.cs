using System;

namespace Archipelago.APChessV.Generation.Environment
{
  public sealed class ArchipelagoEnvironmentValidationMessage
  {
    public ArchipelagoEnvironmentValidationMessage(
      ArchipelagoEnvironmentValidationSeverity severity,
      string message,
      string relatedPath = null)
    {
      if (string.IsNullOrWhiteSpace(message))
      {
        throw new ArgumentException("Validation message text is required.", nameof(message));
      }

      Severity = severity;
      Message = message;
      RelatedPath = relatedPath;
    }

    public ArchipelagoEnvironmentValidationSeverity Severity { get; }

    public string Message { get; }

    public string RelatedPath { get; }

    public override string ToString()
    {
      return string.IsNullOrWhiteSpace(RelatedPath)
        ? $"{Severity}: {Message}"
        : $"{Severity}: {Message} ({RelatedPath})";
    }
  }
}
