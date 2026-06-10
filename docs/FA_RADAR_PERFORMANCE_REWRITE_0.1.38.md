# FA Radar Performance Rewrite 0.1.38

Updated: 2026-06-10

## Scope

0.1.38 is an adjacent runtime rewrite of the available-atom path. The product
surface stays the same: one self-contained MVRScript source, Free/Pro compile
gates, scene/session and Empty/atom-anchor support, generated meshes/materials,
global prefs, direct grip placement, and Pro-only rich visual controls.

Headless Unity prefab generation was evaluated and rejected for this slice.
Repo canon requires runtime code to stay self-contained under
`payload/Custom/Scripts`; no Unity project, assetbundle, repo-local runtime JSON,
or absolute development path may become a runtime dependency. Generated C# meshes
plus cached transforms/materials remain the compliant path.

## Performance Changes

1. Added `RadarFrame` so reference position, rotation, inverse rotation, range,
   height scale, visual radius, flattening, and frame signature are captured once
   per tick.
2. Quantized frame signatures skip available marker rendering when radar
   reference, scale, range, height, flattening, and atom revision are unchanged.
3. Added `AtomRecord` entries built during atom polling instead of deriving all
   marker state in the render loop.
4. Cached each atom root transform.
5. Cached visual center offsets from renderer bounds so markers center on object
   extents without per-frame renderer hierarchy scans.
6. Cached marker world positions and refreshed them only when root transform
   movement or rotation crosses small thresholds.
7. Cached category flags instead of repeatedly calling type/name classifiers.
8. Cached marker mesh choice for panel/SubScene shapes.
9. Cached Pro light handles during atom polling instead of scanning light
   hierarchies during marker rendering.
10. Available atom sort now uses cached squared distances.
11. Marker pools grow in 16-slot blocks to avoid exact-count reallocations.
12. Added `MarkerSlot` to cache marker/stem objects, material, mesh filter, mesh,
   and Pro child visual references.
13. Available marker render loop no longer calls
   `GetComponentsInChildren<Renderer>`.
14. Available marker render loop no longer calls
   `GetComponentsInChildren<Light>`.
15. Available marker render loop no longer resolves marker bounds from atom
   hierarchies.
16. Material writes go through `ApplyMaterialColorIfChanged`.
17. Material state is cached by material to avoid redundant shader property and
   emission writes.
18. Visibility fade still works but does not depend on gradient sphere effects.
19. Marker status text is throttled instead of formatted every frame.
20. Selection changes and click-select explicitly invalidate atom records so the
   selected atom is removed from available markers promptly.
21. Old global prefs migrate with `commonMarkerDefaultsVersion`, forcing
   available target markers back on by default without wiping placement prefs.
22. Grid refresh now uses the cached frame range and existing quantized grid
   offset path.
23. Free build avoids Pro light-cache fields so the rewrite does not add Free
   compile warnings.
24. Runtime keeps generated mesh caching rather than introducing prefabs,
   assetbundles, or Unity-authoring dependencies.

## Current Grab Direction

HUD-mounted grab/reposition works. Pulling a HUD radar away and capturing it to
a hand remains unfinished. The next design should keep the grabbing hand as the
only position authority while grabbed, with no influence from HUD or the other
hand until release or threshold handoff. The likely visual language is a simple
ghost target near the receiving arm/hand when the detach or hand-swap threshold
is nearly satisfied; once crossed, attach to that hand, store that hand-relative
offset, and end the grab event.

## Continuity

When continuing this lane, start from repo-local truth:

- `AGENTS.md`
- `config/fa_radar.version.json`
- `docs/FA_RADAR_ARCHITECTURE_V1.md`
- `docs/FA_RADAR_PRODUCT_EDITIONS_V1.md`
- this file
- `eng/Verify-FaRadarContract.ps1`
- latest build/deploy receipts under `build/receipts`

Do not inspect deployed VaM plugin folders as source. Deploy folders are proof
targets only. The project Hindsight bank is `fa-radar-codex`; retain this slice
as a source-cited performance packet after build/deploy verification.
