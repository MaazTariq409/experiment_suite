# TP01-A: Equipment Maintenance API

| Field | Value |
|---|---|
| Task pair | TP01 |
| Variant | A |
| Capability | REST API + CRUD + validation |
| Matched counterpart | TP01-B (Supplier Contract API) |
| Implementation model | Implement frozen `IMaintenanceRecordService` inside supplied project |
| Standalone program? | **No** — work occurs inside the supplied enterprise baseline |
| Route (documentation mapping) | `api/maintenance-records` |
| Hidden tests | Evaluation project only (not shown to participants) |

## Business scenario

You are extending an internal facilities platform used by an operations team. The platform already defines contracts and persistence abstractions for equipment maintenance records. Your job is to implement the service-layer business logic that registers, retrieves, and updates maintenance records under enterprise validation and lifecycle rules.

## Why this is enterprise-oriented

- Layered architecture with frozen contracts, DTOs, and repository abstraction
- Business identifiers (`Code`) with uniqueness constraints
- Lifecycle statuses (`Draft`, `Scheduled`, `InProgress`, `Closed`)
- Deterministic service-layer status codes analogous to HTTP semantics
- Domain context: equipment maintenance operations, not a toy algorithm

## Frozen artifacts (do not modify)

- `IMaintenanceRecordService` method signatures
- `MaintenanceRecordCreateRequest` / `UpdateRequest` / `Response` DTO shapes
- `IMaintenanceRecordRepository`
- Allowed status set and error codes below

## Functional requirements

1. **Create** a maintenance record from a valid request and return status **201**.
2. **Retrieve** a record by `Id`; return **404** when missing.
3. **Update** mutable fields (`Title`, `ScheduledDate`, `EstimatedCost`, `Status`) for an existing non-closed record; return **200**.
4. Validate required fields:
   - `Code` required on create
   - `Title` required (non-whitespace)
   - `EstimatedCost >= 0`
   - `Status` ∈ {`Draft`, `Scheduled`, `InProgress`, `Closed`}
5. Reject duplicate `Code` values with **409** / `DUPLICATE_CODE`.
6. Reject updates to records already in `Closed` status with **409** / `INVALID_STATE`.
7. Persist through the provided repository abstraction.
8. Do not change `Code` during update.

## Error codes (must match exactly)

| Situation | StatusCode | ErrorCode |
|---|---|---|
| Validation failure | 400 | `VALIDATION_ERROR` |
| Missing entity | 404 | `NOT_FOUND` |
| Duplicate code | 409 | `DUPLICATE_CODE` |
| Illegal lifecycle mutation | 409 | `INVALID_STATE` |

## Acceptance criteria

- Participant project builds
- Service implements all interface methods
- Behavior satisfies the hidden requirement-to-test matrix

## Completion rule

Submit when you believe the requirements are satisfied, **or** when the maximum task time is reached.  
Hidden tests may still fail after submission; failing tests do **not** keep the timer running.

## Implementation scope reminder

You are **not** writing a standalone console program. You implement missing enterprise service behavior inside the supplied project structure.
