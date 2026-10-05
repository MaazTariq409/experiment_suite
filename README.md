# Enterprise .NET Experiment Suite

Reproducibility package for the controlled crossover study of AI-assisted enterprise-oriented .NET development (TP01–TP08, variants A/B).

## What this package proves to reviewers

1. Tasks are **not** standalone toy programs. Each variant is completed **inside a supplied enterprise project** with frozen contracts, DTOs, and persistence abstractions.
2. Participants implement functionality **largely from scratch** in marked service classes (TP07 starts from defective legacy code by design).
3. Independently written solutions remain testable because evaluation binds only to **frozen interface contracts**, not to participant-specific internal design.
4. Hidden tests are separated from the participant package.

## Layout

```
experiment_suite/
  src/Enterprise.Shared/           Shared result/time/security primitives
  Tasks/TP0x/{A|B}/
    Contracts/                     FROZEN — interfaces + DTOs
    Participant/                   What developers edit
    Evaluation/                    HIDDEN tests (reviewer/repro package)
  task_specs/                      Participant-facing requirement sheets
  docs/                            Methodological documentation
  reference_solutions/PRIVATE_...  Private oracles (not for participants)
  scripts/                         Packaging and evaluation helpers
```

## Build

```powershell
dotnet build EnterpriseTasks.sln
```

## Evaluate one variant (example TP01-A)

```powershell
./scripts/evaluate.ps1 -Task TP01 -Variant A
```

The script runs only the Evaluation project for that variant and prints failed-test counts.

## Participant package vs full package

| Content | Participant zip | Reviewer/repro zip |
|---|---|---|
| Contracts + Participant stubs + task specs | Yes | Yes |
| Evaluation hidden tests | No | Yes |
| Reference solutions | No | Optional / private |
| Analyzer config | Manifest only | Full config |

Use `./scripts/pack_participant.ps1` to emit participant zips without Evaluation folders.

## Related manuscript claims this package supports

- Enterprise-oriented operationalization
- Interface-contract based automated correctness
- Completion rule independent of test pass/fail
- Matched A/B variants with shared capability, different domain labels
