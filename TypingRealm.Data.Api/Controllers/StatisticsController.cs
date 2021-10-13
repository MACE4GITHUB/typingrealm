using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Data.Resources.Typing;
using TypingRealm.Typing;

namespace TypingRealm.Data.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public sealed class StatisticsController : ControllerBase
    {
        private readonly ITypedTextProcessor _typedTextProcessor;
        private readonly ITextStore _textStore;
        private readonly ITextTypingStatisticsCalculator _statisticsCalculator;

        public StatisticsController(
            ITypedTextProcessor typedTextProcessor,
            ITextStore textStore,
            ITextTypingStatisticsCalculator statisticsCalculator)
        {
            _typedTextProcessor = typedTextProcessor;
            _textStore = textStore;
            _statisticsCalculator = statisticsCalculator;
        }

        [HttpPost]
        public async ValueTask<ActionResult> Submit(TypedTextDto typedText)
        {
            var text = _typedTextProcessor.ProcessTypedText(typedText);

            // TODO: Get user from authentication data.
            var userId = Guid.Empty;
            _textStore.Append(userId, text);

            return CreatedAtAction(nameof(Submit), new { TextId = text.TextId });
        }

        [HttpGet]
        [Route("{textId}")]
        public async ValueTask<ActionResult<TextTypingStatistics>> GetStatistics(Guid textId)
        {
            // TODO: Do not allow viewing texts from another user.
            var text = _textStore.Find(textId);
            if (text == null)
                return NotFound();

            return _statisticsCalculator.Calculate(text);
        }
    }
}
