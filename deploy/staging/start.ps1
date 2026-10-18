<#
.SYNOPSIS
  Runs the current staging release on http://localhost:5090.

.DESCRIPTION
  Staging-specific configuration is supplied here, not in source control:
  - ASPNETCORE_ENVIRONMENT=Staging    -> loads appsettings.Staging.json
  - ConnectionStrings__CivicConnect   -> one staging database shared by every release (survives redeploys)
  - Seed__PasswordsFile               -> data/seed-passwords.json: one generated password per demo account
                                         (a git-ignored local secret; created on first start, kept across releases)
#>
param([int]$Port = 5090)
$ErrorActionPreference = 'Stop'
$staging = Resolve-Path $PSScriptRoot

$commit = (Get-Content "$staging/current.txt" -ErrorAction Stop).Trim()
$release = "$staging/releases/$commit"
New-Item -ItemType Directory -Force "$staging/data" | Out-Null

$env:ASPNETCORE_ENVIRONMENT = 'Staging'
$env:ASPNETCORE_URLS = "http://localhost:$Port"
$env:ConnectionStrings__CivicConnect = "Data Source=$staging/data/civicconnect.staging.db"
$env:Seed__PasswordsFile = "$staging/data/seed-passwords.json"

Write-Host "Starting staging release $commit on http://localhost:$Port (Ctrl+C to stop)"
dotnet "$release/CivicConnect.Api.dll" --contentRoot $release
