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

            // Recuperar a API Key do appsettings.json com validação
            string? apiKey = configuration.GetValue<string>("OpenAI:ApiKey");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("A chave da API para OpenAI ('OpenAI:ApiKey') não foi encontrada ou está vazia.");
            }

            // Configurar o ChatClient com a API Key
            _chatClient = new ChatClient(
                model: "gpt-4o-mini",
                apiKey: apiKey
            );
        }

        [HttpPost("encrypt")]
        public async Task<IActionResult> EncryptFile([FromForm] IFormFile file, [FromForm] bool isHighlyConfidential, [FromForm] bool isFrequentlyUsed, [FromForm] bool isSharedWithThirdParties)
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

                Response.Headers["X-Encryption-Method"] = recommendedMethod;

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

            var prompt = $"Você é um especialista em criptografia. Escolha o melhor algoritmo para proteger um arquivo do tipo '{fileType}'.";

            if (isHighlyConfidential || isFrequentlyUsed || isSharedWithThirdParties)
            {
                prompt += " Considere os seguintes fatores:";

                if (isHighlyConfidential)
                    prompt += " - Contém informações altamente confidenciais (priorize alta segurança).";
                if (isFrequentlyUsed)
                    prompt += " - É acessado frequentemente (priorize velocidade).";
                if (isSharedWithThirdParties)
                    prompt += " - Será compartilhado com terceiros. Para este caso, priorize compatibilidade e segurança no compartilhamento. **Recomenda-se usar TripleDES como a melhor opção.**";
            }
            else
            {
                prompt += " Não há fatores especiais; escolha um algoritmo equilibrado entre segurança e desempenho.";
            }

            prompt += " Escolha apenas uma das opções: 'Use AES 128' para agilidade, 'Use AES 256' para alta segurança, ou 'Use TripleDES' para compatibilidade e segurança no compartilhamento.";
            prompt += " Responda somente com o nome do algoritmo (ex.: 'Use AES 128'). Não forneça explicações adicionais.";

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
            if (response.Contains("TRIPLEDES")) return "TripleDES";

            return "NONE";
        }

        #endregion
    }
}
