# Release Evidence Summary — M3 Release Candidate

**Candidate:** CivicConnect `0.3.0`, Git tag **`m3-rc2`** on `main` (supersedes `m3-rc1`). The tagged commit hash is given in the formal submission and reported by `/health`.

**Classification: CONDITIONALLY READY.** The critical behaviour is integrated and verified, including on staging. The release
met all three conditions below (2026-10-16). It stays *conditionally* ready because of the accepted, non-blocking limitations recorded here. It is **not** production-ready.

| Evidence area | Result | Interpretation | Concern / action |
|---|---|---|---|
| Build & CI | Local: strict build 0 warnings. GitHub CI: `build-and-test` and `secrets-scan` **passed on push and on the PR** | The gates are build + analyzers, tests, the vulnerability scan and Gitleaks. A green run shows these checks pass. It does not show requirements are complete or the system is secure | **Condition 1 — met:** both checks are required on `main` (branch protection) |
| Automated regression | 88/88 pass (65 Core, 23 API), including CR-M3-02 overdue tests and the DEF-10 regression | Lifecycle, authorization, persistence atomicity, privacy and overdue rules are protected. Mutation checks and a red-then-green DEF-10 test show the tests detect the defects they target | Comments and reassignment: success path covered by `OverdueTests`; no dedicated negative tests (coverage gap) |
| API/integration | 9 API + 5 SQLite tests pass | Authentication, role and privacy rules hold over real HTTP and a real database | — |
| System/E2E | E2E1 and E2E2 pass in-process and **on staging `c5778f9`** | The critical requester → staff → requester journey completes on the deployed build | The browser UI is not automated, only demonstrated manually (TD-09) |
| Static/security | Analyzer findings fixed or suppressed with reasons; 3 High dependency advisories fixed (DEF-05); DEF-01 authorization gap fixed | No known open High security finding | No TLS in staging (TD-07); shared demo password, no MFA (TD-06) |
| Performance | 10 users × 60 s: 0 errors, submit p95 311 ms after the WAL fix (818 ms before) | Stable under small load; writes queue on SQLite | Residual risk RISK-M3-02. No formal NFR-001 target |
| Known defects | Open: DEF-03 (Medium, documentation). DEF-07 closed under CR-M3-02 (accepted); DEF-09 mitigated; DEF-10 closed | None blocks the critical journey | **Condition 2 — met:** CR-M3-02 accepted; it names the proposer (Lethebe) and records the 72-hour rationale and sources |
| Acceptance | FR-001/002/003/004/005/007/008/009/011/013/015 and NFR-004/005/007 demonstrated by tests (PED §1.5.4). FR-013 overdue also checked by a 19-step exploratory run on an isolated instance (2026-10-16) | Core requester, staff and manager scope works | FR-006 search, FR-010 comments, FR-012/014 (partly), NFR-001/002/003/006/008 not fully verified |
| Governance | PR #14 merged with two approvals (Aidan, Lethebe); rc2 documentation update merged by admin bypass, reason recorded in its PR | Brief requires a PR with 2 approvals from non-authors | **Condition 3 — met:** PR #14 approved; ADR-07 and ADR-08 accepted |

## Residual risks
| Risk | Why it remains | Current mitigation | Next action / owner |
|---|---|---|---|
| RISK-M3-01 Account compromise in a real deployment | One shared demo password; no lockout, MFA or TLS in staging | PBKDF2 hashes, rate-limited login, HttpOnly SameSite cookie, authenticated-by-default API | Per-user credentials + MFA + TLS before any real users (TD-06, TD-07) — team, M4 |
| RISK-M3-02 Slow submissions under concurrent use | SQLite serialises writes (ADR-07) | WAL mode; measured at 10 users | Re-test on PostgreSQL or with a write queue before larger use — team, M4 |
| RISK-M3-03 Team cannot explain the AI-assisted code | Much of the M3 code was drafted with AI assistance (AI-M3-01, AI-M3-02) | All tests pass; mutation checks; small commits | **Every team member reviews and can explain the code before approving the PR** — all |
| RISK-M3-04 Requirement gaps at release | FR-006 search, FR-010 comments and FR-014 filters have no dedicated tests; the overdue window is not stakeholder-validated | Gaps listed in PED §1.5.4; CR-M3-02 records the overdue rationale | Add tests or formally defer through change control in M4; validate the 72 h window with stakeholders — Lethebe |
| RISK-M3-05 Evidence does not reflect production scale | One laptop, localhost, short single runs | Results are stated with their limits | Repeat runs; test in a production-like environment — Kasper |
| RISK-M4-01 No Account Lockout, reset, or MFA | Minimal risk given the version being released is still in Staging | Rate-limit Login, authentication-by-default API | Stay in staging with demo data only; add lockout, reset and MFA before any real users (TD-06) — all |
| RISK-M4-02 | Risk of demo passwords being accessed | Hidden JSON file on local server | Git-Ignore file to ensure users do not access prohibited profiles and passwords — all |
