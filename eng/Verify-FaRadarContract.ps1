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
$cuaPresetPath = Join-Path $RepoRoot "payload\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap"

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
        'private const string Version = "0.1.53"',
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
        "CreatePersonMarkerMesh",
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
        "MinHudPlacementScale = 0.05f",
        "MaxHudPlacementScale = 1.25f",
        "ResolveMaxHudPlacementScale",
        'new JSONStorableFloat("HUD Scale", 0.49f, MinHudPlacementScale, MaxHudPlacementScale',
        "Mathf.Clamp(scale, MinHudPlacementScale, ResolveMaxHudPlacementScale())",
        "ReadFloat(hudScaleField, 0.49f), MinHudPlacementScale, ResolveMaxHudPlacementScale()",
        "DefaultRadarVisualRadiusMeters",
        "MaxRadarVisualDiameterMeters = 1.0f",
        "MaxRadarPlacementScale",
        "DefaultAtomAnchorOffsetZ",
        "DefaultAtomAnchorScale",
        "HeightStemHalfWidth = 0.010f",
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
        "BuildCuaAnchorUi",
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
        "IsCustomUnityAssetAnchorHostActive",
        "IsRoomCompassModeActive",
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
        "private void BuildPlacementUi()",
        "private void BuildWristCompassUi()",
        'private const string RadarModeWorld = "world"',
        '"World"',
        "string.Equals(ResolveRadarMode(), RadarModeWorld, StringComparison.Ordinal)",
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
        'new JSONStorableBool("Room Compass", false)',
        "RegisterBool(roomCompassField)",
        'ApplyBoolPreference(preferencesJson, "roomCompass", roomCompassField)',
        'AppendJsonBoolProperty(sb, ref wroteProperty, "roomCompass", ReadBool(roomCompassField, false))',
        "ApplyRoomCompassAnchor",
        'return "Room Compass 1:1"',
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
        "Depth Cue Strength",
        "VisualDepthDefaultsVersion",
        "ApplyVisualDepthDefaultsIfNeeded(preferencesJson)",
        "DirectorReadabilityDefaultsVersion",
        "ApplyDirectorReadabilityDefaultsIfNeeded(preferencesJson)",
        '"directorReadabilityDefaultsVersion"',
        "MaxDirectorBackgroundOverlayBudget",
        "DirectorBackgroundOverlayAlphaCeiling",
        "ResolveDirectorBackgroundOverlayBudget",
        "ResolveDirectorOverlayAlpha",
        "ResolveDirectorOverlayScale",
        '"visualDepthDefaultsVersion"',
        'CommonMarkerDefaultsVersion = "target_markers_visible_fade_v2"',
        "Scene Atom Markers",
        "Lights",
        "Custom Unity Assets",
        "People",
        "Empty",
        "SubScene",
        "ImagePanel",
        "Animation",
        "Force",
        "Shapes",
        "Sounds",
        "Triggers",
        "Player Navigation Panel",
        "Cameras",
        "Uncategorized Atoms",
        "proFilterDefaultsVersion",
        "SetAllProAtomFiltersNoCallback",
        "Click Select Markers",
        "Marker Click Radius Pixels",
        "Atom Poll Seconds",
        "Available Atom Alpha",
        "Max Visible Markers",
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
        "ResolveDepthVisibilityAlpha",
        "ResolveAvailableOverlayAlpha",
        "ResolveAvailableOverlayScale",
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
        "personMarkerMesh",
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
        "ResolveMaxVisibleMarkerCount",
        "InsertAvailableAtomRecordByDistance",
        "EnsureAvailableProOverlayCapacity",
        "ConfigureRichOverlayPreferenceCallback",
        "ResolveRichOverlayBudget",
        "HideAvailableProOverlaysOutsideBudget",
        "CanRenderRichAvailableOverlay",
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
        "FarMarkerOuterRadius",
        "FarMarkerMinimumAlpha",
        "ClampRadarLocalToOuterRadius",
        "Mathf.Max(FarMarkerMinimumAlpha",
        "IsStaticRadarReferenceActive",
        "ResolveRadarReferencePosition",
        "ResolveRadarReferenceRotation",
        "ResolveWorldPositionRadarLocal",
        "FineGridStepMeters",
        "CoarseGridStepMeters",
        "CoarseGridRangeThresholdMeters",
        "ResolveGridStepMeters(float rangeMeters)",
        "return rangeMeters >= CoarseGridRangeThresholdMeters ? CoarseGridStepMeters : FineGridStepMeters;",
        "ShouldFlattenRadarY",
        "!IsVrDisplayActive()",
        "ResolveAtomMarkerWorldPosition",
        "ResolveAtomVisualBoundsCenter",
        "TryResolveViewerAnchoredAtomMarkerWorldPosition",
        "HasCategory(record, AtomCategoryNavigationPanel)",
        "worldPosition = frame.viewer.position",
        "RefreshAtomRecordTransform(selectedAtomRecord, frame)",
        "RefreshAtomRecordTransform(record, frame)",
        "ResolveRadarReferenceDistanceMeters",
        "UpdateUserMarker",
        "bool showUserMarker = IsStaticRadarReferenceActive();",
        "SetActiveIfChanged(centerMarkerObject, showUserMarker);",
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
        "HandleDesktopRadarRangeScroll",
        "ScaleRadarRangeMetersFromScroll",
        "IsMouseOverRadarVisual",
        "ResolveRadarScreenRadiusPixels",
        "Input.mouseScrollDelta.y",
        "SetFloatNoCallback(radarRangeMetersField, nextRangeMeters)",
        "MarkGlobalPreferencesDirty()",
        "Radar range {0:0.0}m",
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
        "ResolveMotionControllerTransform(hand)",
        "TryResolveHandTransform",
        "IsMotionControllerTransform",
        "accordionResizeUsesHandFallback",
        "ResolveControllerOutwardTwistDegrees",
        "SuperController.singleton.leftHand",
        "SuperController.singleton.rightHand",
        'new JSONStorableBool("Show Grab Handle Debug", false)',
        'new JSONStorableBool("Grab Haptics", true)',
        "UpdateSessionGrabHandles",
        "Direct Grip Grab",
        "directGripGrabDefaulted",
        "hasDirectGripDefaultMarker",
        "UpdateDirectGripGrab",
        "EnsurePrimaryGrabHandleAtom(radarCenter);",
        "ConfigureGrabHandleAtom(primaryGrabHandleAtom, radarCenter);",
        "LeftFullGrabbedController",
        "RightFullGrabbedController",
        "atom.hidden = true",
        "controller.hidden = true",
        "controller.drawMeshWhenDeselected = false",
        "controller.canGrabPosition = true",
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
        'new JSONStorableBool("User POV Frustum", true)',
        'new JSONStorableBool("Desktop POV Frustum", true)',
        'new JSONStorableBool("Scene Camera Frustums", true)',
        'new JSONStorableFloat("Sphere Alpha", 0.055f',
        'new JSONStorableFloat("Ring Alpha", 0.30f',
        'new JSONStorableFloat("Grid Alpha", 0.11f',
        'new JSONStorableFloat("Height Stem Alpha", 0.26f',
        'new JSONStorableFloat("Depth Cue Strength", 0.55f',
        'new JSONStorableFloat("Available Atom Alpha", 0.34f',
        'new JSONStorableFloat("Detail Overlay Limit", 10.0f',
        'new JSONStorableFloat("Rotation Axis Length", 0.085f',
        'new JSONStorableFloat("Rotation Axis Width", 0.0045f',
        'new JSONStorableFloat("Light Volume Alpha", 0.045f',
        'new JSONStorableFloat("Point Light Alpha", 0.022f',
        'new JSONStorableFloat("Spotlight Cone Alpha", 0.024f',
        'new JSONStorableFloat("Light Volume Scale", 0.62f',
        'new JSONStorableFloat("Light Marker Scale", 0.28f',
        'new JSONStorableFloat("POV Frustum Length", 0.9f',
        'new JSONStorableFloat("POV Frustum Alpha", 0.035f',
        "SetFloatNoCallback(shellAlphaField, 0.055f);",
        "SetFloatNoCallback(depthSizeStrengthField, 0.55f);",
        "float depthAlpha = ResolveDepthVisibilityAlpha(distanceMeters);",
        "Mathf.Clamp01(availableAtomAlphaField.val) * fadeAlpha * depthAlpha",
        "ResolveDirectorOverlayAlpha(fadeAlpha, depthAlpha)",
        "ResolveDirectorOverlayScale(markerScale, depthAlpha)",
        "showRotationAxesField",
        "showLightRangeVolumesField",
        "showSpotlightConesField",
        "showUserPovFrustumField",
        "showDesktopPovFrustumField",
        "showSceneCameraFrustumsField",
        "SelectedTargetOuterRadius",
        "selectedTargetRingObjects",
        "CreateTargetSelectionRingSet",
        "UpdateTargetSelectionRingSet",
        "ResolveSelectedWorldPositionRadarLocal",
        "IsSelectedTargetInsideViewerFrustum",
        "IsRadarUtilityAtom",
        "CreateAxisHalfPairMesh",
        "CreateAxisCenterCubeMesh",
        "RotationAxisObjectCount = 4",
        "RotationAxisVisualPieceCount = 7",
        "rotationAxisHalfPairMesh",
        "rotationAxisCenterCubeMesh",
        "rotationAxisCenterMaterial",
        "pooledCount * RotationAxisObjectCount",
        "markerIndex * RotationAxisObjectCount",
        "Scene Labels",
        "Label Orientation",
        "Label Limit",
        "Label Scale",
        "Label Alpha",
        "LabelsSelectedAndNearest",
        "LabelOrientationFaceViewer",
        "LabelReadabilityDefaultsVersion",
        "MaxRadarLabelLimit",
        "ResolveLabelLimit",
        "ResolveEffectiveLabelScale",
        "ResolveLabelItemLocal",
        "ResolveLabelCalloutLocal",
        "UpdateLabelLeaderLine",
        "CreateLabelLeaderMesh",
        "UpdateSelectedAtomLabel(frame, selectedAtomRecord, target, radarLocal, markerScale, fadeAlpha);",
        "UpdateAvailableAtomLabel(i, slot, record, frame, radarLocal, markerScale, fadeAlpha * depthAlpha);",
        "RefreshActiveLabelOrientations(frame);",
        "PopulateLabelGlyphMesh",
        "ResolveLabelRadarRotation",
        "BuildProPrimaryFilterUi",
        "BuildProDisplayUi",
        "BuildProAdvancedTuningUi",
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
        "ConfigureRichOverlayPreferenceCallback(showRotationAxesField)",
        "ConfigureRichOverlayPreferenceCallback(showLightRangeVolumesField)",
        "ConfigureRichOverlayPreferenceCallback(showSpotlightConesField)",
        "ConfigureRichOverlayPreferenceCallback(richOverlayBudgetField)",
        'AppendJsonBoolProperty(sb, ref wroteProperty, "showRotationAxes"',
        'ApplyBoolPreference(preferencesJson, "showRotationAxes", showRotationAxesField)',
        "#if FA_RADAR_PRO"
    )

    foreach ($snippet in $requiredSnippets) {
        if (-not $plugin.Contains($snippet)) {
            Add-Failure "Plugin missing required snippet: $snippet"
        }
    }

    $buildCuaAnchorUiIndex = $plugin.IndexOf("private void BuildCuaAnchorUi()")
    $buildSceneSessionUiIndex = $plugin.IndexOf("private void BuildSceneSessionUi()")
    if ($buildCuaAnchorUiIndex -lt 0 -or $buildSceneSessionUiIndex -le $buildCuaAnchorUiIndex) {
        Add-Failure "CUA-hosted UI block must remain inspectable before scene/session UI."
    } else {
        $cuaUiBlock = $plugin.Substring($buildCuaAnchorUiIndex, $buildSceneSessionUiIndex - $buildCuaAnchorUiIndex)
        foreach ($requiredCuaUiSnippet in @(
            "CreateToggle(radarEnabledField, false);",
            "CreateToggle(roomCompassField, true);",
            "CreatePopup(radarModeField, false);",
            "CreateSlider(hudScaleField, true);",
            "CreateSlider(wristScaleField, true);",
            "CreateToggle(grabHandlesEnabledField, false);",
            "CreateToggle(grabHapticsEnabledField, true);",
            "BuildProPrimaryFilterUi();",
            "BuildProDisplayUi();",
            "BuildProAdvancedTuningUi();"
        )) {
            if (-not $cuaUiBlock.Contains($requiredCuaUiSnippet)) {
                Add-Failure "CUA-hosted UI missing required control snippet: $requiredCuaUiSnippet"
            }
        }

        foreach ($forbiddenCuaUiSnippet in @(
            "BuildSceneSessionPlacementUi();",
            "BuildPlacementUi();",
            "BuildEmptyAnchorPlacementUi();",
            "BuildWristCompassUi();",
            "hudOffsetXField",
            "hudOffsetYField",
            "hudOffsetZField",
            "anchorRotationXField",
            "anchorRotationYField",
            "anchorRotationZField",
            "wristOffsetXField",
            "wristOffsetYField",
            "wristOffsetZField",
            "desktopPlacementField",
            "vrPlacementField"
        )) {
            if ($cuaUiBlock.Contains($forbiddenCuaUiSnippet)) {
                Add-Failure "CUA-hosted UI must rely on CUA movement and hide wrist/placement control snippet: $forbiddenCuaUiSnippet"
            }
        }
    }

    $shouldUseCreatorAnchorUiIndex = $plugin.IndexOf("private bool ShouldUseCreatorAnchorUi()")
    if ($buildSceneSessionUiIndex -ge 0 -and $shouldUseCreatorAnchorUiIndex -gt $buildSceneSessionUiIndex) {
        $sceneSessionUiBlock = $plugin.Substring($buildSceneSessionUiIndex, $shouldUseCreatorAnchorUiIndex - $buildSceneSessionUiIndex)
        if (-not $sceneSessionUiBlock.Contains("#if FA_RADAR_PRO") -or -not $sceneSessionUiBlock.Contains("#else") -or -not $sceneSessionUiBlock.Contains("BuildFreeSceneSessionUi();")) {
            Add-Failure "Scene/session UI must compile-gate Free edition to the simplified Free UI block before Pro controls."
        }
        foreach ($legacyPlacementCall in @(
            "BuildSceneSessionPlacementUi();",
            "BuildPlacementUi();",
            "BuildWristCompassUi();"
        )) {
            if ($sceneSessionUiBlock.Contains($legacyPlacementCall)) {
                Add-Failure "0.1.53 scene/session UI must hide the excessive placement block call: $legacyPlacementCall"
            }
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
            "CreateToggle(roomCompassField, true);",
            "CreatePopup(radarModeField, false);",
            "CreateSlider(hudScaleField, false);",
            "CreateSlider(wristScaleField, true);",
            "CreateToggle(grabHandlesEnabledField, false);",
            "CreateToggle(grabHapticsEnabledField, true);"
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
            foreach ($snippet in @(
                "CreatePopup(radarModeField, false);",
                "CreateSlider(hudScaleField, false);",
                "CreateSlider(wristScaleField, true);",
                "CreateToggle(grabHandlesEnabledField, false);",
                "CreateToggle(grabHapticsEnabledField, true);"
            )) {
                if (-not $freeEmptyUiBlock.Contains($snippet)) {
                    Add-Failure "Free Empty/atom-anchor UI missing 0.1.53 mode/scale/grab control: $snippet"
                }
            }
            if ($freeEmptyUiBlock.Contains("BuildProFilterUi();") -or $freeEmptyUiBlock.Contains("CreateSlider(radarRangeMetersField")) {
                Add-Failure "Free Empty/atom-anchor UI must hide filters and radar range tuning."
            }
        }
    }

    $markerMeshFieldSnippet = @"
#if FA_RADAR_PRO
    private Mesh personMarkerMesh;
    private Mesh panelMarkerMesh;
    private Mesh subSceneMarkerMesh;
#endif
"@
    if (-not $plugin.Contains($markerMeshFieldSnippet.Trim())) {
        Add-Failure "Person/Panel/SubScene marker meshes must be compiled only for Pro."
    }

    $resolveAnchorModeIndex = $plugin.IndexOf("private string ResolveAnchorMode()")
    $desktopAttachedIndex = $plugin.IndexOf("private bool IsDesktopPlacementAttachedToUi()", [Math]::Max(0, $resolveAnchorModeIndex))
    if ($resolveAnchorModeIndex -lt 0 -or $desktopAttachedIndex -le $resolveAnchorModeIndex) {
        Add-Failure "World-mode anchor resolver block must remain inspectable."
    } else {
        $resolveAnchorModeBlock = $plugin.Substring($resolveAnchorModeIndex, $desktopAttachedIndex - $resolveAnchorModeIndex)
        $worldIndex = $resolveAnchorModeBlock.IndexOf("RadarModeWorld")
        $emptyIndex = $resolveAnchorModeBlock.IndexOf("IsEmptyAnchorHostActive()")
        if ($worldIndex -lt 0 -or $emptyIndex -lt 0 -or $worldIndex -gt $emptyIndex) {
            Add-Failure "World mode must select the static world anchor before Empty/CUA host anchoring."
        }
    }

    $wristActiveIndex = $plugin.IndexOf("private bool IsWristCompassModeActive()")
    $runtimeVisibleIndex = $plugin.IndexOf("private bool ResolveRadarRuntimeVisible", [Math]::Max(0, $wristActiveIndex))
    if ($wristActiveIndex -lt 0 -or $runtimeVisibleIndex -le $wristActiveIndex) {
        Add-Failure "Wrist-mode activation block must remain inspectable."
    } else {
        $wristActiveBlock = $plugin.Substring($wristActiveIndex, $runtimeVisibleIndex - $wristActiveIndex)
        if ($wristActiveBlock.Contains("IsCuaPreferenceProfileActive")) {
            Add-Failure "Wrist modes must not be suppressed merely because Radar is hosted on Empty/CUA."
        }
    }

    $grabEligibilityIndex = $plugin.IndexOf("private bool ShouldUseSessionGrabHandles")
    $ensurePrimaryIndex = $plugin.IndexOf("private void EnsurePrimaryGrabHandleAtom", [Math]::Max(0, $grabEligibilityIndex))
    if ($grabEligibilityIndex -lt 0 -or $ensurePrimaryIndex -le $grabEligibilityIndex) {
        Add-Failure "Stock full-grab target eligibility block must remain inspectable."
    } else {
        $grabEligibilityBlock = $plugin.Substring($grabEligibilityIndex, $ensurePrimaryIndex - $grabEligibilityIndex)
        if ($grabEligibilityBlock.Contains("IsCuaPreferenceProfileActive") -or $grabEligibilityBlock.Contains("IsCustomUnityAssetAtom")) {
            Add-Failure "The existing stock full-grab center target must remain eligible on Empty/CUA hosts."
        }
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
        if (-not $resolveMarkerMeshBlock.Contains("IsPersonAtom(atom)") -or
            -not $resolveMarkerMeshBlock.Contains("personMarkerMesh != null ? personMarkerMesh : targetBlipMesh") -or
            -not $resolveMarkerMeshBlock.Contains("HasCategory(record, AtomCategoryPerson)")) {
            Add-Failure "Pro marker mesh resolver must route Person atoms through the generated person marker mesh."
        }
    }

    $placementUiIndex = $plugin.IndexOf("CreatePopup(radarModeField, false);")
    $grabUiIndex = $plugin.IndexOf("CreateToggle(grabHandlesEnabledField")
    if ($placementUiIndex -lt 0) {
        Add-Failure "Plugin menu must expose Radar Mode near the top."
    } elseif ($grabUiIndex -ge 0 -and $placementUiIndex -gt $grabUiIndex) {
        Add-Failure "Radar Mode must appear before grab-handle/advanced controls in the plugin menu."
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
    $isCustomUnityAssetIndex = $plugin.IndexOf("private bool IsCustomUnityAssetAnchorHostActive()")
    if ($isEmptyIndex -ge 0 -and $isCustomUnityAssetIndex -gt $isEmptyIndex) {
        $isEmptyBlock = $plugin.Substring($isEmptyIndex, $isCustomUnityAssetIndex - $isEmptyIndex)
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
        if (-not $emptyUiBlock.Contains("CreateToggle(roomCompassField, true);")) {
            Add-Failure "Empty/atom-anchor UI must expose the default-off Room Compass toggle."
        }
    }

    $forbiddenActiveGrabSnippets = @(
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
        "\bSystem\.Runtime\.InteropServices\b",
        "\bValve\.VR\b",
        "\bSteamVR_",
        "\bJSONClass\b",
        "\bJSONNode\b",
        "\bSimpleJSON\b",
        "\bCanvas\b",
        "\bTextMesh\b",
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
        "fa_radar.free.0.1.53.dll",
        "fa_radar.pro.0.1.53.dll",
        "UnityEngine.PhysicsModule.dll",
        "UnityEngine.JSONSerializeModule.dll",
        "FrameAngelDev.Radar.1.var",
        "Preset_FrameAngel_Radar_Empty.vap",
        "Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap",
        "Preset_FrameAngel_Radar_CUA.vap",
        "Custom/Atom/CustomUnityAsset/Preset_FrameAngel_Radar_CUA.vap",
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
        'expectedPluginFileNames = @($editionBuilds',
        "Preset_FrameAngel_Radar_Empty.vap",
        "Custom\Atom\Empty",
        "Preset_FrameAngel_Radar_CUA.vap",
        "Custom\Atom\CustomUnityAsset",
        "Assert-FaRadarVamNotRunning",
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
        '"plugin#0" : "Custom/Plugins/fa_radar.pro.0.1.53.dll"',
        '"id" : "plugin#0_FrameAngelRadar"',
        '"Anchor Mode" : "Containing Atom"',
        '"Radar Enabled" : "true"',
        '"Desktop Placement" : "Pinned In World"',
        '"CUA Anchor Preset" : "true"',
        '"Room Compass" : "false"',
        '"Radar Mode" : "HUD"',
        '"Grab Handles Enabled" : "true"',
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

if (-not (Test-Path -LiteralPath $cuaPresetPath -PathType Leaf)) {
    Add-Failure "Missing CustomUnityAsset Radar preset: $cuaPresetPath"
} else {
    $cuaPreset = Get-Content -Raw -LiteralPath $cuaPresetPath
    $requiredCuaPresetSnippets = @(
        '"setUnlistedParamsToDefault" : "true"',
        '"id" : "PluginManager"',
        '"plugin#0" : "Custom/Plugins/fa_radar.pro.0.1.53.dll"',
        '"id" : "plugin#0_FrameAngelRadar"',
        '"pluginLabel" : "Frame Angel Radar CUA"',
        '"CUA Anchor Preset" : "true"',
        '"Radar Enabled" : "true"',
        '"Room Compass" : "false"',
        '"Radar Mode" : "HUD"',
        '"Grab Handles Enabled" : "true"',
        '"HUD Scale" : "0.75"'
    )
    foreach ($snippet in $requiredCuaPresetSnippets) {
        if (-not $cuaPreset.Contains($snippet)) {
            Add-Failure "CustomUnityAsset Radar preset missing required snippet: $snippet"
        }
    }
    foreach ($forbiddenCuaPresetSnippet in @(
        '"Anchor Mode"',
        '"Anchor Atom UID"',
        '"HUD Offset',
        '"Anchor Rot',
        '"Wrist Offset',
        '"Wrist Scale"',
        '"Desktop Placement"',
        '"VR Placement"',
        '"Grab Haptics"'
    )) {
        if ($cuaPreset.Contains($forbiddenCuaPresetSnippet)) {
            Add-Failure "CustomUnityAsset Radar preset must not carry wrist/session-placement state: $forbiddenCuaPresetSnippet"
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
    if ($version.version -ne "0.1.53") {
        Add-Failure "Version config must declare version 0.1.53."
    }
    if ($version.branch -ne "codex/0.1.53-world-wrist") {
        Add-Failure "Version config branch must match codex/0.1.53-world-wrist."
    }
    $editionNames = @($version.editions.PSObject.Properties.Name)
    if ($editionNames -notcontains "free") {
        Add-Failure "Version config missing free edition."
    }
    if ($editionNames -notcontains "pro") {
        Add-Failure "Version config missing pro edition."
    }
    if ($version.editions.free.pluginFileName -ne "fa_radar.free.0.1.53.dll") {
        Add-Failure "Free edition config must produce fa_radar.free.0.1.53.dll."
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
    if ($version.editions.pro.pluginFileName -ne "fa_radar.pro.0.1.53.dll") {
        Add-Failure "Pro edition config must produce fa_radar.pro.0.1.53.dll."
    }
    if ($version.editions.pro.creatorResources.customUnityAssetPreset -ne "Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap") {
        Add-Failure "Pro edition config must declare the CustomUnityAsset Radar preset."
    }
    if ([bool]$version.editions.pro.creatorResources.roomCompassDefault) {
        Add-Failure "Room Compass must remain default-off in version authority."
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
            (Join-Path $root "Custom\Plugins\fa_radar.free.0.1.53.dll"),
            (Join-Path $root "Custom\Plugins\fa_radar.pro.0.1.53.dll")
        )
        $expectedAnchorPreset = Join-Path $root "Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap"
        $expectedCuaPreset = Join-Path $root "Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap"
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
        if (-not (Test-Path -LiteralPath $expectedCuaPreset -PathType Leaf)) {
            Add-Failure "Live CustomUnityAsset Radar preset was not deployed: $expectedCuaPreset"
        }
    }
}

if (Test-Path -LiteralPath $pluginPath) {
    $plugin = Get-Content -Raw -LiteralPath $pluginPath
    if ($plugin.Contains("UpdateLastSelectedBlip(viewer);")) {
            Add-Failure "Previous-selection rendering must stay disabled in 0.1.48."
    }
    if ($plugin.Contains("CreateToggle(lastSelectedEnabledField")) {
        Add-Failure "Last-selected toggle should not be exposed while the paradigm is parked."
    }
    if ($plugin.Contains('new JSONStorableFloat("HUD Scale", 0.49f, 0.25f') -or
        $plugin.Contains('new JSONStorableFloat("HUD Scale", 0.49f, MinHudPlacementScale, MaxRadarPlacementScale') -or
        $plugin.Contains("Mathf.Clamp(scale, 0.25f, ResolveMaxPlacementScale())") -or
        $plugin.Contains("Mathf.Clamp(scale, MinHudPlacementScale, ResolveMaxPlacementScale())") -or
        $plugin.Contains("ReadFloat(hudScaleField, 0.49f), 0.25f") -or
        $plugin.Contains("ReadFloat(hudScaleField, 0.49f), MinHudPlacementScale, ResolveMaxPlacementScale()")) {
        Add-Failure "HUD scale fine range regressed to the old broad placement slider range."
    }
    if ($plugin.Contains('BuildFilmSubjectName("User Ground') -or
        $plugin.Contains('BuildFilmSubjectName("Self Ground') -or
        $plugin.Contains("userGridDropObject") -or
        $plugin.Contains("selfGridDropObject")) {
        Add-Failure "User/self ground-drop marker must stay removed; self uses User Center plus optional height stem only."
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
        if ($availableUpdateBlock.Contains("fadeAlpha <= 0.01f") -or $availableUpdateBlock.Contains("rangeHiddenCount++")) {
            Add-Failure "Available markers must fade outside range instead of disappearing at a low-alpha cutoff."
        }
    }

    $gridStepIndex = $plugin.IndexOf("private float ResolveGridStepMeters()")
    $effectiveRangeIndex = $plugin.IndexOf("private float ResolveEffectiveRadarRangeMeters()", [Math]::Max(0, $gridStepIndex))
    if ($gridStepIndex -lt 0 -or $effectiveRangeIndex -le $gridStepIndex) {
        Add-Failure "Grid step resolver must remain inspectable before effective range resolution."
    } else {
        $gridStepBlock = $plugin.Substring($gridStepIndex, $effectiveRangeIndex - $gridStepIndex)
        if ($gridStepBlock.Contains("return 1.0f;") -or -not $gridStepBlock.Contains("ResolveEffectiveRadarRangeMeters()")) {
            Add-Failure "Grid step must use range-aware LOD, not the old fixed one-meter resolver."
        }
    }
    if (-not $plugin.Contains("int stepCount = Mathf.Clamp(Mathf.CeilToInt(safeRange / safeStep) + 2, 1, 32);")) {
        Add-Failure "Grid mesh must keep a capped step count for large ranges."
    }

    foreach ($oldAxisSnippet in @(
        "new GameObject[pooledCount * 3]",
        "newAxisObjects[(i * 3) + axis]",
        "axisObjects[(index * 3) + axis]",
        "markerIndex * 3",
        "CreateAxisLineMesh"
    )) {
        if ($plugin.Contains($oldAxisSnippet)) {
            Add-Failure "Rotation axis glyph must use the 0.1.48 four-renderer/seven-piece contract, not old full-bar axis snippet: $oldAxisSnippet"
        }
    }

    $labelLimitIndex = $plugin.IndexOf("private int ResolveLabelLimit()")
    $selectedLabelIndex = $plugin.IndexOf("private void UpdateSelectedAtomLabel")
    if ($labelLimitIndex -lt 0 -or $selectedLabelIndex -le $labelLimitIndex) {
        Add-Failure "Scene labels must expose a capped ResolveLabelLimit helper before selected/available label rendering."
    } else {
        $labelLimitBlock = $plugin.Substring($labelLimitIndex, $selectedLabelIndex - $labelLimitIndex)
        if (-not $labelLimitBlock.Contains("Mathf.Clamp(Mathf.RoundToInt(ReadFloat(labelLimitField, DefaultLabelLimit)), 0, MaxRadarLabelLimit)")) {
            Add-Failure "Scene labels must cap available labels through Label Limit, not Max Visible Markers."
        }
        if ($labelLimitBlock.Contains("maxVisibleMarkersField")) {
            Add-Failure "Scene label limit must not inherit Max Visible Markers."
        }
    }

    $selectedLabelBlockEnd = $plugin.IndexOf("private void UpdateAvailableAtomLabel", [Math]::Max(0, $selectedLabelIndex))
    if ($selectedLabelIndex -ge 0 -and $selectedLabelBlockEnd -gt $selectedLabelIndex) {
        $selectedLabelBlock = $plugin.Substring($selectedLabelIndex, $selectedLabelBlockEnd - $selectedLabelIndex)
        if ($selectedLabelBlock.Contains("ResolveLabelLimit()") -or $selectedLabelBlock.Contains("CanRenderRichAvailableOverlay") -or $selectedLabelBlock.Contains("ResolveRichOverlayBudget")) {
            Add-Failure "Selected target label must stay outside available label/detail overlay budgets."
        }
        if (-not $selectedLabelBlock.Contains("UpdateLabelLeaderLine(targetLabelLeaderObject, itemLocal, labelLocal,")) {
            Add-Failure "Selected target label must draw a pooled leader line from item to outside-shell callout."
        }
    }

    if ($plugin -notmatch 'sceneLabelsField\s*=\s*new JSONStorableStringChooser\(\s*"Scene Labels",[\s\S]*?new List<string> \{ LabelsOff, LabelsSelected, LabelsSelectedAndNearest \},\s*LabelsSelected,\s*"Scene Labels"\);') {
        Add-Failure "Scene labels must default to Selected, not Selected + Nearest."
    }
    foreach ($labelDefaultSnippet in @(
        'private const float DefaultLabelLimit = 4.0f;',
        'private const float DefaultLabelScale = 0.045f;',
        'new JSONStorableFloat("Label Limit", DefaultLabelLimit',
        'new JSONStorableFloat("Label Scale", DefaultLabelScale',
        'ReadFloat(labelScaleField, DefaultLabelScale)',
        'SetSceneLabelsNoCallback(LabelsSelected)',
        'SetFloatNoCallback(labelLimitField, DefaultLabelLimit)',
        'SetFloatNoCallback(labelScaleField, DefaultLabelScale)',
        'AppendJsonStringProperty(sb, ref wroteProperty, "labelReadabilityDefaultsVersion", LabelReadabilityDefaultsVersion)'
    )) {
        if (-not $plugin.Contains($labelDefaultSnippet)) {
            Add-Failure "Scene label readability defaults missing snippet: $labelDefaultSnippet"
        }
    }
    foreach ($oldLabelSnippet in @(
        'new JSONStorableFloat("Label Limit", 12.0f',
        'new JSONStorableFloat("Label Scale", 0.085f',
        "ReadFloat(labelLimitField, 12.0f)",
        "ReadFloat(labelScaleField, 0.085f)",
        "(radarLocal * visualRadius) + new Vector3(offsetScale * 0.72f"
    )) {
        if ($plugin.Contains($oldLabelSnippet)) {
            Add-Failure "Scene labels regressed to loud/inside-sphere 0.1.48 behavior: $oldLabelSnippet"
        }
    }

    $availableLabelIndex = $plugin.IndexOf("private void UpdateAvailableAtomLabel")
    $refreshLabelIndex = $plugin.IndexOf("private void RefreshActiveLabelOrientations", [Math]::Max(0, $availableLabelIndex))
    if ($availableLabelIndex -lt 0 -or $refreshLabelIndex -le $availableLabelIndex) {
        Add-Failure "Available scene label rendering block must remain inspectable."
    } else {
        $availableLabelBlock = $plugin.Substring($availableLabelIndex, $refreshLabelIndex - $availableLabelIndex)
        if (-not $availableLabelBlock.Contains("UpdateLabelLeaderLine(slot.labelLeaderObject, itemLocal, labelLocal,")) {
            Add-Failure "Available labels must draw pooled leader lines from item to outside-shell callouts."
        }
    }

    $ensureLabelIndex = $plugin.IndexOf("private void EnsureAvailableLabelSlot")
    $setAvailableLabelIndex = $plugin.IndexOf("private void SetAvailableLabelVisible", [Math]::Max(0, $ensureLabelIndex))
    if ($ensureLabelIndex -lt 0 -or $setAvailableLabelIndex -le $ensureLabelIndex) {
        Add-Failure "Available scene label pool creation block must remain inspectable."
    } else {
        $ensureLabelBlock = $plugin.Substring($ensureLabelIndex, $setAvailableLabelIndex - $ensureLabelIndex)
        if (-not $ensureLabelBlock.Contains("slot.labelLeaderObject = CreateMeshObject")) {
            Add-Failure "Available label slots must create their pooled leader object once."
        }
    }

    $sceneUiIndex = $plugin.IndexOf("private void BuildSceneSessionUi()")
    $freeSceneUiIndex = $plugin.IndexOf("private void BuildFreeSceneSessionUi()", [Math]::Max(0, $sceneUiIndex))
    if ($sceneUiIndex -lt 0 -or $freeSceneUiIndex -le $sceneUiIndex) {
        Add-Failure "Scene/session UI builder block must remain inspectable."
    } else {
        $sceneUiBlock = $plugin.Substring($sceneUiIndex, $freeSceneUiIndex - $sceneUiIndex)
        $primaryCall = $sceneUiBlock.IndexOf("BuildProPrimaryFilterUi();")
        $displayCall = $sceneUiBlock.IndexOf("BuildProDisplayUi();")
        $advancedCall = $sceneUiBlock.IndexOf("BuildProAdvancedTuningUi();")
        if ($primaryCall -lt 0 -or $displayCall -le $primaryCall -or $advancedCall -le $displayCall) {
            Add-Failure "Scene/session UI must show primary filters first, then display toggles, then advanced tuning."
        }
        if ($sceneUiBlock.Contains("CreateTextField(hostSurfaceField") -or $sceneUiBlock.Contains("CreateTextField(displaySurfaceField")) {
            Add-Failure "Scene/session UI must not spend the top screen on host/display debug text fields."
        }
    }

    $emptyUiIndex = $plugin.IndexOf("private void BuildEmptyAnchorUi()")
    $freeEmptyUiIndex = $plugin.IndexOf("private void BuildFreeEmptyAnchorUi()", [Math]::Max(0, $emptyUiIndex))
    if ($emptyUiIndex -lt 0 -or $freeEmptyUiIndex -le $emptyUiIndex) {
        Add-Failure "Empty-anchor UI builder block must remain inspectable."
    } else {
        $emptyUiBlock = $plugin.Substring($emptyUiIndex, $freeEmptyUiIndex - $emptyUiIndex)
        $primaryCall = $emptyUiBlock.IndexOf("BuildProPrimaryFilterUi();")
        $displayCall = $emptyUiBlock.IndexOf("BuildProDisplayUi();")
        $advancedCall = $emptyUiBlock.IndexOf("BuildProAdvancedTuningUi();")
        if ($primaryCall -lt 0 -or $displayCall -le $primaryCall -or $advancedCall -le $displayCall) {
            Add-Failure "Empty-anchor UI must show primary filters first, then display toggles, then advanced tuning."
        }
        if ($emptyUiBlock.Contains("CreateTextField(hostSurfaceField") -or $emptyUiBlock.Contains("CreateTextField(displaySurfaceField")) {
            Add-Failure "Empty-anchor UI must not expose host/display debug text fields at the top."
        }
        if (-not $emptyUiBlock.Contains("CreateToggle(roomCompassField, true);")) {
            Add-Failure "Empty-anchor UI must expose Room Compass beside the primary enable control."
        }
    }

    $roomCompassHelperIndex = $plugin.IndexOf("private bool IsRoomCompassModeActive()")
    $sceneHostHelperIndex = $plugin.IndexOf("private bool IsSceneSessionPluginHostActive()", [Math]::Max(0, $roomCompassHelperIndex))
    if ($roomCompassHelperIndex -lt 0 -or $sceneHostHelperIndex -le $roomCompassHelperIndex) {
        Add-Failure "Room Compass activation helper must remain inspectable."
    } else {
        $roomCompassHelperBlock = $plugin.Substring($roomCompassHelperIndex, $sceneHostHelperIndex - $roomCompassHelperIndex)
        if (-not $roomCompassHelperBlock.Contains("IsAttachedAtomAnchorHostActive()") -or
            -not $roomCompassHelperBlock.Contains("roomCompassField.val")) {
            Add-Failure "Room Compass must be available to every atom-attached Radar host and controlled by its default-off storable."
        }
    }

    $roomCompassAnchorIndex = $plugin.IndexOf("private void ApplyRoomCompassAnchor()")
    $viewAnchorIndex = $plugin.IndexOf("private void ApplyViewAnchor", [Math]::Max(0, $roomCompassAnchorIndex))
    if ($roomCompassAnchorIndex -lt 0 -or $viewAnchorIndex -le $roomCompassAnchorIndex) {
        Add-Failure "Room Compass scene-origin anchor block must remain inspectable."
    } else {
        $roomCompassAnchorBlock = $plugin.Substring($roomCompassAnchorIndex, $viewAnchorIndex - $roomCompassAnchorIndex)
        foreach ($requiredRoomAnchorSnippet in @(
            "hudRoot.transform.SetParent(null, false);",
            "hudRoot.transform.position = Vector3.zero;",
            "hudRoot.transform.rotation = Quaternion.identity;",
            "hudRoot.transform.localScale = Vector3.one * ResolveHudScale();"
        )) {
            if (-not $roomCompassAnchorBlock.Contains($requiredRoomAnchorSnippet)) {
                Add-Failure "Room Compass must leave its host atom untouched and place only Radar content at scene origin: $requiredRoomAnchorSnippet"
            }
        }
        if ($roomCompassAnchorBlock.Contains("containingAtom") -or $roomCompassAnchorBlock.Contains("mainController")) {
            Add-Failure "Room Compass anchor code must not mutate or reposition the containing host atom."
        }
    }

    $effectiveRangeIndex = $plugin.IndexOf("private float ResolveEffectiveRadarRangeMeters()")
    $smoothPositionIndex = $plugin.IndexOf("private Vector3 SmoothPosition", [Math]::Max(0, $effectiveRangeIndex))
    if ($effectiveRangeIndex -lt 0 -or $smoothPositionIndex -le $effectiveRangeIndex) {
        Add-Failure "Room Compass 1:1 scale helpers must remain inspectable."
    } else {
        $effectiveScaleBlock = $plugin.Substring($effectiveRangeIndex, $smoothPositionIndex - $effectiveRangeIndex)
        if (([regex]::Matches($effectiveScaleBlock, 'ResolveHudScale\(\) \* ResolveVisualRadius\(\)')).Count -lt 2) {
            Add-Failure "Room Compass horizontal and vertical map scales must both cancel the visual root scale for 1:1 world placement."
        }
        if (-not $effectiveScaleBlock.Contains("return ResolveConfiguredRadarRangeMeters() / Mathf.Max(0.001f, ResolveHudScale());")) {
            Add-Failure "Room Compass surface radius must cancel HUD Scale so its sphere, rings, and grid use the configured world-space range."
        }
    }

    $dishIndex = $plugin.IndexOf("private void UpdateRadarDish(Transform viewer)")
    $dishEndIndex = $plugin.IndexOf("private bool IsFlatDesktopCircleActive()", [Math]::Max(0, $dishIndex))
    if ($dishIndex -lt 0 -or $dishEndIndex -le $dishIndex) {
        Add-Failure "Radar dish surface scale block must remain inspectable."
    } else {
        $dishBlock = $plugin.Substring($dishIndex, $dishEndIndex - $dishIndex)
        foreach ($surfaceSnippet in @(
            "float surfaceLocalRadius = ResolveRadarSurfaceLocalRadius();",
            "flatCircleObject.transform.localScale = Vector3.one * surfaceLocalRadius;",
            "sphereObject.transform.localScale = Vector3.one * surfaceLocalRadius;",
            "gridObject.transform.localScale = Vector3.one * surfaceLocalRadius;",
            "ring.transform.localScale = Vector3.one * (surfaceLocalRadius * 1.015f);"
        )) {
            if (-not $dishBlock.Contains($surfaceSnippet)) {
                Add-Failure "Room Compass world-range surface scaling is incomplete: $surfaceSnippet"
            }
        }
    }

    $clampIndex = $plugin.IndexOf("private Vector3 ClampRadarLocalToRadius")
    $worldMetersIndex = $plugin.IndexOf("private Vector3 ResolveWorldMetersFromReference", [Math]::Max(0, $clampIndex))
    if ($clampIndex -lt 0 -or $worldMetersIndex -le $clampIndex -or
        -not $plugin.Substring($clampIndex, $worldMetersIndex - $clampIndex).Contains("if (IsRoomCompassModeActive())")) {
        Add-Failure "Room Compass must bypass radar-shell position clamping so world overlays stay on their subjects."
    }

    $labelRotationIndex = $plugin.IndexOf("private Quaternion ResolveLabelRadarRotation")
    $sanitizeLabelIndex = $plugin.IndexOf("private string SanitizeRadarLabelText", [Math]::Max(0, $labelRotationIndex))
    if ($labelRotationIndex -lt 0 -or $sanitizeLabelIndex -le $labelRotationIndex) {
        Add-Failure "Label facing correction block must remain inspectable."
    } else {
        $labelRotationBlock = $plugin.Substring($labelRotationIndex, $sanitizeLabelIndex - $labelRotationIndex)
        if (-not $labelRotationBlock.Contains("Quaternion.Euler(0.0f, 180.0f, 0.0f)") -or
            ([regex]::Matches($labelRotationBlock, 'readableFacingCorrection')).Count -lt 4) {
            Add-Failure "Procedural labels must apply the readable 180-degree facing correction in viewer, world-axis, and object-rotation modes."
        }
    }

    if (-not $plugin.Contains("float width = HeightStemHalfWidth;")) {
        Add-Failure "Height stems must use the reduced X/Z cross-section constant."
    }

    $primaryUiIndex = $plugin.IndexOf("private void BuildProPrimaryFilterUi()")
    $displayUiIndex = $plugin.IndexOf("private void BuildProDisplayUi()", [Math]::Max(0, $primaryUiIndex))
    if ($primaryUiIndex -lt 0 -or $displayUiIndex -le $primaryUiIndex) {
        Add-Failure "Pro UI must split primary filters into BuildProPrimaryFilterUi before display controls."
    } else {
        $primaryUiBlock = $plugin.Substring($primaryUiIndex, $displayUiIndex - $primaryUiIndex)
        foreach ($primarySnippet in @(
            "showLightAtomsField",
            "showPersonAtomsField",
            "showCameraAtomsField",
            "showCustomUnityAssetAtomsField",
            "showEmptyAtomsField",
            "showSubSceneAtomsField",
            "showImagePanelAtomsField",
            "showNavigationPanelAtomsField",
            "showOtherAtomsField"
        )) {
            if (-not $primaryUiBlock.Contains($primarySnippet)) {
                Add-Failure "Primary Pro filter UI missing top checkbox field: $primarySnippet"
            }
        }
        foreach ($advancedSnippet in @(
            "labelScaleField",
            "richOverlayBudgetField",
            "rotationAxisLengthField",
            "pointLightRangeAlphaField",
            "povFrustumLengthField",
            "grabThrowGrowScaleField"
        )) {
            if ($primaryUiBlock.Contains($advancedSnippet)) {
                Add-Failure "Primary Pro filter UI must not include advanced tuning field: $advancedSnippet"
            }
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
        if ($pollBlock.Contains("availableAtomRecords.Sort")) {
            Add-Failure "Available atom polling must use bounded insertion instead of full-list O(n log n) sort."
        }
        if ($pollBlock.Contains("BuildAtomRecord(atom, frame, availableAtomRecords.Count)") -or $pollBlock.Contains("BuildAtomRecord(atom, frame, availableAtomRecords.Count);")) {
            Add-Failure "Available atom polling must run cheap filtering before renderer/light metadata hydration."
        }
    }

    if (-not $plugin.Contains("record.markerLocalOffset = record.root.InverseTransformPoint(center);")) {
        Add-Failure "Cached atom visual offsets must use Transform.InverseTransformPoint so scaled atom roots stay correct."
    }
    if (-not $plugin.Contains("public Vector3 lastRootScale;")) {
        Add-Failure "AtomRecord must cache root scale so scaled atom changes invalidate marker positions."
    }
    if (-not $plugin.Contains("record.lastRootScale")) {
        Add-Failure "AtomRecord transform refresh must compare and update cached root scale."
    }
    if (-not $plugin.Contains("availableMarkersDirty = true;") -or -not $plugin.Contains("materialStateByMaterial.Clear();")) {
        Add-Failure "Material alpha changes must invalidate pooled available marker material state."
    }

    $capacityIndex = $plugin.IndexOf("private void EnsureAvailableMarkerCapacity(int requiredCount)")
    $availableUpdateIndexForCapacity = $plugin.IndexOf("private void EnsureAvailableProOverlayCapacity(int requiredCount)", [Math]::Max(0, $capacityIndex))
    if ($availableUpdateIndexForCapacity -lt 0) {
        $availableUpdateIndexForCapacity = $plugin.IndexOf("private void UpdateAvailableAtomMarkers(RadarFrame frame)", [Math]::Max(0, $capacityIndex))
    }
    if ($capacityIndex -lt 0 -or $availableUpdateIndexForCapacity -le $capacityIndex) {
        Add-Failure "Available marker capacity function must remain inspectable before marker update."
    } else {
        $capacityBlock = $plugin.Substring($capacityIndex, $availableUpdateIndexForCapacity - $capacityIndex)
        if (($capacityBlock.Contains("FA Radar Available Rotation Axis")) -or
            ($capacityBlock.Contains("FA Radar Available Light Range")) -or
            ($capacityBlock.Contains("FA Radar Available Spotlight Cone"))) {
            Add-Failure "Available marker pool growth must not eagerly allocate Pro rich overlay renderers for every slot."
        }
    }

    $rangeScrollCallIndex = $plugin.IndexOf("HandleDesktopRadarRangeScroll(viewer);")
    $clickCallIndex = $plugin.IndexOf("HandleRadarMarkerClick();", [Math]::Max(0, $rangeScrollCallIndex))
    if ($rangeScrollCallIndex -lt 0 -or $clickCallIndex -le $rangeScrollCallIndex) {
        Add-Failure "Desktop hover-scroll range handling must run before radar marker click handling in TickRadar."
    }

    $rangeScrollHelperIndex = $plugin.IndexOf("private void HandleDesktopRadarRangeScroll")
    $rangeScrollNextHelperIndex = $plugin.IndexOf("private void HandleRadarMarkerClick", [Math]::Max(0, $rangeScrollHelperIndex))
    if ($rangeScrollHelperIndex -lt 0 -or $rangeScrollNextHelperIndex -le $rangeScrollHelperIndex) {
        Add-Failure "Desktop hover-scroll range helper must stay inspectable next to mouse click handling."
    } else {
        $rangeScrollBlock = $plugin.Substring($rangeScrollHelperIndex, $rangeScrollNextHelperIndex - $rangeScrollHelperIndex)
        foreach ($forbiddenRangeScrollSnippet in @(
            "hudScaleField",
            "SetHudScaleNoCallback",
            "SetActivePlacementScaleNoCallback",
            "wristScaleField",
            "Event.current.Use",
            "OnGUI"
        )) {
            if ($rangeScrollBlock.Contains($forbiddenRangeScrollSnippet)) {
                Add-Failure "Desktop hover-scroll range helper must not touch HUD or wrist scale snippet: $forbiddenRangeScrollSnippet"
            }
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
