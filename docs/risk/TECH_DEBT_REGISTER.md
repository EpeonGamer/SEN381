# Technical Debt Register

> Deferred compromises and known maintainability concerns. TD-01 to TD-04 were recorded by Member One in
> the M3 review (PED §1.16); TD-05 onward were added during M3 construction.

| ID | Deferred compromise | Consequence | Mitigation / trigger to repay |
|---|---|---|---|
| TD-01 | Manager UI for Staff-account and Staff-group administration is deferred (ADR-05). Accounts and groups are seeded by `DatabaseStartup`. | Permission changes need a controlled alternative process (developer change + redeploy). | Document the process; build administration when group maintenance enters scope. |
| TD-02 | Request types are plain strings in Staff-group permissions. | Naming inconsistencies could cause wrong access decisions. | Partly mitigated in M3: SQLite foreign keys force group categories to exist in `categories`, and the API stores the catalogue spelling (DEF-08). Move to controlled ids if categories become editable. |
| TD-03 | Manager policy grants visibility of all requests. | Department-specific reporting restrictions would need policy changes. | Revisit when a narrower Manager scope is agreed. |
| TD-04 | Small table-driven lifecycle may become insufficient if state-specific behaviour grows. | More operations could make guards and transition logic harder to maintain. | Follow ADR-04's trigger: reconsider when several operations depend on state. |
| TD-05 | SQLite instead of the M2 PostgreSQL target (ADR-07). | Single writer (DEF-09), no server-side availability features; performance results do not predict PostgreSQL. | `PostgresRequestRepository` behind the same `IRequestRepository`; re-run the integration, API and load tests against it. |
| TD-06 | One shared demo password for all seeded accounts; no password change, lockout or MFA (ADR-08). | Acceptable for staging/demo only; not for production. | Per-user credentials with change/reset, account lockout, and MFA for Staff/Manager (RISK-001) before production. |
| TD-07 | Simulated staging serves plain HTTP on localhost (`Security:RequireHttps=false`). | NFR-002 HTTPS is configured for Production but not exercised in any environment. | Run staging behind TLS (reverse proxy or Kestrel certificate) and add an HTTPS check to the deploy verification. **M4: partly repaid.** `start.ps1 -Https` runs staging over HTTPS with the development certificate; redirect, Secure cookie and E2E verified (TEST_EVIDENCE, H1). Remaining: a real CA certificate for production; HSTS not observable on localhost. |
| TD-08 | Schema is created with `CREATE TABLE IF NOT EXISTS`; no migration mechanism beyond `PRAGMA user_version`. | Future schema changes need manual scripts and careful backups. | Introduce versioned migration scripts before the first breaking schema change. |
| TD-09 | The browser UI is not covered by automated tests (E2E tests drive the HTTP API the UI uses). | UI-only regressions (e.g. a broken button) are caught only by manual demo. | Add a Playwright smoke test of login → submit → list once the UI stabilises. |
