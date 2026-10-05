param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP04/A/Participant/Services/LeaveApprovalService.cs"
} else {
  "Tasks/TP04/B/Participant/Services/ExpenseReimbursementService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP04A/LeaveApprovalService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP04B/ExpenseReimbursementService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP04-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP04 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP04-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP04-$Variant"
}
