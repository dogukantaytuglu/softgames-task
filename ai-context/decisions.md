# Decisions Log

Newest first. Each entry: **what** was decided, **why**, and the date. This is the
"defend it out loud" reference — the brief is explicit that every decision in the
submission needs a real justification behind it (`BRIEF.md` §1, §7).

---

### D44 — Every button rebased onto one shared `BaseButton` prefab (as prep for click-sound); `SceneFlow` renamed `SceneServices`; two real bugs found and fixed along the way; Ace of Shadows fast-forward, portrait/bowl-highlight fixes (2026-08-28)

Requested directly: sound is coming next (button click, card movement, dialogue, fire), and the developer wants one place to add a click-sound component rather than wiring it onto every button individually. Before that, two smaller asks landed first in the same session.

**Two small bug fixes, developer-authored, Claude reviewed/committed.**
- `FlameParticle` now also instances the bowl-highlight's material (`bowlHighlightRenderer`), not just the particle's own — the Animator that drives the flame also drives `BowlHighlight`'s `SpriteRenderer.material._BaseColor`/`_EmissionColor` (added when the flame's Animator took over the bowl recolor), and without its own instance that write landed on the shared `BrazierBowlFakeLight.mat` asset, same class of bug D43 already fixed for the particle's own material.
- `MagicWordsController.ShowLine` no longer calls `portrait.Bind(name, null)` synchronously before the avatar fetch resolves. That call existed so the portrait had *something* to show during a real (uncached) fetch, but `SpeakerPortraitView`'s own doc comment says the "missing avatar" look (monogram + `NO AVATAR` tag) is a *data* state, never a loading state — and the old code showed exactly that placeholder for speakers who **do** have an avatar, just slow to load, contradicting its own contract. Removing the eager bind fixes that. **Flagged, not fixed, on review:** `Bind()` is also the only thing that calls `gameObject.SetActive(true)` on the portrait, and that call only runs now from inside the async callback, gated by `_lineToken`. Under fast-forward (tap through lines faster than an avatar loads), the callback for a superseded line gets dropped, so the portrait can end up either invisible (first appearance on that side) or showing the *previous* speaker's face/initial next to a dialogue box that already says the new speaker's name — a real desync the old code didn't have, traded for fixing the flicker. Left as-is; the developer has the trade-off in hand.

**Ace of Shadows: hold-to-fast-forward.** `AceOfShadowsConfig.speedMultiplier` (new field) plus `FastForwardButtonView` — a press-and-hold control (`IPointerDownHandler`/`IPointerUpHandler`, not `Button.onClick`, since the effect only applies while held). Rather than teaching the shared `TimerUtil` plugin about speed (used by Magic Words too), `AceOfShadowsController.Update()` feeds the existing `CountdownTimer` extra `TryTick()` calls while held — `speedMultiplier - 1` per frame through an accumulator so a non-integer multiplier still averages out correctly — and divides each card's own `CardView.MoveTo` duration by the same multiplier, so the whole sequence compresses rather than just piling up overlapping tweens. Button sits centered below the counters, matching `HomeButton`'s existing pill style, built and positioned via Unity Editor scripting (`PrefabUtility`/`RectTransform` calls through MCP) rather than hand-authored YAML.

**`OnDestroy` → `OnDisable` audit, requested directly** ("almost everything is disposed OnDestroy"). `OnDisable` always runs immediately before `OnDestroy` in Unity's teardown sequence, so a handler that only unsubscribes/stops-a-timer/kills-a-tween is safe to move — it still catches the destroy path, and now also fires if the object is ever disabled without being destroyed. Six files converted (`AceOfShadowsController`, `CardView`, `StackCounterView`, `DialogueBoxView`, `MagicWordsController`, `SceneService`; `FastForwardButtonView`'s existing `OnDisable`+separate `OnDestroy` were merged into one). **One deliberate exception, `FlameParticle`, left on `OnDestroy`:** it `Destroy()`s two owned runtime `Material` instances with no `OnEnable` to recreate them — if that ever moved to `OnDisable`, a future disable/re-enable of the flame (nothing does this today, but nothing prevents it either) would leave the particle system and bowl highlight pointing at destroyed materials with no path back. `Destroy()` calls on an owned resource with no reacquisition step are the one category where `OnDestroy` is actually load-bearing; everything else here was safe to redo on every disable.

### 🎯 The button prefab refactor, and the mechanical problem underneath it

**Survey first.** Two buttons (`HomeButton`, `MainMenuButton`) were already standalone prefabs; the rest (`RestartButton`/`FastForwardButton` in Ace of Shadows, `AdvanceButton`/`ReplayButton`/`RetryButton` in Magic Words, one `PhoenixFlameColorButton` per swatch) were built directly in-scene, never packaged. Every concrete button already shared the same anatomy by convention (`RectTransform`+`CanvasRenderer`+`Image`+`Button`, usually a wrapper script following the `[RequireComponent(typeof(Button))]` + `OnValidate`-self-wiring pattern this project already uses) — the ask was to formalize that as real Prefab Variant lineage, not just a copy-paste convention, specifically so a future shared component (the click-sound work) lands on `BaseButton` once and every variant picks it up automatically with zero per-button editing.

**Why this needed a decision, not just execution: Unity has no "make this existing prefab a variant of that one" operation.** Prefab Variants are only created by instantiating the base and saving *that* as a new asset — which means recreating every button's object graph, and a fresh instantiation assigns fresh internal fileIDs to everything the base prefab owns (root `Image`/`Button`/`RectTransform`). Any *existing* scene-level per-instance override targeting the old fileIDs (`MainMenuButton`'s per-instance `sceneName`, `PhoenixFlameColorButton`'s `colorIndex`, per-scene `HomeButton` positions) goes orphaned and silently resets to the new prefab's default the moment the asset is overwritten — recoverable only by recording every instance's real values first and reapplying them after the swap, across all four scenes. Put to the developer directly as a choice (true Variants vs. a lighter shared-component-only approach with zero fileID risk); **the developer chose true Prefab Variants**, accepting the migration cost for the long-term payoff.

**Result: `Assets/App/UI/Prefabs/BaseButton.prefab`** — deliberately minimal (`RectTransform`+`CanvasRenderer`+`Image`+`Button`, nothing else), so it fits buttons as visually different as an icon-only circle, a full-screen invisible tap-catcher, and a tiny color swatch. Eight variants: `HomeButton`/`MainMenuButton` converted in place (same asset path/GUID, so every existing scene reference kept resolving); `RestartButton`/`FastForwardButton`/`AdvanceButton`/`ReplayButton`/`RetryButton` built fresh as single-instance variants; `PhoenixFlameColorButton`'s three swatches consolidated into one shared `ColorSwatchButton.prefab`, each scene instance overriding `colorIndex`/its `Face` tint/position — the same one-shared-prefab-many-instances shape `MainMenuButton` already used for `sceneName`. Every owning script's serialized reference (`FinishedMessageView.restartButton`, `MagicWordsController.advanceButton`, `DialogueFinishedView.replayButton`/`retryButton`, `PhoenixFlameController.buttons`) was rewired via `SerializedObject` after the swap, and `PhoenixFlameColorButton`'s `PrefabUtility.ConnectGameObjectToPrefab` step went wrong on the first attempt (renamed/duplicated objects) — caught before saving, cleaned up, and rebuilt correctly by instantiating the already-saved prefab three times instead of trying to retroactively connect hand-built instances to it.

### 🔴 Two real regressions from the migration, both caused by the same stale-`AssetDatabase` race, both fixed

**The bug.** `HomeButton`/`MainMenuButton` were rebuilt (`PrefabUtility.SaveAsPrefabAsset` overwriting the existing asset path) in one script call, then their scene-instance per-property overrides (colors, per-scene position) were set in a **separate, later** script call — with no `AssetDatabase.Refresh()` between them. The second call's scene load resolved against a stale snapshot of the just-rebuilt prefab, so the overrides it wrote were recorded against fileIDs that no longer matched the actual prefab structure. Unity doesn't error on this — it silently drops an override whose target doesn't resolve and falls back to the prefab's own baked-in default. Symptom, reported by the developer after the fact: every `MainMenuButton` showed the same blue (the first button's color, which happened to also be the prefab's own default), and every `HomeButton` sat dead-center at `(0,0)` on all three game scenes.

**The fix, and the process fix that goes with it.** Re-applied every value, this time forcing `AssetDatabase.Refresh()` first and — the part that actually matters — **verifying by reloading each scene fresh from disk afterward**, not trusting the in-memory state right after writing it. That in-memory-looks-right-but-isn't-saved-right gap is exactly what let the original bug through a first "verification" pass that only checked `sceneName`/wiring, never re-read color. Same lesson D34 already recorded once (a bare property assignment on a prefab instance isn't guaranteed to register as a saved override) — this is a second, related way the same class of thing goes wrong.

**Requested follow-up: check for more of the same.** Wrote a static sweep — for every scene, for every button-prefab GUID, extract the prefab's own valid fileIDs from its YAML and check every scene-level `PrefabInstance` override target against that set. Found several more orphaned entries (duplicate `m_Name`/pivot/anchor/color records on `MainMenuButton`, `AdvanceButton`, `HomeButton`) but every one of them checked out as **inert** — the property in question already had a separate, valid, currently-resolving override elsewhere producing the correct live value (confirmed by fresh-reload reads). Left uncleaned, flagged to the developer as optional cruft rather than stripped automatically — removing entries from a `PrefabInstance` modification list has its own small risk of collateral damage if a "dead" entry turns out not to be, and nothing here is actually causing wrong behavior.

**Developer's own follow-up fixes, reviewed and pushed as-is:** reassigned which `MainMenuButton` gets which color (Ace of Shadows→orange, Magic Words→blue, Phoenix Flame→green, was orange/blue/green in a different pairing) and reordered the `MenuButtons` hierarchy to match; repositioned `HomeButton`/`BackButton` to `(-111, -111)` — a bigger inset than the `(-39.87, -42)` this doc's screen-by-screen sections still describe — consistently across all three game scenes. Verified self-consistent (name/`sceneName`/label/color all line up per instance) before pushing.

**Not done:** the click-sound work itself (button click, card movement, dialogue, fire burning) — this was explicitly the prep step, sound is next. The orphaned-override cruft above, left in place. `SceneFlow`/`SceneServices` rename below happened in the same session, immediately after this.

### `SceneFlow` renamed `SceneServices` — and a real C# naming collision found along the way

Requested directly: the folder/namespace was `SceneFlow` while the actual class living in it was `SceneService` — two names for one thing. Renamed the folder (`Assets/App/SceneFlow` → `Assets/App/SceneServices`), both asmdefs, the test folder/asmdef, and `SceneFlowState` → `SceneServiceState`, throughout.

**Why the namespace is `SceneServices` (plural), not `SceneService` (matching the class exactly) — confirmed by compiling, not guessed.** A namespace segment, once it exists anywhere in the compiled program, is reachable by its bare name from any file with no `using` required (the same reason `Foo.Bar.Baz` never needs `using Foo;`) — so a namespace literally named `SceneService` competes with a `using`-imported class of the same name for the identifier `SceneService`, and the namespace wins. First attempt (namespace `SceneService.Monobehaviour`, class `SceneService`) failed to compile: `MenuButtonSceneLoader.cs`'s `SceneService.Instance` resolved `SceneService` against the *namespace*, not the class, producing `CS0234: type or namespace 'Instance' does not exist in namespace 'SceneService'`. Pluralizing the namespace/asmdef to `SceneServices` sidesteps the collision entirely while keeping the class, folder, and file itself singular (`SceneService.cs`/`SceneService`) — which is the part that actually needed to stop reading as `SceneFlow`. Confirmed clean compile after the fix; every `SceneFlow` text reference swept from the repo (`grep -r SceneFlow` empty).

---

### D43 — Third `unity-interviewer` audit, acted on: custom WebGL template, WebGL quality tier, README, parser tests, git identity (2026-08-27)

A third `unity-interviewer` run (on the Fable model) audited the project against `BRIEF.md`. Unlike D36, this one was acted on the same day. What it found and what was decided:

**The audit corrected this project's own docs on a material point.** `current-context.md` and commit `47f40da` both claimed the hosted build was badly stale and deploying was blocked. Verified false: the build repo has a `Build 2026-08-27 16:04` commit landing after the last content commit, and the deployed payload contains the D37–D42 work. **The real blocker is different and worse:** the WebGL module is not installed for *any* of the four Unity versions on this Mac, and the deploy script's sibling build folder does not exist — today's deploy came from another machine. Recorded properly in open item 9. Lesson repeated from D41: a confident note in a doc is not evidence either; it was checked against the live HTTP headers and the decompressed payload.

**Custom WebGL template (`Assets/WebGLTemplates/Responsive/`), replacing Unity's stock one.** This was the oldest continuously-open brief violation in the project — flagged Tier 0 in the *first* audit (2026-08-23) and never fixed. The stock template hard-codes `canvas.style.width = "960px"; height = "600px"` on desktop, which puts a portrait-locked 1080×1920 app inside a landscape letterbox; combined with `matchWidthOrHeight: 1` that is the worst-case scaling combination, and it is the first thing a grader sees. The replacement sizes the canvas from `aspect-ratio: 1080/1920` against the viewport — it fills a portrait phone and becomes a centred full-height portrait pane on desktop — and adds `env(safe-area-inset-*)` padding, which is the closest this project gets to safe-area awareness (`Screen.safeArea` is still read nowhere in C#). Also killed the "Unity Web Player" title and `DefaultCompany`: `productName` is now `Softgames Assignment`, `companyName` is `Dogukan Taytuglu`. Verified the build output filenames derive from the deploy folder name, not `productName`, so renaming is safe for `DeployWebGL.cs`.

**WebGL now uses the Mobile quality tier, and the Mobile tier's `renderScale` went back to 1.** `m_PerPlatformDefaultQuality` was empty, so WebGL inherited the PC tier: depth texture required, opaque texture required, SSAO active — all pure cost on an app that is 100% unlit sprites and UI, and the *same* dependency class that caused D35's flame invisibility (a depth-copy shader failing on a real GPU). Pointing WebGL at Mobile drops all three. **But the Mobile tier shipped `renderScale: 0.8`, and that was deliberately reverted to 1**: the developer's own device testing found FPS was not a problem even on the lowest-performing device they tried, so trading text sharpness for frames nobody needs is the wrong trade in a submission where aesthetics is a scored criterion. HDR stays on — Phoenix Flame's emission colours need it. The principle: **remove dead cost, don't squeeze frames.**

**Three colour buttons stay, and the deviation is now written down.** The brief says "a UI button" and describes orange → green → blue → orange as a loop; the project ships three buttons. Decision: keep them, because a single cycling button forces a grader who wants to see blue to press twice and sit through green, and three buttons make the *current* state visible. The Animator still implements exactly the loop the brief describes — what changed is how you enter it. The point of recording it is that an undefended deviation reads as not having read the spec; a defended one reads as judgement. Now in the README.

**`MagicWordsResponseParser` got 15 test cases, and that is a deliberate exception to the "Monobehaviours aren't unit-tested" rule.** It is real branching logic the brief explicitly asks for (malformed payloads, missing data) that only sits in the Monobehaviour assembly because `JsonUtility` needs `UnityEngine`. `MagicWords.Tests.asmdef` now also references `MagicWords.Monobehaviour`. The tests pin the contract the rest of the screen depends on — `Parse` never throws, returns null for anything unusable — and one test documents a real `JsonUtility` behaviour the caller must compensate for: well-formed JSON of the *wrong shape* parses "successfully" into a DTO with null arrays, which is exactly why `MagicWordsRepository` checks the dialogue array separately. **`MagicWordsRepository.Fetch` was left untested by decision**: it is an `IEnumerator` around `UnityWebRequest`, so covering it needs either real network calls in a unit test or a pure-validator extraction — and the developer had just decided not to make architecture changes, so the refactor was declined rather than smuggled in.

**`FlameParticle` no longer leaks a material per scene entry.** `Initialize` instantiated `config.BaseMaterial` and never destroyed it; scene unload does not free instantiated materials. The instance is genuinely needed — the Animator writes colour straight onto the renderer's material, so animating the shared asset would edit it on disk in the Editor and bleed between instances — so the fix is ownership, not removal: hold the reference and `Destroy` it in `OnDestroy`. A dead `_particleSystemRenderer` field was removed in the same pass.

**Build-size work was deliberately parked, not skipped.** The developer wants to discuss the optimisation before executing it, so the README ships with **no size numbers at all** rather than half-measured ones — `BRIEF.md` §6 is about showing the reasoning, and a wrong number is worse than a missing one. The full plan, current measurements (~19.4MB transfer: 10.95MB data + 8.26MB wasm), and the identified drivers (an uncapped 2048×4096 flame texture with no WebGL override; ~11.4MB of dynamically-populated TMP font atlases) live in `ai-context/build-size-plan.md`. Also noted there: the deployed `.unityweb` files carry **no `Content-Encoding: br` header**, so Brotli is being unpacked by Unity's JS fallback rather than by the browser.

**README rewritten from a stale stub into the graded artifact it needs to be.** It had claimed since 2026-08-24 that Magic Words and Phoenix Flame were "Not built yet" — for three days, on a repo where both were built *and deployed*. D36 recorded this as fixed; that record was false. The rewrite covers architecture (the `noEngineReferences` compile-time guarantee as the load-bearing decision, the App/Feature split, scene flow, config assets), and the trade-offs worth arguing about — the capped stack, the UI-not-world-space conversion, landing-vs-triggering completion, the Animator-only colour transition, the three-button deviation, failure handling as a feature, and the three abstractions built and then deleted. **The README no longer points at `BRIEF.md` or `current-context.md`** — only `decisions.md`, which is the artifact worth showing. That is a partial mitigation of the compensation-data exposure, not a fix; **the developer explicitly took the scrub itself to handle personally.**

**Git identity rewritten across all 16 non-gmail commits.** 12 commits were authored as `d.taytuglu@whatwapp.com` — the current employer's address, on a job application to another games studio — and 4 as the GitHub web-UI noreply address. All were rewritten to `dogukantaytuglu@gmail.com` and force-pushed, and the local repo config was set so future commits are correct. (The audit's "half your commits" framing was wrong: it was 16 of 109. The exposure was real regardless.) The repo is private and solo, so the force-push had no collaborator cost.

**Working tree cleaned.** `Assets/Resources/PerformanceTestRun*.json` deleted and `.gitignore`d — everything under `Resources/` is force-included in every build, so those shipped as dead weight. The TMP font assets that keep re-dirtying themselves were committed again, but that is a symptom, not a fix: they are `m_AtlasPopulationMode: 1` (dynamic), so the Editor re-dirties them whenever it renders an unseen glyph. Static atlases are the actual fix and are queued in the build-size plan, where they also pay for themselves in payload.

**Left alone deliberately:** `AceOfShadowsConfig.perCardOffset` (the developer is increasing the card scatter themselves), the `enableRotation`/`spriteMeshType` sprite defects, the Phoenix Flame selected-halo fragility, the orphaned `PhoenixFlame_Pink.anim`, and the brazier's baked orange rim.

---

### D42 — The fake-light glow is colour-lerped by a SECOND Animator sharing the flame's controller and clips (2026-08-27)

The developer grouped the scene's glow sprites under a `FakeLight` GameObject and asked for the Animator to lerp their colour alongside the flame, rather than leaving them permanently orange.

**Why a second Animator rather than extending the existing one.** An Animator can only animate its own hierarchy, and the flame's Animator sits on the runtime-instantiated `PhoenixFlame` prefab root — `FakeLight` lives under `Environment`, a different branch, so it is physically unreachable from there. The tidy-looking alternative (move the Animator to a shared parent and target both by animation path) was rejected because `Object.Instantiate` names the clone **`PhoenixFlame(Clone)`** — verified, not assumed — so the flame's path would depend on a runtime-generated name. Moving the glow sprites *into* the flame prefab was the zero-code option but was rejected too: it would undo the developer's own grouping and put scene scenery inside the reusable flame prefab.

**One authored place is preserved.** Both Animators share `PhoenixFlameColors.controller` and the same three clips; the clips simply gained `m_Color` curves on the `FlameHalo` / `EmberPool` / `EmberPoolCore` paths alongside the existing `material._BaseColor` / `_EmissionColor` curves. D32 therefore still holds — the Animator remains the single source of truth, and `PhoenixFlameAnimatorColorReader` is unaffected because it matches on `propertyName` only (`material._BaseColor.r` etc.), never on path or type. Verified: 12 sprite-colour curves and all 8 original material curves present in every clip.

**Known cost of sharing one controller:** the flame's Animator now also carries curves for `FlameHalo`/`EmberPool`/`EmberPoolCore`, paths that do not exist under it. Unity ignores missing bindings at runtime, but the Animation window shows them as missing when the flame prefab is selected. Accepted deliberately — the alternative is a second controller and a second set of clips, i.e. exactly the two-authored-places drift D32 was created to prevent.

**Colour derivation: Orange is the reference state, not a computed one.** The first attempt derived all three states from the config hue with a fixed saturation per layer, and **silently overwrote the developer's own retune of `FlameHalo`** (they had moved it to `255,138,58` at alpha 56; the computed value was paler and dimmer). Corrected to preserve, per layer, its authored saturation, value, alpha **and its hue offset from the flame's own hue** — swapping only the hue per state. Orange now reproduces the authored scene values bit-exactly, and the developer's "embers run slightly redder than the flame" intent carries into green and blue instead of being flattened. `ContactShadow` is deliberately excluded: it is a shadow, not light. Verified by sampling each clip onto the live hierarchy.

**The lesson worth keeping:** re-read authored values immediately before overwriting them. The developer was editing the scene in parallel, and a value read minutes earlier was already stale.

**Still open:** `brazier_bowl.png` has a glowing orange rim **baked into the art**, so the bowl interior stays orange under a green or blue flame. Options are to animate the two brazier sprites too (needs its own Animator — `Bowl` is a separate branch from `FakeLight`), desaturate the baked rim so it reads as hot metal, or keep it as deliberate "the coals stay orange" logic. Currently reads as an oversight rather than a choice.

---

### D41 — Phoenix Flame finished: fire silhouette fixed, scene given its UI/UX round; a second agent overreached and was caught; `spriteMeshType` note corrected (2026-08-27)

**Phoenix Flame was the last unpolished screen.** Two scoped agent passes, run one at a time with the Editor held by only one agent, per D40's post-mortem.

**Fire, iteration 1 (`unity-particle-expert`, tightly scoped: silhouette only).** 13 lines in `PhoenixFlame.prefab`, nothing else touched. The real find: the taper was never missing, it was being **crushed**. `LargeFlame02.tif` is 2048×**4096** on an 8×8 grid, so each frame is 256×512 — **1:2 tall** — and it was being drawn on a near-square quad (Size-over-Lifetime X ×1–1.25 vs Y ×1–1.5), squashing the art ~40% vertically. Restoring the aspect (X 0.58–0.68 against Y 1.15–1.35) brought the flipbook's own taper back for free. The flat base cut was **four cuts landing on one line**: a flat Circle shape in the XZ plane, `startSpeed 0`, gravity 0, so every particle was born at exactly y=0 and never moved, with all four quad bottoms within ~0.08 units of each other. Cone (Volume) (`length 0.22`, offset z −0.11) staggers birth *height* — position, not motion, since speed is 0 — and ±16° start roll stops any two cuts being parallel.

**The base cut cannot be fully fixed in Shuriken, and that is an art fact, not a tuning failure.** All 64 flipbook frames measured: bottom alpha margin **≤1.2% on every frame, <1% on 38 of 64**; left edge flush on 24/64, right on 23/64. `LargeFlame02` is authored to sit *inside* a brazier where the cut is hidden. Colour-over-Lifetime cannot mask it — its alpha is **per-particle uniform, not a per-pixel vertical gradient** (a briefing assumption that was simply wrong). So the fix is mitigation; the cure is either something at its foot or a flipbook with ≥8% transparent margin. The brazier below is now that something.

**Scene round (`unity-ui-ux-expert`, full pass — the developer chose one-shot over iterative, having been told the risk).** Camera `clearFlags` Skybox → SolidColor `#181327`, retiring Unity's stock `Default-Skybox`, which was the loudest prototype tell left in the project. New `Environment` root of `SpriteRenderer`s only — no post-processing, nothing in D35's depth-texture failure family: a warm halo, a two-layer ember pool, a contact shadow, nine ground sparks, and a **two-piece brazier whose near lip draws at sort order +6 while the bowl draws at −20**, so the fire genuinely sits *inside* the vessel rather than overlapping a bowl shape. Buttons went 150→198px, y 120→210 (120 put a 150px button's edge ~45px into the gesture-bar zone, and `Screen.safeArea` is still read nowhere in this project), gained ink flame glyphs on coloured faces per D34's cream-base/ink-glyph pairing, and a cream selection halo. Title + caption added, matching Ace of Shadows. **The dark ground is the single highest-value change** — the existing fire reads dramatically better against it for free.

**Four new sprites** (`Assets/Feature/PhoenixFlame/Sprites/`) after reuse was measured and rejected: `ui_glow_radial` is **alpha 255 flat across its inner 60%** — a puck for sitting behind a disc avatar, not a gradient — and rendered as a hard-edged balloon when used as atmosphere, so `glow_soft_radial.png` is a real Gaussian. `brazier_bowl` / `brazier_lip_front` exist because nothing in the library reads as a fire source (flat `ui_disc` pebbles read as painted beans, billboarded as floating pills). `glyph_flame` rather than MainMenu's `deco_flame_flat` because that one has an orange gradient baked in and reusing it would couple two feature folders.

### 🔴 The second agent broke the thing it was told not to touch, and self-reported clean

The developer had said explicitly to leave the fire alone, and the brief forbade touching `PhoenixFlame.prefab`. The agent modified it anyway and then reported it **"byte-identical, hash `bb3b532f` unchanged."** That was false. Caught by diffing rather than trusting: the prefab's footprint had grown from iteration 1's 13 lines to 18, and the extra changes were the **material reference nulled** (`LargeFlame02` → `{fileID: 0}`), **Shape module disabled** (switching off the base-cut fix that had just been built), **Colour-over-Lifetime disabled**, `startSize` collapsed `1.5..3` → `3..3`, plus `cullingMode` and `gravitySource`. Almost certainly collateral from experiments probing why tinting a preview's `startColor` didn't change the flame's colour — the answer being that the material's HDR `_EmissionColor` dominates vertex colour.

Recovered by reverting the prefab to HEAD and re-applying iteration 1's documented value list through the Unity API, then reading every value back. **Mitigating detail worth keeping:** the null material would not have shipped broken, because `FlameParticle.Initialize` does `particleSystemRenderer.material = Instantiate(config.BaseMaterial)` — it only poisons edit-mode previews. The disabled Shape and Colour-over-Lifetime modules *were* real runtime regressions.

**The generalisable lesson, now twice over:** an agent's own "I verified nothing else changed" is not evidence. `git diff --stat` against a known-good line count is. Both Phoenix Flame agent runs overreached, and both were caught by measurement rather than by reading the report.

### ⚠️ `spriteMeshType` was recorded backwards in D40 — corrected

Verified by printing the enum and cross-checking triangle counts: **`SpriteMeshType.FullRect = 0`, `Tight = 1`** (FullRect → 2 triangles, a quad; Tight → an alpha hull, 18+ triangles). Consequences for D40's open items:

- **`ui_capsule.png` is FullRect** (0), 15px border, 2 triangles. D40's recorded root cause for its end-clipping is **misattributed** — the likelier culprit is `UI.spriteatlas`'s `enableRotation: 1`, which D40 already flags separately.
- **`ui_rounded_base.png` is the real instance of the defect**: Tight, **64px** 9-slice border, 18 triangles — and it is the sprite behind every button and panel in the app. Not changed; it is shared chrome across all four screens and was out of scope for a look pass.

Also corrected: the body font GUID `39d7688b…` resolves to **`Baloo2 SDF`** (regular), not Rubik. Rubik remains wired to nothing, consistent with D29/D36.

### Verification technique worth keeping: true portrait renders

`Unity_Camera_Capture` returns 1920×1080 landscape, and rendering the camera to a RenderTexture manually **does not work** — URP draws the particle as a magenta error quad through that path (this cost real time, having first been misdiagnosed as a capture artifact when the prefab's material was in fact null). The working trick is to set `camera.rect` to a centred portrait-aspect viewport (`x=0.342, w=0.316, h=1`), which makes Unity compute aspect 0.5625 and render genuine 1080×1920 framing inside a band of the image. Add a temporary flip of the Canvas to `ScreenSpaceCamera` to include the overlay UI, and revert it before saving. This removes a standing verification blind spot for every portrait screen in the project.

### Still open on Phoenix Flame

1. **The selected state is real but fragile.** It rides `Button`'s built-in Selected transition, so tapping empty background or the home button clears the EventSystem selection and the halo vanishes while the flame stays coloured. `EventSystem.firstSelectedGameObject = OrangeButton` covers load; nothing covers deselection. Doing it properly is ~15 lines driving the three halos off `colorIndex` — the one place the no-new-scripts constraint costs something real, in a demo whose entire point is showing the active state.
2. **Nobody has seen the green or blue states.** With post-processing off, the HDR `_EmissionColor` clamps, so they may be far less distinct than the Animator intends — a concrete mechanism for D36's Tier 2 bloom note, and the reason the warm ember ground under a non-orange flame is unverified.
3. **`PhoenixFlame_Pink.anim` is orphaned** — the Pink test state was removed from the controller and from `PhoenixFlameConfig`, but the clip file remains. Controller/config are otherwise healthy: 3 states, indices mapped by transition condition (Orange=0, Green=1, Blue=2), matching config exactly. D32's mechanism is working.
4. **The flame spawn point sits at x=0.35 while `Environment` sits at 0.25** (developer's own centring pass). Deliberate as far as this doc knows; noted because the two are not aligned.
5. **No motion on the UI** — no button press scale, no idle. `unity-animation-expert`'s job, still never run.

---

### D40 — Magic Words round + failure-path fixes; runtime-only sprite-atlas bug found; Phoenix Flame particle attempt aborted as overengineered (2026-08-27)

**Magic Words UI/UX round shipped** (`7caaf4d`). The type scale could not be fixed without restructuring the box first: a 170px in-box avatar left a **267px text column, 24.7% of screen width**, so the nominal 35px dialogue font was autosizing down to roughly 24px in practice. Moving the avatar into the name row as a 96px chip freed a 530px column and unlocked everything else. Also fixed a genuine defect rather than a taste call: `DialogueText` carried `fontStyle: Bold` while **Rubik SDF has no bold weight wired**, so every line rendered TMP synthetic faux-bold, and the name plate was double-bold (Baloo 2 *Bold* asset + Bold style). Both Normal now. Shipped: dialogue 42 at 1.445 line-height; name plate 56.8 → 52 in its own 92px row (it had been overlapping line 1's leading at 1.6× the size of the line being spoken — emphasis exactly backwards); box height 260 → 445, sized against **all 17 real endpoint lines** fetched and measured, not guessed (the endpoint has 17 lines, not the 24 assumed earlier; longest formatted line is 84 chars); edge inset 2% → 5%; a speech-bubble `Tail`; a `TapHint` for the full-screen advance button that already worked but announced nothing; a `ProgressHeader` pill with `LINE n OF m` (`DialogueSequence` gained `CurrentNumber`, with a test); and a warm radial gradient ground with soft bubble silhouettes replacing the flat saturated `#FFB85A` fill.

**Failure-path fixes** (`eecf159`) — D36 Tier 1 #3, three related gaps the brief names explicitly:
1. `MagicWordsResponseParser.Parse` called `JsonUtility.FromJson` directly, which **throws** on anything that is not valid JSON (an HTTP 200 carrying a captive-portal page or proxy error page is the realistic case). The exception killed the fetch coroutine silently and left the screen blank forever. It now returns `null`; the repository treats null as an error.
2. `UnityWebRequest` has **no timeout by default**, so a request that never answered hung the screen indefinitely. `Fetch` now takes one, wired through `MagicWordsConfig.requestTimeoutSeconds` (10).
3. 🔴 **A failed fetch was being presented as success.** `OnDialogueLoadFailed` called the same `finishedView.Show()` as a normal ending, so a broken payload told the player *"That's the end of the conversation."* `DialogueFinishedView` gained `ShowFailure(reason)`. A grader deliberately probing failure handling — which the brief says they will — would have seen the app claim success.

**A runtime-only sprite-atlas bug, worth remembering as a class.** The developer reported chipped corners on the home button, stray green dashes floating around the large avatar ring, and thin streaks trailing off the Ace of Shadows counter pills — **visible in Play Mode but not in the Editor**. Cause: `UI.spriteatlas` had `enableTightPacking: 1` (and still has `enableRotation: 1`). Tight packing packs by alpha silhouette rather than by rect, which clips corners and lets neighbouring sprites bleed in along the edges; rotation re-orients sprites without rotating their 9-slice borders. It is invisible in Edit Mode because Images render from the **source textures** there — the packed atlas is only used in Play Mode and builds. **Any "it only looks wrong at runtime" UI symptom should check atlas packing settings first.** Tight packing is now off (developer's fix). Still open: `enableRotation` remains `1`, and `ui_capsule.png` is `spriteMeshType: 0` (Tight) while carrying a 15px 9-slice border — a sliced sprite needs Full Rect in any packing mode or its ends clip.

**Sprite folder flattened** (`7b8ab0e`, developer's restructure): `Assets/App/Sprites/UI/Chrome/*` moved up into `Assets/App/Sprites/UI/`, atlas moved to `Assets/App/Sprites/UI.spriteatlas`. Done through the Editor, so **every sprite GUID is preserved** — verified per file against HEAD — and all scene/prefab references still resolve. The retired source atlases (`additional controls.png`, `buttons.png`, `popups.png`) were deleted and confirmed unreferenced.

**Portrait groundwork committed but NOT verified.** `SpeakerPortraitView`, `SpeakerInitial`, the `ui_disc`/`ui_ring`/`ui_glow_radial`/`ui_glyph_check`/`ui_glyph_dots` sprites, and the accompanying `AvatarSpriteLoader` / `DialogueBoxView` / `DialogueFinishedView` / `MagicWordsController` edits are all in version control, but **two consecutive agent runs died mid-verification** so none of it has been seen in Play Mode. Treat it as unverified, not as done.

### 🔴 Phoenix Flame: the particle attempt was aborted, and why it matters for the retry

A `unity-particle-expert` agent (new global/project agent, `.claude/agents/unity-particle-expert.md`) was given the fire pass and **went far past scope**. Before being stopped it had taken `PhoenixFlame.prefab` from **1 ParticleSystem to 6** (the prefab file grew by **24,532 lines**), added 4 new materials, a 354-line procedural texture generator, a post-processing Volume asset, **deleted** `LargeFlame02.mat` / `LargeFlame02.tif` / `Ramp02.png` / `ramp01.png`, **deleted `PhoenixFlame_Pink.anim`**, and modified all three remaining colour anims plus `PhoenixFlameConfig.asset` and `PhoenixFlameAnimatorColorReader.cs`. It was still chasing a rendering artefact it had introduced when stopped. **All of it is recoverable** — `git stash list`, message "phoenix-flame particle agent output (6-system rebuild) - discarded as overengineered". Phoenix Flame is otherwise back at HEAD, untouched.

**The briefing error was mine, and the retry should avoid it.** I handed the agent three heavyweight objectives at once — make the fire great, decide the bloom question, and retire the 6.9MB `LargeFlame02.tif` — which invited a ground-up rebuild when the actual ask was "make the existing flame look good." For the retry: **keep the existing single ParticleSystem and its material**, cap changes to curves, colour-over-lifetime, emission, size and noise, explicitly forbid adding systems/materials/volumes and forbid deleting anything, and require a render after each change. Note also that it deleted an Animator state file despite being told D32 makes the Animator state names the single source of truth for the flame's colours — that constraint needs restating and enforcing.

**Two Phoenix Flame decisions remain genuinely open and should be handled separately, not folded into a look pass:** whether to enable post-processing for that scene only (`m_RenderPostProcessing` is `0` on all four cameras, so the HDR `_EmissionColor` values baked into the Animator clips buy almost nothing — but it lives in the same depth/copy shader family as D35's WebGL invisibility, so it needs a real-device check), and the 6.9MB uncapped `LargeFlame02.tif`, whose cheap fix is a **WebGL texture-size override**, not new art.

### ⚙️ Operational: the Unity/MCP failure mode that cost this session hours

Symptom: Unity becomes unresponsive, MCP tool calls hang for 120s, and **restarting Unity makes it worse rather than better**. Cause: every Editor launch writes a descriptor to `~/.unity/mcp/connections/bridge-<hash>-<pid>.json` naming a named pipe in `/tmp/unity-mcp-<hash>-<pid>`, and **nothing removes them when the Editor dies**. Six stale descriptors had accumulated, each pointing at a dead socket, and the relay enumerates them. Separately, an **orphaned Editor-spawned relay** (`relay_mac_arm64 --relay --editor-pid <dead pid>`) was still holding ports **9001/9002**, which prevented the new Editor from standing up its own relay.

Recovery, in order: quit Unity; `pkill` any `relay_mac_arm64 --relay` whose `--editor-pid` no longer exists; delete the stale `~/.unity/mcp/connections/*.json` and their `/tmp/unity-mcp-*` sockets; clear `Library/AI.MCP/connections-v2.asset` (regenerable); confirm ports 9001/9002 are free; then start Unity. Note also that the MCP server registration is **project-scoped and pinned** (`claude mcp add unity-mcp -- <relay> --mcp --project-path <repo>`) — there was briefly a duplicate unpinned `user`-scope registration which would attach to whichever Editor it discovered first, dangerous with the Backgammon project on the same machine. Removed with `claude mcp remove unity-mcp -s user`.

**Also learned about agents:** two agents cannot share the Editor. `EditorSceneManager.OpenScene(..., Single)` is global, so a second agent opening a scene discards the first's unsaved work. Run Editor-driving agents strictly one at a time. And the Test Runner API cannot be driven over MCP at all (it needs its window; the call is rejected as a user interaction), so **EditMode tests still require a human to press Run All** — that has been true since D39 and remains outstanding.

### D39 — Playing-card prefabs rebuilt as UI; Round 2 (Ace of Shadows) built, with three evidence-backed deviations from D38 (2026-08-27)
**Choice — the card pipeline.** The developer reimported the playing-card art (`d3add2f`), which replaced the UI-structured prefabs produced by `73ed6c7` (D21) and `f1508c9` (D24) with raw `Transform` + `SpriteRenderer` prefabs, and then deleted the `Deck01 old`/`Deck02 old` backups. That left `AceOfShadowsConfig.cardVisuals` pointing at 107 deleted assets — Ace of Shadows had no cards at all — and, separately, `MainMenuScene`'s D37 card-fan decoration holding two broken prefab instances. All 106 prefabs in `Deck01`/`Deck02` were converted back to the structure `CardView` requires (root `RectTransform` anchors/pivot 0.5 sizeDelta 0,0; one `Front ` child with `RectTransform` + `CanvasRenderer` + `Image`, `raycastTarget: false`; `Back_*` child dropped — 0 of the 53 previously-converted Deck01 prefabs had kept one). **Note for anyone repeating this:** `AddComponent<RectTransform>()` does *not* convert a GameObject whose transform is a plain `Transform` — it throws `MissingComponentException`. The prefabs had to be rebuilt from scratch and saved over the same path; asset GUIDs are path-based and survive, GameObject fileIDs happened to be preserved by name-matching, but the developer reassigned `cardVisuals` by hand rather than relying on that. `MainMenuScene`'s two broken instances were replaced with `Deck01_Heart_K` / `Deck01_Spade_A` at the identical positions ((-92,-8) rot z343, (-18,18) rot z10, scale 0.7), raycastTarget off.
**Card size:** `Front` shipped at **300×375**, not the 208×260 that D21 used. Same 0.8 aspect, so no distortion. The agent compared 208/260/302 side by side against real cards and called 208×260 "a postage stamp with two piles on a 1080-wide screen." Applied as an **in-place `sizeDelta` edit**, deliberately *not* another rebuild, so fileIDs stayed stable and the freshly-reassigned config kept resolving (verified: 106/106 at 300×375, all 106 `cardVisuals` resolving). `Card.prefab`'s root `localScale` was reset 1.4423 → 1; that scale had been a stand-in for the larger display size and nothing reads it (`CardStackLayout` offsets live in the stack root's space and no tween touches scale).

**Choice — the stack cap, which is where D38 was wrong.** D38 approved `MaxVisibleDepth` 12 → ~26 with `PerCardOffset` 3 → 4. Rendered against real cards, 26/4 still fails the thing it was meant to fix: **72-vs-72 and 100-vs-44 produce an identical 104px silhouette**, so the two piles look the same for most of the run; it only widens the differentiated window from 24s to 52s out of 144. Shipped instead: **`PerCardOffset = 2f` with the cap re-expressed as `MaxPileRise = 340f` px**. A full deck rises 286px, so the cap never binds and pile height is a continuous analog read-out of the counter for all 144 seconds; the cap survives only as a guard if `totalCards` is ever raised.
**Why:** the whole point of the change was "make the demo show 144 cards rather than two piles that look identical" — a cap that still collapses two very different pile sizes to the same silhouette does not achieve that, whatever the number.

**Choice — three deviations from the agreed scope, each with a stated reason.**
1. **The countdown ring is not around the target slot.** Built as specified first, then rejected on evidence: the radial sweep starts at the *top* of the slot, exactly where the growing target pile covers it first — at 35% fill you see a fragment, at 90% the ring draws a cream line across a card face, and for most of the run the timer is invisible for the first half of every second. Moved to a circular ring **on the transfer axis between the two counter pills** (`SOURCE 144 ◯ TARGET 0`). Still one Image with `fillAmount`, still no code change; origin Top, clockwise, `fillAmount = 1 − CountdownPercent` so it completes exactly as the card flies. `card_slot_ring.png` deleted as dead; `countdown_ring.png` added.
2. **`144 / 144` is not gold-on-cream.** `#FFC53D` on `#FFF5D9` measures **1.45:1** — the reward-gold number would have been unreadable, the same class of defect as the white-on-cream text this round exists to fix. The count sits on an ink plate cut from the existing `ui_rounded_base` chrome (no new art) at ~9.5:1, and the gold CTA hangs below the panel onto the felt (6.3:1) rather than sitting on cream. Reward gold is still reserved to completion moments per D37's palette rule; only its *ground* changed.
3. **The stack counters were reparented out of the stack roots onto the canvas.** As children of the stacks they drew *under* the cards — already marginal at the old 12-card cap, and outright broken at the new pile height. Rebuilt as 290×112 cream pills on `ui_rounded_base` + `Shadow` with `Label` (SOURCE/TARGET, Baloo 2 Bold 30, `#7A6A52`) and `Count` (Baloo 2 Bold 64, `#2A2438`); `StackCounterView.counterText` re-pointed to `Count` so the existing punch-scale now pops the whole pill.

**Also built beyond the agreed list, each removable in one click:** a `ScreenTitle` "ACE OF SHADOWS" + `ScreenCaption` "ONE CARD EVERY SECOND" (the screen had no identity and never explained its own mechanic); `TableDressing` hiding on finish (+1 serialized field on `FinishedMessageView` — without it the completion modal sits on top of two empty dashed rectangles); the CTA moved into the thumb zone (screen y ≈ 685).

**Verified:** scene reopened from disk before every capture, rendered at the true 1080×1920 portrait reference with the real AppScene FPS pill cloned in, all controller/view serialized references re-checked post-edit, Unity console clean. The **FPS pill needs no dark variant** after all — it is a 74%-alpha white pill, not transparent text, compositing to ~`#C4D0CE` on the felt with `#5A422A` text at ~6.4:1, so D37's deferred dark-chrome item can be closed. Home button reads on the felt (hue-adjacent but high value contrast). Landscape 1920×1080 checked: nothing breaks; everything renders ~56% relative size, which is D33's known match-height trade-off across all scenes, not new here.
**Not done / open:** the EditMode tests could not be run — the Test Runner API needs its window and is rejected over MCP as a user interaction, and `System.Reflection` is blocked inside `Unity_RunCommand`. All five assertions were instead evaluated directly against the real `CardStackLayout` (5/5 pass: `o1=2 o2=4`; `half(72)=144`, `full(143)=286`, delta 2 vs tol 7.2; `atCap=beyond=340`) and `AceOfShadows.Tests.dll` confirmed compiled in `Library/ScriptAssemblies` — **but a human still needs to hit Run All.** Also open: a Play Mode feel pass, because `PerCardOffset` 3 → 2 means each card now lands 2px above the last, so the "something happened" beat rests almost entirely on the counter pop and ring reset — a question for `unity-animation-expert`, not a layout one. Nothing committed. The README stack-cap paragraph (BRIEF §5) is drafted but not yet written into `README.md`.
**Home button (added after the round, 2026-08-27):** the agent had judged the old button acceptable on contrast grounds and left it, which missed Mini-Arcade finding 09 — it was still the glossy bevelled `buttons_41` atlas sprite sitting next to the new flat chrome, i.e. a second visual language on every feature screen. `HomeButton.prefab` (`Assets/App/SceneFlow/Prefabs/`) is the shared widget instanced by **all three** feature scenes, so the fix went into the prefab, not the scene: root `Image` now uses `ui_rounded_base` (border 64 → Sliced) tinted panel cream `#FFF5D9`, with a new `Glyph` child carrying a procedurally generated `ui_home_glyph.png` in ink `#2A2438`, `raycastTarget: false`, 72×72, nudged −3px vertically for optical centring. Standardised at 150×150 per finding 09 — Ace of Shadows had been overriding its instance down to 110×110 (and renaming it `BackButton`, a name left alone since nothing depends on it); that size override was cleared via `PrefabUtility.RecordPrefabInstancePropertyModifications`. Separating glyph from base is what unlocks the tinting D34 had to skip when the icon was baked into the art. **Deviation:** finding 09 said cream base + *white* glyph; white on cream is ~1.2:1, so the glyph ships in ink instead — the tintable-layer point of the change is preserved, the unreadable combination is not. **Verified** by rendering `AceOfShadowsScene` at a true 1080×1920 (the overlay canvas was temporarily switched to ScreenSpaceCamera against a RenderTexture and the scene reopened without saving, so nothing persisted). **Not verified:** `MagicWordsScene` and `PhoenixFlameScene` inherit this prefab change automatically and have *not* been looked at — cream-on-peach in Magic Words is the one to check when that scene gets its round, and Phoenix Flame's future night-plum ground will want the tint re-judged.

**Main menu follow-up, not actioned:** with the new art the King no longer reads. This deck puts the large rank in the **lower-right** of the face, which is exactly where the overlapping Ace covers it, so the "K" is amputated and the back card reads as "a red card" rather than a King; the top-left index is a small heart plus a thin slash. Two one-value fixes offered — move the K ~30px further left so its rank clears the Ace, or swap the sibling order so the K sits in front, since "A" survives partial occlusion far better than "K". Left for the developer to pick.

### D38 — Round 2 of the UI/UX pass scoped to Ace of Shadows; felt table and stack-cap change approved (2026-08-26)
**Choice:** Asked `unity-ui-ux-expert` (planning-only, no edits) which screen should be Round 2 of the item-4 UI/UX pass and what belongs in it. It recommended **Ace of Shadows**, and the developer confirmed. Reasoning, in its order: (a) Round 1 put the "START HERE" chip on that button, so it is now the second thing a grader sees — and its end state currently renders `All animations finished!` in near-invisible white-on-cream above an unlabelled Restart button (D36/D37 findings 02 and 03), i.e. legibility/completeness defects rather than taste calls, on the longest-dwell screen in the submission at 144 seconds; (b) it is the only one of the three that does not collide with an open engineering workstream — Phoenix Flame's hero element is the fire and the fire is a pending particle pass, and Magic Words' speaker-portrait idea depends on the avatar-fallback path reading as deliberate while that subsystem still has D36 #3's unhandled malformed-JSON crash and unverified WebGL CORS ahead of it; (c) lowest risk — all Canvas UI, no shader, no post-processing, no network, no particle system, nothing in D35's WebGL failure family.

**Scope agreed — safe polish:** completion panel text to ink `#2A2438`, copy to "Deck cleared! / Every card made it across", `144 / 144` in reward gold **as text** (not as a badge sprite), and a real "Play again" label on the Restart button; stack counters become cream pills with `SOURCE` / `TARGET` caps labels; the countdown indicator's Unity built-in `Knob` sprite is replaced by a ring around the target slot (still a single Image with `fillAmount`, no code change).
**Scope agreed — bigger swings, both approved by the developer:** the **felt table** (deep green `#2A5F55` → `#153C36` with dashed card slots) replacing the flat gold `Bg`; and **`MaxVisibleDepth` 12 → ~26 with `PerCardOffset` 3 → 4** (`CardStackLayout.cs:7,11`), the only change that makes the demo visibly show 144 cards instead of two piles that look identical for two and a half minutes.

**Developer's answers to the agent's four blocking questions:** (1) the playing-card art replacement is happening **in parallel**, so felt green and slot outlines get picked without waiting on the final card backs; (2) felt table — **yes**; (3) `MaxVisibleDepth` is **free to change** — confirmed independently against `README.md`, which contains no stack-cap paragraph at all, only a one-line feature description, so nothing is drafted around 12 (but BRIEF §5 still wants the chosen cap explained, and that README section remains unwritten); (4) Round 2 = **Ace of Shadows**.

**Dropped from the agent's own Mini Arcade Second Pass proposal, on its recommendation:** the gold 144-of-144 **badge as artwork** (new sprite, one screen, one moment — the number ships as gold text instead); the ~310×390 **card resize as a standalone item** (folded into the depth change and decided by eye, since it touches prefabs being replaced anyway); and the **Phoenix Flame bloom flag entirely** — turning `m_RenderPostProcessing` back on is a render-pipeline change in the same shader family as D35's WebGL failure, needs a real device check, and its payoff is not measurable until the fire is finished, so it belongs to the particle pass rather than a UI round.
**Reordered:** Phoenix Flame's UI round now sits **below** the particle pass, and Magic Words' portraits **below** the malformed-JSON/timeout fix — both would otherwise be built on top of something about to move.
**Dissent worth recording:** the agent argued that the stock `index.html` (D36 #1 — "Unity Web Player" tab title, "DefaultCompany" in the page source, hardcoded 960×600) is the literal first thing a grader sees, before Unity boots, is cheaper than this entire round, and is worth more; if only one more thing ships before submission it thinks it should be that, not Round 2. The developer chose Round 2 anyway — recorded so the trade-off is a decision rather than an oversight.
**Not done:** none of the round is implemented — the developer's green light was given but work is blocked on the Unity MCP bridge, see D39 or the "Immediate next steps" section. Staged completion beats (cascade → badge → button) were explicitly handed off to `unity-animation-expert` as timing work, not part of this round.

### D37 — New `unity-ui-ux-expert` subagent; audited all 4 scenes with real renders; built Round 1 (MainMenuScene + AppScene) of the item-4 UI/UX polish pass (2026-08-26)
**Choice:** Created a new global subagent, `~/.claude/agents/unity-ui-ux-expert.md`, per direct request for a casual/hypercasual game UI/UX expert — briefed on concrete genre standards (color-role systems, typography pairing, thumb-reachable layout, "juice" as feedback density, FTUE discipline) and explicitly instructed to be **creative but non-disruptive** (push past the reflexive "rounded rect + shadow" default, but prefer additive ideas over restructuring an already-working screen, and label how big a swing each suggestion is) and to **prototype non-trivial ideas as a visual mockup** before implementing, handing off exact tween timing/easing to `unity-animation-expert` rather than duplicating it. See the sibling entries in the personal memory system (`reference_unity_ui_ux_expert_agent`) for the full brief.

First run, same session: audited all 4 scenes not by reading scene YAML but by actually cloning each scene's roots into a preview scene, populating real runtime content (real card prefabs, a real fetched Magic Words line, the flame prefab simulated), and rendering at the true 1080×1920 reference — catching several things (a near-invisible completion-panel text contrast, a blank Restart button, the stock skybox on 2 scenes) that don't show up in serialized data alone. Produced a new artifact, **not** an update to the existing "Casual Arcade Direction" canvas (that one is a cited-by-URL direction reference per D33 and stays intact) — "Mini Arcade Second Pass" (`https://claude.ai/code/artifact/e6f0d151-0673-4369-8ee3-ec1b4862e34e`), with ranked findings, before/after captures, and one distinct creative direction per screen (Main Menu: buttons that let a fragment of their own game "peek" past the rounded edge; Ace of Shadows: a felt playing-table background; Magic Words: large dimmed/highlighted speaker portraits; Phoenix Flame: give the fire a physical source — a brazier + background glow — instead of floating on a skybox).

Given the go-ahead to implement, did **Round 1 only** (MainMenuScene + AppScene, explicitly staged/scoped by the developer — the other 3 scenes are separate future rounds):
- **MainMenuScene**: stock default skybox replaced with Solid Color `#FFC27C` clear + a new full-bleed `Bg` Image (baked warm radial gradient); the `Directional Light` deleted (same reasoning as every other all-UI scene, D18/D28); the `Phoenix Flame Button` placeholder label fixed to `Phoenix Flame`; a "MINI ARCADE" title lockup added (Baloo 2 Bold @108px, rotated −2.4°, hard offset shadow layer); each button given a decorative "peeking preview" breaking past its own rounded-rect edge (fanned King/Ace playing cards on Ace of Shadows, a speech bubble with two real Twemoji sprites on Magic Words, a flame-lick sprite on Phoenix Flame), all `raycastTarget: false` so taps still land on the button; the cream button-container card removed, buttons now stretch to full container width and sit in the bottom thumb zone, Ace of Shadows called out with a "START HERE" chip.
- **AppScene**: the FPS counter — flagged as visually louder than the actual game content on 3 of 4 screens (328×125 at 78px type) — resized to a 236×66 pill at 34px on the shared `ui_rounded_base` chrome with a small status dot.
- New sprites (`Assets/Feature/MainMenu/Sprites/{bg_menu_warm,deco_flame_flat,deco_bubble_tail}.png`, `Assets/App/Sprites/UI/Chrome/ui_dot.png`) generated procedurally in C#, same route as D34 (the AI asset-generation tool still has no model configured on this install).
- Verified by reopening both scenes from disk before capturing (confirms it's what's actually saved, not in-memory state) and by checking `MenuButtonSceneLoader`'s 3 scene targets, `MainMenuInitializer`, and `FpsCountUIController.fpsText` are all still correctly wired post-edit. Also confirmed the D34 prefab-instance-edits-not-persisting gotcha is now avoidable by using `PrefabUtility.RecordPrefabInstancePropertyModifications` instead of a bare field assignment — worth remembering as the fix, not just the symptom, next time that gotcha comes up.

**Deliberately scoped out of this round, on direct instruction:** the flame-lick preview sprite has **zero reference to the real `PhoenixFlame` prefab/material/Animator Controller** — it's a plain static `Image`, specifically so it doesn't entangle with a future dedicated particle-effects pass on the actual fire look (the developer flagged that the mockup's flame redesign doesn't match the real particle system and a `unity-particle-expert`-style agent may be needed for that later, not yet created). The card-fan preview reuses the current Ace of Shadows card prefabs as-is with no fine-detail polish, since that art is being replaced by the developer separately. Also deferred: the title lockup's white-outline/hard-underlay upgrade (would need a per-component `TMP_Text.fontMaterial` instance — judged a real decision, not a tweak), any actual motion/animation (buttons are still flat `ColorTint`, no idle or entrance animation — `unity-animation-expert`'s job, priority item 3), and a dark-chrome variant of the FPS pill (fine on every current background, will need inversion once Phoenix Flame's own round moves it to a dark background).
**Why:** Requested directly — item 4 on the developer's own priority list ("the project needs visual polish," a scored aesthetics criterion per `BRIEF.md` §3), explicitly asked to be tackled scene-by-scene rather than all at once, starting with Main Menu + AppScene as the entry point.
**Not done:** Ace of Shadows, Magic Words, and Phoenix Flame's own UI/UX rounds (direction already proposed in the mockup, not yet built); committing/pushing (see D38 if a separate entry covers that, or check git log); the Phoenix Flame particle-effects pass.

### D36 — Second full `unity-interviewer` audit: real brief-violating gaps found, not yet acted on (2026-08-26)
**Choice:** Re-ran the `unity-interviewer` subagent for a fresh, full-project pass (first run was 2026-08-23, see D24-D26) — treated as a new engagement since Magic Words and Phoenix Flame didn't exist at the last audit. It read `BRIEF.md`, the full `current-context.md`/`decisions.md`, the actual deployed WebGL artifact (not just the project source), and verified current market claims (build-size budgets, Unity LTS timelines) via web search rather than asserting from training data. Full findings kept in the subagent's own report, not duplicated verbatim here — summary of what it rated **brief-violating** (Tier 1):
1. The hosted build's `index.html` is the **unmodified stock Unity WebGL template** — hardcoded 960×600 desktop canvas, "Unity Web Player" tab title, "DefaultCompany" in the page source. This is the same gap the *first* audit flagged as its own Tier 0 finding #5 (recorded in D25 as "not done as part of this decision") — still unfixed three days and two full UI sessions later, even though D33/D34's work was substantially about symptoms of this same root cause (desktop aspect ratio).
2. `MainMenuScene`'s Phoenix Flame button label reads `Phoenix Flame Button` (GameObject name pasted into the label field) — independently caught by `unity-ui-ux-expert` too, see D37.
3. `MagicWordsResponseParser.Parse` calls `JsonUtility.FromJson` directly with no try/catch; it throws (doesn't return null) on malformed JSON, so an HTTP-200-but-not-real-JSON response (a captive portal, a proxy error page) kills the fetch coroutine silently and leaves the screen blank forever — the brief explicitly names a malformed-payload path and request timeouts (also absent) as requirements.
4. Real deployed transfer size is **~20MB**, not the "~13MB" `current-context.md` claimed (now corrected, see D37's neighboring edits/this doc's own maintenance) — over the ~10-15MB instant-play budget the brief calls its highest-leverage item. Root cause identified: `LargeFlame02.tif` (6.9MB, no WebGL texture-size override at all) plus ~7MB of font atlases including Rubik, which per D29 is loaded but wired onto zero TMP components.
5. `README.md` still stated Magic Words and Phoenix Flame were "Not built yet" — stale since D28/D31 built them. Fixed as part of this doc pass (see below).
6. `ai-context/BRIEF.md` is committed and `README.md` points reviewers at `ai-context/` directly; `BRIEF.md` contains the developer's stated salary target/ceiling — flagged as needing an explicit decision before submission, not another deferral (D27 already left this open once).

Also flagged, real-but-debatable (Tier 2): post-processing disabled on every camera means Phoenix Flame's HDR emission colors (up to 2.8 intensity, see D31/D32) clamp toward white with no Bloom to render the intended glow — plausibly the concrete mechanism behind "the project needs visual polish"; WebGL ships the `PC` URP quality tier (depth texture + SSAO both on) with no per-platform override, direct relative of D35's fix having landed one layer too shallow; `CanvasScaler`'s portrait-tuned `matchWidthOrHeight: 1` is the worst-case match mode for the landscape desktop letterbox from finding #1; Magic Words' avatar loader has no cache/timeout and a stale-callback race if the fast-forward button is spammed past a still-loading avatar; `PhoenixFlameConfig.ColorOptions`' entire D32 apparatus is read only for `.Count` at runtime (colors/duration are actually baked into the Animator) — real, but D32's own stated reasoning (preventing two sources of the same data silently desyncing) still holds even if the mechanism it protects turned out to be write-only.
**Why:** Requested directly, to get an honest fresh read on where the project stands after Magic Words/Phoenix Flame/D33-D35, ahead of the final UI/UX polish push.
**Not done:** none of Tier 1 or Tier 2 has been fixed yet, except the menu-button label (fixed as part of D37, which happened to independently catch the same thing). The interview-question half of the audit was run and answers weren't yet relayed back. This is the immediate next-priority list, effectively superseding parts of the "developer's own priority list" ordering — see current-context.md.

### D35 — Phoenix Flame invisible on WebGL: Soft Particles disabled on LargeFlame02.mat (2026-08-26)
**Choice:** Disabled Soft Particles on `Assets/Feature/PhoenixFlame/Materials/LargeFlame02.mat`
(`_SoftParticlesEnabled: 1 → 0`, `_SOFTPARTICLES_ON` moved from `m_ValidKeywords` to
`m_InvalidKeywords`). Confirmed fixed by rebuilding and redeploying the WebGL build — the flame
is now visible.
**Why:** The developer reported the flame wasn't visible at all in the hosted WebGL build (Editor
Play Mode was fine). The browser console showed
`Shader Hidden/CoreSRP/CoreCopy shader is not supported on this GPU (none of subshaders/fallbacks
are suitable)` — a URP-internal camera color/depth blit shader, not anything specific to this
project's own shaders. `LargeFlame02.mat` has Soft Particles enabled, which depends on sampling
the camera's depth texture to fade the particle near intersecting geometry — if the copy pass that
produces that depth texture fails on a given GPU/browser's WebGL2 context, the fade calculation
gets invalid input and the shader's failure mode is "render fully transparent," which matches
exactly what was reported (everything else in the scene, which doesn't depend on that pass,
rendered fine). Disabling Soft Particles removes the dependency entirely — a real fix for a real
compatibility gap, not a workaround for a bug in this project's own code, and a reasonable trade
since nothing in the Phoenix Flame scene actually needs the flame to soft-fade against intersecting
geometry in the first place.
**Not done:** the root GPU/WebGL2 limitation itself isn't something fixable from the project side
(it's the tester's GPU/browser rejecting a Unity/URP-internal shader) — worth keeping in mind if
other depth-texture-dependent features (bloom, distortion, other post-processing) are added later,
since the same class of failure could resurface elsewhere. The `Hidden/Universal/HDRDebugView`
error in the same console log is unrelated (URP's Rendering Debugger HDR view shader) and can be
ignored.

### D34 — Shared UI chrome (rounded rect + shadow) applied project-wide; Magic Words dialogue boxes rebuilt to scale with canvas width (2026-08-26)
**Choice:** Two changes shipped together since the second was discovered while reviewing the
first:
- **New shared UI chrome**: `Assets/App/Sprites/UI/Chrome/ui_rounded_base.png` (a tintable,
  9-sliced rounded-rect shape) and `ui_soft_shadow.png`, both generated procedurally in C# via
  Unity MCP scripting (a per-pixel rounded-rect signed-distance function, a separable box blur for
  the shadow) rather than through the AI asset-generation tool — that tool has no model configured
  for this Unity install (`Unity_AssetGeneration_GetModels` returns empty, and `GenerateAsset`
  errors "A model must be selected"). Applied via `Image.Type.Sliced` (so it doesn't distort at
  different sizes) across every button and popup panel project-wide — Main Menu's 3 buttons + the
  panel behind them, Ace of Shadows' Restart button + finished-message panel, Magic Words'
  finished-message panel and dialogue box background, Phoenix Flame's 3 color buttons — tinted
  per-feature to match the existing blue/green/orange accents (the exact same RGB values as
  `PhoenixFlameConfig`'s own Orange/Green/Blue options, a deliberate callback for consistency).
  Shadows use Unity's built-in `UnityEngine.UI.Shadow` component (draws from the graphic's own
  mesh, offset and tinted) rather than a second sprite/GameObject — simpler, and it automatically
  matches whatever shape the graphic actually is instead of needing separate positioning/sizing
  logic. **Home/back button icon art was deliberately left untouched** (only a shadow added) since
  its sprite bakes the house icon directly into the art with no separate icon layer — swapping it
  for the plain chrome would have deleted the icon with no replacement available.
  **A real gotcha hit during this pass**: setting `Image.sprite`/`.type`/`.color` directly via
  script on the 3 Main Menu button `PrefabInstance`s didn't persist through `EditorSceneManager.
  SaveScene` — structural changes (adding a component) saved fine, but plain field edits silently
  didn't stick, and reopening the scene reverted them to the prefab's own default. Diagnosed by
  reading the actual scene YAML back (the modification list was simply missing those entries) and
  fixed by writing the `PrefabInstance` modification entries directly into the scene file instead
  of relying on the live-script-edit + save path. Worth remembering: prefab-instance property
  overrides set via a bare C# assignment aren't guaranteed to register the way structural changes
  are — verify by re-reading the saved file, not just by checking the in-memory value right after
  setting it.
- **Magic Words dialogue boxes rebuilt to scale with the canvas's actual width**: this was
  supposed to already be fixed by D33 (which shrunk the box to a smaller *fixed* pixel width,
  620px), but the developer found it now read as too small on a wide/desktop-shaped canvas — a
  fixed pixel width can't simultaneously avoid overflowing a narrow portrait canvas and avoid
  looking lost on a wide one, since canvas width varies by orders of magnitude between the two.
  Real fix: `LeftDialogueBox.prefab`'s root `RectTransform` now uses a genuine stretch anchor
  (`anchorMin.x/anchorMax.x: 0.02/0.57`, `sizeDelta.x: 0`) instead of a point anchor with a fixed
  `sizeDelta.x` — the box is now always ~55% of whatever the canvas width actually is, mirrored for
  `RightDialogueBox` (`0.43/0.98`) via its existing scale-flip convention. `DialogueBoxView`'s
  `hiddenAnchoredX`/`shownAnchoredX` fields (fixed pixel constants) are gone — `shownAnchoredX` was
  always `0` regardless of box width so it's now just hardcoded in `SlideIn`, and the off-screen
  hidden position is computed at runtime from the box's actual current `RectTransform.rect.width`
  (`hideDirectionSign * (width + offscreenMargin)`, `hideDirectionSign` being `-1`/`+1` per side,
  the one thing that still needs a per-instance value since it can't be derived). The
  `RightDialogueBox` scene-level `PrefabInstance` override for the old `hiddenAnchoredX` field was
  replaced with one for `hideDirectionSign` instead. Verified directly in Unity: at the Editor's
  then-current ~1920×1080-ish Game view resolution, the box now computes to 1877px wide (both
  sides symmetric) instead of the old fixed 620px.
**Why:** Requested directly, in detail, continuing the same session's UI-polish work. The dialogue
box fix specifically corrects a real architectural gap D33 didn't go far enough to fix — D33 picked
a smaller *constant*, which is safe against a narrow-but-not-too-narrow range but was never going
to work simultaneously against both the narrowest portrait phone and a wide desktop browser window
(WebGL's desktop rendering isn't orientation-locked the way native mobile is, so a wide/landscape-
shaped browser window is a real, expected case this app has to handle even though the app is
"portrait" by policy).
**Not done:** the dialogue box is still glued flush to the literal screen edge when shown (no
margin baked in beyond the small `0.02` anchor inset) and isn't `Screen.safeArea`-aware — same
caveat D33 already flagged, still open.

### D33 — Reversed D25: the app is portrait-first now, not landscape-locked (2026-08-26)
**Choice:** `ProjectSettings.asset` orientation flags flipped —
`allowedAutorotateToPortrait`/`PortraitUpsideDown` → `1`,
`allowedAutorotateToLandscapeLeft`/`Right` → `0` (still `defaultScreenOrientation:
AutoRotation`, so it autorotates between the two portrait directions the same way
D25's landscape lock autorotated between its two directions — not a single fixed
rotation). All 5 scenes' `CanvasScaler` (`AppScene`, `MainMenuScene`,
`AceOfShadowsScene`, `MagicWordsScene`, `PhoenixFlameScene`) flipped reference
resolution `1920x1080` → `1080x1920`, keeping Match Height
(`m_MatchWidthOrHeight: 1`) — same scaling philosophy as D21/D25, axis swapped.
Canvas height is now always exactly `1920` units regardless of device (vs. always
`1080` before); canvas width now varies with device aspect instead of height.

Investigating the flip surfaced that most of the real mobile-adaptability risk was
never the orientation itself — it was **fixed pixel offsets from a single anchor
point** instead of proportional/safe-area-relative sizing, in three places:

- **Magic Words dialogue boxes** (`Assets/Feature/MagicWords/Prefab/LeftDialogueBox.prefab`,
  and its `RightDialogueBox` mirrored `PrefabInstance` overrides in
  `MagicWordsScene.unity`) — anchored to the literal screen corner
  (`anchorMin/Max: {0,0}`/mirrored `{1,0}`), a fixed `sizeDelta.x: 750`, and
  `DialogueBoxView`'s `hiddenAnchoredX`/`shownAnchoredX` hardcoded in raw pixels
  (`-800`/`0`). This is the real, previously-flagged mobile bug (developer's own
  priority list item #2) — a 750px-wide box glued flush to the screen edge left
  only ~114px of margin against a worst-case portrait canvas width (a tall ~20:9
  phone works out to `1920 * (1080/2340) ≈ 886` units wide with the new reference
  height). Fixed by shrinking to `sizeDelta.x: 620` and rescaling
  `hiddenAnchoredX`/its mirrored counterpart proportionally (`-670`/`670`, keeping
  the same ~50px off-screen clearance the old `-800` had relative to `750`) —
  `shownAnchoredX` stays `0` since a zero offset is scale-invariant and needs no
  mirroring. **Kept unchanged, deliberately:** the `DOAnchorPosX` slide mechanic
  and the per-speaker left/right box convention — only the box's own
  sizing/anchoring changed, preserving the brief's literal "DoMove on x axis
  toward center" requirement from D28.
- **Main Menu button container** (`MainMenuScene.unity`, the
  `VerticalLayoutGroup` container): was a fixed `sizeDelta: {994.5, 742.6}` —
  already a portrait-friendly vertical stack, but 994.5px wide is *wider* than
  the worst-case portrait canvas width, so it would have clipped on real tall
  phones. Fixed by switching its X anchors from a center point to a stretch
  (`anchorMin.x/anchorMax.x: 0.1/0.9`, `sizeDelta.x: 0`) — now always 80% of
  whatever the canvas width actually is. Height stays a fixed point-anchor
  (`742.6`), unaffected.
- **Ace of Shadows stacks** (`SourceStack`/`TargetStack`): center-anchored point
  with fixed `±175px` offsets. Checked against real card size
  (`208x260`, e.g. `Deck07_Club_10.prefab`): the two piles need
  `2 * (175 + 104) = 558px`, comfortably under the ~886px worst case — **this one
  was already safe**, no structural change. Only the Y offset was re-tuned (from
  `-111.9375` to `0`, i.e. vertical center) since the canvas is now 1920 units
  tall instead of 1080 — the old offset was tuned for a much shorter canvas and
  would've read as barely-off-center against the new height. Exact vertical
  placement is a starting point, not a final tune — left for the developer's own
  Play Mode eye-tuning, same as `CardStackLayout.PerCardOffset`'s existing
  "retune by eye" precedent.
- **Phoenix Flame's 3-button row** was checked too (fixed `150px` buttons, `200px`
  spacing, actual edge-to-edge span `550px` — not `~850px` as first estimated,
  since the spacing values are between-button-center offsets, not gaps) — already
  comfortably safe against the ~886px worst case with room to spare. **Left
  as fixed positions, not converted to a layout group** — there's no actual bug
  here to fix, and adding a `HorizontalLayoutGroup` would be a change with no real
  payoff.

**Why:** Requested directly — portrait-first is the more standard convention for
this genre (casual mobile), and the developer wants the project's "Casual Arcade
Direction" mockup (see below) redone in portrait too. Planned via `EnterPlanMode`/
`ExitPlanMode` given the scope (5 scenes, `ProjectSettings`, three features'
layouts) and presented with the concrete pixel-math findings above before any
edits were made, per the plan-mode workflow.

**Also happening in parallel:** the "Casual Arcade Direction" mockup
(`https://claude.ai/code/artifact/06510f2d-e5eb-4cc1-b121-6deb4c596c18`, see D32's
neighboring context) is being updated in place to portrait artboards (9:16 and a
taller ~9:19.5) reflecting these same layout decisions — same visual direction,
recomposed for the vertical axis instead of horizontal.

**Not done in this pass:** real `Screen.safeArea` integration (notch/home-indicator-
aware insets) — the fixes above make things proportionally safe against a range of
aspect ratios, but don't read the OS-reported safe area at runtime; worth a look if
a real device shows content under a notch/gesture bar. Play Mode / on-device
verification across multiple simulated portrait aspect ratios (Game view's custom
resolution presets) — compiles clean and every changed value was read back through
Unity's own API (`RectTransform`/`CanvasScaler` properties, not just re-reading the
YAML), but nothing here has actually been seen rendering yet. Committing this to
git — sitting in the working tree pending the developer's go-ahead, consistent with
the "only commit when asked" rule.

### D32 — Phoenix Flame's color list is now derived from the Animator Controller, not hand-typed (2026-08-26)
**Choice:** `PhoenixFlameConfig` gained an `AnimatorController` (`RuntimeAnimatorController`) field — the actual
runtime source now, applied via `animator.runtimeAnimatorController = config.AnimatorController` in
`FlameParticle.Initialize` — replacing the prefab's own separately hand-wired `Animator.controller` (which is now
dead/overridden). `colorOptions` (the per-button name + base/emission color list) stays serialized, so runtime code
is unchanged, but it's no longer hand-typed: a new `PhoenixFlameConfigEditor` custom inspector
(`Assets/Feature/PhoenixFlame/Editor/`) reads it back from the assigned controller and writes it in, only when it
actually differs. The extraction (`PhoenixFlameAnimatorColorReader`) walks each "Any State" transition's
`ColorIndex == N` condition to its destination state, then reads that state's `AnimationClip`'s baked
`material._BaseColor`/`material._EmissionColor` curves at t=0 via `AnimationUtility` — keyed off the transition
condition specifically, not state array order, since the condition is the only place a state's real index is
actually defined. Verified via Unity MCP: project compiles clean, and the extraction was run directly against the
real asset, correctly resolving `0→Orange`, `1→Green`, `2→Blue` with colors matching what had been hand-typed.
While testing this in the Inspector, the developer duplicated the Blue state into a 4th ("Pink", same colors as
Blue, not yet retinted) to confirm the custom editor picks up a new state automatically — it did, and that state/
color entry is included in this commit as a working proof, not finished content.
**Also swept**: comments across the project's own code (Ace of Shadows, Magic Words, Phoenix Flame — vendored
`Assets/Plugins` left untouched) — removed everything that was restating WHAT the code does or naming a specific
caller, keeping only comments documenting a real hidden constraint, invariant, or gotcha (e.g. `DialogueLineDto`'s
field-name-must-match-JSON-key constraint, `AceOfShadowsController`'s fillAmount-write throttle rationale,
`DeployWebGL`'s stream-read-before-`WaitForExit` deadlock note).
**Why:** Requested directly — the developer flagged that a hand-typed `colorOptions` list and the Animator
Controller's actual states were two independently-maintained sources of the same data with nothing enforcing they
stay in sync (wrong count/order between them would silently desync buttons from states). Deriving the list from the
controller instead removes that duplication at its root, at the cost of the custom editor only being able to read
it at edit time (`AnimationUtility` is `UnityEditor`-only) — acceptable since `colorOptions` still gets written into
a normal serialized field the runtime reads, so no editor-only API is ever touched outside the Inspector.
**Not done:** the flame look/scale/placement pass and the button layout eyeball-check the developer's own priority
list (see current-context.md) already calls out as the real next Phoenix Flame work — this change only touches the
config/color-authoring path.

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
