# FA Radar Architecture V1

Updated: 2026-06-05

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.23` on branch
  `codex/0.1.23-direct-grip-move`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as compiled VaM plugin DLLs:
  `Custom/Plugins/fa_radar.free.0.1.23.dll` and
  `Custom/Plugins/fa_radar.pro.0.1.23.dll`.
- Pro also ships a thin CustomUnityAsset preset:
  `Custom/Atom/CustomUnityAsset/Preset_FrameAngel_Radar_CUA.vap`.
- Intended plugin surface: scene or session plugin. Atom plugin loading still
  works for placement capture, but the operator target is scene/session.
- No Unity project, asset bundles, raw runtime file IO, reflection, broad JSON
  object serializers, repo-local runtime dependencies, or absolute dev paths.
  Global user prefs are the only runtime file access and use VaM
  `FileManagerSecure` under `Custom\PluginData\FrameAngel\Radar`.

## Prototype Visual

The `0.1.23` branch preserves the prototype-first generated visual treatment,
promotes placement controls, and replaces visible grab handles with direct grip
movement. It uses generated emissive polygons so targeting can be tested before
any art polish:

- unified desktop/VR treatment using the same sphere shell, meter grid, and
  target markers
- a subtle transparent sphere material using the Standard shader when available,
  with low emission and increased mesh subdivisions so the shell reads round
  without becoming attention-grabbing
- three cached annulus meshes tied to the world-axis visual root
- VaM/world axis ring colors: X is red, Y is green, and Z is blue
- faded meter grid centered through the sphere on the actual world X/Z ground
  plane
- `Floor Area Scale` expands or contracts the represented meter range for the
  grid, marker mapping, and height mapping without changing the compass visual
  radius
- grid lines clipped to the radar circle
- grid panning from viewer world X/Z movement when `Grid Follows User` is on,
  including corrected forward/backward Z direction
- green center marker for the user
- orange selected-atom sphere without an extra outer outline, so marker size
  remains a proximity/depth cue
- optional selected world-ground grid drop marker, disabled by default so the
  current selection does not read as a duplicate highlight
- height stems from the scaled ground plane to user, selected, and visible
  available atom marker heights
- range-edge fade so markers just outside the radar range fade instead of
  popping on the boundary
- depth size cues so closer markers render larger and far markers render
  smaller
- edition-gated available atom markers: Free shows every eligible atom
  together with a neutral marker color; Pro exposes lights, CUA, people, and
  other atom filters and keeps category colors
- click-to-select for visible available CUA/light/person/other atom markers
- session-plugin-only invisible grab handles: one primary handle for moving,
  an active-only resize handle that follows the free controller, OVR
  grip-proximity fallback for move/resize when VaM does not report the dynamic
  handles as grabbed, an unhidden position-grabbable VaM controller for
  built-in handle movement, direct primary-handle displacement follow, haptic
  pulses, and a cached dotted resize guide
- generated HUD objects and materials carry the `favr.hud.radar` filming
  identifier so FAAR/recorder tooling can locate the radar in final-recording
  workflows without adding a recorder dependency or scene-stored control data

The selected atom is resolved with
`SuperController.singleton.GetSelectedAtom()` on a configurable poll interval.
The target position is converted through the current look camera with
`viewer.InverseTransformPoint(target.position)`, then divided by `Radar Range
Meters` and clamped to the unit sphere. This makes the marker represent the
selected item relative to the user's current HUD POV.

By default the selected marker uses all three viewer-local axes. `Flatten Target
Y` remains available as a test toggle when a stricter X/Z desktop read is more
useful than height/depth representation.

`World Axis Align` controls the grid/ring visual root. With the default `Ground
Axis Lock` enabled, the axis root is counter-rotated from the HUD/dish so its
world rotation matches real world XYZ. This keeps the meter grid on the actual
ground X/Z plane for desktop and VR and prevents camera roll from rolling the
radar Z axis. With `Ground Axis Lock` disabled, the older yaw-only axis behavior
is available for VR comparison. Markers remain POV-relative; grid-drop markers
are projected onto the ground-axis root from target world X/Z delta.

Previous-selection rendering is parked in `0.1.23`. `Selected Ground Drop`
controls only the current atom's optional ground projection dot.

## Session Grab Handles

`Grab Handles Enabled` is default-on for the session/scene plugin path only. The
CUA preference profile and CustomUnityAsset-containing atom path skip this
system so creator-anchor CUA behavior remains separate.

The active 0.1.23 session path is direct grip only: when a controller grip press
starts inside `Grab Hit Radius Meters` from the radar center, the plugin records
that controller position, tracks its world delta each frame, and writes the same
HUD/static/anchor offset storables that the native UI sliders use until the grip
is released. No visible VaM grab atom is drawn, no resize handle is spawned, and
no dynamic handle displacement is consumed. `Grab Haptics` uses guarded OVR
haptic pulses on move start and apply/release.

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
- `Custom\PluginData\FrameAngel\Radar\preferences_cua_common.json` for CUA
  preset placement and visual controls
- `Custom\PluginData\FrameAngel\Radar\preferences_cua_pro.json` for CUA Pro
  filter controls

Writes are debounced behind `Global Prefs Auto Save`. The plugin also exposes
`Load Global Prefs`, `Save Global Prefs`, and `Reset Global Prefs` buttons.
Loaded values are applied with `valNoCallback`, and a small shared in-process
cache keeps multiple Radar instances from repeatedly reading the same files.
The CUA preset uses `CUA Anchor Preset` to select the CUA preference profile,
so creator-anchor tuning does not pollute the normal HUD/session Radar profile.

Available atom markers poll `SuperController.singleton.GetAtoms()` on `Atom
Poll Seconds`, sort nearby atoms first, and use pooled generated marker/stem
objects. Free builds show every eligible atom that passes the baseline hidden,
off, selected, and containing-atom checks. Pro builds add the type/category/uid
bucket filters so normal operation can stay focused by leaving only useful
lanes enabled.

`Click Select Markers` uses cheap screen-space picking only on mouse-down. The
plugin projects visible marker objects through `lookCamera.WorldToScreenPoint`,
chooses the nearest marker inside `Marker Click Radius Pixels`, and selects that
atom through `SuperController.singleton.SelectController(atom.mainController,
false, false, false, true)`. This API is proved by the static verifier through
VaM metadata inspection: `Atom.mainController` is `FreeControllerV3`, and
`SuperController.SelectController(FreeControllerV3, alignView,
alignRotationOnly, alignUpDown, openUI)` exists in the target VaM assembly.

`Grid Follows User` uses the viewer's world X/Z position to offset the meter
grid before it is clipped into the radar circle. A 1m movement along world Z
changes the grid's Z offset in the matching direction modulo `Grid Step Meters`,
so the center marker remains the user while the world grid slides underneath it.
`Floor Area Scale` multiplies the effective radar range and height scale used
for per-meter mapping; it does not scale the generated sphere, rings, markers,
or HUD root.

## HUD Placement

The HUD root follows `SuperController.singleton.lookCamera.transform`, with the
last good viewer transform retained before falling back to `Camera.main`. In
`Anchor To View` mode the HUD root is parented to the viewer transform and uses
local offset/rotation instead of chasing a smoothed world-space position. This
is the default for desktop testing because it avoids navigation jitter, keeps
rotation locked to the current view, and prevents add/remove atom churn from
reanchoring to a transient camera.

`Anchor Mode` is the shared runtime adapter for HUD and creator-facing scene
placement. It does not fork the radar core:

- `HUD / View` keeps the existing camera-relative behavior.
- `World Static` freezes the generated radar root at a captured world pose.
- `Containing Atom` parents the radar root under the atom/CUA the plugin is
  loaded on, so creators can move or parent that host with normal VaM tools.
- `Anchor Atom UID` parents the radar root under any explicit atom UID,
  including a camera/CUA/control atom if the scene provides one.

The same `HUD Offset` and `HUD Scale` controls are reused as local anchor
offset/scale for atom-backed modes. `Anchor Rot X/Y/Z` gives a local rotation
for atom-backed modes, and `Static World X/Y/Z/Pitch/Yaw/Roll` stores the
static scene pose. The Pro CUA resource path should therefore stay thin:
creator-facing CUA resources can host or identify an anchor, while Free/Pro
feature behavior remains in one plugin codebase.

The Pro CUA preset loads the same plugin on a CustomUnityAsset atom, sets
`CUA Anchor Preset`, and uses `Containing Atom` anchoring. The switch is
restorable from the preset, but it is not part of the normal global preference
profile. When active, it uses the separate CUA preference files listed above.

Placement uses a saved local offset:

- `HUD Offset X`
- `HUD Offset Y`
- `HUD Offset Z`
- `HUD Scale`
- `Desktop Tilt Degrees`
- `Ground Axis Lock`
- `Floor Area Scale`
- `Selected Ground Drop`
- `Height Stems`
- `Height Scale Meters`
- `Range Fade Meters`
- `Depth Size Cue`
- `Available Atom Markers`
- `Show Lights`
- `Show CUA`
- `Show People`
- `Show Other Atoms`
- `Click Select Markers`
- `Marker Click Radius Pixels`
- `Grid Follows User`
- `Grid Clip Circle`

When the plugin is loaded on a movable atom, `Placement Mode` can capture that
atom's current position relative to the look camera. Scene/session use still
works through the offset sliders and reset button.

## Performance Posture

- Meshes and materials are created once and destroyed with the plugin.
- Selection is polled on `Selection Poll Seconds`; there is no per-frame atom
  scan.
- Per-frame work is selected target transform, small vector math, world-axis
  yaw resolution, optional ring rotation, lightweight grid mesh refresh when
  viewer grid offset changes, and active-state diffs. In anchored mode the HUD
  position/rotation are local to the camera instead of smoothed through world
  space.
- Grid mesh rebuilds only when effective radar range (`Radar Range Meters`
  multiplied by `Floor Area Scale`), `Grid Step Meters`, clip mode, or the
  quantized viewer grid offset changes.
- Click selection is idle unless the desktop left mouse button goes down.
  Marker hit-testing only checks the visible pooled marker list.
- Materials use the FA Keyboard-inspired overlay pattern:
  `Hidden/Internal-Colored`, alpha blend, `ZWrite=0`, and
  `CompareFunction.Always`.
- The generated object and material names include `favr.hud.radar` as a small
  filming identifier. This is intentionally name-based only: no Unity tag,
  file IO, recorder import, or scene persistence is added for filming.
- FAAR recorder visibility is read from
  `Custom\PluginData\FrameAngelMediaCore\recorder_v2_state.json`. If
  `radarHudFilmSubjectIdentifier` matches `favr.hud.radar` and
  `radarHudVisible` is false, Radar hides its generated visual root/materials.
  This does not modify placement, anchor mode, offsets, scale, or scene data.

## Build And Deploy Contract

`scripts\Build-FaRadar.ps1` compiles both editions by default:

- Free: `FA_RADAR_FREE` -> `fa_radar.free.0.1.23.dll`
- Pro: `FA_RADAR_PRO` -> `fa_radar.pro.0.1.23.dll`

The build helper runs `scripts\Obfuscate-FaRadarPlugin.ps1` unless
`-SkipObfuscation` is passed. The wrapper follows the FAP model: pinned
`Obfuscar.GlobalTool`, config-driven profiles, `FrameAngelRadar` keep rules,
VaM lifecycle callback skip rules, and a `.obf-report.json` next to each output
DLL.

The build helper also stages neutral candidate `.var` packages under
`build\packages` with DLLs under `Custom/Plugins` and a root `meta.json`.
The Pro package additionally stages
`Custom/Atom/CustomUnityAsset/Preset_FrameAngel_Radar_CUA.vap`.

`scripts\Deploy-FaRadar.ps1` calls the build helper, then copies edition DLLs
to direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.23.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.23.dll`
- `F:\sim\vam\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.23.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.23.dll`
- `C:\vam\virgin-recordable-02\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`

Future `.var` product naming is undecided. Current candidates are
`FrameAngel.DaFuqIzzit.1.var` and `FrameAngel.Radar.1.var`; current package
outputs remain dev candidates until the product name is chosen.

## Product Editions

Free and Pro are one codebase with compile/package gates for the different
editions, not separate runtime forks.

- Free is the movable, scalable, visually tunable radar, but it shows all
  supported radar atoms together.
- Pro adds visibility switches in the native VaM plugin UI, customizable
  category colors, and richer light visuals such as range spheres and spotlight
  cones that show rotation, range, and spot angle.

The first release is plugin-UI-only. Do not build or assume a browser,
companion, custom external, or separate in-world control surface for v1. The
first version should focus on core radar behavior: placement, scale, all-atom
visibility in Free, Pro category filters/colors, marker clarity, light-finding
basics, and deploy/package reliability.

The current product split authority is
`docs/FA_RADAR_PRODUCT_EDITIONS_V1.md`.

## Parked

- Visual acceptance in VaM.
- Richer final art treatment after prototype targeting is proven.
- External/browser/companion UI and custom control surfaces for later versions.
- Unity-authored HUD assets; Unity remains out of scope until explicitly
  opened for this repo.
