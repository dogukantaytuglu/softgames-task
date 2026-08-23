# softgames-task — Current Context

> 👉 New here (or resuming)? This is the single source of truth for "where things
> stand." Read this, then `decisions.md` if you need the "why" behind something.
> The assignment itself is `BRIEF.md`.

Last updated: 2026-08-23 (Ace of Shadows: world-space → UI conversion (D21,
pushed as `73ed6c7`), then an exit-cascade animation on completion (D22, pushed
as `db4dd75`), then a Restart button (D23, **uncommitted** — pending the
developer's Play Mode check). ⚠️ **Working tree currently also has `totalCards`
at `10` in `AceOfShadowsConfig.asset`, uncommitted** — a leftover test value,
not `144` — revert before committing anything from this session.)

## What we're building

The SOFTGAMES Unity Developer take-home assignment: three self-contained demos
(Ace of Shadows, Magic Words, Phoenix Flame) reachable from an in-game menu, in
Unity 6, responsive on mobile + desktop, FPS counter top-left, built for WebGL and
hosted at a public link. Full detail, grading criteria, and task-by-task guidance:
`BRIEF.md`. No deadline — self-imposed or otherwise (see `decisions.md`, D1).

## Current state

### Task progress
- **Ace of Shadows: built and working**, now past a polish/optimization pass, a
  UI conversion (D21), an exit-cascade animation (D22), and a Restart button
  (D23). 144-card deck drain between two stacks, Source→Target, one move per
  second, message on completion, real playing-card visuals (random per card),
  pop-animated stack counters, project-wide Baloo 2 font. Cards/stacks render as
  UI (RectTransform/Image under a Screen Space - Overlay Canvas) as of
  2026-08-23, not world-space SpriteRenderers. Once the deck empties: both
  counters hide and every landed card cascades off-screen (staggered), the
  finished message shows, and a Restart button on that message rebuilds the
  deck/cards from scratch and restarts the timer. See "Ace of Shadows
  architecture" and "Rendering & performance" below, and D21/D22/D23 in
  decisions.md. **Nothing past D21 has been Play Mode-verified yet** — D21 and
  D22 are committed/pushed anyway (developer's call, per the established
  workflow); D23 (Restart) is still sitting uncommitted pending that check (next
  steps item 0b).
- **Magic Words: not started.** `Assets/Scenes/MagicWordsScene.unity` exists as an empty
  placeholder (default camera + light only), registered in Build Settings, reachable
  from the main menu's "Magic Words" button. No script work yet.
- **Phoenix Flame: not started.** Same as Magic Words — empty placeholder scene,
  wired into the menu, nothing built.
- **Phase 0 (menu/FPS/responsive/WebGL) is done**, see below.
- **Scene-flow/navigation shell built.** `AppScene.unity` is now build-index 0, the sole
  persistent scene, holding the FPS counter (moved out of AceOfShadows) and a
  `SceneService` singleton. Every other scene (MainMenu, AceOfShadows, MagicWords,
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
  architecture review of the SceneFlow feature, found a build-breaking bug (AppScene
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
- **Single init point per feature, no scattered Awake/Start** (2026-08-23, D15): a
  feature's existing composition root (`AceOfShadowsController`, `SceneService`
  — each already held every reference its feature needed) collapses what used to be
  a same-class `Awake()`+`Start()` split into one `Awake()`. Where no composition
  root existed at all (`MainMenuScene`'s 3 independent `MenuButtonSceneLoader`
  buttons), a small new `<Feature>Initializer` MonoBehaviour (e.g.
  `MainMenuInitializer`, on the scene's `Canvas` root) is the *only* thing with an
  `Awake()`, and calls a plain `public void Initialize()` on each component it owns
  (`GetComponentsInChildren`, no reflection/interface — deliberately rejected a
  generic `IInitializable` + new-assembly version of this as too much machinery for
  a 3-demo take-home). Components already owned by another composition root
  (`FinishedMessageView`, called by `AceOfShadowsController`) also get a plain
  `Initialize()` instead of their own `Awake()` — same pattern `StackCounterView.Bind()`
  already used successfully. **Deliberate exception:** `HomeButtonController`
  (SceneFlow's widget, instanced into other features' scenes) and
  `FpsCountUIController` (separate feature sharing AppScene with SceneService)
  keep their own tiny `Awake()` — pulling either into another feature's initializer
  would need a new cross-feature asmdef reference for zero real benefit, since both
  were already single-method/single-purpose with nothing to merge.
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
  `defineConstraints: ["UNITY_INCLUDE_TESTS"]`. **32 EditMode tests actually run**
  right now (5 FpsCounter, 19 Ace of Shadows, 8 SceneFlow) — verified by running
  the suite (`TestRunnerApi`, `PassCount`), not by counting attributes.
  ⚠️ **`grep -c "\[Test\]"` undercounts by 2**: `SceneFlowStateTests.
  TryBeginNavigation_ToNullOrEmptyScene_ReturnsFalse` uses two `[TestCase(...)]`
  attributes with no `[Test]` attribute at all (valid NUnit — `[TestCase]` alone
  makes a method a test), so a literal `[Test]` grep reports 30/6-SceneFlow, not
  the real 32/8. A `unity-interviewer` audit caught this doc claiming 33 and
  "corrected" it to 31 using exactly that flawed grep - the correction was
  itself wrong; the pre-D24 real count was 33 (5/20/8), matching what this doc
  originally said. **If this number ever needs verifying again, run the suite,
  don't grep for `[Test]`.** Down to 32 after D24 removed `CardStack.
  CountChanged` (a dead event with no real subscriber, only a test that covered
  itself) — the CardStackLayoutTests Z-depth-test drop from D21 (35→33) already
  happened before this. FpsCounter's tests moved here
  from `Assets/App/FpsCounter/Scripts/Tests/` (2026-08-23, D13) to
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
- **`System.Reflection` and `UnityEditorInternal.FrameDebuggerUtility` are both
  blocked/inaccessible from `Unity_RunCommand` scripts** (sandboxed assembly,
  no reflection namespace, no internals-visibility) — can't set private fields
  via reflection, and Frame Debugger data has to be read visually, not scripted.
- **`SpriteAtlas` platform overrides need `overridden = true` explicitly set**,
  not just the value — `platformSettings.maxTextureSize = 8192` alone silently
  no-ops if `overridden` stays `false`. Also: the Editor's *preview* pack (what
  Play Mode actually uses) reads the platform-specific bucket matching
  `EditorUserBuildSettings.activeBuildTarget` (e.g. `"WebGL"`), **not**
  `"DefaultTexturePlatform"` — setting only the Default bucket has no visible
  effect on Play Mode testing even though it's correctly saved.
- **`EditorSettings.spritePackerMode` defaults to `Disabled`** — with it disabled,
  every `SpriteRenderer` renders from its raw, unpacked source texture in the
  Editor (including Play Mode) regardless of any `SpriteAtlas` asset's own
  settings; only a real Player build honors the atlas. Set to `AlwaysOnAtlas` to
  make Editor Play Mode actually exercise atlas packing during iteration.

### Ace of Shadows architecture
- **Domain** (`Assets/Feature/AceOfShadows/Scripts/Logic/`): `Card`, `CardStack`
  (LIFO), `CardMove`, `CardDeck`. `CardDeck` is deliberately
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
- **Tuning lives in `AceOfShadowsConfig`** (`Assets/Feature/AceOfShadows/Configs/AceOfShadowsConfig.asset`,
  a `ScriptableObject` in `AceOfShadows.Monobehaviour`, needs `UnityEngine` so it
  can't live in `Logic`): `totalCards`, `moveInterval`, `maxRotationDegrees`,
  `moveDuration`, `moveEase`, `cardVisuals` (the random-visual list, see below) —
  all previously scattered `[SerializeField]`s on `AceOfShadowsController`/`CardView`.
  Public getters only (`TotalCards`, `MoveInterval`, ...). `CardView.Initialize(config)`
  is how each card gets a reference. **Gotcha hit here:** after adding new fields to
  an existing config asset, a stale in-memory `ScriptableObject` picked up garbage
  values instead of the C# field initializer defaults on first reimport — always
  verify a newly-added field's actual serialized value (Inspector or the raw
  `.asset` YAML) rather than trusting the default expression took effect.
- **Card visual: real playing-card prefabs, not a placeholder sprite.** `CardView.Initialize`
  picks a random prefab from `config.CardVisuals` (`Random.Range(0, Count)` —
  watch for the classic off-by-one, the upper bound is already exclusive) and
  instantiates it as a child, zeroing its local position/rotation (the source
  asset pack's prefabs have a baked-in nonzero root offset from their original
  grid layout). Card faces come from a "PlayingCards" asset pack
  (`Assets/Feature/AceOfShadows/Prefabs/PlayingCards/`, `Assets/Feature/AceOfShadows/Textures/`),
  currently 2 decks kept in rotation (trimmed down from the pack's original 8 to
  control texture/build-size cost). `Card.prefab` itself no longer has a
  `SpriteRenderer` — it's just an empty root the picked visual gets parented under.
  **`Assets/uVegas` (the previously-flagged watermarked pack, D10) is fully
  removed** — superseded by the real card art.
- **Stack counters pop-animate on their own timing, not a shared trigger.**
  `StackCounterView.SetCount(int)` (text only, used once at startup) and
  `.Refresh(int)` (text + a `DOPunchScale` pop, used on every move) are called
  *directly* by `AceOfShadowsController` — deliberately not routed through an
  event/interface on `CardStack`. The reason that matters: `CardDeck.MoveNext()`
  pops `Source` and pushes `Target` in the same instant, so a naive
  `CountChanged`-style event on `CardStack` would fire both counters together at
  move *start* — wrong for the target, which should visually pop on *landing*.
  (`CardStack` actually had exactly this event at one point — removed in D24,
  see below, once it turned out to have zero real subscribers.)
  `sourceCounterView.Refresh(...)` is called right after `_deck.MoveNext()` (card
  leaves); `targetCounterView.Refresh(...)` is called from `OnCardLanded()`, the
  actual DOTween completion callback for that card (card arrives). An
  `IStackCountChangeNotifier` interface + `CardStack.Trigger` version of this was
  built first and discarded — it added a round trip (controller → `CardStack.Trigger`
  → back to the same `StackCounterView` the controller already had a reference to)
  with no consumer that needed the decoupling. Same shape of call as D15's
  `IInitializable` rejection: not worth the machinery at this project's scale.
- **Cadence:** `Assets/Plugins/TimerUtil` (vendored, see below) drives a
  `CountdownTimer(config.MoveInterval, loopCount: -1)` (1s) as the *sole* trigger
  for "start next move" — it's fully decoupled from animation completion. The
  only invariant that keeps this race-free: `config.MoveDuration` must stay
  under `config.MoveInterval`, so a card always finishes landing before the next
  one starts. (Values live in `AceOfShadowsConfig` now, see below — don't
  hardcode either number here, they've already drifted once.)
- **Cards are UI now, not world-space sprites — converted 2026-08-23, see D21.**
  `CardView`'s root and every card-art prefab are `RectTransform`s living under the
  scene's single Screen Space - Overlay Canvas, not `Transform`s with
  `SpriteRenderer`s under a Perspective camera. **Draw order comes from Canvas
  sibling index, not Z position or `sortingOrder`** - each card is instantiated/
  reparented as the last sibling of its stack, so "newest card" = "last sibling" =
  "drawn on top", for free, matching the stack's own LIFO order. `CardStackLayout.
  GetOffset` returns a `Vector2` pixel offset (no Z component at all anymore).
  This whole rewrite (Canvas render mode, CanvasScaler, `SourceStack`/`TargetStack`
  reparenting, `CardView`/`CardStackLayout`/`AceOfShadowsController` signature
  changes, all 106 card-art prefabs) is why the world-space-specific notes that
  used to sit here are gone - see D21 in `decisions.md` for the full list of what
  changed and why (the short version: world-space + a Perspective camera doesn't
  scale across resolutions/aspect ratios, which was flagged as a real problem for
  an eventual landscape-to-portrait layout; UI + `CanvasScaler` is what the rest of
  the project already uses for exactly this).
- **Random rotation per card** (`config.MaxRotationDegrees`), assigned once at placement (not deterministic,
  not unit-tested — intentional one-off visual jitter). Combined effect: stacks
  read as a messy pile of individual cards, not a smooth block. **No more landed-
  card "flip"** - the old 180° Y-rotation trick (revealing a second `Back_D7`/
  `Back_D8` sprite) only worked in true 3D world space and was dropped along with
  the SpriteRenderer conversion (that Back sprite was deleted from all 106
  prefabs, not ported) - see D21. If a landed-card visual cue is wanted again, it
  needs a flat-canvas-appropriate technique (e.g. a scale.x squash-flip, or a
  tint), not a port of the old one.
- **Hierarchy:** `SourceStack` / `TargetStack` are `RectTransform`s under the
  Canvas (anchored proportionally at 25%/75% width, not fixed world positions) -
  cards `SetParent` onto them on move, using `worldPositionStays: true` (the
  default) specifically so the reparent itself doesn't cause a visual snap; the
  subsequent DOTween `DOAnchorPos` move then animates smoothly from wherever the
  card actually is.
- **On completion, the board clears itself (D22):** `AceOfShadowsController.
  OnAllAnimationsFinished` hides both `StackCounterView`s, shows the finished
  message, and cascades every card in `_deck.Target.Cards` (the domain stack's
  own bottom-to-top list, mapped through `_cardViewsByCardId` — not a
  `GetComponentsInChildren` scene query) straight down off-screen via
  `CardView.AnimateExitDown`, each with `i * config.ExitStagger` delay so they
  fall in sequence rather than as one block. All four numbers
  (`ExitDistance`/`ExitDuration`/`ExitEase`/`ExitStagger`) live in
  `AceOfShadowsConfig`. These three things (message show, counters hide, cascade
  start) all fire in the same instant right now, not staged — worth deciding
  whether the message should wait for the cascade to finish landing first.
- **Restart (D23):** a `RestartButton` on the finished-message panel calls
  `AceOfShadowsController.Restart()`, which destroys every existing `CardView`,
  throws away the current `CardDeck` and builds a fresh one, calls the same
  `CreateCardViews()` `Awake()` already uses, re-shows/resets both counters,
  hides the finished message, and calls `_timer.Start()` (safe post-`Stop()` —
  resets the countdown fully, see the D13 TimerUtil fix). Not a per-card reset —
  a full rebuild reusing the existing first-time-setup path.

### Rendering & performance (Ace of Shadows)
- **AceOfShadowsScene has no Skybox and no Directional Light** — it's 100% unlit
  sprites, neither was doing anything. Camera clears to Solid Color instead
  (same tint the skybox used to render) and the `Directional Light` GameObject
  was deleted outright.
- **Scene Canvas is Screen Space - Overlay now (was Screen Space - Camera) —
  changed 2026-08-23, see D21.** The old Screen Space - Camera setup existed
  specifically so the `Bg` panel could depth-composite *behind* the 3D cards
  (Overlay mode always draws on top of everything regardless of scene depth).
  That reasoning no longer applies now that cards are UI elements in the same
  Canvas as everything else — depth ordering is just Canvas sibling index (`Bg`
  first, see "Ace of Shadows architecture" above), so Overlay works fine and
  drops the camera dependency entirely.
- **Stack counters (`SourceCounter`/`TargetCounter`) are still World Space
  canvases** (`CounterCanvas.prefab`), parented directly under `SourceStack`/
  `TargetStack` (now `RectTransform`s, not world Transforms) — the count sits
  physically above its own pile. This is a nested Canvas inside the outer Overlay
  Canvas now, which still renders correctly but is a leftover from the pre-D21
  design (it doesn't need to be its own Canvas anymore, since the parent is
  already inside one) — not cleaned up in the D21 pass, flagged as an easy
  follow-up if it turns out to add a real extra draw call.
- **The batching investigation below predates the D21 UI conversion — cards no
  longer use `SpriteRenderer`/`SpriteAtlas`/`Sprite Mesh Type` at all (they're
  `UnityEngine.UI.Image` now), so these specific findings (draw-call counts,
  `spritePackerMode`) don't directly carry over and haven't been re-measured
  under the new UI-based rendering. Worth a fresh Frame Debugger pass if
  performance is revisited, rather than assuming these numbers still hold. Kept
  below for the Bloom/SSAO/post-processing findings, which are still accurate
  (that part of the pipeline didn't change):**
  - Naive assumption going in was wrong: increasing card-visual variety (8 decks)
    tanked SetPass calls (143), and the fix looked like "texture atlas the decks."
    A `PlayingCards.spriteatlas` was built and the 8 deck textures' `Sprite Mesh
    Type` was switched `Tight → Full Rect` (Tight was generating ~24-vert outline
    meshes per card instead of a 4-vert quad — a real, separate bug, worth keeping
    fixed regardless of the atlas).
  - **The atlas turned out to not even be exercised during any of this testing**:
    `EditorSettings.spritePackerMode` was `Disabled` project-wide, so every Editor
    Play Mode session was rendering cards from their raw, unpacked source
    textures the whole time. Fixed to `AlwaysOnAtlas` — Play Mode now actually
    packs and uses it. (A real Player *build* would have used the atlas
    regardless of this Editor-only setting.)
  - **Frame Debugger revealed the real story**: `DrawTransparentObjects` (all 288
    card sprite renderers) is only **2 draw calls** — cards were never the
    bottleneck once deck-texture variety came down. The actual cost is URP
    post-processing: **Bloom alone is ~16 of ~37 total passes**, plus SSAO (4,
    computes ambient occlusion for *lit* geometry — has zero visible effect on
    unlit sprites, a free win to disable, not yet done), Skybox/LUT/CopyColor/UberPost,
    and UI Canvas rendering. `Assets/Settings/SampleSceneProfile.asset` (Bloom +
    Vignette + Tonemapping, all active) is the **unmodified default URP template
    Volume profile** — never deliberately chosen for this project.
  - **Open/undecided**: whether to disable SSAO (free, no visual cost) and
    whether to keep Bloom/Vignette/Tonemapping (real visual effect, aesthetics
    grading criteria cuts both ways) is still the developer's call — see
    Immediate next steps.
  - `UnityEditorInternal.FrameDebuggerUtility` is not accessible from a
    `Unity_RunCommand` script (compiles into a sandboxed assembly without
    internals-visibility) — Frame Debugger has to be read visually/by screenshot,
    not scripted.

### Scene-flow architecture
- **`AppScene.unity` is build-index 0** and the only scene ever opened directly (in
  Editor Play or in the WebGL build) — every other scene is loaded/unloaded
  *additively* on top of it, one "content" scene at a time. AppScene itself never
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
  class (`App = "AppScene"`, `MainMenu = "MainMenuScene"`, `AceOfShadows =
  "AceOfShadowsScene"`, `MagicWords = "MagicWordsScene"`, `PhoenixFlame =
  "PhoenixFlameScene"` — all scene files got a `Scene` suffix 2026-08-23 when the
  developer renamed `Shell.unity` → `AppScene.unity` and asked for the rest to
  match) — is the single source of truth for scene name strings in code
  (per-instance prefab `sceneName` fields are still hand-typed strings; a
  `SceneAsset`-backed wrapper would be overkill at this scale). 8 EditMode tests
  total.
- **`SceneFlow.Monobehaviour`**: `SceneService` — a plain static-instance
  singleton (not `DontDestroyOnLoad`; unnecessary since it lives in AppScene, which is
  never unloaded) living on a `SceneService` GameObject in `AppScene.unity`.
  Single `Awake()` (merged from a former Awake+Start split, 2026-08-23, D15 — see
  "single init point per feature" below) sets the singleton, then adopts whatever
  scene is already active (via `SceneFlowState`'s constructor) if one other than
  AppScene is already loaded — this is what makes the Editor bootstrap below work
  — otherwise calls `Navigate(homeSceneName)` (default `SceneNames.MainMenu`) to
  boot the first content scene. `Navigate(sceneName)` unloads the previous content
  scene (`SceneManager.UnloadSceneAsync`), additively loads the target
  (`LoadSceneAsync(..., LoadSceneMode.Additive)`); if the load operation comes back
  `null` (scene not in Build Settings — a silent Unity API failure otherwise), it
  logs an error and releases the in-flight guard instead of leaving navigation
  permanently bricked. On success, `SceneManager.sceneLoaded` (not the load
  operation's `.completed`) is what triggers `SetActiveScene` + `CompleteNavigation`
  — Unity fires `sceneLoaded` after the new scene's `Awake` calls, which is what
  makes it safe for the new scene's own `Awake()`-driven init to root-`Instantiate`
  things without them silently landing in AppScene. `HomeButtonController` (was
  `BackButtonController` — renamed 2026-08-23,
  D13, since there's no back-stack, it always goes home) calls
  `Instance.NavigateHome()`, guarded against a null `Instance`.
- **Exactly one content scene is ever loaded alongside AppScene**, which is what keeps
  EventSystems from ever duplicating — AppScene itself holds no Light and no
  EventSystem (only a Screen Space Overlay Canvas, which doesn't need a camera to
  render, at `m_SortingOrder: 100` so it's deterministically drawn above whatever
  content scene's own Overlay canvas is loaded alongside it). Each content scene
  keeps its own Main Camera, Directional Light, and `EventSystem`
  (`InputSystemUIInputModule`) exactly as before; only one is ever active at a time
  since AppScene fully unloads the previous content scene before loading the next.
  **AppScene does hold one Camera**, unlike the original design: a `FallbackCamera`
  (Solid Color clear, culling mask `Nothing`, depth `-100`, no AudioListener) added
  2026-08-23 (D13) — closes a real gap where unload-then-load left a frame (worse
  on WebGL) with zero cameras rendering.
- **Editor-only bootstrap** (`EditorSceneBootstrap`, `#if UNITY_EDITOR`,
  `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`, added 2026-08-23, D13):
  additively loads `AppScene` if it isn't already loaded and no `SceneService`
  exists, *unless* the scene about to play is AppScene itself. This is what makes
  opening `MagicWordsScene.unity`/`PhoenixFlameScene.unity`/`AceOfShadowsScene.unity` directly and
  hitting Play still work (FPS counter visible, back-to-home button functional)
  without the friction of having to open AppScene first every time — important now
  that Magic Words and Phoenix Flame are about to be built and Play will get hit
  from those scenes a lot. Compiles out entirely in real builds.
- **`MenuButtonSceneLoader`** (MainMenu) calls `SceneService.Instance.Navigate(sceneName)`
  instead of `SceneManager.LoadScene` directly, guarded against a null `Instance`.
- **Back-to-home buttons**: `Assets/App/SceneFlow/Prefabs/HomeButton.prefab`
  (renamed from `BackButton.prefab`, D13) — top-right anchored, `buttons_41` sprite
  (green "home" icon from `Assets/App/Sprites/UI/buttons.png`, chosen since it reads
  as "return to main menu" and needs no extra label). Instanced into
  `AceOfShadowsScene.unity`, `MagicWordsScene.unity`, `PhoenixFlameScene.unity` (not MainMenu —
  that's home, nothing to go back to). `MagicWords`/`PhoenixFlame` were empty
  placeholders with no Canvas/EventSystem before this session started — both were
  added (Screen Space Overlay, same `CanvasScaler` settings as MainMenu/AppScene,
  `EventSystem` duplicated from AceOfShadows' so the Input-System action-asset
  wiring matches exactly).
- **Full navigate/back loop verified live in Play Mode** via Unity MCP (AppScene→MainMenu→
  each of the three feature scenes→back→MainMenu), confirming: single active
  `EventSystem` at every step, `SceneService.Instance` persists, FPS counter
  stays active/visible throughout, active scene is set correctly after each transition.
  **That verification predates the D13 hardening pass** — the in-flight guard,
  fallback camera, `sceneLoaded`-based activation timing, and Editor bootstrap are
  all new and only compile/EditMode-test verified so far; re-verify live in Play
  Mode before relying on them (developer's own call per the Play Mode testing note
  above).

### Phase 0 status
- **Main menu** (`Assets/Scenes/MainMenuScene.unity`): Canvas (Scale With Screen Size,
  1920×1080 reference, match 1), `EventSystem` with `InputSystemUIInputModule`
  (project's `activeInputHandler` is Input System only — the legacy
  `StandaloneInputModule` won't receive clicks). Three buttons, each a
  `MainMenuButton.prefab` instance with a self-wiring `MenuButtonSceneLoader`
  (`OnValidate` grabs its own `Button` via `TryGetComponent`), calling
  `SceneService.Instance.Navigate` on click (see "Scene-flow architecture").
  Button art: `Assets/App/Sprites/UI/buttons.png` (blue = Ace of Shadows, green = Magic
  Words, red/orange = Phoenix Flame).
- **FPS counter** (`Assets/App/FpsCounter/`): `FpsCalculator` (Logic, plain
  C#, windowed average not instantaneous 1/deltaTime) + `FpsCountUIController`
  (Monobehaviour, only writes `TMP_Text.text` when the rounded value actually
  changes). **Lives once, centrally, in `AppScene.unity`'s always-loaded canvas** (moved
  out of `AceOfShadowsScene.unity`'s `OverlayUI` this session) — visible top-left across
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
- **`Assets/App/Sprites`** (moved from `Assets/Art/Sprites`, GUIDs preserved) —
  clean, no watermark, in active use (main menu buttons, home button icon).
  `buttons.png`, `icons.png`, `popups.png`, `additional controls.png`.
- **`Assets/Feature/AceOfShadows/Prefabs/PlayingCards` + `.../Textures`** — a real
  playing-card asset pack (per-rank/suit prefabs across multiple decks, 2 currently
  kept in `AceOfShadowsConfig.CardVisuals`), packed into
  `Assets/Feature/AceOfShadows/Textures/PlayingCards.spriteatlas`. This is what
  Ace of Shadows' cards actually render now — see "Rendering & performance" above.
- **`Assets/Art/Fonts/Baloo2`** — Baloo 2 (Google Fonts, OFL-licensed), downloaded
  as a single variable-weight `.ttf` (Google Fonts no longer ships separate static
  weight files for it) resolved to the Regular named instance. `Baloo2 SDF.asset`
  is the generated TMP Font Asset, now the TMP Settings default and assigned to
  every existing TMP text in the project (FPS counter, finished-message, stack
  counters). No Bold variant yet — would need extracting a separate static
  instance from the variable font, more tooling than a straight download.
- **`Assets/uVegas` — removed entirely** (was previously kept-but-unused due to a
  watermark on its `Front.png`/`Back.png`, see D10 — fully superseded now that
  real playing-card art is in use, no remaining references anywhere in `Assets/`).
- `Assets/300Mind` (the original "2D Game UI Kit") was **removed** earlier — fully
  superseded by the sprite packs above.

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

0. **Decide on SSAO/Bloom/Vignette/Tonemapping in `SampleSceneProfile.asset`**
   (see "Rendering & performance" above) — SSAO is a free, no-visual-cost
   disable; Bloom/Vignette/Tonemapping are the unmodified URP template defaults
   and a real aesthetics call the developer hasn't made yet.
0a. **Revert `AceOfShadowsConfig.asset`'s `totalCards` from `10` back to `144`**
   before committing anything else — currently sitting uncommitted in the
   working tree as a leftover test value from iterating on the restart loop.
0b. **Play Mode-verify D21 + D22 + D23 together** (cards/stacks moved from
   world-space SpriteRenderers to UI RectTransform/Image under a Screen Space -
   Overlay Canvas; the exit-cascade animation on completion; the Restart button
   — see "Ace of Shadows architecture" and decisions.md D21/D22/D23). D21 and
   D22 are committed/pushed anyway (developer's call); D23 is not yet committed,
   waiting on this check. Compiles clean, all 32 EditMode tests pass, and the
   resulting prefab/scene YAML was read back and sanity-checked, but nothing
   here has been seen running yet: card sizes (260px tall, eyeballed), the
   `PerCardOffset` fan spread (3px/card, also eyeballed), draw order via Canvas
   sibling index, the stack-counter pop animations against the new anchored
   stack positions (25%/75% width), whether the nested `CounterCanvas` World
   Space canvas still renders correctly parented under an Overlay canvas, the
   exit-cascade's timing/distance/stagger (see the "fires simultaneously, not
   staged" note above), and the full Restart click → rebuild → replay loop
   (including clicking Restart mid-exit-animation, which kills in-flight
   DOTween tweens via `CardView.OnDestroy`). Also note: this conversion fixes
   the *underlying* rendering approach (proportional anchoring under
   `CanvasScaler` instead of fixed world positions) but does **not** yet add an
   actual landscape↔portrait layout switch (different anchor presets per
   orientation) — that's still open, this was the prerequisite for it, not the
   thing itself.
1. **Re-verify the AppScene→content Play Mode nav loop live** — the D13 hardening
   pass (in-flight navigation guard, fallback camera, `sceneLoaded`-based active-
   scene timing, Editor auto-bootstrap, `Assets/App` move) is compile-clean and
   EditMode-test-passing (32/32 — see the Tests entry above for the real count
   and why a naive `[Test]`-attribute grep gets it wrong) but only the *previous*,
   unhardened version of
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
   the additive AppScene→content scene transition specifically (untested on-device).
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
