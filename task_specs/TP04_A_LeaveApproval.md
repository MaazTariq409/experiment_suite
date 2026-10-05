# TP04-A: Leave Approval

| Field | Value |
|---|---|
| Task pair | TP04 |
| Variant | A |
| Capability | State-transition workflow |
| Matched counterpart | TP04-B (Expense Reimbursement) |
| Implementation | Frozen `ILeaveApprovalService` |
| Hidden tests | 20 |

## Statuses
`Draft`, `Submitted`, `Approved`, `Rejected`, `Cancelled`

## Roles
`Employee`, `Manager`, `Admin`

## Allowed transitions

| From | To | Roles | Comment |
|---|---|---|---|
| Draft | Submitted | Employee, Manager, Admin | optional |
| Draft | Cancelled | Employee, Manager, Admin | optional |
| Submitted | Approved | Manager, Admin | **required** |
| Submitted | Rejected | Manager, Admin | **required** |
| Submitted | Cancelled | Employee, Manager, Admin | optional |

Terminal: `Approved`, `Rejected`, `Cancelled` (no further transitions).

## Result semantics
- Validation failure → 400 / `VALIDATION_ERROR` (empty Id, unknown status/role)
- Business denial → Succeeded=true, Applied=false, NewStatus=current, ReasonCode ∈ {`INVALID_TRANSITION`,`FORBIDDEN`,`COMMENT_REQUIRED`}
- Success → Applied=true, NewStatus=target (canonical casing), ReasonCode=`APPLIED`

Comparisons are case-insensitive.
