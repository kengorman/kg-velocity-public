using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Api.Services;
using Microsoft.AspNetCore.ResponseCompression;
using AspNetCoreRateLimit;
using OpenAI.Chat;

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
builder.Services.AddSingleton<ChatClient>(sp =>
    sp.GetRequiredService<GroqChatClientFactory>()
      .CreateChatClient("meta-llama/llama-4-scout-17b-16e-instruct"));
builder.Services.AddSingleton<PromptStore>();
builder.Services.AddSingleton<IPersonaSelector, RandomPersonaSelector>();
builder.Services.AddSingleton<TripSummaryPromptBuilder>();
builder.Services.AddSingleton<AiSummaryService>();
builder.Services.AddSingleton<TripComputationService>();
builder.Services.AddSingleton<TripCatalogService>();

var app = builder.Build();

app.UseIpRateLimiting();
app.UseCors();
app.UseResponseCompression();

// Routing must be established before static files
app.UseRouting();

app.MapGet("/api/destinations", (TripCatalogService catalogs) =>
{
    IReadOnlyList<DestinationDto> destinations = catalogs.GetDestinations();
    return Results.Ok(destinations);
});

app.MapGet("/api/speed-presets", (TripCatalogService catalogs) =>
{
    IReadOnlyList<SpeedPresetDto> presets = catalogs.GetSpeedPresets();
    return Results.Ok(presets);
});

app.MapPost("/api/evaluate-trip", async (
    TripEvaluateRequest request,
    TripComputationService tripComputationService,
    AiSummaryService aiService) =>
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

    var trip = tripComputationService.Compute(request);
    var (summary, persona) = await aiService.GenerateSummaryAsync(request, trip);
    return Results.Ok(new TripEvaluateResponse(trip, summary, persona.Id, persona.Name));
});

// Static files and fallback after API routes
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
