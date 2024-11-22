using Microsoft.AspNetCore.Mvc;
using WatchDog.Maui.API.Services.Decrypt;

namespace WatchDog.Maui.API.Controllers.Decrypt
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecryptController : ControllerBase
    {
        private readonly DecryptionStrategyContext _decryptionContext;

        public DecryptController()
        {
            _decryptionContext = new DecryptionStrategyContext();
        }

        [HttpPost("decrypt")]
        public IActionResult DecryptFile([FromForm] IFormFile file, [FromForm] string encryptionMethod)
        {
            if (file == null)
            {
                return BadRequest("Arquivo obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(encryptionMethod))
            {
                return BadRequest("Método de criptografia é obrigatório.");
            }

            try
            {
                // Aplicar a descriptografia com o método especificado
                Stream decryptedFileStream = _decryptionContext.Decrypt(file, encryptionMethod);

                // Retornar o arquivo descriptografado
                return File(decryptedFileStream, "application/octet-stream", $"{file.FileName.Replace(".encrypted", "")}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no processamento: {ex.Message}");
            }
        }
    }
}
