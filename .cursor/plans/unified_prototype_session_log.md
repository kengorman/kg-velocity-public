# Unified Movie Prototype — Session Log (Feb 15, 2026)

## What Happened

User had a prior conversation/session (likely earlier today) where a **deep investigation** was done into issues with the unified movie prototype. That investigation's context was **lost** when switching to a new agent/session. The user is understandably frustrated. This file preserves what we know so it doesn't happen again.

## Current State of `prototype-unified-template.html`

The file was created in this session. It has **known rendering issues**:

### Bug: Everything renders only a couple pixels wide
- **Root cause**: The hybrid coordinate system (`lyToY()`) produces Y-values with a huge range (0 to ~8800 for the full Sun-to-Andromeda span), but X-coordinates for objects like galactic landmarks use tiny values (0.01, -0.015, etc.)
- The background stars also use tiny X spread values while Y spread is multiplied by 800
- This creates an extremely tall, extremely narrow rendering — objects are all stacked on a vertical line a few pixels wide
- **The existing templates don't have this problem** because they use consistent coordinate spaces where X and Y are in the same units

### Other likely issues (not yet investigated)
- LOD cross-fade zoom thresholds (planetAlpha, ssIconAlpha, galacticAlpha, mwSpiralAlpha, andromedaAlpha) were set by guessing absolute zoom values — these need to be tuned to actual zoom ranges the camera traverses
- Spiral galaxy star positions (MW and Andromeda) use mixed coordinate units — Y is in the large lyToY space but X uses tiny fractional values
- The `startZoom = 8` for the camera may not produce the right initial planet sizes (the solar system template uses `zoom = 600` in AU-space)
- Background star Y positions scaled by `* 800` is arbitrary and may not cover the viewport at all zoom levels

## What Exists in the Codebase (for reference)

### Three working templates:
1. **`prototype-solarsystem-template.html`** — AU coordinates, Earth at 1.0 AU, startZoom ~600
2. **`prototype-milkyway-template.html`** — Normalized [0,1] with log scale, `lyToY()` maps 1-30000 ly to [0,1], startZoom = screenH*12
3. **`prototype-extragalactic-template.html`** — Normalized [0,1], Earth at 0.2, Andromeda at 0.9, startZoom = screenH*12

### Common patterns across all three:
- `easeInCubicOut`: 85% cubic ease-in, 15% quadratic ease-out
- Log-space zoom interpolation
- Camera pan clamped so source stays in viewport bottom
- Ship trail: `[6,6]` dash, 0.4 opacity, 1.5px width, dot hidden at 98%
- Arrival highlight: ramps 0.75 to 0.90 progress
- Trip data panel: fades in at 88% progress
- Background stars: 3 parallax layers with twinkle
- High-DPI canvas with devicePixelRatio

### The plan (from `unified_movie_prototype_4b5aebfc.plan.md`):
- One continuous zoom from Earth (1 AU) outward
- 5 LOD layers with cross-fade: Planets → SS Icon → Galactic Landmarks → MW Spiral → Andromeda
- Hybrid coordinate system: linear in AU for planetary band, log10(ly) beyond
- Duration scales with log-distance range: 4s (Moon) to 8s (Andromeda)
- 15 destinations from Moon to Andromeda in one dropdown

## What Needs to Happen Next

1. **Fix the coordinate system** — X and Y must be in compatible units so objects have visual width
2. **Fix background star distribution** — stars need to fill the viewport at every zoom level
3. **Tune LOD cross-fade thresholds** — based on actual zoom values the camera hits during animation
4. **Fix galaxy spiral generation** — MW and Andromeda star positions need proper X-spread in the same coordinate units
5. **Test every destination** — Moon through Andromeda, verify smooth zoom and proper layer transitions

## User's Prior Investigation (LOST)

The user reports going through a "very deep investigation" of these rendering issues in a previous session. The findings from that investigation are not available. If the user recovers those findings, they should be added here.
