# TP05-B: Shipping Rate Service

| Field | Value |
|---|---|
| Task pair | TP05 |
| Variant | B |
| Capability | External API integration |
| Matched counterpart | TP05-A (Currency Rate Service) |
| Implementation | Frozen `IShippingRateService` using provided `IExternalQuoteClient` |
| Hidden tests | 20 |

## Equivalence
Same validation, timeout/failure precedence, multiplication rule, and Source semantics as TP05-A. Domain codes represent shipping zones/airports (e.g., `LAX`/`JFK`) rather than currencies.

## Rules
Identical operational rules to TP05-A (see that spec).
