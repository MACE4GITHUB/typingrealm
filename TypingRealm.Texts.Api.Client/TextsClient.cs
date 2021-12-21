using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Communication;

namespace TypingRealm.Texts.Api.Client;

public interface ITextsClient
{
    ValueTask<GeneratedText> GenerateTextAsync(TextGenerationConfiguration configuration, CancellationToken cancellationToken);
}

public sealed class TextsClient : ITextsClient
{
    public static readonly string ServiceName = "texts";
    public static readonly string RoutePrefix = "api/texts";
    private readonly IServiceClient _serviceClient;

    public TextsClient(IServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public ValueTask<GeneratedText> GenerateTextAsync(TextGenerationConfiguration configuration, CancellationToken cancellationToken)
    {
        return _serviceClient.PostAsync<TextGenerationConfiguration, GeneratedText>(
            ServiceName,
            $"{RoutePrefix}/generate",
            EndpointAuthenticationType.Service, // TODO: Make sure this can be called by Profile as well, but still automatically get token when service calls it.
            configuration,
            cancellationToken);
    }
}
