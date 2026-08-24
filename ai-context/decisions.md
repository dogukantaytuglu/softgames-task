# Decisions Log

Newest first. Each entry: **what** was decided, **why**, and the date. This is the
"defend it out loud" reference — the brief is explicit that every decision in the
submission needs a real justification behind it (`BRIEF.md` §1, §7).

---

### D31 — Phoenix Flame: config-driven flame + Animator Controller color system, built directly per request (2026-08-25)
**Choice:** First real pass at Phoenix Flame, built directly per explicit developer instruction. Summary:
- **Domain** (`PhoenixFlame.Logic`, `noEngineReferences: true`): `PhoenixFlameColorState` - the "colour state machine" the
  brief calls out as testable domain logic. Holds `CurrentIndex`, `TrySelect(index)` (mirrors `SceneFlowState`/`CardDeck`'s
  shape: throws on out-of-range, returns `false`/no-ops on reselecting the already-current color so the caller doesn't
  retrigger an identical Animator transition). 6 EditMode tests.
- **Presentation** (`PhoenixFlame.Monobehaviour`): `PhoenixFlameConfig` (ScriptableObject) exposes `FlamePrefab`,
  `BaseMaterial`, a `List<PhoenixFlameColorOption>` (each option: display name + a plain `baseColor` and an
  `[ColorUsage(true,true)]` HDR `emissionColor` - `LargeFlame02`'s shader takes color from two separate properties,
  `_BaseColor` (multiplies the base map) and `_EmissionColor` (HDR, drives the glow/bloom), so one `Color` field per
  option wasn't enough), `AnimatorController`, and `ColorTransitionDuration`. `PhoenixFlameController` (composition
  root): on `Awake()`, instantiates `config.FlamePrefab` at a spawn point, **instances `config.BaseMaterial`
  (`Instantiate(...)`, not `sharedMaterial`) and assigns it directly to that instance's `ParticleSystemRenderer`** -
  the literal ask ("create an instance of the material and assign it to that flames renderer") - then adds an
  `Animator` pointed at `config.AnimatorController`. `SetColor(index)` (called by 3 `PhoenixFlameColorButton`s, one
  per screen color, same self-wiring-`Initialize(Action)` shape as `MenuButtonSceneLoader`) checks
  `PhoenixFlameColorState.TrySelect` and, if it actually changed, sets an Animator Int parameter (`ColorIndex`) -
  nothing else touches the material directly at runtime.
- **The actual color transition is 100% Animator Controller, no tween/script lerp anywhere** - this was almost not
  the case. Mid-build the developer asked to switch the transition mechanism to DOTween's `Material.DOColor` (a real,
  working alternative - confirmed `ShortcutExtensions.DOColor(Material, Color, string/int propertyID, float)` exists
  in DOTween's core precompiled DLL, no extra asmdef reference needed). Flagged before building it: `BRIEF.md` §5
  states outright for this exact task "⚠️ They specified an animator controller for the colour transitions. Not a
  tween, not a script lerp... this is a follow-the-spec check as much as a visual one" - a named grading risk, not a
  style nitpick. Put to the developer directly via a two-option-then-reconsidered question; the developer reversed
  back to Animator-only immediately after. Recorded here because a request that contradicts an explicitly-flagged
  brief requirement is exactly the kind of thing that needs to survive in the decisions log, even though (especially
  because) it didn't end up being what got built.
- **How "smooth transition between arbitrary states" works with zero lerp code:** each of the 3 states (Orange/
  Green/Blue) has its own tiny `AnimationClip` holding a *single* keyframe (at t=0) per animated channel - 8 curves
  total per clip (`material._BaseColor.{r,g,b,a}` + `material._EmissionColor.{r,g,b,a}`, bound via
  `AnimationClip.SetCurve("", typeof(ParticleSystemRenderer), propertyName, curve)`) - so each clip is really just
  "hold this exact color." The actual smoothing comes entirely from the **Animator transition's own crossfade**: all
  3 states connect via "Any State" transitions gated on the `ColorIndex` int (`AnimatorConditionMode.Equals`),
  `hasExitTime: false`, `hasFixedDuration: true`, `duration: 1.5s` (from `config.ColorTransitionDuration`) - Mecanim
  blends the previously-active state's values into the new state's values over that duration natively. This is the
  standard Mecanim technique for driving a simple property target through an Animator Controller without hand-authored
  multi-keyframe curves per transition pair.
- **Built via Unity MCP scripting** (`AnimatorController.CreateAnimatorControllerAtPath`, `SerializedObject` field
  wiring, scene GameObject construction), same tooling approach as D28/D21, for the same reason - reliable,
  GUID-preserving, read-back-verified construction instead of hand-edited YAML. Hit the already-documented `Image`-
  resolves-to-the-wrong-namespace and multi-top-level-type gotchas immediately, fixed the same documented ways.
- **A real bug caught before it shipped, not after:** the config's `FlamePrefab` was first pointed at the asset
  pack's own `LargeFlames.prefab` directly. That prefab turned out to have a child `FireEmbers` particle system with
  its *own* separate `Embers` material (and its root uses `LargeFlame01`, not `LargeFlame02`) - neither of which the
  Animator-driven recolor touches, since `PhoenixFlameController` only reassigns the root `ParticleSystemRenderer`'s
  material. The developer's original scene object (dragged in before this session) was already a stripped-down
  single-system copy with no embers child, using `LargeFlame02` - i.e. someone had already manually solved this
  exact problem once by deleting the embers child, and pointing the new config straight at the raw pack prefab would
  have silently reintroduced un-tinted orange embers under a "blue" flame. Fixed by building the project's own
  `Assets/Feature/PhoenixFlame/Prefabs/PhoenixFlame.prefab` - `LargeFlames.prefab` instantiated, `FireEmbers` child
  deleted, `LargeFlame02` reassigned as the root's material - and repointing `PhoenixFlameConfig.FlamePrefab` at
  that instead. Caught by reading the actual prefab structure back before wiring the scene, not assumed.
**Why:** Requested directly, in detail (config shape, material-instancing mechanics, per-color base+HDR color pair,
Animator Controller as the transition mechanism) - implemented per the same "genuine delegation → build it fully"
precedent as D28.
**Not done in this pass:** Play Mode verification (compiles clean, 6/6 new EditMode tests pass via `TestRunnerApi`,
the built assets and scene were read back and checked field-by-field, but nothing here has actually been clicked
through yet - developer's own call, per the established Play Mode testing workflow). The button layout (bottom-
center row, 150px, spaced 200px apart) is a first-pass placement, not verified against the 1920x1080 canvas by eye.
`Assets/UnityTechnologies/ParticlePack/` (192MB, dropped in by the developer before this session as raw sample
material) still has ~145MB of unused effect categories (Goop/Magic/Misc/Smoke/Water/Weapon/Legacy Particles) beyond
the `Fire & Explosion Effects` folder our new prefab still depends on (`LargeFlame02.mat` and its source textures) -
flagged, not trimmed, same "decide the levers deliberately" treatment build-size got in D26.

### D30 — Magic Words emoji tokens render as real sprites via Twemoji + a hand-built TMP Sprite Asset (2026-08-24)
**Choice:** Revisited D28's deliberate deferral. Fetched the 6 known `{word}` tokens actually used by the live
endpoint (`affirmative`, `intrigued`, `laughing`, `neutral`, `satisfied`, `win` - confirmed by scanning the full
dialogue array, not guessed) as 72x72 Twemoji PNGs (CC-BY 4.0), composited them into a single-row atlas texture
with Pillow (`Assets/Feature/MagicWords/Sprites/Emoji/MagicWordsEmojiAtlas.png`), sliced it into 6 named sub-sprites
via `TextureImporter.spritesheet`, and built a `TMP_SpriteAsset` from it via editor scripting (TMP's own "Create
Sprite Asset" menu command is selection-driven and not scriptable directly, so `TMP_SpriteAssetMenu`'s internal
logic - glyph/character table construction, default material creation - was replicated by hand against the public
API). `DialogueTextFormatter.StripTokens` became `FormatTokens`: a fixed token→sprite-name table now emits
`<sprite name="word">` for the 6 known tokens, falling back to the old strip-and-collapse behavior for anything
else (defensive - avoids a broken glyph if the endpoint ever adds a 7th token). Verified two ways: 12 manual
assertions against the exact same expected strings the EditMode tests encode (covering known/unknown/mixed/empty/
null cases at both the formatter and `DialogueSequenceBuilder` integration level), plus a direct TMP parse check
(`TMP_Text.textInfo.characterInfo`) confirming `<sprite name="satisfied">` actually resolves to a visible sprite
element against the real asset, not just that the string looks right. The sprite asset is assigned directly to
both `DialogueBoxView`'s `dialogueText` component rather than overriding TMP Settings' project-wide default sprite
asset (which already points at TMP's built-in `EmojiOne` sample) - narrower blast radius, and nothing else in the
project uses `<sprite>` tags.
**Why:** `BRIEF.md` itself flags inline emoji as "the biggest time risk" and prescribes exactly this route (a TMP
Sprite Asset mapping tokens to a sprite atlas) - D28's deferral was about not shipping it half-built inside an
already-large session, not a decision that it wasn't worth doing. Twemoji was chosen over generating custom art
(discussed with the developer directly) for the same reason OFL fonts were used: an established, zero-risk open
license, "nice enough" per the developer's own call, and the fastest path to a working, testable result. Scoping
the sprite asset to the two dialogue `TMP_Text` components instead of the shared TMP Settings default follows the
same reasoning as D29's per-object Baloo Bold wiring: touch shared project state only when a change genuinely
needs to be global.
**Not done:** EditMode-test-runner confirmation via `TestRunnerApi` was attempted but blocked by the Unity MCP
tool itself ("User interactions are not supported for MCP tool calls") - a new restriction not seen earlier in this
same session (D28's Play Mode pass used this exact technique successfully). Worked around with the manual
assertion + TMP-parse verification described above rather than leaving the change unverified; the EditMode test
files themselves were still updated and compile clean, so a future session with the runner available should get
the same "all green" result trivially.

### D29 — Baloo 2 Bold via variable-font instancing; Rubik added as the body-text pair (2026-08-24)
**Choice:** Used `fonttools` (`pip install fonttools`, not previously a project dependency) to instance
`Baloo2-Variable.ttf` at its named `wght=700` instance, producing a static `Baloo2-Bold.ttf`, then generated
`Baloo2 Bold SDF.asset` the same way `Baloo2 SDF.asset` was originally made (`TMP_FontAsset.CreateFontAsset`,
dynamic atlas, same material/texture sub-asset wiring) and wired it into `Baloo2 SDF.asset`'s
`fontWeightTable[6]` (Bold) slot, so `<b>`/`FontStyles.Bold` on any existing Baloo 2 text now render true bold
glyphs instead of TMP's synthetic faux-bold. Also downloaded three body-text candidates (Nunito Sans, Mulish,
Rubik) to pair with Baloo 2 for longer text (Magic Words dialogue lines) since Baloo 2 is a heavy display face
better suited to titles/buttons; the developer trialed all three and picked **Rubik**, so Nunito Sans and Mulish
were removed and only `Rubik SDF.asset` remains.
**Why:** D19 already flagged that Google Fonts only distributes Baloo 2 as a single variable-weight `.ttf`, so a
true Bold needed extracting a static instance rather than a plain download — `fonttools`' `varLib.instancer`
does exactly that from the `wght` axis already embedded in the file. The three body-text candidates (and Rubik,
the survivor) are in the same boat: Google Fonts ships them variable-only too, and their variable *default*
instance is ExtraLight/Light rather than Regular, so each was instanced at `wght=400` before generating its SDF
asset — using it as downloaded would have looked visibly thin, not a deliberate weight choice.
**Not done:** Rubik isn't wired onto any TMP component yet (e.g. the Magic Words dialogue boxes) — the developer
picked the font but hasn't asked for the swap.

---

### D28 — Magic Words built: dialogue sequencer, edge-anchored boxes, TMP typewriter reveal, fast-forward/skip (2026-08-24)
**Choice:** Full first pass at Magic Words, built directly per explicit developer instruction (not a plan-then-ask
pass — see the collaboration note at the bottom of this file). Summary; see "Magic Words architecture" in
current-context.md for the complete design:
- **Domain** (`MagicWords.Logic`, `noEngineReferences: true`): `DialogueSequence` mirrors `CardDeck`'s shape (armed
  with every line up front, stepped strictly via `MoveNext()`, `IsFinished` only true once the last line has
  actually been shown). `DialogueSequenceBuilder` joins the endpoint's `dialogue`/`avatars` arrays by speaker name
  via `SpeakerAvatarLookup` (first-match-wins, tolerating the endpoint's duplicate "Sheldon" entries rather than
  throwing), defaulting to `DialoguePosition.Right` when a speaker has no avatar entry at all.
- **Presentation** (`MagicWords.Monobehaviour`): two `DialogueBoxView`s, one edge-anchored per screen side, that
  slide toward center via a plain `DOAnchorPosX` when it's their speaker's turn (the literal ask: "tween the
  dialogue boxes... by making them a DoMove on x axis towards the center of the screen"). Text reveal uses DOTween
  Pro's `TMP_Text.DOMaxVisibleCharacters` typewriter technique - the "dotween pro's tmp animation to reveal text"
  ask - duration derived per-line from character count so short and long lines read at the same pace. Fast-forward
  is a single full-screen invisible `Button` (transparent `raycastTarget` `Image`) driving a 2-state click handler
  exactly as specified: first click while revealing completes the tween immediately; a click once already fully
  revealed advances/skips; a `TimerUtil.CountdownTimer` auto-advances if the player never clicks at all.
  `MagicWordsRepository`/`AvatarSpriteLoader` fetch the real endpoint + avatar images at runtime via coroutines.
- **New asmdef dependency discovered**: `DOMaxVisibleCharacters` and `DOTweenTMPAnimator` live in
  `DOTweenPro.Scripts` (`Assets/Plugins/Demigiant/DOTweenPro/DOTweenTextMeshPro.cs`), a *different* asmdef from
  `DOTween.Modules` (which only covers `DOAnchorPos`/`DOLocalRotateQuaternion`/etc. - what Ace of Shadows already
  referenced). `MagicWords.Monobehaviour.asmdef` needed both. Caught immediately via a real compile error through
  Unity MCP, not discovered later.
- **Deliberately deferred, not half-built: real emoji-icon sprites.** The endpoint embeds named tokens
  (`{satisfied}`, `{intrigued}`, etc.), not Unicode emoji codepoints - `BRIEF.md`'s own guidance about mapping
  *codepoints* to a sprite asset doesn't match this data (a finding from the previous session, still true).
  Rendering these as real TMP `<sprite name="word">` tags needs a TMP Sprite Asset built from actual icon art for
  each of the ~6 known tokens, which is a real asset-sourcing task on its own and wasn't part of this session's
  ask (the developer's instruction covered box layout/tweening/reveal/skip, not emoji specifically).
  `DialogueTextFormatter.StripTokens` removes `{word}` substrings cleanly instead - a working, finished piece of
  text handling on its own - rather than shipping `<sprite>` tags with no matching asset (which would render as
  broken/missing glyphs) or leaving literal `{word}` text in the dialogue. Swapping in real icons later only needs
  the art plus a small change to `StripTokens`; nothing else in the domain layer changes.
- **Avatar images load for real** (`AvatarSpriteLoader`, `UnityWebRequestTexture`), with a fallback sprite shown
  immediately and swapped in on success - directly answers the brief's "handle cases where avatar URLs may not
  load" requirement against the endpoint's two guaranteed-broken URLs (Sheldon's port-81 URL, "Nobody"'s API-root
  URL), not just a hypothetical.
- **Scene changes mirror D18**: `MagicWordsScene`'s Directional Light deleted, Main Camera switched to Solid Color
  clear - same reasoning as Ace of Shadows' all-UI-scene cleanup, applied here directly for consistency (the
  developer explicitly asked to "check how we implemented ace of shadows and stay consistent with it").
- **Built via Unity MCP scripting, not hand-written scene YAML** - `AssetDatabase`/`SerializedObject` calls
  constructing the GameObject hierarchy and wiring every serialized field, same tooling approach as the D21
  card-prefab conversion and D13's scene moves/renames, chosen for the same reason: reliable GUID-preserving,
  verifiable-by-reading-back-the-result construction instead of error-prone manual YAML edits. Hit the
  already-documented `Image` bare-name ambiguity gotcha (resolves to the wrong namespace inside `Unity_RunCommand`
  scripts specifically) immediately and fixed it the same way the Tooling notes already prescribe: fully qualify
  as `UnityEngine.UI.Image`.
**Why:** Requested directly, in detail (box layout, TMP reveal technique, fast-forward/skip state machine, box
tween direction) - a genuine delegation, not a "let's discuss" - so implemented per the "when a request reads as
genuine delegation... implement it fully" precedent from D12's collaboration note, without a plan-first detour.
**Not done in this pass, explicitly flagged rather than silently skipped:** emoji-icon sprite art (see above);
Play Mode verification (nothing here has been clicked through yet - compiles clean, 54/54 EditMode tests pass, the
saved scene YAML was read back and checked field-by-field, but that's it); committing the work to git.

### D27 — README rewritten from a 4-line stub into an actual front door (2026-08-24)
**Choice:** `README.md` used to just point at `ai-context/BRIEF.md` ("start here") and state there's no deadline —
that was it. Rewritten to actually explain what the project is, what each of the three demos does, honest current
status (Ace of Shadows done, Magic Words/Phoenix Flame not started), how to play it (Editor + the hosted WebGL
link, with a note that the deploy is manual so the live link can lag `git log`), and a pointer to
`ai-context/decisions.md`/`current-context.md` for anyone who wants the real reasoning instead of a summary.
Written in first person, deliberately plain — includes an explicit "this isn't a flex" note: the asmdef/Logic-
Monobehaviour split, the decisions log, etc. are there because the work called for them, not to perform
complexity for a grader.
**Why:** Requested directly. The `unity-interviewer` audit flagged "no README" as a Tier 0 blocker tied directly
to a named grading criterion (documentation) and the brief's own Definition of Done — the old stub didn't meet
that bar. Deliberately does **not** lead with "read `ai-context/BRIEF.md` first" the way the old one did, since
that file is SOFTGAMES' own assignment text — pointing a SOFTGAMES reviewer at their own brief as the front door
is circular; the README now explains the project on its own terms instead. Also deliberately doesn't hard-depend
on `ai-context/` staying in the submitted repo at all, since that's still an open, deferred decision (see the
D-numbered entry on `ai-context/BRIEF.md` privacy, still undecided as of this writing).

### D26 — Build-size levers: packages/modules trimmed, crunch and stripping/exceptions declined (2026-08-23/24)
**Choice:** Of the four build-size levers the `unity-interviewer` audit named (see D24's context) as untouched
against `BRIEF.md` §6:
- **Unused packages/modules**: removed. `com.unity.ai.navigation`, `com.unity.collab-proxy`,
  `com.unity.multiplayer.center`, `com.unity.timeline`, `com.unity.visualscripting`, and 19 unused built-in engine
  modules (androidjni, assetbundle, cloth, director, physics, screencapture, terrain, terrainphysics, tilemap,
  umbra, unityanalytics, unitywebrequestassetbundle, unitywebrequestwww, vehicles, video, vr, wind, xr — see the
  commit for the full reasoning on what was kept and why, e.g. animation/particlesystem for Phoenix Flame,
  jsonserialize/unitywebrequest for Magic Words). **Correction the same session**: `com.unity.modules.physics2d`
  had to be re-added — the removal pass only checked this project's own code for Physics2D usage, not the
  vendored `Assets/Plugins/Demigiant/DOTween` folder, which ships an optional `DOTweenModulePhysics2D.cs` that
  references `Rigidbody2D` directly and broke the build (9x CS1069). Considered deleting the unused DOTween
  module file instead of restoring the package, but `DOTweenAnimation.cs`/its Inspector also reference Physics2D
  types — not worth digging into vendored third-party plugin internals for a marginal size win. Re-adding the
  package was the correct, low-risk fix.
- **Texture crunch compression**: tried on `PlayingCards.spriteatlas`'s WebGL override (quality 60, explicit
  format), then reverted — the size win wasn't worth the visible quality loss on card-face art at that
  compression level. Back to the original settings (`m_CrunchedCompression: 0`, quality 50, format Auto).
- **Managed Stripping Level** and **WebGL Exception Support**: declined entirely, not just deferred. Both carry
  real runtime risk (stripping can remove reflection-reached code TextMeshPro/DOTween touch; disabling exceptions
  means a real `try/catch` stops actually catching) for a build-size win that's marginal next to what the package
  trim already bought. Not worth the risk on a project this size — explicitly decided against, not left open.
**Why:** Requested directly, weighing exactly this risk/reward tradeoff per lever rather than mechanically working
down the audit's list. The package/module trim was worth doing (zero functional risk once the DOTween dependency
was accounted for). Crunch and the two Player Settings levers were each tested/considered on their own merits and
rejected on their own merits — a real decision, not something left undone by omission.

### D25 — Lock the app to landscape, resolving the canvas reference-resolution mismatch (2026-08-24)
**Choice:** The app is landscape-only now, not orientation-agnostic. `MainMenuScene`'s Canvas reference
resolution was `1080×1920` (portrait) while `AceOfShadowsScene`/`AppScene` were both `1920×1080` (landscape) —
three canvases, two contradictory design resolutions, a real bug the `unity-interviewer` audit caught (D24 covers
the audit's *mechanical* findings; this was one of the *judgment-call* findings deliberately left out of D24).
Fixed by changing `MainMenuScene`'s reference resolution to `1920×1080`, matching the other two — verified this
doesn't require touching the actual button layout, since the menu's button container is sized in absolute units
(994.5×742.6) comfortably smaller than either reference frame, and the Canvas already uses Match Height mode,
under which only the reference *height* affects scaling at all (the width component was never actually load-
bearing). `ProjectSettings.asset`: `allowedAutorotateToPortrait`/`PortraitUpsideDown` → `0`,
`allowedAutorotateToLandscapeLeft`/`Right` stay `1`, `defaultScreenOrientation` left at `AutoRotation` — locks
orientation to landscape while still autorotating between the two landscape directions (not a single fixed
rotation, so the game still works right-side-up regardless of which way the phone is held).
**Why:** Requested as a "which orientation strategy" decision with three options on the table: lock landscape,
lock portrait, or build true dual-orientation support (live rotation with per-scene alternate layouts — what D21's
proportional-anchoring conversion was explicitly a prerequisite for, not the thing itself). Landscape lock was
chosen on real evidence, not a guess: the developer checked one of SOFTGAMES' own published games on Facebook
Gaming and found its render canvas is a fixed `2270×1280` internal resolution inside a CSS frame that preserves
the same ratio (`360×203`) — both ≈16:9, a fixed landscape aspect, not something that reflows to portrait. That's
their actual shipped product, and it happens to be exactly the aspect ratio Ace of Shadows was already built at.
Building true dual-orientation support was rejected as scope the brief's "responsive on mobile + desktop" language
doesn't actually require (that's about scaling across screen *sizes*, not necessarily supporting live rotation
between portrait and landscape), and locking portrait instead would have meant reworking Ace of Shadows' side-by-
side stack layout for no evidence-backed reason.
**Not done as part of this decision:** safe-area handling (still open, and now simpler to reason about with only
one orientation to support), and the WebGL `index.html` template's own fixed-960×600-desktop-canvas bug (a
separate, unrelated responsiveness issue — see the `unity-interviewer` audit's Tier 0 finding #5).

### D24 — Mechanical fixes from the first `unity-interviewer` audit (2026-08-23)
**Choice:** Ran the new `unity-interviewer` subagent (see current-context.md, Tooling) against the whole repo for
the first time. It came back with a long findings list, tiered by severity, plus a mock interview. The developer
asked to split the list: fix everything mechanical/non-judgmental now, leave anything requiring a real design
decision for later. This entry covers the mechanical half:
- **Namespace cleanup**: `Feature.AceOfShadows.Scripts.Monobehaviour` (the Rider-generated, folder-path-mirroring
  namespace) renamed to `AceOfShadows.Monobehaviour` across all 6 files in that folder plus
  `CardStackLayoutTests.cs` — now matches `AceOfShadows.Logic` and the `SceneFlow.Logic`/`SceneFlow.Monobehaviour`
  pattern instead of being a fourth, different scheme. `FpsCalculator`/`FpsCountUIController` (previously in the
  global namespace, no namespace at all) got `FpsCounter.Logic`/`FpsCounter.Monobehaviour`. All 5 test classes
  (previously all in the global namespace too) got a `<Feature>.Tests` namespace matching their folder
  (`AceOfShadows.Tests`, `FpsCounter.Tests`, `SceneFlow.Tests`) — a new convention, since no test file had ever
  been namespaced before this.
- **Dead code removed**: `CardStack.CountChanged` (an `event Action<CardStack>`, fired on every push/pop) had
  zero real subscribers — its only consumer was a test that existed purely to cover it. Removed the event and
  that test (`CardStackTests` drops from 7 to 6 cases). This completes what D20 already decided in spirit (stop
  routing counter updates through a `CardStack`-level event) but literally left the event itself sitting there
  unused.
- **`AceOfShadowsConfig.asset` reverted to sane values**: `totalCards` was sitting at `10` (uncommitted leftover
  from testing the D23 restart loop) — back to `144`. `moveDuration` was `1`, equal to `moveInterval` (also `1`)
  — this directly violates the documented invariant ("`MoveDuration` must stay under `MoveInterval`, so a card
  always finishes landing before the next one starts," see "Ace of Shadows architecture" in current-context.md)
  and risked a real race between DOTween's `OnComplete` and the timer loop. Reset to `0.35`, the coded field
  default.
- **`AceOfShadowsController.UpdateCountdownFill` throttled**: it was writing `Image.fillAmount` unconditionally
  on every `OnTick` (every frame, for the whole 144-second run) into a `Canvas` that also contains all 144 card
  `Image`s — each write dirties the Graphic and forces a batch rebuild of every sibling in that Canvas. Now only
  writes when the value has moved by at least `0.01` (1%), reusing the same "only touch it when the displayed
  value actually changed" pattern `FpsCountUIController` already used. The two places that force-set it to `0`
  directly (`TryMoveNext`'s empty-source branch, `Restart()`) now go through the same tracked setter so the
  throttle state doesn't go stale across those resets.
- **`AceOfShadowsScene`'s Main Camera**: `UniversalAdditionalCameraData.m_RenderPostProcessing` set `1` → `0`.
  Post-D21, the scene has zero `SpriteRenderer`s/`MeshRenderer`s (verified: cards are UI now) — the whole Canvas
  composites after URP's camera stack, so Bloom/Vignette/Tonemapping via `SampleSceneProfile.asset`'s Global
  Volume were provably affecting zero visible pixels. This was flagged as an open aesthetics call in the D18
  Rendering & performance notes; post-D21 it isn't a call anymore, it's dead work being done every frame for no
  visual effect. The Global Volume/profile itself is untouched, so re-enabling is a one-flag flip if UI-space
  post-processing is ever wanted.
- **All 106 card-art prefabs**: `Image.raycastTarget` `true` → `false` (batch Editor script, same
  `LoadPrefabContents`/`SaveAsPrefabAsset` pattern as the D21 conversion). Nothing about a card is clickable;
  every one of them was participating in every pointer-event raycast for no reason.
- **`MainMenuButton.prefab`'s TMP font**: was `Oswald-SemiBold SDF`, not `Baloo2 SDF` — meaning the main menu,
  the literal first screen of the app, never actually got the font swap D19 claimed ("swapped onto every
  existing TMP text component"). No per-instance override existed in `MainMenuScene.unity`, so this one prefab
  edit fixes all three menu buttons. `Oswald-SemiBold SDF` itself wasn't deleted — worth checking separately
  whether anything else still legitimately needs it before removing it as dead weight.
- **`DeployWebGL.cs`**: `BuildFolder` was a hardcoded `E:\Projects\...` absolute path — now derived from
  `Application.dataPath` plus the documented sibling-repo naming convention (`<project>-build`), so the tool
  isn't tied to one machine. `RunGit` called `process.WaitForExit()` before reading the redirected stdout/stderr
  streams — the textbook deadlock risk if git ever writes enough output to fill the pipe buffer; now reads the
  streams first. Also corrected D8's text, which claimed the tool "runs tests" — it never did; see the amended
  D8 entry.
- **Dead/template assets removed**: `Assets/TutorialInfo/` and `Assets/Readme.asset` (the URP template's tutorial
  content, still committed, verified nothing else references `Readme.asset`'s GUID), `Assets/App/Sprites/UI/
  icons.png` (2.9MB, verified zero references anywhere in the project via GUID search), and the empty leftover
  `softgames-task/softgames-task/`-style nested folder from D3 (was never tracked by git, filesystem-only
  cleanup).
- **Doc corrections — including one to the audit's own finding**: `current-context.md`'s claimed test count was
  actually correct (33, 8 SceneFlow) before this pass. The `unity-interviewer` audit flagged it as wrong (31, 6
  SceneFlow) based on `grep -c "\[Test\]"` — that grep undercounts by 2, because `SceneFlowStateTests.
  TryBeginNavigation_ToNullOrEmptyScene_ReturnsFalse` uses two `[TestCase(...)]` attributes with no `[Test]`
  attribute at all (valid NUnit). Verified by actually running the suite (`TestRunnerApi`, `PassCount`) rather
  than trusting either grep: real count is 32 now (after this pass's own `CardStack.CountChanged` test removal,
  33 before it) — `current-context.md` now says so explicitly and warns against re-deriving this number by
  grepping `[Test]` again. Worth remembering generally: a static-analysis finding is only as good as the method
  behind it, even from a review specifically designed to be adversarial.
**Why:** Requested directly, in the specific shape "fix the mechanical stuff yourself, flag what needs my
judgment." Every item above has one unambiguous correct answer (a dead code path, a documented invariant being
violated, a doc claim that's checkably false, a namespace that doesn't match its siblings) — none of them trade
off against another reasonable choice, which is what made them safe to just do rather than raise as a question.
**Deliberately not touched in this pass — real judgment calls, left for the developer:** the two missing tasks
(Magic Words, Phoenix Flame); whether/how to sanitize `ai-context/` before submission (it currently contains
compensation/job-search context that reads very differently to a hiring reader than to Claude); redeploying the
now-3-commits-stale hosted build; writing the missing README; the build-size levers (managed stripping level,
exception support, removing unused packages/modules, texture crunch compression — all real trade-offs, not
mechanical); the `MainMenuScene`/`AceOfShadowsScene`/`AppScene` reference-resolution mismatch (1080×1920 portrait
vs. 1920×1080 landscape) and the still-missing landscape↔portrait layout switch; safe-area handling. See the
`unity-interviewer` report itself (not yet copied into this doc verbatim) for the full list and its interview
questions.

### D23 — Ace of Shadows: Restart button, recreate the CardDeck rather than reset it in place (2026-08-23)
**Choice:** `FinishedMessageView` gained a `restartButton` (new `RestartButton` GameObject under the `FinishedMessage`
panel, green button + TMP "Restart" label in Baloo 2, wired to `Button.onClick`) and `Initialize(Action onRestart)`
now takes the restart callback instead of no args. `AceOfShadowsController.Restart()`: destroys every existing
`CardView` and clears `_cardViewsByCardId`, discards the current `CardDeck` and constructs a brand new one
(`_deck = new CardDeck(config.TotalCards)`, unsubscribe/resubscribe `CardMoved`/`AllAnimationsFinished`), calls the
existing `CreateCardViews()` to rebuild the fanned source pile from scratch, re-`Show()`s both counters (new
counterpart to the existing `Hide()`), resets their text, `Hide()`s the finished message, resets the countdown
fill, and calls `_timer.Start()`.
**Why:** Requested directly. Recreating the deck/views from scratch rather than writing bespoke "return each card
to its source position" reset logic reuses the exact same code path `Awake()` already exercises for first-time
setup — one tested path instead of two. This only works cleanly because `Timer.Start()` after `Stop()` fully
resets the countdown (`PrepareStart()` → `ResetDuration()`), which was specifically the point of the D13 TimerUtil
fix ("`Start()` now re-registers... so `Restart()` still works" — this is that promise being cashed in). Not yet
committed to git as of this writing — sitting in the working tree pending the developer's Play Mode check, same as
D22 was before it got committed.

### D22 — Ace of Shadows: cards cascade off-screen when the deck finishes (2026-08-23)
**Choice:** `AceOfShadowsConfig` gained `exitDistance` (1600px), `exitDuration` (0.5s), `exitEase` (`InBack`),
`exitStagger` (0.02s/card). `CardView.AnimateExitDown` tweens a card's `anchoredPosition` straight down by
`exitDistance` with an optional per-card delay. `AceOfShadowsController.OnAllAnimationsFinished` now hides both
stack counters and cascades every card in `_deck.Target.Cards` off-screen (each with `i * exitStagger` delay,
reading from the domain `CardStack`'s own list — same pattern `CreateCardViews` already used for the source
stack — not a `GetComponentsInChildren` scene-hierarchy query) alongside showing the finished message. Committed
as `db4dd75` ("polish animation compelte moment") on top of D21's UI conversion, and pushed to `origin/master`.
**Why:** This iteration happened through the developer resuming the same background agent directly (outside the
main conversation thread) rather than through this session, so the original framing/ask isn't in this
conversation's history — documented here from the committed code itself, which is unambiguous. One open call the
agent flagged at the time and the developer's resolution isn't recorded: the finished message, counter-hide, and
card cascade all fire simultaneously rather than staged (message appears while cards are still falling) — the
shipped code still does this, so either that was accepted as-is or is worth revisiting. `exitDistance: 1600` is a
flat canvas-unit offset, not derived from actual canvas height — not resolution-aware, flagged as fine "for now."

### D21 — Ace of Shadows cards moved from world-space SpriteRenderers to UI (RectTransform/Image) (2026-08-23)
**Choice:** Cards, `SourceStack`/`TargetStack`, and the scene's `OverlayUI` Canvas are no longer split across
world-space (cards) and Screen Space - Camera (HUD) — everything now lives in one Screen Space - Overlay
Canvas. Concretely:
- `OverlayUI` Canvas: render mode Screen Space - Camera → **Overlay** (camera reference cleared), and
  CanvasScaler Constant Pixel Size (800x600, unscaled) → **Scale With Screen Size (1920x1080, match 1)** —
  now identical to `MainMenuScene`'s Canvas settings.
- `SourceStack`/`TargetStack`: plain world-space `Transform`s at scene root (world positions `-3,0,0`/`3,0,0`)
  → `RectTransform`s reparented under the Canvas, anchored at `(0.25, 0.5)`/`(0.75, 0.5)` respectively —
  proportional to canvas size, not a fixed world offset. Sibling order in the Canvas is `Bg → SourceStack →
  TargetStack → FinishedMessage → CountdownFill → HomeButton`.
- `CardView`/`Card.prefab`: root `Transform` → `RectTransform`. `CardStackLayout.GetOffset` returns `Vector2`
  (pixels) instead of `Vector3` (world units + Z depth) — **draw order now comes from Canvas sibling index**
  (each new card is instantiated/reparented as the last sibling = drawn on top = matches LIFO "newest card on
  top"), not from a manually-assigned Z offset. `CardView.MoveTo`/`SetPositionImmediate` now animate
  `RectTransform.anchoredPosition` via DOTween's `DOAnchorPos` instead of `transform.DOLocalMove`.
- All 106 card-art prefabs (`PlayingCards/Prefabs/Deck01|Deck02/*.prefab`, referenced by
  `AceOfShadowsConfig.CardVisuals`) were batch-converted via an Editor script (`PrefabUtility.LoadPrefabContents`
  → strip `SpriteRenderer` → `AddComponent<Image>` with the same sprite, `SetNativeSize()` then scaled to a
  260px-tall card, `AddComponent<RectTransform>` on the root). The second sprite each card had (`Back_D7`/
  `Back_D8`, a card-back sibling offset `z:0.001`) was **deleted**, not converted — see below.
- `CardStackLayout.GetRandomZRotation` dropped its `xSeed`/`ySeed` params — was `Quaternion.Euler(xSeed, ySeed,
  angle)`, now always `Quaternion.Euler(0, 0, angle)`.
- `AceOfShadowsController`'s `sourceStackRoot`/`targetStackRoot` fields: `Transform` → `RectTransform`.
- `AceOfShadows.Monobehaviour.asmdef` gained a `DOTween.Modules` reference — `DOAnchorPos` (and the rest of
  DOTween's UI shortcuts) live in `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUI.cs`, which has its
  own real asmdef (`DOTween.Modules`) unlike DOTween's core shortcuts (`DOLocalMove`, `DOPunchScale`, etc.),
  which ship in a precompiled DLL and are auto-referenced everywhere. Missing this reference is a `CS1061`/
  "no accessible extension method" compile error, not a namespace-import problem — `using DG.Tweening;` alone
  isn't enough.
**Why:** Requested directly — the developer flagged the world-space + Perspective-camera + hand-placed-Z-depth
design as the wrong call for a game that needs to work across different resolutions/aspect ratios, and as a
real blocker for an eventual landscape→portrait layout. UI's `RectTransform` + `CanvasScaler` (Scale With
Screen Size) is what the rest of the project already uses for exactly this problem (main menu, HUD); Canvas
sibling order is a free, already-correct substitute for the old manual Z-depth draw-order scheme, and removes
an entire axis of bookkeeping (`PerCardDepth`, the 12-card visual-fan-cap-vs-uncapped-Z distinction).
**Also decided, not requested — flagged for the developer to reconsider:** the old `Back_D7`/`Back_D8` sprite
per card existed only to support a true 3D trick — rotating a card 180° on Y so the camera sees the "back"
sprite face-on instead of the mirrored front. That trick has no equivalent in a flat Screen Space - Overlay
canvas (no camera depth, and UI's default shader doesn't cull backfaces the way it would need to for this to
work), so **landed cards no longer get a distinguishing "flip"** — they just keep the same per-card Z-rotation
jitter as source-stack cards. If a landed-card visual cue is still wanted, it needs a genuinely different
technique (e.g. a scale.x squash-swap-unsquash flip, or a tint/highlight), not a port of the old one.
**Not done in this pass:** an actual landscape↔portrait layout switch (different anchor presets per
orientation). This conversion fixes the underlying rendering approach — proportional anchoring under
`CanvasScaler` instead of fixed world positions — which is the prerequisite for that, but doesn't yet add
orientation-aware layout logic. The `PerCardOffset` pixel value (3px/card, scaled down from the old 0.03
world-unit fan offset against the new 260px card height) is an eyeballed conversion, not measured — expect to
retune it by eye in Play Mode. Play Mode re-verification of this whole conversion is still open (developer's
call, per the established Play Mode testing workflow — see Tooling in current-context.md).

### D20 — StackCounterView: direct method calls, not an event/interface (2026-08-23)
**Choice:** `AceOfShadowsController` calls `sourceCounterView.Refresh(_deck.Source.Count)`
right after `_deck.MoveNext()` and `targetCounterView.Refresh(_deck.Target.Count)` from
`OnCardLanded()` (the DOTween completion callback) directly. An `IStackCountChangeNotifier`
interface (in `AceOfShadows.Logic`, since it's plain `Action`/`int`, engine-free) +
`CardStack.Trigger` version of this was built first, worked, and was then torn out.
**Why:** The interface/event version's signal path was controller → `CardStack.Trigger`
→ back to the same `StackCounterView` the controller already held a direct reference to
— a round trip that started and ended in the same place, since nothing else in the
project observes a `CardStack`'s count changing independently of
`AceOfShadowsController`'s own move-timing knowledge. Removing it deleted a whole file
(`IStackCountChangeNotifier.cs`) and the subscribe/unsubscribe lifecycle in
`Bind`/`OnDestroy`, with identical runtime behavior. Same shape of tradeoff as D15's
`IInitializable` rejection — reused that precedent directly rather than re-litigating it.
**Also fixed along the way:** the first working version had `CardStack.PopTop`/`PushTop`
auto-invoking `Trigger`, which fired both counters at the same instant (`CardDeck.MoveNext()`
pops `Source` and pushes `Target` back to back) — wrong for the target, which needs to
pop on visual landing, not on the domain push. The final direct-call version sidesteps this
entirely since the controller invokes each counter at its own correct moment already.

### D19 — Baloo 2 as the project-wide font (2026-08-23)
**Choice:** Downloaded Baloo 2 (Google Fonts, OFL-licensed) into `Assets/Art/Fonts/Baloo2/`,
generated a TMP SDF Font Asset from it, and swapped it onto every existing TMP text
component plus the TMP Settings default font asset (previously `LiberationSans SDF`,
TMP's generic fallback).
**Why:** Requested directly — a rounded, casual-game-appropriate typeface reads better
for the aesthetics/UX grading criteria than TMP's default. Google Fonts only distributes
Baloo 2 as a single variable-weight `.ttf` now (no static per-weight files), so the
generated Font Asset resolves to the Regular named instance only; a true Bold variant
would need extracting a separate static instance from the variable font (more tooling
than a plain download) and wasn't done since nothing asked for it yet.

### D18 — Ace of Shadows rendering cleanup: no skybox/light, mesh-type fix, atlas (2026-08-23)
**Choice:** `AceOfShadowsScene`'s Main Camera now clears to Solid Color (was Skybox) and
its `Directional Light` was deleted — the scene is 100% unlit sprites, neither was doing
anything. The scene's Canvas switched from Screen Space - Overlay to Screen Space -
Camera (needed, not cosmetic — a new full-screen `Bg` panel needs to depth-composite
behind the cards, which Overlay mode can't do since it always draws on top regardless of
scene depth). The 8 `PlayingCards` deck textures' Sprite Mesh Type was changed
Tight → Full Rect (Tight was generating ~24-vert outline meshes per card instead of a
4-vert quad). A `PlayingCards.spriteatlas` was built to pack the deck textures together.
**Why:** Requested directly, following up on a SetPass-call investigation. The investigation's
headline finding, worth remembering before touching this area again: **card sprite batching
was never actually the bottleneck** — Frame Debugger showed `DrawTransparentObjects` for
all 288 card renderers at only 2 draw calls once deck-texture variety came down to 2. The
real cost is URP's post-processing stack, Bloom specifically (~16 of ~37 total passes),
running off `Assets/Settings/SampleSceneProfile.asset` — the **unmodified default URP
template Volume profile**, never a deliberate choice for this project. SSAO (4 passes) has
zero visible effect on unlit sprites (it's an ambient-occlusion effect for lit geometry) and
is a free win to disable; Bloom/Vignette/Tonemapping have a real visual effect and are left
as an open call (see current-context.md next steps) rather than stripped unilaterally, since
the brief grades on aesthetics. Also discovered along the way: `EditorSettings.spritePackerMode`
was `Disabled` project-wide, meaning every Play Mode test up to that point (including the
deck-count/atlas experiments) never actually exercised the atlas at all — fixed to
`AlwaysOnAtlas` so Editor testing reflects real build behavior.

### D17 — Real playing-card visuals, uVegas removed for good (2026-08-23)
**Choice:** `CardView` now picks a random visual per card from `AceOfShadowsConfig.CardVisuals`
(a `List<GameObject>`) instead of using the single placeholder `popups_11` sprite, backed by
a "PlayingCards" asset pack (`Assets/Feature/AceOfShadows/Prefabs/PlayingCards/`,
`.../Textures/`). `Assets/uVegas` (previously kept-but-unused due to a watermark on its base
card sprites, D10) was deleted entirely.
**Why:** Requested directly, moving Ace of Shadows from placeholder to final art. uVegas's
rank/suit glyph system is now fully superseded and had a real, previously-documented licensing
risk (watermarked `Front.png`/`Back.png`) — no reason to keep it in the repo once real card
art was in.

### D16 — Ace of Shadows tuning fields moved into an AceOfShadowsConfig ScriptableObject (2026-08-23)
**Choice:** `totalCards`, `moveInterval`, `maxRotationDegrees` (previously `[SerializeField]`s
on `AceOfShadowsController`) and `moveDuration`, `moveEase` (previously on `CardView`) all
moved into a single shared `AceOfShadowsConfig` ScriptableObject asset, exposed via public
getters. `CardView.Initialize(config)` follows the project's established single-init-point
convention (D15) rather than each card re-reading serialized fields of its own.
**Why:** Requested directly. Centralizes tuning in one inspectable asset instead of scattered
across two component types, and sets up the natural place for the card-visual list
(`CardVisuals`, D17) that came right after.

### D15 — Single init point per feature, no generic IInitializable/reflection (2026-08-23)
**Choice:** `AceOfShadowsController` and `SceneService` each merge a former
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
with `SceneService`) were kept as their own tiny self-initializing `Awake()`s
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
per-instance overrides in `MainMenuScene.unity`, `SceneService`'s field in
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
  directly with no `SceneService`, NRE'd on the first button click, and
  never shown the FPS counter.
- **P1 (real bugs the nav loop introduced):** `Timer.Stop()` never unregistered
  from `TimerService`, leaking a full `AceOfShadowsController` graph (144 cards)
  on every MainMenu→AceOfShadows→home cycle — fixed in `TimerUtil` itself (`Stop()`
  unregisters, `Start()` re-registers) since any future timer-using feature would
  hit the same trap. `SceneService.Navigate` had no in-flight guard —
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
`SceneService` singleton, and is never itself unloaded. Every other scene
(MainMenu, AceOfShadows, MagicWords, PhoenixFlame) loads additively on top of it,
exactly one "content" scene at a time — `SceneService.Navigate()` unloads the
previous content scene before additively loading the next and calling
`SceneManager.SetActiveScene`. `SceneFlowState` (pure, EditMode-tested) owns the
"is this actually a different scene" guard; the Monobehaviour layer just does the
unload/load/activate. Back buttons (`BackButton.prefab`, `buttons_41` home-icon
sprite) were added to AceOfShadows/MagicWords/PhoenixFlame, calling
`SceneService.Instance.NavigateHome()`; MagicWords/PhoenixFlame needed a
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
gets the practical outcome that actually matters (WebGL builds, deploys live) for
a fraction of the ongoing maintenance cost. **Correction (2026-08-23, caught by
a `unity-interviewer` audit, D24):** this originally also claimed "tests run" as
part of that outcome — `DeployWebGL.BuildAndDeploy()` never actually runs the
test suite, it only builds and pushes. Either add a `TestRunnerApi` pass before
the build, or stop claiming it does one; not fixed as part of D24, flagged as a
real gap in the deploy tool.
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
