# Velocity — Relativistic Travel Simulation

Experience the mind-bending effects of special relativity as you travel through space at near-light speeds!

## Project Overview

This repository contains **two implementations** of the same relativistic physics simulation:

### 🖥️ Avalonia Desktop App (`Kg.Velocity.Avalonia`)
A cross-platform desktop application built with Avalonia UI.
- Native performance
- Rich desktop UI
- Runs on Windows, macOS, Linux

### 🌐 Blazor WebAssembly App (`Kg.Velocity.Blazor`)
A browser-based version that runs entirely in your web browser.
- No installation required
- Pure C# running via WebAssembly
- Works on any device with a modern browser

### 🧮 Shared Physics Library (`Kg.Velocity.Math`)
The core simulation engine used by **both** applications.
- Relativistic physics calculations
- Time dilation effects
- Lorentz transformations
- Journey simulation

## Features

- **Realistic Physics** - Accurate special relativity calculations
- **Time Dilation** - Watch Earth and ship clocks diverge
- **Multiple Destinations** - From Los Angeles to Andromeda Galaxy
- **Variable Speed** - Accelerate and decelerate during flight
- **Real-time Stats** - Average speed, ETA, distance tracking
- **Journey Progress** - Visual track with spaceship indicator

## Quick Start

### Desktop (Avalonia)
```bash
cd Kg.Velocity.Avalonia
dotnet run
```

### Web (Blazor)
```bash
cd Kg.Velocity.Blazor
dotnet run
```
Then open browser to `https://localhost:5001`

## Controls

- **W** - Accelerate
- **X** - Decelerate  
- **S** - Slow modifier (fine control)
- **Q** - Launch (start journey)
- **A** - Reset

## Architecture

```
Kg.Velocity/
├── Kg.Velocity.Avalonia/     # Desktop UI (Avalonia)
├── Kg.Velocity.Blazor/        # Web UI (Blazor WebAssembly)
├── Kg.Velocity.Math/          # Shared physics engine ⭐
├── Kg.Velocity.Console/       # Console app
└── README.md                  # This file
```

**Key Design:** The `Kg.Velocity.Math` library is shared between all projects, ensuring consistent physics across platforms!

## Technology Stack

- **.NET 9** - Modern, cross-platform framework
- **C#** - Type-safe, high-performance language
- **Avalonia UI** - Cross-platform desktop UI
- **Blazor WebAssembly** - C# in the browser
- **WebAssembly** - Near-native performance in browsers

## Physics Implementation

The simulation accurately models:
- **Lorentz Factor (γ)** - Time dilation and length contraction
- **Proper Time** - Ship clock vs Earth clock
- **Relativistic Velocity** - Up to and beyond light speed (for educational purposes)
- **Average Speed Tracking** - Accounts for velocity changes during flight
- **Incremental Time Integration** - Proper physics for variable speeds

## Building

### Build All Projects
```bash
dotnet build
```

### Build Specific Project
```bash
dotnet build Kg.Velocity.Avalonia/Kg.Velocity.Avalonia.csproj
dotnet build Kg.Velocity.Blazor/Kg.Velocity.Blazor.csproj
```

### Publish for Distribution
```bash
# Desktop
dotnet publish Kg.Velocity.Avalonia/ -c Release

# Web
dotnet publish Kg.Velocity.Blazor/ -c Release
```

## Project Structure

### Kg.Velocity.Math
Core physics engine - platform agnostic
- `SimulationEngine.cs` - Main simulation loop
- `SimulationState.cs` - Current state data
- `RelativisticPhysics.cs` - Physics calculations
- `PhysicsConstants.cs` - Speed of light, etc.

### Kg.Velocity.Avalonia
Desktop application using Avalonia UI framework
- Full MVVM architecture
- Rich desktop controls
- Keyboard input handling

### Kg.Velocity.Blazor
Web application using Blazor WebAssembly
- Component-based UI
- Runs C# natively in browser via WebAssembly
- Static hosting capable

## Development

Both UIs share ~95% of the business logic through the `Kg.Velocity.Math` library. ViewModels are ported with minimal changes between platforms.

## License

See LICENSE file for details.

## Contributing

This is an educational project exploring special relativity concepts through interactive simulation.

## Future Ideas

- [x] Add rocket icon dragging in Blazor version
- [ ] More destinations (black holes, edge of observable universe)
- [ ] Visualization of length contraction
- [ ] Multiple reference frames
- [ ] General relativity effects
- [ ] Journey replay/history

---

**Made with ❤️ to explore the wonders of special relativity**
