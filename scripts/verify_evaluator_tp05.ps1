param(
  [ValidateSet('A','B')]$Variant = 'A'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$serviceRel = if ($Variant -eq 'A') {
  "Tasks/TP05/A/Participant/Services/CurrencyRateService.cs"
} else {
  "Tasks/TP05/B/Participant/Services/ShippingRateService.cs"
}
$working = if ($Variant -eq 'A') {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP05A/CurrencyRateService.Working.cs"
} else {
  Join-Path $root "reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE/TP05B/ShippingRateService.Working.cs"
}

$participantService = Join-Path $root $serviceRel
$backup = "$participantService.bak"
Copy-Item $participantService $backup -Force
try {
  Copy-Item $working $participantService -Force
  Write-Host "Injected reference implementation for TP05-$Variant"
  & (Join-Path $PSScriptRoot 'evaluate.ps1') -Task TP05 -Variant $Variant
  if ($LASTEXITCODE -ne 0) { throw "Evaluation failed for TP05-$Variant" }
}
finally {
  Copy-Item $backup $participantService -Force
  Remove-Item $backup -Force
  Write-Host "Restored participant stub for TP05-$Variant"
}
