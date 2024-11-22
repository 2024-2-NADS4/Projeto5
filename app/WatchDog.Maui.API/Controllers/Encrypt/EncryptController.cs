using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using WatchDog.Maui.API.Services.Encrypt;

namespace WatchDog.Maui.API.Controllers.Encrypt
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncryptController : ControllerBase
    {
        private readonly EncryptionStrategyContext _encryptionContext;
        private readonly ChatClient _chatClient;

        public EncryptController(IConfiguration configuration)
        {
            _encryptionContext = new EncryptionStrategyContext();

            // Recuperar a API Key do appsettings.json
            string apiKey = configuration.GetValue<string>("OpenAI:ApiKey");

            // Configurar o ChatClient com a API Key
            _chatClient = new ChatClient(
                model: "gpt-4o-mini",
                apiKey: apiKey
            );
        }

        [HttpPost("encrypt")]
        public async Task<IActionResult> EncryptFile( IFormFile file, bool isHighlyConfidential, bool isFrequentlyUsed, bool isSharedWithThirdParties)
        {
            if (file == null)
            {
                return BadRequest("Arquivo obrigatório.");
            }

            try
            {
                // Gerar o prompt com base nas checkbox e no tipo de arquivo
                var prompt = GeneratePrompt(file.FileName, isHighlyConfidential, isFrequentlyUsed, isSharedWithThirdParties);

                // Obter o método recomendado pela OpenAI
                var recommendedMethod = await GetEncryptionMethodFromOpenAI(prompt);

                // Aplicar a criptografia com o método recomendado
                var encryptedFileStream = _encryptionContext.Encrypt(file, recommendedMethod);

                // Adicionar ".encrypted" ao final do nome do arquivo original
                var encryptedFileName = $"{file.FileName}.encrypted";

                Response.Headers.Add("X-Encryption-Method", recommendedMethod);

                // Retornar o arquivo criptografado
                return File(encryptedFileStream, "application/octet-stream", encryptedFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no processamento: {ex.Message}");
            }
        }

        #region Private Methods
        private static string GeneratePrompt(string fileName, bool isHighlyConfidential, bool isFrequentlyUsed, bool isSharedWithThirdParties)
        {
            var fileType = Path.GetExtension(fileName).ToLowerInvariant();

            var prompt = $"Você é um especialista em criptografia. Determine a melhor criptografia para proteger um arquivo do tipo {fileType}.";

            if (isHighlyConfidential || isFrequentlyUsed || isSharedWithThirdParties)
            {
                prompt += " Baseie sua decisão nos seguintes fatores:";

                if (isHighlyConfidential)
                    prompt += " - O arquivo contém informações altamente confidenciais.";
                if (isFrequentlyUsed)
                    prompt += " - O arquivo é acessado frequentemente, priorize velocidade.";
                if (isSharedWithThirdParties)
                    prompt += " - O arquivo será compartilhado com terceiros, o que exige segurança adicional.";
            }
            else
            {
                prompt += " Não há prioridades específicas para este arquivo. Escolha um método de criptografia padrão que seja equilibrado entre segurança e desempenho.";
            }

            prompt += " Exemplos de resposta (únicas opções disponiveis): 'Use AES 128' para priorizar agilidade; 'Use AES 256' para alta segurança; 'Use ChaCha20' para compartilhamento seguro.";
            prompt += " Responda apenas com o nome do algoritmo (ex.: 'Use AES 128'). Não escreva explicações ou justifique a escolha. Escolha apenas um entre os três fornecidos.";

            return prompt;
        }

        private async Task<string> GetEncryptionMethodFromOpenAI(string prompt)
        {
            try
            {
                // Enviar o prompt para o ChatClient
                ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);

                // Extrair a resposta do GPT
                return ParseEncryptionMethod(completion.Content[0].Text);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter recomendação da OpenAI: {ex.Message}");
            }
        }

        private static string ParseEncryptionMethod(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return "NONE";

            response = response.Trim().ToUpper();
            if (response.Contains("AES 128")) return "AES 128";
            if (response.Contains("AES 256")) return "AES 256";
            if (response.Contains("RSA")) return "RSA";

            return "NONE";
        }

        #endregion
    }
}
