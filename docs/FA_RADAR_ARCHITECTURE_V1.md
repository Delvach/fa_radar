# FA Radar Architecture V1

Updated: 2026-06-03

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.5` on branch
  `codex/0.1.5-unified-sphere-grid`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as a compiled VaM plugin DLL:
  `Custom/Plugins/fa_radar.0.1.5.dll`.
- Intended plugin surface: scene or session plugin. Atom plugin loading still
  works for placement capture, but the operator target is scene/session.
- No Unity project, asset bundles, external JSON, runtime file IO, or
  reflection.

## Prototype Visual

The `0.1.5` branch is deliberately prototype-first. It uses generated emissive
polygons so targeting can be tested before any art polish:

- unified desktop/VR treatment using the same sphere shell, meter grid, and
  target markers
- a subtle transparent sphere material using the Standard shader when available,
  with low emission so the shell reads round without becoming attention-grabbing
- three cached annulus meshes tied to the world-axis visual root
- distinct, subdued X/Z ring colors for axis readability
- faded meter grid centered through the sphere at user `y=0`
- grid lines clipped to the radar circle
- grid panning from viewer world X/Z movement when `Grid Follows User` is on,
  including corrected forward/backward Z direction
- green center marker for the user
- orange selected-atom sphere plus a faded centered grid drop marker
- faded orange last-selected sphere plus a faded centered grid drop marker

The selected atom is resolved with
`SuperController.singleton.GetSelectedAtom()` on a configurable poll interval.
The target position is converted through the current look camera with
`viewer.InverseTransformPoint(target.position)`, then divided by `Radar Range
Meters` and clamped to the unit sphere. This makes the marker represent the
selected item relative to the user's current HUD POV.

`Flatten Target Y` maps viewer-local `x/z` onto the radar plane and ignores
vertical `y` for marker position. This keeps the user at sphere/grid center and
treats the radar as a world X/Z meter read even when the dish is tilted in the
HUD.

`World Axis Align` rotates the grid/ring visual root from the viewer's yaw, so
world X/Z axes rotate relative to the player's current POV instead of staying
screen-locked. Markers remain POV-relative; the grid and rings provide the
world-axis reference underneath them.

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
- Materials use the FA Keyboard-inspired overlay pattern:
  `Hidden/Internal-Colored`, alpha blend, `ZWrite=0`, and
  `CompareFunction.Always`.

## Build And Deploy Contract

`scripts/Deploy-FaRadar.ps1` is the future deploy helper. Its default targets
are direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.0.1.5.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.0.1.5.dll`

The operator clarified that deploy instructions are forward-looking for now,
so this branch should not be treated as live-deployed until a receipt proves
both destinations.

## Parked

- Visual acceptance in VaM.
- Richer final art treatment after prototype targeting is proven.
- Unity-authored HUD assets; Unity remains out of scope until explicitly
  opened for this repo.
