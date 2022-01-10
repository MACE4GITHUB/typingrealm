using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Library.Api.Client;

public interface ISentencesClient
{
    ValueTask<IEnumerable<SentenceDto>> GetRandomSentencesAsync(
        int count, int consecutiveCount,
        EndpointAuthentication? authentication = null,
        CancellationToken cancellationToken = default);
}

public sealed class SentencesClient : ISentencesClient
{
    public static readonly string ServiceName = ServiceConfiguration.ServiceName;
    public static readonly string RoutePrefix = ServiceConfiguration.SentencesApiPrefix;
    private readonly IServiceClient _serviceClient;

    public SentencesClient(IServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public ValueTask<IEnumerable<SentenceDto>> GetRandomSentencesAsync(
        int count, int consecutiveCount,
        EndpointAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        if (authentication == null)
            authentication = EndpointAuthentication.Service;

        return _serviceClient.GetAsync<IEnumerable<SentenceDto>>(
            ServiceName,
            $"{RoutePrefix}?count={count}&consecutiveCount={consecutiveCount}",
            authentication,
            cancellationToken);
    }
}
