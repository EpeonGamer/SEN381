# CR-M2-01: Request Status Model Simplification and Clarification

**Suggested repo location:** `docs/change/CR-M2-01.md`
**Status:** DRAFT: awaiting review by two team members other than the author

## 1. Change Request Record (Master Brief Appendix E)

| Field | Entry |
|---|---|
| Change ID | CR-M2-01 |
| Requested by | Kasper, Member 3 (Design Patterns, Quality & Governance) |
| Date | 2026-09-30 |
| Baseline affected | PED v1.0.0 (Milestone 1 Requirements Baseline) |
| Requested change | Replace the inconsistent status wording in the baseline with a minimal four-status model: Unassigned, Assigned, Rejected, Resolved. See section 3. |
| Reason / expected value | The baseline describes the request lifecycle inconsistently (section 2). A minimal model is testable, cheap to implement and proportionate to a 3-person team with a fixed deadline. Fewer states mean fewer transitions to build, test and defend. |
| Requirements affected | FR-015 (amended), FR-011 (clarified), FR-013 (amended), FR-007 (clarified), FR-009 (clarified), FR-002 (initial status stated), FR-004/005/014 (status values displayed and filtered). RTM rows for these requirements. |
| Architecture/design affected | Request lifecycle model and the design decision for controlling status transitions (ADR to follow). No technology assumptions are made. |
| UI/API/data affected | Status becomes a controlled set of four values. A mandatory reason is recorded on rejection. Each transition produces an audit entry (FR-010, NFR-004). |
| Security/privacy impact | None negative. Transitions remain restricted to authorised staff (FR-001, FR-009, FR-011). |
| Quality/testing impact | Tests for every valid and invalid transition and for the mandatory rejection reason. Fewer transitions than the original model, so a smaller test set. |
| Scope impact | **Minor reduction of baselined MUST scope.** The baseline distinguishes "In Progress" from "Assigned" and "Closed" from "Resolved". This change removes both distinctions and therefore requires explicit team approval. Reopening and automatic escalation remain deferred (PED 1.6.3). |
| Schedule/resource impact | Low: documentation and RTM updates only. Implementation effort is reduced. |
| Cost impact | None. |
| Risk impact | Reduces requirements-ambiguity risk. Introduces a reporting-granularity risk (section 6). |
| Recommendation | ACCEPT, subject to team approval of the scope reduction |
| Approval / rationale | [Pending. Record reviewer names, dates and decision here after review.] |

## 2. Problem Found in the M1 Baseline

1. **No initial status.** FR-002 stores a submitted request, but no status is defined for a new request. FR-015 lists only Assigned, In Progress, Resolved and Closed.
2. **Rejection is referenced but not modelled.** PED 1.6.1 lists feedback on requests that are "accepted, rejected, updated or completed". FR-007's acceptance criteria refer to the reasoning for the status, and the RTM verification for FR-007 mentions a mandatory rejection reason. FR-015 has no Rejected status.
3. **"Approved state" is undefined.** FR-011 says only a request in an approved state can be closed, but no definition exists.
4. **Transition rules are implicit.** FR-015 lists values but not which status may follow which, so valid and invalid transition tests cannot be written.
5. **Overlapping states.** In Progress and Assigned, and Resolved and Closed, add states without distinct behaviour in the baseline requirements.

## 3. Proposed Status Model

**Statuses:**

| Status | Meaning |
|---|---|
| Unassigned | Initial status on submission. No staff member has accepted responsibility. |
| Assigned | A staff member owns the request. Assignment implies work is under way. |
| Rejected | Staff rejected the request. A reason is mandatory. Terminal. |
| Resolved | Work is complete. This status is also the closed state. Terminal. |

**Allowed transitions:**

| From | To | Trigger | Requirement | Conditions |
|---|---|---|---|---|
| (new request) | Unassigned | Requester submits | FR-002 | Valid input |
| Unassigned | Assigned | Staff assigns or accepts responsibility | FR-009 | Authorised staff only |
| Unassigned | Rejected | Staff rejects request | FR-007, FR-015 | Authorised staff; reason mandatory |
| Assigned | Resolved | Staff resolves and closes | FR-011, FR-010 | Authorised staff; resolution information recorded |

**Definition of "approved state" (FR-011):** a request may be resolved (closed) only when its status is **Assigned**.

**Rules:**
- Any transition not listed is rejected. Status and stored data remain unchanged.
- Reassigning a request to another staff member changes ownership but not status.
- Rejection is only possible before assignment. Rejecting an already assigned request is deliberately excluded for now.
- Every accepted transition produces an audit record in the same atomic operation (NFR-004, FEC-01).
- "Overdue" is not a stored status. It is derived from request age or a due-date rule. That rule is not yet defined and is recorded as an open decision.

## 4. Baseline Preservation and Amended Wording

Original wording is retained. It is not deleted from PED v1.0.0.

| Requirement | Original (PED v1.0.0) | Proposed (PED v2.0) | Status |
|---|---|---|---|
| FR-015 | "The system shall allow authorised staff to update a request through defined status transitions." Acceptance: "The system will only accept the following: Assigned, In Progress, Resolved, Closed." | "The system shall allow authorised staff to update a request only through the defined status transitions in CR-M2-01 section 3." Acceptance: "Every listed transition succeeds for authorised staff. Any unlisted transition is rejected and leaves the request unchanged. A rejection without a reason is refused." Original statuses In Progress and Closed are superseded by Assigned and Resolved. | Changed (CR-M2-01) |
| FR-011 | "...Only a request in an approved state can be closed." | "...Only a request with status Assigned can be resolved (closed)." | Clarified (CR-M2-01) |
| FR-013 | "...identify open, overdue, resolved, and closed requests." Acceptance: counts of open, overdue, resolved, closed requests. | "...identify open (Unassigned or Assigned), overdue, and resolved (closed) requests." Resolved and closed are now one count. | Changed (CR-M2-01) |
| FR-007 | Acceptance references "reasoning for the status given." | Wording unchanged. Rejected status exists and its reason is mandatory. | Clarified (CR-M2-01) |
| FR-002 | Stores the request linked to the requester. | Adds: stored request has initial status Unassigned. | Clarified (CR-M2-01) |

## 5. Artefacts to Update When Approved

- [ ] PED v2.0 Change Control table: add version rows. Do not remove v1.0.0.
- [ ] PED requirements tables: mark FR-015 and FR-013 as Changed, and FR-011, FR-007, FR-002 as Clarified.
- [ ] RTM: update status and verification columns for the affected rows.
- [ ] Risk Register: add rows for requirements ambiguity (mitigated by this CR) and reporting granularity (section 6).
- [ ] State-machine diagram: draw the section 3 model. This is the diagram input to the lifecycle design ADR.
- [ ] AI Usage Register: record any AI assistance used to draft this change request, with what was verified.

## 6. Trade-offs and Risks Introduced

| Removed distinction | Benefit | Cost / risk | Mitigation |
|---|---|---|---|
| In Progress merged into Assigned | Fewer states and transitions. | Management cannot tell "assigned, not started" from "actively being worked". Requesters see less detail on progress. | Audit log records assignment and comments (FR-010). Reinstating In Progress later is a controlled change. |
| Closed merged into Resolved | Simpler lifecycle. One terminal success state. | FR-013 loses the resolved/closed distinction. No separate step to confirm or finalise a resolution. | Explicit approval of the FR-013 change. Record a resolution timestamp so reporting can still analyse completed work. |
| Rejection only from Unassigned | Simple rule. | A request found invalid after assignment cannot be rejected. | Revisit if the team or a stakeholder needs it (controlled change). |

## 7. Review Record

| Reviewer | Date | Decision | Comments |
|---|---|---|---|
| | | | |
| | | | |
