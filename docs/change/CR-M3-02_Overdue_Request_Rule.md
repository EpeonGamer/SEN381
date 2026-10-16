# CR-M3-02: Derived overdue requests

Date: 2026-10-16. Rule selected by the user during implementation; repository review remains pending.

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

## Implementation and evidence

`OverduePolicy` owns the duration and boundary rule. `SqliteRequestRepository` uses the same cutoff in SQL before pagination; its injectable clock and the API clock allow deterministic boundary tests. No stored overdue flag can become stale.

`OverdueTests` covers both open statuses at -1/0/+1 tick around the deadline, terminal exclusions, UTC offsets, future submissions, existing rows, scope/category/status filtering, pagination, assignment, resolution and rejection.

`OverdueApiTests` covers login, submission, exact deadline, overdue list/detail/counts, scope/privacy, unauthorised reports, pagination, invalid filter input and assignment/resolution through the real HTTP pipeline and SQLite. Time-travel tests log in again after the existing eight-hour session expires.

Full regression: 85/85 cases passed (65 Core, 20 API), including all original 72. Release build with `-warnaserror`: zero warnings/errors. See `docs/quality/TEST_EVIDENCE.md` for execution commands.

## Impact and limitations

Resolves DEF-07's missing implementation. The previously undefined rule in CR-M2-01 is superseded by this record. No automatic escalation, notifications, per-category SLA, persisted due-date overrides or historical lateness metrics are introduced. Changes to this three-day policy affect existing open requests because deadlines are derived.
