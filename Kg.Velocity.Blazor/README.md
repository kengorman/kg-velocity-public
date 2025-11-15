# Velocity — Blazor WebAssembly Edition

A browser-based relativistic travel simulation built with Blazor WebAssembly.

## Overview

This is a web version of the Velocity simulation that runs entirely in your browser using WebAssembly. The **same C# physics code** from `Kg.Velocity.Math` runs natively in the browser - no JavaScript translation!

## Features

- ✅ **Full C# Physics Engine** - Runs the exact same `SimulationEngine` and `RelativisticPhysics` code as the Avalonia desktop app
- ✅ **Dark Theme UI** - Matching the desktop application's aesthetic
- ✅ **Real-time Simulation** - 50 FPS update rate
- ✅ **Keyboard Controls** - Same W/X/S/Q/A controls as desktop
- ✅ **No Server Required** - Pure client-side WebAssembly execution
- ✅ **Cross-Platform** - Works on Windows, Mac, Linux, mobile browsers

## How to Run

### Development Mode

```bash
cd Kg.Velocity.Blazor
dotnet run
```

Then open your browser to `https://localhost:5001`

### Build for Production

```bash
dotnet publish -c Release
```

The output will be in `bin/Release/net9.0/publish/wwwroot/` - these static files can be hosted anywhere!

## Hosting Options

Since this is a static Blazor WebAssembly app, you can host it on:

- **GitHub Pages** - Free static hosting
- **Azure Static Web Apps** - Free tier available
- **Netlify / Vercel** - Free static hosting
- **Any web server** - Just serve the `wwwroot` folder

## Technology Stack

- **Blazor WebAssembly (.NET 9)** - UI framework
- **C# WebAssembly** - Runs .NET code in browser
- **Kg.Velocity.Math** - Shared physics library (referenced, not duplicated!)
- **Pure CSS** - No framework dependencies

## Architecture

```
Browser
  └── WebAssembly Runtime
       └── .NET 9 Runtime
            ├── Kg.Velocity.Math.dll (Shared physics library)
            └── Kg.Velocity.Blazor.dll (UI logic)
```

The C# code runs at near-native speeds via WebAssembly, providing excellent performance for the physics simulations.

## Controls

- **W** - Accelerate
- **X** - Decelerate  
- **S** - Slow modifier (fine control)
- **Q** - Launch
- **A** - Reset

## Differences from Desktop Version

**What's the Same:**
- 100% of physics calculations
- All destinations
- Time dilation effects
- Average speed tracking
- Journey progress
- Rocket icon dragging (pre-launch positioning)

**What's Different:**
- Browser-based rendering instead of native

## Performance

First load downloads ~2-3 MB (includes .NET runtime), then everything runs locally in your browser with no server calls.

## Development Notes

The ViewModel was ported from the Avalonia version with minimal changes:
- Removed MVVM Community Toolkit dependencies
- Added `StateChanged` event for UI updates
- Kept all business logic identical

**Code reuse:** ~95% of the logic is shared with the desktop app!

