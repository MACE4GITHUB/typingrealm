using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public TextsController(ITextRepository textRepository)
        {
            _textRepository = textRepository;
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

        [HttpPost]
        public async ValueTask<ActionResult> Post(CreateTextDto dto)
        {
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
