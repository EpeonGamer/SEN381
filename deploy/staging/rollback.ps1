<#
.SYNOPSIS
  Rolls staging back to an earlier release, optionally restoring a database backup.

.EXAMPLE
  ./rollback.ps1 -Commit 4b29631
  ./rollback.ps1 -Commit 4b29631 -RestoreBackup backups/civicconnect.staging.20261016-150000.before-b4a2939.db

.NOTES
  Stop the running staging process first. Restoring a backup discards data written after the backup was
  taken, so only restore it when the newer release damaged the data, not just for a code problem.
#>
param(
    [Parameter(Mandatory)][string]$Commit,
    [string]$RestoreBackup
)
$ErrorActionPreference = 'Stop'
$staging = Resolve-Path $PSScriptRoot

if (-not (Test-Path "$staging/releases/$Commit")) {
    throw "Release $Commit not found. Available: $((Get-ChildItem "$staging/releases").Name -join ', ')"
}

if ($RestoreBackup) {
    $database = "$staging/data/civicconnect.staging.db"
    Copy-Item $database "$database.replaced-$(Get-Date -Format yyyyMMdd-HHmmss)" -ErrorAction SilentlyContinue
    Copy-Item (Join-Path $staging $RestoreBackup) $database -Force
    Write-Host "Database restored from $RestoreBackup (previous file kept alongside it)."
}

Set-Content "$staging/current.txt" $Commit
Write-Host "Staging now points at release $Commit. Start it with start.ps1 and check /health."
