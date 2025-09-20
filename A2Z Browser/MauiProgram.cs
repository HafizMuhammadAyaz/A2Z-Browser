using A2Z_Browser.Services;
using A2Z_Browser.ViewModels;
using A2Z_Browser.Views;
using Microsoft.Extensions.Logging;

namespace A2Z_Browser
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Build connection string
            var connectionString = "Server=DESKTOP-VMCSF78;Database=DbA2zBroswer001;Integrated Security=True;Trusted_Connection=True;";

            // Register services
            builder.Services.AddSingleton(provider => new DbAccess(connectionString));
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ProxyService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<BrowserViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<BrowserPage>();

            return builder.Build();
        }
    }
}