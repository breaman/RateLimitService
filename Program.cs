using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using StokesTest;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;

public class Program
{
    public static void Main(string[] args)
    {
        // const string CacheKeyPrefix = "{0}-lockout-state:subject-id";
        // string stokes = "Stokes";
        // string something = "something";
        // var key = string.Format($"{CacheKeyPrefix}-{stokes.ToLower()}", something);

        // Console.WriteLine(key);
        using IHost host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((_, services) =>
                                {
                                    services.AddScoped<IRateLimitingService, RateLimitingService>();
                                    services.AddScoped<AggregatedRateLimiter<RateLimitInput>, RedisCacheRateLimiter>();
                                    services.AddStackExchangeRedisCache(options =>
                                    {
                                        options.Configuration = "localhost:6379";
                                        options.InstanceName = "redisCache";
                                    });
                                })
                                .Build();

        DoSomethingWithRateLimiting(host.Services);
    }

    static void DoSomethingWithRateLimiting(IServiceProvider services)
    {
        using IServiceScope serviceScope = services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        IRateLimitingService rateLimiting = provider.GetRequiredService<IRateLimitingService>();
        int failedAttempts = 0;
        DateTimeOffset lockedOutUntil;
        int pushNotificationCount = 0;

        bool successful = true;
        RedisCacheRateLimitLease result = null;

        // esult = rateLimiting.CheckLockoutState(RateLimitTypeEnum.Login, successful);
        // result.TryGetMetadata<int>(new MetadataName<int>("FailedAttemptCount"), out failedAttempts);
        // result.TryGetMetadata<DateTimeOffset>(new MetadataName<DateTimeOffset>("LockedOutUntil"), out lockedOutUntil);
        // Console.WriteLine($"{successful} - {failedAttempts} - {result.IsAcquired} - {lockedOutUntil}");

        for(int i = 0; i < 15; i++)
        {
            successful = false;
            // if the login failed
            result = rateLimiting.CheckLockoutState(RateLimitTypeEnum.Login, successful);
            result.TryGetMetadata<int>(new MetadataName<int>("FailedAttemptCount"), out failedAttempts);
            result.TryGetMetadata<DateTimeOffset>(new MetadataName<DateTimeOffset>("LockedOutUntil"), out lockedOutUntil);
            // If IsAquired is true and Login succeeded, then result would be IsNotAllowed = false and IsLockedOut = false
            // if IsAquired is true, but login failed, then result would be IsNotAllowed = true
            // if IsAquired is false and login failed, then result would be IsLockedOut = true
            Console.WriteLine($"{successful} - {failedAttempts} - {result.IsAcquired} - {lockedOutUntil}");
        }

        // for(int i = 0; i < 15; i++)
        // {
        //     var result = rateLimiting.CheckLockoutState(RateLimitTypeEnum.PushNotification, false);
        //     result.TryGetMetadata<int>(new MetadataName<int>("FailedLoginCount"), out pushNotificationCount);
        //     Console.WriteLine($"{pushNotificationCount} - {result.IsAcquired}");
        // }
    }
}