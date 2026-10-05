# TP01-B: Supplier Contract API

| Field | Value |
|---|---|
| Task pair | TP01 |
| Variant | B |
| Capability | REST API + CRUD + validation |
| Matched counterpart | TP01-A (Equipment Maintenance API) |
| Implementation model | Implement frozen `ISupplierContractService` inside supplied project |
| Standalone program? | **No** — work occurs inside the supplied enterprise baseline |
| Route (documentation mapping) | `api/supplier-contracts` |
| Hidden tests | Evaluation project only (not shown to participants) |

## Business scenario

You are extending a procurement platform used by supplier-management staff. The platform already defines contracts and persistence abstractions for supplier contracts. Your job is to implement the service-layer business logic that creates, retrieves, and updates supplier contracts under the same engineering capability profile as TP01-A, with a different business domain.

## Equivalence note (A/B)

TP01-A and TP01-B are matched on:
- CRUD surface (create/get/update)
- validation count and types
- uniqueness constraint
- lifecycle lock on terminal status
- repository-backed persistence
- identical status-code / error-code contract

They differ in domain nouns, sample identifiers, and scenario text so participants do not re-solve an identical problem.

## Frozen artifacts (do not modify)

- `ISupplierContractService` method signatures
- DTO shapes in Contracts
- `ISupplierContractRepository`
- Allowed status set and error codes below

## Functional requirements

1. **Create** a supplier contract from a valid request and return status **201**.
2. **Retrieve** a contract by `Id`; return **404** when missing.
3. **Update** mutable fields for an existing non-closed contract; return **200**.
4. Validate required fields:
   - `Code` required on create
   - `Title` required (non-whitespace)
   - `EstimatedCost >= 0`
   - `Status` ∈ {`Draft`, `Scheduled`, `InProgress`, `Closed`}
5. Reject duplicate `Code` values with **409** / `DUPLICATE_CODE`.
6. Reject updates to contracts already in `Closed` status with **409** / `INVALID_STATE`.
7. Persist through the provided repository abstraction.
8. Do not change `Code` during update.

## Error codes (must match exactly)

| Situation | StatusCode | ErrorCode |
|---|---|---|
| Validation failure | 400 | `VALIDATION_ERROR` |
| Missing entity | 404 | `NOT_FOUND` |
| Duplicate code | 409 | `DUPLICATE_CODE` |
| Illegal lifecycle mutation | 409 | `INVALID_STATE` |

## Completion rule

Submit when you believe the requirements are satisfied, **or** when the maximum task time is reached.  
Hidden tests may still fail after submission; failing tests do **not** keep the timer running.
