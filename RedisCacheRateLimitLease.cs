using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace StokesTest
{
    public class RedisCacheRateLimitLease : RateLimitLease
    {
        IDictionary<string, object> Metadata { get; }

        public RedisCacheRateLimitLease(bool isAcquired, IDictionary<string, object> metadata) {
            IsAcquired = isAcquired;
            Metadata = metadata;
        }

        public override bool TryGetMetadata(string metadataName, out object metadata) {
            if (Metadata != null && Metadata.ContainsKey(metadataName))
            {
                metadata = Metadata[metadataName];
                return true;
            }

            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing) {
            throw new System.NotImplementedException();
        }

        public override bool IsAcquired { get; }

        public override IEnumerable<string> MetadataNames => Metadata?.Keys;
    }
}