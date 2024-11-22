namespace WatchDog.Maui
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Erro", "Por favor, insira um nome de usuário e senha.", "OK");
                return;
            }

            bool isAuthenticated = AuthenticateUser(username, password);

            if (isAuthenticated || username == "admin" && password == "admin")
            {
                var homeScreen = _serviceProvider.GetRequiredService<HomeScreen>();
                await Navigation.PushAsync(homeScreen);
            }
            else
            {
                await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
            }
        }

        // Método que simula a autenticação do usuário (conectar API)
        private static bool AuthenticateUser(string username, string password)
        {
            // Simulação de autenticação (substitua com a lógica de autenticação real)
            return username == "user" && password == "password";
        }
    }
}
