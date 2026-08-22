# Decisions Log

Newest first. Each entry: **what** was decided, **why**, and the date. This is the
"defend it out loud" reference — the brief is explicit that every decision in the
submission needs a real justification behind it (`BRIEF.md` §1, §7).

---

### D5 — Lightweight `ai-context/` + a `SessionStart` hook, not the full pattern (2026-08-22)
**Choice:** Adapted the `ai-context/` convention from this developer's larger
projects (e.g. `menu-app`) down to three files — `README.md` (navigator),
`current-context.md` (living state), `decisions.md` (this file) — plus a Python
`SessionStart` hook (`.claude/hooks/session-start.py`) that prints branch + change
count + a pointer to `current-context.md` on startup/clear. No per-feature docs,
`planned/`/`todo/` split, or agent fleet.
**Why:** `menu-app`'s version is built for an ongoing multi-app product with
multiple contributors; this is a solo 3-task take-home. The trigger was
practical: a Unity MCP config change required restarting Claude Code, and a
written, living context doc plus an automatic orientation hook is what makes a
restart lose nothing. The hook uses Python (not `jq`, which isn't on this
machine's Git Bash `PATH`) and is skipped on `resume`/`compact` since those
already carry the conversation.

### D4 — Unity MCP via the official `com.unity.ai.assistant` package (2026-08-22)
**Choice:** Use Unity's first-party MCP integration (`com.unity.ai.assistant`,
installed via Package Manager, exposing a local relay Claude Code connects to)
rather than a third-party MCP-for-Unity project.
**Why:** It's shipped and maintained by Unity itself for Unity 6, needs no extra
runtime (no `uv`/Python toolchain), and is one less external dependency to explain
if asked how the project was built.
**Note:** `.mcp.json` (the Claude Code connection config) is git-ignored — it
hardcodes a machine-specific Windows path to the relay binary, so it doesn't belong
in the shared repo.

### D3 — Single GitHub repo, nested-project duplicate merged away (2026-08-22)
**Choice:** `github.com/dogukantaytuglu/softgames-task` (private) is the one and
only repo. When creating the Unity project, Unity Hub auto-initialized its own
nested `.git` inside a `softgames-task/softgames-task/` subfolder and pushed it to
a second, auto-named GitHub repo. Deleted the stray remote repo, removed the inner
`.git`, and flattened the project files up into the real repo root.
**Why:** Two repos for one project is confusing and risks history split across
both; only one was ever intended. Kept the Unity-generated `.gitignore` /
`.gitattributes` (Addressables/TestRunner ignores, Git LFS filters) over the
generic starter versions since they're more complete for a real Unity project.

### D2 — Assignment context lives under `ai-context/`, not the repo root (2026-08-22)
**Choice:** `BRIEF.md` and the original assignment PDF live in `ai-context/`
alongside `current-context.md` and this file, rather than at the repo root.
**Why:** Keeps the repo root clean (README + the actual Unity project), and matches
the convention used across this developer's other projects — one folder is where a
session looks for "what is this project and where do things stand."

### D1 — No self-imposed deadline (2026-08-22)
**Choice:** Dropped the originally-planned "Friday 4 September 2026" self-imposed
deadline from the brief and README. The project ships when it's done.
**Why:** SOFTGAMES explicitly gave no deadline ("all the time you need"); inventing
an external-pressure date wasn't requested and only risks rushing the polish pass
that the grading criteria (aesthetics, UX) actually reward.
