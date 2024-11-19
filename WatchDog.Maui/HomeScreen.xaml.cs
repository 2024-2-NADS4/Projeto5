namespace WatchDog.Maui;

public partial class HomeScreen : ContentPage
{
	public HomeScreen()
	{
		InitializeComponent();
	}

    private async void OnEncryptionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EncryptionScreen());
    }

    private async void OnDecryptionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DecryptionScreen());
    }
}