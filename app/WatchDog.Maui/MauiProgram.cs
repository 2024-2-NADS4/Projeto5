using Microsoft.Extensions.Logging;

namespace WatchDog.Maui
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

            builder.Services.AddSingleton<IImageSource>(new FileImageSource { File = "logowatchdogremovebg.png" });
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<HomeScreen>();
            builder.Services.AddTransient<EncryptionScreen>();
            builder.Services.AddTransient<DecryptionScreen>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
