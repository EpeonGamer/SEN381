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
Functional Requirements
+-------------+---------+-----------------+----------+----------+----------------+
| Requirement | Source  | Description     | Priority | Status   | Acceptance     |
| ID          |         |                 |          |          | criteria       |
+=============+=========+=================+==========+==========+================+
| FR-001      | Project | The system      | MUST     | Proposed | A requester    |
|             | Master  | shall allow     |          |          | can only see   |
|             | brief   | authorised      |          |          | their requests |
|             |         | requester to    |          |          | and not others |
|             |         | perform         |          |          |                |
|             |         | authorised      |          |          | Staff can only |
|             |         | functions,      |          |          | access         |
|             |         | according to    |          |          | features can   |
|             |         | their role      |          |          | function they  |
|             |         |                 |          |          | are permitted  |
|             |         |                 |          |          |                |
|             |         |                 |          |          | Management     |
|             |         |                 |          |          | features are   |
|             |         |                 |          |          | unavailable to |
|             |         |                 |          |          | requesters and |
|             |         |                 |          |          | staff          |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-002      | Project | The system must | MUST     | Proposed | Submission     |
|             | Master  | allow           |          |          | form captures  |
|             | brief   | authenticated   |          |          | necessary      |
|             |         | requester to    |          |          | service        |
|             |         | submit requests |          |          | request        |
|             |         |                 |          |          | details        |
|             |         |                 |          |          |                |
|             |         |                 |          |          | When correct   |
|             |         |                 |          |          | input is       |
|             |         |                 |          |          | entered and    |
|             |         |                 |          |          | submit is      |
|             |         |                 |          |          | clicked, the   |
|             |         |                 |          |          | system stores  |
|             |         |                 |          |          | the service    |
|             |         |                 |          |          | request linked |
|             |         |                 |          |          | to the         |
|             |         |                 |          |          | requester      |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-003      | Project | The system      | MUST     | Proposed | The selected   |
|             | Master  | shall allow a   |          |          | category is    |
|             | brief   | requester to    |          |          | stored in      |
|             |         | categorise a    |          |          | correlation to |
|             |         | request         |          |          | the submitted  |
|             |         |                 |          |          | service        |
|             |         |                 |          |          | request        |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-004      | Project | The system      | MUST     | Proposed | The request    |
|             | Master  | shall allow a   |          |          | status         |
|             | brief   | requester to    |          |          | displays       |
|             |         | view status of  |          |          | current status |
|             |         | their submitted |          |          |                |
|             |         | requests        |          |          |                |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-005      | Project | The system      | MUST     | Proposed | The list shows |
|             | Master  | shall allow     |          |          | service        |
|             | brief   | requesters to   |          |          | requests       |
|             |         | view a history/ |          |          | submitted by   |
|             |         | list of their   |          |          | the requester  |
|             |         | previously      |          |          | and the        |
|             |         | submitted       |          |          | relevant       |
|             |         | requests        |          |          | information    |
|             |         |                 |          |          |                |
|             |         |                 |          |          | When no        |
|             |         |                 |          |          | requests       |
|             |         |                 |          |          | exist, an      |
|             |         |                 |          |          | empty screen/  |
|             |         |                 |          |          | history list   |
|             |         |                 |          |          | is returned    |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-006      | Project | The system      | MUST     | Proposed | Staff can      |
|             | Master  | shall allow     |          |          | search using   |
|             | brief   | authorised      |          |          | matching data  |
|             |         | staff to        |          |          | such as unique |
|             |         | search, filter  |          |          | identifier for |
|             |         | and sort        |          |          | service        |
|             |         | service         |          |          | request        |
|             |         | requests        |          |          |                |
|             |         |                 |          |          | Staff can      |
|             |         |                 |          |          | filter by      |
|             |         |                 |          |          | categories     |
|             |         |                 |          |          |                |
|             |         |                 |          |          | Staff can sort |
|             |         |                 |          |          | by categories  |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-007      | Project | The system      | MUST     | Proposed | Each           |
|             | Master  | shall provide   |          |          | application    |
|             | brief   | meaningful feed |          |          | feedback       |
|             |         | back regarding  |          |          | displays the   |
|             |         | the state of a  |          |          | relevant       |
|             |         | request         |          |          | service        |
|             |         |                 |          |          | request along  |
|             |         |                 |          |          | with its       |
|             |         |                 |          |          | status and     |
|             |         |                 |          |          | reasoning for  |
|             |         |                 |          |          | the status     |
|             |         |                 |          |          | given          |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-008      | Project | The system      | MUST     | Proposed | The system     |
|             | Master  | shall allow     |          |          | provides full  |
|             | brief   | authorised      |          |          | service        |
|             |         | staff to view   |          |          | request        |
|             |         | full request    |          |          | details        |
|             |         | details         |          |          |                |
|             |         |                 |          |          | A staff cannot |
|             |         |                 |          |          | view a request |
|             |         |                 |          |          | if they are    |
|             |         |                 |          |          | not authorised |
|             |         |                 |          |          | for the        |
|             |         |                 |          |          | request        |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-009      | Project | The system      | MUST     | Proposed | Responsibility |
|             | Master  | shall allow an  |          |          | is given by    |
|             | brief   | authorised      |          |          | selecting      |
|             |         | staff to assign |          |          | another staff  |
|             |         | or accept       |          |          | role           |
|             |         | responsibility  |          |          |                |
|             |         | for a request   |          |          | No other role  |
|             |         |                 |          |          | but staff can  |
|             |         |                 |          |          | assign/accept  |
|             |         |                 |          |          | responsibility |
|             |         |                 |          |          | for a request  |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-009      | Project | The system      | MUST     | Proposed | The system     |
|             | Master  | shall allow     |          |          | will only      |
|             | brief   | authorised      |          |          | accept the     |
|             |         | staff to update |          |          | following:     |
|             |         | a request       |          |          | Assigned, In   |
|             |         | through defined |          |          | Progress,      |
|             |         | status          |          |          | Resolved,      |
|             |         | transactions    |          |          | Closed         |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-010      | Project | The system      | MUST     | Proposed | Comments will  |
|             | Master  | shall record    |          |          | be saved as an |
|             | brief   | material        |          |          | entry with the |
|             |         | actions and     |          |          | author and     |
|             |         | comments        |          |          | description of |
|             |         | associated with |          |          | the comment    |
|             |         | a request       |          |          |                |
|             |         |                 |          |          | Empty comments |
|             |         |                 |          |          | are rejected   |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-0011     | Project | The system      | MUST     | Proposed | After a        |
|             | Master  | shall allow     |          |          | service has    |
|             | brief   | authorised      |          |          | been resolved, |
|             |         | staff to        |          |          | the requester  |
|             |         | resolve/ close  |          |          | will be able   |
|             |         | requests        |          |          | to see the new |
|             |         |                 |          |          | status         |
|             |         |                 |          |          |                |
|             |         |                 |          |          | Only a request |
|             |         |                 |          |          | in an approved |
|             |         |                 |          |          | state can be   |
|             |         |                 |          |          | closed         |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-012      | Project | The system      | MUST     | Proposed | The overview   |
|             | Master  | shall allow     |          |          | does not       |
|             | brief   | authorised      |          |          | expose request |
|             |         | managers to     |          |          | information    |
|             |         | view service    |          |          | outside the    |
|             |         | activity        |          |          | manager\'s     |
|             |         | information     |          |          | authorised     |
|             |         |                 |          |          | scope          |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-013      | Project | The system      | MUST     | Proposed | System         |
|             | Master  | shall allow     |          |          | provides       |
|             | brief   | management to   |          |          | managers with  |
|             |         | identify open,  |          |          | counts of      |
|             |         | overdue,        |          |          | open, overdue, |
|             |         | resolved, and   |          |          | resolved,      |
|             |         | closed requests |          |          | closed         |
|             |         |                 |          |          | requests       |
+-------------+---------+-----------------+----------+----------+----------------+
| FR-0014     | Project | The system      | MUST     | Proposed | System         |
|             | Master  | shall allow     |          |          | provides       |
|             | brief   | management to   |          |          | filter options |
|             |         | view request    |          |          | for managers   |
|             |         | information     |          |          | to select from |
|             |         | according to    |          |          |                |
|             |         | filters such as |          |          |                |
|             |         | category/status |          |          |                |
+-------------+---------+-----------------+----------+----------+----------------+


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

* **SEN381 Master Project Brief v1.0**
* **SEN381 Milestone 1 Brief v1.0**
