<#
.SYNOPSIS
    Runs every scenario once on both SDKs and throws the output away, so the NuGet cache and the
    file-based-app build cache are warm before you go on stage. Failures are expected for some
    scenarios on .NET 10 (that is the point of the demo).
#>
$ErrorActionPreference = 'Continue'
$run = Join-Path $PSScriptRoot 'run.ps1'
Get-ChildItem (Join-Path $PSScriptRoot 'scenarios') -Recurse -File -Filter '*.cs' | Sort-Object Name | ForEach-Object {
    $m = [regex]::Match($_.Name, '^(?<s>.+)\.(?<v>old|new)\.cs$')
    if ($m.Success) {
        Write-Host ("warming {0} ({1})" -f $m.Groups['s'].Value, $m.Groups['v'].Value)
        & $run $m.Groups['s'].Value -Variant $m.Groups['v'].Value *> $null
    }
}
Write-Host 'done.'
