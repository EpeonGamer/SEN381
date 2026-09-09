# 1. Project Engineering Document

> CivicConnect Public Service Reporting Platform  
> **SEN381** *Final Engineering Project*

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
  - [1.6. Project Scope, Boundaries \& Constraints](#16-project-scope-boundaries--constraints)
  - [1.7. Stakeholder Identification \& Governance](#17-stakeholder-identification--governance)
  - [1.8. Forward Engineering Considerations (FEC)](#18-forward-engineering-considerations-fec)
  - [1.9. Risk Identification and Management](#19-risk-identification-and-management)
  - [1.10. System Architecture \& Design](#110-system-architecture--design)
  - [1.11. Implementation \& Verification](#111-implementation--verification)
  - [1.12. Deployment, Operations \& SRE](#112-deployment-operations--sre)
  - [1.13. References \& Evidence Traceability](#113-references--evidence-traceability)


---

## 1.2. Change Control

|**Version**|**Date**|**Last Edit**|**By**|**Reviewers**|
|---|---|---|---|---|
|0.0.1|2026/09/08|Document structure|Kasper|Aidan, Lethebe|
|0.1.0|2026/09/08|Convert to .md|Kasper|NONE|
|1.0.0|*pending*|Formal Milestone 1 Baseline Submission.|Team|NONE|

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

*Baselined in Milestone 1.*

---

## 1.5. Requirements Baseline (Functional & Non-Functional)

*Baselined in Milestone 1.*

---

## 1.6. Project Scope, Boundaries & Constraints

*Baselined in Milestone 1.*

---

## 1.7. Stakeholder Identification & Governance

Stakeholders:

 Staff: Reporting staff working with CivicConnect have a stake in the project as they are the ones who are going to be using the system. Their interests and feedback will have significant contributions to the project with regards to its user interface, accessibility, as well as the product testing.

Contractors: Any service requests, requirements, payments and communications for contractors go through CivicConnect. As such, any potential contractor would have a stake in the project. However, it would be difficult to consult with potential contractors as they can change with each project and dependant on their tenders. They will therefore be considered, however are not significant stakeholders in the project.

Communal Users: The community that uses CivicConnect will want to understand how the software works, and their input for the development of CivicConnect will be required to allow for a functional and satisfactory product upon completion.

Business and Financial Stakeholders: The business, as well as its and any other financial backers, will want to ensure that they get value out of the product they are investing in, as well as a return on their investment. As such they have a significant stake and interest in a successful outcome of the project.

Additional Requirements for parties of interest:

Government Regulatory Bodies: Regulatory Bodies would want to ensure that CivicConnect complies with code requirements and regulations. As such, the development team needs to ensure that any potential regulatory requirements are analysed and adhered to within the project scope.

SARS: The South African Revenue Service will want financial reports with respect to the project, as well as potential information relating to audits. This is both to ensure lawful compliance, as well as required taxes that can arise from the product’s potential profits or losses.


---

## 1.8. Forward Engineering Considerations (FEC)

*Baselined in Milestone 1 (Observability, Scalability, Automated Testing, Security, Deployment).*

---

## 1.9. Risk Identification and Management

Security: The organization deal with a lot of sensitive information, as such there needs to be adequate measures in place to deal with the data. These include encryptions, backups, two-factor authentication, and role-authenticated login for specific members included in the project.

Cost: As this is a community-led project, there is an idea to have as few costs as possible, potentially even including as many reliable, reputable and qualitative resources as possible. There is a concern that a rise in costs could be a substantial risk, including but not limited to paid-for technologies, subscriptions, employee wages, equipment and more. Ways to mitigate this is by investigating costs beforehand and ensuring they fit a predetermined budget, as well as ensuring any new technology, product or service that could be used for the project gets discussed and analysed financially before being added.

Quality: The Project needs to meet quality standards and contain the necessary features promised from the outset. Should technologies fail to meet requirements of the project it could result in a loss of quality. Additionally, if employees fail to contribute to project in a meaningful way, quality can also suffer. One way to ensure quality standards are maintained would be to analyse progress in stages to ensure each element of the product is held to a satisfactory standard.

Reliability: CivicConnect needs to be accessible to its users as often as possible, particularly at peak hours. Problems can arise during peak hours, such as latency. Additionally, data loss, connectivity issues, power outages and more can cause reliability concerns. Ways to combat this can include service backups, extra server or even servers for consistent access, as well as hardware such as a UPS to maintain power in the event of outages.

Scope Creep: The addition of extra features and stakeholder requests can result in scope creep of the project beyond a reasonable state. This can cause an increase to costs, a delay in work completion, reduction in quality and unsatisfied stakeholders. Ways to avoid this is by repeatedly engaging stakeholders, keeping track of scope and ensuring there is continuous analysis of features being requested to ensure it is within the team’s capabilities.


---

## 1.10. System Architecture & Design

*(To be baselined in Milestone 2)*

---

## 1.11. Implementation & Verification

*(To be baselined in Milestone 3)*

---

## 1.12. Deployment, Operations & SRE

*(To be baselined in Milestone 4)*

---

## 1.13. References & Evidence Traceability

* **SEN381 Master Project Brief v1.0**
* **SEN381 Milestone 1 Brief v1.0**
