# Absurd Travel Simulator — Relativistic Travel Simulation

Showing the vastness of space, and in that context the near crawl-speed of light.
See the impact in time and space by going at car speed to Mars, or twice the speed of light to Polaris.
Its intention is to make the user think: "Wow, that is amazing how long a trip from [a] to [b] would really take!"

**Try it live: [absurdtravelsimulator.com](https://www.absurdtravelsimulator.com/)**

Built by Ken Gorman · [LinkedIn](https://www.linkedin.com/in/kengormansoftware/)

<p align="center">
  <img src="docs/readme_images/atsHome.png" width="300" alt="Home screen: pick a destination and a speed, then press Go">
</p>

## Project Overview

This repository contains a simplified relativistic travel simulation:

### 🌐 Blazor WebAssembly App (`Kg.Velocity.Blazor`)
The user interface, running in the web browser. It gets trip results and AI content from the API.
- No installation required
- Mostly C# via WebAssembly, with JavaScript for the movie and carousels
- Works on any device with a modern browser
- Tuned for mobile display

### ⚙️ API Backend (`Kg.Velocity.Api`)
An ASP.NET Core server that hosts the web app, works out the trip physics, and asks OpenAI to write the summary, poster events, and mission log.

### 🧮 Shared Physics Library (`Kg.Velocity.Math`)
The core relativity math, used by the API (and the test apps).
- Lorentz factor
- Time dilation (Earth clock vs ship clock)

## Features

- **Real physics, simplified** - Correct special relativity math for a trip at one constant speed
- **Time Dilation** - See how Earth and ship clocks diverge over long distances based upon speed.
- **Multiple Destinations** - New destinations can be added easily

## What you see

<table>
  <tr>
    <td width="260"><img src="docs/readme_images/atsMovie.png" width="240" alt="Trip movie flying through the Milky Way"></td>
    <td><b>Canvas/JS Movie</b> - Overall view of the journey displayed while content is loaded in the backend.</td>
  </tr>
  <tr>
    <td><img src="docs/readme_images/atsSummary.png" width="240" alt="Journey summary"></td>
    <td><b>Journey Summary</b> - An always-unique summary of the trip in terms of major historic happenings while on the journey.</td>
  </tr>
  <tr>
    <td><img src="docs/readme_images/atsPoster.png" width="240" alt="Absurd Travel Simulator poster"></td>
    <td><b>Absurd Travel Simulator Poster</b> - An always-unique tree-based poster displaying additional unique, sometimes fun, happenings due to the distance and length of the trip.</td>
  </tr>
  <tr>
    <td><img src="docs/readme_images/atsTripLog.png" width="240" alt="Mission log"></td>
    <td><b>Mission Log</b> - Written from the perspective of the captain of the ship. First person experiences during the journey.</td>
  </tr>
  <tr>
    <td><img src="docs/readme_images/atsTripData.png" width="240" alt="Trip data points"></td>
    <td><b>Data Points</b> - The distance, Earth time and ship time for the trip, the gap between them, and the arrival date by Earth and ship calendars.</td>
  </tr>
</table>

## Local Quick Start

You need the [.NET 9 SDK](https://dotnet.microsoft.com/download) and an API key for OpenAI (or another AI provider, see below).
The key is needed for the AI-written parts (summary, poster, mission log); without it only the trip numbers and movie work.

```bash
dotnet dev-certs https --trust   # first time only, so the browser trusts the local https address
dotnet user-secrets set OpenAI:ApiKey <your-key> --project Kg.Velocity.Api
dotnet run --project Kg.Velocity.Api
```

The browser opens to `https://localhost:5100`. The API serves the web app too, so it's the only thing you need to run.

### Using a different AI provider

The app uses OpenAI by default, but any server that accepts OpenAI-style requests works (Ollama, Groq, OpenRouter, Gemini, ...). These settings control it (how to set them is below):

| Setting | What it does |
|---|---|
| `OpenAI:Endpoint` | The server's address, e.g. `http://localhost:11434/v1` for Ollama. Leave unset for OpenAI. |
| `OpenAI:Model` | The model name, e.g. `llama3.1`. Defaults to `gpt-5.2`. |
| `OpenAI:ApiKey` | That server's key (not needed for most local servers). The key is sent to whichever server is set, so don't leave your OpenAI key in place when switching. |
| `OpenAI:MaxTemperature` | Optional. Set to `1.0` for providers that reject higher values. |

The prompts were tuned on gpt-5.2, so other models will write differently. If a model's reply can't be read, the poster and mission log fall back to built-in text.

#### Setting the values

**On your own machine: user secrets.** These are stored in your user profile, outside the repo, so keys never end up in git.

```bash
# Example: switch to Ollama running locally
dotnet user-secrets set OpenAI:Endpoint http://localhost:11434/v1 --project Kg.Velocity.Api
dotnet user-secrets set OpenAI:Model llama3.1 --project Kg.Velocity.Api
dotnet user-secrets remove OpenAI:ApiKey --project Kg.Velocity.Api

# See what's set (careful: this prints your key)
dotnet user-secrets list --project Kg.Velocity.Api

# Back to OpenAI
dotnet user-secrets remove OpenAI:Endpoint --project Kg.Velocity.Api
dotnet user-secrets remove OpenAI:Model --project Kg.Velocity.Api
dotnet user-secrets set OpenAI:ApiKey <your-openai-key> --project Kg.Velocity.Api
```

**On a server (Azure, Docker, etc.): environment variables.** Use a double underscore where the name has a colon, e.g. `OpenAI__ApiKey`, `OpenAI__Endpoint`, `OpenAI__Model`. In Azure App Service, add these under Settings → Environment variables.

Settings that aren't secret (`OpenAI:Endpoint`, `OpenAI:Model`, `OpenAI:MaxTemperature`) can also go in `Kg.Velocity.Api/appsettings.json`. Never put the key there, because that file is committed.

## Architecture

```
Kg.Velocity.Blazor              # Blazor WebAssembly app (runs in the browser)
└── Kg.Velocity.UI              # Razor pages and components, view model, JS (movie, carousel)
    └── Kg.Velocity.Contracts   # Data shapes shared by the web app and the API

Kg.Velocity.Api                 # ASP.NET Core API: hosts the web app, calls OpenAI, draws SVG posters
├── Kg.Velocity.Contracts
└── Kg.Velocity.Engine          # Trip logic: time formatting, speed presets, insight classifier
    └── Kg.Velocity.Math        # Relativity math: Lorentz factor, time dilation
```

All the physics runs on the server; the browser app only shows the results.

Testing:
- `Kg.Velocity.Tests` - automated tests (xUnit) for the physics, time formatting, and insight classifier, including the example trips in this README. They run on every push.

The AI writing is different every time (that's the point), so it has no automated tests. Two small console apps help check it by eye:
- `Kg.Velocity.InsightTests` - prints what the insight classifier decides for many trips (no AI calls)
- `Kg.Velocity.PosterTests` - prints AI-written poster events for many trips (calls the AI model)

When you press Go, the web app makes two requests at the same time: one for the trip physics (fast, no AI) and one for the AI-written content. The movie plays while the AI content is being written.

## Technology Stack

- **.NET 9 / C#**
- **Blazor WebAssembly** - C# in the browser, hosted by an **ASP.NET Core** API
- **OpenAI** (gpt-5.2) - trip summary, poster events, mission log
- **Scriban** - templates for the SVG poster and mission log
- **Embla Carousel** - swipeable destination/speed pickers and results
- **HTML Canvas** - the trip movie
- **Azure App Service** + **Application Insights**, deployed by **GitHub Actions**

## Physics Implementation

Each trip is a single, constant speed.
- **Earth time** = distance ÷ speed
- **Lorentz factor (γ)** = 1 / √(1 − v²/c²)
- **Ship time** = Earth time ÷ γ. The faster you go, the less time passes for the traveler.
- **Faster than light** - not physically possible, but allowed for fun. The trip is instant for the traveler (ship time 0) while Earth still waits the full distance ÷ speed.

What's simplified:
- Instant top speed: no speeding up or slowing down
- Distances are fixed averages (Mars is always 140 million miles, even though planets move)
- No gravity effects (no general relativity)

## Building

```bash
dotnet build
dotnet test
dotnet publish Kg.Velocity.Api -c Release   # includes the web app
```

Node.js is only needed if you change the carousel JavaScript (`Kg.Velocity.UI/wwwroot/js/embla-carousel.js`). Rebuild its bundle with:

```bash
npm install
npm run build:embla
```

## How this evolved

- **It began as a simple prompt sent to a model.** Challenge what the user thinks about a 'trip' to some faraway point anywhere in the universe. Tell some interesting - possibly fun or quirky - details about how far time on Earth vs time in the spaceship went out of sync. Drop in historical references used for comparison to show the true scope of the journey. But I wasn't happy with the plain, repetitive, and uncreative responses.

- **Adding subtlety with weights.** I split the text into three calls - for the summary, poster, and travel log - made at the same time, each with its own prompt ([Kg.Velocity.Api/Prompts](Kg.Velocity.Api/Prompts)). Each prompt also got a set of weights worked out from the trip's numbers ([JourneyWeights](Kg.Velocity.Engine/JourneyWeights.cs)): how much the story should lean on awe, patience, loneliness, and so on. Here's an excerpt from `travel-log.md`:
  ```
  Journey Weights (guide which consequences to emphasize, but never name them):
  - Emotion: {{WeightEmotion}}%
  - Distance: {{WeightDistance}}%
  - Awe: {{WeightAwe}}%
  - TimeGoneBy: {{WeightTimeGoneBy}}%
  - Memories: {{WeightMemories}}%
  - Patience: {{WeightPatience}}%
  - Loneliness: {{WeightLoneliness}}%

  Interpret the weights as:
  - High Distance → emphasize scale, crossings, separation thresholds
  - High TimeGoneBy → emphasize aging, calendar shifts, historical distance
  - High Loneliness → emphasize isolation, communication asymmetry, signal delay
  - High Awe → emphasize surprise at scale, sky changes, and loss of familiar reference
  - High Emotion / Memories → emphasize irreversibility and generational effects
  - High Patience → emphasize long waits, delayed outcomes, slow crossings

  Journey Data:
  - From: Earth
  - To: {{Destination}}
  - Departed: {{DepartedTime}}
  - Distance: {{DistanceLightYears}} light-years ({{DistanceMiles}} miles)
  - Speed: {{SpeedName}} ({{PercentageOfLightSpeed}} percent of light speed)
  - Earth time elapsed: {{EarthTimeFormatted}}
  - Ship time elapsed: {{ShipTimeFormatted}}
  - Time dilation difference: {{TimeDifference}}
  ```

- **From weights to one insight.** The number of weights turned out to push the writing toward naming feelings ("you felt lonely"), which falls flat. For the summary and poster I replaced them with a single *insight*: what this trip is really about. [JourneyInsightClassifier](Kg.Velocity.Engine/JourneyInsightClassifier.cs) scores the trip on speed, duration, dilation, scale, and farewell, and picks the winner - sometimes two, like "speed and dilation" - or "journey" when nothing stands out. Walking to the Moon (9 years) is about duration; the Moon at 99% light speed is about speed and dilation; Andromeda at 1000x light speed is about farewell - everything that carried on back home without the traveler. The prompts then describe concrete details and let the reader feel the rest. The travel log still uses the weights; moving it to the insight is on the list.

- **The end result...** There are interesting points here. 1) The typical user has no idea that multiple model calls are generating arguably a totally unique answer. In fact most may guess that these are hard-coded replies. 2) The separate calls to the model help vary the tone of the responses. The travel poster - displaying interesting 'think about this' points - has a different feel than the travel log which is written in first-person as an occupant on the journey.

- **An Android app, then back to web only.** In January 2026 I added an Android version built with .NET MAUI (Blazor Hybrid). To make that work, the Blazor pages and components moved into a shared library (`Kg.Velocity.UI`), so the web and Android apps used the same screens and the same API. It was published on Google Play ([Absurd Travel Simulator](https://play.google.com/store/apps/details?id=com.absurdtravelsimulator)), where an early version is still available. In September 2026 I stopped Android work: for a project this small, one web version that runs anywhere is the most convenient to maintain. The MAUI project was removed; it's still in git history, and the plan is in [docs/MAUI-MIGRATION-PLAN.md](docs/MAUI-MIGRATION-PLAN.md).

- **The trip movie started as standalone prototypes.** The zoom-out animation that plays while the AI text is being written began as three separate HTML pages, one for each scale: the solar system (measured in distances from the Sun), the Milky Way (light-years, spaced on a log scale so near and far stars both fit), and other galaxies (out to Andromeda). We also tried a single animation that zooms through all three scales in one go. It was dropped because each scale had its own hand-tuned look (planet detail, star landmarks, spiral galaxies) that got lost when combined. The three versions now live together in `Kg.Velocity.UI/wwwroot/js/trip-movie.js`, and the app picks one based on the destination.

## Image credits

Destination images are from NASA and ESA/Hubble ([esahubble.org](https://esahubble.org/)).
ESA/Hubble images are used under the [Creative Commons Attribution 4.0 license](https://creativecommons.org/licenses/by/4.0/) (CC BY 4.0).
Example: Sombrero Galaxy, NASA/ESA and The Hubble Heritage Team (STScI/AURA).

## License

See LICENSE file for details.

## Contributing

If you're interested in contributing, please contact me at gorman.kenneth@gmail.com or on [LinkedIn](https://www.linkedin.com/in/kengormansoftware/).

---

**Enjoy the trip!**
