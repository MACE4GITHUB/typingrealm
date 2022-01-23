using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Texts.Api.Client;

public interface ITextsClient
{
    ValueTask<GeneratedText> GenerateTextAsync(
        TextGenerationConfiguration configuration,
        EndpointAuthentication authentication,
        CancellationToken cancellationToken);
}

public sealed class TextsClient : ITextsClient
{
    public static readonly string ServiceName = ServiceConfiguration.ServiceName;
    public static readonly string RoutePrefix = ServiceConfiguration.TextsApiPrefix;
    private readonly IServiceClient _serviceClient;

    public TextsClient(IServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public ValueTask<GeneratedText> GenerateTextAsync(
        TextGenerationConfiguration configuration,
        EndpointAuthentication authentication,
        CancellationToken cancellationToken)
    {
        return _serviceClient.PostAsync<TextGenerationConfigurationDto, GeneratedText>(
            ServiceName,
            $"{RoutePrefix}/generate",
            authentication,
            TextGenerationConfigurationDto.ToDto(configuration),
            cancellationToken);
    }
}
