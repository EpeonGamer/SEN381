# 1. Project Engineering Document

> CivicConnect Public Service Reporting Platform
> **SEN381** *Integrated Team Project*

**Contributors:**

* Kasper van Niekerk (602622)
* Aidan Lakmeeharan (602899)
* Lethebe Kutloano (601712)

## 1.1. Contents

- [1. Project Engineering Document](#1-project-engineering-document)
  - [1.1. Contents](#11-contents)
  - [1.2. Change Control](#12-change-control)
  - [1.3. Executive Summary \& System Context](#13-executive-summary--system-context)
    - [1.3.1. System Context \& Problem Domain](#131-system-context--problem-domain)
  - [1.4. Business \& System Objectives](#14-business--system-objectives)
  - [1.5. Requirements Baseline (Functional \& Non-Functional)](#15-requirements-baseline-functional--non-functional)
    - [1.5.1. Functional Requirements](#151-functional-requirements)
    - [1.5.2. Non-Functional Requirements](#152-non-functional-requirements)
    - [1.5.3. Requirements Traceability Matrix (Initial)](#153-requirements-traceability-matrix-initial)
  - [1.6. Project Scope, Boundaries \& Constraints](#16-project-scope-boundaries--constraints)
    - [1.6.1. In Scope](#161-in-scope)
    - [1.6.2. Out of Scope](#162-out-of-scope)
    - [1.6.3. Deferred](#163-deferred)
    - [1.6.4. Scope Acceptance Rules](#164-scope-acceptance-rules)
    - [1.6.5. Project Constraints](#165-project-constraints)
    - [1.6.6. System Constraints](#166-system-constraints)
  - [1.7. Stakeholder Identification \& Governance](#17-stakeholder-identification--governance)
  - [1.8. Team Working Agreement](#18-team-working-agreement)
  - [1.9. Forward Engineering Considerations (FEC)](#19-forward-engineering-considerations-fec)
    - [1.9.1. FEC-01: Traceability](#191-fec-01-traceability)
    - [1.9.2. FEC-02: Maintainability](#192-fec-02-maintainability)
    - [1.9.3. FEC-03: Testability](#193-fec-03-testability)
    - [1.9.4. FEC-04: Security \& Data Privacy](#194-fec-04-security--data-privacy)
    - [1.9.5. FEC-05: Handoff](#195-fec-05-handoff)
  - [1.10. Risk Identification and Management](#110-risk-identification-and-management)
  - [1.11. Baseline Sign-Off \& Gate Evidence](#111-baseline-sign-off--gate-evidence)
  - [1.12. System Architecture \& Design](#112-system-architecture--design)
  - [1.13. Implementation \& Verification](#113-implementation--verification)
  - [1.14. Deployment, Operations \& SRE](#114-deployment-operations--sre)
  - [1.15. References \& Evidence Traceability](#115-references--evidence-traceability)

---

## 1.2. Change Control

| **Version** | **Date** | **Last Edit** | **By** | **Reviewers** |
|---|---|---|---|---|
| 0.0.1 | 2026/09/08 | Document structure | Kasper | Aidan, Lethebe |
| 0.1.0 | 2026/09/08 | Convert to .md | Kasper | Aidan, Lethebe |
| 0.2.0 | 2026/09/09 | Consolidated PED_1, PED_2 and Member One's requirements/scope work into a single controlled document; corrected duplicate/malformed requirement IDs; added missing FR-015; updated NFR-003 to 99.9% availability target | Kasper | Aidan, Lethebe |
| 0.3.0 | 2026/09/09 | Appended AI Usage Register (from `docs/AI_USAGE_1.md`) and Engineering Decision Log (from `docs/decisions/ENG_DECISION_LOG.md`) as controlled PED sections; renumbered downstream sections | Kasper | Aidan, Lethebe |
| 0.4.0 | 2026/09/09 | Added Team Working Agreement (draft) and Baseline Sign-Off & Gate Evidence (draft, per Master Brief Appendix D) as controlled PED sections; renumbered downstream sections | Kasper | Aidan, Lethebe |
| 0.5.0 | 2026/09/09 | Reconciled repository PED.md (which had reverted to uncorrected content) against this corrected/consolidated version; added dedicated Project Constraints subsection (§1.6.5) with cost/quality/security trade-off; finalised Team Working Agreement roles and communication plan | Kasper | Aidan, Lethebe |
| 1.0.0 | 2026/09/09 | Formal Milestone 1 Baseline Submission | Team | Aidan, Lethebe |


---

## 1.3. Executive Summary & System Context

The CivicConnect platform is a digital service request management system engineered to solve operational inefficiencies in community service reporting. Currently, service issues (e.g., municipal faults, facility damage, security concerns, and IT support) are handled through decentralized channels including WhatsApp, email, paper logs, and phone calls. This project establishes a centralized, auditable, and traceable platform that connects Requesters, Operational Staff, and Management.

Milestone 1 establishes the controlled engineering foundation, baselining functional and non-functional requirements, scope boundaries, stakeholder governance, and risk mitigation strategies under strict configuration control.

### 1.3.1. System Context & Problem Domain

* **Current Operational Deficiencies:**
  * *Duplication & Loss:* Requests submitted across informal channels are frequently lost, duplicated, or misassigned.
  * *Lack of Visibility:* Citizens have zero visibility into ticket progress, status transitions, or resolution timelines.
  * *Weak Accountability:* No formal audit record exists to track staff actions, ownership changes, or management oversight.
* **Proposed Solution Boundaries:** CivicConnect provides a unified platform for service requests; from initial categorization and assignment to controlled status updates and final closure.

---

## 1.4. Business & System Objectives

The business needs a digital platform that will allow service requests to be submitted, managed, monitored and reported in a much more reliable manner. The digital platform, CivicConnect, is intended to streamline the service request process for the business and improve operational efficiency; this is the intended value the system will provide for the business. It will act as a centralized control system for service requests.

Regarding the need for the system within the business, CivicConnect will strengthen and increase the reliability of service request operations, staff and management/oversight capabilities. Service requesters will have better visibility regarding the current state of submitted requests and be able to view a history of submitted requests; they will receive feedback regarding the state of their service request and be able to categorise a request using the controlled category mechanism feature. Staff will be able to sort and search for requests and view full request details, be able to assign or accept responsibility for a request, update request status and resolve those requests when authorised.

Management will be able to view service activity information, be given reliable information about requests in order to identify whether they are open, overdue, resolved or closed, and use the available information to support accountability and service lifecycle analysis.

**Intended business value:**

- Reduced risk of requests being duplicated, overlooked or lost
- Improved visibility of request status for service requesters
- Clear communicated responsibility and ownership for staff members
- Greater accountability
- More reliable information for management regarding state of work
- Improved protection of sensitive request information
- Streamlined service request lifecycle process

---

## 1.5. Requirements Baseline (Functional & Non-Functional)

### 1.5.1. Functional Requirements

| Requirement ID | Source | Description | Priority | Status | Acceptance criteria |
|---|---|---|---|---|---|
| FR-001 | System (role-based access, all stakeholders) | The system shall allow authorised requester to perform authorised functions, according to their role | MUST | Proposed | A requester can only see their requests and not others. Staff can only access features they are permitted. Management features are unavailable to requesters and staff |
| FR-002 | Requester | The system must allow authenticated requester to submit requests | MUST | Proposed | Submission form captures necessary service request details. When correct input is entered and submit is clicked, the system stores the service request linked to the requester |
| FR-003 | Requester | The system shall allow a requester to categorise a request | MUST | Proposed | The selected category is stored in correlation to the submitted service request |
| FR-004 | Requester | The system shall allow a requester to view status of their submitted requests | MUST | Proposed | The request status displays current status |
| FR-005 | Requester | The system shall allow requesters to view a history/list of their previously submitted requests | MUST | Proposed | The list shows service requests submitted by the requester and the relevant information. When no requests exist, an empty screen/history list is returned |
| FR-006 | Staff | The system shall allow authorised staff to search, filter and sort service requests | MUST | Proposed | Staff can search using matching data such as unique identifier for service request. Staff can filter by categories. Staff can sort by categories |
| FR-007 | Requester | The system shall provide meaningful feedback regarding the state of a request | MUST | Proposed | Each application feedback displays the relevant service request along with its status and reasoning for the status given |
| FR-008 | Staff | The system shall allow authorised staff to view full request details | MUST | Proposed | The system provides full service request details. A staff cannot view a request if they are not authorised for the request |
| FR-009 | Staff | The system shall allow an authorised staff to assign or accept responsibility for a request | MUST | Proposed | Responsibility is given by selecting another staff role. No other role but staff can assign/accept responsibility for a request |
| FR-010 | Staff | The system shall record material actions and comments associated with a request | MUST | Proposed | Comments will be saved as an entry with the author and description of the comment. Empty comments are rejected |
| FR-011 | Staff | The system shall allow authorised staff to resolve/close requests | MUST | Proposed | After a service has been resolved, the requester will be able to see the new status. Only a request in an approved state can be closed |
| FR-012 | Management | The system shall allow authorised managers to view service activity information | MUST | Proposed | The overview does not expose request information outside the manager's authorised scope |
| FR-013 | Management | The system shall allow management to identify open, overdue, resolved, and closed requests | MUST | Proposed | System provides managers with counts of open, overdue, resolved, closed requests |
| FR-014 | Management | The system shall allow management to view request information according to filters such as category/status | MUST | Proposed | System provides filter options for managers to select from |
| FR-015 | Staff | The system shall allow authorised staff to update a request through defined status transitions | MUST | Proposed | The system will only accept the following: Assigned, In Progress, Resolved, Closed |

### 1.5.2. Non-Functional Requirements

| Requirement ID | Source | Category | Description | Priority | Status | Verification |
|---|---|---|---|---|---|---|
| NFR-001 | Project Master Brief/projectmanagement.com | Performance | The system shall respond as intended during normal operational conditions | MUST | Proposed | Measured via load test |
| NFR-002 | Project Master Brief/projectmanagement.com | Security | The system should use HTTPS and use hashing to store passwords | MUST | Proposed | Inspect authentication configuration, pass when HTTPS is enforced and no plaintext passwords stored |
| NFR-003 | Project Master Brief/projectmanagement.com | Availability | The system must be able to achieve 99.9% availability | SHOULD | Proposed | Availability measurement |
| NFR-004 | Project Master Brief/projectmanagement.com | Reliability | A service request lifecycle will either save all request details or not when an error occurs during an assignment or status update, the database contains either the changed and completed result or the unchanged result | SHOULD | Proposed | Fault testing |
| NFR-005 | Project Master Brief/projectmanagement.com | Access control | The system shall prevent unauthorised and unauthenticated users from accessing service request information | MUST | Proposed | Security tests which confirm that protected endpoints reject unauthenticated users |
| NFR-006 | Project Master Brief/projectmanagement.com | Compatibility | The system shall operate on the latest stable versions available to the team | SHOULD | Proposed | Cross-browser testing |
| NFR-007 | Project Master Brief/projectmanagement.com | Data privacy | The system shall prevent requesters from viewing other requests that don't belong to them unless authorised | MUST | Proposed | Using two requester accounts and try to access other account's requests |
| NFR-008 | Project Master Brief/projectmanagement.com | Usability | Users should be allowed to complete service request functions without requiring any assistance | SHOULD | Proposed | Tested via completion rate and time taken to complete the service request process |

### 1.5.3. Requirements Traceability Matrix (Initial)

**Functional requirement traceability matrix:**

| Requirement | Source | Priority | Status | Verification method |
|---|---|---|---|---|
| FR-001 | Stakeholder | MUST | Proposed | Access-control tests for role-to-function and role-to-data rule |
| FR-002 | Stakeholder | MUST | Proposed | Tests for valid and invalid submission |
| FR-003 | Stakeholder | MUST | Proposed | Test for stored-data check regarding category selection |
| FR-004 | Stakeholder | MUST | Proposed | Authorisation tests for requester status view |
| FR-005 | Stakeholder | MUST | Proposed | Tests for populated and empty history states |
| FR-006 | Stakeholder | MUST | Proposed | Tests for search, filters, sorting |
| FR-007 | Stakeholder | MUST | Proposed | Tests for status feedback and mandatory rejection reason |
| FR-008 | Stakeholder | MUST | Proposed | Access-control tests for full detail view |
| FR-009 | Stakeholder | MUST | Proposed | Authorisation tests for assignment and acceptance |
| FR-010 | Stakeholder | MUST | Proposed | Tests covering recordings and recorded actions |
| FR-011 | Stakeholder | MUST | Proposed | Tests to resolve and close service requests |
| FR-012 | Stakeholder | MUST | Proposed | Management-view and authorisation tests |
| FR-013 | Stakeholder | MUST | Proposed | Tests conducted for identification of requests via request state |
| FR-014 | Stakeholder | MUST | Proposed | Filter-combination, reset and no-results tests |
| FR-015 | Stakeholder | MUST | Proposed | Tests for valid/invalid status-transition enforcement |

**Non-functional requirement traceability matrix:**

| Requirement | Source | Priority | Status | Verification method |
|---|---|---|---|---|
| NFR-001 | Stakeholder | MUST | Proposed | Load Testing |
| NFR-002 | Stakeholder | MUST | Proposed | Password storage and HTTPS inspection |
| NFR-003 | Stakeholder | SHOULD | Proposed | Monitoring testing and maintenance logs |
| NFR-004 | Stakeholder | SHOULD | Proposed | Fault testing |
| NFR-005 | Stakeholder | MUST | Proposed | Authorisation security tests |
| NFR-006 | Stakeholder | SHOULD | Proposed | Access control tests |
| NFR-007 | Stakeholder | MUST | Proposed | URL and API testing across two accounts |
| NFR-008 | Stakeholder | SHOULD | Proposed | Completion rate and time taken to complete service request tests |

---

## 1.6. Project Scope, Boundaries & Constraints

The scope of CivicConnect is to provide a digital platform that the business will use to submit, manage and report on service requests in a more reliable manner. It prioritises the reliable management and traceability of service requests over other features. The scope is needed to ensure that stakeholder needs remain achievable within project constraints and to provide enough detail to guide team members through the project lifecycle.

### 1.6.1. In Scope

**Requester capabilities:**
- Submitting a new request with appropriate information
- Ability to categorise a request
- Viewing the status of submitted requests
- Viewing history of submitted requests
- Receive feedback regarding request status (accepted, rejected, updated or completed)
- Viewing only request information that the requester has access to

**Staff capabilities:**
- Viewing service requests relevant to the authorised staff
- Searching, filtering or sorting actions on requests
- Viewing full request details
- Assigning or accepting responsibility for a request
- Ability to update request status
- Recording relevant information, actions or comments
- Resolving or closing requests where authorised

**Management and oversight capabilities:**
- Viewing service activity information
- Identify requests according to states (open, overdue, resolved, and closed)
- Viewing request information by selected filters: category and status
- Accessing enough information to support accountability and service-performance analysis

**System capabilities:**
- User access and authorization based on roles and responsibilities
- Validation of service request input
- Storage of request details, ownership, status and lifecycle history
- Protection of sensitive information
- Show each requester their own request list, current status and feedback

### 1.6.2. Out of Scope

- Creation of a Native Android or iOS mobile application
- Importing of requests from external platforms/systems such as WhatsApp, email, telephone systems, etc.
- Payment functions/financial transactions
- Inventory control, asset management functions of the service requests
- Functionality unrelated to service-request lifecycle
- Purchase or installation of hardware
- Failure recovery. CivicConnect is responsible for ensuring reliable delivery of the service request lifecycle; it will not perform any maintenance should a failure occur.

### 1.6.3. Deferred

- Integration with existing external systems
- Notification system regarding the state of service request
- Requester satisfaction, making use of feedback services for evaluations
- Exporting reports to text-document formats
- Automatic escalation of overdue service requests
- Configurable management dashboards

**Deliberate exclusion:** Importing from WhatsApp, Email, telephone systems, etc.

Focus will be purely emphasized upon the service request lifecycle. Having importing from WhatsApp, email, and other platforms might introduce integration complexity and additional testing, shifting the focus from the core feature expressed to ensuring that the importing works. Whilst the importing will remove manual data capture, it still is an uncertainty to include within the system.

### 1.6.4. Scope Acceptance Rules

- In-scope, out-of-scope and deferred scope items must not contradict each other.
- Every requirement appears within the RTM.
- The team must confirm that the committed scope is feasible within the upcoming and available milestones.
- Stakeholders and team members review documentation and approvals are recorded.
- Every in-scope feature must possess a unique requirement identifier.
- Requirement and scope identifiers must remain stable across milestones.
- The team must use a change request and impact analysis before altering any scope/requirement items.
- A MUST requirement belongs to the committed baseline unless formally altered by the team.
- A SHOULD requirement may be deferred through documented impact analysis.
- A COULD requirement is optional and must not replace a MUST requirement.

### 1.6.5. Project Constraints

| Constraint | Minimum Expectation for CivicConnect | Engineering Implication |
|---|---|---|
| Team size | Exactly 3 registered students (Kasper, Aidan, Lethebe). | Individual accountability evidence (§1.8, §1.11) must be traceable per member, not only per team. |
| Schedule | Four formal milestones across the SEN381 delivery period; M1 baseline due before M2 architecture work begins. | Technology-stack and architecture decisions are deliberately deferred to M2 (DEC-M1-02, §1.12) rather than rushed now. |
| Cost | Prefer free/low-cost services; operational cost beyond the educational context must be identified. | Directly drives RISK-002 (§1.10); any paid service/technology requires a cost check and Decision Log entry before adoption. |
| Scope | Baselined scope (§1.6.1–1.6.3) is controlled; changes require impact analysis, not silent absorption. | The deliberate WhatsApp/email-import exclusion (§1.6.3) is the first tested case of this control. |
| Quality | Quality attributes must be measurable, not asserted (NFR-001–NFR-008, §1.5.2). | Claims like "the system is reliable" are insufficient without the linked test evidence required from M3 onward. |
| Security | Security is lifecycle-wide, not a final add-on. | FEC-04 (§1.9) and NFR-002/NFR-005/NFR-007 (§1.5.2) commit the team to access control, HTTPS, and data-privacy requirements from the requirements stage, before any code exists. |
| Technology | No stack is prescribed; selection must be justified against requirements, team capability, and constraints. | Formally deferred to M2 via DEC-M1-02 (§1.12), informed by the FR/NFR baseline above. |

**Interaction/trade-off worth noting:** the Cost constraint and the Quality/Security constraints pull in opposite directions — preferring free-tier services (Cost) can mean weaker built-in security or reliability guarantees (Quality, Security, RISK-001/RISK-004), so free-tier selections in M2 will need an explicit check against NFR-002, NFR-005 and NFR-007 rather than being chosen on price alone.

---

### 1.6.6. System Constraints

**Schedule:** The schedule of the project follows four milestones, one of which is the submission of this section. The final milestone will be submitted by the 18th of October.

**Cost:** The project should have minimal costs, that do not financially burden the community or business. Free software should be used where possible.

**Scope:** Functional requirements outlined earlier in the document need to be adhered to, non-functional requirements can be deferred to another time if necessary. Costs need to be respected, the project must be completed on time, along with all quality requirements being met.

**Quality:** The project needs to be functional, reliable, stable, usable by the majority of participants, and allow for potential improvements.

**Security:** Data needs to be kept secure, backed up, relevant and reliable. Should there be any data risks, systems such as backups need to be kept up to date.

---

## 1.7. Stakeholder Identification & Governance

**Stakeholders:**

**Staff:** Reporting staff working with CivicConnect have a stake in the project as they are the ones who are going to be using the system. Their interests and feedback will have significant contributions to the project with regards to its user interface, accessibility, as well as the product testing.

**Contractors:** Any service requests, requirements, payments and communications for contractors go through CivicConnect. As such, any potential contractor would have a stake in the project. However, it would be difficult to consult with potential contractors as they can change with each project and dependent on their tenders. They will therefore be considered, however are not significant stakeholders in the project.

**Communal Users:** The community that uses CivicConnect will want to understand how the software works, and their input for the development of CivicConnect will be required to allow for a functional and satisfactory product upon completion.

**Business and Financial Stakeholders:** The business, as well as its and any other financial backers, will want to ensure that they get value out of the product they are investing in, as well as a return on their investment. As such they have a significant stake and interest in a successful outcome of the project.

**Additional parties of interest:**

**Government Regulatory Bodies:** Regulatory Bodies would want to ensure that CivicConnect complies with code requirements and regulations. As such, the development team needs to ensure that any potential regulatory requirements are analysed and adhered to within the project scope.

**SARS:** The South African Revenue Service will want financial reports with respect to the project, as well as potential information relating to audits. This is both to ensure lawful compliance, as well as required taxes that can arise from the product's potential profits or losses.

---

## 1.8. Team Working Agreement

**Team members:** Kasper van Niekerk (602622), Aidan Lakmeeharan (602899), Lethebe Kutloano (601712)

**Purpose:** This agreement records how the team will collaborate, communicate, take decisions and hold each other accountable across the CivicConnect project, in line with Master Project Brief §8 (Team Engineering and Individual Accountability).

**Collaboration & communication**
- Primary communication channel: team WhatsApp group for day-to-day coordination; formal decisions and substantive discussion recorded via GitHub Issues/PRs, not left only in chat.
- Standing check-in cadence: weekly sync ahead of each milestone deadline, plus ad hoc check-ins as needed.
- All substantive engineering discussion and decisions that affect the baseline must be reflected in a controlled artefact (Decision Log, PR, or issue); not left only in chat.

**Roles (may be adjusted per milestone, but do not remove collective responsibility per §8)**
- Requirements & Documentation Lead — Kasper
- Architecture/Backend Lead — Aidan
- Quality & Risk Lead — Lethebe
- Roles rotate or share workload as needed; every member remains individually responsible for understanding the complete project per Master Brief §8.

**Decision-making**
- Minor/technical decisions within an individual's assigned area may proceed without full team sign-off, but must be logged in the Decision Log.
- Decisions affecting baselined scope, requirements, architecture or shared infrastructure require agreement from all three members before being actioned.
- Disagreements that cannot be resolved within the team will be raised with the lecturer rather than left unresolved.

**GitHub & review conduct**
- Every substantive change to controlled artefacts (documentation or code) goes through a Pull Request; no direct commits to `main`.
- Minimum of two approvals from members other than the author, per Master Brief §9. Self-approval is not accepted.
- Reviewers are expected to leave meaningful comments (see Master Brief §9.1), not rubber-stamp approvals.

**Accountability & conduct**
- Each member commits to producing the minimum individual evidence required by Master Brief §8.1 (meaningful commits, PRs authored and reviewed, contribution to decision records, ability to trace and defend at least one requirement).
- A member unable to meet a commitment must raise this with the team as early as possible, not at the deadline.
- All members are expected at every milestone presentation and individual defence per Master Brief §19; attendance is not optional and is not substitutable by another member's contribution.

**Conflict handling**
- Concerns about workload distribution or contribution imbalance will be raised directly and early within the team.
- If unresolved, the concern will be escalated to the lecturer before it affects milestone delivery.

**Agreement**

| Member | Agreed (Y/N) | Date |
|---|---|---|
| Kasper van Niekerk | Y | 2026/09/09 |
| Aidan Lakmeeharan | Y | 2026/09/09 |
| Lethebe Kutloano | Y | 2026/09/09 |

---

## 1.9. Forward Engineering Considerations (FEC)

### 1.9.1. FEC-01: Traceability

* **Planning Influence**: Facilitating tracking and auditing service requests (required for accountability) will require specific data solutions and transaction management, directly impacting technology and architecture choice.
* **Future Influence**: Data structure, storage policy, and audit log design.
* **Information Needs**: Log retention timeline, compliant audit attributes to be collected.
* **Risk If Ignored**: Poor management capabilities as requests cannot be linked to the agents that changed them, causing errors to compound as they cannot be monitored to be detected. Legal trouble when services cannot be linked to requests via audit.

### 1.9.2. FEC-02: Maintainability

* **Planning Influence**: Designing to facilitate future code changes and ease of maintenance requires modular, loosely coupled code that directly influences architecture design. It will also guide code structures and documentation requirements from the start, and so must be planned for early.
* **Future Influence**: Code structure, backend stack options, documentation, module design, coupling, API format.
* **Information Needs**: Stack choice, hand-off requirements, final software lifetime and maintenance scope.
* **Risk If Ignored**: If the team doesn't explicitly develop with maintainability in mind then systems will be tightly coupled and individual services cannot be updated. Instead time and costs increase as the ecosystem must be overhauled to suit small changes, which is unacceptable.

### 1.9.3. FEC-03: Testability

* **Planning Influence**: A testable system requires isolated components with independent functionality that can be separately verified so a nested failure doesn't go unnoticed and cause cascading failures. This will directly impact coding strategy and validation processes, and should be accounted for before moving to the design phase.
* **Future Influence**: Component scope and communication, verification process, test suites.
* **Information Needs**: Component granularity, service complexity, code coverage goals, and the selected testing tools/frameworks.
* **Risk If Ignored**: Even a well designed system cannot be analysed and corrected if it cannot be properly tested. Without proper testability architecture in place, errors will accumulate and undermine the functionality and reliability of the system, including security.

### 1.9.4. FEC-04: Security & Data Privacy

* **Planning Influence**: Security will directly impact the system approach to ensure role-based access is enforced at every step of the program, while data privacy similarly affects data collection and retention considerations, thereby impacting both the frontend and backend planning.
* **Future Influence**: User account management, verification, recovery strategies, encryption, backup management, role management.
* **Information Needs**: Data retention regulations, privacy regulations, required user information, variation of roles required, chosen stack encryption support.
* **Risk If Ignored**: Data loss or leak that can lead to system interruptions, failure, and legal action. Account failures leading to unauthorized actions or unrecoverable roles.

### 1.9.5. FEC-05: Handoff

* **Planning Influence**: The final interface should be operable by non-technical municipal staff. Onboarding, user roles, system administration, and help documentation must be planned early so the system doesn't rely on developers for day-to-day use.
* **Future Influence**: Admin UI design, user role management, system setting controls, error messages, and admin/user documentation.
* **Information Needs**: Target admin skill level, required system settings (like ticket categories and department routing), and training documentation scope.
* **Risk If Ignored**: Non-technical staff won't be able to run the system, change settings, or onboard new agents without developer intervention. This leads to mismanaged requests, system misuse, and high maintenance overhead.

---

## 1.10. Risk Identification and Management

| Risk ID | Description | Cause | Probability | Impact | Priority | Mitigation | Contingency | Owner | Status |
|---|---|---|---|---|---|---|---|---|---|
| RISK-001 | Sensitive request/personal data is exposed, leaked, or accessed without authorisation | Insufficient access control, weak authentication, unencrypted storage/transit | Medium | High | High | Enforce role-based access control, encryption in transit/at rest, two-factor authentication for staff/management accounts | Revoke affected credentials, notify affected stakeholders, review access logs | Team (Security) | Open |
| RISK-002 | Project cost exceeds the team's available/educational budget | Reliance on paid technologies, subscriptions, or services not evaluated for cost before adoption | Medium | Medium | Medium | Investigate and document cost of any new technology/service before adoption; prefer free/low-cost tiers where practical | Substitute the technology/service with a lower-cost equivalent; defer the feature | Team | Open |
| RISK-003 | Delivered software fails to meet baselined quality/functional standards | Technology limitations, incomplete testing, or inconsistent individual contribution | Medium | High | High | Stage-gate review of each milestone's artefacts against acceptance criteria before sign-off | Revise/rework the affected feature before the next milestone; escalate to lecturer if capability gap is the cause | Team | Open |
| RISK-004 | System is unavailable or degrades during peak usage, causing service disruption | Latency, connectivity issues, power outages, insufficient hosting capacity | Low | Medium | Medium | Select hosting with acceptable uptime guarantees; monitor performance; plan for redundant/backup capacity where cost allows | Manual fallback communication channel; restore from latest backup | Team | Open |

*(Risk content carried forward from the M1 draft register; probability/impact/priority ratings are the team's initial estimate and should be reviewed and justified as a team before baseline sign-off.)*

---

## 1.11. Baseline Sign-Off & Gate Evidence

Based on Master Project Brief Appendix D.

**Project:** CivicConnect

**Baseline type:** Milestone 1; Engineering Foundation & Requirements Baseline (PED v1.0)

**Version:** 1.0.0

**Date:** 2026-09-09

| Check | Status |
|---|---|
| Scope reviewed | YES; in-scope/out-of-scope/deferred baseline complete. |
| Requirements/traceability checked | YES; 15 FRs, 8 NFRs, and FR/NFR traceability matrices complete with consistent IDs. |
| Risk review completed | YES; initial Risk Register with 4 prioritised risks, mitigation and contingency. |
| Repository/governance controls checked | YES |

**Outcome:** ACCEPTED

**Reviewers:** Kasper, Aidan, Lethebe

---

## 1.12. System Architecture & Design

*(To be baselined in Milestone 2)*

---

## 1.13. Implementation & Verification

*(To be baselined in Milestone 3)*

---

## 1.14. Deployment, Operations & SRE

*(To be baselined in Milestone 4)*

---

## 1.15. References & Evidence Traceability

* **SEN381 Master Project Brief v1.0**
* **SEN381 Milestone 1 Brief v1.0**
* Indeed Editorial Team. Business value. Available at: https://www.indeed.com/career-advice/career-development/business-value (Accessed 9 September 2026).
* ProjectManager. How to write a project scope statement. Available at: https://www.projectmanager.com/blog/project-scope-statement (Accessed 9 September 2026).
* Project Management Academy. Project scope statement. Available at: https://projectmanagementacademy.net/resources/blog/project-scope-statement-pmp/ (Accessed 9 September 2026).
* ProjectManager. Acceptance criteria in project management. Available at: https://www.projectmanager.com/blog/acceptance-criteria-project-management (Accessed 9 September 2026).
* Project-Management.com. Requirements traceability matrix. Available at: https://project-management.com/requirements-traceability-matrix-rtm/ (Accessed 9 September 2026).
