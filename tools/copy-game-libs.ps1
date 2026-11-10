<#
.SYNOPSIS
    Copies the Koikatsu official-VR assemblies needed for compilation into lib/.

.EXAMPLE
    ./tools/copy-game-libs.ps1 -GameDir "C:\Illusion\Koikatsu Party"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir
)

$ErrorActionPreference = 'Stop'

$candidates = @(
    (Join-Path $GameDir 'Koikatsu Party VR_Data\Managed'),
    (Join-Path $GameDir 'KoikatuVR_Data\Managed')
)
$managed = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $managed) {
    throw "No VR Managed folder found under '$GameDir'. Expected one of:`n  $($candidates -join "`n  ")"
}

$libDir = Join-Path $PSScriptRoot '..\lib'
New-Item -ItemType Directory -Force -Path $libDir | Out-Null

$files = 'Assembly-CSharp.dll', 'Assembly-CSharp-firstpass.dll', 'UnityEngine.dll', 'UnityEngine.UI.dll'
foreach ($f in $files) {
    Copy-Item -Path (Join-Path $managed $f) -Destination $libDir -Force
    Write-Host "copied $f"
}
Write-Host "Done. Assemblies copied from $managed"
