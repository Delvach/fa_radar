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
$buildPath = Join-Path $RepoRoot "scripts\Build-FaRadar.ps1"
$obfuscatePath = Join-Path $RepoRoot "scripts\Obfuscate-FaRadarPlugin.ps1"
$deployPath = Join-Path $RepoRoot "scripts\Deploy-FaRadar.ps1"
$docPath = Join-Path $RepoRoot "docs\FA_RADAR_ARCHITECTURE_V1.md"
$versionPath = Join-Path $RepoRoot "config\fa_radar.version.json"
$obfuscationConfigPath = Join-Path $RepoRoot "config\obfuscation.defaults.json"

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
        'private const string Version = "0.1.14"',
        "#if FA_RADAR_PRO",
        "private const bool IsProEdition = true",
        'private const string EditionName = "Pro"',
        "#else",
        "private const bool IsProEdition = false",
        'private const string EditionName = "Free"',
        "Frame Angel Radar " + '"' + " + Version + " + '"' + " " + '"' + " + EditionName",
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
        "using MVR.FileManagementSecure",
        "FrameAngelRadarPreferencesRootPath",
        "FrameAngelRadarCommonPreferencesPath",
        "FrameAngelRadarProPreferencesPath",
        "FrameAngelRadarCommonPreferencesSchemaVersion",
        "FrameAngelRadarProPreferencesSchemaVersion",
        "Custom\\PluginData\\FrameAngel\\Radar",
        "FileManagerSecure.FileExists",
        "FileManagerSecure.ReadAllText",
        "FileManagerSecure.WriteAllText",
        "FileManagerSecure.CreateDirectory",
        "sharedRadarCommonPreferencesCacheKnown",
        "sharedRadarProPreferencesCacheKnown",
        "LoadGlobalPreferences",
        "WriteGlobalPreferences",
        "TryReadGlobalPreferencesFromDisk",
        "TryReadSharedGlobalPreferencesCache",
        "PollSharedGlobalPreferences",
        "MarkGlobalPreferencesDirty",
        "FlushGlobalPreferencesIfDue",
        "Global Prefs Auto Save",
        "Save Global Prefs",
        "Load Global Prefs",
        "Reset Global Prefs",
        "valNoCallback",
        "ConfigureGlobalPreferenceField",
        "field.isStorable = false",
        "field.isRestorable = false",
        "FilmSubjectIdentifier",
        '"favr.hud.radar"',
        "BuildFilmSubjectName",
        "Anchor To View",
        "Floor Area Scale",
        "Desktop Tilt Degrees",
        "Height Stems",
        'new JSONStorableBool("Height Stems", true)',
        "Height Scale Meters",
        "Height Stem Alpha",
        "Range Fade Meters",
        "Depth Size Cue",
        "Depth Size Strength",
        "Available Atom Markers",
        "Show Lights",
        "Show CUA",
        "Show People",
        "Show Other Atoms",
        "Click Select Markers",
        "Marker Click Radius Pixels",
        "Atom Poll Seconds",
        "Available Atom Alpha",
        "Previous Selection Disabled",
        'new JSONStorableBool("Last Selected Enabled", false)',
        "axisRoot",
        "lastGridOffsetMeters",
        "ResolveViewerGridOffsetMeters",
        "PositiveModulo",
        "PositiveModulo(worldPosition.z",
        "gridY = 0.0f",
        "AxisXColor",
        "AxisYColor",
        "AxisZColor",
        'BuildFilmSubjectName("X Axis Ring Material")',
        'BuildFilmSubjectName("Y Axis Ring Material")',
        'BuildFilmSubjectName("Z Axis Ring Material")',
        "CreateSphereShellMaterial",
        "CreateSphereMesh(16, 32, 1.0f)",
        "CreateSphereMesh(8, 16, 1.0f)",
        "hasSelection && selectedGroundDropEnabledField.val",
        "CreateHeightStemMesh",
        "UpdateHeightStem",
        "userHeightStemObject",
        "targetHeightStemObject",
        "floorAreaScaleField",
        'new JSONStorableFloat("Floor Area Scale", 1.0f',
        "ResolveFloorAreaScale",
        "ResolveEffectiveRadarRangeMeters",
        "ResolveEffectiveHeightScaleMeters",
        "ResolveHeightRadarY",
        "ResolveRangeFadeAlpha",
        "ResolveDepthScale",
        "HandleRadarMarkerClick",
        "ResolveViewerCamera",
        "ResolveClickedAvailableAtom",
        "ResolveMarkerScreenRadiusPixels",
        "SelectRadarAtom",
        "Input.GetMouseButtonDown(0)",
        "WorldToScreenPoint",
        "SuperController.singleton.SelectController(atom.mainController, false, false, false, true)",
        "PollAvailableAtomsIfDue",
        "UpdateAvailableAtomMarkers",
        "IsLightAtom",
        "IsCustomUnityAssetAtom",
        "IsPersonAtom",
        "IsAtomVisibleByFilter",
        "return true",
        "ResolveAvailableAtomColor",
        "EnsureAvailableMarkerCapacity",
        "availableMarkerObjects",
        "availableStemObjects",
        "availableMarkerMaterials",
        "trackedAvailableAtoms",
        'new JSONStorableFloat("Ring Rotation Speed", 0.0f',
        "AddClippedGridLine",
        "ResolveAxisLocalRotation",
        "ResolveGroundAxisWorldRotation",
        "Quaternion.Inverse(radarRoot.transform.rotation)",
        "ResolveTargetGroundRadarLocal",
        "target.position - viewer.position",
        'targetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Target Grid Drop"), axisRoot.transform',
        'lastTargetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Last Target Grid Drop"), axisRoot.transform',
        "ResolveWorldAxisYawDegrees",
        "UpdateAxisVisualRotation",
        "lastTargetBlipObject",
        "lastSelectedUid",
        "ApplyHudAnchor",
        "lastGoodViewerTransform",
        "ResolveStableViewerTransform",
        "return lastGoodViewerTransform",
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
        "\bReflection\b",
        "\bJSONClass\b",
        "\bJSONNode\b",
        "\bSimpleJSON\b",
        "\btargetOutlineObject\b",
        "\btargetOutlineMaterial\b",
        "CreateSlider\(viewYawOffsetField",
        "CreateSlider\(axisYawOffsetField",
        "viewYawOffsetField\.val",
        "axisYawOffsetField\.val"
    )

    foreach ($pattern in $forbiddenPatterns) {
        if ($plugin -match $pattern) {
            Add-Failure "Plugin contains forbidden runtime pattern: $pattern"
        }
    }
}

if (-not (Test-Path -LiteralPath $buildPath)) {
    Add-Failure "Missing edition build helper: $buildPath"
} else {
    $build = Get-Content -Raw -LiteralPath $buildPath
    $requiredBuildSnippets = @(
        "FA_RADAR_FREE",
        "FA_RADAR_PRO",
        "fa_radar.free.0.1.14.dll",
        "fa_radar.pro.0.1.14.dll",
        "Obfuscate-FaRadarPlugin.ps1",
        "Custom\Plugins",
        "meta.json",
        "Compress-Archive",
        "fa_radar_build_receipt_v1",
        "packagePath",
        "obfuscationReportPath"
    )

    foreach ($snippet in $requiredBuildSnippets) {
        if (-not $build.Contains($snippet)) {
            Add-Failure "Build helper missing required edition/package snippet: $snippet"
        }
    }
}

if (-not (Test-Path -LiteralPath $obfuscatePath)) {
    Add-Failure "Missing obfuscation helper: $obfuscatePath"
} else {
    $obfuscate = Get-Content -Raw -LiteralPath $obfuscatePath
    $requiredObfuscationSnippets = @(
        "Obfuscar.GlobalTool",
        "obfuscar.console.exe",
        "config\obfuscation.defaults.json",
        "SkipType",
        "SkipMethod",
        "outputDiffersFromInput",
        ".obf-report.json"
    )

    foreach ($snippet in $requiredObfuscationSnippets) {
        if (-not $obfuscate.Contains($snippet)) {
            Add-Failure "Obfuscation helper missing required FAP-style snippet: $snippet"
        }
    }
}

if (-not (Test-Path -LiteralPath $deployPath)) {
    Add-Failure "Missing deploy helper: $deployPath"
} else {
    $deploy = Get-Content -Raw -LiteralPath $deployPath
    $requiredDeploySnippets = @(
        "Build-FaRadar.ps1",
        "fa_radar.free.0.1.14.dll",
        "fa_radar.pro.0.1.14.dll",
        "F:\sim\vam",
        "C:\vam\virgin-recordable-02",
        "Custom\Plugins",
        "fa_radar_deploy_receipt_v1",
        "buildReceiptPath",
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

if (-not (Test-Path -LiteralPath $obfuscationConfigPath)) {
    Add-Failure "Missing obfuscation config: $obfuscationConfigPath"
} else {
    $obfuscationConfig = Get-Content -Raw -LiteralPath $obfuscationConfigPath
    $requiredObfuscationConfigSnippets = @(
        '"package": "Obfuscar.GlobalTool"',
        '"version": "2.2.44"',
        '"profile": "vam_compat"',
        '"fa_radar"',
        '"FrameAngelRadar"',
        '"Init"',
        '"Update"',
        '"OnDestroy"'
    )

    foreach ($snippet in $requiredObfuscationConfigSnippets) {
        if (-not $obfuscationConfig.Contains($snippet)) {
            Add-Failure "Obfuscation config missing required snippet: $snippet"
        }
    }
}

if (-not (Test-Path -LiteralPath $versionPath)) {
    Add-Failure "Missing version config: $versionPath"
} else {
    $version = Get-Content -Raw -LiteralPath $versionPath | ConvertFrom-Json
    if ($version.version -ne "0.1.14") {
        Add-Failure "Version config must declare version 0.1.14."
    }
    if ($version.branch -ne "codex/0.1.14-global-prefs") {
        Add-Failure "Version config branch must match codex/0.1.14-global-prefs."
    }
    $editionNames = @($version.editions.PSObject.Properties.Name)
    if ($editionNames -notcontains "free") {
        Add-Failure "Version config missing free edition."
    }
    if ($editionNames -notcontains "pro") {
        Add-Failure "Version config missing pro edition."
    }
    $targets = @($version.deployment.vamRoots)
    if ($targets -notcontains "F:\sim\vam") {
        Add-Failure "Version config missing F:\sim\vam deploy root."
    }
    if ($targets -notcontains "C:\vam\virgin-recordable-02") {
        Add-Failure "Version config missing C:\vam\virgin-recordable-02 deploy root."
    }
}

$vamManagedDir = Join-Path $VamRoot "VaM_Data\Managed"
$cecilPath = Join-Path $vamManagedDir "Mono.Cecil.dll"
$vamAssemblyPath = Join-Path $vamManagedDir "Assembly-CSharp.dll"
if (-not (Test-Path -LiteralPath $cecilPath -PathType Leaf)) {
    Add-Failure "Cannot prove VaM selection API; missing Mono.Cecil: $cecilPath"
} elseif (-not (Test-Path -LiteralPath $vamAssemblyPath -PathType Leaf)) {
    Add-Failure "Cannot prove VaM selection API; missing VaM Assembly-CSharp.dll: $vamAssemblyPath"
} else {
    Add-Type -Path $cecilPath
    $resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
    $resolver.AddSearchDirectory($vamManagedDir)
    $readerParameters = New-Object Mono.Cecil.ReaderParameters
    $readerParameters.AssemblyResolver = $resolver
    $vamAssembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($vamAssemblyPath, $readerParameters)
    $superControllerType = $vamAssembly.MainModule.Types |
        Where-Object { $_.FullName -eq "SuperController" } |
        Select-Object -First 1
    $atomType = $vamAssembly.MainModule.Types |
        Where-Object { $_.FullName -eq "Atom" } |
        Select-Object -First 1

    if ($null -eq $superControllerType) {
        Add-Failure "Cannot prove VaM selection API; missing SuperController type."
    } else {
        $selectControllerMethod = $superControllerType.Methods |
            Where-Object {
                $_.Name -eq "SelectController" -and
                $_.Parameters.Count -eq 5 -and
                $_.Parameters[0].ParameterType.FullName -eq "FreeControllerV3" -and
                $_.Parameters[1].Name -eq "alignView" -and
                $_.Parameters[2].Name -eq "alignRotationOnly" -and
                $_.Parameters[3].Name -eq "alignUpDown" -and
                $_.Parameters[4].Name -eq "openUI"
            } |
            Select-Object -First 1
        if ($null -eq $selectControllerMethod) {
            Add-Failure "VaM SuperController selection API proof failed: SelectController(FreeControllerV3, alignView, alignRotationOnly, alignUpDown, openUI) not found."
        }
    }

    if ($null -eq $atomType) {
        Add-Failure "Cannot prove VaM selection API; missing Atom type."
    } else {
        $mainControllerField = $atomType.Fields |
            Where-Object {
                $_.Name -eq "mainController" -and
                $_.FieldType.FullName -eq "FreeControllerV3"
            } |
            Select-Object -First 1
        if ($null -eq $mainControllerField) {
            Add-Failure "VaM atom selection API proof failed: Atom.mainController FreeControllerV3 field not found."
        }
    }
}

$unityProjectFolders = Get-ChildItem -LiteralPath $RepoRoot -Recurse -Force -Directory |
    Where-Object {
        $_.FullName -notmatch "\\\.git(\\|$)" -and
        $_.FullName -notmatch "\\build\\(bin|packages|package_work)(\\|$)" -and
        $_.FullName -notmatch "\\tools(\\|$)" -and
        ($_.Name -eq "Assets" -or $_.Name -eq "Library" -or $_.Name -eq "Packages" -or $_.Name -eq "ProjectSettings")
    }

foreach ($folder in $unityProjectFolders) {
    Add-Failure "Unity project folder is out of scope: $($folder.FullName)"
}

$unityFiles = Get-ChildItem -LiteralPath $RepoRoot -Recurse -Force -File |
    Where-Object {
        $_.FullName -notmatch "\\\.git(\\|$)" -and
        $_.FullName -notmatch "\\build\\(bin|packages|package_work)(\\|$)" -and
        $_.FullName -notmatch "\\tools(\\|$)" -and
        ($_.Extension -eq ".unity" -or $_.Extension -eq ".asset" -or $_.Extension -eq ".assetbundle")
    }

foreach ($file in $unityFiles) {
    Add-Failure "Unity asset file is out of scope: $($file.FullName)"
}

if ($ValidateLiveDeploy.IsPresent) {
    $roots = @("F:\sim\vam", "C:\vam\virgin-recordable-02")
    foreach ($root in $roots) {
        $expectedDlls = @(
            (Join-Path $root "Custom\Plugins\fa_radar.free.0.1.14.dll"),
            (Join-Path $root "Custom\Plugins\fa_radar.pro.0.1.14.dll")
        )
        $legacyLooseScript = Join-Path $root "Custom\Scripts\FrameAngel\Radar\FrameAngelRadar.cs"

        foreach ($deployedDll in $expectedDlls) {
            if (-not (Test-Path -LiteralPath $deployedDll -PathType Leaf)) {
                Add-Failure "Live radar DLL was not deployed: $deployedDll"
            } else {
                $dllItem = Get-Item -LiteralPath $deployedDll
                if ($dllItem.Length -le 0) {
                    Add-Failure "Live radar DLL is empty: $deployedDll"
                }
            }
        }

        if (Test-Path -LiteralPath $legacyLooseScript -PathType Leaf) {
            Add-Failure "Legacy loose radar .cs remains in VaM script load path: $legacyLooseScript"
        }
    }
}

if (Test-Path -LiteralPath $pluginPath) {
    $plugin = Get-Content -Raw -LiteralPath $pluginPath
    if ($plugin.Contains("UpdateLastSelectedBlip(viewer);")) {
        Add-Failure "Previous-selection rendering must stay disabled in 0.1.14."
    }
    if ($plugin.Contains("CreateToggle(lastSelectedEnabledField")) {
        Add-Failure "Last-selected toggle should not be exposed while the paradigm is parked."
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
