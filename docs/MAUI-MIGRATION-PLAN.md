# MAUI Migration Plan

## Goal
Add iOS and Android apps to kg-velocity while maximizing code reuse with the existing Blazor web app.

## Approach: MAUI Blazor Hybrid

All three apps (web, iOS, Android) will:
- Use the exact same API (`Kg.Velocity.Api`)
- Share UI components via a Razor Class Library
- Share all physics/engine code

## Target Project Structure

```
Kg.Velocity/
├── Kg.Velocity.Math/           # Unchanged - physics library
├── Kg.Velocity.Engine/         # Unchanged - simulation engine
├── Kg.Velocity.Contracts/      # Unchanged - shared DTOs
├── Kg.Velocity.Api/            # Unchanged - single backend for all clients
│
├── Kg.Velocity.UI/             # NEW - Razor Class Library
│   ├── Components/             # All Blazor components move here
│   ├── ViewModels/             # MainViewModel, etc.
│   ├── Services/               # API client services
│   └── wwwroot/                # Shared CSS, assets
│
├── Kg.Velocity.Blazor/         # REFACTORED - thin host shell only
│   ├── Program.cs
│   └── wwwroot/index.html
│
└── Kg.Velocity.Maui/           # NEW - MAUI Blazor Hybrid app
    ├── Platforms/iOS/
    ├── Platforms/Android/
    ├── MauiProgram.cs
    └── MainPage.xaml           # Hosts BlazorWebView
```

## Code Reuse Summary

| Layer | Reuse |
|-------|-------|
| Physics (`Math`, `Engine`) | 100% shared |
| DTOs (`Contracts`) | 100% shared |
| API | Single backend, all clients call it |
| UI Components | 100% shared via Razor Class Library |
| Host apps | Thin shells only (~5% unique each) |

## Development Environment (Windows)

### Android Testing
- Android Emulator via Visual Studio
- Install "Mobile development with .NET" workload
- Create virtual devices in Android Device Manager

### iOS Testing
- **Hot Restart**: Deploy to physical iPhone via USB from Windows
- Requirements:
  - Physical iPhone
  - USB cable
  - iTunes installed (for drivers)
  - Apple ID (free works, 7-day app expiration)
- Good for: UI validation, debugging, general testing
- Not for: Performance benchmarks, App Store builds

### iOS Building/Publishing
Apple requires macOS for final iOS builds. Options:
1. **GitHub Actions** (recommended) - macOS runners, CI/CD pipeline
2. **Cloud Mac** - MacStadium, MacinCloud
3. **Mac Mini** - cheapest physical option (~$600)

## Recommended Workflow

1. Develop entirely on Windows
2. Test Android locally with emulator
3. Test iOS via Hot Restart on physical iPhone
4. Push to git → GitHub Actions builds both platforms → publishes to stores

## Migration Steps (High Level)

1. Create `Kg.Velocity.UI` Razor Class Library project
2. Move shared components from `Kg.Velocity.Blazor` to `Kg.Velocity.UI`
3. Update `Kg.Velocity.Blazor` to reference the new RCL
4. Verify web app still works
5. Create `Kg.Velocity.Maui` project
6. Reference `Kg.Velocity.UI` from MAUI project
7. Configure BlazorWebView in MainPage.xaml
8. Test on Android emulator
9. Test on iOS via Hot Restart
10. Set up GitHub Actions for CI/CD builds

---
*Plan created: January 2026*
