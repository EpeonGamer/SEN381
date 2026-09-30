# ADR-05 — Role-Based Request Visibility and Authorization

- **Status:** Accepted
- **Date:** 2026-09-30
- **Decision owners:** CivicConnect team
- **Related:** ADR-04 Request Lifecycle Control

## 1. Problem

CivicConnect has different visibility and operational permissions for Requesters, Staff and Managers. Requesters must only see their own requests, Staff belong to configurable groups whose permitted request types determine their visibility, and Managers have broader visibility and management operations.

If these rules are placed separately in endpoints, screens, lifecycle services and queries, a missed check can expose another user's request or allow an operation outside the actor's permissions. Changes to roles, staff groups or request types would also require authorization logic to be changed in multiple places, making the rules difficult to test consistently.

The design therefore needs one controlled server-side authorization responsibility that can produce a visibility scope for both list filtering and individual-request checks, while remaining small enough for the current project.

## 2. Decision

Create a dedicated `Authorization` area in `CivicConnect.Core`, separate from the `Lifecycle` area established by ADR-04.

Use a Strategy-based `IVisibilityPolicy` to translate an authenticated `ActorContext` into a `VisibilityScope`.

The current implementation uses `RoleVisibilityPolicy` as the Strategy:

- **Requester:** scope is limited to requests owned by the actor.
- **Staff:** scope contains the union of request types permitted by all of the actor's Staff groups.
- **Manager:** scope covers all requests currently represented by the Core policy.
- Authorization actions are also represented by the scope so lifecycle/application services do not hard-code role checks.

The scope can answer both:

1. whether an individual request is visible; and
2. which request scope a list/query layer should apply.

`TransitionService` therefore delegates the `ChangeStatus` authorization decision to the policy rather than directly checking `ActorRole`.

The existing `ActorRole` placeholder in `TransitionModels` is replaced by the shared `Authorization.ActorRole` and `ActorContext`.

## 3. Alternatives considered

| Alternative | Advantages | Disadvantages |
|---|---|---|
| Scattered role checks | Lowest immediate implementation cost | Rules can diverge between endpoints, lists and operations |
| ASP.NET Core authorization alone | Uses framework support for endpoint/resource authorization | Does not by itself define CivicConnect's list-visibility query scope |
| **Dedicated authorization + Strategy** | Central policy, reusable visibility scope, testable plain C#, supports changing groups/roles | Adds a small abstraction |
| Database row-level security | Strong database enforcement | Adds database-specific complexity beyond the current Core-only implementation |

The selected approach is deliberately proportionate to the current project. The M2 decision is an application of the research alternatives to CivicConnect rather than a requirement to reproduce an A2 recommendation.

## 4. Rationale

The primary reason for the decision is consistency between list visibility and individual-request authorization.

A simple boolean `CanView(actor, request)` would answer an individual-request question but would not naturally provide the scope needed by a request list. A `VisibilityScope` gives the policy one output that can support both uses.

Separating authorization from `Lifecycle` also preserves ADR-04's responsibility boundary: `TransitionService` controls valid state changes, while the authorization layer controls who is permitted to perform operations.

## 5. Expected benefit and complexity introduced

| Benefit | Complexity / trade-off |
|---|---|
| One authorization policy for role/group visibility | Introduces an additional policy abstraction |
| Staff groups combine naturally through set union | Group/request-type configuration is not yet persisted in this Core-only slice |
| Visibility scope can support both list and detail checks | Query integration will be required when the data/API layer is implemented |
| Lifecycle no longer owns role-specific authorization rules | Application services must use the authorization policy rather than bypassing it |
| Policy is independently unit-testable | More types than a simple role switch |

## 6. Affected components

| Component | Responsibility |
|---|---|
| `Authorization.ActorRole` | Shared role definition |
| `Authorization.ActorContext` | Authenticated actor identity, role and Staff groups |
| `Authorization.StaffGroup` | Configurable Staff group and permitted request types |
| `Authorization.RequestAccess` | Minimum request information needed for visibility checks |
| `Authorization.VisibilityScope` | Represents the actor's permitted request scope and actions |
| `Authorization.IVisibilityPolicy` | Strategy interface |
| `Authorization.RoleVisibilityPolicy` | Current role/group Strategy implementation |
| `Lifecycle.TransitionRequest` | Uses `ActorContext` instead of owning a duplicate role representation |
| `Lifecycle.TransitionService` | Delegates authorization to `IVisibilityPolicy` |
| `AuthorizationTests` | Verifies role/group visibility and action scope |
| `LifecycleTests` | Verifies lifecycle behaviour remains intact after authorization refactoring |

## 7. Scope and deferred work

This ADR establishes the Core authorization model. It does not implement:

- ASP.NET authentication/token issuance;
- database persistence of Staff groups;
- management UI for maintaining Staff accounts;
- final request-type catalogue;
- database/API query integration for list filtering.

Manager staff-account administration is explicitly deferred from this implementation.

When the API/data layer is implemented, list queries should consume the `VisibilityScope` rather than loading all requests and filtering them after retrieval.

## 8. Verification plan

Current unit verification covers:

- Requesters can view their own requests but not another requester's requests.
- Staff visibility is the union of the request types granted by all their groups.
- Staff cannot see a request type outside their groups.
- Managers have the broader request visibility scope.
- Manager scope does not grant the Staff lifecycle `ChangeStatus` action.
- Staff receive the `ChangeStatus` action through the policy.
- Existing lifecycle tests continue to verify that Requesters and Managers cannot change status and Staff can.

Future application-layer verification must also demonstrate that list queries and individual request endpoints consume the same scope.

## 9. Traceability

| Item | Link |
|---|---|
| Requirements | FR-001, FR-005, FR-006, FR-008, FR-012, FR-013, FR-014, NFR-005, NFR-007 |
| Drivers | FEC-01 Traceability, FEC-02 Maintainability, FEC-03 Testability, FEC-04 Security & Data Privacy |
| Related ADR | ADR-04 Request Lifecycle Control |
| Implementation | `src/CivicConnect.Core/Authorization/` and updated `Lifecycle/TransitionService.cs` |
| Verification | `tests/CivicConnect.Core.Tests/AuthorizationTests.cs` and updated `LifecycleTests.cs` |

## 10. Consequences if this decision changes

The `Authorization` contracts and Strategy implementation, affected lifecycle/application services, authorization tests, RTM design/implementation/verification evidence, and this ADR would need to be reviewed. The existing lifecycle decision in ADR-04 should not be silently rewritten; any change to its responsibility boundary should be recorded as a controlled change.

## 11. References

- SEN381 M2 Architecture, Technology & Initial Design Baseline, sections 5.6 and 9.
- CivicConnect A2 design-pattern research, as supporting research for the design-pattern alternatives.
- ADR-04 — Request Lifecycle Control.
