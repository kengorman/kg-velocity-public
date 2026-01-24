using Microsoft.Extensions.Logging;
using Kg.Velocity.Maui.Services;
using Kg.Velocity.UI.Services;
using Kg.Velocity.UI.ViewModels;

namespace Kg.Velocity.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var apiBaseUrl = "https://www.absurdtravelsimulator.com";
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

        builder.Services.AddScoped<TripEvaluationService>();
        builder.Services.AddScoped<TripCatalogClient>();
        builder.Services.AddScoped<IPersonaIdStore, MauiPersonaIdStore>();
        builder.Services.AddScoped<MainViewModel>();

        return builder.Build();
    }
}
