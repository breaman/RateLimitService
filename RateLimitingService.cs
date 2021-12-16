using System;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;

namespace StokesTest
{
    public class RateLimitingService : IRateLimitingService
    {
        public AggregatedRateLimiter<string> RateLimiter { get; }
        public RateLimitingService(AggregatedRateLimiter<string> rateLimiter)
        {
            RateLimiter = rateLimiter;
        }

        public bool CheckLockoutState(ServiceEnum service)
        {
            bool isLockedOut = true;
            switch (service)
            {
                case ServiceEnum.Login:
                case ServiceEnum.PushNotification:
                    isLockedOut = GetStrictLockoutState(service);
                    break;
            }

            return isLockedOut;
        }

        private bool GetStrictLockoutState(ServiceEnum service)
        {
            var result = RateLimiter.Acquire(service.ToString(), 1);
            return result.IsAcquired;
        }
    }
}