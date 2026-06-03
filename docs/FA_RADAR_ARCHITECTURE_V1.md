# FA Radar Architecture V1

Updated: 2026-06-03

## Goal

VaM selection can leave the operator visually hunting for the selected atom.
Frame Angel Radar is a scene/session plugin DLL that gives a small,
HUD-relative radar centered on the user.

## Current Slice

- Version: `0.1.2` on branch
  `codex/0.1.2-desktop-anchor-targets`.
- One MVRScript source: `FrameAngelRadar`.
- Distributed as a compiled VaM plugin DLL:
  `Custom/Plugins/fa_radar.0.1.2.dll`.
- Intended plugin surface: scene or session plugin. Atom plugin loading still
  works for placement capture, but the operator target is scene/session.
- No Unity project, asset bundles, external JSON, runtime file IO, or
  reflection.

## Prototype Visual

The `0.1.2` branch is deliberately prototype-first. It uses generated emissive
polygons so targeting can be tested before any art polish:

- translucent sphere shell
- three cached annulus meshes rotating on different axes
- faded meter grid under the sphere
- green center marker for the user
- orange selected-atom sphere plus a faded grid drop marker
- faded orange last-selected sphere plus a faded grid drop marker

The selected atom is resolved with
`SuperController.singleton.GetSelectedAtom()` on a configurable poll interval.
The target position is converted through the current look camera with
`viewer.InverseTransformPoint(target.position)`, then divided by `Radar Range
Meters` and clamped to the unit sphere. This makes the marker represent the
selected item relative to the user's current HUD POV.

For desktop testing, `Desktop Top Down` maps viewer-local `x/z` onto the radar
plane and ignores vertical `y` for the marker position. This gives a smaller
2D top-down read while preserving the same selected-atom source.

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

When the plugin is loaded on a movable atom, `Placement Mode` can capture that
atom's current position relative to the look camera. Scene/session use still
works through the offset sliders and reset button.

## Performance Posture

- Meshes and materials are created once and destroyed with the plugin.
- Selection is polled on `Selection Poll Seconds`; there is no per-frame atom
  scan.
- Per-frame work is selected target transform, small vector math, ring rotation,
  and active-state diffs. In anchored mode the HUD position/rotation are local
  to the camera instead of smoothed through world space.
- Grid mesh rebuilds only when `Radar Range Meters` or `Grid Step Meters`
  changes.
- Materials use the FA Keyboard-inspired overlay pattern:
  `Hidden/Internal-Colored`, alpha blend, `ZWrite=0`, and
  `CompareFunction.Always`.

## Build And Deploy Contract

`scripts/Deploy-FaRadar.ps1` is the future deploy helper. Its default targets
are direct plugin folders, not subfolders:

- `F:\sim\vam\Custom\Plugins\fa_radar.0.1.2.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.0.1.2.dll`

The operator clarified that deploy instructions are forward-looking for now,
so this branch should not be treated as live-deployed until a receipt proves
both destinations.

## Parked

- Visual acceptance in VaM.
- Richer final art treatment after prototype targeting is proven.
- Unity-authored HUD assets; Unity remains out of scope until explicitly
  opened for this repo.
