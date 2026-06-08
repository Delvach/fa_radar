# FA Radar Product Editions V1

Updated: 2026-06-08

## Decision

Free and Pro are compiled from one codebase. Edition differences are controlled
by build gates, not by maintaining separate runtime forks.

The current build slice is `0.1.30`. It produces Free and Pro DLLs from the
same source file:

- `fa_radar.free.0.1.30.dll`
- `fa_radar.pro.0.1.30.dll`

Current package outputs use neutral dev candidate names while release branding
remains undecided.

## First Release UI Scope

The first release uses native VaM plugin UI only.

- no external browser UI
- no companion app UI
- no custom in-world Frame Angel switch surface
- no separate control panel outside VaM's plugin UI

Controls for the first version are ordinary plugin checkboxes, sliders, popups,
buttons, and text fields, but 0.1.30 keeps the normal UI limited to daily
operation: host/display status, desktop/VR placement, HUD/wrist placement, mode,
range, atom visibility, Pro filters, grab, haptics, reset, and status.
Preference writes happen
automatically after value changes; manual save buttons are not exposed.
Prototype calibration controls remain registered but hidden. The first release
should focus on core radar features: placement, scale up to a 1m rendered radar
diameter, all-atom visibility in Free, Pro filters/colors, marker clarity,
useful light discovery, package/deploy reliability, and stable performance.

## Free Edition

Free is the radar.

- same generated HUD radar foundation
- free movement, scale, placement, and appearance controls
- session-plugin direct grip movement with OVR haptics; grip near the radar,
  move the controller, and release to apply the HUD/static/wrist placement state
- two-hand outward-twist accordion scaling for HUD and wrist modes
- optional `Radar Mode` values for HUD and wrist-left/right projection,
  including always-on wrist variants
- wrist modes keep their own wrist-relative offset/scale preferences and can
  hand off to the opposing controller with a cross-body grip drag
- shared HUD/static/atom anchor modes
- global placement/scale/look preferences under
  `Custom\PluginData\FrameAngel\Radar`
- normal HUD/session prefs are separate from Empty/atom-anchor prefs
- user-controlled visual tuning for the radar itself
- no visibility filtering
- all available radar-supported atoms are shown together
- no in-game filter control surface
- no category-specific Pro color system
- no light volume/range/spot visualization

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
- light range visualization as generated spheres
- spotlight visualization as generated cones
- spotlight rotation, range, and spot-angle representation
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
  and light volume visuals
- no raw runtime file IO, reflection, broad JSON object serializers, repo-local
  runtime JSON dependency, or absolute development paths are introduced for
  edition gating
- global preferences use VaM `FileManagerSecure` and flat scalar JSON only;
  normal HUD/session and Empty/atom-anchor profiles use separate files
- edition gates should be static compile/package gates, not fragile runtime
  string checks

## Packaging Notes

Current build helpers stage candidate `.var` packages:

- `fa_radar.free.0.1.30.var`
- `fa_radar.pro.0.1.30.var`

The Pro candidate package includes the Empty atom preset under
`Custom/Atom/Empty`; Free does not ship creator-facing anchor resources.

Human-facing release `.var` product naming remains undecided. Current
candidates:

- `FrameAngel.DaFuqIzzit.1.var`
- `FrameAngel.Radar.1.var`

The release package-name decision is separate from the compile-gate
architecture.

## Parked

- license or entitlement mechanism
- custom Frame Angel switch UI
- external/browser/companion UI
- final people classification source for women/men color defaults
