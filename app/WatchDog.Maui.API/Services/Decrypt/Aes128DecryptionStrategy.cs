using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Decrypt;

namespace WatchDog.Maui.API.Services.Decrypt
{
    public class Aes128DecryptionStrategy : IDecryptionStrategy
    {
        public Stream Decrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var aes = Aes.Create();

            // Configurar para AES com chave de 128 bits
            aes.KeySize = 128;
            aes.Key = Convert.FromBase64String("wO9sKk8Jd8Wqj8hQXHTiYQ==");
            aes.IV = Convert.FromBase64String("4xIrMQw8/N0aV85jQaDDUQ==");

            using var cryptoStream = new CryptoStream(file.OpenReadStream(), aes.CreateDecryptor(), CryptoStreamMode.Read);
            cryptoStream.CopyTo(outputStream);

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
