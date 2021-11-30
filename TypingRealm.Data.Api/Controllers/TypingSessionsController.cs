using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Typing;

namespace TypingRealm.Data.Api.Controllers
{
#pragma warning disable CS8618
    public sealed class TypingSessionTextDto
    {
        public int Index { get; set; }
        public string TextId { get; set; }
        public string Value { get; set; }
    }

    public sealed class TypingSessionDto
    {
        public string TypingSessionId { get; set; }
        public IEnumerable<TypingSessionTextDto> Texts { get; set; }

        public static TypingSessionDto From(TypingSession typingSession)
        {
            var state = typingSession.GetState();

            return new TypingSessionDto
            {
                TypingSessionId = state.TypingSessionId,
                Texts = state.Texts.Select(x => new TypingSessionTextDto
                {
                    Index = x.Key,
                    TextId = x.Value.TextId,
                    Value = x.Value.Value
                }).ToList()
            };
        }
    }

    public sealed class AddTextToTypingSessionDto
    {
        public string TextId { get; set; }
    }
#pragma warning restore CS8618

    [Route("api/[controller]")]
    public sealed class TypingSessionsController : TyrController
    {
        private readonly ITypingSessionRepository _typingSessionRepository;
        private readonly ITextRepository _textRepository;

        public TypingSessionsController(
            ITypingSessionRepository typingSessionRepository,
            ITextRepository textRepository)
        {
            _typingSessionRepository = typingSessionRepository;
            _textRepository = textRepository;
        }

        [HttpGet]
        [Route("{typingSessionId}")]
        public async ValueTask<ActionResult<TypingSessionDto>> GetById(string typingSessionId)
        {
            var typingSession = await _typingSessionRepository.FindAsync(typingSessionId);
            if (typingSession == null)
                return NotFound();

            // TODO: Validate that the session was created by this user or that it's public / shared, otherwise return NotFound.

            return TypingSessionDto.From(typingSession);
        }

        [HttpPost]
        public async ValueTask<ActionResult> StartTypingSession()
        {
            var typingSessionId = await _typingSessionRepository.NextIdAsync();

            var typingSession = new TypingSession(typingSessionId, ProfileId, DateTime.UtcNow, new TypingSessionConfiguration());

            await _typingSessionRepository.SaveAsync(typingSession);

            var result = new { typingSessionId };

            return CreatedAtAction(nameof(GetById), result, result);
        }

        [HttpPost]
        [Route("{typingSessionId}/texts")]
        public async ValueTask<ActionResult> AddText(string typingSessionId, AddTextToTypingSessionDto dto)
        {
            var typingSession = await _typingSessionRepository.FindAsync(typingSessionId);
            if (typingSession == null)
                return NotFound();

            // TODO: Validate that the session was created by this user or that it's public / shared, otherwise return NotFound.

            var text = await _textRepository.FindAsync(dto.TextId);
            if (text == null)
                return NotFound();

            var textIndex = typingSession.AddText(new TypingSessionText(dto.TextId, text.Value));
            await _typingSessionRepository.SaveAsync(typingSession);

            var result = new
            {
                typingSessionId = typingSessionId,
                textIndex = textIndex
            };

            return CreatedAtAction(nameof(GetTextInTypingSession), result, result);
        }

        [HttpGet]
        [Route("{typingSessionId}/texts/{textIndex}")]
        public async ValueTask<ActionResult<TypingSessionTextDto>> GetTextInTypingSession(string typingSessionId, int textIndex)
        {
            var typingSession = await _typingSessionRepository.FindAsync(typingSessionId);
            if (typingSession == null)
                return NotFound();

            // TODO: Validate that the session was created by this user or that it's public / shared, otherwise return NotFound.

            var text = typingSession.GetTypingSessionTextAtIndexOrDefault(textIndex);
            if (text == null)
                return NotFound();

            return Ok(new TypingSessionTextDto
            {
                Index = textIndex,
                TextId = text.TextId,
                Value = text.Value
            });
        }
    }
}
