using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Profiles.Api.Client
{
    public sealed class ServiceToServiceClient : IServiceToServiceClient
    {
        public static readonly string RoutePrefix = "api/local-auth";
        private readonly IServiceClient _serviceClient;

        public ServiceToServiceClient(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<DateTime> GetServiceToServiceCurrentDateAsync(CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<DateTime>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/service",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

        public ValueTask<DateTime> GetProfileToServiceCurrentDateAsync(CancellationToken cancellationToken)
        {
            return _serviceClient.GetAsync<DateTime>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/profile",
                EndpointAuthenticationType.Profile,
                cancellationToken);
        }
    }
}
