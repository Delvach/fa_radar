using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FrameAngelRadar : MVRScript
{
    private const string Version = "0.1.4";
    private const int ShellRenderQueue = 4980;
    private const int GridRenderQueue = 4990;
    private const int RingRenderQueue = 5000;
    private const int MarkerRenderQueue = 5010;
    private const int ShellSortingOrder = 32730;
    private const int GridSortingOrder = 32740;
    private const int RingSortingOrder = 32750;
    private const int MarkerSortingOrder = 32760;

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
    private JSONStorableBool lastSelectedEnabledField;

    private JSONStorableFloat hudOffsetXField;
    private JSONStorableFloat hudOffsetYField;
    private JSONStorableFloat hudOffsetZField;
    private JSONStorableFloat hudScaleField;
    private JSONStorableFloat viewYawOffsetField;
    private JSONStorableFloat desktopTiltDegreesField;
    private JSONStorableFloat axisYawOffsetField;
    private JSONStorableFloat radarRangeMetersField;
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
    private JSONStorableFloat pollIntervalField;
    private JSONStorableFloat responseSmoothingField;

    private JSONStorableString statusField;

    private GameObject hudRoot;
    private GameObject radarRoot;
    private GameObject axisRoot;
    private GameObject flatCircleObject;
    private GameObject sphereObject;
    private GameObject gridObject;
    private GameObject centerMarkerObject;
    private GameObject targetBlipObject;
    private GameObject targetGridDropObject;
    private GameObject lastTargetBlipObject;
    private GameObject lastTargetGridDropObject;
    private GameObject[] ringObjects;
    private Quaternion[] ringBaseRotations;
    private MeshFilter gridFilter;

    private Mesh sphereMesh;
    private Mesh flatCircleMesh;
    private Mesh ringMesh;
    private Mesh gridMesh;
    private Mesh targetBlipMesh;
    private Mesh centerMarkerMesh;

    private Material shellMaterial;
    private Material ringMaterial;
    private Material gridMaterial;
    private Material centerMaterial;
    private Material targetMaterial;
    private Material targetDropMaterial;
    private Material lastTargetMaterial;
    private Material lastTargetDropMaterial;

    private Atom selectedAtom;
    private Atom lastSelectedAtom;
    private string selectedUid = "";
    private string lastSelectedUid = "";
    private float nextSelectionPollTime;
    private float lastSelectedAtTime = -1000.0f;
    private float lastGridRangeMeters = -1.0f;
    private float lastGridStepMeters = -1.0f;
    private Vector2 lastGridOffsetMeters;
    private bool lastGridClipCircle;
    private bool haveLastGridOffset;
    private bool visualsReady;
    private bool haveSmoothedHudPosition;
    private Vector3 smoothedHudPosition;
    private Transform currentHudAnchor;

    public override void Init()
    {
        BuildStorables();
        BuildUi();
        EnsureRuntimeVisuals();
        SetStatus("Frame Angel Radar " + Version + " prototype ready.");
    }

    private void Update()
    {
        if (!visualsReady)
        {
            EnsureRuntimeVisuals();
        }

        TickRadar();
    }

    private void OnDestroy()
    {
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
        desktopTopDownField = new JSONStorableBool("Desktop Top Down", true);
        flatDesktopCircleField = new JSONStorableBool("Flat Desktop Circle", true);
        worldAxisAlignField = new JSONStorableBool("World Axis Align", true);
        lastSelectedEnabledField = new JSONStorableBool("Last Selected Enabled", true);

        hudOffsetXField = new JSONStorableFloat("HUD Offset X", 0.32f, -1.0f, 1.0f, true, true);
        hudOffsetYField = new JSONStorableFloat("HUD Offset Y", -0.24f, -1.0f, 1.0f, true, true);
        hudOffsetZField = new JSONStorableFloat("HUD Offset Z", 0.78f, 0.15f, 1.5f, true, true);
        hudScaleField = new JSONStorableFloat("HUD Scale", 1.0f, 0.25f, 3.0f, true, true);
        viewYawOffsetField = new JSONStorableFloat("View Yaw Offset", 0.0f, -180.0f, 180.0f, true, true);
        desktopTiltDegreesField = new JSONStorableFloat("Desktop Tilt Degrees", 90.0f, 0.0f, 90.0f, true, true);
        axisYawOffsetField = new JSONStorableFloat("Axis Yaw Offset", 0.0f, -180.0f, 180.0f, true, true);
        radarRangeMetersField = new JSONStorableFloat("Radar Range Meters", 5.0f, 0.5f, 30.0f, true, true);
        radarVisualRadiusField = new JSONStorableFloat("Radar Visual Radius", 0.075f, 0.025f, 0.25f, true, true);
        gridStepMetersField = new JSONStorableFloat("Grid Step Meters", 1.0f, 0.25f, 5.0f, true, true);
        shellAlphaField = new JSONStorableFloat("Sphere Alpha", 0.09f, 0.0f, 0.45f, true, true);
        ringAlphaField = new JSONStorableFloat("Ring Alpha", 0.34f, 0.02f, 0.9f, true, true);
        gridAlphaField = new JSONStorableFloat("Grid Alpha", 0.16f, 0.0f, 0.5f, true, true);
        markerAlphaField = new JSONStorableFloat("Marker Alpha", 0.9f, 0.1f, 1.0f, true, true);
        emissionStrengthField = new JSONStorableFloat("Emission Strength", 1.4f, 0.0f, 4.0f, true, true);
        ringRotationSpeedField = new JSONStorableFloat("Ring Rotation Speed", 18.0f, 0.0f, 90.0f, true, true);
        targetMarkerScaleField = new JSONStorableFloat("Target Marker Scale", 0.085f, 0.025f, 0.25f, true, true);
        lastSelectedFadeSecondsField = new JSONStorableFloat("Last Selected Fade Seconds", 12.0f, 1.0f, 60.0f, true, true);
        pollIntervalField = new JSONStorableFloat("Selection Poll Seconds", 0.15f, 0.03f, 1.0f, true, true);
        responseSmoothingField = new JSONStorableFloat("Response Smoothing", 0.0f, 0.0f, 1.0f, true, true);

        statusField = new JSONStorableString("Status", "");

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
        RegisterBool(lastSelectedEnabledField);

        RegisterFloat(hudOffsetXField);
        RegisterFloat(hudOffsetYField);
        RegisterFloat(hudOffsetZField);
        RegisterFloat(hudScaleField);
        RegisterFloat(viewYawOffsetField);
        RegisterFloat(desktopTiltDegreesField);
        RegisterFloat(axisYawOffsetField);
        RegisterFloat(radarRangeMetersField);
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
        RegisterFloat(pollIntervalField);
        RegisterFloat(responseSmoothingField);

        RegisterString(statusField);

        RegisterAction(new JSONStorableAction("Capture HUD Offset From Atom", CaptureHudOffsetFromAttachedAtom));
        RegisterAction(new JSONStorableAction("Reset HUD Offset", ResetHudOffset));
    }

    private void BuildUi()
    {
        CreateToggle(radarEnabledField, false);
        CreateToggle(desktopTopDownField, true);
        CreateToggle(anchorToViewField, false);
        CreateToggle(lastSelectedEnabledField, true);
        CreateToggle(flatDesktopCircleField, false);
        CreateToggle(worldAxisAlignField, true);
        CreateToggle(ringsEnabledField, false);
        CreateToggle(gridEnabledField, true);
        CreateToggle(gridFollowsUserField, false);
        CreateToggle(gridClipCircleField, true);
        CreateToggle(ignoreContainingAtomField, false);
        CreateTextField(statusField, true);

        CreateSlider(radarRangeMetersField, false);
        CreateSlider(gridStepMetersField, true);
        CreateSlider(radarVisualRadiusField, false);
        CreateSlider(hudScaleField, true);

        CreateSlider(hudOffsetXField, false);
        CreateSlider(hudOffsetYField, true);
        CreateSlider(hudOffsetZField, false);
        CreateSlider(viewYawOffsetField, true);
        CreateSlider(desktopTiltDegreesField, false);
        CreateSlider(axisYawOffsetField, true);
        CreateSlider(responseSmoothingField, true);

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
        CreateSlider(lastSelectedFadeSecondsField, false);
        CreateSlider(shellAlphaField, false);
        CreateSlider(ringAlphaField, true);
        CreateSlider(gridAlphaField, false);
        CreateSlider(markerAlphaField, true);
        CreateSlider(emissionStrengthField, false);
        CreateSlider(pollIntervalField, true);
    }

    private void EnsureRuntimeVisuals()
    {
        if (visualsReady)
        {
            return;
        }

        shellMaterial = CreateEmissiveOverlayMaterial("FA Radar Sphere Material", new Color(0.20f, 0.72f, 1.0f, 0.09f), ShellRenderQueue);
        ringMaterial = CreateEmissiveOverlayMaterial("FA Radar Ring Material", new Color(0.25f, 0.90f, 1.0f, 0.34f), RingRenderQueue);
        gridMaterial = CreateEmissiveOverlayMaterial("FA Radar Grid Material", new Color(0.55f, 0.95f, 1.0f, 0.16f), GridRenderQueue);
        centerMaterial = CreateEmissiveOverlayMaterial("FA Radar Center Material", new Color(0.40f, 1.0f, 0.62f, 0.9f), MarkerRenderQueue);
        targetMaterial = CreateEmissiveOverlayMaterial("FA Radar Target Material", new Color(1.0f, 0.70f, 0.18f, 0.9f), MarkerRenderQueue);
        targetDropMaterial = CreateEmissiveOverlayMaterial("FA Radar Target Drop Material", new Color(1.0f, 0.70f, 0.18f, 0.35f), MarkerRenderQueue);
        lastTargetMaterial = CreateEmissiveOverlayMaterial("FA Radar Last Target Material", new Color(1.0f, 0.48f, 0.12f, 0.32f), MarkerRenderQueue);
        lastTargetDropMaterial = CreateEmissiveOverlayMaterial("FA Radar Last Target Drop Material", new Color(1.0f, 0.48f, 0.12f, 0.15f), MarkerRenderQueue);

        sphereMesh = CreateSphereMesh(8, 16, 1.0f);
        flatCircleMesh = CreateDesktopDiskMesh(72, 1.0f);
        ringMesh = CreateRingMesh(72, 0.975f, 1.0f);
        centerMarkerMesh = CreateCenterMarkerMesh();
        targetBlipMesh = CreateTargetBlipMesh();
        gridMesh = CreateGridMesh(radarRangeMetersField.val, gridStepMetersField.val, Vector2.zero, gridClipCircleField.val);
        lastGridRangeMeters = radarRangeMetersField.val;
        lastGridStepMeters = gridStepMetersField.val;
        lastGridOffsetMeters = Vector2.zero;
        lastGridClipCircle = gridClipCircleField.val;
        haveLastGridOffset = true;

        hudRoot = new GameObject("FA Radar HUD");
        radarRoot = new GameObject("FA Radar Dish");
        radarRoot.transform.SetParent(hudRoot.transform, false);
        axisRoot = new GameObject("FA Radar World Axis");
        axisRoot.transform.SetParent(radarRoot.transform, false);

        flatCircleObject = CreateMeshObject("FA Radar Flat Desktop Circle", axisRoot.transform, flatCircleMesh, shellMaterial, ShellRenderQueue, ShellSortingOrder);
        sphereObject = CreateMeshObject("FA Radar Sphere", radarRoot.transform, sphereMesh, shellMaterial, ShellRenderQueue, ShellSortingOrder);
        gridObject = CreateMeshObject("FA Radar Meter Grid", axisRoot.transform, gridMesh, gridMaterial, GridRenderQueue, GridSortingOrder);
        gridFilter = gridObject.GetComponent<MeshFilter>();

        centerMarkerObject = CreateMeshObject("FA Radar User Center", radarRoot.transform, centerMarkerMesh, centerMaterial, MarkerRenderQueue, MarkerSortingOrder);
        targetBlipObject = CreateMeshObject("FA Radar Target Blip", radarRoot.transform, targetBlipMesh, targetMaterial, MarkerRenderQueue, MarkerSortingOrder);
        targetGridDropObject = CreateMeshObject("FA Radar Target Grid Drop", radarRoot.transform, targetBlipMesh, targetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 1);
        lastTargetBlipObject = CreateMeshObject("FA Radar Last Target Blip", radarRoot.transform, targetBlipMesh, lastTargetMaterial, MarkerRenderQueue, MarkerSortingOrder - 2);
        lastTargetGridDropObject = CreateMeshObject("FA Radar Last Target Grid Drop", radarRoot.transform, targetBlipMesh, lastTargetDropMaterial, MarkerRenderQueue, MarkerSortingOrder - 3);

        ringObjects = new GameObject[3];
        ringBaseRotations = new Quaternion[3];
        ringBaseRotations[0] = Quaternion.identity;
        ringBaseRotations[1] = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        ringBaseRotations[2] = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        ringObjects[0] = CreateMeshObject("FA Radar Ring XY", axisRoot.transform, ringMesh, ringMaterial, RingRenderQueue, RingSortingOrder);
        ringObjects[1] = CreateMeshObject("FA Radar Ring XZ", axisRoot.transform, ringMesh, ringMaterial, RingRenderQueue, RingSortingOrder);
        ringObjects[2] = CreateMeshObject("FA Radar Ring YZ", axisRoot.transform, ringMesh, ringMaterial, RingRenderQueue, RingSortingOrder);

        SetActiveIfChanged(hudRoot, false);
        SetActiveIfChanged(targetBlipObject, false);
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

    private void TickRadar()
    {
        if (hudRoot == null)
        {
            visualsReady = false;
            return;
        }

        Transform viewer = ResolveViewerTransform();
        if (!radarEnabledField.val || viewer == null)
        {
            SetActiveIfChanged(hudRoot, false);
            if (viewer == null)
            {
                SetStatus("Waiting for VaM look camera.");
            }
            return;
        }

        SetActiveIfChanged(hudRoot, true);
        PollSelectionIfDue();
        TrackAttachedAtomPlacement(viewer);
        RefreshGridMeshIfNeeded(viewer);
        UpdateMaterials();
        UpdateRadarDish(viewer);

        Transform target = ResolveAtomRootTransform(selectedAtom);
        bool hasSelection = target != null;
        SetActiveIfChanged(targetBlipObject, hasSelection);
        SetActiveIfChanged(targetGridDropObject, hasSelection);

        if (hasSelection)
        {
            UpdateTargetBlip(viewer, target);
        }

        UpdateLastSelectedBlip(viewer);
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
        Vector3 localOffset = viewer.InverseTransformPoint(worldPosition);
        SetHudOffset(localOffset);
    }

    private void UpdateRadarDish(Transform viewer)
    {
        float visualRadius = ResolveVisualRadius();
        float scaledMarker = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val);
        bool flatDesktop = IsFlatDesktopCircleActive();
        float ringTime = flatDesktop ? 0.0f : Time.time * Mathf.Max(0.0f, ringRotationSpeedField.val);

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

        gridObject.transform.localPosition = Vector3.zero;
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
            bool showRing = ringsEnabledField.val && (!flatDesktop || i == 0);
            SetActiveIfChanged(ring, showRing);
        }
    }

    private bool IsFlatDesktopCircleActive()
    {
        return desktopTopDownField.val && flatDesktopCircleField.val;
    }

    private void UpdateAxisVisualRotation(Transform viewer)
    {
        if (axisRoot == null)
        {
            return;
        }

        float yaw = worldAxisAlignField.val ? ResolveWorldAxisYawDegrees(viewer) : 0.0f;
        axisRoot.transform.localPosition = Vector3.zero;
        axisRoot.transform.localRotation = Quaternion.AngleAxis(yaw + axisYawOffsetField.val, Vector3.up);
        axisRoot.transform.localScale = Vector3.one;
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

        if (anchorToViewField.val)
        {
            if (currentHudAnchor != viewer || hudRoot.transform.parent != viewer)
            {
                hudRoot.transform.SetParent(viewer, false);
                currentHudAnchor = viewer;
                haveSmoothedHudPosition = false;
            }

            hudRoot.transform.localPosition = GetHudOffset();
            hudRoot.transform.localRotation = Quaternion.AngleAxis(viewYawOffsetField.val, Vector3.forward);
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
        hudRoot.transform.rotation = viewer.rotation * Quaternion.AngleAxis(viewYawOffsetField.val, Vector3.forward);
        hudRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, hudScaleField.val);
    }

    private Quaternion ResolveDishLocalRotation()
    {
        if (!desktopTopDownField.val)
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(Mathf.Clamp(desktopTiltDegreesField.val, 0.0f, 90.0f), 0.0f, 0.0f);
    }

    private void UpdateTargetBlip(Transform viewer, Transform target)
    {
        float visualRadius = ResolveVisualRadius();
        Vector3 radarLocal = ResolveTargetRadarLocal(viewer, target);
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val);
        float spin = Time.time * Mathf.Max(12.0f, ringRotationSpeedField.val * 1.75f);

        PositionTargetSphere(targetBlipObject, radarLocal, visualRadius, markerScale, spin);

        targetGridDropObject.transform.localPosition = new Vector3(
            radarLocal.x * visualRadius,
            -visualRadius * 1.08f,
            radarLocal.z * visualRadius);
        targetGridDropObject.transform.localRotation = Quaternion.Euler(90.0f, spin, 0.0f);
        targetGridDropObject.transform.localScale = Vector3.one * (markerScale * 0.55f);

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
        float markerScale = visualRadius * Mathf.Max(0.01f, targetMarkerScaleField.val) * 0.82f;
        float spin = Time.time * Mathf.Max(10.0f, ringRotationSpeedField.val);

        PositionTargetSphere(lastTargetBlipObject, radarLocal, visualRadius, markerScale, -spin);

        lastTargetGridDropObject.transform.localPosition = new Vector3(
            radarLocal.x * visualRadius,
            -visualRadius * 1.08f,
            radarLocal.z * visualRadius);
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
        float range = Mathf.Max(0.25f, radarRangeMetersField.val);
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

    private void RefreshGridMeshIfNeeded(Transform viewer)
    {
        if (gridFilter == null)
        {
            return;
        }

        float range = Mathf.Max(0.5f, radarRangeMetersField.val);
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
        if (SuperController.singleton != null && SuperController.singleton.lookCamera != null)
        {
            return SuperController.singleton.lookCamera.transform;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return null;
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
        ApplyMaterialColor(shellMaterial, new Color(0.18f, 0.70f, 1.0f, Mathf.Clamp01(shellAlphaField.val)), emission);
        ApplyMaterialColor(ringMaterial, new Color(0.18f, 0.92f, 1.0f, Mathf.Clamp01(ringAlphaField.val)), emission);
        ApplyMaterialColor(gridMaterial, new Color(0.48f, 0.95f, 1.0f, Mathf.Clamp01(gridAlphaField.val)), emission);
        ApplyMaterialColor(centerMaterial, new Color(0.38f, 1.0f, 0.60f, Mathf.Clamp01(markerAlphaField.val)), emission);
        ApplyMaterialColor(targetMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val)), emission);
        ApplyMaterialColor(targetDropMaterial, new Color(1.0f, 0.68f, 0.16f, Mathf.Clamp01(markerAlphaField.val) * 0.42f), emission);
        ApplyMaterialColor(lastTargetMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.26f), emission);
        ApplyMaterialColor(lastTargetDropMaterial, new Color(1.0f, 0.48f, 0.12f, Mathf.Clamp01(markerAlphaField.val) * 0.12f), emission);
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

    private void ApplyMaterialColor(Material material, Color color, float emissionStrength)
    {
        if (material == null)
        {
            return;
        }

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
        float gridY = -1.08f;

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
        Mesh mesh = CreateSphereMesh(6, 12, 1.0f);
        mesh.name = "FA Radar Prototype Target Sphere Mesh";
        return mesh;
    }

    private Mesh CreateCenterMarkerMesh()
    {
        Mesh mesh = CreateSphereMesh(6, 12, 1.0f);
        mesh.name = "FA Radar Prototype User Center Sphere Mesh";
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
        SetHudOffset(offset);
        haveSmoothedHudPosition = false;
        SetStatus("Captured HUD offset from attached atom.");
    }

    private void ResetHudOffset()
    {
        SetHudOffset(new Vector3(0.32f, -0.24f, 0.78f));
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
        hudRoot = null;
        radarRoot = null;
        axisRoot = null;
        flatCircleObject = null;
        sphereObject = null;
        gridObject = null;
        centerMarkerObject = null;
        targetBlipObject = null;
        targetGridDropObject = null;
        lastTargetBlipObject = null;
        lastTargetGridDropObject = null;
        ringObjects = null;
        ringBaseRotations = null;
        gridFilter = null;
        currentHudAnchor = null;

        DestroyOwnedObject(shellMaterial);
        DestroyOwnedObject(ringMaterial);
        DestroyOwnedObject(gridMaterial);
        DestroyOwnedObject(centerMaterial);
        DestroyOwnedObject(targetMaterial);
        DestroyOwnedObject(targetDropMaterial);
        DestroyOwnedObject(lastTargetMaterial);
        DestroyOwnedObject(lastTargetDropMaterial);
        shellMaterial = null;
        ringMaterial = null;
        gridMaterial = null;
        centerMaterial = null;
        targetMaterial = null;
        targetDropMaterial = null;
        lastTargetMaterial = null;
        lastTargetDropMaterial = null;

        DestroyOwnedObject(sphereMesh);
        DestroyOwnedObject(flatCircleMesh);
        DestroyOwnedObject(ringMesh);
        DestroyOwnedObject(gridMesh);
        DestroyOwnedObject(targetBlipMesh);
        DestroyOwnedObject(centerMarkerMesh);
        sphereMesh = null;
        flatCircleMesh = null;
        ringMesh = null;
        gridMesh = null;
        targetBlipMesh = null;
        centerMarkerMesh = null;

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
