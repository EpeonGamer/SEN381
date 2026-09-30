# Engineering Decision Log

> Title format: `DEC-MILESTONE-ENTRY_NUM: TITLE`

## DEC-M1-01: Adoption of Markdown for Documentation

* **Date:** 2026-09-08
* **Description:** Adopt `.md` format for all project documentation.
* **Context & Constraints:** Project documentation requires iteration and tracking.
* **Alternatives:** `docx`, `txt`
* **Decision & Rationale:** Use Markdown for project documentation.
* **Trade-offs:** Will not have the formatting and tools of MS Word.
* **Risks:** Team members may struggle with syntax.
* **Evidence:** Master Brief Appendix C; team feedback.
* **Later Consequence:** TBD

## DEC-M1-02: Deferral of Technology-Stack Selection

* **Date:** 2026-09-09
* **Description:** Defer final selection of the backend/frontend stack and hosting platform to Milestone 2.
* **Context & Constraints:** M1 Brief Section 5 excludes final technology-stack selection from Milestone 1 scope.
* **Alternatives:** Select stack now based on team familiarity; defer until M2 once requirements/architecture drivers are known.
* **Decision & Rationale:** Defer to M2. Stack choice should follow the baselined requirements, not precede them.
* **Trade-offs:** No early start on implementation/tooling familiarisation.
* **Risks:** Late selection compresses time available in M2 for prototyping before M3 construction begins.
* **Evidence:** M1 Brief Section 5; PED FEC-02, FEC-03.
* **Later Consequence:** TBD


## DEC-M2-02: Centralised Role-Based Request Visibility and Authorization

* **Date:** 2026-09-30
* **Description:** Establish a dedicated authorization responsibility for request visibility and operational permissions, separate from lifecycle control.
* **Context & Constraints:** CivicConnect must distinguish requester ownership, Staff group-based request visibility and broader Manager access. The Core implementation must remain small and independently testable while the API/data layers are still being developed.
* **Alternatives:** Scattered role checks; ASP.NET Core authorization alone; dedicated authorization plus Strategy-based visibility policy; database row-level security.
* **Decision & Rationale:** Use a dedicated `Authorization` area with `IVisibilityPolicy` and `VisibilityScope`. Staff group permissions combine as a union. Lifecycle services delegate authorization to this policy rather than owning role-specific checks.
* **Trade-offs:** Adds a small abstraction and requires later integration with API/query and persistence layers.
* **Risks:** Future implementations could bypass the policy; mitigated through service boundaries, tests and PR review.
* **Evidence:** ADR-05; 29 Core tests passing after implementation; `src/CivicConnect.Core/Authorization/`; `tests/CivicConnect.Core.Tests/AuthorizationTests.cs`.
* **Later Consequence:** Integrate `VisibilityScope` with list queries and authenticated API actors when those layers are implemented.
