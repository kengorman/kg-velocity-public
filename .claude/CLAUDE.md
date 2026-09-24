# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build entire solution
dotnet build

# Run Blazor frontend (https://localhost:5001)
dotnet run --project Kg.Velocity.Blazor

# Run API backend (https://localhost:5100)
dotnet run --project Kg.Velocity.Api

# Run tests
dotnet run --project Kg.Velocity.InsightTests

# Build MAUI Android (no longer under development)
dotnet build Kg.Velocity.Maui -f net9.0-android

# Bundle Embla carousel (after npm install)
npm run build:embla
```

## Architecture

**Relativistic travel simulator** with multi-platform frontends sharing a common physics engine.

### Project Dependencies (bottom-up)

```
Kg.Velocity.Math          # Pure physics: Lorentz factor, time dilation
    ↓
Kg.Velocity.Engine        # Simulation logic: FlightComputer, destinations, presets
    ↓
Kg.Velocity.Contracts     # DTOs shared between UI and API
    ↓
Kg.Velocity.UI            # Razor components (Index.razor is the main UI)
    ↓
├── Kg.Velocity.Blazor    # WebAssembly frontend
├── Kg.Velocity.Maui      # Android app (no longer under development; web/Blazor only going forward)
└── Kg.Velocity.Api       # ASP.NET Core backend with OpenAI integration
```

### Key Patterns

- **Shared library strategy**: ~95% code reuse via Kg.Velocity.Math/Engine
- **MVVM**: MainViewModel in Kg.Velocity.UI handles state, services handle business logic
- **Scriban templates**: SVG poster generation in Api/Templates/*.sbn
- **LLM prompts**: Embedded resources in Api/Prompts/*.md

### API Endpoints

- `GET /api/destinations` - Available travel destinations
- `GET /api/speed-presets` - Speed preset options
- `POST /api/compute-trip` - Calculate trip physics (no AI)
- `POST /api/generate-content` - AI-generated summary, poster events, and travel log; returns poster and travel log URLs
- `GET /api/poster.svg` - Trip poster image (uses the nonce from generate-content)
- `GET /api/travel-log.svg` - Travel log image (uses the nonce from generate-content)

The app calls `compute-trip` and `generate-content` in parallel. The old `POST /api/evaluate-trip` endpoint is commented out in `Program.cs` (kept for reference).

## Configuration

- User secrets ID: `kg-velocity-api` (for OpenAI API key)
- Rate limiting: 30 requests/minute on generate-content endpoint (the one that calls OpenAI)
- CORS: Configured for Azure deployment + localhost:5100-5101

## LLM Prompt Design Philosophy

The app uses GPT to generate trip summaries. These principles emerged from iterative testing:

### Core Approach: Insights, Not Emotions

The `JourneyInsightClassifier` determines what each trip is *about* (speed, duration, dilation, scale, farewell). This single insight guides the narrative—no emotion weights, no binary flags, just one mechanism.

**Why this works**: Telling users "you felt lonely" falls flat. Describing concrete details that *evoke* loneliness lets users feel it themselves.

### Key Principles

1. **Describe, don't prescribe** - Never tell users what they felt. Describe what happened; let them infer emotion.

2. **Quiet observational lines** - Include one line that *notices* something small (a bootlace fraying, a label curling, a coffee droplet) rather than *concluding* something philosophical.

3. **Back home without mourning** - When Earth time is long, acknowledge what continued (seasons, generations, maps redrawn) without being mournful or heavy-handed.

4. **Let significance emerge from the insight**:
   - "speed" → don't dwell on trivial time differences
   - "dilation" → the two-clocks story matters, mention it concretely
   - "farewell" → focus on what continued without the traveler
   - "journey" → nothing extreme, keep it grounded

5. **FTL handling** - Faster-than-light means ship time = 0 (instant for traveler). This is extreme dilation but the classifier scores it via "farewell" (Earth time passes) not "dilation" (which requires sub-light γ > 1).

### Files Involved

- `Kg.Velocity.Engine/JourneyInsightClassifier.cs` - Determines what the story is about
- `Kg.Velocity.Api/Services/TripSummaryPromptBuilder.cs` - Injects insight into prompt
- `Kg.Velocity.Api/Prompts/trip-summary.md` - The prompt template (uses `{{JourneyInsight}}`)
- `Kg.Velocity.InsightTests/Program.cs` - Console app to test classifier outputs

### Testing Approach

Run `Kg.Velocity.InsightTests` to see classifier outputs across scenarios. Then test actual LLM output by running the app and trying contrasting trips:
- Moon @ walking vs 99% c vs 100x c
- Pluto @ walking vs 99% c vs 100x c
- Andromeda @ 1000x c (farewell at cosmic scale)

The goal: every trip feels remarkable in its own way, calibrated to what's actually interesting about that specific combination of distance and speed.
