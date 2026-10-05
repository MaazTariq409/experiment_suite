param(
  [Parameter(Mandatory = $true)][string]$Task,
  [Parameter(Mandatory = $true)][ValidateSet('A','B')][string]$Variant,
  [string]$OutDir = "..\participant_packages"
)

$root = Split-Path -Parent $PSScriptRoot
$destRoot = Join-Path $root $OutDir
$dest = Join-Path $destRoot "$Task$Variant"
New-Item -ItemType Directory -Force -Path $dest | Out-Null

Copy-Item -Recurse -Force (Join-Path $root "Tasks/$Task/$Variant/Contracts") (Join-Path $dest "Contracts")
Copy-Item -Recurse -Force (Join-Path $root "Tasks/$Task/$Variant/Participant") (Join-Path $dest "Participant")
Copy-Item -Recurse -Force (Join-Path $root "src/Enterprise.Shared") (Join-Path $dest "Enterprise.Shared")

$spec = Get-ChildItem (Join-Path $root "task_specs") -Filter "$Task=${Variant}_*" -ErrorAction SilentlyContinue
# fallback glob
Get-ChildItem (Join-Path $root "task_specs") -Filter "${Task}_${Variant}_*.md" | ForEach-Object {
  Copy-Item $_.FullName (Join-Path $dest "TASK_SPEC.md") -Force
}

Write-Host "Participant package written to $dest"
Write-Host "NOTE: Evaluation/ intentionally excluded."
