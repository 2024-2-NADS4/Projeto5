using System.Security.Cryptography;
using WatchDog.Maui.API.Interfaces.Encrypt;

namespace WatchDog.Maui.API.Services.Encrypt
{
    public class ChaCha20EncryptionStrategy : IEncryptionStrategy
    {
        private readonly byte[] _key;
        private readonly byte[] _nonce;

        public ChaCha20EncryptionStrategy()
        {
            // Gerar uma chave de 256 bits (32 bytes) e um nonce (12 bytes) para ChaCha20
            _key = RandomNumberGenerator.GetBytes(32); // Chave
            _nonce = RandomNumberGenerator.GetBytes(12); // Nonce

            // Aqui você pode salvar a _key e _nonce para descriptografia futura
            SaveKeyAndNonce(_key, _nonce);
        }

        public Stream Encrypt(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using var chacha20 = new ChaCha20Poly1305(_key); // ChaCha20 com autenticação

            // Transformar o arquivo em bytes
            using var inputStream = file.OpenReadStream();
            var fileData = new byte[inputStream.Length];
            inputStream.Read(fileData, 0, fileData.Length);

            // Espaço para o ciphertext e tag
            var ciphertext = new byte[fileData.Length];
            var tag = new byte[16];

            // Criptografar os dados
            chacha20.Encrypt(_nonce, fileData, ciphertext, tag);

            // Escrever o nonce, tag e ciphertext no stream de saída
            outputStream.Write(_nonce, 0, _nonce.Length); // Nonce (12 bytes)
            outputStream.Write(tag, 0, tag.Length);       // Tag (16 bytes)
            outputStream.Write(ciphertext, 0, ciphertext.Length); // Dados criptografados
            outputStream.Position = 0;

            return outputStream;
        }

        private void SaveKeyAndNonce(byte[] key, byte[] nonce)
        {
            // Implemente o método para salvar a chave e o nonce associados ao arquivo
        }
    }
}
