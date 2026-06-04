# fa_radar

Frame Angel Radar is a small VaM scene/session utility plugin that shows a
HUD-relative radar for the currently selected atom.

Current branch version: `0.1.9`.

Current product contract version: `0.1.10`.

The current slice is compiled C# only:

- no Unity project
- no asset bundle
- compiled VaM plugin DLL
- no runtime file reads or writes
- generated translucent/emissive sphere shell with a subtle lit material and
  higher subdivisions
- three generated rotating rings, colored to match VaM/world axes
- faded generated meter grid
- user center marker
- unified desktop/VR sphere-grid treatment
- camera-local anchoring to reduce desktop navigation jitter
- optional world-axis alignment for the grid and rings
- ground-axis lock so the grid represents real world X/Z and camera roll does
  not roll the radar Z axis
- circular-clipped grid centered with the sphere and panned from user world X/Z movement
- selected-atom sphere marker without an extra outer outline
- selected ground-drop projection is opt-in so a current selection does not
  read as a duplicate highlight by default
- height stems for user, selected atom, and visible available atoms
- range-edge fade and depth size cues for selected/available markers
- filterable available atom markers, with lights enabled by default
- click-to-select for visible available CUA/light/person/other atom markers
- previous-selection rendering parked for now
- future Free/Pro editions will compile from one codebase with edition gates;
  Free is the unrestricted radar that shows everything, while Pro adds filters,
  in-game controls, customizable category colors, and light volume visuals

## Files

- `config/fa_radar.version.json` - branch/version/deploy authority.
- `payload/Custom/Scripts/FrameAngel/Radar/FrameAngelRadar.cs` - VaM MVRScript source.
- `scripts/Deploy-FaRadar.ps1` - future compile/deploy helper.
- `eng/Verify-FaRadarContract.ps1` - static contract check for this repo.
- `docs/FA_RADAR_ARCHITECTURE_V1.md` - current implementation notes.
- `docs/FA_RADAR_PRODUCT_EDITIONS_V1.md` - Free/Pro product split contract.

## Future Deploy

The deploy helper defaults to both VaM roots and copies the DLL directly into
each root's `Custom/Plugins` folder:

```powershell
.\scripts\Deploy-FaRadar.ps1
```

Default targets:

- `F:\sim\vam\Custom\Plugins\fa_radar.0.1.9.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.0.1.9.dll`

The operator clarified these are instructions for going forward; this branch is
not live-deployed unless a deploy receipt says so.
