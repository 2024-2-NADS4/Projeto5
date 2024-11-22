using WatchDog.Maui.App.Interfaces;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WatchDog.Maui.App.WinUI.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<string?> PickSaveFileAsync(string suggestedFileName, string defaultExtension)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = suggestedFileName
            };
            picker.FileTypeChoices.Add("Arquivo", new List<string> { defaultExtension });

            var mainWindow = Application.Current?.Windows.FirstOrDefault();
            if (mainWindow?.Handler?.PlatformView is not MauiWinUIWindow platformWindow)
            {
                throw new InvalidOperationException("Handler da janela principal não encontrado.");
            }

            var hwnd = platformWindow.WindowHandle;
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            return file?.Path;
        }

        public async Task<string?> PickFileAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");

            var mainWindow = Application.Current?.Windows.FirstOrDefault();
            if (mainWindow?.Handler?.PlatformView is not MauiWinUIWindow platformWindow)
            {
                throw new InvalidOperationException("Handler da janela principal não encontrado.");
            }

            var hwnd = platformWindow.WindowHandle;
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            return file?.Path;
        }
    }
}
