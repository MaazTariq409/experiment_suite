param(
  [Parameter(Mandatory = $true)][string]$Task,
  [Parameter(Mandatory = $true)][ValidateSet('A','B')][string]$Variant
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$participantProj = Get-ChildItem -Path (Join-Path $root "Tasks/$Task/$Variant") -Recurse -Filter "*.Participant.csproj" | Select-Object -First 1
if (-not $participantProj) { throw "Participant project not found for $Task-$Variant" }

$props = Join-Path $root "analyzer/Participant.Analyzer.props"
$outDir = Join-Path $root "analyzer/out/$Task$Variant"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host "Building participant project with frozen SonarAnalyzer.CSharp 10.35.0.4138 ..."
dotnet build $participantProj.FullName -p:CustomAfterMicrosoftCSharpTargets=$props -p:CodeAnalysisRuleSet="$(Join-Path $root 'analyzer\EnterpriseStudy.ruleset')" --nologo |
  Tee-Object -FilePath (Join-Path $outDir "build.log")

# Count scored smells from build log (Sxxxx warnings)
$smellIds = Get-Content (Join-Path $root "analyzer/smell_rules.txt") |
  Where-Object { $_ -match '^S\d+' } |
  ForEach-Object { ($_ -split '\s+')[0] }

$log = Get-Content (Join-Path $outDir "build.log") -Raw
$count = 0
foreach ($id in $smellIds) {
  $matches = [regex]::Matches($log, [regex]::Escape($id))
  $count += $matches.Count
}

# Metrics via Microsoft.CodeAnalysis.Metrics if installed; otherwise write placeholder instructions
$metricsOut = Join-Path $outDir "metrics.xml"
$metricsTool = Get-Command Metrics -ErrorAction SilentlyContinue
if ($metricsTool) {
  Metrics /project:$($participantProj.FullName) /out:$metricsOut | Out-Null
  Write-Host "Metrics XML written to $metricsOut"
} else {
  @"
Microsoft.CodeAnalysis.Metrics not installed in this environment.
Install once:
  dotnet tool install -g Microsoft.CodeAnalysis.Metrics --version 5.6.0
Then re-run this script.
Aggregation rules remain as documented in docs/05_STATIC_ANALYSIS.md
"@ | Set-Content (Join-Path $outDir "METRICS_TOOL_REQUIRED.txt")
}

$result = [pscustomobject]@{
  task = $Task
  variant = $Variant
  code_smells = $count
  analyzer_package = "SonarAnalyzer.CSharp"
  analyzer_version = "10.35.0.4138"
  ruleset = "analyzer/EnterpriseStudy.ruleset"
}
$result | ConvertTo-Json | Set-Content (Join-Path $outDir "code_smells.json")
Write-Host ($result | Format-List | Out-String)
