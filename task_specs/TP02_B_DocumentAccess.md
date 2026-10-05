# TP02-B: Document Access Policy

| Field | Value |
|---|---|
| Task pair | TP02 |
| Variant | B |
| Capability | Authorization + business rules |
| Matched counterpart | TP02-A (Project Access Policy) |
| Implementation model | Implement frozen `IDocumentAccessService` inside supplied project |
| Standalone program? | **No** — work occurs inside the supplied enterprise baseline |
| Hidden tests | 20 confirmatory tests in Evaluation project |

## Business scenario

You are extending an enterprise document-control platform. Document resources have lifecycle statuses and role-based permissions. Implement the authorization decision service that enforces the same capability profile as TP02-A with document-domain identifiers.

## Equivalence note (A/B)

Matched on: role set, action set, status set, Closed-mutation rule, Contributor Open-only update rule, validation/error semantics, and test count.  
Differ on: resource domain nouns (`DOC-*` vs `PRJ-*`) and scenario text.

## Frozen vocabulary

**Roles:** `Admin`, `Manager`, `Contributor`, `Viewer`  
**Actions:** `View`, `Update`, `Approve`, `Archive`  
**Statuses:** `Open`, `InReview`, `Closed`  
Comparisons are **case-insensitive**.

## Policy matrix (non-Closed resources)

| Role | View | Update | Approve | Archive |
|---|---|---|---|---|
| Admin | allow | allow | allow | allow |
| Manager | allow | allow | allow | deny |
| Contributor | allow | allow only if status=`Open` | deny | deny |
| Viewer | allow | deny | deny | deny |

## Additional business rules

1. If `ResourceStatus=Closed` and action ∈ {Update, Approve, Archive} → `Allowed=false`, `ReasonCode=INVALID_STATE`.
2. `View` on Closed resources remains allowed for all valid roles.
3. Malformed inputs → **400** / `VALIDATION_ERROR`.
4. Authorization denials are successful results with `Allowed=false`.

## Completion rule

Submit when you believe requirements are satisfied, or when the time limit is reached. Hidden test failures do not keep the timer running.
