# FA Radar Agent Canon

This repo is the independent home for the Frame Angel Radar VaM plugin.

## Scope

- Build a small VaM MVRScript utility that points toward the currently selected
  atom.
- Keep this lane plugin-only until the operator explicitly opens Unity work.
- Do not touch `C:\projects\fap`, active theater/demo projects, controller
  assets, character/CUA pipelines, or older FrameAngel repos while working here.

## Runtime Rules

- Runtime code must be self-contained C# under `payload/Custom/Scripts`.
- Deployable runtime artifact is a compiled DLL under VaM `Custom\Plugins`,
  not a loose `.cs` file under VaM `Custom\Scripts`.
- Runtime code must not read or write files.
- Runtime code must not use reflection.
- Runtime code must not depend on repo-local JSON, Unity project assets, or
  absolute development-machine paths.
- Runtime visuals should be generated once, cached, and updated with lightweight
  transforms/material changes.

## Work Rules

- Read `C:\projects\PUNCHCARD.md` before starting meaningful work.
- Append punchcard start/finish/park entries for durable handoff context.
- Prefer static verifiers and small deploy helpers over hidden runtime debug
  plumbing.
- Keep documentation concise and repo-local.
## Operator Command Obedience Gate

Operator instructions are hard operating constraints, not preferences. Before
any repo read, edit, build, deploy, proof action, or explanation beyond a brief
status, classify the newest operator message into:

1. mode lock: read-only, hydrate-only, report-only, no edits, no deploy, pause,
   stop, park, use agents, no agents
2. active lane: the exact repo, product, seam, plugin surface, or file named by
   the operator
3. forbidden action: anything the operator said not to touch or not to do
4. proof requirement: exact evidence required before claiming success
5. stop line: condition that requires stopping instead of continuing

Rules:

1. The newest operator instruction wins over older chat, memory, plans,
   handoffs, receipts, and agent momentum.
2. A mode lock forbids adjacent helpful work. Read-only means no edits, no
   generated artifacts, no builds, no deploys, no cleanup, no branch changes,
   and no commits.
3. If corrected by the operator, stop the current plan immediately, acknowledge
   the exact disobedience in one sentence, and restart from the newest
   instruction only.
4. Do not narrate around a blocked or failed task. State the blocked invariant
   and the next allowed action.
5. Do not treat abundant docs as permission to rediscover. Use this repo's
   current authority surface, then act only inside the named lane.
