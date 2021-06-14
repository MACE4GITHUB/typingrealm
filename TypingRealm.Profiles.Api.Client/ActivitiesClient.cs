using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Api.Client
{
    public sealed class ActivitiesClient : IActivitiesClient
    {
        public static readonly string RoutePrefix = "api/activities";
        private readonly IServiceClient _serviceClient;

        public ActivitiesClient(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public ValueTask<ActivityResource?> GetCurrentActivityAsync(string characterId, CancellationToken cancellationToken)
        {
            // TODO: If controller returnsNotFound - do not fail but return null.
            return _serviceClient.GetAsync<ActivityResource?>(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/current/{characterId}",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }

        public ValueTask StartActivityAsync(ActivityResource activityResource, CancellationToken cancellationToken)
        {
            return _serviceClient.PostAsync(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}",
                EndpointAuthenticationType.Service,
                activityResource,
                cancellationToken);
        }

        public ValueTask FinishActivityAsync(string activityId, CancellationToken cancellationToken)
        {
            return _serviceClient.DeleteAsync(
                ServiceConfiguration.ServiceName,
                $"{RoutePrefix}/{activityId}",
                EndpointAuthenticationType.Service,
                cancellationToken);
        }
    }
}
