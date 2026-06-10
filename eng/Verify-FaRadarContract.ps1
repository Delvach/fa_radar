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
$anchorPresetPath = Join-Path $RepoRoot "payload\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap"

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
        'private const string Version = "0.1.38"',
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
        "DefaultRadarVisualRadiusMeters",
        "MaxRadarVisualDiameterMeters = 1.0f",
        "MaxRadarPlacementScale",
        "DefaultAtomAnchorOffsetZ",
        "DefaultAtomAnchorScale",
        "ResolveMaxPlacementScale",
        "ResolveHudScale",
        "ResolveWristScale",
        "Vector3.one * ResolveHudScale()",
        "Vector3.one * ResolveWristScale()",
        'CreateSlider(hudOffsetXField, false);',
        'CreateSlider(hudOffsetYField, false);',
        'CreateSlider(hudOffsetZField, false);',
        'new JSONStorableFloat("Radar Visual Radius", DefaultRadarVisualRadiusMeters',
        "using MVR.FileManagementSecure",
        "FrameAngelRadarPreferencesRootPath",
        "FrameAngelRadarCommonPreferencesPath",
        "FrameAngelRadarProPreferencesPath",
        "FrameAngelRadarCuaCommonPreferencesPath",
        "FrameAngelRadarCuaProPreferencesPath",
        "FrameAngelRadarCommonPreferencesSchemaVersion",
        "FrameAngelRadarProPreferencesSchemaVersion",
        "FrameAngelRadarCuaCommonPreferencesSchemaVersion",
        "FrameAngelRadarCuaProPreferencesSchemaVersion",
        "Custom\\PluginData\\FrameAngel\\Radar",
        "preferences_cua_common.json",
        "preferences_cua_pro.json",
        "FileManagerSecure.FileExists",
        "FileManagerSecure.ReadAllText",
        "FileManagerSecure.WriteAllText",
        "FileManagerSecure.CreateDirectory",
        "sharedRadarCommonPreferencesCacheKnown",
        "sharedRadarProPreferencesCacheKnown",
        "sharedRadarCuaCommonPreferencesCacheKnown",
        "sharedRadarCuaProPreferencesCacheKnown",
        "LoadGlobalPreferences",
        "WriteGlobalPreferences",
        "TryReadGlobalPreferencesFromDisk",
        "TryReadSharedGlobalPreferencesCache",
        "PollSharedGlobalPreferences",
        "MarkGlobalPreferencesDirty",
        "FlushGlobalPreferencesIfDue",
        "IsCuaPreferenceProfileActive",
        "ResolveCommonPreferencesPath",
        "ResolveProPreferencesPath",
        "ResolveCommonPreferencesSchemaVersion",
        "ResolveProPreferencesSchemaVersion",
        "Global Prefs Auto Save",
        'AppendJsonBoolProperty(sb, ref wroteProperty, "globalPrefsAutoSave", true)',
        "Save Global Prefs",
        "Load Global Prefs",
        "Reset Global Prefs",
        "ShouldUseCreatorAnchorUi",
        "BuildEmptyAnchorUi",
        "BuildEmptyAnchorPlacementUi",
        "BuildSceneSessionUi",
        "BuildFreeSceneSessionUi",
        "BuildFreeEmptyAnchorUi",
        "BuildFreePlacementUi",
        "BuildFreeStaticWorldPlacementUi",
        "BuildSceneSessionPlacementUi",
        "ResetCreatorAnchorPlacement",
        "IsAttachedAtomAnchorHostActive",
        "ResolveAttachedAtomAnchorHost",
        "IsPluginManagerHostAtom",
        "IsEmptyAnchorHostActive",
        "IsSceneSessionPluginHostActive",
        "ResolvePluginHostSurfaceName",
        "ResolveDisplaySurfaceName",
        "IsVrDisplayActive",
        "hostSurfaceField",
        "displaySurfaceField",
        '"Host Surface"',
        '"Display Surface"',
        "SuperController.singleton.isOVR",
        "SuperController.singleton.isOpenVR",
        "SuperController.singleton.disableVR",
        "Creator anchor preset active.",
        "BuildPlacementUi();",
        "private void BuildPlacementUi()",
        "BuildWristCompassUi();",
        "private void BuildWristCompassUi()",
        "ConfigureImmediatePlacementPreferenceCallback",
        "FlushGlobalPreferencesIfDue(true)",
        "valNoCallback",
        "ConfigureGlobalPreferenceField",
        "field.isStorable = false",
        "field.isRestorable = false",
        "FilmSubjectIdentifier",
        '"favr.hud.radar"',
        "BuildFilmSubjectName",
        "FrameAngelRecorderStatePath",
        '"Custom\\PluginData\\FrameAngelMediaCore\\recorder_v2_state.json"',
        "radarHudFilmSubjectIdentifier",
        "radarHudVisible",
        "RadarVisibilityFadeSeconds",
        "WristRevealGraceSeconds",
        "WristHandOffDistanceMeters",
        "SetRadarVisualsVisible",
        "PollRecorderRadarVisibility",
        "ReadRecorderRadarVisible",
        "ApplyRecorderRadarVisibility",
        "Hidden by FAAR radarHudVisible=false.",
        "SetRadarVisualsVisible",
        "SetMaterialAlphaMultiplier",
        "ExtractJsonBool",
        "ExtractJsonString",
        "AnchorModeHud",
        "AnchorModeWorldStatic",
        "AnchorModeContainingAtom",
        "AnchorModeAtomUid",
        'new JSONStorableBool("CUA Anchor Preset", false)',
        "RegisterBool(cuaAnchorPresetField)",
        "anchorModeField = new JSONStorableStringChooser",
        'new JSONStorableString("Anchor Atom UID"',
        "RegisterStringChooser(anchorModeField)",
        "RegisterString(anchorAtomUidField)",
        "Use Selected As Anchor",
        "Use Containing Atom Anchor",
        "Capture Static From Current View",
        "ResolveRadarAnchorTransform",
        "ApplyViewAnchor",
        "ApplyWorldStaticAnchor",
        "ApplyTransformAnchor",
        "ResolveAnchorAtom",
        "FindAtomByUid",
        "field.valNoCallback = NormalizeAnchorMode",
        'field.valNoCallback = value ?? ""',
        '"anchorMode"',
        '"anchorAtomUid"',
        "Anchor To View",
        "Desktop Placement",
        "DesktopPlacementAttachedToUi",
        "DesktopPlacementPinnedInWorld",
        "vrPlacementField",
        '"VR Placement"',
        "ResolveSceneSessionPlacement",
        "ResolveDesktopPlacement",
        "ResolveVRPlacement",
        "ApplySceneSessionPlacementPreference",
        "DesktopVisibilityRecoveryVersion",
        "ApplyDesktopVisibilityRecoveryIfNeeded(preferencesJson)",
        '"desktopVisibilityRecoveryVersion"',
        "NormalizeDesktopPlacement",
        "ApplyDesktopPlacementPreference",
        '"desktopPlacement"',
        '"vrPlacement"',
        "Floor Area Scale",
        "Desktop Tilt Degrees",
        "Height Stems",
        'new JSONStorableBool("Height Stems", true)',
        "Height Scale Meters",
        "Height Stem Alpha",
        "Range Fade Meters",
        "Depth Size Cue",
        "Depth Size Strength",
        "Show Target Markers",
        "Show Lights",
        "Show Custom Unity Assets",
        "Show People",
        "Show Empty",
        "Show SubScene",
        "Show ImagePanel",
        "Show Animation",
        "Show Force",
        "Show Shapes",
        "Show Sounds",
        "Show Triggers",
        "Show Navigation Panels",
        "Show Camera Atoms",
        "Show Uncategorized Atoms",
        "proFilterDefaultsVersion",
        "SetAllProAtomFiltersNoCallback",
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
        "-PositiveModulo(worldPosition.x, safeStep)",
        "-PositiveModulo(worldPosition.z, safeStep)",
        "ResolveGridStepMeters",
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
        "return 1.0f",
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
        "IsFemalePersonAtom",
        "IsMalePersonAtom",
        "IsEmptyAtom",
        "IsSubSceneAtom",
        "IsImagePanelAtom",
        "IsAnimationAtom",
        "IsForceAtom",
        "IsShapeAtom",
        "IsSoundAtom",
        "IsTriggerAtom",
        "IsNavigationPanelAtom",
        "IsCameraAtom",
        "IsRadarGrabHandleAtom",
        "IsAtomVisibleByFilter",
        "return true",
        "ResolveAvailableAtomColor",
        "FreeAtomMarkerColor",
        "return WithAlpha(FreeAtomMarkerColor, alpha)",
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
        "worldPosition - ResolveRadarReferencePosition(viewer)",
        "IsStaticRadarReferenceActive",
        "ResolveRadarReferencePosition",
        "ResolveRadarReferenceRotation",
        "ResolveWorldPositionRadarLocal",
        "ShouldFlattenRadarY",
        "!IsVrDisplayActive()",
        "ResolveAtomMarkerWorldPosition",
        "ResolveAtomVisualBoundsCenter",
        "ResolveRadarReferenceDistanceMeters",
        "UpdateUserMarker",
        "ResolveGridReferencePosition",
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
        "renderer.shadowCastingMode = ShadowCastingMode.Off",
        "renderer.receiveShadows = false",
        "renderer.lightProbeUsage = LightProbeUsage.Off",
        "renderer.reflectionProbeUsage = ReflectionProbeUsage.Off",
        "DestroyRuntimeVisuals",
        "Session Grab Handles",
        'new JSONStorableBool("Grab Handles Enabled", true)',
        'new JSONStorableStringChooser(',
        '"Radar Mode"',
        "RadarModeHud",
        "RadarModeWristLeft",
        "RadarModeWristRight",
        "RadarModeWristLeftAlwaysOn",
        "RadarModeWristRightAlwaysOn",
        "radarMode",
        '"Wrist Offset X"',
        '"Wrist Offset Y"',
        '"Wrist Offset Z"',
        '"Wrist Scale"',
        "wristOffsetX",
        "wristOffsetY",
        "wristOffsetZ",
        "wristScale",
        "GetWristOffset",
        "SetWristOffsetNoCallback",
        "ResolveActivePlacementScale",
        "SetActivePlacementScaleNoCallback",
        'new JSONStorableFloat("Wrist Twist Degrees", 65.0f',
        "ApplyRadarModePreference",
        "SetRadarModeNoCallback",
        "NormalizeRadarMode",
        "ResolveRadarModeForHand",
        "TryCompleteWristGrabHandOff",
        "FinishMoveGrabAfterWristHandOff",
        "wristRevealGraceUntil",
        "wristCompassRevealed",
        "UpdateWristCompassReveal",
        "ResolveRadarRuntimeVisible",
        "IsWristCompassModeActive",
        "ResolveWristCompassAnchorTransform",
        "TryResolveHandTransform",
        "ResolveControllerOutwardTwistDegrees",
        "SuperController.singleton.leftHand",
        "SuperController.singleton.rightHand",
        "PulseGrabHandleHaptics(ResolveWristCompassHand()",
        'new JSONStorableBool("Show Grab Handle Debug", false)',
        'new JSONStorableBool("Grab Haptics", true)',
        "UpdateSessionGrabHandles",
        "Direct Grip Grab",
        "directGripGrabDefaulted",
        "hasDirectGripDefaultMarker",
        "UpdateDirectGripGrab",
        "leftControllerCamera",
        "rightControllerCamera",
        "StartMoveGrab",
        "DestroyResizeGrabHandleAtom",
        "CreateDottedLineMesh",
        "UpdateResizeGuideLine",
        "PulseGrabHandleHaptics",
        "OVRInput.SetControllerVibration",
        "if (IsCuaPreferenceProfileActive())",
        "Grip Grab Fallback",
        'new JSONStorableFloat("Grab Hit Radius Meters", 0.16f',
        "ReadLeftGripValue",
        "ReadRightGripValue",
        "OVRInput.Axis1D.PrimaryHandTrigger",
        "OVRInput.Axis1D.SecondaryHandTrigger",
        "OVRInput.RawButton.LHandTrigger",
        "OVRInput.RawButton.RHandTrigger",
        "TryStartFauxPrimaryGrab",
        "TryStartFauxPrimaryGrab(radarCenter)",
        "moveGrabUsesGripFallback",
        "moveGrabStartRadarWorldCenter",
        "moveGrabCurrentRadarWorldCenter",
        "moveGrabWorldOverrideActive",
        "ApplyMoveGrabWorldAnchor",
        "ApplyMoveGrabWorldCenterToPreferences",
        "TryCompleteWristGrabHandOff(moveGrabCurrentRadarWorldCenter, worldDelta, viewer)",
        "TryCompleteHudGrabHandOff(moveGrabCurrentRadarWorldCenter, worldDelta, viewer)",
        "TryCompleteHudDetachToWrist(moveGrabCurrentRadarWorldCenter, worldDelta, viewer)",
        "ResolveHandoffAlwaysOn",
        "SetWristOffsetNoCallback(targetAnchor.InverseTransformPoint(proposedRadarPosition))",
        "accordionResizeActive",
        "accordionResizeStartDistance",
        "accordionResizeStartScale",
        "AccordionResizeMinimumStartDistanceMeters",
        "UpdateDirectGripAccordionResize",
        "SetActivePlacementScaleNoCallback(accordionResizeStartScale * ratio)",
        "UpdateFauxMoveGrab",
        "UpdateFauxMoveGrab(viewer)",
        "ResolveGripGrabHitRadiusMeters",
        "IsGripPressedThisFrame",
        "GetGripControllerWorldPosition",
        "moveGrabReleaseVelocity",
        "UpdateMoveGrabVelocitySample",
        "TryStartGrabThrowPinOnRelease",
        "UpdateGrabThrowPin(viewer)",
        "FinishGrabThrowPin",
        "CancelGrabThrowPinForGrab",
        "SetStaticWorldRotationNoCallback",
        "Physics.Raycast"
    )

#if FA_RADAR_PRO contract surface lives as source snippets because Free and Pro
# compile from the same file.
    $requiredSnippets += @(
        'new JSONStorableBool("Rotation Axes", true)',
        'new JSONStorableBool("Light Range Volumes", true)',
        'new JSONStorableBool("Spotlight Cones", true)',
        'new JSONStorableBool("User POV Frustum", false)',
        'new JSONStorableBool("Desktop POV Frustum", false)',
        'new JSONStorableBool("Scene Camera Frustums", false)',
        'new JSONStorableFloat("Rotation Axis Length", 0.18f',
        'new JSONStorableFloat("Rotation Axis Width", 0.012f',
        'new JSONStorableFloat("Light Volume Alpha", 0.16f',
        'new JSONStorableFloat("Light Marker Scale", 0.38f',
        'new JSONStorableFloat("POV Frustum Length", 2.0f',
        'new JSONStorableFloat("POV Frustum Alpha", 0.12f',
        "showRotationAxesField",
        "showLightRangeVolumesField",
        "showSpotlightConesField",
        "showUserPovFrustumField",
        "showDesktopPovFrustumField",
        "showSceneCameraFrustumsField",
        "CreateAxisLineMesh",
        "CreateSpotlightConeMesh",
        "Spotlight Cone Mesh Open End",
        "ResolveClippedSpotlightConeScale",
        "ResolveDistanceToRadarShell",
        "lastAvailableAtomVisibleCount",
        "Markers: 0 visible / ",
        "outside range",
        "private struct RadarFrame",
        "private sealed class AtomRecord",
        "private sealed class MarkerSlot",
        "private struct CachedMaterialState",
        "List<AtomRecord> availableAtomRecords",
        "BuildRadarFrame(viewer)",
        "PollAvailableAtomsIfDue(frame)",
        "UpdateAvailableAtomMarkers(frame)",
        "RefreshAtomRecordTransform(record)",
        "ApplyMaterialColorIfChanged",
        "materialStateByMaterial",
        "availableMarkersDirty",
        "lastAvailableMarkerFrameSignature",
        "ResolveAnchorAtomCached",
        "cachedAnchorAtomUid",
        "RecordAtomVisualCenterOffset",
        "CreateFrustumMesh",
        "TryResolveUnityLight",
        "UpdateProTargetVisuals",
        "UpdateProAvailableAtomVisuals",
        "UpdateProCameraFrustums",
        "ResolveLightVolumeColor",
        "LightAlphaDefaultsVersion",
        'new JSONStorableFloat("Point Light Alpha", 0.07f',
        'new JSONStorableFloat("Spotlight Cone Alpha", 0.08f',
        'new JSONStorableFloat("Light Volume Scale", 1.0f',
        'new JSONStorableBool("Throw Pin On Release", false)',
        'new JSONStorableBool("Throw Surface Stop", true)',
        'new JSONStorableFloat("Throw Grow Scale", 1.0f',
        'new JSONStorableFloat("Throw Velocity Scale", 0.45f',
        'ApplySplitLightAlphaDefaultsIfNeeded(preferencesJson)',
        'AppendJsonFloatProperty(sb, ref wroteProperty, "pointLightRangeAlpha"',
        'AppendJsonFloatProperty(sb, ref wroteProperty, "spotlightConeAlpha"',
        'AppendJsonFloatProperty(sb, ref wroteProperty, "lightVolumeScale"',
        'AppendJsonBoolProperty(sb, ref wroteProperty, "grabThrowPinEnabled"',
        'AppendJsonFloatProperty(sb, ref wroteProperty, "grabThrowGrowScale"',
        'ApplyBoolPreference(preferencesJson, "grabThrowPinEnabled", grabThrowPinEnabledField)',
        "ResolveLightVolumeScale",
        "light.type == LightType.Point",
        "light.type != LightType.Directional",
        "CreatePanelMarkerMesh",
        "CreateSubSceneMarkerMesh",
        "CreateBoxMarkerMesh",
        "ApplyMarkerMeshForAtom",
        "ResolveMarkerMeshForAtom",
        "IsPanelLikeAtom",
        "ResolveAxisRadarRotation",
        "ConfigureGlobalPreferenceCallback(showRotationAxesField)",
        'AppendJsonBoolProperty(sb, ref wroteProperty, "showRotationAxes"',
        'ApplyBoolPreference(preferencesJson, "showRotationAxes", showRotationAxesField)',
        "#if FA_RADAR_PRO"
    )

    foreach ($snippet in $requiredSnippets) {
        if (-not $plugin.Contains($snippet)) {
            Add-Failure "Plugin missing required snippet: $snippet"
        }
    }

    $buildSceneSessionUiIndex = $plugin.IndexOf("private void BuildSceneSessionUi()")
    $shouldUseCreatorAnchorUiIndex = $plugin.IndexOf("private bool ShouldUseCreatorAnchorUi()")
    if ($buildSceneSessionUiIndex -ge 0 -and $shouldUseCreatorAnchorUiIndex -gt $buildSceneSessionUiIndex) {
        $sceneSessionUiBlock = $plugin.Substring($buildSceneSessionUiIndex, $shouldUseCreatorAnchorUiIndex - $buildSceneSessionUiIndex)
        if (-not $sceneSessionUiBlock.Contains("#if FA_RADAR_PRO") -or -not $sceneSessionUiBlock.Contains("#else") -or -not $sceneSessionUiBlock.Contains("BuildFreeSceneSessionUi();")) {
            Add-Failure "Scene/session UI must compile-gate Free edition to the simplified Free UI block before Pro controls."
        }
    }

    $freeSceneUiIndex = $plugin.IndexOf("private void BuildFreeSceneSessionUi()")
    $freeEmptyUiIndex = $plugin.IndexOf("private void BuildFreeEmptyAnchorUi()")
    if ($freeSceneUiIndex -lt 0 -or $freeEmptyUiIndex -le $freeSceneUiIndex) {
        Add-Failure "Free scene/session UI block must exist before Free Empty UI block."
    } else {
        $freeSceneUiEndIndex = $plugin.IndexOf("private bool ShouldUseCreatorAnchorUi()", $freeSceneUiIndex)
        $freeSceneUiBlock = if ($freeSceneUiEndIndex -gt $freeSceneUiIndex) {
            $plugin.Substring($freeSceneUiIndex, $freeSceneUiEndIndex - $freeSceneUiIndex)
        } else {
            $plugin.Substring($freeSceneUiIndex, $freeEmptyUiIndex - $freeSceneUiIndex)
        }
        $requiredFreeSceneUiSnippets = @(
            "BuildSceneSessionPlacementUi();",
            "BuildFreePlacementUi();",
            "BuildFreeStaticWorldPlacementUi();"
        )
        foreach ($snippet in $requiredFreeSceneUiSnippets) {
            if (-not $freeSceneUiBlock.Contains($snippet)) {
                Add-Failure "Free scene/session UI missing simplified control snippet: $snippet"
            }
        }

        $forbiddenFreeSceneUiSnippets = @(
            "BuildWristCompassUi();",
            "BuildProFilterUi();",
            "CreateSlider(radarRangeMetersField",
            "CreateToggle(availableAtomMarkersEnabledField",
            "CreateToggle(gridEnabledField",
            "CreateToggle(grabHandlesEnabledField",
            "CreateToggle(grabHapticsEnabledField",
            "CreateTextField(statusField"
        )
        foreach ($snippet in $forbiddenFreeSceneUiSnippets) {
            if ($freeSceneUiBlock.Contains($snippet)) {
                Add-Failure "Free scene/session UI must hide non-core control snippet: $snippet"
            }
        }
    }

    $buildSceneSessionPlacementUiIndex = $plugin.IndexOf("private void BuildSceneSessionPlacementUi()")
    $buildPlacementUiIndex = $plugin.IndexOf("private void BuildPlacementUi()")
    if ($buildSceneSessionPlacementUiIndex -ge 0 -and $buildPlacementUiIndex -gt $buildSceneSessionPlacementUiIndex) {
        $scenePlacementUiBlock = $plugin.Substring($buildSceneSessionPlacementUiIndex, $buildPlacementUiIndex - $buildSceneSessionPlacementUiIndex)
        if (-not $scenePlacementUiBlock.Contains("CreatePopup(desktopPlacementField, false);") -or -not $scenePlacementUiBlock.Contains("CreatePopup(vrPlacementField, true);")) {
            Add-Failure "Free scene/session UI must expose Desktop Placement and VR Placement through the shared placement block."
        }
    }

    $buildFreePlacementUiIndex = $plugin.IndexOf("private void BuildFreePlacementUi()")
    $buildFreeStaticWorldPlacementUiIndex = $plugin.IndexOf("private void BuildFreeStaticWorldPlacementUi()")
    if ($buildFreePlacementUiIndex -ge 0 -and $buildFreeStaticWorldPlacementUiIndex -gt $buildFreePlacementUiIndex) {
        $freePlacementUiBlock = $plugin.Substring($buildFreePlacementUiIndex, $buildFreeStaticWorldPlacementUiIndex - $buildFreePlacementUiIndex)
        $requiredFreePlacementUiSnippets = @(
            "CreateSlider(hudScaleField, false);",
            "CreateSlider(hudOffsetXField, false);",
            "CreateSlider(hudOffsetYField, false);",
            "CreateSlider(hudOffsetZField, false);"
        )
        foreach ($snippet in $requiredFreePlacementUiSnippets) {
            if (-not $freePlacementUiBlock.Contains($snippet)) {
                Add-Failure "Free HUD placement UI missing control snippet: $snippet"
            }
        }
    }

    $buildWristCompassUiIndex = $plugin.IndexOf("private void BuildWristCompassUi()")
    if ($buildFreeStaticWorldPlacementUiIndex -ge 0 -and $buildWristCompassUiIndex -gt $buildFreeStaticWorldPlacementUiIndex) {
        $freeStaticWorldPlacementUiBlock = $plugin.Substring($buildFreeStaticWorldPlacementUiIndex, $buildWristCompassUiIndex - $buildFreeStaticWorldPlacementUiIndex)
        $requiredFreeStaticUiSnippets = @(
            "CreateSlider(staticWorldXField, true);",
            "CreateSlider(staticWorldYField, true);",
            "CreateSlider(staticWorldZField, true);"
        )
        foreach ($snippet in $requiredFreeStaticUiSnippets) {
            if (-not $freeStaticWorldPlacementUiBlock.Contains($snippet)) {
                Add-Failure "Free desktop/static placement UI missing control snippet: $snippet"
            }
        }
    }

    if ($freeEmptyUiIndex -ge 0) {
        $buildEmptyPlacementUiIndex = $plugin.IndexOf("private void BuildEmptyAnchorPlacementUi()")
        if ($buildEmptyPlacementUiIndex -gt $freeEmptyUiIndex) {
            $freeEmptyUiBlock = $plugin.Substring($freeEmptyUiIndex, $buildEmptyPlacementUiIndex - $freeEmptyUiIndex)
            if (-not $freeEmptyUiBlock.Contains("BuildFreePlacementUi();")) {
                Add-Failure "Free Empty/atom-anchor UI must expose only the simplified placement block."
            }
            if ($freeEmptyUiBlock.Contains("BuildProFilterUi();") -or $freeEmptyUiBlock.Contains("CreateSlider(radarRangeMetersField")) {
                Add-Failure "Free Empty/atom-anchor UI must hide filters and radar range tuning."
            }
        }
    }

    $markerMeshFieldSnippet = @"
#if FA_RADAR_PRO
    private Mesh panelMarkerMesh;
    private Mesh subSceneMarkerMesh;
#endif
"@
    if (-not $plugin.Contains($markerMeshFieldSnippet.Trim())) {
        Add-Failure "Panel/SubScene marker meshes must be compiled only for Pro."
    }

    $resolveMarkerMeshIndex = $plugin.IndexOf("private Mesh ResolveMarkerMeshForAtom(Atom atom)")
    $resolveTargetRadarLocalIndex = $plugin.IndexOf("private Vector3 ResolveTargetRadarLocal")
    if ($resolveMarkerMeshIndex -ge 0 -and $resolveTargetRadarLocalIndex -gt $resolveMarkerMeshIndex) {
        $resolveMarkerMeshBlock = $plugin.Substring($resolveMarkerMeshIndex, $resolveTargetRadarLocalIndex - $resolveMarkerMeshIndex)
        if (-not $resolveMarkerMeshBlock.Contains("#if FA_RADAR_PRO")) {
            Add-Failure "Free marker mesh resolution must compile to plain dots; panel/subscene shape logic must be Pro-gated."
        }
        if (-not $resolveMarkerMeshBlock.Contains("return targetBlipMesh;")) {
            Add-Failure "Marker mesh resolver must fall back to the plain dot mesh."
        }
    }

    $placementUiIndex = $plugin.IndexOf("BuildPlacementUi();")
    $grabUiIndex = $plugin.IndexOf("CreateToggle(grabHandlesEnabledField")
    if ($placementUiIndex -lt 0) {
        Add-Failure "Plugin menu must expose placement controls near the top via BuildPlacementUi."
    } elseif ($grabUiIndex -ge 0 -and $placementUiIndex -gt $grabUiIndex) {
        Add-Failure "Placement controls must appear before grab-handle/advanced controls in the plugin menu."
    }

    $hiddenCompatibilityUiSnippets = @(
        "CreatePopup(anchorModeField",
        "CreateTextField(anchorAtomUidField",
        "CreateToggle(anchorToViewField",
        "CreateToggle(selectedGroundDropEnabledField",
        "CreateToggle(heightStemsEnabledField",
        "CreateToggle(depthSizeCueField",
        "CreateToggle(worldAxisAlignField",
        "CreateToggle(groundAxisLockField",
        "CreateToggle(clickSelectMarkersField",
        "CreateToggle(grabHandleDebugVisibleField",
        "CreateToggle(ringsEnabledField",
        "CreateToggle(gridFollowsUserField",
        "CreateToggle(gridClipCircleField",
        "CreateToggle(ignoreContainingAtomField",
        "CreateToggle(cuaAnchorPresetField",
        "CreateSlider(floorAreaScaleField",
        "CreateSlider(gridStepMetersField",
        "CreateSlider(radarVisualRadiusField",
        "CreateSlider(desktopTiltDegreesField",
        "CreateSlider(responseSmoothingField",
        "CreateSlider(wristTwistDegreesField",
        "CreateSlider(ringRotationSpeedField",
        "CreateSlider(targetMarkerScaleField",
        "CreateSlider(heightScaleMetersField",
        "CreateSlider(heightStemAlphaField",
        "CreateSlider(rangeFadeMetersField",
        "CreateSlider(depthSizeStrengthField",
        "CreateSlider(availableAtomAlphaField",
        "CreateSlider(markerClickRadiusPixelsField",
        "CreateSlider(grabHitRadiusMetersField",
        "CreateSlider(shellAlphaField",
        "CreateSlider(ringAlphaField",
        "CreateSlider(gridAlphaField",
        "CreateSlider(markerAlphaField",
        "CreateSlider(emissionStrengthField",
        "CreateSlider(pollIntervalField",
        "CreateSlider(atomPollSecondsField",
        "CreateToggle(globalPrefsAutoSaveField",
        'CreateButton("Save Global Prefs"',
        'CreateButton("Save Anchor Prefs"',
        'CreateButton("Load Global Prefs"',
        'CreateButton("Use Selected As Anchor"',
        'CreateButton("Use Containing Atom Anchor"',
        'CreateButton("Capture Static From Current View"',
        'CreateButton("Capture HUD Offset From Atom"'
    )

    foreach ($snippet in $hiddenCompatibilityUiSnippets) {
        if ($plugin.Contains($snippet)) {
            Add-Failure "Normal plugin UI must keep compatibility/prototype control hidden: $snippet"
        }
    }

    if ($plugin.Contains("CreateToggle(grabHapticsEnabledField, true)") -and $plugin.IndexOf("BuildEmptyAnchorUi") -gt $plugin.IndexOf("CreateToggle(grabHapticsEnabledField, true)")) {
        Add-Failure "Creator-anchor UI must be separated before normal grab/haptics controls."
    }

    if (-not $plugin.Contains("ConfigureGlobalPreferenceField(cuaAnchorPresetField);")) {
        Add-Failure "Legacy CUA Anchor Preset flag must be non-storable/non-restorable so session defaults cannot hide scene/session placement UI."
    }

    $isEmptyIndex = $plugin.IndexOf("private bool IsEmptyAnchorHostActive()")
    $isSceneIndex = $plugin.IndexOf("private bool IsSceneSessionPluginHostActive()")
    if ($isEmptyIndex -ge 0 -and $isSceneIndex -gt $isEmptyIndex) {
        $isEmptyBlock = $plugin.Substring($isEmptyIndex, $isSceneIndex - $isEmptyIndex)
        if ($isEmptyBlock.Contains("cuaAnchorPresetField") -or $isEmptyBlock.Contains(".val")) {
            Add-Failure "Scene/session UI selection must not depend on restored legacy CUA Anchor Preset values."
        }
        if (-not $isEmptyBlock.Contains("IsAttachedAtomAnchorHostActive()")) {
            Add-Failure "Empty-anchor UI selection must be based on the attached atom host."
        }
    }

    $emptyUiIndex = $plugin.IndexOf("private void BuildEmptyAnchorUi()")
    $sceneSessionUiIndex = $plugin.IndexOf("private void BuildSceneSessionUi()")
    if ($emptyUiIndex -ge 0 -and $sceneSessionUiIndex -gt $emptyUiIndex) {
        $emptyUiBlock = $plugin.Substring($emptyUiIndex, $sceneSessionUiIndex - $emptyUiIndex)
        if ($emptyUiBlock.Contains("CreatePopup(vrPlacementField")) {
            Add-Failure "Empty/atom-anchor UI must not expose scene/session VR placement."
        }
        if (-not $emptyUiBlock.Contains("BuildEmptyAnchorPlacementUi();")) {
            Add-Failure "Empty/atom-anchor UI must use the Empty placement block."
        }
    }

    $forbiddenActiveGrabSnippets = @(
        "EnsurePrimaryGrabHandleAtom(radarCenter);",
        "ConfigureGrabHandleAtom(primaryGrabHandleAtom, radarCenter);",
        "UpdateResizeGrabHandle(viewer, primaryController);",
        "controller.drawMeshWhenDeselected = true",
        "controller.hidden = false"
    )

    foreach ($snippet in $forbiddenActiveGrabSnippets) {
        if ($plugin.Contains($snippet)) {
            Add-Failure "Direct grip path must not retain visible/active built-in handle snippet: $snippet"
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
        "\bmotionControllerLeft\b",
        "\bmotionControllerRight\b",
        "CreateSlider\(viewYawOffsetField",
        "CreateSlider\(axisYawOffsetField",
        "viewYawOffsetField\.val",
        "axisYawOffsetField\.val",
        'ApplyBoolPreference\(preferencesJson, "globalPrefsAutoSave", globalPrefsAutoSaveField\);'
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
        "fa_radar.free.0.1.38.dll",
        "fa_radar.pro.0.1.38.dll",
        "UnityEngine.PhysicsModule.dll",
        "FrameAngelDev.Radar.1.var",
        "Preset_FrameAngel_Radar_Empty.vap",
        "Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap",
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
        "fa_radar.free.0.1.38.dll",
        "fa_radar.pro.0.1.38.dll",
        "Preset_FrameAngel_Radar_Empty.vap",
        "Custom\Atom\Empty",
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

if (-not (Test-Path -LiteralPath $anchorPresetPath -PathType Leaf)) {
    Add-Failure "Missing Empty anchor preset: $anchorPresetPath"
} else {
    $anchorPreset = Get-Content -Raw -LiteralPath $anchorPresetPath
    $requiredAnchorPresetSnippets = @(
        '"setUnlistedParamsToDefault" : "true"',
        '"id" : "PluginManager"',
        '"plugin#0" : "Custom/Plugins/fa_radar.pro.0.1.38.dll"',
        '"id" : "plugin#0_FrameAngelRadar"',
        '"Anchor Mode" : "Containing Atom"',
        '"Radar Enabled" : "true"',
        '"Desktop Placement" : "Pinned In World"',
        '"CUA Anchor Preset" : "true"',
        '"HUD Offset X"',
        '"HUD Offset Y"',
        '"HUD Offset Z"',
        '"HUD Scale"',
        '"Anchor Rot X"',
        '"Anchor Rot Y"',
        '"Anchor Rot Z"',
        '"pluginLabel" : "Frame Angel Radar Empty"'
    )

    foreach ($snippet in $requiredAnchorPresetSnippets) {
        if (-not $anchorPreset.Contains($snippet)) {
            Add-Failure "Empty anchor preset missing required snippet: $snippet"
        }
    }
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
    if ($version.version -ne "0.1.38") {
        Add-Failure "Version config must declare version 0.1.38."
    }
    if ($version.branch -ne "codex/0.1.38-performance-architecture") {
        Add-Failure "Version config branch must match codex/0.1.38-performance-architecture."
    }
    $editionNames = @($version.editions.PSObject.Properties.Name)
    if ($editionNames -notcontains "free") {
        Add-Failure "Version config missing free edition."
    }
    if ($editionNames -notcontains "pro") {
        Add-Failure "Version config missing pro edition."
    }
    if ($version.editions.free.pluginFileName -ne "fa_radar.free.0.1.38.dll") {
        Add-Failure "Free edition config must produce fa_radar.free.0.1.38.dll."
    }
    if ($version.editions.free.packageFileName -ne "FrameAngelDev.Radar.1.var") {
        Add-Failure "Free edition config must package as FrameAngelDev.Radar.1.var."
    }
    if ($version.editions.free.packageCreator -ne "FrameAngelDev") {
        Add-Failure "Free edition config must use FrameAngelDev package creator for the first dev package."
    }
    if ($version.editions.free.packageName -ne "Radar") {
        Add-Failure "Free edition config must use packageName Radar for FrameAngelDev.Radar.1.var."
    }
    if ($version.editions.pro.pluginFileName -ne "fa_radar.pro.0.1.38.dll") {
        Add-Failure "Pro edition config must produce fa_radar.pro.0.1.38.dll."
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
            (Join-Path $root "Custom\Plugins\fa_radar.free.0.1.38.dll"),
            (Join-Path $root "Custom\Plugins\fa_radar.pro.0.1.38.dll")
        )
        $expectedAnchorPreset = Join-Path $root "Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap"
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

        if (-not (Test-Path -LiteralPath $expectedAnchorPreset -PathType Leaf)) {
            Add-Failure "Live Empty anchor preset was not deployed: $expectedAnchorPreset"
        }
    }
}

if (Test-Path -LiteralPath $pluginPath) {
    $plugin = Get-Content -Raw -LiteralPath $pluginPath
    if ($plugin.Contains("UpdateLastSelectedBlip(viewer);")) {
            Add-Failure "Previous-selection rendering must stay disabled in 0.1.38."
    }
    if ($plugin.Contains("CreateToggle(lastSelectedEnabledField")) {
        Add-Failure "Last-selected toggle should not be exposed while the paradigm is parked."
    }

    $availableUpdateIndex = $plugin.IndexOf("private void UpdateAvailableAtomMarkers(RadarFrame frame)")
    $availableStatusIndex = $plugin.IndexOf("private void UpdateAvailableAtomMarkerStatus()", [Math]::Max(0, $availableUpdateIndex))
    if ($availableUpdateIndex -lt 0 -or $availableStatusIndex -le $availableUpdateIndex) {
        Add-Failure "Available marker rendering must use the RadarFrame-based optimized update path."
    } else {
        $availableUpdateBlock = $plugin.Substring($availableUpdateIndex, $availableStatusIndex - $availableUpdateIndex)
        if ($availableUpdateBlock.Contains("GetComponentsInChildren<Renderer>")) {
            Add-Failure "Available marker render loop must not scan renderer hierarchies."
        }
        if ($availableUpdateBlock.Contains("GetComponentsInChildren<Light>")) {
            Add-Failure "Available marker render loop must not scan light hierarchies."
        }
        if ($availableUpdateBlock.Contains("ResolveAtomMarkerWorldPosition(atom")) {
            Add-Failure "Available marker render loop must use cached atom marker positions."
        }
        if ($availableUpdateBlock.Contains("ApplyMaterialColor(availableMarkerMaterials")) {
            Add-Failure "Available marker render loop must use cached material-state writes."
        }
    }

    $pollIndex = $plugin.IndexOf("private void PollAvailableAtomsIfDue(RadarFrame frame)")
    $filterIndex = $plugin.IndexOf("private bool IsAtomVisibleByFilter(AtomRecord record)", [Math]::Max(0, $pollIndex))
    if ($pollIndex -lt 0 -or $filterIndex -le $pollIndex) {
        Add-Failure "Available atom polling must build cached AtomRecord entries before filtering."
    } else {
        $pollBlock = $plugin.Substring($pollIndex, $filterIndex - $pollIndex)
        if ($pollBlock.Contains("ResolveAtomMarkerWorldPosition(left") -or $pollBlock.Contains("ResolveAtomMarkerWorldPosition(right")) {
            Add-Failure "Available atom polling sort must use cached distance values, not hierarchy-bound scans in comparer."
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
