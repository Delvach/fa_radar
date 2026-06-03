# fa_radar

Frame Angel Radar is a small VaM scene/session utility plugin that shows a
HUD-relative radar for the currently selected atom.

Current branch version: `0.1.2`.

The current slice is compiled C# only:

- no Unity project
- no asset bundle
- compiled VaM plugin DLL
- no runtime file reads or writes
- generated translucent/emissive sphere shell
- three generated rotating rings
- faded generated meter grid
- user center marker
- desktop top-down mode for first desktop testing
- camera-local anchoring to reduce desktop navigation jitter
- selected-atom and faded last-selected sphere markers

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

- `F:\sim\vam\Custom\Plugins\fa_radar.0.1.2.dll`
- `C:\vam\virgin-recordable-02\Custom\Plugins\fa_radar.0.1.2.dll`

The operator clarified these are instructions for going forward; this branch is
not live-deployed unless a deploy receipt says so.
