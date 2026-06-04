# fa_radar

Frame Angel Radar is a small VaM scene/session utility plugin that shows a
HUD-relative radar for the currently selected atom.

Current branch version: `0.1.8`.

The current slice is compiled C# only:

- no Unity project
- no asset bundle
- compiled VaM plugin DLL
- no runtime file reads or writes
- generated translucent/emissive sphere shell with a subtle lit material and
  higher subdivisions
- three generated rotating rings, with separate X/Z colors
- faded generated meter grid
- user center marker
- unified desktop/VR sphere-grid treatment
- camera-local anchoring to reduce desktop navigation jitter
- optional world-axis alignment for the grid and rings
- ground-axis lock so the grid represents real world X/Z and camera roll does
  not roll the radar Z axis
- circular-clipped grid centered with the sphere and panned from user world X/Z movement
- selected-atom and faded last-selected sphere markers
- selected ground-drop projection is opt-in so a current selection does not
  read as a duplicate highlight by default
- height stems for user, selected atom, and visible available atoms
- range-edge fade and depth size cues for selected/available markers
- filterable available atom markers, with lights enabled by default
- previous-selection rendering parked for now

## Files

- `config/fa_radar.version.json` - branch/version/deploy authority.
- `payload/Custom/Scripts/FrameAngel/Radar/FrameAngelRadar.cs` - VaM MVRScript source.
- `scripts/Deploy-FaRadar.ps1` - future compile/deploy helper.
- `eng/Verify-FaRadarContract.ps1` - static contract check for this repo.
- `docs/FA_RADAR_ARCHITECTURE_V1.md` - current implementation notes.

## Future Deploy

The deploy helper defaults to both VaM roots and copies the DLL directly into
each root's `Custom/Plugins` folder:

```powershell
.\scripts\Deploy-FaRadar.ps1
```

Default targets:

- `F:\sim\vam\Custom\Plugins\fa_radar.0.1.8.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.0.1.8.dll`

The operator clarified these are instructions for going forward; this branch is
not live-deployed unless a deploy receipt says so.
