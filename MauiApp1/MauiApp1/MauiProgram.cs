using MauiApp1.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;

namespace MauiApp1
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
#if ANDROID
            var host = "http://10.0.2.2:8080";
            ImageHandler.Mapper.AppendToMapping(nameof(Microsoft.Maui.IImage.Source), (handler, view) =>
            {
                if (view is Image image && OperatingSystem.IsAndroid())
                {
                    try
                    {
                        if (image.Source is UriImageSource || image.Source is StreamImageSource)
                        {

                            if (handler.PlatformView?.Handle != IntPtr.Zero)
                            {
                                handler.PlatformView?.SetImageBitmap(null);
                                handler.PlatformView?.Invalidate();
                            }
                        }
                    }
                    catch (Java.Lang.RuntimeException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Bitmap error: {ex.Message}");
                    }
                 }
            });
#else
            var host = "http://localhost:8080";
#endif


        builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<NewsPage>();
            builder.Services.AddTransient<OrderPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ShopPage>();
            builder.Services.AddTransient<MessagesPage>();
            builder.Services.AddTransient<RegistrationPage>();
           
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var handler = new SocketsHttpHandler()
                {
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    }
                };

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(host)
                };

                return client;
            });
            return builder.Build();
        }


    }
}
