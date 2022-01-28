using System;

namespace TypingRealm.Communication;

public interface IDistributedLockProvider
{
    /// <summary>
    /// Gets distributed lock for requested name. The lock is scoped to the
    /// service so that multiple instances of the service can sync up with each
    /// other, it's not distributed between different services.
    /// In-memory implementation works withing one process, so every instance of
    /// every service would get it's own lock.
    /// </summary>
    ILock AcquireDistributedLock(string name, TimeSpan expiration);
}
