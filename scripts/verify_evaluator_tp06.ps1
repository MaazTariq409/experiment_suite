param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP06/A/Participant/Services/EmployeeImportService.cs"
} else {
  "Tasks/TP06/B/Participant/Services/ProductCatalogImportService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP06A/EmployeeImportService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP06B/ProductCatalogImportService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP06-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP06 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP06-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP06-$Variant"
}
