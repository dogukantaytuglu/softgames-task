# softgames-task — Current Context

> 👉 New here (or resuming)? This is the single source of truth for "where things
> stand." Read this, then `decisions.md` if you need the "why" behind something.
> The assignment itself is `BRIEF.md`.

Last updated: 2026-08-23 (end of the Ace of Shadows build session).

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

### Tooling
- **Unity 6000.0.82f1**, Unity MCP (`com.unity.ai.assistant`) connected and used
  throughout this session for scene/prefab building, compile checks, EditMode test
  runs, and Play Mode screenshots. Its tools occasionally report "Unity not
  detected" transiently — retrying once always resolved it this session.
  **As of this update the MCP server itself disconnected** (tools no longer
  resolve via ToolSearch) — reconnect it in a fresh session before relying on it.
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
- **Tests:** `Assets/Tests/EditMode/<Feature>/`, own `<Feature>.Tests.asmdef`
  referencing both the feature's Logic and Monobehaviour asmdefs, `includePlatforms:
  ["Editor"]`, `precompiledReferences: ["nunit.framework.dll"]`,
  `defineConstraints: ["UNITY_INCLUDE_TESTS"]`. 27 EditMode tests total right now
  (5 FpsCounter, 22 Ace of Shadows), all passing.
- **In Unity MCP `Unity_RunCommand` scripts specifically** (not normal project
  code): bare `Image` and `CodeEditor` resolve to the wrong thing (some other
  namespace collides) — always fully-qualify as `UnityEngine.UI.Image` /
  `Unity.CodeEditor.CodeEditor` inside those scripts only.

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

### Phase 0 status
- **Main menu** (`Assets/Scenes/MainMenu.unity`): Canvas (Scale With Screen Size,
  1080×1920 reference, match 0.5), `EventSystem` with `InputSystemUIInputModule`
  (project's `activeInputHandler` is Input System only — the legacy
  `StandaloneInputModule` won't receive clicks). Three buttons, each a
  `MainMenuButton.prefab` instance with a self-wiring `MenuButtonSceneLoader`
  (`OnValidate` grabs its own `Button` via `TryGetComponent`), calling
  `SceneManager.LoadScene` on click. Button art: `Assets/Art/Sprites/buttons.png`
  (blue = Ace of Shadows, green = Magic Words, red/orange = Phoenix Flame).
- **FPS counter** (`Assets/Feature/FpsCounter/`): `FpsCalculator` (Logic, plain
  C#, windowed average not instantaneous 1/deltaTime) + `FpsCountUIController`
  (Monobehaviour, only writes `TMP_Text.text` when the rounded value actually
  changes). Present in `AceOfShadows.unity`'s `OverlayUI` canvas; **not yet added
  to MainMenu/MagicWords/PhoenixFlame scenes** — worth doing before final
  submission since the brief wants it visible throughout.
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
- Input System package only (`activeInputHandler: 1`) — no legacy Input Manager.

## Immediate next steps

1. Reconnect Unity MCP (disconnected as of this context update).
2. Add the FPS counter to MainMenu/MagicWords/PhoenixFlame scenes, not just
   AceOfShadows.
3. Build Magic Words (fetch the endpoint first — `BRIEF.md` §5 is explicit about
   this — before designing anything; the emoji-in-TextMeshPro spike from the
   original Phase 0 plan was never actually done, so that risk is still live).
4. Build Phoenix Flame (particle system + Animator-driven color transitions —
   the brief specifically requires an Animator Controller, not a script lerp).
5. Verify responsive layout + touch input on a real phone.
6. Build-size measurement/reduction write-up for the README (`BRIEF.md` §6) —
   not started; the Brotli+Fallback numbers from the Ace of Shadows deploy work
   are a natural starting point (~13MB vs ~60MB uncompressed, already measured
   in this session's conversation history, just not written up anywhere yet).
7. README covering architecture/decisions/trade-offs — not started.

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
