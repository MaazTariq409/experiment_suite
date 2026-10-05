param(
  [Parameter(Mandatory = $true)][string]$Task,
  [Parameter(Mandatory = $true)][ValidateSet('A','B')][string]$Variant
)

$root = Split-Path -Parent $PSScriptRoot
$evalProj = Join-Path $root "Tasks/$Task/$Variant/Evaluation/${Task}${Variant}.Evaluation.csproj"
if (-not (Test-Path $evalProj)) { throw "Evaluation project not found: $evalProj" }

Write-Host "Evaluating $Task-$Variant ..."
dotnet test $evalProj --nologo
