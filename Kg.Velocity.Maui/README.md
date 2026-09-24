# Kg.Velocity.Maui (archived)

An Android version of the app, built with .NET MAUI. It shares the same UI
components (Kg.Velocity.UI) as the web version and calls the same API.

**No longer maintained.** Work continues on the web version only. This project
has been removed from the solution and from the CI build, so it is left here
for reference and may not build against the current code.

To try building it anyway (needs the MAUI Android workload installed):

```bash
dotnet workload install maui-android
dotnet build Kg.Velocity.Maui -f net9.0-android
```

A signed Release build also needs the `ANDROID_KEYSTORE_PATH` and
`ANDROID_KEYSTORE_PASSWORD` environment variables set.
