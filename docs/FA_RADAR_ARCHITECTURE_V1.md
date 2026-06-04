# FA Radar Architecture V1

Updated: 2026-06-04

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.11` on branch
  `codex/0.1.11-edition-build-packaging`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as compiled VaM plugin DLLs:
  `Custom/Plugins/fa_radar.free.0.1.11.dll` and
  `Custom/Plugins/fa_radar.pro.0.1.11.dll`.
- Intended plugin surface: scene or session plugin. Atom plugin loading still
  works for placement capture, but the operator target is scene/session.
- No Unity project, asset bundles, external JSON, runtime file IO, or
  reflection.

## Prototype Visual

The `0.1.11` branch is deliberately prototype-first. It uses generated emissive
polygons so targeting can be tested before any art polish:

- unified desktop/VR treatment using the same sphere shell, meter grid, and
  target markers
- a subtle transparent sphere material using the Standard shader when available,
  with low emission and increased mesh subdivisions so the shell reads round
  without becoming attention-grabbing
- three cached annulus meshes tied to the world-axis visual root
- VaM/world axis ring colors: X is red, Y is green, and Z is blue
- faded meter grid centered through the sphere on the actual world X/Z ground
  plane
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

Previous-selection rendering is parked in `0.1.11`. `Selected Ground Drop`
controls only the current atom's optional ground projection dot.

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

## HUD Placement

The HUD root follows `SuperController.singleton.lookCamera.transform`, with
`Camera.main` as fallback. In `Anchor To View` mode the HUD root is parented to
the viewer transform and uses local offset/rotation instead of chasing a
smoothed world-space position. This is the default for desktop testing because
it avoids navigation jitter and keeps rotation locked to the current view.

Placement uses a saved local offset:

- `HUD Offset X`
- `HUD Offset Y`
- `HUD Offset Z`
- `HUD Scale`
- `View Yaw Offset`
- `Desktop Tilt Degrees`
- `Axis Yaw Offset`
- `Ground Axis Lock`
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
- Grid mesh rebuilds only when `Radar Range Meters`, `Grid Step Meters`, clip
  mode, or the quantized viewer grid offset changes.
- Click selection is idle unless the desktop left mouse button goes down.
  Marker hit-testing only checks the visible pooled marker list.
- Materials use the FA Keyboard-inspired overlay pattern:
  `Hidden/Internal-Colored`, alpha blend, `ZWrite=0`, and
  `CompareFunction.Always`.

## Build And Deploy Contract

`scripts\Build-FaRadar.ps1` compiles both editions by default:

- Free: `FA_RADAR_FREE` -> `fa_radar.free.0.1.11.dll`
- Pro: `FA_RADAR_PRO` -> `fa_radar.pro.0.1.11.dll`

The build helper runs `scripts\Obfuscate-FaRadarPlugin.ps1` unless
`-SkipObfuscation` is passed. The wrapper follows the FAP model: pinned
`Obfuscar.GlobalTool`, config-driven profiles, `FrameAngelRadar` keep rules,
VaM lifecycle callback skip rules, and a `.obf-report.json` next to each output
DLL.

The build helper also stages neutral candidate `.var` packages under
`build\packages` with DLLs under `Custom/Plugins` and a root `meta.json`.

`scripts\Deploy-FaRadar.ps1` calls the build helper, then copies edition DLLs
to direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.11.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.11.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.11.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.11.dll`

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
