# Android Store Deployment Notes

*Captured: 2026-01-26*

## Current Readiness: ~70-75%

### What's Ready
- Project structure, icons, splash screen
- Android manifest with permissions (INTERNET, ACCESS_NETWORK_STATE)
- API pointing to production URL (absurdtravelsimulator.com)
- Targets Android 7.0+ (API 24)
- Color scheme and branding (purple #512BD4)

### Must Fix Before Play Store

| Item | Current | Needs |
|------|---------|-------|
| Package ID | `com.companyname.kg.velocity.maui` | Your real domain (e.g., `com.yourdomain.ats`) |
| App Title | `Kg.Velocity.Maui` | User-friendly name like "Absurd Travel Simulator" |
| Signing | None | Create a keystore and configure signing |
| Build format | APK (default) | AAB (required by Play Store now) |

### Optional But Recommended
- ProGuard/R8 obfuscation for release builds
- Explicit Release build configuration in csproj
- targetSdkVersion configuration
- Activity exported attributes for API 31+

### Files to Modify
- `Kg.Velocity.Maui/Kg.Velocity.Maui.csproj` - Update ApplicationId, ApplicationTitle, add signing config
- Create signing keystore (.jks) - store securely outside repo

### Next Steps
1. Choose final package ID (must be unique, can't change after Play Store upload)
2. Generate signing keystore
3. Configure Release build with AAB output
4. Test release build locally
5. Create Play Store developer account ($25 one-time fee)
6. Prepare store listing assets (screenshots, description, etc.)
