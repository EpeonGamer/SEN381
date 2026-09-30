# ADR-04: Request Lifecycle Control (Design Problem 1)

**Status:** Accepted

**Date:** 2026-09-30  **Author:** Kasper, Member 3

## 1. Design Problem and Context

Request status is CivicConnect's central business rule (FR-009, FR-011, FR-015, FR-007). After CR-M2-01 the rules are:

- Four statuses: Unassigned, Assigned, Rejected, Resolved.
- Three valid transitions. Every other transition must be refused, leaving the request unchanged.
- Each transition has its own data requirement: an assignee (FR-009), a mandatory reason (FR-007), resolution information (FR-011, FR-010).
- Each accepted transition must be saved together with its audit entry, or not at all (NFR-004, FEC-01).

**The problem:** if these rules are enforced in several places (UI, API handlers, service code), invalid states become possible, the rules cannot be tested in one place (FEC-03) and the audit guarantee can be bypassed. The design must give the lifecycle rules **one controlled home**.

**Forces and constraints:**
- The model is deliberately minimal (CR-M2-01). Reinstating In Progress or adding reopening is possible later through change control, so adding a state or transition must stay cheap.
- Maintainability and testability are committed drivers (FEC-02, FEC-03).
- Three-person team, fixed deadline, cost and learning-curve constraints (PED 1.6.5).
- Stack selected by the team: C# with ASP.NET Core and PostgreSQL on a centralised database server (ADR-06; persistence baseline in PED §1.12).

## 2. Research Evidence Used (A2), Referenced Not Copied

| A2 evidence | How it was used |
|---|---|
| A2, Design Quality and Design Patterns: Problem 1 alternatives (Factory Method, Builder, Bridge, State) and the Research-to-Decision Map row "Design Problem 1" | Starting list of alternatives. A2 recommended Factory Method + Bridge. |
| A2, same section: State pattern dismissed as not following SOLID as well as the others | Re-examined below on **fit to the problem**, not SOLID compliance alone. |
| A2, SOLID summary (Single Responsibility, Open-Closed) | Evaluation criteria. |
| A2, Persistence & Data Integrity, recommendation of synchronous commits (ADR-02) | Transition and audit entry are committed in one transaction. |
| Refactoring.Guru, State pattern (see References) | Describes State as closely related to finite-state machines, and identifies its benefit as removing the state conditionals that spread across methods as state-dependent behaviour grows. Used as the criterion for when State pays off (section 5). |
| Embedded.com, Implementing finite state machines in embedded systems (see References) | Describes table-driven state machines as making handling explicit and easier to maintain, notes memory cost for sparse tables, and states that there is no default implementation choice. Used for the trade-off analysis (sections 3 and 6). |

**Where M2 differs from the A2 recommendation, and why.** A2 framed Problem 1 as class proliferation and recommended Factory Method + Bridge. After the M1 review and CR-M2-01, the real problem is enforcing rules on a small lifecycle. CivicConnect has no family of interchangeable products for Factory Method to create, and no abstraction/implementation hierarchy for Bridge to decouple, so neither is carried forward. A2 also dismissed State on SOLID grounds. This ADR reaches a similar outcome for a different and stronger reason: fit.

## 3. Alternatives Considered

| Option | Rules in one place | Testability | Cost of adding a state | Complexity | Fit |
|---|---|---|---|---|---|
| A. Status enum with conditionals scattered across services | No | Poor | Edit many locations | Lowest | Rejected: reproduces the problem |
| B. State pattern (one class per state) | Yes, per state | Good | New class plus edits to existing states | 4 classes for 3 transitions. Terminal states hold only refusals. | Weak: see rationale |
| **C. Table-driven state machine (chosen)** | Yes, one table plus guard rules | Best: table can be tested exhaustively | Add a row | Low. Small indirection through the table and guards. | Strong |
| D. Factory Method + Bridge (A2 recommendation) | Not applicable | Not applicable | Not applicable | Adds hierarchy with no matching problem | Rejected: no matching problem |

## 4. Decision

Use a **table-driven state machine**:

- `RequestStatus`: a controlled enumeration of the four statuses.
- `TransitionRules`: a single table mapping each allowed (from, to) pair to its required conditions (assignee, reason, resolution information). Any pair not in the table is refused.
- `TransitionService`: the **only** code path allowed to change status. It (1) checks the actor is authorised (FR-001), (2) checks the transition exists in the table, (3) checks the guard conditions, (4) saves the status change and its audit entry in one transaction (ADR-02).

Concrete C# form (namespace `CivicConnect.Core.Lifecycle`):

- `RequestStatus`: an `enum` with the four values.
- `TransitionRules`: a static, read-only dictionary keyed by `(From, To)` whose value is a `TransitionRequirement` flag (Assignee, Reason, ResolutionNote).
- `IRequestRepository`: the persistence boundary. `SaveTransitionAsync` must save the status change and audit entry atomically and only if the stored status still equals the expected `From` value (protects against two staff acting at once).
- `TransitionService.ApplyAsync`: the single entry point. Storage failures propagate as exceptions after rollback. They are never reported as success.
- The PostgreSQL implementation of `IRequestRepository` belongs to the data layer and uses a database transaction (see section 8, verification).

## 5. Rationale

- **Rules attach to transitions, not states.** Each requirement (assignee, reason, resolution information) belongs to a transition. The State pattern pays off when the *same operation behaves differently depending on state*. CivicConnect has no such operation in the baseline.
- **The model is small.** Three valid transitions and two terminal states would become four classes, two of which only refuse.
- **Testing is direct.** The 3 valid transitions and the 9 refused pairs between different states can be enumerated from the table. Same-state repeats are also refused.
- **Change is cheap.** Adding In Progress or reopening means adding rows and guard rules, matching the controlled-change path in CR-M2-01.
- **Idiomatic in C#.** An enum plus a read-only dictionary is simple to read and review. The repository interface keeps the rules independent of the database, so the rules can be unit tested without a database and the data store can be replaced later.

## 6. Expected Benefit and Complexity Introduced

| Benefit | Complexity / trade-off |
|---|---|
| One authoritative definition of the lifecycle | Indirection: developers must read the table plus guard rules rather than a flat method |
| Exhaustive, cheap unit tests | All status changes must go through `TransitionService`. This needs review discipline (PR checklist item). |
| Cheap extension by adding rows | If states later gain genuinely different behaviour, the table may need refactoring towards State (see 8) |
| Supports the atomic status + audit guarantee | Guard rules keyed by transition add a small registry to maintain |

## 7. Affected Components

| Component | Responsibility |
|---|---|
| `RequestStatus` | Defines valid status values |
| `TransitionRules` | Holds the allowed transitions and required conditions |
| `TransitionService` | Applies rules, authorisation and atomic save with audit |
| `IRequestRepository` | Persistence boundary. Atomic save plus conflict detection. |
| PostgreSQL repository (data layer, not yet written) | Implements `IRequestRepository` using a database transaction |
| Request and audit entities | Store current status and lifecycle history (data model owned by the data/persistence ADR) |
| UI and API handlers | Call `TransitionService` only. They never set status directly. |

Diagram: `docs/architecture/request-status-state-machine` (CR-M2-01, section 3 model).

## 8. Risks and Revisit Triggers

- **Bypass risk:** code sets status without the service. Mitigation: PR review checklist and a test that fails on direct status writes where practical.
- **Over-simplification risk:** future states need per-state behaviour. **Revisit trigger:** if two or more operations must behave differently by state, reconsider the State pattern through a new ADR that supersedes this one. Do not edit this record silently.
- **Database dependency risk:** the atomic status + audit guarantee (NFR-004) depends on a correct transaction in the PostgreSQL data layer, and the app now depends on a reachable central database server. Mitigations: keep all database access behind `IRequestRepository`; prove the guarantee with one test against a real database (write status and audit rows, force a failure, confirm both roll back) and record the result here; keep connection details out of the repository (configuration or environment variables). Availability and hosting consequences belong in the persistence and deployment ADRs and the Risk Register.
- **Consistency risk:** table and state diagram drift apart. Mitigation: both reference CR-M2-01 and change together.

## 9. Traceability

| Item | Link |
|---|---|
| Requirements | FR-007, FR-009, FR-011, FR-015, NFR-004 (FR-001 for authorisation) |
| Drivers | FEC-02 maintainability, FEC-03 testability, FEC-01 traceability |
| Change record | CR-M2-01 |
| Related ADRs | ADR-02 (persistence: synchronous commit); ADR-05 (role-based authorization) |
| Risk Register | RISK-M2-05: lifecycle bypass / over-simplification |
| RTM columns to fill | Design/interface decision: ADR-04. Implementation evidence: `src/CivicConnect.Core/Lifecycle/` [branch: state-management]. Verification evidence: `tests/CivicConnect.Core.Tests/LifecycleTests.cs` [29 Core tests passed in Visual Studio]. |
| AI Usage Register | AI-M2-04 |

ADR-05 refines the authorization responsibility originally represented by the placeholder `ActorRole` check in this ADR. The lifecycle decision remains responsible for controlling state transitions; authorization is now owned by the dedicated authorization policy.


## 9a. Verification Plan

Unit tests (no database) cover FR-007, FR-009, FR-011, FR-015 and the transactional contract in NFR-004 through a fake repository:

| Test | Expected |
|---|---|
| All 16 (from, to) status pairs | Exactly 3 allowed, 13 refused |
| Assign, reject and resolve with required data | Accepted, one save |
| Reject without reason (empty or whitespace) | Refused, nothing saved |
| Resolve without note; assign without assignee | Refused, nothing saved |
| Resolve an unassigned request | Refused as invalid transition |
| Requester or manager attempts a transition | Refused as not authorised |
| Unknown request | Refused as not found |
| Repository reports a concurrent change | Reported as conflict |
| Repository throws during save | Exception propagates, no success reported |

Not covered by unit tests: real database transactions. That needs the database test described in section 8.

## 10. If This Decision Changes (Q10)

Artefacts that would need to change: the state diagram, `TransitionRules` and `TransitionService` code and tests, the RTM design/implementation/verification columns for FR-007/009/011/015, the Risk Register, the README module descriptions, and any related ADR that refers to this one.

## 11. Later Consequence

Authorization responsibility was separated into ADR-05. The lifecycle decision remains responsible for controlling state transitions; authorization is now owned by the dedicated authorization policy.

## 12. Review Record

| Reviewer | Date | Decision | Comments |
|---|---|---|---|
| Aidan | 2026-09-30 | Accepted | Reviewed ADR-04 and approved the lifecycle control decision. |
| Lethebe | 2026-09-30 | Accepted | Reviewed ADR-04 and approved the lifecycle control decision. |
| | | | |

## 13. References

- Refactoring.Guru (n.d.). *State*. [online] Available at: https://refactoring.guru/design-patterns/state [Accessed 30 September 2026].
- Embedded.com (n.d.). *Implementing finite state machines in embedded systems*. [online] Available at: https://www.embedded.com/?p=4442212 [Accessed 30 September 2026].
- A2 (team report). *Design Quality and Design Patterns*; *Persistence & Data Integrity*; Research-to-Decision Map.
