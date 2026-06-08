# FA Radar Movie Studio Integration Notes 0.1.31

Updated: 2026-06-08

## Intent

Radar remains a standalone VaM compass product. The 0.1.31 direction is to keep
its runtime small, self-contained, and useful by itself while shaping the
scene-map foundation so a future video recorder or movie studio plugin can share
the same coordinate and marker model cleanly.

The integration goal is architectural compatibility, not a product merger. Radar
should continue to compile and deploy as its own DLL, with no hard dependency on
FAAR, movie studio tooling, Unity projects, repo-local assets, or scene-stored
recorder data.

## Product Split

Free is the plain radar: generated grid, user marker, selected target, and
available atoms as simple dots. Free should not become an authoring or filming
surface.

Pro is the semantic scene and filming layer. Pro can add category filters,
category colors, light radius and spotlight volume visuals, camera/frustum
helpers, richer marker semantics, and static-scene reference behavior that reads
like a usable scene map for lighting and filming.

The future movie studio product should consume or mirror the Pro scene-map model
when it needs richer context, but Radar should not inherit movie-studio UI,
timeline, recorder, export, or shot-management responsibilities.

## Reusable Core Boundaries

Keep the reusable foundation centered on pure scene-map concepts:

- A reference frame that can be HUD-relative, wrist-relative, atom-anchored, or
  static-world.
- A marker coordinate model that converts real atom positions into radar-local
  X/Y/Z, ground projection, height, range fade, and depth size cues.
- A generated visual pool for markers, stems, rings, grids, and Pro volumes.
- A category model for atoms that can support Free plain dots and Pro semantic
  lanes without forking the runtime.
- A small filming identifier path for generated Radar HUD objects, such as the
  existing `favr.hud.radar` naming convention.

The core should stay independent from recorder storage. Shared code should expose
state through in-memory transforms, names, and generated objects, not through raw
runtime file IO or repo-local JSON.

## Integration Direction For Video Recorder

The recorder/movie studio plugin should be able to locate Radar visuals and
understand their meaning without Radar importing recorder code. The current
name-based filming identifier is the right pattern: it gives recorder tooling a
stable discovery hook while keeping Radar standalone.

The current FAAR contract remains deliberately narrow:

- Radar visual identifier: `favr.hud.radar`
- FAAR state fields:
  - `radarHudFilmSubjectIdentifier=favr.hud.radar`
  - `radarHudVisible=true/false`
- FAAR consumes identifiers for visibility and recording behavior only.
- FAAR does not own Radar placement, anchoring, scene atom names, camera attach
  targets, scale, range, or transform authority.
- New generated Radar/studio visuals should continue to use stable reusable
  identifiers derived from `favr.hud.radar` so recorder visibility can locate
  them without adding a placement contract.

Static-scene reference mode is the most important bridge. In that mode, the
radar's own world pose becomes the map origin, visible scene items remain stable
on the map, and the user/camera can move as a marker through the represented
space. That makes the same foundation useful for:

- finding selected or available scene atoms during setup
- judging camera position against lights, people, CUAs, and props
- previewing a filming/lighting landscape without requiring a movie-studio UI
- future recorder overlays that can hide/show Radar by identifier

Recorder integration should remain one-way and optional. Radar may offer stable
generated object names, material names, visibility behavior, and coordinate
semantics. The recorder should own shot state, capture state, video output,
timeline concepts, and any recorder persistence.

## Director Mode Readiness

Future Director Mode is a Pro-scale interaction layer, not a 0.1.31 requirement.
The core should still avoid choices that would block it.

Useful readiness rules:

- Markers should retain identity back to their source atoms after projection.
- Static reference mode should preserve a clear mapping between radar-local
  movement and world-space movement.
- Marker grouping or clustering should keep member atom lists so later click,
  grab, drag, rotate, and multi-select behavior has a stable target set.
- Pro semantic categories should be able to render richer glyphs later, including
  primitive polygon people or simple shape language, without changing Free.
- Large-scale grab/drag/rotate of visible objects should be built as a later
  transform-authoring layer above the marker coordinate model, not as special
  cases inside the display code.

Director Mode should feel like manipulating a scene map. Radar 0.1.31 only needs
to keep the map honest.

## Near-Term Code Shape Rules

- Keep runtime code self-contained C# under `payload/Custom/Scripts`.
- Keep the deployable runtime artifact a compiled DLL under VaM
  `Custom\Plugins`.
- Do not introduce Unity projects, asset bundles, reflection, broad serializers,
  repo-local runtime dependencies, absolute development paths, or raw runtime
  file IO.
- Generated visuals should be created once, cached, pooled where appropriate,
  and updated through transforms, mesh refreshes, material changes, and active
  state diffs.
- Keep static-scene reference, marker projection, category filtering, and visual
  pooling as separable helpers so recorder/movie-studio reuse does not require
  copying HUD placement code.
- Keep Free and Pro compiled from one codebase with static edition gates.
- Keep Pro rich landscape work, including lighting and filming aids, layered on
  shared marker and reference-frame primitives.

## Non-Goals For 0.1.31

- No movie studio plugin implementation.
- No recorder dependency inside Radar.
- No Unity-authored Radar assets.
- No external/browser/companion UI.
- No timeline, shot list, capture workflow, or video export surface.
- No raw runtime file IO for movie-studio integration.
- No Director Mode grab/drag/rotate implementation.
- No primitive polygon people or custom glyph system yet.
- No conversion of Free into a semantic scene-management tool.
