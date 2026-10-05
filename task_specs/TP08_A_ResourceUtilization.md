# TP08-A: Resource Utilization

| Field | Value |
|---|---|
| Task pair | TP08 |
| Variant | A |
| Capability | Reporting + date logic |
| Matched counterpart | TP08-B (Support Ticket SLA) |
| Implementation | Frozen `IResourceUtilizationService` |
| Hidden tests | 20 |

## Rules
1. Null source or `From > To` → 400 / `VALIDATION_ERROR`
2. Any row with blank `ResourceId`, `End < Start`, or `Units < 0` → 400
3. Trim `ResourceId`
4. Inclusive period capacity = `To - From + 1` days
5. Overlap days = inclusive intersection of row `[Start,End]` with query `[From,To]`
6. Contribution = `Units * overlapDays` (`Units` is a daily rate)
7. Aggregate contributions by `ResourceId` (ordinal, case-sensitive)
8. Omit resources whose total contribution is 0
9. `UtilizationPercent = Round(total/capacity*100, 2, AwayFromZero)`
10. `OverallUtilizationPercent` uses grand total of all contributions
11. Lines sorted by `ResourceId` ordinal ascending

## Completion rule
Submit when finished or at time limit; hidden tests scored post-hoc.
