using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Texts.Api.Controllers;

public sealed record GeneratedText(
    string Value);

[Route("api/[controller]")]
public sealed class TextsController : TyrController
{
    private readonly TextRetrieverResolver _textGeneratorResolver;

    public TextsController(TextRetrieverResolver textGeneratorResolver)
    {
        _textGeneratorResolver = textGeneratorResolver;
    }

    [HttpPost]
    [Route("{language}/generate")]
    public async ValueTask<ActionResult<GeneratedText>> GenerateText(string language, TextGenerationConfiguration configuration)
    {
        _ = configuration;
        var textGenerator = _textGeneratorResolver(language);

        var text = await textGenerator.RetrieveTextAsync();

        return new GeneratedText(text);
    }
}
