# TP06-A: Employee Import

| Field | Value |
|---|---|
| Task pair | TP06 |
| Variant | A |
| Capability | File import + validation |
| Matched counterpart | TP06-B (Product Catalog Import) |
| Implementation | Frozen `IEmployeeImportService` |
| Hidden tests | 20 |

## CSV format
Exactly three comma-separated fields: `Key,Name,Value`

## Rules
1. Null `rows` → 400 / `VALIDATION_ERROR`
2. Process every row; do not stop on first error
3. `LineNumber <= 0` or blank `Raw` → reject `MALFORMED`
4. Not exactly 3 fields → `MALFORMED`
5. Empty Key/Name (after trim) → `EMPTY_KEY` / `EMPTY_NAME`
6. Value must parse as invariant-culture decimal and be `>= 0` else `INVALID_VALUE`
7. Duplicate Key (case-insensitive) → first accepted, later `DUPLICATE_KEY`
8. Error text format: `Line {n}: {REASON}`
9. Errors ordered by line number ascending
10. `Accepted + Rejected == rows.Count`

## Completion rule
Submit when finished or at time limit; hidden tests scored post-hoc.
