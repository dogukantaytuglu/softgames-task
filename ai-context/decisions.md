# Decisions Log

Newest first. Each entry: **what** was decided, **why**, and the date. This is the
"defend it out loud" reference — the brief is explicit that every decision in the
submission needs a real justification behind it (`BRIEF.md` §1, §7).

---

### D11 — Ace of Shadows: 2-stack drain, TimerUtil-driven cadence, Z-depth draw order (2026-08-23)
**Choice:** Exactly 2 stacks (Source drains into Target, one card/second, terminal
state = Target has all 144). `CardDeck` (domain) exposes `MoveNext()` +
`NotifyCardLanded()` as two independent entry points rather than a single
`Tick(deltaTime)` — a `TimerUtil.CountdownTimer(1s, loopCount: -1)` in the
presentation layer is the *only* thing deciding when to start a move, and each
card's own DOTween completion is the *only* thing deciding when to acknowledge a
landing. Card draw order comes from Z position (camera is Perspective, default
transparency sort mode already sorts by distance) rather than
`SpriteRenderer.sortingOrder`.
**Why:** 2 stacks is the only topology where "message when all animations
finished" has a well-defined terminal state — a cyclic/round-robin design never
finishes. Splitting trigger from acknowledgement avoids the classic bug of tying
completion to a timer tick instead of the actual animation, and avoids any need to
synchronize the two processes beyond one invariant (move duration < tick
interval). Z-depth removes a redundant, hand-maintained sorting-order calculation
that the camera/renderer would already get right on its own, and adds a free,
genuine (not faked) subtle depth cue as a side effect.

### D10 — Avoid uVegas's Front/Back card sprites (watermarked) (2026-08-23)
**Choice:** `Assets/uVegas` (a full card-game asset framework with themes,
rank/suit glyphs, and a `UICard` component) is in the project, but its base
`Front.png`/`Back.png` card shape sprites are not used for Ace of Shadows'
card visual — went with a plain sprite from `Assets/Art/Sprites` instead.
**Why:** Close-up in-engine capture confirmed a "uVegas" watermark baked directly
into those two textures (invisible in a flattened/downscaled preview, clearly
visible rendered at actual scale) — almost certainly a trial/demo limitation.
Shipping that in a job-application submission would read as an unlicensed asset,
regardless of whether it's a "buy to remove" gate or permanent required
attribution.

### D9 — Rider (free non-commercial license) over VS Code for coding (2026-08-23)
**Choice:** Switched the actual coding IDE from VS Code (+ C# Dev Kit) to Rider,
using JetBrains' free non-commercial license (changed policy since 2024 — covers
personal/portfolio projects like this one, no payment needed).
**Why:** VS Code's C# Dev Kit became unreliable once the project grew past a
couple of asmdefs — stale diagnostics, false "unused import" warnings, and the
project model not refreshing after asmdef changes even after reloading the
window. Root-caused as a real tooling gap, not user error, before switching.

### D8 — Manual "Build & Deploy" Editor tool instead of CI (2026-08-22/23)
**Choice:** `Assets/Editor/DeployWebGL.cs` (Build → Build & Deploy WebGL menu
item) builds WebGL straight into the sibling `softgames-task-build` repo folder
and git add/commit/pushes it, all in one click. No GitHub Actions workflow.
**Why:** Two CI paths were attempted and both hit real, non-trivial walls: (1)
GameCI's cloud-hosted actions can't activate a Unity Personal license anymore —
Unity killed manual/offline activation for Personal specifically (Plus/Pro only,
needs a serial), and `unity-builder`/`unity-test-runner` require an actual license
file or serial, not just email/password despite what the docs implied. (2) A
self-hosted runner on this same machine sidesteps the license problem (reuses the
already-activated local Unity) but hit its own wall: this Windows account is
Microsoft-account-linked, and Windows service logon doesn't reliably authenticate
those accounts (Error 1069) even with "Log on as a service" rights correctly
granted — a known class of limitation, not a config mistake. Running the runner
interactively (not as a service) worked but requires remembering to keep a
terminal window open, and still hit an unrelated PowerShell execution-policy
block and a stale-ownership git error along the way. Given the actual ceiling is
"one developer, clicks a button before submitting," the one-click manual tool
gets the identical practical outcome (tests run, WebGL builds, deploys live) for
a fraction of the ongoing maintenance cost.
**Also decided along the way:** WebGL Compression Format must be **Brotli with
Decompression Fallback enabled**, not Compression Disabled — GitHub Pages can't
send a `Content-Encoding: br` header, but Decompression Fallback embeds a
client-side JS decoder so it loads correctly anyway, keeping the ~13MB transfer
size instead of ~60MB uncompressed.

### D7 — Feature folders split into Logic/Monobehaviour asmdefs (2026-08-22/23)
**Choice:** Every feature (`FpsCounter`, `AceOfShadows`, `MainMenu`) splits its
`Scripts/` into `Logic/` (own asmdef, `noEngineReferences: true`) and
`Monobehaviour/` (own asmdef referencing Logic by name), rather than one shared
assembly or no asmdefs at all.
**Why:** Makes "logic stays out of MonoBehaviours" a compiler-enforced guarantee
instead of just a convention someone could drift from — Logic code that tried to
`using UnityEngine` simply wouldn't compile. Costs a bit more asmdef bookkeeping
per feature in exchange.

### D6 — TimerUtil vendored, not installed as a package (2026-08-23)
**Choice:** `dogukantaytuglu/TimerUtil` (the developer's own reusable timer
library — `CountdownTimer`, `TimerService`, fluent extension methods) is copied
directly into `Assets/Plugins/TimerUtil`, same treatment as DOTween, rather than
referenced as a git-URL UPM package in `Packages/manifest.json`.
**Why:** That repo has no `.meta` files committed. Unity's Package Manager
silently fails to import packages without them (no error — the package just never
compiles), which cost real time to root-cause. If `.meta` files are ever added to
that repo, switching to a real package reference is a trivial follow-up.

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
