namespace WatchDog.Maui.API.Interfaces.Decrypt
{
    public interface IDecryptionStrategy
    {
        Stream Decrypt(IFormFile file);
    }

}
