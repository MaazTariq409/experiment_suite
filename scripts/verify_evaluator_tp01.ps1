param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$task = 'TP01'
$participantService = if ($Variant -eq 'A') {
  Join-Path $root "Tasks/TP01/A/Participant/Services/MaintenanceRecordService.cs"
} else {
  Join-Path $root "Tasks/TP01/B/Participant/Services/SupplierContractService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP01A/MaintenanceRecordService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP01B/SupplierContractService.Working.cs"
}

if (-not (Test-Path $working)) { throw "Working reference not found: $working" }

$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP01-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP01 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP01-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP01-$Variant"
}
