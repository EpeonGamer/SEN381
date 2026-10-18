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
* **Later Consequence:** Held for the whole project: the PED, ADRs, CRs and registers are Markdown in the repository, so every change is reviewed in pull requests and traceable in Git history.

## DEC-M1-02: Deferral of Technology-Stack Selection

* **Date:** 2026-09-09
* **Description:** Defer final selection of the backend/frontend stack and hosting platform to Milestone 2.
* **Context & Constraints:** M1 Brief Section 5 excludes final technology-stack selection from Milestone 1 scope.
* **Alternatives:** Select stack now based on team familiarity; defer until M2 once requirements/architecture drivers are known.
* **Decision & Rationale:** Defer to M2. Stack choice should follow the baselined requirements, not precede them.
* **Trade-offs:** No early start on implementation/tooling familiarisation.
* **Risks:** Late selection compresses time available in M2 for prototyping before M3 construction begins.
* **Evidence:** M1 Brief Section 5; PED FEC-02, FEC-03.
* **Later Consequence:** The stack was chosen in M2 (DEC-M2-06, ADR-06). The recorded risk materialised in M3: with construction time compressed, PostgreSQL was replaced by SQLite for the release (ADR-07, TD-05).


## DEC-M2-05: Centralised Role-Based Request Visibility and Authorization

* **Date:** 2026-09-30
* **Description:** Establish a dedicated authorization responsibility for request visibility and operational permissions, separate from lifecycle control.
* **Context & Constraints:** CivicConnect must distinguish requester ownership, Staff group-based request visibility and broader Manager access. The Core implementation must remain small and independently testable while the API/data layers are still being developed.
* **Alternatives:** Scattered role checks; ASP.NET Core authorization alone; dedicated authorization plus Strategy-based visibility policy; database row-level security.
* **Decision & Rationale:** Use a dedicated `Authorization` area with `IVisibilityPolicy` and `VisibilityScope`. Staff group permissions combine as a union. Lifecycle services delegate authorization to this policy rather than owning role-specific checks.
* **Trade-offs:** Adds a small abstraction and requires later integration with API/query and persistence layers.
* **Risks:** Future implementations could bypass the policy; mitigated through service boundaries, tests and PR review.
* **Evidence:** ADR-05; 29 Core tests passing after implementation; `src/CivicConnect.Core/Authorization/`; `tests/CivicConnect.Core.Tests/AuthorizationTests.cs`.
* **Later Consequence:** Integrate `VisibilityScope` with list queries and authenticated API actors when those layers are implemented.


## DEC-M2-06: Technology Stack Selection

* **Date:** 2026-09-30
* **Description:** Use C#, ASP.NET, PostgreSQL and npgSQL as the technology stack.
* **Context & Constraints:** CivicConnect requires technology stack for development. The stack needs to adhere to the Architecturally significant requirements established in the PED.
* **Alternatives:** Python Programming, Django, with PostgreSQL/SQLite; Java, SpringBoot, with PostgreSQL, JUnit and Maven; Typescript, Node/Express, with PostgreSQL/Vitest/Jest, React/Server-rendered Pages.
* **Decision & Rationale:** The reasoning behind the decisions is due to the familiarity of the software available, the languages and the integrations to the development team. Additionally security and maintainability are also adequately accounted for from these technologies.
* **Trade-offs:** Potential feature of other software not explored; Python may have simpler development, combined with unknown future integration support.
* **Risks:** Potential performance and cost parameters if the technology stack changes.
* **Evidence:** ADR-06; StackDiagram; PED 1.12; CivicConnect A2 design quality and Design Pattern research.
* **Later Consequence:** Should there be a change in technology stack used, the software to implement the stack will also be required to change.


## DEC-M4-01: Only the Current Assignee May Resolve a Request

* **Date:** 2026-10-18
* **Status:** Accepted 2026-10-18 (Kasper reviewed the decision against Lethebe's implementation and tests).
* **Description:** A request in Assigned status can be resolved only by the staff member it is currently assigned to. Any other in-scope Staff member is refused with `NotAssigned` (403) and must first reassign the request to themselves.
* **Context & Constraints:** Up to M3, any Staff member whose groups covered the request's category could resolve it (FR-011, CR-M2-01). With reassignment exposed in M4 (FR-009), the stored assignee is the accountable owner, and letting someone else close the request would leave the audit trail naming a different person than the one responsible. The approved transitions (CR-M2-01 §3) must not change.
* **Alternatives:** Keep resolution open to all in-scope Staff; allow it but record "resolved on behalf of"; restrict to the assignee (chosen).
* **Decision & Rationale:** Restrict to the assignee. It matches the stakeholder need for clear responsibility and accountability (§1.4) and keeps assignment history and resolution consistent. It narrows *who* may perform an existing transition; it adds no status or transition, so no change request is required.
* **Trade-offs:** An absent assignee blocks resolution until someone reassigns the request (one extra step).
* **Risks:** Low. Reassignment uses the version check, so a stale reassignment cannot silently override a newer one.
* **Evidence:** `TransitionService` (`NotAssigned` check); `ResolutionOwnershipTests` (non-assignee refused with nothing changed, must reassign to self, former assignee refused, current assignee succeeds); PED §1.5.4 FR-011. Proposed and implemented by Lethebe in commit `2c82f1b`.
* **Later Consequence:** Resolution and assignment history now always name the same accountable person. Operational effect to review if real use shows frequent reassign-before-resolve steps.
