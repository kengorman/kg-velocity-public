using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Kg.Velocity.Blazor;
using Kg.Velocity.Blazor.ViewModels;
using Kg.Velocity.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5100") });
builder.Services.AddScoped<TripEvaluationService>();
builder.Services.AddScoped<MainViewModel>();

await builder.Build().RunAsync();
