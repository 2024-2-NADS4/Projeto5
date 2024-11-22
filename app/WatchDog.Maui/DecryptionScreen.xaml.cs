using Microsoft.Extensions.Logging; // Biblioteca de logging
using System.Net.Http.Headers;
using Windows.Storage.Pickers;

namespace WatchDog.Maui;

public partial class DecryptionScreen : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private readonly ILogger<DecryptionScreen> _logger;
    private string filePath = string.Empty;
    private string decryptedFilePath = string.Empty;

    public DecryptionScreen(ILogger<DecryptionScreen> logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    // Botão de Upload
    private async void OnUploadFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                filePath = result.FullPath;
                _logger.LogInformation("Arquivo carregado: {FileName}", result.FileName);

                // Notificação de sucesso no upload
                await DisplayAlert("Upload", "Arquivo carregado com sucesso.", "OK");

                // Processa automaticamente o arquivo após upload
                await ProcessFileAsync();
            }
            else
            {
                _logger.LogWarning("Nenhum arquivo foi selecionado.");
                await DisplayAlert("Erro", "Nenhum arquivo selecionado.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar arquivo.");
            await DisplayAlert("Erro", $"Erro ao carregar arquivo: {ex.Message}", "OK");
        }
    }

    // Processar o Arquivo
    private async Task ProcessFileAsync()
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("Tentativa de processar sem arquivo carregado.");
            await DisplayAlert("Erro", "Nenhum arquivo carregado. Por favor, faça o upload primeiro.", "OK");
            return;
        }

        var selectedItem = EncryptionTypePicker.SelectedItem;
        if (selectedItem == null)
        {
            _logger.LogWarning("Método de criptografia não selecionado.");
            await DisplayAlert("Erro", "Selecione o método de criptografia antes de prosseguir.", "OK");
            return;
        }

        string encryptionMethod = selectedItem.ToString() ?? string.Empty;

        try
        {
            decryptedFilePath = await SendFileToDecrypt(filePath, encryptionMethod);

            if (!string.IsNullOrEmpty(decryptedFilePath))
            {
                _logger.LogInformation("Arquivo descriptografado com sucesso: {DecryptedFile}", decryptedFilePath);
                await DisplayAlert("Sucesso", "Arquivo descriptografado com sucesso!", "OK");

                await SaveDecryptedFile(decryptedFilePath);
            }
            else
            {
                _logger.LogError("Erro ao descriptografar o arquivo.");
                await DisplayAlert("Erro", "Erro ao descriptografar o arquivo.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o processamento do arquivo.");
            await DisplayAlert("Erro", $"Erro ao processar o arquivo: {ex.Message}", "OK");
        }
    }

    // Enviar Arquivo para a API
    private async Task<string> SendFileToDecrypt(string filePath, string encryptionMethod)
    {
        try
        {
            var fileContent = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContentPart.Headers.ContentLength = fileStream.Length; // Ensure Content-Length is set
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath));
            fileContent.Add(new StringContent(encryptionMethod), "encryptionMethod");

            string apiUrl = "https://localhost:44361/api/Decrypt/decrypt";

            _logger.LogInformation("Enviando arquivo para descriptografia: {FilePath} usando {Method}", filePath, encryptionMethod);

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, fileContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Resposta bem-sucedida da API.");

                var decryptedFileStream = await response.Content.ReadAsStreamAsync();

                // Remove the .encrypted extension to get the original file name
                string outputFileName = Path.GetFileName(filePath).Replace(".encrypted", "");
                string outputPath = Path.Combine(FileSystem.AppDataDirectory, outputFileName);

                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await decryptedFileStream.CopyToAsync(fs);
                }

                return outputPath;
            }
            else
            {
                _logger.LogError("Erro na API ao descriptografar: {StatusCode}", response.StatusCode);
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo para descriptografia.");
            return string.Empty;
        }
    }

    // Permitir ao usuário salvar o arquivo em um local escolhido
    private async Task SaveDecryptedFile(string decryptedFilePath)
    {
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            // Get the original file name without the .encrypted extension
            string originalFileName = Path.GetFileName(decryptedFilePath);
            string originalFileNameWithoutExtension = originalFileName.Replace(".encrypted", "");

            picker.SuggestedFileName = originalFileNameWithoutExtension;

            // Get the original file extension
            string originalExtension = Path.GetExtension(originalFileNameWithoutExtension);

            // If there's no extension (e.g., the file was "file.encrypted"), you might want to handle it
            if (string.IsNullOrEmpty(originalExtension))
            {
                originalExtension = "*"; // Allow all files
                picker.FileTypeChoices.Add("Todos os Arquivos", ["."]);
            }
            else
            {
                picker.FileTypeChoices.Add($"Arquivo {originalExtension}", [originalExtension]);
            }

            // Mostrar o picker ao usuário
            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                _logger.LogInformation("Usuário escolheu salvar o arquivo em: {FilePath}", file.Path);

                using var inputStream = File.OpenRead(decryptedFilePath);
                using var outputStream = await file.OpenStreamForWriteAsync();

                await inputStream.CopyToAsync(outputStream);
                await DisplayAlert("Sucesso", "Arquivo salvo com sucesso!", "OK");
            }
            else
            {
                _logger.LogWarning("Usuário cancelou o salvamento do arquivo.");
                await DisplayAlert("Cancelado", "O salvamento do arquivo foi cancelado.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar arquivo descriptografado.");
            await DisplayAlert("Erro", $"Erro ao salvar arquivo: {ex.Message}", "OK");
        }
    }
}