# FA Radar Architecture V1

Updated: 2026-06-09

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.35` on branch
  `codex/0.1.35-free-edition-simplification`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as compiled VaM plugin DLLs:
  `Custom/Plugins/fa_radar.free.0.1.35.dll` and
  `Custom/Plugins/fa_radar.pro.0.1.35.dll`.
- Pro also ships a thin Empty atom preset:
  `Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap`.
- Intended plugin surface: scene or session plugin. Atom plugin loading still
  works for placement capture, but the operator target is scene/session.
- No Unity project, asset bundles, raw runtime file IO, reflection, broad JSON
  object serializers, repo-local runtime dependencies, or absolute dev paths.
  Global user prefs are the only runtime file access and use VaM
  `FileManagerSecure` under `Custom\PluginData\FrameAngel\Radar`.

## Prototype Visual

The `0.1.35` branch preserves the generated visual treatment, keeps the normal
plugin UI trimmed to daily controls, forces automatic preference writes, and
keeps separate scene/session `Desktop Placement` and `VR Placement` prefs. It
also keeps optional wrist compass projection modes that can reveal on an
outward twist or stay always-on per hand. The 0.1.35 behavior change is Free
edition simplification: Free exposes only desktop placement, VR placement,
scale, HUD offsets, and static desktop offsets; all item markers are plain
yellow dots. Pro remains the richer studio-map surface.

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
- session-plugin-only direct grip movement with OVR haptics: grip near the
  radar, move the controller, and release to apply placement; the active
  controller owns world-space movement until release or wrist hand-off
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

By default the selected marker uses all three world-axis display dimensions.
`Flatten Target Y` remains available as a test toggle when a stricter X/Z
desktop read is more useful than height/depth representation.

`World Axis Align` controls the grid/ring visual root. With the default `Ground
Axis Lock` enabled, the axis root is counter-rotated from the HUD/dish so its
world rotation matches real world XYZ. This keeps the meter grid on the actual
ground X/Z plane for desktop and VR and prevents camera roll from rolling the
radar Z axis. With `Ground Axis Lock` disabled, the older yaw-only axis behavior
is available for VR comparison. Markers remain reference-frame-relative;
grid-drop markers are projected onto the ground-axis root from target world X/Z
delta against the active radar reference position.

Previous-selection rendering is parked in `0.1.35`. `Selected Ground Drop`
controls only the current atom's optional ground projection dot.

## Session Grab Handles

`Grab Handles Enabled` is default-on for the session/scene plugin path only.
The creator-anchor preference profile and atom-host path skip this system so
Empty/CUA creator-anchor behavior remains separate.

The active 0.1.35 session path is direct grip only: when a controller grip press
starts inside `Grab Hit Radius Meters` from the radar center, the plugin records
that controller position plus the radar world center. During the grab, the
controller owns the radar's world-space center; HUD, static, atom-anchor, and
wrist-relative preferences are updated only when the grip is released. In wrist
mode, dragging past the hand-off threshold switches to the opposing hand and
restores the pre-grab wrist offset instead of continuing to solve against both
hands.

No visible VaM grab atom is drawn, no resize handle is spawned, and no dynamic
handle displacement is consumed. `Grab Haptics` uses guarded OVR haptic pulses
on move start, hand-off, and apply/release. When both hands perform the outward
twist pose, their current distance starts an accordion scale gesture; changing
that distance scales the active HUD or wrist mode until either hand leaves the
pose.

## Wrist Compass

`Radar Mode` is a session/scene-only projection selector with these values:
`HUD`, `wrist-left`, `wrist-right`, `wrist-left-always-on`, and
`wrist-right-always-on`. It does not alter the Empty/atom-anchor preset path,
and wrist modes are ignored while the creator-anchor preference profile is
active.

In `HUD`, the existing HUD/static/atom anchor behavior is unchanged. In wrist
modes, the HUD root position is anchored to the selected hand/controller
transform plus the wrist-relative `Wrist Offset X/Y/Z` and `Wrist Scale`
preferences. Wrist mode does not inherit the live wrist rotation for display;
rotation stays view-facing while the wrist transform only supplies the position
anchor and twist reveal signal.

`wrist-left` and `wrist-right` start hidden and reveal only when the selected
hand's up vector rolls outward far enough to pass `Wrist Twist Degrees`; the
reveal uses a small hysteresis band and pulses haptics once when it begins. The
two `always-on` wrist modes stay visible whenever the selected hand/controller
transform exists. Show/hide is a short alpha fade, and hand-off placement uses a
short reveal grace so the radar can pop to the destination wrist before fading
again if that wrist is not in the reveal pose.

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
An atom-attached Radar instance, including the shipped Empty preset, selects
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
prefs, but 0.1.35 keeps panning always-on, keeps the visible grid at one meter,
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

`Desktop Placement` and `VR Placement` are the daily selectors for scene/session
plugins. The runtime chooses between them from VaM's active desktop/VR display
state, so a desktop pinned-world session preference does not carry into VR HUD
placement:

- `Attached To UI` maps to the existing HUD/view anchor.
- `Pinned In World` maps to world static anchoring for session/scene plugins.
- Empty/atom-hosted instances ignore the scene/session placement chooser and
  always use containing-atom anchoring, with separate creator-anchor prefs.
- The legacy `CUA Anchor Preset` flag is compatibility-only and does not select
  the Empty/atom-anchor UI for scene/session plugins.

`Anchor Mode` remains the lower-level shared runtime adapter for HUD and
creator-facing scene
placement. It does not fork the radar core:

- `HUD / View` keeps the existing camera-relative behavior.
- `World Static` freezes the generated radar root at a captured world pose.
- `Containing Atom` parents the radar root under the atom/Empty host the plugin is
  loaded on, so creators can move or parent that host with normal VaM tools.
- `Anchor Atom UID` parents the radar root under any explicit atom UID,
  including a camera/CUA/control atom if the scene provides one.

The same `HUD Offset` and `HUD Scale` controls are reused as local anchor
offset/scale for atom-backed modes. `Anchor Rot X/Y/Z` gives a local rotation
for atom-backed modes, and `Static World X/Y/Z/Pitch/Yaw/Roll` stores the
static scene pose. The Pro Empty resource path should therefore stay thin:
creator-facing atom resources can host or identify an anchor, while Free/Pro
feature behavior remains in one plugin codebase.

The Pro Empty preset loads the same plugin on an Empty atom, sets the legacy
`CUA Anchor Preset` compatibility flag, and uses `Containing Atom` anchoring.
Any atom-attached Radar instance is treated as a creator anchor by default, so
the Empty preset appears on and anchored without extra binding steps. When
active, it uses the separate creator-anchor preference files listed above.

The creator-anchor UI intentionally omits session/HUD-only controls: no wrist
mode selector, wrist offset/scale sliders, grab/haptic controls, global reset,
VR placement selector, manual anchor-mode controls, or static-world capture
controls. The kept controls
are atom-local X/Y/Z offset, scale, local rotation, represented range, available
atom markers, Pro category filters, grid visibility, reset anchor placement,
and status.

The normal plugin UI exposes the daily placement and operation controls:

- `Host Surface`
- `Display Surface`
- `Desktop Placement`
- `VR Placement`
- `HUD Offset X`
- `HUD Offset Y`
- `HUD Offset Z`
- `HUD Scale`
- `Radar Mode`
- `Wrist Scale`
- `Wrist Offset X`
- `Wrist Offset Y`
- `Wrist Offset Z`
- `Radar Range Meters`
- `Show Target Markers`
- `Show Lights`
- `Show People`
- `Show Custom Unity Assets`
- `Show Empty`
- `Show SubScene`
- `Show ImagePanel`
- `Show Animation`
- `Show Force`
- `Show Shapes`
- `Show Sounds`
- `Show Triggers`
- `Show Uncategorized Atoms`
- `Grid Enabled`
- `Grab Handles Enabled`
- `Grab Haptics`

The older calibration and anchor controls remain registered so existing prefs
and older CUA presets load, but they are not part of the normal plugin UI. When the
plugin is loaded on a movable atom, the hidden `Placement Mode` and capture
actions still exist for compatibility; scene/session use works through the
offset sliders and reset button.

## Performance Posture

- Meshes and materials are created once and destroyed with the plugin.
- Selection is polled on `Selection Poll Seconds`; there is no per-frame atom
  scan.
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

- Free: `FA_RADAR_FREE` -> `fa_radar.free.0.1.35.dll`
- Pro: `FA_RADAR_PRO` -> `fa_radar.pro.0.1.35.dll`

The build helper runs `scripts\Obfuscate-FaRadarPlugin.ps1` unless
`-SkipObfuscation` is passed. The wrapper follows the FAP model: pinned
`Obfuscar.GlobalTool`, config-driven profiles, `FrameAngelRadar` keep rules,
VaM lifecycle callback skip rules, and a `.obf-report.json` next to each output
DLL.

The build helper also stages candidate `.var` packages under `build\packages`
with DLLs under `Custom/Plugins` and a root `meta.json`. The first Free test
package is `FrameAngelDev.Radar.1.var`. The Pro package additionally stages
`Custom/Atom/Empty/Preset_FrameAngel_Radar_Empty.vap`.

`scripts\Deploy-FaRadar.ps1` calls the build helper, then copies edition DLLs
to direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.35.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.35.dll`
- `F:\sim\vam\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.35.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.35.dll`
- `C:\vam\virgin-recordable-02\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`

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

Overlap clustering and God Mode are planned but not implemented in 0.1.35.
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
