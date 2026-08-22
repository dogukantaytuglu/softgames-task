# ai-context — Navigator

**Purpose:** index of everything in `ai-context/` so a session (new or resumed) can get
oriented fast instead of re-deriving context from scratch. Read `current-context.md`
first, then branch out as needed.

> **Maintenance rule:** update `current-context.md` whenever the project state changes
> meaningfully (new package added, phase completed, MCP connected, etc.) and log any
> real decision in `decisions.md` when it's made — not at some later cleanup pass.
> A stale context file is worse than none, because it's actively misleading.

## Files

| File | What it is | Read when |
|---|---|---|
| `current-context.md` | Single living spine doc: what's built, current state, immediate next step | **Always, first** — onboarding or resuming |
| `decisions.md` | Decision log (newest first) — what was chosen and why | Before re-debating a choice; prepping to defend a decision out loud |
| `BRIEF.md` | The original self-contained assignment handoff brief (context, plan, grading criteria, definition of done) | Understanding the assignment itself, task-by-task guidance |
| `Softgames_-_Unity_Developer_Assignment.pdf` | Original assignment PDF, verbatim | Cross-checking the brief against the source |

## Why this exists (small but deliberate)

This project is a solo 3-task take-home, not a multi-app product — so this is
deliberately lighter than a full `ai-context/` setup (no per-feature docs, no
`planned/`/`todo/` split, no agent fleet). Two files carry the load:
`current-context.md` (state) and `decisions.md` (why). Both are living documents,
edited in place as things change — not dated session dumps that go stale.

A `SessionStart` hook (`.claude/settings.json`) prints a short orientation summary
(git status + a pointer to this folder) at the start of every session, so restarting
Claude Code — including the Unity MCP restart this was built for — doesn't lose the
thread.
