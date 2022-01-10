using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;
using TypingRealm.Data.Resources;

namespace TypingRealm.Data.Api.Client
{
    public sealed class LocationsClient : ILocationsClient
    {
        public static readonly string RoutePrefix = "api/locations";
        private readonly IServiceClient _serviceClient;

        public LocationsClient(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<Location> GetLocationAsync(string locationId, CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<Location>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{locationId}",
                EndpointAuthentication.Profile,
                cancellationToken);
        }
    }
}
