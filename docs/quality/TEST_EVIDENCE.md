# Test Evidence (M3)

## DEF-10 fix and overdue exploratory test (2026-10-16)

- **Full run after the DEF-10 fix: 88/88 passed** (Core 65, API 23). New: `API10_Malformed_query_values_get_400_not_500`
  (3 cases). It was written first and failed in Development (`Expected BadRequest, Actual InternalServerError`), then passed after the fix.
- **Exploratory system test of CR-M3-02:** 19/19 checks passed against release `5042b6d` on an isolated instance (Staging
  configuration, throwaway database, submission times backdated). Checks covered the live deadline crossing (a request 50 s before its
  deadline became overdue with no data change), privacy per requester, staff category scope, manager counts overall and per category,
  assignment keeping the deadline, and resolve/reject clearing overdue. The script was run once during the session and is not part of the automated suite.

## Overdue follow-up (2026-10-16, working tree)

CR-M3-02 implements the user-selected three-day overdue rule. Baseline before edits: 72/72 passed.
After implementation: **85/85 passed** (Core 65, API 20), including all original cases and 13 new cases.
Release build with `dotnet build CivicConnect.sln -c Release --no-restore -warnaserror`: zero warnings/errors.
Full regression command: `dotnet test CivicConnect.sln --no-restore --logger "console;verbosity=minimal"`.

| Test basis | Cases | Expected / observed result | Interpretation |
|---|---:|---|---|
| `OverdueTests.Three_day_boundary_matches_policy_sql_counts_and_filters` | 8 | Both open states: -1/0 ticks not overdue, +1 tick overdue; terminal states excluded. Policy, SQL counts and true/false filters agree. All passed. | Strict elapsed-time boundary, including SQL precision. |
| `Derived_overdue_works_for_existing_rows_and_preserves_scope_filters_and_pagination` | 1 | Existing rows become overdue without writes; scope/category/status filters and pagination retained; reports remain Manager-only. Passed. | Backwards-compatible persistence and scoped reads. |
| `Assignment_keeps_original_deadline_and_terminal_transitions_clear_overdue` | 1 | Assignment keeps the original deadline; resolution/rejection remove overdue; audit histories retained. Passed. | Lifecycle and reporting cooperate. |
| `Deadline_uses_elapsed_time_across_offsets_and_future_submissions_are_not_overdue` | 1 | Offset changes do not change elapsed duration; future submission excluded. Passed. | Timezone-safe policy. |
| `OverdueApiTests.Overdue_journey_preserves_privacy_counts_filters_and_lifecycle` | 1 | Real login/submission/detail/list/counts/assignment/resolution through HTTP and SQLite; deadline flags/counts and access rules correct. Passed. | Integrated journey. |
| `Overdue_count_is_not_limited_to_the_first_page_and_invalid_filter_is_rejected` | 1 | One-row page with three total overdue requests; invalid boolean filter returns 400. Passed. | Count independence and HTTP input validation. |

The earlier 72-case run and coverage figures below are historical evidence, not new coverage measurements.

Manual browser smoke check used a separate local SQLite file on port 5088: requester login/submission,
deadline display and overdue label after backdating that isolated row, overdue/not-overdue filtering,
request detail, logout/Manager login and matching filtered Manager counts. The development database was
not touched. Assignment/reassignment/comments and terminal transitions are also exercised by the new
SQLite overdue journey. No new load or cross-browser claim is made.

**Run:** `dotnet test CivicConnect.sln -c Release` at commit `c5778f9`, 2026-10-16. **72 test cases, 72 passed, 0 failed**
(Core.Tests 54, Api.Tests 18). CI runs the same command on every push and PR (`.github/workflows/ci.yml`) and
uploads TRX results and coverage as the `test-results` artifact.

## Minimum baseline (M3 brief §9–10)

The headline count is inflated by data-driven cases: one theory over 16 status pairs counts as 16. The counts below are distinct test methods.

| Required | Minimum | Provided (distinct test methods) |
|---|---|---|
| Unit/component | 5 | 17 (`AuthorizationTests` 4, `LifecycleTests` 13) |
| Black-box functional | 5 | 7 (`BlackBoxTests` D1, D2, EP1, EP2, DT1, ST1; `API6` BVA) |
| API/integration | 3 | 9 API (`API1`–`API9`) + 5 SQLite integration (`SqliteRepositoryIntegrationTests`) |
| E2E/system | 2 | 2 (`E2E1`, `E2E2`), also run against staging |
| Negative / failure / unauthorised | 3 | 15+, e.g. API1, API2, API4, API5, API7, EP1 invalid partitions, DT1 R1–R3, rollback and conflict tests |
| High-priority requirement/risk | 5 | FR-001, FR-002, FR-007, FR-011, FR-015, NFR-004, NFR-005, NFR-007 (all MUST or High risk) |
| Black-box techniques | 2 | 4: equivalence partitioning, boundary value analysis, decision table, state transition |
| Performance | 1 | [PERFORMANCE_TEST.md](PERFORMANCE_TEST.md) |

## Key test evidence records

Expected results come from the acceptance criteria in PED §1.5 and CR-M2-01, written before execution.
"Result" means the 2026-10-16 run.

| Test | Level / technique | Basis | Input / precondition | Expected | Result | What it demonstrates |
|---|---|---|---|---|---|---|
| `Table_allows_only_the_three_approved_transitions` | Unit / state transition | FR-015, CR-M2-01 | All 16 (from, to) status pairs | Exactly 3 allowed | Pass | The transition table matches the approved model. It does not test permissions or saving. |
| `Reject_without_reason_is_refused_and_nothing_is_saved` | Unit / negative | FR-007 | Reject with whitespace reason | `MissingReason`, nothing saved | Pass | Rejection always carries feedback for the requester. |
| `Staff_outside_request_category_cannot_change_status` | Unit / negative, regression | DEF-01, FR-008 | Roads-only staff acts on a General request | `NotAuthorised`, nothing saved | Pass | Request-level authorization (mutation-checked). |
| `Failed_audit_write_rolls_back_the_status_change` | Integration (SQLite) / fault injection | NFR-004 | Trigger makes the audit insert fail | Exception; status, version and history unchanged | Pass | Status and audit save all-or-nothing in a real transaction. |
| `Stale_transition_is_reported_as_conflict_and_changes_nothing` | Integration (SQLite) | NFR-004, RISK-M2-03 | Same transition replayed with an old version | Second save returns false; one history row | Pass | No silent overwrite. True parallel writers are not tested. |
| `EP1_Submission_input_partitions` | Black-box / EP | FR-002, FR-003 | 2 valid + 4 invalid partitions | `None` / `InvalidInput` / `InvalidCategory`; invalid input stores nothing | Pass | Validation of each input class. |
| `DT1_Assignment_decision_table` | Black-box / decision table | FR-001, FR-008, FR-009 | Rules R1–R4 (role × group scope) | Only R4 (in-scope Staff) succeeds | Pass | Combined role and scope rule, against the real database. |
| `ST1_Request_follows_approved_path_and_resolved_is_terminal` | Black-box / state transition | FR-011, FR-015, FR-004 | Unassigned → Assigned → Resolved → Assigned | First two succeed; reopen is `InvalidTransition`; requester sees Resolved | Pass | Valid path persisted; terminal state enforced. |
| `API1_Unauthenticated_calls_get_401` | API / negative | NFR-005 | No cookie; 4 protected endpoints | 401 | Pass | Authentication is enforced at the HTTP boundary (fallback policy). |
| `API4_Requester_cannot_see_another_requesters_request` | API / negative | NFR-007, FR-005 | requester-2 reads requester-1's request | 404 detail; empty list | Pass | Privacy enforced end to end. A 404 does not confirm the request exists. |
| `API5_Non_staff_cannot_change_status` | API / negative | FR-001 | Requester or Manager posts a transition | 403; status unchanged | Pass | Role restriction over HTTP. |
| `API6_Title_length_boundaries` | API / BVA | FR-002 API contract (max 120) | Title length 0, 1, 120, 121 | 400, 201, 201, 400 | Pass | Both edges of the valid range. |
| `API8_Manager_counts_match_stored_requests_and_are_manager_only` | API | FR-012, FR-013 | 3 submitted, 1 rejected | Open 2, Rejected 1, Resolved 0; Staff 403 | Pass | Counts are correct for controlled data. Overdue is not covered (DEF-07). |
| `E2E1_Request_is_submitted_assigned_resolved_and_the_requester_sees_the_outcome` | E2E / system | FR-002, FR-004, FR-009, FR-011 | 2 logins; full journey via public API | Requester sees Resolved, assignee and note | Pass in-process **and on staging `c5778f9`** | The critical journey works across the integrated release candidate. |
| `E2E2_Rejected_request_shows_the_requester_the_reason` | E2E / system | FR-007 | Staff rejects with a reason | Requester sees Rejected and the reason | Pass in-process and on staging | Feedback reaches the requester. |

## Mutation checks (do the tests detect the defects they claim to?)
| Change made temporarily | Tests that failed | Restored |
|---|---|---|
| Removed the DEF-01 `CanView` guard in `TransitionService` | `Staff_outside_request_category_cannot_change_status`, `DT1` rule R3 (2 failed / 54) | Yes (commit `29ffc3a` message) |
| Removed the DEF-08 category mapping in the API | `API3` (`Expected "Roads", Actual "roads"`) | Yes (commit `4b29631` message) |

## Coverage (Coverlet, 2026-10-16) and what it means
| Measured by | Api | Core | Data |
|---|---|---|---|
| Api.Tests | 89% line / 59% branch | 82% / 54% | 85% / 88% |
| Core.Tests | — | 80% / 58% | 67% / 67% |

Coverage shows which code ran during the tests, not whether its results were checked. The ~55–60% branch
coverage in Core and Api reflects untested paths, mainly comment and reassignment
(`RequestService.AddCommentAsync`/`ReassignAsync` have no direct test) and some error-mapping branches. No coverage target is set, as the M3 brief prescribes none.

## Static analysis and security checks
| Check | Finding | Handling |
|---|---|---|
| .NET analyzers (`latest-recommended`, CI `-warnaserror`) | CA2208 in Core (wrong `paramName`), CA1050, CA1305 (locale-dependent parsing in the load tool) | Fixed |
| | CA1707 (underscore test names), CA1848/CA1873 (logging micro-optimisation) | Suppressed with written reasons in `.editorconfig` |
| `dotnet list package --vulnerable` | 3 High advisories (DEF-05) | Fixed; CI gate now blocks new ones |
| Gitleaks (CI) | Runs on GitHub only; not run locally | See the CI run for its result |
| Manual security review | DEF-01 resource-level authorization (Member One) | Fixed and regression-tested |
