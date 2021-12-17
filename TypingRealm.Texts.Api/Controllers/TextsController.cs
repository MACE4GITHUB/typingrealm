using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;

namespace TypingRealm.Texts.Api.Controllers;

public sealed record GeneratedText(
    string Value);

public sealed record TextGenerationConfiguration(
    int length);

[Route("api/[controller]")]
public sealed class TextsController : TyrController
{
    [HttpPost]
    [Route("generate")]
    public ValueTask<ActionResult<GeneratedText>> GenerateText(TextGenerationConfiguration configuration)
    {
        _ = configuration;

        return new ValueTask<ActionResult<GeneratedText>>(
            Ok(new GeneratedText("Some text.")));
    }
}
