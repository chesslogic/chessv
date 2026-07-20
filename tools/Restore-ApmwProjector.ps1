#requires -Version 7.0
[CmdletBinding(DefaultParameterSetName = 'Restore')]
param(
  [Parameter(Mandatory = $true)]
  [string]$LockPath,

  [ValidateSet('x86', 'x64')]
  [string]$Architecture,

  [Parameter(Mandatory = $true)]
  [string]$DestinationRoot,

  [Parameter(ParameterSetName = 'ValidateArchive', Mandatory = $true)]
  [string]$ArchivePath,

  [Parameter(ParameterSetName = 'ValidateArchive', Mandatory = $true)]
  [switch]$ValidateArchive
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not ('System.Text.Json.JsonDocument' -as [type])) {
  Add-Type -Path (Join-Path $PSHOME 'System.Text.Json.dll')
}

$script:ContractHash = 'f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15'
$script:BuildManifestName = 'apmw-projector-manifest.json'

function Fail([string]$Message) {
  throw "Invalid APMW projector lock: $Message"
}

function Get-RequiredProperty([System.Text.Json.JsonElement]$Object, [string]$Name, [string]$Path) {
  $value = [System.Text.Json.JsonElement]::new()
  if (-not $Object.TryGetProperty($Name, [ref]$value)) {
    Fail "$Path.$Name is required"
  }
  return $value
}

function Get-AsciiString([System.Text.Json.JsonElement]$Object, [string]$Name, [string]$Path) {
  $value = Get-RequiredProperty $Object $Name $Path
  if ($value.ValueKind -ne [System.Text.Json.JsonValueKind]::String) {
    Fail "$Path.$Name must be a string"
  }
  $text = $value.GetString()
  if ([string]::IsNullOrEmpty($text) -or $text -notmatch '^[\x21-\x7e]+$') {
    Fail "$Path.$Name must be non-empty printable ASCII"
  }
  return $text
}

function Get-Integer([System.Text.Json.JsonElement]$Object, [string]$Name, [string]$Path) {
  $value = Get-RequiredProperty $Object $Name $Path
  [int]$result = 0
  if ($value.ValueKind -ne [System.Text.Json.JsonValueKind]::Number -or -not $value.TryGetInt32([ref]$result)) {
    Fail "$Path.$Name must be a 32-bit integer"
  }
  return $result
}

function Get-Int64([System.Text.Json.JsonElement]$Object, [string]$Name, [string]$Path) {
  $value = Get-RequiredProperty $Object $Name $Path
  [long]$result = 0
  if ($value.ValueKind -ne [System.Text.Json.JsonValueKind]::Number -or -not $value.TryGetInt64([ref]$result)) {
    Fail "$Path.$Name must be a 64-bit integer"
  }
  return $result
}

function Assert-ExactProperties(
  [System.Text.Json.JsonElement]$Object,
  [string[]]$Expected,
  [string]$Path
) {
  if ($Object.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) {
    Fail "$Path must be an object"
  }
  $actual = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
  foreach ($property in $Object.EnumerateObject()) {
    if (-not $actual.Add($property.Name)) {
      Fail "$Path contains duplicate JSON property $($property.Name)"
    }
  }
  $expectedSet = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]$Expected, [System.StringComparer]::Ordinal)
  if (-not $actual.SetEquals($expectedSet)) {
    $missing = @($expectedSet | Where-Object { -not $actual.Contains($_) } | Sort-Object) -join ','
    $unknown = @($actual | Where-Object { -not $expectedSet.Contains($_) } | Sort-Object) -join ','
    Fail "$Path fields differ; missing=[$missing], unknown=[$unknown]"
  }
}

function Assert-Sha256([string]$Value, [string]$Path) {
  if ($Value -notmatch '^[0-9a-f]{64}$') {
    Fail "$Path must be 64 lowercase hexadecimal characters"
  }
}

function Assert-RelativeArchivePath([string]$Value, [string]$Path) {
  if ($Value.Contains('\') -or $Value.StartsWith('/') -or $Value.Contains(':')) {
    Fail "$Path must be a slash-separated, non-rooted archive path"
  }
  foreach ($segment in $Value.Split('/')) {
    if ([string]::IsNullOrEmpty($segment) -or $segment -eq '.' -or $segment -eq '..') {
      Fail "$Path must not contain empty, current, or parent path segments"
    }
  }
}

function Read-ApmwProjectorLock([string]$Path) {
  $fullPath = [IO.Path]::GetFullPath($Path)
  $options = [System.Text.Json.JsonDocumentOptions]::new()
  $options.AllowTrailingCommas = $false
  $options.CommentHandling = [System.Text.Json.JsonCommentHandling]::Disallow
  try {
    $document = [System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText($fullPath), $options)
  }
  catch [System.Text.Json.JsonException] {
    Fail "invalid JSON: $($_.Exception.Message)"
  }
  try {
    $root = $document.RootElement
    Assert-ExactProperties $root @(
      'schema', 'version', 'runtime_semantic_version', 'protocol_version', 'contract_hash',
      'source_repository', 'source_commit', 'release_tag', 'assets') '$'
    if ((Get-AsciiString $root 'schema' '$') -ne 'apmw_projector_lock') { Fail '$.schema must be apmw_projector_lock' }
    if ((Get-Integer $root 'version' '$') -ne 1) { Fail '$.version must be 1' }
    $runtime = Get-AsciiString $root 'runtime_semantic_version' '$'
    if ($runtime -ne '0.1.0') { Fail '$.runtime_semantic_version must equal 0.1.0' }
    $protocol = Get-Integer $root 'protocol_version' '$'
    if ($protocol -ne 1) { Fail "unsupported projector protocol version $protocol" }
    $contract = Get-AsciiString $root 'contract_hash' '$'
    Assert-Sha256 $contract '$.contract_hash'
    if ($contract -ne $script:ContractHash) { Fail '$.contract_hash does not match the supported contract' }
    $repository = Get-AsciiString $root 'source_repository' '$'
    if ($repository -notmatch '^[A-Za-z0-9][A-Za-z0-9_.-]*/[A-Za-z0-9][A-Za-z0-9_.-]*$') {
      Fail '$.source_repository must be an owner/repository identifier'
    }
    $commit = Get-AsciiString $root 'source_commit' '$'
    if ($commit -notmatch '^[0-9a-f]{40}$') { Fail '$.source_commit must be exactly 40 lowercase hexadecimal characters' }
    $tag = Get-AsciiString $root 'release_tag' '$'
    if ($tag -ne "apmw-projector-v$runtime") { Fail '$.release_tag must equal apmw-projector-v plus the runtime version' }

    $assetsElement = Get-RequiredProperty $root 'assets' '$'
    Assert-ExactProperties $assetsElement @('windows-x86', 'windows-x64') '$.assets'
    $assets = @{}
    $zipNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($platform in @('windows-x86', 'windows-x64')) {
      $entryPath = '$.assets.' + $platform
      $entry = Get-RequiredProperty $assetsElement $platform '$.assets'
      Assert-ExactProperties $entry @('filename', 'sha256', 'size', 'executable') $entryPath
      $filename = Get-AsciiString $entry 'filename' $entryPath
      if ($filename -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*\.zip$') { Fail "$entryPath.filename must be a plain .zip file name" }
      if (-not $zipNames.Add($filename)) { Fail "$.assets contains duplicate filename $filename" }
      $sha256 = Get-AsciiString $entry 'sha256' $entryPath
      Assert-Sha256 $sha256 "$entryPath.sha256"
      $size = Get-Int64 $entry 'size' $entryPath
      if ($size -le 0) { Fail "$entryPath.size must be positive" }
      $executable = Get-RequiredProperty $entry 'executable' $entryPath
      Assert-ExactProperties $executable @('relative_path', 'sha256') "$entryPath.executable"
      $relativePath = Get-AsciiString $executable 'relative_path' "$entryPath.executable"
      Assert-RelativeArchivePath $relativePath "$entryPath.executable.relative_path"
      $executableSha256 = Get-AsciiString $executable 'sha256' "$entryPath.executable"
      Assert-Sha256 $executableSha256 "$entryPath.executable.sha256"
      $assets[$platform] = [pscustomobject]@{
        Platform = $platform; Filename = $filename; Sha256 = $sha256; Size = $size
        ExecutableRelativePath = $relativePath; ExecutableSha256 = $executableSha256
      }
    }
    return [pscustomobject]@{
      Runtime = $runtime; Protocol = $protocol; Contract = $contract; Repository = $repository
      Commit = $commit; Tag = $tag; Assets = $assets
    }
  }
  finally {
    $document.Dispose()
  }
}

function Get-Sha256([string]$Path) {
  return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Assert-Provenance(
  [string]$Archive,
  [string]$Repository,
  [string]$ReleaseTag,
  [string]$SourceCommit
) {
  $gh = Get-Command gh -ErrorAction SilentlyContinue
  if (-not $gh) {
    throw 'GitHub CLI with attestation support is required to verify projector provenance'
  }

  $attestationHelp = (& $gh.Source attestation verify --help 2>&1 | Out-String)
  if (
    $LASTEXITCODE -ne 0 -or
    -not $attestationHelp.Contains('--signer-workflow') -or
    -not $attestationHelp.Contains('--signer-digest') -or
    -not $attestationHelp.Contains('--source-digest') -or
    -not $attestationHelp.Contains('--source-ref') -or
    -not $attestationHelp.Contains('--deny-self-hosted-runners')
  ) {
    throw 'The installed GitHub CLI does not support the required projector attestation checks'
  }

  $signerWorkflow = "$Repository/.github/workflows/apmw-projector-release.yml"
  & $gh.Source attestation verify $Archive `
    --repo $Repository `
    --signer-workflow $signerWorkflow `
    --signer-digest $SourceCommit `
    --source-digest $SourceCommit `
    --source-ref "refs/tags/$ReleaseTag" `
    --deny-self-hosted-runners
  if ($LASTEXITCODE -ne 0) {
    throw 'GitHub build-provenance verification failed for the projector archive'
  }
}

function Assert-ZipEntryPath([string]$Path) {
  Assert-RelativeArchivePath $Path "zip entry $Path"
}

function Test-ZipEntryLink([System.IO.Compression.ZipArchiveEntry]$Entry) {
  $unixMode = ($Entry.ExternalAttributes -shr 16) -band 0xf000
  $isUnixSymlink = $unixMode -eq 0xa000
  $isWindowsReparsePoint = ($Entry.ExternalAttributes -band 0x400) -ne 0
  if ($isUnixSymlink -or $isWindowsReparsePoint) {
    throw "Refusing symlink or reparse-point archive entry: $($Entry.FullName)"
  }
}

function Expand-VerifiedArchive([string]$Archive, [string]$ExtractionRoot) {
  Add-Type -AssemblyName System.IO.Compression
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $root = [IO.Path]::GetFullPath($ExtractionRoot)
  [IO.Directory]::CreateDirectory($root) | Out-Null
  $rootPrefix = $root.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
  $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
  $stream = [IO.File]::OpenRead($Archive)
  try {
    $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read, $false)
    try {
      foreach ($entry in $zip.Entries) {
        if ([string]::IsNullOrEmpty($entry.FullName)) { throw 'Refusing unnamed zip entry' }
        if ($entry.FullName.Contains('\')) { throw "Refusing backslash zip entry: $($entry.FullName)" }
        $isDirectory = $entry.FullName.EndsWith('/', [StringComparison]::Ordinal)
        $entryName = if ($isDirectory) { $entry.FullName.TrimEnd('/') } else { $entry.FullName }
        if ([string]::IsNullOrEmpty($entryName)) { continue }
        Assert-ZipEntryPath $entryName
        Test-ZipEntryLink $entry
        if (-not $seen.Add($entryName)) { throw "Refusing duplicate zip entry: $entryName" }
        $relativeWindowsPath = $entryName.Replace('/', [IO.Path]::DirectorySeparatorChar)
        $destination = [IO.Path]::GetFullPath([IO.Path]::Combine($root, $relativeWindowsPath))
        if (-not $destination.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
          throw "Refusing zip entry outside extraction root: $entryName"
        }
        if ($isDirectory) {
          [IO.Directory]::CreateDirectory($destination) | Out-Null
          continue
        }
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
        $input = $entry.Open()
        try {
          $output = [IO.File]::Create($destination)
          try { $input.CopyTo($output) }
          finally { $output.Dispose() }
        }
        finally { $input.Dispose() }
      }
    }
    finally { $zip.Dispose() }
  }
  finally { $stream.Dispose() }
}

function Assert-BuildManifest([string]$ExtractionRoot, $Lock, $Asset) {
  $manifestPath = Join-Path $ExtractionRoot $script:BuildManifestName
  if (-not [IO.File]::Exists($manifestPath)) { throw "Archive does not contain $script:BuildManifestName" }
  $options = [System.Text.Json.JsonDocumentOptions]::new()
  $options.AllowTrailingCommas = $false
  $options.CommentHandling = [System.Text.Json.JsonCommentHandling]::Disallow
  $document = [System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText($manifestPath), $options)
  try {
    $root = $document.RootElement
    Assert-ExactProperties $root @(
      'schema', 'version', 'runtime_semantic_version', 'protocol_version', 'contract_hash',
      'target_platform', 'target_architecture', 'executable_relative_path',
      'executable_sha256') '$.build_manifest'
    if ((Get-AsciiString $root 'schema' '$.build_manifest') -ne 'apmw_projector_build_manifest') { throw 'Invalid build manifest schema' }
    if ((Get-Integer $root 'version' '$.build_manifest') -ne 1) { throw 'Invalid build manifest version' }
    $expected = @{
      runtime_semantic_version = $Lock.Runtime; protocol_version = $Lock.Protocol
      contract_hash = $Lock.Contract; target_platform = 'windows'
      target_architecture = $Asset.Platform.Substring('windows-'.Length)
      executable_relative_path = $Asset.ExecutableRelativePath
      executable_sha256 = $Asset.ExecutableSha256
    }
    foreach ($name in $expected.Keys) {
      $actual = if ($name -eq 'protocol_version') {
        Get-Integer $root $name '$.build_manifest'
      } else {
        Get-AsciiString $root $name '$.build_manifest'
      }
      if ($actual -ne $expected[$name]) { throw "Build manifest $name does not match the lock" }
    }
    $executable = Join-Path $ExtractionRoot ($Asset.ExecutableRelativePath.Replace('/', [IO.Path]::DirectorySeparatorChar))
    $fullRoot = [IO.Path]::GetFullPath($ExtractionRoot).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $fullExecutable = [IO.Path]::GetFullPath($executable)
    if (-not $fullExecutable.StartsWith($fullRoot, [StringComparison]::OrdinalIgnoreCase) -or
        -not [IO.File]::Exists($fullExecutable)) {
      throw 'Embedded executable is absent or outside the extraction root'
    }
    if ((Get-Sha256 $fullExecutable) -ne $Asset.ExecutableSha256) {
      throw 'Embedded executable SHA-256 does not match the lock'
    }
  }
  finally { $document.Dispose() }
}

function Remove-GeneratedPath([string]$Path) {
  if ($Path -and (Test-Path -LiteralPath $Path)) {
    $root = Get-Item -LiteralPath $Path -Force
    if (($root.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
      throw "Refusing to recursively remove generated reparse point: $Path"
    }
    if ($root.PSIsContainer) {
      $reparsePoint = Get-ChildItem -LiteralPath $Path -Force -Recurse |
        Where-Object { ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 } |
        Select-Object -First 1
      if ($reparsePoint) {
        throw "Refusing to recursively remove generated tree containing reparse point: $($reparsePoint.FullName)"
      }
    }
    Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
  }
}

if (-not $Architecture) {
  $hostArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
  if ($hostArchitecture -eq 'X64') { $Architecture = 'x64' }
  elseif ($hostArchitecture -eq 'X86') { $Architecture = 'x86' }
  else { throw 'Specify -Architecture x86 or x64 on a non-x86 Windows host' }
}

$lock = Read-ApmwProjectorLock $LockPath
$asset = $lock.Assets["windows-$Architecture"]
$destination = [IO.Path]::GetFullPath($DestinationRoot)
[IO.Directory]::CreateDirectory($destination) | Out-Null
$restoreLockPath = Join-Path $destination (".restore-windows-$Architecture.lock")
try {
  $restoreLock = [IO.File]::Open(
    $restoreLockPath,
    [IO.FileMode]::OpenOrCreate,
    [IO.FileAccess]::ReadWrite,
    [IO.FileShare]::None)
}
catch [IO.IOException] {
  throw "Another APMW projector restore is already running for $Architecture"
}

$nonce = [Guid]::NewGuid().ToString('N')
$temporaryZip = Join-Path $destination (".$nonce.download")
$extraction = Join-Path $destination (".$nonce.extract")
$target = Join-Path $destination ("windows-$Architecture")
$backup = Join-Path $destination (".$nonce.previous")
$movedPrevious = $false

try {
  if ($ValidateArchive) {
    Copy-Item -LiteralPath ([IO.Path]::GetFullPath($ArchivePath)) -Destination $temporaryZip -ErrorAction Stop
  }
  else {
    $uri = "https://github.com/$($lock.Repository)/releases/download/$($lock.Tag)/$($asset.Filename)"
    Invoke-WebRequest -Uri $uri -OutFile $temporaryZip -MaximumRedirection 5 -TimeoutSec 300
  }
  if ((Get-Item -LiteralPath $temporaryZip).Length -ne $asset.Size) {
    throw 'Downloaded archive size does not match the lock'
  }
  if ((Get-Sha256 $temporaryZip) -ne $asset.Sha256) {
    throw 'Downloaded archive SHA-256 does not match the lock'
  }
  if (-not $ValidateArchive) {
    Assert-Provenance $temporaryZip $lock.Repository $lock.Tag $lock.Commit
  }
  Expand-VerifiedArchive $temporaryZip $extraction
  Assert-BuildManifest $extraction $lock $asset

  if ($ValidateArchive) {
    Write-Output 'Local projector archive validation succeeded.'
  }
  else {
    if (Test-Path -LiteralPath $target) {
      [IO.Directory]::Move($target, $backup)
      $movedPrevious = $true
    }
    [IO.Directory]::Move($extraction, $target)
    $extraction = $null
    if ($movedPrevious) {
      Remove-GeneratedPath $backup
      $backup = $null
    }
    Write-Output "Restored verified APMW projector to $target"
  }
}
catch {
  $failure = $_
  if ($movedPrevious -and -not (Test-Path -LiteralPath $target) -and (Test-Path -LiteralPath $backup)) {
    [IO.Directory]::Move($backup, $target)
    $backup = $null
  }
  Remove-GeneratedPath $temporaryZip
  Remove-GeneratedPath $extraction
  Remove-GeneratedPath $backup
  throw $failure
}
finally {
  try {
    Remove-GeneratedPath $temporaryZip
    Remove-GeneratedPath $extraction
  }
  finally {
    $restoreLock.Dispose()
  }
}
