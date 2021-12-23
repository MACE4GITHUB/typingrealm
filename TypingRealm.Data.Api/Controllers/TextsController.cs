using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Communication;
using TypingRealm.Hosting;
using TypingRealm.Profiles;
using TypingRealm.Texts.Api.Client;
using TypingRealm.Typing;

namespace TypingRealm.Data.Api.Controllers
{
#pragma warning disable CS8618
    public sealed class TextDto
    {
        public string TextId { get; set; }
        public string Value { get; set; }
        public bool IsPublic { get; set; }

        public static TextDto From(Text text)
        {
            var state = text.GetState();

            return new TextDto
            {
                TextId = state.TextId,
                Value = state.Value,
                IsPublic = state.IsPublic
            };
        }
    }

    public sealed class CreateTextDto
    {
        public string Value { get; set; }
        public bool IsPublic { get; set; }
        public string Language { get; set; }
    }
#pragma warning restore CS8618

    [Route("api/[controller]")]
    public sealed class TextsController : TyrController
    {
        private readonly ITextRepository _textRepository;
        private readonly ITypingReportGenerator _typingReportGerenator;
        private readonly ITextsClient _textsClient;

        public TextsController(
            ITextRepository textRepository,
            ITypingReportGenerator typingReportGenerator,
            ITextsClient textsClient)
        {
            _textRepository = textRepository;
            _typingReportGerenator = typingReportGenerator;
            _textsClient = textsClient;
        }

        [HttpGet]
        [Route("{textId}")]
        public async ValueTask<ActionResult<TextDto>> GetById(string textId)
        {
            var text = await _textRepository.FindAsync(textId);
            if (text == null || text.IsArchived)
                return NotFound();

            return TextDto.From(text);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("generate")]
        public async ValueTask<ActionResult<string>> GenerateTextValue(
            int length,
            TextGenerationType textType = TextGenerationType.Text,
            string? language = "en",
            int maxShouldContainErrors = 10,
            int maxShouldContainSlow = 10)
        {
            if (language == null)
                language = "en";

            var shouldContain = new List<string>();

            if (Profile.Type == ProfileType.User)
            {
                // Personalize text generation.
                var data = await _typingReportGerenator.GenerateUserStatisticsAsync(Profile.ProfileId, language);

                shouldContain.AddRange(data.KeyPairs
                    .Where(x => x.FromKey?.Length == 1 && x.ToKey.Length == 1)
                    .OrderByDescending(x => x.MistakesToSuccessRatio)
                    .Select(x => $"{x.FromKey}{x.ToKey}")
                    .Take(maxShouldContainErrors));

                shouldContain.AddRange(data.KeyPairs
                    .Where(x => x.FromKey?.Length == 1 && x.ToKey.Length == 1)
                    .OrderByDescending(x => x.AverageDelay)
                    .Select(x => $"{x.FromKey}{x.ToKey}")
                    .Take(maxShouldContainSlow));
            }

            //var textValue = await _textGenerator.GenerateTextAsync(new TextGenerationConfigurationDto(length, shouldContain, textType, language));
            var generatedText = await _textsClient.GenerateTextAsync(new Texts.Api.Client.TextGenerationConfiguration(language, length, textType, shouldContain), EndpointAuthenticationType.Service, default);
            var textValue = generatedText.Value;

            var textId = await _textRepository.NextIdAsync();

            var configuration = new TextConfiguration(
                TextType.Generated,
                new Typing.TextGenerationConfiguration(length, shouldContain, textType),
                language);

            var text = new Text(textId, textValue, ProfileId, DateTime.UtcNow, false, configuration);
            await _textRepository.SaveAsync(text);

            return Ok(textId);
        }

        [HttpPost]
        public async ValueTask<ActionResult> Post(CreateTextDto dto)
        {
            // TODO: Make a global attribute that will ensure that in order to
            // execute endpoint we need USER-authentication and not SERVICE-authentication.
            var textId = await _textRepository.NextIdAsync();

            var configuration = new TextConfiguration(
                TextType.User, null, dto.Language);

            var text = new Text(textId, dto.Value, ProfileId, DateTime.UtcNow, dto.IsPublic, configuration);

            await _textRepository.SaveAsync(text);

            var result = new { textId };

            return CreatedAtAction(nameof(GetById), result, result);
        }

        [HttpDelete]
        [Route("{textId}")]
        public async ValueTask<ActionResult> Delete(string textId)
        {
            var text = await _textRepository.FindAsync(textId);
            if (text == null || text.IsArchived)
                return NotFound();

            text.Archive();
            await _textRepository.SaveAsync(text);

            return NoContent();
        }

        [HttpPost]
        [Route("{textId}/public")]
        public async ValueTask<ActionResult> MakePublic(string textId)
        {
            var text = await _textRepository.FindAsync(textId);
            if (text == null || text.IsArchived)
                return NotFound();

            text.MakePublic();
            await _textRepository.SaveAsync(text);

            return NoContent();
        }

        [HttpPost]
        [Route("{textId}/private")]
        public async ValueTask<ActionResult> MakePrivate(string textId)
        {
            var text = await _textRepository.FindAsync(textId);
            if (text == null || text.IsArchived)
                return NotFound();

            text.MakePrivate();
            await _textRepository.SaveAsync(text);

            return NoContent();
        }
    }
}
