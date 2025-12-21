using Kg.Velocity.Api.Models;
using Kg.Velocity.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapPost("/api/evaluate-trip", (TripEvaluationRequest request) =>
{
    var summary = TripSummaryGenerator.Generate(request);
    return Results.Ok(new TripEvaluationResponse(summary));
});

app.MapFallbackToFile("index.html");

app.Run();
