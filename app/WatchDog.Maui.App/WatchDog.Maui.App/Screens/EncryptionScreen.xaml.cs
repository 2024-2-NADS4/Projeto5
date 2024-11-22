using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using WatchDog.Maui.App.Interfaces;

namespace WatchDog.Maui.App;

public partial class EncryptionScreen : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private readonly ILogger<EncryptionScreen> _logger;
    private string filePath = string.Empty;
    private readonly IFilePickerService _filePickerService;

    public EncryptionScreen(ILogger<EncryptionScreen> logger, IFilePickerService filePickerService)
    {
        InitializeComponent();
        _logger = logger;
        _filePickerService = filePickerService;
    }

    // Bot�o de Upload
    private async void OnUploadFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                filePath = result.FullPath;
                _logger.LogInformation("Arquivo carregado: {FileName}", result.FileName);

                // Mostrar popup de confirma��o
                bool confirmUpload = await DisplayAlert(
                    "Confirma��o de Upload",
                    $"Voc� selecionou o arquivo: {result.FileName}\nDeseja envi�-lo para criptografia?",
                    "Sim",
                    "N�o"
                );

                if (confirmUpload)
                {
                    _logger.LogInformation("Usu�rio confirmou o envio do arquivo: {FileName}", result.FileName);
                    await DisplayAlert("Upload", "Arquivo carregado com sucesso.", "OK");

                    // Processa automaticamente o arquivo ap�s upload
                    await ProcessFileAsync();
                }
                else
                {
                    _logger.LogWarning("Usu�rio cancelou o envio do arquivo.");
                    await DisplayAlert("Cancelado", "O envio do arquivo foi cancelado.", "OK");
                }
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
            await DisplayAlert("Erro", "Nenhum arquivo carregado. Por favor, fa�a o upload primeiro.", "OK");
            return;
        }

        // Obter valores das checkboxes
        bool isHighlyConfidential = IsHighlyConfidentialCheckBox.IsChecked;
        bool isFrequentlyUsed = IsFrequentlyUsedCheckBox.IsChecked;
        bool isSharedWithThirdParties = IsSharedWithThirdPartiesCheckBox.IsChecked;

        try
        {
            var (encryptedFilePath, encryptionMethod) = await SendFileToEncrypt(filePath, isHighlyConfidential, isFrequentlyUsed, isSharedWithThirdParties);

            if (!string.IsNullOrEmpty(encryptedFilePath))
            {
                _logger.LogInformation("Arquivo criptografado com sucesso: {EncryptedFile}", encryptedFilePath);

                await DisplayAlert("M�todo de Criptografia", $"O arquivo foi criptografado usando: {encryptionMethod}", "OK");

                // Permitir que o usu�rio escolha onde salvar o arquivo
                await SaveEncryptedFile(encryptedFilePath);
            }
            else
            {
                _logger.LogError("Erro ao criptografar o arquivo.");
                await DisplayAlert("Erro", "Erro ao criptografar o arquivo.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o processamento do arquivo.");
            await DisplayAlert("Erro", $"Erro ao processar o arquivo: {ex.Message}", "OK");
        }
    }

    // Enviar Arquivo para a API
    private async Task<(string outputPath, string? encryptionMethod)> SendFileToEncrypt(string filePath, bool isHighlyConfidential, bool isFrequentlyUsed, bool isSharedWithThirdParties)
    {
        try
        {
            var fileContent = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath));

            // Adicionar os par�metros esperados pela API
            fileContent.Add(new StringContent(isHighlyConfidential.ToString()), "isHighlyConfidential");
            fileContent.Add(new StringContent(isFrequentlyUsed.ToString()), "isFrequentlyUsed");
            fileContent.Add(new StringContent(isSharedWithThirdParties.ToString()), "isSharedWithThirdParties");

            string apiUrl = "https://localhost:44361/api/Encrypt/encrypt";
            _logger.LogInformation("Enviando arquivo para API: {ApiUrl}", apiUrl);

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, fileContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Resposta bem-sucedida da API.");

                // Read the encryption method from the response headers
                string? encryptionMethod = null;
                if (response.Headers.TryGetValues("X-Encryption-Method", out var values))
                {
                    encryptionMethod = values.FirstOrDefault();
                }
                else
                {
                    _logger.LogWarning("Encryption method header not found.");
                    encryptionMethod = "Desconhecido";
                }

                var encryptedFileStream = await response.Content.ReadAsStreamAsync();
                string outputPath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(filePath) + ".encrypted");

                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await encryptedFileStream.CopyToAsync(fs);
                }

                return (outputPath, encryptionMethod);
            }
            else
            {
                _logger.LogError("Erro na API ao criptografar: {StatusCode}", response.StatusCode);
                return (string.Empty, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo para API.");
            return (string.Empty, null);
        }
    }

    // Permitir ao usu�rio salvar o arquivo em um local escolhido
    private async Task SaveEncryptedFile(string encryptedFilePath)
    {
        if (string.IsNullOrWhiteSpace(encryptedFilePath))
        {
            _logger.LogWarning("Caminho do arquivo criptografado � nulo ou vazio.");
            await DisplayAlert("Erro", "Arquivo criptografado n�o encontrado.", "OK");
            return;
        }

        try
        {
            // Usar o servi�o injetado para abrir o FileSavePicker
            var suggestedFileName = Path.GetFileName(encryptedFilePath);
            var savePath = await _filePickerService.PickSaveFileAsync(suggestedFileName, ".encrypted");

            if (string.IsNullOrEmpty(savePath))
            {
                _logger.LogWarning("Usu�rio cancelou o salvamento do arquivo.");
                await DisplayAlert("Cancelado", "O salvamento do arquivo foi cancelado.", "OK");
                return;
            }

            _logger.LogInformation("Usu�rio escolheu salvar o arquivo em: {FilePath}", savePath);

            // Copiar o arquivo criptografado para o local escolhido
            using var inputStream = File.OpenRead(encryptedFilePath);
            using var outputStream = File.Create(savePath);
            await inputStream.CopyToAsync(outputStream);

            await DisplayAlert("Sucesso", "Arquivo salvo com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar arquivo criptografado.");
            await DisplayAlert("Erro", $"Erro ao salvar arquivo: {ex.Message}", "OK");
        }
    }
}