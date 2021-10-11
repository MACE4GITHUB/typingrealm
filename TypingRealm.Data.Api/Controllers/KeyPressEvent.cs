using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Data.Api.Controllers
{
#pragma warning disable CS8618
    public sealed class KeyPressEvent
    {
        public string Key { get; set; }
        public decimal Perf { get; set; }
        public int Index { get; set; }
    }

    public sealed class TextTyped
    {
        public TextData TextData { get; set; }
        public DateTimeOffset StartedTypingAt { get; set; }
        public IEnumerable<KeyPressEvent> Events { get; set; }
    }

    public sealed class TextData
    {
        public string Text { get; set; }
    }

    public sealed class StatisticsResult
    {
        public decimal SpeedCpm { get; set; }
        public decimal SpeedWpm { get; set; }
    }
#pragma warning restore CS8618

    [AllowAnonymous]
    [Route("api/[controller]")]
    public sealed class StatisticsController : ControllerBase
    {
        [HttpPost]
        public async ValueTask<ActionResult<StatisticsResult>> Submit(TextTyped textTyped)
        {
            var statisticsResult = CalculateStatistics(textTyped);

            // TODO: Consider getting it separately after submitting, by ID (REST).
            return CreatedAtAction(nameof(Submit), statisticsResult);
        }

        private StatisticsResult CalculateStatistics(TextTyped textTyped)
        {
            var textLength = textTyped.TextData.Text.Length;
            var timeMs = textTyped.Events.Last().Perf;

            return new StatisticsResult
            {
                SpeedCpm = textLength / (timeMs / 1000) * 60,
                SpeedWpm = textLength / (timeMs / 1000) * 60 / 5
            };
        }
    }
}
