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

*Baselined in Milestone 1.*

---

## 3. Project Scope, Boundaries & Constraints

*Baselined in Milestone 1.*

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
