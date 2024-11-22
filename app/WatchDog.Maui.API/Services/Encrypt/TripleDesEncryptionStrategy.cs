using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Encrypt;

namespace WatchDog.Maui.API.Services.Encrypt
{
    public class TripleDesEncryptionStrategy : IEncryptionStrategy
    {
        private readonly byte[] _key = Convert.FromBase64String("fEL2oge/8drVzun+PyEWu5/X5qIWZE8k");
        private readonly byte[] _iv = Convert.FromBase64String("+Zffgn8G9YQ=");

        public Stream Encrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var tripleDes = TripleDES.Create();

            tripleDes.Key = _key;
            tripleDes.IV = _iv;

            using var cryptoStream = new CryptoStream(outputStream, tripleDes.CreateEncryptor(), CryptoStreamMode.Write);
            using var inputStream = file.OpenReadStream();
            inputStream.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
