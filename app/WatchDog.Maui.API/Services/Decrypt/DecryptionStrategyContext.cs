using WatchDog.Maui.API.Interfaces.Decrypt;
using WatchDog.Maui.API.Services.Encrypt;

namespace WatchDog.Maui.API.Services.Decrypt
{
    public class DecryptionStrategyContext
    {
        private readonly Dictionary<string, IDecryptionStrategy> _strategies;

        public DecryptionStrategyContext()
        {
            _strategies = new Dictionary<string, IDecryptionStrategy>
            {
                { "AES 128", new Aes128DecryptionStrategy() },
                { "AES 256", new Aes256DecryptionStrategy() },
                { "TRIPLEDES", new TripleDesDecryptionStrategy() }
            };
        }

        public Stream Decrypt(IFormFile file, string method)
        {
            if (_strategies.ContainsKey(method.ToUpper()))
            {
                return _strategies[method.ToUpper()].Decrypt(file);
            }
            throw new InvalidOperationException("Método de descriptografia desconhecido.");
        }
    }

}
