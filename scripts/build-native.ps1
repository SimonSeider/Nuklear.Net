param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "win-arm64")]
    [string]$Rid
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$NativeSrc = Join-Path $RepoRoot "native"
$BuildDir = Join-Path $NativeSrc "build\$Rid"
$RuntimeDir = Join-Path $RepoRoot "Nuklear.Net.Native\runtimes\$Rid\native"
$LibName = "NuklearNetNative.dll"

$GeneratorArch = switch ($Rid) {
    "win-x64" { "x64" }
    "win-arm64" { "ARM64" }
}

New-Item -ItemType Directory -Force -Path $BuildDir, $RuntimeDir | Out-Null

cmake -S $NativeSrc -B $BuildDir -A $GeneratorArch
cmake --build $BuildDir --config Release

$Candidates = @(
    (Join-Path $BuildDir $LibName),
    (Join-Path $BuildDir "Release\$LibName"),
    (Join-Path $BuildDir "Debug\$LibName")
)

$Built = $Candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $Built) {
    $Built = Get-ChildItem -Path $BuildDir -Recurse -Filter $LibName -File | Select-Object -First 1 -ExpandProperty FullName
}

if (-not $Built) {
    throw "Could not find $LibName under $BuildDir"
}

Copy-Item -Force $Built (Join-Path $RuntimeDir $LibName)
Write-Host "Installed $(Join-Path $RuntimeDir $LibName)"
