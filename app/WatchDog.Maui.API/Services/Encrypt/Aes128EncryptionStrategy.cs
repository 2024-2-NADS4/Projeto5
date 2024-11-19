using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Encrypt;

namespace WatchDog.Maui.API.Services.Encrypt
{
    public class Aes128EncryptionStrategy : IEncryptionStrategy
    {
        public Stream Encrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var aes = Aes.Create();

            // Configurar para AES com chave de 128 bits
            aes.KeySize = 128;
            aes.Key = Convert.FromBase64String("wO9sKk8Jd8Wqj8hQXHTiYQ==");
            aes.IV = Convert.FromBase64String("4xIrMQw8/N0aV85jQaDDUQ==");

            using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var inputStream = file.OpenReadStream();
            inputStream.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
