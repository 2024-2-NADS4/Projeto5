using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Encrypt;

namespace WatchDog.Maui.API.Services.Encrypt
{
    public class Aes256EncryptionStrategy : IEncryptionStrategy
    {
        public Stream Encrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var aes = Aes.Create();

            aes.KeySize = 256;
            aes.Key = Convert.FromBase64String("7DcBN2m71xc78kILxWASToTohBv/c59OpzNPy5vEB38=");
            aes.IV = Convert.FromBase64String("4xIrMQw8/N0aV85jQaDDUQ==");

            using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var inputStream = file.OpenReadStream();
            inputStream.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();

            return new MemoryStream(outputStream.ToArray());
        }

    }
}
