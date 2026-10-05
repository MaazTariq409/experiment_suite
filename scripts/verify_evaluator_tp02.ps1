param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP02/A/Participant/Services/ProjectAccessService.cs"
} else {
  "Tasks/TP02/B/Participant/Services/DocumentAccessService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP02A/ProjectAccessService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP02B/DocumentAccessService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP02-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP02 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP02-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP02-$Variant"
}
