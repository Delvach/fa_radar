param(
    [ValidateSet("All", "Free", "Pro")]
    [string]$Edition = "All",
    [string]$RepoRoot = "",
    [string]$VamRoot = "F:\sim\vam",
    [string]$VamManagedDir = "",
    [string]$Configuration = "Release",
    [switch]$SkipCompile,
    [switch]$SkipObfuscation,
    [switch]$SkipPackage,
    [switch]$Clean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 0.1.38 audit anchors. The live values are read from config/fa_radar.version.json.
# Free: FA_RADAR_FREE -> fa_radar.free.0.1.38.dll, FrameAngelDev.Radar.1.var
# Pro: FA_RADAR_PRO -> fa_radar.pro.0.1.38.dll
# Pro Empty preset: Preset_FrameAngel_Radar_Empty.vap

function Ensure-FaRadarDirectory {
    param([string]$PathValue)

    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function Assert-FaRadarPathInsideRoot {
    param(
        [string]$RootPath,
        [string]$TargetPath,
        [string]$Label
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd('\', '/')
    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    if (-not $resolvedTarget.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must stay inside repo root. Target: $resolvedTarget"
    }
}

function Clear-FaRadarDirectory {
    param(
        [string]$RepoRootValue,
        [string]$PathValue,
        [string]$Label
    )

    Assert-FaRadarPathInsideRoot -RootPath $RepoRootValue -TargetPath $PathValue -Label $Label
    if (Test-Path -LiteralPath $PathValue -PathType Container) {
        Remove-Item -LiteralPath $PathValue -Recurse -Force
    }
    Ensure-FaRadarDirectory -PathValue $PathValue
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
        [string]$ManagedDir,
        [string]$Name
    )

    $path = Join-Path $ManagedDir $Name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required VaM managed reference: $path"
    }

    return $path
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

$versionPath = Join-Path $resolvedRepoRoot "config\fa_radar.version.json"
if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Missing version config: $versionPath"
}

$versionInfo = Get-Content -LiteralPath $versionPath -Raw | ConvertFrom-Json
$version = [string]$versionInfo.version
$source = Join-Path $resolvedRepoRoot "payload\Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
    throw "Missing source plugin: $source"
}
$anchorPresetRelativePath = "Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap"
$anchorPresetSource = Join-Path $resolvedRepoRoot ("payload\" + $anchorPresetRelativePath)

if ([string]::IsNullOrWhiteSpace($VamManagedDir)) {
    $VamManagedDir = Join-Path $VamRoot "VaM_Data\Managed"
}
if (-not (Test-Path -LiteralPath $VamManagedDir -PathType Container)) {
    throw "VaM managed directory does not exist: $VamManagedDir"
}
$resolvedVamManagedDir = (Resolve-Path $VamManagedDir).Path

$requestedEditions = if ([string]::Equals($Edition, "All", [System.StringComparison]::OrdinalIgnoreCase)) {
    @("free", "pro")
} else {
    @($Edition.ToLowerInvariant())
}

$csc = ""
$references = @()
if (-not $SkipCompile.IsPresent) {
    $csc = Resolve-CSharpCompiler
    $references = @(
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "Assembly-CSharp.dll"),
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "UnityEngine.dll"),
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "UnityEngine.CoreModule.dll"),
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "UnityEngine.PhysicsModule.dll"),
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "UnityEngine.UI.dll"),
        (Get-RequiredReference -ManagedDir $resolvedVamManagedDir -Name "UnityEngine.UIModule.dll")
    )
}

$builds = New-Object System.Collections.ArrayList
$buildRoot = Join-Path $resolvedRepoRoot "build"
$receiptDirectory = Join-Path $buildRoot "receipts"
Ensure-FaRadarDirectory -PathValue $receiptDirectory

foreach ($editionId in $requestedEditions) {
    $editionProperty = $versionInfo.editions.PSObject.Properties[$editionId]
    if ($null -eq $editionProperty) {
        throw "Unknown FA Radar edition '$editionId'."
    }

    $editionInfo = $editionProperty.Value
    $displayName = [string]$editionInfo.displayName
    $pluginFileName = [string]$editionInfo.pluginFileName
    $packageFileName = [string]$editionInfo.packageFileName
    $packageName = [string]$editionInfo.packageName
    $packageCreator = [string]$editionInfo.packageCreator
    $compileSymbols = @($editionInfo.compileSymbols)
    if ($compileSymbols.Count -le 0) {
        throw "Edition '$editionId' has no compile symbols."
    }

    $editionBuildDirectory = Join-Path (Join-Path (Join-Path $buildRoot "bin") $Configuration) $editionId
    $rawDllPath = Join-Path $editionBuildDirectory "FrameAngelRadar.dll"
    $pluginPath = Join-Path $editionBuildDirectory $pluginFileName
    $obfuscationReportPath = $pluginPath + ".obf-report.json"

    if ($Clean.IsPresent) {
        Clear-FaRadarDirectory -RepoRootValue $resolvedRepoRoot -PathValue $editionBuildDirectory -Label "edition build directory"
    } else {
        Ensure-FaRadarDirectory -PathValue $editionBuildDirectory
    }

    if (-not $SkipCompile.IsPresent) {
        $defineValue = [string]::Join(";", @($compileSymbols))
        $compileArgs = @(
            "/nologo",
            "/target:library",
            "/optimize+",
            "/warn:4",
            "/define:$defineValue",
            "/out:$rawDllPath"
        )
        foreach ($reference in $references) {
            $compileArgs += "/reference:$reference"
        }
        $compileArgs += $source

        & $csc @compileArgs
        if ($LASTEXITCODE -ne 0) {
            throw "FrameAngelRadar $displayName DLL compile failed with exit code $LASTEXITCODE."
        }
    }

    if (-not (Test-Path -LiteralPath $rawDllPath -PathType Leaf)) {
        throw "Raw radar DLL not found for '$editionId': $rawDllPath"
    }

    if ($SkipObfuscation.IsPresent) {
        Copy-Item -LiteralPath $rawDllPath -Destination $pluginPath -Force
        [pscustomobject]@{
            generatedAtUtc = [DateTime]::UtcNow.ToString("o")
            enabled = $false
            pluginKey = "fa_radar"
            profile = "skipped"
            inputAssemblyPath = $rawDllPath
            outputAssemblyPath = $pluginPath
            outputDiffersFromInput = $false
            keepTypes = @("FrameAngelRadar")
            skipMethods = @()
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $obfuscationReportPath -Encoding UTF8
    } else {
        $obfuscateScript = Join-Path $resolvedRepoRoot "scripts\Obfuscate-FaRadarPlugin.ps1"
        & $obfuscateScript `
            -RepoRoot $resolvedRepoRoot `
            -PluginKey "fa_radar" `
            -InputAssemblyPath $rawDllPath `
            -OutputAssemblyPath $pluginPath `
            -ConfigPath "config\obfuscation.defaults.json" `
            -ReferenceSearchPath @($resolvedVamManagedDir) |
            ForEach-Object {
                if (-not [string]::IsNullOrWhiteSpace([string]$_)) {
                    Write-Host $_
                }
            }
        if ($LASTEXITCODE -ne 0) {
            throw "Obfuscation failed for FA Radar $displayName."
        }
    }

    if (-not (Test-Path -LiteralPath $pluginPath -PathType Leaf)) {
        throw "Edition plugin DLL was not produced: $pluginPath"
    }
    if (-not (Test-Path -LiteralPath $obfuscationReportPath -PathType Leaf)) {
        throw "Obfuscation report was not produced: $obfuscationReportPath"
    }

    $packagePath = ""
    $packageSha256 = ""
    $stageRoot = ""
    $anchorPresetPackagePath = ""
    if (-not $SkipPackage.IsPresent) {
        $packageWorkRoot = Join-Path (Join-Path $buildRoot "package_work") $editionId
        $stageRoot = Join-Path $packageWorkRoot "stage"
        Clear-FaRadarDirectory -RepoRootValue $resolvedRepoRoot -PathValue $stageRoot -Label "package stage directory"

        $pluginStageDirectory = Join-Path $stageRoot "Custom\Plugins"
        Ensure-FaRadarDirectory -PathValue $pluginStageDirectory
        Copy-Item -LiteralPath $pluginPath -Destination (Join-Path $pluginStageDirectory $pluginFileName) -Force

        $contentList = New-Object System.Collections.ArrayList
        [void]$contentList.Add("Custom/Plugins/$pluginFileName")
        if ([string]::Equals($editionId, "pro", [System.StringComparison]::OrdinalIgnoreCase)) {
            if (-not (Test-Path -LiteralPath $anchorPresetSource -PathType Leaf)) {
                throw "Missing Pro Empty anchor preset: $anchorPresetSource"
            }

            $anchorPresetStageDirectory = Join-Path $stageRoot "Custom\Atom\Empty"
            Ensure-FaRadarDirectory -PathValue $anchorPresetStageDirectory
            $anchorPresetPackagePath = Join-Path $anchorPresetStageDirectory "Preset_FrameAngel_Radar_Empty.vap"
            Copy-Item -LiteralPath $anchorPresetSource -Destination $anchorPresetPackagePath -Force
            [void]$contentList.Add("Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap")
        }

        $meta = [ordered]@{
            licenseType = "CC BY"
            creatorName = $packageCreator
            packageName = $packageName
            description = "Frame Angel Radar $displayName $version plugin build."
            instructions = "Install this package in VaM AddonPackages. The plugin DLL is staged under Custom/Plugins. Pro also includes an Empty atom preset for scene anchoring."
            contentList = @($contentList)
            dependencies = @{}
        }
        Write-FaRadarJson -PathValue (Join-Path $stageRoot "meta.json") -Value $meta

        $packageDirectory = Join-Path $buildRoot "packages"
        Ensure-FaRadarDirectory -PathValue $packageDirectory
        $packagePath = Join-Path $packageDirectory $packageFileName
        $tempZipPath = $packagePath + ".zip"
        if (Test-Path -LiteralPath $packagePath -PathType Leaf) {
            Remove-Item -LiteralPath $packagePath -Force
        }
        if (Test-Path -LiteralPath $tempZipPath -PathType Leaf) {
            Remove-Item -LiteralPath $tempZipPath -Force
        }
        Compress-Archive -Path (Join-Path $stageRoot "*") -DestinationPath $tempZipPath -Force
        Move-Item -LiteralPath $tempZipPath -Destination $packagePath -Force
        $packageSha256 = Get-FaRadarFileHashOrEmpty -PathValue $packagePath
    }

    [void]$builds.Add([ordered]@{
        edition = $editionId
        displayName = $displayName
        version = $version
        compileSymbols = @($compileSymbols)
        rawDllPath = $rawDllPath
        rawDllSha256 = Get-FaRadarFileHashOrEmpty -PathValue $rawDllPath
        pluginFileName = $pluginFileName
        pluginPath = $pluginPath
        pluginSha256 = Get-FaRadarFileHashOrEmpty -PathValue $pluginPath
        bytes = (Get-Item -LiteralPath $pluginPath).Length
        obfuscated = -not [bool]$SkipObfuscation
        obfuscationReportPath = $obfuscationReportPath
        packageFileName = $packageFileName
        packagePath = $packagePath
        packageSha256 = $packageSha256
        anchorPresetPackagePath = $anchorPresetPackagePath
        stageRoot = $stageRoot
    })
}

$receiptPath = Join-Path $receiptDirectory ("fa_radar_build_{0}.json" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
$gitBranch = (& git -C $resolvedRepoRoot branch --show-current).Trim()
$gitCommit = (& git -C $resolvedRepoRoot rev-parse HEAD).Trim()
$gitStatus = @(& git -C $resolvedRepoRoot status --short)

$receipt = [ordered]@{
    schemaVersion = "fa_radar_build_receipt_v1"
    createdAtUtc = [DateTime]::UtcNow.ToString("o")
    repoRoot = $resolvedRepoRoot
    branch = $gitBranch
    commit = $gitCommit
    dirtyState = $gitStatus
    version = $version
    editionRequest = $Edition
    configuration = $Configuration
    skipCompile = [bool]$SkipCompile
    skipObfuscation = [bool]$SkipObfuscation
    skipPackage = [bool]$SkipPackage
    source = $source
    csharpCompiler = $csc
    vamManagedDir = $resolvedVamManagedDir
    editionBuilds = @($builds)
}
Write-FaRadarJson -PathValue $receiptPath -Value $receipt

[pscustomobject]@{
    RepoRoot = $resolvedRepoRoot
    Version = $version
    EditionRequest = $Edition
    ReceiptPath = $receiptPath
    EditionBuilds = @($builds)
    Built = $true
}
