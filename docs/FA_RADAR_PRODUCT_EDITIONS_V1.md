# FA Radar Product Editions V1

Updated: 2026-06-10

## Decision

Free and Pro are compiled from one codebase. Edition differences are controlled
by build gates, not by maintaining separate runtime forks.

The current build slice is `0.1.38`. It produces Free and Pro DLLs from the
same source file:

- `fa_radar.free.0.1.38.dll`
- `fa_radar.pro.0.1.38.dll`

The first Free testing package is `FrameAngelDev.Radar.1.var`. Public release
branding remains undecided.

## First Release UI Scope

The first release uses native VaM plugin UI only.

- no external browser UI
- no companion app UI
- no custom in-world Frame Angel switch surface
- no separate control panel outside VaM's plugin UI

Controls for the first version are ordinary plugin checkboxes, sliders, popups,
buttons, and text fields, but 0.1.38 intentionally keeps Free sparse: desktop
placement, VR placement, scale, HUD offsets, and static desktop offsets.
Free's grab/wrist behavior remains available through the default runtime path
and saved preferences without exposing prototype tuning controls.
Preference writes happen
automatically after value changes; manual save buttons are not exposed.
Prototype calibration controls remain registered but hidden. The first release
should focus on core radar features: placement, scale up to a 1m rendered radar
diameter, all-atom visibility in Free, Pro filters/colors, marker clarity,
useful light discovery, package/deploy reliability, and stable performance.
The `0.1.37` Pro filters keep navigation panel and camera/display-control atoms
as separate default-hidden categories; this is distinct from optional POV and
scene-camera frustum overlays.
The `0.1.37` Pro tuning pass keeps split light alpha for point-light and
spotlight controls, adds a light volume scale control, and starts the
non-sphere marker language with flat rectangles for panel/slate/screen-style
atoms and wider rectangles for SubScene atoms.
The 0.1.36 Pro grab pass also adds a default-off throw-pin option: releasing
a direct-grabbed radar with velocity can launch it into world space, grow it,
and persist it as a pinned world-static radar until it is grabbed again.
The `0.1.37` visual fix keeps spotlight cones open-ended and clipped to the
radar shell, and adds a no-visible-marker status diagnostic for range/filter
debugging after scene reloads.
The `0.1.38` performance pass keeps those product features but moves available
atom rendering to cached atom records, block-grown marker pools, frame
signatures, cached bounds/light metadata, and coalesced material writes.

## Free Edition

Free is the radar.

- same generated HUD radar foundation
- free movement, scale, and placement controls
- session-plugin direct grip movement with OVR haptics; grip near the radar,
  move the controller, and release to apply the HUD/static/wrist placement state
- two-hand outward-twist accordion scaling for HUD and wrist modes
- wrist/HUD placement behavior and handoff logic remain runtime features, but
  Free does not expose prototype wrist tuning controls in the plugin UI
- global placement/scale preferences under
  `Custom\PluginData\FrameAngel\Radar`
- normal HUD/session prefs are separate from Empty/atom-anchor prefs
- no visibility filtering
- all available radar-supported atoms are shown together as the same yellow dot
- no in-game filter control surface
- no category-specific Pro color system or custom appearance controls
- no light volume/range/spot visualization
- no rotation-axis, richer marker-shape, or camera/frustum semantic overlays

Free should stay useful, generous, and simple: the operator can make the radar
look and sit how they want, but the radar does not become an atom-management
instrument.

## Pro Edition

Pro is the operational radar.

- visibility switches for atom categories and specific target lanes
- Pro-only filter preferences stored separately from common Free/Pro prefs
- native VaM plugin UI controls for first release filters and tuning
- category colors, including customizable defaults for women and men
- default people colors start as pink for women and blue for men
- Light, Person, CUA, Empty, SubScene, ImagePanel, Animation, Force, Shapes,
  Sounds, Triggers, and other-atom visibility can be controlled separately
- point-light range visualization as generated spheres
- spotlight visualization as generated cones
- spotlight rotation, range, and spot-angle representation
- directional lights stay as dots until they have a better dedicated visual
- separate point-light alpha, spotlight cone alpha, and light volume scale
  controls for VR readability
- per-marker rotation axes in VaM/world colors
- first-pass non-sphere marker meshes for panel/slate/screen and SubScene-style
  atoms
- optional user, desktop, and scene-camera frustum helpers for creator filming
- color customization is a Pro feature
- creator-facing Empty/atom resources should be thin anchor hosts around the shared
  runtime, not duplicate radar logic
- Pro ships `Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap` as a
  scene-anchor starter
- Empty/atom-anchor instances use `preferences_cua_common.json` and
  `preferences_cua_pro.json` so creator-anchor tuning does not pollute normal
  HUD/session Radar preferences. The legacy filenames are kept for older CUA
  compatibility.
- The first grab-handle implementation is session/scene-plugin only; Empty
  anchoring uses VaM's normal atom movement/parenting instead.

Pro should make scene diagnosis and targeting faster without turning every
scene object into noise. Filters and semantic visuals are the value line.

## Build Gate Contract

The source tree stays shared. Compile/package gates decide which edition is
produced.

Build symbols:

- `FA_RADAR_FREE`
- `FA_RADAR_PRO`

Rules:

- shared radar placement, grid, marker, click-selection, and generated-material
  foundation remains common code
- Free builds exclude Pro-only controls and Pro-only generated visuals
- Pro builds include filters, native plugin UI controls, color customization,
  light volume visuals, rotation axes, and filming POV helpers
- no raw runtime file IO, reflection, broad JSON object serializers, repo-local
  runtime JSON dependency, or absolute development paths are introduced for
  edition gating
- global preferences use VaM `FileManagerSecure` and flat scalar JSON only;
  normal HUD/session and Empty/atom-anchor profiles use separate files
- edition gates should be static compile/package gates, not fragile runtime
  string checks

## Packaging Notes

Current build helpers stage candidate `.var` packages:

- `FrameAngelDev.Radar.1.var`
- `fa_radar.pro.0.1.38.var`

The Pro candidate package includes the Empty atom preset under
`Custom/Atom/Empty`; Free does not ship creator-facing anchor resources.

Human-facing release `.var` product naming remains undecided. Current
candidates:

- `FrameAngelDev.Radar.1.var`
- `FrameAngel.DaFuqIzzit.1.var`
- `FrameAngel.Radar.1.var`

The release package-name decision is separate from the compile-gate
architecture.

## Parked

- license or entitlement mechanism
- custom Frame Angel switch UI
- external/browser/companion UI
- final people classification source for women/men color defaults
- Director Mode object grab/drag/rotate
