param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP08/A/Participant/Services/ResourceUtilizationService.cs"
} else {
  "Tasks/TP08/B/Participant/Services/SupportTicketSlaService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP08A/ResourceUtilizationService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP08B/SupportTicketSlaService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP08-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP08 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP08-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP08-$Variant"
}
