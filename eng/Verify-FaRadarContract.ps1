param(
    [string]$RepoRoot = "",
    [string]$VamRoot = "F:\sim\vam",
    [switch]$ValidateLiveDeploy
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

$pluginPath = Join-Path $RepoRoot "payload\Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
$deployPath = Join-Path $RepoRoot "scripts\Deploy-FaRadar.ps1"
$docPath = Join-Path $RepoRoot "docs\FA_RADAR_ARCHITECTURE_V1.md"
$versionPath = Join-Path $RepoRoot "config\fa_radar.version.json"

$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure([string]$message) {
    $script:failures.Add($message) | Out-Null
}

if (-not (Test-Path -LiteralPath $pluginPath)) {
    Add-Failure "Missing plugin file: $pluginPath"
} else {
    $plugin = Get-Content -Raw -LiteralPath $pluginPath

    $requiredSnippets = @(
        "class FrameAngelRadar : MVRScript",
        'private const string Version = "0.1.7"',
        "GetSelectedAtom()",
        "lookCamera",
        "CreateSphereMesh",
        "CreateDesktopDiskMesh",
        "CreateGridMesh",
        "CreateRingMesh",
        "CreateTargetBlipMesh",
        "CreateCenterMarkerMesh",
        "UpdateRadarDish",
        "ResolveTargetRadarLocal",
        "Flatten Target Y",
        "Unified Sphere Treatment",
        "World Axis Align",
        "Ground Axis Lock",
        'new JSONStorableBool("Ground Axis Lock", true)',
        'new JSONStorableBool("Flatten Target Y", false)',
        "Selected Ground Drop",
        'new JSONStorableBool("Selected Ground Drop", false)',
        "selectedGroundDropEnabledField",
        "Grid Follows User",
        "Grid Clip Circle",
        'new JSONStorableFloat("HUD Offset X", -0.59f',
        'new JSONStorableFloat("HUD Offset Y", 0.22f',
        'new JSONStorableFloat("HUD Offset Z", 0.78f',
        'new JSONStorableFloat("HUD Scale", 0.49f',
        'new JSONStorableFloat("Radar Visual Radius", 0.08f',
        "Anchor To View",
        "View Yaw Offset",
        "Desktop Tilt Degrees",
        "Axis Yaw Offset",
        "Last Selected Enabled",
        "Last Selected Fade Seconds",
        "axisRoot",
        "lastGridOffsetMeters",
        "ResolveViewerGridOffsetMeters",
        "PositiveModulo",
        "PositiveModulo(worldPosition.z",
        "gridY = 0.0f",
        "ringXMaterial",
        "ringZMaterial",
        "CreateSphereShellMaterial",
        "CreateSphereMesh(16, 32, 1.0f)",
        "CreateSphereMesh(8, 16, 1.0f)",
        "hasSelection && selectedGroundDropEnabledField.val",
        "AddClippedGridLine",
        "ResolveAxisLocalRotation",
        "ResolveGroundAxisWorldRotation",
        "Quaternion.Inverse(radarRoot.transform.rotation)",
        "ResolveTargetGroundRadarLocal",
        "target.position - viewer.position",
        'targetGridDropObject = CreateMeshObject("FA Radar Target Grid Drop", axisRoot.transform',
        'lastTargetGridDropObject = CreateMeshObject("FA Radar Last Target Grid Drop", axisRoot.transform',
        "ResolveWorldAxisYawDegrees",
        "UpdateAxisVisualRotation",
        "lastTargetBlipObject",
        "lastSelectedUid",
        "ApplyHudAnchor",
        "Radar Range Meters",
        "Grid Step Meters",
        "Ring Rotation Speed",
        "Placement Mode",
        "Capture HUD Offset From Atom",
        'Shader.Find("Hidden/Internal-Colored")',
        "CompareFunction.Always",
        "DestroyRuntimeVisuals"
    )

    foreach ($snippet in $requiredSnippets) {
        if (-not $plugin.Contains($snippet)) {
            Add-Failure "Plugin missing required snippet: $snippet"
        }
    }

    $forbiddenPatterns = @(
        "\bSystem\.IO\b",
        "\bFile\.",
        "\bDirectory\.",
        "\bPath\.",
        "\bSystem\.Reflection\b",
        "\bReflection\b"
    )

    foreach ($pattern in $forbiddenPatterns) {
        if ($plugin -match $pattern) {
            Add-Failure "Plugin contains forbidden runtime pattern: $pattern"
        }
    }
}

if (-not (Test-Path -LiteralPath $deployPath)) {
    Add-Failure "Missing deploy helper: $deployPath"
} else {
    $deploy = Get-Content -Raw -LiteralPath $deployPath
    $requiredDeploySnippets = @(
        "FrameAngelRadar.dll",
        "fa_radar.0.1.7.dll",
        "F:\sim\vam",
        "C:\vam\virgin-recordable-02",
        "Custom\Plugins",
        "VaM_Data\Managed",
        "Assembly-CSharp.dll",
        "UnityEngine.CoreModule.dll",
        "fa_radar_deploy_receipt_v1",
        "deployedDlls",
        "archivedLegacyScripts"
    )

    foreach ($snippet in $requiredDeploySnippets) {
        if (-not $deploy.Contains($snippet)) {
            Add-Failure "Deploy helper missing required DLL deploy snippet: $snippet"
        }
    }

    if ($deploy -match "Copy-Item[^\r\n]+FrameAngelRadar\.cs") {
        Add-Failure "Deploy helper still appears to copy the loose .cs plugin instead of a DLL."
    }

    if ($deploy.Contains("Custom\Plugins\FrameAngel\Radar")) {
        Add-Failure "Deploy helper must deploy directly to Custom\Plugins, not a radar subfolder."
    }
}

if (-not (Test-Path -LiteralPath $docPath)) {
    Add-Failure "Missing architecture doc: $docPath"
}

if (-not (Test-Path -LiteralPath $versionPath)) {
    Add-Failure "Missing version config: $versionPath"
} else {
    $version = Get-Content -Raw -LiteralPath $versionPath | ConvertFrom-Json
    if ($version.version -ne "0.1.7") {
        Add-Failure "Version config must declare version 0.1.7."
    }
    if ($version.branch -ne "codex/0.1.7-marker-clarity-sphere-smooth") {
        Add-Failure "Version config branch must match codex/0.1.7-marker-clarity-sphere-smooth."
    }
    $targets = @($version.deployment.vamRoots)
    if ($targets -notcontains "F:\sim\vam") {
        Add-Failure "Version config missing F:\sim\vam deploy root."
    }
    if ($targets -notcontains "C:\vam\virgin-recordable-02") {
        Add-Failure "Version config missing C:\vam\virgin-recordable-02 deploy root."
    }
}

$unityProjectFolders = Get-ChildItem -LiteralPath $RepoRoot -Recurse -Force -Directory |
    Where-Object {
        $_.FullName -notmatch "\\\.git(\\|$)" -and
        ($_.Name -eq "Assets" -or $_.Name -eq "Library" -or $_.Name -eq "Packages" -or $_.Name -eq "ProjectSettings")
    }

foreach ($folder in $unityProjectFolders) {
    Add-Failure "Unity project folder is out of scope: $($folder.FullName)"
}

$unityFiles = Get-ChildItem -LiteralPath $RepoRoot -Recurse -Force -File |
    Where-Object {
        $_.FullName -notmatch "\\\.git(\\|$)" -and
        ($_.Extension -eq ".unity" -or $_.Extension -eq ".asset" -or $_.Extension -eq ".assetbundle")
    }

foreach ($file in $unityFiles) {
    Add-Failure "Unity asset file is out of scope: $($file.FullName)"
}

if ($ValidateLiveDeploy.IsPresent) {
    $roots = @("F:\sim\vam", "C:\vam\virgin-recordable-02")
    foreach ($root in $roots) {
        $deployedDll = Join-Path $root "Custom\Plugins\fa_radar.0.1.7.dll"
        $legacyLooseScript = Join-Path $root "Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"
        if (-not (Test-Path -LiteralPath $deployedDll -PathType Leaf)) {
            Add-Failure "Live radar DLL was not deployed: $deployedDll"
        } else {
            $dllItem = Get-Item -LiteralPath $deployedDll
            if ($dllItem.Length -le 0) {
                Add-Failure "Live radar DLL is empty: $deployedDll"
            }
        }

        if (Test-Path -LiteralPath $legacyLooseScript -PathType Leaf) {
            Add-Failure "Legacy loose radar .cs remains in VaM script load path: $legacyLooseScript"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "FA Radar contract verification failed:" -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }
    exit 1
}

[pscustomobject]@{
    RepoRoot = $RepoRoot
    Plugin = $pluginPath
    DeployHelper = $deployPath
    ArchitectureDoc = $docPath
    VersionConfig = $versionPath
    ValidateLiveDeploy = [bool]$ValidateLiveDeploy
    Verified = $true
}
