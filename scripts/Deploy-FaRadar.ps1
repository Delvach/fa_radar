param(
    [string[]]$VamRoots = @("F:\sim\vam", "C:\vam\virgin-recordable-02"),
    [string]$VamRoot = "",
    [string]$RepoRoot = "",
    [string]$VamManagedDir = "",
    [string]$PluginFileName = "fa_radar.0.1.7.dll",
    [string]$Configuration = "Release",
    [switch]$SkipBuild,
    [switch]$KeepLegacyLooseScript
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Ensure-Directory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PathValue
    )

    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function Resolve-CSharpCompiler {
    $candidates = @(
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    throw "C# compiler not found. Expected .NET Framework csc.exe."
}

function Get-RequiredReference {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ManagedDir,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $path = Join-Path $ManagedDir $Name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required VaM managed reference: $path"
    }

    return $path
}

$source = Join-Path $RepoRoot "payload\Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
if (-not (Test-Path -LiteralPath $source)) {
    throw "Missing source plugin: $source"
}

$effectiveVamRoots = New-Object System.Collections.Generic.List[string]
if (-not [string]::IsNullOrWhiteSpace($VamRoot)) {
    $effectiveVamRoots.Add($VamRoot) | Out-Null
}
foreach ($root in $VamRoots) {
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

$resolvedRepoRoot = (Resolve-Path $RepoRoot).Path

if ([string]::IsNullOrWhiteSpace($VamManagedDir)) {
    $VamManagedDir = Join-Path $resolvedVamRoots[0] "VaM_Data\Managed"
}
if (-not (Test-Path -LiteralPath $VamManagedDir -PathType Container)) {
    throw "VaM managed directory does not exist: $VamManagedDir"
}

$buildDirectory = Join-Path $resolvedRepoRoot ("build\bin\{0}" -f $Configuration)
$builtDll = Join-Path $buildDirectory "FrameAngelRadar.dll"
Ensure-Directory -PathValue $buildDirectory

$csc = ""
if (-not $SkipBuild.IsPresent) {
    $csc = Resolve-CSharpCompiler
    $references = @(
        (Get-RequiredReference -ManagedDir $VamManagedDir -Name "Assembly-CSharp.dll"),
        (Get-RequiredReference -ManagedDir $VamManagedDir -Name "UnityEngine.dll"),
        (Get-RequiredReference -ManagedDir $VamManagedDir -Name "UnityEngine.CoreModule.dll"),
        (Get-RequiredReference -ManagedDir $VamManagedDir -Name "UnityEngine.UI.dll"),
        (Get-RequiredReference -ManagedDir $VamManagedDir -Name "UnityEngine.UIModule.dll")
    )

    $compileArgs = @(
        "/nologo",
        "/target:library",
        "/optimize+",
        "/warn:4",
        "/out:$builtDll"
    )
    foreach ($reference in $references) {
        $compileArgs += "/reference:$reference"
    }
    $compileArgs += $source

    & $csc @compileArgs
    if ($LASTEXITCODE -ne 0) {
        throw "FrameAngelRadar DLL compile failed with exit code $LASTEXITCODE"
    }
}

if (-not (Test-Path -LiteralPath $builtDll -PathType Leaf)) {
    throw "Built radar DLL not found: $builtDll"
}

$deployedDlls = @()
$archivedLegacyScripts = @()
foreach ($root in $resolvedVamRoots) {
    $destinationDirectory = Join-Path $root "Custom\Plugins"
    $destination = Join-Path $destinationDirectory $PluginFileName
    Ensure-Directory -PathValue $destinationDirectory
    Copy-Item -LiteralPath $builtDll -Destination $destination -Force

    $sourceHash = Get-FileHash -Algorithm SHA256 -LiteralPath $builtDll
    $destinationHash = Get-FileHash -Algorithm SHA256 -LiteralPath $destination
    $deployedDlls += [ordered]@{
        vamRoot = $root
        pluginDirectory = $destinationDirectory
        path = $destination
        sha256 = $destinationHash.Hash
        sourceSha256 = $sourceHash.Hash
        bytes = (Get-Item -LiteralPath $destination).Length
    }

    $legacyLooseScript = Join-Path $root "Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
    if ((Test-Path -LiteralPath $legacyLooseScript -PathType Leaf) -and -not $KeepLegacyLooseScript.IsPresent) {
        $archiveDirectory = Join-Path $root ("Custom\PluginData\FrameAngel\Radar\Rejected\{0}_legacy_loose_script" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
        Ensure-Directory -PathValue $archiveDirectory
        $archivedLegacyScript = Join-Path $archiveDirectory "FrameAngelRadar.cs"
        Move-Item -LiteralPath $legacyLooseScript -Destination $archivedLegacyScript -Force
        $archivedLegacyScripts += [ordered]@{
            vamRoot = $root
            source = $legacyLooseScript
            archived = $archivedLegacyScript
        }
    }
}

$receiptDirectory = Join-Path $resolvedRepoRoot "build\receipts"
Ensure-Directory -PathValue $receiptDirectory
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
    version = "0.1.7"
    source = $source
    csharpCompiler = $csc
    vamManagedDir = $VamManagedDir
    builtDll = $builtDll
    pluginFileName = $PluginFileName
    skipBuild = [bool]$SkipBuild
    dirtyState = $gitStatus
    deployedDlls = $deployedDlls
    archivedLegacyScripts = $archivedLegacyScripts
}
$receipt | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $receiptPath -Encoding UTF8

[pscustomobject]@{
    RepoRoot = $resolvedRepoRoot
    VamRoots = $resolvedVamRoots
    Source = $source
    BuiltDll = $builtDll
    DeployedDlls = $deployedDlls
    ArchivedLegacyScripts = $archivedLegacyScripts
    ReceiptPath = $receiptPath
    Deployed = $true
}
