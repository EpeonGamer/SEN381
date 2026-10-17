# SEN381 CivicConnect Project

The CivicConnect project is developed in order to assist civilians and streamline an old management system. The basic functionality of the project is to connect users, management and service workers in to one system and allow for a reliable way to submit, monitor and report service requests.

## Requirements

- .NET SDK 8 or later (the projects target `net8.0`; the .NET 8 runtime must be installed)
- No database server: SQLite files are created automatically

## Setup and run (Development)

```powershell
# One-time: choose a demo password (8+ characters). It is stored in your user profile, never in the repo.
dotnet user-secrets set "Seed:DemoPassword" "<your password>" --project src/CivicConnect.Api

dotnet run --project src/CivicConnect.Api --launch-profile CivicConnect.Api
```

Open http://localhost:5080 and log in as `requester-1`, `requester-2`, `staff-roads`, `staff-roads-2`,
`staff-utilities` or `manager-1` with that password.

## Test

```powershell
dotnet test CivicConnect.sln
```

This runs unit, black-box, SQLite integration, API integration and E2E tests. The same checks run in GitHub Actions
(`.github/workflows/ci.yml`). Staging, the load test and rollback are described in `docs/deployment/STAGING.md`.

## Project structure

| Folder | Contents |
|---|---|
| `src/CivicConnect.Core` | Business rules: request lifecycle (ADR-04) and authorization (ADR-05) |
| `src/CivicConnect.Data` | SQLite persistence (ADR-07) |
| `src/CivicConnect.Api` | ASP.NET Core API, authentication (ADR-08) and the web UI (`wwwroot`) |
| `tests/` | Automated tests (see `docs/quality/TEST_EVIDENCE.md`) |
| `deploy/staging`, `tools/` | Simulated staging scripts and the load-test tool |
| `docs/` | PED, decisions, change requests, quality, risk and deployment records |

## Project Engineering Document
Can be found here: docs\PED\PED.md

## Contributors
Aidan Lakmeeharan\
Kasper Van Niekerk\
Kutloano Lethebe Maine
