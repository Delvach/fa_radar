# FA Radar Architecture V1

Updated: 2026-08-25

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.53` on branch
  `codex/0.1.53-world-wrist`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as compiled VaM plugin DLLs:
  `Custom/Plugins/fa_radar.free.0.1.53.dll` and
  `Custom/Plugins/fa_radar.pro.0.1.53.dll`.
- Pro ships thin Empty and CustomUnityAsset host presets:
  `Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap` and
  `Custom/Atom/CustomUnityAsset/Preset_FrameAngel_Radar_CUA.vap`.
- Supported plugin surfaces are scene/session, Empty, and CustomUnityAsset.
  Empty/CUA-hosted Radar can follow its atom, select the same wrist/palm modes,
  or select `World`; all hosts reuse one VaM stock full-grab center target.
- No Unity project, asset bundles, raw runtime file IO, reflection, broad JSON
  object serializers, repo-local runtime dependencies, or absolute dev paths.
  Global user prefs are the only runtime file access and use VaM
  `FileManagerSecure` under `Custom\PluginData\FrameAngel\Radar`.

## Prototype Visual

The `0.1.53` branch preserves the complete 0.1.52 generated visual treatment,
keeps the normal plugin UI trimmed to daily controls, and retains all legacy
placement storables without drawing their offset/rotation/desktop/VR control
forest. One visible `World` mode selects the existing static-world anchor. It
also replaces 0.1.52's one-shot hand-root registration with bounded
re-acquisition and idempotent unregister while disconnected. It
also keeps the 0.1.39 performance budget pass and fixes player
navigation/crosshair utility markers so they resolve to the active viewer
height instead of VaM's floor-anchored utility atom root. The 0.1.41 polish
adds a smaller HUD scale floor, hides the redundant HUD-mode self marker,
keeps far markers visible by fading/projecting them outside the shell, and
switches large-area grids from 1m cells to 10m cells. It
also narrows the HUD scale slider to the daily-use span so small HUD sizing is
not compressed by the full placement cap. The 0.1.43 marker pass adds a
generated Pro-only polygon person mesh while keeping person gender color as
pink for female metadata, blue for male metadata, and neutral when unknown. It
also adds selected-target 3-ring/cue rendering, turns useful marker/filter
overlays on by default except the Player Navigation Panel utility marker,
extends the default outside-range fade, and renames the overlay cap to
`Detail Overlay Limit`. The 0.1.45 depth-clarity pass then lowers the default
visual weight of context shells, stems, light volumes, axes, and camera
frustums; applies depth-weighted alpha/scale to available markers and Pro
details; and migrates saved prefs once with `visualDepthDefaultsVersion`. It
also keeps optional wrist compass projection modes that can reveal on an
outward twist or stay always-on per hand. The Pro/default-off throw-pin mechanic
from 0.1.36 remains available for direct grabs: a velocity release can launch
Radar, decelerate it, optionally stop it on a cheap surface raycast, pin it as a
larger world-static radar, and let the next grab shrink it back into the normal
placement rules. Free remains the simplified radar surface: desktop placement,
VR placement, scale, HUD offsets, and static desktop offsets; all item markers
are plain yellow dots.
The 0.1.47 director-readability pass then adds a Pro-only
`directorReadabilityDefaultsVersion` migration, lowers camera/light/axis detail
defaults, caps background rich overlays at 10, and applies stronger alpha/scale
attenuation to non-selected overlays so selected targets remain readable in
dense movie-studio scenes.
The 0.1.48 label/axis pass keeps that budget posture while replacing full
rotation-axis bars with a generated four-renderer/seven-piece glyph and adding
capped procedural scene labels. Labels default to facing the active viewer, can
instead use world-axis or object rotation, and selected labels stay outside the
available-label budget.
The 0.1.49 label-callout/UI pass then changes the default label posture to
selected-only, reduces glyph scale, moves labels to outside-shell callout
anchors with pooled leader lines, and reorders the native Pro plugin UI so
primary atom/category checkboxes and display toggles appear before advanced
tuning sliders.
The 0.1.52 pass preserves the corrected generated label mesh-facing basis, reduces
height-stem X/Z half-width from `0.018` to `0.010`, adds the CUA preset, and
adds default-off `Room Compass` to both Empty- and CUA-hosted instances.

The previous behavior change was VR
readability tuning: point-light range spheres and spotlight cones get separate
alpha sliders plus a light volume scale slider, the old `Light Volume Alpha`
pref remains hidden compatibility, and the first non-sphere markers are routed
to panel/slate/screen and SubScene-style atoms. It uses generated emissive
polygons so targeting can be tested before any art polish:

- unified desktop/VR treatment using the same sphere shell, meter grid, and
  target markers
- a subtle transparent unlit overlay sphere material, with low emission and
  increased mesh subdivisions so the shell reads round without becoming
  attention-grabbing
- three cached annulus meshes tied to the world-axis visual root
- VaM/world axis ring colors: X is red, Y is green, and Z is blue
- faded meter grid centered through the sphere on the actual world X/Z ground
  plane
- `Radar Range Meters` expands or contracts the represented meter range for the
  grid, marker mapping, and height mapping without changing the compass visual
  radius
- on desktop, mouse wheel over the visible radar adjusts `Radar Range Meters`
  directly; wheel-up zooms into fewer represented meters and wheel-down expands
  range
- `HUD Scale` and `Wrist Scale` change overall rendered size only; at the
  default visual radius their max maps to a 1m radar diameter
- grid lines clipped to the radar circle
- one-meter grid panning from the active radar reference position without
  relying on a visible toggle or stale scene/global pref
- green user marker; in HUD/wrist modes it stays centered, while in static
  scene/Empty modes it moves through the radar display
- orange selected-atom marker without an extra outer outline, so marker size
  remains a proximity/depth cue
- first-pass marker-shape routing: panel, slate, screen, FAP/FAPP, and similar
  flat-surface atoms render as thin rectangles; SubScene atoms render as a
  broader flat rectangle; point-like and unknown atoms remain spheres
- optional selected world-ground grid drop marker, disabled by default so the
  current selection does not read as a duplicate highlight
- height stems from the scaled ground plane to user, selected, and visible
  available atom marker heights
- range-edge fade so markers just outside the radar range fade instead of
  popping on the boundary
- depth size cues so closer markers render larger and far markers render
  smaller
- edition-gated available atom markers: Free shows every eligible atom
  together with a neutral marker color; Pro exposes Light, Person, CUA, Empty,
  SubScene, ImagePanel, Animation, Force, Shapes, Sounds, Triggers, and other
  atom filters with category colors
- Pro-only rotation-axis glyphs draw short red, green, and blue bars through
  selected/available markers using each item's real scene rotation relative to
  the radar's world-axis display
- Pro-only light visuals draw translucent range spheres for point lights and
  translucent spot cones for spotlights, while directional lights stay as dots;
  alpha and display scale are adjustable without changing the represented meter
  range
- Pro-only user, desktop, and scene-camera POV/frustum helpers are generated as
  translucent radar-local projections for filming setup
- click-to-select for visible available CUA/light/person/other atom markers
- session-plugin-only movement accepts the existing controller-grip proximity
  path or an optical pinch/HoldGrab lease of an invisible Radar-owned VaM
  `FreeControllerV3`; VaM's side-specific full-grab ownership is only observed,
  and Radar never synthesizes a hand/controller action
- two-hand outward-twist accordion scaling for HUD and wrist modes, using the
  current hand distance as the scale ratio
- generated HUD objects and materials carry the `favr.hud.radar` filming
  identifier so FAAR/recorder tooling can locate the radar in final-recording
  workflows without adding a recorder dependency or scene-stored control data

The selected atom is resolved with
`SuperController.singleton.GetSelectedAtom()` on a configurable poll interval.
The target position is converted through the active radar reference frame, then
divided by `Radar Range Meters` and clamped to the unit sphere. HUD and wrist
modes use the viewer as the reference origin. Static world and Empty/atom-anchor
modes use the radar's own world pose as the reference origin, so moving the user
does not slide scene items around the static radar.

Atom-attached `Room Compass` is the exception. It leaves the containing Empty or
CUA atom at its VaM pose, detaches only the generated Radar visual root, and places that root
at scene origin with identity world rotation. Horizontal and vertical mapping
both use `HUD Scale * Radar Visual Radius`, cancelling the visual-root scale so
world positions land at 1:1. Shell-edge clamping, range fading, and depth
attenuation are bypassed in this mode; spotlight geometry is not clipped to the
Radar shell. The sphere, rings, and meter grid use the configured Radar range as
their world-space radius, independent of `HUD Scale`; marker and label scale
remain tunable. Labels use small item-local callouts instead of returning to the
shell. Rotation continues through the existing world/object rotation path.

By default the selected marker uses all three world-axis display dimensions.
`Flatten Target Y` remains available as a test toggle when a stricter X/Z
desktop read is more useful than height/depth representation.

`0.1.37` keeps Pro spotlight cones open-ended and clipped to the radar shell so
wide spotlights cannot render as a huge filled far-cap disc over the scene. It
also reports a marker diagnostic in `Status` when available atom markers are
enabled but no marker is visible, distinguishing zero tracked atoms after
filters from tracked atoms hidden outside range or missing usable targets.

`World Axis Align` controls the grid/ring visual root. With the default `Ground
Axis Lock` enabled, the axis root is counter-rotated from the HUD/dish so its
world rotation matches real world XYZ. This keeps the meter grid on the actual
ground X/Z plane for desktop and VR and prevents camera roll from rolling the
radar Z axis. With `Ground Axis Lock` disabled, the older yaw-only axis behavior
is available for VR comparison. Markers remain reference-frame-relative;
grid-drop markers are projected onto the ground-axis root from target world X/Z
delta against the active radar reference position.

Previous-selection rendering is parked in `0.1.37`. `Selected Ground Drop`
controls only the current atom's optional ground projection dot.

## Session Grab Handles

`Grab Handles Enabled` is default-on for the session/scene plugin path only.
The creator-anchor preference profile and atom-host path skip this system so
Empty/CUA creator-anchor behavior remains separate.

For optical hands, Radar keeps one active but visual-free Empty atom controller
at its current center. The existing hand plugin can discover that normal VaM
`FreeControllerV3` within its 0.1 m pinch radius and acquire it through VaM's
stock full-grab operation. Radar only compares that target with
`LeftFullGrabbedController` / `RightFullGrabbedController` and follows its
position until VaM releases it. No reflection, private arm object, or optical
input emulation is used.

The prior controller path remains a graceful fallback: a controller grip press
inside `Grab Hit Radius Meters` records that controller position and moves the
same Radar world center. No visible VaM handle or resize handle is drawn.
`Grab Haptics` applies only to the controller fallback; optical grabs and wrist
reveal do not synthesize controller haptics. HUD, static, and wrist-relative
preferences are committed on release. When both available hands perform the
twist pose, their current distance still drives accordion scaling.

## Wrist Compass

`Radar Mode` is shared by scene/session, Empty, and CUA hosts. Its values are
`HUD`, `World`, `wrist-left`, `wrist-right`, `wrist-left-always-on`,
`wrist-right-always-on`, `palm-left`, and `palm-right`. `World` captures the
current Radar world pose when selected and then uses the existing static-world
anchor. Wrist and palm modes are no longer suppressed on creator hosts.

In `HUD`, the existing HUD/static/atom anchor behavior is unchanged. In wrist
modes, Radar first reads the current active VaM motion-controller transform,
then an active VaM hand/alternate-hand transform. If neither public VaM source
exists, the wrist mode fails closed. Radar does not reference `Valve.VR` or
`SteamVR.dll`; a hand-specific exact wrist remains an integration seam for the
hand plugin to publish through an ordinary VaM-safe transform.

The HUD root position uses that selected hand/controller transform plus the
wrist-relative `Wrist Offset X/Y/Z` and `Wrist Scale` preferences. Wrist mode
does not inherit the live wrist rotation for display; rotation stays view-facing
while the wrist transform supplies only position and reveal orientation.

`wrist-left` and `wrist-right` start hidden and reveal from the selected public
VaM transform's outward-roll calculation, with the existing hysteresis band.
The two `always-on` wrist modes stay visible whenever the selected transform
exists. Show/hide is a short alpha fade, and hand-off placement uses a short
reveal grace.

While in wrist mode, the opposing controller can grip the visible radar and
adjust the current wrist-relative offset. If the drag carries the radar closer
to the opposing hand past the hand-off threshold, `Radar Mode` switches to that
hand, the pre-grab wrist offset is restored, preferences are marked dirty, and
the grab event ends.

## Global Preferences

Radar placement, scale, visual tuning, and available-marker behavior are global
user preferences, not scene authority. Preference controls are registered for
the native VaM plugin UI but marked `isStorable=false` and `isRestorable=false`
so scene saves do not become the source of truth.

The runtime writes only flat scalar JSON through `MVR.FileManagementSecure`:

- `Custom\PluginData\FrameAngel\Radar\preferences_common.json` for controls
  available in both Free and Pro
- `Custom\PluginData\FrameAngel\Radar\preferences_pro.json` for Pro-only
  visibility filters
- `Custom\PluginData\FrameAngel\Radar\preferences_cua_common.json` for
  creator-anchor placement and visual controls. The filename is kept for
  compatibility with older CUA builds.
- `Custom\PluginData\FrameAngel\Radar\preferences_cua_pro.json` for
  creator-anchor Pro filter controls. The filename is kept for compatibility
  with older CUA builds.

Writes are debounced and automatic. `Global Prefs Auto Save` and
`Save Global Prefs` remain registered for compatibility, but neither is exposed
as a visible daily control; old `globalPrefsAutoSave=false` values are ignored
and the runtime stores the value as true. `Load Global Prefs` remains registered
as an action for compatibility. Loaded values are applied with `valNoCallback`,
and a small shared in-process cache keeps multiple Radar instances from
repeatedly reading the same files.
An atom-attached Radar instance, including the shipped Empty and CUA presets, selects
the creator-anchor preference profile by default. The legacy `CUA Anchor
Preset` storable remains registered so older CUA presets still load, but it is
not exposed in the reduced Empty-host UI.

Available atom markers poll `SuperController.singleton.GetAtoms()` on `Atom
Poll Seconds`, sort nearby atoms first, and use pooled generated marker/stem
objects. Free builds show every eligible atom that passes the baseline hidden,
off, selected, and containing-atom checks. Pro builds add the type/category/uid
bucket filters so normal operation can stay focused by leaving only useful
lanes enabled. Person markers use pink for female metadata, blue for male
metadata, and a neutral person color when no gender clue is available.

`Click Select Markers` uses cheap screen-space picking only on mouse-down. The
plugin projects visible marker objects through `lookCamera.WorldToScreenPoint`,
chooses the nearest marker inside `Marker Click Radius Pixels`, and selects that
atom through `SuperController.singleton.SelectController(atom.mainController,
false, false, false, true)`. This API is proved by the static verifier through
VaM metadata inspection: `Atom.mainController` is `FreeControllerV3`, and
`SuperController.SelectController(FreeControllerV3, alignView,
alignRotationOnly, alignUpDown, openUI)` exists in the target VaM assembly.

The grid uses the active radar reference position to offset the one-meter mesh
before it is clipped into the radar circle. In HUD/wrist modes the reference is
the viewer, so movement slides the grid under the centered user marker. In
static world and Empty/atom-anchor modes the reference is the radar itself, so
the grid and scene atoms stay stable while the user marker moves. `Grid Follows
User`, `Grid Step Meters`, and `Floor Area Scale` remain registered legacy
prefs, but 0.1.37 keeps panning always-on, keeps the visible grid at one meter,
and makes `Radar Range Meters` the authority for how much world the sphere
represents.

## HUD Placement

The HUD root follows `SuperController.singleton.lookCamera.transform`, with the
last good viewer transform retained before falling back to `Camera.main`. In
`Anchor To View` mode the HUD root is parented to the viewer transform and uses
local offset/rotation instead of chasing a smoothed world-space position. This
is the default for desktop testing because it avoids navigation jitter, keeps
rotation locked to the current view, and prevents add/remove atom churn from
reanchoring to a transient camera.

`Desktop Placement` and `VR Placement` remain hidden compatibility storables.
The runtime still understands their prior values, but `Radar Mode: World` is
the single visible world-lock choice on every supported host:

- `Attached To UI` maps to the existing HUD/view anchor.
- `Pinned In World` maps to world static anchoring for session/scene plugins.
- Empty/CUA-hosted instances ignore the scene/session placement chooser and
  always use containing-atom anchoring, with separate creator-anchor prefs.
- The legacy `CUA Anchor Preset` flag is compatibility-only and does not select
  the Empty/atom-anchor UI for scene/session plugins.

`Anchor Mode` remains the lower-level shared runtime adapter for HUD and
creator-facing scene
placement. It does not fork the radar core:

- `HUD / View` keeps the existing camera-relative behavior.
- `World Static` freezes the generated radar root at a captured world pose.
- `Containing Atom` parents the radar root under the atom/Empty/CUA host the plugin is
  loaded on, so creators can move or parent that host with normal VaM tools.
- `Anchor Atom UID` parents the radar root under any explicit atom UID,
  including a camera/CUA/control atom if the scene provides one.

The same `HUD Offset` and `HUD Scale` controls are reused as local anchor
offset/scale for atom-backed modes. `Anchor Rot X/Y/Z` gives a local rotation
for atom-backed modes, and `Static World X/Y/Z/Pitch/Yaw/Roll` stores the
static scene pose. The Pro Empty resource path should therefore stay thin:
creator-facing atom resources can host or identify an anchor, while Free/Pro
feature behavior remains in one plugin codebase.

The Pro Empty and CUA presets load the same plugin, set the legacy `CUA Anchor
Preset` compatibility flag, and use `Containing Atom` anchoring.
Any atom-attached Radar instance is treated as a creator anchor by default, so
the Empty preset appears on and anchored without extra binding steps. When
active, it uses the separate creator-anchor preference files listed above.

Creator-anchor UIs expose Radar mode, HUD/wrist scale, grab/haptic toggles,
represented range, filters, display/tuning, grid, status, and the default-off
`Room Compass` toggle. They omit manual offset, anchor-rotation, desktop/VR,
static-coordinate, and throw-placement controls. Existing hidden storables keep
older scenes and preferences compatible.

The normal plugin UI exposes the daily placement and operation controls:

- Pro primary atom/category checkboxes
- Pro display toggles and label mode/orientation
- `Radar Mode`
- `HUD Scale`
- `Wrist Scale`
- `Grab Handles Enabled`
- `Grab Haptics`
- `Radar Range Meters`
- `Scene Atom Markers`
- `Lights`
- `People`
- `Custom Unity Assets`
- `Empty`
- `SubScene`
- `ImagePanel`
- `Animation`
- `Force`
- `Shapes`
- `Sounds`
- `Triggers`
- `Cameras`
- `Uncategorized Atoms`
- `Grid Enabled`
- `Grab Handles Enabled`
- `Grab Haptics`

The older calibration and anchor controls remain registered so existing prefs
and older CUA presets load, but they are not part of the normal plugin UI. When the
plugin is loaded on a movable atom, the hidden `Placement Mode` and capture
actions still exist for compatibility; scene/session use works through the
offset sliders and reset button.

## Performance Posture

Detailed 0.1.38 rewrite notes and the concrete improvement list live in
`docs/FA_RADAR_PERFORMANCE_REWRITE_0.1.38.md`.

- Meshes and materials are created once and destroyed with the plugin.
- Selection is polled on `Selection Poll Seconds`; there is no per-frame atom
  scan.
- Available atom discovery is polled on `Atom Poll Seconds` and builds cached
  `AtomRecord` entries with root transform, category flags, marker mesh,
  scale-safe visual-center offset, optional Pro light handle, and cached
  distance. Cheap category/visibility filtering happens before renderer bounds
  or Unity light hierarchy scans.
- `RadarFrame` captures the active reference position/rotation/range/height
  scale/visual radius once per tick and produces a quantized signature. The
  available marker loop skips when the frame signature and atom transforms have
  not changed.
- Marker pools grow in 8-slot blocks through `MarkerSlot` records, avoiding
  resize churn as atoms or filters change.
- Available atom records are kept nearest-first with bounded insertion and
  capped by `Max Visible Markers`, avoiding full-list sort and unlimited
  "everything visible" growth.
- Pro available-marker axes, light range spheres, and spotlight cones use a
  separate lazy overlay pool capped by `Detail Overlay Limit`; selected targets
  retain full Pro detail outside that budget.
- Pro labels are generated glyph meshes, capped by `Label Limit`, and only
  rebuild per slot when the cached atom label changes. Viewer-facing labels get
  a small capped orientation refresh even when marker transforms skip.
- Available markers and context-only Pro overlays use depth-weighted alpha/scale
  so dense scenes read as layers while selected-target markers keep their
  stronger selected fade floor and camera-edge cue.
- Renderer bounds and Unity light hierarchy scans happen during atom polling,
  not in the available marker render loop or sort comparer.
- Available-atom sorting uses cached squared distances.
- Material writes go through `ApplyMaterialColorIfChanged`, so unchanged color
  and emission values do not reapply shader properties every frame.
- Marker status text is throttled to avoid per-tick formatting.
- `commonMarkerDefaultsVersion` migrates older saved prefs back to showing
  target markers by default, preventing stale prototype prefs from making a
  scene look empty until a toggle is clicked.
- Headless Unity prefab generation remains intentionally unused: this repo's
  runtime contract is self-contained C# under `payload/Custom/Scripts`, with no
  Unity project, assetbundle, or repo-local runtime asset dependency.
- Per-frame work is selected target transform, small vector math, world-axis
  yaw resolution, optional ring rotation, lightweight grid mesh refresh when
  viewer grid offset changes, and active-state diffs. In anchored mode the HUD
  position/rotation are local to the camera instead of smoothed through world
  space.
- Grid mesh rebuilds only when effective radar range (`Radar Range Meters`),
  clip mode, or the quantized radar-reference grid offset changes.
- Click selection is idle unless the desktop left mouse button goes down.
  Marker hit-testing only checks the visible pooled marker list.
- Materials use the FA Keyboard-inspired overlay pattern:
  `Hidden/Internal-Colored`, alpha blend, `ZWrite=0`, and
  `CompareFunction.Always`.
- The generated object and material names include `favr.hud.radar` as the
  current filming identifier. New generated radar/studio visuals should derive
  stable reusable names from this identifier.
- FAAR recorder visibility is read from
  `Custom\PluginData\FrameAngelMediaCore\recorder_v2_state.json`. If
  `radarHudFilmSubjectIdentifier` matches `favr.hud.radar` and
  `radarHudVisible` is false, Radar hides its generated visual root/materials.
  This does not modify placement, anchor mode, offsets, scale, or scene data.
  FAAR consumes identifiers and visibility state only; Radar retains placement
  authority.

## Build And Deploy Contract

`scripts\Build-FaRadar.ps1` compiles both editions by default:

- Free: `FA_RADAR_FREE` -> `fa_radar.free.0.1.53.dll`
- Pro: `FA_RADAR_PRO` -> `fa_radar.pro.0.1.53.dll`

The build helper runs `scripts\Obfuscate-FaRadarPlugin.ps1` unless
`-SkipObfuscation` is passed. The wrapper follows the FAP model: pinned
`Obfuscar.GlobalTool`, config-driven profiles, `FrameAngelRadar` keep rules,
VaM lifecycle callback skip rules, and a `.obf-report.json` next to each output
DLL.

The build helper also stages candidate `.var` packages under `build\packages`
with DLLs under `Custom/Plugins` and a root `meta.json`. The first Free test
package is `FrameAngelDev.Radar.1.var`. The Pro package additionally stages the
Empty and CustomUnityAsset Radar presets.

`scripts\Deploy-FaRadar.ps1` calls the build helper, then copies edition DLLs
to direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.53.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.53.dll`
- `F:\sim\vam\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `F:\sim\vam\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.53.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.53.dll`
- `C:\vam\virgin-recordable-02\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `C:\vam\virgin-recordable-02\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`

Future public `.var` product naming is undecided. Current candidates are
`FrameAngel.DaFuqIzzit.1.var` and `FrameAngel.Radar.1.var`; the first Free
test package is `FrameAngelDev.Radar.1.var`.

## Product Editions

Free and Pro are one codebase with compile/package gates for the different
editions, not separate runtime forks.

- Free is the movable, scalable, visually tunable radar, but it shows all
  supported radar atoms together.
- Free exposes desktop/VR placement, scale, HUD offsets, static desktop offsets,
  and all atoms as plain yellow dots.
- Pro adds visibility switches in the native VaM plugin UI, customizable
  category colors, per-item rotation axes, richer light visuals such as range
  spheres and spotlight cones, and filming POV/frustum helpers.

The first release is plugin-UI-only. Do not build or assume a browser,
companion, custom external, or separate in-world control surface for v1. The
first version should focus on core radar behavior: placement, scale, all-atom
visibility in Free, Pro category filters/colors, marker clarity, light-finding
basics, and deploy/package reliability.

The current product split authority is
`docs/FA_RADAR_PRODUCT_EDITIONS_V1.md`.

## Planned Interaction Extensions

Overlap clustering and God Mode are planned but not implemented in 0.1.37.
The implementation should preserve the current marker identity path: visible
atoms are still collected into `trackedAvailableAtoms`, projected into radar
local space, and represented by pooled marker objects.

For overlap clustering, the future pass should happen after radar-local
projection and depth sizing. If two or more visible atoms would be
indistinguishable at the current radar scale and marker size, they should share
one pooled cube marker with a small count label. The cluster still needs to
retain the member atoms for click/select and future manipulation.

For God Mode, the radar should become a larger static scene tool. It should not
chase the HUD or wrist while active. Grabbing, moving, or rotating a marker
inside the radar should update the represented real atom transform through the
same relative world/radar mapping used for display. This is a Pro-scale
interaction layer and should be added only after the marker grouping and
selection contracts are explicit.

## Parked

- Visual acceptance in VaM.
- Richer final art treatment after prototype targeting is proven.
- External/browser/companion UI and custom control surfaces for later versions.
- Unity-authored HUD assets; Unity remains out of scope until explicitly
  opened for this repo.
