# Frame Angel Radar 2.0.0

Radar 2 is a separate compiled VaM `MVRScript` product. It does not load or
modify the legacy `FrameAngelRadar` class or its Free/Pro controls.

## Product surface

- `Radar Enabled`
- `Mode`: Scene, Room, Left Controller, Right Controller, Left Wrist, or Right Wrist
- one compact live status field

Scene position, quaternion rotation, and uniform scale are registered storables
but are intentionally not exposed as UI controls. VaM scene serialization is
their only persistence route.

## Tracked-hand join

The sole accepted route is:

`DirectGripHand accepted state -> FixedStepConsumer faar.tracked-hand-state.v7 -> FAARTrackedHandArmColliders receiver registration -> Segment_0 / Segment_27`

Radar retries that registration at a bounded interval only while disconnected.
Wrist modes fail closed when the producer, state, or selected palm is absent.
They never read a controller transform. Controller transforms are read only by
the two explicit Controller modes.

## Placement states

- Scene uses its saved world pose. Either tracked hand can lease the Empty's
  center controller. A second tracked pinch changes the interaction to uniform
  midpoint resize; releasing one side rebases the surviving single-hand grab.
- Room fixes the visual root to world origin, identity rotation, and scale one.
  Its content coordinates remain world meters, including point-light ranges and
  spotlight direction/range.
- Wrist reveal is driven by the producer's palm-presentation state. Reversing
  presentation during an active center grab captures the exact current world
  pose into Scene and switches modes without a release throw.

No mode implements throw, inertia, snapping, file preferences, reflection, or
a SteamVR/Valve dependency.
