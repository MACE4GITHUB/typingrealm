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
        private readonly ITextTypingStatisticsCalculator _statisticsCalculator;

        public StatisticsController(
            ITypedTextProcessor typedTextProcessor,
            ITextTypingStatisticsCalculator statisticsCalculator)
        {
            _typedTextProcessor = typedTextProcessor;
            _statisticsCalculator = statisticsCalculator;
        }

        [HttpPost]
        public async ValueTask<ActionResult<TextTypingStatistics>> Submit(TypedTextDto typedText)
        {
            var text = _typedTextProcessor.ProcessTypedText(typedText);

            var statistics = _statisticsCalculator.Calculate(text);

            // TODO: Consider getting it separately after submitting, by ID (REST).
            return CreatedAtAction(nameof(Submit), statistics);
        }
    }
}
