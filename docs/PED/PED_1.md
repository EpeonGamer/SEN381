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
    - [FEC-01: Traceability](#fec-01-traceability)
    - [FEC-02: Maintainability](#fec-02-maintainability)
    - [FEC-03: Testability](#fec-03-testability)
    - [FEC-04: Security \& Data Privacy](#fec-04-security--data-privacy)
    - [FEC-05: Handoff](#fec-05-handoff)
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

*Baselined in Milestone 1.*

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

### FEC-01: Traceability

* **Planning Influence**: Facilitating tracking and auditing service requests (required for accountability) will require specific data solutions and transaction management, directly impacting technology and architecture choice.
* **Future Influence**: Data structure, storage policy, and audit log design.
* **Information Needs**: Log retention timeline, compliant audit attributes to be collected.
* **Risk If Ignored**: Poor management capabilities as requests cannot be linked to the agents that changed them, causing errors to compound as they cannot be monitored to be detect. Legal trouble when services cannot be linked to requests via audit.

### FEC-02: Maintainability

* **Planning Influence**: Designing to facilitate future code changes and ease of maintenance requires modular, loosely coupled code that directly influences arhcitecture design. It will also guide code structures and documentation requirements from the start, and so must be planned for early.
* **Future Influence**: Code structure, backend stack options, documentation, module design, coupling, API format.
* **Information Needs**: Stack choice, hand-off requirements, final software lifetime and maintenance scope.
* **Risk If Ignored**: If the team doesn't explicitly develop with maintainability in mind then systems will be tightly coupled and individual services cannot be updated. Instead time and costs increase as the ecosystem must be overhauled to suit small changes, which is unacceptable.

### FEC-03: Testability

* **Planning Influence**: A testable system requires isolated components with independent functionality that can be separately verified so a nested failure doesn't go unnoticed and cause cascading failures. This will directly impact coding strategy and validation processes, and should be accounted for before moving to the design phase.
* **Future Influence**: Component scope and communcation, verification process, test suites.
* **Information Needs**: Component granularity, services complexity, code coverage goals, and the selected testing tools/frameworks.
* **Risk If Ignored**: Even a well designed system cannot be analyzed and corrected if it cannot be properly tested. Without proper testability architecture in place, errors will accumulate and undermine the funcatinility and reliability of the system, including security.

### FEC-04: Security & Data Privacy

* **Planning Influence**: Security will directly impact the system approach to ensure role-based access is enforced at every step of the program, while data privacy similarly affects data collectiona and retention considerations, thereby impacting both the frontand and backend planning.
* **Future Influence**: User account management, verification, recovery strategies, encryption, backup management, role management.
* **Information Needs**: Data retention regulations, privacy regulations, required user information, varaition fo roles required, chosen stack encryption support.
* **Risk If Ignored**: Data loss or leak that can lead to system interruptions, failure, and legl action. Account failures leading to unauthorized actions or unrecoverable roles.

### FEC-05: Handoff

* **Planning Influence**: The final interface should be operable by non-technical municipal staff. Onboarding, user roles, system administration, and help documentation must be planned early so the system doesn't rely on developers for day-to-day use.
* **Future Influence**: Admin UI design, user role management, system setting controls, error messages, and admin/user documentation.
* **Information Needs**: Target admin skill level, required system settings (like ticket categories and department routing), and training documentation scope.
* **Risk If Ignored**: Non-technical staff won't be able to run the system, change settings, or onboard new agents without developer intervention. This leads to mismanaged requests, system misuse, and high maintenance overhead.

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
