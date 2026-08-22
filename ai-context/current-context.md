# softgames-task — Current Context

> 👉 New here (or resuming)? This is the single source of truth for "where things
> stand." Read this, then `decisions.md` if you need the "why" behind something.
> The assignment itself is `BRIEF.md`.

Last updated: 2026-08-22 (post ai-context/hook setup, pre-restart).

## What we're building

The SOFTGAMES Unity Developer take-home assignment: three self-contained demos
(Ace of Shadows, Magic Words, Phoenix Flame) reachable from an in-game menu, in
Unity 6, responsive on mobile + desktop, FPS counter top-left, built for WebGL and
hosted at a public link. Full detail, grading criteria, and task-by-task guidance:
`BRIEF.md`. No deadline — self-imposed or otherwise (see `decisions.md`).

## Current state

- **Repo:** `github.com/dogukantaytuglu/softgames-task` (private), single repo,
  `master` branch, tracking `origin/master`. (An earlier duplicate — Unity Hub had
  auto-created a nested project folder with its own `.git` and pushed to a stray
  GitHub repo — was merged/cleaned up; see `decisions.md`.)
- **Unity project:** Unity 6000.0.82f1, default URP 3D template, **not yet
  customized** — still has the sample scene and TutorialInfo readme from project
  creation. None of the three tasks have been started.
- **Unity MCP:** `com.unity.ai.assistant` (2.18.0-pre.2) installed and **committed**
  (`Packages/manifest.json` + `ProjectSettings/Packages/com.unity.ai.assistant/`).
  Unity Bridge running, relay binary confirmed at
  `C:\Users\doguk\.unity\relay\relay_win.exe`. `.mcp.json` is staged at the repo
  root (git-ignored — machine-specific path) pointing Claude Code at that relay.
  **Not yet connected** — this session hasn't loaded `.mcp.json` yet because it
  started before the file existed. A restart is pending (the reason this
  ai-context system + the SessionStart hook below got built) — after it: Accept
  the pending connection in Unity's Edit → Project Settings → AI → Unity MCP
  panel, then verify MCP tools are available (e.g. `Unity_ManageScene`,
  `Unity_ReadConsole`).
- **ai-context system:** this folder (navigator + this file + `decisions.md`) and
  a `SessionStart` hook (`.claude/hooks/session-start.py`, wired via
  `.claude/settings.json`) are built, pipe-tested, and committed. The hook prints
  a short orientation on startup/clear (skipped on resume/compact) — branch,
  change count, pointer to this file. See D5 in `decisions.md`.
- **Tooling confirmed working:** `gh` CLI authenticated as `dogukantaytuglu`; Unity
  CLI batchmode confirmed functional (license resolves, can create projects, and by
  extension can run tests / trigger builds headlessly via `-executeMethod`).

## Immediate next step

1. **Restart Claude Code** (pending — this is the next action) so `.mcp.json`
   loads and the `SessionStart` hook fires.
2. Accept the pending Unity MCP connection in the Editor (Edit → Project
   Settings → AI → Unity MCP).
3. Verify MCP tools are available (e.g. `Unity_ManageScene`, `Unity_ReadConsole`).
4. Then start **Phase 0** per `BRIEF.md` §4: menu/scene routing to three empty task
   screens, FPS counter top-left, responsive canvas (touch + mouse), WebGL build
   deployed live (verify on a real phone + desktop browser), and the timeboxed
   emoji-in-TextMeshPro spike. Nothing task-specific (cards/dialogue/fire) should
   start before Phase 0 is done.

## Conventions (binding for all three tasks)

- **Logic stays out of MonoBehaviours** — deck model, dialogue parser, colour state
  machine as plain testable C# classes; MonoBehaviours only bind to the scene.
  This is simultaneously the architecture answer and the testability answer.
- **Unit tests via Unity Test Framework, EditMode**, written alongside the code
  they cover — not retrofitted.
- Every decision must be defensible out loud in a follow-up conversation — don't
  introduce a pattern or dependency that can't be explained. Record real decisions
  in `decisions.md` as they're made.

## How to update this file

Edit in place whenever the state changes meaningfully — a phase completes, a task
is built, a tool gets connected. This file describes *now*, not history; history
that matters (the "why") goes in `decisions.md` instead. Don't let this drift —
a stale current-context.md is worse than none.
