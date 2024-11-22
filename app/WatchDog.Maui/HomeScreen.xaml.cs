namespace WatchDog.Maui;

public partial class HomeScreen : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public HomeScreen(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private async void OnEncryptionClicked(object sender, EventArgs e)
    {
        var encryptionScreen = _serviceProvider.GetRequiredService<EncryptionScreen>();
        await Navigation.PushAsync(encryptionScreen);
    }

    private async void OnDecryptionClicked(object sender, EventArgs e)
    {
        var decryptionScreen = _serviceProvider.GetRequiredService<DecryptionScreen>();
        await Navigation.PushAsync(decryptionScreen);
    }
}
