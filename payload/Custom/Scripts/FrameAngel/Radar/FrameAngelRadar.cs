using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.Rendering;

public class FrameAngelRadar : MVRScript
{
    private const string Version = "0.1.20";
#if FA_RADAR_PRO && FA_RADAR_FREE
#error Define only one FA Radar edition symbol.
#endif
#if FA_RADAR_PRO
    private const bool IsProEdition = true;
    private const string EditionName = "Pro";
#else
    // FA_RADAR_FREE is the default-safe build behavior when no Pro symbol is present.
    private const bool IsProEdition = false;
    private const string EditionName = "Free";
#endif
    private const int ShellRenderQueue = 4980;
    private const int GridRenderQueue = 4990;
    private const int RingRenderQueue = 5000;
    private const int MarkerRenderQueue = 5010;
    private const int ShellSortingOrder = 32730;
    private const int GridSortingOrder = 32740;
    private const int RingSortingOrder = 32750;
    private const int MarkerSortingOrder = 32760;
    private const string FrameAngelRadarPreferencesRootPath = "Custom\\PluginData\\FrameAngel\\Radar";
    private const string FrameAngelRadarCommonPreferencesPath = "Custom\\PluginData\\FrameAngel\\Radar\\preferences_common.json";
    private const string FrameAngelRadarProPreferencesPath = "Custom\\PluginData\\FrameAngel\\Radar\\preferences_pro.json";
    private const string FrameAngelRadarCuaCommonPreferencesPath = "Custom\\PluginData\\FrameAngel\\Radar\\preferences_cua_common.json";
    private const string FrameAngelRadarCuaProPreferencesPath = "Custom\\PluginData\\FrameAngel\\Radar\\preferences_cua_pro.json";
    private const string FrameAngelRadarCommonPreferencesSchemaVersion = "frameangel_radar_common_preferences_v1";
    private const string FrameAngelRadarProPreferencesSchemaVersion = "frameangel_radar_pro_preferences_v1";
    private const string FrameAngelRadarCuaCommonPreferencesSchemaVersion = "frameangel_radar_cua_common_preferences_v1";
    private const string FrameAngelRadarCuaProPreferencesSchemaVersion = "frameangel_radar_cua_pro_preferences_v1";
    private const string FilmSubjectIdentifier = "favr.hud.radar";
    private const string FrameAngelRecorderStatePath = "Custom\\PluginData\\FrameAngelMediaCore\\recorder_v2_state.json";
    private const string AnchorModeHud = "HUD / View";
    private const string AnchorModeWorldStatic = "World Static";
    private const string AnchorModeContainingAtom = "Containing Atom";
    private const string AnchorModeAtomUid = "Anchor Atom UID";
    private const string GrabHandlePrimarySuffix = "primary";
    private const string GrabHandleResizeSuffix = "resize";
    private const float GlobalPreferencesFlushDelaySeconds = 0.75f;
    private const float GlobalPreferencesSharedStatePollIntervalSeconds = 1.0f;
    private const float RecorderVisibilityPollIntervalSeconds = 0.25f;
    private const float GrabResizeMinimumStartDistanceMeters = 0.05f;
    private const float GrabHapticCooldownSeconds = 0.08f;
    private const float GripGrabPressThreshold = 0.62f;
    private const float GripGrabReleaseThreshold = 0.34f;
    private const int GrabHandUnknown = -1;
    private const int GrabHandLeft = 0;
    private const int GrabHandRight = 1;
    private static readonly Color AxisXColor = new Color(1.0f, 0.18f, 0.12f, 1.0f);
    private static readonly Color AxisYColor = new Color(0.22f, 1.0f, 0.34f, 1.0f);
    private static readonly Color AxisZColor = new Color(0.26f, 0.52f, 1.0f, 1.0f);
    private static bool sharedRadarCommonPreferencesCacheKnown;
    private static string sharedRadarCommonPreferencesJson = "";
    private static float sharedRadarCommonPreferencesNextReadAt;
    private static bool sharedRadarProPreferencesCacheKnown;
    private static string sharedRadarProPreferencesJson = "";
    private static float sharedRadarProPreferencesNextReadAt;
    private static bool sharedRadarCuaCommonPreferencesCacheKnown;
    private static string sharedRadarCuaCommonPreferencesJson = "";
    private static float sharedRadarCuaCommonPreferencesNextReadAt;
    private static bool sharedRadarCuaProPreferencesCacheKnown;
    private static string sharedRadarCuaProPreferencesJson = "";
    private static float sharedRadarCuaProPreferencesNextReadAt;

    private JSONStorableBool radarEnabledField;
    private JSONStorableBool ignoreContainingAtomField;
    private JSONStorableBool placementModeField;
    private JSONStorableBool ringsEnabledField;
    private JSONStorableBool gridEnabledField;
    private JSONStorableBool gridFollowsUserField;
    private JSONStorableBool gridClipCircleField;
    private JSONStorableBool anchorToViewField;
    private JSONStorableBool desktopTopDownField;
    private JSONStorableBool flatDesktopCircleField;
    private JSONStorableBool worldAxisAlignField;
    private JSONStorableBool groundAxisLockField;
    private JSONStorableBool lastSelectedEnabledField;
    private JSONStorableBool selectedGroundDropEnabledField;
    private JSONStorableBool heightStemsEnabledField;
    private JSONStorableBool depthSizeCueField;
    private JSONStorableBool availableAtomMarkersEnabledField;
    private JSONStorableBool showLightAtomsField;
    private JSONStorableBool showCustomUnityAssetAtomsField;
    private JSONStorableBool showPersonAtomsField;
    private JSONStorableBool showOtherAtomsField;
    private JSONStorableBool clickSelectMarkersField;
    private JSONStorableBool grabHandlesEnabledField;
    private JSONStorableBool grabHandleDebugVisibleField;
    private JSONStorableBool grabHapticsEnabledField;
    private JSONStorableBool globalPrefsAutoSaveField;
    private JSONStorableBool cuaAnchorPresetField;

    private JSONStorableFloat hudOffsetXField;
    private JSONStorableFloat hudOffsetYField;
    private JSONStorableFloat hudOffsetZField;
    private JSONStorableFloat hudScaleField;
    private JSONStorableFloat desktopTiltDegreesField;
    private JSONStorableFloat radarRangeMetersField;
    private JSONStorableFloat floorAreaScaleField;
    private JSONStorableFloat radarVisualRadiusField;
    private JSONStorableFloat gridStepMetersField;
    private JSONStorableFloat shellAlphaField;
    private JSONStorableFloat ringAlphaField;
    private JSONStorableFloat gridAlphaField;
    private JSONStorableFloat markerAlphaField;
    private JSONStorableFloat emissionStrengthField;
    private JSONStorableFloat ringRotationSpeedField;
    private JSONStorableFloat targetMarkerScaleField;
    private JSONStorableFloat lastSelectedFadeSecondsField;
    private JSONStorableFloat heightScaleMetersField;
    private JSONStorableFloat heightStemAlphaField;
    private JSONStorableFloat rangeFadeMetersField;
    private JSONStorableFloat depthSizeStrengthField;
    private JSONStorableFloat atomPollSecondsField;
    private JSONStorableFloat availableAtomAlphaField;
    private JSONStorableFloat markerClickRadiusPixelsField;
    private JSONStorableFloat grabHitRadiusMetersField;
    private JSONStorableFloat pollIntervalField;
    private JSONStorableFloat responseSmoothingField;
    private JSONStorableFloat anchorRotationXField;
    private JSONStorableFloat anchorRotationYField;
    private JSONStorableFloat anchorRotationZField;
    private JSONStorableFloat staticWorldXField;
    private JSONStorableFloat staticWorldYField;
    private JSONStorableFloat staticWorldZField;
    private JSONStorableFloat staticWorldPitchField;
    private JSONStorableFloat staticWorldYawField;
    private JSONStorableFloat staticWorldRollField;

    private JSONStorableString statusField;
    private JSONStorableString anchorAtomUidField;
    private JSONStorableStringChooser anchorModeField;

    private GameObject hudRoot;
    private GameObject radarRoot;
    private GameObject axisRoot;
    private GameObject flatCircleObject;
    private GameObject sphereObject;
    private GameObject gridObject;
    private GameObject centerMarkerObject;
    private GameObject userHeightStemObject;
    private GameObject targetBlipObject;
    private GameObject targetHeightStemObject;
    private GameObject targetGridDropObject;
    private GameObject lastTargetBlipObject;
    private GameObject lastTargetGridDropObject;
    private GameObject resizeGuideLineObject;
    private GameObject[] availableMarkerObjects;
    private GameObject[] availableStemObjects;
    private GameObject[] ringObjects;
    private Quaternion[] ringBaseRotations;
    private MeshFilter gridFilter;

    private Mesh sphereMesh;
    private Mesh flatCircleMesh;
    private Mesh ringMesh;
    private Mesh gridMesh;
    private Mesh targetBlipMesh;
    private Mesh centerMarkerMesh;
    private Mesh heightStemMesh;
    private Mesh resizeGuideLineMesh;

    private Material shellMaterial;
    private Material ringMaterial;
    private Material ringXMaterial;
    private Material ringZMaterial;
    private Material gridMaterial;
    private Material centerMaterial;
    private Material userHeightStemMaterial;
    private Material targetMaterial;
    private Material targetHeightStemMaterial;
    private Material targetDropMaterial;
    private Material lastTargetMaterial;
    private Material lastTargetDropMaterial;
    private Material availableHeightStemMaterial;
    private Material grabGuideMaterial;
    private Material[] availableMarkerMaterials;

    private Atom primaryGrabHandleAtom;
    private Atom resizeGrabHandleAtom;
    private Atom selectedAtom;
    private Atom lastSelectedAtom;
    private List<Atom> trackedAvailableAtoms = new List<Atom>();
    private string selectedUid = "";
    private string lastSelectedUid = "";
    private float nextSelectionPollTime;
    private float nextAtomPollTime;
    private float lastSelectedAtTime = -1000.0f;
    private float lastGridRangeMeters = -1.0f;
    private float lastGridStepMeters = -1.0f;
    private Vector2 lastGridOffsetMeters;
    private bool lastGridClipCircle;
    private bool haveLastGridOffset;
    private bool visualsReady;
    private bool haveSmoothedHudPosition;
    private bool globalPreferencesLoading;
    private bool globalPreferencesDirty;
    private bool recorderRadarVisible = true;
    private bool lastAppliedRecorderRadarVisible = true;
    private bool cuaAnchorPresetApplied;
    private bool primaryGrabHandleCreatePending;
    private bool resizeGrabHandleCreatePending;
    private bool moveGrabActive;
    private bool resizeGrabActive;
    private bool moveGrabUsesGripFallback;
    private bool resizeGrabUsesGripFallback;
    private bool resizeHandleDismissedUntilMoveRelease;
    private bool ovrLeftGrabHapticActive;
    private bool ovrRightGrabHapticActive;
    private Vector3 smoothedHudPosition;
    private Vector3 moveStartHandlePosition;
    private Vector3 moveStartHudOffset;
    private Vector3 moveStartStaticPosition;
    private Vector3 resizeStartPrimaryPosition;
    private Vector3 resizeStartHandlePosition;
    private float nextGlobalPreferencesFlushTime;
    private float nextGlobalPreferencesPollTime;
    private float nextRecorderVisibilityPollTime;
    private float resizeStartScale;
    private float resizeStartDistance;
    private float leftGripValue;
    private float rightGripValue;
    private float previousLeftGripValue;
    private float previousRightGripValue;
    private float lastGrabHandleHapticAt;
    private float ovrLeftGrabHapticStopAt;
    private float ovrRightGrabHapticStopAt;
    private float radarMaterialAlphaMultiplier = 1.0f;
    private int moveGrabHand = GrabHandUnknown;
    private int resizeGrabHand = GrabHandUnknown;
    private string lastAppliedCommonPreferencesJson = "";
    private string lastAppliedProPreferencesJson = "";
    private string primaryGrabHandleUid = "";
    private string resizeGrabHandleUid = "";
    private Transform currentHudAnchor;
    private Transform lastGoodViewerTransform;

    public override void Init()
    {
        BuildStorables();
        LoadGlobalPreferences();
        BuildUi();
        EnsureRuntimeVisuals();
        SetStatus("Frame Angel Radar " + Version + " " + EditionName + " ready.");
    }

    private void Update()
    {
        if (!visualsReady)
        {
            EnsureRuntimeVisuals();
        }

        PollSharedGlobalPreferences();
        FlushGlobalPreferencesIfDue(false);
        TickGrabHandleHapticStops(Time.unscaledTime);
        TickRadar();
    }

    private void OnDestroy()
    {
        FlushGlobalPreferencesIfDue(true);
        DestroySessionGrabHandles();
        DestroyRuntimeVisuals();
    }

    private void BuildStorables()
    {
        radarEnabledField = new JSONStorableBool("Radar Enabled", true);
        ignoreContainingAtomField = new JSONStorableBool("Ignore Attached Atom", true);
        placementModeField = new JSONStorableBool("Placement Mode", false);
        ringsEnabledField = new JSONStorableBool("Rings Enabled", true);
        gridEnabledField = new JSONStorableBool("Grid Enabled", true);
        gridFollowsUserField = new JSONStorableBool("Grid Follows User", true);
        gridClipCircleField = new JSONStorableBool("Grid Clip Circle", true);
        anchorToViewField = new JSONStorableBool("Anchor To View", true);
        desktopTopDownField = new JSONStorableBool("Flatten Target Y", false);
        flatDesktopCircleField = new JSONStorableBool("Flat Desktop Circle", false);
        worldAxisAlignField = new JSONStorableBool("World Axis Align", true);
        groundAxisLockField = new JSONStorableBool("Ground Axis Lock", true);
        // Previous Selection Disabled for now; it was adding a second paradigm too early.
        lastSelectedEnabledField = new JSONStorableBool("Last Selected Enabled", false);
        selectedGroundDropEnabledField = new JSONStorableBool("Selected Ground Drop", false);
        heightStemsEnabledField = new JSONStorableBool("Height Stems", true);
        depthSizeCueField = new JSONStorableBool("Depth Size Cue", true);
        availableAtomMarkersEnabledField = new JSONStorableBool("Available Atom Markers", true);
        showLightAtomsField = new JSONStorableBool("Show Lights", true);
        showCustomUnityAssetAtomsField = new JSONStorableBool("Show CUA", false);
        showPersonAtomsField = new JSONStorableBool("Show People", false);
        showOtherAtomsField = new JSONStorableBool("Show Other Atoms", false);
        clickSelectMarkersField = new JSONStorableBool("Click Select Markers", true);
        // Grip Grab Fallback: keep session handles visual/debug-only when VaM does not report them as grabbed.
        // Session Grab Handles are a disposable VR interaction aid for the session/scene plugin path only.
        grabHandlesEnabledField = new JSONStorableBool("Grab Handles Enabled", false);
        grabHandleDebugVisibleField = new JSONStorableBool("Show Grab Handle Debug", false);
        grabHapticsEnabledField = new JSONStorableBool("Grab Haptics", true);
        globalPrefsAutoSaveField = new JSONStorableBool("Global Prefs Auto Save", true);
        cuaAnchorPresetField = new JSONStorableBool("CUA Anchor Preset", false);
        anchorModeField = new JSONStorableStringChooser(
            "Anchor Mode",
            new List<string> { AnchorModeHud, AnchorModeWorldStatic, AnchorModeContainingAtom, AnchorModeAtomUid },
            AnchorModeHud,
            "Anchor Mode");
        anchorModeField.displayChoices = new List<string> { "HUD / View", "World Static", "Containing Atom", "Anchor Atom UID" };

        hudOffsetXField = new JSONStorableFloat("HUD Offset X", -0.59f, -1.0f, 1.0f, true, true);
        hudOffsetYField = new JSONStorableFloat("HUD Offset Y", 0.22f, -1.0f, 1.0f, true, true);
        hudOffsetZField = new JSONStorableFloat("HUD Offset Z", 0.78f, 0.15f, 1.5f, true, true);
        hudScaleField = new JSONStorableFloat("HUD Scale", 0.49f, 0.25f, 3.0f, true, true);
        desktopTiltDegreesField = new JSONStorableFloat("Desktop Tilt Degrees", 90.0f, 0.0f, 90.0f, true, true);
        radarRangeMetersField = new JSONStorableFloat("Radar Range Meters", 5.0f, 0.5f, 30.0f, true, true);
        floorAreaScaleField = new JSONStorableFloat("Floor Area Scale", 1.0f, 0.25f, 6.0f, true, true);
        radarVisualRadiusField = new JSONStorableFloat("Radar Visual Radius", 0.08f, 0.025f, 0.25f, true, true);
        gridStepMetersField = new JSONStorableFloat("Grid Step Meters", 1.0f, 0.25f, 5.0f, true, true);
        shellAlphaField = new JSONStorableFloat("Sphere Alpha", 0.09f, 0.0f, 0.45f, true, true);
        ringAlphaField = new JSONStorableFloat("Ring Alpha", 0.34f, 0.02f, 0.9f, true, true);
        gridAlphaField = new JSONStorableFloat("Grid Alpha", 0.16f, 0.0f, 0.5f, true, true);
        markerAlphaField = new JSONStorableFloat("Marker Alpha", 0.9f, 0.1f, 1.0f, true, true);
        emissionStrengthField = new JSONStorableFloat("Emission Strength", 1.4f, 0.0f, 4.0f, true, true);
        ringRotationSpeedField = new JSONStorableFloat("Ring Rotation Speed", 0.0f, 0.0f, 90.0f, true, true);
        targetMarkerScaleField = new JSONStorableFloat("Target Marker Scale", 0.09f, 0.025f, 0.25f, true, true);
        lastSelectedFadeSecondsField = new JSONStorableFloat("Last Selected Fade Seconds", 12.0f, 1.0f, 60.0f, true, true);
        heightScaleMetersField = new JSONStorableFloat("Height Scale Meters", 6.0f, 1.0f, 20.0f, true, true);
        heightStemAlphaField = new JSONStorableFloat("Height Stem Alpha", 0.42f, 0.0f, 1.0f, true, true);
        rangeFadeMetersField = new JSONStorableFloat("Range Fade Meters", 1.25f, 0.0f, 10.0f, true, true);
        depthSizeStrengthField = new JSONStorableFloat("Depth Size Strength", 0.35f, 0.0f, 1.0f, true, true);
        atomPollSecondsField = new JSONStorableFloat("Atom Poll Seconds", 0.75f, 0.15f, 5.0f, true, true);
        availableAtomAlphaField = new JSONStorableFloat("Available Atom Alpha", 0.46f, 0.0f, 1.0f, true, true);
        markerClickRadiusPixelsField = new JSONStorableFloat("Marker Click Radius Pixels", 20.0f, 4.0f, 80.0f, true, true);
        grabHitRadiusMetersField = new JSONStorableFloat("Grab Hit Radius Meters", 0.16f, 0.04f, 0.45f, true, true);
        pollIntervalField = new JSONStorableFloat("Selection Poll Seconds", 0.15f, 0.03f, 1.0f, true, true);
        responseSmoothingField = new JSONStorableFloat("Response Smoothing", 0.0f, 0.0f, 1.0f, true, true);
        anchorRotationXField = new JSONStorableFloat("Anchor Rot X", 0.0f, -180.0f, 180.0f, true, true);
        anchorRotationYField = new JSONStorableFloat("Anchor Rot Y", 0.0f, -180.0f, 180.0f, true, true);
        anchorRotationZField = new JSONStorableFloat("Anchor Rot Z", 0.0f, -180.0f, 180.0f, true, true);
        staticWorldXField = new JSONStorableFloat("Static World X", 0.0f, -20.0f, 20.0f, true, true);
        staticWorldYField = new JSONStorableFloat("Static World Y", 1.5f, -5.0f, 20.0f, true, true);
        staticWorldZField = new JSONStorableFloat("Static World Z", 1.0f, -20.0f, 20.0f, true, true);
        staticWorldPitchField = new JSONStorableFloat("Static Pitch", 0.0f, -180.0f, 180.0f, true, true);
        staticWorldYawField = new JSONStorableFloat("Static Yaw", 0.0f, -180.0f, 180.0f, true, true);
        staticWorldRollField = new JSONStorableFloat("Static Roll", 0.0f, -180.0f, 180.0f, true, true);

        statusField = new JSONStorableString("Status", "");
        anchorAtomUidField = new JSONStorableString("Anchor Atom UID", "");

        RegisterBool(radarEnabledField);
        RegisterBool(ignoreContainingAtomField);
        RegisterBool(placementModeField);
        RegisterBool(ringsEnabledField);
        RegisterBool(gridEnabledField);
        RegisterBool(gridFollowsUserField);
        RegisterBool(gridClipCircleField);
        RegisterBool(anchorToViewField);
        RegisterBool(desktopTopDownField);
        RegisterBool(flatDesktopCircleField);
        RegisterBool(worldAxisAlignField);
        RegisterBool(groundAxisLockField);
        RegisterBool(lastSelectedEnabledField);
        RegisterBool(selectedGroundDropEnabledField);
        RegisterBool(heightStemsEnabledField);
        RegisterBool(depthSizeCueField);
        RegisterBool(availableAtomMarkersEnabledField);
        RegisterBool(showLightAtomsField);
        RegisterBool(showCustomUnityAssetAtomsField);
        RegisterBool(showPersonAtomsField);
        RegisterBool(showOtherAtomsField);
        RegisterBool(clickSelectMarkersField);
        RegisterBool(grabHandlesEnabledField);
        RegisterBool(grabHandleDebugVisibleField);
        RegisterBool(grabHapticsEnabledField);
        RegisterBool(globalPrefsAutoSaveField);
        RegisterBool(cuaAnchorPresetField);
        RegisterStringChooser(anchorModeField);

        RegisterFloat(hudOffsetXField);
        RegisterFloat(hudOffsetYField);
        RegisterFloat(hudOffsetZField);
        RegisterFloat(hudScaleField);
        RegisterFloat(desktopTiltDegreesField);
        RegisterFloat(radarRangeMetersField);
        RegisterFloat(floorAreaScaleField);
        RegisterFloat(radarVisualRadiusField);
        RegisterFloat(gridStepMetersField);
        RegisterFloat(shellAlphaField);
        RegisterFloat(ringAlphaField);
        RegisterFloat(gridAlphaField);
        RegisterFloat(markerAlphaField);
        RegisterFloat(emissionStrengthField);
        RegisterFloat(ringRotationSpeedField);
        RegisterFloat(targetMarkerScaleField);
        RegisterFloat(lastSelectedFadeSecondsField);
        RegisterFloat(heightScaleMetersField);
        RegisterFloat(heightStemAlphaField);
        RegisterFloat(rangeFadeMetersField);
        RegisterFloat(depthSizeStrengthField);
        RegisterFloat(atomPollSecondsField);
        RegisterFloat(availableAtomAlphaField);
        RegisterFloat(markerClickRadiusPixelsField);
        RegisterFloat(grabHitRadiusMetersField);
        RegisterFloat(pollIntervalField);
        RegisterFloat(responseSmoothingField);
        RegisterFloat(anchorRotationXField);
        RegisterFloat(anchorRotationYField);
        RegisterFloat(anchorRotationZField);
        RegisterFloat(staticWorldXField);
        RegisterFloat(staticWorldYField);
        RegisterFloat(staticWorldZField);
        RegisterFloat(staticWorldPitchField);
        RegisterFloat(staticWorldYawField);
        RegisterFloat(staticWorldRollField);

        RegisterString(statusField);
        RegisterString(anchorAtomUidField);

        RegisterAction(new JSONStorableAction("Capture HUD Offset From Atom", CaptureHudOffsetFromAttachedAtom));
        RegisterAction(new JSONStorableAction("Reset HUD Offset", ResetHudOffset));
        RegisterAction(new JSONStorableAction("Save Global Prefs", SaveGlobalPreferencesAction));
        RegisterAction(new JSONStorableAction("Load Global Prefs", LoadGlobalPreferencesAction));
        RegisterAction(new JSONStorableAction("Reset Global Prefs", ResetGlobalPreferencesAction));
        RegisterAction(new JSONStorableAction("Use Selected As Anchor", UseSelectedAsAnchor));
        RegisterAction(new JSONStorableAction("Use Containing Atom Anchor", UseContainingAtomAnchor));
        RegisterAction(new JSONStorableAction("Capture Static From Current View", CaptureStaticFromCurrentView));
        ConfigureGlobalPreferenceCallbacks();
    }

    private void BuildUi()
    {
        CreateToggle(radarEnabledField, false);
        CreateToggle(globalPrefsAutoSaveField, true);
        CreatePopup(anchorModeField, false);
        CreateTextField(anchorAtomUidField, true);
        CreateToggle(desktopTopDownField, true);
        CreateToggle(anchorToViewField, false);
        CreateToggle(selectedGroundDropEnabledField, false);
        CreateToggle(heightStemsEnabledField, true);
        CreateToggle(depthSizeCueField, false);
        CreateToggle(worldAxisAlignField, true);
        CreateToggle(groundAxisLockField, false);
        CreateToggle(availableAtomMarkersEnabledField, true);
#if FA_RADAR_PRO
        CreateToggle(showLightAtomsField, false);
        CreateToggle(showCustomUnityAssetAtomsField, true);
        CreateToggle(showPersonAtomsField, false);
        CreateToggle(showOtherAtomsField, true);
#endif
        CreateToggle(clickSelectMarkersField, false);
        CreateToggle(grabHandlesEnabledField, true);
        CreateToggle(grabHandleDebugVisibleField, false);
        CreateToggle(grabHapticsEnabledField, true);
        CreateToggle(ringsEnabledField, false);
        CreateToggle(gridEnabledField, true);
        CreateToggle(gridFollowsUserField, false);
        CreateToggle(gridClipCircleField, true);
        CreateToggle(ignoreContainingAtomField, false);
        CreateTextField(statusField, true);
        CreateButton("Load Global Prefs", false).button.onClick.AddListener(delegate
        {
            LoadGlobalPreferencesAction();
        });
        CreateButton("Save Global Prefs", true).button.onClick.AddListener(delegate
        {
            SaveGlobalPreferencesAction();
        });
        CreateButton("Reset Global Prefs", false).button.onClick.AddListener(delegate
        {
            ResetGlobalPreferencesAction();
        });
        CreateToggle(cuaAnchorPresetField, true);
        CreateButton("Use Selected As Anchor", true).button.onClick.AddListener(delegate
        {
            UseSelectedAsAnchor();
        });
        CreateButton("Use Containing Atom Anchor", false).button.onClick.AddListener(delegate
        {
            UseContainingAtomAnchor();
        });
        CreateButton("Capture Static From Current View", true).button.onClick.AddListener(delegate
        {
            CaptureStaticFromCurrentView();
        });

        CreateSlider(radarRangeMetersField, false);
        CreateSlider(floorAreaScaleField, true);
        CreateSlider(gridStepMetersField, false);
        CreateSlider(radarVisualRadiusField, false);
        CreateSlider(hudScaleField, true);

        CreateSlider(hudOffsetXField, false);
        CreateSlider(hudOffsetYField, true);
        CreateSlider(hudOffsetZField, false);
        CreateSlider(desktopTiltDegreesField, false);
        CreateSlider(responseSmoothingField, true);
        CreateSlider(anchorRotationXField, false);
        CreateSlider(anchorRotationYField, true);
        CreateSlider(anchorRotationZField, false);
        CreateSlider(staticWorldXField, false);
        CreateSlider(staticWorldYField, true);
        CreateSlider(staticWorldZField, false);
        CreateSlider(staticWorldPitchField, false);
        CreateSlider(staticWorldYawField, true);
        CreateSlider(staticWorldRollField, false);

        CreateToggle(placementModeField, false);
        CreateButton("Capture HUD Offset From Atom", false).button.onClick.AddListener(delegate
        {
            CaptureHudOffsetFromAttachedAtom();
        });
        CreateButton("Reset HUD Offset", false).button.onClick.AddListener(delegate
        {
            ResetHudOffset();
        });

        CreateSlider(ringRotationSpeedField, false);
        CreateSlider(targetMarkerScaleField, true);
        CreateSlider(heightScaleMetersField, false);
        CreateSlider(heightStemAlphaField, true);
        CreateSlider(rangeFadeMetersField, false);
        CreateSlider(depthSizeStrengthField, true);
        CreateSlider(availableAtomAlphaField, false);
        CreateSlider(markerClickRadiusPixelsField, true);
        CreateSlider(grabHitRadiusMetersField, false);
        CreateSlider(shellAlphaField, false);
        CreateSlider(ringAlphaField, true);
        CreateSlider(gridAlphaField, false);
        CreateSlider(markerAlphaField, true);
        CreateSlider(emissionStrengthField, false);
        CreateSlider(pollIntervalField, true);
        CreateSlider(atomPollSecondsField, true);
    }

    private void ConfigureGlobalPreferenceCallbacks()
    {
        ConfigureGlobalPreferenceField(globalPrefsAutoSaveField);
        if (globalPrefsAutoSaveField != null)
        {
            globalPrefsAutoSaveField.setCallbackFunction = delegate(bool value)
            {
                if (!globalPreferencesLoading)
                {
                    WriteGlobalPreferences();
                }
            };
        }

        ConfigureGlobalPreferenceCallback(radarEnabledField);
        ConfigureGlobalPreferenceCallback(ignoreContainingAtomField);
        ConfigureGlobalPreferenceCallback(ringsEnabledField);
        ConfigureGlobalPreferenceCallback(gridEnabledField);
        ConfigureGlobalPreferenceCallback(gridFollowsUserField);
        ConfigureGlobalPreferenceCallback(gridClipCircleField);
        ConfigureGlobalPreferenceCallback(anchorToViewField);
        ConfigureGlobalPreferenceCallback(desktopTopDownField);
        ConfigureGlobalPreferenceCallback(worldAxisAlignField);
        ConfigureGlobalPreferenceCallback(groundAxisLockField);
        ConfigureGlobalPreferenceCallback(selectedGroundDropEnabledField);
        ConfigureGlobalPreferenceCallback(heightStemsEnabledField);
        ConfigureGlobalPreferenceCallback(depthSizeCueField);
        ConfigureGlobalPreferenceCallback(availableAtomMarkersEnabledField);
        ConfigureGlobalPreferenceCallback(clickSelectMarkersField);
        ConfigureGlobalPreferenceCallback(grabHandlesEnabledField);
        ConfigureGlobalPreferenceCallback(grabHandleDebugVisibleField);
        ConfigureGlobalPreferenceCallback(grabHapticsEnabledField);

        ConfigureGlobalPreferenceCallback(hudOffsetXField);
        ConfigureGlobalPreferenceCallback(hudOffsetYField);
        ConfigureGlobalPreferenceCallback(hudOffsetZField);
        ConfigureGlobalPreferenceCallback(hudScaleField);
        ConfigureGlobalPreferenceCallback(desktopTiltDegreesField);
        ConfigureGlobalPreferenceCallback(radarRangeMetersField);
        ConfigureGlobalPreferenceCallback(floorAreaScaleField);
        ConfigureGlobalPreferenceCallback(radarVisualRadiusField);
        ConfigureGlobalPreferenceCallback(gridStepMetersField);
        ConfigureGlobalPreferenceCallback(shellAlphaField);
        ConfigureGlobalPreferenceCallback(ringAlphaField);
        ConfigureGlobalPreferenceCallback(gridAlphaField);
        ConfigureGlobalPreferenceCallback(markerAlphaField);
        ConfigureGlobalPreferenceCallback(emissionStrengthField);
        ConfigureGlobalPreferenceCallback(ringRotationSpeedField);
        ConfigureGlobalPreferenceCallback(targetMarkerScaleField);
        ConfigureGlobalPreferenceCallback(heightScaleMetersField);
        ConfigureGlobalPreferenceCallback(heightStemAlphaField);
        ConfigureGlobalPreferenceCallback(rangeFadeMetersField);
        ConfigureGlobalPreferenceCallback(depthSizeStrengthField);
        ConfigureGlobalPreferenceCallback(atomPollSecondsField);
        ConfigureGlobalPreferenceCallback(availableAtomAlphaField);
        ConfigureGlobalPreferenceCallback(markerClickRadiusPixelsField);
        ConfigureGlobalPreferenceCallback(grabHitRadiusMetersField);
        ConfigureGlobalPreferenceCallback(pollIntervalField);
        ConfigureGlobalPreferenceCallback(responseSmoothingField);
        ConfigureGlobalPreferenceCallback(anchorRotationXField);
        ConfigureGlobalPreferenceCallback(anchorRotationYField);
        ConfigureGlobalPreferenceCallback(anchorRotationZField);
        ConfigureGlobalPreferenceCallback(staticWorldXField);
        ConfigureGlobalPreferenceCallback(staticWorldYField);
        ConfigureGlobalPreferenceCallback(staticWorldZField);
        ConfigureGlobalPreferenceCallback(staticWorldPitchField);
        ConfigureGlobalPreferenceCallback(staticWorldYawField);
        ConfigureGlobalPreferenceCallback(staticWorldRollField);
        ConfigureGlobalPreferenceCallback(anchorModeField);
        ConfigureGlobalPreferenceCallback(anchorAtomUidField);

        ConfigureGlobalPreferenceCallback(showLightAtomsField);
        ConfigureGlobalPreferenceCallback(showCustomUnityAssetAtomsField);
        ConfigureGlobalPreferenceCallback(showPersonAtomsField);
        ConfigureGlobalPreferenceCallback(showOtherAtomsField);
    }

    private static void ConfigureGlobalPreferenceField(JSONStorableParam field)
    {
        if (field == null)
        {
            return;
        }

        field.isStorable = false;
        field.isRestorable = false;
    }

    private void ConfigureGlobalPreferenceCallback(JSONStorableBool field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(bool value)
        {
            MarkGlobalPreferencesDirty();
        };
    }

    private void ConfigureGlobalPreferenceCallback(JSONStorableFloat field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(float value)
        {
            MarkGlobalPreferencesDirty();
        };
    }

    private void ConfigureGlobalPreferenceCallback(JSONStorableString field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(string value)
        {
            MarkGlobalPreferencesDirty();
        };
    }

    private void ConfigureGlobalPreferenceCallback(JSONStorableStringChooser field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(string value)
        {
            MarkGlobalPreferencesDirty();
        };
    }

    private void SaveGlobalPreferencesAction()
    {
        WriteGlobalPreferences();
    }

    private void LoadGlobalPreferencesAction()
    {
        LoadGlobalPreferences();
    }

    private void ResetGlobalPreferencesAction()
    {
        bool previousLoading = globalPreferencesLoading;
        globalPreferencesLoading = true;
        try
        {
            ApplyBuiltInGlobalPreferenceDefaults();
        }
        finally
        {
            globalPreferencesLoading = previousLoading;
        }

        haveSmoothedHudPosition = false;
        InvalidateGridMesh();
        WriteGlobalPreferences();
        SetStatus("Global prefs reset.");
    }

    private void MarkGlobalPreferencesDirty()
    {
        if (globalPreferencesLoading)
        {
            return;
        }

        if (globalPrefsAutoSaveField != null && !globalPrefsAutoSaveField.val)
        {
            return;
        }

        globalPreferencesDirty = true;
        nextGlobalPreferencesFlushTime = Time.unscaledTime + GlobalPreferencesFlushDelaySeconds;
    }

    private void FlushGlobalPreferencesIfDue(bool force)
    {
        if (!globalPreferencesDirty)
        {
            return;
        }

        if (globalPrefsAutoSaveField != null && !globalPrefsAutoSaveField.val)
        {
            return;
        }

        if (!force && Time.unscaledTime < nextGlobalPreferencesFlushTime)
        {
            return;
        }

        WriteGlobalPreferences();
    }

    private void LoadGlobalPreferences()
    {
        bool loadedAny = false;
        string preferencesJson;
        if (TryReadSharedGlobalPreferencesCache(false, out preferencesJson))
        {
            loadedAny = ApplyCommonGlobalPreferences(preferencesJson) || loadedAny;
        }

#if FA_RADAR_PRO
        if (TryReadSharedGlobalPreferencesCache(true, out preferencesJson))
        {
            loadedAny = ApplyProGlobalPreferences(preferencesJson) || loadedAny;
        }
#endif

        globalPreferencesDirty = false;
        if (loadedAny)
        {
            SetStatus("Global prefs loaded.");
        }
        else
        {
            SetStatus("No global prefs found; defaults active.");
        }
    }

    private void PollSharedGlobalPreferences()
    {
        if (globalPreferencesDirty || Time.unscaledTime < nextGlobalPreferencesPollTime)
        {
            return;
        }

        nextGlobalPreferencesPollTime = Time.unscaledTime + GlobalPreferencesSharedStatePollIntervalSeconds;

        bool applied = false;
        string preferencesJson;
        if (TryReadSharedGlobalPreferencesCache(false, out preferencesJson)
            && !string.Equals(preferencesJson ?? "", lastAppliedCommonPreferencesJson ?? "", StringComparison.Ordinal))
        {
            applied = ApplyCommonGlobalPreferences(preferencesJson) || applied;
        }

#if FA_RADAR_PRO
        if (TryReadSharedGlobalPreferencesCache(true, out preferencesJson)
            && !string.Equals(preferencesJson ?? "", lastAppliedProPreferencesJson ?? "", StringComparison.Ordinal))
        {
            applied = ApplyProGlobalPreferences(preferencesJson) || applied;
        }
#endif

        if (applied)
        {
            SetStatus("Global prefs refreshed.");
        }
    }

    private void WriteGlobalPreferences()
    {
        if (globalPreferencesLoading)
        {
            return;
        }

        string errorMessage;
        string commonJson = BuildCommonGlobalPreferencesJson();
        if (!TryWriteGlobalPreferencesToDisk(ResolveCommonPreferencesPath(), commonJson, out errorMessage))
        {
            globalPreferencesDirty = true;
            SetStatus("Global prefs save failed: " + errorMessage);
            return;
        }

        UpdateSharedGlobalPreferencesCache(false, commonJson);
        lastAppliedCommonPreferencesJson = commonJson;

#if FA_RADAR_PRO
        string proJson = BuildProGlobalPreferencesJson();
        if (!TryWriteGlobalPreferencesToDisk(ResolveProPreferencesPath(), proJson, out errorMessage))
        {
            globalPreferencesDirty = true;
            SetStatus("Pro prefs save failed: " + errorMessage);
            return;
        }

        UpdateSharedGlobalPreferencesCache(true, proJson);
        lastAppliedProPreferencesJson = proJson;
#endif

        globalPreferencesDirty = false;
        SetStatus("Global prefs saved.");
    }

    private bool TryWriteGlobalPreferencesToDisk(string preferencesPath, string preferencesJson, out string errorMessage)
    {
        errorMessage = "";
        try
        {
            if (!FileManagerSecure.DirectoryExists(FrameAngelRadarPreferencesRootPath, false))
            {
                FileManagerSecure.CreateDirectory(FrameAngelRadarPreferencesRootPath);
            }

            FileManagerSecure.WriteAllText(preferencesPath, preferencesJson);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private bool TryReadGlobalPreferencesFromDisk(
        string preferencesPath,
        string expectedSchemaVersion,
        out string preferencesJson)
    {
        preferencesJson = "";
        if (string.IsNullOrEmpty(preferencesPath) || !FileManagerSecure.FileExists(preferencesPath, false))
        {
            return false;
        }

        try
        {
            preferencesJson = FileManagerSecure.ReadAllText(preferencesPath);
        }
        catch
        {
            preferencesJson = "";
            return false;
        }

        string schemaVersion;
        if (TryReadStringPreference(preferencesJson, "schemaVersion", out schemaVersion)
            && !string.IsNullOrEmpty(schemaVersion)
            && !string.Equals(schemaVersion, expectedSchemaVersion, StringComparison.OrdinalIgnoreCase))
        {
            preferencesJson = "";
            return false;
        }

        return true;
    }

    private bool TryReadSharedGlobalPreferencesCache(bool proPreferences, out string preferencesJson)
    {
        preferencesJson = "";
        if (proPreferences)
        {
            if (IsCuaPreferenceProfileActive())
            {
                preferencesJson = sharedRadarCuaProPreferencesJson ?? "";
                if (sharedRadarCuaProPreferencesCacheKnown && Time.unscaledTime < sharedRadarCuaProPreferencesNextReadAt)
                {
                    return true;
                }
            }
            else
            {
                preferencesJson = sharedRadarProPreferencesJson ?? "";
                if (sharedRadarProPreferencesCacheKnown && Time.unscaledTime < sharedRadarProPreferencesNextReadAt)
                {
                    return true;
                }
            }

            if (!TryReadGlobalPreferencesFromDisk(
                ResolveProPreferencesPath(),
                ResolveProPreferencesSchemaVersion(),
                out preferencesJson))
            {
                return IsCuaPreferenceProfileActive()
                    ? sharedRadarCuaProPreferencesCacheKnown
                    : sharedRadarProPreferencesCacheKnown;
            }

            UpdateSharedGlobalPreferencesCache(true, preferencesJson);
            return true;
        }

        if (IsCuaPreferenceProfileActive())
        {
            preferencesJson = sharedRadarCuaCommonPreferencesJson ?? "";
            if (sharedRadarCuaCommonPreferencesCacheKnown && Time.unscaledTime < sharedRadarCuaCommonPreferencesNextReadAt)
            {
                return true;
            }
        }
        else
        {
            preferencesJson = sharedRadarCommonPreferencesJson ?? "";
            if (sharedRadarCommonPreferencesCacheKnown && Time.unscaledTime < sharedRadarCommonPreferencesNextReadAt)
            {
                return true;
            }
        }

        if (!TryReadGlobalPreferencesFromDisk(
            ResolveCommonPreferencesPath(),
            ResolveCommonPreferencesSchemaVersion(),
            out preferencesJson))
        {
            return IsCuaPreferenceProfileActive()
                ? sharedRadarCuaCommonPreferencesCacheKnown
                : sharedRadarCommonPreferencesCacheKnown;
        }

        UpdateSharedGlobalPreferencesCache(false, preferencesJson);
        return true;
    }

    private void UpdateSharedGlobalPreferencesCache(bool proPreferences, string preferencesJson)
    {
        if (proPreferences)
        {
            if (IsCuaPreferenceProfileActive())
            {
                sharedRadarCuaProPreferencesJson = preferencesJson ?? "";
                sharedRadarCuaProPreferencesCacheKnown = true;
                sharedRadarCuaProPreferencesNextReadAt = Time.unscaledTime + GlobalPreferencesSharedStatePollIntervalSeconds;
                return;
            }

            sharedRadarProPreferencesJson = preferencesJson ?? "";
            sharedRadarProPreferencesCacheKnown = true;
            sharedRadarProPreferencesNextReadAt = Time.unscaledTime + GlobalPreferencesSharedStatePollIntervalSeconds;
            return;
        }

        if (IsCuaPreferenceProfileActive())
        {
            sharedRadarCuaCommonPreferencesJson = preferencesJson ?? "";
            sharedRadarCuaCommonPreferencesCacheKnown = true;
            sharedRadarCuaCommonPreferencesNextReadAt = Time.unscaledTime + GlobalPreferencesSharedStatePollIntervalSeconds;
            return;
        }

        sharedRadarCommonPreferencesJson = preferencesJson ?? "";
        sharedRadarCommonPreferencesCacheKnown = true;
        sharedRadarCommonPreferencesNextReadAt = Time.unscaledTime + GlobalPreferencesSharedStatePollIntervalSeconds;
    }

    private bool IsCuaPreferenceProfileActive()
    {
        return cuaAnchorPresetField != null && cuaAnchorPresetField.val;
    }

    private string ResolveCommonPreferencesPath()
    {
        return IsCuaPreferenceProfileActive()
            ? FrameAngelRadarCuaCommonPreferencesPath
            : FrameAngelRadarCommonPreferencesPath;
    }

    private string ResolveProPreferencesPath()
    {
        return IsCuaPreferenceProfileActive()
            ? FrameAngelRadarCuaProPreferencesPath
            : FrameAngelRadarProPreferencesPath;
    }

    private string ResolveCommonPreferencesSchemaVersion()
    {
        return IsCuaPreferenceProfileActive()
            ? FrameAngelRadarCuaCommonPreferencesSchemaVersion
            : FrameAngelRadarCommonPreferencesSchemaVersion;
    }

    private string ResolveProPreferencesSchemaVersion()
    {
        return IsCuaPreferenceProfileActive()
            ? FrameAngelRadarCuaProPreferencesSchemaVersion
            : FrameAngelRadarProPreferencesSchemaVersion;
    }

    private bool ApplyCommonGlobalPreferences(string preferencesJson)
    {
        if (string.IsNullOrEmpty(preferencesJson))
        {
            return false;
        }

        string schemaVersion;
        if (TryReadStringPreference(preferencesJson, "schemaVersion", out schemaVersion)
            && !string.IsNullOrEmpty(schemaVersion)
            && !string.Equals(schemaVersion, ResolveCommonPreferencesSchemaVersion(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bool previousLoading = globalPreferencesLoading;
        globalPreferencesLoading = true;
        try
        {
            ApplyBoolPreference(preferencesJson, "globalPrefsAutoSave", globalPrefsAutoSaveField);
            ApplyBoolPreference(preferencesJson, "radarEnabled", radarEnabledField);
            ApplyBoolPreference(preferencesJson, "ignoreAttachedAtom", ignoreContainingAtomField);
            ApplyBoolPreference(preferencesJson, "ringsEnabled", ringsEnabledField);
            ApplyBoolPreference(preferencesJson, "gridEnabled", gridEnabledField);
            ApplyBoolPreference(preferencesJson, "gridFollowsUser", gridFollowsUserField);
            ApplyBoolPreference(preferencesJson, "gridClipCircle", gridClipCircleField);
            ApplyBoolPreference(preferencesJson, "anchorToView", anchorToViewField);
            ApplyBoolPreference(preferencesJson, "flattenTargetY", desktopTopDownField);
            ApplyBoolPreference(preferencesJson, "worldAxisAlign", worldAxisAlignField);
            ApplyBoolPreference(preferencesJson, "groundAxisLock", groundAxisLockField);
            ApplyBoolPreference(preferencesJson, "selectedGroundDrop", selectedGroundDropEnabledField);
            ApplyBoolPreference(preferencesJson, "heightStems", heightStemsEnabledField);
            ApplyBoolPreference(preferencesJson, "depthSizeCue", depthSizeCueField);
            ApplyBoolPreference(preferencesJson, "availableAtomMarkers", availableAtomMarkersEnabledField);
            ApplyBoolPreference(preferencesJson, "clickSelectMarkers", clickSelectMarkersField);
            ApplyBoolPreference(preferencesJson, "grabHandlesEnabled", grabHandlesEnabledField);
            ApplyBoolPreference(preferencesJson, "grabHandleDebugVisible", grabHandleDebugVisibleField);
            ApplyBoolPreference(preferencesJson, "grabHaptics", grabHapticsEnabledField);
            ApplyStringPreference(preferencesJson, "anchorMode", anchorModeField);
            ApplyStringPreference(preferencesJson, "anchorAtomUid", anchorAtomUidField);

            ApplyFloatPreference(preferencesJson, "hudOffsetX", hudOffsetXField);
            ApplyFloatPreference(preferencesJson, "hudOffsetY", hudOffsetYField);
            ApplyFloatPreference(preferencesJson, "hudOffsetZ", hudOffsetZField);
            ApplyFloatPreference(preferencesJson, "hudScale", hudScaleField);
            ApplyFloatPreference(preferencesJson, "desktopTiltDegrees", desktopTiltDegreesField);
            ApplyFloatPreference(preferencesJson, "radarRangeMeters", radarRangeMetersField);
            ApplyFloatPreference(preferencesJson, "floorAreaScale", floorAreaScaleField);
            ApplyFloatPreference(preferencesJson, "radarVisualRadius", radarVisualRadiusField);
            ApplyFloatPreference(preferencesJson, "gridStepMeters", gridStepMetersField);
            ApplyFloatPreference(preferencesJson, "shellAlpha", shellAlphaField);
            ApplyFloatPreference(preferencesJson, "ringAlpha", ringAlphaField);
            ApplyFloatPreference(preferencesJson, "gridAlpha", gridAlphaField);
            ApplyFloatPreference(preferencesJson, "markerAlpha", markerAlphaField);
            ApplyFloatPreference(preferencesJson, "emissionStrength", emissionStrengthField);
            ApplyFloatPreference(preferencesJson, "ringRotationSpeed", ringRotationSpeedField);
            ApplyFloatPreference(preferencesJson, "targetMarkerScale", targetMarkerScaleField);
            ApplyFloatPreference(preferencesJson, "heightScaleMeters", heightScaleMetersField);
            ApplyFloatPreference(preferencesJson, "heightStemAlpha", heightStemAlphaField);
            ApplyFloatPreference(preferencesJson, "rangeFadeMeters", rangeFadeMetersField);
            ApplyFloatPreference(preferencesJson, "depthSizeStrength", depthSizeStrengthField);
            ApplyFloatPreference(preferencesJson, "atomPollSeconds", atomPollSecondsField);
            ApplyFloatPreference(preferencesJson, "availableAtomAlpha", availableAtomAlphaField);
            ApplyFloatPreference(preferencesJson, "markerClickRadiusPixels", markerClickRadiusPixelsField);
            ApplyFloatPreference(preferencesJson, "grabHitRadiusMeters", grabHitRadiusMetersField);
            ApplyFloatPreference(preferencesJson, "selectionPollSeconds", pollIntervalField);
            ApplyFloatPreference(preferencesJson, "responseSmoothing", responseSmoothingField);
            ApplyFloatPreference(preferencesJson, "anchorRotationX", anchorRotationXField);
            ApplyFloatPreference(preferencesJson, "anchorRotationY", anchorRotationYField);
            ApplyFloatPreference(preferencesJson, "anchorRotationZ", anchorRotationZField);
            ApplyFloatPreference(preferencesJson, "staticWorldX", staticWorldXField);
            ApplyFloatPreference(preferencesJson, "staticWorldY", staticWorldYField);
            ApplyFloatPreference(preferencesJson, "staticWorldZ", staticWorldZField);
            ApplyFloatPreference(preferencesJson, "staticWorldPitch", staticWorldPitchField);
            ApplyFloatPreference(preferencesJson, "staticWorldYaw", staticWorldYawField);
            ApplyFloatPreference(preferencesJson, "staticWorldRoll", staticWorldRollField);
        }
        finally
        {
            globalPreferencesLoading = previousLoading;
        }

        lastAppliedCommonPreferencesJson = preferencesJson;
        haveSmoothedHudPosition = false;
        InvalidateGridMesh();
        return true;
    }

    private bool ApplyProGlobalPreferences(string preferencesJson)
    {
        if (string.IsNullOrEmpty(preferencesJson))
        {
            return false;
        }

        string schemaVersion;
        if (TryReadStringPreference(preferencesJson, "schemaVersion", out schemaVersion)
            && !string.IsNullOrEmpty(schemaVersion)
            && !string.Equals(schemaVersion, ResolveProPreferencesSchemaVersion(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bool previousLoading = globalPreferencesLoading;
        globalPreferencesLoading = true;
        try
        {
            ApplyBoolPreference(preferencesJson, "showLights", showLightAtomsField);
            ApplyBoolPreference(preferencesJson, "showCUA", showCustomUnityAssetAtomsField);
            ApplyBoolPreference(preferencesJson, "showPeople", showPersonAtomsField);
            ApplyBoolPreference(preferencesJson, "showOtherAtoms", showOtherAtomsField);
        }
        finally
        {
            globalPreferencesLoading = previousLoading;
        }

        lastAppliedProPreferencesJson = preferencesJson;
        trackedAvailableAtoms.Clear();
        nextAtomPollTime = 0.0f;
        return true;
    }

    private void ApplyBuiltInGlobalPreferenceDefaults()
    {
        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetBoolNoCallback(radarEnabledField, true);
        SetBoolNoCallback(ignoreContainingAtomField, true);
        SetBoolNoCallback(ringsEnabledField, true);
        SetBoolNoCallback(gridEnabledField, true);
        SetBoolNoCallback(gridFollowsUserField, true);
        SetBoolNoCallback(gridClipCircleField, true);
        SetBoolNoCallback(anchorToViewField, true);
        SetBoolNoCallback(desktopTopDownField, false);
        SetBoolNoCallback(worldAxisAlignField, true);
        SetBoolNoCallback(groundAxisLockField, true);
        SetBoolNoCallback(selectedGroundDropEnabledField, false);
        SetBoolNoCallback(heightStemsEnabledField, true);
        SetBoolNoCallback(depthSizeCueField, true);
        SetBoolNoCallback(availableAtomMarkersEnabledField, true);
        SetBoolNoCallback(clickSelectMarkersField, true);
        SetBoolNoCallback(grabHandlesEnabledField, false);
        SetBoolNoCallback(grabHandleDebugVisibleField, false);
        SetBoolNoCallback(grabHapticsEnabledField, true);
        SetBoolNoCallback(showLightAtomsField, true);
        SetBoolNoCallback(showCustomUnityAssetAtomsField, false);
        SetBoolNoCallback(showPersonAtomsField, false);
        SetBoolNoCallback(showOtherAtomsField, false);
        SetStringNoCallback(anchorModeField, AnchorModeHud);
        SetStringNoCallback(anchorAtomUidField, "");

        SetFloatNoCallback(hudOffsetXField, -0.59f);
        SetFloatNoCallback(hudOffsetYField, 0.22f);
        SetFloatNoCallback(hudOffsetZField, 0.78f);
        SetFloatNoCallback(hudScaleField, 0.49f);
        SetFloatNoCallback(desktopTiltDegreesField, 90.0f);
        SetFloatNoCallback(radarRangeMetersField, 5.0f);
        SetFloatNoCallback(floorAreaScaleField, 1.0f);
        SetFloatNoCallback(radarVisualRadiusField, 0.08f);
        SetFloatNoCallback(gridStepMetersField, 1.0f);
        SetFloatNoCallback(shellAlphaField, 0.09f);
        SetFloatNoCallback(ringAlphaField, 0.34f);
        SetFloatNoCallback(gridAlphaField, 0.16f);
        SetFloatNoCallback(markerAlphaField, 0.9f);
        SetFloatNoCallback(emissionStrengthField, 1.4f);
        SetFloatNoCallback(ringRotationSpeedField, 0.0f);
        SetFloatNoCallback(targetMarkerScaleField, 0.09f);
        SetFloatNoCallback(heightScaleMetersField, 6.0f);
        SetFloatNoCallback(heightStemAlphaField, 0.42f);
        SetFloatNoCallback(rangeFadeMetersField, 1.25f);
        SetFloatNoCallback(depthSizeStrengthField, 0.35f);
        SetFloatNoCallback(atomPollSecondsField, 0.75f);
        SetFloatNoCallback(availableAtomAlphaField, 0.46f);
        SetFloatNoCallback(markerClickRadiusPixelsField, 20.0f);
        SetFloatNoCallback(grabHitRadiusMetersField, 0.16f);
        SetFloatNoCallback(pollIntervalField, 0.15f);
        SetFloatNoCallback(responseSmoothingField, 0.0f);
        SetFloatNoCallback(anchorRotationXField, 0.0f);
        SetFloatNoCallback(anchorRotationYField, 0.0f);
        SetFloatNoCallback(anchorRotationZField, 0.0f);
        SetFloatNoCallback(staticWorldXField, 0.0f);
        SetFloatNoCallback(staticWorldYField, 1.5f);
        SetFloatNoCallback(staticWorldZField, 1.0f);
        SetFloatNoCallback(staticWorldPitchField, 0.0f);
        SetFloatNoCallback(staticWorldYawField, 0.0f);
        SetFloatNoCallback(staticWorldRollField, 0.0f);
    }

    private string BuildCommonGlobalPreferencesJson()
    {
        StringBuilder sb = new StringBuilder(2048);
        bool wroteProperty = false;
        sb.Append('{');
        AppendJsonStringProperty(sb, ref wroteProperty, "schemaVersion", ResolveCommonPreferencesSchemaVersion());
        AppendJsonStringProperty(sb, ref wroteProperty, "savedAtUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        AppendJsonBoolProperty(sb, ref wroteProperty, "globalPrefsAutoSave", ReadBool(globalPrefsAutoSaveField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "radarEnabled", ReadBool(radarEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "ignoreAttachedAtom", ReadBool(ignoreContainingAtomField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "ringsEnabled", ReadBool(ringsEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "gridEnabled", ReadBool(gridEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "gridFollowsUser", ReadBool(gridFollowsUserField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "gridClipCircle", ReadBool(gridClipCircleField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "anchorToView", ReadBool(anchorToViewField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "flattenTargetY", ReadBool(desktopTopDownField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "worldAxisAlign", ReadBool(worldAxisAlignField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "groundAxisLock", ReadBool(groundAxisLockField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "selectedGroundDrop", ReadBool(selectedGroundDropEnabledField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "heightStems", ReadBool(heightStemsEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "depthSizeCue", ReadBool(depthSizeCueField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "availableAtomMarkers", ReadBool(availableAtomMarkersEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "clickSelectMarkers", ReadBool(clickSelectMarkersField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHandlesEnabled", ReadBool(grabHandlesEnabledField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHandleDebugVisible", ReadBool(grabHandleDebugVisibleField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHaptics", ReadBool(grabHapticsEnabledField, true));
        AppendJsonStringProperty(sb, ref wroteProperty, "anchorMode", ResolveAnchorMode());
        AppendJsonStringProperty(sb, ref wroteProperty, "anchorAtomUid", ReadString(anchorAtomUidField, ""));

        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetX", ReadFloat(hudOffsetXField, -0.59f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetY", ReadFloat(hudOffsetYField, 0.22f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetZ", ReadFloat(hudOffsetZField, 0.78f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudScale", ReadFloat(hudScaleField, 0.49f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "desktopTiltDegrees", ReadFloat(desktopTiltDegreesField, 90.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "radarRangeMeters", ReadFloat(radarRangeMetersField, 5.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "floorAreaScale", ReadFloat(floorAreaScaleField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "radarVisualRadius", ReadFloat(radarVisualRadiusField, 0.08f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "gridStepMeters", ReadFloat(gridStepMetersField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "shellAlpha", ReadFloat(shellAlphaField, 0.09f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "ringAlpha", ReadFloat(ringAlphaField, 0.34f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "gridAlpha", ReadFloat(gridAlphaField, 0.16f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "markerAlpha", ReadFloat(markerAlphaField, 0.9f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "emissionStrength", ReadFloat(emissionStrengthField, 1.4f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "ringRotationSpeed", ReadFloat(ringRotationSpeedField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "targetMarkerScale", ReadFloat(targetMarkerScaleField, 0.09f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "heightScaleMeters", ReadFloat(heightScaleMetersField, 6.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "heightStemAlpha", ReadFloat(heightStemAlphaField, 0.42f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "rangeFadeMeters", ReadFloat(rangeFadeMetersField, 1.25f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "depthSizeStrength", ReadFloat(depthSizeStrengthField, 0.35f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "atomPollSeconds", ReadFloat(atomPollSecondsField, 0.75f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "availableAtomAlpha", ReadFloat(availableAtomAlphaField, 0.46f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "markerClickRadiusPixels", ReadFloat(markerClickRadiusPixelsField, 20.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabHitRadiusMeters", ReadFloat(grabHitRadiusMetersField, 0.16f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "selectionPollSeconds", ReadFloat(pollIntervalField, 0.15f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "responseSmoothing", ReadFloat(responseSmoothingField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "anchorRotationX", ReadFloat(anchorRotationXField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "anchorRotationY", ReadFloat(anchorRotationYField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "anchorRotationZ", ReadFloat(anchorRotationZField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldX", ReadFloat(staticWorldXField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldY", ReadFloat(staticWorldYField, 1.5f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldZ", ReadFloat(staticWorldZField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldPitch", ReadFloat(staticWorldPitchField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldYaw", ReadFloat(staticWorldYawField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "staticWorldRoll", ReadFloat(staticWorldRollField, 0.0f));
        sb.Append('}');
        return sb.ToString();
    }

    private string BuildProGlobalPreferencesJson()
    {
        StringBuilder sb = new StringBuilder(384);
        bool wroteProperty = false;
        sb.Append('{');
        AppendJsonStringProperty(sb, ref wroteProperty, "schemaVersion", ResolveProPreferencesSchemaVersion());
        AppendJsonStringProperty(sb, ref wroteProperty, "savedAtUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showLights", ReadBool(showLightAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showCUA", ReadBool(showCustomUnityAssetAtomsField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showPeople", ReadBool(showPersonAtomsField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showOtherAtoms", ReadBool(showOtherAtomsField, false));
        sb.Append('}');
        return sb.ToString();
    }

    private void ApplyBoolPreference(string preferencesJson, string key, JSONStorableBool field)
    {
        bool value;
        if (field != null && TryReadBoolPreference(preferencesJson, key, out value))
        {
            field.valNoCallback = value;
        }
    }

    private void ApplyFloatPreference(string preferencesJson, string key, JSONStorableFloat field)
    {
        float value;
        if (field != null && TryReadFloatPreference(preferencesJson, key, out value))
        {
            field.valNoCallback = value;
        }
    }

    private void ApplyStringPreference(string preferencesJson, string key, JSONStorableString field)
    {
        string value;
        if (field != null && TryReadStringPreference(preferencesJson, key, out value))
        {
            field.valNoCallback = value ?? "";
        }
    }

    private void ApplyStringPreference(string preferencesJson, string key, JSONStorableStringChooser field)
    {
        string value;
        if (field != null && TryReadStringPreference(preferencesJson, key, out value))
        {
            field.valNoCallback = NormalizeAnchorMode(value);
        }
    }

    private static void SetBoolNoCallback(JSONStorableBool field, bool value)
    {
        if (field != null)
        {
            field.valNoCallback = value;
        }
    }

    private static void SetFloatNoCallback(JSONStorableFloat field, float value)
    {
        if (field != null)
        {
            field.valNoCallback = value;
        }
    }

    private static void SetStringNoCallback(JSONStorableString field, string value)
    {
        if (field != null)
        {
            field.valNoCallback = value ?? "";
        }
    }

    private static void SetStringNoCallback(JSONStorableStringChooser field, string value)
    {
        if (field != null)
        {
            field.valNoCallback = NormalizeAnchorMode(value);
        }
    }

    private static bool ReadBool(JSONStorableBool field, bool fallback)
    {
        return field != null ? field.val : fallback;
    }

    private static float ReadFloat(JSONStorableFloat field, float fallback)
    {
        return field != null ? field.val : fallback;
    }

    private static string ReadString(JSONStorableString field, string fallback)
    {
        return field != null ? (field.val ?? "") : fallback;
    }

    private static void AppendJsonStringProperty(StringBuilder sb, ref bool wroteProperty, string key, string value)
    {
        AppendJsonPropertyPrefix(sb, ref wroteProperty, key);
        sb.Append('"').Append(EscapeJsonString(value ?? "")).Append('"');
    }

    private static void AppendJsonBoolProperty(StringBuilder sb, ref bool wroteProperty, string key, bool value)
    {
        AppendJsonPropertyPrefix(sb, ref wroteProperty, key);
        sb.Append(value ? "true" : "false");
    }

    private static void AppendJsonFloatProperty(StringBuilder sb, ref bool wroteProperty, string key, float value)
    {
        AppendJsonPropertyPrefix(sb, ref wroteProperty, key);
        sb.Append(FormatPreferenceFloat(value));
    }

    private static void AppendJsonPropertyPrefix(StringBuilder sb, ref bool wroteProperty, string key)
    {
        if (wroteProperty)
        {
            sb.Append(',');
        }
        else
        {
            wroteProperty = true;
        }

        sb.Append('"').Append(EscapeJsonString(key ?? "")).Append("\":");
    }

    private static string FormatPreferenceFloat(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static string EscapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        StringBuilder sb = new StringBuilder(value.Length + 8);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    private static bool TryReadStringPreference(string preferencesJson, string key, out string value)
    {
        return TryReadRawPreferenceValue(preferencesJson, key, out value);
    }

    private static bool ExtractJsonString(string json, string key, out string value)
    {
        return TryReadStringPreference(json, key, out value);
    }

    private static bool ExtractJsonBool(string json, string key, out bool value)
    {
        return TryReadBoolPreference(json, key, out value);
    }

    private static bool TryReadBoolPreference(string preferencesJson, string key, out bool value)
    {
        value = false;
        string rawValue;
        if (!TryReadRawPreferenceValue(preferencesJson, key, out rawValue))
        {
            return false;
        }

        rawValue = rawValue.Trim();
        if (string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(rawValue, "false", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        return false;
    }

    private static bool TryReadFloatPreference(string preferencesJson, string key, out float value)
    {
        value = 0.0f;
        string rawValue;
        if (!TryReadRawPreferenceValue(preferencesJson, key, out rawValue))
        {
            return false;
        }

        return float.TryParse(rawValue.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadRawPreferenceValue(string preferencesJson, string key, out string value)
    {
        value = "";
        if (string.IsNullOrEmpty(preferencesJson) || string.IsNullOrEmpty(key))
        {
            return false;
        }

        string needle = "\"" + key + "\"";
        int keyIndex = preferencesJson.IndexOf(needle, StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return false;
        }

        int colonIndex = preferencesJson.IndexOf(':', keyIndex + needle.Length);
        if (colonIndex < 0)
        {
            return false;
        }

        int valueStart = colonIndex + 1;
        while (valueStart < preferencesJson.Length && char.IsWhiteSpace(preferencesJson[valueStart]))
        {
            valueStart++;
        }

        if (valueStart >= preferencesJson.Length)
        {
            return false;
        }

        if (preferencesJson[valueStart] == '"')
        {
            return TryReadQuotedJsonString(preferencesJson, valueStart + 1, out value);
        }

        int valueEnd = valueStart;
        while (valueEnd < preferencesJson.Length
            && preferencesJson[valueEnd] != ','
            && preferencesJson[valueEnd] != '}')
        {
            valueEnd++;
        }

        value = preferencesJson.Substring(valueStart, valueEnd - valueStart).Trim();
        return value.Length > 0;
    }

    private static bool TryReadQuotedJsonString(string json, int startIndex, out string value)
    {
        value = "";
        StringBuilder sb = new StringBuilder();
        bool escaping = false;
        for (int i = startIndex; i < json.Length; i++)
        {
            char c = json[i];
            if (escaping)
            {
                switch (c)
                {
                    case '"':
                    case '\\':
                    case '/':
                        sb.Append(c);
                        break;
                    case 'n':
                        sb.Append('\n');
                        break;
                    case 'r':
                        sb.Append('\r');
                        break;
                    case 't':
                        sb.Append('\t');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }

                escaping = false;
                continue;
            }

            if (c == '\\')
            {
                escaping = true;
                continue;
            }

            if (c == '"')
            {
                value = sb.ToString();
                return true;
            }

            sb.Append(c);
        }

        return false;
    }

    private void InvalidateGridMesh()
    {
        lastGridRangeMeters = -1.0f;
        lastGridStepMeters = -1.0f;
        haveLastGridOffset = false;
    }

    private void EnsureRuntimeVisuals()
    {
        if (visualsReady)
        {
            return;
        }

        shellMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Sphere Material"), new Color(0.16f, 0.64f, 0.92f, 0.10f), ShellRenderQueue);
        ringMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Z Axis Ring Material"), WithAlpha(AxisZColor, 0.34f), RingRenderQueue);
        ringXMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Y Axis Ring Material"), WithAlpha(AxisYColor, 0.34f), RingRenderQueue);
        ringZMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("X Axis Ring Material"), WithAlpha(AxisXColor, 0.34f), RingRenderQueue);
        gridMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Grid Material"), new Color(0.55f, 0.95f, 1.0f, 0.16f), GridRenderQueue);
        centerMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Center Material"), new Color(0.40f, 1.0f, 0.62f, 0.9f), MarkerRenderQueue);
        userHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("User Height Stem Material"), new Color(0.40f, 1.0f, 0.62f, 0.42f), MarkerRenderQueue);
        targetMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Material"), new Color(1.0f, 0.70f, 0.18f, 0.9f), MarkerRenderQueue);
        targetHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Height Stem Material"), new Color(1.0f, 0.70f, 0.18f, 0.42f), MarkerRenderQueue);
        targetDropMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Drop Material"), new Color(1.0f, 0.70f, 0.18f, 0.35f), MarkerRenderQueue);
        lastTargetMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Last Target Material"), new Color(1.0f, 0.48f, 0.12f, 0.32f), MarkerRenderQueue);
        lastTargetDropMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Last Target Drop Material"), new Color(1.0f, 0.48f, 0.12f, 0.15f), MarkerRenderQueue);
        availableHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Available Height Stem Material"), new Color(0.78f, 0.88f, 1.0f, 0.28f), MarkerRenderQueue);

        sphereMesh = CreateSphereMesh(16, 32, 1.0f);
        flatCircleMesh = CreateDesktopDiskMesh(72, 1.0f);
        ringMesh = CreateRingMesh(72, 0.975f, 1.0f);
        centerMarkerMesh = CreateCenterMarkerMesh();
        targetBlipMesh = CreateTargetBlipMesh();
        heightStemMesh = CreateHeightStemMesh();
        gridMesh = CreateGridMesh(ResolveEffectiveRadarRangeMeters(), gridStepMetersField.val, Vector2.zero, gridClipCircleField.val);
        lastGridRangeMeters = ResolveEffectiveRadarRangeMeters();
        lastGridStepMeters = gridStepMetersField.val;
        lastGridOffsetMeters = Vector2.zero;
        lastGridClipCircle = gridClipCircleField.val;
        haveLastGridOffset = true;

        hudRoot = new GameObject(BuildFilmSubjectName("HUD"));
        radarRoot = new GameObject(BuildFilmSubjectName("Dish"));
        radarRoot.transform.SetParent(hudRoot.transform, false);
        axisRoot = new GameObject("FA Radar World Axis");
        axisRoot.transform.SetParent(radarRoot.transform, false);

        flatCircleObject = CreateMeshObject(BuildFilmSubjectName("Flat Desktop Circle"), axisRoot.transform, flatCircleMesh, shellMaterial, ShellRenderQueue, ShellSortingOrder);
        sphereObject = CreateMeshObject(BuildFilmSubjectName("Sphere"), radarRoot.transform, sphereMesh, shellMaterial, ShellRenderQueue, ShellSortingOrder);
        gridObject = CreateMeshObject(BuildFilmSubjectName("Meter Grid"), axisRoot.transform, gridMesh, gridMaterial, GridRenderQueue, GridSortingOrder);
        gridFilter = gridObject.GetComponent<MeshFilter>();

        centerMarkerObject = CreateMeshObject(BuildFilmSubjectName("User Center"), radarRoot.transform, centerMarkerMesh, centerMaterial, MarkerRenderQueue, MarkerSortingOrder);
        userHeightStemObject = CreateMeshObject(BuildFilmSubjectName("User Height Stem"), axisRoot.transform, heightStemMesh, userHeightStemMaterial, MarkerRenderQueue, MarkerSortingOrder - 5);
        targetBlipObject = CreateMeshObject(BuildFilmSubjectName("Target Blip"), axisRoot.transform, targetBlipMesh, targetMaterial, MarkerRenderQueue, MarkerSortingOrder);
        targetHeightStemObject = CreateMeshObject(BuildFilmSubjectName("Target Height Stem"), axisRoot.transform, heightStemMesh, targetHeightStemMaterial, MarkerRenderQueue, MarkerSortingOrder - 4);
        targetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Target Grid Drop"), axisRoot.transform, targetBlipMesh, targetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 1);
        lastTargetBlipObject = CreateMeshObject(BuildFilmSubjectName("Last Target Blip"), radarRoot.transform, targetBlipMesh, lastTargetMaterial, MarkerRenderQueue, MarkerSortingOrder - 2);
        lastTargetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Last Target Grid Drop"), axisRoot.transform, targetBlipMesh, lastTargetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 3);

        ringObjects = new GameObject[3];
        ringBaseRotations = new Quaternion[3];
        ringBaseRotations[0] = Quaternion.identity;
        ringBaseRotations[1] = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        ringBaseRotations[2] = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        ringObjects[0] = CreateMeshObject(BuildFilmSubjectName("Ring XY"), axisRoot.transform, ringMesh, ringMaterial, RingRenderQueue, RingSortingOrder);
        ringObjects[1] = CreateMeshObject(BuildFilmSubjectName("Ring XZ"), axisRoot.transform, ringMesh, ringXMaterial, RingRenderQueue, RingSortingOrder);
        ringObjects[2] = CreateMeshObject(BuildFilmSubjectName("Ring YZ"), axisRoot.transform, ringMesh, ringZMaterial, RingRenderQueue, RingSortingOrder);

        SetActiveIfChanged(hudRoot, false);
        SetActiveIfChanged(userHeightStemObject, false);
        SetActiveIfChanged(targetBlipObject, false);
        SetActiveIfChanged(targetHeightStemObject, false);
        SetActiveIfChanged(targetGridDropObject, false);
        SetActiveIfChanged(lastTargetBlipObject, false);
        SetActiveIfChanged(lastTargetGridDropObject, false);

        visualsReady = true;
    }

    private GameObject CreateMeshObject(string objectName, Transform parent, Mesh mesh, Material material, int renderQueue, int sortingOrder)
    {
        GameObject target = new GameObject(objectName);
        target.transform.SetParent(parent, false);
        MeshFilter filter = target.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = target.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        ApplyRendererOverlaySettings(renderer, renderQueue, sortingOrder);
        return target;
    }

    private static string BuildFilmSubjectName(string role)
    {
        return FilmSubjectIdentifier + "." + role;
    }

    private void TickRadar()
    {
        if (hudRoot == null)
        {
            visualsReady = false;
            return;
        }

        PollRecorderRadarVisibility();
        ApplyCuaAnchorPresetMode();
        if (!recorderRadarVisible)
        {
            ApplyRecorderRadarVisibility(false);
            DestroySessionGrabHandles();
            return;
        }
        ApplyRecorderRadarVisibility(true);

        Transform viewer = ResolveViewerTransform();
        if (!radarEnabledField.val || viewer == null)
        {
            SetActiveIfChanged(hudRoot, false);
            DestroySessionGrabHandles();
            if (viewer == null)
            {
                SetStatus("Waiting for VaM look camera.");
            }
            return;
        }

        SetActiveIfChanged(hudRoot, true);
        PollSelectionIfDue();
        PollAvailableAtomsIfDue(viewer);
        TrackAttachedAtomPlacement(viewer);
        RefreshGridMeshIfNeeded(viewer);
        UpdateMaterials();
        UpdateSessionGrabHandles(viewer);
        UpdateRadarDish(viewer);
        UpdateUserHeightStem(viewer);

        Transform target = ResolveAtomRootTransform(selectedAtom);
        bool hasSelection = target != null;
        bool showSelectedGroundDrop = hasSelection && selectedGroundDropEnabledField.val;
        SetActiveIfChanged(targetBlipObject, hasSelection);
        SetActiveIfChanged(targetHeightStemObject, hasSelection && heightStemsEnabledField.val);
        SetActiveIfChanged(targetGridDropObject, showSelectedGroundDrop);

        if (hasSelection)
        {
            UpdateTargetBlip(viewer, target, showSelectedGroundDrop);
        }

        SetActiveIfChanged(lastTargetBlipObject, false);
        SetActiveIfChanged(lastTargetGridDropObject, false);
        UpdateAvailableAtomMarkers(viewer);
        HandleRadarMarkerClick();
    }

    private void ApplyCuaAnchorPresetMode()
    {
        if (cuaAnchorPresetApplied || cuaAnchorPresetField == null || !cuaAnchorPresetField.val || containingAtom == null)
        {
            return;
        }

        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetStringNoCallback(anchorModeField, AnchorModeContainingAtom);
        SetStringNoCallback(anchorAtomUidField, containingAtom.uid ?? "");
        SetHudOffset(new Vector3(0.0f, 0.0f, 0.15f));
        SetFloatNoCallback(hudScaleField, 0.75f);
        SetFloatNoCallback(anchorRotationXField, 0.0f);
        SetFloatNoCallback(anchorRotationYField, 0.0f);
        SetFloatNoCallback(anchorRotationZField, 0.0f);
        LoadGlobalPreferences();
        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetStringNoCallback(anchorModeField, AnchorModeContainingAtom);
        SetStringNoCallback(anchorAtomUidField, containingAtom.uid ?? "");
        haveSmoothedHudPosition = false;
        cuaAnchorPresetApplied = true;
        SetStatus("CUA anchor preset active.");
    }

    private void PollRecorderRadarVisibility()
    {
        if (Time.unscaledTime < nextRecorderVisibilityPollTime)
        {
            return;
        }

        nextRecorderVisibilityPollTime = Time.unscaledTime + RecorderVisibilityPollIntervalSeconds;
        bool visible;
        recorderRadarVisible = !ReadRecorderRadarVisible(out visible) || visible;
    }

    private bool ReadRecorderRadarVisible(out bool visible)
    {
        visible = true;
        if (!FileManagerSecure.FileExists(FrameAngelRecorderStatePath, false))
        {
            return false;
        }

        string stateJson;
        try
        {
            stateJson = FileManagerSecure.ReadAllText(FrameAngelRecorderStatePath);
        }
        catch
        {
            return false;
        }

        string radarHudFilmSubjectIdentifier;
        if (ExtractJsonString(stateJson, "radarHudFilmSubjectIdentifier", out radarHudFilmSubjectIdentifier)
            && !string.IsNullOrEmpty(radarHudFilmSubjectIdentifier)
            && !string.Equals(radarHudFilmSubjectIdentifier, FilmSubjectIdentifier, StringComparison.Ordinal))
        {
            return false;
        }

        return ExtractJsonBool(stateJson, "radarHudVisible", out visible);
    }

    private void ApplyRecorderRadarVisibility(bool visible)
    {
        if (visible == lastAppliedRecorderRadarVisible)
        {
            return;
        }

        lastAppliedRecorderRadarVisible = visible;
        SetMaterialAlphaMultiplier(visible ? 1.0f : 0.0f);
        if (!visible)
        {
            SetRadarVisualsVisible(false);
        }
    }

    private void SetRadarVisualsVisible(bool visible)
    {
        SetActiveIfChanged(hudRoot, visible);
    }

    private void PollSelectionIfDue()
    {
        if (Time.time < nextSelectionPollTime)
        {
            return;
        }

        float interval = Mathf.Max(0.03f, pollIntervalField.val);
        nextSelectionPollTime = Time.time + interval;

        Atom nextAtom = null;
        if (SuperController.singleton != null)
        {
            nextAtom = SuperController.singleton.GetSelectedAtom();
        }

        if (ignoreContainingAtomField.val && nextAtom != null && containingAtom != null && nextAtom == containingAtom)
        {
            nextAtom = null;
        }

        string nextUid = nextAtom != null ? nextAtom.uid : "";
        if (nextUid == selectedUid)
        {
            selectedAtom = nextAtom;
            return;
        }

        if (selectedAtom != null && !string.IsNullOrEmpty(selectedUid))
        {
            lastSelectedAtom = selectedAtom;
            lastSelectedUid = selectedUid;
            lastSelectedAtTime = Time.time;
        }

        selectedAtom = nextAtom;
        selectedUid = nextUid;

        if (selectedAtom != null)
        {
            SetStatus("Selected: " + selectedUid);
        }
        else
        {
            SetStatus("No selected atom.");
        }
    }

    private void TrackAttachedAtomPlacement(Transform viewer)
    {
        if (!placementModeField.val || containingAtom == null || containingAtom.mainController == null)
        {
            return;
        }

        Vector3 worldPosition = containingAtom.mainController.transform.position;
        Transform anchor = ResolveRadarAnchorTransform(ResolveAnchorMode());
        Vector3 localOffset = anchor != null
            ? anchor.InverseTransformPoint(worldPosition)
            : viewer.InverseTransformPoint(worldPosition);
        SetHudOffset(localOffset);
    }

    private void UpdateRadarDish(Transform viewer)
    {
        float visualRadius = ResolveVisualRadius();
        float scaledMarker = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val);
        bool flatDesktop = IsFlatDesktopCircleActive();
        float ringTime = Time.time * Mathf.Max(0.0f, ringRotationSpeedField.val);

        ApplyHudAnchor(viewer);

        radarRoot.transform.localPosition = Vector3.zero;
        radarRoot.transform.localRotation = ResolveDishLocalRotation();
        radarRoot.transform.localScale = Vector3.one;
        UpdateAxisVisualRotation(viewer);

        flatCircleObject.transform.localPosition = Vector3.zero;
        flatCircleObject.transform.localRotation = Quaternion.identity;
        flatCircleObject.transform.localScale = Vector3.one * visualRadius;
        SetActiveIfChanged(flatCircleObject, flatDesktop);

        sphereObject.transform.localPosition = Vector3.zero;
        sphereObject.transform.localRotation = Quaternion.identity;
        sphereObject.transform.localScale = Vector3.one * visualRadius;
        SetActiveIfChanged(sphereObject, !flatDesktop);

        centerMarkerObject.transform.localPosition = Vector3.zero;
        centerMarkerObject.transform.localRotation = Quaternion.AngleAxis(ringTime * 0.5f, Vector3.forward);
        centerMarkerObject.transform.localScale = Vector3.one * (scaledMarker * 0.72f);

        gridObject.transform.localPosition = new Vector3(0.0f, ResolveHeightRadarY(-viewer.position.y) * visualRadius, 0.0f);
        gridObject.transform.localRotation = Quaternion.identity;
        gridObject.transform.localScale = Vector3.one * visualRadius;
        SetActiveIfChanged(gridObject, gridEnabledField.val);

        for (int i = 0; i < ringObjects.Length; i++)
        {
            GameObject ring = ringObjects[i];
            if (ring == null)
            {
                continue;
            }

            float direction = i == 1 ? -1.0f : 1.0f;
            Quaternion spin = Quaternion.AngleAxis(ringTime * direction * (1.0f + (i * 0.17f)), Vector3.forward);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = ringBaseRotations[i] * spin;
            ring.transform.localScale = Vector3.one * (visualRadius * 1.015f);
            bool showRing = ringsEnabledField.val;
            SetActiveIfChanged(ring, showRing);
        }
    }

    private bool IsFlatDesktopCircleActive()
    {
        // Unified Sphere Treatment keeps desktop and VR on the same generated sphere/grid treatment.
        return false;
    }

    private void UpdateAxisVisualRotation(Transform viewer)
    {
        if (axisRoot == null)
        {
            return;
        }

        axisRoot.transform.localPosition = Vector3.zero;
        axisRoot.transform.localRotation = ResolveAxisLocalRotation(viewer);
        axisRoot.transform.localScale = Vector3.one;
    }

    private Quaternion ResolveAxisLocalRotation(Transform viewer)
    {
        if (!worldAxisAlignField.val)
        {
            return Quaternion.identity;
        }

        if (groundAxisLockField.val && radarRoot != null)
        {
            return Quaternion.Inverse(radarRoot.transform.rotation) * ResolveGroundAxisWorldRotation();
        }

        float yaw = ResolveWorldAxisYawDegrees(viewer);
        return Quaternion.AngleAxis(yaw, Vector3.up);
    }

    private Quaternion ResolveGroundAxisWorldRotation()
    {
        return Quaternion.identity;
    }

    private float ResolveWorldAxisYawDegrees(Transform viewer)
    {
        if (viewer == null)
        {
            return 0.0f;
        }

        Vector3 localWorldRight = viewer.InverseTransformDirection(Vector3.right);
        localWorldRight.y = 0.0f;
        if (localWorldRight.sqrMagnitude < 0.0001f)
        {
            return 0.0f;
        }

        localWorldRight.Normalize();
        return -Mathf.Atan2(localWorldRight.z, localWorldRight.x) * Mathf.Rad2Deg;
    }

    private void ApplyHudAnchor(Transform viewer)
    {
        if (hudRoot == null || viewer == null)
        {
            return;
        }

        string anchorMode = ResolveAnchorMode();
        if (string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal))
        {
            ApplyWorldStaticAnchor();
            return;
        }

        if (!string.Equals(anchorMode, AnchorModeHud, StringComparison.Ordinal))
        {
            Transform anchor = ResolveRadarAnchorTransform(anchorMode);
            if (anchor != null)
            {
                ApplyTransformAnchor(anchor);
                return;
            }
        }

        ApplyViewAnchor(viewer);
    }

    private void ApplyViewAnchor(Transform viewer)
    {
        if (hudRoot == null || viewer == null)
        {
            return;
        }

        if (anchorToViewField.val)
        {
            if (currentHudAnchor != viewer || hudRoot.transform.parent != viewer)
            {
                hudRoot.transform.SetParent(viewer, false);
                currentHudAnchor = viewer;
                haveSmoothedHudPosition = false;
            }

            hudRoot.transform.localPosition = GetHudOffset();
            hudRoot.transform.localRotation = Quaternion.identity;
            hudRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, hudScaleField.val);
            return;
        }

        if (hudRoot.transform.parent != null)
        {
            hudRoot.transform.SetParent(null, true);
            currentHudAnchor = null;
            haveSmoothedHudPosition = false;
        }

        Vector3 desiredWorldPosition = viewer.TransformPoint(GetHudOffset());
        smoothedHudPosition = SmoothPosition(
            desiredWorldPosition,
            smoothedHudPosition,
            ref haveSmoothedHudPosition);

        hudRoot.transform.position = smoothedHudPosition;
        hudRoot.transform.rotation = viewer.rotation;
        hudRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, hudScaleField.val);
    }

    private void ApplyWorldStaticAnchor()
    {
        if (hudRoot == null)
        {
            return;
        }

        if (hudRoot.transform.parent != null)
        {
            hudRoot.transform.SetParent(null, true);
            currentHudAnchor = null;
            haveSmoothedHudPosition = false;
        }

        hudRoot.transform.position = GetStaticWorldPosition();
        hudRoot.transform.rotation = GetStaticWorldRotation();
        hudRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, hudScaleField.val);
    }

    private void ApplyTransformAnchor(Transform anchor)
    {
        if (hudRoot == null || anchor == null)
        {
            return;
        }

        if (currentHudAnchor != anchor || hudRoot.transform.parent != anchor)
        {
            hudRoot.transform.SetParent(anchor, false);
            currentHudAnchor = anchor;
            haveSmoothedHudPosition = false;
        }

        hudRoot.transform.localPosition = GetHudOffset();
        hudRoot.transform.localRotation = GetAnchorLocalRotation();
        hudRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, hudScaleField.val);
    }

    private Transform ResolveRadarAnchorTransform(string anchorMode)
    {
        if (string.Equals(anchorMode, AnchorModeContainingAtom, StringComparison.Ordinal))
        {
            return ResolveAtomRootTransform(containingAtom);
        }

        if (string.Equals(anchorMode, AnchorModeAtomUid, StringComparison.Ordinal))
        {
            return ResolveAtomRootTransform(ResolveAnchorAtom());
        }

        return null;
    }

    private Atom ResolveAnchorAtom()
    {
        string uid = anchorAtomUidField != null ? (anchorAtomUidField.val ?? "") : "";
        return FindAtomByUid(uid);
    }

    private Atom FindAtomByUid(string uid)
    {
        if (string.IsNullOrEmpty(uid) || SuperController.singleton == null)
        {
            return null;
        }

        List<Atom> atoms = SuperController.singleton.GetAtoms();
        if (atoms == null)
        {
            return null;
        }

        string trimmedUid = uid.Trim();
        for (int i = 0; i < atoms.Count; i++)
        {
            Atom atom = atoms[i];
            if (atom != null && string.Equals(atom.uid, trimmedUid, StringComparison.OrdinalIgnoreCase))
            {
                return atom;
            }
        }

        return null;
    }

    private string ResolveAnchorMode()
    {
        string value = anchorModeField != null ? anchorModeField.val : "";
        return NormalizeAnchorMode(value);
    }

    private static string NormalizeAnchorMode(string value)
    {
        if (string.Equals(value, AnchorModeWorldStatic, StringComparison.OrdinalIgnoreCase))
        {
            return AnchorModeWorldStatic;
        }

        if (string.Equals(value, AnchorModeContainingAtom, StringComparison.OrdinalIgnoreCase))
        {
            return AnchorModeContainingAtom;
        }

        if (string.Equals(value, AnchorModeAtomUid, StringComparison.OrdinalIgnoreCase))
        {
            return AnchorModeAtomUid;
        }

        return AnchorModeHud;
    }

    private Quaternion GetAnchorLocalRotation()
    {
        return Quaternion.Euler(
            anchorRotationXField != null ? anchorRotationXField.val : 0.0f,
            anchorRotationYField != null ? anchorRotationYField.val : 0.0f,
            anchorRotationZField != null ? anchorRotationZField.val : 0.0f);
    }

    private Vector3 GetStaticWorldPosition()
    {
        return new Vector3(
            staticWorldXField != null ? staticWorldXField.val : 0.0f,
            staticWorldYField != null ? staticWorldYField.val : 1.5f,
            staticWorldZField != null ? staticWorldZField.val : 1.0f);
    }

    private Quaternion GetStaticWorldRotation()
    {
        return Quaternion.Euler(
            staticWorldPitchField != null ? staticWorldPitchField.val : 0.0f,
            staticWorldYawField != null ? staticWorldYawField.val : 0.0f,
            staticWorldRollField != null ? staticWorldRollField.val : 0.0f);
    }

    private Quaternion ResolveDishLocalRotation()
    {
        if (!desktopTopDownField.val)
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(Mathf.Clamp(desktopTiltDegreesField.val, 0.0f, 90.0f), 0.0f, 0.0f);
    }

    private void UpdateTargetBlip(Transform viewer, Transform target, bool showGroundDrop)
    {
        float visualRadius = ResolveVisualRadius();
        Vector3 radarLocal = ResolveTargetWorldRadarLocal(viewer, target);
        Vector3 groundLocal = ResolveTargetGroundRadarLocal(viewer, target);
        float distanceMeters = ResolveWorldDistanceMeters(viewer, target);
        float fadeAlpha = ResolveRangeFadeAlpha(distanceMeters);
        float depthScale = ResolveDepthScale(distanceMeters);
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * depthScale;
        float spin = Time.time * Mathf.Max(0.0f, ringRotationSpeedField.val * 1.75f);

        PositionTargetSphere(targetBlipObject, radarLocal, visualRadius, markerScale, spin);
        ApplyMaterialColor(targetMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val) * fadeAlpha), Mathf.Max(0.0f, emissionStrengthField.val));
        UpdateHeightStem(targetHeightStemObject, radarLocal.x, groundLocal.y, radarLocal.y, radarLocal.z, visualRadius, heightStemsEnabledField.val && fadeAlpha > 0.01f);

        if (showGroundDrop)
        {
            targetGridDropObject.transform.localPosition = new Vector3(
                groundLocal.x * visualRadius,
                groundLocal.y * visualRadius,
                groundLocal.z * visualRadius);
            targetGridDropObject.transform.localRotation = Quaternion.Euler(90.0f, spin, 0.0f);
            targetGridDropObject.transform.localScale = Vector3.one * (markerScale * 0.55f);
        }

        Vector3 meterLocal = viewer.InverseTransformPoint(target.position);
        SetStatus(string.Format(
            "Selected: {0}  x:{1:0.0}m y:{2:0.0}m z:{3:0.0}m",
            selectedUid,
            meterLocal.x,
            meterLocal.y,
            meterLocal.z));
    }

    private void UpdateLastSelectedBlip(Transform viewer)
    {
        bool showLast = false;
        float fade = 0.0f;
        Transform lastTarget = ResolveAtomRootTransform(lastSelectedAtom);
        if (lastSelectedEnabledField.val && lastTarget != null && !string.IsNullOrEmpty(lastSelectedUid))
        {
            bool sameAsCurrent = selectedAtom != null && lastSelectedAtom == selectedAtom;
            float age = Time.time - lastSelectedAtTime;
            float fadeSeconds = Mathf.Max(0.1f, lastSelectedFadeSecondsField.val);
            fade = Mathf.Clamp01(1.0f - (age / fadeSeconds));
            showLast = !sameAsCurrent && fade > 0.01f;
        }

        SetActiveIfChanged(lastTargetBlipObject, showLast);
        SetActiveIfChanged(lastTargetGridDropObject, showLast);
        if (!showLast)
        {
            return;
        }

        ApplyMaterialColor(lastTargetMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.42f * fade), Mathf.Max(0.0f, emissionStrengthField.val));
        ApplyMaterialColor(lastTargetDropMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.20f * fade), Mathf.Max(0.0f, emissionStrengthField.val));

        float visualRadius = ResolveVisualRadius();
        Vector3 radarLocal = ResolveTargetRadarLocal(viewer, lastTarget);
        Vector3 groundLocal = ResolveTargetGroundRadarLocal(viewer, lastTarget);
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * 0.82f;
        float spin = Time.time * Mathf.Max(10.0f, ringRotationSpeedField.val);

        PositionTargetSphere(lastTargetBlipObject, radarLocal, visualRadius, markerScale, -spin);

        lastTargetGridDropObject.transform.localPosition = new Vector3(
            groundLocal.x * visualRadius,
            0.0f,
            groundLocal.z * visualRadius);
        lastTargetGridDropObject.transform.localRotation = Quaternion.Euler(90.0f, -spin, 0.0f);
        lastTargetGridDropObject.transform.localScale = Vector3.one * (markerScale * 0.50f);
    }

    private void PositionTargetSphere(GameObject targetObject, Vector3 radarLocal, float visualRadius, float markerScale, float spin)
    {
        if (targetObject == null)
        {
            return;
        }

        targetObject.transform.localPosition = radarLocal * visualRadius;
        targetObject.transform.localRotation = Quaternion.AngleAxis(spin, Vector3.forward);
        targetObject.transform.localScale = Vector3.one * markerScale;
    }

    private Vector3 ResolveTargetRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        Vector3 meterLocal = viewer.InverseTransformPoint(target.position);
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal;
        if (desktopTopDownField.val)
        {
            radarLocal = new Vector3(meterLocal.x, 0.0f, meterLocal.z) / range;
        }
        else
        {
            radarLocal = meterLocal / range;
        }

        if (radarLocal.sqrMagnitude > 1.0f)
        {
            radarLocal.Normalize();
        }

        return radarLocal;
    }

    private Vector3 ResolveTargetGroundRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        Vector3 worldDelta = target.position - viewer.position;
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal = new Vector3(
            worldDelta.x / range,
            ResolveHeightRadarY(-viewer.position.y),
            worldDelta.z / range);
        Vector2 horizontal = new Vector2(radarLocal.x, radarLocal.z);
        if (horizontal.sqrMagnitude > 1.0f)
        {
            horizontal.Normalize();
            radarLocal.x = horizontal.x;
            radarLocal.z = horizontal.y;
        }

        return radarLocal;
    }

    private Vector3 ResolveTargetWorldRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        Vector3 worldDelta = target.position - viewer.position;
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal = new Vector3(
            worldDelta.x / range,
            ResolveHeightRadarY(worldDelta.y),
            worldDelta.z / range);
        Vector2 horizontal = new Vector2(radarLocal.x, radarLocal.z);
        if (horizontal.sqrMagnitude > 1.0f)
        {
            horizontal.Normalize();
            radarLocal.x = horizontal.x;
            radarLocal.z = horizontal.y;
        }

        return radarLocal;
    }

    private float ResolveHeightRadarY(float worldYDeltaMeters)
    {
        float heightScale = ResolveEffectiveHeightScaleMeters();
        return Mathf.Clamp(worldYDeltaMeters / heightScale, -1.0f, 1.0f);
    }

    private float ResolveWorldDistanceMeters(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return 0.0f;
        }

        return (target.position - viewer.position).magnitude;
    }

    private float ResolveRangeFadeAlpha(float distanceMeters)
    {
        float range = ResolveEffectiveRadarRangeMeters();
        float fadeMeters = Mathf.Max(0.0f, rangeFadeMetersField.val);
        if (distanceMeters <= range || fadeMeters <= 0.001f)
        {
            return 1.0f;
        }

        return Mathf.Clamp01(1.0f - ((distanceMeters - range) / fadeMeters));
    }

    private float ResolveDepthScale(float distanceMeters)
    {
        if (!depthSizeCueField.val)
        {
            return 1.0f;
        }

        float range = ResolveEffectiveRadarRangeMeters();
        float t = Mathf.Clamp01(distanceMeters / range);
        float strength = Mathf.Clamp01(depthSizeStrengthField.val);
        float nearScale = 1.0f + strength;
        float farScale = Mathf.Max(0.35f, 1.0f - (strength * 0.75f));
        return Mathf.Lerp(nearScale, farScale, t);
    }

    private void UpdateUserHeightStem(Transform viewer)
    {
        float visualRadius = ResolveVisualRadius();
        float groundY = viewer != null ? ResolveHeightRadarY(-viewer.position.y) : 0.0f;
        UpdateHeightStem(userHeightStemObject, 0.0f, groundY, 0.0f, 0.0f, visualRadius, heightStemsEnabledField.val);
    }

    private void UpdateHeightStem(GameObject stemObject, float x, float yStart, float yEnd, float z, float visualRadius, bool visible)
    {
        if (stemObject == null)
        {
            return;
        }

        float length = Mathf.Abs(yEnd - yStart);
        bool showStem = visible && length > 0.01f;
        SetActiveIfChanged(stemObject, showStem);
        if (!showStem)
        {
            return;
        }

        float midpoint = (yStart + yEnd) * 0.5f;
        stemObject.transform.localPosition = new Vector3(x * visualRadius, midpoint * visualRadius, z * visualRadius);
        stemObject.transform.localRotation = Quaternion.identity;
        stemObject.transform.localScale = new Vector3(visualRadius, visualRadius * length, visualRadius);
    }

    private void PollAvailableAtomsIfDue(Transform viewer)
    {
        if (Time.time < nextAtomPollTime)
        {
            return;
        }

        float interval = Mathf.Max(0.15f, atomPollSecondsField.val);
        nextAtomPollTime = Time.time + interval;
        trackedAvailableAtoms.Clear();

        if (!availableAtomMarkersEnabledField.val || SuperController.singleton == null)
        {
            return;
        }

        List<Atom> atoms = SuperController.singleton.GetAtoms();
        if (atoms == null)
        {
            return;
        }

        for (int i = 0; i < atoms.Count; i++)
        {
            Atom atom = atoms[i];
            if (!IsAtomVisibleByFilter(atom))
            {
                continue;
            }

            trackedAvailableAtoms.Add(atom);
        }

        if (viewer != null)
        {
            trackedAvailableAtoms.Sort(delegate(Atom left, Atom right)
            {
                Transform leftTransform = ResolveAtomRootTransform(left);
                Transform rightTransform = ResolveAtomRootTransform(right);
                float leftDistance = leftTransform != null ? (leftTransform.position - viewer.position).sqrMagnitude : float.MaxValue;
                float rightDistance = rightTransform != null ? (rightTransform.position - viewer.position).sqrMagnitude : float.MaxValue;
                return leftDistance.CompareTo(rightDistance);
            });
        }

        EnsureAvailableMarkerCapacity(trackedAvailableAtoms.Count);
    }

    private bool IsAtomVisibleByFilter(Atom atom)
    {
        if (atom == null)
        {
            return false;
        }

        if (!atom.on || atom.hidden)
        {
            return false;
        }

        if (selectedAtom != null && atom == selectedAtom)
        {
            return false;
        }

        if (IsRadarGrabHandleAtom(atom))
        {
            return false;
        }

        if (ignoreContainingAtomField.val && containingAtom != null && atom == containingAtom)
        {
            return false;
        }

#if FA_RADAR_PRO
        bool light = IsLightAtom(atom);
        bool cua = IsCustomUnityAssetAtom(atom);
        bool person = IsPersonAtom(atom);
        bool other = !light && !cua && !person;
        return
            (light && showLightAtomsField.val) ||
            (cua && showCustomUnityAssetAtomsField.val) ||
            (person && showPersonAtomsField.val) ||
            (other && showOtherAtomsField.val);
#else
        return true;
#endif
    }

    private bool IsLightAtom(Atom atom)
    {
        return AtomTextContains(atom, "light");
    }

    private bool IsRadarGrabHandleAtom(Atom atom)
    {
        return atom != null
            && !string.IsNullOrEmpty(atom.uid)
            && atom.uid.StartsWith("FA_Radar_" + EditionName + "_grab_", StringComparison.Ordinal);
    }

    private bool IsCustomUnityAssetAtom(Atom atom)
    {
        return AtomTextContains(atom, "customunityasset") || AtomTextContains(atom, "cua");
    }

    private bool IsPersonAtom(Atom atom)
    {
        return AtomTextContains(atom, "person");
    }

    private bool AtomTextContains(Atom atom, string value)
    {
        if (atom == null || string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(atom.type) && atom.type.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }
        if (!string.IsNullOrEmpty(atom.category) && atom.category.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }
        if (!string.IsNullOrEmpty(atom.uid) && atom.uid.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return false;
    }

    private void EnsureAvailableMarkerCapacity(int requiredCount)
    {
        int currentCount = availableMarkerObjects != null ? availableMarkerObjects.Length : 0;
        if (currentCount >= requiredCount)
        {
            return;
        }

        GameObject[] newMarkers = new GameObject[requiredCount];
        GameObject[] newStems = new GameObject[requiredCount];
        Material[] newMaterials = new Material[requiredCount];

        for (int i = 0; i < currentCount; i++)
        {
            newMarkers[i] = availableMarkerObjects[i];
            newStems[i] = availableStemObjects[i];
            newMaterials[i] = availableMarkerMaterials[i];
        }

        for (int i = currentCount; i < requiredCount; i++)
        {
            Material markerMaterial = CreateEmissiveOverlayMaterial("FA Radar Available Atom Material " + i, new Color(0.58f, 0.74f, 1.0f, 0.46f), MarkerRenderQueue);
            newMaterials[i] = markerMaterial;
            newMarkers[i] = CreateMeshObject("FA Radar Available Atom " + i, axisRoot.transform, targetBlipMesh, markerMaterial, MarkerRenderQueue, MarkerSortingOrder - 8);
            newStems[i] = CreateMeshObject("FA Radar Available Height Stem " + i, axisRoot.transform, heightStemMesh, availableHeightStemMaterial, MarkerRenderQueue, MarkerSortingOrder - 9);
            SetActiveIfChanged(newMarkers[i], false);
            SetActiveIfChanged(newStems[i], false);
        }

        availableMarkerObjects = newMarkers;
        availableStemObjects = newStems;
        availableMarkerMaterials = newMaterials;
    }

    private void UpdateAvailableAtomMarkers(Transform viewer)
    {
        int visibleCount = availableAtomMarkersEnabledField.val && trackedAvailableAtoms != null ? trackedAvailableAtoms.Count : 0;
        float visualRadius = ResolveVisualRadius();
        for (int i = 0; availableMarkerObjects != null && i < availableMarkerObjects.Length; i++)
        {
            bool show = i < visibleCount;
            if (!show)
            {
                SetActiveIfChanged(availableMarkerObjects[i], false);
                if (availableStemObjects != null && i < availableStemObjects.Length)
                {
                    SetActiveIfChanged(availableStemObjects[i], false);
                }
                continue;
            }

            Atom atom = trackedAvailableAtoms[i];
            Transform target = ResolveAtomRootTransform(atom);
            if (target == null)
            {
                SetActiveIfChanged(availableMarkerObjects[i], false);
                continue;
            }

            Vector3 radarLocal = ResolveTargetWorldRadarLocal(viewer, target);
            Vector3 groundLocal = ResolveTargetGroundRadarLocal(viewer, target);
            float distanceMeters = ResolveWorldDistanceMeters(viewer, target);
            float fadeAlpha = ResolveRangeFadeAlpha(distanceMeters);
            if (fadeAlpha <= 0.01f)
            {
                SetActiveIfChanged(availableMarkerObjects[i], false);
                if (availableStemObjects != null && i < availableStemObjects.Length)
                {
                    SetActiveIfChanged(availableStemObjects[i], false);
                }
                continue;
            }

            float depthScale = ResolveDepthScale(distanceMeters);
            float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * 0.58f * depthScale;
            Color color = ResolveAvailableAtomColor(atom, Mathf.Clamp01(availableAtomAlphaField.val) * fadeAlpha);
            ApplyMaterialColor(availableMarkerMaterials[i], color, Mathf.Max(0.0f, emissionStrengthField.val) * 0.85f);
            SetActiveIfChanged(availableMarkerObjects[i], true);
            PositionTargetSphere(availableMarkerObjects[i], radarLocal, visualRadius, markerScale, 0.0f);
            UpdateHeightStem(availableStemObjects[i], radarLocal.x, groundLocal.y, radarLocal.y, radarLocal.z, visualRadius, heightStemsEnabledField.val && fadeAlpha > 0.08f);
        }
    }

    private void HandleRadarMarkerClick()
    {
        if (!clickSelectMarkersField.val || !availableAtomMarkersEnabledField.val || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        Camera camera = ResolveViewerCamera();
        if (camera == null)
        {
            SetStatus("Radar click ignored: no active camera.");
            return;
        }

        Atom atom = ResolveClickedAvailableAtom(camera);
        if (atom != null)
        {
            SelectRadarAtom(atom);
        }
    }

    private void UpdateSessionGrabHandles(Transform viewer)
    {
        RefreshGripGrabState();
        if (!ShouldUseSessionGrabHandles(viewer))
        {
            DestroySessionGrabHandles();
            return;
        }

        Vector3 radarCenter = ResolveRadarWorldCenter(viewer);
        EnsurePrimaryGrabHandleAtom(radarCenter);
        if (primaryGrabHandleAtom == null || primaryGrabHandleAtom.mainController == null)
        {
            SetResizeGuideLineVisible(false);
            return;
        }

        ConfigureGrabHandleAtom(primaryGrabHandleAtom, radarCenter);
        FreeControllerV3 primaryController = primaryGrabHandleAtom.mainController;
        int grabbedHand;
        bool primaryGrabbed = TryResolveGrabbedHand(primaryController, out grabbedHand);
        if (!moveGrabActive)
        {
            if (primaryGrabbed)
            {
                StartMoveGrab(primaryController, grabbedHand);
            }
            else
            {
                TryStartFauxPrimaryGrab(radarCenter);
            }
        }
        else if (moveGrabUsesGripFallback)
        {
            if (!IsGripHeld(moveGrabHand))
            {
                EndMoveGrab();
            }
        }
        else if (!primaryGrabbed)
        {
            EndMoveGrab();
        }

        if (moveGrabActive)
        {
            if (moveGrabUsesGripFallback)
            {
                UpdateFauxMoveGrab(viewer);
            }
            else
            {
                UpdateMoveGrab(viewer, primaryController);
            }
            UpdateResizeGrabHandle(viewer, primaryController);
            return;
        }

        MoveGrabHandleAtom(primaryGrabHandleAtom, radarCenter);
        DestroyResizeGrabHandleAtom();
        SetResizeGuideLineVisible(false);
    }

    private bool ShouldUseSessionGrabHandles(Transform viewer)
    {
        if (viewer == null || hudRoot == null || !ReadBool(grabHandlesEnabledField, false))
        {
            return false;
        }

        if (IsCuaPreferenceProfileActive())
        {
            return false;
        }

        if (containingAtom != null && IsCustomUnityAssetAtom(containingAtom))
        {
            return false;
        }

        return recorderRadarVisible && radarEnabledField != null && radarEnabledField.val;
    }

    private void EnsurePrimaryGrabHandleAtom(Vector3 worldPosition)
    {
        primaryGrabHandleUid = BuildGrabHandleUid(GrabHandlePrimarySuffix);
        primaryGrabHandleAtom = ResolveGrabHandleAtom(primaryGrabHandleUid, primaryGrabHandleAtom);
        if (primaryGrabHandleAtom != null || primaryGrabHandleCreatePending)
        {
            return;
        }

        primaryGrabHandleCreatePending = true;
        StartCoroutine(CreateGrabHandleAtomCoroutine(primaryGrabHandleUid, true, worldPosition));
    }

    private void EnsureResizeGrabHandleAtom(Vector3 worldPosition)
    {
        resizeGrabHandleUid = BuildGrabHandleUid(GrabHandleResizeSuffix);
        resizeGrabHandleAtom = ResolveGrabHandleAtom(resizeGrabHandleUid, resizeGrabHandleAtom);
        if (resizeGrabHandleAtom != null || resizeGrabHandleCreatePending)
        {
            return;
        }

        resizeGrabHandleCreatePending = true;
        StartCoroutine(CreateGrabHandleAtomCoroutine(resizeGrabHandleUid, false, worldPosition));
    }

    private IEnumerator CreateGrabHandleAtomCoroutine(string uid, bool primary, Vector3 worldPosition)
    {
        if (SuperController.singleton == null || string.IsNullOrEmpty(uid))
        {
            SetGrabHandleCreatePending(primary, false);
            yield break;
        }

        Atom atom = SuperController.singleton.GetAtomByUid(uid);
        if (atom == null)
        {
            yield return SuperController.singleton.AddAtomByType("Empty", uid, false, false, false);
            atom = SuperController.singleton.GetAtomByUid(uid);
        }

        if (atom != null)
        {
            ConfigureGrabHandleAtom(atom, worldPosition);
            if (primary)
            {
                primaryGrabHandleAtom = atom;
            }
            else
            {
                resizeGrabHandleAtom = atom;
            }
        }

        SetGrabHandleCreatePending(primary, false);
    }

    private void SetGrabHandleCreatePending(bool primary, bool pending)
    {
        if (primary)
        {
            primaryGrabHandleCreatePending = pending;
        }
        else
        {
            resizeGrabHandleCreatePending = pending;
        }
    }

    private Atom ResolveGrabHandleAtom(string uid, Atom current)
    {
        if (SuperController.singleton == null || string.IsNullOrEmpty(uid))
        {
            return null;
        }

        Atom found = SuperController.singleton.GetAtomByUid(uid);
        if (found != null)
        {
            return found;
        }

        return null;
    }

    private string BuildGrabHandleUid(string suffix)
    {
        string instance = GetInstanceID().ToString(CultureInfo.InvariantCulture).Replace("-", "n");
        return "FA_Radar_" + EditionName + "_grab_" + instance + "_" + suffix;
    }

    private void ConfigureGrabHandleAtom(Atom atom, Vector3 worldPosition)
    {
        if (atom == null)
        {
            return;
        }

        try
        {
            atom.SetOn(true);
            atom.hidden = false;
        }
        catch
        {
        }

        FreeControllerV3 controller = atom.mainController;
        if (controller == null)
        {
            return;
        }

        bool debugVisible = ReadBool(grabHandleDebugVisibleField, false);
        try
        {
            // Built-In Grab Target: keep the controller active for VaM grabbing while suppressing physics/collision noise.
            controller.currentPositionState = FreeControllerV3.PositionState.On;
            controller.currentRotationState = FreeControllerV3.RotationState.Off;
            controller.controlMode = FreeControllerV3.ControlMode.Position;
            controller.canGrabPosition = true;
            controller.canGrabRotation = false;
            controller.hidden = false;
            controller.guihidden = !debugVisible;
            controller.collisionEnabled = false;
            controller.physicsEnabled = false;
            controller.controlsCollisionEnabled = false;
            controller.controlsOn = true;
            controller.freezeAtomPhysicsWhenGrabbed = false;
            controller.GUIalwaysVisibleWhenSelected = debugVisible;
            controller.drawMeshWhenDeselected = true;
            controller.deselectedMeshScale = ResolveBuiltInGrabHandleScaleMeters();
            controller.selectedScale = ResolveBuiltInGrabHandleScaleMeters();
        }
        catch
        {
        }

        int unusedHand;
        if (!TryResolveGrabbedHand(controller, out unusedHand))
        {
            MoveGrabHandleAtom(atom, worldPosition);
        }
    }

    private void MoveGrabHandleAtom(Atom atom, Vector3 worldPosition)
    {
        if (atom == null || atom.mainController == null)
        {
            return;
        }

        try
        {
            atom.mainController.MoveTo(worldPosition, true);
        }
        catch
        {
            try
            {
                atom.mainController.transform.position = worldPosition;
            }
            catch
            {
            }
        }
    }

    private bool TryResolveGrabbedHand(FreeControllerV3 controller, out int hand)
    {
        hand = GrabHandUnknown;
        if (controller == null)
        {
            return false;
        }

        SuperController sc = SuperController.singleton;
        if (sc != null)
        {
            try
            {
                if (sc.LeftGrabbedController == controller || sc.LeftFullGrabbedController == controller)
                {
                    hand = GrabHandLeft;
                    return true;
                }

                if (sc.RightGrabbedController == controller || sc.RightFullGrabbedController == controller)
                {
                    hand = GrabHandRight;
                    return true;
                }
            }
            catch
            {
            }
        }

        try
        {
            if (controller.isGrabbing)
            {
                hand = ResolveNearestGripControllerHand(GetControllerWorldPosition(controller));
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private void StartMoveGrab(FreeControllerV3 primaryController, int hand)
    {
        StartMoveGrabAtPosition(GetControllerWorldPosition(primaryController), hand, false);
    }

    private void StartMoveGrabAtPosition(Vector3 handlePosition, int hand, bool gripFallback)
    {
        moveGrabActive = true;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = gripFallback;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabHand = hand;
        resizeGrabHand = GrabHandUnknown;
        moveStartHandlePosition = handlePosition;
        moveStartHudOffset = GetHudOffset();
        moveStartStaticPosition = GetStaticWorldPosition();
        haveSmoothedHudPosition = false;
        PulseGrabHandleHaptics(hand, 0.35f, 0.28f, 0.045f);
        SetStatus("Radar grab move active.");
    }

    private void UpdateMoveGrab(Transform viewer, FreeControllerV3 primaryController)
    {
        if (!moveGrabActive || primaryController == null)
        {
            return;
        }

        Vector3 worldDelta = GetControllerWorldPosition(primaryController) - moveStartHandlePosition;
        ApplyMoveGrabDelta(viewer, worldDelta);
    }

    private void UpdateFauxMoveGrab(Transform viewer)
    {
        if (!moveGrabActive)
        {
            return;
        }

        Vector3 controllerPosition;
        if (!GetGripControllerWorldPosition(moveGrabHand, out controllerPosition))
        {
            return;
        }

        ApplyMoveGrabDelta(viewer, controllerPosition - moveStartHandlePosition);
    }

    private void EndMoveGrab()
    {
        if (moveGrabActive)
        {
            MarkGlobalPreferencesDirty();
            PulseGrabHandleHaptics(moveGrabHand, 0.22f, 0.20f, 0.035f);
        }

        moveGrabActive = false;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = false;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroyResizeGrabHandleAtom();
        SetResizeGuideLineVisible(false);
        SetStatus("Radar grab move applied.");
    }

    private void ApplyMoveGrabDelta(Transform viewer, Vector3 worldDelta)
    {
        string anchorMode = ResolveAnchorMode();
        if (string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal))
        {
            SetStaticWorldPositionNoCallback(moveStartStaticPosition + worldDelta);
            haveSmoothedHudPosition = false;
            return;
        }

        Transform reference = viewer;
        if (!string.Equals(anchorMode, AnchorModeHud, StringComparison.Ordinal))
        {
            Transform anchor = ResolveRadarAnchorTransform(anchorMode);
            if (anchor != null)
            {
                reference = anchor;
            }
        }

        Vector3 localDelta = reference != null ? reference.InverseTransformVector(worldDelta) : worldDelta;
        SetHudOffsetNoCallback(moveStartHudOffset + localDelta);
        haveSmoothedHudPosition = false;
    }

    private void UpdateResizeGrabHandle(Transform viewer, FreeControllerV3 primaryController)
    {
        if (primaryController == null || moveGrabHand == GrabHandUnknown || resizeHandleDismissedUntilMoveRelease)
        {
            DestroyResizeGrabHandleAtom();
            SetResizeGuideLineVisible(false);
            return;
        }

        int freeHand = moveGrabHand == GrabHandLeft ? GrabHandRight : GrabHandLeft;
        Vector3 primaryPosition = GetControllerWorldPosition(primaryController);
        Vector3 gripPrimaryPosition;
        if (moveGrabUsesGripFallback && GetGripControllerWorldPosition(moveGrabHand, out gripPrimaryPosition))
        {
            primaryPosition = gripPrimaryPosition;
        }

        Vector3 resizeTarget;
        if (!GetGripControllerWorldPosition(freeHand, out resizeTarget))
        {
            Transform freeController = ResolveMotionControllerTransform(freeHand);
            resizeTarget = freeController != null
                ? freeController.position
                : primaryPosition + ((viewer != null ? viewer.right : Vector3.right) * 0.25f);
        }

        EnsureResizeGrabHandleAtom(resizeTarget);
        if (resizeGrabHandleAtom == null || resizeGrabHandleAtom.mainController == null)
        {
            UpdateResizeGuideLine(primaryPosition, resizeTarget, false);
            return;
        }

        ConfigureGrabHandleAtom(resizeGrabHandleAtom, resizeTarget);
        FreeControllerV3 resizeController = resizeGrabHandleAtom.mainController;
        int grabbedHand;
        bool resizeGrabbed = TryResolveGrabbedHand(resizeController, out grabbedHand);
        if (resizeGrabbed && !resizeGrabActive && grabbedHand != moveGrabHand)
        {
            StartResizeGrab(primaryController, resizeController, grabbedHand);
        }
        else if (!resizeGrabActive)
        {
            TryStartFauxResizeGrab(primaryPosition, resizeTarget, freeHand);
        }
        else if (resizeGrabUsesGripFallback)
        {
            if (!IsGripHeld(resizeGrabHand))
            {
                EndResizeGrab(true);
                return;
            }
        }
        else if ((!resizeGrabbed || grabbedHand == moveGrabHand) && resizeGrabActive)
        {
            EndResizeGrab(true);
            return;
        }

        if (resizeGrabActive)
        {
            if (resizeGrabUsesGripFallback)
            {
                UpdateFauxResizeGrab();
            }
            else
            {
                UpdateResizeGrab(primaryController, resizeController);
                UpdateResizeGuideLine(GetControllerWorldPosition(primaryController), GetControllerWorldPosition(resizeController), true);
            }
            return;
        }

        MoveGrabHandleAtom(resizeGrabHandleAtom, resizeTarget);
        UpdateResizeGuideLine(primaryPosition, resizeTarget, false);
    }

    private void StartResizeGrab(FreeControllerV3 primaryController, FreeControllerV3 resizeController, int hand)
    {
        StartResizeGrabAtPositions(GetControllerWorldPosition(primaryController), GetControllerWorldPosition(resizeController), hand, false);
    }

    private void StartResizeGrabAtPositions(Vector3 primaryPosition, Vector3 resizePosition, int hand, bool gripFallback)
    {
        resizeGrabActive = true;
        resizeGrabUsesGripFallback = gripFallback;
        resizeGrabHand = hand;
        resizeStartScale = Mathf.Max(0.01f, ReadFloat(hudScaleField, 0.49f));
        resizeStartPrimaryPosition = primaryPosition;
        resizeStartHandlePosition = resizePosition;
        resizeStartDistance = Mathf.Max(
            GrabResizeMinimumStartDistanceMeters,
            Vector3.Distance(resizeStartPrimaryPosition, resizeStartHandlePosition));
        PulseGrabHandleHaptics(hand, 0.48f, 0.35f, 0.05f);
        SetStatus("Radar grab resize active.");
    }

    private void UpdateResizeGrab(FreeControllerV3 primaryController, FreeControllerV3 resizeController)
    {
        if (!resizeGrabActive || primaryController == null || resizeController == null)
        {
            return;
        }

        float currentDistance = Mathf.Max(
            GrabResizeMinimumStartDistanceMeters,
            Vector3.Distance(GetControllerWorldPosition(primaryController), GetControllerWorldPosition(resizeController)));
        float ratio = currentDistance / Mathf.Max(GrabResizeMinimumStartDistanceMeters, resizeStartDistance);
        SetHudScaleNoCallback(resizeStartScale * ratio);
    }

    private void UpdateFauxResizeGrab()
    {
        if (!resizeGrabActive)
        {
            return;
        }

        Vector3 primaryPosition;
        Vector3 resizePosition;
        if (!GetGripControllerWorldPosition(moveGrabHand, out primaryPosition)
            || !GetGripControllerWorldPosition(resizeGrabHand, out resizePosition))
        {
            return;
        }

        float currentDistance = Mathf.Max(
            GrabResizeMinimumStartDistanceMeters,
            Vector3.Distance(primaryPosition, resizePosition));
        float ratio = currentDistance / Mathf.Max(GrabResizeMinimumStartDistanceMeters, resizeStartDistance);
        SetHudScaleNoCallback(resizeStartScale * ratio);
        UpdateResizeGuideLine(primaryPosition, resizePosition, true);
    }

    private void EndResizeGrab(bool dismissUntilMoveRelease)
    {
        if (resizeGrabActive)
        {
            MarkGlobalPreferencesDirty();
            PulseGrabHandleHaptics(resizeGrabHand, 0.26f, 0.22f, 0.035f);
        }

        resizeGrabActive = false;
        resizeGrabUsesGripFallback = false;
        resizeGrabHand = GrabHandUnknown;
        resizeHandleDismissedUntilMoveRelease = dismissUntilMoveRelease;
        DestroyResizeGrabHandleAtom();
        SetResizeGuideLineVisible(false);
        SetStatus("Radar grab resize applied.");
    }

    private void RefreshGripGrabState()
    {
        previousLeftGripValue = leftGripValue;
        previousRightGripValue = rightGripValue;
        leftGripValue = ReadLeftGripValue();
        rightGripValue = ReadRightGripValue();
    }

    private float ReadLeftGripValue()
    {
        return ReadGripValue(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Button.PrimaryHandTrigger, OVRInput.RawButton.LHandTrigger, OVRInput.Controller.LTouch);
    }

    private float ReadRightGripValue()
    {
        return ReadGripValue(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Button.SecondaryHandTrigger, OVRInput.RawButton.RHandTrigger, OVRInput.Controller.RTouch);
    }

    private float ReadGripValue(OVRInput.Axis1D axis, OVRInput.Button button, OVRInput.RawButton rawButton, OVRInput.Controller controller)
    {
        float value = 0.0f;
        try
        {
            value = Mathf.Clamp01(OVRInput.Get(axis));
            value = Mathf.Max(value, Mathf.Clamp01(OVRInput.Get(axis, controller)));
            if (OVRInput.Get(button) || OVRInput.Get(button, controller) || OVRInput.Get(rawButton))
            {
                value = 1.0f;
            }
        }
        catch
        {
            value = 0.0f;
        }

        return value;
    }

    private bool TryStartFauxPrimaryGrab(Vector3 radarCenter)
    {
        Vector3 leftPosition = Vector3.zero;
        Vector3 rightPosition = Vector3.zero;
        bool leftPressed = IsGripPressedThisFrame(GrabHandLeft) && GetGripControllerWorldPosition(GrabHandLeft, out leftPosition);
        bool rightPressed = IsGripPressedThisFrame(GrabHandRight) && GetGripControllerWorldPosition(GrabHandRight, out rightPosition);
        float hitRadius = ResolveGripGrabHitRadiusMeters();

        if (leftPressed && Vector3.Distance(leftPosition, radarCenter) <= hitRadius)
        {
            StartMoveGrabAtPosition(leftPosition, GrabHandLeft, true);
            return true;
        }

        if (rightPressed && Vector3.Distance(rightPosition, radarCenter) <= hitRadius)
        {
            StartMoveGrabAtPosition(rightPosition, GrabHandRight, true);
            return true;
        }

        return false;
    }

    private bool TryStartFauxResizeGrab(Vector3 primaryPosition, Vector3 resizePosition, int hand)
    {
        if (hand == GrabHandUnknown || hand == moveGrabHand || !IsGripHeld(moveGrabHand) || !IsGripPressedThisFrame(hand))
        {
            return false;
        }

        StartResizeGrabAtPositions(primaryPosition, resizePosition, hand, true);
        UpdateResizeGuideLine(primaryPosition, resizePosition, true);
        return true;
    }

    private bool IsGripPressedThisFrame(int hand)
    {
        return GetGripValue(hand) >= GripGrabPressThreshold && GetPreviousGripValue(hand) < GripGrabPressThreshold;
    }

    private bool IsGripHeld(int hand)
    {
        return GetGripValue(hand) > GripGrabReleaseThreshold;
    }

    private float GetGripValue(int hand)
    {
        if (hand == GrabHandLeft)
        {
            return leftGripValue;
        }

        if (hand == GrabHandRight)
        {
            return rightGripValue;
        }

        return 0.0f;
    }

    private float GetPreviousGripValue(int hand)
    {
        if (hand == GrabHandLeft)
        {
            return previousLeftGripValue;
        }

        if (hand == GrabHandRight)
        {
            return previousRightGripValue;
        }

        return 0.0f;
    }

    private float ResolveGripGrabHitRadiusMeters()
    {
        return Mathf.Clamp(ReadFloat(grabHitRadiusMetersField, 0.16f), 0.04f, 0.45f);
    }

    private float ResolveBuiltInGrabHandleScaleMeters()
    {
        return Mathf.Clamp(ResolveGripGrabHitRadiusMeters(), 0.06f, 0.22f);
    }

    private int ResolveNearestGripControllerHand(Vector3 worldPosition)
    {
        Vector3 leftPosition;
        Vector3 rightPosition;
        bool haveLeft = GetGripControllerWorldPosition(GrabHandLeft, out leftPosition);
        bool haveRight = GetGripControllerWorldPosition(GrabHandRight, out rightPosition);
        if (haveLeft && haveRight)
        {
            return Vector3.SqrMagnitude(worldPosition - leftPosition) <= Vector3.SqrMagnitude(worldPosition - rightPosition)
                ? GrabHandLeft
                : GrabHandRight;
        }

        if (haveLeft)
        {
            return GrabHandLeft;
        }

        if (haveRight)
        {
            return GrabHandRight;
        }

        return GrabHandUnknown;
    }

    private bool GetGripControllerWorldPosition(int hand, out Vector3 position)
    {
        position = Vector3.zero;
        Transform controller = ResolveMotionControllerTransform(hand);
        if (controller == null)
        {
            return false;
        }

        position = controller.position;
        return true;
    }

    private Transform ResolveMotionControllerTransform(int hand)
    {
        SuperController sc = SuperController.singleton;
        if (sc == null)
        {
            return null;
        }

        try
        {
            Camera controllerCamera = hand == GrabHandLeft ? sc.leftControllerCamera : sc.rightControllerCamera;
            return controllerCamera != null ? controllerCamera.transform : null;
        }
        catch
        {
            return null;
        }
    }

    private Vector3 GetControllerWorldPosition(FreeControllerV3 controller)
    {
        if (controller == null)
        {
            return Vector3.zero;
        }

        try
        {
            if (controller.control != null)
            {
                return controller.control.position;
            }
        }
        catch
        {
        }

        return controller.transform.position;
    }

    private Vector3 ResolveRadarWorldCenter(Transform viewer)
    {
        string anchorMode = ResolveAnchorMode();
        if (string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal))
        {
            return GetStaticWorldPosition();
        }

        Transform anchor = !string.Equals(anchorMode, AnchorModeHud, StringComparison.Ordinal)
            ? ResolveRadarAnchorTransform(anchorMode)
            : null;
        if (anchor != null)
        {
            return anchor.TransformPoint(GetHudOffset());
        }

        if (viewer != null)
        {
            return viewer.TransformPoint(GetHudOffset());
        }

        return hudRoot != null ? hudRoot.transform.position : Vector3.zero;
    }

    private void EnsureResizeGuideLine()
    {
        if (resizeGuideLineObject != null)
        {
            return;
        }

        resizeGuideLineMesh = CreateDottedLineMesh();
        grabGuideMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Grab Resize Guide Material"), new Color(0.72f, 1.0f, 1.0f, 0.36f), MarkerRenderQueue);
        resizeGuideLineObject = CreateMeshObject(BuildFilmSubjectName("Grab Resize Guide"), null, resizeGuideLineMesh, grabGuideMaterial, MarkerRenderQueue, MarkerSortingOrder - 6);
        SetActiveIfChanged(resizeGuideLineObject, false);
    }

    private Mesh CreateDottedLineMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Grab Resize Dotted Line Mesh";
        return mesh;
    }

    private void UpdateResizeGuideLine(Vector3 start, Vector3 end, bool grabbed)
    {
        EnsureResizeGuideLine();
        if (resizeGuideLineMesh == null)
        {
            return;
        }

        Vector3 delta = end - start;
        float distance = delta.magnitude;
        if (distance < 0.01f)
        {
            SetResizeGuideLineVisible(false);
            return;
        }

        Vector3 direction = delta / distance;
        Vector3 side = Vector3.Cross(direction, Vector3.up);
        if (side.sqrMagnitude < 0.0001f)
        {
            side = Vector3.Cross(direction, Vector3.right);
        }
        side.Normalize();

        float width = grabbed ? 0.006f : 0.004f;
        float dashLength = Mathf.Clamp(distance * 0.075f, 0.025f, 0.09f);
        float gapLength = dashLength * 0.85f;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (float cursor = 0.0f; cursor < distance; cursor += dashLength + gapLength)
        {
            float segmentEnd = Mathf.Min(distance, cursor + dashLength);
            AddWorldLineSegment(vertices, triangles, start + (direction * cursor), start + (direction * segmentEnd), side * width);
        }

        resizeGuideLineMesh.Clear();
        resizeGuideLineMesh.SetVertices(vertices);
        resizeGuideLineMesh.SetTriangles(triangles, 0);
        resizeGuideLineMesh.RecalculateBounds();
        ApplyMaterialColor(grabGuideMaterial, new Color(0.72f, 1.0f, 1.0f, grabbed ? 0.74f : 0.36f), Mathf.Max(0.0f, emissionStrengthField.val));
        SetResizeGuideLineVisible(true);
    }

    private void AddWorldLineSegment(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, Vector3 side)
    {
        int index = vertices.Count;
        vertices.Add(start - side);
        vertices.Add(start + side);
        vertices.Add(end + side);
        vertices.Add(end - side);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private void SetResizeGuideLineVisible(bool visible)
    {
        SetActiveIfChanged(resizeGuideLineObject, visible);
    }

    private void PulseGrabHandleHaptics(int hand, float frequency, float amplitude, float durationSeconds)
    {
        if (!ReadBool(grabHapticsEnabledField, true))
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now - lastGrabHandleHapticAt < GrabHapticCooldownSeconds)
        {
            return;
        }

        lastGrabHandleHapticAt = now;
        if (hand == GrabHandLeft || hand == GrabHandUnknown)
        {
            PulseGrabHandleOvrHaptic(true, frequency, amplitude, durationSeconds);
        }
        if (hand == GrabHandRight || hand == GrabHandUnknown)
        {
            PulseGrabHandleOvrHaptic(false, frequency, amplitude, durationSeconds);
        }
    }

    private void PulseGrabHandleOvrHaptic(bool left, float frequency, float amplitude, float durationSeconds)
    {
        try
        {
            OVRInput.Controller controller = left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            OVRInput.SetControllerVibration(Mathf.Clamp01(frequency), Mathf.Clamp01(amplitude), controller);
            if (left)
            {
                ovrLeftGrabHapticActive = true;
                ovrLeftGrabHapticStopAt = Time.unscaledTime + Mathf.Clamp(durationSeconds, 0.01f, 0.25f);
            }
            else
            {
                ovrRightGrabHapticActive = true;
                ovrRightGrabHapticStopAt = Time.unscaledTime + Mathf.Clamp(durationSeconds, 0.01f, 0.25f);
            }
        }
        catch
        {
        }
    }

    private void TickGrabHandleHapticStops(float now)
    {
        if (ovrLeftGrabHapticActive && now >= ovrLeftGrabHapticStopAt)
        {
            StopGrabHandleOvrHaptic(true);
        }
        if (ovrRightGrabHapticActive && now >= ovrRightGrabHapticStopAt)
        {
            StopGrabHandleOvrHaptic(false);
        }
    }

    private void StopGrabHandleOvrHaptic(bool left)
    {
        try
        {
            OVRInput.SetControllerVibration(0.0f, 0.0f, left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
        }
        catch
        {
        }

        if (left)
        {
            ovrLeftGrabHapticActive = false;
        }
        else
        {
            ovrRightGrabHapticActive = false;
        }
    }

    private void DestroySessionGrabHandles()
    {
        if (moveGrabActive || resizeGrabActive)
        {
            MarkGlobalPreferencesDirty();
        }

        moveGrabActive = false;
        resizeGrabActive = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroyGrabHandleAtom(ref primaryGrabHandleAtom, primaryGrabHandleUid);
        primaryGrabHandleCreatePending = false;
        DestroyResizeGrabHandleAtom();
        SetResizeGuideLineVisible(false);
        StopGrabHandleOvrHaptic(true);
        StopGrabHandleOvrHaptic(false);
    }

    private void DestroyResizeGrabHandleAtom()
    {
        DestroyGrabHandleAtom(ref resizeGrabHandleAtom, resizeGrabHandleUid);
        resizeGrabHandleCreatePending = false;
    }

    private void DestroyGrabHandleAtom(ref Atom atom, string uid)
    {
        Atom target = atom;
        if (target == null && !string.IsNullOrEmpty(uid) && SuperController.singleton != null)
        {
            target = SuperController.singleton.GetAtomByUid(uid);
        }

        if (target != null && SuperController.singleton != null)
        {
            try
            {
                RemoveGrabHandleAtom(target);
            }
            catch
            {
                try
                {
                    target.Remove();
                }
                catch
                {
                }
            }
        }

        atom = null;
    }

    private void RemoveGrabHandleAtom(Atom atom)
    {
        if (atom != null && SuperController.singleton != null)
        {
            SuperController.singleton.RemoveAtom(atom);
        }
    }

    private Atom ResolveClickedAvailableAtom(Camera camera)
    {
        if (camera == null || trackedAvailableAtoms == null || availableMarkerObjects == null)
        {
            return null;
        }

        Vector2 clickPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        float bestScore = float.MaxValue;
        float bestDepth = float.MaxValue;
        Atom bestAtom = null;
        int visibleCount = Mathf.Min(trackedAvailableAtoms.Count, availableMarkerObjects.Length);

        for (int i = 0; i < visibleCount; i++)
        {
            Atom atom = trackedAvailableAtoms[i];
            GameObject markerObject = availableMarkerObjects[i];
            if (atom == null || markerObject == null || !markerObject.activeSelf)
            {
                continue;
            }

            Vector3 screenPosition = camera.WorldToScreenPoint(markerObject.transform.position);
            if (screenPosition.z <= 0.0f)
            {
                continue;
            }

            Vector2 markerPosition = new Vector2(screenPosition.x, screenPosition.y);
            float clickRadius = ResolveMarkerScreenRadiusPixels(camera, markerObject);
            float distance = Vector2.Distance(clickPosition, markerPosition);
            if (distance > clickRadius)
            {
                continue;
            }

            float score = distance / Mathf.Max(1.0f, clickRadius);
            if (score < bestScore || (Mathf.Abs(score - bestScore) < 0.001f && screenPosition.z < bestDepth))
            {
                bestScore = score;
                bestDepth = screenPosition.z;
                bestAtom = atom;
            }
        }

        return bestAtom;
    }

    private float ResolveMarkerScreenRadiusPixels(Camera camera, GameObject markerObject)
    {
        float minimumRadius = Mathf.Max(2.0f, markerClickRadiusPixelsField.val);
        if (camera == null || markerObject == null)
        {
            return minimumRadius;
        }

        Vector3 center = markerObject.transform.position;
        Vector3 edge = center + (camera.transform.right * Mathf.Max(0.001f, markerObject.transform.lossyScale.x));
        Vector3 centerScreen = camera.WorldToScreenPoint(center);
        Vector3 edgeScreen = camera.WorldToScreenPoint(edge);
        float projectedRadius = Vector2.Distance(
            new Vector2(centerScreen.x, centerScreen.y),
            new Vector2(edgeScreen.x, edgeScreen.y));
        return Mathf.Clamp(Mathf.Max(minimumRadius, projectedRadius * 1.15f), 2.0f, 120.0f);
    }

    private void SelectRadarAtom(Atom atom)
    {
        if (atom == null)
        {
            return;
        }

        if (SuperController.singleton == null || atom.mainController == null)
        {
            SetStatus("Radar click could not select: missing atom controller.");
            return;
        }

        if (selectedAtom != null && selectedAtom != atom && !string.IsNullOrEmpty(selectedUid))
        {
            lastSelectedAtom = selectedAtom;
            lastSelectedUid = selectedUid;
            lastSelectedAtTime = Time.time;
        }

        SuperController.singleton.SelectController(atom.mainController, false, false, false, true);
        selectedAtom = atom;
        selectedUid = atom.uid;
        nextSelectionPollTime = 0.0f;
        nextAtomPollTime = 0.0f;
        SetStatus("Radar selected: " + selectedUid);
    }

    private Color ResolveAvailableAtomColor(Atom atom, float alpha)
    {
#if FA_RADAR_PRO
        if (IsLightAtom(atom))
        {
            return new Color(1.0f, 0.88f, 0.30f, alpha);
        }
        if (IsCustomUnityAssetAtom(atom))
        {
            return new Color(1.0f, 0.62f, 0.24f, alpha);
        }
        if (IsPersonAtom(atom))
        {
            return new Color(0.96f, 0.42f, 0.90f, alpha);
        }

        return new Color(0.58f, 0.74f, 1.0f, alpha);
#else
        return new Color(0.62f, 0.82f, 1.0f, alpha);
#endif
    }

    private void RefreshGridMeshIfNeeded(Transform viewer)
    {
        if (gridFilter == null)
        {
            return;
        }

        float range = ResolveEffectiveRadarRangeMeters();
        float step = Mathf.Max(0.05f, gridStepMetersField.val);
        Vector2 offset = gridFollowsUserField.val ? ResolveViewerGridOffsetMeters(viewer, step) : Vector2.zero;
        bool clipCircle = gridClipCircleField.val;
        bool sameOffset = haveLastGridOffset && (offset - lastGridOffsetMeters).sqrMagnitude < 0.0001f;
        if (
            Mathf.Abs(range - lastGridRangeMeters) < 0.001f &&
            Mathf.Abs(step - lastGridStepMeters) < 0.001f &&
            sameOffset &&
            clipCircle == lastGridClipCircle)
        {
            return;
        }

        if (gridMesh != null)
        {
            Destroy(gridMesh);
            gridMesh = null;
        }

        gridMesh = CreateGridMesh(range, step, offset, clipCircle);
        gridFilter.sharedMesh = gridMesh;
        lastGridRangeMeters = range;
        lastGridStepMeters = step;
        lastGridOffsetMeters = offset;
        lastGridClipCircle = clipCircle;
        haveLastGridOffset = true;
    }

    private Vector2 ResolveViewerGridOffsetMeters(Transform viewer, float stepMeters)
    {
        if (viewer == null)
        {
            return Vector2.zero;
        }

        float safeStep = Mathf.Max(0.05f, stepMeters);
        Vector3 worldPosition = viewer.position;
        return new Vector2(
            -PositiveModulo(worldPosition.x, safeStep),
            -PositiveModulo(worldPosition.z, safeStep));
    }

    private float PositiveModulo(float value, float modulus)
    {
        float safeModulus = Mathf.Max(0.0001f, modulus);
        float result = value % safeModulus;
        if (result < 0.0f)
        {
            result += safeModulus;
        }

        return result;
    }

    private float ResolveVisualRadius()
    {
        return Mathf.Max(0.01f, radarVisualRadiusField.val);
    }

    private float ResolveFloorAreaScale()
    {
        return Mathf.Clamp(floorAreaScaleField.val, 0.25f, 6.0f);
    }

    private float ResolveEffectiveRadarRangeMeters()
    {
        return Mathf.Max(0.25f, radarRangeMetersField.val) * ResolveFloorAreaScale();
    }

    private float ResolveEffectiveHeightScaleMeters()
    {
        return Mathf.Max(0.25f, heightScaleMetersField.val) * ResolveFloorAreaScale();
    }

    private Vector3 SmoothPosition(Vector3 target, Vector3 current, ref bool hasCurrent)
    {
        if (!hasCurrent)
        {
            hasCurrent = true;
            return target;
        }

        float smoothing = Mathf.Clamp01(responseSmoothingField.val);
        float rate = Mathf.Lerp(36.0f, 8.0f, smoothing);
        float step = Mathf.Clamp01(Time.deltaTime * rate);
        return Vector3.Lerp(current, target, step);
    }

    private Transform ResolveViewerTransform()
    {
        return ResolveStableViewerTransform();
    }

    private Transform ResolveStableViewerTransform()
    {
        if (SuperController.singleton != null && SuperController.singleton.lookCamera != null)
        {
            lastGoodViewerTransform = SuperController.singleton.lookCamera.transform;
            return lastGoodViewerTransform;
        }

        if (lastGoodViewerTransform != null)
        {
            return lastGoodViewerTransform;
        }

        if (Camera.main != null)
        {
            lastGoodViewerTransform = Camera.main.transform;
            return lastGoodViewerTransform;
        }

        return null;
    }

    private Camera ResolveViewerCamera()
    {
        if (SuperController.singleton != null && SuperController.singleton.lookCamera != null)
        {
            lastGoodViewerTransform = SuperController.singleton.lookCamera.transform;
            return SuperController.singleton.lookCamera;
        }

        return Camera.main;
    }

    private Transform ResolveAtomRootTransform(Atom atom)
    {
        if (atom == null)
        {
            return null;
        }

        if (atom.mainController != null)
        {
            return atom.mainController.transform;
        }

        return atom.transform;
    }

    private void UpdateMaterials()
    {
        float emission = Mathf.Max(0.0f, emissionStrengthField.val);
        ApplyMaterialColor(shellMaterial, new Color(0.14f, 0.58f, 0.86f, Mathf.Clamp01(shellAlphaField.val)), emission * 0.55f);
        ApplyMaterialColor(ringMaterial, WithAlpha(AxisZColor, Mathf.Clamp01(ringAlphaField.val)), emission);
        ApplyMaterialColor(ringXMaterial, WithAlpha(AxisYColor, Mathf.Clamp01(ringAlphaField.val)), emission);
        ApplyMaterialColor(ringZMaterial, WithAlpha(AxisXColor, Mathf.Clamp01(ringAlphaField.val)), emission);
        ApplyMaterialColor(gridMaterial, new Color(0.48f, 0.95f, 1.0f, Mathf.Clamp01(gridAlphaField.val)), emission);
        ApplyMaterialColor(centerMaterial, new Color(0.38f, 1.0f, 0.60f, Mathf.Clamp01(markerAlphaField.val)), emission);
        ApplyMaterialColor(userHeightStemMaterial, new Color(0.38f, 1.0f, 0.60f, Mathf.Clamp01(heightStemAlphaField.val)), emission);
        ApplyMaterialColor(targetMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val)), emission);
        ApplyMaterialColor(targetHeightStemMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(heightStemAlphaField.val)), emission);
        ApplyMaterialColor(targetDropMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val) * 0.18f), emission);
        ApplyMaterialColor(lastTargetMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.26f), emission);
        ApplyMaterialColor(lastTargetDropMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.12f), emission);
        ApplyMaterialColor(availableHeightStemMaterial, new Color(0.78f, 0.88f, 1.0f, Mathf.Clamp01(heightStemAlphaField.val) * 0.72f), emission);
    }

    private void SetMaterialAlphaMultiplier(float multiplier)
    {
        multiplier = Mathf.Clamp01(multiplier);
        if (Mathf.Abs(radarMaterialAlphaMultiplier - multiplier) <= 0.0001f)
        {
            return;
        }

        radarMaterialAlphaMultiplier = multiplier;
        if (visualsReady)
        {
            UpdateMaterials();
        }
    }

    private Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private Material CreateEmissiveOverlayMaterial(string materialName, Color color, int renderQueue)
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        Material material = new Material(shader);
        material.name = materialName;
        ApplyMaterialColor(material, color, 1.0f);
        ApplyOverlayMaterialSettings(material, renderQueue);
        return material;
    }

    private Material CreateSphereShellMaterial(string materialName, Color color, int renderQueue)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Hidden/Internal-Colored");
        }
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        Material material = new Material(shader);
        material.name = materialName;
        ApplyMaterialColor(material, color, 0.45f);
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.32f);
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.0f);
        }
        ApplyOverlayMaterialSettings(material, renderQueue);
        return material;
    }

    private void ApplyMaterialColor(Material material, Color color, float emissionStrength)
    {
        if (material == null)
        {
            return;
        }

        color.a *= Mathf.Clamp01(radarMaterialAlphaMultiplier);

        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3.0f);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        material.color = color;

        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
        {
            Color emission = new Color(color.r, color.g, color.b, 1.0f) * emissionStrength;
            material.SetColor("_EmissionColor", emission);
        }
    }

    private void ApplyOverlayMaterialSettings(Material material, int renderQueue)
    {
        if (material == null)
        {
            return;
        }

        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_ZTest", (int)CompareFunction.Always);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = renderQueue;
    }

    private void ApplyRendererOverlaySettings(Renderer renderer, int renderQueue, int sortingOrder)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sortingOrder = sortingOrder;
        if (renderer.sharedMaterial != null)
        {
            ApplyOverlayMaterialSettings(renderer.sharedMaterial, renderQueue);
        }
    }

    private Mesh CreateSphereMesh(int latitudeSegments, int longitudeSegments, float radius)
    {
        int latCount = Mathf.Max(4, latitudeSegments);
        int lonCount = Mathf.Max(8, longitudeSegments);
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Prototype Sphere Mesh";

        Vector3[] vertices = new Vector3[(latCount + 1) * (lonCount + 1)];
        List<int> triangles = new List<int>();

        int index = 0;
        for (int lat = 0; lat <= latCount; lat++)
        {
            float theta = ((float)lat / (float)latCount) * Mathf.PI;
            float y = Mathf.Cos(theta) * radius;
            float ring = Mathf.Sin(theta) * radius;
            for (int lon = 0; lon <= lonCount; lon++)
            {
                float phi = ((float)lon / (float)lonCount) * Mathf.PI * 2.0f;
                vertices[index++] = new Vector3(Mathf.Cos(phi) * ring, y, Mathf.Sin(phi) * ring);
            }
        }

        for (int lat = 0; lat < latCount; lat++)
        {
            for (int lon = 0; lon < lonCount; lon++)
            {
                int a = lat * (lonCount + 1) + lon;
                int b = a + lonCount + 1;
                int c = b + 1;
                int d = a + 1;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateRingMesh(int segments, float innerRadius, float outerRadius)
    {
        int safeSegments = Mathf.Max(12, segments);
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Prototype Ring Mesh";

        Vector3[] vertices = new Vector3[safeSegments * 2];
        int[] triangles = new int[safeSegments * 12];

        for (int i = 0; i < safeSegments; i++)
        {
            float t = ((float)i / (float)safeSegments) * Mathf.PI * 2.0f;
            float sin = Mathf.Sin(t);
            float cos = Mathf.Cos(t);
            vertices[i * 2] = new Vector3(cos * outerRadius, sin * outerRadius, 0.0f);
            vertices[i * 2 + 1] = new Vector3(cos * innerRadius, sin * innerRadius, 0.0f);
        }

        int tri = 0;
        for (int i = 0; i < safeSegments; i++)
        {
            int next = (i + 1) % safeSegments;
            int outerA = i * 2;
            int innerA = outerA + 1;
            int outerB = next * 2;
            int innerB = outerB + 1;

            triangles[tri++] = outerA;
            triangles[tri++] = outerB;
            triangles[tri++] = innerB;
            triangles[tri++] = outerA;
            triangles[tri++] = innerB;
            triangles[tri++] = innerA;

            triangles[tri++] = innerB;
            triangles[tri++] = outerB;
            triangles[tri++] = outerA;
            triangles[tri++] = innerA;
            triangles[tri++] = innerB;
            triangles[tri++] = outerA;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateDesktopDiskMesh(int segments, float radius)
    {
        int safeSegments = Mathf.Max(24, segments);
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Flat Desktop Circle Mesh";

        Vector3[] vertices = new Vector3[safeSegments + 1];
        int[] triangles = new int[safeSegments * 6];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < safeSegments; i++)
        {
            float t = ((float)i / (float)safeSegments) * Mathf.PI * 2.0f;
            vertices[i + 1] = new Vector3(Mathf.Cos(t) * radius, 0.0f, Mathf.Sin(t) * radius);
        }

        int tri = 0;
        for (int i = 0; i < safeSegments; i++)
        {
            int next = ((i + 1) % safeSegments) + 1;
            int current = i + 1;

            triangles[tri++] = 0;
            triangles[tri++] = current;
            triangles[tri++] = next;

            triangles[tri++] = next;
            triangles[tri++] = current;
            triangles[tri++] = 0;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateGridMesh(float rangeMeters, float stepMeters, Vector2 offsetMeters, bool clipCircle)
    {
        float safeRange = Mathf.Max(0.5f, rangeMeters);
        float safeStep = Mathf.Max(0.05f, stepMeters);
        int stepCount = Mathf.Clamp(Mathf.CeilToInt(safeRange / safeStep) + 2, 1, 32);
        float lineHalfWidth = 0.006f;
        float gridY = 0.0f;

        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Panning Clipped Meter Grid Mesh";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = -stepCount; i <= stepCount; i++)
        {
            float xCoordinate = (((float)i * safeStep) + offsetMeters.x) / safeRange;
            AddClippedGridLine(vertices, triangles, xCoordinate, true, gridY, lineHalfWidth, clipCircle);

            float zCoordinate = (((float)i * safeStep) + offsetMeters.y) / safeRange;
            AddClippedGridLine(vertices, triangles, zCoordinate, false, gridY, lineHalfWidth, clipCircle);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void AddClippedGridLine(List<Vector3> vertices, List<int> triangles, float coordinate, bool vertical, float gridY, float width, bool clipCircle)
    {
        if (coordinate < -1.0f || coordinate > 1.0f)
        {
            return;
        }

        float extent = 1.0f;
        if (clipCircle)
        {
            extent = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - (coordinate * coordinate)));
        }

        Vector3 start;
        Vector3 end;
        if (vertical)
        {
            start = new Vector3(coordinate, gridY, -extent);
            end = new Vector3(coordinate, gridY, extent);
        }
        else
        {
            start = new Vector3(-extent, gridY, coordinate);
            end = new Vector3(extent, gridY, coordinate);
        }

        AddGridLine(vertices, triangles, start, end, width);
    }

    private void AddGridLine(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = end - start;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();
        Vector3 side = Vector3.Cross(direction, Vector3.up);
        if (side.sqrMagnitude < 0.0001f)
        {
            side = Vector3.right;
        }
        side.Normalize();
        side *= width * 0.5f;

        int index = vertices.Count;
        vertices.Add(start - side);
        vertices.Add(start + side);
        vertices.Add(end + side);
        vertices.Add(end - side);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);

        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
        triangles.Add(index);
    }

    private Mesh CreateTargetBlipMesh()
    {
        Mesh mesh = CreateSphereMesh(8, 16, 1.0f);
        mesh.name = "FA Radar Prototype Target Sphere Mesh";
        return mesh;
    }

    private Mesh CreateCenterMarkerMesh()
    {
        Mesh mesh = CreateSphereMesh(8, 16, 1.0f);
        mesh.name = "FA Radar Prototype User Center Sphere Mesh";
        return mesh;
    }

    private Mesh CreateHeightStemMesh()
    {
        float width = 0.018f;
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Height Stem Mesh";
        mesh.vertices = new Vector3[]
        {
            new Vector3(-width, -0.5f, 0.0f),
            new Vector3(width, -0.5f, 0.0f),
            new Vector3(width, 0.5f, 0.0f),
            new Vector3(-width, 0.5f, 0.0f),
            new Vector3(0.0f, -0.5f, -width),
            new Vector3(0.0f, -0.5f, width),
            new Vector3(0.0f, 0.5f, width),
            new Vector3(0.0f, 0.5f, -width)
        };
        mesh.triangles = new int[]
        {
            0, 1, 2, 0, 2, 3,
            2, 1, 0, 3, 2, 0,
            4, 5, 6, 4, 6, 7,
            6, 5, 4, 7, 6, 4
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Vector3 GetHudOffset()
    {
        return new Vector3(hudOffsetXField.val, hudOffsetYField.val, hudOffsetZField.val);
    }

    private void SetHudOffset(Vector3 offset)
    {
        hudOffsetXField.SetVal(offset.x);
        hudOffsetYField.SetVal(offset.y);
        hudOffsetZField.SetVal(offset.z);
    }

    private void SetHudOffsetNoCallback(Vector3 offset)
    {
        SetFloatNoCallback(hudOffsetXField, ClampStorableFloat(hudOffsetXField, offset.x, -1.0f, 1.0f));
        SetFloatNoCallback(hudOffsetYField, ClampStorableFloat(hudOffsetYField, offset.y, -1.0f, 1.0f));
        SetFloatNoCallback(hudOffsetZField, ClampStorableFloat(hudOffsetZField, offset.z, 0.15f, 1.5f));
    }

    private void SetHudScaleNoCallback(float scale)
    {
        SetFloatNoCallback(hudScaleField, ClampStorableFloat(hudScaleField, scale, 0.25f, 3.0f));
    }

    private void SetStaticWorldPositionNoCallback(Vector3 position)
    {
        SetFloatNoCallback(staticWorldXField, ClampStorableFloat(staticWorldXField, position.x, -20.0f, 20.0f));
        SetFloatNoCallback(staticWorldYField, ClampStorableFloat(staticWorldYField, position.y, -5.0f, 20.0f));
        SetFloatNoCallback(staticWorldZField, ClampStorableFloat(staticWorldZField, position.z, -20.0f, 20.0f));
    }

    private static float ClampStorableFloat(JSONStorableFloat field, float value, float fallbackMin, float fallbackMax)
    {
        float min = field != null ? field.min : fallbackMin;
        float max = field != null ? field.max : fallbackMax;
        return Mathf.Clamp(value, min, max);
    }

    private void CaptureHudOffsetFromAttachedAtom()
    {
        Transform viewer = ResolveViewerTransform();
        if (viewer == null)
        {
            SetStatus("Cannot capture: no look camera.");
            return;
        }

        if (containingAtom == null || containingAtom.mainController == null)
        {
            SetStatus("Capture needs the plugin loaded on a movable atom.");
            return;
        }

        Vector3 offset = viewer.InverseTransformPoint(containingAtom.mainController.transform.position);
        Transform anchor = ResolveRadarAnchorTransform(ResolveAnchorMode());
        if (anchor != null)
        {
            offset = anchor.InverseTransformPoint(containingAtom.mainController.transform.position);
        }
        SetHudOffset(offset);
        haveSmoothedHudPosition = false;
        SetStatus("Captured HUD offset from attached atom.");
    }

    private void UseSelectedAsAnchor()
    {
        Atom selected = selectedAtom;
        if (selected == null && SuperController.singleton != null)
        {
            selected = SuperController.singleton.GetSelectedAtom();
        }

        if (selected == null)
        {
            SetStatus("No selected atom to use as anchor.");
            return;
        }

        if (anchorAtomUidField != null)
        {
            anchorAtomUidField.SetVal(selected.uid ?? "");
        }
        if (anchorModeField != null)
        {
            anchorModeField.SetVal(AnchorModeAtomUid);
        }

        haveSmoothedHudPosition = false;
        SetStatus("Anchor atom set: " + (selected.uid ?? ""));
    }

    private void UseContainingAtomAnchor()
    {
        if (containingAtom == null)
        {
            SetStatus("Containing atom anchor needs the plugin loaded on an atom or CUA.");
            return;
        }

        if (anchorAtomUidField != null)
        {
            anchorAtomUidField.SetVal(containingAtom.uid ?? "");
        }
        if (anchorModeField != null)
        {
            anchorModeField.SetVal(AnchorModeContainingAtom);
        }

        haveSmoothedHudPosition = false;
        SetStatus("Using containing atom anchor.");
    }

    private void CaptureStaticFromCurrentView()
    {
        Transform viewer = ResolveViewerTransform();
        Vector3 position;
        Quaternion rotation;
        if (hudRoot != null)
        {
            position = hudRoot.transform.position;
            rotation = hudRoot.transform.rotation;
        }
        else if (viewer != null)
        {
            position = viewer.TransformPoint(GetHudOffset());
            rotation = viewer.rotation;
        }
        else
        {
            SetStatus("Cannot capture static anchor: no current view.");
            return;
        }

        SetStaticWorldPose(position, rotation);
        if (anchorModeField != null)
        {
            anchorModeField.SetVal(AnchorModeWorldStatic);
        }

        haveSmoothedHudPosition = false;
        SetStatus("Captured static world anchor.");
    }

    private void SetStaticWorldPose(Vector3 position, Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        if (staticWorldXField != null)
        {
            staticWorldXField.SetVal(position.x);
        }
        if (staticWorldYField != null)
        {
            staticWorldYField.SetVal(position.y);
        }
        if (staticWorldZField != null)
        {
            staticWorldZField.SetVal(position.z);
        }
        if (staticWorldPitchField != null)
        {
            staticWorldPitchField.SetVal(NormalizeEulerDegrees(euler.x));
        }
        if (staticWorldYawField != null)
        {
            staticWorldYawField.SetVal(NormalizeEulerDegrees(euler.y));
        }
        if (staticWorldRollField != null)
        {
            staticWorldRollField.SetVal(NormalizeEulerDegrees(euler.z));
        }
    }

    private static float NormalizeEulerDegrees(float degrees)
    {
        float normalized = degrees;
        while (normalized > 180.0f)
        {
            normalized -= 360.0f;
        }
        while (normalized < -180.0f)
        {
            normalized += 360.0f;
        }

        return normalized;
    }

    private void ResetHudOffset()
    {
        SetHudOffset(new Vector3(-0.59f, 0.22f, 0.78f));
        haveSmoothedHudPosition = false;
        SetStatus("HUD offset reset.");
    }

    private void SetActiveIfChanged(GameObject target, bool active)
    {
        if (target == null)
        {
            return;
        }

        if (target.activeSelf == active)
        {
            return;
        }

        target.SetActive(active);
    }

    private void SetStatus(string message)
    {
        if (statusField != null && statusField.val != message)
        {
            statusField.SetVal(message);
        }
    }

    private void DestroyRuntimeVisuals()
    {
        DestroyOwnedObject(hudRoot);
        DestroyOwnedObject(resizeGuideLineObject);
        hudRoot = null;
        radarRoot = null;
        axisRoot = null;
        flatCircleObject = null;
        sphereObject = null;
        gridObject = null;
        centerMarkerObject = null;
        userHeightStemObject = null;
        targetBlipObject = null;
        targetHeightStemObject = null;
        targetGridDropObject = null;
        lastTargetBlipObject = null;
        lastTargetGridDropObject = null;
        resizeGuideLineObject = null;
        availableMarkerObjects = null;
        availableStemObjects = null;
        ringObjects = null;
        ringBaseRotations = null;
        gridFilter = null;
        currentHudAnchor = null;
        lastGoodViewerTransform = null;

        DestroyOwnedObject(shellMaterial);
        DestroyOwnedObject(ringMaterial);
        DestroyOwnedObject(ringXMaterial);
        DestroyOwnedObject(ringZMaterial);
        DestroyOwnedObject(gridMaterial);
        DestroyOwnedObject(centerMaterial);
        DestroyOwnedObject(userHeightStemMaterial);
        DestroyOwnedObject(targetMaterial);
        DestroyOwnedObject(targetHeightStemMaterial);
        DestroyOwnedObject(targetDropMaterial);
        DestroyOwnedObject(lastTargetMaterial);
        DestroyOwnedObject(lastTargetDropMaterial);
        DestroyOwnedObject(availableHeightStemMaterial);
        DestroyOwnedObject(grabGuideMaterial);
        if (availableMarkerMaterials != null)
        {
            for (int i = 0; i < availableMarkerMaterials.Length; i++)
            {
                DestroyOwnedObject(availableMarkerMaterials[i]);
            }
        }
        shellMaterial = null;
        ringMaterial = null;
        ringXMaterial = null;
        ringZMaterial = null;
        gridMaterial = null;
        centerMaterial = null;
        userHeightStemMaterial = null;
        targetMaterial = null;
        targetHeightStemMaterial = null;
        targetDropMaterial = null;
        lastTargetMaterial = null;
        lastTargetDropMaterial = null;
        availableHeightStemMaterial = null;
        grabGuideMaterial = null;
        availableMarkerMaterials = null;

        DestroyOwnedObject(sphereMesh);
        DestroyOwnedObject(flatCircleMesh);
        DestroyOwnedObject(ringMesh);
        DestroyOwnedObject(gridMesh);
        DestroyOwnedObject(targetBlipMesh);
        DestroyOwnedObject(centerMarkerMesh);
        DestroyOwnedObject(heightStemMesh);
        DestroyOwnedObject(resizeGuideLineMesh);
        sphereMesh = null;
        flatCircleMesh = null;
        ringMesh = null;
        gridMesh = null;
        targetBlipMesh = null;
        centerMarkerMesh = null;
        heightStemMesh = null;
        resizeGuideLineMesh = null;
        trackedAvailableAtoms.Clear();

        visualsReady = false;
    }

    private void DestroyOwnedObject(UnityEngine.Object target)
    {
        if (target != null)
        {
            Destroy(target);
        }
    }
}
