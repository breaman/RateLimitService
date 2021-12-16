using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace StokesTest
{
    public class RedisCacheRateLimitLease : RateLimitLease
    {
        public override bool IsAcquired { get; }

        public override IEnumerable<string> MetadataNames => throw new NotImplementedException();

        public RedisCacheRateLimitLease(bool isAcquired)
        {
            IsAcquired = isAcquired;
        }
        public override bool TryGetMetadata(string metadataName, out object metadata)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose();
        }
    }
}