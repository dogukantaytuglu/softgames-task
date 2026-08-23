# softgames-task

Hey — this is my take-home for SOFTGAMES' Senior Unity Developer role. Three small,
self-contained demos, built in Unity 6, the way the brief actually asked for them
to be built.

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

## What's actually in here

Three demos, reachable from an in-game menu:

- **Ace of Shadows** — a 144-card deck drains from one stack into another, one
  card a second, until it's done. Done, and past a real polish pass.
- **Magic Words** — a dialogue viewer rendering text and inline emoji together,
  fed by a real fetched endpoint. Not built yet.
- **Phoenix Flame** — a looping particle fire effect with an Animator-driven
  color shift. Not built yet.

Also: an FPS counter visible the whole time (top-left), and the app scales for
both mobile and desktop.

## Where things stand (2026-08-24)

Ace of Shadows is done. Magic Words and Phoenix Flame haven't been started yet —
I'm building them next. SOFTGAMES was explicit that there's no deadline on this,
so I'd rather ship all three done properly than rush the last two just to have
something in every box.

## Playing it

- **Editor** — open `Assets/Scenes/AppScene.unity` and hit Play. It boots
  straight to the main menu.
- **WebGL** — hosted at
  [dogukantaytuglu.github.io/softgames-task-build](https://dogukantaytuglu.github.io/softgames-task-build/).
  I deploy that manually (a "Build & Deploy WebGL" menu item, not CI on every
  push), so at any given moment it may be a commit or two behind what's in
  `git log` here.

## If you want the "why" behind something

`ai-context/decisions.md` has the real reasoning behind the choices in this repo,
written down as I made each one, oldest first. `ai-context/current-context.md` is
the living "where things actually stand" doc I keep up to date instead of letting
this README go stale. Both are more useful than this file if you want depth
rather than a front door.

## Building it yourself

Unity 6000.0.82f1. Open the project, let it import, hit Play on `AppScene`.
EditMode tests live under `Assets/Tests/` (Window → General → Test Runner).
