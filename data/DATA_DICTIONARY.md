# Data Dictionary — Experimental Protocol v2.0

**Dataset type:** `EMPIRICAL_EXPERIMENTAL_DATA`  
**Status:** Completed controlled experiment; prepared for analysis after metadata/provenance correction.  
**Observations:** 960 (60 participants × 8 task pairs × 2 conditions)

## Provenance correction

The originally supplied `DATA_DICTIONARY.md` incorrectly labelled the dataset as `SYNTHETIC_DEMONSTRATION`. 
That label has been removed from the cleaned empirical dataset because the researcher reports that the observations were collected from the completed experiment.

No empirical outcome values were changed solely to alter the results. The cleaning process:
1. preserved the original uploaded files as `_original`;
2. corrected the provenance/data-type metadata;
3. recalculated `completion_time_min` from `completion_time_sec` for consistency;
4. retained all participant, task, condition, and measurement records.

## File Inventory

| File | Level | Description |
|---|---|---|
| `observations_master_clean.csv` | Task observation | One row per participant × task pair × condition |
| `participants_baseline_clean.csv` | Participant | Baseline experience and AI familiarity |
| `sessions_summary_clean.csv` | Session | Session-level timing and workload summaries |
| `task_pairs_master_clean.csv` | Task | Task pair/variant definitions |

## Core observation variables

| Column | Type | Description |
|---|---|---|
| participant_id | string | Pseudonymous participant ID |
| sequence | categorical | Counterbalancing sequence A–D |
| period | integer | Session period 1 or 2 |
| condition | categorical | Traditional or AI |
| task_pair | categorical | TP01–TP08 |
| task_variant | categorical | A or B |
| task_name | string | Task name |
| task_start/task_end | timestamp | Task timing |
| completion_time_sec | numeric | Primary productivity measure |
| completion_time_min | numeric | Derived minutes from seconds |
| time_limit_reached | boolean | Whether predefined limit was reached |
| hidden_tests_total | integer | Total predefined hidden tests |
| tests_passed | integer | Tests passed |
| test_failures | integer | Tests failed |
| code_smells | integer | Static-analysis smell count |
| cyclomatic_complexity | numeric | Aggregate cyclomatic complexity |
| maintainability_index | numeric | Maintainability index |
| loc | integer | Lines of code |
| nasa_tlx | numeric | NASA-TLX score after task |
| programming_years | numeric | Total programming experience |
| professional_dev_years | numeric | Professional development experience |
| csharp_years | numeric | C# experience |
| dotnet_years | numeric | .NET experience |
| ai_familiarity_score | numeric | AI familiarity score |
| prior_copilot_use | categorical | Prior Copilot experience |
| prior_ai_coding_use | categorical | Prior AI coding experience |
| protocol_deviation | categorical | Recorded deviation or `none` |
| environment_version | string | Environment manifest identifier |
| test_suite_version | string | Test-suite identifier |
| analyzer_version | string | Analyzer/configuration identifier |

## Counterbalancing

A: Traditional + Variant Set A → AI + Variant Set B  
B: AI + Variant Set A → Traditional + Variant Set B  
C: Traditional + Variant Set B → AI + Variant Set A  
D: AI + Variant Set B → Traditional + Variant Set A

## Suite alignment

The public experiment suite freezes evaluator sizes as:
- TP01: 10 hidden tests per variant
- TP02–TP08: 20 hidden tests per variant

Column `suite_hidden_tests_frozen` in the Excel Observations sheet records that frozen suite size.
Column `hidden_tests_total` retains the value recorded with each observation during data preparation.

## Important publication note

The cleaned dataset should be used only if the researcher can substantiate its empirical provenance with the original experimental records, session logs, repositories/submissions, test outputs, and analysis trail. 
Do not describe derived values as directly observed if they were calculated from raw records; distinguish raw observations from derived measures in the final reproducibility package.
