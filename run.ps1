<#
.SYNOPSIS
    Runs one demo scenario on the .NET 10 SDK, the .NET 11 SDK, or both.

.EXAMPLE
    ./run.ps1 -List
.EXAMPLE
    ./run.ps1 01-union                    # "new" variant on both SDKs, one after the other
.EXAMPLE
    ./run.ps1 01-union -Variant old       # the "before" code on both SDKs
.EXAMPLE
    ./run.ps1 09 -Target net10            # left terminal
    ./run.ps1 09 -Target net11            # right terminal
.EXAMPLE
    ./run.ps1 01 -Pause                   # wait for Enter between the two SDKs
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)][string]$Scenario,
    [ValidateSet('old', 'new')][string]$Variant = 'new',
    [ValidateSet('net10', 'net11', 'both')][string]$Target = 'both',
    [switch]$List,
    [switch]$Pause,
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$scenarioRoot = Join-Path $root 'scenarios'

if ($Clean) {
    foreach ($t in 'net10', 'net11') {
        $work = Join-Path (Join-Path $root $t) 'work'
        if (Test-Path $work) { Remove-Item $work -Recurse -Force }
    }
    Write-Host 'work folders removed.'
    return
}

if ($List -or -not $Scenario) {
    Get-ChildItem $scenarioRoot -Recurse -File -Filter '*.cs' |
        Sort-Object Name |
        ForEach-Object { '{0,-8} {1}' -f $_.Directory.Name, $_.Name }
    return
}

$file = Get-ChildItem $scenarioRoot -Recurse -File -Filter "$Scenario*.$Variant.cs" |
    Sort-Object Name |
    Select-Object -First 1
if (-not $file) {
    Write-Warning "No '$Variant' variant for '$Scenario'. Run ./run.ps1 -List to see what exists."
    return
}

# One stable, valid file name per scenario so the file-based-app build cache is reused between runs.
$appName = 'demo_' + ($file.BaseName -replace '[^A-Za-z0-9]', '_') + '.cs'

function Invoke-Demo([string]$tfmFolder) {
    $dir = Join-Path $root $tfmFolder
    $work = Join-Path $dir 'work'
    New-Item -ItemType Directory -Force -Path $work | Out-Null
    Copy-Item $file.FullName (Join-Path $work $appName) -Force

    # Running from <tfmFolder>/work makes dotnet pick up <tfmFolder>/global.json (SDK choice)
    # and <tfmFolder>/Directory.Packages.props (package versions) by walking up the tree.
    Push-Location $work
    try {
        $sdk = (dotnet --version 2>&1 | Select-Object -First 1)
        Write-Host ''
        Write-Host ("=== {0}  |  {1}  |  SDK {2} ===" -f $file.Name, $tfmFolder, $sdk) -ForegroundColor Cyan
        dotnet run $appName
        Write-Host ("--- exit code {0} ---" -f $LASTEXITCODE) -ForegroundColor DarkGray
    }
    finally {
        Pop-Location
    }
}

$targets = if ($Target -eq 'both') { @('net10', 'net11') } else { @($Target) }
foreach ($t in $targets) {
    Invoke-Demo $t
    if ($Pause -and $t -ne $targets[-1]) { Read-Host 'Press Enter for the next SDK' | Out-Null }
}
