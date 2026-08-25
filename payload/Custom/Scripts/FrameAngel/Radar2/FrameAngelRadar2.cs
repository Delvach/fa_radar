using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Radar 2 is intentionally independent from the legacy FrameAngelRadar class.
// It consumes VaM scene state and the accepted FAAR tracked-hand publication only.
public class FrameAngelRadar2 : MVRScript
{
    private const string Version = "2.0.0";

    private const string ModeScene = "Scene";
    private const string ModeRoom = "Room";
    private const string ModeLeftController = "Left Controller";
    private const string ModeRightController = "Right Controller";
    private const string ModeLeftWrist = "Left Wrist";
    private const string ModeRightWrist = "Right Wrist";

    private const string TrackedHandRuntimeRootName = "FAARTrackedHandArmColliders";
    private const string TrackedHandStateSchema = "faar.tracked-hand-state.v7";
    private const string LeftPalmSegmentName = "Segment_0";
    private const string RightPalmSegmentName = "Segment_27";

    private const int LeftHand = 0;
    private const int RightHand = 1;
    private const int NoHand = -1;

    private const float PalmAcquireIntervalSeconds = 0.50f;
    private const float AtomPollIntervalSeconds = 0.75f;
    private const float MapRangeMeters = 5.0f;
    private const float MiniContentScale = 0.020f;
    private const float MarkerRadiusMeters = 0.012f;
    private const float CenterRadiusMeters = 0.018f;
    private const float StemThicknessMeters = 0.0025f;
    private const float SecondHandJoinRadiusMeters = 0.20f;
    private const float MinimumDualDistanceMeters = 0.04f;
    private const float MinimumDisplayScale = 0.05f;
    private const float MaximumDisplayScale = 10.0f;
    private const int MaximumMarkerCount = 96;

    private const int ShellRenderQueue = 4980;
    private const int GridRenderQueue = 4990;
    private const int RingRenderQueue = 5000;
    private const int MarkerRenderQueue = 5010;

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

    private sealed class MarkerSlot
    {
        public Atom atom;
        public Transform atomTransform;
        public Light light;
        public GameObject markerObject;
        public GameObject stemObject;
        public GameObject pointRangeObject;
        public GameObject spotConeObject;
        public Material markerMaterial;
        public Material pointRangeMaterial;
        public Material spotConeMaterial;
        public Color appliedColor;
        public bool colorKnown;
    }

    private enum GrabState
    {
        None,
        SceneSingle,
        SceneDual,
        Wrist
    }

    private JSONStorableBool radarEnabledField;
    private JSONStorableStringChooser modeField;
    private JSONStorableString statusField;

    // Hidden scene storables are the placement authority. There is no global file state.
    private JSONStorableFloat scenePositionXField;
    private JSONStorableFloat scenePositionYField;
    private JSONStorableFloat scenePositionZField;
    private JSONStorableFloat sceneRotationXField;
    private JSONStorableFloat sceneRotationYField;
    private JSONStorableFloat sceneRotationZField;
    private JSONStorableFloat sceneRotationWField;
    private JSONStorableFloat sceneScaleField;

    private readonly JSONStorableFloat[,] wristPositionFields = new JSONStorableFloat[2, 3];
    private readonly JSONStorableFloat[,] wristRotationFields = new JSONStorableFloat[2, 4];

    private GameObject visualRoot;
    private GameObject contentRoot;
    private GameObject shellObject;
    private GameObject gridObject;
    private GameObject centerObject;
    private readonly GameObject[] ringObjects = new GameObject[3];
    private readonly List<Mesh> ownedMeshes = new List<Mesh>();
    private readonly List<Material> ownedMaterials = new List<Material>();
    private readonly List<MarkerSlot> markerSlots = new List<MarkerSlot>();
    private bool visualsReady;

    private GameObject trackedHandRuntimeRoot;
    private readonly Transform[] trackedPalmTransforms = new Transform[2];
    private readonly bool[] trackedHandsLive = new bool[2];
    private readonly bool[] trackedIndexPinched = new bool[2];
    private readonly bool[] trackedHoldGrabLatched = new bool[2];
    private readonly bool[] trackedPalmsPresented = new bool[2];
    private bool trackedHandReceiverRegistered;
    private float nextPalmAcquireAt;

    private readonly List<Atom> visibleAtoms = new List<Atom>();
    private float nextAtomPollAt;

    private GrabState grabState;
    private int primaryGrabHand = NoHand;
    private int secondaryGrabHand = NoHand;
    private Vector3 singlePalmLocalPosition;
    private Quaternion singlePalmLocalRotation = Quaternion.identity;
    private Vector3 dualStartMidpoint;
    private Vector3 dualStartRootPosition;
    private Quaternion dualStartRootRotation = Quaternion.identity;
    private float dualStartDistance;
    private float dualStartScale;
    private string activeMode = ModeScene;
    private string lastStatus = "";

    public override void Init()
    {
        BuildStorables();
        BuildUi();
        EnsureVisuals();
        activeMode = NormalizeMode(modeField.val);
        nextPalmAcquireAt = 0.0f;
        nextAtomPollAt = 0.0f;
        SetStatus("Radar " + Version + " ready.");
    }

    private void LateUpdate()
    {
        if (!visualsReady)
        {
            EnsureVisuals();
        }

        MaintainTrackedHandConnection();
        TickRadar();
    }

    private void OnDestroy()
    {
        EndGrab(false);
        DisconnectTrackedHandRuntime(true);
        DestroyVisuals();
    }

    private void BuildStorables()
    {
        FreeControllerV3 host = ResolveHostController();
        Vector3 initialPosition = ReadControllerPosition(host);
        Quaternion initialRotation = ReadControllerRotation(host);

        radarEnabledField = new JSONStorableBool("Radar Enabled", true);
        modeField = new JSONStorableStringChooser(
            "Mode",
            new List<string>
            {
                ModeScene,
                ModeRoom,
                ModeLeftController,
                ModeRightController,
                ModeLeftWrist,
                ModeRightWrist
            },
            ModeScene,
            "Mode");
        modeField.displayChoices = new List<string>
        {
            ModeScene,
            ModeRoom,
            ModeLeftController,
            ModeRightController,
            ModeLeftWrist,
            ModeRightWrist
        };
        modeField.setCallbackFunction = OnModeChanged;
        statusField = new JSONStorableString("Status", "");

        scenePositionXField = CreateSceneFloat("Scene Position X", initialPosition.x, -100.0f, 100.0f);
        scenePositionYField = CreateSceneFloat("Scene Position Y", initialPosition.y, -100.0f, 100.0f);
        scenePositionZField = CreateSceneFloat("Scene Position Z", initialPosition.z, -100.0f, 100.0f);
        sceneRotationXField = CreateSceneFloat("Scene Rotation X", initialRotation.x, -1.0f, 1.0f);
        sceneRotationYField = CreateSceneFloat("Scene Rotation Y", initialRotation.y, -1.0f, 1.0f);
        sceneRotationZField = CreateSceneFloat("Scene Rotation Z", initialRotation.z, -1.0f, 1.0f);
        sceneRotationWField = CreateSceneFloat("Scene Rotation W", initialRotation.w, -1.0f, 1.0f);
        sceneScaleField = CreateSceneFloat("Scene Scale", 1.0f, MinimumDisplayScale, MaximumDisplayScale);

        for (int hand = LeftHand; hand <= RightHand; hand++)
        {
            string prefix = hand == LeftHand ? "Left Wrist " : "Right Wrist ";
            wristPositionFields[hand, 0] = CreateSceneFloat(prefix + "Local Position X", 0.0f, -2.0f, 2.0f);
            wristPositionFields[hand, 1] = CreateSceneFloat(prefix + "Local Position Y", 0.08f, -2.0f, 2.0f);
            wristPositionFields[hand, 2] = CreateSceneFloat(prefix + "Local Position Z", 0.0f, -2.0f, 2.0f);
            wristRotationFields[hand, 0] = CreateSceneFloat(prefix + "Local Rotation X", 0.0f, -1.0f, 1.0f);
            wristRotationFields[hand, 1] = CreateSceneFloat(prefix + "Local Rotation Y", 0.0f, -1.0f, 1.0f);
            wristRotationFields[hand, 2] = CreateSceneFloat(prefix + "Local Rotation Z", 0.0f, -1.0f, 1.0f);
            wristRotationFields[hand, 3] = CreateSceneFloat(prefix + "Local Rotation W", 1.0f, -1.0f, 1.0f);
        }

        RegisterBool(radarEnabledField);
        RegisterStringChooser(modeField);
        RegisterFloat(scenePositionXField);
        RegisterFloat(scenePositionYField);
        RegisterFloat(scenePositionZField);
        RegisterFloat(sceneRotationXField);
        RegisterFloat(sceneRotationYField);
        RegisterFloat(sceneRotationZField);
        RegisterFloat(sceneRotationWField);
        RegisterFloat(sceneScaleField);
        for (int hand = LeftHand; hand <= RightHand; hand++)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                RegisterFloat(wristPositionFields[hand, axis]);
            }
            for (int component = 0; component < 4; component++)
            {
                RegisterFloat(wristRotationFields[hand, component]);
            }
        }
    }

    private JSONStorableFloat CreateSceneFloat(string name, float value, float minimum, float maximum)
    {
        return new JSONStorableFloat(name, value, minimum, maximum, true, true);
    }

    private void BuildUi()
    {
        CreateToggle(radarEnabledField, false);
        CreatePopup(modeField, false);
        CreateTextField(statusField, true);
    }

    private void OnModeChanged(string requestedMode)
    {
        string normalized = NormalizeMode(requestedMode);
        if (!string.Equals(normalized, requestedMode, StringComparison.Ordinal))
        {
            modeField.valNoCallback = normalized;
        }

        if (grabState == GrabState.SceneSingle || grabState == GrabState.SceneDual)
        {
            CaptureScenePose();
        }
        EndGrab(false);
        activeMode = normalized;
    }

    private string NormalizeMode(string value)
    {
        if (string.Equals(value, ModeRoom, StringComparison.Ordinal)
            || string.Equals(value, ModeLeftController, StringComparison.Ordinal)
            || string.Equals(value, ModeRightController, StringComparison.Ordinal)
            || string.Equals(value, ModeLeftWrist, StringComparison.Ordinal)
            || string.Equals(value, ModeRightWrist, StringComparison.Ordinal))
        {
            return value;
        }
        return ModeScene;
    }

    private void TickRadar()
    {
        if (visualRoot == null)
        {
            visualsReady = false;
            return;
        }

        if (!radarEnabledField.val)
        {
            SetVisualRootVisible(false);
            SetStatus("Radar disabled.");
            return;
        }

        activeMode = NormalizeMode(modeField.val);
        bool visible;
        bool roomMode = false;

        if (string.Equals(activeMode, ModeRoom, StringComparison.Ordinal))
        {
            EndGrab(false);
            ApplyRootPose(Vector3.zero, Quaternion.identity, 1.0f);
            visible = true;
            roomMode = true;
            SetStatus("Room: exact world origin, identity rotation, 1:1 scale.");
        }
        else if (string.Equals(activeMode, ModeLeftController, StringComparison.Ordinal))
        {
            EndGrab(false);
            visible = ApplyControllerMode(LeftHand);
        }
        else if (string.Equals(activeMode, ModeRightController, StringComparison.Ordinal))
        {
            EndGrab(false);
            visible = ApplyControllerMode(RightHand);
        }
        else if (string.Equals(activeMode, ModeLeftWrist, StringComparison.Ordinal))
        {
            visible = ApplyWristMode(LeftHand);
        }
        else if (string.Equals(activeMode, ModeRightWrist, StringComparison.Ordinal))
        {
            visible = ApplyWristMode(RightHand);
        }
        else
        {
            visible = ApplySceneMode();
        }

        SetVisualRootVisible(visible);
        if (!visible)
        {
            return;
        }

        PollAtomsIfDue();
        UpdateVisualContent(roomMode);
    }

    private bool ApplySceneMode()
    {
        if (grabState == GrabState.Wrist)
        {
            EndGrab(false);
        }

        if (grabState == GrabState.None)
        {
            ApplyRootPose(ReadScenePosition(), ReadSceneRotation(), ReadSceneScale());
            PositionHostGrabTargetAtRadar();
            TryStartSceneGrab();
        }
        else
        {
            UpdateSceneGrab();
        }

        if (grabState == GrabState.SceneDual)
        {
            SetStatus("Scene: two-hand uniform resize.");
        }
        else if (grabState == GrabState.SceneSingle)
        {
            SetStatus("Scene: hand placement.");
        }
        else if (!trackedHandReceiverRegistered)
        {
            SetStatus("Scene: placed; tracked-hand runtime disconnected.");
        }
        else
        {
            SetStatus("Scene: saved pose; grab the center with either hand.");
        }
        return true;
    }

    private bool ApplyControllerMode(int hand)
    {
        Transform controller = ResolveMotionControllerTransform(hand);
        if (controller == null)
        {
            SetStatus((hand == LeftHand ? "Left" : "Right") + " Controller: source unavailable.");
            return false;
        }

        Vector3 localOffset = hand == LeftHand
            ? new Vector3(0.055f, 0.045f, 0.10f)
            : new Vector3(-0.055f, 0.045f, 0.10f);
        Quaternion localRotation = Quaternion.Euler(42.0f, 0.0f, 0.0f);
        ApplyRootPose(
            controller.TransformPoint(localOffset),
            controller.rotation * localRotation,
            ReadSceneScale());
        SetStatus((hand == LeftHand ? "Left" : "Right") + " Controller: attached.");
        return true;
    }

    private bool ApplyWristMode(int hand)
    {
        Transform palm = ResolveLivePalm(hand);
        if (palm == null)
        {
            if (grabState == GrabState.Wrist)
            {
                EndGrab(false);
            }
            SetStatus((hand == LeftHand ? "Left" : "Right") + " Wrist: tracked palm unavailable.");
            return false;
        }

        bool presented = trackedPalmsPresented[hand];
        if (grabState == GrabState.Wrist && primaryGrabHand == hand)
        {
            UpdateWristGrab(hand, palm, presented);
            if (string.Equals(activeMode, ModeScene, StringComparison.Ordinal))
            {
                SetStatus("Scene: wrist handoff complete.");
                return true;
            }
            if (!presented)
            {
                return false;
            }
            return true;
        }

        if (grabState != GrabState.None)
        {
            EndGrab(false);
        }

        if (!presented)
        {
            SetStatus((hand == LeftHand ? "Left" : "Right") + " Wrist: hidden until palm presentation.");
            return false;
        }

        ApplyWristAnchor(hand, palm);
        PositionHostGrabTargetAtRadar();
        if (IsHostGrabbedByHand(hand))
        {
            StartWristGrab(hand, palm);
            UpdateWristGrab(hand, palm, true);
            SetStatus((hand == LeftHand ? "Left" : "Right") + " Wrist: center grab active.");
        }
        else
        {
            SetStatus((hand == LeftHand ? "Left" : "Right") + " Wrist: presented.");
        }
        return true;
    }

    private void ApplyWristAnchor(int hand, Transform palm)
    {
        Vector3 localPosition = ReadWristLocalPosition(hand);
        Quaternion localRotation = ReadWristLocalRotation(hand);
        ApplyRootPose(
            palm.TransformPoint(localPosition),
            palm.rotation * localRotation,
            ReadSceneScale());
    }

    private void StartWristGrab(int hand, Transform palm)
    {
        grabState = GrabState.Wrist;
        primaryGrabHand = hand;
        secondaryGrabHand = NoHand;
        RebaseSinglePalm(palm);
    }

    private void UpdateWristGrab(int hand, Transform palm, bool presented)
    {
        if (!presented)
        {
            if (IsHostGrabbedByHand(hand))
            {
                FreeControllerV3 host = ResolveHostController();
                if (host != null)
                {
                    ApplyRootPose(ReadControllerPosition(host), ReadControllerRotation(host), ReadSceneScale());
                }
            }
            CaptureScenePose();
            EndGrab(false);
            modeField.valNoCallback = ModeScene;
            activeMode = ModeScene;
            PositionHostGrabTargetAtRadar();
            return;
        }

        if (IsHostGrabbedByHand(hand))
        {
            ApplyRootPose(ReadControllerPosition(ResolveHostController()), ReadControllerRotation(ResolveHostController()), ReadSceneScale());
            RebaseSinglePalm(palm);
            return;
        }

        if (IsHandGrabSignalActive(hand))
        {
            ApplyRootPose(
                palm.TransformPoint(singlePalmLocalPosition),
                palm.rotation * singlePalmLocalRotation,
                ReadSceneScale());
            return;
        }

        WriteWristLocalPose(hand, palm);
        EndGrab(false);
        PositionHostGrabTargetAtRadar();
    }

    private void TryStartSceneGrab()
    {
        int hand = ResolveHostGrabbedHand();
        if (hand == NoHand)
        {
            return;
        }

        Transform palm = ResolveLivePalm(hand);
        if (palm == null)
        {
            return;
        }

        grabState = GrabState.SceneSingle;
        primaryGrabHand = hand;
        secondaryGrabHand = NoHand;
        RebaseSinglePalm(palm);
        UpdateSceneSingleGrab();
    }

    private void UpdateSceneGrab()
    {
        if (grabState == GrabState.SceneDual)
        {
            UpdateSceneDualGrab();
            return;
        }
        if (grabState == GrabState.SceneSingle)
        {
            UpdateSceneSingleGrab();
        }
    }

    private void UpdateSceneSingleGrab()
    {
        Transform palm = ResolveLivePalm(primaryGrabHand);
        if (palm == null || !IsHandGrabSignalActive(primaryGrabHand))
        {
            CaptureScenePose();
            EndGrab(false);
            return;
        }

        if (IsHostGrabbedByHand(primaryGrabHand))
        {
            FreeControllerV3 host = ResolveHostController();
            ApplyRootPose(ReadControllerPosition(host), ReadControllerRotation(host), ReadSceneScale());
            RebaseSinglePalm(palm);
        }
        else
        {
            ApplyRootPose(
                palm.TransformPoint(singlePalmLocalPosition),
                palm.rotation * singlePalmLocalRotation,
                ReadSceneScale());
        }

        int otherHand = primaryGrabHand == LeftHand ? RightHand : LeftHand;
        Transform otherPalm = ResolveLivePalm(otherHand);
        if (otherPalm != null
            && IsHandGrabSignalActive(otherHand)
            && Vector3.Distance(otherPalm.position, visualRoot.transform.position) <= SecondHandJoinRadiusMeters)
        {
            StartSceneDualGrab(otherHand, palm, otherPalm);
        }

        CaptureScenePose();
    }

    private void StartSceneDualGrab(int otherHand, Transform primaryPalm, Transform otherPalm)
    {
        float distance = Vector3.Distance(primaryPalm.position, otherPalm.position);
        if (distance < MinimumDualDistanceMeters)
        {
            return;
        }

        secondaryGrabHand = otherHand;
        dualStartMidpoint = (primaryPalm.position + otherPalm.position) * 0.5f;
        dualStartRootPosition = visualRoot.transform.position;
        dualStartRootRotation = visualRoot.transform.rotation;
        dualStartDistance = distance;
        dualStartScale = ReadSceneScale();
        grabState = GrabState.SceneDual;
    }

    private void UpdateSceneDualGrab()
    {
        bool primaryActive = ResolveLivePalm(primaryGrabHand) != null && IsHandGrabSignalActive(primaryGrabHand);
        bool secondaryActive = ResolveLivePalm(secondaryGrabHand) != null && IsHandGrabSignalActive(secondaryGrabHand);

        if (!primaryActive || !secondaryActive)
        {
            int remainingHand = primaryActive ? primaryGrabHand : (secondaryActive ? secondaryGrabHand : NoHand);
            if (remainingHand == NoHand)
            {
                CaptureScenePose();
                EndGrab(false);
                return;
            }

            primaryGrabHand = remainingHand;
            secondaryGrabHand = NoHand;
            grabState = GrabState.SceneSingle;
            RebaseSinglePalm(ResolveLivePalm(remainingHand));
            CaptureScenePose();
            return;
        }

        Transform primaryPalm = ResolveLivePalm(primaryGrabHand);
        Transform secondaryPalm = ResolveLivePalm(secondaryGrabHand);
        Vector3 midpoint = (primaryPalm.position + secondaryPalm.position) * 0.5f;
        float distance = Mathf.Max(MinimumDualDistanceMeters, Vector3.Distance(primaryPalm.position, secondaryPalm.position));
        float ratio = distance / Mathf.Max(MinimumDualDistanceMeters, dualStartDistance);
        float scale = Mathf.Clamp(dualStartScale * ratio, MinimumDisplayScale, MaximumDisplayScale);
        Vector3 position = midpoint + ((dualStartRootPosition - dualStartMidpoint) * ratio);
        ApplyRootPose(position, dualStartRootRotation, scale);
        CaptureScenePose();
    }

    private void RebaseSinglePalm(Transform palm)
    {
        if (palm == null || visualRoot == null)
        {
            return;
        }
        singlePalmLocalPosition = palm.InverseTransformPoint(visualRoot.transform.position);
        singlePalmLocalRotation = Quaternion.Inverse(palm.rotation) * visualRoot.transform.rotation;
    }

    private void EndGrab(bool captureScene)
    {
        if (captureScene && (grabState == GrabState.SceneSingle || grabState == GrabState.SceneDual))
        {
            CaptureScenePose();
        }
        grabState = GrabState.None;
        primaryGrabHand = NoHand;
        secondaryGrabHand = NoHand;
    }

    private bool IsHandGrabSignalActive(int hand)
    {
        if (hand < LeftHand || hand > RightHand || !trackedHandsLive[hand])
        {
            return false;
        }
        return IsHostGrabbedByHand(hand)
            || trackedIndexPinched[hand]
            || trackedHoldGrabLatched[hand];
    }

    private int ResolveHostGrabbedHand()
    {
        if (IsHostGrabbedByHand(LeftHand))
        {
            return LeftHand;
        }
        if (IsHostGrabbedByHand(RightHand))
        {
            return RightHand;
        }
        return NoHand;
    }

    private bool IsHostGrabbedByHand(int hand)
    {
        FreeControllerV3 host = ResolveHostController();
        SuperController sc = SuperController.singleton;
        if (host == null || sc == null || hand < LeftHand || hand > RightHand)
        {
            return false;
        }

        try
        {
            return hand == LeftHand
                ? sc.LeftGrabbedController == host || sc.LeftFullGrabbedController == host
                : sc.RightGrabbedController == host || sc.RightFullGrabbedController == host;
        }
        catch
        {
            return false;
        }
    }

    private FreeControllerV3 ResolveHostController()
    {
        return containingAtom != null ? containingAtom.mainController : null;
    }

    private void PositionHostGrabTargetAtRadar()
    {
        FreeControllerV3 host = ResolveHostController();
        if (host == null || visualRoot == null)
        {
            return;
        }

        try
        {
            if (host.control != null)
            {
                host.control.position = visualRoot.transform.position;
                host.control.rotation = visualRoot.transform.rotation;
            }
            else
            {
                host.transform.position = visualRoot.transform.position;
                host.transform.rotation = visualRoot.transform.rotation;
            }
        }
        catch
        {
        }
    }

    private Vector3 ReadControllerPosition(FreeControllerV3 controller)
    {
        if (controller == null)
        {
            return Vector3.zero;
        }
        try
        {
            return controller.control != null ? controller.control.position : controller.transform.position;
        }
        catch
        {
            return controller.transform.position;
        }
    }

    private Quaternion ReadControllerRotation(FreeControllerV3 controller)
    {
        if (controller == null)
        {
            return Quaternion.identity;
        }
        try
        {
            return controller.control != null ? controller.control.rotation : controller.transform.rotation;
        }
        catch
        {
            return controller.transform.rotation;
        }
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
            Camera camera = hand == LeftHand ? sc.leftControllerCamera : sc.rightControllerCamera;
            if (camera == null || !camera.gameObject.activeInHierarchy)
            {
                return null;
            }
            return camera.transform;
        }
        catch
        {
            return null;
        }
    }

    private void MaintainTrackedHandConnection()
    {
        if (trackedHandReceiverRegistered)
        {
            if (trackedHandRuntimeRoot != null
                && trackedPalmTransforms[LeftHand] != null
                && trackedPalmTransforms[RightHand] != null)
            {
                return;
            }
            DisconnectTrackedHandRuntime(true);
            nextPalmAcquireAt = 0.0f;
        }

        if (Time.unscaledTime < nextPalmAcquireAt)
        {
            return;
        }
        nextPalmAcquireAt = Time.unscaledTime + PalmAcquireIntervalSeconds;
        TryConnectTrackedHandRuntime();
    }

    private void TryConnectTrackedHandRuntime()
    {
        GameObject candidate = GameObject.Find(TrackedHandRuntimeRootName);
        if (candidate == null)
        {
            return;
        }

        Transform leftPalm = candidate.transform.Find(LeftPalmSegmentName);
        Transform rightPalm = candidate.transform.Find(RightPalmSegmentName);
        if (leftPalm == null || rightPalm == null)
        {
            return;
        }

        try
        {
            candidate.SendMessage(
                "RegisterHandStateReceiver",
                gameObject,
                SendMessageOptions.RequireReceiver);
            trackedHandRuntimeRoot = candidate;
            trackedPalmTransforms[LeftHand] = leftPalm;
            trackedPalmTransforms[RightHand] = rightPalm;
            trackedHandReceiverRegistered = true;
        }
        catch
        {
            DisconnectTrackedHandRuntime(false);
        }
    }

    private void DisconnectTrackedHandRuntime(bool unregister)
    {
        GameObject previousRoot = trackedHandRuntimeRoot;
        bool wasRegistered = trackedHandReceiverRegistered;
        trackedHandRuntimeRoot = null;
        trackedPalmTransforms[LeftHand] = null;
        trackedPalmTransforms[RightHand] = null;
        trackedHandReceiverRegistered = false;
        InvalidateTrackedHandState();

        if (!unregister || !wasRegistered || previousRoot == null)
        {
            return;
        }
        try
        {
            previousRoot.SendMessage(
                "UnregisterHandStateReceiver",
                gameObject,
                SendMessageOptions.DontRequireReceiver);
        }
        catch
        {
        }
    }

    private void InvalidateTrackedHandState()
    {
        for (int hand = LeftHand; hand <= RightHand; hand++)
        {
            trackedHandsLive[hand] = false;
            trackedIndexPinched[hand] = false;
            trackedHoldGrabLatched[hand] = false;
            trackedPalmsPresented[hand] = false;
        }
    }

    public void ApplyHandRuntimeStateJson(string json)
    {
        TrackedHandRuntimeState state = null;
        try
        {
            state = JsonUtility.FromJson<TrackedHandRuntimeState>(json);
        }
        catch
        {
            state = null;
        }

        if (state == null || !string.Equals(state.schema, TrackedHandStateSchema, StringComparison.Ordinal))
        {
            InvalidateTrackedHandState();
            return;
        }

        trackedHandsLive[LeftHand] = state.leftTracking;
        trackedHandsLive[RightHand] = state.rightTracking;
        trackedIndexPinched[LeftHand] = state.leftIndexPinched;
        trackedIndexPinched[RightHand] = state.rightIndexPinched;
        trackedHoldGrabLatched[LeftHand] = state.leftHoldGrabLatched;
        trackedHoldGrabLatched[RightHand] = state.rightHoldGrabLatched;
        trackedPalmsPresented[LeftHand] = state.leftPalmPresented;
        trackedPalmsPresented[RightHand] = state.rightPalmPresented;
    }

    private Transform ResolveLivePalm(int hand)
    {
        if (!trackedHandReceiverRegistered
            || hand < LeftHand
            || hand > RightHand
            || !trackedHandsLive[hand])
        {
            return null;
        }
        Transform palm = trackedPalmTransforms[hand];
        if (palm == null || !palm.gameObject.activeInHierarchy)
        {
            return null;
        }
        return palm;
    }

    private Vector3 ReadScenePosition()
    {
        return new Vector3(scenePositionXField.val, scenePositionYField.val, scenePositionZField.val);
    }

    private Quaternion ReadSceneRotation()
    {
        Quaternion rotation = new Quaternion(
            sceneRotationXField.val,
            sceneRotationYField.val,
            sceneRotationZField.val,
            sceneRotationWField.val);
        float magnitudeSquared = rotation.x * rotation.x
            + rotation.y * rotation.y
            + rotation.z * rotation.z
            + rotation.w * rotation.w;
        return magnitudeSquared > 0.0001f ? NormalizeQuaternion(rotation) : Quaternion.identity;
    }

    private float ReadSceneScale()
    {
        return Mathf.Clamp(sceneScaleField.val, MinimumDisplayScale, MaximumDisplayScale);
    }

    private void CaptureScenePose()
    {
        if (visualRoot == null)
        {
            return;
        }
        Vector3 position = visualRoot.transform.position;
        Quaternion rotation = NormalizeQuaternion(visualRoot.transform.rotation);
        float scale = Mathf.Clamp(visualRoot.transform.lossyScale.x, MinimumDisplayScale, MaximumDisplayScale);
        scenePositionXField.valNoCallback = position.x;
        scenePositionYField.valNoCallback = position.y;
        scenePositionZField.valNoCallback = position.z;
        sceneRotationXField.valNoCallback = rotation.x;
        sceneRotationYField.valNoCallback = rotation.y;
        sceneRotationZField.valNoCallback = rotation.z;
        sceneRotationWField.valNoCallback = rotation.w;
        sceneScaleField.valNoCallback = scale;
    }

    private Vector3 ReadWristLocalPosition(int hand)
    {
        return new Vector3(
            wristPositionFields[hand, 0].val,
            wristPositionFields[hand, 1].val,
            wristPositionFields[hand, 2].val);
    }

    private Quaternion ReadWristLocalRotation(int hand)
    {
        Quaternion rotation = new Quaternion(
            wristRotationFields[hand, 0].val,
            wristRotationFields[hand, 1].val,
            wristRotationFields[hand, 2].val,
            wristRotationFields[hand, 3].val);
        return NormalizeQuaternion(rotation);
    }

    private void WriteWristLocalPose(int hand, Transform palm)
    {
        if (palm == null || visualRoot == null)
        {
            return;
        }
        Vector3 localPosition = palm.InverseTransformPoint(visualRoot.transform.position);
        Quaternion localRotation = NormalizeQuaternion(Quaternion.Inverse(palm.rotation) * visualRoot.transform.rotation);
        wristPositionFields[hand, 0].valNoCallback = localPosition.x;
        wristPositionFields[hand, 1].valNoCallback = localPosition.y;
        wristPositionFields[hand, 2].valNoCallback = localPosition.z;
        wristRotationFields[hand, 0].valNoCallback = localRotation.x;
        wristRotationFields[hand, 1].valNoCallback = localRotation.y;
        wristRotationFields[hand, 2].valNoCallback = localRotation.z;
        wristRotationFields[hand, 3].valNoCallback = localRotation.w;
    }

    private Quaternion NormalizeQuaternion(Quaternion rotation)
    {
        float magnitude = Mathf.Sqrt(
            rotation.x * rotation.x
            + rotation.y * rotation.y
            + rotation.z * rotation.z
            + rotation.w * rotation.w);
        if (magnitude <= 0.0001f)
        {
            return Quaternion.identity;
        }
        float inverse = 1.0f / magnitude;
        return new Quaternion(
            rotation.x * inverse,
            rotation.y * inverse,
            rotation.z * inverse,
            rotation.w * inverse);
    }

    private void ApplyRootPose(Vector3 position, Quaternion rotation, float scale)
    {
        if (visualRoot == null)
        {
            return;
        }
        visualRoot.transform.position = position;
        visualRoot.transform.rotation = rotation;
        visualRoot.transform.localScale = Vector3.one * Mathf.Clamp(scale, MinimumDisplayScale, MaximumDisplayScale);
    }

    private void PollAtomsIfDue()
    {
        if (Time.unscaledTime < nextAtomPollAt)
        {
            return;
        }
        nextAtomPollAt = Time.unscaledTime + AtomPollIntervalSeconds;
        visibleAtoms.Clear();
        SuperController sc = SuperController.singleton;
        if (sc == null)
        {
            BindMarkerSlots();
            return;
        }

        List<Atom> atoms = null;
        try
        {
            atoms = sc.GetAtoms();
        }
        catch
        {
            atoms = null;
        }
        if (atoms != null)
        {
            for (int i = 0; i < atoms.Count && visibleAtoms.Count < MaximumMarkerCount; i++)
            {
                Atom atom = atoms[i];
                if (atom == null || atom == containingAtom)
                {
                    continue;
                }
                visibleAtoms.Add(atom);
            }
        }
        BindMarkerSlots();
    }

    private void BindMarkerSlots()
    {
        EnsureMarkerSlotCount(visibleAtoms.Count);
        for (int i = 0; i < markerSlots.Count; i++)
        {
            MarkerSlot slot = markerSlots[i];
            if (i >= visibleAtoms.Count)
            {
                slot.atom = null;
                slot.atomTransform = null;
                slot.light = null;
                SetSlotVisible(slot, false);
                continue;
            }

            Atom atom = visibleAtoms[i];
            slot.atom = atom;
            slot.atomTransform = ResolveAtomTransform(atom);
            slot.light = ResolveUnityLight(atom);
        }
    }

    private Transform ResolveAtomTransform(Atom atom)
    {
        if (atom == null)
        {
            return null;
        }
        try
        {
            return atom.mainController != null ? atom.mainController.transform : atom.transform;
        }
        catch
        {
            return null;
        }
    }

    private Light ResolveUnityLight(Atom atom)
    {
        if (atom == null)
        {
            return null;
        }
        try
        {
            return atom.GetComponentInChildren<Light>(true);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateVisualContent(bool roomMode)
    {
        if (contentRoot == null)
        {
            return;
        }
        float contentScale = roomMode ? 1.0f : MiniContentScale;
        contentRoot.transform.localPosition = Vector3.zero;
        contentRoot.transform.localRotation = Quaternion.identity;
        contentRoot.transform.localScale = Vector3.one * contentScale;

        Transform viewer = ResolveViewerTransform();
        Vector3 referencePosition = roomMode || viewer == null ? Vector3.zero : viewer.position;

        float shellMarkerScale = roomMode
            ? MarkerRadiusMeters
            : MarkerRadiusMeters / MiniContentScale;
        float centerScale = roomMode
            ? CenterRadiusMeters
            : CenterRadiusMeters / MiniContentScale;
        centerObject.transform.localScale = Vector3.one * centerScale;

        for (int i = 0; i < markerSlots.Count; i++)
        {
            UpdateMarkerSlot(markerSlots[i], referencePosition, roomMode, contentScale, shellMarkerScale);
        }
    }

    private void UpdateMarkerSlot(
        MarkerSlot slot,
        Vector3 referencePosition,
        bool roomMode,
        float contentScale,
        float markerScale)
    {
        if (slot == null || slot.atom == null || slot.atomTransform == null)
        {
            SetSlotVisible(slot, false);
            return;
        }

        Transform target = slot.light != null ? slot.light.transform : slot.atomTransform;
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            SetSlotVisible(slot, false);
            return;
        }

        Vector3 localPosition = target.position - referencePosition;
        if (!roomMode && localPosition.magnitude > MapRangeMeters)
        {
            SetSlotVisible(slot, false);
            return;
        }

        SetActiveIfChanged(slot.markerObject, true);
        slot.markerObject.transform.localPosition = localPosition;
        slot.markerObject.transform.localRotation = Quaternion.Inverse(visualRoot.transform.rotation) * target.rotation;
        slot.markerObject.transform.localScale = Vector3.one * markerScale;

        Color color = ResolveMarkerColor(slot);
        ApplyMaterialColorIfNeeded(slot, color);

        float stemHeight = localPosition.y;
        bool showStem = Mathf.Abs(stemHeight) > 0.01f;
        SetActiveIfChanged(slot.stemObject, showStem);
        if (showStem)
        {
            float thickness = StemThicknessMeters / Mathf.Max(0.0001f, contentScale);
            slot.stemObject.transform.localPosition = new Vector3(localPosition.x, stemHeight * 0.5f, localPosition.z);
            slot.stemObject.transform.localRotation = Quaternion.identity;
            slot.stemObject.transform.localScale = new Vector3(thickness, Mathf.Abs(stemHeight), thickness);
        }

        UpdateLightObjects(slot, localPosition, target.rotation, color);
    }

    private void UpdateLightObjects(MarkerSlot slot, Vector3 localPosition, Quaternion worldRotation, Color color)
    {
        Light light = slot.light;
        bool pointVisible = light != null && light.enabled && light.type == LightType.Point;
        bool spotVisible = light != null && light.enabled && light.type == LightType.Spot;
        SetActiveIfChanged(slot.pointRangeObject, pointVisible);
        SetActiveIfChanged(slot.spotConeObject, spotVisible);

        if (pointVisible)
        {
            slot.pointRangeObject.transform.localPosition = localPosition;
            slot.pointRangeObject.transform.localRotation = Quaternion.identity;
            slot.pointRangeObject.transform.localScale = Vector3.one * Mathf.Max(0.001f, light.range);
            ApplyMaterialColor(slot.pointRangeMaterial, WithAlpha(color, 0.035f), 0.30f);
        }

        if (spotVisible)
        {
            float range = Mathf.Max(0.001f, light.range);
            float radius = Mathf.Tan(Mathf.Clamp(light.spotAngle, 0.0f, 179.0f) * 0.5f * Mathf.Deg2Rad) * range;
            slot.spotConeObject.transform.localPosition = localPosition;
            slot.spotConeObject.transform.localRotation = Quaternion.Inverse(visualRoot.transform.rotation) * worldRotation;
            slot.spotConeObject.transform.localScale = new Vector3(radius, radius, range);
            ApplyMaterialColor(slot.spotConeMaterial, WithAlpha(color, 0.045f), 0.30f);
        }
    }

    private Color ResolveMarkerColor(MarkerSlot slot)
    {
        if (slot.light != null)
        {
            Color lightColor = slot.light.color;
            lightColor.a = 0.92f;
            return lightColor;
        }
        string type = slot.atom != null ? slot.atom.type ?? "" : "";
        if (type.IndexOf("Person", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return new Color(1.0f, 0.35f, 0.72f, 0.88f);
        }
        if (type.IndexOf("CustomUnityAsset", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return new Color(1.0f, 0.76f, 0.18f, 0.88f);
        }
        return new Color(0.45f, 0.88f, 1.0f, 0.82f);
    }

    private void ApplyMaterialColorIfNeeded(MarkerSlot slot, Color color)
    {
        if (slot.colorKnown && ColorsClose(slot.appliedColor, color))
        {
            return;
        }
        ApplyMaterialColor(slot.markerMaterial, color, 1.2f);
        slot.appliedColor = color;
        slot.colorKnown = true;
    }

    private bool ColorsClose(Color left, Color right)
    {
        return Mathf.Abs(left.r - right.r) < 0.001f
            && Mathf.Abs(left.g - right.g) < 0.001f
            && Mathf.Abs(left.b - right.b) < 0.001f
            && Mathf.Abs(left.a - right.a) < 0.001f;
    }

    private void EnsureVisuals()
    {
        if (visualsReady)
        {
            return;
        }

        Mesh sphereMesh = OwnMesh(CreateSphereMesh(16, 28, MapRangeMeters, "FA Radar 2 Sphere"));
        Mesh ringMesh = OwnMesh(CreateRingMesh(72, MapRangeMeters - 0.025f, MapRangeMeters, "FA Radar 2 Ring"));
        Mesh gridMesh = OwnMesh(CreateGridMesh(MapRangeMeters, 1.0f));
        Mesh markerMesh = OwnMesh(CreateSphereMesh(8, 14, 1.0f, "FA Radar 2 Marker"));

        Material shellMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Shell Material", new Color(0.16f, 0.64f, 0.92f, 0.055f), ShellRenderQueue));
        Material gridMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Grid Material", new Color(0.55f, 0.95f, 1.0f, 0.11f), GridRenderQueue));
        Material centerMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Center Material", new Color(0.40f, 1.0f, 0.62f, 0.90f), MarkerRenderQueue));
        Material ringXMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 X Ring Material", new Color(1.0f, 0.18f, 0.12f, 0.30f), RingRenderQueue));
        Material ringYMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Y Ring Material", new Color(0.22f, 1.0f, 0.34f, 0.30f), RingRenderQueue));
        Material ringZMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Z Ring Material", new Color(0.26f, 0.52f, 1.0f, 0.30f), RingRenderQueue));

        visualRoot = new GameObject("FA Radar 2 Visual Root");
        contentRoot = new GameObject("FA Radar 2 Content Root");
        contentRoot.transform.SetParent(visualRoot.transform, false);
        shellObject = CreateMeshObject("FA Radar 2 Sphere", contentRoot.transform, sphereMesh, shellMaterial, ShellRenderQueue);
        gridObject = CreateMeshObject("FA Radar 2 Meter Grid", contentRoot.transform, gridMesh, gridMaterial, GridRenderQueue);
        centerObject = CreateMeshObject("FA Radar 2 Center Grab Target Visual", contentRoot.transform, markerMesh, centerMaterial, MarkerRenderQueue);
        ringObjects[0] = CreateMeshObject("FA Radar 2 Ring XY", contentRoot.transform, ringMesh, ringZMaterial, RingRenderQueue);
        ringObjects[1] = CreateMeshObject("FA Radar 2 Ring XZ", contentRoot.transform, ringMesh, ringYMaterial, RingRenderQueue);
        ringObjects[2] = CreateMeshObject("FA Radar 2 Ring YZ", contentRoot.transform, ringMesh, ringXMaterial, RingRenderQueue);
        ringObjects[1].transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        ringObjects[2].transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

        SetVisualRootVisible(false);
        visualsReady = true;
    }

    private void EnsureMarkerSlotCount(int count)
    {
        while (markerSlots.Count < count)
        {
            MarkerSlot slot = new MarkerSlot();
            int index = markerSlots.Count;
            Mesh markerMesh = OwnMesh(CreateSphereMesh(8, 14, 1.0f, "FA Radar 2 Marker Mesh " + index));
            Mesh stemMesh = OwnMesh(CreateBoxMesh("FA Radar 2 Stem Mesh " + index));
            Mesh rangeMesh = OwnMesh(CreateSphereMesh(10, 18, 1.0f, "FA Radar 2 Light Range Mesh " + index));
            Mesh coneMesh = OwnMesh(CreateSpotlightConeMesh(32, "FA Radar 2 Spotlight Mesh " + index));
            slot.markerMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Marker Material " + index, new Color(0.45f, 0.88f, 1.0f, 0.82f), MarkerRenderQueue));
            slot.pointRangeMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Point Range Material " + index, new Color(1.0f, 0.82f, 0.35f, 0.035f), MarkerRenderQueue - 2));
            slot.spotConeMaterial = OwnMaterial(CreateOverlayMaterial("FA Radar 2 Spot Cone Material " + index, new Color(1.0f, 0.82f, 0.35f, 0.045f), MarkerRenderQueue - 1));
            slot.markerObject = CreateMeshObject("FA Radar 2 Atom " + index, contentRoot.transform, markerMesh, slot.markerMaterial, MarkerRenderQueue);
            slot.stemObject = CreateMeshObject("FA Radar 2 Height Stem " + index, contentRoot.transform, stemMesh, slot.markerMaterial, MarkerRenderQueue - 3);
            slot.pointRangeObject = CreateMeshObject("FA Radar 2 Point Range " + index, contentRoot.transform, rangeMesh, slot.pointRangeMaterial, MarkerRenderQueue - 2);
            slot.spotConeObject = CreateMeshObject("FA Radar 2 Spotlight Cone " + index, contentRoot.transform, coneMesh, slot.spotConeMaterial, MarkerRenderQueue - 1);
            SetSlotVisible(slot, false);
            markerSlots.Add(slot);
        }
    }

    private GameObject CreateMeshObject(string name, Transform parent, Mesh mesh, Material material, int renderQueue)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(parent, false);
        MeshFilter filter = target.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = target.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.sortingOrder = 32740;
        ApplyOverlaySettings(material, renderQueue);
        return target;
    }

    private Material CreateOverlayMaterial(string name, Color color, int renderQueue)
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
        Material material = new Material(shader);
        material.name = name;
        ApplyMaterialColor(material, color, 1.0f);
        ApplyOverlaySettings(material, renderQueue);
        return material;
    }

    private void ApplyMaterialColor(Material material, Color color, float emissionStrength)
    {
        if (material == null)
        {
            return;
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        material.color = color;
        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", new Color(color.r, color.g, color.b, 1.0f) * emissionStrength);
        }
    }

    private void ApplyOverlaySettings(Material material, int renderQueue)
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

    private Mesh OwnMesh(Mesh mesh)
    {
        ownedMeshes.Add(mesh);
        return mesh;
    }

    private Material OwnMaterial(Material material)
    {
        ownedMaterials.Add(material);
        return material;
    }

    private Mesh CreateSphereMesh(int latitudeSegments, int longitudeSegments, float radius, string name)
    {
        int latitudeCount = Mathf.Max(4, latitudeSegments);
        int longitudeCount = Mathf.Max(8, longitudeSegments);
        Mesh mesh = new Mesh();
        mesh.name = name;
        Vector3[] vertices = new Vector3[(latitudeCount + 1) * (longitudeCount + 1)];
        List<int> triangles = new List<int>();
        int vertex = 0;
        for (int latitude = 0; latitude <= latitudeCount; latitude++)
        {
            float theta = ((float)latitude / latitudeCount) * Mathf.PI;
            float y = Mathf.Cos(theta) * radius;
            float ring = Mathf.Sin(theta) * radius;
            for (int longitude = 0; longitude <= longitudeCount; longitude++)
            {
                float phi = ((float)longitude / longitudeCount) * Mathf.PI * 2.0f;
                vertices[vertex++] = new Vector3(Mathf.Cos(phi) * ring, y, Mathf.Sin(phi) * ring);
            }
        }
        for (int latitude = 0; latitude < latitudeCount; latitude++)
        {
            for (int longitude = 0; longitude < longitudeCount; longitude++)
            {
                int a = latitude * (longitudeCount + 1) + longitude;
                int b = a + longitudeCount + 1;
                int c = b + 1;
                int d = a + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateRingMesh(int segments, float innerRadius, float outerRadius, string name)
    {
        int count = Mathf.Max(12, segments);
        Mesh mesh = new Mesh();
        mesh.name = name;
        Vector3[] vertices = new Vector3[count * 2];
        List<int> triangles = new List<int>();
        for (int i = 0; i < count; i++)
        {
            float angle = ((float)i / count) * Mathf.PI * 2.0f;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            vertices[i * 2] = new Vector3(x * outerRadius, y * outerRadius, 0.0f);
            vertices[i * 2 + 1] = new Vector3(x * innerRadius, y * innerRadius, 0.0f);
        }
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int outerA = i * 2;
            int innerA = outerA + 1;
            int outerB = next * 2;
            int innerB = outerB + 1;
            triangles.Add(outerA); triangles.Add(outerB); triangles.Add(innerB);
            triangles.Add(outerA); triangles.Add(innerB); triangles.Add(innerA);
            triangles.Add(innerB); triangles.Add(outerB); triangles.Add(outerA);
            triangles.Add(innerA); triangles.Add(innerB); triangles.Add(outerA);
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateGridMesh(float range, float step)
    {
        Mesh mesh = new Mesh();
        mesh.name = "FA Radar 2 Meter Grid";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        float halfWidth = 0.006f;
        int steps = Mathf.CeilToInt(range / step);
        for (int i = -steps; i <= steps; i++)
        {
            float coordinate = i * step;
            AddGridLine(vertices, triangles, new Vector3(coordinate, 0.0f, -range), new Vector3(coordinate, 0.0f, range), halfWidth);
            AddGridLine(vertices, triangles, new Vector3(-range, 0.0f, coordinate), new Vector3(range, 0.0f, coordinate), halfWidth);
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void AddGridLine(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 side = Vector3.Cross(direction, Vector3.up).normalized * width * 0.5f;
        int index = vertices.Count;
        vertices.Add(start - side);
        vertices.Add(start + side);
        vertices.Add(end + side);
        vertices.Add(end - side);
        triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 3);
        triangles.Add(index + 2); triangles.Add(index + 1); triangles.Add(index);
        triangles.Add(index + 3); triangles.Add(index + 2); triangles.Add(index);
    }

    private Mesh CreateBoxMesh(string name)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
        };
        mesh.triangles = new int[]
        {
            0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4, 3, 6, 2, 3, 7, 6,
            1, 2, 6, 1, 6, 5, 0, 4, 7, 0, 7, 3
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateSpotlightConeMesh(int segments, string name)
    {
        int count = Mathf.Max(12, segments);
        Mesh mesh = new Mesh();
        mesh.name = name;
        Vector3[] vertices = new Vector3[count + 1];
        List<int> triangles = new List<int>();
        vertices[0] = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            float angle = ((float)i / count) * Mathf.PI * 2.0f;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 1.0f);
        }
        for (int i = 0; i < count; i++)
        {
            int current = i + 1;
            int next = ((i + 1) % count) + 1;
            triangles.Add(0); triangles.Add(current); triangles.Add(next);
            triangles.Add(0); triangles.Add(next); triangles.Add(current);
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private Transform ResolveViewerTransform()
    {
        SuperController sc = SuperController.singleton;
        if (sc != null && sc.lookCamera != null)
        {
            return sc.lookCamera.transform;
        }
        return Camera.main != null ? Camera.main.transform : null;
    }

    private void SetVisualRootVisible(bool visible)
    {
        SetActiveIfChanged(visualRoot, visible);
    }

    private void SetSlotVisible(MarkerSlot slot, bool visible)
    {
        if (slot == null)
        {
            return;
        }
        SetActiveIfChanged(slot.markerObject, visible);
        SetActiveIfChanged(slot.stemObject, false);
        SetActiveIfChanged(slot.pointRangeObject, false);
        SetActiveIfChanged(slot.spotConeObject, false);
    }

    private void SetActiveIfChanged(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    private Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void SetStatus(string value)
    {
        value = value ?? "";
        if (string.Equals(lastStatus, value, StringComparison.Ordinal))
        {
            return;
        }
        lastStatus = value;
        statusField.valNoCallback = value;
    }

    private void DestroyVisuals()
    {
        if (visualRoot != null)
        {
            UnityEngine.Object.Destroy(visualRoot);
        }
        visualRoot = null;
        contentRoot = null;
        shellObject = null;
        gridObject = null;
        centerObject = null;
        for (int i = 0; i < ownedMaterials.Count; i++)
        {
            if (ownedMaterials[i] != null)
            {
                UnityEngine.Object.Destroy(ownedMaterials[i]);
            }
        }
        for (int i = 0; i < ownedMeshes.Count; i++)
        {
            if (ownedMeshes[i] != null)
            {
                UnityEngine.Object.Destroy(ownedMeshes[i]);
            }
        }
        ownedMaterials.Clear();
        ownedMeshes.Clear();
        markerSlots.Clear();
        visibleAtoms.Clear();
        visualsReady = false;
    }
}
