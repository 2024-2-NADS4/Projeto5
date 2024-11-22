using WatchDog.Maui.App.Interfaces;
using WatchDog.Maui.App.WinUI.Services;

namespace WatchDog.Maui.App.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseSharedMauiApp();

            builder.Services.AddSingleton<EncryptionScreen>();
            builder.Services.AddSingleton<DecryptionScreen>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<HomeScreen>();
            builder.Services.AddSingleton<IFilePickerService, FilePickerService>();


            return builder.Build();
        }
    }
}
