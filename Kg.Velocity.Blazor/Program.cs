using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Kg.Velocity.Blazor;
using Kg.Velocity.UI.Services;
using Kg.Velocity.UI.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<TripEvaluationService>();
builder.Services.AddScoped<TripCatalogClient>();
builder.Services.AddScoped<MainViewModel>();

await builder.Build().RunAsync();
