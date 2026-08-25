param(
    [string]$RepoRoot = "",
    [string]$VamRoot = "F:\sim\vam",
    [string]$VamManagedDir = "",
    [string]$Configuration = "Release",
    [switch]$Clean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-Directory {
    param([string]$PathValue)
    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function Get-RequiredFile {
    param([string]$PathValue, [string]$Label)
    if (-not (Test-Path -LiteralPath $PathValue -PathType Leaf)) {
        throw "Missing $Label`: $PathValue"
    }
    return (Resolve-Path -LiteralPath $PathValue).Path
}

function Resolve-Compiler {
    $candidates = @(
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }
    throw "C# compiler not found."
}

$resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} else {
    (Resolve-Path $RepoRoot).Path
}
$configPath = Get-RequiredFile -PathValue (Join-Path $resolvedRepoRoot "config\fa_radar2.version.json") -Label "Radar 2 version config"
$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
$sourcePath = Get-RequiredFile -PathValue (Join-Path $resolvedRepoRoot ([string]$config.sourceRelativePath).Replace('/', '\')) -Label "Radar 2 source"
$presetPath = Get-RequiredFile -PathValue (Join-Path $resolvedRepoRoot ([string]$config.presetRelativePath).Replace('/', '\')) -Label "Radar 2 preset"

& (Join-Path $resolvedRepoRoot "eng\Verify-FaRadar2Contract.ps1") -RepoRoot $resolvedRepoRoot

if ([string]::IsNullOrWhiteSpace($VamManagedDir)) {
    $VamManagedDir = Join-Path $VamRoot "VaM_Data\Managed"
}
if (-not (Test-Path -LiteralPath $VamManagedDir -PathType Container)) {
    throw "VaM managed reference directory does not exist: $VamManagedDir"
}
$resolvedManagedDir = (Resolve-Path -LiteralPath $VamManagedDir).Path

$referenceNames = @(
    "Assembly-CSharp.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.PhysicsModule.dll",
    "UnityEngine.JSONSerializeModule.dll",
    "UnityEngine.UI.dll",
    "UnityEngine.UIModule.dll"
)
$references = @()
foreach ($referenceName in $referenceNames) {
    $references += Get-RequiredFile -PathValue (Join-Path $resolvedManagedDir $referenceName) -Label "VaM managed reference $referenceName"
}

$buildRoot = Join-Path $resolvedRepoRoot "build\radar2"
$binDirectory = Join-Path $buildRoot ("bin\" + $Configuration)
$stageDirectory = Join-Path $buildRoot "stage"
$receiptDirectory = Join-Path $buildRoot "receipts"
if ($Clean.IsPresent -and (Test-Path -LiteralPath $buildRoot -PathType Container)) {
    $resolvedBuildRoot = [System.IO.Path]::GetFullPath($buildRoot)
    $expectedPrefix = [System.IO.Path]::GetFullPath($resolvedRepoRoot).TrimEnd('\') + "\build\radar2"
    if (-not [string]::Equals($resolvedBuildRoot.TrimEnd('\'), $expectedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean unexpected build root: $resolvedBuildRoot"
    }
    Remove-Item -LiteralPath $resolvedBuildRoot -Recurse -Force
}
Ensure-Directory $binDirectory
Ensure-Directory $stageDirectory
Ensure-Directory $receiptDirectory

$pluginFileName = [string]$config.pluginFileName
$pluginPath = Join-Path $binDirectory $pluginFileName
$compiler = Resolve-Compiler
$arguments = @(
    "/nologo",
    "/target:library",
    "/optimize+",
    "/warn:4",
    "/out:$pluginPath"
)
foreach ($reference in $references) {
    $arguments += "/reference:$reference"
}
$arguments += $sourcePath
& $compiler @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Radar 2 compile failed with exit code $LASTEXITCODE."
}

& (Join-Path $resolvedRepoRoot "eng\Verify-FaRadar2Contract.ps1") `
    -RepoRoot $resolvedRepoRoot `
    -AssemblyPath $pluginPath

$stagedPluginDirectory = Join-Path $stageDirectory "Custom\Plugins"
$stagedPresetDirectory = Join-Path $stageDirectory "Custom\Atom\Empty"
Ensure-Directory $stagedPluginDirectory
Ensure-Directory $stagedPresetDirectory
$stagedPluginPath = Join-Path $stagedPluginDirectory $pluginFileName
$stagedPresetPath = Join-Path $stagedPresetDirectory (Split-Path -Leaf $presetPath)
Copy-Item -LiteralPath $pluginPath -Destination $stagedPluginPath -Force
Copy-Item -LiteralPath $presetPath -Destination $stagedPresetPath -Force

$commit = (& git -C $resolvedRepoRoot rev-parse HEAD).Trim()
$branch = (& git -C $resolvedRepoRoot branch --show-current).Trim()
$sourceDirty = $false
& git -C $resolvedRepoRoot diff --quiet HEAD -- ([string]$config.sourceRelativePath)
if ($LASTEXITCODE -ne 0) {
    $sourceDirty = $true
}
$timestamp = [DateTime]::UtcNow.ToString("yyyyMMdd_HHmmss")
$receiptPath = Join-Path $receiptDirectory ("fa_radar_2_build_" + $timestamp + ".json")
$receipt = [ordered]@{
    schema = "frameangel.radar2.build-receipt.v1"
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    version = [string]$config.version
    branch = $branch
    commit = $commit
    sourceDirtyAgainstCommit = $sourceDirty
    source = [ordered]@{
        path = $sourcePath
        sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourcePath).Hash
    }
    preset = [ordered]@{
        path = $presetPath
        sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $presetPath).Hash
    }
    artifact = [ordered]@{
        path = $pluginPath
        sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $pluginPath).Hash
        length = (Get-Item -LiteralPath $pluginPath).Length
        publicType = [string]$config.publicType
    }
    staged = @(
        [ordered]@{
            path = $stagedPluginPath
            sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $stagedPluginPath).Hash
            length = (Get-Item -LiteralPath $stagedPluginPath).Length
        },
        [ordered]@{
            path = $stagedPresetPath
            sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $stagedPresetPath).Hash
            length = (Get-Item -LiteralPath $stagedPresetPath).Length
        }
    )
    managedReferenceRoot = $resolvedManagedDir
    verifier = [ordered]@{
        path = "eng/Verify-FaRadar2Contract.ps1"
        sourceContract = "passed-before-compile"
        compiledMetadata = "passed-after-compile"
    }
    installation = "not-performed"
    runtimeObservation = "not-performed"
    operatorAcceptance = "not-performed"
}
$receipt | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $receiptPath -Encoding UTF8

Write-Host ("Radar 2 DLL: " + $pluginPath)
Write-Host ("SHA-256: " + $receipt.artifact.sha256)
Write-Host ("Receipt: " + $receiptPath)
