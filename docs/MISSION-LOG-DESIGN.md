# Mission Log Panel Design

## Overview
A new panel displaying a "logbook" style summary with trip metadata and LLM-generated facts about the journey and destination.

## Visual Layout (mockup)
```
MISSION LOG — EXCERPTS

Frame of reference: Earth departure
Trajectory: [destination] — [speed]
Departure: [departure date]
Arrival: [arrival date]

interesting fact 1
interesting fact 2
interesting fact 3
interesting fact 4
interesting fact 5
```

## Design Decisions

### 1. Panel Position
- Appears **after the poster**, **before the calculations** (time dilation chart, etc.)

### 2. LLM Call
- Uses the **journey weights** (Emotion, Awe, Loneliness, Distance, TimeGoneBy, Memories, Patience) as input
- Should be **async** - follows the same pattern as the poster generation
- Separate API endpoint (or bundled with generate-content)

### 3. SVG Rendering
- **Server-side rendered** using Scriban template (same pattern as trip-poster.svg.sbn)
- Logbook-style background with facts baked into the SVG

### 4. Content Style
- **Mix of both** scientific destination facts AND narrative journey experience
- Will need iteration/tuning on the prompt

## Technical Notes
- Follow existing patterns in:
  - `AiPosterEventsService.cs` for async LLM calls
  - `TripPosterService.cs` / `trip-poster.svg.sbn` for SVG templating
  - `MainViewModel.cs` phase flow for UI state management

## Open Questions
- Exact prompt wording for the right mix of facts
- Visual styling of the logbook SVG (aged paper, ink colors, etc.)
- Whether to bundle with existing generate-content call or separate endpoint
