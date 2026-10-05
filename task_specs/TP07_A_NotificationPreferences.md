# TP07-A: Notification Preferences

| Field | Value |
|---|---|
| Task pair | TP07 |
| Variant | A |
| Capability | Refactoring + defect correction |
| Matched counterpart | TP07-B (Alert Subscriptions) |
| Starting point | **Defective legacy service** (not a blank stub) |
| Hidden tests | 20 |

## Task model
Unlike TP01–TP06, you receive working-but-wrong legacy code. Correct defects, tighten validation, and clean structural smells while preserving `INotificationPreferencesService`.

## Known defects to fix
1. Upsert accepts blank `UserId` / `Channel` and invalid `Frequency` / `Channel`
2. Get returns **400 / BAD_REQUEST** for missing records (must be **404 / NOT_FOUND**)
3. Keys are case-sensitive (must be case-insensitive)
4. Duplicated key construction / magic separators (refactor smell)

## Correct behavior
**Channels:** `Email`, `Sms`, `Push`  
**Frequencies:** `Immediate`, `Daily`, `Weekly`

### Upsert
- null preference → 400 / `VALIDATION_ERROR`
- blank UserId/Channel/Frequency or unknown values → 400 / `VALIDATION_ERROR`
- store trimmed UserId and canonical Channel/Frequency
- overwrite on same user+channel

### Get
- blank UserId or unknown Channel → 400 / `VALIDATION_ERROR`
- missing preference → 404 / `NOT_FOUND`
- case-insensitive user/channel lookup; trim inputs

## Completion rule
Submit when finished or at time limit; hidden tests scored post-hoc. Static analysis still applies to your refactored Participant code.
