param(
    [ValidateSet("All", "Free", "Pro")]
    [string]$Edition = "All",
    [string[]]$VamRoots = @("F:\sim\vam", "C:\vam\virgin-recordable-02"),
    [string]$VamRoot = "",
    [string]$RepoRoot = "",
    [string]$VamManagedDir = "",
    [string]$Configuration = "Release",
    [switch]$SkipBuild,
    [switch]$SkipObfuscation,
    [switch]$SkipPackage,
    [switch]$KeepLegacyLooseScript
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-FaRadarDirectory {
    param([string]$PathValue)

    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function Get-FaRadarFileHashOrEmpty {
    param([string]$PathValue)

    if ([string]::IsNullOrWhiteSpace($PathValue) -or -not (Test-Path -LiteralPath $PathValue -PathType Leaf)) {
        return ""
    }

    return (Get-FileHash -Algorithm SHA256 -LiteralPath $PathValue).Hash
}

function Write-FaRadarJson {
    param(
        [string]$PathValue,
        [object]$Value
    )

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $PathValue -Encoding UTF8
}

$resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} else {
    (Resolve-Path $RepoRoot).Path
}

$effectiveVamRoots = New-Object System.Collections.Generic.List[string]
if (-not [string]::IsNullOrWhiteSpace($VamRoot)) {
    $effectiveVamRoots.Add($VamRoot) | Out-Null
}
foreach ($root in @($VamRoots)) {
    if (-not [string]::IsNullOrWhiteSpace($root) -and -not $effectiveVamRoots.Contains($root)) {
        $effectiveVamRoots.Add($root) | Out-Null
    }
}
if ($effectiveVamRoots.Count -eq 0) {
    throw "No VaM roots were provided."
}

$resolvedVamRoots = New-Object System.Collections.Generic.List[string]
foreach ($root in $effectiveVamRoots) {
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "VaM root does not exist: $root"
    }
    $resolvedVamRoots.Add((Resolve-Path $root).Path) | Out-Null
}

if ([string]::IsNullOrWhiteSpace($VamManagedDir)) {
    $VamManagedDir = Join-Path $resolvedVamRoots[0] "VaM_Data\Managed"
}
if (-not (Test-Path -LiteralPath $VamManagedDir -PathType Container)) {
    throw "VaM managed directory does not exist: $VamManagedDir"
}

# Explicit 0.1.29 filenames keep this deploy helper auditable while version
# metadata remains the source of truth consumed by Build-FaRadar.ps1.
$expectedPluginFileNames = @(
    "fa_radar.free.0.1.29.dll",
    "fa_radar.pro.0.1.29.dll"
)
$anchorPresetFileName = "Preset_FrameAngel_Radar_Empty.vap"
$anchorPresetRelativeDirectory = "Custom\Atom\Empty"
$anchorPresetSource = Join-Path $resolvedRepoRoot "payload\$anchorPresetRelativeDirectory\$anchorPresetFileName"

$buildScript = Join-Path $resolvedRepoRoot "scripts\Build-FaRadar.ps1"
if (-not (Test-Path -LiteralPath $buildScript -PathType Leaf)) {
    throw "Missing edition build helper: $buildScript"
}

$buildArguments = @{
    Edition = $Edition
    RepoRoot = $resolvedRepoRoot
    VamManagedDir = $VamManagedDir
    Configuration = $Configuration
}
if ($SkipBuild.IsPresent) {
    $buildArguments["SkipCompile"] = $true
}
if ($SkipObfuscation.IsPresent) {
    $buildArguments["SkipObfuscation"] = $true
}
if ($SkipPackage.IsPresent) {
    $buildArguments["SkipPackage"] = $true
}

$buildOutput = @(& $buildScript @buildArguments)
if ($LASTEXITCODE -ne 0) {
    throw "FA Radar edition build failed."
}
$buildResult = $buildOutput |
    Where-Object { $null -ne $_.PSObject.Properties["EditionBuilds"] } |
    Select-Object -Last 1
if ($null -eq $buildResult) {
    throw "FA Radar edition build produced no receipt object."
}

$editionBuilds = @($buildResult.EditionBuilds)
if ($editionBuilds.Count -le 0) {
    throw "FA Radar edition build produced no DLL outputs."
}

$deployedDlls = New-Object System.Collections.ArrayList
$deployedPresets = New-Object System.Collections.ArrayList
$archivedLegacyScripts = New-Object System.Collections.ArrayList
$deployAnchorPreset = @($editionBuilds | Where-Object { [string]$_.edition -eq "pro" }).Count -gt 0
if ($deployAnchorPreset -and -not (Test-Path -LiteralPath $anchorPresetSource -PathType Leaf)) {
    throw "Missing Pro Empty anchor preset for deploy: $anchorPresetSource"
}
foreach ($root in $resolvedVamRoots) {
    $destinationDirectory = Join-Path $root "Custom\Plugins"
    Ensure-FaRadarDirectory -PathValue $destinationDirectory

    foreach ($editionBuild in $editionBuilds) {
        $pluginPath = [string]$editionBuild.pluginPath
        $pluginFileName = [string]$editionBuild.pluginFileName
        if (-not (Test-Path -LiteralPath $pluginPath -PathType Leaf)) {
            throw "Built radar DLL not found for deploy: $pluginPath"
        }

        $destination = Join-Path $destinationDirectory $pluginFileName
        Copy-Item -LiteralPath $pluginPath -Destination $destination -Force
        [void]$deployedDlls.Add([ordered]@{
            edition = [string]$editionBuild.edition
            vamRoot = $root
            pluginDirectory = $destinationDirectory
            path = $destination
            pluginFileName = $pluginFileName
            sha256 = Get-FaRadarFileHashOrEmpty -PathValue $destination
            sourceSha256 = Get-FaRadarFileHashOrEmpty -PathValue $pluginPath
            bytes = (Get-Item -LiteralPath $destination).Length
        })
    }

    if ($deployAnchorPreset) {
        $presetDestinationDirectory = Join-Path $root $anchorPresetRelativeDirectory
        Ensure-FaRadarDirectory -PathValue $presetDestinationDirectory
        $presetDestination = Join-Path $presetDestinationDirectory $anchorPresetFileName
        Copy-Item -LiteralPath $anchorPresetSource -Destination $presetDestination -Force
        [void]$deployedPresets.Add([ordered]@{
            vamRoot = $root
            presetDirectory = $presetDestinationDirectory
            path = $presetDestination
            presetFileName = $anchorPresetFileName
            sha256 = Get-FaRadarFileHashOrEmpty -PathValue $presetDestination
            sourceSha256 = Get-FaRadarFileHashOrEmpty -PathValue $anchorPresetSource
            bytes = (Get-Item -LiteralPath $presetDestination).Length
        })
    }

    $legacyLooseScript = Join-Path $root "Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
    if ((Test-Path -LiteralPath $legacyLooseScript -PathType Leaf) -and -not $KeepLegacyLooseScript.IsPresent) {
        $archiveDirectory = Join-Path $root ("Custom\PluginData\FrameAngel\Radar\Rejected\{0}_legacy_loose_script" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
        Ensure-FaRadarDirectory -PathValue $archiveDirectory
        $archivedLegacyScript = Join-Path $archiveDirectory "FrameAngelRadar.cs"
        Move-Item -LiteralPath $legacyLooseScript -Destination $archivedLegacyScript -Force
        [void]$archivedLegacyScripts.Add([ordered]@{
            vamRoot = $root
            source = $legacyLooseScript
            archived = $archivedLegacyScript
        })
    }
}

$receiptDirectory = Join-Path $resolvedRepoRoot "build\receipts"
Ensure-FaRadarDirectory -PathValue $receiptDirectory
$receiptPath = Join-Path $receiptDirectory ("fa_radar_deploy_{0}.json" -f (Get-Date -Format "yyyyMMdd_HHmmss"))

$gitBranch = (& git -C $resolvedRepoRoot branch --show-current).Trim()
$gitCommit = (& git -C $resolvedRepoRoot rev-parse HEAD).Trim()
$gitStatus = @(& git -C $resolvedRepoRoot status --short)

$receipt = [ordered]@{
    schemaVersion = "fa_radar_deploy_receipt_v1"
    createdAtUtc = [DateTime]::UtcNow.ToString("o")
    repoRoot = $resolvedRepoRoot
    branch = $gitBranch
    commit = $gitCommit
    dirtyState = $gitStatus
    version = [string]$buildResult.Version
    editionRequest = $Edition
    expectedPluginFileNames = $expectedPluginFileNames
    anchorPresetFileName = $anchorPresetFileName
    configuration = $Configuration
    buildReceiptPath = [string]$buildResult.ReceiptPath
    vamManagedDir = $VamManagedDir
    skipBuild = [bool]$SkipBuild
    skipObfuscation = [bool]$SkipObfuscation
    skipPackage = [bool]$SkipPackage
    deployedDlls = @($deployedDlls)
    deployedPresets = @($deployedPresets)
    archivedLegacyScripts = @($archivedLegacyScripts)
}
Write-FaRadarJson -PathValue $receiptPath -Value $receipt

[pscustomobject]@{
    RepoRoot = $resolvedRepoRoot
    VamRoots = $resolvedVamRoots
    Version = [string]$buildResult.Version
    EditionRequest = $Edition
    BuildReceiptPath = [string]$buildResult.ReceiptPath
    DeployedDlls = @($deployedDlls)
    DeployedPresets = @($deployedPresets)
    ArchivedLegacyScripts = @($archivedLegacyScripts)
    ReceiptPath = $receiptPath
    Deployed = $true
}
