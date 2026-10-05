# How independently written solutions are tested

## Reviewer question

“How can the same test suite run against implementations created independently by different developers?”

## Answer

Because tests never depend on participants’ private class designs.
They depend only on **frozen contracts**.

```
Participant writes code here          Evaluator binds here
-----------------------------         -----------------------
Participant/Services/*.cs      --->   Contracts/I*Service.cs
                                      Contracts DTO shapes
                                      StatusCode / ErrorCode semantics
```

### Fixed artifacts (identical for every participant)
- Interface names and method signatures
- DTO property names and types
- Allowed status values / business-rule tables in the task spec
- Error code strings required by tests (`VALIDATION_ERROR`, `NOT_FOUND`, ...)
- Repository abstractions / external client abstractions where applicable

### Free artifacts (may differ across participants)
- Private helper methods
- Internal decomposition
- Validation implementation style
- Additional files under Participant (as long as contracts remain unchanged)

## Technical binding

Evaluation projects:
1. reference `*.Contracts` and `*.Participant`;
2. construct the participant service with provided dependencies (in-memory repository, fake external client, fixed clock);
3. invoke interface methods;
4. assert on `OperationResult` success/failure, status codes, and DTO field values.

No source-code text matching is used for correctness scoring.

## Visibility policy

- Participants receive **Contracts + Participant + task spec**.
- Participants do **not** receive Evaluation projects.
- Optional visible smoke examples may be provided in future packs; the confirmatory suite remains hidden.
- AI tools in the participant environment therefore cannot read hidden tests unless a packaging error occurs.

## Build failure / incomplete submission policy

| Situation | Scoring rule |
|---|---|
| Project does not compile | All hidden tests count as failed |
| Interface not implemented / NotImplementedException | Corresponding tests fail |
| Partial implementation | Failed tests counted individually |
| Submission after time limit | Same evaluation applied; completion time censored at limit |

## Test-to-requirement traceability

Each Evaluation fact method maps to an explicit requirement in `task_specs/*.md`.
Example (TP01): `Create_DuplicateCode_ReturnsConflict` ↔ “Reject duplicate business codes with conflict semantics.”
