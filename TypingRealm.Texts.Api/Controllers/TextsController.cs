using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Texts.Api.Controllers;

public sealed record GeneratedText(
    string Value);

[Route("api/[controller]")]
public sealed class TextsController : TyrController
{
    private readonly TextGeneratorResolver _textGeneratorResolver;

    public TextsController(TextGeneratorResolver textGeneratorResolver)
    {
        _textGeneratorResolver = textGeneratorResolver;
    }

    [HttpPost]
    [Route("{language}/generate")]
    public async ValueTask<ActionResult<GeneratedText>> GenerateText(string language, TextGenerationConfiguration configuration)
    {
        var textGenerator = _textGeneratorResolver(language);

        var text = await textGenerator.GenerateTextAsync(configuration);

        return new GeneratedText(text);
    }
}
