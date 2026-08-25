using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class FrameAngelRadar : MVRScript
{
    private const string Version = "0.1.53";
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
    private const string DesktopPlacementAttachedToUi = "Attached To UI";
    private const string DesktopPlacementPinnedInWorld = "Pinned In World";
    private const string DesktopVisibilityRecoveryVersion = "desktop_visibility_recovery_v1";
    private const string PluginHostSurfaceEmptyAnchor = "Empty / Atom Anchor";
    private const string PluginHostSurfaceSceneSession = "Scene / Session Plugin";
    private const string DisplaySurfaceDesktop = "Desktop";
    private const string DisplaySurfaceVR = "VR";
    private const string CommonMarkerDefaultsVersion = "target_markers_visible_fade_v2";
    private const string ProFilterDefaultsVersion = "visible_defaults_v6";
    private const string LightAlphaDefaultsVersion = "split_alpha_v2";
    private const string VisualDepthDefaultsVersion = "depth_clarity_v1";
    private const string DirectorReadabilityDefaultsVersion = "director_readability_v1";
    private const string LabelReadabilityDefaultsVersion = "label_callouts_v1";
    private const string RadarModeHud = "HUD";
    private const string RadarModeWorld = "world";
    private const string RadarModeWristLeft = "wrist-left";
    private const string RadarModeWristRight = "wrist-right";
    private const string RadarModeWristLeftAlwaysOn = "wrist-left-always-on";
    private const string RadarModeWristRightAlwaysOn = "wrist-right-always-on";
    private const string RadarModePalmLeft = "palm-left";
    private const string RadarModePalmRight = "palm-right";
    private const string TrackedHandRuntimeRoot = "FAARTrackedHandArmColliders";
    private const string TrackedHandStateSchema = "faar.tracked-hand-state.v7";
    private const string LeftPalmSegmentName = "Segment_0";
    private const string RightPalmSegmentName = "Segment_27";
    private const string GrabHandlePrimarySuffix = "primary";
    private const string GrabHandleResizeSuffix = "resize";
    private const float GlobalPreferencesFlushDelaySeconds = 0.75f;
    private const float GlobalPreferencesSharedStatePollIntervalSeconds = 1.0f;
    private const float RecorderVisibilityPollIntervalSeconds = 0.25f;
    private const float RadarVisibilityFadeSeconds = 0.18f;
    private const float WristRevealGraceSeconds = 0.55f;
    private const float WristHandOffDistanceMeters = 0.61f;
    private const float WristHandOffMinimumTravelMeters = 0.30f;
    private const float HudHandOffDistanceMeters = 0.38f;
    private const float HudDetachToWristDistanceMeters = 0.55f;
    private const float GrabResizeMinimumStartDistanceMeters = 0.05f;
    private const float AccordionResizeMinimumStartDistanceMeters = 0.08f;
    private const float GrabHapticCooldownSeconds = 0.08f;
    private const float GripGrabPressThreshold = 0.62f;
    private const float GripGrabReleaseThreshold = 0.34f;
#if FA_RADAR_PRO
    private const float GrabThrowMinimumReleaseVelocity = 0.18f;
    private const float GrabThrowStopVelocity = 0.04f;
    private const float GrabThrowScaleSeconds = 0.45f;
    private const float GrabThrowMaxSeconds = 2.5f;
    private const float GrabThrowSurfaceInsetMeters = 0.03f;
#endif
    private const float DefaultRadarVisualRadiusMeters = 0.08f;
    private const float MaxRadarVisualDiameterMeters = 1.0f;
    private const float MaxRadarPlacementScale = MaxRadarVisualDiameterMeters / (DefaultRadarVisualRadiusMeters * 2.0f);
    private const float MinHudPlacementScale = 0.05f;
    private const float MaxHudPlacementScale = 1.25f;
    private const float FarMarkerOuterRadius = 1.75f;
    private const float SelectedTargetOuterRadius = 3.5f;
    private const float FarMarkerMinimumAlpha = 0.08f;
    private const float FineGridStepMeters = 1.0f;
    private const float CoarseGridStepMeters = 10.0f;
    private const float CoarseGridRangeThresholdMeters = 12.0f;
    private const float RadarRangeScrollZoomStep = 1.12f;
    private const float RadarRangeScrollStatusIntervalSeconds = 0.18f;
    private const float RadarHoverScreenPaddingPixels = 12.0f;
#if FA_RADAR_PRO
    private const int MaxDirectorBackgroundOverlayBudget = 10;
    private const float DirectorBackgroundOverlayAlphaCeiling = 0.42f;
    private const int RotationAxisObjectCount = 4;
    private const int RotationAxisVisualPieceCount = 7;
    private const int RotationAxisCenterObjectIndex = 3;
    private const int MaxRadarLabelLimit = 64;
    private const float DefaultLabelLimit = 4.0f;
    private const float DefaultLabelScale = 0.045f;
    private const float DefaultLabelAlpha = 0.58f;
    private const string LabelsOff = "Off";
    private const string LabelsSelected = "Selected";
    private const string LabelsSelectedAndNearest = "Selected + Nearest";
    private const string LabelOrientationFaceViewer = "Face Viewer";
    private const string LabelOrientationWorldAxis = "World Axis";
    private const string LabelOrientationObjectRotation = "Object Rotation";
#endif
    private const float DefaultAtomAnchorOffsetZ = 0.15f;
    private const float DefaultAtomAnchorScale = 0.75f;
    private const float HeightStemHalfWidth = 0.010f;
    private const int GrabHandUnknown = -1;
    private const int GrabHandLeft = 0;
    private const int GrabHandRight = 1;
    private const int AvailableMarkerPoolBlockSize = 8;
    private const float AvailableMarkerFrameMoveThresholdMeters = 0.0025f;
    private const float AvailableMarkerFrameRotateThresholdDegrees = 0.25f;
    private const float AvailableMarkerTransformMoveThresholdMeters = 0.0025f;
    private const float AvailableMarkerStatusIntervalSeconds = 0.25f;
    private const int AtomCategoryLight = 1 << 0;
    private const int AtomCategoryCua = 1 << 1;
    private const int AtomCategoryPerson = 1 << 2;
    private const int AtomCategoryFemale = 1 << 3;
    private const int AtomCategoryMale = 1 << 4;
    private const int AtomCategoryEmpty = 1 << 5;
    private const int AtomCategorySubScene = 1 << 6;
    private const int AtomCategoryImagePanel = 1 << 7;
    private const int AtomCategoryAnimation = 1 << 8;
    private const int AtomCategoryForce = 1 << 9;
    private const int AtomCategoryShape = 1 << 10;
    private const int AtomCategorySound = 1 << 11;
    private const int AtomCategoryTrigger = 1 << 12;
    private const int AtomCategoryNavigationPanel = 1 << 13;
    private const int AtomCategoryCamera = 1 << 14;
    private static readonly Color AxisXColor = new Color(1.0f, 0.18f, 0.12f, 1.0f);
    private static readonly Color AxisYColor = new Color(0.22f, 1.0f, 0.34f, 1.0f);
    private static readonly Color AxisZColor = new Color(0.26f, 0.52f, 1.0f, 1.0f);
    private static readonly Color FreeAtomMarkerColor = new Color(1.0f, 0.78f, 0.18f, 1.0f);
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

    private struct RadarFrame
    {
        public Transform viewer;
        public Vector3 referencePosition;
        public Quaternion referenceRotation;
        public Quaternion inverseReferenceRotation;
        public float rangeMeters;
        public float heightScaleMeters;
        public float visualRadius;
        public bool flattenY;
        public int signature;
    }

    private sealed class AtomRecord
    {
        public int recordId;
        public Atom atom;
        public string uid;
        public Transform root;
        public Vector3 markerLocalOffset;
        public bool markerLocalOffsetKnown;
        public Vector3 markerWorldPosition;
        public Vector3 lastRootPosition;
        public Vector3 lastRootScale;
        public Quaternion lastRootRotation;
        public bool hasTransformSample;
        public float distanceSq;
        public int categoryFlags;
        public Mesh markerMesh;
#if FA_RADAR_PRO
        public string labelText;
        public Light light;
        public bool hasLight;
        public bool lightResolved;
#endif
    }

    private sealed class MarkerSlot
    {
        public GameObject markerObject;
        public GameObject stemObject;
        public Material markerMaterial;
        public MeshFilter markerFilter;
        public int recordId = -1;
        public Mesh markerMesh;
#if FA_RADAR_PRO
        public GameObject[] rotationAxisObjects;
        public GameObject lightRangeObject;
        public GameObject spotlightConeObject;
        public Material lightRangeMaterial;
        public Material spotlightConeMaterial;
        public GameObject labelObject;
        public GameObject labelLeaderObject;
        public MeshFilter labelFilter;
        public Material labelMaterial;
        public Mesh labelMesh;
        public string labelText;
#endif
    }

    [Serializable]
    private sealed class TrackedHandRuntimeState
    {
        public string schema = "";
        public bool leftTracking = false;
        public bool rightTracking = false;
        public bool leftIndexPinched = false;
        public bool rightIndexPinched = false;
        public bool leftMiddlePinched = false;
        public bool rightMiddlePinched = false;
        public bool leftHoldGrabLatched = false;
        public bool rightHoldGrabLatched = false;
        public bool leftPalmPresented = false;
        public bool rightPalmPresented = false;
    }

    private struct CachedMaterialState
    {
        public Color color;
        public float emissionStrength;
        public bool known;
    }

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
    private JSONStorableBool showEmptyAtomsField;
    private JSONStorableBool showSubSceneAtomsField;
    private JSONStorableBool showImagePanelAtomsField;
    private JSONStorableBool showAnimationAtomsField;
    private JSONStorableBool showForceAtomsField;
    private JSONStorableBool showShapeAtomsField;
    private JSONStorableBool showSoundAtomsField;
    private JSONStorableBool showTriggerAtomsField;
    private JSONStorableBool showNavigationPanelAtomsField;
    private JSONStorableBool showCameraAtomsField;
    private JSONStorableBool showOtherAtomsField;
#if FA_RADAR_PRO
    private JSONStorableBool showRotationAxesField;
    private JSONStorableBool showLightRangeVolumesField;
    private JSONStorableBool showSpotlightConesField;
    private JSONStorableBool showUserPovFrustumField;
    private JSONStorableBool showDesktopPovFrustumField;
    private JSONStorableBool showSceneCameraFrustumsField;
    private JSONStorableBool grabThrowPinEnabledField;
    private JSONStorableBool grabThrowSurfaceStopField;
    private JSONStorableBool grabThrowPinnedField;
    private JSONStorableStringChooser sceneLabelsField;
    private JSONStorableStringChooser labelOrientationField;
#endif
    private JSONStorableBool clickSelectMarkersField;
    private JSONStorableBool grabHandlesEnabledField;
    private JSONStorableBool grabHandleDebugVisibleField;
    private JSONStorableBool grabHapticsEnabledField;
    private JSONStorableBool globalPrefsAutoSaveField;
    private JSONStorableBool cuaAnchorPresetField;
    private JSONStorableBool roomCompassField;

    private JSONStorableFloat hudOffsetXField;
    private JSONStorableFloat hudOffsetYField;
    private JSONStorableFloat hudOffsetZField;
    private JSONStorableFloat hudScaleField;
    private JSONStorableFloat wristOffsetXField;
    private JSONStorableFloat wristOffsetYField;
    private JSONStorableFloat wristOffsetZField;
    private JSONStorableFloat wristScaleField;
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
    private JSONStorableFloat maxVisibleMarkersField;
    private JSONStorableFloat markerClickRadiusPixelsField;
#if FA_RADAR_PRO
    private JSONStorableFloat rotationAxisLengthField;
    private JSONStorableFloat rotationAxisWidthField;
    private JSONStorableFloat lightVolumeAlphaField;
    private JSONStorableFloat pointLightRangeAlphaField;
    private JSONStorableFloat spotlightConeAlphaField;
    private JSONStorableFloat lightVolumeScaleField;
    private JSONStorableFloat lightMarkerScaleField;
    private JSONStorableFloat richOverlayBudgetField;
    private JSONStorableFloat povFrustumLengthField;
    private JSONStorableFloat povFrustumAlphaField;
    private JSONStorableFloat grabThrowGrowScaleField;
    private JSONStorableFloat grabThrowVelocityScaleField;
    private JSONStorableFloat grabThrowDecelerationField;
    private JSONStorableFloat grabThrowReturnScaleField;
    private JSONStorableFloat labelLimitField;
    private JSONStorableFloat labelScaleField;
    private JSONStorableFloat labelAlphaField;
#endif
    private JSONStorableFloat grabHitRadiusMetersField;
    private JSONStorableFloat wristTwistDegreesField;
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
    private JSONStorableString hostSurfaceField;
    private JSONStorableString displaySurfaceField;
    private JSONStorableString anchorAtomUidField;
    private JSONStorableStringChooser anchorModeField;
    private JSONStorableStringChooser radarModeField;
    private JSONStorableStringChooser desktopPlacementField;
    private JSONStorableStringChooser vrPlacementField;

    private GameObject hudRoot;
    private GameObject radarRoot;
    private GameObject axisRoot;
    private GameObject flatCircleObject;
    private GameObject sphereObject;
    private GameObject gridObject;
    private GameObject centerMarkerObject;
    private GameObject userHeightStemObject;
    private GameObject targetBlipObject;
    private GameObject[] selectedTargetRingObjects;
    private GameObject selectedViewCueObject;
    private GameObject targetHeightStemObject;
    private GameObject targetGridDropObject;
    private GameObject lastTargetBlipObject;
    private GameObject lastTargetGridDropObject;
    private GameObject resizeGuideLineObject;
#if FA_RADAR_PRO
    private GameObject[] targetRotationAxisObjects;
    private GameObject targetLabelObject;
    private GameObject targetLabelLeaderObject;
    private GameObject targetLightRangeObject;
    private GameObject targetSpotlightConeObject;
    private GameObject userPovFrustumObject;
    private GameObject desktopPovFrustumObject;
    private GameObject[] sceneCameraFrustumObjects;
    private GameObject[] availableRotationAxisObjects;
    private GameObject[] availableLightRangeObjects;
    private GameObject[] availableSpotlightConeObjects;
#endif
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
#if FA_RADAR_PRO
    private Mesh personMarkerMesh;
    private Mesh panelMarkerMesh;
    private Mesh subSceneMarkerMesh;
#endif
    private Mesh centerMarkerMesh;
    private Mesh heightStemMesh;
    private Mesh resizeGuideLineMesh;
#if FA_RADAR_PRO
    private Mesh rotationAxisHalfPairMesh;
    private Mesh rotationAxisCenterCubeMesh;
    private Mesh targetLabelMesh;
    private Mesh labelLeaderMesh;
    private Mesh lightVolumeSphereMesh;
    private Mesh spotlightConeMesh;
    private Mesh povFrustumMesh;
#endif

    private Material shellMaterial;
    private Material ringMaterial;
    private Material ringXMaterial;
    private Material ringZMaterial;
    private Material gridMaterial;
    private Material centerMaterial;
    private Material userHeightStemMaterial;
    private Material targetMaterial;
    private Material selectedTargetRingXMaterial;
    private Material selectedTargetRingYMaterial;
    private Material selectedTargetRingZMaterial;
    private Material selectedViewCueMaterial;
    private Material targetHeightStemMaterial;
    private Material targetDropMaterial;
    private Material lastTargetMaterial;
    private Material lastTargetDropMaterial;
    private Material availableHeightStemMaterial;
    private Material grabGuideMaterial;
#if FA_RADAR_PRO
    private Material rotationAxisXMaterial;
    private Material rotationAxisYMaterial;
    private Material rotationAxisZMaterial;
    private Material rotationAxisCenterMaterial;
    private Material targetLabelMaterial;
    private Material targetLightRangeMaterial;
    private Material targetSpotlightConeMaterial;
    private Material userPovFrustumMaterial;
    private Material desktopPovFrustumMaterial;
    private Material sceneCameraFrustumMaterial;
    private Material[] availableLightRangeMaterials;
    private Material[] availableSpotlightConeMaterials;
#endif
    private Material[] availableMarkerMaterials;

    private Atom primaryGrabHandleAtom;
    private Atom resizeGrabHandleAtom;
    private Atom selectedAtom;
    private Atom lastSelectedAtom;
    private List<Atom> trackedAvailableAtoms = new List<Atom>();
    private List<AtomRecord> availableAtomRecords = new List<AtomRecord>();
    private MarkerSlot[] availableMarkerSlots;
    private Dictionary<Material, CachedMaterialState> materialStateByMaterial = new Dictionary<Material, CachedMaterialState>();
    private AtomRecord selectedAtomRecord;
    private Atom cachedAnchorAtom;
    private int lastAvailableAtomSceneCount;
    private int lastAvailableAtomTrackedCount;
    private int lastAvailableAtomVisibleCount;
    private int lastAvailableAtomRangeHiddenCount;
    private int lastAvailableAtomMissingTargetCount;
    private int lastAvailableAtomBudgetHiddenCount;
    private string selectedUid = "";
    private string lastSelectedUid = "";
    private string cachedAnchorAtomUid = "";
#if FA_RADAR_PRO
    private string targetLabelText = "";
#endif
    private float nextSelectionPollTime;
    private float nextAtomPollTime;
    private float nextAvailableMarkerStatusTime;
    private float lastSelectedAtTime = -1000.0f;
    private float nextSelectedStatusTime;
    private float nextRangeScrollStatusTime;
    private float lastGridRangeMeters = -1.0f;
    private float lastGridStepMeters = -1.0f;
    private Vector2 lastGridOffsetMeters;
    private bool lastGridClipCircle;
    private bool haveLastGridOffset;
    private bool visualsReady;
    private bool haveSmoothedHudPosition;
    private bool globalPreferencesLoading;
    private bool globalPreferencesDirty;
    private bool globalPreferencesWriteAfterApply;
    private bool materialsDirty = true;
    private bool availableMarkersDirty = true;
    private bool recorderRadarVisible = true;
    private bool lastAppliedRecorderRadarVisible = true;
    private bool cuaAnchorPresetApplied;
    private bool wristCompassRevealed;
    private bool radarVisibilityAlphaInitialized;
    private bool primaryGrabHandleCreatePending;
    private bool resizeGrabHandleCreatePending;
    private bool moveGrabActive;
    private bool resizeGrabActive;
    private bool moveGrabUsesGripFallback;
    private bool resizeGrabUsesGripFallback;
    private bool resizeHandleDismissedUntilMoveRelease;
    private bool ovrLeftGrabHapticActive;
    private bool ovrRightGrabHapticActive;
#if FA_RADAR_PRO
    private bool grabThrowActive;
    private bool grabThrowUsesControllerHaptics;
#endif
    private Vector3 smoothedHudPosition;
    private Vector3 moveStartHandlePosition;
    private Vector3 moveGrabStartRadarWorldCenter;
    private Vector3 moveGrabCurrentRadarWorldCenter;
    private Quaternion moveGrabStartRadarWorldRotation = Quaternion.identity;
    private Vector3 moveStartHudOffset;
    private Vector3 moveStartWristOffset;
    private Vector3 moveStartStaticPosition;
#if FA_RADAR_PRO
    private Vector3 moveGrabPreviousControllerPosition;
    private Vector3 moveGrabReleaseVelocity;
    private Vector3 grabThrowPosition;
    private Vector3 grabThrowStartPosition;
    private Vector3 grabThrowVelocity;
#endif
    private Vector3 resizeStartPrimaryPosition;
    private Vector3 resizeStartHandlePosition;
    private float nextGlobalPreferencesFlushTime;
    private float nextGlobalPreferencesPollTime;
    private float nextRecorderVisibilityPollTime;
    private float radarVisibilityAlpha = 1.0f;
    private float radarVisibilityTargetAlpha = 1.0f;
    private float resizeStartScale;
    private float resizeStartDistance;
    private float accordionResizeStartScale;
    private float accordionResizeStartDistance;
    private float wristRevealGraceUntil;
    private float leftGripValue;
    private float rightGripValue;
    private float previousLeftGripValue;
    private float previousRightGripValue;
    private float lastGrabHandleHapticAt;
    private float ovrLeftGrabHapticStopAt;
    private float ovrRightGrabHapticStopAt;
    private float radarMaterialAlphaMultiplier = 1.0f;
#if FA_RADAR_PRO
    private float moveGrabPreviousSampleTime;
    private float grabThrowStartedAt;
    private float grabThrowStartScale;
    private float grabThrowTargetScale;
#endif
    private int moveGrabHand = GrabHandUnknown;
    private int resizeGrabHand = GrabHandUnknown;
    private bool moveGrabWorldOverrideActive;
    private bool accordionResizeActive;
    private bool accordionResizeUsesHandFallback;
    private string lastAppliedCommonPreferencesJson = "";
    private string lastAppliedProPreferencesJson = "";
    private string primaryGrabHandleUid = "";
    private string resizeGrabHandleUid = "";
    private Transform currentHudAnchor;
    private GameObject trackedHandRuntimeRoot;
    private readonly Transform[] trackedPalmAnchors = new Transform[2];
    private readonly bool[] trackedHandsLive = new bool[2];
    private readonly bool[] trackedPalmsPresented = new bool[2];
    private readonly bool[] trackedIndexPinched = new bool[2];
    private readonly bool[] trackedMiddlePinched = new bool[2];
    private readonly bool[] trackedHoldGrabLatched = new bool[2];
    private Transform lastGoodViewerTransform;
    private int availableAtomRevision;
    private int lastAvailableMarkerFrameSignature = int.MinValue;

    public override void Init()
    {
        BuildStorables();
        LoadGlobalPreferences();
        BuildUi();
        EnsureRuntimeVisuals();
        ConnectTrackedHandRuntime();
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
        DisconnectTrackedHandRuntime();
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
        availableAtomMarkersEnabledField = new JSONStorableBool("Scene Atom Markers", true);
        showLightAtomsField = new JSONStorableBool("Lights", true);
        showCustomUnityAssetAtomsField = new JSONStorableBool("Custom Unity Assets", true);
        showPersonAtomsField = new JSONStorableBool("People", true);
        showEmptyAtomsField = new JSONStorableBool("Empty", true);
        showSubSceneAtomsField = new JSONStorableBool("SubScene", true);
        showImagePanelAtomsField = new JSONStorableBool("ImagePanel", true);
        showAnimationAtomsField = new JSONStorableBool("Animation", true);
        showForceAtomsField = new JSONStorableBool("Force", true);
        showShapeAtomsField = new JSONStorableBool("Shapes", true);
        showSoundAtomsField = new JSONStorableBool("Sounds", true);
        showTriggerAtomsField = new JSONStorableBool("Triggers", true);
        showNavigationPanelAtomsField = new JSONStorableBool("Player Navigation Panel", false);
        showCameraAtomsField = new JSONStorableBool("Cameras", true);
        showOtherAtomsField = new JSONStorableBool("Uncategorized Atoms", true);
#if FA_RADAR_PRO
        showRotationAxesField = new JSONStorableBool("Rotation Axes", true);
        showLightRangeVolumesField = new JSONStorableBool("Light Range Volumes", true);
        showSpotlightConesField = new JSONStorableBool("Spotlight Cones", true);
        showUserPovFrustumField = new JSONStorableBool("User POV Frustum", true);
        showDesktopPovFrustumField = new JSONStorableBool("Desktop POV Frustum", true);
        showSceneCameraFrustumsField = new JSONStorableBool("Scene Camera Frustums", true);
        grabThrowPinEnabledField = new JSONStorableBool("Throw Pin On Release", false);
        grabThrowSurfaceStopField = new JSONStorableBool("Throw Surface Stop", true);
        grabThrowPinnedField = new JSONStorableBool("Throw Pinned State", false);
        sceneLabelsField = new JSONStorableStringChooser(
            "Scene Labels",
            new List<string> { LabelsOff, LabelsSelected, LabelsSelectedAndNearest },
            LabelsSelected,
            "Scene Labels");
        sceneLabelsField.displayChoices = new List<string> { LabelsOff, LabelsSelected, LabelsSelectedAndNearest };
        labelOrientationField = new JSONStorableStringChooser(
            "Label Orientation",
            new List<string> { LabelOrientationFaceViewer, LabelOrientationWorldAxis, LabelOrientationObjectRotation },
            LabelOrientationFaceViewer,
            "Label Orientation");
        labelOrientationField.displayChoices = new List<string> { LabelOrientationFaceViewer, LabelOrientationWorldAxis, LabelOrientationObjectRotation };
#endif
        clickSelectMarkersField = new JSONStorableBool("Click Select Markers", true);
        // Session Grab Handles: Direct Grip Grab replaces the old visible-handle behavior.
        // Grip Grab Fallback is now active: grip near the radar, track controller movement, release to apply placement.
        grabHandlesEnabledField = new JSONStorableBool("Grab Handles Enabled", true);
        grabHandleDebugVisibleField = new JSONStorableBool("Show Grab Handle Debug", false);
        grabHapticsEnabledField = new JSONStorableBool("Grab Haptics", true);
        globalPrefsAutoSaveField = new JSONStorableBool("Global Prefs Auto Save", true);
        cuaAnchorPresetField = new JSONStorableBool("CUA Anchor Preset", false);
        roomCompassField = new JSONStorableBool("Room Compass", false);
        anchorModeField = new JSONStorableStringChooser(
            "Anchor Mode",
            new List<string> { AnchorModeHud, AnchorModeWorldStatic, AnchorModeContainingAtom, AnchorModeAtomUid },
            AnchorModeHud,
            "Anchor Mode");
        anchorModeField.displayChoices = new List<string> { "HUD / View", "World Static", "Containing Atom", "Anchor Atom UID" };
        radarModeField = new JSONStorableStringChooser(
            "Radar Mode",
            new List<string>
            {
                RadarModeHud,
                RadarModeWorld,
                RadarModeWristLeft,
                RadarModeWristRight,
                RadarModeWristLeftAlwaysOn,
                RadarModeWristRightAlwaysOn,
                RadarModePalmLeft,
                RadarModePalmRight
            },
            RadarModeHud,
            "Radar Mode");
        radarModeField.displayChoices = new List<string>
        {
            RadarModeHud,
            "World",
            "Left Wrist",
            "Right Wrist",
            "Left Wrist - Always On",
            "Right Wrist - Always On",
            "Palm - Left",
            "Palm - Right"
        };
        desktopPlacementField = new JSONStorableStringChooser(
            "Desktop Placement",
            new List<string> { DesktopPlacementAttachedToUi, DesktopPlacementPinnedInWorld },
            ResolveDefaultDesktopPlacement(),
            "Desktop Placement");
        desktopPlacementField.displayChoices = new List<string>
        {
            DesktopPlacementAttachedToUi,
            DesktopPlacementPinnedInWorld
        };
        vrPlacementField = new JSONStorableStringChooser(
            "VR Placement",
            new List<string> { DesktopPlacementAttachedToUi, DesktopPlacementPinnedInWorld },
            ResolveDefaultVRPlacement(),
            "VR Placement");
        vrPlacementField.displayChoices = new List<string>
        {
            DesktopPlacementAttachedToUi,
            DesktopPlacementPinnedInWorld
        };

        hudOffsetXField = new JSONStorableFloat("HUD Offset X", -0.59f, -1.0f, 1.0f, true, true);
        hudOffsetYField = new JSONStorableFloat("HUD Offset Y", 0.22f, -1.0f, 1.0f, true, true);
        hudOffsetZField = new JSONStorableFloat("HUD Offset Z", 0.78f, 0.15f, 1.5f, true, true);
        hudScaleField = new JSONStorableFloat("HUD Scale", 0.49f, MinHudPlacementScale, MaxHudPlacementScale, true, true);
        wristOffsetXField = new JSONStorableFloat("Wrist Offset X", 0.0f, -0.5f, 0.5f, true, true);
        wristOffsetYField = new JSONStorableFloat("Wrist Offset Y", 0.08f, -0.5f, 0.5f, true, true);
        wristOffsetZField = new JSONStorableFloat("Wrist Offset Z", 0.12f, -0.5f, 0.5f, true, true);
        wristScaleField = new JSONStorableFloat("Wrist Scale", 0.35f, 0.05f, MaxRadarPlacementScale, true, true);
        desktopTiltDegreesField = new JSONStorableFloat("Desktop Tilt Degrees", 90.0f, 0.0f, 90.0f, true, true);
        radarRangeMetersField = new JSONStorableFloat("Radar Range Meters", 5.0f, 0.5f, 30.0f, true, true);
        floorAreaScaleField = new JSONStorableFloat("Floor Area Scale", 1.0f, 0.25f, 6.0f, true, true);
        radarVisualRadiusField = new JSONStorableFloat("Radar Visual Radius", DefaultRadarVisualRadiusMeters, 0.025f, 0.25f, true, true);
        gridStepMetersField = new JSONStorableFloat("Grid Step Meters", FineGridStepMeters, 0.25f, CoarseGridStepMeters, true, true);
        shellAlphaField = new JSONStorableFloat("Sphere Alpha", 0.055f, 0.0f, 0.45f, true, true);
        ringAlphaField = new JSONStorableFloat("Ring Alpha", 0.30f, 0.02f, 0.9f, true, true);
        gridAlphaField = new JSONStorableFloat("Grid Alpha", 0.11f, 0.0f, 0.5f, true, true);
        markerAlphaField = new JSONStorableFloat("Marker Alpha", 0.9f, 0.1f, 1.0f, true, true);
        emissionStrengthField = new JSONStorableFloat("Emission Strength", 1.4f, 0.0f, 4.0f, true, true);
        ringRotationSpeedField = new JSONStorableFloat("Ring Rotation Speed", 0.0f, 0.0f, 90.0f, true, true);
        targetMarkerScaleField = new JSONStorableFloat("Target Marker Scale", 0.09f, 0.025f, 0.25f, true, true);
        lastSelectedFadeSecondsField = new JSONStorableFloat("Last Selected Fade Seconds", 12.0f, 1.0f, 60.0f, true, true);
        heightScaleMetersField = new JSONStorableFloat("Height Scale Meters", 6.0f, 1.0f, 20.0f, true, true);
        heightStemAlphaField = new JSONStorableFloat("Height Stem Alpha", 0.26f, 0.0f, 1.0f, true, true);
        rangeFadeMetersField = new JSONStorableFloat("Range Fade Meters", 10.0f, 0.0f, 50.0f, true, true);
        depthSizeStrengthField = new JSONStorableFloat("Depth Cue Strength", 0.55f, 0.0f, 1.0f, true, true);
        atomPollSecondsField = new JSONStorableFloat("Atom Poll Seconds", 0.75f, 0.15f, 5.0f, true, true);
        availableAtomAlphaField = new JSONStorableFloat("Available Atom Alpha", 0.34f, 0.0f, 1.0f, true, true);
        maxVisibleMarkersField = new JSONStorableFloat("Max Visible Markers", 192.0f, 8.0f, 512.0f, true, true);
        markerClickRadiusPixelsField = new JSONStorableFloat("Marker Click Radius Pixels", 20.0f, 4.0f, 80.0f, true, true);
#if FA_RADAR_PRO
        rotationAxisLengthField = new JSONStorableFloat("Rotation Axis Length", 0.085f, 0.03f, 0.75f, true, true);
        rotationAxisWidthField = new JSONStorableFloat("Rotation Axis Width", 0.0045f, 0.003f, 0.05f, true, true);
        lightVolumeAlphaField = new JSONStorableFloat("Light Volume Alpha", 0.045f, 0.0f, 0.6f, true, true);
        pointLightRangeAlphaField = new JSONStorableFloat("Point Light Alpha", 0.022f, 0.0f, 0.35f, true, true);
        spotlightConeAlphaField = new JSONStorableFloat("Spotlight Cone Alpha", 0.024f, 0.0f, 0.35f, true, true);
        lightVolumeScaleField = new JSONStorableFloat("Light Volume Scale", 0.62f, 0.1f, 2.0f, true, true);
        lightMarkerScaleField = new JSONStorableFloat("Light Marker Scale", 0.28f, 0.12f, 1.0f, true, true);
        richOverlayBudgetField = new JSONStorableFloat("Detail Overlay Limit", 10.0f, 0.0f, 128.0f, true, true);
        povFrustumLengthField = new JSONStorableFloat("POV Frustum Length", 0.9f, 0.25f, 8.0f, true, true);
        povFrustumAlphaField = new JSONStorableFloat("POV Frustum Alpha", 0.035f, 0.0f, 0.5f, true, true);
        grabThrowGrowScaleField = new JSONStorableFloat("Throw Grow Scale", 1.0f, 0.25f, MaxRadarPlacementScale, true, true);
        grabThrowVelocityScaleField = new JSONStorableFloat("Throw Velocity Scale", 0.45f, 0.05f, 2.0f, true, true);
        grabThrowDecelerationField = new JSONStorableFloat("Throw Deceleration", 1.5f, 0.2f, 8.0f, true, true);
        grabThrowReturnScaleField = new JSONStorableFloat("Throw Return Scale", 0.49f, 0.05f, MaxRadarPlacementScale, true, true);
        labelLimitField = new JSONStorableFloat("Label Limit", DefaultLabelLimit, 0.0f, MaxRadarLabelLimit, true, true);
        labelScaleField = new JSONStorableFloat("Label Scale", DefaultLabelScale, 0.01f, 0.18f, true, true);
        labelAlphaField = new JSONStorableFloat("Label Alpha", DefaultLabelAlpha, 0.0f, 1.0f, true, true);
#endif
        grabHitRadiusMetersField = new JSONStorableFloat("Grab Hit Radius Meters", 0.16f, 0.04f, 0.45f, true, true);
        wristTwistDegreesField = new JSONStorableFloat("Wrist Twist Degrees", 65.0f, 15.0f, 120.0f, true, true);
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
        hostSurfaceField = new JSONStorableString("Host Surface", "");
        displaySurfaceField = new JSONStorableString("Display Surface", "");
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
        RegisterBool(showEmptyAtomsField);
        RegisterBool(showSubSceneAtomsField);
        RegisterBool(showImagePanelAtomsField);
        RegisterBool(showAnimationAtomsField);
        RegisterBool(showForceAtomsField);
        RegisterBool(showShapeAtomsField);
        RegisterBool(showSoundAtomsField);
        RegisterBool(showTriggerAtomsField);
        RegisterBool(showNavigationPanelAtomsField);
        RegisterBool(showCameraAtomsField);
        RegisterBool(showOtherAtomsField);
#if FA_RADAR_PRO
        RegisterBool(showRotationAxesField);
        RegisterBool(showLightRangeVolumesField);
        RegisterBool(showSpotlightConesField);
        RegisterBool(showUserPovFrustumField);
        RegisterBool(showDesktopPovFrustumField);
        RegisterBool(showSceneCameraFrustumsField);
        RegisterBool(grabThrowPinEnabledField);
        RegisterBool(grabThrowSurfaceStopField);
        RegisterBool(grabThrowPinnedField);
#endif
        RegisterBool(clickSelectMarkersField);
        RegisterBool(grabHandlesEnabledField);
        RegisterBool(grabHandleDebugVisibleField);
        RegisterBool(grabHapticsEnabledField);
        RegisterBool(globalPrefsAutoSaveField);
        RegisterBool(cuaAnchorPresetField);
        RegisterBool(roomCompassField);

        RegisterFloat(hudOffsetXField);
        RegisterFloat(hudOffsetYField);
        RegisterFloat(hudOffsetZField);
        RegisterFloat(hudScaleField);
        RegisterFloat(wristOffsetXField);
        RegisterFloat(wristOffsetYField);
        RegisterFloat(wristOffsetZField);
        RegisterFloat(wristScaleField);
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
        RegisterFloat(maxVisibleMarkersField);
        RegisterFloat(markerClickRadiusPixelsField);
#if FA_RADAR_PRO
        RegisterFloat(rotationAxisLengthField);
        RegisterFloat(rotationAxisWidthField);
        RegisterFloat(lightVolumeAlphaField);
        RegisterFloat(pointLightRangeAlphaField);
        RegisterFloat(spotlightConeAlphaField);
        RegisterFloat(lightVolumeScaleField);
        RegisterFloat(lightMarkerScaleField);
        RegisterFloat(richOverlayBudgetField);
        RegisterFloat(povFrustumLengthField);
        RegisterFloat(povFrustumAlphaField);
        RegisterFloat(grabThrowGrowScaleField);
        RegisterFloat(grabThrowVelocityScaleField);
        RegisterFloat(grabThrowDecelerationField);
        RegisterFloat(grabThrowReturnScaleField);
        RegisterFloat(labelLimitField);
        RegisterFloat(labelScaleField);
        RegisterFloat(labelAlphaField);
#endif
        RegisterFloat(grabHitRadiusMetersField);
        RegisterFloat(wristTwistDegreesField);
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
        RegisterString(hostSurfaceField);
        RegisterString(displaySurfaceField);
        RegisterString(anchorAtomUidField);
        RegisterStringChooser(anchorModeField);
        RegisterStringChooser(radarModeField);
        RegisterStringChooser(desktopPlacementField);
        RegisterStringChooser(vrPlacementField);
#if FA_RADAR_PRO
        RegisterStringChooser(sceneLabelsField);
        RegisterStringChooser(labelOrientationField);
#endif

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
        if (IsCustomUnityAssetAnchorHostActive())
        {
            BuildCuaAnchorUi();
            return;
        }

        if (ShouldUseCreatorAnchorUi())
        {
            BuildEmptyAnchorUi();
            return;
        }

        BuildSceneSessionUi();
    }

    private void BuildCuaAnchorUi()
    {
        UpdatePluginSurfaceStatus();
#if FA_RADAR_PRO
        CreateToggle(radarEnabledField, false);
        CreateToggle(roomCompassField, true);
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, true);
        CreateToggle(availableAtomMarkersEnabledField, false);
        CreateSlider(wristScaleField, true);
        BuildProPrimaryFilterUi();
        BuildProDisplayUi();
        CreateToggle(gridEnabledField, false);
        CreateSlider(radarRangeMetersField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
        BuildProAdvancedTuningUi();
        CreateTextField(statusField, true);
#else
        CreateToggle(radarEnabledField, false);
        CreateToggle(roomCompassField, true);
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, false);
        CreateSlider(wristScaleField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
#endif
    }

    private void BuildSceneSessionUi()
    {
        UpdatePluginSurfaceStatus();
#if FA_RADAR_PRO
        CreateToggle(radarEnabledField, false);
        CreateToggle(roomCompassField, true);
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, true);
        CreateToggle(availableAtomMarkersEnabledField, true);
        CreateSlider(wristScaleField, true);
        BuildProPrimaryFilterUi();
        BuildProDisplayUi();
        CreateToggle(gridEnabledField, false);
        CreateSlider(radarRangeMetersField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
        BuildProAdvancedTuningUi();
        CreateButton("Reset Global Prefs", false).button.onClick.AddListener(delegate
        {
            ResetGlobalPreferencesAction();
        });
        CreateTextField(statusField, true);
#else
        BuildFreeSceneSessionUi();
#endif
    }

    private void BuildFreeSceneSessionUi()
    {
        CreateToggle(roomCompassField, true);
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, false);
        CreateSlider(wristScaleField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
    }

    private bool ShouldUseCreatorAnchorUi()
    {
        return IsEmptyAnchorHostActive();
    }

    private void BuildEmptyAnchorUi()
    {
        UpdatePluginSurfaceStatus();
        CreateToggle(radarEnabledField, false);
        CreateToggle(roomCompassField, true);
#if FA_RADAR_PRO
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, true);
        CreateToggle(availableAtomMarkersEnabledField, true);
        CreateSlider(wristScaleField, true);
        BuildProPrimaryFilterUi();
        BuildProDisplayUi();
        CreateToggle(gridEnabledField, false);
        CreateSlider(radarRangeMetersField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
        BuildProAdvancedTuningUi();
        CreateTextField(statusField, true);
#else
        BuildFreeEmptyAnchorUi();
#endif
    }

    private void BuildFreeEmptyAnchorUi()
    {
        CreatePopup(radarModeField, false);
        CreateSlider(hudScaleField, false);
        CreateSlider(wristScaleField, true);
        CreateToggle(grabHandlesEnabledField, false);
        CreateToggle(grabHapticsEnabledField, true);
    }

    private void BuildEmptyAnchorPlacementUi()
    {
        CreateSlider(hudOffsetXField, false);
        CreateSlider(hudOffsetYField, false);
        CreateSlider(hudOffsetZField, false);
        CreateSlider(hudScaleField, false);
        CreateSlider(anchorRotationXField, true);
        CreateSlider(anchorRotationYField, true);
        CreateSlider(anchorRotationZField, true);
    }

    private void BuildSceneSessionPlacementUi()
    {
        CreatePopup(desktopPlacementField, false);
        CreatePopup(vrPlacementField, true);
    }

    private void BuildPlacementUi()
    {
        CreateSlider(hudOffsetXField, false);
        CreateSlider(hudOffsetYField, false);
        CreateSlider(hudOffsetZField, false);
        CreateSlider(hudScaleField, false);
        CreateButton("Reset HUD Offset", false).button.onClick.AddListener(delegate
        {
            ResetHudOffset();
        });
    }

    private void BuildFreePlacementUi()
    {
        CreateSlider(hudScaleField, false);
        CreateSlider(hudOffsetXField, false);
        CreateSlider(hudOffsetYField, false);
        CreateSlider(hudOffsetZField, false);
    }

    private void BuildFreeStaticWorldPlacementUi()
    {
        CreateSlider(staticWorldXField, true);
        CreateSlider(staticWorldYField, true);
        CreateSlider(staticWorldZField, true);
    }

    private void BuildWristCompassUi()
    {
        CreatePopup(radarModeField, true);
        CreateSlider(wristScaleField, true);
        CreateSlider(wristOffsetXField, true);
        CreateSlider(wristOffsetYField, true);
        CreateSlider(wristOffsetZField, true);
    }

#if FA_RADAR_PRO
    private void BuildProPrimaryFilterUi()
    {
        CreateToggle(showLightAtomsField, false);
        CreateToggle(showPersonAtomsField, true);
        CreateToggle(showCameraAtomsField, false);
        CreateToggle(showCustomUnityAssetAtomsField, true);
        CreateToggle(showEmptyAtomsField, false);
        CreateToggle(showSubSceneAtomsField, true);
        CreateToggle(showImagePanelAtomsField, false);
        CreateToggle(showAnimationAtomsField, true);
        CreateToggle(showForceAtomsField, false);
        CreateToggle(showShapeAtomsField, true);
        CreateToggle(showSoundAtomsField, false);
        CreateToggle(showTriggerAtomsField, true);
        CreateToggle(showNavigationPanelAtomsField, false);
        CreateToggle(showOtherAtomsField, true);
    }

    private void BuildProDisplayUi()
    {
        CreatePopup(sceneLabelsField, false);
        CreatePopup(labelOrientationField, true);
        CreateToggle(showRotationAxesField, false);
        CreateToggle(showLightRangeVolumesField, true);
        CreateToggle(showSpotlightConesField, false);
        CreateToggle(showSceneCameraFrustumsField, true);
        CreateToggle(showUserPovFrustumField, false);
        CreateToggle(showDesktopPovFrustumField, true);
    }

    private void BuildProAdvancedTuningUi()
    {
        CreateSlider(labelLimitField, false);
        CreateSlider(labelScaleField, true);
        CreateSlider(labelAlphaField, false);
        CreateSlider(maxVisibleMarkersField, false);
        CreateSlider(richOverlayBudgetField, true);
        CreateSlider(rotationAxisLengthField, false);
        CreateSlider(rotationAxisWidthField, true);
        CreateSlider(pointLightRangeAlphaField, false);
        CreateSlider(spotlightConeAlphaField, true);
        CreateSlider(lightVolumeScaleField, false);
        CreateSlider(lightMarkerScaleField, true);
        CreateSlider(povFrustumLengthField, false);
        CreateSlider(povFrustumAlphaField, true);
    }
#endif

    private void ConfigureGlobalPreferenceCallbacks()
    {
        ConfigureGlobalPreferenceField(globalPrefsAutoSaveField);
        ConfigureGlobalPreferenceField(cuaAnchorPresetField);
        ConfigureGlobalPreferenceField(roomCompassField);
        ConfigureGlobalPreferenceField(hostSurfaceField);
        ConfigureGlobalPreferenceField(displaySurfaceField);
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
        if (roomCompassField != null)
        {
            roomCompassField.setCallbackFunction = delegate(bool value)
            {
                MarkGlobalPreferencesDirty();
                availableMarkersDirty = true;
                haveSmoothedHudPosition = false;
                InvalidateGridMesh();
            };
        }
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
        if (radarModeField != null)
        {
            radarModeField.setCallbackFunction = delegate(string value)
            {
                if (!globalPreferencesLoading
                    && string.Equals(NormalizeRadarMode(value), RadarModeWorld, StringComparison.Ordinal)
                    && hudRoot != null)
                {
                    SetStaticWorldPositionNoCallback(hudRoot.transform.position);
                    SetStaticWorldRotationNoCallback(hudRoot.transform.rotation);
                }

                MarkGlobalPreferencesDirty();
                availableMarkersDirty = true;
                haveSmoothedHudPosition = false;
                InvalidateGridMesh();
            };
        }
        ConfigureGlobalPreferenceCallback(desktopPlacementField);
        ConfigureGlobalPreferenceCallback(vrPlacementField);
        ConfigureGlobalPreferenceCallback(grabHandleDebugVisibleField);
        ConfigureGlobalPreferenceCallback(grabHapticsEnabledField);

        ConfigureImmediatePlacementPreferenceCallback(hudOffsetXField);
        ConfigureImmediatePlacementPreferenceCallback(hudOffsetYField);
        ConfigureImmediatePlacementPreferenceCallback(hudOffsetZField);
        ConfigureImmediatePlacementPreferenceCallback(hudScaleField);
        ConfigureImmediatePlacementPreferenceCallback(wristOffsetXField);
        ConfigureImmediatePlacementPreferenceCallback(wristOffsetYField);
        ConfigureImmediatePlacementPreferenceCallback(wristOffsetZField);
        ConfigureImmediatePlacementPreferenceCallback(wristScaleField);
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
        ConfigureGlobalPreferenceCallback(maxVisibleMarkersField);
        ConfigureGlobalPreferenceCallback(markerClickRadiusPixelsField);
        ConfigureGlobalPreferenceCallback(grabHitRadiusMetersField);
        ConfigureGlobalPreferenceCallback(wristTwistDegreesField);
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
        ConfigureGlobalPreferenceCallback(showEmptyAtomsField);
        ConfigureGlobalPreferenceCallback(showSubSceneAtomsField);
        ConfigureGlobalPreferenceCallback(showImagePanelAtomsField);
        ConfigureGlobalPreferenceCallback(showAnimationAtomsField);
        ConfigureGlobalPreferenceCallback(showForceAtomsField);
        ConfigureGlobalPreferenceCallback(showShapeAtomsField);
        ConfigureGlobalPreferenceCallback(showSoundAtomsField);
        ConfigureGlobalPreferenceCallback(showTriggerAtomsField);
        ConfigureGlobalPreferenceCallback(showNavigationPanelAtomsField);
        ConfigureGlobalPreferenceCallback(showCameraAtomsField);
        ConfigureGlobalPreferenceCallback(showOtherAtomsField);
#if FA_RADAR_PRO
        ConfigureRichOverlayPreferenceCallback(showRotationAxesField);
        ConfigureRichOverlayPreferenceCallback(showLightRangeVolumesField);
        ConfigureRichOverlayPreferenceCallback(showSpotlightConesField);
        ConfigureGlobalPreferenceCallback(sceneLabelsField);
        ConfigureGlobalPreferenceCallback(labelOrientationField);
        ConfigureGlobalPreferenceCallback(showUserPovFrustumField);
        ConfigureGlobalPreferenceCallback(showDesktopPovFrustumField);
        ConfigureGlobalPreferenceCallback(showSceneCameraFrustumsField);
        ConfigureGlobalPreferenceCallback(grabThrowPinEnabledField);
        ConfigureGlobalPreferenceCallback(grabThrowSurfaceStopField);
        ConfigureGlobalPreferenceCallback(grabThrowPinnedField);
        ConfigureGlobalPreferenceCallback(rotationAxisLengthField);
        ConfigureGlobalPreferenceCallback(rotationAxisWidthField);
        ConfigureGlobalPreferenceCallback(lightVolumeAlphaField);
        ConfigureGlobalPreferenceCallback(lightMarkerScaleField);
        ConfigureGlobalPreferenceCallback(pointLightRangeAlphaField);
        ConfigureGlobalPreferenceCallback(spotlightConeAlphaField);
        ConfigureGlobalPreferenceCallback(lightVolumeScaleField);
        ConfigureRichOverlayPreferenceCallback(richOverlayBudgetField);
        ConfigureGlobalPreferenceCallback(povFrustumLengthField);
        ConfigureGlobalPreferenceCallback(povFrustumAlphaField);
        ConfigureGlobalPreferenceCallback(grabThrowGrowScaleField);
        ConfigureGlobalPreferenceCallback(grabThrowVelocityScaleField);
        ConfigureGlobalPreferenceCallback(grabThrowDecelerationField);
        ConfigureGlobalPreferenceCallback(grabThrowReturnScaleField);
        ConfigureGlobalPreferenceCallback(labelLimitField);
        ConfigureGlobalPreferenceCallback(labelScaleField);
        ConfigureGlobalPreferenceCallback(labelAlphaField);
#endif
    }

#if FA_RADAR_PRO
    private void ConfigureRichOverlayPreferenceCallback(JSONStorableBool field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(bool value)
        {
            MarkGlobalPreferencesDirty();
            HideAvailableProOverlaysOutsideBudget();
        };
    }

    private void ConfigureRichOverlayPreferenceCallback(JSONStorableFloat field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(float value)
        {
            MarkGlobalPreferencesDirty();
            HideAvailableProOverlaysOutsideBudget();
        };
    }
#endif

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

    private void ConfigureImmediatePlacementPreferenceCallback(JSONStorableFloat field)
    {
        ConfigureGlobalPreferenceField(field);
        if (field == null)
        {
            return;
        }

        field.setCallbackFunction = delegate(float value)
        {
            MarkGlobalPreferencesDirty();
            FlushGlobalPreferencesIfDue(true);
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

        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        globalPreferencesDirty = true;
        materialsDirty = true;
        availableMarkersDirty = true;
        nextGlobalPreferencesFlushTime = Time.unscaledTime + GlobalPreferencesFlushDelaySeconds;
    }

    private void FlushGlobalPreferencesIfDue(bool force)
    {
        if (!globalPreferencesDirty)
        {
            return;
        }

        SetBoolNoCallback(globalPrefsAutoSaveField, true);
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

        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        bool writeAfterApply = globalPreferencesWriteAfterApply;
        globalPreferencesWriteAfterApply = false;
        globalPreferencesDirty = false;
        if (writeAfterApply)
        {
            WriteGlobalPreferences();
            return;
        }

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
            if (globalPreferencesWriteAfterApply)
            {
                globalPreferencesWriteAfterApply = false;
                WriteGlobalPreferences();
                return;
            }

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
        // Legacy name: this profile now covers creator anchor hosts, including Empty atoms.
        return IsEmptyAnchorHostActive();
    }

    private bool IsEmptyAnchorHostActive()
    {
        return IsAttachedAtomAnchorHostActive();
    }

    private bool IsCustomUnityAssetAnchorHostActive()
    {
        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        return anchorHost != null && IsCustomUnityAssetAtom(anchorHost);
    }

    private bool IsRoomCompassModeActive()
    {
        return IsAttachedAtomAnchorHostActive()
            && roomCompassField != null
            && roomCompassField.val;
    }

    private bool IsSceneSessionPluginHostActive()
    {
        return !IsEmptyAnchorHostActive();
    }

    private bool IsAttachedAtomAnchorHostActive()
    {
        return ResolveAttachedAtomAnchorHost() != null;
    }

    private Atom ResolveAttachedAtomAnchorHost()
    {
        if (containingAtom == null || IsPluginManagerHostAtom(containingAtom))
        {
            return null;
        }

        return containingAtom;
    }

    private bool IsPluginManagerHostAtom(Atom atom)
    {
        if (atom == null)
        {
            return false;
        }

        string uid = atom.uid ?? "";
        string type = atom.type ?? "";
        string category = atom.category ?? "";

        if (IsEmptyAtom(atom) || IsCustomUnityAssetAtom(atom))
        {
            return false;
        }

        if (atom.mainController == null)
        {
            return true;
        }

        return ContainsOrdinalIgnoreCase(uid, "PluginManager")
            || ContainsOrdinalIgnoreCase(uid, "ScenePlugin")
            || ContainsOrdinalIgnoreCase(uid, "SessionPlugin")
            || ContainsOrdinalIgnoreCase(type, "PluginManager")
            || ContainsOrdinalIgnoreCase(type, "ScenePlugin")
            || ContainsOrdinalIgnoreCase(type, "SessionPlugin")
            || ContainsOrdinalIgnoreCase(category, "PluginManager")
            || ContainsOrdinalIgnoreCase(category, "ScenePlugin")
            || ContainsOrdinalIgnoreCase(category, "SessionPlugin");
    }

    private static bool ContainsOrdinalIgnoreCase(string value, string fragment)
    {
        return !string.IsNullOrEmpty(value)
            && !string.IsNullOrEmpty(fragment)
            && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void UpdatePluginSurfaceStatus()
    {
        SetStringNoCallback(hostSurfaceField, ResolvePluginHostSurfaceName());
        SetStringNoCallback(displaySurfaceField, ResolveDisplaySurfaceName());
    }

    private string ResolvePluginHostSurfaceName()
    {
        return IsEmptyAnchorHostActive()
            ? PluginHostSurfaceEmptyAnchor
            : PluginHostSurfaceSceneSession;
    }

    private string ResolveDisplaySurfaceName()
    {
        if (IsRoomCompassModeActive())
        {
            return "Room Compass 1:1";
        }

        if (string.Equals(ResolveRadarMode(), RadarModeWorld, StringComparison.Ordinal))
        {
            return "World";
        }

        if (!IsSceneSessionPluginHostActive())
        {
            return IsCustomUnityAssetAnchorHostActive() ? "CUA Anchor" : "Scene Anchor";
        }

        return IsVrDisplayActive()
            ? DisplaySurfaceVR
            : DisplaySurfaceDesktop;
    }

    private bool IsVrDisplayActive()
    {
        if (SuperController.singleton == null)
        {
            return false;
        }

        try
        {
            return !SuperController.singleton.disableVR
                && (SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
        }
        catch
        {
            return false;
        }
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
            SetBoolNoCallback(globalPrefsAutoSaveField, true);
            ApplyBoolPreference(preferencesJson, "radarEnabled", radarEnabledField);
            ApplyBoolPreference(preferencesJson, "roomCompass", roomCompassField);
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
            string markerDefaultsVersion;
            bool markerDefaultsCurrent = TryReadStringPreference(preferencesJson, "commonMarkerDefaultsVersion", out markerDefaultsVersion)
                && string.Equals(markerDefaultsVersion, CommonMarkerDefaultsVersion, StringComparison.Ordinal);
            ApplyBoolPreference(preferencesJson, "availableAtomMarkers", availableAtomMarkersEnabledField);
            ApplyBoolPreference(preferencesJson, "clickSelectMarkers", clickSelectMarkersField);
            bool hasDirectGripDefaultMarker = preferencesJson.Contains("\"directGripGrabDefaulted\"");
            ApplyBoolPreference(preferencesJson, "grabHandlesEnabled", grabHandlesEnabledField);
            if (!hasDirectGripDefaultMarker)
            {
                SetBoolNoCallback(grabHandlesEnabledField, true);
            }
            ApplyBoolPreference(preferencesJson, "grabHandleDebugVisible", grabHandleDebugVisibleField);
            ApplyBoolPreference(preferencesJson, "grabHaptics", grabHapticsEnabledField);
            ApplyStringPreference(preferencesJson, "anchorMode", anchorModeField);
            ApplyStringPreference(preferencesJson, "anchorAtomUid", anchorAtomUidField);
            ApplySceneSessionPlacementPreference(preferencesJson);
            ApplyDesktopVisibilityRecoveryIfNeeded(preferencesJson);
            ApplyRadarModePreference(preferencesJson);

            ApplyFloatPreference(preferencesJson, "hudOffsetX", hudOffsetXField);
            ApplyFloatPreference(preferencesJson, "hudOffsetY", hudOffsetYField);
            ApplyFloatPreference(preferencesJson, "hudOffsetZ", hudOffsetZField);
            ApplyFloatPreference(preferencesJson, "hudScale", hudScaleField);
            ApplyFloatPreference(preferencesJson, "wristOffsetX", wristOffsetXField);
            ApplyFloatPreference(preferencesJson, "wristOffsetY", wristOffsetYField);
            ApplyFloatPreference(preferencesJson, "wristOffsetZ", wristOffsetZField);
            ApplyFloatPreference(preferencesJson, "wristScale", wristScaleField);
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
            if (!markerDefaultsCurrent)
            {
                SetBoolNoCallback(availableAtomMarkersEnabledField, true);
                SetFloatNoCallback(rangeFadeMetersField, 10.0f);
                globalPreferencesWriteAfterApply = true;
            }
            ApplyFloatPreference(preferencesJson, "depthSizeStrength", depthSizeStrengthField);
            ApplyFloatPreference(preferencesJson, "atomPollSeconds", atomPollSecondsField);
            ApplyFloatPreference(preferencesJson, "availableAtomAlpha", availableAtomAlphaField);
            ApplyFloatPreference(preferencesJson, "maxVisibleMarkers", maxVisibleMarkersField);
            ApplyFloatPreference(preferencesJson, "markerClickRadiusPixels", markerClickRadiusPixelsField);
            ApplyFloatPreference(preferencesJson, "grabHitRadiusMeters", grabHitRadiusMetersField);
            ApplyFloatPreference(preferencesJson, "wristTwistDegrees", wristTwistDegreesField);
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
            ApplyVisualDepthDefaultsIfNeeded(preferencesJson);
        }
        finally
        {
            globalPreferencesLoading = previousLoading;
        }

        lastAppliedCommonPreferencesJson = preferencesJson;
        haveSmoothedHudPosition = false;
        materialsDirty = true;
        availableMarkersDirty = true;
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
            string filterDefaultsVersion;
            bool proFilterDefaultsCurrent = TryReadStringPreference(preferencesJson, "proFilterDefaultsVersion", out filterDefaultsVersion)
                && string.Equals(filterDefaultsVersion, ProFilterDefaultsVersion, StringComparison.Ordinal);
            ApplyBoolPreference(preferencesJson, "showLights", showLightAtomsField);
            ApplyBoolPreference(preferencesJson, "showCUA", showCustomUnityAssetAtomsField);
            ApplyBoolPreference(preferencesJson, "showPeople", showPersonAtomsField);
            ApplyBoolPreference(preferencesJson, "showEmpty", showEmptyAtomsField);
            ApplyBoolPreference(preferencesJson, "showSubScene", showSubSceneAtomsField);
            ApplyBoolPreference(preferencesJson, "showImagePanel", showImagePanelAtomsField);
            ApplyBoolPreference(preferencesJson, "showAnimation", showAnimationAtomsField);
            ApplyBoolPreference(preferencesJson, "showForce", showForceAtomsField);
            ApplyBoolPreference(preferencesJson, "showShapes", showShapeAtomsField);
            ApplyBoolPreference(preferencesJson, "showSounds", showSoundAtomsField);
            ApplyBoolPreference(preferencesJson, "showTriggers", showTriggerAtomsField);
            ApplyBoolPreference(preferencesJson, "showNavigationPanels", showNavigationPanelAtomsField);
            ApplyBoolPreference(preferencesJson, "showCameraAtoms", showCameraAtomsField);
            ApplyBoolPreference(preferencesJson, "showOtherAtoms", showOtherAtomsField);
#if FA_RADAR_PRO
            ApplyBoolPreference(preferencesJson, "showRotationAxes", showRotationAxesField);
            ApplyBoolPreference(preferencesJson, "showLightRangeVolumes", showLightRangeVolumesField);
            ApplyBoolPreference(preferencesJson, "showSpotlightCones", showSpotlightConesField);
            ApplyBoolPreference(preferencesJson, "showUserPovFrustum", showUserPovFrustumField);
            ApplyBoolPreference(preferencesJson, "showDesktopPovFrustum", showDesktopPovFrustumField);
            ApplyBoolPreference(preferencesJson, "showSceneCameraFrustums", showSceneCameraFrustumsField);
            if (!proFilterDefaultsCurrent)
            {
                SetDefaultProAtomFiltersNoCallback();
                globalPreferencesWriteAfterApply = true;
            }
            ApplyBoolPreference(preferencesJson, "grabThrowPinEnabled", grabThrowPinEnabledField);
            ApplyBoolPreference(preferencesJson, "grabThrowSurfaceStop", grabThrowSurfaceStopField);
            ApplyBoolPreference(preferencesJson, "grabThrowPinned", grabThrowPinnedField);
            ApplySceneLabelsPreference(preferencesJson);
            ApplyLabelOrientationPreference(preferencesJson);
            ApplySplitLightAlphaDefaultsIfNeeded(preferencesJson);
            ApplyFloatPreference(preferencesJson, "rotationAxisLength", rotationAxisLengthField);
            ApplyFloatPreference(preferencesJson, "rotationAxisWidth", rotationAxisWidthField);
            ApplyFloatPreference(preferencesJson, "lightVolumeAlpha", lightVolumeAlphaField);
            ApplyFloatPreference(preferencesJson, "pointLightRangeAlpha", pointLightRangeAlphaField);
            ApplyFloatPreference(preferencesJson, "spotlightConeAlpha", spotlightConeAlphaField);
            ApplyFloatPreference(preferencesJson, "lightVolumeScale", lightVolumeScaleField);
            ApplyFloatPreference(preferencesJson, "lightMarkerScale", lightMarkerScaleField);
            ApplyFloatPreference(preferencesJson, "richOverlayBudget", richOverlayBudgetField);
            ApplyFloatPreference(preferencesJson, "povFrustumLength", povFrustumLengthField);
            ApplyFloatPreference(preferencesJson, "povFrustumAlpha", povFrustumAlphaField);
            ApplyFloatPreference(preferencesJson, "grabThrowGrowScale", grabThrowGrowScaleField);
            ApplyFloatPreference(preferencesJson, "grabThrowVelocityScale", grabThrowVelocityScaleField);
            ApplyFloatPreference(preferencesJson, "grabThrowDeceleration", grabThrowDecelerationField);
            ApplyFloatPreference(preferencesJson, "grabThrowReturnScale", grabThrowReturnScaleField);
            ApplyFloatPreference(preferencesJson, "labelLimit", labelLimitField);
            ApplyFloatPreference(preferencesJson, "labelScale", labelScaleField);
            ApplyFloatPreference(preferencesJson, "labelAlpha", labelAlphaField);
            ApplyVisualDepthDefaultsIfNeeded(preferencesJson);
            ApplyDirectorReadabilityDefaultsIfNeeded(preferencesJson);
            ApplyLabelReadabilityDefaultsIfNeeded(preferencesJson);
#endif
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

    private void ApplySplitLightAlphaDefaultsIfNeeded(string preferencesJson)
    {
#if FA_RADAR_PRO
        string defaultsVersion;
        if (TryReadStringPreference(preferencesJson, "lightAlphaDefaultsVersion", out defaultsVersion)
            && string.Equals(defaultsVersion, LightAlphaDefaultsVersion, StringComparison.Ordinal))
        {
            return;
        }

        SetFloatNoCallback(pointLightRangeAlphaField, 0.022f);
        SetFloatNoCallback(spotlightConeAlphaField, 0.024f);
        globalPreferencesWriteAfterApply = true;
#endif
    }

    private void ApplyVisualDepthDefaultsIfNeeded(string preferencesJson)
    {
        string defaultsVersion;
        if (TryReadStringPreference(preferencesJson, "visualDepthDefaultsVersion", out defaultsVersion)
            && string.Equals(defaultsVersion, VisualDepthDefaultsVersion, StringComparison.Ordinal))
        {
            return;
        }

        SetFloatNoCallback(shellAlphaField, 0.055f);
        SetFloatNoCallback(ringAlphaField, 0.30f);
        SetFloatNoCallback(gridAlphaField, 0.11f);
        SetFloatNoCallback(heightStemAlphaField, 0.26f);
        SetFloatNoCallback(depthSizeStrengthField, 0.55f);
        SetFloatNoCallback(availableAtomAlphaField, 0.34f);
#if FA_RADAR_PRO
        SetFloatNoCallback(rotationAxisLengthField, 0.085f);
        SetFloatNoCallback(rotationAxisWidthField, 0.0045f);
        SetFloatNoCallback(lightVolumeAlphaField, 0.045f);
        SetFloatNoCallback(pointLightRangeAlphaField, 0.022f);
        SetFloatNoCallback(spotlightConeAlphaField, 0.024f);
        SetFloatNoCallback(lightVolumeScaleField, 0.62f);
        SetFloatNoCallback(lightMarkerScaleField, 0.28f);
        SetFloatNoCallback(richOverlayBudgetField, MaxDirectorBackgroundOverlayBudget);
        SetFloatNoCallback(povFrustumLengthField, 0.9f);
        SetFloatNoCallback(povFrustumAlphaField, 0.035f);
#endif
        globalPreferencesWriteAfterApply = true;
    }

#if FA_RADAR_PRO
    private void ApplyDirectorReadabilityDefaultsIfNeeded(string preferencesJson)
    {
        string defaultsVersion;
        if (TryReadStringPreference(preferencesJson, "directorReadabilityDefaultsVersion", out defaultsVersion)
            && string.Equals(defaultsVersion, DirectorReadabilityDefaultsVersion, StringComparison.Ordinal))
        {
            return;
        }

        SetFloatNoCallback(rotationAxisLengthField, 0.085f);
        SetFloatNoCallback(rotationAxisWidthField, 0.0045f);
        SetFloatNoCallback(lightVolumeAlphaField, 0.045f);
        SetFloatNoCallback(pointLightRangeAlphaField, 0.022f);
        SetFloatNoCallback(spotlightConeAlphaField, 0.024f);
        SetFloatNoCallback(lightVolumeScaleField, 0.62f);
        SetFloatNoCallback(richOverlayBudgetField, MaxDirectorBackgroundOverlayBudget);
        SetFloatNoCallback(povFrustumLengthField, 0.9f);
        SetFloatNoCallback(povFrustumAlphaField, 0.035f);
        globalPreferencesWriteAfterApply = true;
    }

    private void ApplyLabelReadabilityDefaultsIfNeeded(string preferencesJson)
    {
        string defaultsVersion;
        if (TryReadStringPreference(preferencesJson, "labelReadabilityDefaultsVersion", out defaultsVersion)
            && string.Equals(defaultsVersion, LabelReadabilityDefaultsVersion, StringComparison.Ordinal))
        {
            return;
        }

        SetSceneLabelsNoCallback(LabelsSelected);
        SetFloatNoCallback(labelLimitField, DefaultLabelLimit);
        SetFloatNoCallback(labelScaleField, DefaultLabelScale);
        SetFloatNoCallback(labelAlphaField, DefaultLabelAlpha);
        globalPreferencesWriteAfterApply = true;
    }
#endif

    private void ApplyBuiltInGlobalPreferenceDefaults()
    {
        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetBoolNoCallback(radarEnabledField, true);
        SetBoolNoCallback(roomCompassField, false);
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
        SetBoolNoCallback(grabHandlesEnabledField, true);
        SetBoolNoCallback(grabHandleDebugVisibleField, false);
        SetBoolNoCallback(grabHapticsEnabledField, true);
        SetDefaultProAtomFiltersNoCallback();
#if FA_RADAR_PRO
        SetBoolNoCallback(showRotationAxesField, true);
        SetBoolNoCallback(showLightRangeVolumesField, true);
        SetBoolNoCallback(showSpotlightConesField, true);
        SetBoolNoCallback(showUserPovFrustumField, true);
        SetBoolNoCallback(showDesktopPovFrustumField, true);
        SetBoolNoCallback(showSceneCameraFrustumsField, true);
        SetBoolNoCallback(grabThrowPinEnabledField, false);
        SetBoolNoCallback(grabThrowSurfaceStopField, true);
        SetBoolNoCallback(grabThrowPinnedField, false);
        SetSceneLabelsNoCallback(LabelsSelected);
        SetLabelOrientationNoCallback(LabelOrientationFaceViewer);
#endif
        SetStringNoCallback(anchorModeField, AnchorModeHud);
        SetStringNoCallback(anchorAtomUidField, "");
        SetDesktopPlacementNoCallback(ResolveDefaultDesktopPlacement());
        SetVRPlacementNoCallback(ResolveDefaultVRPlacement());
        SetRadarModeNoCallback(RadarModeHud);

        SetFloatNoCallback(hudOffsetXField, -0.59f);
        SetFloatNoCallback(hudOffsetYField, 0.22f);
        SetFloatNoCallback(hudOffsetZField, 0.78f);
        SetFloatNoCallback(hudScaleField, 0.49f);
        SetFloatNoCallback(wristOffsetXField, 0.0f);
        SetFloatNoCallback(wristOffsetYField, 0.08f);
        SetFloatNoCallback(wristOffsetZField, 0.12f);
        SetFloatNoCallback(wristScaleField, 0.35f);
        SetFloatNoCallback(desktopTiltDegreesField, 90.0f);
        SetFloatNoCallback(radarRangeMetersField, 5.0f);
        SetFloatNoCallback(floorAreaScaleField, 1.0f);
        SetFloatNoCallback(radarVisualRadiusField, DefaultRadarVisualRadiusMeters);
        SetFloatNoCallback(gridStepMetersField, 1.0f);
        SetFloatNoCallback(shellAlphaField, 0.055f);
        SetFloatNoCallback(ringAlphaField, 0.30f);
        SetFloatNoCallback(gridAlphaField, 0.11f);
        SetFloatNoCallback(markerAlphaField, 0.9f);
        SetFloatNoCallback(emissionStrengthField, 1.4f);
        SetFloatNoCallback(ringRotationSpeedField, 0.0f);
        SetFloatNoCallback(targetMarkerScaleField, 0.09f);
        SetFloatNoCallback(heightScaleMetersField, 6.0f);
        SetFloatNoCallback(heightStemAlphaField, 0.26f);
        SetFloatNoCallback(rangeFadeMetersField, 10.0f);
        SetFloatNoCallback(depthSizeStrengthField, 0.55f);
        SetFloatNoCallback(atomPollSecondsField, 0.75f);
        SetFloatNoCallback(availableAtomAlphaField, 0.34f);
        SetFloatNoCallback(maxVisibleMarkersField, 192.0f);
        SetFloatNoCallback(markerClickRadiusPixelsField, 20.0f);
#if FA_RADAR_PRO
        SetFloatNoCallback(rotationAxisLengthField, 0.085f);
        SetFloatNoCallback(rotationAxisWidthField, 0.0045f);
        SetFloatNoCallback(lightVolumeAlphaField, 0.045f);
        SetFloatNoCallback(pointLightRangeAlphaField, 0.022f);
        SetFloatNoCallback(spotlightConeAlphaField, 0.024f);
        SetFloatNoCallback(lightVolumeScaleField, 0.62f);
        SetFloatNoCallback(lightMarkerScaleField, 0.28f);
        SetFloatNoCallback(richOverlayBudgetField, MaxDirectorBackgroundOverlayBudget);
        SetFloatNoCallback(povFrustumLengthField, 0.9f);
        SetFloatNoCallback(povFrustumAlphaField, 0.035f);
        SetFloatNoCallback(grabThrowGrowScaleField, 1.0f);
        SetFloatNoCallback(grabThrowVelocityScaleField, 0.45f);
        SetFloatNoCallback(grabThrowDecelerationField, 1.5f);
        SetFloatNoCallback(grabThrowReturnScaleField, 0.49f);
        SetFloatNoCallback(labelLimitField, DefaultLabelLimit);
        SetFloatNoCallback(labelScaleField, DefaultLabelScale);
        SetFloatNoCallback(labelAlphaField, DefaultLabelAlpha);
#endif
        SetFloatNoCallback(grabHitRadiusMetersField, 0.16f);
        SetFloatNoCallback(wristTwistDegreesField, 65.0f);
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

        if (IsCuaPreferenceProfileActive())
        {
            SetStringNoCallback(anchorModeField, AnchorModeContainingAtom);
            Atom anchorHost = ResolveAttachedAtomAnchorHost();
            SetStringNoCallback(anchorAtomUidField, anchorHost != null ? (anchorHost.uid ?? "") : "");
            SetRadarModeNoCallback(RadarModeHud);
            ApplyCreatorAnchorPlacementDefaultsNoCallback();
        }
    }

    private void ApplyCreatorAnchorPlacementDefaultsNoCallback()
    {
        SetFloatNoCallback(hudOffsetXField, 0.0f);
        SetFloatNoCallback(hudOffsetYField, 0.0f);
        SetFloatNoCallback(
            hudOffsetZField,
            IsCustomUnityAssetAnchorHostActive() ? 0.0f : DefaultAtomAnchorOffsetZ);
        SetFloatNoCallback(hudScaleField, DefaultAtomAnchorScale);
        SetFloatNoCallback(anchorRotationXField, 0.0f);
        SetFloatNoCallback(anchorRotationYField, 0.0f);
        SetFloatNoCallback(anchorRotationZField, 0.0f);
    }

    private string BuildCommonGlobalPreferencesJson()
    {
        StringBuilder sb = new StringBuilder(2048);
        bool wroteProperty = false;
        sb.Append('{');
        AppendJsonStringProperty(sb, ref wroteProperty, "schemaVersion", ResolveCommonPreferencesSchemaVersion());
        AppendJsonStringProperty(sb, ref wroteProperty, "savedAtUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        AppendJsonBoolProperty(sb, ref wroteProperty, "globalPrefsAutoSave", true);
        AppendJsonBoolProperty(sb, ref wroteProperty, "radarEnabled", ReadBool(radarEnabledField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "roomCompass", ReadBool(roomCompassField, false));
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
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHandlesEnabled", ReadBool(grabHandlesEnabledField, true));
        AppendJsonStringProperty(sb, ref wroteProperty, "commonMarkerDefaultsVersion", CommonMarkerDefaultsVersion);
        AppendJsonStringProperty(sb, ref wroteProperty, "visualDepthDefaultsVersion", VisualDepthDefaultsVersion);
        AppendJsonBoolProperty(sb, ref wroteProperty, "directGripGrabDefaulted", true);
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHandleDebugVisible", ReadBool(grabHandleDebugVisibleField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabHaptics", ReadBool(grabHapticsEnabledField, true));
        AppendJsonStringProperty(sb, ref wroteProperty, "anchorMode", ResolveAnchorMode());
        AppendJsonStringProperty(sb, ref wroteProperty, "anchorAtomUid", ReadString(anchorAtomUidField, ""));
        AppendJsonStringProperty(sb, ref wroteProperty, "desktopPlacement", ResolveDesktopPlacement());
        AppendJsonStringProperty(sb, ref wroteProperty, "vrPlacement", ResolveVRPlacement());
        AppendJsonStringProperty(sb, ref wroteProperty, "radarMode", ResolveRadarMode());
        AppendJsonStringProperty(sb, ref wroteProperty, "desktopVisibilityRecoveryVersion", DesktopVisibilityRecoveryVersion);

        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetX", ReadFloat(hudOffsetXField, -0.59f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetY", ReadFloat(hudOffsetYField, 0.22f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudOffsetZ", ReadFloat(hudOffsetZField, 0.78f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "hudScale", ReadFloat(hudScaleField, 0.49f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "wristOffsetX", ReadFloat(wristOffsetXField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "wristOffsetY", ReadFloat(wristOffsetYField, 0.08f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "wristOffsetZ", ReadFloat(wristOffsetZField, 0.12f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "wristScale", ReadFloat(wristScaleField, 0.35f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "desktopTiltDegrees", ReadFloat(desktopTiltDegreesField, 90.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "radarRangeMeters", ReadFloat(radarRangeMetersField, 5.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "floorAreaScale", ReadFloat(floorAreaScaleField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "radarVisualRadius", ReadFloat(radarVisualRadiusField, DefaultRadarVisualRadiusMeters));
        AppendJsonFloatProperty(sb, ref wroteProperty, "gridStepMeters", ReadFloat(gridStepMetersField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "shellAlpha", ReadFloat(shellAlphaField, 0.055f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "ringAlpha", ReadFloat(ringAlphaField, 0.30f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "gridAlpha", ReadFloat(gridAlphaField, 0.11f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "markerAlpha", ReadFloat(markerAlphaField, 0.9f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "emissionStrength", ReadFloat(emissionStrengthField, 1.4f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "ringRotationSpeed", ReadFloat(ringRotationSpeedField, 0.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "targetMarkerScale", ReadFloat(targetMarkerScaleField, 0.09f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "heightScaleMeters", ReadFloat(heightScaleMetersField, 6.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "heightStemAlpha", ReadFloat(heightStemAlphaField, 0.26f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "rangeFadeMeters", ReadFloat(rangeFadeMetersField, 10.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "depthSizeStrength", ReadFloat(depthSizeStrengthField, 0.55f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "atomPollSeconds", ReadFloat(atomPollSecondsField, 0.75f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "availableAtomAlpha", ReadFloat(availableAtomAlphaField, 0.34f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "maxVisibleMarkers", ReadFloat(maxVisibleMarkersField, 192.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "markerClickRadiusPixels", ReadFloat(markerClickRadiusPixelsField, 20.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabHitRadiusMeters", ReadFloat(grabHitRadiusMetersField, 0.16f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "wristTwistDegrees", ReadFloat(wristTwistDegreesField, 65.0f));
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
        AppendJsonStringProperty(sb, ref wroteProperty, "proFilterDefaultsVersion", ProFilterDefaultsVersion);
        AppendJsonBoolProperty(sb, ref wroteProperty, "showLights", ReadBool(showLightAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showCUA", ReadBool(showCustomUnityAssetAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showPeople", ReadBool(showPersonAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showEmpty", ReadBool(showEmptyAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showSubScene", ReadBool(showSubSceneAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showImagePanel", ReadBool(showImagePanelAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showAnimation", ReadBool(showAnimationAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showForce", ReadBool(showForceAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showShapes", ReadBool(showShapeAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showSounds", ReadBool(showSoundAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showTriggers", ReadBool(showTriggerAtomsField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showNavigationPanels", ReadBool(showNavigationPanelAtomsField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showCameraAtoms", ReadBool(showCameraAtomsField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showOtherAtoms", ReadBool(showOtherAtomsField, true));
#if FA_RADAR_PRO
        AppendJsonBoolProperty(sb, ref wroteProperty, "showRotationAxes", ReadBool(showRotationAxesField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showLightRangeVolumes", ReadBool(showLightRangeVolumesField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showSpotlightCones", ReadBool(showSpotlightConesField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showUserPovFrustum", ReadBool(showUserPovFrustumField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showDesktopPovFrustum", ReadBool(showDesktopPovFrustumField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "showSceneCameraFrustums", ReadBool(showSceneCameraFrustumsField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabThrowPinEnabled", ReadBool(grabThrowPinEnabledField, false));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabThrowSurfaceStop", ReadBool(grabThrowSurfaceStopField, true));
        AppendJsonBoolProperty(sb, ref wroteProperty, "grabThrowPinned", ReadBool(grabThrowPinnedField, false));
        AppendJsonStringProperty(sb, ref wroteProperty, "sceneLabels", ResolveSceneLabelsMode());
        AppendJsonStringProperty(sb, ref wroteProperty, "labelOrientation", ResolveLabelOrientationMode());
        AppendJsonStringProperty(sb, ref wroteProperty, "lightAlphaDefaultsVersion", LightAlphaDefaultsVersion);
        AppendJsonStringProperty(sb, ref wroteProperty, "visualDepthDefaultsVersion", VisualDepthDefaultsVersion);
        AppendJsonStringProperty(sb, ref wroteProperty, "directorReadabilityDefaultsVersion", DirectorReadabilityDefaultsVersion);
        AppendJsonStringProperty(sb, ref wroteProperty, "labelReadabilityDefaultsVersion", LabelReadabilityDefaultsVersion);
        AppendJsonFloatProperty(sb, ref wroteProperty, "rotationAxisLength", ReadFloat(rotationAxisLengthField, 0.085f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "rotationAxisWidth", ReadFloat(rotationAxisWidthField, 0.0045f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "lightVolumeAlpha", ReadFloat(lightVolumeAlphaField, 0.045f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "pointLightRangeAlpha", ReadFloat(pointLightRangeAlphaField, 0.022f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "spotlightConeAlpha", ReadFloat(spotlightConeAlphaField, 0.024f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "lightVolumeScale", ReadFloat(lightVolumeScaleField, 0.62f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "lightMarkerScale", ReadFloat(lightMarkerScaleField, 0.28f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "richOverlayBudget", ReadFloat(richOverlayBudgetField, MaxDirectorBackgroundOverlayBudget));
        AppendJsonFloatProperty(sb, ref wroteProperty, "povFrustumLength", ReadFloat(povFrustumLengthField, 0.9f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "povFrustumAlpha", ReadFloat(povFrustumAlphaField, 0.035f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabThrowGrowScale", ReadFloat(grabThrowGrowScaleField, 1.0f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabThrowVelocityScale", ReadFloat(grabThrowVelocityScaleField, 0.45f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabThrowDeceleration", ReadFloat(grabThrowDecelerationField, 1.5f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "grabThrowReturnScale", ReadFloat(grabThrowReturnScaleField, 0.49f));
        AppendJsonFloatProperty(sb, ref wroteProperty, "labelLimit", ReadFloat(labelLimitField, DefaultLabelLimit));
        AppendJsonFloatProperty(sb, ref wroteProperty, "labelScale", ReadFloat(labelScaleField, DefaultLabelScale));
        AppendJsonFloatProperty(sb, ref wroteProperty, "labelAlpha", ReadFloat(labelAlphaField, DefaultLabelAlpha));
#endif
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

    private void ApplyRadarModePreference(string preferencesJson)
    {
        string value;
        if (radarModeField != null && TryReadStringPreference(preferencesJson, "radarMode", out value))
        {
            SetRadarModeNoCallback(value);
            return;
        }

        bool legacyWristMode;
        if (!TryReadBoolPreference(preferencesJson, "wristCompassMode", out legacyWristMode) || !legacyWristMode)
        {
            return;
        }

        bool legacyTwistReveal = true;
        bool parsedLegacyTwistReveal;
        if (TryReadBoolPreference(preferencesJson, "wristTwistReveal", out parsedLegacyTwistReveal))
        {
            legacyTwistReveal = parsedLegacyTwistReveal;
        }
        string legacyHand = "";
        TryReadStringPreference(preferencesJson, "wristHand", out legacyHand);
        bool legacyRightHand = string.Equals(legacyHand, "Right", StringComparison.OrdinalIgnoreCase);
        if (legacyRightHand)
        {
            SetRadarModeNoCallback(legacyTwistReveal ? RadarModeWristRight : RadarModeWristRightAlwaysOn);
            return;
        }

        SetRadarModeNoCallback(legacyTwistReveal ? RadarModeWristLeft : RadarModeWristLeftAlwaysOn);
    }

    private void ApplySceneSessionPlacementPreference(string preferencesJson)
    {
        ApplyDesktopPlacementPreference(preferencesJson);
        ApplyVRPlacementPreference(preferencesJson);
    }

    private void ApplyDesktopPlacementPreference(string preferencesJson)
    {
        string value;
        if (desktopPlacementField != null && TryReadStringPreference(preferencesJson, "desktopPlacement", out value))
        {
            SetDesktopPlacementNoCallback(value);
        }
    }

    private void ApplyDesktopVisibilityRecoveryIfNeeded(string preferencesJson)
    {
        string recoveryVersion;
        if (TryReadStringPreference(preferencesJson, "desktopVisibilityRecoveryVersion", out recoveryVersion)
            && string.Equals(recoveryVersion, DesktopVisibilityRecoveryVersion, StringComparison.Ordinal))
        {
            return;
        }

        if (IsSceneSessionPluginHostActive() && !IsVrDisplayActive())
        {
            string desktopPlacement;
            if (TryReadStringPreference(preferencesJson, "desktopPlacement", out desktopPlacement)
                && string.Equals(NormalizeDesktopPlacement(desktopPlacement), DesktopPlacementPinnedInWorld, StringComparison.Ordinal))
            {
                SetDesktopPlacementNoCallback(DesktopPlacementAttachedToUi);
                haveSmoothedHudPosition = false;
            }
        }

        globalPreferencesWriteAfterApply = true;
    }

    private void ApplyVRPlacementPreference(string preferencesJson)
    {
        string value;
        if (vrPlacementField != null && TryReadStringPreference(preferencesJson, "vrPlacement", out value))
        {
            SetVRPlacementNoCallback(value);
        }
    }

#if FA_RADAR_PRO
    private void ApplySceneLabelsPreference(string preferencesJson)
    {
        string value;
        if (sceneLabelsField != null && TryReadStringPreference(preferencesJson, "sceneLabels", out value))
        {
            SetSceneLabelsNoCallback(value);
        }
    }

    private void ApplyLabelOrientationPreference(string preferencesJson)
    {
        string value;
        if (labelOrientationField != null && TryReadStringPreference(preferencesJson, "labelOrientation", out value))
        {
            SetLabelOrientationNoCallback(value);
        }
    }
#endif

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

    private void SetDesktopPlacementNoCallback(string value)
    {
        if (desktopPlacementField != null)
        {
            desktopPlacementField.valNoCallback = NormalizeDesktopPlacement(value);
        }
    }

    private void SetVRPlacementNoCallback(string value)
    {
        if (vrPlacementField != null)
        {
            vrPlacementField.valNoCallback = NormalizeDesktopPlacement(value);
        }
    }

    private void SetRadarModeNoCallback(string value)
    {
        if (radarModeField != null)
        {
            radarModeField.valNoCallback = NormalizeRadarMode(value);
        }
    }

#if FA_RADAR_PRO
    private void SetSceneLabelsNoCallback(string value)
    {
        if (sceneLabelsField != null)
        {
            sceneLabelsField.valNoCallback = NormalizeSceneLabelsMode(value);
        }
    }

    private void SetLabelOrientationNoCallback(string value)
    {
        if (labelOrientationField != null)
        {
            labelOrientationField.valNoCallback = NormalizeLabelOrientationMode(value);
        }
    }

    private string ResolveSceneLabelsMode()
    {
        return NormalizeSceneLabelsMode(ReadString(sceneLabelsField, LabelsSelected));
    }

    private string ResolveLabelOrientationMode()
    {
        return NormalizeLabelOrientationMode(ReadString(labelOrientationField, LabelOrientationFaceViewer));
    }

    private static string NormalizeSceneLabelsMode(string value)
    {
        if (string.Equals(value, LabelsOff, StringComparison.OrdinalIgnoreCase))
        {
            return LabelsOff;
        }
        if (string.Equals(value, LabelsSelected, StringComparison.OrdinalIgnoreCase))
        {
            return LabelsSelected;
        }

        return LabelsSelected;
    }

    private static string NormalizeLabelOrientationMode(string value)
    {
        if (string.Equals(value, LabelOrientationWorldAxis, StringComparison.OrdinalIgnoreCase))
        {
            return LabelOrientationWorldAxis;
        }
        if (string.Equals(value, LabelOrientationObjectRotation, StringComparison.OrdinalIgnoreCase))
        {
            return LabelOrientationObjectRotation;
        }

        return LabelOrientationFaceViewer;
    }
#endif

    private void SetAllProAtomFiltersNoCallback(bool value)
    {
        SetBoolNoCallback(showLightAtomsField, value);
        SetBoolNoCallback(showCustomUnityAssetAtomsField, value);
        SetBoolNoCallback(showPersonAtomsField, value);
        SetBoolNoCallback(showEmptyAtomsField, value);
        SetBoolNoCallback(showSubSceneAtomsField, value);
        SetBoolNoCallback(showImagePanelAtomsField, value);
        SetBoolNoCallback(showAnimationAtomsField, value);
        SetBoolNoCallback(showForceAtomsField, value);
        SetBoolNoCallback(showShapeAtomsField, value);
        SetBoolNoCallback(showSoundAtomsField, value);
        SetBoolNoCallback(showTriggerAtomsField, value);
        SetBoolNoCallback(showNavigationPanelAtomsField, value);
        SetBoolNoCallback(showCameraAtomsField, value);
        SetBoolNoCallback(showOtherAtomsField, value);
    }

    private void SetDefaultProAtomFiltersNoCallback()
    {
        SetAllProAtomFiltersNoCallback(true);
        SetBoolNoCallback(showNavigationPanelAtomsField, false);
#if FA_RADAR_PRO
        SetBoolNoCallback(showRotationAxesField, true);
        SetBoolNoCallback(showLightRangeVolumesField, true);
        SetBoolNoCallback(showSpotlightConesField, true);
        SetBoolNoCallback(showUserPovFrustumField, true);
        SetBoolNoCallback(showDesktopPovFrustumField, true);
        SetBoolNoCallback(showSceneCameraFrustumsField, true);
#endif
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

    private static string ReadString(JSONStorableStringChooser field, string fallback)
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

        shellMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Sphere Material"), new Color(0.16f, 0.64f, 0.92f, 0.055f), ShellRenderQueue);
        ringMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Z Axis Ring Material"), WithAlpha(AxisZColor, 0.30f), RingRenderQueue);
        ringXMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Y Axis Ring Material"), WithAlpha(AxisYColor, 0.30f), RingRenderQueue);
        ringZMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("X Axis Ring Material"), WithAlpha(AxisXColor, 0.30f), RingRenderQueue);
        gridMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Grid Material"), new Color(0.55f, 0.95f, 1.0f, 0.11f), GridRenderQueue);
        centerMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Center Material"), new Color(0.40f, 1.0f, 0.62f, 0.9f), MarkerRenderQueue);
        userHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("User Height Stem Material"), new Color(0.40f, 1.0f, 0.62f, 0.26f), MarkerRenderQueue);
        targetMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Material"), new Color(1.0f, 0.70f, 0.18f, 0.9f), MarkerRenderQueue);
        selectedTargetRingXMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Selected Target X Ring Material"), new Color(1.0f, 0.24f, 0.16f, 0.78f), MarkerRenderQueue + 1);
        selectedTargetRingYMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Selected Target Y Ring Material"), new Color(0.24f, 1.0f, 0.42f, 0.88f), MarkerRenderQueue + 1);
        selectedTargetRingZMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Selected Target Z Ring Material"), new Color(0.30f, 0.58f, 1.0f, 0.92f), MarkerRenderQueue + 1);
        selectedViewCueMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Selected View Cue Material"), new Color(1.0f, 0.46f, 0.94f, 0.82f), MarkerRenderQueue + 1);
        targetHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Height Stem Material"), new Color(1.0f, 0.70f, 0.18f, 0.26f), MarkerRenderQueue);
        targetDropMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Target Drop Material"), new Color(1.0f, 0.70f, 0.18f, 0.35f), MarkerRenderQueue);
        lastTargetMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Last Target Material"), new Color(1.0f, 0.48f, 0.12f, 0.32f), MarkerRenderQueue);
        lastTargetDropMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Last Target Drop Material"), new Color(1.0f, 0.48f, 0.12f, 0.15f), MarkerRenderQueue);
        availableHeightStemMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Available Height Stem Material"), new Color(0.78f, 0.88f, 1.0f, 0.28f), MarkerRenderQueue);
#if FA_RADAR_PRO
        rotationAxisXMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Rotation X Axis Material"), WithAlpha(AxisXColor, 0.48f), MarkerRenderQueue);
        rotationAxisYMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Rotation Y Axis Material"), WithAlpha(AxisYColor, 0.48f), MarkerRenderQueue);
        rotationAxisZMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Rotation Z Axis Material"), WithAlpha(AxisZColor, 0.48f), MarkerRenderQueue);
        rotationAxisCenterMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Rotation Axis Center Material"), new Color(0.86f, 0.96f, 1.0f, 0.66f), MarkerRenderQueue);
        targetLabelMaterial = CreateEmissiveOverlayMaterial(BuildFilmSubjectName("Selected Label Material"), new Color(0.96f, 1.0f, 1.0f, DefaultLabelAlpha), MarkerRenderQueue);
        targetLightRangeMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Selected Light Range Material"), new Color(1.0f, 0.86f, 0.42f, 0.08f), MarkerRenderQueue - 12);
        targetSpotlightConeMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Selected Spotlight Cone Material"), new Color(1.0f, 0.86f, 0.42f, 0.08f), MarkerRenderQueue - 11);
        userPovFrustumMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("User POV Frustum Material"), new Color(0.38f, 1.0f, 0.62f, 0.055f), MarkerRenderQueue - 10);
        desktopPovFrustumMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Desktop POV Frustum Material"), new Color(0.50f, 0.84f, 1.0f, 0.055f), MarkerRenderQueue - 10);
        sceneCameraFrustumMaterial = CreateSphereShellMaterial(BuildFilmSubjectName("Scene Camera Frustum Material"), new Color(0.94f, 0.72f, 1.0f, 0.055f), MarkerRenderQueue - 10);
#endif

        sphereMesh = CreateSphereMesh(16, 32, 1.0f);
        flatCircleMesh = CreateDesktopDiskMesh(72, 1.0f);
        ringMesh = CreateRingMesh(72, 0.975f, 1.0f);
        centerMarkerMesh = CreateCenterMarkerMesh();
        targetBlipMesh = CreateTargetBlipMesh();
#if FA_RADAR_PRO
        personMarkerMesh = CreatePersonMarkerMesh();
        panelMarkerMesh = CreatePanelMarkerMesh();
        subSceneMarkerMesh = CreateSubSceneMarkerMesh();
#endif
        heightStemMesh = CreateHeightStemMesh();
#if FA_RADAR_PRO
        rotationAxisHalfPairMesh = CreateAxisHalfPairMesh();
        rotationAxisCenterCubeMesh = CreateAxisCenterCubeMesh();
        targetLabelMesh = new Mesh();
        targetLabelMesh.name = "FA Radar Selected Label Glyph Mesh";
        targetLabelMesh.MarkDynamic();
        labelLeaderMesh = CreateLabelLeaderMesh();
        lightVolumeSphereMesh = CreateSphereMesh(12, 24, 1.0f);
        spotlightConeMesh = CreateSpotlightConeMesh(40);
        povFrustumMesh = CreateFrustumMesh();
#endif
        gridMesh = CreateGridMesh(ResolveEffectiveRadarRangeMeters(), ResolveGridStepMeters(), Vector2.zero, gridClipCircleField.val);
        lastGridRangeMeters = ResolveEffectiveRadarRangeMeters();
        lastGridStepMeters = ResolveGridStepMeters();
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
        selectedTargetRingObjects = CreateTargetSelectionRingSet(BuildFilmSubjectName("Selected Target Ring"));
        selectedViewCueObject = CreateMeshObject(BuildFilmSubjectName("Selected View Cue"), axisRoot.transform, ringMesh, selectedViewCueMaterial, MarkerRenderQueue + 1, MarkerSortingOrder + 1);
        targetHeightStemObject = CreateMeshObject(BuildFilmSubjectName("Target Height Stem"), axisRoot.transform, heightStemMesh, targetHeightStemMaterial, MarkerRenderQueue, MarkerSortingOrder - 4);
        targetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Target Grid Drop"), axisRoot.transform, targetBlipMesh, targetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 1);
        lastTargetBlipObject = CreateMeshObject(BuildFilmSubjectName("Last Target Blip"), radarRoot.transform, targetBlipMesh, lastTargetMaterial, MarkerRenderQueue, MarkerSortingOrder - 2);
        lastTargetGridDropObject = CreateMeshObject(BuildFilmSubjectName("Last Target Grid Drop"), axisRoot.transform, targetBlipMesh, lastTargetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 3);
#if FA_RADAR_PRO
        targetRotationAxisObjects = CreateRotationAxisSet(BuildFilmSubjectName("Selected Rotation Axis"));
        targetLabelObject = CreateMeshObject(BuildFilmSubjectName("Selected Label"), axisRoot.transform, targetLabelMesh, targetLabelMaterial, MarkerRenderQueue, MarkerSortingOrder - 7);
        targetLabelLeaderObject = CreateMeshObject(BuildFilmSubjectName("Selected Label Leader"), axisRoot.transform, labelLeaderMesh, targetLabelMaterial, MarkerRenderQueue, MarkerSortingOrder - 8);
        targetLightRangeObject = CreateMeshObject(BuildFilmSubjectName("Selected Light Range"), axisRoot.transform, lightVolumeSphereMesh, targetLightRangeMaterial, MarkerRenderQueue - 12, MarkerSortingOrder - 12);
        targetSpotlightConeObject = CreateMeshObject(BuildFilmSubjectName("Selected Spotlight Cone"), axisRoot.transform, spotlightConeMesh, targetSpotlightConeMaterial, MarkerRenderQueue - 11, MarkerSortingOrder - 11);
        userPovFrustumObject = CreateMeshObject(BuildFilmSubjectName("User POV Frustum"), axisRoot.transform, povFrustumMesh, userPovFrustumMaterial, MarkerRenderQueue - 10, MarkerSortingOrder - 10);
        desktopPovFrustumObject = CreateMeshObject(BuildFilmSubjectName("Desktop POV Frustum"), axisRoot.transform, povFrustumMesh, desktopPovFrustumMaterial, MarkerRenderQueue - 10, MarkerSortingOrder - 10);
#endif

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
        SetTargetSelectionRingSetVisible(false);
        SetActiveIfChanged(selectedViewCueObject, false);
        SetActiveIfChanged(targetHeightStemObject, false);
        SetActiveIfChanged(targetGridDropObject, false);
        SetActiveIfChanged(lastTargetBlipObject, false);
        SetActiveIfChanged(lastTargetGridDropObject, false);
#if FA_RADAR_PRO
        SetRotationAxisSetVisible(targetRotationAxisObjects, false);
        SetActiveIfChanged(targetLabelObject, false);
        SetActiveIfChanged(targetLabelLeaderObject, false);
        SetActiveIfChanged(targetLightRangeObject, false);
        SetActiveIfChanged(targetSpotlightConeObject, false);
        SetActiveIfChanged(userPovFrustumObject, false);
        SetActiveIfChanged(desktopPovFrustumObject, false);
#endif

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
        UpdatePluginSurfaceStatus();

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
            SetStatus("Hidden by FAAR radarHudVisible=false.");
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

        UpdateWristCompassReveal(viewer);
        SetRadarVisualsVisible(ResolveRadarRuntimeVisible(viewer));
        PollSelectionIfDue();
        RadarFrame frame = BuildRadarFrame(viewer);
        PollAvailableAtomsIfDue(frame);
        frame.signature = BuildRadarFrameSignature(frame);
        TrackAttachedAtomPlacement(viewer);
        RefreshGridMeshIfNeeded(frame);
        UpdateMaterialsIfNeeded();
#if FA_RADAR_PRO
        UpdateGrabThrowPin(viewer);
#endif
        UpdateSessionGrabHandles(viewer);
        UpdateRadarDish(viewer);
        UpdateUserMarker(viewer);

        Transform target = ResolveAtomRootTransform(selectedAtom);
        bool hasSelection = target != null;
        bool showSelectedGroundDrop = hasSelection && selectedGroundDropEnabledField.val;
        SetActiveIfChanged(targetBlipObject, hasSelection);
        SetActiveIfChanged(targetHeightStemObject, hasSelection && heightStemsEnabledField.val);
        SetActiveIfChanged(targetGridDropObject, showSelectedGroundDrop);

        if (hasSelection)
        {
            UpdateTargetBlip(frame, target, showSelectedGroundDrop);
        }
#if FA_RADAR_PRO
        else
        {
            SetTargetSelectionRingSetVisible(false);
            SetActiveIfChanged(selectedViewCueObject, false);
            SetRotationAxisSetVisible(targetRotationAxisObjects, false);
            SetSelectedLabelVisible(false);
            SetActiveIfChanged(targetLightRangeObject, false);
            SetActiveIfChanged(targetSpotlightConeObject, false);
        }
#else
        if (!hasSelection)
        {
            SetTargetSelectionRingSetVisible(false);
            SetActiveIfChanged(selectedViewCueObject, false);
        }
#endif

        SetActiveIfChanged(lastTargetBlipObject, false);
        SetActiveIfChanged(lastTargetGridDropObject, false);
        UpdateAvailableAtomMarkers(frame);
        UpdateAvailableAtomMarkerStatus();
#if FA_RADAR_PRO
        UpdateProCameraFrustums(viewer);
#endif
        HandleDesktopRadarRangeScroll(viewer);
        HandleRadarMarkerClick();
    }

    private void ApplyCuaAnchorPresetMode()
    {
        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        if (cuaAnchorPresetApplied || anchorHost == null || !IsCuaPreferenceProfileActive())
        {
            return;
        }

        SetBoolNoCallback(cuaAnchorPresetField, true);
        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetBoolNoCallback(radarEnabledField, true);
        SetStringNoCallback(anchorModeField, AnchorModeContainingAtom);
        SetStringNoCallback(anchorAtomUidField, anchorHost.uid ?? "");
        ApplyCreatorAnchorPlacementDefaultsNoCallback();
        LoadGlobalPreferences();
        SetBoolNoCallback(globalPrefsAutoSaveField, true);
        SetStringNoCallback(anchorModeField, AnchorModeContainingAtom);
        SetStringNoCallback(anchorAtomUidField, anchorHost.uid ?? "");
        SetRadarModeNoCallback(RadarModeHud);
        haveSmoothedHudPosition = false;
        cuaAnchorPresetApplied = true;
        SetStatus("Creator anchor preset active.");
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
        if (hudRoot == null)
        {
            return;
        }

        radarVisibilityTargetAlpha = visible ? 1.0f : 0.0f;
        if (!radarVisibilityAlphaInitialized)
        {
            radarVisibilityAlphaInitialized = true;
            radarVisibilityAlpha = radarVisibilityTargetAlpha;
            SetMaterialAlphaMultiplier(radarVisibilityAlpha);
            SetActiveIfChanged(hudRoot, visible);
            return;
        }

        if (visible)
        {
            SetActiveIfChanged(hudRoot, true);
        }

        float step = RadarVisibilityFadeSeconds > 0.0f
            ? Mathf.Max(Time.unscaledDeltaTime, 0.001f) / RadarVisibilityFadeSeconds
            : 1.0f;
        radarVisibilityAlpha = Mathf.MoveTowards(radarVisibilityAlpha, radarVisibilityTargetAlpha, step);
        SetMaterialAlphaMultiplier(radarVisibilityAlpha);

        if (!visible && radarVisibilityAlpha <= 0.001f)
        {
            SetActiveIfChanged(hudRoot, false);
        }
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

        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        if (ignoreContainingAtomField.val && nextAtom != null && anchorHost != null && nextAtom == anchorHost)
        {
            nextAtom = null;
        }

        if (nextAtom != null && (IsRadarUtilityAtom(nextAtom) || IsNavigationPanelAtom(nextAtom)))
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
        selectedAtomRecord = null;
        availableMarkersDirty = true;
        availableAtomRevision++;

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
        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        if (!placementModeField.val || anchorHost == null || anchorHost.mainController == null)
        {
            return;
        }

        Vector3 worldPosition = anchorHost.mainController.transform.position;
        Transform anchor = ResolveRadarAnchorTransform(ResolveAnchorMode());
        Vector3 localOffset = anchor != null
            ? anchor.InverseTransformPoint(worldPosition)
            : viewer.InverseTransformPoint(worldPosition);
        SetHudOffset(localOffset);
    }

    private void UpdateRadarDish(Transform viewer)
    {
        float visualRadius = ResolveVisualRadius();
        float surfaceLocalRadius = ResolveRadarSurfaceLocalRadius();
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
        flatCircleObject.transform.localScale = Vector3.one * surfaceLocalRadius;
        SetActiveIfChanged(flatCircleObject, flatDesktop);

        sphereObject.transform.localPosition = Vector3.zero;
        sphereObject.transform.localRotation = Quaternion.identity;
        sphereObject.transform.localScale = Vector3.one * surfaceLocalRadius;
        SetActiveIfChanged(sphereObject, !flatDesktop);

        Vector3 gridReferencePosition = ResolveGridReferencePosition(viewer);
        gridObject.transform.localPosition = new Vector3(0.0f, ResolveHeightRadarY(-gridReferencePosition.y) * visualRadius, 0.0f);
        gridObject.transform.localRotation = Quaternion.identity;
        gridObject.transform.localScale = Vector3.one * surfaceLocalRadius;
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
            ring.transform.localScale = Vector3.one * (surfaceLocalRadius * 1.015f);
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

    private void ConnectTrackedHandRuntime()
    {
        GameObject root = GameObject.Find(TrackedHandRuntimeRoot);
        if (root == null)
        {
            return;
        }

        trackedHandRuntimeRoot = root;
        trackedPalmAnchors[GrabHandLeft] =
            root.transform.Find(LeftPalmSegmentName);
        trackedPalmAnchors[GrabHandRight] =
            root.transform.Find(RightPalmSegmentName);
        root.SendMessage(
            "RegisterHandStateReceiver",
            gameObject,
            SendMessageOptions.RequireReceiver);
    }

    private void DisconnectTrackedHandRuntime()
    {
        GameObject root = trackedHandRuntimeRoot;
        trackedHandRuntimeRoot = null;
        for (int hand = GrabHandLeft; hand <= GrabHandRight; hand++)
        {
            trackedPalmAnchors[hand] = null;
            trackedHandsLive[hand] = false;
            trackedPalmsPresented[hand] = false;
            trackedIndexPinched[hand] = false;
            trackedMiddlePinched[hand] = false;
            trackedHoldGrabLatched[hand] = false;
        }
        if (root == null) return;
        try
        {
            root.SendMessage(
                "UnregisterHandStateReceiver",
                gameObject,
                SendMessageOptions.DontRequireReceiver);
        }
        catch
        {
        }
    }

    public void ApplyHandRuntimeStateJson(string json)
    {
        TrackedHandRuntimeState state =
            JsonUtility.FromJson<TrackedHandRuntimeState>(json);
        if (state == null || !string.Equals(
            state.schema, TrackedHandStateSchema, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "FAAR tracked-hand state is invalid.");
        }

        trackedHandsLive[GrabHandLeft] = state.leftTracking;
        trackedHandsLive[GrabHandRight] = state.rightTracking;
        trackedIndexPinched[GrabHandLeft] = state.leftIndexPinched;
        trackedIndexPinched[GrabHandRight] = state.rightIndexPinched;
        trackedMiddlePinched[GrabHandLeft] = state.leftMiddlePinched;
        trackedMiddlePinched[GrabHandRight] = state.rightMiddlePinched;
        trackedHoldGrabLatched[GrabHandLeft] = state.leftHoldGrabLatched;
        trackedHoldGrabLatched[GrabHandRight] = state.rightHoldGrabLatched;
        trackedPalmsPresented[GrabHandLeft] = state.leftPalmPresented;
        trackedPalmsPresented[GrabHandRight] = state.rightPalmPresented;
    }

    private void ApplyHudAnchor(Transform viewer)
    {
        if (hudRoot == null || viewer == null)
        {
            return;
        }

        if (IsRoomCompassModeActive())
        {
            ApplyRoomCompassAnchor();
            return;
        }

        if (moveGrabWorldOverrideActive)
        {
            ApplyMoveGrabWorldAnchor(viewer);
            return;
        }

        string anchorMode = ResolveAnchorMode();
        if (IsWristCompassModeActive())
        {
            Transform wristAnchor = ResolveWristCompassAnchorTransform();
            if (wristAnchor != null)
            {
                ApplyWristAnchor(wristAnchor, viewer);
                return;
            }
        }

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

    private void ApplyRoomCompassAnchor()
    {
        if (hudRoot == null)
        {
            return;
        }

        if (hudRoot.transform.parent != null)
        {
            hudRoot.transform.SetParent(null, false);
            currentHudAnchor = null;
            haveSmoothedHudPosition = false;
        }

        hudRoot.transform.position = Vector3.zero;
        hudRoot.transform.rotation = Quaternion.identity;
        hudRoot.transform.localScale = Vector3.one * ResolveHudScale();
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
            hudRoot.transform.localScale = Vector3.one * ResolveHudScale();
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
        hudRoot.transform.localScale = Vector3.one * ResolveHudScale();
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
        hudRoot.transform.localScale = Vector3.one * ResolveHudScale();
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
        hudRoot.transform.localScale = Vector3.one * ResolveHudScale();
    }

    private void ApplyWristAnchor(Transform wristAnchor, Transform viewer)
    {
        if (hudRoot == null || wristAnchor == null)
        {
            return;
        }

        if (hudRoot.transform.parent != null)
        {
            hudRoot.transform.SetParent(null, true);
            currentHudAnchor = null;
            haveSmoothedHudPosition = false;
        }

        hudRoot.transform.position = wristAnchor.TransformPoint(GetWristOffset());
        hudRoot.transform.rotation = viewer != null ? viewer.rotation : Quaternion.identity;
        hudRoot.transform.localScale = Vector3.one * ResolveWristScale();
    }

    private void ApplyMoveGrabWorldAnchor(Transform viewer)
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

        hudRoot.transform.position = moveGrabCurrentRadarWorldCenter;
        hudRoot.transform.rotation = IsWristCompassModeActive() && viewer != null
            ? viewer.rotation
            : moveGrabStartRadarWorldRotation;
        hudRoot.transform.localScale = Vector3.one * ResolveActivePlacementScale();
    }

    private Transform ResolveRadarAnchorTransform(string anchorMode)
    {
        if (string.Equals(anchorMode, AnchorModeContainingAtom, StringComparison.Ordinal))
        {
            return ResolveAtomRootTransform(ResolveAttachedAtomAnchorHost());
        }

        if (string.Equals(anchorMode, AnchorModeAtomUid, StringComparison.Ordinal))
        {
            return ResolveAtomRootTransform(ResolveAnchorAtom());
        }

        return null;
    }

    private Atom ResolveAnchorAtom()
    {
        return ResolveAnchorAtomCached();
    }

    private Atom ResolveAnchorAtomCached()
    {
        string uid = anchorAtomUidField != null ? (anchorAtomUidField.val ?? "") : "";
        string trimmedUid = uid.Trim();
        if (string.Equals(trimmedUid, cachedAnchorAtomUid, StringComparison.OrdinalIgnoreCase)
            && cachedAnchorAtom != null
            && string.Equals(cachedAnchorAtom.uid, trimmedUid, StringComparison.OrdinalIgnoreCase))
        {
            return cachedAnchorAtom;
        }

        cachedAnchorAtomUid = trimmedUid;
        cachedAnchorAtom = FindAtomByUid(trimmedUid);
        return cachedAnchorAtom;
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

    private bool IsWristCompassModeActive()
    {
        return IsRadarModeWrist(ResolveRadarMode());
    }

    private bool ResolveRadarRuntimeVisible(Transform viewer)
    {
        if (!recorderRadarVisible || radarEnabledField == null || !radarEnabledField.val)
        {
            return false;
        }

        if (!IsWristCompassModeActive())
        {
            return true;
        }

        if (IsWristCompassAlwaysOn())
        {
            return ResolveWristCompassAnchorTransform() != null;
        }

        return wristCompassRevealed;
    }

    private void UpdateWristCompassReveal(Transform viewer)
    {
        if (!IsWristCompassModeActive())
        {
            wristCompassRevealed = false;
            wristRevealGraceUntil = 0.0f;
            return;
        }

        if (moveGrabActive)
        {
            wristCompassRevealed = true;
            wristRevealGraceUntil = Time.unscaledTime + WristRevealGraceSeconds;
            return;
        }

        if (IsPalmCompassModeActive())
        {
            int palm = ResolveWristCompassHand();
            wristCompassRevealed = trackedHandsLive[palm]
                && trackedPalmsPresented[palm]
                && ResolveTrackedPalmTransform(palm) != null;
            wristRevealGraceUntil = 0f;
            return;
        }

        if (IsWristCompassAlwaysOn())
        {
            wristCompassRevealed = ResolveWristCompassAnchorTransform() != null;
            return;
        }

        Transform wristAnchor = ResolveWristCompassAnchorTransform();
        if (wristAnchor == null)
        {
            wristCompassRevealed = false;
            return;
        }

        if (Time.unscaledTime < wristRevealGraceUntil)
        {
            wristCompassRevealed = true;
            return;
        }

        int hand = ResolveWristCompassHand();
        float twistDegrees = ResolveControllerOutwardTwistDegrees(wristAnchor, hand, viewer);
        float threshold = Mathf.Clamp(ReadFloat(wristTwistDegreesField, 65.0f), 15.0f, 120.0f);
        float releaseThreshold = Mathf.Max(0.0f, threshold - 12.0f);
        if (!wristCompassRevealed && twistDegrees >= threshold)
        {
            wristCompassRevealed = true;
            if (IsMotionControllerTransform(wristAnchor, hand))
            {
                PulseGrabHandleHaptics(hand, 0.22f, 0.22f, 0.035f);
            }
            SetStatus("Wrist compass revealed.");
        }
        else if (wristCompassRevealed && twistDegrees < releaseThreshold)
        {
            wristCompassRevealed = false;
        }
    }

    private Transform ResolveWristCompassAnchorTransform()
    {
        if (IsPalmCompassModeActive())
        {
            return ResolveTrackedPalmTransform(ResolveWristCompassHand());
        }
        return ResolveHandOrControllerTransform(ResolveWristCompassHand());
    }

    private Transform ResolveTrackedPalmTransform(int hand)
    {
        if (hand != GrabHandLeft && hand != GrabHandRight) return null;
        if (!trackedHandsLive[hand]) return null;
        GameObject root = trackedHandRuntimeRoot;
        if (root == null) return null;
        Transform anchor = trackedPalmAnchors[hand];
        if (anchor == null)
        {
            anchor = root.transform.Find(hand == GrabHandLeft
                ? LeftPalmSegmentName : RightPalmSegmentName);
            trackedPalmAnchors[hand] = anchor;
        }
        return IsActiveTransform(anchor) ? anchor : null;
    }

    private Transform ResolveHandOrControllerTransform(int hand)
    {
        Transform controllerTransform = ResolveMotionControllerTransform(hand);
        if (controllerTransform != null)
        {
            return controllerTransform;
        }

        Transform handTransform;
        if (TryResolveHandTransform(hand, out handTransform))
        {
            return handTransform;
        }

        return null;
    }

    private bool TryResolveHandTransform(int hand, out Transform handTransform)
    {
        handTransform = null;
        if (SuperController.singleton == null)
        {
            return false;
        }

        try
        {
            if (hand == GrabHandLeft)
            {
                if (IsActiveTransform(SuperController.singleton.leftHand))
                {
                    handTransform = SuperController.singleton.leftHand;
                    return true;
                }

                if (IsActiveTransform(SuperController.singleton.leftHandAlternate))
                {
                    handTransform = SuperController.singleton.leftHandAlternate;
                    return true;
                }

                return false;
            }

            if (IsActiveTransform(SuperController.singleton.rightHand))
            {
                handTransform = SuperController.singleton.rightHand;
                return true;
            }

            if (IsActiveTransform(SuperController.singleton.rightHandAlternate))
            {
                handTransform = SuperController.singleton.rightHandAlternate;
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static bool IsActiveTransform(Transform candidate)
    {
        return candidate != null && candidate.gameObject.activeInHierarchy;
    }

    private bool IsMotionControllerTransform(Transform candidate, int hand)
    {
        Transform controller = ResolveMotionControllerTransform(hand);
        return candidate != null && controller != null && candidate == controller;
    }

    private int ResolveWristCompassHand()
    {
        string radarMode = ResolveRadarMode();
        return string.Equals(radarMode, RadarModeWristRight, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModeWristRightAlwaysOn, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModePalmRight, StringComparison.Ordinal)
            ? GrabHandRight
            : GrabHandLeft;
    }

    private bool IsPalmCompassModeActive()
    {
        string radarMode = ResolveRadarMode();
        return string.Equals(radarMode, RadarModePalmLeft, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModePalmRight, StringComparison.Ordinal);
    }

    private bool IsWristCompassAlwaysOn()
    {
        string radarMode = ResolveRadarMode();
        return string.Equals(radarMode, RadarModeWristLeftAlwaysOn, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModeWristRightAlwaysOn, StringComparison.Ordinal);
    }

    private string ResolveRadarMode()
    {
        return NormalizeRadarMode(radarModeField != null ? radarModeField.val : "");
    }

    private static bool IsRadarModeWrist(string radarMode)
    {
        return string.Equals(radarMode, RadarModeWristLeft, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModeWristRight, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModeWristLeftAlwaysOn, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModeWristRightAlwaysOn, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModePalmLeft, StringComparison.Ordinal)
            || string.Equals(radarMode, RadarModePalmRight, StringComparison.Ordinal);
    }

    private static string NormalizeRadarMode(string value)
    {
        if (string.Equals(value, RadarModeWorld, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "World", StringComparison.OrdinalIgnoreCase))
        {
            return RadarModeWorld;
        }

        if (string.Equals(value, RadarModeWristLeft, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModeWristLeft;
        }

        if (string.Equals(value, RadarModeWristRight, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModeWristRight;
        }

        if (string.Equals(value, RadarModeWristLeftAlwaysOn, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModeWristLeftAlwaysOn;
        }

        if (string.Equals(value, RadarModeWristRightAlwaysOn, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModeWristRightAlwaysOn;
        }

        if (string.Equals(value, RadarModePalmLeft, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModePalmLeft;
        }

        if (string.Equals(value, RadarModePalmRight, StringComparison.OrdinalIgnoreCase))
        {
            return RadarModePalmRight;
        }

        return RadarModeHud;
    }

    private float ResolveControllerOutwardTwistDegrees(Transform controller, int hand, Transform viewer)
    {
        if (controller == null)
        {
            return 0.0f;
        }

        Vector3 outward = viewer != null
            ? (hand == GrabHandLeft ? -viewer.right : viewer.right)
            : (hand == GrabHandLeft ? Vector3.left : Vector3.right);
        outward.Normalize();
        Vector3 controllerUp = controller.up.normalized;
        float outwardAmount = Mathf.Max(0.0f, Vector3.Dot(controllerUp, outward));
        float upAmount = Mathf.Max(0.0f, Vector3.Dot(controllerUp, Vector3.up));
        return Mathf.Atan2(outwardAmount, upAmount) * Mathf.Rad2Deg;
    }

    private string ResolveAnchorMode()
    {
        if (string.Equals(ResolveRadarMode(), RadarModeWorld, StringComparison.Ordinal))
        {
            return AnchorModeWorldStatic;
        }

        if (IsEmptyAnchorHostActive())
        {
            return AnchorModeContainingAtom;
        }

        string placement = ResolveSceneSessionPlacement();
        if (string.Equals(placement, DesktopPlacementAttachedToUi, StringComparison.Ordinal))
        {
            return AnchorModeHud;
        }

        if (string.Equals(placement, DesktopPlacementPinnedInWorld, StringComparison.Ordinal))
        {
            return AnchorModeWorldStatic;
        }

        string value = anchorModeField != null ? anchorModeField.val : "";
        return NormalizeAnchorMode(value);
    }

    private bool IsDesktopPlacementAttachedToUi()
    {
        return string.Equals(ResolveSceneSessionPlacement(), DesktopPlacementAttachedToUi, StringComparison.Ordinal);
    }

    private string ResolveSceneSessionPlacement()
    {
        return IsVrDisplayActive()
            ? ResolveVRPlacement()
            : ResolveDesktopPlacement();
    }

    private string ResolveDesktopPlacement()
    {
        string value = desktopPlacementField != null ? desktopPlacementField.val : "";
        if (string.IsNullOrEmpty(value))
        {
            return ResolveDefaultDesktopPlacement();
        }

        return NormalizeDesktopPlacement(value);
    }

    private string ResolveVRPlacement()
    {
        string value = vrPlacementField != null ? vrPlacementField.val : "";
        if (string.IsNullOrEmpty(value))
        {
            return ResolveDefaultVRPlacement();
        }

        return NormalizeDesktopPlacement(value);
    }

    private string ResolveDefaultDesktopPlacement()
    {
        return IsEmptyAnchorHostActive()
            ? DesktopPlacementPinnedInWorld
            : DesktopPlacementAttachedToUi;
    }

    private string ResolveDefaultVRPlacement()
    {
        return DesktopPlacementAttachedToUi;
    }

    private static string NormalizeDesktopPlacement(string value)
    {
        if (string.Equals(value, DesktopPlacementPinnedInWorld, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Pinned In-World", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Pinned", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "World Static", StringComparison.OrdinalIgnoreCase))
        {
            return DesktopPlacementPinnedInWorld;
        }

        return DesktopPlacementAttachedToUi;
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

    private RadarFrame BuildRadarFrame(Transform viewer)
    {
        RadarFrame frame = new RadarFrame();
        frame.viewer = viewer;
        frame.referencePosition = ResolveRadarReferencePosition(viewer);
        frame.referenceRotation = ResolveRadarReferenceRotation(viewer);
        frame.inverseReferenceRotation = Quaternion.Inverse(frame.referenceRotation);
        frame.rangeMeters = ResolveEffectiveRadarRangeMeters();
        frame.heightScaleMeters = ResolveEffectiveHeightScaleMeters();
        frame.visualRadius = ResolveVisualRadius();
        frame.flattenY = ShouldFlattenRadarY();
        frame.signature = BuildRadarFrameSignature(frame);
        return frame;
    }

    private int BuildRadarFrameSignature(RadarFrame frame)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + Quantize(frame.referencePosition.x, AvailableMarkerFrameMoveThresholdMeters);
            hash = (hash * 31) + Quantize(frame.referencePosition.y, AvailableMarkerFrameMoveThresholdMeters);
            hash = (hash * 31) + Quantize(frame.referencePosition.z, AvailableMarkerFrameMoveThresholdMeters);
            Vector3 euler = frame.referenceRotation.eulerAngles;
            hash = (hash * 31) + Quantize(euler.x, AvailableMarkerFrameRotateThresholdDegrees);
            hash = (hash * 31) + Quantize(euler.y, AvailableMarkerFrameRotateThresholdDegrees);
            hash = (hash * 31) + Quantize(euler.z, AvailableMarkerFrameRotateThresholdDegrees);
            hash = (hash * 31) + Quantize(frame.rangeMeters, 0.01f);
            hash = (hash * 31) + Quantize(frame.heightScaleMeters, 0.01f);
            hash = (hash * 31) + Quantize(frame.visualRadius, 0.001f);
            hash = (hash * 31) + (frame.flattenY ? 1 : 0);
            hash = (hash * 31) + availableAtomRevision;
            return hash;
        }
    }

    private int Quantize(float value, float step)
    {
        return Mathf.RoundToInt(value / Mathf.Max(0.00001f, step));
    }

    private Quaternion ResolveDishLocalRotation()
    {
        if (!ShouldFlattenRadarY())
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(Mathf.Clamp(desktopTiltDegreesField.val, 0.0f, 90.0f), 0.0f, 0.0f);
    }

    private bool ShouldFlattenRadarY()
    {
        return desktopTopDownField != null && desktopTopDownField.val && !IsVrDisplayActive();
    }

    private void UpdateTargetBlip(RadarFrame frame, Transform target, bool showGroundDrop)
    {
        if (selectedAtomRecord == null || selectedAtomRecord.atom != selectedAtom)
        {
            selectedAtomRecord = BuildAtomRecord(selectedAtom, frame, -1);
        }
        RefreshAtomRecordTransform(selectedAtomRecord, frame);

        float visualRadius = frame.visualRadius;
        Vector3 targetWorldPosition = selectedAtomRecord != null ? selectedAtomRecord.markerWorldPosition : (target != null ? target.position : Vector3.zero);
        Vector3 radarLocal = ResolveSelectedWorldPositionRadarLocal(frame, targetWorldPosition);
        Vector3 groundLocal = ResolveTargetGroundRadarLocal(frame, targetWorldPosition);
        float distanceMeters = ResolveWorldDistanceMeters(frame, targetWorldPosition);
        float fadeAlpha = ResolveSelectedRangeFadeAlpha(distanceMeters);
        float depthScale = ResolveDepthScale(distanceMeters);
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * depthScale;
#if FA_RADAR_PRO
        if (selectedAtomRecord != null && HasCategory(selectedAtomRecord, AtomCategoryLight))
        {
            markerScale *= Mathf.Clamp(ReadFloat(lightMarkerScaleField, 0.28f), 0.12f, 1.0f);
        }
#endif
        float spin = Time.time * Mathf.Max(0.0f, ringRotationSpeedField.val * 1.75f);

        ApplyMarkerMeshForAtom(targetBlipObject, selectedAtom);
        PositionTargetSphere(targetBlipObject, radarLocal, visualRadius, markerScale * 0.34f, spin);
        bool selectedInsideViewerFrustum = IsSelectedTargetInsideViewerFrustum(targetWorldPosition);
        UpdateTargetSelectionRingSet(radarLocal, visualRadius, markerScale, fadeAlpha, !selectedInsideViewerFrustum);
        UpdateSelectedViewCue(radarLocal, visualRadius, markerScale, fadeAlpha, !selectedInsideViewerFrustum);
#if FA_RADAR_PRO
        Color targetColor = new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val) * 0.24f * fadeAlpha);
#else
        Color targetColor = WithAlpha(FreeAtomMarkerColor, Mathf.Clamp01(markerAlphaField.val) * 0.24f * fadeAlpha);
#endif
        ApplyMaterialColorIfChanged(targetMaterial, targetColor, Mathf.Max(0.0f, emissionStrengthField.val));
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

        if (Time.unscaledTime >= nextSelectedStatusTime)
        {
            nextSelectedStatusTime = Time.unscaledTime + AvailableMarkerStatusIntervalSeconds;
            Vector3 meterLocal = ResolveWorldMetersFromReference(frame, targetWorldPosition);
            SetStatus(string.Format(
                "Selected: {0}  x:{1:0.0}m y:{2:0.0}m z:{3:0.0}m",
                selectedUid,
                meterLocal.x,
                meterLocal.y,
                meterLocal.z));
        }
#if FA_RADAR_PRO
        UpdateSelectedAtomLabel(frame, selectedAtomRecord, target, radarLocal, markerScale, fadeAlpha);
        UpdateProTargetVisuals(frame, target, radarLocal, markerScale, fadeAlpha);
#endif
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
        Vector3 lastTargetWorldPosition = ResolveAtomMarkerWorldPosition(lastSelectedAtom, lastTarget);
        Vector3 radarLocal = ResolveWorldPositionRadarLocal(viewer, lastTargetWorldPosition);
        Vector3 groundLocal = ResolveTargetGroundRadarLocal(viewer, lastTargetWorldPosition);
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * 0.82f;
        float spin = Time.time * Mathf.Max(10.0f, ringRotationSpeedField.val);

        ApplyMarkerMeshForAtom(lastTargetBlipObject, lastSelectedAtom);
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

    private GameObject[] CreateTargetSelectionRingSet(string namePrefix)
    {
        GameObject[] rings = new GameObject[3];
        rings[0] = CreateMeshObject(namePrefix + ".X", axisRoot.transform, ringMesh, selectedTargetRingXMaterial, MarkerRenderQueue + 1, MarkerSortingOrder + 1);
        rings[1] = CreateMeshObject(namePrefix + ".Y", axisRoot.transform, ringMesh, selectedTargetRingYMaterial, MarkerRenderQueue + 1, MarkerSortingOrder + 1);
        rings[2] = CreateMeshObject(namePrefix + ".Z", axisRoot.transform, ringMesh, selectedTargetRingZMaterial, MarkerRenderQueue + 1, MarkerSortingOrder + 1);
        return rings;
    }

    private void SetTargetSelectionRingSetVisible(bool visible)
    {
        if (selectedTargetRingObjects == null)
        {
            return;
        }

        for (int i = 0; i < selectedTargetRingObjects.Length; i++)
        {
            SetActiveIfChanged(selectedTargetRingObjects[i], visible);
        }
    }

    private void UpdateTargetSelectionRingSet(Vector3 radarLocal, float visualRadius, float markerScale, float fadeAlpha, bool outsideViewerFrustum)
    {
        if (selectedTargetRingObjects == null || selectedTargetRingObjects.Length < 3)
        {
            return;
        }

        float alphaScale = outsideViewerFrustum ? 1.0f : 0.72f;
        float emission = Mathf.Max(0.0f, emissionStrengthField.val);
        ApplyMaterialColorIfChanged(selectedTargetRingXMaterial, new Color(1.0f, 0.22f, 0.16f, 0.62f * fadeAlpha * alphaScale), emission);
        ApplyMaterialColorIfChanged(selectedTargetRingYMaterial, new Color(0.22f, 1.0f, 0.42f, 0.78f * fadeAlpha * alphaScale), emission);
        ApplyMaterialColorIfChanged(selectedTargetRingZMaterial, new Color(0.30f, 0.60f, 1.0f, 0.86f * fadeAlpha * alphaScale), emission);

        Vector3 localPosition = radarLocal * visualRadius;
        float pulse = outsideViewerFrustum ? (1.0f + Mathf.Sin(Time.time * 5.5f) * 0.08f) : 1.0f;
        float ringScale = Mathf.Max(markerScale * 1.85f, visualRadius * 0.018f) * pulse;
        Quaternion spin = Quaternion.AngleAxis(Time.time * Mathf.Max(8.0f, ringRotationSpeedField.val), Vector3.forward);
        Quaternion[] rotations = new Quaternion[]
        {
            spin,
            Quaternion.Euler(90.0f, 0.0f, 0.0f) * spin,
            Quaternion.Euler(0.0f, 90.0f, 0.0f) * spin
        };

        for (int i = 0; i < selectedTargetRingObjects.Length; i++)
        {
            GameObject ring = selectedTargetRingObjects[i];
            if (ring == null)
            {
                continue;
            }

            ring.transform.localPosition = localPosition;
            ring.transform.localRotation = rotations[i];
            ring.transform.localScale = Vector3.one * ringScale;
            SetActiveIfChanged(ring, true);
        }
    }

    private void UpdateSelectedViewCue(Vector3 radarLocal, float visualRadius, float markerScale, float fadeAlpha, bool visible)
    {
        SetActiveIfChanged(selectedViewCueObject, visible);
        if (!visible || selectedViewCueObject == null)
        {
            return;
        }

        Vector3 direction = radarLocal.sqrMagnitude > 0.0001f ? radarLocal.normalized : Vector3.forward;
        selectedViewCueObject.transform.localPosition = direction * visualRadius * 1.015f;
        selectedViewCueObject.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, direction);
        selectedViewCueObject.transform.localScale = Vector3.one * Mathf.Max(markerScale * 1.35f, visualRadius * 0.016f);
        ApplyMaterialColorIfChanged(selectedViewCueMaterial, new Color(1.0f, 0.46f, 0.94f, 0.82f * fadeAlpha), Mathf.Max(0.0f, emissionStrengthField.val));
    }

    private void ApplyMarkerMeshForAtom(GameObject markerObject, Atom atom)
    {
        if (markerObject == null)
        {
            return;
        }

        MeshFilter filter = markerObject.GetComponent<MeshFilter>();
        if (filter == null)
        {
            return;
        }

        Mesh mesh = ResolveMarkerMeshForAtom(atom);
        if (mesh != null && filter.sharedMesh != mesh)
        {
            filter.sharedMesh = mesh;
        }
    }

    private Mesh ResolveMarkerMeshForAtom(Atom atom)
    {
#if FA_RADAR_PRO
        if (IsPersonAtom(atom))
        {
            return personMarkerMesh != null ? personMarkerMesh : targetBlipMesh;
        }

        if (IsSubSceneAtom(atom))
        {
            return subSceneMarkerMesh != null ? subSceneMarkerMesh : targetBlipMesh;
        }

        if (IsPanelLikeAtom(atom))
        {
            return panelMarkerMesh != null ? panelMarkerMesh : targetBlipMesh;
        }
#endif

        return targetBlipMesh;
    }

    private Mesh ResolveMarkerMeshForRecord(AtomRecord record)
    {
#if FA_RADAR_PRO
        if (HasCategory(record, AtomCategoryPerson))
        {
            return personMarkerMesh != null ? personMarkerMesh : targetBlipMesh;
        }

        if (HasCategory(record, AtomCategorySubScene))
        {
            return subSceneMarkerMesh != null ? subSceneMarkerMesh : targetBlipMesh;
        }

        if (record != null && (HasCategory(record, AtomCategoryImagePanel) || IsPanelLikeAtom(record.atom)))
        {
            return panelMarkerMesh != null ? panelMarkerMesh : targetBlipMesh;
        }
#endif

        return targetBlipMesh;
    }

    private Vector3 ResolveTargetRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        Vector3 meterLocal = ResolveWorldMetersFromReference(viewer, target.position);
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal;
        if (ShouldFlattenRadarY())
        {
            radarLocal = new Vector3(meterLocal.x, 0.0f, meterLocal.z) / range;
        }
        else
        {
            radarLocal = meterLocal / range;
        }

        return ClampRadarLocalToOuterRadius(radarLocal);
    }

    private Vector3 ResolveTargetGroundRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        return ResolveTargetGroundRadarLocal(viewer, target.position);
    }

    private Vector3 ResolveTargetGroundRadarLocal(Transform viewer, Vector3 worldPosition)
    {
        if (viewer == null)
        {
            return Vector3.zero;
        }

        Vector3 referencePosition = ResolveRadarReferencePosition(viewer);
        Vector3 worldDelta = worldPosition - referencePosition;
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal = new Vector3(
            worldDelta.x / range,
            ResolveHeightRadarY(-referencePosition.y),
            worldDelta.z / range);
        return ClampRadarLocalToOuterRadius(radarLocal);
    }

    private Vector3 ResolveTargetGroundRadarLocal(RadarFrame frame, Vector3 worldPosition)
    {
        Vector3 worldDelta = worldPosition - frame.referencePosition;
        Vector3 radarLocal = new Vector3(
            worldDelta.x / frame.rangeMeters,
            ResolveHeightRadarY(frame, -frame.referencePosition.y),
            worldDelta.z / frame.rangeMeters);
        return ClampRadarLocalToOuterRadius(radarLocal);
    }

    private Vector3 ResolveTargetWorldRadarLocal(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return Vector3.zero;
        }

        return ResolveWorldPositionRadarLocal(viewer, target.position);
    }

    private Vector3 ResolveWorldPositionRadarLocal(Transform viewer, Vector3 worldPosition)
    {
        Vector3 worldDelta = ResolveWorldMetersFromReference(viewer, worldPosition);
        float range = ResolveEffectiveRadarRangeMeters();
        Vector3 radarLocal = ShouldFlattenRadarY()
            ? new Vector3(worldDelta.x / range, 0.0f, worldDelta.z / range)
            : new Vector3(worldDelta.x / range, ResolveHeightRadarY(worldDelta.y), worldDelta.z / range);
        return ClampRadarLocalToOuterRadius(radarLocal);
    }

    private Vector3 ResolveWorldPositionRadarLocal(RadarFrame frame, Vector3 worldPosition)
    {
        return ResolveWorldPositionRadarLocal(frame, worldPosition, FarMarkerOuterRadius);
    }

    private Vector3 ResolveSelectedWorldPositionRadarLocal(RadarFrame frame, Vector3 worldPosition)
    {
        return ResolveWorldPositionRadarLocal(frame, worldPosition, SelectedTargetOuterRadius);
    }

    private Vector3 ResolveWorldPositionRadarLocal(RadarFrame frame, Vector3 worldPosition, float outerRadius)
    {
        Vector3 worldDelta = ResolveWorldMetersFromReference(frame, worldPosition);
        Vector3 radarLocal = frame.flattenY
            ? new Vector3(worldDelta.x / frame.rangeMeters, 0.0f, worldDelta.z / frame.rangeMeters)
            : new Vector3(worldDelta.x / frame.rangeMeters, ResolveHeightRadarY(frame, worldDelta.y), worldDelta.z / frame.rangeMeters);
        return ClampRadarLocalToRadius(radarLocal, outerRadius);
    }

    private Vector3 ClampRadarLocalToOuterRadius(Vector3 radarLocal)
    {
        return ClampRadarLocalToRadius(radarLocal, FarMarkerOuterRadius);
    }

    private Vector3 ClampRadarLocalToRadius(Vector3 radarLocal, float outerRadius)
    {
        if (IsRoomCompassModeActive())
        {
            return radarLocal;
        }

        float safeOuterRadius = Mathf.Max(1.0f, outerRadius);
        float maxSq = safeOuterRadius * safeOuterRadius;
        if (radarLocal.sqrMagnitude <= maxSq)
        {
            return radarLocal;
        }

        return radarLocal.normalized * safeOuterRadius;
    }

    private Vector3 ResolveWorldMetersFromReference(Transform viewer, Vector3 worldPosition)
    {
        Quaternion referenceRotation = ResolveRadarReferenceRotation(viewer);
        return Quaternion.Inverse(referenceRotation) * (worldPosition - ResolveRadarReferencePosition(viewer));
    }

    private Vector3 ResolveWorldMetersFromReference(RadarFrame frame, Vector3 worldPosition)
    {
        return frame.inverseReferenceRotation * (worldPosition - frame.referencePosition);
    }

    private bool IsStaticRadarReferenceActive()
    {
        if (IsWristCompassModeActive())
        {
            return false;
        }

        string anchorMode = ResolveAnchorMode();
        return string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal)
            || string.Equals(anchorMode, AnchorModeContainingAtom, StringComparison.Ordinal)
            || string.Equals(anchorMode, AnchorModeAtomUid, StringComparison.Ordinal);
    }

    private Vector3 ResolveRadarReferencePosition(Transform viewer)
    {
        if (IsRoomCompassModeActive())
        {
            return Vector3.zero;
        }

        if (IsStaticRadarReferenceActive())
        {
            if (moveGrabWorldOverrideActive)
            {
                return moveGrabCurrentRadarWorldCenter;
            }

            string anchorMode = ResolveAnchorMode();
            if (string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal))
            {
                return GetStaticWorldPosition();
            }

            Transform anchor = ResolveRadarAnchorTransform(anchorMode);
            if (anchor != null)
            {
                return anchor.TransformPoint(GetHudOffset());
            }

            if (hudRoot != null)
            {
                return hudRoot.transform.position;
            }
        }

        return viewer != null ? viewer.position : Vector3.zero;
    }

    private Quaternion ResolveRadarReferenceRotation(Transform viewer)
    {
        if (IsStaticRadarReferenceActive())
        {
            return ResolveGroundAxisWorldRotation();
        }

        return ResolveGroundAxisWorldRotation();
    }

    private float ResolveRadarReferenceDistanceMeters(Transform viewer, Transform target)
    {
        if (target == null)
        {
            return 0.0f;
        }

        return ResolveRadarReferenceDistanceMeters(viewer, target.position);
    }

    private float ResolveRadarReferenceDistanceMeters(Transform viewer, Vector3 worldPosition)
    {
        return Vector3.Distance(ResolveRadarReferencePosition(viewer), worldPosition);
    }

    private float ResolveHeightRadarY(float worldYDeltaMeters)
    {
        float heightScale = ResolveEffectiveHeightScaleMeters();
        return Mathf.Clamp(worldYDeltaMeters / heightScale, -1.0f, 1.0f);
    }

    private float ResolveHeightRadarY(RadarFrame frame, float worldYDeltaMeters)
    {
        return Mathf.Clamp(worldYDeltaMeters / Mathf.Max(0.25f, frame.heightScaleMeters), -1.0f, 1.0f);
    }

    private float ResolveWorldDistanceMeters(Transform viewer, Transform target)
    {
        if (viewer == null || target == null)
        {
            return 0.0f;
        }

        return ResolveRadarReferenceDistanceMeters(viewer, target);
    }

    private float ResolveWorldDistanceMeters(Transform viewer, Vector3 worldPosition)
    {
        if (viewer == null)
        {
            return 0.0f;
        }

        return ResolveRadarReferenceDistanceMeters(viewer, worldPosition);
    }

    private float ResolveWorldDistanceMeters(RadarFrame frame, Vector3 worldPosition)
    {
        return Vector3.Distance(frame.referencePosition, worldPosition);
    }

    private float ResolveRangeFadeAlpha(float distanceMeters)
    {
        if (IsRoomCompassModeActive())
        {
            return 1.0f;
        }

        float range = ResolveEffectiveRadarRangeMeters();
        float fadeMeters = Mathf.Max(0.0f, rangeFadeMetersField.val);
        if (distanceMeters <= range || fadeMeters <= 0.001f)
        {
            return 1.0f;
        }

        return Mathf.Max(FarMarkerMinimumAlpha, Mathf.Clamp01(1.0f - ((distanceMeters - range) / fadeMeters)));
    }

    private float ResolveSelectedRangeFadeAlpha(float distanceMeters)
    {
        return Mathf.Max(0.38f, ResolveRangeFadeAlpha(distanceMeters));
    }

    private bool IsSelectedTargetInsideViewerFrustum(Vector3 targetWorldPosition)
    {
        Camera camera = ResolveViewerCamera();
        if (camera == null)
        {
            return true;
        }

        Vector3 viewportPosition = camera.WorldToViewportPoint(targetWorldPosition);
        return viewportPosition.z > Mathf.Max(0.001f, camera.nearClipPlane)
            && viewportPosition.x >= 0.0f
            && viewportPosition.x <= 1.0f
            && viewportPosition.y >= 0.0f
            && viewportPosition.y <= 1.0f;
    }

    private float ResolveDepthScale(float distanceMeters)
    {
        if (IsRoomCompassModeActive() || !depthSizeCueField.val)
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

    private float ResolveDepthVisibilityAlpha(float distanceMeters)
    {
        if (IsRoomCompassModeActive() || !depthSizeCueField.val)
        {
            return 1.0f;
        }

        float range = Mathf.Max(0.001f, ResolveEffectiveRadarRangeMeters());
        float t = Mathf.Clamp01(distanceMeters / range);
        float strength = Mathf.Clamp01(depthSizeStrengthField.val);
        return Mathf.Lerp(1.0f, 0.32f, t * strength);
    }

    private float ResolveAvailableOverlayAlpha(float fadeAlpha, float depthAlpha)
    {
        float safeDepth = Mathf.Clamp01(depthAlpha);
        return Mathf.Clamp01(fadeAlpha * safeDepth * safeDepth);
    }

    private float ResolveAvailableOverlayScale(float markerScale, float depthAlpha)
    {
        return markerScale * Mathf.Lerp(0.58f, 1.0f, Mathf.Clamp01(depthAlpha));
    }

#if FA_RADAR_PRO
    private float ResolveDirectorOverlayAlpha(float fadeAlpha, float depthAlpha)
    {
        float overlayAlpha = ResolveAvailableOverlayAlpha(fadeAlpha, depthAlpha);
        float rangeFade = Mathf.Clamp01(fadeAlpha);
        float directorCeiling = Mathf.Lerp(DirectorBackgroundOverlayAlphaCeiling * 0.55f, DirectorBackgroundOverlayAlphaCeiling, rangeFade);
        return Mathf.Min(overlayAlpha, directorCeiling);
    }

    private float ResolveDirectorOverlayScale(float markerScale, float depthAlpha)
    {
        return ResolveAvailableOverlayScale(markerScale, depthAlpha) * Mathf.Lerp(0.72f, 0.92f, Mathf.Clamp01(depthAlpha));
    }
#endif

    private void UpdateUserMarker(Transform viewer)
    {
        float visualRadius = ResolveVisualRadius();
        float scaledMarker = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val);
        bool showUserMarker = IsStaticRadarReferenceActive();
        Vector3 userLocal = IsStaticRadarReferenceActive() && viewer != null
            ? ResolveWorldPositionRadarLocal(viewer, viewer.position)
            : Vector3.zero;
        Vector3 referencePosition = ResolveRadarReferencePosition(viewer);
        Vector3 userGroundLocal = new Vector3(userLocal.x, ResolveHeightRadarY(-referencePosition.y), userLocal.z);
        float spin = Time.time * Mathf.Max(0.0f, ringRotationSpeedField.val * 0.5f);

        if (centerMarkerObject != null)
        {
            SetActiveIfChanged(centerMarkerObject, showUserMarker);
            if (!showUserMarker)
            {
                UpdateHeightStem(userHeightStemObject, userLocal.x, userGroundLocal.y, userLocal.y, userLocal.z, visualRadius, false);
                return;
            }

            centerMarkerObject.transform.localPosition = userLocal * visualRadius;
            centerMarkerObject.transform.localRotation = Quaternion.AngleAxis(spin, Vector3.forward);
            centerMarkerObject.transform.localScale = Vector3.one * (scaledMarker * 0.72f);
        }

        UpdateHeightStem(userHeightStemObject, userLocal.x, userGroundLocal.y, userLocal.y, userLocal.z, visualRadius, showUserMarker && heightStemsEnabledField.val);
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

    private void PollAvailableAtomsIfDue(RadarFrame frame)
    {
        if (Time.time < nextAtomPollTime && !availableMarkersDirty)
        {
            return;
        }

        float interval = Mathf.Max(0.15f, atomPollSecondsField.val);
        nextAtomPollTime = Time.time + interval;
        trackedAvailableAtoms.Clear();
        availableAtomRecords.Clear();
        lastAvailableAtomSceneCount = 0;
        lastAvailableAtomTrackedCount = 0;
        lastAvailableAtomVisibleCount = 0;
        lastAvailableAtomRangeHiddenCount = 0;
        lastAvailableAtomMissingTargetCount = 0;
        lastAvailableAtomBudgetHiddenCount = 0;

        if (!availableAtomMarkersEnabledField.val || SuperController.singleton == null)
        {
            return;
        }

        List<Atom> atoms = SuperController.singleton.GetAtoms();
        if (atoms == null)
        {
            return;
        }
        lastAvailableAtomSceneCount = atoms.Count;

        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        int maxVisibleMarkers = ResolveMaxVisibleMarkerCount();
        int filteredCandidateCount = 0;
        for (int i = 0; i < atoms.Count; i++)
        {
            Atom atom = atoms[i];
            AtomRecord record = BuildAtomRecord(atom, frame, availableAtomRecords.Count, false);
            if (!IsAtomVisibleByFilter(record, anchorHost))
            {
                continue;
            }

            filteredCandidateCount++;
            HydrateAtomRecord(record, frame);
            InsertAvailableAtomRecordByDistance(record, maxVisibleMarkers);
        }

        trackedAvailableAtoms.Clear();
        for (int i = 0; i < availableAtomRecords.Count; i++)
        {
            AtomRecord record = availableAtomRecords[i];
            if (record == null)
            {
                continue;
            }

            record.recordId = i;
            trackedAvailableAtoms.Add(record.atom);
        }

        lastAvailableAtomTrackedCount = availableAtomRecords.Count;
        lastAvailableAtomBudgetHiddenCount = Mathf.Max(0, filteredCandidateCount - availableAtomRecords.Count);
        EnsureAvailableMarkerCapacity(availableAtomRecords.Count);
        availableAtomRevision++;
        availableMarkersDirty = true;
    }

    private AtomRecord BuildAtomRecord(Atom atom, RadarFrame frame, int recordId)
    {
        return BuildAtomRecord(atom, frame, recordId, true);
    }

    private AtomRecord BuildAtomRecord(Atom atom, RadarFrame frame, int recordId, bool hydrateMetadata)
    {
        AtomRecord record = new AtomRecord();
        record.recordId = recordId;
        record.atom = atom;
        record.uid = atom != null ? (atom.uid ?? "") : "";
#if FA_RADAR_PRO
        record.labelText = SanitizeRadarLabelText(record.uid);
#endif
        record.root = ResolveAtomRootTransform(atom);
        record.categoryFlags = ResolveAtomCategoryFlags(atom);
        record.markerMesh = ResolveMarkerMeshForRecord(record);
        if (hydrateMetadata)
        {
            HydrateAtomRecord(record, frame);
        }
        else if (TryResolveViewerAnchoredAtomMarkerWorldPosition(record, frame, out record.markerWorldPosition))
        {
            record.distanceSq = (record.markerWorldPosition - frame.referencePosition).sqrMagnitude;
        }
        else if (record.root != null)
        {
            record.markerWorldPosition = record.root.position;
            record.distanceSq = (record.markerWorldPosition - frame.referencePosition).sqrMagnitude;
        }
        return record;
    }

    private void HydrateAtomRecord(AtomRecord record, RadarFrame frame)
    {
        if (record == null)
        {
            return;
        }

        Vector3 viewerAnchoredWorldPosition;
        if (!TryResolveViewerAnchoredAtomMarkerWorldPosition(record, frame, out viewerAnchoredWorldPosition))
        {
            RecordAtomVisualCenterOffset(record);
        }
        RefreshAtomRecordTransform(record, frame);
#if FA_RADAR_PRO
        if (HasCategory(record, AtomCategoryLight))
        {
            Light ignored;
            TryResolveUnityLight(record, out ignored);
        }
#endif
        record.distanceSq = (record.markerWorldPosition - frame.referencePosition).sqrMagnitude;
    }

    private void InsertAvailableAtomRecordByDistance(AtomRecord record, int maxCount)
    {
        if (record == null || maxCount <= 0)
        {
            return;
        }

        int insertAt = availableAtomRecords.Count;
        while (insertAt > 0)
        {
            AtomRecord previous = availableAtomRecords[insertAt - 1];
            float previousDistance = previous != null ? previous.distanceSq : float.MaxValue;
            if (record.distanceSq >= previousDistance)
            {
                break;
            }

            insertAt--;
        }

        if (availableAtomRecords.Count >= maxCount && insertAt >= availableAtomRecords.Count)
        {
            return;
        }

        availableAtomRecords.Insert(insertAt, record);
        if (availableAtomRecords.Count > maxCount)
        {
            availableAtomRecords.RemoveAt(availableAtomRecords.Count - 1);
        }
    }

    private bool RefreshAtomRecordTransform(AtomRecord record, RadarFrame frame)
    {
        Vector3 viewerAnchoredWorldPosition;
        if (TryResolveViewerAnchoredAtomMarkerWorldPosition(record, frame, out viewerAnchoredWorldPosition))
        {
            if (record.root == null)
            {
                record.root = ResolveAtomRootTransform(record.atom);
            }

            bool changed = !record.hasTransformSample
                || (viewerAnchoredWorldPosition - record.markerWorldPosition).sqrMagnitude > (AvailableMarkerTransformMoveThresholdMeters * AvailableMarkerTransformMoveThresholdMeters);

            record.markerWorldPosition = viewerAnchoredWorldPosition;
            if (record.root != null)
            {
                record.lastRootPosition = record.root.position;
                record.lastRootRotation = record.root.rotation;
                record.lastRootScale = record.root.lossyScale;
            }
            record.hasTransformSample = true;
            return changed;
        }

        return RefreshAtomRecordTransform(record);
    }

    private bool RefreshAtomRecordTransform(AtomRecord record)
    {
        if (record == null)
        {
            return false;
        }

        if (record.root == null)
        {
            record.root = ResolveAtomRootTransform(record.atom);
        }

        if (record.root == null)
        {
            return false;
        }

        Vector3 rootPosition = record.root.position;
        Quaternion rootRotation = record.root.rotation;
        Vector3 rootScale = record.root.lossyScale;
        bool changed = !record.hasTransformSample
            || (rootPosition - record.lastRootPosition).sqrMagnitude > (AvailableMarkerTransformMoveThresholdMeters * AvailableMarkerTransformMoveThresholdMeters)
            || Quaternion.Angle(rootRotation, record.lastRootRotation) > AvailableMarkerFrameRotateThresholdDegrees
            || (rootScale - record.lastRootScale).sqrMagnitude > 0.000001f;

        if (!changed)
        {
            return false;
        }

        record.markerWorldPosition = record.markerLocalOffsetKnown
            ? record.root.TransformPoint(record.markerLocalOffset)
            : rootPosition;
        record.lastRootPosition = rootPosition;
        record.lastRootRotation = rootRotation;
        record.lastRootScale = rootScale;
        record.hasTransformSample = true;
        return true;
    }

    private bool TryResolveViewerAnchoredAtomMarkerWorldPosition(AtomRecord record, RadarFrame frame, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (record == null || frame.viewer == null)
        {
            return false;
        }

        // VaM's player navigation/crosshair atom is floor anchored; the useful radar point is the active viewer height.
        if (!HasCategory(record, AtomCategoryNavigationPanel))
        {
            return false;
        }

        worldPosition = frame.viewer.position;
        return true;
    }

    private void RecordAtomVisualCenterOffset(AtomRecord record)
    {
        if (record == null || record.root == null)
        {
            return;
        }

        Vector3 center;
        if (!ResolveAtomVisualBoundsCenter(record.atom, record.root, out center))
        {
            record.markerLocalOffset = Vector3.zero;
            record.markerLocalOffsetKnown = false;
            return;
        }

        record.markerLocalOffset = record.root.InverseTransformPoint(center);
        record.markerLocalOffsetKnown = true;
    }

    private int ResolveAtomCategoryFlags(Atom atom)
    {
        int flags = 0;
        if (IsLightAtom(atom))
        {
            flags |= AtomCategoryLight;
        }
        if (IsCustomUnityAssetAtom(atom))
        {
            flags |= AtomCategoryCua;
        }
        if (IsPersonAtom(atom))
        {
            flags |= AtomCategoryPerson;
            if (IsFemalePersonAtom(atom))
            {
                flags |= AtomCategoryFemale;
            }
            if (IsMalePersonAtom(atom))
            {
                flags |= AtomCategoryMale;
            }
        }
        if (IsEmptyAtom(atom))
        {
            flags |= AtomCategoryEmpty;
        }
        if (IsSubSceneAtom(atom))
        {
            flags |= AtomCategorySubScene;
        }
        if (IsImagePanelAtom(atom))
        {
            flags |= AtomCategoryImagePanel;
        }
        if (IsAnimationAtom(atom))
        {
            flags |= AtomCategoryAnimation;
        }
        if (IsForceAtom(atom))
        {
            flags |= AtomCategoryForce;
        }
        if (IsShapeAtom(atom))
        {
            flags |= AtomCategoryShape;
        }
        if (IsSoundAtom(atom))
        {
            flags |= AtomCategorySound;
        }
        if (IsTriggerAtom(atom))
        {
            flags |= AtomCategoryTrigger;
        }
        if (IsNavigationPanelAtom(atom))
        {
            flags |= AtomCategoryNavigationPanel;
        }
        if (IsCameraAtom(atom))
        {
            flags |= AtomCategoryCamera;
        }

        return flags;
    }

    private bool HasCategory(AtomRecord record, int categoryFlag)
    {
        return record != null && (record.categoryFlags & categoryFlag) != 0;
    }

    private bool IsAtomVisibleByFilter(AtomRecord record)
    {
        return IsAtomVisibleByFilter(record, ResolveAttachedAtomAnchorHost());
    }

    private bool IsAtomVisibleByFilter(AtomRecord record, Atom anchorHost)
    {
        Atom atom = record != null ? record.atom : null;
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

        if (IsRadarUtilityAtom(atom))
        {
            return false;
        }

        if (ignoreContainingAtomField.val && anchorHost != null && atom == anchorHost)
        {
            return false;
        }

#if FA_RADAR_PRO
        bool light = HasCategory(record, AtomCategoryLight);
        bool cua = HasCategory(record, AtomCategoryCua);
        bool person = HasCategory(record, AtomCategoryPerson);
        bool empty = HasCategory(record, AtomCategoryEmpty);
        bool subScene = HasCategory(record, AtomCategorySubScene);
        bool imagePanel = HasCategory(record, AtomCategoryImagePanel);
        bool animation = HasCategory(record, AtomCategoryAnimation);
        bool force = HasCategory(record, AtomCategoryForce);
        bool shape = HasCategory(record, AtomCategoryShape);
        bool sound = HasCategory(record, AtomCategorySound);
        bool trigger = HasCategory(record, AtomCategoryTrigger);
        bool navigationPanel = HasCategory(record, AtomCategoryNavigationPanel);
        bool camera = HasCategory(record, AtomCategoryCamera);
        int knownFlags = AtomCategoryLight | AtomCategoryCua | AtomCategoryPerson | AtomCategoryEmpty | AtomCategorySubScene | AtomCategoryImagePanel | AtomCategoryAnimation | AtomCategoryForce | AtomCategoryShape | AtomCategorySound | AtomCategoryTrigger | AtomCategoryNavigationPanel | AtomCategoryCamera;
        bool other = (record.categoryFlags & knownFlags) == 0;
        return
            (light && showLightAtomsField.val) ||
            (cua && showCustomUnityAssetAtomsField.val) ||
            (person && showPersonAtomsField.val) ||
            (empty && showEmptyAtomsField.val) ||
            (subScene && showSubSceneAtomsField.val) ||
            (imagePanel && showImagePanelAtomsField.val) ||
            (animation && showAnimationAtomsField.val) ||
            (force && showForceAtomsField.val) ||
            (shape && showShapeAtomsField.val) ||
            (sound && showSoundAtomsField.val) ||
            (trigger && showTriggerAtomsField.val) ||
            (navigationPanel && showNavigationPanelAtomsField.val) ||
            (camera && showCameraAtomsField.val) ||
            (other && showOtherAtomsField.val);
#else
        if (HasCategory(record, AtomCategoryNavigationPanel))
        {
            return false;
        }

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

    private bool IsRadarUtilityAtom(Atom atom)
    {
        if (atom == null)
        {
            return false;
        }

        string uid = atom.uid ?? "";
        if (uid.StartsWith("FA_Radar_", StringComparison.Ordinal)
            || uid.StartsWith("FrameAngel_Radar", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IsRadarGrabHandleAtom(atom)
            || AtomTextContains(atom, "frameangelradar")
            || AtomTextContains(atom, "frame angel radar");
    }

    private bool IsCustomUnityAssetAtom(Atom atom)
    {
        return AtomTextContains(atom, "customunityasset") || AtomTextContains(atom, "cua");
    }

    private bool IsPersonAtom(Atom atom)
    {
        return AtomTextContains(atom, "person");
    }

    private bool IsFemalePersonAtom(Atom atom)
    {
        return IsPersonAtom(atom)
            && (AtomTextContains(atom, "female")
                || AtomTextContains(atom, "woman")
                || AtomTextContains(atom, "girl"));
    }

    private bool IsMalePersonAtom(Atom atom)
    {
        return IsPersonAtom(atom)
            && !IsFemalePersonAtom(atom)
            && (AtomTextContains(atom, "male")
                || AtomTextContains(atom, "man")
                || AtomTextContains(atom, "boy"));
    }

    private bool IsEmptyAtom(Atom atom)
    {
        return AtomTextContains(atom, "empty");
    }

    private bool IsSubSceneAtom(Atom atom)
    {
        return AtomTextContains(atom, "subscene") || AtomTextContains(atom, "sub scene");
    }

    private bool IsImagePanelAtom(Atom atom)
    {
        return AtomTextContains(atom, "imagepanel") || AtomTextContains(atom, "image panel");
    }

#if FA_RADAR_PRO
    private bool IsPanelLikeAtom(Atom atom)
    {
        return IsImagePanelAtom(atom)
            || AtomTextContains(atom, "fap")
            || AtomTextContains(atom, "fapp")
            || AtomTextContains(atom, "screen")
            || AtomTextContains(atom, "panel")
            || AtomTextContains(atom, "slate")
            || AtomTextContains(atom, "surface");
    }
#endif

    private bool IsAnimationAtom(Atom atom)
    {
        return AtomTextContains(atom, "animation")
            || AtomTextContains(atom, "animationpattern")
            || AtomTextContains(atom, "timeline");
    }

    private bool IsForceAtom(Atom atom)
    {
        return AtomTextContains(atom, "force");
    }

    private bool IsShapeAtom(Atom atom)
    {
        return AtomTextContains(atom, "shape")
            || AtomTextContains(atom, "sphere")
            || AtomTextContains(atom, "cube")
            || AtomTextContains(atom, "capsule")
            || AtomTextContains(atom, "plane");
    }

    private bool IsSoundAtom(Atom atom)
    {
        return AtomTextContains(atom, "sound") || AtomTextContains(atom, "audio");
    }

    private bool IsTriggerAtom(Atom atom)
    {
        return AtomTextContains(atom, "trigger");
    }

    private bool IsNavigationPanelAtom(Atom atom)
    {
        return AtomTextContains(atom, "playernavigationpanel")
            || AtomTextContains(atom, "navigationpanel")
            || AtomTextContains(atom, "navigation panel");
    }

    private bool IsCameraAtom(Atom atom)
    {
        return AtomTextContains(atom, "windowcamera")
            || AtomTextContains(atom, "displaycontrol")
            || AtomTextContains(atom, "display control")
            || AtomTextContains(atom, "camera");
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
        if (currentCount >= requiredCount && availableMarkerSlots != null && availableMarkerSlots.Length >= currentCount)
        {
            return;
        }

        int targetCount = Mathf.Max(requiredCount, currentCount + 1);
        int pooledCount = Mathf.CeilToInt((float)targetCount / (float)AvailableMarkerPoolBlockSize) * AvailableMarkerPoolBlockSize;
        GameObject[] newMarkers = new GameObject[pooledCount];
        GameObject[] newStems = new GameObject[pooledCount];
        Material[] newMaterials = new Material[pooledCount];
        MarkerSlot[] newSlots = new MarkerSlot[pooledCount];

        for (int i = 0; i < currentCount; i++)
        {
            newMarkers[i] = availableMarkerObjects[i];
            newStems[i] = availableStemObjects[i];
            newMaterials[i] = availableMarkerMaterials[i];
            newSlots[i] = availableMarkerSlots != null && i < availableMarkerSlots.Length && availableMarkerSlots[i] != null
                ? availableMarkerSlots[i]
                : new MarkerSlot();
            newSlots[i].markerObject = newMarkers[i];
            newSlots[i].stemObject = newStems[i];
            newSlots[i].markerMaterial = newMaterials[i];
            newSlots[i].markerFilter = newMarkers[i] != null ? newMarkers[i].GetComponent<MeshFilter>() : null;
        }

        for (int i = currentCount; i < pooledCount; i++)
        {
            MarkerSlot slot = new MarkerSlot();
            Material markerMaterial = CreateEmissiveOverlayMaterial("FA Radar Available Atom Material " + i, new Color(0.58f, 0.74f, 1.0f, 0.34f), MarkerRenderQueue);
            newMaterials[i] = markerMaterial;
            newMarkers[i] = CreateMeshObject("FA Radar Available Atom " + i, axisRoot.transform, targetBlipMesh, markerMaterial, MarkerRenderQueue, MarkerSortingOrder - 8);
            newStems[i] = CreateMeshObject("FA Radar Available Height Stem " + i, axisRoot.transform, heightStemMesh, availableHeightStemMaterial, MarkerRenderQueue, MarkerSortingOrder - 9);
            slot.markerObject = newMarkers[i];
            slot.stemObject = newStems[i];
            slot.markerMaterial = markerMaterial;
            slot.markerFilter = newMarkers[i] != null ? newMarkers[i].GetComponent<MeshFilter>() : null;
            SetActiveIfChanged(newMarkers[i], false);
            SetActiveIfChanged(newStems[i], false);
            newSlots[i] = slot;
        }

        availableMarkerObjects = newMarkers;
        availableStemObjects = newStems;
        availableMarkerMaterials = newMaterials;
        availableMarkerSlots = newSlots;
    }

#if FA_RADAR_PRO
    private void EnsureAvailableProOverlayCapacity(int requiredCount)
    {
        int currentCount = availableLightRangeObjects != null ? availableLightRangeObjects.Length : 0;
        if (currentCount >= requiredCount)
        {
            return;
        }

        int targetCount = Mathf.Max(requiredCount, currentCount + 1);
        int pooledCount = Mathf.CeilToInt((float)targetCount / (float)AvailableMarkerPoolBlockSize) * AvailableMarkerPoolBlockSize;
        GameObject[] newAxisObjects = new GameObject[pooledCount * RotationAxisObjectCount];
        GameObject[] newLightRanges = new GameObject[pooledCount];
        GameObject[] newSpotlightCones = new GameObject[pooledCount];
        Material[] newLightRangeMaterials = new Material[pooledCount];
        Material[] newSpotlightConeMaterials = new Material[pooledCount];

        for (int i = 0; i < currentCount; i++)
        {
            if (availableRotationAxisObjects != null)
            {
                for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
                {
                    newAxisObjects[(i * RotationAxisObjectCount) + axisObject] = availableRotationAxisObjects[(i * RotationAxisObjectCount) + axisObject];
                }
            }
            if (availableLightRangeObjects != null)
            {
                newLightRanges[i] = availableLightRangeObjects[i];
            }
            if (availableSpotlightConeObjects != null)
            {
                newSpotlightCones[i] = availableSpotlightConeObjects[i];
            }
            if (availableLightRangeMaterials != null)
            {
                newLightRangeMaterials[i] = availableLightRangeMaterials[i];
            }
            if (availableSpotlightConeMaterials != null)
            {
                newSpotlightConeMaterials[i] = availableSpotlightConeMaterials[i];
            }
            AssignAvailableProOverlaySlot(i, newAxisObjects, newLightRanges, newSpotlightCones, newLightRangeMaterials, newSpotlightConeMaterials);
        }

        for (int i = currentCount; i < pooledCount; i++)
        {
            for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
            {
                newAxisObjects[(i * RotationAxisObjectCount) + axisObject] = CreateMeshObject(
                    "FA Radar Available Rotation Axis " + i + "." + ResolveRotationAxisObjectSuffix(axisObject),
                    axisRoot.transform,
                    ResolveRotationAxisObjectMesh(axisObject),
                    ResolveRotationAxisObjectMaterial(axisObject),
                    MarkerRenderQueue,
                    MarkerSortingOrder - 6);
                SetActiveIfChanged(newAxisObjects[(i * RotationAxisObjectCount) + axisObject], false);
            }

            newLightRangeMaterials[i] = CreateSphereShellMaterial("FA Radar Available Light Range Material " + i, new Color(1.0f, 0.86f, 0.42f, 0.08f), MarkerRenderQueue - 12);
            newSpotlightConeMaterials[i] = CreateSphereShellMaterial("FA Radar Available Spotlight Cone Material " + i, new Color(1.0f, 0.86f, 0.42f, 0.08f), MarkerRenderQueue - 11);
            newLightRanges[i] = CreateMeshObject("FA Radar Available Light Range " + i, axisRoot.transform, lightVolumeSphereMesh, newLightRangeMaterials[i], MarkerRenderQueue - 12, MarkerSortingOrder - 12);
            newSpotlightCones[i] = CreateMeshObject("FA Radar Available Spotlight Cone " + i, axisRoot.transform, spotlightConeMesh, newSpotlightConeMaterials[i], MarkerRenderQueue - 11, MarkerSortingOrder - 11);
            SetActiveIfChanged(newLightRanges[i], false);
            SetActiveIfChanged(newSpotlightCones[i], false);
            AssignAvailableProOverlaySlot(i, newAxisObjects, newLightRanges, newSpotlightCones, newLightRangeMaterials, newSpotlightConeMaterials);
        }

        availableRotationAxisObjects = newAxisObjects;
        availableLightRangeObjects = newLightRanges;
        availableSpotlightConeObjects = newSpotlightCones;
        availableLightRangeMaterials = newLightRangeMaterials;
        availableSpotlightConeMaterials = newSpotlightConeMaterials;
    }

    private void AssignAvailableProOverlaySlot(
        int index,
        GameObject[] axisObjects,
        GameObject[] lightRanges,
        GameObject[] spotlightCones,
        Material[] lightRangeMaterials,
        Material[] spotlightConeMaterials)
    {
        if (availableMarkerSlots == null || index < 0 || index >= availableMarkerSlots.Length || availableMarkerSlots[index] == null)
        {
            return;
        }

        MarkerSlot slot = availableMarkerSlots[index];
        if (slot.rotationAxisObjects == null)
        {
            slot.rotationAxisObjects = new GameObject[RotationAxisObjectCount];
        }
        for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
        {
            slot.rotationAxisObjects[axisObject] = axisObjects[(index * RotationAxisObjectCount) + axisObject];
        }
        slot.lightRangeObject = lightRanges[index];
        slot.spotlightConeObject = spotlightCones[index];
        slot.lightRangeMaterial = lightRangeMaterials[index];
        slot.spotlightConeMaterial = spotlightConeMaterials[index];
    }
#endif

    private void UpdateAvailableAtomMarkers(RadarFrame frame)
    {
        int trackedCount = availableAtomMarkersEnabledField.val && availableAtomRecords != null ? availableAtomRecords.Count : 0;
        bool atomTransformChanged = false;
        for (int i = 0; i < trackedCount; i++)
        {
            AtomRecord record = availableAtomRecords[i];
            if (RefreshAtomRecordTransform(record, frame))
            {
                atomTransformChanged = true;
            }
        }

        if (atomTransformChanged)
        {
            availableAtomRevision++;
            frame.signature = BuildRadarFrameSignature(frame);
        }

        if (!availableMarkersDirty && frame.signature == lastAvailableMarkerFrameSignature)
        {
#if FA_RADAR_PRO
            RefreshActiveLabelOrientations(frame);
#endif
            return;
        }

        int visibleMarkerCount = 0;
        int rangeHiddenCount = 0;
        int missingTargetCount = 0;
        float visualRadius = frame.visualRadius;
        for (int i = 0; availableMarkerSlots != null && i < availableMarkerSlots.Length; i++)
        {
            MarkerSlot slot = availableMarkerSlots[i];
            bool show = i < trackedCount;
            if (slot == null || !show)
            {
                if (slot != null)
                {
                    SetActiveIfChanged(slot.markerObject, false);
                    SetActiveIfChanged(slot.stemObject, false);
                    slot.recordId = -1;
                }
#if FA_RADAR_PRO
                SetProAvailableAtomVisualsVisible(i, false);
                SetAvailableLabelVisible(slot, false);
#endif
                continue;
            }

            AtomRecord record = availableAtomRecords[i];
            Transform target = record != null ? record.root : null;
            if (record == null || record.atom == null || target == null || !record.atom.on || record.atom.hidden)
            {
                missingTargetCount++;
                SetActiveIfChanged(slot.markerObject, false);
                SetActiveIfChanged(slot.stemObject, false);
                slot.recordId = -1;
#if FA_RADAR_PRO
                SetProAvailableAtomVisualsVisible(i, false);
                SetAvailableLabelVisible(slot, false);
#endif
                continue;
            }

            Vector3 targetWorldPosition = record.markerWorldPosition;
            Vector3 radarLocal = ResolveWorldPositionRadarLocal(frame, targetWorldPosition);
            Vector3 groundLocal = ResolveTargetGroundRadarLocal(frame, targetWorldPosition);
            float distanceMeters = ResolveWorldDistanceMeters(frame, targetWorldPosition);
            float fadeAlpha = ResolveRangeFadeAlpha(distanceMeters);

            float depthScale = ResolveDepthScale(distanceMeters);
            float depthAlpha = ResolveDepthVisibilityAlpha(distanceMeters);
            float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * 0.58f * depthScale;
#if FA_RADAR_PRO
            if (HasCategory(record, AtomCategoryLight))
            {
                markerScale *= Mathf.Clamp(ReadFloat(lightMarkerScaleField, 0.28f), 0.12f, 1.0f);
            }
#endif
            Color color = ResolveAvailableAtomColor(record, Mathf.Clamp01(availableAtomAlphaField.val) * fadeAlpha * depthAlpha);
            ApplyMaterialColorIfChanged(slot.markerMaterial, color, Mathf.Max(0.0f, emissionStrengthField.val) * 0.85f);
            if (slot.markerFilter != null && record.markerMesh != null && slot.markerMesh != record.markerMesh)
            {
                slot.markerFilter.sharedMesh = record.markerMesh;
                slot.markerMesh = record.markerMesh;
            }
            slot.recordId = record.recordId;
            SetActiveIfChanged(slot.markerObject, true);
            visibleMarkerCount++;
            PositionTargetSphere(slot.markerObject, radarLocal, visualRadius, markerScale, 0.0f);
            UpdateHeightStem(slot.stemObject, radarLocal.x, groundLocal.y, radarLocal.y, radarLocal.z, visualRadius, heightStemsEnabledField.val && (fadeAlpha * depthAlpha) > 0.08f);
#if FA_RADAR_PRO
            UpdateProAvailableAtomVisuals(i, record, radarLocal, ResolveDirectorOverlayScale(markerScale, depthAlpha), ResolveDirectorOverlayAlpha(fadeAlpha, depthAlpha));
            UpdateAvailableAtomLabel(i, slot, record, frame, radarLocal, markerScale, fadeAlpha * depthAlpha);
#endif
        }

        lastAvailableAtomTrackedCount = trackedCount;
        lastAvailableAtomVisibleCount = visibleMarkerCount;
        lastAvailableAtomRangeHiddenCount = rangeHiddenCount;
        lastAvailableAtomMissingTargetCount = missingTargetCount;
        lastAvailableMarkerFrameSignature = frame.signature;
        availableMarkersDirty = false;
    }

    private void UpdateAvailableAtomMarkerStatus()
    {
        if (Time.unscaledTime < nextAvailableMarkerStatusTime)
        {
            return;
        }

        nextAvailableMarkerStatusTime = Time.unscaledTime + AvailableMarkerStatusIntervalSeconds;
        if (selectedAtom != null || !availableAtomMarkersEnabledField.val)
        {
            return;
        }

        if (lastAvailableAtomSceneCount <= 0)
        {
            SetStatus("Markers: no scene atoms reported.");
            return;
        }

        if (lastAvailableAtomTrackedCount <= 0)
        {
            SetStatus(string.Format(
                "Markers: 0 tracked / {0} scene atoms after filters; {1} over budget.",
                lastAvailableAtomSceneCount,
                lastAvailableAtomBudgetHiddenCount));
            return;
        }

        if (lastAvailableAtomVisibleCount <= 0)
        {
            SetStatus(string.Format(
                "Markers: 0 visible / {0} tracked; {1} outside range, {2} missing target, {3} over budget.",
                lastAvailableAtomTrackedCount,
                lastAvailableAtomRangeHiddenCount,
                lastAvailableAtomMissingTargetCount,
                lastAvailableAtomBudgetHiddenCount));
        }
    }

    private int ResolveMaxVisibleMarkerCount()
    {
        return Mathf.Clamp(Mathf.RoundToInt(ReadFloat(maxVisibleMarkersField, 96.0f)), 8, 512);
    }

#if FA_RADAR_PRO
    private Mesh ResolveRotationAxisObjectMesh(int axisObject)
    {
        return axisObject == RotationAxisCenterObjectIndex ? rotationAxisCenterCubeMesh : rotationAxisHalfPairMesh;
    }

    private Material ResolveRotationAxisObjectMaterial(int axisObject)
    {
        if (axisObject == RotationAxisCenterObjectIndex)
        {
            return rotationAxisCenterMaterial;
        }
        if (axisObject == 1)
        {
            return rotationAxisYMaterial;
        }
        if (axisObject == 2)
        {
            return rotationAxisZMaterial;
        }

        return rotationAxisXMaterial;
    }

    private string ResolveRotationAxisObjectSuffix(int axisObject)
    {
        if (axisObject == RotationAxisCenterObjectIndex)
        {
            return "Center";
        }
        if (axisObject == 1)
        {
            return "Y";
        }
        if (axisObject == 2)
        {
            return "Z";
        }

        return "X";
    }

    private GameObject[] CreateRotationAxisSet(string namePrefix)
    {
        GameObject[] axisObjects = new GameObject[RotationAxisObjectCount];
        for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
        {
            axisObjects[axisObject] = CreateMeshObject(
                namePrefix + "." + ResolveRotationAxisObjectSuffix(axisObject),
                axisRoot.transform,
                ResolveRotationAxisObjectMesh(axisObject),
                ResolveRotationAxisObjectMaterial(axisObject),
                MarkerRenderQueue,
                MarkerSortingOrder - 6);
        }
        SetRotationAxisSetVisible(axisObjects, false);
        return axisObjects;
    }

    private void SetRotationAxisSetVisible(GameObject[] axisObjects, bool visible)
    {
        if (axisObjects == null)
        {
            return;
        }

        for (int i = 0; i < axisObjects.Length; i++)
        {
            SetActiveIfChanged(axisObjects[i], visible);
        }
    }

    private void SetProAvailableAtomVisualsVisible(int markerIndex, bool visible)
    {
        if (availableRotationAxisObjects != null)
        {
            int start = markerIndex * RotationAxisObjectCount;
            for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
            {
                int index = start + axisObject;
                if (index >= 0 && index < availableRotationAxisObjects.Length)
                {
                    SetActiveIfChanged(availableRotationAxisObjects[index], visible);
                }
            }
        }

        if (availableLightRangeObjects != null && markerIndex >= 0 && markerIndex < availableLightRangeObjects.Length)
        {
            SetActiveIfChanged(availableLightRangeObjects[markerIndex], visible);
        }
        if (availableSpotlightConeObjects != null && markerIndex >= 0 && markerIndex < availableSpotlightConeObjects.Length)
        {
            SetActiveIfChanged(availableSpotlightConeObjects[markerIndex], visible);
        }
    }

    private void UpdateProTargetVisuals(RadarFrame frame, Transform target, Vector3 radarLocal, float markerScale, float fadeAlpha)
    {
        if (target == null || fadeAlpha <= 0.01f)
        {
            SetRotationAxisSetVisible(targetRotationAxisObjects, false);
            SetActiveIfChanged(targetLightRangeObject, false);
            SetActiveIfChanged(targetSpotlightConeObject, false);
            return;
        }

        UpdateRotationAxisSet(targetRotationAxisObjects, 0, target, radarLocal, markerScale, fadeAlpha);

        Light light = null;
        bool hasLight = selectedAtomRecord != null && TryResolveUnityLight(selectedAtomRecord, out light);
        UpdateLightRangeVolume(targetLightRangeObject, targetLightRangeMaterial, selectedAtom, target, light, radarLocal, fadeAlpha, hasLight);
        UpdateSpotlightCone(targetSpotlightConeObject, targetSpotlightConeMaterial, selectedAtom, target, light, radarLocal, fadeAlpha, hasLight);
    }

    private void UpdateProAvailableAtomVisuals(int markerIndex, AtomRecord record, Vector3 radarLocal, float markerScale, float fadeAlpha)
    {
        Atom atom = record != null ? record.atom : null;
        Transform target = record != null ? record.root : null;
        if (atom == null || target == null || !CanRenderRichAvailableOverlay(markerIndex, record, fadeAlpha))
        {
            SetProAvailableAtomVisualsVisible(markerIndex, false);
            return;
        }

        EnsureAvailableProOverlayCapacity(markerIndex + 1);
        UpdateRotationAxisSet(availableRotationAxisObjects, markerIndex * RotationAxisObjectCount, target, radarLocal, markerScale, fadeAlpha);

        Light light = null;
        bool hasLight = TryResolveUnityLight(record, out light);
        Material rangeMaterial = availableLightRangeMaterials != null && markerIndex < availableLightRangeMaterials.Length
            ? availableLightRangeMaterials[markerIndex]
            : null;
        Material coneMaterial = availableSpotlightConeMaterials != null && markerIndex < availableSpotlightConeMaterials.Length
            ? availableSpotlightConeMaterials[markerIndex]
            : null;
        GameObject rangeObject = availableLightRangeObjects != null && markerIndex < availableLightRangeObjects.Length
            ? availableLightRangeObjects[markerIndex]
            : null;
        GameObject coneObject = availableSpotlightConeObjects != null && markerIndex < availableSpotlightConeObjects.Length
            ? availableSpotlightConeObjects[markerIndex]
            : null;
        UpdateLightRangeVolume(rangeObject, rangeMaterial, atom, target, light, radarLocal, fadeAlpha, hasLight);
        UpdateSpotlightCone(coneObject, coneMaterial, atom, target, light, radarLocal, fadeAlpha, hasLight);
    }

    private int ResolveRichOverlayBudget()
    {
        return ResolveDirectorBackgroundOverlayBudget();
    }

    private int ResolveDirectorBackgroundOverlayBudget()
    {
        int requestedBudget = Mathf.RoundToInt(ReadFloat(richOverlayBudgetField, MaxDirectorBackgroundOverlayBudget));
        return Mathf.Clamp(requestedBudget, 0, MaxDirectorBackgroundOverlayBudget);
    }

    private void HideAvailableProOverlaysOutsideBudget()
    {
        int budget = ResolveRichOverlayBudget();
        bool anyRichOverlayEnabled =
            (showRotationAxesField != null && showRotationAxesField.val) ||
            (showLightRangeVolumesField != null && showLightRangeVolumesField.val) ||
            (showSpotlightConesField != null && showSpotlightConesField.val);

        if (availableMarkerSlots == null)
        {
            return;
        }

        for (int i = 0; i < availableMarkerSlots.Length; i++)
        {
            if (!anyRichOverlayEnabled || i >= budget)
            {
                SetProAvailableAtomVisualsVisible(i, false);
            }
        }
    }

    private bool CanRenderRichAvailableOverlay(int markerIndex, AtomRecord record, float fadeAlpha)
    {
        if (markerIndex < 0 || markerIndex >= ResolveRichOverlayBudget() || record == null || fadeAlpha <= 0.01f)
        {
            return false;
        }

        bool wantsAxis = showRotationAxesField != null && showRotationAxesField.val;
        bool wantsLight = HasCategory(record, AtomCategoryLight)
            && ((showLightRangeVolumesField != null && showLightRangeVolumesField.val)
                || (showSpotlightConesField != null && showSpotlightConesField.val));
        return wantsAxis || wantsLight;
    }

    private void UpdateRotationAxisSet(GameObject[] axisObjects, int startIndex, Transform target, Vector3 radarLocal, float markerScale, float fadeAlpha)
    {
        bool show = showRotationAxesField != null && showRotationAxesField.val && target != null && fadeAlpha > 0.01f && axisObjects != null;
        if (!show)
        {
            if (axisObjects == targetRotationAxisObjects)
            {
                SetRotationAxisSetVisible(axisObjects, false);
            }
            else
            {
                for (int axisObject = 0; axisObject < RotationAxisObjectCount; axisObject++)
                {
                    int hideIndex = startIndex + axisObject;
                    if (hideIndex >= 0 && hideIndex < axisObjects.Length)
                    {
                        SetActiveIfChanged(axisObjects[hideIndex], false);
                    }
                }
            }
            return;
        }

        float visualRadius = ResolveVisualRadius();
        bool selectedAxisSet = axisObjects == targetRotationAxisObjects;
        float contextScale = selectedAxisSet ? 1.0f : Mathf.Lerp(0.55f, 1.0f, Mathf.Clamp01(fadeAlpha));
        float axisLength = Mathf.Max(markerScale * 2.4f, visualRadius * Mathf.Clamp(ReadFloat(rotationAxisLengthField, 0.085f), 0.03f, 0.75f)) * contextScale;
        float axisWidth = visualRadius * Mathf.Clamp(ReadFloat(rotationAxisWidthField, 0.0045f), 0.003f, 0.05f) * (selectedAxisSet ? 1.0f : Mathf.Lerp(0.65f, 1.0f, Mathf.Clamp01(fadeAlpha)));
        float centerSize = Mathf.Min(axisLength * 0.34f, Mathf.Max(axisWidth * 2.4f, axisLength / (float)RotationAxisVisualPieceCount));
        Quaternion targetRotation = ResolveAxisRadarRotation(target);
        Vector3 localPosition = radarLocal * visualRadius;

        for (int axisObjectIndex = 0; axisObjectIndex < RotationAxisObjectCount; axisObjectIndex++)
        {
            int index = startIndex + axisObjectIndex;
            if (index < 0 || index >= axisObjects.Length)
            {
                continue;
            }

            GameObject axisObject = axisObjects[index];
            if (axisObject == null)
            {
                continue;
            }

            axisObject.transform.localPosition = localPosition;
            if (axisObjectIndex == RotationAxisCenterObjectIndex)
            {
                axisObject.transform.localRotation = targetRotation;
                axisObject.transform.localScale = Vector3.one * centerSize;
            }
            else
            {
                axisObject.transform.localRotation = ResolveAxisLineRotation(targetRotation, axisObjectIndex);
                axisObject.transform.localScale = new Vector3(axisLength, axisWidth, axisWidth);
            }
            SetActiveIfChanged(axisObject, true);
        }
    }

    private int ResolveLabelLimit()
    {
        return Mathf.Clamp(Mathf.RoundToInt(ReadFloat(labelLimitField, DefaultLabelLimit)), 0, MaxRadarLabelLimit);
    }

    private bool ShouldShowSelectedLabel()
    {
        string mode = ResolveSceneLabelsMode();
        return !string.Equals(mode, LabelsOff, StringComparison.Ordinal);
    }

    private bool ShouldShowAvailableLabels()
    {
        return string.Equals(ResolveSceneLabelsMode(), LabelsSelectedAndNearest, StringComparison.Ordinal);
    }

    private void UpdateSelectedAtomLabel(RadarFrame frame, AtomRecord record, Transform target, Vector3 radarLocal, float markerScale, float fadeAlpha)
    {
        if (!ShouldShowSelectedLabel() || targetLabelObject == null || targetLabelMesh == null || target == null || record == null)
        {
            SetSelectedLabelVisible(false);
            return;
        }

        string text = !string.IsNullOrEmpty(record.labelText) ? record.labelText : SanitizeRadarLabelText(selectedUid);
        if (!string.Equals(targetLabelText ?? "", text ?? "", StringComparison.Ordinal))
        {
            PopulateLabelGlyphMesh(targetLabelMesh, text);
            targetLabelText = text ?? "";
        }

        float alpha = ResolveLabelEffectiveAlpha(fadeAlpha, true);
        if (alpha <= 0.08f)
        {
            SetSelectedLabelVisible(false);
            return;
        }

        ApplyMaterialColorIfChanged(
            targetLabelMaterial,
            ResolveLabelColor(record, alpha),
            Mathf.Max(0.0f, emissionStrengthField.val) * 0.78f);
        Vector3 itemLocal;
        Vector3 labelLocal;
        PositionLabelObject(targetLabelObject, frame, target, radarLocal, true, -1, out itemLocal, out labelLocal);
        UpdateLabelLeaderLine(targetLabelLeaderObject, itemLocal, labelLocal, alpha);
        SetSelectedLabelVisible(true);
    }

    private void UpdateAvailableAtomLabel(int markerIndex, MarkerSlot slot, AtomRecord record, RadarFrame frame, Vector3 radarLocal, float markerScale, float fadeAlpha)
    {
        if (slot == null || record == null || record.root == null || !ShouldShowAvailableLabels() || markerIndex < 0 || markerIndex >= ResolveLabelLimit())
        {
            SetAvailableLabelVisible(slot, false);
            return;
        }

        float alpha = ResolveLabelEffectiveAlpha(fadeAlpha, false);
        if (alpha <= 0.12f)
        {
            SetAvailableLabelVisible(slot, false);
            return;
        }

        EnsureAvailableLabelSlot(slot, markerIndex);
        if (slot.labelObject == null || slot.labelMesh == null)
        {
            return;
        }

        string text = !string.IsNullOrEmpty(record.labelText) ? record.labelText : SanitizeRadarLabelText(record.uid);
        if (!string.Equals(slot.labelText ?? "", text ?? "", StringComparison.Ordinal))
        {
            PopulateLabelGlyphMesh(slot.labelMesh, text);
            slot.labelText = text ?? "";
            if (slot.labelFilter != null)
            {
                slot.labelFilter.sharedMesh = slot.labelMesh;
            }
        }

        ApplyMaterialColorIfChanged(
            slot.labelMaterial,
            ResolveLabelColor(record, alpha),
            Mathf.Max(0.0f, emissionStrengthField.val) * 0.66f);
        Vector3 itemLocal;
        Vector3 labelLocal;
        PositionLabelObject(slot.labelObject, frame, record.root, radarLocal, false, markerIndex, out itemLocal, out labelLocal);
        UpdateLabelLeaderLine(slot.labelLeaderObject, itemLocal, labelLocal, alpha);
        SetAvailableLabelVisible(slot, true);
    }

    private void RefreshActiveLabelOrientations(RadarFrame frame)
    {
        if (!string.Equals(ResolveLabelOrientationMode(), LabelOrientationFaceViewer, StringComparison.Ordinal))
        {
            return;
        }

        Transform selectedTarget = selectedAtomRecord != null ? selectedAtomRecord.root : null;
        if (targetLabelObject != null && targetLabelObject.activeSelf && selectedTarget != null)
        {
            targetLabelObject.transform.localRotation = ResolveLabelRadarRotation(frame, selectedTarget, targetLabelObject.transform.localPosition);
        }

        int limit = ResolveLabelLimit();
        for (int i = 0; availableMarkerSlots != null && i < availableMarkerSlots.Length && i < limit; i++)
        {
            MarkerSlot slot = availableMarkerSlots[i];
            AtomRecord record = availableAtomRecords != null && i < availableAtomRecords.Count ? availableAtomRecords[i] : null;
            if (slot == null || slot.labelObject == null || !slot.labelObject.activeSelf || record == null || record.root == null)
            {
                continue;
            }

            slot.labelObject.transform.localRotation = ResolveLabelRadarRotation(frame, record.root, slot.labelObject.transform.localPosition);
        }
    }

    private void EnsureAvailableLabelSlot(MarkerSlot slot, int markerIndex)
    {
        if (slot == null || slot.labelObject != null)
        {
            return;
        }

        slot.labelMesh = new Mesh();
        slot.labelMesh.name = "FA Radar Available Label Glyph Mesh " + markerIndex;
        slot.labelMesh.MarkDynamic();
        slot.labelMaterial = CreateEmissiveOverlayMaterial("FA Radar Available Label Material " + markerIndex, new Color(0.96f, 1.0f, 1.0f, DefaultLabelAlpha), MarkerRenderQueue);
        slot.labelObject = CreateMeshObject("FA Radar Available Label " + markerIndex, axisRoot.transform, slot.labelMesh, slot.labelMaterial, MarkerRenderQueue, MarkerSortingOrder - 7);
        slot.labelLeaderObject = CreateMeshObject("FA Radar Available Label Leader " + markerIndex, axisRoot.transform, labelLeaderMesh, slot.labelMaterial, MarkerRenderQueue, MarkerSortingOrder - 8);
        slot.labelFilter = slot.labelObject != null ? slot.labelObject.GetComponent<MeshFilter>() : null;
        SetActiveIfChanged(slot.labelObject, false);
        SetActiveIfChanged(slot.labelLeaderObject, false);
    }

    private void SetAvailableLabelVisible(MarkerSlot slot, bool visible)
    {
        if (slot == null)
        {
            return;
        }

        SetActiveIfChanged(slot.labelObject, visible);
        if (!visible)
        {
            SetActiveIfChanged(slot.labelLeaderObject, false);
        }
    }

    private void SetSelectedLabelVisible(bool visible)
    {
        SetActiveIfChanged(targetLabelObject, visible);
        if (!visible)
        {
            SetActiveIfChanged(targetLabelLeaderObject, false);
        }
    }

    private float ResolveLabelEffectiveAlpha(float fadeAlpha, bool selected)
    {
        float alpha = Mathf.Clamp01(ReadFloat(labelAlphaField, DefaultLabelAlpha));
        float fade = Mathf.Clamp01(fadeAlpha);
        if (selected)
        {
            fade = Mathf.Max(0.34f, fade);
        }

        return alpha * fade;
    }

    private Color ResolveLabelColor(AtomRecord record, float alpha)
    {
        Color baseColor = ResolveAvailableAtomColor(record, 1.0f);
        Color labelColor = Color.Lerp(new Color(0.96f, 1.0f, 1.0f, 1.0f), baseColor, 0.45f);
        labelColor.a = Mathf.Clamp01(alpha);
        return labelColor;
    }

    private void PositionLabelObject(GameObject labelObject, RadarFrame frame, Transform target, Vector3 radarLocal, bool selected, int markerIndex, out Vector3 itemLocal, out Vector3 labelLocal)
    {
        float visualRadius = Mathf.Max(0.001f, frame.visualRadius);
        itemLocal = ResolveLabelItemLocal(radarLocal, visualRadius);
        labelLocal = ResolveLabelCalloutLocal(radarLocal, visualRadius, selected, markerIndex);
        if (labelObject == null)
        {
            return;
        }

        labelObject.transform.localPosition = labelLocal;
        labelObject.transform.localRotation = ResolveLabelRadarRotation(frame, target, labelLocal);
        labelObject.transform.localScale = Vector3.one * ResolveEffectiveLabelScale(visualRadius);
    }

    private Vector3 ResolveLabelItemLocal(Vector3 radarLocal, float visualRadius)
    {
        return radarLocal * Mathf.Max(0.001f, visualRadius);
    }

    private Vector3 ResolveLabelCalloutLocal(Vector3 radarLocal, float visualRadius, bool selected, int markerIndex)
    {
        float safeRadius = Mathf.Max(0.001f, visualRadius);
        if (IsRoomCompassModeActive())
        {
            Vector3 itemLocal = radarLocal * safeRadius;
            float localLift = safeRadius * (selected ? 0.16f : 0.11f);
            return itemLocal + (Vector3.up * localLift);
        }

        Vector3 direction = radarLocal;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = new Vector3(0.0f, 1.0f, 0.0f);
        }
        direction.Normalize();

        Vector3 tangent = Vector3.Cross(direction, Vector3.up);
        if (tangent.sqrMagnitude <= 0.0001f)
        {
            tangent = Vector3.Cross(direction, Vector3.right);
        }
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(tangent, direction).normalized;
        int laneIndex = Mathf.Max(0, markerIndex);
        float tangentLane = selected ? 0.0f : ((laneIndex % 5) - 2) * safeRadius * 0.075f;
        float bitangentLane = selected ? safeRadius * 0.10f : (((laneIndex / 5) % 3) - 1) * safeRadius * 0.045f;
        float shellScale = selected ? 1.28f : 1.16f;
        return (direction * safeRadius * shellScale) + (tangent * tangentLane) + (bitangent * bitangentLane);
    }

    private float ResolveEffectiveLabelScale(float visualRadius)
    {
        float placementMultiplier = IsDesktopPlacementAttachedToUi() ? 1.0f : 1.35f;
        return Mathf.Max(0.001f, visualRadius)
            * Mathf.Clamp(ReadFloat(labelScaleField, DefaultLabelScale), 0.01f, 0.18f)
            * placementMultiplier;
    }

    private void UpdateLabelLeaderLine(GameObject lineObject, Vector3 itemLocal, Vector3 labelLocal, float alpha)
    {
        if (lineObject == null)
        {
            return;
        }

        Vector3 delta = labelLocal - itemLocal;
        float length = delta.magnitude;
        bool visible = alpha > 0.06f && length > 0.0005f;
        SetActiveIfChanged(lineObject, visible);
        if (!visible)
        {
            return;
        }

        lineObject.transform.localPosition = itemLocal + (delta * 0.5f);
        lineObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta / length);
        float width = Mathf.Clamp(length * 0.012f, 0.00035f, 0.0035f);
        lineObject.transform.localScale = new Vector3(width, length, width);
    }

    private Quaternion ResolveLabelRadarRotation(RadarFrame frame, Transform target, Vector3 labelLocalPosition)
    {
        Quaternion readableFacingCorrection = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        string mode = ResolveLabelOrientationMode();
        if (string.Equals(mode, LabelOrientationObjectRotation, StringComparison.Ordinal))
        {
            return ResolveAxisRadarRotation(target) * readableFacingCorrection;
        }
        if (string.Equals(mode, LabelOrientationWorldAxis, StringComparison.Ordinal))
        {
            return readableFacingCorrection;
        }

        Transform viewer = frame.viewer != null ? frame.viewer : ResolveViewerTransform();
        if (viewer == null || axisRoot == null)
        {
            return Quaternion.identity;
        }

        Vector3 worldPosition = axisRoot.transform.TransformPoint(labelLocalPosition);
        Vector3 toViewer = viewer.position - worldPosition;
        if (toViewer.sqrMagnitude <= 0.000001f)
        {
            return Quaternion.identity;
        }

        Quaternion worldRotation = Quaternion.LookRotation(toViewer.normalized, viewer.up);
        return Quaternion.Inverse(axisRoot.transform.rotation) * worldRotation * readableFacingCorrection;
    }

    private string SanitizeRadarLabelText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "ATOM";
        }

        StringBuilder sb = new StringBuilder(Mathf.Min(value.Length, 24));
        for (int i = 0; i < value.Length && sb.Length < 24; i++)
        {
            char c = char.ToUpperInvariant(value[i]);
            if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-' || c == '/' || c == '.' || c == '#')
            {
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                sb.Append('_');
            }
        }

        return sb.Length > 0 ? sb.ToString() : "ATOM";
    }

    private void PopulateLabelGlyphMesh(Mesh mesh, string text)
    {
        if (mesh == null)
        {
            return;
        }

        string safeText = SanitizeRadarLabelText(text);
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        float cell = 0.09f;
        float gap = 0.018f;
        float advance = (cell + gap) * 6.0f;
        float cursor = 0.0f;
        for (int charIndex = 0; charIndex < safeText.Length; charIndex++)
        {
            string[] rows = ResolveLabelGlyphRows(safeText[charIndex]);
            for (int y = 0; y < rows.Length; y++)
            {
                string row = rows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    if (row[x] != '1')
                    {
                        continue;
                    }

                    float left = cursor + (x * (cell + gap));
                    float bottom = (rows.Length - 1 - y) * (cell + gap);
                    AddLabelQuad(vertices, triangles, left, bottom, cell, cell);
                }
            }
            cursor += advance;
        }

        float width = Mathf.Max(cell, cursor - gap);
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            vertex.x -= width * 0.5f;
            vertices[i] = vertex;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    private void AddLabelQuad(List<Vector3> vertices, List<int> triangles, float x, float y, float width, float height)
    {
        int index = vertices.Count;
        vertices.Add(new Vector3(x, y, 0.0f));
        vertices.Add(new Vector3(x + width, y, 0.0f));
        vertices.Add(new Vector3(x + width, y + height, 0.0f));
        vertices.Add(new Vector3(x, y + height, 0.0f));
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private string[] ResolveLabelGlyphRows(char c)
    {
        switch (c)
        {
            case 'A': return new string[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" };
            case 'B': return new string[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" };
            case 'C': return new string[] { "01111", "10000", "10000", "10000", "10000", "10000", "01111" };
            case 'D': return new string[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" };
            case 'E': return new string[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" };
            case 'F': return new string[] { "11111", "10000", "10000", "11110", "10000", "10000", "10000" };
            case 'G': return new string[] { "01111", "10000", "10000", "10011", "10001", "10001", "01111" };
            case 'H': return new string[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" };
            case 'I': return new string[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" };
            case 'J': return new string[] { "00111", "00010", "00010", "00010", "10010", "10010", "01100" };
            case 'K': return new string[] { "10001", "10010", "10100", "11000", "10100", "10010", "10001" };
            case 'L': return new string[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" };
            case 'M': return new string[] { "10001", "11011", "10101", "10101", "10001", "10001", "10001" };
            case 'N': return new string[] { "10001", "11001", "10101", "10011", "10001", "10001", "10001" };
            case 'O': return new string[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" };
            case 'P': return new string[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" };
            case 'Q': return new string[] { "01110", "10001", "10001", "10001", "10101", "10010", "01101" };
            case 'R': return new string[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" };
            case 'S': return new string[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" };
            case 'T': return new string[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" };
            case 'U': return new string[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" };
            case 'V': return new string[] { "10001", "10001", "10001", "10001", "10001", "01010", "00100" };
            case 'W': return new string[] { "10001", "10001", "10001", "10101", "10101", "10101", "01010" };
            case 'X': return new string[] { "10001", "10001", "01010", "00100", "01010", "10001", "10001" };
            case 'Y': return new string[] { "10001", "10001", "01010", "00100", "00100", "00100", "00100" };
            case 'Z': return new string[] { "11111", "00001", "00010", "00100", "01000", "10000", "11111" };
            case '0': return new string[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" };
            case '1': return new string[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" };
            case '2': return new string[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" };
            case '3': return new string[] { "11110", "00001", "00001", "01110", "00001", "00001", "11110" };
            case '4': return new string[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" };
            case '5': return new string[] { "11111", "10000", "10000", "11110", "00001", "00001", "11110" };
            case '6': return new string[] { "01110", "10000", "10000", "11110", "10001", "10001", "01110" };
            case '7': return new string[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" };
            case '8': return new string[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" };
            case '9': return new string[] { "01110", "10001", "10001", "01111", "00001", "00001", "01110" };
            case '-': return new string[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" };
            case '_': return new string[] { "00000", "00000", "00000", "00000", "00000", "00000", "11111" };
            case '/': return new string[] { "00001", "00001", "00010", "00100", "01000", "10000", "10000" };
            case '.': return new string[] { "00000", "00000", "00000", "00000", "00000", "01100", "01100" };
            case '#': return new string[] { "01010", "11111", "01010", "01010", "11111", "01010", "01010" };
            default: return new string[] { "11111", "10001", "00001", "00010", "00100", "00000", "00100" };
        }
    }

    private Quaternion ResolveAxisRadarRotation(Transform target)
    {
        if (target == null)
        {
            return Quaternion.identity;
        }

        Quaternion axisWorldRotation = axisRoot != null ? axisRoot.transform.rotation : Quaternion.identity;
        return Quaternion.Inverse(axisWorldRotation) * target.rotation;
    }

    private Quaternion ResolveAxisLineRotation(Quaternion targetRotation, int axis)
    {
        if (axis == 1)
        {
            return targetRotation * Quaternion.FromToRotation(Vector3.right, Vector3.up);
        }
        if (axis == 2)
        {
            return targetRotation * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
        }

        return targetRotation;
    }

    private void UpdateLightRangeVolume(GameObject volumeObject, Material volumeMaterial, Atom atom, Transform target, Light light, Vector3 radarLocal, float fadeAlpha, bool hasLight)
    {
        bool show = showLightRangeVolumesField != null
            && showLightRangeVolumesField.val
            && volumeObject != null
            && hasLight
            && IsLightAtom(atom)
            && light != null
            && light.type == LightType.Point
            && light.type != LightType.Directional
            && fadeAlpha > 0.01f;
        SetActiveIfChanged(volumeObject, show);
        if (!show)
        {
            return;
        }

        float visualRadius = ResolveVisualRadius();
        float rangeScale = Mathf.Max(0.001f, light.range) / ResolveEffectiveRadarRangeMeters() * visualRadius * ResolveLightVolumeScale();
        volumeObject.transform.localPosition = radarLocal * visualRadius;
        volumeObject.transform.localRotation = Quaternion.identity;
        volumeObject.transform.localScale = Vector3.one * rangeScale;
        ApplyMaterialColor(volumeMaterial, ResolveLightVolumeColor(atom, light, ResolvePointLightRangeAlpha() * fadeAlpha), Mathf.Max(0.0f, emissionStrengthField.val) * 0.35f);
    }

    private void UpdateSpotlightCone(GameObject coneObject, Material coneMaterial, Atom atom, Transform target, Light light, Vector3 radarLocal, float fadeAlpha, bool hasLight)
    {
        bool show = showSpotlightConesField != null
            && showSpotlightConesField.val
            && coneObject != null
            && hasLight
            && IsLightAtom(atom)
            && light != null
            && light.type == LightType.Spot
            && fadeAlpha > 0.01f;
        SetActiveIfChanged(coneObject, show);
        if (!show)
        {
            return;
        }

        float visualRadius = ResolveVisualRadius();
        float rawRangeScale = Mathf.Max(0.001f, light.range) / ResolveEffectiveRadarRangeMeters() * visualRadius * ResolveLightVolumeScale();
        float spotAngle = Mathf.Clamp(light.spotAngle, 0.0f, 179.0f);
        float coneAngleRadius = Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad);
        float maxRangeScale = ResolveDistanceToRadarShell(radarLocal * visualRadius, ResolveAxisRadarRotation(light.transform) * Vector3.forward, visualRadius);
        float rangeScale = IsRoomCompassModeActive()
            ? rawRangeScale
            : ResolveClippedSpotlightConeScale(rawRangeScale, maxRangeScale);
        float coneRadius = IsRoomCompassModeActive()
            ? coneAngleRadius * rangeScale
            : Mathf.Min(coneAngleRadius * rangeScale, visualRadius * 0.96f);
        coneObject.transform.localPosition = radarLocal * visualRadius;
        coneObject.transform.localRotation = ResolveAxisRadarRotation(light.transform);
        coneObject.transform.localScale = new Vector3(coneRadius, coneRadius, rangeScale);
        ApplyMaterialColor(coneMaterial, ResolveLightVolumeColor(atom, light, ResolveSpotlightConeAlpha() * fadeAlpha), Mathf.Max(0.0f, emissionStrengthField.val) * 0.35f);
    }

    private float ResolveClippedSpotlightConeScale(float rawRangeScale, float maxRangeScale)
    {
        return Mathf.Clamp(
            rawRangeScale,
            0.001f,
            Mathf.Max(0.001f, maxRangeScale));
    }

    private float ResolveDistanceToRadarShell(Vector3 localStart, Vector3 localDirection, float visualRadius)
    {
        Vector3 direction = localDirection.sqrMagnitude > 0.0001f
            ? localDirection.normalized
            : Vector3.forward;
        float radius = Mathf.Max(0.001f, visualRadius);
        float b = Vector3.Dot(localStart, direction);
        float c = Vector3.Dot(localStart, localStart) - radius * radius;
        float discriminant = b * b - c;
        if (discriminant <= 0.0f)
        {
            return radius * 0.25f;
        }

        float distance = -b + Mathf.Sqrt(discriminant);
        return Mathf.Clamp(distance, radius * 0.05f, radius * 1.96f);
    }

    private float ResolvePointLightRangeAlpha()
    {
        return Mathf.Clamp01(ReadFloat(pointLightRangeAlphaField, 0.022f));
    }

    private float ResolveSpotlightConeAlpha()
    {
        return Mathf.Clamp01(ReadFloat(spotlightConeAlphaField, 0.024f));
    }

    private float ResolveLightVolumeScale()
    {
        return Mathf.Clamp(ReadFloat(lightVolumeScaleField, 0.62f), 0.1f, 2.0f);
    }

    private Color ResolveLightVolumeColor(Atom atom, Light light, float alpha)
    {
        Color color = light != null ? light.color : new Color(1.0f, 0.86f, 0.42f, 1.0f);
        if (Mathf.Max(color.r, Mathf.Max(color.g, color.b)) <= 0.001f)
        {
            color = ResolveAvailableAtomColor(atom, 1.0f);
        }
        color.a = alpha;
        return color;
    }

    private bool TryResolveUnityLight(Atom atom, out Light light)
    {
        AtomRecord record = new AtomRecord();
        record.atom = atom;
        record.root = ResolveAtomRootTransform(atom);
        return TryResolveUnityLight(record, out light);
    }

    private bool TryResolveUnityLight(AtomRecord record, out Light light)
    {
        light = null;
        if (record == null)
        {
            return false;
        }

        if (record.lightResolved)
        {
            light = record.light;
            return record.hasLight && light != null;
        }

        Transform root = record.atom != null && record.atom.transform != null
            ? record.atom.transform
            : record.root;
        if (root == null)
        {
            record.lightResolved = true;
            record.hasLight = false;
            return false;
        }

        try
        {
            Light[] lights = root.GetComponentsInChildren<Light>(true);
            if (lights == null || lights.Length <= 0)
            {
                record.lightResolved = true;
                record.hasLight = false;
                return false;
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].enabled)
                {
                    light = lights[i];
                    record.light = light;
                    record.hasLight = true;
                    record.lightResolved = true;
                    return true;
                }
            }

            light = lights[0];
            record.light = light;
            record.hasLight = light != null;
            record.lightResolved = true;
            return record.hasLight;
        }
        catch
        {
            record.lightResolved = true;
            record.hasLight = false;
            return false;
        }
    }

    private void UpdateProCameraFrustums(Transform viewer)
    {
        Camera viewerCamera = ResolveViewerCamera();
        UpdateCameraFrustumObject(
            userPovFrustumObject,
            userPovFrustumMaterial,
            viewerCamera,
            viewer != null ? viewer : (viewerCamera != null ? viewerCamera.transform : null),
            viewer,
            new Color(0.38f, 1.0f, 0.62f, 1.0f),
            showUserPovFrustumField != null && showUserPovFrustumField.val,
            true);

        Camera desktopCamera = Camera.main;
        UpdateCameraFrustumObject(
            desktopPovFrustumObject,
            desktopPovFrustumMaterial,
            desktopCamera,
            desktopCamera != null ? desktopCamera.transform : null,
            viewer,
            new Color(0.50f, 0.84f, 1.0f, 1.0f),
            showDesktopPovFrustumField != null && showDesktopPovFrustumField.val,
            false);

        UpdateSceneCameraFrustums(viewer, viewerCamera, desktopCamera);
    }

    private void UpdateSceneCameraFrustums(Transform viewer, Camera viewerCamera, Camera desktopCamera)
    {
        bool showSceneCameras = showSceneCameraFrustumsField != null && showSceneCameraFrustumsField.val;
        Camera[] cameras = showSceneCameras ? Camera.allCameras : null;
        int requiredCount = 0;
        if (cameras != null)
        {
            requiredCount = Mathf.Min(cameras.Length, 16);
        }

        EnsureSceneCameraFrustumCapacity(requiredCount);
        int used = 0;
        if (showSceneCameras && cameras != null)
        {
            for (int i = 0; i < cameras.Length && used < requiredCount; i++)
            {
                Camera camera = cameras[i];
                if (camera == null || camera == viewerCamera || camera == desktopCamera)
                {
                    continue;
                }

                GameObject frustumObject = sceneCameraFrustumObjects != null && used < sceneCameraFrustumObjects.Length
                    ? sceneCameraFrustumObjects[used]
                    : null;
                UpdateCameraFrustumObject(
                    frustumObject,
                    sceneCameraFrustumMaterial,
                    camera,
                    camera.transform,
                    viewer,
                    new Color(0.94f, 0.72f, 1.0f, 1.0f),
                    true,
                    false);
                used++;
            }
        }

        if (sceneCameraFrustumObjects != null)
        {
            for (int i = used; i < sceneCameraFrustumObjects.Length; i++)
            {
                SetActiveIfChanged(sceneCameraFrustumObjects[i], false);
            }
        }
    }

    private void EnsureSceneCameraFrustumCapacity(int requiredCount)
    {
        int currentCount = sceneCameraFrustumObjects != null ? sceneCameraFrustumObjects.Length : 0;
        if (currentCount >= requiredCount)
        {
            return;
        }

        GameObject[] newFrustums = new GameObject[requiredCount];
        for (int i = 0; i < currentCount; i++)
        {
            newFrustums[i] = sceneCameraFrustumObjects[i];
        }

        for (int i = currentCount; i < requiredCount; i++)
        {
            newFrustums[i] = CreateMeshObject("FA Radar Scene Camera Frustum " + i, axisRoot.transform, povFrustumMesh, sceneCameraFrustumMaterial, MarkerRenderQueue - 10, MarkerSortingOrder - 10);
            SetActiveIfChanged(newFrustums[i], false);
        }

        sceneCameraFrustumObjects = newFrustums;
    }

    private void UpdateCameraFrustumObject(GameObject frustumObject, Material material, Camera camera, Transform cameraTransform, Transform viewer, Color baseColor, bool visible, bool primaryFrustum)
    {
        bool show = visible && frustumObject != null && cameraTransform != null;
        SetActiveIfChanged(frustumObject, show);
        if (!show)
        {
            return;
        }

        float visualRadius = ResolveVisualRadius();
        float lengthMeters = Mathf.Clamp(ReadFloat(povFrustumLengthField, 0.9f), 0.25f, 8.0f);
        float depthAlpha = primaryFrustum ? 1.0f : ResolveDepthVisibilityAlpha(ResolveRadarReferenceDistanceMeters(viewer, cameraTransform.position));
        float lengthScale = lengthMeters / ResolveEffectiveRadarRangeMeters() * visualRadius;
        if (!primaryFrustum)
        {
            lengthScale *= Mathf.Lerp(0.62f, 1.0f, Mathf.Clamp01(depthAlpha));
        }
        float fov = camera != null ? Mathf.Clamp(camera.fieldOfView, 1.0f, 179.0f) : 55.0f;
        float aspect = camera != null ? Mathf.Clamp(camera.aspect, 0.25f, 4.0f) : 1.777f;
        float halfY = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfX = halfY * aspect;

        frustumObject.transform.localPosition = ResolveWorldPositionRadarLocal(viewer, cameraTransform.position) * visualRadius;
        frustumObject.transform.localRotation = ResolveAxisRadarRotation(cameraTransform);
        frustumObject.transform.localScale = new Vector3(lengthScale * halfX, lengthScale * halfY, lengthScale);
        float frustumAlpha = Mathf.Clamp01(ReadFloat(povFrustumAlphaField, 0.035f));
        if (!primaryFrustum)
        {
            frustumAlpha *= ResolveDirectorOverlayAlpha(1.0f, depthAlpha);
        }
        ApplyMaterialColor(material, WithAlpha(baseColor, frustumAlpha), Mathf.Max(0.0f, emissionStrengthField.val) * 0.3f);
    }
#endif

    private void HandleDesktopRadarRangeScroll(Transform viewer)
    {
        if (viewer == null
            || IsVrDisplayActive()
            || hudRoot == null
            || radarRoot == null
            || !hudRoot.activeInHierarchy
            || radarVisibilityAlpha <= 0.08f)
        {
            return;
        }

        float scrollSteps = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollSteps) <= 0.001f)
        {
            return;
        }

        if (IsPointerOverVaMUi())
        {
            return;
        }

        Camera camera = ResolveViewerCamera();
        if (!IsMouseOverRadarVisual(camera))
        {
            return;
        }

        ScaleRadarRangeMetersFromScroll(scrollSteps);
    }

    private void ScaleRadarRangeMetersFromScroll(float scrollSteps)
    {
        if (radarRangeMetersField == null)
        {
            return;
        }

        float currentRangeMeters = ClampStorableFloat(radarRangeMetersField, ReadFloat(radarRangeMetersField, 5.0f), 0.5f, 30.0f);
        float nextRangeMeters = ClampStorableFloat(
            radarRangeMetersField,
            currentRangeMeters * Mathf.Pow(RadarRangeScrollZoomStep, -scrollSteps),
            0.5f,
            30.0f);

        if (Mathf.Abs(nextRangeMeters - currentRangeMeters) <= 0.0005f)
        {
            return;
        }

        SetFloatNoCallback(radarRangeMetersField, nextRangeMeters);
        MarkGlobalPreferencesDirty();
        InvalidateGridMesh();

        if (Time.unscaledTime >= nextRangeScrollStatusTime)
        {
            nextRangeScrollStatusTime = Time.unscaledTime + RadarRangeScrollStatusIntervalSeconds;
            SetStatus(string.Format(CultureInfo.InvariantCulture, "Radar range {0:0.0}m", nextRangeMeters));
        }
    }

    private bool IsPointerOverVaMUi()
    {
        try
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
        catch
        {
            return false;
        }
    }

    private bool IsMouseOverRadarVisual(Camera camera)
    {
        if (camera == null || radarRoot == null)
        {
            return false;
        }

        Vector3 radarCenter = radarRoot.transform.position;
        Vector3 centerScreen = camera.WorldToScreenPoint(radarCenter);
        if (centerScreen.z <= 0.0f)
        {
            return false;
        }

        float screenRadius = ResolveRadarScreenRadiusPixels(camera);
        if (screenRadius <= 0.0f)
        {
            return false;
        }

        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 radarScreenCenter = new Vector2(centerScreen.x, centerScreen.y);
        return Vector2.Distance(mousePosition, radarScreenCenter) <= screenRadius + RadarHoverScreenPaddingPixels;
    }

    private float ResolveRadarScreenRadiusPixels(Camera camera)
    {
        if (camera == null || radarRoot == null)
        {
            return 0.0f;
        }

        float visualRadius = ResolveRadarSurfaceLocalRadius();
        Vector3 center = radarRoot.transform.position;
        Vector3 centerScreen = camera.WorldToScreenPoint(center);
        if (centerScreen.z <= 0.0f)
        {
            return 0.0f;
        }

        Vector3 rightScreen = camera.WorldToScreenPoint(radarRoot.transform.TransformPoint(Vector3.right * visualRadius));
        Vector3 upScreen = camera.WorldToScreenPoint(radarRoot.transform.TransformPoint(Vector3.up * visualRadius));
        Vector2 screenCenter = new Vector2(centerScreen.x, centerScreen.y);
        float rightRadius = Vector2.Distance(screenCenter, new Vector2(rightScreen.x, rightScreen.y));
        float upRadius = Vector2.Distance(screenCenter, new Vector2(upScreen.x, upScreen.y));
        return Mathf.Clamp(Mathf.Max(rightRadius, upRadius), 4.0f, Mathf.Max(Screen.width, Screen.height));
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
        UpdateDirectGripGrab(viewer, radarCenter);
    }

    private void UpdateDirectGripGrab(Transform viewer, Vector3 radarCenter)
    {
        if (!moveGrabActive)
        {
            if (radarVisibilityAlpha <= 0.08f)
            {
                DestroySessionGrabHandleAtoms();
                EndDirectGripAccordionResize(false);
                SetResizeGuideLineVisible(false);
                return;
            }

            EnsurePrimaryGrabHandleAtom(radarCenter);
            if (primaryGrabHandleAtom != null && primaryGrabHandleAtom.mainController != null)
            {
                FreeControllerV3 primaryController = primaryGrabHandleAtom.mainController;
                int grabbedHand;
                if (TryResolveGrabbedHand(primaryController, out grabbedHand))
                {
                    StartMoveGrab(primaryController, grabbedHand);
                }
                else
                {
                    ConfigureGrabHandleAtom(primaryGrabHandleAtom, radarCenter);
                    MoveGrabHandleAtom(primaryGrabHandleAtom, radarCenter);
                    TryStartFauxPrimaryGrab(radarCenter);
                }
            }
            else
            {
                // Controller grip remains the graceful path while VaM creates the optional stock grab target.
                TryStartFauxPrimaryGrab(radarCenter);
            }

            bool accordionResizeVisible = UpdateDirectGripAccordionResize(viewer);
            if (!accordionResizeVisible)
            {
                SetResizeGuideLineVisible(false);
            }
            return;
        }

        if (!moveGrabUsesGripFallback)
        {
            FreeControllerV3 primaryController = primaryGrabHandleAtom != null
                ? primaryGrabHandleAtom.mainController
                : null;
            int grabbedHand;
            bool primaryGrabbed = TryResolveGrabbedHand(primaryController, out grabbedHand);
            if (!primaryGrabbed || grabbedHand != moveGrabHand)
            {
                EndMoveGrab();
                return;
            }

            UpdateMoveGrab(viewer, primaryController);
            bool stockMoveAccordionResizeVisible = UpdateDirectGripAccordionResize(viewer);
            if (!stockMoveAccordionResizeVisible)
            {
                SetResizeGuideLineVisible(false);
            }
            return;
        }

        if (!IsGripHeld(moveGrabHand))
        {
            EndMoveGrab();
            return;
        }

        UpdateFauxMoveGrab(viewer);
        bool moveAccordionResizeVisible = UpdateDirectGripAccordionResize(viewer);
        if (!moveAccordionResizeVisible)
        {
            SetResizeGuideLineVisible(false);
        }
    }

    private bool UpdateDirectGripAccordionResize(Transform viewer)
    {
        Vector3 leftPosition;
        Vector3 rightPosition;
        if (!TryResolveAccordionResizeHands(viewer, out leftPosition, out rightPosition))
        {
            EndDirectGripAccordionResize(true);
            return false;
        }

        float currentDistance = Mathf.Max(
            AccordionResizeMinimumStartDistanceMeters,
            Vector3.Distance(leftPosition, rightPosition));
        if (!accordionResizeActive)
        {
            Transform leftSource = ResolveHandOrControllerTransform(GrabHandLeft);
            Transform rightSource = ResolveHandOrControllerTransform(GrabHandRight);
            accordionResizeActive = true;
            accordionResizeUsesHandFallback = !IsMotionControllerTransform(leftSource, GrabHandLeft)
                || !IsMotionControllerTransform(rightSource, GrabHandRight);
            accordionResizeStartScale = ResolveActivePlacementScale();
            accordionResizeStartDistance = currentDistance;
            if (!accordionResizeUsesHandFallback)
            {
                PulseGrabHandleHaptics(GrabHandUnknown, 0.30f, 0.22f, 0.035f);
            }
            SetStatus("Radar accordion scale active.");
        }

        float ratio = currentDistance / Mathf.Max(AccordionResizeMinimumStartDistanceMeters, accordionResizeStartDistance);
        SetActivePlacementScaleNoCallback(accordionResizeStartScale * ratio);
        MarkGlobalPreferencesDirty();
        UpdateResizeGuideLine(leftPosition, rightPosition, true);
        return true;
    }

    private bool TryResolveAccordionResizeHands(Transform viewer, out Vector3 leftPosition, out Vector3 rightPosition)
    {
        leftPosition = Vector3.zero;
        rightPosition = Vector3.zero;

        Transform leftController = ResolveHandOrControllerTransform(GrabHandLeft);
        Transform rightController = ResolveHandOrControllerTransform(GrabHandRight);
        if (leftController == null || rightController == null)
        {
            return false;
        }

        float threshold = Mathf.Clamp(ReadFloat(wristTwistDegreesField, 65.0f), 15.0f, 120.0f);
        float requiredTwist = accordionResizeActive ? Mathf.Max(0.0f, threshold - 12.0f) : threshold;
        if (ResolveControllerOutwardTwistDegrees(leftController, GrabHandLeft, viewer) < requiredTwist
            || ResolveControllerOutwardTwistDegrees(rightController, GrabHandRight, viewer) < requiredTwist)
        {
            return false;
        }

        leftPosition = leftController.position;
        rightPosition = rightController.position;
        return true;
    }

    private void EndDirectGripAccordionResize(bool applied)
    {
        if (!accordionResizeActive)
        {
            return;
        }

        if (applied)
        {
            MarkGlobalPreferencesDirty();
            FlushGlobalPreferencesIfDue(true);
            if (!accordionResizeUsesHandFallback)
            {
                PulseGrabHandleHaptics(GrabHandUnknown, 0.24f, 0.18f, 0.03f);
            }
            SetStatus("Radar accordion scale applied.");
        }

        accordionResizeActive = false;
        accordionResizeUsesHandFallback = false;
        accordionResizeStartDistance = 0.0f;
        accordionResizeStartScale = 0.0f;
        SetResizeGuideLineVisible(false);
    }

    private bool ShouldUseSessionGrabHandles(Transform viewer)
    {
        if (viewer == null || hudRoot == null || !ReadBool(grabHandlesEnabledField, true))
        {
            return false;
        }

        return recorderRadarVisible && radarEnabledField != null && radarEnabledField.val;
    }

    private void EnsurePrimaryGrabHandleAtom(Vector3 worldPosition)
    {
        primaryGrabHandleUid = BuildGrabHandleUid(GrabHandlePrimarySuffix);
        Atom resolvedAtom = ResolveGrabHandleAtom(primaryGrabHandleUid, primaryGrabHandleAtom);
        primaryGrabHandleAtom = resolvedAtom;
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
            MoveGrabHandleAtom(atom, worldPosition);
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
            atom.hidden = true;
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
            // Built-In Grab Target is kept invisible and inactive for compatibility while direct grip owns session movement.
            controller.currentPositionState = FreeControllerV3.PositionState.On;
            controller.currentRotationState = FreeControllerV3.RotationState.Off;
            controller.controlMode = FreeControllerV3.ControlMode.Position;
            controller.canGrabPosition = true;
            controller.canGrabRotation = false;
            controller.hidden = true;
            controller.guihidden = !debugVisible;
            controller.collisionEnabled = false;
            controller.physicsEnabled = false;
            controller.controlsCollisionEnabled = false;
            controller.controlsOn = true;
            controller.freezeAtomPhysicsWhenGrabbed = false;
            controller.GUIalwaysVisibleWhenSelected = debugVisible;
            controller.drawMeshWhenDeselected = false;
            controller.deselectedMeshScale = 0.01f;
            controller.selectedScale = 0.01f;
        }
        catch
        {
        }

        int unusedHand;
        TryResolveGrabbedHand(controller, out unusedHand);
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
        StartMoveGrabAtPosition(GetControllerWorldPosition(primaryController), hand, false, ResolveRadarWorldCenter(ResolveViewerTransform()));
    }

    private void StartMoveGrabAtPosition(Vector3 handlePosition, int hand, bool gripFallback, Vector3 radarWorldCenter)
    {
#if FA_RADAR_PRO
        CancelGrabThrowPinForGrab();
        ResetMoveGrabVelocitySample(handlePosition);
#endif
        moveGrabActive = true;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = gripFallback;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabHand = hand;
        resizeGrabHand = GrabHandUnknown;
        moveStartHandlePosition = handlePosition;
        moveGrabStartRadarWorldCenter = radarWorldCenter;
        moveGrabCurrentRadarWorldCenter = radarWorldCenter;
        moveGrabStartRadarWorldRotation = hudRoot != null
            ? hudRoot.transform.rotation
            : Quaternion.identity;
        moveGrabWorldOverrideActive = true;
        moveStartHudOffset = GetHudOffset();
        moveStartWristOffset = GetWristOffset();
        moveStartStaticPosition = GetStaticWorldPosition();
        haveSmoothedHudPosition = false;
        if (gripFallback)
        {
            PulseGrabHandleHaptics(hand, 0.35f, 0.28f, 0.045f);
        }
        SetStatus("Radar grab move active.");
    }

    private void UpdateMoveGrab(Transform viewer, FreeControllerV3 primaryController)
    {
        if (!moveGrabActive || primaryController == null)
        {
            return;
        }

        Vector3 controllerPosition = GetControllerWorldPosition(primaryController);
#if FA_RADAR_PRO
        UpdateMoveGrabVelocitySample(controllerPosition);
#endif
        Vector3 worldDelta = controllerPosition - moveStartHandlePosition;
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

#if FA_RADAR_PRO
        UpdateMoveGrabVelocitySample(controllerPosition);
#endif
        ApplyMoveGrabDelta(viewer, controllerPosition - moveStartHandlePosition);
    }

    private void EndMoveGrab()
    {
        bool startedThrowPin = false;
        bool useControllerHaptics = moveGrabUsesGripFallback;
        if (moveGrabActive)
        {
#if FA_RADAR_PRO
            startedThrowPin = TryStartGrabThrowPinOnRelease(ResolveViewerTransform());
            if (!startedThrowPin)
            {
#endif
            ApplyMoveGrabWorldCenterToPreferences(ResolveViewerTransform());
            MarkGlobalPreferencesDirty();
            FlushGlobalPreferencesIfDue(true);
            if (useControllerHaptics)
            {
                PulseGrabHandleHaptics(moveGrabHand, 0.22f, 0.20f, 0.035f);
            }
#if FA_RADAR_PRO
            }
#endif
        }

        moveGrabActive = false;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = false;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
#if FA_RADAR_PRO
        if (!startedThrowPin)
        {
            moveGrabWorldOverrideActive = false;
        }
#else
        moveGrabWorldOverrideActive = false;
#endif
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroyResizeGrabHandleAtom();
        EndDirectGripAccordionResize(false);
        if (!startedThrowPin)
        {
            SetStatus("Radar grab move applied.");
        }
    }

#if FA_RADAR_PRO
    private void ResetMoveGrabVelocitySample(Vector3 controllerPosition)
    {
        moveGrabPreviousControllerPosition = controllerPosition;
        moveGrabPreviousSampleTime = Time.unscaledTime;
        moveGrabReleaseVelocity = Vector3.zero;
    }

    private void UpdateMoveGrabVelocitySample(Vector3 controllerPosition)
    {
        float now = Time.unscaledTime;
        float dt = now - moveGrabPreviousSampleTime;
        if (dt > 0.0001f && dt < 0.25f)
        {
            Vector3 instantVelocity = (controllerPosition - moveGrabPreviousControllerPosition) / dt;
            if (instantVelocity.sqrMagnitude < 100.0f)
            {
                moveGrabReleaseVelocity = Vector3.Lerp(moveGrabReleaseVelocity, instantVelocity, 0.45f);
            }
        }

        moveGrabPreviousControllerPosition = controllerPosition;
        moveGrabPreviousSampleTime = now;
    }

    private bool TryStartGrabThrowPinOnRelease(Transform viewer)
    {
        if (!ReadBool(grabThrowPinEnabledField, false)
            || IsCuaPreferenceProfileActive()
            || IsEmptyAnchorHostActive()
            || viewer == null)
        {
            return false;
        }

        float velocityScale = Mathf.Clamp(ReadFloat(grabThrowVelocityScaleField, 0.45f), 0.05f, 2.0f);
        Vector3 launchVelocity = moveGrabReleaseVelocity * velocityScale;
        if (launchVelocity.sqrMagnitude < GrabThrowMinimumReleaseVelocity * GrabThrowMinimumReleaseVelocity)
        {
            return false;
        }

        grabThrowActive = true;
        grabThrowUsesControllerHaptics = moveGrabUsesGripFallback;
        grabThrowPosition = moveGrabCurrentRadarWorldCenter;
        grabThrowStartPosition = grabThrowPosition;
        grabThrowVelocity = launchVelocity;
        grabThrowStartedAt = Time.unscaledTime;
        grabThrowStartScale = ResolveActivePlacementScale();
        grabThrowTargetScale = Mathf.Clamp(
            ReadFloat(grabThrowGrowScaleField, 1.0f),
            0.25f,
            ResolveMaxPlacementScale());
        if (grabThrowTargetScale < grabThrowStartScale)
        {
            grabThrowTargetScale = grabThrowStartScale;
        }

        SetFloatNoCallback(grabThrowReturnScaleField, grabThrowStartScale);
        SetBoolNoCallback(grabThrowPinnedField, false);
        SetActiveDisplayPlacementNoCallback(DesktopPlacementPinnedInWorld);
        SetRadarModeNoCallback(RadarModeHud);
        SetActivePlacementScaleNoCallback(grabThrowStartScale);
        moveGrabCurrentRadarWorldCenter = grabThrowPosition;
        moveGrabWorldOverrideActive = true;
        haveSmoothedHudPosition = false;
        MarkGlobalPreferencesDirty();
        if (grabThrowUsesControllerHaptics)
        {
            PulseGrabHandleHaptics(moveGrabHand, 0.38f, 0.26f, 0.05f);
        }
        SetStatus("Radar throw pin launched.");
        return true;
    }

    private void UpdateGrabThrowPin(Transform viewer)
    {
        if (!grabThrowActive)
        {
            return;
        }

        float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0.0f, 0.05f);
        if (dt <= 0.0f)
        {
            dt = 0.016f;
        }

        bool hitSurface = false;
        Vector3 previousPosition = grabThrowPosition;
        float frameDistance = grabThrowVelocity.magnitude * dt;
        if (ReadBool(grabThrowSurfaceStopField, true) && frameDistance > 0.0001f)
        {
            Vector3 direction = grabThrowVelocity.normalized;
            RaycastHit hit;
            if (Physics.Raycast(previousPosition, direction, out hit, frameDistance + GrabThrowSurfaceInsetMeters))
            {
                grabThrowPosition = hit.point - direction * GrabThrowSurfaceInsetMeters;
                grabThrowVelocity = Vector3.zero;
                hitSurface = true;
            }
        }

        if (!hitSurface)
        {
            grabThrowPosition += grabThrowVelocity * dt;
            float speed = grabThrowVelocity.magnitude;
            float deceleration = Mathf.Clamp(ReadFloat(grabThrowDecelerationField, 1.5f), 0.2f, 8.0f);
            speed = Mathf.Max(0.0f, speed - deceleration * dt);
            grabThrowVelocity = speed > 0.0f ? grabThrowVelocity.normalized * speed : Vector3.zero;
        }

        moveGrabCurrentRadarWorldCenter = grabThrowPosition;
        moveGrabWorldOverrideActive = true;
        haveSmoothedHudPosition = false;

        float scaleT = Mathf.Clamp01((Time.unscaledTime - grabThrowStartedAt) / GrabThrowScaleSeconds);
        SetActivePlacementScaleNoCallback(Mathf.Lerp(grabThrowStartScale, grabThrowTargetScale, scaleT));

        bool timedOut = Time.unscaledTime - grabThrowStartedAt >= GrabThrowMaxSeconds;
        bool stopped = grabThrowVelocity.magnitude <= GrabThrowStopVelocity;
        if (hitSurface || stopped || timedOut)
        {
            FinishGrabThrowPin(viewer, hitSurface);
        }
    }

    private void FinishGrabThrowPin(Transform viewer, bool hitSurface)
    {
        Quaternion rotation = hudRoot != null
            ? hudRoot.transform.rotation
            : (viewer != null ? viewer.rotation : Quaternion.identity);

        grabThrowActive = false;
        moveGrabWorldOverrideActive = false;
        moveGrabCurrentRadarWorldCenter = grabThrowPosition;
        SetActiveDisplayPlacementNoCallback(DesktopPlacementPinnedInWorld);
        SetRadarModeNoCallback(RadarModeHud);
        SetStaticWorldPositionNoCallback(grabThrowPosition);
        SetStaticWorldRotationNoCallback(rotation);
        SetActivePlacementScaleNoCallback(grabThrowTargetScale);
        SetBoolNoCallback(grabThrowPinnedField, true);
        MarkGlobalPreferencesDirty();
        FlushGlobalPreferencesIfDue(true);
        if (grabThrowUsesControllerHaptics)
        {
            PulseGrabHandleHaptics(GrabHandUnknown, hitSurface ? 0.35f : 0.22f, 0.18f, 0.04f);
        }
        grabThrowUsesControllerHaptics = false;
        SetStatus(hitSurface ? "Radar throw pinned to surface." : "Radar throw pinned in world.");
    }

    private void CancelGrabThrowPinForGrab()
    {
        if (!grabThrowActive && !ReadBool(grabThrowPinnedField, false))
        {
            return;
        }

        float returnScale = Mathf.Clamp(
            ReadFloat(grabThrowReturnScaleField, 0.49f),
            0.05f,
            ResolveMaxPlacementScale());
        grabThrowActive = false;
        grabThrowUsesControllerHaptics = false;
        SetBoolNoCallback(grabThrowPinnedField, false);
        SetActiveDisplayPlacementNoCallback(DesktopPlacementAttachedToUi);
        SetRadarModeNoCallback(RadarModeHud);
        SetActivePlacementScaleNoCallback(returnScale);
        moveGrabWorldOverrideActive = true;
        haveSmoothedHudPosition = false;
        MarkGlobalPreferencesDirty();
        SetStatus("Radar throw pin picked up.");
    }
#endif

    private void ApplyMoveGrabDelta(Transform viewer, Vector3 worldDelta)
    {
        moveGrabCurrentRadarWorldCenter = moveGrabStartRadarWorldCenter + worldDelta;
        moveGrabWorldOverrideActive = true;
        if (TryCompleteHudDetachToWrist(moveGrabCurrentRadarWorldCenter, worldDelta, viewer))
        {
            return;
        }

        if (IsWristCompassModeActive())
        {
            if (TryCompleteWristGrabHandOff(moveGrabCurrentRadarWorldCenter, worldDelta, viewer))
            {
                return;
            }

            if (TryCompleteHudGrabHandOff(moveGrabCurrentRadarWorldCenter, worldDelta, viewer))
            {
                return;
            }

            haveSmoothedHudPosition = false;
            return;
        }

        haveSmoothedHudPosition = false;
    }

    private void ApplyMoveGrabWorldCenterToPreferences(Transform viewer)
    {
        if (!moveGrabWorldOverrideActive)
        {
            return;
        }

        if (IsWristCompassModeActive())
        {
            Transform wristAnchor = ResolveWristCompassAnchorTransform();
            if (wristAnchor != null)
            {
                SetWristOffsetNoCallback(wristAnchor.InverseTransformPoint(moveGrabCurrentRadarWorldCenter));
            }
            haveSmoothedHudPosition = false;
            return;
        }

        string anchorMode = ResolveAnchorMode();
        if (string.Equals(anchorMode, AnchorModeWorldStatic, StringComparison.Ordinal))
        {
            SetStaticWorldPositionNoCallback(moveGrabCurrentRadarWorldCenter);
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

        SetHudOffsetNoCallback(reference != null
            ? reference.InverseTransformPoint(moveGrabCurrentRadarWorldCenter)
            : moveStartHudOffset);
        haveSmoothedHudPosition = false;
    }

    private bool TryCompleteWristGrabHandOff(Vector3 proposedRadarPosition, Vector3 worldDelta, Transform viewer)
    {
        int activeHand = ResolveWristCompassHand();
        if (moveGrabHand == GrabHandUnknown || moveGrabHand == activeHand)
        {
            return false;
        }

        Transform activeAnchor = ResolveHandOrControllerTransform(activeHand);
        Transform targetAnchor = ResolveHandOrControllerTransform(moveGrabHand);
        if (activeAnchor == null || targetAnchor == null)
        {
            return false;
        }

        float activeDistance = Vector3.Distance(proposedRadarPosition, activeAnchor.position);
        float targetDistance = Vector3.Distance(proposedRadarPosition, targetAnchor.position);
        if (worldDelta.magnitude < WristHandOffMinimumTravelMeters
            || targetDistance > WristHandOffDistanceMeters
            || targetDistance + 0.08f >= activeDistance)
        {
            return false;
        }

        bool alwaysOn = ResolveHandoffAlwaysOn(targetAnchor, moveGrabHand, viewer);
        SetRadarModeNoCallback(ResolveRadarModeForHand(moveGrabHand, alwaysOn));
        SetWristOffsetNoCallback(targetAnchor.InverseTransformPoint(proposedRadarPosition));
        wristCompassRevealed = true;
        wristRevealGraceUntil = Time.unscaledTime + WristRevealGraceSeconds;
        FinishMoveGrabAfterWristHandOff(moveGrabHand);
        return true;
    }

    private bool TryCompleteHudGrabHandOff(Vector3 proposedRadarPosition, Vector3 worldDelta, Transform viewer)
    {
        if (!IsWristCompassModeActive() || viewer == null || worldDelta.magnitude < WristHandOffMinimumTravelMeters)
        {
            return false;
        }

        Vector3 hudTargetPosition = viewer.TransformPoint(moveStartHudOffset);
        if (Vector3.Distance(proposedRadarPosition, hudTargetPosition) > HudHandOffDistanceMeters)
        {
            return false;
        }

        SetActiveDisplayPlacementNoCallback(DesktopPlacementAttachedToUi);
        SetRadarModeNoCallback(RadarModeHud);
        SetHudOffsetNoCallback(viewer.InverseTransformPoint(proposedRadarPosition));
        wristCompassRevealed = false;
        FinishMoveGrabAfterHudHandOff();
        return true;
    }

    private bool TryCompleteHudDetachToWrist(Vector3 proposedRadarPosition, Vector3 worldDelta, Transform viewer)
    {
        if (IsWristCompassModeActive()
            || moveGrabHand == GrabHandUnknown
            || viewer == null
            || !string.Equals(ResolveAnchorMode(), AnchorModeHud, StringComparison.Ordinal)
            || worldDelta.magnitude < WristHandOffMinimumTravelMeters)
        {
            return false;
        }

        Transform targetAnchor = ResolveHandOrControllerTransform(moveGrabHand);
        if (targetAnchor == null)
        {
            return false;
        }

        float handDistance = Vector3.Distance(proposedRadarPosition, targetAnchor.position);
        float hudDistance = Vector3.Distance(proposedRadarPosition, viewer.TransformPoint(moveStartHudOffset));
        if (handDistance > WristHandOffDistanceMeters || hudDistance < HudDetachToWristDistanceMeters)
        {
            return false;
        }

        bool alwaysOn = ResolveHandoffAlwaysOn(targetAnchor, moveGrabHand, viewer);
        SetRadarModeNoCallback(ResolveRadarModeForHand(moveGrabHand, alwaysOn));
        SetWristOffsetNoCallback(targetAnchor.InverseTransformPoint(proposedRadarPosition));
        wristCompassRevealed = true;
        wristRevealGraceUntil = Time.unscaledTime + WristRevealGraceSeconds;
        FinishMoveGrabAfterWristHandOff(moveGrabHand);
        return true;
    }

    private bool ResolveHandoffAlwaysOn(Transform targetAnchor, int hand, Transform viewer)
    {
        float threshold = Mathf.Clamp(ReadFloat(wristTwistDegreesField, 65.0f), 15.0f, 120.0f);
        return ResolveControllerOutwardTwistDegrees(targetAnchor, hand, viewer) < threshold;
    }

    private void SetActiveDisplayPlacementNoCallback(string value)
    {
        if (IsVrDisplayActive())
        {
            SetVRPlacementNoCallback(value);
            return;
        }

        SetDesktopPlacementNoCallback(value);
    }

    private string ResolveRadarModeForHand(int hand, bool alwaysOn)
    {
        if (hand == GrabHandRight)
        {
            return alwaysOn ? RadarModeWristRightAlwaysOn : RadarModeWristRight;
        }

        return alwaysOn ? RadarModeWristLeftAlwaysOn : RadarModeWristLeft;
    }

    private void FinishMoveGrabAfterWristHandOff(int hand)
    {
        MarkGlobalPreferencesDirty();
        FlushGlobalPreferencesIfDue(true);
        if (moveGrabUsesGripFallback)
        {
            PulseGrabHandleHaptics(hand, 0.42f, 0.32f, 0.05f);
        }
        moveGrabActive = false;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = false;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabWorldOverrideActive = false;
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroyResizeGrabHandleAtom();
        EndDirectGripAccordionResize(false);
        SetStatus(hand == GrabHandRight ? "Wrist compass moved to right hand." : "Wrist compass moved to left hand.");
    }

    private void FinishMoveGrabAfterHudHandOff()
    {
        MarkGlobalPreferencesDirty();
        FlushGlobalPreferencesIfDue(true);
        if (moveGrabUsesGripFallback)
        {
            PulseGrabHandleHaptics(moveGrabHand, 0.42f, 0.32f, 0.05f);
        }
        moveGrabActive = false;
        resizeGrabActive = false;
        moveGrabUsesGripFallback = false;
        resizeGrabUsesGripFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabWorldOverrideActive = false;
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroyResizeGrabHandleAtom();
        EndDirectGripAccordionResize(false);
        SetStatus("Radar moved to HUD.");
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
        resizeStartScale = ResolveActivePlacementScale();
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
        SetActivePlacementScaleNoCallback(resizeStartScale * ratio);
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
        SetActivePlacementScaleNoCallback(resizeStartScale * ratio);
        UpdateResizeGuideLine(primaryPosition, resizePosition, true);
    }

    private void EndResizeGrab(bool dismissUntilMoveRelease)
    {
        if (resizeGrabActive)
        {
            MarkGlobalPreferencesDirty();
            FlushGlobalPreferencesIfDue(true);
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
            StartMoveGrabAtPosition(leftPosition, GrabHandLeft, true, radarCenter);
            return true;
        }

        if (rightPressed && Vector3.Distance(rightPosition, radarCenter) <= hitRadius)
        {
            StartMoveGrabAtPosition(rightPosition, GrabHandRight, true, radarCenter);
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
            Transform controllerTransform = controllerCamera != null ? controllerCamera.transform : null;
            return IsActiveTransform(controllerTransform) ? controllerTransform : null;
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
        if (moveGrabWorldOverrideActive)
        {
            return moveGrabCurrentRadarWorldCenter;
        }

        if (IsWristCompassModeActive())
        {
            Transform wristAnchor = ResolveWristCompassAnchorTransform();
            if (wristAnchor != null)
            {
                return wristAnchor.TransformPoint(GetWristOffset());
            }
        }

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
        if (moveGrabActive || resizeGrabActive || accordionResizeActive)
        {
            MarkGlobalPreferencesDirty();
        }

        moveGrabActive = false;
        resizeGrabActive = false;
        accordionResizeActive = false;
        accordionResizeUsesHandFallback = false;
        resizeHandleDismissedUntilMoveRelease = false;
        moveGrabWorldOverrideActive = false;
        moveGrabHand = GrabHandUnknown;
        resizeGrabHand = GrabHandUnknown;
        DestroySessionGrabHandleAtoms();
        SetResizeGuideLineVisible(false);
        StopGrabHandleOvrHaptic(true);
        StopGrabHandleOvrHaptic(false);
    }

    private void DestroySessionGrabHandleAtoms()
    {
        DestroyGrabHandleAtom(ref primaryGrabHandleAtom, primaryGrabHandleUid);
        primaryGrabHandleUid = "";
        primaryGrabHandleCreatePending = false;
        DestroyResizeGrabHandleAtom();
    }

    private void DestroyResizeGrabHandleAtom()
    {
        DestroyGrabHandleAtom(ref resizeGrabHandleAtom, resizeGrabHandleUid);
        resizeGrabHandleUid = "";
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
        selectedAtomRecord = null;
        nextSelectionPollTime = 0.0f;
        nextAtomPollTime = 0.0f;
        availableMarkersDirty = true;
        availableAtomRevision++;
        SetStatus("Radar selected: " + selectedUid);
    }

    private Color ResolveAvailableAtomColor(AtomRecord record, float alpha)
    {
#if FA_RADAR_PRO
        if (HasCategory(record, AtomCategoryLight))
        {
            return new Color(1.0f, 0.88f, 0.30f, alpha);
        }
        if (HasCategory(record, AtomCategoryCua))
        {
            return new Color(1.0f, 0.62f, 0.24f, alpha);
        }
        if (HasCategory(record, AtomCategoryPerson))
        {
            if (HasCategory(record, AtomCategoryFemale))
            {
                return new Color(1.0f, 0.34f, 0.72f, alpha);
            }
            if (HasCategory(record, AtomCategoryMale))
            {
                return new Color(0.25f, 0.52f, 1.0f, alpha);
            }

            return new Color(0.78f, 0.54f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategoryEmpty))
        {
            return new Color(0.52f, 0.94f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategorySubScene))
        {
            return new Color(0.70f, 0.52f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategoryImagePanel))
        {
            return new Color(0.78f, 0.88f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategoryAnimation))
        {
            return new Color(0.36f, 1.0f, 0.56f, alpha);
        }
        if (HasCategory(record, AtomCategoryForce))
        {
            return new Color(1.0f, 0.30f, 0.26f, alpha);
        }
        if (HasCategory(record, AtomCategoryShape))
        {
            return new Color(0.36f, 0.68f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategorySound))
        {
            return new Color(0.94f, 0.72f, 1.0f, alpha);
        }
        if (HasCategory(record, AtomCategoryTrigger))
        {
            return new Color(1.0f, 0.72f, 0.32f, alpha);
        }
        if (HasCategory(record, AtomCategoryNavigationPanel))
        {
            return new Color(0.56f, 0.86f, 0.86f, alpha);
        }
        if (HasCategory(record, AtomCategoryCamera))
        {
            return new Color(0.84f, 0.84f, 0.92f, alpha);
        }

        return new Color(0.58f, 0.74f, 1.0f, alpha);
#else
        return WithAlpha(FreeAtomMarkerColor, alpha);
#endif
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
            if (IsFemalePersonAtom(atom))
            {
                return new Color(1.0f, 0.34f, 0.72f, alpha);
            }
            if (IsMalePersonAtom(atom))
            {
                return new Color(0.25f, 0.52f, 1.0f, alpha);
            }

            return new Color(0.78f, 0.54f, 1.0f, alpha);
        }
        if (IsEmptyAtom(atom))
        {
            return new Color(0.52f, 0.94f, 1.0f, alpha);
        }
        if (IsSubSceneAtom(atom))
        {
            return new Color(0.70f, 0.52f, 1.0f, alpha);
        }
        if (IsImagePanelAtom(atom))
        {
            return new Color(0.78f, 0.88f, 1.0f, alpha);
        }
        if (IsAnimationAtom(atom))
        {
            return new Color(0.36f, 1.0f, 0.56f, alpha);
        }
        if (IsForceAtom(atom))
        {
            return new Color(1.0f, 0.30f, 0.26f, alpha);
        }
        if (IsShapeAtom(atom))
        {
            return new Color(0.36f, 0.68f, 1.0f, alpha);
        }
        if (IsSoundAtom(atom))
        {
            return new Color(0.94f, 0.72f, 1.0f, alpha);
        }
        if (IsTriggerAtom(atom))
        {
            return new Color(1.0f, 0.72f, 0.32f, alpha);
        }
        if (IsNavigationPanelAtom(atom))
        {
            return new Color(0.56f, 0.86f, 0.86f, alpha);
        }
        if (IsCameraAtom(atom))
        {
            return new Color(0.84f, 0.84f, 0.92f, alpha);
        }

        return new Color(0.58f, 0.74f, 1.0f, alpha);
#else
        return WithAlpha(FreeAtomMarkerColor, alpha);
#endif
    }

    private void RefreshGridMeshIfNeeded(RadarFrame frame)
    {
        if (gridFilter == null)
        {
            return;
        }

        float range = IsRoomCompassModeActive()
            ? ResolveConfiguredRadarRangeMeters()
            : frame.rangeMeters;
        float step = ResolveGridStepMeters(range);
        Vector2 offset = ResolveViewerGridOffsetMeters(frame.viewer, step);
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
        Vector3 worldPosition = ResolveGridReferencePosition(viewer);
        return new Vector2(
            -PositiveModulo(worldPosition.x, safeStep),
            -PositiveModulo(worldPosition.z, safeStep));
    }

    private Vector3 ResolveGridReferencePosition(Transform viewer)
    {
        return ResolveRadarReferencePosition(viewer);
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

    private float ResolveMaxPlacementScale()
    {
        float visualDiameter = Mathf.Max(0.001f, ResolveVisualRadius() * 2.0f);
        return Mathf.Max(0.01f, MaxRadarVisualDiameterMeters / visualDiameter);
    }

    private float ResolveMaxHudPlacementScale()
    {
        return Mathf.Min(MaxHudPlacementScale, ResolveMaxPlacementScale());
    }

    private float ResolveFloorAreaScale()
    {
        // Kept as a registered legacy pref, but the visible range control owns the meter contract.
        return 1.0f;
    }

    private float ResolveGridStepMeters()
    {
        return ResolveGridStepMeters(ResolveEffectiveRadarRangeMeters());
    }

    private float ResolveGridStepMeters(float rangeMeters)
    {
        rangeMeters = Mathf.Max(0.25f, rangeMeters);
        return rangeMeters >= CoarseGridRangeThresholdMeters ? CoarseGridStepMeters : FineGridStepMeters;
    }

    private float ResolveEffectiveRadarRangeMeters()
    {
        if (IsRoomCompassModeActive())
        {
            return Mathf.Max(0.001f, ResolveHudScale() * ResolveVisualRadius());
        }

        return ResolveConfiguredRadarRangeMeters();
    }

    private float ResolveConfiguredRadarRangeMeters()
    {
        return Mathf.Max(0.25f, radarRangeMetersField.val) * ResolveFloorAreaScale();
    }

    private float ResolveRadarSurfaceLocalRadius()
    {
        if (IsRoomCompassModeActive())
        {
            return ResolveConfiguredRadarRangeMeters() / Mathf.Max(0.001f, ResolveHudScale());
        }

        return ResolveVisualRadius();
    }

    private float ResolveEffectiveHeightScaleMeters()
    {
        if (IsRoomCompassModeActive())
        {
            return Mathf.Max(0.001f, ResolveHudScale() * ResolveVisualRadius());
        }

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

    private Vector3 ResolveAtomMarkerWorldPosition(Atom atom, Transform fallback)
    {
        Vector3 center;
        if (ResolveAtomVisualBoundsCenter(atom, fallback, out center))
        {
            return center;
        }

        return fallback != null ? fallback.position : Vector3.zero;
    }

    private bool ResolveAtomVisualBoundsCenter(Atom atom, Transform fallback, out Vector3 center)
    {
        center = fallback != null ? fallback.position : Vector3.zero;
        Transform root = atom != null && atom.transform != null ? atom.transform : fallback;
        if (root == null)
        {
            return false;
        }

        try
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length <= 0)
            {
                return false;
            }

            bool hasBounds = false;
            Bounds bounds = new Bounds(center, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (radarRoot != null && renderer.transform != null && renderer.transform.IsChildOf(radarRoot.transform))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            center = bounds.center;
            return true;
        }
        catch
        {
            return false;
        }
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
        ApplyMaterialColor(selectedTargetRingXMaterial, new Color(1.0f, 0.22f, 0.16f, 0.62f), emission);
        ApplyMaterialColor(selectedTargetRingYMaterial, new Color(0.22f, 1.0f, 0.42f, 0.78f), emission);
        ApplyMaterialColor(selectedTargetRingZMaterial, new Color(0.30f, 0.60f, 1.0f, 0.86f), emission);
        ApplyMaterialColor(selectedViewCueMaterial, new Color(1.0f, 0.46f, 0.94f, 0.82f), emission);
        ApplyMaterialColor(targetHeightStemMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(heightStemAlphaField.val)), emission);
        ApplyMaterialColor(targetDropMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val) * 0.18f), emission);
        ApplyMaterialColor(lastTargetMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.26f), emission);
        ApplyMaterialColor(lastTargetDropMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.12f), emission);
        ApplyMaterialColor(availableHeightStemMaterial, new Color(0.78f, 0.88f, 1.0f, Mathf.Clamp01(heightStemAlphaField.val) * 0.72f), emission);
#if FA_RADAR_PRO
        ApplyMaterialColor(rotationAxisXMaterial, WithAlpha(AxisXColor, 0.48f), emission);
        ApplyMaterialColor(rotationAxisYMaterial, WithAlpha(AxisYColor, 0.48f), emission);
        ApplyMaterialColor(rotationAxisZMaterial, WithAlpha(AxisZColor, 0.48f), emission);
        ApplyMaterialColor(rotationAxisCenterMaterial, new Color(0.86f, 0.96f, 1.0f, 0.66f), emission);
        ApplyMaterialColor(targetLabelMaterial, new Color(0.96f, 1.0f, 1.0f, Mathf.Clamp01(ReadFloat(labelAlphaField, DefaultLabelAlpha))), emission * 0.72f);
        ApplyMaterialColor(targetLightRangeMaterial, new Color(1.0f, 0.86f, 0.42f, ResolvePointLightRangeAlpha()), emission * 0.35f);
        ApplyMaterialColor(targetSpotlightConeMaterial, new Color(1.0f, 0.86f, 0.42f, ResolveSpotlightConeAlpha()), emission * 0.35f);
        ApplyMaterialColor(userPovFrustumMaterial, new Color(0.38f, 1.0f, 0.62f, Mathf.Clamp01(ReadFloat(povFrustumAlphaField, 0.035f))), emission * 0.3f);
        ApplyMaterialColor(desktopPovFrustumMaterial, new Color(0.50f, 0.84f, 1.0f, Mathf.Clamp01(ReadFloat(povFrustumAlphaField, 0.035f))), emission * 0.3f);
        ApplyMaterialColor(sceneCameraFrustumMaterial, new Color(0.94f, 0.72f, 1.0f, Mathf.Clamp01(ReadFloat(povFrustumAlphaField, 0.035f))), emission * 0.3f);
#endif
    }

    private void UpdateMaterialsIfNeeded()
    {
        if (!materialsDirty)
        {
            return;
        }

        UpdateMaterials();
        materialsDirty = false;
    }

    private void SetMaterialAlphaMultiplier(float multiplier)
    {
        multiplier = Mathf.Clamp01(multiplier);
        if (Mathf.Abs(radarMaterialAlphaMultiplier - multiplier) <= 0.0001f)
        {
            return;
        }

        radarMaterialAlphaMultiplier = multiplier;
        materialsDirty = true;
        availableMarkersDirty = true;
        materialStateByMaterial.Clear();
        if (visualsReady)
        {
            UpdateMaterialsIfNeeded();
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
        ApplyMaterialColorIfChanged(material, color, emissionStrength);
    }

    private void ApplyMaterialColorIfChanged(Material material, Color color, float emissionStrength)
    {
        if (material == null)
        {
            return;
        }

        color.a *= Mathf.Clamp01(radarMaterialAlphaMultiplier);
        CachedMaterialState state;
        if (materialStateByMaterial.TryGetValue(material, out state)
            && state.known
            && AreColorsClose(state.color, color)
            && Mathf.Abs(state.emissionStrength - emissionStrength) <= 0.0001f)
        {
            return;
        }

        ApplyMaterialColorRaw(material, color, emissionStrength);
        state.color = color;
        state.emissionStrength = emissionStrength;
        state.known = true;
        materialStateByMaterial[material] = state;
    }

    private bool AreColorsClose(Color left, Color right)
    {
        return Mathf.Abs(left.r - right.r) <= 0.0001f
            && Mathf.Abs(left.g - right.g) <= 0.0001f
            && Mathf.Abs(left.b - right.b) <= 0.0001f
            && Mathf.Abs(left.a - right.a) <= 0.0001f;
    }

    private void ApplyMaterialColorRaw(Material material, Color color, float emissionStrength)
    {
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
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
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
        float lineHalfWidth = IsRoomCompassModeActive()
            ? 0.002f / safeRange
            : 0.006f;
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

#if FA_RADAR_PRO
    private Mesh CreatePersonMarkerMesh()
    {
        float depth = 0.08f;
        float front = depth * 0.5f;
        float back = -front;
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Person Marker Mesh";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        AddFlatPolygonPrism(
            vertices,
            triangles,
            new Vector2[]
            {
                new Vector2(0.0f, 0.52f),
                new Vector2(0.18f, 0.45f),
                new Vector2(0.25f, 0.28f),
                new Vector2(0.18f, 0.10f),
                new Vector2(0.0f, 0.03f),
                new Vector2(-0.18f, 0.10f),
                new Vector2(-0.25f, 0.28f),
                new Vector2(-0.18f, 0.45f)
            },
            front,
            back);
        AddFlatPolygonPrism(
            vertices,
            triangles,
            new Vector2[]
            {
                new Vector2(-0.36f, -0.05f),
                new Vector2(0.36f, -0.05f),
                new Vector2(0.24f, -0.48f),
                new Vector2(-0.24f, -0.48f)
            },
            front,
            back);
        AddFlatPolygonPrism(
            vertices,
            triangles,
            new Vector2[]
            {
                new Vector2(-0.24f, -0.48f),
                new Vector2(-0.04f, -0.48f),
                new Vector2(-0.12f, -0.82f),
                new Vector2(-0.32f, -0.82f)
            },
            front,
            back);
        AddFlatPolygonPrism(
            vertices,
            triangles,
            new Vector2[]
            {
                new Vector2(0.04f, -0.48f),
                new Vector2(0.24f, -0.48f),
                new Vector2(0.32f, -0.82f),
                new Vector2(0.12f, -0.82f)
            },
            front,
            back);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void AddFlatPolygonPrism(List<Vector3> vertices, List<int> triangles, Vector2[] points, float front, float back)
    {
        if (vertices == null || triangles == null || points == null || points.Length < 3)
        {
            return;
        }

        int frontStart = vertices.Count;
        for (int i = 0; i < points.Length; i++)
        {
            vertices.Add(new Vector3(points[i].x, points[i].y, front));
        }
        int backStart = vertices.Count;
        for (int i = 0; i < points.Length; i++)
        {
            vertices.Add(new Vector3(points[i].x, points[i].y, back));
        }

        for (int i = 1; i < points.Length - 1; i++)
        {
            triangles.Add(frontStart);
            triangles.Add(frontStart + i);
            triangles.Add(frontStart + i + 1);

            triangles.Add(backStart);
            triangles.Add(backStart + i + 1);
            triangles.Add(backStart + i);
        }

        for (int i = 0; i < points.Length; i++)
        {
            int next = (i + 1) % points.Length;
            int a = frontStart + i;
            int b = frontStart + next;
            int c = backStart + next;
            int d = backStart + i;

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }
    }

    private Mesh CreatePanelMarkerMesh()
    {
        return CreateBoxMarkerMesh("FA Radar Panel Marker Mesh", 1.35f, 0.74f, 0.14f);
    }

    private Mesh CreateSubSceneMarkerMesh()
    {
        return CreateBoxMarkerMesh("FA Radar SubScene Marker Mesh", 1.55f, 0.96f, 0.16f);
    }

    private Mesh CreateBoxMarkerMesh(string meshName, float width, float height, float depth)
    {
        float x = Mathf.Max(0.01f, width) * 0.5f;
        float y = Mathf.Max(0.01f, height) * 0.5f;
        float z = Mathf.Max(0.01f, depth) * 0.5f;
        Mesh mesh = new Mesh();
        mesh.name = meshName;
        mesh.vertices = new Vector3[]
        {
            new Vector3(-x, -y, -z),
            new Vector3(x, -y, -z),
            new Vector3(x, y, -z),
            new Vector3(-x, y, -z),
            new Vector3(-x, -y, z),
            new Vector3(x, -y, z),
            new Vector3(x, y, z),
            new Vector3(-x, y, z)
        };
        mesh.triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            3, 6, 2, 3, 7, 6,
            1, 2, 6, 1, 6, 5,
            0, 4, 7, 0, 7, 3
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
#endif

    private Mesh CreateCenterMarkerMesh()
    {
        Mesh mesh = CreateSphereMesh(8, 16, 1.0f);
        mesh.name = "FA Radar Prototype User Center Sphere Mesh";
        return mesh;
    }

    private Mesh CreateHeightStemMesh()
    {
        float width = HeightStemHalfWidth;
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

#if FA_RADAR_PRO
    private Mesh CreateLabelLeaderMesh()
    {
        return CreateBoxMarkerMesh("FA Radar Label Leader Line Mesh", 1.0f, 1.0f, 1.0f);
    }

    private Mesh CreateAxisHalfPairMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Rotation Axis Half Pair Mesh";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        AddBoxToMesh(vertices, triangles, new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.14f, 0.5f, 0.5f));
        AddBoxToMesh(vertices, triangles, new Vector3(0.14f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateAxisCenterCubeMesh()
    {
        return CreateBoxMarkerMesh("FA Radar Rotation Axis Center Cube Mesh", 1.0f, 1.0f, 1.0f);
    }

    private void AddBoxToMesh(List<Vector3> vertices, List<int> triangles, Vector3 min, Vector3 max)
    {
        int index = vertices.Count;
        vertices.Add(new Vector3(min.x, min.y, min.z));
        vertices.Add(new Vector3(max.x, min.y, min.z));
        vertices.Add(new Vector3(max.x, max.y, min.z));
        vertices.Add(new Vector3(min.x, max.y, min.z));
        vertices.Add(new Vector3(min.x, min.y, max.z));
        vertices.Add(new Vector3(max.x, min.y, max.z));
        vertices.Add(new Vector3(max.x, max.y, max.z));
        vertices.Add(new Vector3(min.x, max.y, max.z));

        int[] boxTriangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            3, 6, 2, 3, 7, 6,
            1, 2, 6, 1, 6, 5,
            0, 4, 7, 0, 7, 3
        };
        for (int i = 0; i < boxTriangles.Length; i++)
        {
            triangles.Add(index + boxTriangles[i]);
        }
    }

    private Mesh CreateSpotlightConeMesh(int segments)
    {
        int safeSegments = Mathf.Max(12, segments);
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Spotlight Cone Mesh Open End";
        Vector3[] vertices = new Vector3[safeSegments + 2];
        List<int> triangles = new List<int>();
        vertices[0] = Vector3.zero;
        vertices[1] = new Vector3(0.0f, 0.0f, 1.0f);

        for (int i = 0; i < safeSegments; i++)
        {
            float t = ((float)i / (float)safeSegments) * Mathf.PI * 2.0f;
            vertices[i + 2] = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 1.0f);
        }

        for (int i = 0; i < safeSegments; i++)
        {
            int current = i + 2;
            int next = ((i + 1) % safeSegments) + 2;
            triangles.Add(0);
            triangles.Add(current);
            triangles.Add(next);
            triangles.Add(0);
            triangles.Add(next);
            triangles.Add(current);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateFrustumMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar Camera Frustum Mesh";
        mesh.vertices = new Vector3[]
        {
            Vector3.zero,
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f)
        };
        mesh.triangles = new int[]
        {
            0, 1, 2, 0, 2, 1,
            0, 2, 3, 0, 3, 2,
            0, 3, 4, 0, 4, 3,
            0, 4, 1, 0, 1, 4,
            1, 4, 3, 1, 3, 2,
            2, 3, 4, 2, 4, 1
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
#endif

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
        SetFloatNoCallback(hudScaleField, Mathf.Clamp(scale, MinHudPlacementScale, ResolveMaxHudPlacementScale()));
    }

    private Vector3 GetWristOffset()
    {
        return new Vector3(
            ReadFloat(wristOffsetXField, 0.0f),
            ReadFloat(wristOffsetYField, 0.08f),
            ReadFloat(wristOffsetZField, 0.12f));
    }

    private void SetWristOffsetNoCallback(Vector3 offset)
    {
        SetFloatNoCallback(wristOffsetXField, ClampStorableFloat(wristOffsetXField, offset.x, -0.5f, 0.5f));
        SetFloatNoCallback(wristOffsetYField, ClampStorableFloat(wristOffsetYField, offset.y, -0.5f, 0.5f));
        SetFloatNoCallback(wristOffsetZField, ClampStorableFloat(wristOffsetZField, offset.z, -0.5f, 0.5f));
    }

    private float ResolveActivePlacementScale()
    {
        return IsWristCompassModeActive()
            ? ResolveWristScale()
            : ResolveHudScale();
    }

    private void SetActivePlacementScaleNoCallback(float scale)
    {
        if (IsWristCompassModeActive())
        {
            SetFloatNoCallback(wristScaleField, Mathf.Clamp(scale, 0.05f, ResolveMaxPlacementScale()));
            return;
        }

        SetHudScaleNoCallback(scale);
    }

    private float ResolveHudScale()
    {
        return Mathf.Clamp(ReadFloat(hudScaleField, 0.49f), MinHudPlacementScale, ResolveMaxHudPlacementScale());
    }

    private float ResolveWristScale()
    {
        return Mathf.Clamp(ReadFloat(wristScaleField, 0.35f), 0.05f, ResolveMaxPlacementScale());
    }

    private void SetStaticWorldPositionNoCallback(Vector3 position)
    {
        SetFloatNoCallback(staticWorldXField, ClampStorableFloat(staticWorldXField, position.x, -20.0f, 20.0f));
        SetFloatNoCallback(staticWorldYField, ClampStorableFloat(staticWorldYField, position.y, -5.0f, 20.0f));
        SetFloatNoCallback(staticWorldZField, ClampStorableFloat(staticWorldZField, position.z, -20.0f, 20.0f));
    }

    private void SetStaticWorldRotationNoCallback(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        SetFloatNoCallback(staticWorldPitchField, NormalizeEulerDegrees(euler.x));
        SetFloatNoCallback(staticWorldYawField, NormalizeEulerDegrees(euler.y));
        SetFloatNoCallback(staticWorldRollField, NormalizeEulerDegrees(euler.z));
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

        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        if (anchorHost == null || anchorHost.mainController == null)
        {
            SetStatus("Capture needs the plugin loaded on a movable atom.");
            return;
        }

        Vector3 offset = viewer.InverseTransformPoint(anchorHost.mainController.transform.position);
        Transform anchor = ResolveRadarAnchorTransform(ResolveAnchorMode());
        if (anchor != null)
        {
            offset = anchor.InverseTransformPoint(anchorHost.mainController.transform.position);
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
        Atom anchorHost = ResolveAttachedAtomAnchorHost();
        if (anchorHost == null)
        {
            SetStatus("Containing atom anchor needs the plugin loaded on an atom or CUA.");
            return;
        }

        if (anchorAtomUidField != null)
        {
            anchorAtomUidField.SetVal(anchorHost.uid ?? "");
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

    private void ResetCreatorAnchorPlacement()
    {
        ApplyCreatorAnchorPlacementDefaultsNoCallback();
        haveSmoothedHudPosition = false;
        MarkGlobalPreferencesDirty();
        FlushGlobalPreferencesIfDue(true);
        SetStatus("Anchor placement reset.");
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
        selectedTargetRingObjects = null;
        selectedViewCueObject = null;
        targetHeightStemObject = null;
        targetGridDropObject = null;
        lastTargetBlipObject = null;
        lastTargetGridDropObject = null;
        resizeGuideLineObject = null;
        availableMarkerObjects = null;
        availableStemObjects = null;
#if FA_RADAR_PRO
        targetRotationAxisObjects = null;
        targetLabelObject = null;
        targetLabelLeaderObject = null;
        targetLightRangeObject = null;
        targetSpotlightConeObject = null;
        userPovFrustumObject = null;
        desktopPovFrustumObject = null;
        sceneCameraFrustumObjects = null;
        availableRotationAxisObjects = null;
        availableLightRangeObjects = null;
        availableSpotlightConeObjects = null;
#endif
        ringObjects = null;
        ringBaseRotations = null;
        gridFilter = null;
        currentHudAnchor = null;
        lastGoodViewerTransform = null;
#if FA_RADAR_PRO
        targetLabelText = "";
#endif

        DestroyOwnedObject(shellMaterial);
        DestroyOwnedObject(ringMaterial);
        DestroyOwnedObject(ringXMaterial);
        DestroyOwnedObject(ringZMaterial);
        DestroyOwnedObject(gridMaterial);
        DestroyOwnedObject(centerMaterial);
        DestroyOwnedObject(userHeightStemMaterial);
        DestroyOwnedObject(targetMaterial);
        DestroyOwnedObject(selectedTargetRingXMaterial);
        DestroyOwnedObject(selectedTargetRingYMaterial);
        DestroyOwnedObject(selectedTargetRingZMaterial);
        DestroyOwnedObject(selectedViewCueMaterial);
        DestroyOwnedObject(targetHeightStemMaterial);
        DestroyOwnedObject(targetDropMaterial);
        DestroyOwnedObject(lastTargetMaterial);
        DestroyOwnedObject(lastTargetDropMaterial);
        DestroyOwnedObject(availableHeightStemMaterial);
        DestroyOwnedObject(grabGuideMaterial);
#if FA_RADAR_PRO
        DestroyOwnedObject(rotationAxisXMaterial);
        DestroyOwnedObject(rotationAxisYMaterial);
        DestroyOwnedObject(rotationAxisZMaterial);
        DestroyOwnedObject(rotationAxisCenterMaterial);
        DestroyOwnedObject(targetLabelMaterial);
        DestroyOwnedObject(targetLightRangeMaterial);
        DestroyOwnedObject(targetSpotlightConeMaterial);
        DestroyOwnedObject(userPovFrustumMaterial);
        DestroyOwnedObject(desktopPovFrustumMaterial);
        DestroyOwnedObject(sceneCameraFrustumMaterial);
        if (availableLightRangeMaterials != null)
        {
            for (int i = 0; i < availableLightRangeMaterials.Length; i++)
            {
                DestroyOwnedObject(availableLightRangeMaterials[i]);
            }
        }
        if (availableSpotlightConeMaterials != null)
        {
            for (int i = 0; i < availableSpotlightConeMaterials.Length; i++)
            {
                DestroyOwnedObject(availableSpotlightConeMaterials[i]);
            }
        }
        if (availableMarkerSlots != null)
        {
            for (int i = 0; i < availableMarkerSlots.Length; i++)
            {
                MarkerSlot slot = availableMarkerSlots[i];
                if (slot == null)
                {
                    continue;
                }

                DestroyOwnedObject(slot.labelMaterial);
                DestroyOwnedObject(slot.labelMesh);
                slot.labelObject = null;
                slot.labelLeaderObject = null;
                slot.labelFilter = null;
                slot.labelMaterial = null;
                slot.labelMesh = null;
                slot.labelText = null;
            }
        }
#endif
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
        selectedTargetRingXMaterial = null;
        selectedTargetRingYMaterial = null;
        selectedTargetRingZMaterial = null;
        selectedViewCueMaterial = null;
        targetHeightStemMaterial = null;
        targetDropMaterial = null;
        lastTargetMaterial = null;
        lastTargetDropMaterial = null;
        availableHeightStemMaterial = null;
        grabGuideMaterial = null;
#if FA_RADAR_PRO
        rotationAxisXMaterial = null;
        rotationAxisYMaterial = null;
        rotationAxisZMaterial = null;
        rotationAxisCenterMaterial = null;
        targetLabelMaterial = null;
        targetLightRangeMaterial = null;
        targetSpotlightConeMaterial = null;
        userPovFrustumMaterial = null;
        desktopPovFrustumMaterial = null;
        sceneCameraFrustumMaterial = null;
        availableLightRangeMaterials = null;
        availableSpotlightConeMaterials = null;
#endif
        availableMarkerMaterials = null;
        availableMarkerSlots = null;

        DestroyOwnedObject(sphereMesh);
        DestroyOwnedObject(flatCircleMesh);
        DestroyOwnedObject(ringMesh);
        DestroyOwnedObject(gridMesh);
        DestroyOwnedObject(targetBlipMesh);
#if FA_RADAR_PRO
        DestroyOwnedObject(personMarkerMesh);
        DestroyOwnedObject(panelMarkerMesh);
        DestroyOwnedObject(subSceneMarkerMesh);
#endif
        DestroyOwnedObject(centerMarkerMesh);
        DestroyOwnedObject(heightStemMesh);
        DestroyOwnedObject(resizeGuideLineMesh);
#if FA_RADAR_PRO
        DestroyOwnedObject(rotationAxisHalfPairMesh);
        DestroyOwnedObject(rotationAxisCenterCubeMesh);
        DestroyOwnedObject(targetLabelMesh);
        DestroyOwnedObject(labelLeaderMesh);
        DestroyOwnedObject(lightVolumeSphereMesh);
        DestroyOwnedObject(spotlightConeMesh);
        DestroyOwnedObject(povFrustumMesh);
#endif
        sphereMesh = null;
        flatCircleMesh = null;
        ringMesh = null;
        gridMesh = null;
        targetBlipMesh = null;
#if FA_RADAR_PRO
        personMarkerMesh = null;
        panelMarkerMesh = null;
        subSceneMarkerMesh = null;
#endif
        centerMarkerMesh = null;
        heightStemMesh = null;
        resizeGuideLineMesh = null;
#if FA_RADAR_PRO
        rotationAxisHalfPairMesh = null;
        rotationAxisCenterCubeMesh = null;
        targetLabelMesh = null;
        labelLeaderMesh = null;
        lightVolumeSphereMesh = null;
        spotlightConeMesh = null;
        povFrustumMesh = null;
#endif
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
