#requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$testRoot = Join-Path $PSScriptRoot 'bin\ProjectorRestoreValidation'
$inputRoot = Join-Path $testRoot 'input'
$archive = Join-Path $testRoot 'projector.zip'
$maliciousArchive = Join-Path $testRoot 'zip-slip.zip'
$lockPath = Join-Path $testRoot 'synthetic-lock.json'
$restore = Join-Path $PSScriptRoot '..\tools\Restore-ApmwProjector.ps1'

if (Test-Path -LiteralPath $testRoot) {
  Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction Stop
}
try {
  New-Item -ItemType Directory -Path $inputRoot -Force | Out-Null
  [IO.File]::WriteAllBytes((Join-Path $inputRoot 'ApmwProjector.exe'), [byte[]](1, 2, 3, 4))
  $executableHash = (Get-FileHash -LiteralPath (Join-Path $inputRoot 'ApmwProjector.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
  @"
{
  "schema": "apmw_projector_build_manifest",
  "version": 1,
  "runtime_semantic_version": "0.1.0",
  "protocol_version": 1,
  "contract_hash": "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15",
  "target_platform": "windows",
  "target_architecture": "x64",
  "executable_relative_path": "ApmwProjector.exe",
  "executable_sha256": "$executableHash"
}
"@ | Set-Content -LiteralPath (Join-Path $inputRoot 'apmw-projector-manifest.json') -NoNewline
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  [IO.Compression.ZipFile]::CreateFromDirectory(
    $inputRoot, $archive, [IO.Compression.CompressionLevel]::NoCompression, $false)

  $lock = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Fixtures\ProjectorLock\synthetic-lock.json') -Raw |
    ConvertFrom-Json
  $asset = $lock.assets.'windows-x64'
  $asset.size = (Get-Item -LiteralPath $archive).Length
  $asset.sha256 = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
  $asset.executable.sha256 = $executableHash
  $lock | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $lockPath -NoNewline

  & $restore -ValidateArchive -LockPath $lockPath -Architecture x64 -DestinationRoot $testRoot -ArchivePath $archive
  if (Test-Path -LiteralPath (Join-Path $testRoot 'windows-x64')) {
    throw 'Validation mode must not create a cache directory.'
  }

  $restoreLockPath = Join-Path $testRoot '.restore-windows-x64.lock'
  $heldRestoreLock = [IO.File]::Open(
    $restoreLockPath,
    [IO.FileMode]::OpenOrCreate,
    [IO.FileAccess]::ReadWrite,
    [IO.FileShare]::None)
  try {
    $concurrentRestoreRejected = $false
    try {
      & $restore -ValidateArchive -LockPath $lockPath -Architecture x64 -DestinationRoot $testRoot -ArchivePath $archive
    }
    catch {
      $concurrentRestoreRejected = $_.Exception.Message.Contains('already running')
    }
    if (-not $concurrentRestoreRejected) {
      throw 'Restore validation did not reject a concurrent restore for the same architecture.'
    }
  }
  finally {
    $heldRestoreLock.Dispose()
  }

  $stream = [IO.File]::Create($maliciousArchive)
  try {
    $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $false)
    try {
      $writer = [IO.StreamWriter]::new($zip.CreateEntry('../outside.txt').Open())
      try { $writer.Write('must not extract') }
      finally { $writer.Dispose() }
    }
    finally { $zip.Dispose() }
  }
  finally { $stream.Dispose() }
  $asset.size = (Get-Item -LiteralPath $maliciousArchive).Length
  $asset.sha256 = (Get-FileHash -LiteralPath $maliciousArchive -Algorithm SHA256).Hash.ToLowerInvariant()
  $lock | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $lockPath -NoNewline
  $rejected = $false
  try {
    & $restore -ValidateArchive -LockPath $lockPath -Architecture x64 -DestinationRoot $testRoot -ArchivePath $maliciousArchive
  }
  catch {
    $rejected = $true
  }
  if (-not $rejected) {
    throw 'Restore validation accepted a zip-slip archive.'
  }
  if (Test-Path -LiteralPath (Join-Path $testRoot 'outside.txt')) {
    throw 'Zip-slip archive wrote outside its extraction root.'
  }
}
finally {
  if (Test-Path -LiteralPath $testRoot) {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction Stop
  }
}
