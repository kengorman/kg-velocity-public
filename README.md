# Absurd Travel Simulator — Relativistic and Fun Travel Simulation

Showing the vastness of space, and in that context the near crawl-speed of light.
See the impact in time and space by going at car speed to Mars, or twice the speed of light to Polaris.
It's intention is to make the user think: "Wow, that is amazing how long a trip from [a] to [b] would really take!"!

## Project Overview

This repository contains a relativistic physics simulation:

### 🌐 Blazor WebAssembly App (`Kg.Velocity.Blazor`)
A browser-based version that runs entirely in a web browser.
- No installation required
- Pure C# running via WebAssembly
- Works on any device with a modern browser
- Tuned for mobile display

### 🧮 Shared Physics Library (`Kg.Velocity.Math`)
The core simulation engine used by **both** applications.
- Relativistic physics calculations
- Time dilation effects
- Lorentz transformations
- Journey simulation

## Features

- **Realistic Physics** - Accurate special relativity calculations
- **Time Dilation** - See how Earth and ship clocks diverge over long distances based upon speed.
- **Multiple Destinations** - New destinations can be added easily
- **Real-time Stats** - Average speed, ETA, distance tracking

## Displays
- **Canvas/JS Movie** - Overall view of the journey displayed while content is loaded in the backend
loaded in the backend
- **Journey Summary** - An always-unique summary of the trip in terms of major historic happenings while on the journey.
- **Absurd Travel Simulator Poster** - An always-unique tree-based poster displaying additional unique, sometimes fun, happenings due to the distance and length of the trip.
- **Mission Log** - Written from the perspective of the captain of the ship. First person experiences during the journey.
- **Data Points** - Displaying the distance, the arrival date by earth and ship calendar, the duration in earth years of the trip, the arrival date by ship calendar, the length of time travel, and the difference in time between the earth and ship.


## Local Quick Start 

### Web (Blazor)
```bash
cd Kg.Velocity.Api
dotnet run
```
Browser will open to: `https://localhost:5001`

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

## How this evolved

- **The trip movie started as standalone prototypes.** The zoom-out animation that plays while the AI text is being written began as three separate HTML pages, one for each scale: the solar system (measured in distances from the Sun), the Milky Way (light-years, spaced on a log scale so near and far stars both fit), and other galaxies (out to Andromeda). We also tried a single animation that zooms through all three scales in one go. It was dropped because each scale had its own hand-tuned look (planet detail, star landmarks, spiral galaxies) that got lost when combined. The three versions now live together in `Kg.Velocity.UI/wwwroot/js/trip-movie.js`, and the app picks one based on the destination.

## License

See LICENSE file for details.

## Contributing

This is a fun, educational project exploring special relativity concepts through interactive simulation.

## Future Ideas
- [ ] More destinations (black holes, edge of observable universe)
- [ ] Visualization of length contraction
- [ ] Multiple reference frames
- [ ] General relativity effects
- [ ] Journey replay/history

---

**Made with ❤️ to explore the wonders of special relativity**
