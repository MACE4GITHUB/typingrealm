﻿using System.Collections.Generic;
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
    public async ValueTask<ActionResult<GeneratedText>> GenerateText(TextGenerationConfigurationDto configuration)
    {
        if (configuration.Contains == null)
            return await _textGenerator.GenerateTextAsync(
                TextGenerationConfiguration.Standard(
                    new(configuration.Language)));

        return await _textGenerator.GenerateTextAsync(
            TextGenerationConfiguration.SelfImprovement(
                new(configuration.Language),
                shouldContain: configuration.Contains));
    }
}

public sealed record TextGenerationConfigurationDto(string Language, IEnumerable<string>? Contains = null);
