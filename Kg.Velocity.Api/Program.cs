using Kg.Velocity.Api.Models;
using Kg.Velocity.Api.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

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

app.UseResponseCompression();

// Routing must be established before static files
app.UseRouting();

app.MapPost("/api/evaluate-trip", async (TripEvaluationRequest request, AiSummaryService aiService) =>
{
    var summary = await aiService.GenerateSummaryAsync(request);
    return Results.Ok(new TripEvaluationResponse(summary));
});

// Static files and fallback after API routes
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
