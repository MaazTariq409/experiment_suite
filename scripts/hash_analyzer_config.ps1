$root = Split-Path -Parent $PSScriptRoot
$files = @(
  (Join-Path $root "analyzer/EnterpriseStudy.ruleset"),
  (Join-Path $root "analyzer/smell_rules.txt"),
  (Join-Path $root "analyzer/.editorconfig"),
  (Join-Path $root "analyzer/Participant.Analyzer.props"),
  (Join-Path $root "docs/05_STATIC_ANALYSIS.md")
)

$sha = [System.Security.Cryptography.SHA256]::Create()
$ms = New-Object System.IO.MemoryStream
foreach ($f in $files) {
  $bytes = [System.IO.File]::ReadAllBytes($f)
  $ms.Write($bytes, 0, $bytes.Length)
  $ms.WriteByte(0)
}
$hash = -join ($sha.ComputeHash($ms.ToArray()) | ForEach-Object { $_.ToString("x2") })
$short = $hash.Substring(0, 16)
Write-Host "analyzer_config_hash=$short"
Set-Content -Path (Join-Path $root "analyzer/CONFIG_HASH.txt") -Value $short
