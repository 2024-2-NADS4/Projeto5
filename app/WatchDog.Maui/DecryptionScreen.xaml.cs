using System.Net.Http.Headers;

namespace WatchDog.Maui;

public partial class DecryptionScreen : ContentPage
{
    private string filePath = string.Empty;
    private string decryptedFilePath = string.Empty;
    private HttpClient _httpClient = new HttpClient();
    public DecryptionScreen()
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

    // Processar Arquivo (Enviar para API para Descriptografar)
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

            // Enviar o arquivo para a API para descriptografar
            decryptedFilePath = await SendFileToDecrypt(filePath);
            if (!string.IsNullOrEmpty(decryptedFilePath))
            {
                LogHistory.Text += $"\nArquivo descriptografado com sucesso: {Path.GetFileName(decryptedFilePath)}";

                // Habilitar botão para download
                DownloadButton.IsEnabled = true;
            }
            else
            {
                LogHistory.Text += "\nErro ao descriptografar o arquivo.";
            }
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao processar arquivo: {ex.Message}";
        }
    }

    // Baixar Arquivo Descriptografado
    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(decryptedFilePath))
        {
            LogHistory.Text += "\nNenhum arquivo descriptografado disponível para download.";
            return;
        }

        try
        {
            // Aqui usamos o Launcher para abrir o arquivo descriptografado
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(decryptedFilePath)
            });

            LogHistory.Text += "\nArquivo descriptografado aberto.";
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
            ".aes" => "Arquivo Criptografado (AES)",
            ".txt" => "Texto",
            ".jpg" => "Imagem JPEG",
            ".png" => "Imagem PNG",
            ".pdf" => "Documento PDF",
            _ => "Desconhecido"
        };
    }

    // Enviar Arquivo para a API para Descriptografar
    private async Task<string> SendFileToDecrypt(string filePath)
    {
        try
        {
            var fileContent = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileContentPart = new StreamContent(fileStream);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Add(fileContentPart, "file", Path.GetFileName(filePath));

            // URL da API que descriptografa o arquivo
            string apiUrl = "https://suaapi.com/decrypt"; // Coloque a URL da sua API de descriptografia aqui.

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, fileContent);
            if (response.IsSuccessStatusCode)
            {
                var decryptedFileStream = await response.Content.ReadAsStreamAsync();
                string decryptedFilePath = Path.Combine(FileSystem.AppDataDirectory, "decrypted_file.txt");

                using (var fs = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write))
                {
                    await decryptedFileStream.CopyToAsync(fs);
                }

                return decryptedFilePath;
            }
            else
            {
                LogHistory.Text += "\nFalha ao descriptografar o arquivo.";
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            LogHistory.Text += $"\nErro ao enviar arquivo para descriptografar: {ex.Message}";
            return string.Empty;
        }
    }
}