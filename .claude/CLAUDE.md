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

# Build MAUI Android
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
├── Kg.Velocity.Maui      # Android app
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
- `POST /api/evaluate-trip` - Full evaluation with AI-generated summary and poster

## Configuration

- User secrets ID: `kg-velocity-api` (for OpenAI API key)
- Rate limiting: 30 requests/minute on evaluate-trip endpoint
- CORS: Configured for Azure deployment + localhost:5100-5101
