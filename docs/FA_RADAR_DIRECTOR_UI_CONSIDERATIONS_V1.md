# FA Radar Director UI Considerations V1

Updated: 2026-06-28

## Intent

Radar is expected to evolve into the central VR director interface for the
Movie Studio experience. It is not there yet. Current Radar work should keep
improving the scene-map foundation while avoiding choices that would block a
future director surface.

The desired direction is sci-fi, but utility-driven: the flashy visual elements
must also be the controls. Camera rails, light volumes, frustums, movement
vectors, direction handles, preview ghosts, and scrub markers should explain
what will happen and let the operator control it directly.

## Experience Laws

- Radar is the center-stage director surface, not just a passive mini-map.
- Flash is acceptable when it makes the scene easier to direct, preview, or
  understand in VR.
- Visual clutter is a product risk. Dense scenes need selected/prioritized
  detail, not every possible overlay at equal strength.
- The first movie-studio retask should behave like a shot manager, not a
  general-purpose 3D gizmo.
- The studio can include stereoscopic SBS skybox review. 180 and 360 media
  are first-class viewing spaces, not just flat reference clips.
- Cameras and lights need manual control paths before full automation feels
  trustworthy.
- Lines, arcs, rays, and rails are first-class interaction objects. The user
  should be able to draw, select, preview, scrub, and later commit movement
  along them.
- A preview must be visually understandable before any live camera, light, or
  scene object is moved.
- Free Radar remains simple. Director UI is a Pro/Movie Studio scale layer.

## Director Objects

Future director interaction should treat these as manipulable scene objects:

- cameras: position, rotation, look-at, FOV, zoom, dolly, truck, pedestal,
  orbit, push-in, pull-out, and preview frustum
- lights: position, aim, range, intensity, cone/volume, sweep direction, and
  preview coverage
- paths: straight rails, drawn polylines, arcs/orbits, direction vectors,
  locked axes, and constrained movement handles
- timing: playhead, sample ticks, keyframes, easing hints, and ghosted future
  positions
- targets: selected atom, look-at target, light target, rail endpoint, and
  temporary preview point

The first implementation layers should not require all of these. Each slice
should preserve the possibility of adding them without changing the base map
contract.

## Existing Visual Reinterpretation

The current radar visual language should mostly survive the movie-studio retask,
with clearer director meanings:

- center green sphere: subject, look-at target, or actor anchor
- grid floor: studio floor plane
- horizontal green ring: orbit plane or camera placement ring
- red and blue vertical rings: pitch, roll, and world-orientation reference
- orange and white nodes: cameras, lights, screens, actors, microphones, shot
  targets, or utility markers
- transparent planes and cones: camera frustums, screen surfaces, capture zones,
  or light volumes
- small icons: live camera, recording camera, audio source, lip-sync target,
  selected rig, or armed/export state

This reinterpretation is a direction guide, not a requirement to show every
symbol at once.

## View Layers

Default director view should answer "where are my shots?" with as little noise
as possible:

- up to four camera markers
- subject marker
- camera aim lines
- active, armed, and recording state
- simple frustum preview
- orbit ring

Expanded rig/debug view should appear when a camera or object is selected,
rather than being visible for every object all the time:

- stabilization axes
- horizon and roll
- look-at target
- resolution, FPS, and audio status
- dropped-frame or encoder-load warnings
- tracking target
- exact transform controls

The selected camera should bloom open; unselected cameras should stay quiet.

## Shot Manager Contract

The first Director Mode should be a four-camera shot manager. It should answer
this immediately: what are my exports going to see?

Minimum useful state per camera:

- camera identity, such as A, B, C, or D
- recording/export state: idle, armed, recording, warning
- output mode, such as 1080p30 or 4K30
- lock/tracking state
- warning state when FPS, encoder, tracking, or audio is unhealthy

Status cards should stay hidden or compact by default, then appear when the
operator points at or selects a camera.

## Immersive Skybox Review

Stereoscopic side-by-side video skybox display in VaM is considered solved as a
local capability. The Movie Studio surface can plan around 180 and 360 review
spaces, including stereo SBS footage, rather than assuming every preview is a
flat panel.

This expands the studio model:

- flat screens remain useful for thumbnails, four-camera exports, and status
  cards
- 180 skyboxes are useful for partial immersive review and shot comparison
- 360 skyboxes are useful for full environment playback, blocking review, and
  context checks around the actor/subject
- stereo SBS matters for depth acceptance, camera placement, and whether a shot
  reads correctly in VR

Wi-Fi passthrough can still add latency. Director controls should therefore
distinguish:

- live control: immediate camera/light/subject manipulation
- local preview: low-latency generated rails, frustums, and shot state
- streamed passthrough: useful for review, but not the only source of truth for
  precise timing
- recorded/export review: what the final 180/360/SBS output actually contains

Radar should avoid becoming a heavy video player. Its role is to show where the
immersive review surface belongs in the studio, which camera/export state feeds
it, and whether the operator is looking at live control, preview, streamed
passthrough, or recorded output.

## Orbit Ring Interaction

The green orbit ring is the strongest first control metaphor:

- grab a camera marker and drag it around the ring to orbit around the subject
- drag up or down for boom height
- twist or rotate for roll or dutch angle
- select the center sphere to retarget cameras to the subject
- select a camera marker to solo or preview that camera
- long press a camera marker to arm or disarm recording
- double select a camera marker to snap to common shots such as wide, close,
  side, over-shoulder, and top-down

These interactions should preview first. Live movement and recording-state
changes should come only after the preview, selection, and cancellation language
is obvious in VR.

## Ownership Boundaries

- Radar owns the VR director surface, scene-map visualization, direct handles,
  preview rails, camera/light glyphs, and operator interaction language.
- Wings owns schemas, orchestration, receipts, track/intent documents, and any
  adapter-facing command vocabulary.
- FAAR owns live VaM camera execution and recording once a preview or command is
  accepted.
- FAP behavior/player surfaces remain separate and should be used only through
  public player/behavior contracts.

Radar should not import movie-studio runtime storage or broad external state.
Any future bridge should pass small in-memory commands or public schemas into
Radar-facing preview/control seams.

## Slice Discipline

Current core slices should keep making the map honest:

- stable reference-frame mapping between world space and radar-local space
- readable depth, fade, priority, and selection behavior in dense scenes
- stable identity for source atoms and generated visual objects
- pooled generated visuals rather than runtime file or Unity asset dependencies
- Pro-only semantic overlays layered on top of the Free-simple base

Discovery prototypes are useful when they answer a visual/control question that
cannot be settled in docs. Good prototype candidates:

- display-only camera rail with sampled camera glyphs
- display-only four-camera shot-manager view with compact state labels
- selected rail playhead moving along a path without mutating the scene
- light direction ray and cone preview from the selected light
- manual nudge/aim handles that report intended deltas before applying them
- one-shot preview of a `camera.motion_track.v1` path as a rail/frustum overlay

Bad prototype candidates:

- live mutation before a preview surface exists
- hidden scene persistence before the schema is chosen
- importing recorder/movie-studio storage into Radar
- adding broad UI controls that are not visible and useful in the first
  operator screenful

## Per-Slice Review Questions

Every future Radar slice related to Movie Studio or Director Mode should answer:

- What does this add to the eventual director surface?
- Is it preview-only, manual control, or live mutation?
- Is this a flat, 180, 360, or stereoscopic SBS review surface?
- Does Wi-Fi passthrough latency matter for this control, or is it only a
  review/display concern?
- Which object is selected, what is previewed, and what would be committed?
- Does the visual still read in a dense scene screenshot?
- Does it preserve future manual camera and light control?
- Does it keep Free simple and keep Director work out of Free-only contracts?
- Does it avoid raw runtime file IO, repo-local runtime dependencies, broad
  serializers, reflection, and absolute development paths?

## Suggested Next Slices

1. Finish the current depth/readability work so selected objects and useful
   overlays remain understandable in dense screenshots.
2. Add a display-only Pro four-camera shot-manager prototype. It should show
   camera A-D markers, aim lines, compact hidden-until-selected status, subject
   target, and simple frustums without moving or arming any camera.
3. Add a display-only orbit-ring control preview: camera marker follows the
   horizontal ring, with boom-height and roll preview language but no live
   mutation.
4. Add a display-only light direction/cone preview for the selected light using
   the same priority and depth attenuation rules.
5. Add an immersive review-surface marker prototype: flat, 180, 360, and stereo
   SBS states should read differently without turning Radar into the player.
6. Bridge a small public `camera.motion_track.v1` preview seam after Wings has a
   stable track document shape and receipt behavior.
7. Add manual apply controls only after preview, selection, and cancellation are
   visually obvious.
