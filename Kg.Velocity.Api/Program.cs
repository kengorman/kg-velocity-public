using AspNetCoreRateLimit;
using Kg.Velocity.Api.Services;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Trips;
using Microsoft.AspNetCore.ResponseCompression;
using OpenAI.Chat;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Rate limiting configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules =
    [
        new RateLimitRule
        {
            Endpoint = "POST:/api/evaluate-trip",
            Period = "1m",
            Limit = 30
        }
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

builder.Services.AddSingleton<GroqChatClientFactory>();
builder.Services.AddSingleton<OpenAIChatClientFactory>();
builder.Services.AddSingleton<AzureOpenAIChatClientFactory>();
builder.Services.AddSingleton<ChatClient>(sp =>
    sp.GetRequiredService<OpenAIChatClientFactory>()
      .CreateChatClient("gpt-5.2"));
builder.Services.AddSingleton<PromptStore>();
builder.Services.AddSingleton<IPersonaSelector, RandomPersonaSelector>();
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

var app = builder.Build();

app.UseIpRateLimiting();
app.UseCors();
app.UseResponseCompression();

// Routing must be established before static files
app.UseRouting();

// Get the destinations
app.MapGet("/api/destinations", (TripCatalogService catalogs) =>
{
    IReadOnlyList<DestinationDto> destinations = catalogs.GetDestinations();
    return Results.Ok(destinations);
});

// Get the speed presets
app.MapGet("/api/speed-presets", (TripCatalogService catalogs) =>
{
    IReadOnlyList<SpeedPresetDto> presets = catalogs.GetSpeedPresets();
    return Results.Ok(presets);
});

// Evaluate the trip including generating a summary and a poster svg
app.MapPost("/api/evaluate-trip", async (
    TripEvaluateRequest request,
    TripComputationService tripComputationService,
    AiSummaryService aiService,
    AiPosterEventsService posterEventsService,
    PosterEventsCache posterEventsCache) =>
{
    // Input validation
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

    // 1. Compute physics
    var trip = tripComputationService.Compute(request);

    // 2. AI Call #1: Generate persona-flavored summary
    var (summary, persona) = await aiService.GenerateSummaryAsync(request, trip);

    // 3. AI Call #2: Generate poster events (uses summary for tone)
    var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    var posterEvents = await posterEventsService.GenerateEventsAsync(trip, summary);

    // 4. Cache events by nonce for poster endpoint to retrieve
    posterEventsCache.Store(nonce, posterEvents);

    var posterUrl =
        $"/api/poster.svg?nonce={nonce}" +
        $"&destination={Uri.EscapeDataString(request.Destination)}" +
        $"&speed={Uri.EscapeDataString(request.SpeedName)}" +
        $"&earthTime={Uri.EscapeDataString(trip.EarthTimeFormatted)}" +
        $"&shipTime={Uri.EscapeDataString(trip.ShipTimeFormatted)}";

    return Results.Ok(new TripEvaluateResponse(trip, summary, persona.Id, persona.Name, PosterUrl: posterUrl));
});

// Get the poster svg using the nonce created for the summary
app.MapGet("/api/poster.svg", (HttpRequest httpRequest, TripPosterService posterService) =>
{
    var bytes = posterService.GeneratePoster(httpRequest);
    var nonce = httpRequest.Query["nonce"].ToString();
    return Results.File(bytes, "image/svg+xml; charset=utf-8", fileDownloadName: $"velocity-poster-{nonce}.svg");
});

// Static files and fallback after API routes
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
