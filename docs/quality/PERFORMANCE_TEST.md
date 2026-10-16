# Performance / Load Test (M3, NFR-001)

## Operation and why it matters
**Request submission plus reloading "My requests"** — the requester's most frequent journey (FR-002, FR-005).
Submission is the write path: SQLite allows only one writer at a time (ADR-07), so concurrent submissions
are the most likely place for the release candidate to slow down. The list is the most frequent read.

## Workload and environment
| Item | Value |
|---|---|
| Tool | `tools/CivicConnect.LoadTest` (team-written, no paid tooling): each virtual user logs in once, then loops *submit → list* with no think time |
| Workload | 1 user × 20 s (baseline); 10 concurrent users × 60 s (load) |
| Target | Simulated staging, `http://localhost:5090`, Release build, `Staging` environment |
| Data | Staging database already holding earlier test data; the second 10-user run started with ~4,050 more rows than the first (the first run's submissions) |
| Machine | Intel Core i5-8250U, 20 GB RAM, Windows 11, .NET 8.0.32; load generator and server on the **same** machine |
| Measures | Response time (p50, p95, max), error count/rate, throughput |

Raw output: [`evidence/`](evidence/).

## Results
| Run | Release | Operation | Requests | Errors | p50 ms | p95 ms | Max ms | Throughput req/s |
|---|---|---|---|---|---|---|---|---|
| Baseline, 1 user | `f9fbdf6` | submit | 1,585 | 0 | 8 | 14 | 28 | 79.2 |
| | | list | 1,585 | 0 | 3 | 7 | 21 | 79.2 |
| Load, 10 users | `f9fbdf6` (before WAL) | submit | 4,050 | 0 | 11 | **818** | **7,316** | 66.8 |
| | | list | 4,050 | 0 | 7 | 27 | 213 | 66.8 |
| Load, 10 users | `c5778f9` (WAL) | submit | 8,286 | 0 | 6 | **311** | 2,181 | 137.7 |
| | | list | 8,286 | 0 | 24 | 46 | 651 | 137.7 |

## Target / baseline
NFR-001 ("respond as intended during normal operational conditions") has **no numeric target**, so no
pass/fail threshold is claimed. Results are compared with the 1-user baseline measured in the same environment.

## Interpretation
- **Correctness under load:** 0 errors in 27,842 requests. No lost submissions or failed reads at 10 concurrent users.
- **Defect found (DEF-09):** with 10 users, submission p95 rose from 14 ms to 818 ms (max 7.3 s) while reads
  stayed fast. That pattern means writers queue for SQLite's single write lock.
- **Mitigation measured:** WAL journal mode (readers no longer wait for writers) cut submit p95 to 311 ms
  and the max to 2.2 s, and doubled throughput. List p50 rose from 7 to 24 ms because the server completed twice
  as much work in the same time on the same CPU, with a larger database. This is a trade-off, not a free win.
- **Still a residual risk:** submit p95 at 10 users is ~22× the single-user value. Writes remain serialised.

## What this does not prove
- Nothing about municipal-scale load, PostgreSQL (the production target), or network latency. Client and server
  shared one laptop CPU.
- Single 60-second runs. Run-to-run variance was not measured.
- Requester traffic only. Staff transitions and manager counts were not loaded.
- With no think time, 10 virtual users represent far more than 10 real people. They do not represent a specific real user count.
