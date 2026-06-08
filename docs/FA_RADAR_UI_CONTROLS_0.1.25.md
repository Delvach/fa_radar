# FA Radar UI Controls 0.1.28

This inventory records the current plugin UI pruning decision. The runtime still
registers the older controls and preferences for compatibility, but the normal
VaM plugin UI now favors the controls needed during ordinary testing.

## Normal UI

- `Radar Enabled`: master runtime visibility and tick gate.
- `Desktop Placement`: chooses `Attached To UI` or `Pinned In World`.
- `HUD Offset X`, `HUD Offset Y`, `HUD Offset Z`: HUD-relative placement.
- `HUD Scale`: HUD size only; it does not change represented meters.
- `Reset HUD Offset`: restores the default HUD placement.
- `Radar Mode`: `HUD`, wrist-left/right, and wrist-left/right always-on.
- `Wrist Scale`: wrist radar size only; it does not change represented meters.
- `Wrist Offset X`, `Wrist Offset Y`, `Wrist Offset Z`: wrist-relative
  placement.
- `Radar Range Meters`: represented world radius in meters.
- `Available Atom Markers`: shows or hides non-selected atom markers.
- `Show Lights`, `Show People`, `Show CUA Atoms`, `Show Empty`,
  `Show SubScene`, `Show ImagePanel`, `Show Animation`, `Show Force`,
  `Show Shapes`, `Show Sounds`, `Show Triggers`, `Show Other Atoms`: Pro-only
  category filters.
- `Grid Enabled`: shows or hides the meter grid.
- `Grab Handles Enabled`: enables invisible direct grip placement.
- `Grab Haptics`: enables controller feedback for grab, hand-off, reveal, and
  scale gestures.
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
  `Capture Static From Current View`, static world position and rotation, and
  manual anchor rotation.
- Visual calibration: rings, ring rotation speed, shell/ring/grid/marker alpha,
  emission strength, target marker scale, height scale, height stem alpha,
  available atom alpha, depth size cue and strength, selected ground drop, and
  range fade.
- Prototype behavior switches: `Flatten Target Y`, `World Axis Align`,
  `Ground Axis Lock`, `Grid Follows User`, `Grid Clip Circle`,
  `Placement Mode`, `Capture HUD Offset From Atom`, `Wrist Twist Degrees`,
  click selection radius, grab hit radius, selection poll interval, and atom
  poll interval, `Global Prefs Auto Save`, and manual save/load actions.
- Legacy range modifiers: `Floor Area Scale` and `Grid Step Meters` remain
  registered, but 0.1.28 makes the visible range and one-meter grid contract the
  runtime authority.

## Decision

Agent review agreed that normal testing needs mode, HUD/wrist placement, grab,
target visibility, Pro category filters, meter range, reset, and status. The
rest is kept out of the main UI so stale prototype settings cannot hide the
important controls or silently change the meter contract.
