using Newtonsoft.Json;
using System.Text;

namespace WatchDog.Maui
{
    public partial class LoginPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        public LoginPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _httpClient = new HttpClient();
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

            bool isAuthenticated = await AuthenticateUser(username, password);

            if (isAuthenticated)
            {
                var homeScreen = _serviceProvider.GetRequiredService<HomeScreen>();
                await Navigation.PushAsync(homeScreen);
            }
            else
            {
                await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
            }
        }

        // Método que realiza a autenticação do usuário via API
        private async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                var loginModel = new
                {
                    Username = username,
                    Password = password
                };

                string jsonData = JsonConvert.SerializeObject(loginModel);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                string apiUrl = "https://suaapi.com/api/auth/login";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Ler o conteúdo da resposta
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        // Armazenar o token para uso futuro
                        await SecureStorage.SetAsync("auth_token", result.Token);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                // Trate exceções conforme necessário
                await DisplayAlert("Erro", $"Falha na autenticação: {ex.Message}", "OK");
                return false;
            }
        }
    }

    // Classe para desserializar a resposta da API
    public class LoginResponse
    {
        public string Token { get; set; }
    }
}
