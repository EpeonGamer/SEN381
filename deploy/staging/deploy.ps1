<#
.SYNOPSIS
  Builds, verifies and publishes the current commit as a staging release (simulated staging, M3).

.DESCRIPTION
  1. Refuses to deploy uncommitted changes, so the release always matches a Git commit.
  2. Runs the full automated test suite (release gate).
  3. Publishes a Release build to deploy/staging/releases/<commit>.
  4. Backs up the staging database before the new release touches it.
  5. Marks the release as current. Start it with start.ps1.
#>
$ErrorActionPreference = 'Stop'
$root = Resolve-Path "$PSScriptRoot/../.."
$staging = "$root/deploy/staging"

# 1. Release must correspond to a commit.
if (git -C $root status --porcelain --untracked-files=no) {
    throw 'Working tree has uncommitted changes. Commit them first so the release matches a commit.'
}
$commit = (git -C $root rev-parse --short HEAD).Trim()

# 2. Release gate: every automated test must pass.
dotnet test "$root/CivicConnect.sln" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed; release blocked.' }

# 3. Publish this commit as its own release folder (old releases are kept for rollback).
$release = "$staging/releases/$commit"
dotnet publish "$root/src/CivicConnect.Api" -c Release -o $release
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

# 4. Back up the staging database before the new release runs against it.
$database = "$staging/data/civicconnect.staging.db"
if (Test-Path $database) {
    New-Item -ItemType Directory -Force "$staging/backups" | Out-Null
    $backup = "$staging/backups/civicconnect.staging.$(Get-Date -Format yyyyMMdd-HHmmss).before-$commit.db"
    Copy-Item $database $backup
    Write-Host "Database backed up to $backup"
}

# 5. Mark as current.
Set-Content "$staging/current.txt" $commit
Write-Host "Release $commit is ready. Start it with deploy/staging/start.ps1"
