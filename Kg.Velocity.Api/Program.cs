using AspNetCoreRateLimit;
using Kg.Velocity.Api.Services;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Trips;
using Microsoft.AspNetCore.ResponseCompression;
using OpenAI.Chat;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Telemetry is only turned on when a connection string is set (in Azure, the APPLICATIONINSIGHTS_CONNECTION_STRING
// app setting). Without one, App Insights 3.x stops the app at startup, so local runs and forks skip it.
var appInsightsConnectionString =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] ??
    builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Rate limiting configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules =
    [
        // generate-content calls the AI model (summary, poster events, travel log), so it's the one worth limiting
        new RateLimitRule
        {
            Endpoint = "POST:/api/generate-content",
            Period = "1m",
            Limit = 30
        }

        // Not used: evaluate-trip is commented out below; the app now uses compute-trip + generate-content.
        // new RateLimitRule
        // {
        //     Endpoint = "POST:/api/evaluate-trip",
        //     Period = "1m",
        //     Limit = 30
        // }
    ];
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS - locked to production domain
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://kg-velocity-e9dvhgcdere8cdeh.centralus-01.azurewebsites.net",
                "https://localhost:5100",
                "http://localhost:5101")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream", "application/wasm"]);
});

builder.Services.AddSingleton<ModelClientFactory>();
builder.Services.AddSingleton<ChatClient>(sp =>
    sp.GetRequiredService<ModelClientFactory>()
      .CreateChatClient());
builder.Services.AddSingleton<PromptStore>();
// Not used: personas were replaced by JourneyInsightClassifier (see Services/PersonaCatalog.cs).
// builder.Services.AddSingleton<IPersonaSelector, RandomPersonaSelector>();
builder.Services.AddSingleton<TripSummaryPromptBuilder>();
builder.Services.AddSingleton<AiSummaryService>();
builder.Services.AddSingleton<TripComputationService>();
builder.Services.AddSingleton<TripCatalogService>();

// Poster services
builder.Services.AddSingleton<PosterEventsPromptBuilder>();
builder.Services.AddSingleton<AiPosterEventsService>();
builder.Services.AddSingleton<PosterEventsCache>();
builder.Services.AddSingleton<DestinationIconService>();
builder.Services.AddSingleton<TripPosterService>();

// Travel log services
builder.Services.AddSingleton<TravelLogPromptBuilder>();
builder.Services.AddSingleton<AiTravelLogService>();
builder.Services.AddSingleton<TravelLogEventsCache>();
builder.Services.AddSingleton<TravelLogService>();

var app = builder.Build();

app.UseIpRateLimiting();
app.UseCors();
app.UseResponseCompression();

// Routing must be established before static files
app.UseRouting();

// Get the destinations
app.MapGet("/api/destinations", (HttpContext ctx, TripCatalogService catalogs) =>
{
    ctx.Response.Headers.CacheControl = "no-store, no-cache";
    IReadOnlyList<DestinationDto> destinations = catalogs.GetDestinations();
    return Results.Ok(destinations);
});

// Get the speed presets
app.MapGet("/api/speed-presets", (TripCatalogService catalogs) =>
{
    IReadOnlyList<SpeedPresetDto> presets = catalogs.GetSpeedPresets();
    return Results.Ok(presets);
});

// Compute trip physics only (instant, no AI)
app.MapPost("/api/compute-trip", async (
    TripEvaluateRequest request,
    TripComputationService tripComputationService) =>
{
    const int maxLength = 100;
    if (string.IsNullOrWhiteSpace(request.Destination) || request.Destination.Length > maxLength)
        return Results.BadRequest("Invalid destination");
    if (string.IsNullOrWhiteSpace(request.SpeedName) || request.SpeedName.Length > maxLength)
        return Results.BadRequest("Invalid speed name");
    if (request.SpeedMph <= 0 || double.IsNaN(request.SpeedMph) || double.IsInfinity(request.SpeedMph))
        return Results.BadRequest("Invalid speed");
    if (request.DistanceMiles <= 0 || double.IsNaN(request.DistanceMiles) || double.IsInfinity(request.DistanceMiles))
        return Results.BadRequest("Invalid distance");
    if (request.StartTime == default)
        return Results.BadRequest("Invalid start time");

    var trip = tripComputationService.Compute(request);
    await Task.Delay(2000);
    return Results.Ok(trip);
});

// Generate AI content (summary + poster + travel log)
app.MapPost("/api/generate-content", async (
    TripEvaluateRequest request,
    TripComputationService tripComputationService,
    AiSummaryService aiService,
    AiPosterEventsService posterEventsService,
    AiTravelLogService travelLogService,
    PosterEventsCache posterEventsCache,
    TravelLogEventsCache travelLogEventsCache) =>
{
    const int maxLength = 100;
    if (string.IsNullOrWhiteSpace(request.Destination) || request.Destination.Length > maxLength)
        return Results.BadRequest("Invalid destination");
    if (string.IsNullOrWhiteSpace(request.SpeedName) || request.SpeedName.Length > maxLength)
        return Results.BadRequest("Invalid speed name");
    if (request.SpeedMph <= 0 || double.IsNaN(request.SpeedMph) || double.IsInfinity(request.SpeedMph))
        return Results.BadRequest("Invalid speed");
    if (request.DistanceMiles <= 0 || double.IsNaN(request.DistanceMiles) || double.IsInfinity(request.DistanceMiles))
        return Results.BadRequest("Invalid distance");
    if (request.StartTime == default)
        return Results.BadRequest("Invalid start time");

    var trip = tripComputationService.Compute(request);

    var summaryTask = aiService.GenerateSummaryAsync(request, trip);
    var posterEventsTask = posterEventsService.GenerateEventsAsync(trip);
    var travelLogTask = travelLogService.GenerateEntriesAsync(trip);
    await Task.WhenAll(summaryTask, posterEventsTask, travelLogTask);

    var (summary, persona) = summaryTask.Result;
    var posterEvents = posterEventsTask.Result;
    var travelLogEntries = travelLogTask.Result;

    var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    posterEventsCache.Store(nonce, posterEvents);
    travelLogEventsCache.Store(nonce, travelLogEntries);

    var posterUrl =
        $"/api/poster.svg?nonce={nonce}" +
        $"&destination={Uri.EscapeDataString(request.Destination)}" +
        $"&speed={Uri.EscapeDataString(request.SpeedName)}" +
        $"&earthTime={Uri.EscapeDataString(trip.EarthTimeFormatted)}" +
        $"&shipTime={Uri.EscapeDataString(trip.ShipTimeFormatted)}";

    var travelLogUrl =
        $"/api/travel-log.svg?nonce={nonce}" +
        $"&destination={Uri.EscapeDataString(request.Destination)}" +
        $"&speed={Uri.EscapeDataString(request.SpeedName)}" +
        $"&departure={Uri.EscapeDataString(trip.DepartedEarthTime)}" +
        $"&arrival={Uri.EscapeDataString(trip.ArrivedEarthTime)}";

    return Results.Ok(new TripContentResponse(summary, persona.Id, persona.Name, posterUrl, travelLogUrl));
});

// Not used: the app no longer calls /api/evaluate-trip. It was replaced by two calls:
// /api/compute-trip (physics only, fast) and /api/generate-content (AI summary, poster, travel log).
// Kept here for reference.
//
// // Evaluate the trip including generating a summary and a poster svg
// app.MapPost("/api/evaluate-trip", async (
//     TripEvaluateRequest request,
//     TripComputationService tripComputationService,
//     AiSummaryService aiService,
//     AiPosterEventsService posterEventsService,
//     PosterEventsCache posterEventsCache) =>
// {
//     // Input validation
//     const int maxLength = 100;
//     if (string.IsNullOrWhiteSpace(request.Destination) || request.Destination.Length > maxLength)
//         return Results.BadRequest("Invalid destination");
//     if (string.IsNullOrWhiteSpace(request.SpeedName) || request.SpeedName.Length > maxLength)
//         return Results.BadRequest("Invalid speed name");
//
//     if (request.SpeedMph <= 0 || double.IsNaN(request.SpeedMph) || double.IsInfinity(request.SpeedMph))
//         return Results.BadRequest("Invalid speed");
//     if (request.DistanceMiles <= 0 || double.IsNaN(request.DistanceMiles) || double.IsInfinity(request.DistanceMiles))
//         return Results.BadRequest("Invalid distance");
//     if (request.StartTime == default)
//         return Results.BadRequest("Invalid start time");
//
//     // 1. Compute physics
//     var trip = tripComputationService.Compute(request);
//
//     // 2. AI calls: Generate summary and poster events concurrently
//     var summaryTask = aiService.GenerateSummaryAsync(request, trip);
//     var posterEventsTask = posterEventsService.GenerateEventsAsync(trip);
//     await Task.WhenAll(summaryTask, posterEventsTask);
//
//     var (summary, persona) = summaryTask.Result;
//     var posterEvents = posterEventsTask.Result;
//
//     var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
//
//     // 4. Cache events by nonce for poster endpoint to retrieve
//     posterEventsCache.Store(nonce, posterEvents);
//
//     var posterUrl =
//         $"/api/poster.svg?nonce={nonce}" +
//         $"&destination={Uri.EscapeDataString(request.Destination)}" +
//         $"&speed={Uri.EscapeDataString(request.SpeedName)}" +
//         $"&earthTime={Uri.EscapeDataString(trip.EarthTimeFormatted)}" +
//         $"&shipTime={Uri.EscapeDataString(trip.ShipTimeFormatted)}";
//
//     return Results.Ok(new TripEvaluateResponse(trip, summary, persona.Id, persona.Name, PosterUrl: posterUrl));
// });

// Get the poster svg using the nonce created for the summary
app.MapGet("/api/poster.svg", (HttpRequest httpRequest, TripPosterService posterService) =>
{
    var bytes = posterService.GeneratePoster(httpRequest);
    var nonce = httpRequest.Query["nonce"].ToString();
    return Results.File(bytes, "image/svg+xml; charset=utf-8", fileDownloadName: $"velocity-poster-{nonce}.svg");
});

// Get the travel log svg
app.MapGet("/api/travel-log.svg", (HttpRequest httpRequest, TravelLogService travelLogService) =>
{
    var bytes = travelLogService.GenerateTravelLog(httpRequest);
    var nonce = httpRequest.Query["nonce"].ToString();
    return Results.File(bytes, "image/svg+xml; charset=utf-8", fileDownloadName: $"travel-log-{nonce}.svg");
});

// Static files and fallback after API routes
// Force browsers to revalidate index.html, JS and CSS on every load (cheap 304 via ETag
// when unchanged). Without a Cache-Control header, browsers guess a lifetime and can
// keep serving a stale bundle for days after a deploy. _framework files are left alone
// because Blazor fingerprints and cache-manages them itself.
var revalidateStaticFiles = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (!ctx.Context.Request.Path.StartsWithSegments("/_framework"))
            ctx.Context.Response.Headers.CacheControl = "no-cache";
    }
};

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(revalidateStaticFiles);
app.MapFallbackToFile("index.html", revalidateStaticFiles);

app.Run();
