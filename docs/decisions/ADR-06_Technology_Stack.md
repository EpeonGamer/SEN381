# ADR-06 — Technology Stack

- **Status:** Accepted
- **Date:** 2026-09-30
- **Decision owners:** CivicConnect team

## 1. Problem

CivicConnect requires technology stack for development. The stack needs to adhere to the Architecturally significant requirements established in the PED.

## 2. Decision

The stack of C#, ASP.NET, PostgreSQL and npgSQL.

## 3. Alternatives considered

Python Programming, Django, with PostgreSQL/SQLite\
Java, SpringBoot, with PostgreSQL, JUnit and Maven\
Typescript, Node/Express, with PostgreSQL/Vitest/Jest, React/Server-rendered Pages

## 4. Rationale

The reasoning behind the decisions is due to the familiarity of the software available, the languages and the integrations to the development team. Additionally security and maintainability are also adequately accounted for from these technologies.

Furthermore, based off of research gathered from Assignment 2, the stack allows for an adequate cohesion and coupling balance, such that the system works effectively whilst maintaining a degree of flexibility should and alternative programming software prove to be necessary.

Lastly the software utilized for the technology stack was also selected for its integration capabilities and well as the familiarity to the developers. Alternative stack technologies are also able to integrate with the backend software appropriately.

## 5. Expected benefit and complexity introduced

| Benefit | Complexity / trade-off |
|---|---|
| Familiarity | Potential feature of other software not explored |
| Maintainability and ease of development | Python may have simpler development, combined with unknown future integration support |

## 6. Risks

- Potential feature of other software not explored.
- Potential performance and cost parameters if the technology stack changes.
- The software to implement the stack will also be required to change if the technology stack changes.

## 7. Scope and deferred work

This ADR establishes technology stacks to be utilized and the reasoning behind that decision. It does not include any implemented programming as of yet in the full application stack.

## 8. Evidence

- PED 1.12 — System Architecture & Design.
- StackDiagram in `docs/architecture/diagrams/StackDiagram.png`.
- CivicConnect A2 design quality and Design Pattern research.
- Current C#/.NET Core project structure in `src/CivicConnect.Core/`.

## 9. Consequences of decision change

Should there be a change in technology stack used, the software to implement the stack will also be required to change. Additionally the integration to allow Postgre SQL will also need adjustment, as well as potential performance and cost parameters.

## 10. References

- CivicConnect A2 design quality and Design Pattern research
