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
        using IHost host = Host.CreateDefaultBuilder(args)
                                .ConfigureServices((_, services) =>
                                {
                                    services.AddScoped<IRateLimitingService, RateLimitingService>();
                                    services.AddScoped<AggregatedRateLimiter<string>, RedisCacheRateLimiter>();
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

        for(int i = 0; i < 15; i++)
        {
            Console.WriteLine(rateLimiting.CheckLockoutState(ServiceEnum.Login));
        }

        for(int i = 0; i < 15; i++)
        {
            Console.WriteLine(rateLimiting.CheckLockoutState(ServiceEnum.PushNotification));
        }
    }
}