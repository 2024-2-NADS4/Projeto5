using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Decrypt;

namespace WatchDog.Maui.API.Services.Decrypt
{
    public class Aes256DecryptionStrategy : IDecryptionStrategy
    {
        public Stream Decrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var aes = Aes.Create();

            aes.KeySize = 256;
            aes.Key = Convert.FromBase64String("7DcBN2m71xc78kILxWASToTohBv/c59OpzNPy5vEB38=");
            aes.IV = Convert.FromBase64String("4xIrMQw8/N0aV85jQaDDUQ==");

            using var cryptoStream = new CryptoStream(file.OpenReadStream(), aes.CreateDecryptor(), CryptoStreamMode.Read);
            cryptoStream.CopyTo(outputStream);

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
