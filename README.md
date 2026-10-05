# Enterprise .NET Experiment Suite

Reproducibility package for a controlled crossover study of AI-assisted enterprise-oriented .NET development (JOCL revision materials).

**Public repository:** https://github.com/MaazTariq409/experiment_suite

## Contents

| Path | Purpose |
|---|---|
| `Tasks/TP01`–`TP08` | Frozen contracts, participant packages, hidden evaluators (variants A/B) |
| `task_specs/` | Participant-facing requirement sheets |
| `analyzer/` | SonarAnalyzer.CSharp 10.35.0.4138 ruleset + config hash |
| `data/` | Suite-aligned anonymized dataset (Excel + CSV) |
| `docs/` | Methodological documentation answering reviewer concerns |
| `scripts/` | Evaluate / pack / verify / rebuild Excel |
| `REVIEWER_PACKAGE.md` | Start here for peer review |

## Study snapshot

- Design: counterbalanced within-subject crossover (sequences A–D)
- N = 60 developers; 8 task pairs; Traditional vs AI
- 960 observations
- .NET 8 / C# enterprise-oriented service tasks

## Build and evaluate

```powershell
dotnet build EnterpriseTasks.sln
./scripts/evaluate.ps1 -Task TP01 -Variant A
./scripts/verify_evaluator_tp02.ps1 -Variant A   # injects private oracle, runs tests, restores stub
```

## Data

- Excel: `data/AI_NET_Experimental_Dataset_SuiteAligned.xlsx`
- CSV: `data/csv/`
- Dictionary: `data/DATA_DICTIONARY.md`

This workbook supersedes the older flat `T1`–`T8` Excel schema.

## License / use

Intended for peer review and reproducibility of the reported experiment. Do not redistribute reference oracles as participant worksheets for new cohorts without removing `reference_solutions/`.
