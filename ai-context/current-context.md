# softgames-task — Current Context

> 👉 New here (or resuming)? This is the single source of truth for "where things
> stand." Read this, then `decisions.md` if you need the "why" behind something.
> The assignment itself is `BRIEF.md`.

Last updated: 2026-08-23 (end of the `unity-architect` review + fix pass on the
Shell/scene-flow work).

## What we're building

The SOFTGAMES Unity Developer take-home assignment: three self-contained demos
(Ace of Shadows, Magic Words, Phoenix Flame) reachable from an in-game menu, in
Unity 6, responsive on mobile + desktop, FPS counter top-left, built for WebGL and
hosted at a public link. Full detail, grading criteria, and task-by-task guidance:
`BRIEF.md`. No deadline — self-imposed or otherwise (see `decisions.md`, D1).

## Current state

### Task progress
- **Ace of Shadows: built and working.** 144-card deck drain between two stacks,
  Source→Target, one move per second, message on completion. See "Ace of Shadows
  architecture" below for detail.
- **Magic Words: not started.** `Assets/Scenes/MagicWords.unity` exists as an empty
  placeholder (default camera + light only), registered in Build Settings, reachable
  from the main menu's "Magic Words" button. No script work yet.
- **Phoenix Flame: not started.** Same as Magic Words — empty placeholder scene,
  wired into the menu, nothing built.
- **Phase 0 (menu/FPS/responsive/WebGL) is done**, see below.
- **Scene-flow/navigation shell built.** `Shell.unity` is now build-index 0, the sole
  persistent scene, holding the FPS counter (moved out of AceOfShadows) and a
  `SceneFlowController` singleton. Every other scene (MainMenu, AceOfShadows, MagicWords,
  PhoenixFlame) loads additively on top of it, one at a time. See "Scene-flow architecture"
  below for detail. **Reviewed by the `unity-architect` subagent** (2026-08-23, first
  confirmed-working run — see Tooling below) and hardened based on its findings: see D13.

### Tooling
- **Unity 6000.0.82f1**, Unity MCP (`com.unity.ai.assistant`) — reconnected and used
  this session for scene/prefab building, compile checks, EditMode test runs, and
  Play Mode verification (scene-flow nav loop). Its tools occasionally report "Unity
  not detected" transiently — retrying once always resolved it. Live Play Mode
  testing via MCP is token-expensive (each check is a full RunCommand round-trip) —
  the developer prefers to do Play Mode testing themselves going forward; default to
  static checks (compile, EditMode tests, reading scene YAML) and let them drive
  Play Mode.
- **`unity-architect` subagent** (global, `~/.claude/agents/unity-architect.md`,
  not project-specific) — created 2026-08-23 for Unity architecture design/review:
  readability + scalability first, sized honestly to actual project scale, encodes
  the developer's stated preferences (feature-based foldering with asmdef
  independence, bootstrapper/service-initializer over scattered cross-feature
  `Awake()`/`Start()`, facade + focused-controller composition, `OnValidate()`
  self-wiring) as defaults it can push back on, not rules. Reads a repo's
  `ai-context/`/`CLAUDE.md` first and defers to already-established real
  conventions. **Confirmed working** (2026-08-23, fresh session) — ran a real
  architecture review of the SceneFlow feature, found a build-breaking bug (Shell
  missing from Build Settings) plus several real async/lifecycle bugs; findings
  were acted on, see D13.
- **IDE: Rider, not VS Code.** Rider is free for non-commercial use (JetBrains
  changed this in 2024) — activate via the "Free non-commercial license" option in
  Rider's license dialog, no payment needed for this take-home. VS Code + C# Dev
  Kit was tried first but had real reliability problems on this project (stale
  diagnostics, false "unused import" warnings, project model not refreshing after
  asmdef changes) once the project grew past a couple of asmdefs — see D9.
- **`.editorconfig`** at repo root: Allman braces (opening brace on its own line),
  space after `if`/`for`/`while` before the paren, 4-space indent. Read by Rider,
  VS, and VS Code's C# formatter alike.
- **Repo:** `github.com/dogukantaytuglu/softgames-task` (private), `master` branch.

### Project conventions (established this session, apply going forward)
- **`Assets/App/` vs `Assets/Feature/`**: app-level infrastructure that every scene
  depends on (`SceneFlow`, `FpsCounter`) lives under `Assets/App/`; independent,
  deletable content features (`MainMenu`, `AceOfShadows`, `MagicWords`,
  `PhoenixFlame`) live under `Assets/Feature/`. Split out 2026-08-23 (D13) — before
  this both kinds of thing sat in `Assets/Feature/` together, which stopped being
  legible once SceneFlow (infra) landed alongside AceOfShadows (content). Answers
  "which of these can I delete without breaking the app" from the tree alone.
- **Every feature's scripts split into `Scripts/Logic/` and `Scripts/Monobehaviour/`
  subfolders**, each its own asmdef, named `<Feature>.Logic` / `<Feature>.Monobehaviour`
  (e.g. `AceOfShadows.Logic`, `FpsCounter.Monobehaviour`). `Logic` asmdefs have
  **`noEngineReferences: true`** — a compile-time guarantee that domain logic can't
  reach into UnityEngine, not just a convention. `Monobehaviour` asmdefs reference
  the matching `Logic` asmdef by name.
  ⚠️ **Gotcha hit twice this session:** a `.cs` file sitting in the parent `Scripts/`
  folder (outside both subfolders) silently compiles into the default assembly
  instead of either asmdef — no error until something tries to reference it. Always
  double-check new files actually land inside `Logic/` or `Monobehaviour/`.
  ⚠️ **Also:** exactly one asmdef per folder — Unity errors if two exist side by
  side, even with different names.
  **Exception: `MainMenu`** stays flat (`MainMenu.asmdef` at the feature root, no
  Logic/Monobehaviour split) — it's pure wiring (`MenuButtonSceneLoader`), nothing
  to unit-test, so the split would be ceremony with no payoff. Deliberate, not
  drift.
- **Tests:** `Assets/Tests/EditMode/<Feature>/`, own `<Feature>.Tests.asmdef`
  referencing the feature's Logic asmdef (plus Monobehaviour too, if anything in
  there is actually unit-tested — usually nothing is, since Monobehaviours in this
  project are thin wiring), `includePlatforms: ["Editor"]`,
  `precompiledReferences: ["nunit.framework.dll"]`,
  `defineConstraints: ["UNITY_INCLUDE_TESTS"]`. 35 EditMode tests total right now
  (5 FpsCounter, 22 Ace of Shadows, 8 SceneFlow), all passing. FpsCounter's tests
  moved here from `Assets/App/FpsCounter/Scripts/Tests/` (2026-08-23, D13) to
  actually match this convention instead of just stating it.
- **In Unity MCP `Unity_RunCommand` scripts specifically** (not normal project
  code): bare `Image` and `CodeEditor` resolve to the wrong thing (some other
  namespace collides) — always fully-qualify as `UnityEngine.UI.Image` /
  `Unity.CodeEditor.CodeEditor` inside those scripts only.
- **In Unity MCP `Unity_RunCommand` scripts, `Debug.Log`/`GetConsoleLogs` is
  unreliable for anything async** (a `TestRunnerApi` callback, an `EditorApplication.update`-driven
  coroutine) — `GetConsoleLogs` appeared to return a stale/cached snapshot that never
  picked up new logs during this session. The reliable pattern: write results to a
  file (e.g. under the project's `Temp/`) from the async callback, then `Read` that
  file directly in a follow-up step. Also, `TestRunnerApi` needs to be kept alive
  (a static field reference) or its callback silently never fires (GC'd mid-run).
- **`AssetDatabase.DeleteAsset` from a `Unity_RunCommand` script fails outright**
  ("User interactions are not supported for MCP tool calls") — this Editor isn't
  running with `-automated`, so whatever confirmation Unity tries to show for a
  scripted delete blocks and the MCP relay refuses the call rather than hang.
  `AssetDatabase.MoveAsset`/`RenameAsset`/`CreateFolder` all work fine from
  scripts; only deletion is affected. Workaround: delete the file(s) and their
  `.meta` directly via a normal shell command instead, then call
  `AssetDatabase.Refresh()` from a follow-up `Unity_RunCommand` — safe here only
  because the folder was already empty (no GUIDs to lose).
- **A `Unity_RunCommand` script with more than one top-level type, where a
  non-`CommandScript` type is declared `private`/nested, sometimes fails to
  compile** — the tool's own auto-formatter (visible in the error's
  `localFixedCode`) mis-wraps it, duplicating the class both nested inside
  `CommandScript` and again at namespace scope (illegal for a `private` type).
  Fix: declare every extra type as a separate top-level `internal class`, not
  nested inside `CommandScript`.

### Ace of Shadows architecture
- **Domain** (`Assets/Feature/AceOfShadows/Scripts/Logic/`): `Card`, `CardStack`
  (LIFO, `CountChanged` event), `CardMove`, `CardDeck`. `CardDeck` is deliberately
  **timer-agnostic** — `MoveNext()` triggers one move (called externally, once per
  "it's time" tick), `NotifyCardLanded()` is called by the presentation layer from
  each card's actual tween completion, and `AllAnimationsFinished` fires exactly
  once when the *last card lands*, not when the last move was merely triggered.
  This split is what makes the domain fully EditMode-testable with no real-time
  waiting (`deck.MoveNext()` called 144 times in a loop replaces "wait 144
  seconds").
- **Presentation** (`Scripts/Monobehaviour/`): `CardView` (DOTween move + rotate,
  `Assets/Plugins/DOTween`), `CardStackLayout` (pure math — each card's Y-fan-offset
  and Z-depth are computed once at placement time from its distance-from-bottom in
  whatever stack it just joined, then never recomputed as more cards join on top),
  `AceOfShadowsController` (composition root), `StackCounterView`,
  `FinishedMessageView`.
- **Cadence:** `Assets/Plugins/TimerUtil` (vendored, see below) drives a
  `CountdownTimer(1s, loopCount: -1)` as the *sole* trigger for "start next move" —
  it's fully decoupled from animation completion. The only invariant that keeps
  this race-free: card move duration (0.35s) must stay under the 1s tick interval,
  so a card always finishes landing before the next one starts.
- **Draw order comes from Z position, not `SpriteRenderer.sortingOrder`.** The
  scene's camera is Perspective (not orthographic) with default transparency sort
  mode, which already sorts by camera distance — explicit sortingOrder was
  redundant. Each card gets a unique Z offset (uncapped, unlike the Y fan which
  caps at 12 cards deep for the visual) so draw order stays well-defined even among
  visually-overlapping cards.
- **Random ±6° rotation per card**, assigned once at placement (not deterministic,
  not unit-tested — intentional one-off visual jitter). Target-stack cards get an
  additional 180° Y rotation as a visual "flip" distinguishing landed cards.
  Combined effect: stacks read as a messy pile of individual cards, not a smooth
  block.
- **Hierarchy:** `SourceStack` / `TargetStack` are real parent `Transform`s (not
  just position anchors) — cards `SetParent` onto them on move, using
  `worldPositionStays: true` (the default) specifically so the reparent itself
  doesn't cause a visual snap; the subsequent DOTween move then animates smoothly
  from wherever the card actually is.
- **Card visual:** `popups_11` sprite from `Assets/Art/Sprites/popups.png` (a
  generic 2D UI asset pack, teal-bordered cream card-shaped panel). **Not**
  `Assets/uVegas`'s card sprites — see decisions D10.

### Scene-flow architecture
- **`Shell.unity` is build-index 0** and the only scene ever opened directly (in
  Editor Play or in the WebGL build) — every other scene is loaded/unloaded
  *additively* on top of it, one "content" scene at a time. Shell itself never
  reloads or unloads. Lives at `Assets/App/SceneFlow/` (not `Assets/Feature/` —
  see the `Assets/App/` vs `Assets/Feature/` convention above).
- **`SceneFlow.Logic`** (`Assets/App/SceneFlow/Scripts/Logic/`, `noEngineReferences:
  true`): `SceneFlowState` — pure state machine holding `CurrentScene`,
  `IsTransitioning`, and `TryBeginNavigation(target, out previousScene)` /
  `CompleteNavigation()`. `TryBeginNavigation` no-ops (returns false) if
  `target` is null/empty/already-current, **or if a navigation is already
  in-flight** — this is the fix for a real bug the `unity-architect` review caught
  (2026-08-23, D13): double-clicking two menu buttons quickly used to desync the
  state machine from what was actually loaded. `SceneNames` — a plain const-string
  class (`Shell`/`MainMenu`/`AceOfShadows`/`MagicWords`/`PhoenixFlame`) — is the
  single source of truth for scene name strings in code (per-instance prefab
  `sceneName` fields are still hand-typed strings; a `SceneAsset`-backed wrapper
  would be overkill at this scale). 8 EditMode tests total.
- **`SceneFlow.Monobehaviour`**: `SceneFlowController` — a plain static-instance
  singleton (not `DontDestroyOnLoad`; unnecessary since it lives in Shell, which is
  never unloaded) living on a `SceneFlowController` GameObject in `Shell.unity`.
  `Start()` adopts whatever scene is already active (via `SceneFlowState`'s
  constructor) if one other than Shell is already loaded — this is what makes the
  Editor bootstrap below work — otherwise calls `Navigate(homeSceneName)` (default
  `SceneNames.MainMenu`) to boot the first content scene. `Navigate(sceneName)`
  unloads the previous content scene (`SceneManager.UnloadSceneAsync`), additively
  loads the target (`LoadSceneAsync(..., LoadSceneMode.Additive)`); if the load
  operation comes back `null` (scene not in Build Settings — a silent Unity API
  failure otherwise), it logs an error and releases the in-flight guard instead of
  leaving navigation permanently bricked. On success, `SceneManager.sceneLoaded`
  (not the load operation's `.completed`) is what triggers `SetActiveScene` +
  `CompleteNavigation` — Unity fires `sceneLoaded` after the new scene's `Awake`
  calls but *before* its `Start` calls, which is what makes it safe for the new
  scene's own `Start()` to root-`Instantiate` things without them silently landing
  in Shell. `HomeButtonController` (was `BackButtonController` — renamed 2026-08-23,
  D13, since there's no back-stack, it always goes home) calls
  `Instance.NavigateHome()`, guarded against a null `Instance`.
- **Exactly one content scene is ever loaded alongside Shell**, which is what keeps
  EventSystems from ever duplicating — Shell itself holds no Light and no
  EventSystem (only a Screen Space Overlay Canvas, which doesn't need a camera to
  render, at `m_SortingOrder: 100` so it's deterministically drawn above whatever
  content scene's own Overlay canvas is loaded alongside it). Each content scene
  keeps its own Main Camera, Directional Light, and `EventSystem`
  (`InputSystemUIInputModule`) exactly as before; only one is ever active at a time
  since Shell fully unloads the previous content scene before loading the next.
  **Shell does hold one Camera**, unlike the original design: a `FallbackCamera`
  (Solid Color clear, culling mask `Nothing`, depth `-100`, no AudioListener) added
  2026-08-23 (D13) — closes a real gap where unload-then-load left a frame (worse
  on WebGL) with zero cameras rendering.
- **Editor-only bootstrap** (`EditorSceneBootstrap`, `#if UNITY_EDITOR`,
  `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`, added 2026-08-23, D13):
  additively loads `Shell` if it isn't already loaded and no `SceneFlowController`
  exists, *unless* the scene about to play is Shell itself. This is what makes
  opening `MagicWords.unity`/`PhoenixFlame.unity`/`AceOfShadows.unity` directly and
  hitting Play still work (FPS counter visible, back-to-home button functional)
  without the friction of having to open Shell first every time — important now
  that Magic Words and Phoenix Flame are about to be built and Play will get hit
  from those scenes a lot. Compiles out entirely in real builds.
- **`MenuButtonSceneLoader`** (MainMenu) calls `SceneFlowController.Instance.Navigate(sceneName)`
  instead of `SceneManager.LoadScene` directly, guarded against a null `Instance`.
- **Back-to-home buttons**: `Assets/App/SceneFlow/Prefabs/HomeButton.prefab`
  (renamed from `BackButton.prefab`, D13) — top-right anchored, `buttons_41` sprite
  (green "home" icon from `Assets/Art/Sprites/buttons.png`, chosen since it reads
  as "return to main menu" and needs no extra label). Instanced into
  `AceOfShadows.unity`, `MagicWords.unity`, `PhoenixFlame.unity` (not MainMenu —
  that's home, nothing to go back to). `MagicWords`/`PhoenixFlame` were empty
  placeholders with no Canvas/EventSystem before this session started — both were
  added (Screen Space Overlay, same `CanvasScaler` settings as MainMenu/Shell,
  `EventSystem` duplicated from AceOfShadows' so the Input-System action-asset
  wiring matches exactly).
- **Full navigate/back loop verified live in Play Mode** via Unity MCP (Shell→MainMenu→
  each of the three feature scenes→back→MainMenu), confirming: single active
  `EventSystem` at every step, `SceneFlowController.Instance` persists, FPS counter
  stays active/visible throughout, active scene is set correctly after each transition.
  **That verification predates the D13 hardening pass** — the in-flight guard,
  fallback camera, `sceneLoaded`-based activation timing, and Editor bootstrap are
  all new and only compile/EditMode-test verified so far; re-verify live in Play
  Mode before relying on them (developer's own call per the Play Mode testing note
  above).

### Phase 0 status
- **Main menu** (`Assets/Scenes/MainMenu.unity`): Canvas (Scale With Screen Size,
  1920×1080 reference, match 1), `EventSystem` with `InputSystemUIInputModule`
  (project's `activeInputHandler` is Input System only — the legacy
  `StandaloneInputModule` won't receive clicks). Three buttons, each a
  `MainMenuButton.prefab` instance with a self-wiring `MenuButtonSceneLoader`
  (`OnValidate` grabs its own `Button` via `TryGetComponent`), calling
  `SceneFlowController.Instance.Navigate` on click (see "Scene-flow architecture").
  Button art: `Assets/Art/Sprites/buttons.png` (blue = Ace of Shadows, green = Magic
  Words, red/orange = Phoenix Flame).
- **FPS counter** (`Assets/App/FpsCounter/`): `FpsCalculator` (Logic, plain
  C#, windowed average not instantaneous 1/deltaTime) + `FpsCountUIController`
  (Monobehaviour, only writes `TMP_Text.text` when the rounded value actually
  changes). **Lives once, centrally, in `Shell.unity`'s always-loaded canvas** (moved
  out of `AceOfShadows.unity`'s `OverlayUI` this session) — visible top-left across
  every scene automatically, no per-scene duplication needed.
- **WebGL build + hosting:** build settings are **Brotli compression +
  Decompression Fallback enabled** (Player Settings → Publishing Settings) — this
  combination is required because the host (GitHub Pages) can't send a
  `Content-Encoding: br` header, and Decompression Fallback embeds a client-side
  JS decompressor so it works anyway while keeping the ~13MB transfer size (vs
  ~60MB with compression disabled). Verified working live.
  - Hosted at **https://dogukantaytuglu.github.io/softgames-task-build/**, a
    **separate repo** `github.com/dogukantaytuglu/softgames-task-build` (public,
    required for free GitHub Pages), sibling folder at
    `E:\Projects\UnityProjects\softgames-task-build`.
  - **Deploy mechanism: manual, one click.** `Assets/Editor/DeployWebGL.cs` adds
    **Build → Build & Deploy WebGL** to the Unity menu — builds straight into the
    `softgames-task-build` folder, `git add`/commit/push automatically, shows a
    result dialog either way. This replaced an attempted CI pipeline (GitHub
    Actions) — see D8 for why that was abandoned. No auto-trigger on push; you
    have to click the menu item each time you want a new deploy live.
- **Responsive canvas / touch+mouse / real-device verification:** not yet
  independently confirmed this session — the CanvasScaler setup exists (main menu,
  Ace of Shadows overlay) but nobody has checked a real phone yet. Still on the
  Definition-of-Done list.

### Available asset packs
- **`Assets/Art/Sprites`** — clean, no watermark, currently in active use (main
  menu buttons, Ace of Shadows card). `buttons.png`, `icons.png`, `popups.png`,
  `additional controls.png`.
- **`Assets/uVegas`** — a real card-game framework (rank/suit glyphs for all 13
  ranks + 4 suits, `UICard` component, 9 `CardTheme` ScriptableObjects). **Its
  `Front.png`/`Back.png` base card sprites have a "uVegas" watermark baked directly
  into the texture** (confirmed via in-engine close-up capture, not visible in a
  flattened preview) — don't use those two specifically unless there's a licensed,
  non-trial copy. The rank/suit glyphs weren't checked for watermarks.
- `Assets/300Mind` (the original "2D Game UI Kit") was **removed** — fully
  superseded by `Assets/Art/Sprites`.

### Other packages/deps in use
- **DOTween + DOTweenPro** (`Assets/Plugins/Demigiant`), `DOTWEEN` /
  `DOTWEEN_TEXTMESHPRO` scripting defines active.
- **TimerUtil** (`dogukantaytuglu/TimerUtil`, the developer's own reusable timer
  library) — **vendored** into `Assets/Plugins/TimerUtil`, not installed as a git
  package, because that source repo has no `.meta` files committed, so Unity's
  Package Manager can't import it at all. If that repo ever gets `.meta` files
  added, it could switch to a real package dependency instead.
  **Fixed a real leak in it 2026-08-23 (D13):** `Timer.Stop()` never called
  `TimerService.UnregisterTimer`, so every `AceOfShadowsController` created via
  scene navigation (MainMenu→AceOfShadows→home→...) stayed permanently rooted in
  the service's static `Timers` list along with everything it closed over (144
  cards' worth). `Stop()` now unregisters; `Start()` now re-registers (idempotent,
  guarded in `RegisterTimer`) so `Restart()` still works. Worth upstreaming to the
  `TimerUtil` source repo itself, not just this vendored copy, since any future
  feature that uses a timer and gets navigated away from will hit the same trap.
- Input System package only (`activeInputHandler: 1`) — no legacy Input Manager.

## Immediate next steps

1. **Re-verify the Shell→content Play Mode nav loop live** — the D13 hardening
   pass (in-flight navigation guard, fallback camera, `sceneLoaded`-based active-
   scene timing, Editor auto-bootstrap, `Assets/App` move) is compile-clean and
   EditMode-test-passing (35/35) but only the *previous*, unhardened version of
   this loop was ever actually clicked through in Play Mode. Do that before
   building on top of it.
2. Build Magic Words (fetch the endpoint first — `BRIEF.md` §5 is explicit about
   this — before designing anything; the emoji-in-TextMeshPro spike from the
   original Phase 0 plan was never actually done, so that risk is still live).
   The scene already has a Canvas/EventSystem/HomeButton now — just needs the
   actual dialogue content built inside it. Any root-`Instantiate` inside it should
   go through a scene-local parent (see the `sceneLoaded`-timing note under
   Scene-flow architecture) rather than relying on the active scene being correct.
3. Build Phoenix Flame (particle system + Animator-driven color transitions —
   the brief specifically requires an Animator Controller, not a script lerp).
   Same starting point as Magic Words — Canvas/EventSystem/HomeButton already there.
   If it uses a timer, no leak risk anymore (D13 fixed that in TimerUtil itself).
4. Verify responsive layout + touch input on a real phone — now also needs to cover
   the additive Shell→content scene transition specifically (untested on-device).
5. Build-size measurement/reduction write-up for the README (`BRIEF.md` §6) —
   not started; the Brotli+Fallback numbers from the Ace of Shadows deploy work
   are a natural starting point (~13MB vs ~60MB uncompressed, already measured
   in this session's conversation history, just not written up anywhere yet).
6. README covering architecture/decisions/trade-offs — not started.

## Conventions (binding for all three tasks, from the original brief)

- **Logic stays out of MonoBehaviours** — now additionally enforced at
  compile-time per-feature via the Logic/Monobehaviour asmdef split (see above).
- **Unit tests via Unity Test Framework, EditMode**, written alongside the code
  they cover — not retrofitted.
- Every decision must be defensible out loud in a follow-up conversation — don't
  introduce a pattern or dependency that can't be explained. Record real decisions
  in `decisions.md` as they're made.
- **Collaboration note:** the developer writes the actual game code himself
  (in Rider). Claude's default role is review/bug-hunt/optimize, not author —
  code gets written by Claude only when explicitly asked for. Tooling/infra work
  (CI attempts, deploy scripts, editor config, git operations, asmdef plumbing)
  has been more collaborative/Claude-authored throughout.

## How to update this file

Edit in place whenever the state changes meaningfully — a phase completes, a task
is built, a tool gets connected. This file describes *now*, not history; history
that matters (the "why") goes in `decisions.md` instead. Don't let this drift —
a stale current-context.md is worse than none.
