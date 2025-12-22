using Kg.Velocity.Api.Models;
using Kg.Velocity.Api.Services;
using Microsoft.AspNetCore.ResponseCompression;
using AspNetCoreRateLimit;

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

builder.Services.AddSingleton<AiSummaryService>();

var app = builder.Build();

app.UseIpRateLimiting();
app.UseCors();
app.UseResponseCompression();

// Routing must be established before static files
app.UseRouting();

app.MapPost("/api/evaluate-trip", async (TripEvaluationRequest request, AiSummaryService aiService) =>
{
    // Input validation
    const int maxLength = 100;
    if (string.IsNullOrWhiteSpace(request.Destination) || request.Destination.Length > maxLength)
        return Results.BadRequest("Invalid destination");
    if (string.IsNullOrWhiteSpace(request.SpeedName) || request.SpeedName.Length > maxLength)
        return Results.BadRequest("Invalid speed name");
    
    var (summary, personaName) = await aiService.GenerateSummaryAsync(request);
    return Results.Ok(new TripEvaluationResponse(summary, personaName));
});

// Static files and fallback after API routes
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
