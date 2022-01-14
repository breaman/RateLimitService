using System;

namespace StokesTest
{
    public class FixedLockoutState
    {
        public int FailedAttemptCount { get; set; }
        public DateTimeOffset LockedOutUntil { get; set; }
    }
}