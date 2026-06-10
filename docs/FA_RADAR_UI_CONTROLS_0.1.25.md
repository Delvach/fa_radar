# FA Radar UI Controls 0.1.36

This inventory records the current plugin UI pruning decision. The runtime still
registers the older controls and preferences for compatibility, but the normal
VaM plugin UI now favors the controls needed during ordinary testing.

## Free UI

- `Desktop Placement`: scene/session desktop choice, `Attached To UI` or
  `Pinned In World`.
- `VR Placement`: scene/session VR choice, `Attached To UI` or
  `Pinned In World`.
- `HUD Scale`: radar size only; it does not change represented meters.
- `HUD Offset X`, `HUD Offset Y`, `HUD Offset Z`: HUD-relative placement.
- `Static World X`, `Static World Y`, `Static World Z`: desktop/static
  placement offsets for pinned in-world placement.

Free keeps runtime grab, wrist reveal, haptics, all-atom visibility, grid,
height stems, and default visual behavior active, but does not expose prototype
tuning controls. All available atom markers render as the same yellow dot.

## Pro UI

Pro keeps the same placement controls and additionally exposes the operational
scene-map controls:

- `Radar Enabled`: master runtime visibility and tick gate.
- `Host Surface`: read-only surface classification, either scene/session or
  Empty/atom anchor.
- `Display Surface`: read-only display classification for scene/session,
  desktop or VR.
- `Reset HUD Offset`: restores the default HUD placement.
- `Radar Mode`: `HUD`, wrist-left/right, and wrist-left/right always-on.
- `Wrist Scale`: wrist radar size only; it does not change represented meters.
- `Wrist Offset X`, `Wrist Offset Y`, `Wrist Offset Z`: wrist-relative
  placement.
- `Radar Range Meters`: represented world radius in meters.
- `Show Target Markers`: shows or hides non-selected atom markers.
- `Show Lights`, `Show People`, `Show Custom Unity Assets`, `Show Empty`,
  `Show SubScene`, `Show ImagePanel`, `Show Animation`, `Show Force`,
  `Show Shapes`, `Show Sounds`, `Show Triggers`,
  `Show Uncategorized Atoms`: Pro-only category filters, checked on by default
  when old prefs do not carry the current filter-defaults marker.
- `Show Navigation Panels` and `Show Camera Atoms`: Pro-only utility atom
  filters, separated from uncategorized atoms and off by default.
- `Rotation Axes`, `Light Range Volumes`, `Spotlight Cones`, `User POV
  Frustum`, `Desktop POV Frustum`, and `Scene Camera Frustums`: Pro-only
  scene-map and filming overlays.
- `Rotation Axis Length`, `Rotation Axis Width`, `Point Light Alpha`,
  `Spotlight Cone Alpha`, `Light Volume Scale`, `Light Marker Scale`, `POV
  Frustum Length`, and `POV Frustum Alpha`: Pro-only overlay tuning.
- `Grid Enabled`: shows or hides the meter grid.
- `Grab Handles Enabled`: enables invisible direct grip placement.
- `Grab Haptics`: enables controller feedback for grab, hand-off, reveal, and
  scale gestures.
- `Throw Pin On Release`, `Throw Surface Stop`, `Throw Grow Scale`, and `Throw
  Velocity Scale`: Pro-only, default-off throw-to-world pin behavior for direct
  grip release.
- `Reset Global Prefs`: factory-reset global prefs.
- `Status`: read-only runtime feedback.

Preferences save automatically after value changes and reload automatically
from the shared preference files. There is no visible save button.

## Hidden Compatibility Controls

These controls remain registered and preference-backed, but are no longer shown
in the normal plugin UI because they are prototype, rare calibration, or CUA
anchoring surface:

- Anchor controls: `Anchor Mode`, `Anchor Atom UID`, `Anchor To View`,
  `CUA Anchor Preset`, `Use Selected As Anchor`, `Use Containing Atom Anchor`,
  `Capture Static From Current View`, static world rotation, and manual anchor
  rotation.
- Visual calibration: rings, ring rotation speed, shell/ring/grid/marker alpha,
  emission strength, target marker scale, height scale, height stem alpha,
  available atom alpha, legacy `Light Volume Alpha`, depth size cue and
  strength, selected ground drop, and range fade.
- Prototype behavior switches: `Flatten Target Y`, `World Axis Align`,
  `Ground Axis Lock`, `Grid Follows User`, `Grid Clip Circle`,
  `Placement Mode`, `Capture HUD Offset From Atom`, `Wrist Twist Degrees`,
  click selection radius, grab hit radius, selection poll interval, and atom
  poll interval, `Global Prefs Auto Save`, and manual save/load actions.
- Legacy range modifiers: `Floor Area Scale` and `Grid Step Meters` remain
  registered, but 0.1.36 makes the visible range and one-meter grid contract the
  runtime authority.

## Decision

The current Free testing pass needs only desktop/VR mode, scale, and basic
placement offsets. The richer scene-map controls stay in Pro so stale prototype
settings cannot hide the important controls or silently change the meter
contract.
