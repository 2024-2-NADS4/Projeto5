using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Decrypt;

namespace WatchDog.Maui.API.Services.Decrypt
{
    public class TripleDesDecryptionStrategy : IDecryptionStrategy
    {
        private readonly byte[] _key = Convert.FromBase64String("fEL2oge/8drVzun+PyEWu5/X5qIWZE8k");
        private readonly byte[] _iv = Convert.FromBase64String("+Zffgn8G9YQ=");

        public Stream Decrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var tripleDes = TripleDES.Create();

            tripleDes.Key = _key;
            tripleDes.IV = _iv;

            using var cryptoStream = new CryptoStream(file.OpenReadStream(), tripleDes.CreateDecryptor(), CryptoStreamMode.Read);
            cryptoStream.CopyTo(outputStream);

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
