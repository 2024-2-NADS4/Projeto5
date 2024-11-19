using System.Net.Http.Headers;
using System.Net.Http;

namespace WatchDog.Maui;

public partial class EncryptionScreen : ContentPage
{
    private string filePath = string.Empty;
    private string encryptedFilePath = string.Empty;
    private readonly HttpClient _httpClient = new();

    public EncryptionScreen()
    {
        InitializeComponent();
    }

    private async void OnUploadFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                filePath = result.FullPath;
                LogHistory.Text += $"\nArquivo carregado: {result.FileName}";
            }
            else
            {
                LogHistory.Text += "\nNenhum arquivo selecionado.";
            }
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao carregar arquivo: {ex.Message}";
        }
    }

    private async void OnProcessFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            LogHistory.Text += "\nNenhum arquivo carregado. Por favor, faça o upload primeiro.";
            return;
        }

        try
        {
            // Gerar prompt com base nas seleções
            string prompt = GeneratePrompt();

            LogHistory.Text += $"\nPrompt gerado: {prompt}";

            // Enviar o arquivo para a API para criptografar
            encryptedFilePath = await SendFileToEncrypt(filePath, prompt);
            if (!string.IsNullOrEmpty(encryptedFilePath))
            {
                LogHistory.Text += $"\nArquivo criptografado com sucesso: {Path.GetFileName(encryptedFilePath)}";

                // Habilitar botão para download
                DownloadButton.IsEnabled = true;
            }
            else
            {
                LogHistory.Text += "\nErro ao criptografar o arquivo.";
            }
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao processar arquivo: {ex.Message}";
        }
    }

    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(encryptedFilePath))
        {
            LogHistory.Text += "\nNenhum arquivo criptografado disponível para download.";
            return;
        }

        try
        {
            // Abrir o arquivo criptografado
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(encryptedFilePath)
            });

            LogHistory.Text += "\nArquivo criptografado aberto.";
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao abrir o arquivo: {ex.Message}";
        }
    }

    private string GeneratePrompt()
    {
        string prompt = "Criptografe este arquivo.";

        if (IsFrequentlyUsedCheckBox.IsChecked)
        {
            prompt += " O arquivo é usado com frequência, priorize velocidade.";
        }
        else
        {
            prompt += " O arquivo é usado raramente, priorize segurança.";
        }

        if (IsHighlyConfidentialCheckBox.IsChecked)
        {
            prompt += " O arquivo contém dados extremamente sigilosos.";
        }

        if (IsSharedWithThirdPartiesCheckBox.IsChecked)
        {
            prompt += " O arquivo será compartilhado com terceiros.";
        }

        return prompt;
    }

    private async Task<string> SendFileToEncrypt(string filePath, string prompt)
    {
        try
        {
            var fileContent = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath));

            // Adicionar o prompt ao request
            fileContent.Add(new StringContent(prompt), "prompt");

            string apiUrl = "http://localhost:5236/api/encrypt";

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, fileContent);

            if (response.IsSuccessStatusCode)
            {
                var encryptedFileStream = await response.Content.ReadAsStreamAsync();
                string encryptedFilePath = Path.Combine(FileSystem.AppDataDirectory, "encrypted_file.aes");

                using (var fs = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write))
                {
                    await encryptedFileStream.CopyToAsync(fs);
                }

                return encryptedFilePath;
            }
            else
            {
                LogHistory.Text += "\nErro ao criptografar o arquivo.";
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao enviar arquivo para criptografar: {ex.Message}";
            return string.Empty;
        }
    }
}
