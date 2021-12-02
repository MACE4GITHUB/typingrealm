using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Profiles;
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
    }
#pragma warning restore CS8618

    [Route("api/[controller]")]
    public sealed class TextsController : TyrController
    {
        private readonly ITextRepository _textRepository;
        private readonly ITextGenerator _textGenerator;
        private readonly ITypingReportGenerator _typingReportGerenator;

        public TextsController(
            ITextRepository textRepository,
            ITextGenerator textGenerator,
            ITypingReportGenerator typingReportGenerator)
        {
            _textRepository = textRepository;
            _textGenerator = textGenerator;
            _typingReportGerenator = typingReportGenerator;
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
        public async ValueTask<ActionResult<string>> GenerateTextValue(int length, TextType textType = TextType.Text)
        {
            var shouldContain = new List<string>();

            if (Profile.Type == ProfileType.User)
            {
                // Personalize text generation.
                var data = await _typingReportGerenator.GenerateReportAsync(Profile.ProfileId);

                shouldContain.AddRange(data.AggregatedData
                    .Where(x => x.FromKey?.Length == 1 && x.ToKey.Length == 1)
                    .OrderByDescending(x => x.MistakesToSuccessRatio)
                    .Select(x => $"{x.FromKey}{x.ToKey}")
                    .Take(10));

                shouldContain.AddRange(data.AggregatedData
                    .Where(x => x.FromKey?.Length == 1 && x.ToKey.Length == 1)
                    .OrderByDescending(x => x.AverageDelay)
                    .Select(x => $"{x.FromKey}{x.ToKey}")
                    .Take(10));
            }

            var text = await _textGenerator.GenerateTextAsync(new TextConfiguration(length, shouldContain, textType));

            return Ok(text);
        }

        [HttpPost]
        public async ValueTask<ActionResult> Post(CreateTextDto dto)
        {
            // TODO: Make a global attribute that will ensure that in order to
            // execute endpoint we need USER-authentication and not SERVICE-authentication.
            var textId = await _textRepository.NextIdAsync();

            var text = new Text(textId, dto.Value, ProfileId, DateTime.UtcNow, dto.IsPublic);

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
