using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Texts.Api.Client;

namespace TypingRealm.Texts.Api.Controllers;

[Route("api/[controller]")]
public sealed class TextsController : TyrController
{
    private readonly ITextGenerator _textGenerator;

    public TextsController(ITextGenerator textGenerator)
    {
        _textGenerator = textGenerator;
    }

    [HttpPost]
    [Route("generate")]
    public async ValueTask<ActionResult<GeneratedText>> GenerateText([FromBody] TextGenerationConfiguration configuration)
    {
        var text = await _textGenerator.GenerateTextAsync(configuration);

        return new GeneratedText(text);
    }
}
