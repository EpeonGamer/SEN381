# ADR-08: API Authentication for the M3 Release Candidate

**Status:** Accepted (2026-10-16, approved by Aidan and Lethebe in PR #14)

**Date:** 2026-10-16

**Relates to:** ADR-05 (authorization), ADR-07 (SQLite), NFR-002, NFR-005, RISK-001

## 1. Context
ADR-05 decides *what* each actor may do, but the Core trusted whatever `ActorContext` it was given. M3 needs an
HTTP API, so the system must establish *who* is calling. The constraints were: a deadline of one day, a three-person
team, no paid identity service, a same-origin browser frontend, and NFR-002 (no plaintext passwords).

## 2. Alternatives
| Option | For | Against |
|---|---|---|
| A. Trust a user-id header | Trivial | Anyone can impersonate anyone. Rejected: it would fail NFR-005. |
| B. JWT bearer tokens | Stateless; suits mobile and third-party clients | Extra package, a signing-key secret to manage, and tokens held in browser JavaScript |
| C. External identity provider (e.g. Entra ID) | Production-grade MFA and account management | Setup and cost are not achievable in M3; we have no tenant |
| **D. ASP.NET Core cookie authentication + PBKDF2 hashes in SQLite** | Built into .NET 8 with no extra packages; HttpOnly cookie that scripts cannot read; works with a same-origin UI | Server-side account store; CSRF has to be handled |

## 3. Decision
Option D:
- `POST /api/auth/login` checks a PBKDF2-SHA256 hash (100,000 iterations, per-user salt, constant-time compare) stored in a new `user_credentials` table. The Core schema is unchanged.
- The cookie is HttpOnly and `SameSite=Strict`, and Secure when `Security:RequireHttps` (default on).
- **Authenticated by default:** a fallback authorization policy, so a new endpoint is protected unless it explicitly allows anonymous access.
- Login is rate-limited per IP, and "unknown user" and "wrong password" get the same answer.
- The cookie carries only the user id and role. Core services **reload** role and groups from the database on every call (ADR-05), so a stale cookie cannot widen access.
- CSRF: `SameSite=Strict` plus JSON-only request bodies.

## 4. Consequences
- **Verified:** API1 (401 when unauthenticated), API2 (no user enumeration), API5 (role enforced over HTTP), E2E1/E2E2 (login works on staging).
- **Not verified / debt:** HTTPS is not exercised in simulated staging (TD-07). One shared demo password, no lockout and no MFA (TD-06, RISK-001).
- **Reversal path:** replace the login endpoint and cookie handler with OpenID Connect against an identity provider. The endpoints and Core remain unchanged because they only read the authenticated user id.
