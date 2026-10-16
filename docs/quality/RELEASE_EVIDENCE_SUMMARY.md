# Release Evidence Summary — M3 Release Candidate

**Candidate:** CivicConnect `0.3.0`, branch `m3-release-candidate`. The final commit and tag are recorded in PED §1.16 once merged.

**Classification: CONDITIONALLY READY.** The critical behaviour is integrated and verified, including on staging. The release
depends on the conditions below, and the known limitations are recorded. It is **not** production-ready.

| Evidence area | Result | Interpretation | Concern / action |
|---|---|---|---|
| Build & CI | Local: strict build 0 warnings; all gates pass. GitHub CI: **not yet run** at time of writing | The gates are build + analyzers, tests, the vulnerability scan and Gitleaks. A green run shows these checks pass. It does not show requirements are complete or the system is secure | **Condition 1:** CI must pass on the PR into `main`, and `CI / build-and-test` must be a required check |
| Automated regression | 72/72 pass (54 Core, 18 API) | Lifecycle, authorization, persistence atomicity and privacy rules are protected. Two mutation checks show the tests detect the defects they target | Comments and reassignment are untested (coverage gap) |
| API/integration | 9 API + 5 SQLite tests pass | Authentication, role and privacy rules hold over real HTTP and a real database | — |
| System/E2E | E2E1 and E2E2 pass in-process and **on staging `c5778f9`** | The critical requester → staff → requester journey completes on the deployed build | The browser UI is not automated, only demonstrated manually (TD-09) |
| Static/security | Analyzer findings fixed or suppressed with reasons; 3 High dependency advisories fixed (DEF-05); DEF-01 authorization gap fixed | No known open High security finding | No TLS in staging (TD-07); shared demo password, no MFA (TD-06) |
| Performance | 10 users × 60 s: 0 errors, submit p95 311 ms after the WAL fix (818 ms before) | Stable under small load; writes queue on SQLite | Residual risk RISK-M3-02. No formal NFR-001 target |
| Known defects | Open: DEF-03 (Medium, documentation), DEF-07 (Medium, overdue undefined); DEF-09 mitigated | None blocks the critical journey | **Condition 2:** agree the overdue rule (FR-013) or formally defer it through change control |
| Acceptance | FR-001/002/003/004/005/007/008/009/011/015 and NFR-004/005/007 demonstrated by tests (PED §1.5.4) | Core requester and staff scope works | FR-006 search, FR-010 comments, FR-012–014 (partly), NFR-001/002/003/006/008 not fully verified |
| Governance | Work on branch `m3-release-candidate` | Brief requires a PR with 2 approvals from non-authors | **Condition 3:** PR review + 2 approvals; ADR-07 and ADR-08 move from Proposed to Accepted |

## Residual risks
| Risk | Why it remains | Current mitigation | Next action / owner |
|---|---|---|---|
| RISK-M3-01 Account compromise in a real deployment | One shared demo password; no lockout, MFA or TLS in staging | PBKDF2 hashes, rate-limited login, HttpOnly SameSite cookie, authenticated-by-default API | Per-user credentials + MFA + TLS before any real users (TD-06, TD-07) — team, M4 |
| RISK-M3-02 Slow submissions under concurrent use | SQLite serialises writes (ADR-07) | WAL mode; measured at 10 users | Re-test on PostgreSQL or with a write queue before larger use — team, M4 |
| RISK-M3-03 Evidence does not reflect production scale | One laptop, localhost, short single runs | Results are stated with their limits | Repeat runs; test in a production-like environment |
| RISK-M3-04 Large AI-assisted change set | Much of the M3 code was drafted with AI assistance (AI-M3-01, AI-M3-02) | All tests pass; mutation checks; small commits | **Every team member reviews and can explain the code before approving the PR** — all |
