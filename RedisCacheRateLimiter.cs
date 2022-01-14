using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace StokesTest
{
    public class RedisCacheRateLimiter : AggregatedRateLimiter<RateLimitInput>
    {
        const string CacheKeyPrefix = "{0}-lockout-state:subject-id";
        private IDistributedCache Cache { get; }
        public RedisCacheRateLimiter(IDistributedCache cache)
        {
            Cache = cache;
        }

        // instead of being hard coded, this should really return a value that is passed in dynamically
        // Looking at doing this through the Options API, just was exploring the RateLimiter API at the moment
        public override int AvailablePermits(RateLimitInput rateLimitInput)
        {
            switch(rateLimitInput.RateLimitType)
            {
                case RateLimitTypeEnum.Login:
                    return 10;
                case RateLimitTypeEnum.PushNotification:
                    return 4;
                default:
                    return 3;
            }
        }

        public override RateLimitLease Acquire(RateLimitInput rateLimitInput, int pointsAgainstTotal)
        {
            var key = string.Format($"{CacheKeyPrefix}-{rateLimitInput.SubjectId.ToString()}", rateLimitInput.RateLimitType.ToString().ToLower());
            RedisCacheRateLimitLease retValue = null;
            if (pointsAgainstTotal == 0)
            {
                Cache.RemoveAsync(key).Wait();
                retValue = new RedisCacheRateLimitLease(true, null);
            }
            else
            {
                var stateJson = Cache.GetStringAsync(key).Result;
                var deserializedState = string.IsNullOrWhiteSpace(stateJson)
                    ? new FixedLockoutState()
                    : JsonConvert.DeserializeObject<FixedLockoutState>(stateJson);

                deserializedState.FailedAttemptCount += pointsAgainstTotal;

                var canAquire = deserializedState.FailedAttemptCount < AvailablePermits(rateLimitInput);
                if (!canAquire && deserializedState.LockedOutUntil == DateTimeOffset.MinValue)
                {
                    deserializedState.LockedOutUntil = DateTime.UtcNow.AddSeconds(30);
                }

                retValue = new RedisCacheRateLimitLease(canAquire, deserializedState.AsDictionary());

                // after doing logic to know if we can acquire or not, we need to persist to the cache again
                if (deserializedState.LockedOutUntil > DateTimeOffset.MinValue)
                {
                    Cache.SetStringAsync(key, JsonConvert.SerializeObject(deserializedState),
                    new DistributedCacheEntryOptions {AbsoluteExpiration = deserializedState.LockedOutUntil}).Wait();
                }
                else
                {
                    Cache.SetStringAsync(key, JsonConvert.SerializeObject(deserializedState)).Wait();
                }
            }

            return retValue;
        }

        public override ValueTask<RateLimitLease> WaitAsync(RateLimitInput rateLimitInput, int requestedCount, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}