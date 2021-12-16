using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace StokesTest
{
    public class RedisCacheRateLimiter : AggregatedRateLimiter<string>
    {
        private IDistributedCache Cache { get; }
        public RedisCacheRateLimiter(IDistributedCache cache)
        {
            Cache = cache;
        }

        // instead of being hard coded, this should really return a value that is passed in dynamically
        // Looking at doing this through the Options API, just was exploring the RateLimiter API at the moment
        public override int AvailablePermits(string objectKey)
        {
            switch(objectKey)
            {
                case "Login":
                    return 10;
                case "PushNotification":
                    return 4;
                default:
                    return 3;
            }
        }

        public override RateLimitLease Acquire(string objectKey, int permitCount)
        {
            var tempString = Cache.GetString(objectKey);

            int count = string.IsNullOrWhiteSpace(tempString) ? 0 : Convert.ToInt32(tempString);
            
            bool retValue = count < AvailablePermits(objectKey);

            count += permitCount;
            Cache.SetString(objectKey, count.ToString());

            return new RedisCacheRateLimitLease(retValue);
        }

        public override ValueTask<RateLimitLease> WaitAsync(string resourceID, int requestedCount, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}