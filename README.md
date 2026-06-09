# fa_radar

Frame Angel Radar is a small VaM scene/session utility plugin that shows a
HUD, wrist, or static-scene radar for selected and available atoms.

Current branch version: `0.1.32`.

Current product contract version: `0.1.32`.

The current slice is compiled C# only:

- no Unity project
- no asset bundle
- compiled VaM plugin DLL
- no raw runtime file IO; global prefs use VaM `FileManagerSecure` only
- generated translucent/emissive sphere shell with a subtle lit material and
  higher subdivisions
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
- HUD and wrist scale controls change overall rendered radar size only, with a
  1m displayed diameter cap at the default visual radius
- user center marker
- unified desktop/VR sphere-grid treatment
- camera-local anchoring to reduce desktop navigation jitter
- stable look-camera caching to prevent HUD anchor jumps during atom add/remove
  churn
- shared anchor modes for HUD/view, static world placement, containing-atom/Empty
  placement, and explicit atom UID placement without duplicating radar logic
- promoted HUD X/Y/Z and scale controls near the top of the native plugin UI,
  with automatic global preference saves after values change
- scene/session plugins expose separate `Desktop Placement` and `VR Placement`
  choices, while Empty/atom-hosted instances stay scene-anchored to their host
- scene/session desktop loads recover older pinned-world desktop prefs back to
  `Attached To UI` once, so a saved off-screen/static desktop placement cannot
  make Radar appear lost; Empty/atom-anchor prefs are not migrated
- scene/session placement controls stay visible even if older saved plugin
  state contains the legacy `CUA Anchor Preset` compatibility flag
- session grab movement is default-on and direct: grip near the radar, move the
  controller, release to apply placement; no visible grab handles are drawn
- optional `Radar Mode` values: `HUD`, `wrist-left`, `wrist-right`,
  `wrist-left-always-on`, and `wrist-right-always-on`; non-always-on wrist
  modes start hidden until the outward wrist-twist reveal is active
- wrist modes use their own wrist-relative offset/scale prefs and can hand off
  to the opposing controller with a grip drag across the body
- Pro Empty atom preset at
  `Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- FAAR visibility handshake reads
  `Custom\PluginData\FrameAngelMediaCore\recorder_v2_state.json` and hides
  Radar visuals when `radarHudVisible` is false; the plugin status reports
  `Hidden by FAAR radarHudVisible=false.` when that is the reason it is hidden
- optional world-axis alignment for the grid and rings
- ground-axis lock so the grid represents real world X/Z and camera roll does
  not roll the radar Z axis
- circular-clipped one-meter grid centered with the sphere and always panned
  from user world X/Z movement
- selected-atom sphere marker without an extra outer outline
- selected ground-drop projection is opt-in so a current selection does not
  read as a duplicate highlight by default
- height stems for user, selected atom, and visible available atoms
- range-edge fade and depth size cues for selected/available markers
- edition-gated available atom markers: Free shows every eligible atom together,
  Pro exposes Light, Person, CUA, Empty, SubScene, ImagePanel, Animation, Force,
  Shapes, Sounds, Triggers, and other category filters
- Pro-only rotation axes, light range spheres, spotlight cones, and optional
  user/desktop/scene-camera POV frustums as the first movie-studio scene-map
  layer
- click-to-select for visible available CUA/light/person/other atom markers
- session-plugin-only direct grip movement with OVR haptics; grip near the
  radar, move the controller, and release to apply HUD/static/wrist-relative
  offsets
- two-hand outward-twist accordion scaling for HUD and wrist modes
- global non-scene-stored preferences under
  `Custom\PluginData\FrameAngel\Radar`, split into common and Pro files
- Empty/atom-anchor preset instances use separate global preference files:
  `preferences_cua_common.json` and `preferences_cua_pro.json`
- the plugin UI reports `Host Surface` and `Display Surface` so a saved session
  can distinguish scene/session Desktop, scene/session VR, and Empty anchors
- previous-selection rendering parked for now
- Free/Pro editions compile from one codebase with static symbols; Free is the
  unrestricted radar that shows everything, while Pro adds filters, category
  colors, light volumes, rotation axes, and filming POV helpers
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

- `F:\sim\vam\Custom\Plugins\fa_radar.free.0.1.32.dll`
- `F:\sim\vam\Custom\Plugins\fa_radar.pro.0.1.32.dll`
- `F:\sim\vam\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.free.0.1.32.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.pro.0.1.32.dll`
- `C:\vam\virgin-recordable-02\Custom\Atom\Empty\Preset_FrameAngel_Radar_Empty.vap`

Release `.var` names remain undecided; current package outputs use neutral
dev candidate filenames under `build/packages`.
