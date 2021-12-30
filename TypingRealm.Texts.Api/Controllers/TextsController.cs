using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Texts.Api.Controllers;

[Route(ServiceConfiguration.TextsApiPrefix)]
public sealed class TextsController : TyrController
{
    private readonly ITextGenerator _textGenerator;

    public TextsController(ITextGenerator textGenerator)
    {
        _textGenerator = textGenerator;
    }

    [HttpPost]
    [Route("generate")]
    public async ValueTask<ActionResult<GeneratedText>> GenerateText(TextGenerationConfiguration configuration)
    {
        var text = await _textGenerator.GenerateTextAsync(configuration);

        return new GeneratedText(text);
    }
}
