# fa_radar

Frame Angel Radar is a small VaM scene/session utility plugin that shows a
HUD, wrist, or static-scene radar for selected and available atoms.

Current branch version: `0.1.54`.

Current product contract version: `0.1.54`.

The current slice is compiled C# only:

- no Unity project
- no asset bundle
- compiled VaM plugin DLL
- no raw runtime file IO; global prefs use VaM `FileManagerSecure` only
- generated translucent/emissive sphere shell with a subtle unlit overlay
  material and higher subdivisions
- generated HUD objects and materials carry the `favr.hud.radar` filming
  identifier so FAAR/recorder tooling can locate the radar without scene
  storage or a product dependency
- static scene and Empty/atom-anchor radar displays use the radar's own world
  pose as the map origin; the user moves as a green marker while scene atoms
  stay stable in the display
- three generated rotating rings, colored to match VaM/world axes
- faded generated meter grid
- visible `Radar Range Meters` control that expands or contracts represented
  meters without changing the compass visual size
- desktop hover-wheel range scaling when the pointer is over the radar; wheel-up
  zooms into fewer represented meters and wheel-down expands range
- HUD and wrist scale controls change overall rendered radar size only, with a
  1m displayed diameter cap at the default visual radius
- user center marker
- unified desktop/VR sphere-grid treatment
- camera-local anchoring to reduce desktop navigation jitter
- stable look-camera caching to prevent HUD anchor jumps during atom add/remove
  churn
- shared anchor modes for HUD/view, static world placement, containing-atom/Empty
  placement, and explicit atom UID placement without duplicating radar logic
- one top-level `Radar Mode` selector exposes `World` alongside HUD, wrist, and
  palm modes; `World` captures the current Radar pose and then keeps it fixed
  in scene space
- legacy HUD, wrist, static-world, and atom-relative placement storables remain
  serialized for compatibility, but their offset/rotation/desktop/VR slider
  forest is no longer drawn in the normal native plugin UI
- scene/session desktop loads recover older pinned-world desktop prefs back to
  `Attached To UI` once, so a saved off-screen/static desktop placement cannot
  make Radar appear lost; Empty/atom-anchor prefs are not migrated
- scene/session placement controls stay visible even if older saved plugin
  state contains the legacy `CUA Anchor Preset` compatibility flag
- session grab movement is default-on and direct: controller grip keeps its
  existing proximity path, while optical pinch/HoldGrab can lease Radar's
  invisible VaM `FreeControllerV3` target through VaM's normal full-grab
  ownership; no visible grab handles are drawn
- the existing FAAR tracked-palm receiver is re-acquired at a bounded cadence
  while disconnected, so plugin/root load order cannot permanently strand Palm
  mode; disconnect and destruction unregister the prior receiver cleanly
- optional `Radar Mode` values: `HUD`, `World`, `wrist-left`, `wrist-right`,
  `wrist-left-always-on`, and `wrist-right-always-on`; non-always-on wrist
  modes start hidden until the wrist-twist reveal is active; Radar reads only
  active VaM controller/hand transforms and fails closed when neither exists,
  without taking a hard dependency on VaM's prohibited SteamVR API surface
- wrist modes use their own wrist-relative offset/scale prefs and can hand off
  to the opposing controller with a grip drag across the body
- Pro creator presets for both movable host types:
  `Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap` and
  `Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`
- Empty and CUA hosts expose mode, HUD/wrist scale, and the existing stock VaM
  center-grab toggle, but no manual offset, anchor-rotation, desktop/VR, or
  throw-placement controls
- atom-attached `Room Compass` is default-off on both Empty and CUA hosts; when
  enabled it leaves the host atom untouched, places Radar content at scene origin, bypasses radar-edge
  clamping/fading, and maps world positions and height at 1:1 while retaining
  tunable Radar visual scale; its sphere, rings, and meter grid expand to the
  configured `Radar Range Meters` in scene space
- FAAR visibility handshake reads
  `Custom\PluginData\FrameAngelMediaCore\recorder_v2_state.json` and hides
  Radar visuals when `radarHudVisible` is false; the plugin status reports
  `Hidden by FAAR radarHudVisible=false.` when that is the reason it is hidden
- optional world-axis alignment for the grid and rings
- ground-axis lock so the grid represents real world X/Z and camera roll does
  not roll the radar Z axis
- circular-clipped one-meter grid centered with the sphere and always panned
  from user world X/Z movement
- selected-atom marker without an extra outer outline
- selected navigation/crosshair utility markers use the active viewer height
  rather than VaM's floor-anchored utility atom root
- HUD scale uses a smaller daily adjustment range for finer small-size control
- in HUD/view mode the redundant user center/stem marker is hidden; it remains
  available for world-static and atom-anchored radar modes
- far selected and available markers fade and project just outside the radar
  shell instead of disappearing at the range edge
- the grid uses one-meter cells at room scale and switches to 10-meter cells
  for large represented areas
- Pro person atoms use a generated polygon person marker mesh while preserving
  pink/blue/neutral gender colors
- non-sphere marker meshes for panel/slate/screen-style atoms and SubScene
  atoms, while point-like atoms stay spherical
- selected ground-drop projection is opt-in so a current selection does not
  read as a duplicate highlight by default
- thinner height stems for user, selected atom, and visible available atoms;
  their X/Z half-width is `0.010` instead of `0.018`
- range-edge fade and depth size cues for selected/available markers
- edition-gated available atom markers: Free shows every eligible atom as the
  same yellow dot, while Pro exposes Light, Person, CUA, Empty, SubScene,
  ImagePanel, Animation, Force, Shapes, Sounds, Triggers, and other category
  filters
- Pro-only rotation axes, light range spheres, spotlight cones, separate light
  alpha/size tuning, and optional user/desktop/scene-camera POV frustums as the
  first movie-studio scene-map layer
- Pro spotlight cones are open-ended and clipped to the radar shell so wide
  spotlights cannot become filled world-covering discs
- click-to-select for visible available CUA/light/person/other atom markers
- session-plugin-only direct grip movement; controller grip retains OVR
  haptics, while optical HoldGrab observes VaM full-grab ownership without
  synthesizing actions or controller haptics
- two-hand outward-twist accordion scaling for HUD and wrist modes
- global non-scene-stored preferences under
  `Custom\PluginData\FrameAngel\Radar`, split into common and Pro files
- Empty/atom-anchor preset instances use separate global preference files:
  `preferences_cua_common.json` and `preferences_cua_pro.json`
- the runtime tracks host/display surface internally so saved sessions can
  distinguish scene/session Desktop, scene/session VR, and Empty anchors
- the `Status` field reports marker visibility counts when target markers are
  enabled but nothing is visible after a scene reload/filter/range change
- available markers are capped by `Max Visible Markers`, kept nearest-first, and
  report over-budget counts in status diagnostics
- Pro detail overlays use a plain `Detail Overlay Limit` and lazy-create
  per-marker axes/light/cone renderers only when enabled and near enough
- dense-scene context uses depth-weighted alpha/scale so unselected markers and
  Pro overlays recede instead of stacking at the same visual weight
- the selected target uses a small 3-ring marker plus an edge cue when it is
  outside the active desktop/VR camera view, instead of a large opaque ball
- Director readability defaults cap background Pro detail overlays and reduce
  camera/light/axis visual weight so busy movie-studio scenes stay readable
- Pro rotation axes use a generated four-renderer/seven-piece glyph: three
  colored half-axis pairs plus a small center cube, avoiding center overdraw
  fights without allocating seven renderers per marker
- Pro scene labels use capped procedural glyph meshes with `Scene Labels`,
  `Label Orientation`, `Label Limit`, `Label Scale`, and `Label Alpha`;
  selected labels stay outside the available-label budget
- Pro scene labels default to selected-only, use a smaller glyph scale, and
  place labels as outside-shell callouts with thin pooled leader lines instead
  of piling text inside the radar sphere
- procedural label facing includes the 180-degree mesh-front correction so
  viewer/world/object orientation modes read forward instead of mirrored
- Pro native plugin UI shows the daily atom/category checkboxes and high-value
  overlay toggles before placement/debug-style tuning sliders
- previous-selection rendering parked for now
- Free/Pro editions compile from one codebase with static symbols; Free exposes
  only desktop/VR placement, scale, HUD offsets, and static desktop offsets,
  while Pro adds filters, category colors, marker shapes, light volumes,
  rotation axes, and filming POV helpers
- FAAR/video-recorder integration consumes stable identifiers and visibility
  state only; Radar keeps placement authority
- first release UI is native VaM plugin UI only; no external/browser/companion
  UI surface
- FAP-style Obfuscar wrapper and `.var` package candidate staging

## Files

- `config/fa_radar.version.json` - branch/version/deploy authority.
- `config/obfuscation.defaults.json` - Obfuscar profile and keep-rule authority.
- `payload/Custom/Scripts/FrameAngel/Radar/FrameAngelRadar.cs` - VaM MVRScript source.
- `scripts/Build-FaRadar.ps1` - edition compile, obfuscation, package, and receipt helper.
- `scripts/Obfuscate-FaRadarPlugin.ps1` - bounded Obfuscar.GlobalTool wrapper.
- `scripts/Deploy-FaRadar.ps1` - edition-aware compile/deploy helper.
- `eng/Verify-FaRadarContract.ps1` - static contract check for this repo.
- `docs/FA_RADAR_ARCHITECTURE_V1.md` - current implementation notes.
- `docs/FA_RADAR_PRODUCT_EDITIONS_V1.md` - Free/Pro product split contract.

## Build And Deploy

The build helper defaults to Free and Pro, obfuscates with the `vam_compat`
profile, stages candidate `.var` packages, and writes a build receipt:

```powershell
.\scripts\Build-FaRadar.ps1
```

The deploy helper defaults to both VaM roots and copies both edition DLLs
directly into each root's `Custom/Plugins` folder:

```powershell
.\scripts\Deploy-FaRadar.ps1
```

Default targets:

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.54.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.54.dll`
- `F:\sim\vam\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `F:\sim\vam\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.54.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.54.dll`
- `C:\vam\virgin-recordable-02\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `C:\vam\virgin-recordable-02\Custom\Atom\CustomUnityAsset\Preset_FrameAngel_Radar_CUA.vap`

The first Free test package output is
`build/packages/FrameAngelDev.Radar.1.var`; public release `.var` branding
remains undecided.
