using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Texts.Api.Controllers;

public sealed record GeneratedTextDto(string Text);

[Route("api/texts")]
public sealed class TextsController : ControllerBase
{
    private readonly ITextRetriever _textRetriever;

    public TextsController(ITextRetriever textRetriever)
    {
        _textRetriever = textRetriever;
    }

    [HttpGet]
    public async ValueTask<GeneratedTextDto> GenerateText()
    {
        var text = await _textRetriever.RetrieveTextAsync();

        return new GeneratedTextDto(text);
    }
}
