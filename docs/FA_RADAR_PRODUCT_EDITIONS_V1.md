# FA Radar Product Editions V1

Updated: 2026-06-04

## Decision

Free and Pro are compiled from one codebase. Edition differences are controlled
by build gates, not by maintaining separate runtime forks.

The current build slice is `0.1.11`. It produces Free and Pro DLLs from the
same source file:

- `fa_radar.free.0.1.11.dll`
- `fa_radar.pro.0.1.11.dll`

Current package outputs use neutral dev candidate names while release branding
remains undecided.

## First Release UI Scope

The first release uses native VaM plugin UI only.

- no external browser UI
- no companion app UI
- no custom in-world Frame Angel switch surface
- no separate control panel outside VaM's plugin UI

Controls for the first version should be ordinary plugin checkboxes, sliders,
buttons, and text fields. The first release should focus on core radar
features: placement, scale, all-atom visibility in Free, Pro filters/colors,
marker clarity, useful light discovery, package/deploy reliability, and stable
performance.

## Free Edition

Free is the radar.

- same generated HUD radar foundation
- free movement, scale, placement, and appearance controls
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
- native VaM plugin UI controls for first release filters and tuning
- category colors, including customizable defaults for women and men
- default people colors start as pink for women and blue for men
- CUA, light, people, and other-atom visibility can be controlled separately
- light range visualization as generated spheres
- spotlight visualization as generated cones
- spotlight rotation, range, and spot-angle representation
- color customization is a Pro feature

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
- no runtime file IO, reflection, repo-local JSON dependency, or absolute
  development paths are introduced for edition gating
- edition gates should be static compile/package gates, not fragile runtime
  string checks

## Packaging Notes

Current build helpers stage candidate `.var` packages:

- `fa_radar.free.0.1.11.var`
- `fa_radar.pro.0.1.11.var`

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
