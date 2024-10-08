﻿using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace MauiApp1;
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
            })
            .UseBarcodeReader();
            
        ;

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // setup OidcClient
        builder.Services.AddSingleton(new OidcClient(new()
        {
            Authority = "https://dev.amnathrig.app",

            ClientId = "maui_client",
            Scope = "openid profile amnathrigAPI",
            RedirectUri = "amnathrig://callback",
            PostLogoutRedirectUri = "amnathrig://callback", 
            

            Browser = new MauiAuthenticationBrowser()
        }));

        // add main page
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddTransient<CreateAssetPage>();

        return builder.Build();
    }
}
