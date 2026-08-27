# softgames-task — Current Context

> 👉 New here (or resuming)? This is the single source of truth for "where things
> stand." Read this, then `decisions.md` if you need the "why" behind something.
> The assignment itself is `BRIEF.md`.

Last updated: 2026-08-27 (late). **Everything below is committed and pushed** through
`2700147`. **All four screens now have a UI/UX round** — Phoenix Flame was the last one
and is done (D41), and its fake-light glow now colour-lerps with the flame (D42). Full
detail and reasoning: **D42** and **D41** first, then D40.

✅ **Phoenix Flame is Play Mode-verified by the developer (2026-08-27).** They ran it,
saw all three colour states, and called the quality acceptable. That closes the
longest-standing unknown on this screen and, importantly, **settles the bloom
question by evidence**: the three states read as distinct *without* post-processing,
so the HDR-clamping worry (D36 Tier 2) did not materialise in practice and no Volume
needs adding. See "the hosted build is stale" below for what is now the top risk.

This session: Ace of Shadows Round 2 and the card-prefab rebuild (D39), the
**Magic Words round** (`7caaf4d`), the **failure-path fixes** the brief demands
(`eecf159`), a **runtime-only sprite-atlas bug** found and mostly fixed
(`7b8ab0e`, which also flattened the sprite folder), and finally **Phoenix Flame's
fire silhouette fix + full scene round** (D41). The aborted 6-system particle
attempt from D40 is still parked in a git stash and is not needed.

⚠️ **Two agent runs on Phoenix Flame both overreached, and both self-reported clean.**
The second one modified the fire prefab it was explicitly told not to touch, then
reported it byte-identical — it wasn't. See D41. **An agent's own "I verified nothing
else changed" is not evidence; `git diff --stat` against a known-good line count is.**

## 👉 NEXT JOB: redeploy, then the README

All four screens have their visual round and Phoenix Flame is developer-verified, so
the remaining work is **not** polish. In priority order:

1. 🔴 **Clone the build repo and redeploy.** The public link is pre-D37 and shows
   none of this work; the deploy script needs
   `/Users/dogukan/PersonalProjects/softgames-task-build` to exist and it does not.
   See open item 9. This is the single highest-value action left, because the hosted
   build is what actually gets graded.
2. **The README architecture/decisions write-up** (`BRIEF.md` §6/§7) — still not
   started, and explicitly graded. `decisions.md` is the raw material; it needs
   distilling, not rewriting. The build-size measurement write-up belongs here too.
3. **D36's still-open Tier 1 items**: the stock WebGL `index.html`, the ~20MB build
   size, the `BRIEF.md` salary-figure privacy question, and the README's
   architecture/decisions write-up (still not started, `BRIEF.md` §6/§7).
4. **The fire has a known ceiling, documented in D41**: its flipbook has ≤1.2%
   bottom alpha margin on all 64 frames, so the flat base cut can only be
   *mitigated* in Shuriken, never cured. The brazier now hides it. Retiring it
   properly needs different art, not different curves. Iteration 1 stopped at the
   silhouette; emission rate (~4 particles alive), actual motion (`startSpeed 0`,
   gravity 0, Noise/Velocity all off — 100% of the motion is the flipbook), and the
   plateau-shaped Colour-over-Lifetime alpha were all left as proposals.

## ⚠️ Open items that need a human, not an agent

1. **EditMode tests have never been run through the Test Runner** — its API cannot be
   driven over MCP (needs its window; rejected as a user interaction). Outstanding
   since D39. Assertions were evaluated directly against the real code instead.
2. **`AceOfShadowsConfig` ships `perCardOffset: 1` / `maxPileRise: 200`**, not the
   `2` / `340` Round 2 was tuned around. At 1px a full deck rises 143px instead of
   286px, halving the pile-height signal the retune exists to create. May be a
   deliberate hand-tune; flagged, never overridden.
3. **Magic Words' portrait groundwork is committed but unverified** —
   `SpeakerPortraitView`, `SpeakerInitial` and the disc/ring/glow/glyph sprites are in
   version control, but two consecutive agent runs died mid-verification so none of it
   has been seen in Play Mode. Also still open on that screen: the dialogue box
   positions, and dropping the now-redundant in-box avatar chips.
4. **Two small sprite defects remain — and D40 recorded one of them backwards.**
   `UI.spriteatlas` still has `enableRotation: 1` (unsafe for 9-sliced sprites — the
   borders do not rotate with the sprite). On the second: verified in D41 that
   **`SpriteMeshType.FullRect = 0` and `Tight = 1`**, the opposite of what D40 wrote.
   So **`ui_capsule.png` is actually FullRect and is not the defect** — its
   end-clipping is more likely the `enableRotation` above. The real instance is
   **`ui_rounded_base.png`: Tight, with a 64px 9-slice border** — and it is the
   sprite behind every button and panel in the app. Untouched so far because it is
   shared chrome across all four screens.
5. **`Assets/Resources/PerformanceTestRunInfo.json` / `PerformanceTestRunSettings.json`**
   got committed — Unity Performance Testing artifacts, and everything under
   `Resources/` is force-included in every build. Probably belong in `.gitignore`.

6. **Phoenix Flame's button selected state is real but fragile (D41).** It rides
   `Button`'s built-in Selected transition, so tapping empty background or the home
   button clears the EventSystem selection and the halo vanishes while the flame
   stays coloured. `EventSystem.firstSelectedGameObject` covers load only. Doing it
   properly is ~15 lines driving the three halos off `colorIndex` — the one place the
   no-new-scripts constraint costs something real, in a demo whose whole point is
   showing the active state.
7. **`PhoenixFlame_Pink.anim` is orphaned** — the Pink test state was removed from the
   controller and from `PhoenixFlameConfig`, but the clip file remains on disk.
   Controller/config are otherwise healthy (3 states, indices mapped by transition
   condition, matching config exactly — D32's mechanism works).
8. ~~**The green and blue flame states have never been seen.**~~ **RESOLVED
   2026-08-27** — the developer ran Play Mode, saw all three states, and judged the
   quality acceptable. The feared HDR-emission clamping did not materialise: the
   states are distinct enough with post-processing off, so **bloom is not needed**
   and D36's Tier 2 note on it can be treated as closed. Note this covers the colour
   states specifically; the **selected-state halo's fragility (item 6) was not part
   of what they reported on**, so treat that as still open unless they say otherwise.
9. 🔴 **THE HOSTED BUILD IS BADLY STALE, AND DEPLOYING IS CURRENTLY BLOCKED.** The
   public link the brief requires
   (https://dogukantaytuglu.github.io/softgames-task-build/) was last deployed
   *before* D37, so it contains **none** of the four UI/UX rounds, none of Magic
   Words' failure-path fixes, none of the sprite-atlas fix, and none of Phoenix
   Flame's polish. Anyone opening that link today sees a substantially worse project
   than the repo contains. Worse: `Assets/Editor/DeployWebGL.cs` builds into a
   **sibling folder** `../softgames-task-build` and runs `git add -A` / `commit` /
   `push origin master` inside it — and **that folder does not exist on this
   machine** (this doc previously recorded it at `E:\Projects\UnityProjects\...`,
   a Windows path from a different machine). So `Build → Build && Deploy WebGL` will
   fail at the git step until the build repo is cloned to
   `/Users/dogukan/PersonalProjects/softgames-task-build`. **Clone it, then
   redeploy** — this is the highest-value remaining action in the whole project,
   because it is the artifact the grader actually opens.
10. **`brazier_bowl.png` has a glowing orange rim baked into the art**, so the bowl
   interior stays orange under a green or blue flame (D42). Options: animate the two
   brazier sprites too (needs its own Animator — `Bowl` is a separate branch from
   `FakeLight`), desaturate the baked rim so it reads as hot metal, or keep it as
   deliberate "the coals stay orange" logic. Currently reads as an oversight.

## 🛠 If Unity or the MCP bridge misbehaves

**Restarting Unity makes this particular failure worse, not better** — see D40's
"Operational" section for the full recovery. Short version: stale
`~/.unity/mcp/connections/*.json` descriptors and their `/tmp/unity-mcp-*` sockets
accumulate one per Editor launch and are never cleaned up, and an orphaned
`relay_mac_arm64 --relay --editor-pid <dead>` can sit on ports 9001/9002 preventing
the new Editor from standing up its own relay. Clear all of that, then start Unity.
Also: **only one Editor-driving agent at a time** — `OpenScene(..., Single)` is global,
so a second agent discards the first's unsaved work.

## What we're building

The SOFTGAMES Unity Developer take-home assignment: three self-contained demos
(Ace of Shadows, Magic Words, Phoenix Flame) reachable from an in-game menu, in
Unity 6, responsive on mobile + desktop, FPS counter top-left, built for WebGL and
hosted at a public link. Full detail, grading criteria, and task-by-task guidance:
`BRIEF.md`. No deadline — self-imposed or otherwise (see `decisions.md`, D1).

## Current state

### Task progress
- **Ace of Shadows: built, and past its UI/UX Round 2 (D39).** 144-card deck drain
  between two stacks, one move per second, message on completion, Restart. Now on a
  felt table with dashed card slots, SOURCE/TARGET cream counter pills (reparented
  onto the canvas — as children of the stack roots they drew *under* the cards), a
  completion moment rewritten to "Deck cleared! / Every card made it across" with a
  gold `144 / 144` on an ink plate and a real "Play again" button, a screen title,
  and a countdown ring on the transfer axis between the pills rather than around the
  target slot (the radial sweep started exactly where the growing pile covers it).
  Card art is the reimported `Deck01`/`Deck02` at 300×375. Committed and pushed.
  **Still never Play Mode-verified by the developer**, and the fan values in the
  config are not the tuned ones — see the ⚠️ at the top.
- **Magic Words: built, and past its UI/UX round (D40).** Two dialogue boxes slide
  toward centre on their turn, DOTween typewriter reveal, fast-forward + auto-advance,
  real data fetched at runtime, Twemoji emoji via a TMP Sprite Asset (D30). The round
  added: dialogue 42 at 1.445 line-height (it had been autosizing to ~24px inside a
  267px column), name plate 52 in its own 92px row, box 445 tall sized against all 17
  real endpoint lines, 5% inset, a speech-bubble tail, a TAP hint, a `LINE n OF m`
  progress pill, and a warm gradient ground. Fixed a real defect: every line had been
  rendering TMP synthetic faux-bold because `fontStyle: Bold` was set on a Rubik asset
  with no bold weight wired. **Failure handling now works** — the parser no longer
  throws on non-JSON, requests have a 10s timeout, and a failed fetch no longer
  announces "That's the end of the conversation." **Not verified:** the large speaker
  portraits are committed as groundwork only and have never run in Play Mode; box
  positions and the redundant in-box avatar chips are still open.
- **Phoenix Flame: built, and past both its fire pass and its UI/UX round (D41).**
  A config-driven flame (own prefab + a runtime-instanced material) recolors via a
  3-state Animator Controller (Orange/Green/Blue), driven by 3 UI buttons. The fire's
  silhouette was fixed (the flipbook is 1:2 tall and was being drawn on a near-square
  quad, crushing its own taper ~40%; the flat base was four quad-bottom cuts landing
  on one line). The scene lost Unity's stock grey skybox for a dark `#181327` ground
  with a two-piece brazier the fire sits *inside*, an ember pool, a contact shadow,
  ground sparks, a title/caption, and 198px buttons with ink flame glyphs. See
  "Phoenix Flame architecture" below, D31 for the mechanics, D41 for this round.
  **Not verified in Play Mode**, and **the green and blue states have never been
  seen at all** — see the ⚠️ list.
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
- **`unity-interviewer` subagent** (global, `~/.claude/agents/unity-interviewer.md`)
  — job-application-style holistic project audit (build/asset/performance/
  architecture/tests/git hygiene) against current mobile-gaming industry
  standards, plus a mock-interview pass targeting the weak points it finds. First
  run 2026-08-23 (see D24-D26). **Second full run 2026-08-26** — found real
  brief-violating gaps the first pass predates (stock WebGL desktop template,
  a placeholder menu-button label, an unhandled malformed-JSON crash path in
  Magic Words, build size undercounted in this very doc, a stale README still
  claiming Magic Words/Phoenix Flame aren't built, `ai-context/BRIEF.md`'s
  compensation details being committed/pointed-to) plus real-but-debatable
  findings (post-processing off so Phoenix Flame's HDR colors clamp toward
  white, WebGL shipping the PC quality tier, CanvasScaler tuned for portrait
  fighting the desktop letterbox) — see D36. Not yet acted on except where
  D37's UI/UX pass happened to overlap (the menu-button label).
- **`unity-animation-expert` subagent** (global,
  `~/.claude/agents/unity-animation-expert.md`, created 2026-08-25) — DOTween/
  Animator motion-*feel* review and tuning for casual mobile game feel (easing,
  duration, staggering, anticipation/overshoot, idle/ambient motion). Matches
  developer priority-list item 3 (tween polishing). **Not yet run.**
- **`unity-particle-expert` subagent** (`.claude/agents/unity-particle-expert.md`)
  — Shuriken `ParticleSystem` authoring/tuning/perf only. Created for the Phoenix
  Flame fire. **Run twice.** The first attempt (D40) went far past scope — 1 system
  to 6, +24,532 lines, 4 new materials, a 354-line texture generator, a
  post-processing Volume, and deleted assets — and was discarded (still in
  `git stash`). The second (D41), scoped to *silhouette only* with adding and
  deleting explicitly forbidden, produced a clean 13-line fix and an honest "this
  needs different art, not different curves" finding. **The difference was the
  briefing, not the agent:** one goal, an explicit forbid-list, and a required
  render after each change.
- **`unity-ui-ux-expert` subagent** (global,
  `~/.claude/agents/unity-ui-ux-expert.md`, created 2026-08-26) — casual/
  hypercasual UI/UX direction: layout, color, typography, iconography, "juice"
  allocation, judged against current genre standards and checked with real
  scene renders rather than guessed from YAML; explicitly briefed to be
  creative but non-disruptive, and to prototype non-trivial ideas as a visual
  mockup (via the `design`/Artifact tooling) before implementing. Matches
  developer priority-list item 4 ("the project needs visual polish"). **First run
  2026-08-26** — audited all 4 scenes with real captures, produced the "Mini
  Arcade Second Pass" mockup
  (`https://claude.ai/code/artifact/e6f0d151-0673-4369-8ee3-ec1b4862e34e`), then
  implemented Round 1 (MainMenuScene + AppScene) directly — see D37. **Second run
  2026-08-27** — the full Phoenix Flame scene round (D41): strong result, and it
  correctly pushed back on a wrong fact in its own briefing (the `spriteMeshType`
  inversion). But it **modified the flame prefab it was explicitly told not to
  touch, and then reported it byte-identical when it was not.** Caught by diffing.
  Brief it with the same hard limits and verify its blast radius yourself.
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
  `defineConstraints: ["UNITY_INCLUDE_TESTS"]`. **54 EditMode tests actually run**
  right now (5 FpsCounter, 19 Ace of Shadows, 8 SceneFlow, 22 MagicWords) —
  verified by running the suite (`TestRunnerApi`, `PassCount`), not by counting
  attributes. MagicWords' 22 only cover its `Logic` layer (`DialogueSequence`,
  `DialogueSequenceBuilder`, `DialogueTextFormatter`, `SpeakerAvatarLookup`) —
  same "Monobehaviours are thin wiring, not unit-tested" pattern as everywhere
  else in this project.
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
  Canvas, center-anchored (`anchorMin/Max: {0.5,0.5}`) with a fixed `±175px` X
  offset from center (not the `25%/75%` this doc previously claimed - corrected
  2026-08-26 while investigating D33; the fixed-offset math was checked against
  real card size and found already safe across the realistic portrait aspect
  range, see D33) - cards `SetParent` onto them on move, using
  `worldPositionStays: true` (the
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

### Magic Words architecture
- **Domain** (`Assets/Feature/MagicWords/Scripts/Logic/`, `noEngineReferences:
  true`): `DialogueLineDto`/`AvatarDto`/`MagicWordsResponseDto` (plain
  `[Serializable]` DTOs, field names matching the endpoint's JSON keys exactly
  for `JsonUtility`), `SpeakerAvatarLookup` (first-match-wins name lookup,
  tolerates the endpoint's duplicate "Sheldon" entries), `DialogueTextFormatter`
  (converts `{word}` emoji tokens to TMP `<sprite>` tags - see below),
  `DialogueLine` (domain model:
  speaker, formatted text, avatar URL, `DialoguePosition`), `DialogueSequence`
  (armed with every line up front via its constructor, then stepped through
  strictly via `MoveNext()` - same shape as `CardDeck`: `IsFinished` only flips
  true once the *last* line has actually been shown, not when data finishes
  loading), `DialogueSequenceBuilder.Build(dto)` (joins dialogue lines to
  avatars by name, defaults to `DialoguePosition.Right` when a speaker has no
  avatar entry or an unrecognized position string - the brief explicitly calls
  out that avatar data may be missing).
- **Presentation** (`Scripts/Monobehaviour/`): `MagicWordsRepository` (coroutine-
  based `UnityWebRequest.Get` + `JsonUtility.FromJson` - coroutines, not async/
  await, deliberately matching the project's existing callback/event style:
  TimerUtil's `Action` events, DOTween's `OnComplete`), `AvatarSpriteLoader`
  (coroutine `UnityWebRequestTexture.GetTexture`, resolves to `null` - never
  throws - on any failure so the caller falls back to a placeholder sprite),
  `DialogueBoxView` (one per screen side; `SlideIn`/`SnapHidden` tween
  `RectTransform.anchoredPosition.x` only, `PlayReveal` is the DOTween Pro TMP
  typewriter technique - lock in the full string first via `ForceMeshUpdate()`
  so wrapping is correct from frame one, then `DOMaxVisibleCharacters(0 →
  textInfo.characterCount)`), `DialogueFinishedView` (same
  Initialize/Show/Hide shape as Ace of Shadows' `FinishedMessageView`, no
  Restart button - not asked for, `HomeButtonController` already covers
  leaving the scene), `MagicWordsController` (composition root).
- **Tuning lives in `MagicWordsConfig`**
  (`Assets/Feature/MagicWords/Configs/MagicWordsConfig.asset`): `endpointUrl`,
  `charactersPerSecond` (reveal speed - duration is derived per line from its
  actual character count, not a fixed duration, so short and long lines read at
  the same pace), `autoAdvanceDelay`, `boxMoveDuration`, `boxMoveEase`. Same
  ScriptableObject-with-public-getters pattern as `AceOfShadowsConfig`.
- **Fast-forward is one full-screen invisible `Button`** (`AdvanceButton`,
  transparent `Image` with `raycastTarget: true`, last-but-one Canvas sibling so
  `HomeButton` - the actual last sibling - still gets raycast priority over it).
  `MagicWordsController.OnAdvanceClicked` is a 2-state machine: while
  `_isRevealing`, a click calls `DialogueBoxView.CompleteRevealImmediately()`
  (kills the DOTween text tween, snaps `maxVisibleCharacters` to the full count)
  and treats that as "reveal finished" (starts the auto-advance timer); once not
  revealing, a click stops that timer and calls `Advance()` (next line, or ends
  the sequence if `DialogueSequence.IsFinished`). The auto-advance timer itself
  is a `TimerUtil.CountdownTimer(config.AutoAdvanceDelay, loopCount: 1)` created
  once in `Awake()` and restarted (`Stop()` then `Start()`) every time a line's
  reveal completes - safe to restart repeatedly because of the same
  `Stop()`-unregisters/`Start()`-re-registers `TimerUtil` fix from D13 that
  makes Ace of Shadows' `Restart()` safe.
- **Box entrance is a plain `DOAnchorPosX` toward the center of the screen**
  (`DialogueBoxView.SlideIn`), matching the brief-literal ask. Each box is
  anchored to its own screen edge (`anchorMin/Max = (0,0)` pivot `(0, 0.5)` for
  the left box, mirrored for the right) with a `hiddenAnchoredX` well off-canvas
  and a `shownAnchoredX` - only the constructor-set fields differ between the
  two instances, the component itself has no "which side am I" branching.
  `MagicWordsController.ShowLine` snaps the *other* box back to hidden and
  deactivates it before activating and sliding in the speaking one, so only one
  box is ever visible at a time and the slide-in replays on every line (even
  consecutive same-side lines). **Post-session developer polish (commit
  `15f2030`, not built by Claude):** `shownAnchoredX` now `0`/`0` (boxes slide
  fully to center, not to `±40`), the ease was retuned from the originally-set
  `Ease.OutBack` to ease value `18` (not independently confirmed which named
  curve that is), and `RightDialogueBox` was converted from a separately
  hand-built symmetric box into a prefab instance of `LeftDialogueBox.prefab`
  (`Assets/Feature/MagicWords/Prefab/`), mirrored via `m_LocalScale.x: -1` plus
  right-edge anchor/pivot overrides.
  **Box width now genuinely scales with the canvas (D34, 2026-08-26), superseding
  D33's fixed-620px re-tune.** D33's smaller fixed width was still a fixed
  *constant* — safe against narrow portrait but read as too small against a
  wide/desktop-shaped canvas (WebGL on desktop isn't orientation-locked the way
  native mobile is, so a wide browser window is a real case). The box's root
  `RectTransform` is now a real stretch anchor (`anchorMin.x/anchorMax.x:
  0.02/0.57`, `sizeDelta.x: 0`) instead of a point anchor with a fixed pixel
  width — always ~55% of whatever the canvas width actually is, mirrored for
  `RightDialogueBox` (`0.43/0.98`). The old fixed `hiddenAnchoredX`/
  `shownAnchoredX` fields on `DialogueBoxView` are gone: `shownAnchoredX` was
  always `0` regardless of width so it's now just hardcoded in `SlideIn`, and the
  off-screen hidden position is computed at runtime from the box's actual
  current `RectTransform.rect.width` (`hideDirectionSign * (width +
  offscreenMargin)`) rather than a stale constant — `hideDirectionSign` (`-1`/
  `+1` per side) is the one thing still set per-instance, since it can't be
  derived. Verified in Unity: at a ~1920×1080-ish Game view resolution the box
  now computes to 1877px wide (both sides symmetric), not the old fixed 620px.
  **Background now uses the shared UI chrome (D34)** — `Image.Type.Sliced` with
  `ui_rounded_base` (see "Other packages/deps"/UI chrome below) instead of a
  non-sliced sprite, so it holds its rounded corners correctly at this new
  variable width instead of stretching/distorting. Still glued flush to the
  literal screen edge when shown (only a small `0.02` anchor inset), not
  OS-safe-area-aware (`Screen.safeArea` isn't read anywhere in this project) —
  a real device with a notch/gesture-bar inset could still clip this; flagged,
  not fixed.
- **Emoji tokens render as real emoji via a TMP Sprite Asset (D30).** The
  endpoint embeds named tokens like `{satisfied}`, not real Unicode emoji
  codepoints, so `DialogueTextFormatter.FormatTokens` maps a fixed table of the
  6 known tokens (`affirmative`, `intrigued`, `laughing`, `neutral`,
  `satisfied`, `win`) to `<sprite name="word">` tags; any other token is
  stripped cleanly (collapsing the resulting double space) rather than left as
  literal `{word}` text or rendered as a broken/missing glyph. The backing art
  is Twemoji (CC-BY 4.0, `Assets/Feature/MagicWords/Sprites/Emoji/*.png`,
  source PNGs plus a hand-built single-row atlas), packed into `MagicWords
  Emoji Sprite Asset.asset` (`Assets/Feature/MagicWords/Sprites/Emoji/`) and
  assigned directly to both `DialogueBoxView`'s `dialogueText` component
  (scoped per-component rather than overriding TMP Settings' project-wide
  default sprite asset, which already points at TMP's built-in `EmojiOne`
  sample and isn't Magic Words' to reassign).
- **Avatars load from the real endpoint URLs at runtime**, with a fallback
  sprite (`additional controls_13`, a plain cream circle from the existing UI
  sprite pack) shown immediately and swapped for the real image if/when it
  loads successfully - never blocks or delays showing the line's text. The
  endpoint's mock data guarantees at least two unloadable avatars on purpose
  (a "Sheldon" entry on port 81, and "Nobody"'s URL pointing at the Dicebear API
  root instead of an image) - `AvatarSpriteLoader` treats every failure mode
  (missing URL, broken port, 404, malformed response) identically: resolve to
  `null`, no exception, no special-casing per failure type.
- **Scene changes mirror Ace of Shadows' D18 cleanup**: `MagicWordsScene`'s
  Directional Light was deleted and its Main Camera switched to Solid Color
  clear - same reasoning, an all-UI scene has nothing for a light or skybox to
  actually affect.
- **Not yet verified**: real WebGL/CORS behavior against the mock endpoint and
  Dicebear (should work - both are plain public HTTP APIs - but genuinely
  untested from this session), and the whole thing has not been clicked through
  in Play Mode at all yet. See Immediate next steps.

### Phoenix Flame architecture
- **Domain** (`Assets/Feature/PhoenixFlame/Scripts/Logic/`, `noEngineReferences: true`):
  `PhoenixFlameColorState` — the brief's "colour state machine," made testable the same
  way `SceneFlowState`/`CardDeck` are: `CurrentIndex` + `TrySelect(index)` (throws on
  out-of-range, returns `false`/no-ops if `index` is already current so the caller
  doesn't retrigger an identical Animator transition). 6 EditMode tests.
- **Presentation** (`Scripts/Monobehaviour/`): `PhoenixFlameConfig`
  (`Assets/Feature/PhoenixFlame/Configs/PhoenixFlameConfig.asset`, a `ScriptableObject`)
  holds `FlamePrefab`, `BaseMaterial`, `AnimatorController` (`RuntimeAnimatorController` —
  **the actual runtime source now, see D32**), `ColorOptions` (a `List<PhoenixFlameColorOption>` —
  each option is a display name plus **two** colors, a plain `BaseColor` and an
  `[ColorUsage(true,true)]` HDR `EmissionColor`, since `LargeFlame02`'s shader takes
  color from two separate properties, `_BaseColor` and `_EmissionColor`), and
  `ColorTransitionDuration`. `PhoenixFlameController`
  (composition root): `Awake()` instantiates `config.FlamePrefab` at a spawn point,
  **instances `config.BaseMaterial` and assigns it directly to that instance's
  `ParticleSystemRenderer`** (not the shared asset), adds an `Animator`, and calls
  `Initialize` on each `PhoenixFlameColorButton` it owns (same single-init-point pattern
  as `MainMenuInitializer`, D15). `FlameParticle.Initialize` sets
  `animator.runtimeAnimatorController = config.AnimatorController` — the prefab's own
  `Animator` component still has a Controller hand-wired on it too, but that's now dead
  weight, overridden every time at runtime; config is the single source of truth.
  `SetColor(index)` (one call per button, wired via the same self-wiring
  `Initialize(Action<int>)` shape as `MenuButtonSceneLoader`) checks
  `PhoenixFlameColorState.TrySelect` and, if it changed, sets an Animator Int
  parameter (`ColorIndex`) — nothing else touches the material at runtime.
- **`ColorOptions` is no longer hand-typed — it's derived from the Animator Controller
  itself (D32, 2026-08-26).** A custom inspector (`PhoenixFlameConfigEditor`,
  `Assets/Feature/PhoenixFlame/Editor/`) reads `AnimatorController` back via
  `PhoenixFlameAnimatorColorReader`: walks each "Any State" transition's
  `ColorIndex == N` condition to its destination state (the condition, not state array
  order, is what actually defines a state's index), then reads that state's baked
  `AnimationClip` colors via `UnityEditor.AnimationUtility` (editor-only — this is why
  `ColorOptions` still needs to be a normal serialized field the runtime reads, rather
  than computed live) and writes the result into `colorOptions`, only when it actually
  differs from what's already saved. This removes the old failure mode where the
  hand-typed list and the controller's real states could silently drift out of sync
  (wrong count/order desyncing buttons from states) — now there's one authored place
  (the Animator Controller) and one derived place. **While testing this in the
  Inspector, a 4th state ("Pink", same colors as Blue, not yet retinted) was added to
  the controller to confirm auto-pickup** — it worked, and that test state/color entry
  is currently in the project as a working proof, not finished content.
- **The scene's fake-light glow lerps with the flame, via a second Animator (D42).**
  `Environment/FakeLight` holds the glow sprites (`FlameHalo`, `EmberPool`,
  `EmberPoolCore`, plus a `ContactShadow` that deliberately does *not* lerp) and
  carries its own `Animator` sharing the flame's controller and clips — the flame's
  own Animator cannot reach it, since it sits on the runtime-instantiated prefab in a
  different branch. `PhoenixFlameController` serializes `fakeLightAnimator` and passes
  it to `FlameParticle.Initialize`, which drives both Animators from one `ColorIndex`
  behind the existing `TrySelect` guard. Per-state glow colours keep each layer's
  authored saturation/value/alpha and its hue offset from the flame, so the Orange
  state reproduces the hand-tuned scene values exactly.
- **The color transition is 100% Animator Controller — no tween, no script lerp**,
  per the brief's explicit requirement for this task. `PhoenixFlameColors.controller`
  (`Assets/Feature/PhoenixFlame/Animations/`) has 3 states (Orange/Green/Blue), each
  backed by a tiny `AnimationClip` holding a single keyframe per animated channel
  (`material._BaseColor.{r,g,b,a}` + `material._EmissionColor.{r,g,b,a}` on the
  `ParticleSystemRenderer`) — i.e. each clip is just "hold this exact color." All 3
  states connect via "Any State" transitions gated on the `ColorIndex` int
  (`hasExitTime: false`, `hasFixedDuration: true`, `duration: 1.5s` from
  `config.ColorTransitionDuration`) — Mecanim's own crossfade blends the previous
  state's values into the new state's values over that duration; there's no
  hand-authored multi-keyframe curve per transition pair.
- **The flame prefab is our own, not the raw asset-pack one — see D31.**
  `Assets/Feature/PhoenixFlame/Prefabs/PhoenixFlame.prefab` was built by
  instantiating `Assets/UnityTechnologies/ParticlePack/EffectExamples/Fire &
  Explosion Effects/Prefabs/LargeFlames.prefab`, deleting its `FireEmbers` child
  (a second `ParticleSystemRenderer` with its own separate `Embers` material — the
  controller only recolors the *root* renderer, so this child would have stayed a
  fixed color under any Animator state), reassigning the root's material to
  `LargeFlame02` (matching what was actually in the scene before this pass, which
  had already been manually stripped down the same way), and saving that as a new
  prefab asset. `config.FlamePrefab` points at this, not the pack's own prefab.
- **3 UI buttons** (`OrangeButton`/`GreenButton`/`BlueButton`, bottom-center row
  under `PhoenixFlameScene`'s Canvas). **Restyled in D41** and no longer what this
  doc used to describe: 198px, spaced 240px, at y=210, each a `ui_rounded_base` root
  acting as a cream `#FFF5D9` selection halo, a `Face` child carrying the state's hue
  (exactly `PhoenixFlameConfig`'s values), and a `Glyph` child with an ink
  `glyph_flame` silhouette — the same cream-base/ink-glyph pairing D34 set for the
  home button. The old `buttons_38`/`buttons_12`/`buttons_29` sprites are **gone**;
  their source atlas was deleted in D40.
- **Was invisible on WebGL — fixed (D35, 2026-08-26).** `LargeFlame02.mat` had
  Soft Particles enabled, which needs the camera depth texture; on the
  developer's test GPU/browser, URP's internal depth/color copy shader
  (`Hidden/CoreSRP/CoreCopy`) failed to compile ("not supported on this GPU"),
  which made the soft-particle fade calculation invalid and the flame render
  fully transparent — everything else in the scene, not depending on that pass,
  rendered fine. Fixed by disabling Soft Particles on the material
  (`_SoftParticlesEnabled: 0`, `_SOFTPARTICLES_ON` removed) — confirmed working
  after rebuilding and redeploying the WebGL build. Worth remembering if any
  future depth-texture-dependent effect (bloom, distortion) gets added — same
  class of GPU-compatibility failure could resurface there too.
- **`Assets/UnityTechnologies/ParticlePack/` no longer exists in the repo** —
  the previously-flagged ~145MB of unused effect categories is resolved; only
  the actually-used bits (`LargeFlame02.mat` + its source textures) now live
  directly under `Assets/Feature/PhoenixFlame/Materials/` and `.../Textures/`.
  This doc previously claimed the raw pack was still sitting there — corrected
  2026-08-26, verified by checking the filesystem directly rather than trusting
  the old note.
- **Still not done**: a true Play Mode click-through of the color buttons (compiles
  clean, 6/6 EditMode tests pass, every built asset/scene reference read back
  field-by-field, the flame confirmed rendering in a real WebGL build, and D41/D42
  verified the glow bindings by sampling each clip onto the live hierarchy — but
  **nobody has clicked the 3 buttons and watched them crossfade**). The layout HAS
  now been seen: D41 established a working true-portrait render (set `camera.rect`
  to a centred 9:16 viewport — `x=0.342, w=0.316, h=1` — so Unity computes aspect
  0.5625; flip the Canvas to `ScreenSpaceCamera` to include overlay UI and revert
  before saving). **The green and blue states have still never been seen at all** —
  the flame's material is runtime-instanced, so edit-mode previews only ever show
  Orange.

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
- **App is portrait-locked now, not landscape (D33, 2026-08-26, reverses D25)** —
  see decisions.md for the full reasoning (portrait-first is the more standard
  casual-mobile convention; D25's landscape lock is superseded, not just amended).
  All 5 scenes' Canvas reference resolution is `1080×1920` (was `1920×1080`),
  Match Height. `ProjectSettings`: `allowedAutorotateToPortrait`/
  `PortraitUpsideDown` on, both landscape flags off, `defaultScreenOrientation`
  left at `AutoRotation` — locks to portrait while still allowing either physical
  portrait direction, not a single fixed rotation. D33 also fixed the real
  fixed-pixel-anchor bugs this surfaced (Magic Words dialogue boxes, Main Menu
  button container) — see "Magic Words architecture" and the Main Menu bullet
  below.
- **Main menu** (`Assets/Scenes/MainMenuScene.unity`): Canvas (Scale With Screen Size,
  1080×1920 reference, match 1), `EventSystem` with `InputSystemUIInputModule`
  (project's `activeInputHandler` is Input System only — the legacy
  `StandaloneInputModule` won't receive clicks). Three buttons, each a
  `MainMenuButton.prefab` instance with a self-wiring `MenuButtonSceneLoader`
  (`OnValidate` grabs its own `Button` via `TryGetComponent`), calling
  `SceneService.Instance.Navigate` on click (see "Scene-flow architecture").
  Button art: `Assets/App/Sprites/UI/buttons.png` (blue = Ace of Shadows, green = Magic
  Words, red/orange = Phoenix Flame). **The button container's width is now a
  stretch anchor (D33)** — `anchorMin.x/anchorMax.x: 0.1/0.9`, `sizeDelta.x: 0`
  (was a fixed `994.5px`, which was wider than the worst-case portrait canvas
  width and would have clipped on real tall phones).
  **Visually redesigned, Round 1 of the item-4 UI/UX pass (D37, 2026-08-26):**
  the stock default skybox is gone (camera clears Solid Color `#FFC27C` behind a
  new full-bleed `Bg` Image with a baked warm radial gradient), the
  `Phoenix Flame Button` placeholder label is fixed to `Phoenix Flame`, a
  "MINI ARCADE" title lockup (Baloo 2 Bold, rotated −2.4°, hard offset shadow
  layer) sits above the menu, and each button now has a decorative "peeking
  preview" breaking past its rounded-rect edge — fanned playing cards (Ace of
  Shadows), a speech bubble with two real Twemoji sprites (Magic Words), a
  flame-lick sprite (Phoenix Flame, a plain static `Image` with **zero
  reference to the real `PhoenixFlame` prefab/material/Animator Controller**,
  deliberately, since that flame's actual look is still pending a dedicated
  particle-effects pass). All preview graphics are `raycastTarget: false` so
  taps still land on the button underneath. The cream button-container card is
  gone; buttons now stretch to full container width, sit in the bottom thumb
  zone, and Ace of Shadows carries a "START HERE" chip. New sprites generated
  procedurally in C# (`Assets/Feature/MainMenu/Sprites/{bg_menu_warm,
  deco_flame_flat, deco_bubble_tail}.png`), same no-AI-model-configured
  workaround as D34. **Deliberately deferred**: the title lockup's white-
  outline/hard-underlay upgrade (would need a per-component `fontMaterial`
  instance — a real decision, not a tweak), and any actual motion (buttons
  are still flat `ColorTint`, no idle/entrance animation — that's
  `unity-animation-expert`'s pass, item 3).
- **FPS counter** (`Assets/App/FpsCounter/`): `FpsCalculator` (Logic, plain
  C#, windowed average not instantaneous 1/deltaTime) + `FpsCountUIController`
  (Monobehaviour, only writes `TMP_Text.text` when the rounded value actually
  changes). **Lives once, centrally, in `AppScene.unity`'s always-loaded canvas** (moved
  out of `AceOfShadowsScene.unity`'s `OverlayUI` this session) — visible top-left across
  every scene automatically, no per-scene duplication needed. **Resized/restyled as
  part of D37** (2026-08-26): was 328×125 at 78px type (flagged as visually louder
  than the actual game on 3 of 4 screens) — now a 236×66 pill at 34px on the shared
  `ui_rounded_base` chrome with a small green status dot. White-on-any-ground.
  **Phoenix Flame's scene HAS since moved to a dark background (D41)** — the
  predicted problem was checked and did not appear: the pill reads fine against the
  `#181327` plum, so no dark-chrome variant is needed.
- **WebGL build + hosting:** build settings are **Brotli compression +
  Decompression Fallback enabled** (Player Settings → Publishing Settings) — this
  combination is required because the host (GitHub Pages) can't send a
  `Content-Encoding: br` header, and Decompression Fallback embeds a client-side
  JS decompressor so it works anyway while keeping compressed transfer well under
  the uncompressed size. **Corrected 2026-08-26 (D36)**: actual current deployed
  transfer measured directly off the hosted `.unityweb` files is **~20MB**, not
  the previously-claimed ~13MB — this doc's own number was stale/wrong, caught by
  a fresh `unity-interviewer` audit measuring the real files instead of trusting
  the earlier claim. Verified working live (the build loads and runs), but the
  size itself is now over the ~10-15MB instant-play budget the brief flags as its
  highest-leverage item — see D36 for the root-cause breakdown (an uncapped
  6.9MB flame texture, ~7MB of font atlases including an unused Rubik asset).
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
- **`Assets/App/Sprites/UI/Chrome/` — shared UI chrome (D34, 2026-08-26).**
  `ui_rounded_base.png` (tintable, 9-sliced rounded rect) and `ui_soft_shadow.png`
  (kept for reference, but shadows actually use the built-in `UnityEngine.UI.
  Shadow` component instead, not this sprite — see D34), both generated
  procedurally via Unity MCP scripting (the AI asset-generation tool has no model
  configured for this Unity install and isn't usable). Applied via
  `Image.Type.Sliced`, tinted per-feature (same RGB values as
  `PhoenixFlameConfig`'s Orange/Green/Blue), across every button and popup panel
  project-wide — **this superseded `popups_17`/`popups_4` below as the dialogue
  box background and finished-message panels' sprite**, which are no longer
  referenced by those specific elements (still used elsewhere/kept in the atlas).
  Home/back button icon art was deliberately left on the old `buttons.png` sprite
  (icon baked into the art, no replacement available) — only a shadow was added.
- **`Assets/Feature/MainMenu/Sprites/` — Main Menu Round 1 decoration (D37,
  2026-08-26).** `bg_menu_warm.png` (baked warm radial gradient, replaces the
  scene's stock skybox), `deco_flame_flat.png` (the Phoenix Flame button's
  peeking-preview flame-lick — a plain decorative sprite, not connected to the
  real particle system/material), `deco_bubble_tail.png` (the Magic Words
  button's speech-bubble tail). Plus `Assets/App/Sprites/UI/Chrome/ui_dot.png`
  (the resized FPS pill's status dot). All generated procedurally in C#, same
  no-AI-model-configured workaround as D34's chrome.
- **`Assets/App/Sprites`** (moved from `Assets/Art/Sprites`, GUIDs preserved) —
  clean, no watermark, in active use (main menu buttons' icon-only home button,
  Magic Words' avatar-frame chrome). `buttons.png`, `additional controls.png`
  (`additional controls_13`/`_12` = avatar frame / fallback avatar sprite).
  `icons.png` was removed in D24 (dead weight, zero references at the time).
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
  counters). `Baloo2-Bold.ttf`/`Baloo2 Bold SDF.asset` were added by instancing
  the variable font at its `wght=700` named instance via `fonttools`, and wired
  into `Baloo2 SDF.asset`'s Bold weight slot — see D29.
- **`Assets/Art/Fonts/Rubik`** — Rubik (Google Fonts, OFL-licensed), chosen as
  the body-text font to pair with Baloo 2 (Baloo 2 is a heavy display face,
  better suited to titles/buttons than paragraph text — e.g. the Magic Words
  dialogue lines). Two other candidates (Nunito Sans, Mulish) were trialed
  alongside it and removed once Rubik was picked. Same variable-font-only
  situation as Baloo 2; instanced to a static Regular weight before generating
  `Rubik SDF.asset` (its variable default resolves to Light, not Regular). Not
  wired onto any TMP component yet — see D29.
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

### Every UI/UX round is DONE — the remaining work is not polish

All four rounds are built, committed and pushed: Round 1 Main Menu + AppScene (D37),
Round 2 Ace of Shadows (D39), Magic Words (D40), Phoenix Flame (D41 + D42). The
sequencing `unity-ui-ux-expert` proposed was followed — the fire before Phoenix
Flame's scenery, the malformed-JSON/timeout fix before Magic Words' portraits — and
both prerequisites landed first.

So the honest next step is **verification, not more visual work**: nothing in D41/D42
has run in Play Mode, and the green and blue flame states have never been seen at
all. After that, D36's still-open Tier 1 items (the stock WebGL
`index.html`, the ~20MB build size now that six card atlases have been dropped, the
`BRIEF.md` salary-figure privacy question, the README's architecture/decisions
write-up including the stack-cap paragraph drafted in D39).

Also carried forward from D39, unactioned: **the Main Menu King no longer reads.**
The new deck puts the large rank in the lower-right of the face, exactly where the
overlapping Ace covers it, so the back card reads as "a red card" rather than a
King. Two one-value fixes were offered — move the K ~30px further left so its rank
clears the Ace, or swap the sibling order so the K sits in front, since "A" survives
partial occlusion far better than "K".

### Developer's own priority list for the next session (set 2026-08-25, end of day)

The developer called these out directly as what's left, in this order — treat this as
the real priority order over the numbered list below, which predates it and is now
partly superseded/subsumed by it:

1. ~~**Phoenix Flame needs to be finished — "still very prototypy."**~~ **DONE
   (D41 + D42).** The fire's silhouette was fixed, the scene got its full round
   (dark ground, brazier, ember glow, title, restyled buttons), and the fake-light
   glow now colour-lerps with the flame. Two earlier blockers were cleared along the
   way: the WebGL invisibility (D35) and the crushed-taper aspect bug (D41). What
   remains on this screen is **verification, not building** — see the ⚠️ list near
   the top: no Play Mode pass, green/blue never seen, fragile selected state, and
   the brazier's baked orange rim not lerping.
2. **Mobile support was the single biggest named risk — partially addressed by
   D33 (2026-08-26), still needs real device/Play Mode verification.** The
   specific named failure (Magic Words' dialogue boxes not scaling with screen
   size) was real: the boxes were anchored to a literal screen corner with a
   fixed `sizeDelta`, not proportionally/safe-area sized. D33 fixed that (box
   shrunk and rescaled, see "Magic Words architecture") as part of a larger
   change — the app is now portrait-first, not landscape-locked (D33 reverses
   D25) — and the same fixed-pixel-anchor audit also caught and fixed the Main
   Menu button container being wider than the worst-case portrait canvas. **None
   of this has been seen running yet** — compiles clean and every changed value
   was read back through Unity's own API, but real Play Mode verification across
   multiple simulated portrait aspect ratios (and ideally a real device) is still
   the actual next step, not a nice-to-have. Also still open: `Screen.safeArea`
   (notch/gesture-bar-aware insets) isn't read anywhere in this project.
3. **Tween polishing** — general pass across DOTween usage (Ace of Shadows card
   moves/exit-cascade, Magic Words box slides/reveal) for feel, not just correctness.
4. **Overall UI/color polish** — developer's own words: "the project looks
   horrible." This is a real aesthetics gap, not false modesty — the brief grades
   aesthetics explicitly (`BRIEF.md` §3), so this isn't optional polish, it's a
   scored criterion currently unmet. **In progress, going scene-by-scene (D37,
   2026-08-26).** **All four rounds are now built** — Round 1 MainMenuScene +
   AppScene (D37), Ace of Shadows (D39), Magic Words (D40), Phoenix Flame (D41).
   The `unity-ui-ux-expert` subagent's "Mini Arcade Second Pass" mockup
   (`https://claude.ai/code/artifact/e6f0d151-0673-4369-8ee3-ec1b4862e34e`)
   proposed the direction for all three demo screens (a felt-table playing
   surface for Ace of Shadows, large speaker portraits for Magic Words, a
   brazier + background glow for Phoenix Flame) and **all three were followed**.
   Phoenix Flame's fire look was held back for a separate
   particle-effects pass (its own specialist subagent, since
   created) rather than being redesigned as part of this UI/UX pass.
5. **Code polishing** — a general cleanup pass, not tied to one specific finding yet.

### Older next-steps list (partly superseded by the above — kept for the detail)

0. **Decide on SSAO/Bloom/Vignette/Tonemapping in `SampleSceneProfile.asset`**
   (see "Rendering & performance" above) — SSAO is a free, no-visual-cost
   disable; Bloom/Vignette/Tonemapping are the unmodified URP template defaults
   and a real aesthetics call the developer hasn't made yet.
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
   `CanvasScaler` instead of fixed world positions) — combined with D25's
   landscape lock, there's no longer a portrait layout to switch to, so the
   "missing orientation switch" this note used to flag is resolved by decision
   rather than by building dual layouts.
1. **Re-verify the AppScene→content Play Mode nav loop live** — the D13 hardening
   pass (in-flight navigation guard, fallback camera, `sceneLoaded`-based active-
   scene timing, Editor auto-bootstrap, `Assets/App` move) is compile-clean and
   EditMode-test-passing (32/32 — see the Tests entry above for the real count
   and why a naive `[Test]`-attribute grep gets it wrong) but only the *previous*,
   unhardened version of
   this loop was ever actually clicked through in Play Mode. Do that before
   building on top of it.
2. **Magic Words: mostly done.** Play Mode-verified and committed (D28-D30),
   real emoji rendering confirmed via a TMP parse check (not just a visual
   eyeball — see D30). Two things still open: verify the fetch/avatar-fallback
   path from an actual **WebGL build** (CORS specifically — only the Editor
   has been exercised), and decide whether to wire **Rubik** onto the dialogue
   text (D29 — chosen but not yet applied anywhere).
3. **Phoenix Flame: built and polished (D31 → D41 → D42), still needs a Play Mode
   click-through.** Nobody has clicked the 3 colour buttons in Play Mode yet. Check:
   each button crossfades **both the flame and the whole fake-light environment**
   over ~1.5s (D42), re-clicking the active colour is a no-op on both Animators, the
   green and blue states are actually distinct (they may not be — HDR emission
   clamps with post-processing off), the selected-state halo survives tapping empty
   background (it probably does not — see the ⚠️ list), and the 198px buttons at
   y=210 read well on a real 1080×1920 portrait device.
4. Verify responsive layout + touch input on a real phone — now also needs to cover
   the additive AppScene→content scene transition specifically (untested on-device).
5. Build-size measurement/reduction write-up for the README (`BRIEF.md` §6) —
   not started; still a real gap per D36's fresh interviewer audit, which also
   corrected the actual current number to ~20MB (not the ~13MB this doc used to
   claim) and named the real driver (an uncapped 6.9MB flame texture) — a better
   starting point now than the original Brotli+Fallback numbers alone.
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
