# Accountability and Verification

## USE OF AI

> Title format: `AI-MILESTONE-ENTRY_NUM: TITLE`


### AI-M1-01: Setup of Documentation & Governance Templates

* **Date:** 2026-09-08
* **Student:** Kasper
* **Tool Used:** Gemini 1.5 Flash
* **Engineering Task:** Setup of documentation and baseline templates.
* **AI Contribution:** Generated initial Markdown templates for project documentation, Decision Logs, and Accountability and Verification.
* **Verification Applied:** Cross-checked generated template schemas against the mandatory fields specified in SEN381 Master Brief Section 10.1, Section 13, and Appendix C.
* **Decision:** Accepted with modifications.
* **Issues Found & Corrected:** The generated output was verbose and jargon-heavy. It added unnecessary or combined fields (e.g., *Reviewers & Status*), requiring manual additions and edits before committing to the repository.

### AI-M1-02: Table structure & Templates for: FR | NFR | Traceability matrix

* Date: 2026-08-09
* Student: Lethebe
* Tool used: GPT-5.6 Sol
* Engineering Task: Template for requirements & traceability matrix
* AI Contribution: Generated the table for the Functional & Non-functional Requirements, and the traceability matrix. Column headers.
* Verification Applied: Cross-checked and manually reviewed and compared against the project scope and stakeholder needs
* Decision: Accepted with modifications
* Issues found & corrected: The output contained unecessary columns which the decision was taken to not include those columns within the PED.  

### AI-M1-03: FEC Formatting and Grammar Pass

* **Date:** 2026-09-09
* **Student:** Kasper
* **Tool Used:** Gemini 1.5 Flash
* **Engineering Task:** FEC formatting and grammar pass.
* **AI Contribution:** Checked if FEC language use was fit-for-purpose and suggested edits.
* **Verification Applied:** Checked against established language use and ease of reading standards.
* **Decision:** Rejected most edits. Kept genuine clarity improvements.
* **Issues Found & Corrected:** The generated output was often difficult to parse or softened points made.

### AI-M2-01: RTM Table Structure

* **Date:** 2026-09-29  
* **Student:** Lethebe Kutloano Maine  
* **Tool used:** GPT-6 Sol Medium  
* **Engineering Task:** Evolve the Milestone 1 Requirements Traceability Matrix (RTM) for Milestone 2.  
* **AI Contribution:** Suggested a table structure linking each requirement to its source, priority, acceptance criteria, architecture responsibility, data impact, design and technology decisions, application artefacts, verification evidence and status.  
* **Verification Applied:** Compared the proposed columns with Milestone 2 and ensured that the row data match.  
* **Decision:** Accepted with modifications. Accepted table structure such as number of columns, rows and the column labels.  
* **Issues found & corrected**: Removed the proposed “Assignment 2” tracking column. Distinguished actual M2 evidence from work that must be marked *Planned* or *Not Yet Implemented*.

### AI-M2-02: Persistence Baseline Table Structure

* **Date:** 2026-09-28  
* **Student:** Lethebe Kutloano Maine  
* **Tool used:** GPT-6 Sol Medium  
* **Engineering Task:** Structure the initial CivicConnect data and persistence baseline.  
* **AI Contribution:** Suggested table headings for entities, key attributes, relationships, ownership, lifecycle implications and persistence decisions.  
* **Verification Applied:** Compared the structure with the requester, staff and management capabilities back in the Master Project Brief  
* **Decision:** Accepted with modifications.  
* **Issues found & corrected:** Refined the distinction between stored request data, historical records and the management reporting view.

### AI-M2-03: Initial ERD Model

* **Date:** 2026-09-28  
* **Student:** Lethebe Kutloano Maine  
* **Tool used:** GPT-6 Sol Medium  
* **Engineering Task:** Draft an initial entity-relationship diagram (ERD) for CivicConnect.  
* **AI Contribution:** Proposed entities and relationships for users, requests, categories, assignment responsibility, status history, actions/comments, feedback and management reporting.  
* **Verification Applied:** Reviewed the diagram against the persistence table and checked relationships  
* **Decision:** Accepted with modifications.  
* **Issues found & corrected:** Simplified the diagram for readability, added missing concepts identified during review, and clarified that the management reporting will be regarded as a view instead of its own hard written entity.
### AI-M2-04: ADR Structure and Submission Review

* **Date:** 2026-09-30
* **Student:** Kasper
* **Tool used:** GPT-5.6 Luna
* **Engineering Task:** Structure and review M2 ADRs before submission.
* **AI Contribution:** Suggested ADR structure and reviewed the submission for consistency and unsupported assumptions.
* **Verification Applied:** Team requirements and existing project evidence were checked; substantial changes were made to align with the rest of the team's work.
* **Decision:** Accepted with modifications.
* **Issues found & corrected:** Unfounded assumptions in the initial output were removed or revised during team review.


### AI-M3-01: SQLite repository, schema and integration tests

* **Date:** 2026-10-15
* **Student:** Kasper
* **Tool Used:** Claude Sonnet 5.5
* **Engineering Task:** Create the missing persistence layer (`src/CivicConnect.Data`) so the Core can be integration-tested against a real database.
* **AI Contribution:** Drafted the SQLite schema, `SqliteRequestRepository` (all `IRequestRepository` members), `SqliteSetup` seeding helper, five integration tests and ADR-07.
* **Verification Applied:** Kasper reviewed the schema and repository code himself, focusing on the SQL visibility filter, the single transaction per status change and the version check. When drafted, the AI could only compile-check against stubbed package APIs. On 2026-10-16 it was built and run with the real `Microsoft.Data.Sqlite` package and xUnit: 0 warnings, all 5 integration tests passing (40/40 suite). Since then the same code has passed CI, the API integration and E2E tests, the staging E2E run and a load test (88/88 suite).
* **Decision:** Accepted with modifications.
* **Issues Found & Corrected:** (1) `Microsoft.Data.Sqlite 8.0.10` pulled in a High-severity vulnerable native SQLite library. Upgraded to 8.0.32 (DEF-05, commit `950f885`). (2) Under 10 concurrent users, submissions queued behind SQLite's single writer (p95 818 ms). WAL journal mode added (DEF-09, `c5778f9`). (3) WAL side files were not git-ignored. Fixed in `d57997b`. (4) The repository was later extended with an injectable clock for overdue reporting (Lethebe, `5042b6d`). ADR-07 records the remaining SQLite-versus-PostgreSQL trade-off.

### AI-M3-02: M3 recovery — API, frontend, tests, CI, staging and release evidence

* **Date:** 2026-10-16
* **Student:** Kasper (directed the work, approved the plan and its scope)
* **Tool Used:** Claude Opus 5.5 (Claude Code, agentic coding assistant)
* **Engineering Task:** Turn the Core + SQLite code into an integrated, verified M3 release candidate.
* **AI Contribution:** Audited the repository against the M3 brief. Drafted `src/CivicConnect.Api` (endpoints, cookie authentication, PBKDF2 hashing, seeding, static frontend), `SqliteCredentialStore`/`SqliteReferenceData`, the completed `BlackBoxTests`, `tests/CivicConnect.Api.Tests` (API and E2E), the CI workflow, the staging scripts, the load-test tool, the WAL change, ADR-08, the defect/technical-debt/test/performance/staging/release documents and the PED v3.0 sections. It also merged Member One's M3 review into the PED.
* **Verification Applied:** Every change ran through `dotnet build -warnaserror` and the full test suite (now 88/88). Mutation checks confirmed that the DEF-01 and DEF-08 regression tests fail when the fix is removed. E2E tests ran against the deployed staging build, and staging `/health` was checked against `git rev-parse HEAD`. Kasper then verified it independently. He ran the test suite locally and walked through the UI as requester, staff and manager, including the denial checks. He re-ran the load test (submit p95 316 ms, against the AI's 311 ms) and confirmed CI passed on the push and the pull request. The overdue feature (Lethebe, CR-M3-02) was reviewed and tested with a 19-check exploratory run. All three team members must still be able to explain the code (RISK-M3-03).
* **Decision:** Accepted with modifications (Kasper). Final team sign-off is the two approvals on the M3 pull request.
* **Issues Found & Corrected:** The CI analyzer gate rejected the AI's own load-test code (CA1305 locale-dependent parsing), which was then fixed. Exploratory testing found that the AI's API error handler returned 500 instead of 400 for malformed input in Development (DEF-10). A regression test was written first and failed, then the handler was fixed (`d57997b`). The AI's first presentation PDF was silently shrunk to about half size by a CSS multi-column print bug; this was found by rendering the pages and corrected. A failed shell edit briefly produced a misleading "build succeeded", and miscounted test and request numbers in drafts were corrected. Changes to the existing Core/Data code were limited to two lines (CA2208 parameter name in Core; WAL pragma in Data). DEF-08, DEF-09 and DEF-10 were found through testing rather than assumed.

### AI-M4-01: Per-account seed passwords, new seed accounts and optional HTTPS staging

* **Date:** 2026-10-18
* **Student:** Kasper (chose the scope, reviewed every diff, ran the manual checks)
* **Tool Used:** Claude Opus 5.5 (Claude Code)
* **Engineering Task:** Repay TD-06 (shared demo password), add the General and Waste staff accounts for Member Three, and exercise HTTPS (NFR-002, TD-07), without adding product features.
* **AI Contribution:** Drafted per-account password seeding (`Seed:PasswordsFile`), `API11`/`API12`, the test-harness, load-test and staging-script changes, the two new accounts, the `start.ps1 -Https` switch and the README/STAGING.md updates. Proposed making HTTPS opt-in after Kasper asked how a default would affect teammates and the demo.
* **Verification Applied:** Strict build and full suite after every change (90/90, then 109/109 after merging Member One's work). Mutation check: removing the shared-password guard makes `API12` fail. Staging E2E and a short load test passed; the merge was dry-run first. HTTPS checked on staging (307 redirect, `Secure` cookie, E2E over HTTPS) and plain HTTP re-checked. Kasper tested the merged build by hand, checked sign-in, filtering and navigation in three browsers (B1), and trusted the development certificate personally.
* **Decision:** Accepted (Kasper). Waste-Team grouping to be confirmed by Member Three.
* **Issues Found & Corrected:** (1) A .NET 9-only API in a .NET 8 project, replaced before building. (2) Analyzer warning CA1869, fixed. (3) The AI's branch setup pushed the first commits to the shared `m4-refinement`; reverted there and re-created on `m4-passwords` with new hashes so the later merge would apply. (4) README wrongly said STAGING.md covered the load test. (5) A leftover HTTPS setting could break plain-HTTP logins; the default mode now resets it. (6) A manual login failure (staging password used on the dev server) led to documenting both password files.

### AI-M4-02: Final RTM, scope completion, decision record and technical-debt closure

* **Date:** 2026-10-18
* **Student:** Kasper
* **Tool Used:** Claude Opus 5.5 (Claude Code)
* **Engineering Task:** Bring the RTM, decision log, test evidence and Technical Debt Register to final status and calculate the Scope Completion Percentage.
* **AI Contribution:** Checked Member One's new tests against each open acceptance criterion and drafted the RTM final-status column, PED §1.6.7, DEC-M4-01 (inferred from Member One's commit and code), the M4 test-evidence entries and the debt register's final status with TD-10.
* **Verification Applied:** Kasper reviewed each diff before committing and made the judgement calls stated in the documents: FR-012 Met against its written criterion (sensitivity shown), DEC-M4-01 accepted after checking the code, NFR-001 left Partially met instead of inventing a target, HTTPS opt-in, and a reworded TD-02 after Kasper challenged the AI's draft. Totals were recomputed and checked; the NFR-006 record claims only what Kasper reported.
* **Decision:** Accepted (Kasper).
* **Issues Found & Corrected:** An early scope estimate (85%) was an arithmetic slip, corrected before writing. DEC-M4-01 was first left "to be added", then written. The missing HSTS header was recorded as a localhost limitation, not claimed. Items outside the AI's remit were raised, not decided: Waste-Team grouping, unowned `Later Consequence: TBD` entries, and the Defect and AI registers still at M3 status.

* ### AI-M4-03: Frontend structure refined into a bento grid layout

* **Date:** 2026-10-17 
* **Student:** Lethebe Kutloano Maine.
* **Tool Used:** GPT-6.1 Sol Medium.
* **Engineering Task:** Improve the frontend structure and presentation of the existing CivicConnect workflows.
* **AI Contribution:** Assisted with reorganising the frontend into a bento-style grid of panels, reported by Lethebe. The resulting HTML/CSS separates submission, request history/detail and Manager counts, with responsive spacing and layout.
* **Verification Applied:** Repository review of `src/CivicConnect.Api/wwwroot/index.html`, `styles.css` and `app.js`, committed in `2c82f1b`. Existing team evidence in `docs/quality/TEST_EVIDENCE.md` records Kasper's manual sign-in, filtering and navigation checks in three desktop browsers. 
* **Decision:** AI-assisted layout is integrated in the final code. Retain the implemented changes; changes were accepted with minor modifications entailing to the container structuring.
* **Issues Found & Corrected:** The repository contains responsive layout rules and horizontally scrollable tables. 

### AI-M4-04: Tests for assigning a request to another Staff member

* **Date:** 2026-10-17.
* **Student:** Lethebe Kutloano Maine.
* **Tool Used:** GPT-6.1 Sol Medium.
* **Engineering Task:** Create tests for assignment to another authorised Staff member and subsequent reassignment (FR-009).
* **AI Contribution:** Assisted with creating tests for the “assign to other staff member” feature.
* **Verification Applied:** Reviewed `RefinementApiTests`, `ReassignmentTests` and `ResolutionOwnershipTests` from `2c82f1b`. Checks cover eligible colleagues, denial of unauthorised/ineligible callers, stale-version conflicts, preserved assignment history, rollback on failed audit, refusal of terminal/same-assignee reassignment, assignment directly to a colleague, and resolution by the current assignee. The recorded M4 run is 109/109 passing; the fresh review result is recorded separately in `docs/quality/TEST_EVIDENCE.md`.
* **Decision:** Tests are integrated in the final suite. 
* **Issues Found & Corrected:** The M3 coverage/integration gap was addressed by dedicated tests and exposed reassignment endpoints. Additional group membership used by tests is confined to disposable test data.

### AI-M4-05: Tests for the category feature

* **Date:** 2026-10-18
* **Student:** Lethebe Kutloano Maine.
* **Tool Used:** GPT-6.1 Sol Medium 
* **Engineering Task:** Create tests associated with the category feature
* **AI Contribution:** Assisted with creating category-feature tests. The identifiable M4 test is `RefinementApiTests.Combined_filters_scope_pagination_and_counts_match`; existing category submission/validation tests also remain in the suite. 
* **Verification Applied:** Reviewed the combined-filter test: it creates seven matching Roads requests plus nonmatching Water/title records, applies category + status + search, checks 5/2 pagination and seven distinct results, verifies matching Manager counts, case-insensitive category sorting, and an empty result for a different requester. 
`BlackBoxTests.EP1_Submission_input_partitions` and `ApiIntegrationTests.API3_Submitted_request_is_created_and_readable_back` provide separate baseline category validation/storage evidence. Fresh Release regression on 2026-10-18 passed 109/109 (68 Core, 41 API).
* **Decision:** Tests are integrated in the final suite.
* **Issues Found & Corrected:** The repository implements selection from an existing category catalogue and category filtering, but no endpoint or UI for creating catalogue categories. 

### AI-M4-06: Git commit title and description creation

* **Date:** 2026-10-18
* **Student:** Lethebe Kutloano Maine.
* **Tool Used:** GPT-6.1 Sol Medium 
* **Engineering Task:** Writing commit messages to Github
* **AI Contribution:** Assisted with creating said commit titles and descriptions pertaining to M4 refinement. 
* **Verification Applied:** Wording was checked to ensure that it matches with the implementation being commited to Github. Each modifications or additions were ensure to be included and correctly described.  
* **Decision:** AI output was accepted with minor changes in wording and reduction of sentence lengths.
* **Issues Found & Corrected:** Some generated commit descriptions were too wordy and required minor wording changes. Sentences were shortened, and descriptions were checked against the committed changes to ensure accuracy. 

