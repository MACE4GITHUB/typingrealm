using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Typing;

namespace TypingRealm.Data.Api.Controllers
{
#pragma warning disable CS8618
    public sealed class TextDto
    {
        public string Id { get; set; }
        public string Value { get; set; }

        public static TextDto From(Text text)
        {
            return new TextDto
            {
                Id = text.Id,
                Value = text.Value
            };
        }
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
            if (text == null)
                return NotFound();

            return TextDto.From(text);
        }
    }
}
