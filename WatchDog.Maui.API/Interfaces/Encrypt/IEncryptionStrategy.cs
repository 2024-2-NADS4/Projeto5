namespace WatchDog.Maui.API.Interfaces.Encrypt
{
    public interface IEncryptionStrategy
    {
        Stream Encrypt(IFormFile file);
    }

}
