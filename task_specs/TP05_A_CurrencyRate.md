# TP05-A: Currency Rate Service

| Field | Value |
|---|---|
| Task pair | TP05 |
| Variant | A |
| Capability | External API integration |
| Matched counterpart | TP05-B (Shipping Rate Service) |
| Implementation | Frozen `ICurrencyRateService` using provided `IExternalQuoteClient` |
| Hidden tests | 20 |

## Enterprise characteristics
- Integration through a frozen external-client abstraction (no direct HTTP)
- Deterministic timeout/failure mapping to gateway status codes
- Input validation before calling the external dependency

## Validation (400 / `VALIDATION_ERROR`)
- null request
- `From`/`To` not exactly 3 letters (after trim)
- `From` equals `To` (case-insensitive)
- `Amount <= 0`

Do **not** call the external client when validation fails.

## External mapping
1. If `TimedOut` → 504 / `GATEWAY_TIMEOUT` (precedence over Failed)
2. Else if `Failed` **or** `Rate <= 0` → 502 / `BAD_GATEWAY`
3. Else success:
   - `ConvertedAmount = Amount * Rate` (exact decimal multiplication)
   - `From`/`To` uppercased
   - `Source = "External"`

## Completion rule
Submit when finished or at time limit; hidden tests scored post-hoc.
