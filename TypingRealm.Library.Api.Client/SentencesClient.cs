using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;
using TypingRealm.Library.Sentences;

namespace TypingRealm.Library.Api.Client;

public interface ISentencesClient
{
    ValueTask<IEnumerable<SentenceDto>> GetSentencesAsync(
        SentencesRequest request,
        string language, // TODO: Use default "en" value.
        EndpointAuthentication? authentication = null,
        CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<string>> GetWordsAsync(
        WordsRequest request,
        string language, // TODO: Use default "en" value.
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

    public ValueTask<IEnumerable<SentenceDto>> GetSentencesAsync(
        SentencesRequest request,
        string language,
        EndpointAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        if (authentication == null)
            authentication = EndpointAuthentication.Service;

        return _serviceClient.PostAsync<SentencesRequest, IEnumerable<SentenceDto>>(
            ServiceName,
            $"{RoutePrefix}?language={language}",
            authentication,
            request,
            cancellationToken);
    }

    public ValueTask<IEnumerable<string>> GetWordsAsync(WordsRequest request, string language, EndpointAuthentication? authentication = null, CancellationToken cancellationToken = default)
    {
        if (authentication == null)
            authentication = EndpointAuthentication.Service;

        return _serviceClient.PostAsync<WordsRequest, IEnumerable<string>>(
            ServiceName,
            $"{RoutePrefix}/words?language={language}",
            authentication,
            request,
            cancellationToken);
    }
}
