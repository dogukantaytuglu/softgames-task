# Build size — measurement plan (PARKED, not yet executed)

> **Status: parked deliberately.** The decision was to *talk through* size
> optimisation before doing it, rather than cutting first and writing it up after.
> Nothing in this file has been actioned. The README's hosted-build section
> deliberately contains **no size numbers** until this work happens — a half-measured
> claim is worse than none, and `BRIEF.md` §6 is explicitly about showing the
> *reasoning*, not the number.

## Why this matters more than a normal optimisation task

`BRIEF.md` §6: in interview 1 the WebGL download-payload gap was conceded openly.
This assignment requires a hosted WebGL build, so the write-up is the chance to close
that gap inside the same process where it was admitted. The point is not to hit a
specific MB figure — it's to demonstrate that the levers are understood and that each
cut was a judgement call with a stated cost.

## Current measurement (2026-08-27, off the live deployed files)

| File | Transferred | Notes |
|---|---|---|
| `.data.unityweb` | 10.95 MB | assets, scenes, fonts |
| `.wasm.unityweb` | 8.26 MB | engine code |
| loader + framework | ~0.19 MB | |
| **Total** | **~19.4 MB** | |

Two things to verify before quoting these anywhere:

1. **These are Brotli-compressed bytes.** `webGLCompressionFormat: 0` is Brotli, and
   the deployed files carry **no `Content-Encoding: br` header** — GitHub Pages serves
   them as opaque `application/vnd.unity`, so Unity's JS decompression fallback is
   doing the work client-side. That means 19.4 MB *is* the real transfer, but it also
   means the browser pays a JS decompression cost the header path would avoid.
   Whether GitHub Pages can be made to send the header at all is worth checking — if
   not, that's itself a documented trade-off of the host choice.
2. Re-measure after a fresh build. The numbers above predate the WebGL quality-tier
   change and the custom template.

## Identified drivers, largest first

### 1. `LargeFlame02.tif` — 2048×4096, no WebGL platform override
`Assets/Feature/PhoenixFlame/Textures/LargeFlame02.tif.meta` has overrides for
`DefaultTexturePlatform` (max 4096) and `Standalone` only. WebGL inherits the default.
As DXT5 with mips this is plausibly ~11 MB of GPU data for a flipbook whose particles
render at a fraction of screen size.

**Lever:** a WebGL platform override capping max size. **Cost:** flipbook frame
resolution — needs a visual check at each step down, not a blind cap, because this is
the aesthetics showcase screen.
**Expected:** the single largest win available.

### 2. TMP font assets — ~11.4 MB serialized
- `Baloo2 Bold SDF.asset` — 6.8 MB
- `Rubik SDF.asset` — 3.1 MB
- `Baloo2 SDF.asset` — 1.5 MB

All three are `m_AtlasPopulationMode: 1` (dynamic). Two consequences: the committed
atlases are large, *and* they get re-dirtied every time the Editor renders a glyph
that isn't in them yet — which is why a commit that existed purely to snapshot a clean
atlas state was dirty again within the hour.

**Lever:** static atlases built from the actual character set, which is fully known at
build time (menu labels, three screens of UI copy, and 17 lines of endpoint dialogue).
**Cost:** any character outside the baked set renders as a missing glyph — so the
endpoint's real payload has to be the source of truth for the character set, and a
changed payload would need a rebake. **Bonus:** fixes the perpetually-dirty tree.

### 3. `webGLExceptionSupport: 1` + default managed stripping
Documented in D26 as a deliberate decline — full exception support was kept for
debuggability. That's defensible, but only if the trade-off is written down where a
grader sees it. Right now it's a decision made and never surfaced.

**Lever:** exceptions → "Explicitly Thrown Exceptions Only", higher managed stripping
level. **Cost:** worse stack traces from the hosted build; stripping can break
reflection-dependent code (none obvious here, but it needs a real click-through of all
three demos after, not a compile check).

### 4. Not yet investigated
- Whether unused engine modules can still be trimmed further (D26 did a pass).
- Shader variant stripping.
- Whether the card-art textures still carry more than the two decks in rotation needs.

## What "done" looks like

A README section with: the before number, each lever applied, what it cost, and the
after number — plus at least one lever *considered and rejected*, with the reason.
The rejections are the part that shows judgement.

## Order of work when this un-parks

1. ~~Install the WebGL module + clone the build repo~~ — **done**, both exist on the
   current machine (`6000.0.82f1` has WebGLSupport; `../softgames-task-build` is
   cloned and up to date, last deploy `2026-08-28 04:21`).
2. ~~Baseline build, measured off the actual output files~~ — **done, re-verified
   2026-08-28** off the current deployed build (not re-built, just re-measured — see
   table below). Numbers essentially match the 2026-08-27 measurement, so nothing
   material changed the payload in between.
3. Flame texture cap → re-measure → visual check.
4. Static font atlases → re-measure → glyph check across all four screens.
5. Decide exceptions/stripping with the click-through cost priced in.
6. Write it up.

## Baseline re-verified (2026-08-28, off `../softgames-task-build/Build`, commit `4753444`)

| File | Bytes | MB |
|---|---|---|
| `.data.unityweb` | 10,948,081 | 10.44 |
| `.wasm.unityweb` | 8,241,881 | 7.86 |
| `.framework.js.unityweb` | 77,315 | 0.07 |
| `.loader.js` | 117,366 | 0.11 |
| **Total** | **19,384,643** | **~18.5 MB** |

Matches the 2026-08-27 figures closely (~19.4 MB then too) — confirms the numbers in
this file are still trustworthy as a starting point, not stale.
