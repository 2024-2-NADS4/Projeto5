using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace WatchDog.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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

            // Simular a autenticação (conectar API)
            bool isAuthenticated = AuthenticateUser(username, password);

            if (isAuthenticated || username == "admin" && password == "admin")
            {
                await Navigation.PushAsync(new HomeScreen());
            }
            else
            {
                await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
            }
        }

        // Método que simula a autenticação do usuário (conectar API)
        private bool AuthenticateUser(string username, string password)
        {
            // Simulação de autenticação (substitua com a lógica de autenticação real)
            return username == "user" && password == "password";
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterScreen());
        }
    }
}
