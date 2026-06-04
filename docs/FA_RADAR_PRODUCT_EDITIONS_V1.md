# FA Radar Product Editions V1

Updated: 2026-06-04

## Decision

Free and Pro are compiled from one codebase. Edition differences are controlled
by build gates, not by maintaining separate runtime forks.

The current deployed prototype remains `fa_radar.0.1.9.dll`. This document is a
product-contract slice for the future Free/Pro build shape.

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
- available in-game control UI using Frame Angel's own switch controls
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

Expected future build symbols:

- `FA_RADAR_FREE`
- `FA_RADAR_PRO`

Rules:

- shared radar placement, grid, marker, click-selection, and generated-material
  foundation remains common code
- Free builds exclude Pro-only controls and Pro-only generated visuals
- Pro builds include filters, in-game controls, color customization, and light
  volume visuals
- no runtime file IO, reflection, repo-local JSON dependency, or absolute
  development paths are introduced for edition gating
- edition gates should be static compile/package gates, not fragile runtime
  string checks

## Packaging Notes

Future `.var` product naming remains undecided. Current candidates:

- `FrameAngel.DaFuqIzzit.1.var`
- `FrameAngel.Radar.1.var`

The package name decision is separate from the compile-gate architecture.

## Parked

- exact Free DLL/package filename
- exact Pro DLL/package filename
- license or entitlement mechanism
- final in-game Pro switch UI implementation
- final people classification source for women/men color defaults
