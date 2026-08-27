# softgames-task

Hey — this is my take-home for SOFTGAMES' Senior Unity Developer role. Three small,
self-contained demos, built in Unity 6, the way the brief actually asked for them
to be built.

**Play it:** [dogukantaytuglu.github.io/softgames-task-build](https://dogukantaytuglu.github.io/softgames-task-build/)

## What this is, and what it isn't

This isn't me trying to show off every pattern I know. There's no DI framework, no
generic event bus, no data-driven config system for things that only ever have one
configuration. Where you do see structure — feature folders split into
Logic/Monobehaviour asmdefs, a couple of small state machines, a decisions log kept
as I went — it's there because the thing being built actually called for it, not
because I wanted the repo to look impressive. In a few places the honest answer was
"the simple version is fine," and I said so directly rather than padding it out.

If anything here reads as more (or less) engineered than you'd expect for what it
does, ask me about it in the interview — every real choice in this repo is one I
can explain and defend, not just something that happened.

## What's in here

Three demos, reachable from an in-game menu, with a way back from each:

- **Ace of Shadows** — a 144-card deck drains from one stack into another, one card
  a second, on a felt table with live counters over each stack. When the last card
  lands the board clears itself in a staggered cascade and a completion panel offers
  a replay.
- **Magic Words** — a dialogue viewer that fetches its script from the assignment's
  endpoint at runtime and renders text with inline emoji. Tap to fast-forward the
  typewriter reveal, or let it auto-advance. Avatars load from remote URLs with a
  placeholder shown immediately and swapped in on success.
- **Phoenix Flame** — a particle fire that recolours orange → green → blue through
  an Animator Controller, driven by UI buttons, with the surrounding scene light
  lerping along with it.

Plus an FPS counter pinned top-left throughout, and a layout that holds up from a
portrait phone to a desktop browser window.

## Architecture

### The one decision everything else follows from

**Game logic lives outside MonoBehaviours, and that's enforced at compile time.**

Every feature splits into two assemblies — `<Feature>.Logic` and
`<Feature>.Monobehaviour`. The `Logic` asmdefs set `noEngineReferences: true`, which
means they physically cannot reference `UnityEngine`. It isn't a convention I'm
promising to keep; it's a compile error if I break it.

That buys the thing the brief asked about directly: those types are unit-testable in
plain EditMode tests with no scene, no Play Mode, and no waiting on real time.
`CardDeck` is the clearest example — draining all 144 cards is a loop in a test that
runs instantly, rather than 144 real seconds of waiting, because the domain model has
no idea a second is passing.

### Layout

```
Assets/
  App/          infrastructure every scene depends on — SceneFlow, FpsCounter
  Feature/      independent, deletable demos — MainMenu, AceOfShadows,
                MagicWords, PhoenixFlame
  Tests/EditMode/<Feature>/
```

The `App` / `Feature` split answers one question from the folder tree alone: *what
can I delete without breaking the app?* Anything under `Feature/` can go. Anything
under `App/` can't.

### Scene flow

`AppScene` is build index 0 and never unloads. It holds the FPS counter and a
`SceneService`; every demo loads additively on top of it, exactly one at a time.
That keeps the FPS counter alive across navigation without `DontDestroyOnLoad`
juggling, and guarantees there's never a second EventSystem fighting the first.

The navigation state machine (`SceneFlowState`) is plain C# and refuses to begin a
navigation while one is already in flight — that's a real bug I hit, where
double-tapping two menu buttons desynced the state machine from what was actually
loaded.

### Where the tuning lives

Each demo has a `ScriptableObject` config asset holding its numbers — card count,
move interval, easing curves, reveal speed, colour options. They started life as
scattered `[SerializeField]`s and got consolidated, so tuning happens in one
inspector rather than by hunting across components.

## Decisions and trade-offs

The full reasoning, decision by decision as I made them, is in
[`ai-context/decisions.md`](ai-context/decisions.md). The ones most worth arguing about:

**144 cards in a visible stack is physically absurd, so the stack is capped.** At a
realistic per-card offset a full deck is taller than any phone screen. The pile rise
is clamped, which means the offset stops communicating "how many cards are left" past
a point — that's what the counter pills above each stack are for. An uncapped stack
would have been literally faithful and visually broken.

**Cards are UI, not world-space sprites.** They started as `SpriteRenderer`s under a
perspective camera and moved to `Image`s under a `CanvasScaler`. World space doesn't
scale across aspect ratios, and the rest of the app was already solving that problem
with a Canvas. A nice side effect: draw order became sibling index, which already
matches the stack's own LIFO order, so sorting is free rather than something I
maintain.

**Completion fires on the last card *landing*, not the last move being *started*.**
`CardDeck` exposes `MoveNext()` and a separate `NotifyCardLanded()`. The presentation
layer calls the second one from each tween's completion callback. Without that split,
"all animations finished" would fire a full move-duration too early — and the domain
would need to know about tween durations to fix it.

**Phoenix Flame's colour transition is 100% Animator Controller.** No tween, no script
lerp. The brief named the mechanism, so the mechanism is what's there — three states
holding a single colour keyframe each, connected by Any State transitions on an int
parameter, with Mecanim's own crossfade doing the blend. The colour list the buttons
read is *derived from the controller* by a custom inspector rather than hand-typed,
because a hand-typed list can silently drift out of sync with the states it's meant
to mirror.

**Three buttons instead of one.** The brief says "a UI button" cycling
orange → green → blue → orange. I shipped three, one per colour. A single cycling
button means a grader who wants to see blue has to press twice and wait through
green; three buttons make every state reachable immediately and make the *current*
state visible. The loop the brief describes is still exactly what the Animator does —
I changed how you enter it, not what it is. If the literal reading was the point, it's
a one-button change.

**Failure handling is a feature here, not an afterthought.** The brief calls out that
avatar URLs may not load and data may be missing, so Magic Words treats every failure
mode identically and visibly: the response parser returns null rather than throwing
(an exception inside a coroutine dies silently and leaves the screen blank forever),
requests carry a timeout, the avatar loader resolves to null on any failure and caches
that result, and a failed fetch shows a real error panel with a retry rather than
claiming the conversation ended. The endpoint ships two deliberately-broken avatars —
they're visible in the running build.

**Things I deliberately built and then deleted:** a generic `IInitializable` +
reflection-driven initializer, an `IStackCountChangeNotifier` interface, and a
`CountChanged` event on `CardStack`. Each one added a layer whose only consumer was
the thing that already had a direct reference. They're in the decisions log with the
reasoning, because "I considered it and it wasn't worth it" is a different answer from
"I didn't think of it."

## Tests

EditMode tests live in `Assets/Tests/EditMode/<Feature>/`, each with its own asmdef
referencing the feature under test. Run them via **Window → General → Test Runner**.

Coverage is deliberately concentrated on the logic assemblies — the deck model, the
dialogue sequencer and text formatter, the navigation state machine, the colour state
machine, and the response parser's malformed-payload paths. The MonoBehaviours in this
project are thin wiring; testing them would mostly be testing Unity.

## Running it

Unity **6000.0.82f1**. Open the project, let it import, and hit Play on
`Assets/Scenes/AppScene.unity` — it boots straight to the main menu.

You can also open any demo scene directly and hit Play; an editor-only bootstrap
loads `AppScene` underneath it so the FPS counter and home button still work. That
compiles out of real builds entirely.

## The hosted build

Built for WebGL and deployed to GitHub Pages via a **Build → Build && Deploy WebGL**
menu item — a deliberate manual step rather than CI on every push, so I choose when
the public link moves.

The page uses a custom WebGL template rather than Unity's stock one. The stock
template hard-codes a 960×600 canvas on desktop, which puts a portrait-first app
inside a landscape letterbox and shrinks every screen. The replacement sizes the
canvas from its aspect ratio against the viewport, so it fills a portrait phone and
becomes a centred full-height portrait pane on a desktop browser, and it respects
safe-area insets instead of drawing under notches.

Because the build is deployed by hand, the link can sit a commit or two behind
`git log`.
