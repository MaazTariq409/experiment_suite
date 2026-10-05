# TP02-A: Project Access Policy

| Field | Value |
|---|---|
| Task pair | TP02 |
| Variant | A |
| Capability | Authorization + business rules |
| Matched counterpart | TP02-B (Document Access Policy) |
| Implementation model | Implement frozen `IProjectAccessService` inside supplied project |
| Standalone program? | **No** — work occurs inside the supplied enterprise baseline |
| Hidden tests | 20 confirmatory tests in Evaluation project |

## Business scenario

You are extending an enterprise project-collaboration platform. Project resources have lifecycle statuses and role-based permissions. Implement the authorization decision service that enforces the policy matrix below.

## Why this is enterprise-oriented

- Role-based access control (RBAC) over business resources
- Status-dependent authorization (Closed resources reject mutations)
- Explicit deny reasons (`FORBIDDEN`, `INVALID_STATE`) separate from input validation
- Domain context: project governance, not a boolean toy predicate

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

1. If `ResourceStatus=Closed` and action ∈ {Update, Approve, Archive} → `Allowed=false`, `ReasonCode=INVALID_STATE` (even for Admin).
2. `View` on Closed resources remains allowed for all valid roles.
3. Malformed inputs (blank `ActorUserId`/`ResourceId`, unknown role/action/status) → `OperationResult` failure, status **400**, `VALIDATION_ERROR`.
4. Authorization denials are **successful** results (`Succeeded=true`) with `Allowed=false`.

## Reason codes

| Situation | Succeeded | Allowed | ReasonCode / ErrorCode |
|---|---|---|---|
| Validation failure | false | — | `VALIDATION_ERROR` |
| Closed mutation | true | false | `INVALID_STATE` |
| Role insufficient | true | false | `FORBIDDEN` |
| Permitted | true | true | `ALLOWED` |

## Completion rule

Submit when you believe requirements are satisfied, or when the time limit is reached. Hidden test failures do not keep the timer running.
