# ADR-07: SQLite Persistence Adapter for the M3 Release Candidate

**Status:** Proposed (needs two reviewer approvals before it is Accepted)

**Date:** 2026-10-15  **Author:** Kasper, Member 3

**Revises:** DEC-M2-01 / DEC-M2-06 (PostgreSQL via Npgsql), ADR-06

## 1. Original decision (M2 baseline)

Use a relational database, PostgreSQL via Npgsql, with a single primary node (DEC-M2-01, DEC-M2-04, ADR-06). The persistence boundary is `IRequestRepository` (ADR-04), so the lifecycle and authorization rules do not depend on the database.

## 2. Implementation evidence / problem

At the start of M3 verification the repository contained the Core rules and the `IRequestRepository` interface but **no concrete repository, no schema and no database project**. The test project referenced `src/CivicConnect.Data/CivicConnect.Data.csproj`, which had never been committed, so a fresh clone could not build. Verification of persistence-dependent requirements (FR-002, FR-005, FR-008, NFR-004, NFR-007) had no real database to run against. Standing up and hosting a PostgreSQL server for build, CI and staging is not achievable within the M3 timeframe for a three-person team.

## 3. Revised decision

Implement `IRequestRepository` with **SQLite** (`Microsoft.Data.Sqlite`) in a new project `src/CivicConnect.Data`. The schema keeps the M2 data model intent: current state plus history (DEC-M2-02), a version column for optimistic concurrency (DEC-M2-03), foreign keys for ownership and category (ADR-05), and one transaction for each status change plus its audit entry (NFR-004). Visibility filtering is applied in SQL before rows are returned (NFR-007).

PostgreSQL remains the intended production target. Core code is unchanged and still depends only on `IRequestRepository`.

## 4. Consequences

**Gains**
- Real-database integration tests run anywhere with no server or Docker, including CI.
- NFR-004 (all-or-nothing save) is genuinely testable using a real transaction and a forced audit-write failure.
- Staging can be a separate database file with environment-specific configuration.

**Costs and risks (carried into the Risk Register / Release Evidence Summary)**
- SQLite allows one writer at a time. Concurrency behaviour and performance measured on SQLite do **not** predict PostgreSQL behaviour (RISK-M2-02, NFR-001).
- Type and constraint semantics differ (e.g. timestamps stored as ISO-8601 text; case-insensitive category matching via NOCASE collation). A PostgreSQL adapter will need its own integration test run.
- NFR-003 (availability) and backup/restore planning (RISK-M2-01) are not addressed by this adapter.
- No user-management or password-hash storage is implemented (NFR-002 remains unverified).

**Reversal path:** write `PostgresRequestRepository` against the same interface and re-run the integration tests in `SqliteRepositoryIntegrationTests` against it.

## 5. Traceability

FR-002, FR-005, FR-006, FR-008, NFR-004, NFR-007; ADR-04, ADR-05, ADR-06; DEC-M2-01..03; RISK-M2-02, RISK-M2-03.
