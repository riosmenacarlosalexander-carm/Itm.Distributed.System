using Itm.Store.Mobile.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;

namespace Itm.Store.Mobile
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

            builder.Services.AddTransient<AuthHandler>();

            builder.Services.AddHttpClient("GatewayClient", client =>
            {
                client.BaseAddress = new Uri("http://10.0.2.2:5110/");
            }).AddHttpMessageHandler<AuthHandler>();

            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}

