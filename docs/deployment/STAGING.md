# Staging / Test Environment (M3)

CivicConnect uses a **simulated staging environment** on one machine. Real cloud staging is optional in M3
(M3 brief §14). This one is kept separate from development by configuration, build and data.

| | Development | Staging | Production (intended) |
|---|---|---|---|
| How it runs | `dotnet run` (Debug) | Published Release build per commit (`deploy/staging/releases/<commit>`) | Published Release build |
| `ASPNETCORE_ENVIRONMENT` | Development | Staging | Production |
| URL | http://localhost:5080 | http://localhost:5090 | HTTPS only |
| Database | `src/CivicConnect.Api/data/civicconnect.dev.db` | `deploy/staging/data/civicconnect.staging.db` (shared by all staging releases) | PostgreSQL target (ADR-07; not built) |
| Demo accounts | Seeded | Seeded | Not seeded (`Seed:Enabled=false`) |
| Demo password | `dotnet user-secrets` | `CIVICCONNECT_STAGING_PASSWORD` environment variable | n/a |
| HTTPS / HSTS / secure cookie | Off | **Off** (TD-07) | On (`Security:RequireHttps=true`, the default) |
| Login rate limit | 10/min | 30/min (allows E2E + load-test logins) | 10/min |

No secrets are stored in the repository. All `*.db` files and the staging `releases/`, `data/` and `backups/` folders are git-ignored.

## Deploy, verify, roll back (live demo script)

```powershell
$env:CIVICCONNECT_STAGING_PASSWORD = '<choose one, 8+ characters>'
./deploy/staging/deploy.ps1   # refuses uncommitted changes, runs all tests, publishes, backs up the DB
./deploy/staging/start.ps1    # serves http://localhost:5090 (leave running)
```

In a second terminal:

```powershell
Invoke-RestMethod http://localhost:5090/health   # version must equal: git rev-parse HEAD
$env:CIVICCONNECT_E2E_BASEURL = 'http://localhost:5090'
$env:CIVICCONNECT_E2E_PASSWORD = $env:CIVICCONNECT_STAGING_PASSWORD
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
2. Plain HTTP on localhost. TLS, the secure-cookie flag and HSTS are not exercised (TD-07).
3. One machine: no network latency, no load balancer, no separate database server.
4. Seeded demo accounts with one shared password (TD-06). Production needs real account management.
5. Console logging only. No central log collection, metrics or alerting. `/health` is the only health signal.
