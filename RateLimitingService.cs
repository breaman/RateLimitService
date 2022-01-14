using System;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;

namespace StokesTest
{
    public class RateLimitingService : IRateLimitingService
    {
        public AggregatedRateLimiter<RateLimitInput> RateLimiter { get; }
        public RateLimitingService(AggregatedRateLimiter<RateLimitInput> rateLimiter)
        {
            RateLimiter = rateLimiter;
        }

        public RedisCacheRateLimitLease CheckLockoutState(RateLimitTypeEnum rateLimitType, bool loginSuccessful)
        {
            // need to somehow know what attempt number this was
            RedisCacheRateLimitLease redisCacheRateLimiterLease = null;
            switch (rateLimitType)
            {
                case RateLimitTypeEnum.Login:
                case RateLimitTypeEnum.PushNotification:
                    redisCacheRateLimiterLease = GetStrictLockoutState(rateLimitType, loginSuccessful ? 0 : 1);
                    break;
            }

            return redisCacheRateLimiterLease;
        }

        private RedisCacheRateLimitLease GetStrictLockoutState(RateLimitTypeEnum rateLimitType, int attemptNumber)
        {
            var rateLimitInput = new RateLimitInput { RateLimitType = rateLimitType, SubjectId = "2" };
            return RateLimiter.Acquire(rateLimitInput, attemptNumber) as RedisCacheRateLimitLease;
        }
    }
}