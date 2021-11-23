using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TypingRealm.Typing.Tests
{
    public record Input(string TextValue, TextTypingResult TextTypingResult);
    public class TextTypingResultValidatorTests
    {
        private readonly TextTypingResultValidator _sut;

        public TextTypingResultValidatorTests()
        {
            _sut = new TextTypingResultValidator();
        }

        [Fact]
        public async Task ShouldProduceCorrectResults()
        {
            foreach (var data in GetTestData())
            {
                var result = await _sut.ValidateAsync(data.Key.TextValue, data.Key.TextTypingResult);

                Assert.Equal(Math.Floor(data.Value.SpeedCpm), Math.Floor(result.SpeedCpm));
                Assert.True(data.Value.KeyPairs.SequenceEqual(result.KeyPairs));
            }
        }

        private IEnumerable<KeyValuePair<Input, TextAnalysisResult>> GetTestData()
        {
            return new[]
            {
                new KeyValuePair<Input, TextAnalysisResult>(
                    new Input("test", MakeTextTypingResult(new[]
                    {
                        new KeyPressEvent(0, KeyAction.Press, "t", 0),
                        new KeyPressEvent(1, KeyAction.Release, "t", 10),
                        new KeyPressEvent(1, KeyAction.Press, "e", 20),
                        new KeyPressEvent(2, KeyAction.Release, "e", 30),
                        new KeyPressEvent(2, KeyAction.Press, "s", 40),
                        new KeyPressEvent(3, KeyAction.Release, "s", 50),
                        new KeyPressEvent(3, KeyAction.Press, "t", 60)
                    })), new TextAnalysisResult(4000, new[]
                    {
                        new KeyPair("", "t", 0, KeyPairType.Correct, 0),
                        new KeyPair("t", "e", 20, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", 20, KeyPairType.Correct, 0)
                    })),

                new KeyValuePair<Input, TextAnalysisResult>(
                    new Input("Test", MakeTextTypingResult(new[]
                    {
                        new KeyPressEvent(0, KeyAction.Press, "shift", 0),
                        new KeyPressEvent(0, KeyAction.Press, "T", 10),
                        new KeyPressEvent(1, KeyAction.Release, "T", 20),
                        new KeyPressEvent(1, KeyAction.Release, "shift", 30),
                        new KeyPressEvent(1, KeyAction.Press, "e", 40),
                        new KeyPressEvent(2, KeyAction.Release, "e", 50),
                        new KeyPressEvent(2, KeyAction.Press, "s", 60),
                        new KeyPressEvent(3, KeyAction.Release, "s", 70),
                        new KeyPressEvent(3, KeyAction.Press, "t", 80)
                    })), new TextAnalysisResult(3000, new[]
                    {
                        new KeyPair("", "T", 10, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", 30, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", 20, KeyPairType.Correct, 0)
                    }))
            };
        }

        private TextTypingResult MakeTextTypingResult(IEnumerable<KeyPressEvent> events)
        {
            return new TextTypingResult("", 1, 1, DateTime.UtcNow, DateTime.UtcNow, events);
        }
    }
}
