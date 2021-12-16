namespace StokesTest
{
    public interface IRateLimitingService
    {
        bool CheckLockoutState(ServiceEnum service);
        // void ReadFromCache();
        // void WriteToCache();
    }

    public enum ServiceEnum
    {
        PushNotification,
        Login
    }
}