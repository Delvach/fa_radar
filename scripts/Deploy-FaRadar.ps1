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

function Assert-FaRadarVamNotRunning {
    $vamProcesses = @(Get-Process -Name "VaM" -ErrorAction SilentlyContinue)
    if ($vamProcesses.Count -gt 0) {
        $processIds = [string]::Join(", ", @($vamProcesses | ForEach-Object { [string]$_.Id }))
        throw "VaM.exe is running (PID(s): $processIds). Radar deploy requires both target roots to remain stopped."
    }
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
    $vamExecutable = Join-Path $root "VaM.exe"
    if (-not (Test-Path -LiteralPath $vamExecutable -PathType Leaf)) {
        throw "Exact VaM root is missing VaM.exe: $root"
    }
    $resolvedVamRoots.Add((Resolve-Path $root).Path) | Out-Null
}

if ([string]::IsNullOrWhiteSpace($VamManagedDir)) {
    $VamManagedDir = Join-Path $resolvedVamRoots[0] "VaM_Data\Managed"
}
if (-not (Test-Path -LiteralPath $VamManagedDir -PathType Container)) {
    throw "VaM managed directory does not exist: $VamManagedDir"
}

$anchorPresetFileName = "Preset_FrameAngel_Radar_Empty.vap"
$anchorPresetRelativeDirectory = "Custom\Atom\Empty"
$anchorPresetSource = Join-Path $resolvedRepoRoot "payload\$anchorPresetRelativeDirectory\$anchorPresetFileName"
$cuaPresetFileName = "Preset_FrameAngel_Radar_CUA.vap"
$cuaPresetRelativeDirectory = "Custom\Atom\CustomUnityAsset"
$cuaPresetSource = Join-Path $resolvedRepoRoot "payload\$cuaPresetRelativeDirectory\$cuaPresetFileName"

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
$expectedPluginFileNames = @($editionBuilds | ForEach-Object { [string]$_.pluginFileName })

foreach ($editionBuild in $editionBuilds) {
    $pluginPath = [string]$editionBuild.pluginPath
    if (-not (Test-Path -LiteralPath $pluginPath -PathType Leaf)) {
        throw "Built radar DLL not found for deploy: $pluginPath"
    }
}

Assert-FaRadarVamNotRunning

$deployedDlls = New-Object System.Collections.ArrayList
$deployedPresets = New-Object System.Collections.ArrayList
$archivedLegacyScripts = New-Object System.Collections.ArrayList
$deployAnchorPreset = @($editionBuilds | Where-Object { [string]$_.edition -eq "pro" }).Count -gt 0
if ($deployAnchorPreset -and -not (Test-Path -LiteralPath $anchorPresetSource -PathType Leaf)) {
    throw "Missing Pro Empty anchor preset for deploy: $anchorPresetSource"
}
if ($deployAnchorPreset -and -not (Test-Path -LiteralPath $cuaPresetSource -PathType Leaf)) {
    throw "Missing Pro CUA anchor preset for deploy: $cuaPresetSource"
}
foreach ($root in $resolvedVamRoots) {
    Assert-FaRadarVamNotRunning
    $destinationDirectory = Join-Path $root "Custom\Plugins"
    Ensure-FaRadarDirectory -PathValue $destinationDirectory

    foreach ($editionBuild in $editionBuilds) {
        Assert-FaRadarVamNotRunning
        $pluginPath = [string]$editionBuild.pluginPath
        $pluginFileName = [string]$editionBuild.pluginFileName
        if (-not (Test-Path -LiteralPath $pluginPath -PathType Leaf)) {
            throw "Built radar DLL not found for deploy: $pluginPath"
        }

        $destination = Join-Path $destinationDirectory $pluginFileName
        Copy-Item -LiteralPath $pluginPath -Destination $destination -Force
        $sourceSha256 = Get-FaRadarFileHashOrEmpty -PathValue $pluginPath
        $destinationSha256 = Get-FaRadarFileHashOrEmpty -PathValue $destination
        $sourceBytes = (Get-Item -LiteralPath $pluginPath).Length
        $destinationBytes = (Get-Item -LiteralPath $destination).Length
        if ($destinationSha256 -ne $sourceSha256 -or $destinationBytes -ne $sourceBytes) {
            throw "Radar DLL readback mismatch: $destination"
        }
        [void]$deployedDlls.Add([ordered]@{
            edition = [string]$editionBuild.edition
            vamRoot = $root
            pluginDirectory = $destinationDirectory
            path = $destination
            pluginFileName = $pluginFileName
            sha256 = $destinationSha256
            sourceSha256 = $sourceSha256
            bytes = $destinationBytes
        })
    }

    if ($deployAnchorPreset) {
        Assert-FaRadarVamNotRunning
        $presetDestinationDirectory = Join-Path $root $anchorPresetRelativeDirectory
        Ensure-FaRadarDirectory -PathValue $presetDestinationDirectory
        $presetDestination = Join-Path $presetDestinationDirectory $anchorPresetFileName
        Copy-Item -LiteralPath $anchorPresetSource -Destination $presetDestination -Force
        $presetSourceSha256 = Get-FaRadarFileHashOrEmpty -PathValue $anchorPresetSource
        $presetDestinationSha256 = Get-FaRadarFileHashOrEmpty -PathValue $presetDestination
        $presetSourceBytes = (Get-Item -LiteralPath $anchorPresetSource).Length
        $presetDestinationBytes = (Get-Item -LiteralPath $presetDestination).Length
        if ($presetDestinationSha256 -ne $presetSourceSha256 -or $presetDestinationBytes -ne $presetSourceBytes) {
            throw "Empty preset readback mismatch: $presetDestination"
        }
        [void]$deployedPresets.Add([ordered]@{
            vamRoot = $root
            presetDirectory = $presetDestinationDirectory
            path = $presetDestination
            presetFileName = $anchorPresetFileName
            sha256 = $presetDestinationSha256
            sourceSha256 = $presetSourceSha256
            bytes = $presetDestinationBytes
        })

        Assert-FaRadarVamNotRunning
        $cuaPresetDestinationDirectory = Join-Path $root $cuaPresetRelativeDirectory
        Ensure-FaRadarDirectory -PathValue $cuaPresetDestinationDirectory
        $cuaPresetDestination = Join-Path $cuaPresetDestinationDirectory $cuaPresetFileName
        Copy-Item -LiteralPath $cuaPresetSource -Destination $cuaPresetDestination -Force
        $cuaPresetSourceSha256 = Get-FaRadarFileHashOrEmpty -PathValue $cuaPresetSource
        $cuaPresetDestinationSha256 = Get-FaRadarFileHashOrEmpty -PathValue $cuaPresetDestination
        $cuaPresetSourceBytes = (Get-Item -LiteralPath $cuaPresetSource).Length
        $cuaPresetDestinationBytes = (Get-Item -LiteralPath $cuaPresetDestination).Length
        if ($cuaPresetDestinationSha256 -ne $cuaPresetSourceSha256 -or $cuaPresetDestinationBytes -ne $cuaPresetSourceBytes) {
            throw "CUA preset readback mismatch: $cuaPresetDestination"
        }
        [void]$deployedPresets.Add([ordered]@{
            vamRoot = $root
            presetDirectory = $cuaPresetDestinationDirectory
            path = $cuaPresetDestination
            presetFileName = $cuaPresetFileName
            sha256 = $cuaPresetDestinationSha256
            sourceSha256 = $cuaPresetSourceSha256
            bytes = $cuaPresetDestinationBytes
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

Assert-FaRadarVamNotRunning

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
    cuaPresetFileName = $cuaPresetFileName
    configuration = $Configuration
    buildReceiptPath = [string]$buildResult.ReceiptPath
    sourceSha256 = [string]$buildResult.SourceSha256
    versionAuthoritySha256 = [string]$buildResult.VersionAuthoritySha256
    emptyPresetSha256 = [string]$buildResult.EmptyPresetSha256
    cuaPresetSha256 = [string]$buildResult.CuaPresetSha256
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
