# TP03-B: Purchase Order Fulfilment

| Field | Value |
|---|---|
| Task pair | TP03 |
| Variant | B |
| Capability | Database query + aggregation |
| Matched counterpart | TP03-A (Invoice Aging) |
| Implementation model | Implement frozen `IPurchaseOrderFulfilmentService` inside supplied project |
| Standalone program? | **No** |
| Hidden tests | 20 confirmatory tests |

## Business scenario

Build a purchase-order fulfilment aging report from order rows. Filter by supplier (`OwnerKey`), compute outstanding order value, classify by days past due relative to `AsOfDate`, and aggregate into the same fixed buckets as TP03-A.

## Equivalence note (A/B)

Matched on: filter semantics, outstanding exclusion, five-bucket taxonomy/order, boundary days, validation structure, and test count.  
Differ on: domain nouns (`OrderedAmount`/`ReceivedAmount` vs `Amount`/`PaidAmount`) and scenario text.

## Frozen rules

1. `outstanding = OrderedAmount - ReceivedAmount`
2. Exclude rows with `outstanding <= 0`
3. Owner filter case-insensitive when provided
4. Same `daysPastDue` and bucket taxonomy as TP03-A
5. Always return five buckets in frozen order
6. `GrandTotal` = sum of bucket totals

## Validation (400 / `VALIDATION_ERROR`)

- null source
- `OrderedAmount < 0`
- `ReceivedAmount < 0`
- `ReceivedAmount > OrderedAmount`

## Completion rule

Submit when finished or when time limit is reached. Hidden test failures do not keep the timer running.
