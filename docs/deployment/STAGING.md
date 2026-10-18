# Staging / Test Environment (M3)

CivicConnect uses a **simulated staging environment** on one machine. Real cloud staging is optional in M3
(M3 brief §14). This one is kept separate from development by configuration, build and data.

| | Development | Staging | Production (intended) |
|---|---|---|---|
| How it runs | `dotnet run` (Debug) | Published Release build per commit (`deploy/staging/releases/<commit>`) | Published Release build |
| `ASPNETCORE_ENVIRONMENT` | Development | Staging | Production |
| URL | http://localhost:5080 | http://localhost:5090, or https://localhost:5091 with `-Https` | HTTPS only |
| Database | `src/CivicConnect.Api/data/civicconnect.dev.db` | `deploy/staging/data/civicconnect.staging.db` (shared by all staging releases) | PostgreSQL target (ADR-07; not built) |
| Demo accounts | Seeded | Seeded | Not seeded (`Seed:Enabled=false`) |
| Demo passwords | One per account, generated into `src/CivicConnect.Api/data/seed-passwords.json` | One per account, generated into `deploy/staging/data/seed-passwords.json` | n/a |
| HTTPS / HSTS / secure cookie | Off | Off by default; **on with `start.ps1 -Https`** (development certificate) | On (`Security:RequireHttps=true`, the default) |
| Login rate limit | 10/min | 30/min (allows E2E + load-test logins) | 10/min |

Each demo account has its own random password (TD-06). The app creates `seed-passwords.json` on first start and adds a password for any new demo account; only PBKDF2 hashes go into the database. Open the file to see the passwords for a demo. No secrets are stored in the repository. All `*.db` files and the staging `releases/`, `data/` and `backups/` folders are git-ignored.

## Deploy, verify, roll back (live demo script)

```powershell
./deploy/staging/deploy.ps1   # refuses uncommitted changes, runs all tests, publishes, backs up the DB
./deploy/staging/start.ps1    # serves http://localhost:5090 (leave running)
```

**HTTPS option (NFR-002, TD-07):** `./deploy/staging/start.ps1 -Https` serves https://localhost:5091 with
`Security:RequireHttps=true`, as in Production: HTTP on 5090 redirects (307), the login cookie gets the `Secure` flag and
HSTS is enabled. It uses the .NET development certificate, which each machine must trust once with
`dotnet dev-certs https --trust` (the script stops with that hint if it is missing). Use `https://localhost:5091` in
the commands below. The default run stays plain HTTP so a machine without the certificate can still run staging.

In a second terminal:

```powershell
Invoke-RestMethod http://localhost:5090/health   # version must equal: git rev-parse HEAD
$env:CIVICCONNECT_E2E_BASEURL = 'http://localhost:5090'
$env:CIVICCONNECT_E2E_PASSWORDS_FILE = (Resolve-Path ./deploy/staging/data/seed-passwords.json)
dotnet test tests/CivicConnect.Api.Tests --filter "FullyQualifiedName~EndToEndTests"   # critical journeys vs staging
```

**Proving the right candidate is deployed:** `/health` returns `version` = `0.3.0+<full commit hash>`, stamped
into the build by the .NET SDK. Example from 2026-10-16: `0.3.0+c5778f9d5ec95a93b926a1b6fd22a3f4e0164d01`, environment `Staging`.

**Rollback:** stop staging, then `./deploy/staging/rollback.ps1 -Commit <previous>` and start it again.
- *Artefact:* each commit's publish folder is kept, so rollback switches back to the earlier build.
- *Data:* `deploy.ps1` copies the database to `backups/` before each new release. Add `-RestoreBackup <file>` only
  when the new release damaged data. Restoring discards anything written since the backup.
- *Configuration:* environment variables are set by `start.ps1` and are not versioned with the build. Check them
  if a rollback crosses a configuration change.

## Differences that could affect production behaviour
1. SQLite file vs. the PostgreSQL target: locking, types and performance differ (ADR-07, DEF-09).
2. HTTPS is exercised only with `-Https` and a development certificate trusted on this machine; production needs a
   certificate from a real certificate authority. ASP.NET Core does not send the HSTS header to `localhost`, so HSTS
   itself cannot be observed here (TD-07).
3. One machine: no network latency, no load balancer, no separate database server.
4. Seeded demo accounts with generated per-account passwords in a local file (TD-06). Production needs real account management (password change/reset, lockout, MFA).
5. Console logging only. No central log collection, metrics or alerting. `/health` is the only health signal.
