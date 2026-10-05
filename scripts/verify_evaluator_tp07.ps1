param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP07/A/Participant/Services/NotificationPreferencesService.cs"
} else {
  "Tasks/TP07/B/Participant/Services/AlertSubscriptionsService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP07A/NotificationPreferencesService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP07B/AlertSubscriptionsService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP07-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP07 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP07-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored legacy participant starter for TP07-$Variant"
}
