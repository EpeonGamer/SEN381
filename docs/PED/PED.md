# Project Engineering Document

> CivicConnect Public Service Reporting Platform  
> **SEN381** *Final Engineering Project*

**Contributors:**

* Kasper van Niekerk (602622)
* Aidan Lakmeeharan (602899)
* Lethebe Kutloano (601712)

## Contents

- [Project Engineering Document](#project-engineering-document)
  - [Contents](#contents)
  - [Change Control](#change-control)
  - [Executive Summary \& System Context](#executive-summary--system-context)
    - [System Context \& Problem Domain](#system-context--problem-domain)
  - [1. Business \& System Objectives](#1-business--system-objectives)
  - [2. Requirements Baseline (Functional \& Non-Functional)](#2-requirements-baseline-functional--non-functional)
  - [3. Project Scope, Boundaries \& Constraints](#3-project-scope-boundaries--constraints)
  - [4. Stakeholder Identification \& Governance](#4-stakeholder-identification--governance)
  - [5. Forward Engineering Considerations (FEC)](#5-forward-engineering-considerations-fec)
  - [6. System Architecture \& Design](#6-system-architecture--design)
  - [7. Implementation \& Verification](#7-implementation--verification)
  - [8. Deployment, Operations \& SRE](#8-deployment-operations--sre)
  - [References \& Evidence Traceability](#references--evidence-traceability)

---

## Change Control

|**Version**|**Date**|**Last Edit**|**By**|**Reviewers**|
|---|---|---|---|---|
|0.0.1|2026/09/08|Document structure|Kasper|Aidan, Lethebe|
|0.1.0|2026/09/08|Convert to .md|Kasper|NONE|
|1.0.0|*pending*|Formal Milestone 1 Baseline Submission.|Team|NONE|

---

## Executive Summary & System Context

The CivicConnect platform is a digital service request management system engineered to solve operational inefficiencies in community service reporting. Currently, service issues (e.g., municipal faults, facility damage, security concerns, and IT support) are handled through decentralized channels including WhatsApp, email, paper logs, and phone calls. This project establishes a centralized, auditable, and traceable platform that connects Requesters, Operational Staff, and Management. 

Milestone 1 establishes the controlled engineering foundation, baselining functional and non-functional requirements, scope boundaries, stakeholder governance, and risk mitigation strategies under strict configuration control.

### System Context & Problem Domain

* **Current Operational Deficiencies:** 
  * *Duplication & Loss:* Requests submitted across informal channels are frequently lost, duplicated, or misassigned.
  * *Lack of Visibility:* Citizens have zero visibility into ticket progress, status transitions, or resolution timelines.
  * *Weak Accountability:* No formal audit record exists to track staff actions, ownership changes, or management oversight.
* **Proposed Solution Boundaries:** CivicConnect provides a unified platform for service requests; from initial categorization and assignment to controlled status updates and final closure.

---

## 1. Business & System Objectives

Problem statement: 

An organization manages its service requests through a combination of emails, telephone calls, WhatsApp messages, spreadsheets and paper-based controls. Controlling and managing service requests in this manner has proven to create a vast number of problems for the organization. The whole service management lifecycle is disconnected and uncontrolled.

The approach used by the business can be categorized as a more manual process when dealing with business operations, this introduces recurring problems such as requests getting duplicated or mismanaged between channels,  the staff having difficulty prioritising requests, identifying ownership and coordinating work, there is weak accountability for changes to request status and actions taken, reporting being manual, inconsistent and difficult to audit and there is no single controlled record of the lifecycle of a service request. 

All these problems affecting staff also create problems for the people using the system, management having limited reliable information about outstanding, overdue and resolved work causes the requesters to have limited visibility of the flow of requests: whether or not a request was received, assigned, delayed, resolved or closed.  And lastly, sensitive request information may be handled inconsistently across informal communication channels. 
The central problem that stands and is present is the process of service requests being unreliable and not controlled throughout the lifecycle. 

Business need: 
The business needs a digital platform that will allow service requests to be submitted, managed, monitored and reported in a much more reliable manner. The digital platform CivicConnect, is intended to streamline the service request process for the business and improve operational efficiency, this is the intended value that the system will provide for the business. It will act as a centralized control system for service requests. 

Regarding the need for the system within the business, CivicConnect will strengthen and increase the reliability of service request operations, staff and management/oversight capabilities. Service requesters will have better visibility regarding the current state of submitted requests and be able to view a history of submitted requests, they will receive feedback regarding the state of their service request and be able to categorise a request by use of the controlled category mechanism feature. Staff will be able to sort and search for requests and view full request details, be able to assign or accept a responsibility for a request, update request status and resolve those requests when authorised. 

Management will be able to view service activity information, be given reliable information about requests to be able to identify whether it is open, overdue, resolved or closed, and review request information and use the available information to support accountability and service lifecycle analysis. 

Intended business value: 
-	Reduced risk of requests being duplicated, overlooked or lost 
-	Improved visibility of request status for service requesters 
-	Clear communicated responsibility and ownership for staff members 
-	Greater accountability 
-	More reliable information for management regarding state of work 
-	Improved protection of sensitive request information 
-	Streamlined service request lifecycle process 


---

## 2. Requirements Baseline (Functional & Non-Functional)
## Functional requirements

| Requirement ID | Source | Description | Priority | Status | Acceptance criteria |
| --- | --- | --- | --- | --- | --- |
| FR-001 | Project Master brief | The system shall allow authorised requester to perform authorised functions, according to their role | MUST | Proposed | A requester can only see their requests and not others.<br><br>Staff can only access features can function they are permitted.<br><br>Management features are unavailable to requesters and staff. |
| FR-002 | Project Master brief | The system must allow authenticated requester to submit requests | MUST | Proposed | Submission form captures necessary service request details.<br><br>When correct input is entered and submit is clicked, the system stores the service request linked to the requester. |
| FR-003 | Project Master brief | The system shall allow a requester to categorise a request | MUST | Proposed | The selected category is stored in correlation to the submitted service request. |
| FR-004 | Project Master brief | The system shall allow a requester to view status of their submitted requests | MUST | Proposed | The request status displays current status. |
| FR-005 | Project Master brief | The system shall allow requesters to view a history/list of their previously submitted requests | MUST | Proposed | The list shows service requests submitted by the requester and the relevant information.<br><br>When no requests exist, an empty screen/history list is returned. |
| FR-006 | Project Master brief | The system shall allow authorised staff to search, filter and sort service requests | MUST | Proposed | Staff can search using matching data such as unique identifier for service request.<br><br>Staff can filter by categories.<br><br>Staff can sort by categories. |
| FR-007 | Project Master brief | The system shall provide meaningful feedback regarding the state of a request | MUST | Proposed | Each application feedback displays the relevant service request along with its status and reasoning for the status given. |
| FR-008 | Project Master brief | The system shall allow authorised staff to view full request details | MUST | Proposed | The system provides full service request details.<br><br>A staff cannot view a request if they are not authorised for the request. |
| FR-009 | Project Master brief | The system shall allow an authorised staff to assign or accept responsibility for a request | MUST | Proposed | Responsibility is given by selecting another staff role.<br><br>No other role but staff can assign/accept responsibility for a request. |
| FR-009 | Project Master brief | The system shall allow authorised staff to update a request through defined status transactions | MUST | Proposed | The system will only accept the following: Assigned, In Progress, Resolved, Closed. |
| FR-010 | Project Master brief | The system shall record material actions and comments associated with a request | MUST | Proposed | Comments will be saved as an entry with the author and description of the comment.<br><br>Empty comments are rejected. |
| FR-0011 | Project Master brief | The system shall allow authorised staff to resolve/close requests | MUST | Proposed | After a service has been resolved, the requester will be able to see the new status.<br><br>Only a request in an approved state can be closed. |
| FR-012 | Project Master brief | The system shall allow authorised managers to view service activity information | MUST | Proposed | The overview does not expose request information outside the manager's authorised scope. |
| FR-013 | Project Master brief | The system shall allow management to identify open, overdue, resolved, and closed requests | MUST | Proposed | System provides managers with counts of open, overdue, resolved, closed requests. |
| FR-014 | Project Master brief | The system shall allow management to view request information according to filters such as category/status | MUST | Proposed | System provides filter options for managers to select from. |

## Non-functional requirements

| Requirement ID | Source | Category | Description | Priority | Status | Acceptance criteria |
| --- | --- | --- | --- | --- | --- | --- |
| NFR-001 | Project Master Brief / projectmanagement.com | Performance | The system shall responds as intended during normal operational conditions | MUST | Proposed | Measured via load test. |
| NFR-002 | Project Master Brief / projectmanagement.com | Security | The system should use HTTPS and use hashing to store passwords | MUST | Proposed | Inspect authentication configuration,<br><br>pass when HTTPS is enforced and no plaintext passwords stored. |
| NFR-003 | Project Master Brief / projectmanagement.com | Availability | The system must be able to achieve 90% availability | SHOULD | Proposed | Availability measurement. |
| NFR-004 | Project Master Brief / projectmanagement.com | Reliability | A service request lifecycle will either save all request details or not when an error occurs | SHOULD | Proposed | When there is an application failure during request creation, assignment or status update, the database contains either the changed and completed result or the unchanged result. |
| NFR-005 | Project Master Brief / projectmanagement.com | Access control | The system shall prevent unauthorised and unauthenticated users from accessing service request information | MUST | Proposed | Security tests which confirm that protected endpoints reject unauthenticated users. |
| NFR-006 | Project Master Brief / projectmanagement.com | Compatibility | The system shall operate on the latest stable versions available to the team | SHOULD | Proposed | Cross-browser testing. |
| NFR-007 | Project Master Brief / projectmanagement.com | Data privacy | The system shall prevent requesters from viewing other requests that don't belong to them unless authorised | MUST | Proposed | Using two requester accounts and try to access other account's requests. |
| NFR-008 | Project Master Brief / projectmanagement.com | Usability | Users should be allowed to complete service request functions without requiring any assistance | SHOULD | Proposed | Tested via completion rate and time taken to complete the service request process. |


---

## 3. Project Scope, Boundaries & Constraints

Scope baseline
The scope of CivicConnect is to provide a digital platform that the business will use to submit, manage and report on service requests in a more reliable manner. It prioritises the reliable management and traceability of service requests over other features. The scope is needed to ensure that stakeholder needs remain achievable within project constraints and providing enough detail to guide team members through the project lifecycle. 


1	In scope 
Requester capabilities: 
-	Submitting a new request with appropriate information 
-	Ability to categorise a request 
-	Viewing the status of submitted requests 
-	Viewing history of submitted requests 
-	Receive feedback regarding request status (accepted, rejected, updated or completed) 
-	Viewing only request information that the requester has access to


Staff capabilities: 
-	Viewing service requests relevant to the authorised staff
-	Searching, filtering or sorting actions on requests
-	Viewing full request details 
-	Assigning or accepting responsibility for a request 
-	Ability to update request status 
-	Recording relevant information, actions or comments 
-	Resolving or closing requests where authorised


Management and oversight capabilities: 
-	Viewing service activity information 
-	Identify requests according to states (open, overdue, resolved, and closed) 
-	Viewing request information by selected filters: category and status 
-	Accessing enough information to support accountability and service-performance analysis


System capabilities: 
-	User access and authorization based on roles and responsibilities
-	Validation of service request input 
-	Storage of request details, ownership, status and lifecycle history 
-	Protection of sensitive information
-	Show each requester their own request list, current status and feedback 

2	Out of scope 
-	Creation of a Native Android or IOS mobile application 
-	Importing of requests from external platforms/ systems such as WhatsApp, email, telephone systems, etc 
-	Payment functions/ financial transactions 
-	Inventory control, asset management functions of the service requests 
-	Functionality unrelated to service- request lifecycle
-	Purchase or installation of hardware
-	Failure recovery. Civic Connect is responsible for ensuring reliable delivery of the service request lifecycle, it will not preform any maintenance should a failure occur. 

3	Deferred
-	Integration with existing external systems 
-	Notification system regarding the state of service request 
-	Requester satisfaction, making use of feedback services for evaluations. 
-	Exporting reports to text-document formats 
-	Automatic escalation of overdue service requests 
-	Configurable management dashboards 

Deliberate exclusion: 

-	Importing from WhatsApp, Email, telephone systems, etc

Focus will be purely emphasized upon the service request lifecycle. Having importing from WhatsApp, email, and other platforms might introduce integration complexity and additional testing, shifting the focus from the core feature expressed to ensuring that the importing works. Whilst the importing will remove manual data capture, It still is an uncertainty to include within the system. 

Scope acceptance rules: 
-	In-scope, out of scope, deferred scope items must not contradict each other 
-	Every requirement appears within the RTM
-	The team must confirm that the committed scope is feasible within the upcoming and available milestones
-	Stakeholders and team members review documentation and approvals are recorded
-	Every in-scope feature must possess a unique requirement identifier
-	Requirement and scope identifiers must remain stable across milestones 
-	The team must use a change request and impact analysis before altering any scope/ requirement items
-	A MUST requirement belongs to the committed baseline unless altered by the team, formally. 
-	A SHOULD requirement may be deferred through documented impact analysis. 
-	A COULD requirement is optional and must not replace a MUST requirement. 



---

## 4. Stakeholder Identification & Governance

*Baselined in Milestone 1.*

---

## 5. Forward Engineering Considerations (FEC)

*Baselined in Milestone 1 (Observability, Scalability, Automated Testing, Security, Deployment).*

---

## 6. System Architecture & Design

*(To be baselined in Milestone 2)*

---

## 7. Implementation & Verification

*(To be baselined in Milestone 3)*

---

## 8. Deployment, Operations & SRE

*(To be baselined in Milestone 4)*

---

## References & Evidence Traceability
## Traceability matrix

### Functional requirement traceability matrix

| Requirement | Source | Priority | Status | Verification method |
| --- | --- | --- | --- | --- |
| FR-001 | Stakeholder | MUST | Proposed | Access-control tests for role-to-function and role-to-data rule. |
| FR-002 | Stakeholder | MUST | Proposed | Tests for valid and invalid submission. |
| FR-003 | Stakeholder | MUST | Proposed | Test for stored-data check regarding category selection. |
| FR-004 | Stakeholder | MUST | Proposed | Authorisation tests for requester status view. |
| FR-005 | Stakeholder | MUST | Proposed | Tests for populated and empty history states. |
| FR-006 | Stakeholder | MUST | Proposed | Tests for search, filters, sorting. |
| FR-007 | Stakeholder | MUST | Proposed | Tests for status feedback and mandatory rejection reason. |
| FR-008 | Stakeholder | MUST | Proposed | Access-control tests for full detail view. |
| FR-009 | Stakeholder | MUST | Proposed | Authorisation tests for assignment and acceptance. |
| FR-010 | Stakeholder | MUST | Proposed | Tests covering recordings and recorded actions. |
| FR-011 | Stakeholder | MUST | Proposed | Tests to resolve and close service requests. |
| FR-012 | Stakeholder | MUST | Proposed | Management-view and authorisation tests. |
| FR-013 | Stakeholder | MUST | Proposed | Tests conducted for identification of requests via request state. |
| FR-014 | Stakeholder | MUST | Proposed | Filter-combination, reset and no-results tests. |

### Non-functional requirement traceability matrix

| Requirement | Source | Priority | Status | Verification method |
| --- | --- | --- | --- | --- |
| NFR-001 | Stakeholder | MUST | Proposed | Load Testing. |
| NFR-002 | Stakeholder | MUST | Proposed | Password storage and HTTPS inspection. |
| NFR-003 | Stakeholder | SHOULD | Proposed | Monitoring testing and maintenance logs. |
| NFR-004 | Stakeholder | SHOULD | Proposed | Fault testing. |
| NFR-005 | Stakeholder | MUST | Proposed | Authorisation security tests. |
| NFR-006 | Stakeholder | SHOULD | Proposed | Access control tests. |
| NFR-007 | Stakeholder | MUST | Proposed | URL and API testing across two accounts. |
| NFR-008 | Stakeholder | SHOULD | Proposed | Completion rate and time taken to complete service request tests. |
# Bibliography

Indeed Editorial Team. Business value. Available at:
https://www.indeed.com/career-advice/career-development/business-value
(Accessed 9 September 2026).

ProjectManager. How to write a project scope statement. Available at:
https://www.projectmanager.com/blog/project-scope-statement (Accessed 9
September 2026).

Project Management Academy. Project scope statement. Available at:
https://projectmanagementacademy.net/resources/blog/project-scope-statement-pmp/
(Accessed 9 September 2026).

ProjectManager. Acceptance criteria in project management. Available at:
https://www.projectmanager.com/blog/acceptance-criteria-project-management
(Accessed 9 September 2026).

Project-Management.com. Requirements traceability matrix. Available at:
https://project-management.com/requirements-traceability-matrix-rtm/
(Accessed 9 September 2026).

* **SEN381 Master Project Brief v1.0**
* **SEN381 Milestone 1 Brief v1.0**
