# Reviewer package guide

Repository: https://github.com/MaazTariq409/experiment_suite

This repository is the reproducibility package for the controlled crossover study of AI-assisted enterprise-oriented .NET development.

## What reviewers asked for → where it is

| Reviewer request | Location |
|---|---|
| Exact task definitions / enterprise orientation | `docs/01_ENTERPRISE_ORIENTATION.md`, `task_specs/` |
| What participants saw | `Tasks/*/Participant/`, `Tasks/*/Contracts/`, `docs/04_PARTICIPANT_PACKAGE.md` |
| How independent solutions are tested | `docs/02_EVALUATION_MODEL.md`, `Tasks/*/Evaluation/` |
| Completion / stopping rule | `docs/03_COMPLETION_CRITERIA.md` |
| Static-analysis tool/version/ruleset/aggregation | `docs/05_STATIC_ANALYSIS.md`, `analyzer/` |
| Anonymized data + scripts | `data/` |
| A/B matched variants | `Tasks/TP0x/A` and `Tasks/TP0x/B`, `data/csv/task_pairs_master.csv` |
| Evidence map | `docs/00_REVIEWER_EVIDENCE_MAP.md` |

## Quick start

### Build / evaluate one task variant

```powershell
dotnet build EnterpriseTasks.sln
./scripts/evaluate.ps1 -Task TP02 -Variant A
```

Participant packages intentionally exclude Evaluation projects:

```powershell
./scripts/pack_participant.ps1 -Task TP02 -Variant A
```

### Inspect data

Open `data/AI_NET_Experimental_Dataset_SuiteAligned.xlsx` or the CSVs under `data/csv/`.

## Design summary

- 60 participants, counterbalanced sequences A–D
- 8 matched task pairs × 2 non-identical variants (A/B)
- Within-subject Traditional vs AI conditions
- 960 task-level observations
- Outcomes: completion time, hidden-test failures, code smells, cyclomatic complexity, maintainability index, LOC, NASA-TLX
