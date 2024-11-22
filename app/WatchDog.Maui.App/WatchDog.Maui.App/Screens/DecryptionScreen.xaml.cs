using Microsoft.Extensions.Logging; 
using System.Net.Http.Headers;
using WatchDog.Maui.App.Interfaces;

namespace WatchDog.Maui.App;

public partial class DecryptionScreen : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private readonly ILogger<DecryptionScreen> _logger;
    private string filePath = string.Empty;
    private string decryptedFilePath = string.Empty;
    private readonly IFilePickerService _filePickerService;

    public DecryptionScreen(ILogger<DecryptionScreen> logger, IFilePickerService filePickerService)
    {
        InitializeComponent();
        _logger = logger;
        _filePickerService = filePickerService;
    }

    // Botão de Upload
    private async void OnUploadFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                filePath = result.FullPath ?? string.Empty;
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
        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("Caminho do arquivo é nulo ou vazio.");
            return string.Empty;
        }

        try
        {
            var fileContent = new MultipartFormDataContent();

            // Ensure the file exists
            if (!File.Exists(filePath))
            {
                _logger.LogError("Arquivo não encontrado: {FilePath}", filePath);
                await DisplayAlert("Erro", "Arquivo não encontrado.", "OK");
                return string.Empty;
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContentPart.Headers.ContentLength = fileStream.Length; // Ensure Content-Length is set
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath) ?? "uploaded_file");
            fileContent.Add(new StringContent(encryptionMethod), "encryptionMethod");

            string apiUrl = "https://localhost:44361/api/Decrypt/decrypt";

            _logger.LogInformation("Enviando arquivo para descriptografia: {FilePath} usando {Method}", filePath, encryptionMethod);

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, fileContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Resposta bem-sucedida da API.");

                var decryptedFileStream = await response.Content.ReadAsStreamAsync();

                // Remove the .encrypted extension to get the original file name
                string originalFileName = Path.GetFileName(filePath) ?? "decrypted_file";
                string outputFileName = originalFileName.Replace(".encrypted", "");
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
                await DisplayAlert("Erro", "Falha na comunicação com o servidor.", "OK");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo para descriptografia.");
            await DisplayAlert("Erro", $"Erro ao enviar arquivo: {ex.Message}", "OK");
            return string.Empty;
        }
    }

    // Permitir ao usuário salvar o arquivo em um local escolhido
    private async Task SaveDecryptedFile(string decryptedFilePath)
    {
        if (string.IsNullOrEmpty(decryptedFilePath))
        {
            _logger.LogWarning("Caminho do arquivo descriptografado é nulo ou vazio.");
            await DisplayAlert("Erro", "Arquivo descriptografado não encontrado.", "OK");
            return;
        }

        try
        {
            // Usar o serviço injetado para abrir o FileSavePicker
            var suggestedFileName = Path.GetFileName(decryptedFilePath)?.Replace(".encrypted", "") ?? "arquivo_descriptografado";
            var savePath = await _filePickerService.PickSaveFileAsync(suggestedFileName, Path.GetExtension(decryptedFilePath) ?? ".txt");

            if (string.IsNullOrEmpty(savePath))
            {
                _logger.LogWarning("Usuário cancelou o salvamento do arquivo.");
                await DisplayAlert("Cancelado", "O salvamento do arquivo foi cancelado.", "OK");
                return;
            }

            _logger.LogInformation("Usuário escolheu salvar o arquivo em: {FilePath}", savePath);

            // Copiar o arquivo descriptografado para o local escolhido
            using var inputStream = File.OpenRead(decryptedFilePath);
            using var outputStream = File.Create(savePath);
            await inputStream.CopyToAsync(outputStream);

            await DisplayAlert("Sucesso", "Arquivo salvo com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar arquivo descriptografado.");
            await DisplayAlert("Erro", $"Erro ao salvar arquivo: {ex.Message}", "OK");
        }
    }
}
