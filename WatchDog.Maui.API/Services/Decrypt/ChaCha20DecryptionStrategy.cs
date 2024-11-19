using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Decrypt;

namespace WatchDog.Maui.API.Services.Decrypt
{
    public class ChaCha20DecryptionStrategy : IDecryptionStrategy
    {
        private readonly byte[] _key;

        public ChaCha20DecryptionStrategy()
        {
            // Recuperar a chave associada ao arquivo
            _key = GetKeyForFile();
        }

        public Stream Decrypt(IFormFile file)
        {
            using var inputStream = file.OpenReadStream();
            using var outputStream = new MemoryStream();
            using var chacha20 = new ChaCha20Poly1305(_key);

            // Ler o nonce, tag e dados criptografados
            var nonce = new byte[12];
            inputStream.Read(nonce, 0, nonce.Length);

            var tag = new byte[16];
            inputStream.Read(tag, 0, tag.Length);

            var ciphertext = new byte[inputStream.Length - nonce.Length - tag.Length];
            inputStream.Read(ciphertext, 0, ciphertext.Length);

            // Espaço para os dados descriptografados
            var plaintext = new byte[ciphertext.Length];

            // Descriptografar os dados
            chacha20.Decrypt(nonce, ciphertext, tag, plaintext);

            // Escrever os dados descriptografados no stream de saída
            outputStream.Write(plaintext, 0, plaintext.Length);
            outputStream.Position = 0;

            return outputStream;
        }

        private byte[] GetKeyForFile()
        {
            // Implemente o método para recuperar a chave associada ao arquivo
            return RandomNumberGenerator.GetBytes(32); // Substitua pelo método real
        }
    }
}
