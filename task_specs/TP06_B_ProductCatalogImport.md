# TP06-B: Product Catalog Import

| Field | Value |
|---|---|
| Task pair | TP06 |
| Variant | B |
| Capability | File import + validation |
| Matched counterpart | TP06-A (Employee Import) |
| Implementation | Frozen `IProductCatalogImportService` |
| Hidden tests | 20 |

## Equivalence
Same CSV grammar, validation reasons, duplicate-key semantics, and summary accounting as TP06-A. Domain labels differ (product SKU/name/price vs employee key/name/value).

## Rules
Identical operational rules to TP06-A.
