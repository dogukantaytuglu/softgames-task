# SOFTGAMES Unity Assignment — BUILD BRIEF
**Self-contained handoff.** Written 21.08.2026, to be executed by a separate implementation
session. The original PDF is alongside this file at
`ai-context/Softgames_-_Unity_Developer_Assignment.pdf`.

---

## 1. Context — what this is
This is a take-home assignment for a Senior Unity Developer role at **SOFTGAMES** (Berlin,
fully remote, casual/social games for instant platforms).

**No deadline was given** ("all the time you need"). ⚠️ That *raises* the bar rather than
lowering it: there is no "I only had a weekend" defence, so the submission is judged as
considered best work. No self-imposed deadline either — **it ships when it's done**, not
against a calendar date.

### Relevant technical background, for architecture decisions
- **Deep:** C# (5+ yrs hand-written), Unity, **UniRx** (reactive, used daily on a live multiplayer
  title), Unity **Addressables**, **Entitas ECS**, unit testing in Unity/C#, UI/animation/particle
  work (game feel is a genuine strength from hyper-casual and casual titles).
- **Has:** Cloudflare Workers/Pages deployment experience (side project), Docker, CI/CD pipelines.
- **Does NOT have, and must not fake:** HTML5/web game frameworks (PIXI, Phaser, Three.js), and
  **WebGL download-payload/byte-budget optimisation** — see §6, which turns this into an advantage.
- 🔴 **Every architectural decision in this submission must be defensible out loud.** The
  follow-up conversation will probe it. Do not introduce patterns or dependencies that can't be
  explained.

---

## 2. The assignment, verbatim
Create a **new project from scratch** and complete the 3 tasks below.

1. **"Ace of Shadows"** — Create 144 sprites stacked like cards in a deck, with each top card
   partially covering the one below. Every 1 second the top card should move smoothly to another
   stack. Display a counter above each stack and show a message when all animations are finished.
2. **"Magic Words"** — Create a system that combines text and Unicode emojis to render character
   dialogue using data from the endpoint below. Load the data dynamically at runtime and handle cases
   where avatar URLs may not load or data is missing.
   `https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords`
3. **"Phoenix Flame"** — Create a particle effect demo that shows a great fire effect. Add a UI button
   that controls the fire colour using an **animator controller**. The fire should transition smoothly
   from orange to green to blue and loop back to orange.

**Technical requirements:**
- Write your code in **C#** and use **Unity 6**.
- Each task should be accessed via an **in-game menu**.
- Render **responsively for both mobile and desktop** devices.
- Display the **fps in the top left corner**.
- **Build for WebGL** and provide a **link to the hosted version**.

> "We evaluate your project structure, architecture, readability, testability and documentation of your
> code. But as game developers we also deeply care about the style, visual aesthetics and UX of your
> solution :)"

---

## 3. 🎯 The grading criteria, read literally
**Two separate scorecards. Most candidates serve only the first.** Working code is the floor — assume
they have graded this same assignment many times, so completion is baseline.

| Scorecard | What it means here |
|---|---|
| **Architecture** — structure, readability, **testability**, **documentation** | Named explicitly. Testability and documentation are the two most commonly skipped, so they are the cheapest differentiators. |
| **Craft** — style, visual aesthetics, UX | Also named explicitly. Three tasks that merely function will lose to three that look considered. **This is a real strength here — do not treat it as decoration.** |

### The one decision that scores twice
🔑 **Keep the logic OUT of MonoBehaviours.** Deck model, dialogue parser/model, colour state machine as
plain testable C# classes; MonoBehaviours only bind them to the scene and drive presentation.
That is simultaneously the architecture answer and the testability answer.

**Tests are not optional.** Unit testing in Unity/C# is a claimed skill, and they named
testability. Use the Unity Test Framework, EditMode tests for the plain C# logic.
**Write them as you go** — retrofitted tests get skipped.

---

## 4. Plan — kill the two unknowns first
Two things can wreck a final evening: the WebGL/hosting pipeline, and inline emoji rendering.
**Do both on a skeleton before building any real feature.**

### Phase 0 — de-risk
1. Unity 6 project, git repo, folder structure.
2. In-game menu + scene/state routing to three empty task screens, and a way back.
3. FPS counter, top-left.
4. Responsive canvas setup — CanvasScaler, anchoring, safe areas, **touch *and* mouse input**.
5. **WebGL build → deployed live to Cloudflare Pages**, with correct Brotli/gzip content-encoding
   headers. Verify the empty app loads on a real phone and a desktop browser.
6. **Timeboxed emoji spike** — prove ONE Unicode emoji renders inline in TextMeshPro via a sprite
   asset. Stop as soon as it's proven; do not build the dialogue system yet.

### Phase 1 — build, in order of confidence
1. **Phoenix Flame** — strongest area, fastest win, and it *is* the aesthetics scorecard.
2. **Ace of Shadows** — the architecture/perf task.
3. **Magic Words** — most buffer, fiddliest.

### Phase 2 — the differentiators (do NOT leave these to the last night)
1. README documenting **decisions and trade-offs**, not a feature list.
2. **Build-size measurement and reduction, documented** — see §6.
3. Polish pass on all three; verify on a real device.

---

## 5. Task-by-task technical guidance
### Ace of Shadows
- ⚠️ **144 naively instantiated GameObjects is the trap.** Pool them; watch draw calls (sprites should
  batch). The FPS counter they demanded is effectively part of grading *this* task.
- **144 cards at a visible offset makes an absurdly tall stack.** This needs a deliberate layout
  decision — cap the visible offset, compress the stack, or only offset the top N.
  **Whatever is chosen, explain it in the README.** An unexplained arbitrary choice reads as an
  accident; an explained one reads as judgement.
- Sorting order must be re-established as cards move between stacks.
- "Smoothly" + "message when all animations are finished" ⇒ a real sequencing/completion mechanism.
  **Not a coroutine per card with a guessed delay.** UniRx is a legitimate option here and is a
  genuine strength above — but only if it stays readable to a grader who may not know UniRx.
  A plain async/await or a tween library with a completion signal is equally defensible.

### Magic Words
- 🔴 **Inline Unicode emoji is the biggest time risk in the brief.** Unity does not render colour emoji
  fonts well; the practical route is a **TextMeshPro sprite asset** mapping emoji codepoints to a
  sprite atlas. This is why it gets a Phase 0 spike.
- ✅ **First action: fetch the endpoint and read the actual schema.** Design nothing before seeing it.
- **The failure handling IS the test** — they spelled out that avatar URLs may fail and data may be
  missing. Implement placeholder avatars, request timeouts, and a malformed/missing-payload path.
  **Make the fallbacks visibly demonstrable** (e.g. a way to trigger the failure state) so a grader
  can see they exist rather than taking them on faith.
- Remote images in WebGL are subject to CORS — verify avatar loading works **in the hosted build**,
  not just in the editor.

### Phoenix Flame
- They asked for "a **great** fire effect" and they grade aesthetics. This is the polish showcase.
- ⚠️ **They specified an animator controller** for the colour transitions. Not a tween, not a script
  lerp. **Use the mechanism they asked for** — this is a follow-the-spec check as much as a visual one.
- Orange → green → blue → loop back to orange, smoothly, driven by a UI button.

---

## 6. Build-size measurement and reduction
WebGL download-payload / byte-budget optimisation is a real gap against instant-games experience
(Addressables experience exists, byte-budget-under-a-strict-cap experience does not) — and download
size is the technical heart of SOFTGAMES' business, since they ship Unity to instant platforms.

**This assignment requires a hosted WebGL build. Treat it as a chance to close that gap directly:**
- Measure the initial build size, then reduce it deliberately: **Brotli compression, managed code
  stripping, disabling unused engine modules, exceptions off, texture compression, audio bitrate/mono,
  stripping unused shader variants.**
- **Document the before/after numbers and the reasoning in the README.**

→ **Almost no submission will mention build size at all.** High-leverage item relative to its cost.

---

## 7. Definition of Done — ship when this is ticked, then stop
- [ ] Three tasks working, and **polished**
- [ ] Logic outside MonoBehaviours; **unit tests** on deck model, dialogue parser, colour state machine
- [ ] In-game menu routing to all three, with a way back
- [ ] **FPS counter, top-left**
- [ ] Responsive: verified on a **real phone** and a desktop browser; touch *and* mouse
- [ ] **Unity 6**
- [ ] WebGL build hosted at a **public link that loads**
- [ ] README: architecture, decisions, trade-offs
- [ ] **Build-size numbers documented** (§6)
- [ ] Every decision is defensible out loud

⚠️ **No gold-plating past this list.** Open-ended becomes infinite otherwise.

---

## 8. Notes
- 📌 **On AI tooling:** agentic development with Claude Code is already public elsewhere (CV, cover
  letter), so building this with AI assistance is consistent with how the work actually happens — no
  need to hide it, and the straightforward answer is the true one if asked. **The binding constraint is
  §1's last bullet: every decision must be defensible.** Anything that can't be explained is a
  liability, not an asset.
- **Contact for questions about the test:** the SOFTGAMES recruiting contact.
