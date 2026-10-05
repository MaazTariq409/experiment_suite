param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP03/A/Participant/Services/InvoiceAgingService.cs"
} else {
  "Tasks/TP03/B/Participant/Services/PurchaseOrderFulfilmentService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP03A/InvoiceAgingService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP03B/PurchaseOrderFulfilmentService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP03-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP03 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP03-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP03-$Variant"
}
