using System;
using System.Threading;
using System.Threading.Tasks;

namespace TypingRealm.Profiles.Api.Client
{
    /// <summary>
    /// Client for testing communication between services. Will be moved to ServiceDiscovery once it's implemented.
    /// </summary>
    public interface IServiceToServiceClient
    {
        ValueTask<DateTime> GetServiceToServiceCurrentDateAsync(CancellationToken cancellationToken);
        ValueTask<DateTime> GetProfileToServiceCurrentDateAsync(CancellationToken cancellationToken);
    }
}
