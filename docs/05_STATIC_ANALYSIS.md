# Frozen static-analysis configuration

| Item | Frozen value |
|---|---|
| Code-smell tool | **SonarAnalyzer.CSharp** |
| Code-smell version | **10.35.0.4138** (NuGet) |
| Ruleset | `analyzer/EnterpriseStudy.ruleset` |
| EditorConfig | `analyzer/.editorconfig` |
| Scored smell IDs | `analyzer/smell_rules.txt` (19 rules) |
| Metrics tool | **Microsoft.CodeAnalysis.Metrics** |
| Metrics version | **5.6.0** (NuGet; MI & CC extraction) |
| Analysis scope | Participant-authored `Participant/**/*.cs` only (exclude `obj/`, `bin/`, Contracts) |
| Config hash script | `scripts/hash_analyzer_config.ps1` |

## Outcome definitions

### `code_smells` (lower better)
Count of Sonar diagnostics in scope whose rule ID is listed in `smell_rules.txt`.  
Instance counting: each reported location increments the count by 1.

### `cyclomatic_complexity` (lower better)
Method-level cyclomatic complexity from Microsoft.CodeAnalysis.Metrics (Roslyn-based).  
**Aggregation (frozen):** arithmetic **mean** of method-level CC over all analyzed methods in Participant scope.  
Empty-method edge case: methods with no executable body contribute CC = 1 if reported by the tool; if a submission has zero methods, record CC as missing and flag protocol deviation.

### `maintainability_index` (higher better)
Maintainability Index from Microsoft.CodeAnalysis.Metrics, using the Visual Studio MI formula embedded in that tool:

\[
MI = \max\left(0,\ \frac{171 - 5.2\ln(HV) - 0.23\,CC - 16.2\ln(LOC)}{171}\times 100\right)
\]

**Aggregation (frozen):** arithmetic **mean** of member-level MI values over analyzed members in Participant scope (same member set used for CC where available).

### `loc` (descriptive)
Physical lines of code in Participant scope as reported by the metrics tool (or `dotnet` line-count fallback documented in `analyze.ps1`).

## How to run

```powershell
./scripts/analyze_participant.ps1 -Task TP02 -Variant A
```

## Reproducibility note for the manuscript

Report these exact package versions and the config hash produced by `hash_analyzer_config.ps1`.  
Do not mix Sonar versions across participants.
