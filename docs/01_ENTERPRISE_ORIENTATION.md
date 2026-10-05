# Operational definition: enterprise-oriented .NET tasks

## Definition used in this study

A task is treated as **enterprise-oriented** when it requires implementation work inside a supplied multi-layer .NET application baseline and includes **at least four** of the following characteristics:

1. **Layered architecture** — separation of contracts/API surface, domain/service logic, and persistence/integration abstractions.
2. **Business-rule enforcement** — status lifecycles, authorization predicates, validation thresholds, or aggregation rules that are not reducible to a single algorithmic puzzle.
3. **Deterministic enterprise responses** — explicit success/error codes analogous to service-layer/HTTP semantics used in business systems.
4. **Domain scenario** — named business entities, identifiers, and operational context (maintenance, contracts, access policy, fulfilment, SLA, etc.).
5. **Integration or persistence boundary** — repository, file import, or external-service abstraction provided by the baseline.
6. **Quality constraints beyond “code runs”** — hidden functional tests plus static-analysis outcomes on the submitted implementation.

## Why these are not ordinary standalone exercises

Participants do **not** create a new console app from a blank folder.
They receive a predefined solution structure and must implement service behavior that plugs into frozen contracts used by the evaluation harness.

Ordinary programming exercises typically:
- start from a blank file or single-function template;
- omit authorization/workflow/persistence boundaries;
- evaluate only stdout or a few public tests visible to the solver.

This suite requires enterprise-style boundaries even when the amount of code is intentionally bounded for experimental control.

## Standalone vs enhancement

| Question | Answer in this suite |
|---|---|
| Standalone greenfield application? | No |
| Modify an existing large production monolith? | No |
| Implement missing business functionality inside a supplied baseline? | **Yes** |
| Starting codebase contents | Frozen contracts, DTOs, repository abstractions, in-memory persistence helpers, task specification |
| What participants write | Service implementations (and helpers) satisfying the contract |

This is best described as **constrained greenfield implementation inside a supplied enterprise scaffold**, not as maintenance of a legacy production system (except TP07, which is refactoring of defective supplied code).
