# FA Radar Product Editions V1

Updated: 2026-08-03

## Decision

Free and Pro are compiled from one codebase. Edition differences are controlled
by build gates, not by maintaining separate runtime forks.

The current build slice is `0.1.51`. It produces Free and Pro DLLs from the
same source file:

- `fa_radar.free.0.1.51.dll`
- `fa_radar.pro.0.1.51.dll`

The first Free testing package is `FrameAngelDev.Radar.1.var`. Public release
branding remains undecided.

## First Release UI Scope

The first release uses native VaM plugin UI only.

- no external browser UI
- no companion app UI
- no custom in-world Frame Angel switch surface
- no separate control panel outside VaM's plugin UI

Controls for the first version are ordinary plugin checkboxes, sliders, popups,
buttons, and text fields, but 0.1.51 intentionally keeps Free sparse: desktop
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
The `0.1.39` budget pass adds nearest-first marker caps, scale-safe visual
center caching, lazy Pro rich overlay renderers, and default-off noisy Pro
overlays so wild scenes do not allocate axes/light volumes for every marker.
The `0.1.40` hotfix keeps those budgets and maps player navigation/crosshair
utility atoms to the active viewer height instead of their floor-rooted atom
transform.
The `0.1.41` polish pass keeps HUD-scale adjustment usable at small sizes,
prevents the redundant HUD-mode self marker from reading as a stray floor dot,
keeps far markers visible with fade/projection outside the range shell, and
switches large-area grid density to 10-meter cells.
The `0.1.42` HUD correction narrows the HUD scale slider's maximum daily range
so small adjustments are not compressed by the full 1m placement cap.
The `0.1.43` marker pass gives Pro person atoms generated polygon person
markers while preserving the existing pink/blue/neutral gender color language.
The `0.1.44` visibility polish replaces the large selected-target ball with a
small 3-ring/cue treatment, turns useful Pro marker overlays on by default
except Player Navigation Panel utility atoms, and renames the overlay cap to
`Detail Overlay Limit`.
The `0.1.45` depth-clarity pass keeps those useful overlays enabled while
making unselected dense-scene context quieter: available markers, axes, light
volumes, and camera frustums get lower defaults plus depth-weighted visual
alpha/scale. Selected targets remain prioritized outside the detail-overlay
budget.
The `0.1.46` desktop interaction pass lets mouse wheel over the visible radar
adjust `Radar Range Meters` directly, zooming represented meters without
changing HUD or wrist placement scale.
The `0.1.47` director-readability pass quiets background Pro detail for dense
movie-studio scenes: non-selected axes/light volumes/frustums get lower
defaults, stronger context attenuation, and a capped background overlay budget
while selected-target detail remains outside that budget.
The `0.1.48` label/axis pass adds capped Pro procedural scene labels with
viewer/world/object orientation choices and changes rotation axes to a
four-renderer/seven-piece glyph so the center cube stays readable without
seven renderers per marker.
The `0.1.49` label-callout/UI pass keeps those generated label meshes but makes
labels selected-only by default, lowers default scale/limit, moves tags to
outside-shell callouts with pooled leader lines, and moves primary category
checkboxes above advanced label/overlay tuning in the native VaM panel.
The `0.1.51` hand-input pass preserves the corrected creator-host visuals and
Room Compass behavior, reads only active public VaM controller/hand transforms
for wrist placement, fails closed when neither exists, and restores an invisible
VaM full-grab target for optical HoldGrab movement without a hard SteamVR
assembly dependency.

## Free Edition

Free is the radar.

- same generated HUD radar foundation
- free movement, scale, and placement controls
- session-plugin direct grip movement: controller grip keeps its proximity and
  OVR-haptic path, while optical pinch/HoldGrab moves the same placement state
  through an invisible VaM `FreeControllerV3` target and stock full-grab result
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
- creator-facing Empty/CUA resources should be thin anchor hosts around the shared
  runtime, not duplicate radar logic
- Pro ships `Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap` as a
  scene-anchor starter
- Pro also ships
  `Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`; its UI keeps
  scale, range, filters, display/tuning, grid, status, and `Room Compass`, but
  excludes wrist, grab, local-offset, anchor-rotation, and session-placement
  controls because the CUA atom owns movement
- Empty/CUA `Room Compass` is off by default; when enabled, the host remains at
  its VaM pose while Radar content is scene-origin/world-aligned and positions map 1:1
- Empty/CUA anchor instances use `preferences_cua_common.json` and
  `preferences_cua_pro.json` so creator-anchor tuning does not pollute normal
  HUD/session Radar preferences. The legacy filenames are kept for older CUA
  compatibility.
- The first grab-handle implementation is session/scene-plugin only; Empty/CUA
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
- `fa_radar.pro.0.1.51.var`

The Pro candidate package includes both creator presets under
`Custom/Atom/Empty` and `Custom/Atom/CustomUnityAsset`; Free does not ship
creator-facing anchor resources.

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
