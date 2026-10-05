# TP03-A: Invoice Aging

| Field | Value |
|---|---|
| Task pair | TP03 |
| Variant | A |
| Capability | Database query + aggregation |
| Matched counterpart | TP03-B (Purchase Order Fulfilment) |
| Implementation model | Implement frozen `IInvoiceAgingService` inside supplied project |
| Standalone program? | **No** |
| Hidden tests | 20 confirmatory tests |

## Business scenario

Build an accounts-receivable aging report from invoice rows. Filter by customer (`OwnerKey`), compute outstanding balances, classify by days past due relative to `AsOfDate`, and aggregate into fixed buckets.

## Enterprise-oriented characteristics

- Deterministic financial aggregation with explicit bucket taxonomy
- Owner filtering and zero-balance exclusion
- Boundary-sensitive date logic
- Validation of ledger-like amount invariants

## Frozen rules

1. `outstanding = Amount - PaidAmount`
2. Exclude rows with `outstanding <= 0`
3. If `OwnerKey` is provided, filter case-insensitively; if null/blank, include all owners
4. `daysPastDue = AsOfDate.DayNumber - DueDate.DayNumber`
5. Bucket classification:
   - `Current`: daysPastDue ≤ 0
   - `1-30`: 1–30
   - `31-60`: 31–60
   - `61-90`: 61–90
   - `90+`: ≥ 91
6. Always return **exactly these five buckets in this order**, including zeros
7. `GrandTotal` = sum of bucket `TotalAmount` values

## Validation (400 / `VALIDATION_ERROR`)

- `source` is null
- any row with `Amount < 0`
- any row with `PaidAmount < 0`
- any row with `PaidAmount > Amount`

## Completion rule

Submit when finished or when time limit is reached. Hidden test failures do not keep the timer running.
