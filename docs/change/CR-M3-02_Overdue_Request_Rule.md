# CR-M3-02: Derived overdue requests

Date: 2026-10-16. Proposed and implemented by Lethebe (Member One, commit `5042b6d`). Status: **team approval pending** in the M3 pull-request review.

## Decision and acceptance criteria (FR-013)

- A request is overdue only while Unassigned or Assigned, and only when the current instant is strictly later than submission plus 3 days (72 elapsed hours).
- Exactly at the deadline it is not overdue. Compare instants in UTC; weekends, calendar-day boundaries and local daylight-saving changes do not alter the duration.
- Assignment and reassignment do not reset the deadline. Rejected and Resolved requests are never currently overdue.
- Overdue is a subset of Open, not an additional lifecycle state. Existing transitions, atomic audit writes and version checks remain unchanged.
- Deadline and overdue state are derived at read time. Existing database rows work without a schema migration or background job. The UI updates on reload/filter/action, not on a timer.
- Request summaries/details add `dueAt` and `isOverdue`; report counts add `overdue`. Existing fields remain.
- Lists and counts accept optional `overdue=true|false`, combined with existing category/status/scope filters. Omission preserves the previous unfiltered behaviour. Counts cover all matching records, independently of list pagination.
- Requester ownership, staff category scope and Manager-only report access continue to apply.
- The UI shows deadlines, overdue labels, an overdue filter and a Manager overdue count. List and count filters agree.

## Rationale: why 72 hours

No CivicConnect stakeholder has set a resolution target, and the M3 brief says not to invent one. The window is therefore
an **explicit team assumption**, chosen to sit in the range that municipalities publish for routine faults:

| Source | Published turnaround |
|---|---|
| Johannesburg Roads Agency, in Parliament reply NW585 (CoGTA, 11 April 2016) | "attempt to resolve the calls within the 3 days turnaround time" |
| Ekurhuleni, same reply | Streetlights: average 3 days. Potholes: 24 hours (major roads), 5 working days (minor roads) |
| Tshwane / Mangaung, same reply | Target 48 hours / pothole target 5 days |
| Washington DC Department of Transportation | Potholes repaired "within three business days (72 hours)" of the report |

IT service desks show the same pattern: resolution targets are set per organisation and per priority, not by ITIL itself.
That is why CivicConnect records its own figure in this change record.

**Alternatives considered**

| Option | Why not chosen for M3 |
|---|---|
| 48 hours | Matches Tshwane's strictest target, but staff have no priority levels yet, so ordinary requests would be flagged before they could reasonably be done |
| 5 working days | Needs a business-day and South African public-holiday calendar, which adds complexity and a new failure point |
| Per-category targets | Best long-term fit (a burst pipe is not a missing bin), but there are no agreed targets per category; deferred |
| No rule / defer | Leaves FR-013 unmet (DEF-07) |

**Elapsed hours, not business days.** DC counts 3 *business* days. CivicConnect counts 72 *elapsed* hours, so weekends count.
This is simpler (no holiday calendar) and errs towards flagging a request for management attention. A request submitted on
Friday afternoon becomes overdue on Monday afternoon.

**Low cost of being wrong.** The window is one constant (`OverduePolicy.ResolutionWindow`) and overdue is derived at read
time. Changing it is a one-line change plus the boundary tests, with no data migration. "Overdue" only flags a request; it
triggers no escalation or penalty.

**Limits of this evidence.** The benchmarks are routine *road and electrical* faults from other cities and from 2016. They
do not validate the window for all CivicConnect categories, and no stakeholder has confirmed it. Stakeholder validation is
an action for M4.

Sources: [PMG — Question NW585 to the Minister of CoGTA](https://pmg.org.za/committee-question/2699/) ·
[DDOT — Pothole Repair](https://ddot.dc.gov/node/543302) ·
[University of Minnesota IT — incident priority targets](https://it.umn.edu/node/56176) (example of organisation-set targets)

## Implementation and evidence

`OverduePolicy` owns the duration and boundary rule. `SqliteRequestRepository` uses the same cutoff in SQL before pagination; its injectable clock and the API clock allow deterministic boundary tests. No stored overdue flag can become stale.

`OverdueTests` covers both open statuses at -1/0/+1 tick around the deadline, terminal exclusions, UTC offsets, future submissions, existing rows, scope/category/status filtering, pagination, assignment, resolution and rejection.

`OverdueApiTests` covers login, submission, exact deadline, overdue list/detail/counts, scope/privacy, unauthorised reports, pagination, invalid filter input and assignment/resolution through the real HTTP pipeline and SQLite. Time-travel tests log in again after the existing eight-hour session expires.

Full regression: 85/85 cases passed (65 Core, 20 API), including all original 72. Release build with `-warnaserror`: zero warnings/errors. See `docs/quality/TEST_EVIDENCE.md` for execution commands.

## Impact and limitations

Resolves DEF-07's missing implementation. The previously undefined rule in CR-M2-01 is superseded by this record. No automatic escalation, notifications, per-category SLA, persisted due-date overrides or historical lateness metrics are introduced. Changes to this three-day policy affect existing open requests because deadlines are derived.
