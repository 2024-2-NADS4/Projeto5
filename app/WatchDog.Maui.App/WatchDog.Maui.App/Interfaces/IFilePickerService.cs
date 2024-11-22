namespace WatchDog.Maui.App.Interfaces
{
    public interface IFilePickerService
    {
        Task<string?> PickSaveFileAsync(string suggestedFileName, string defaultExtension);
        Task<string?> PickFileAsync();
    }
}
