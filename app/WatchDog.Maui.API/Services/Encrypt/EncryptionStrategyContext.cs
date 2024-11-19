using WatchDog.Maui.API.Interfaces.Encrypt;

namespace WatchDog.Maui.API.Services.Encrypt
{
    public class EncryptionStrategyContext
    {
        private readonly Dictionary<string, IEncryptionStrategy> _strategies;

        public EncryptionStrategyContext()
        {
            _strategies = new Dictionary<string, IEncryptionStrategy>
            {
                { "AES 128", new Aes128EncryptionStrategy() },
                { "AES 256", new Aes256EncryptionStrategy() },
                { "ChaCha20", new ChaCha20EncryptionStrategy() }
            };
        }

        public Stream Encrypt(IFormFile file, string method)
        {
            if (_strategies.ContainsKey(method.ToUpper()))
            {
                return _strategies[method.ToUpper()].Encrypt(file);
            }
            throw new InvalidOperationException("Método de criptografia desconhecido.");
        }
    }

}
