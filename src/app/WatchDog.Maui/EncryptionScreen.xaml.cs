using System.Net.Http.Headers;
using System.Net.Http;

namespace WatchDog.Maui;

public partial class EncryptionScreen : ContentPage
{
    private string filePath = string.Empty;
    private string encryptedFilePath = string.Empty;
    private HttpClient _httpClient = new HttpClient();
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

    // Processar Arquivo (Enviar para API para Criptografar)
    private async void OnProcessFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            LogHistory.Text += "\nNenhum arquivo carregado. Por favor, faça o upload primeiro.";
            return;
        }

        try
        {
            // Simular identificação de tipo com IA
            string fileType = IdentifyFileType(filePath);
            LogHistory.Text += $"\nTipo de arquivo identificado: {fileType}";

            // Enviar o arquivo para a API para criptografar
            encryptedFilePath = await SendFileToEncrypt(filePath);
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

    // Baixar Arquivo Criptografado
    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(encryptedFilePath))
        {
            LogHistory.Text += "\nNenhum arquivo criptografado disponível para download.";
            return;
        }

        try
        {
            // Aqui usamos o Launcher para abrir o arquivo criptografado
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

    // Identificar Tipo de Arquivo (Simulação de IA)
    private string IdentifyFileType(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "Texto",
            ".jpg" => "Imagem JPEG",
            ".png" => "Imagem PNG",
            ".pdf" => "Documento PDF",
            _ => "Desconhecido"
        };
    }

    // Enviar Arquivo para a API para Criptografar
    private async Task<string> SendFileToEncrypt(string filePath)
    {
        try
        {
            var fileContent = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath));

            // URL da API que criptografa o arquivo
            string apiUrl = "";

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
                LogHistory.Text += "\nFalha ao criptografar o arquivo.";
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