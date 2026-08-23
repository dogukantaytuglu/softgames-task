# Decisions Log

Newest first. Each entry: **what** was decided, **why**, and the date. This is the
"defend it out loud" reference — the brief is explicit that every decision in the
submission needs a real justification behind it (`BRIEF.md` §1, §7).

---

### D15 — Single init point per feature, no generic IInitializable/reflection (2026-08-23)
**Choice:** `AceOfShadowsController` and `SceneFlowController` each merge a former
`Awake()`+`Start()` split into one `Awake()` — both already held every reference their
feature needed, so nothing was actually gated on Unity's Awake-before-Start ordering
across objects. `MainMenuScene` (3 independent `MenuButtonSceneLoader` buttons, no
shared owner) gets one small new `MainMenuInitializer` on its `Canvas` root, which
`GetComponentsInChildren`s for loaders and calls a plain `public void Initialize()`
on each. `FinishedMessageView`'s `Awake()` became the same kind of plain `Initialize()`,
called explicitly by `AceOfShadowsController` — matching the pattern
`StackCounterView.Bind()` already used. `HomeButtonController` and
`FpsCountUIController` were deliberately left alone (see below).

The first version of this (a shared `IInitializable` interface + a new
`Initialization` assembly + a generic scene-wide reflection scan) was proposed, then
explicitly rejected by the developer as too much machinery — no new assembly, no new
interface, no reflection in the final version.
**Why:** Requested directly by the developer: get rid of scattered Awake/Start calls,
give each feature one initialization point. `HomeButtonController` (SceneFlow's
back-to-home widget, instanced into `AceOfShadowsScene`/`MagicWordsScene`/
`PhoenixFlameScene`) and `FpsCountUIController` (a separate feature sharing `AppScene`
with `SceneFlowController`) were kept as their own tiny self-initializing `Awake()`s
rather than folded into another feature's initializer, specifically to avoid
reintroducing a cross-feature asmdef reference that was deliberately avoided when
`HomeButton.prefab` was kept inside `SceneFlow`'s own folder (see D13) — both were
already single-method/single-purpose, so there was nothing to actually gain by
centralizing them elsewhere at the cost of that coupling.

### D14 — All scene files got a `Scene` suffix (2026-08-23)
**Choice:** Renamed every scene: `Shell.unity` → `AppScene.unity`,
`MainMenu.unity` → `MainMenuScene.unity`, `AceOfShadows.unity` →
`AceOfShadowsScene.unity`, `MagicWords.unity` → `MagicWordsScene.unity`,
`PhoenixFlame.unity` → `PhoenixFlameScene.unity`. Done via `AssetDatabase.RenameAsset`
(preserves GUIDs), with Build Settings, `SceneNames` (renamed `Shell` → `App` as a
const identifier), the `EditorSceneBootstrap` method, and every serialized
`sceneName`/`homeSceneName` field (`MainMenuButton.prefab`'s default plus two
per-instance overrides in `MainMenuScene.unity`, `SceneFlowController`'s field in
`AppScene.unity`) updated to match and re-verified through Unity's own APIs, not
just text search.
**Why:** Developer request, specifically to rename `Shell` → `App` for
consistency with the `Assets/App/` folder (D13) that now holds the SceneFlow/
FpsCounter infra — but `App` alone would then mean two different things in
conversation (the folder vs. the scene), so `AppScene` was chosen instead, and the
same `Scene` suffix was applied to the other four scenes for consistency rather
than leaving just one scene oddly named out of the group.

### D13 — Acted on the `unity-architect` review of the SceneFlow work (2026-08-23)
**Choice:** Ran the `unity-architect` subagent (see current-context.md, Tooling) —
its first confirmed-working invocation — against the AppScene/SceneFlow work from
D12, then implemented its full findings list:
- **P0 (build-breaking):** `AppScene.unity` was never actually added to
  `EditorBuildSettings.scenes` despite D12/current-context.md claiming it was —
  added at index 0. A WebGL build made before this fix would have booted MainMenu
  directly with no `SceneFlowController`, NRE'd on the first button click, and
  never shown the FPS counter.
- **P1 (real bugs the nav loop introduced):** `Timer.Stop()` never unregistered
  from `TimerService`, leaking a full `AceOfShadowsController` graph (144 cards)
  on every MainMenu→AceOfShadows→home cycle — fixed in `TimerUtil` itself (`Stop()`
  unregisters, `Start()` re-registers) since any future timer-using feature would
  hit the same trap. `SceneFlowController.Navigate` had no in-flight guard —
  double-clicking two menu buttons quickly desynced `SceneFlowState` from what was
  actually loaded — fixed by moving the guard into `SceneFlowState` itself
  (`TryBeginNavigation`/`CompleteNavigation`, 2 new EditMode tests) rather than
  patching the symptom in the Monobehaviour. `SceneManager.SetActiveScene` fired
  from the load operation's `.completed`, which runs *after* the new scene's own
  `Start()` — switched to the `SceneManager.sceneLoaded` event instead, which
  Unity fires after `Awake` but before `Start`, closing a landmine for
  root-`Instantiate`d objects in Magic Words/Phoenix Flame. Unload-before-load left
  a frame with zero cameras (AppScene has none by design) — added a `FallbackCamera`
  to AppScene (Solid Color clear, culling mask `Nothing`, depth `-100`) rather than
  reordering to load-then-unload, which would have reintroduced the
  two-cameras/two-EventSystems problem D12 was designed to avoid.
- **P2/P3 (structure and polish):** `Assets/Feature/SceneFlow` and
  `Assets/Feature/FpsCounter` moved to `Assets/App/` — infra every scene depends on
  is now visually distinct from independent, deletable content features
  (`MainMenu`/`AceOfShadows`/`MagicWords`/`PhoenixFlame`, still under
  `Assets/Feature/`). Added an Editor-only `EditorSceneBootstrap`
  (`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`, `#if UNITY_EDITOR`) that
  additively loads AppScene when a content scene is opened and Play is hit directly —
  removes friction that would otherwise be paid twice more building Magic Words and
  Phoenix Flame. `BackButtonController`/`BackButton.prefab` renamed to
  `HomeButtonController`/`HomeButton.prefab` (it always goes home, there's no back
  stack) via `AssetDatabase.RenameAsset` to preserve GUIDs/prefab references. Added
  a `SceneNames` const-string class in `SceneFlow.Logic`. Gave AppScene's persistent
  Canvas an explicit `sortingOrder: 100` so it's deterministically drawn above
  whatever content scene's Overlay canvas is loaded alongside it (both were at 0,
  an unstable tie). Fixed `CardView.MoveTo`'s DOTween `Sequence` not being killable
  by `transform.DOKill()` (`.SetTarget(transform)` + an `OnDestroy` kill). Cached
  `TimerService.AllTimers` as a `ReadOnlyCollection` instead of allocating one on
  every call. Moved FpsCounter's tests from `Scripts/Tests/` into
  `Assets/Tests/EditMode/FpsCounter/` to actually match the stated test-location
  convention instead of just claiming it.
**Why:** The review caught a genuine build-breaking bug that manual Play Mode
testing in the Editor couldn't have caught (opening `AppScene.unity` directly bypasses
Build Settings entirely), plus several real async/lifecycle bugs the new
additive-loading design introduced. Fixing all of it now — before Magic Words and
Phoenix Flame get built on top of this same pattern — is cheaper than fixing it
after two more features have copied the same landmines. All moves/renames went
through Unity's `AssetDatabase` API (via Unity MCP `Unity_RunCommand`), not raw
filesystem operations, specifically to preserve GUIDs and keep every existing
prefab/scene reference intact. 35/35 EditMode tests pass after the pass (up from
33 — the new in-flight-guard tests); the previously-verified live Play Mode loop
predates this hardening and needs re-verification (left as an explicit next step,
Play Mode testing being the developer's own call per the Tooling note above).

### D12 — Persistent AppScene scene + additive scene-flow, instead of MainMenu as the boot scene (2026-08-23)
**Choice:** New `AppScene.unity` is now build-index 0 and the *only* scene ever opened
directly — it holds the FPS counter (moved out of AceOfShadows) and a
`SceneFlowController` singleton, and is never itself unloaded. Every other scene
(MainMenu, AceOfShadows, MagicWords, PhoenixFlame) loads additively on top of it,
exactly one "content" scene at a time — `SceneFlowController.Navigate()` unloads the
previous content scene before additively loading the next and calling
`SceneManager.SetActiveScene`. `SceneFlowState` (pure, EditMode-tested) owns the
"is this actually a different scene" guard; the Monobehaviour layer just does the
unload/load/activate. Back buttons (`BackButton.prefab`, `buttons_41` home-icon
sprite) were added to AceOfShadows/MagicWords/PhoenixFlame, calling
`SceneFlowController.Instance.NavigateHome()`; MagicWords/PhoenixFlame needed a
Canvas + `EventSystem` added since they were empty placeholders before this.
**Why:** Requested directly by the developer (build a shared top nav bar holding the
FPS counter, with every scene opening additively on top of it, plus back buttons on
the project scenes) — an explicit ask to implement app-level code himself this time,
not just infra (see the collaboration note below). Keeping AppScene camera-less and
light-less (a Screen Space Overlay Canvas doesn't need a camera to render) and never
loading more than one content scene alongside it is what keeps cameras/lights/
EventSystems from ever duplicating, without needing any extra cleanup logic — each
content scene keeps owning its own Main Camera/Directional Light/EventSystem exactly
as before, and only one set is ever live. This also naturally centralizes the FPS
counter instead of duplicating it per scene, which was the "add FPS counter to every
scene" next-step from the previous session.

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
