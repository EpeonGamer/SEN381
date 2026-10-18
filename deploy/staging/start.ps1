<#
.SYNOPSIS
  Runs the current staging release on http://localhost:5090, or over HTTPS with -Https.

.DESCRIPTION
  Staging-specific configuration is supplied here, not in source control:
  - ASPNETCORE_ENVIRONMENT=Staging    -> loads appsettings.Staging.json
  - ConnectionStrings__CivicConnect   -> one staging database shared by every release (survives redeploys)
  - Seed__PasswordsFile               -> data/seed-passwords.json: one generated password per demo account
                                         (a git-ignored local secret; created on first start, kept across releases)
  - Security__RequireHttps            -> false by default; true with -Https (NFR-002, TD-07)

  -Https serves https://localhost:5091 with the .NET development certificate and redirects HTTP to it, with HSTS
  and the Secure cookie flag on, as in Production. It needs a trusted development certificate on this machine,
  created once with:  dotnet dev-certs https --trust

.EXAMPLE
  ./start.ps1
  ./start.ps1 -Https
#>
param(
    [int]$Port = 5090,
    [switch]$Https,
    [int]$HttpsPort = 5091
)
$ErrorActionPreference = 'Stop'
$staging = Resolve-Path $PSScriptRoot

$commit = (Get-Content "$staging/current.txt" -ErrorAction Stop).Trim()
$release = "$staging/releases/$commit"
New-Item -ItemType Directory -Force "$staging/data" | Out-Null

$env:ASPNETCORE_ENVIRONMENT = 'Staging'
$env:ConnectionStrings__CivicConnect = "Data Source=$staging/data/civicconnect.staging.db"
$env:Seed__PasswordsFile = "$staging/data/seed-passwords.json"

if ($Https) {
    dotnet dev-certs https --check --trust | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'No trusted development certificate. Run "dotnet dev-certs https --trust" once, then try again.'
    }
    $env:ASPNETCORE_URLS = "https://localhost:$HttpsPort;http://localhost:$Port"
    $env:Security__RequireHttps = 'true'
    $url = "https://localhost:$HttpsPort (http://localhost:$Port redirects)"
} else {
    # Set explicitly so an earlier -Https run in the same terminal cannot leave HTTPS-only cookies behind.
    $env:ASPNETCORE_URLS = "http://localhost:$Port"
    $env:Security__RequireHttps = 'false'
    $url = "http://localhost:$Port"
}

Write-Host "Starting staging release $commit on $url (Ctrl+C to stop)"
dotnet "$release/CivicConnect.Api.dll" --contentRoot $release
