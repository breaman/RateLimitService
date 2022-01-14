namespace StokesTest
{
    public interface IRateLimitingService
    {
        RedisCacheRateLimitLease CheckLockoutState(RateLimitTypeEnum rateLimitType, bool loginSuccessful);
        // void ReadFromCache();
        // void WriteToCache();
    }
}