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
            for (var i = 0; i < GetTestData().Count(); i++)
            {
                await TestData(i);
            }
        }

        [Fact]
        public Task ShouldHandleSimpleText() => TestData(0);

        [Fact]
        public Task ShouldHandleFirstCapitalCharacter() => TestData(1);

        [Fact]
        public Task ShouldHandleCorrections() => TestData(2);

        [Fact]
        public Task ShouldHandleJustBackspaces() => TestData(3);

        [Fact]
        public Task ShouldHandleCapitalCharactersInTheMiddle() => TestData(4);

        [Fact]
        public Task ShouldHandleMistakeInFirstCharacter() => TestData(5);

        private async Task TestData(int index)
        {
            var data = GetTestData(index);
            var result = await _sut.ValidateAsync(data.Key.TextValue, data.Key.TextTypingResult);

            Assert.Equal(Math.Floor(data.Value.SpeedCpm), Math.Floor(result.SpeedCpm));
            Assert.True(data.Value.KeyPairs.SequenceEqual(result.KeyPairs));
        }

        private KeyValuePair<Input, TextAnalysisResult> GetTestData(int index)
            => GetTestData().ToList()[index];

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
                        new KeyPair("", "t", "t", 0, KeyPairType.Correct, 0),
                        new KeyPair("t", "e", "e", 20, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0)
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
                        new KeyPair("", "T", "T", 10, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", "e", 30, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0)
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

                        // Mistake.
                        new KeyPressEvent(2, KeyAction.Press, "x", 60),
                        new KeyPressEvent(3, KeyAction.Release, "x", 70),

                        // Correct, but it shouldn't be logged.
                        new KeyPressEvent(3, KeyAction.Press, "t", 80),
                        new KeyPressEvent(4, KeyAction.Release, "t", 90),

                        // Corrections.
                        new KeyPressEvent(4, KeyAction.Press, "backspace", 100),
                        new KeyPressEvent(3, KeyAction.Press, "backspace", 110),
                        new KeyPressEvent(2, KeyAction.Release, "backspace", 120),

                        new KeyPressEvent(2, KeyAction.Press, "s", 130),
                        new KeyPressEvent(3, KeyAction.Release, "s", 140),
                        new KeyPressEvent(3, KeyAction.Press, "t", 150)
                    })), new TextAnalysisResult(1600, new[]
                    {
                        new KeyPair("", "T", "T", 10, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", "e", 30, KeyPairType.Correct, 0),

                        new KeyPair("e", "x", "s", 20, KeyPairType.Mistake, 0),
                        new KeyPair("x", "backspace", "", 50, KeyPairType.Correction, 0),

                        new KeyPair("backspace", "s", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0)
                    })),

                // Do not get just backspaces on correct keys to statistics.
                // After a backspace was pressed without any corrections, start analytics from scratch (no FromKey).
                new KeyValuePair<Input, TextAnalysisResult>(
                    new Input("Test", MakeTextTypingResult(new[]
                    {
                        new KeyPressEvent(0, KeyAction.Press, "shift", 0),
                        new KeyPressEvent(0, KeyAction.Press, "T", 10),
                        new KeyPressEvent(1, KeyAction.Release, "T", 20),
                        new KeyPressEvent(1, KeyAction.Release, "shift", 30),
                        new KeyPressEvent(1, KeyAction.Press, "e", 40),
                        new KeyPressEvent(2, KeyAction.Release, "e", 50),

                        new KeyPressEvent(2, KeyAction.Press, "backspace", 60),
                        new KeyPressEvent(1, KeyAction.Press, "backspace", 70),
                        new KeyPressEvent(0, KeyAction.Release, "backspace", 80),

                        new KeyPressEvent(0, KeyAction.Press, "shift", 90),
                        new KeyPressEvent(0, KeyAction.Press, "T", 100),
                        new KeyPressEvent(1, KeyAction.Release, "T", 110),
                        new KeyPressEvent(1, KeyAction.Release, "shift", 120),
                        new KeyPressEvent(1, KeyAction.Press, "e", 130),
                        new KeyPressEvent(2, KeyAction.Release, "e", 140),

                        new KeyPressEvent(2, KeyAction.Press, "s", 150),
                        new KeyPressEvent(3, KeyAction.Release, "s", 160),
                        new KeyPressEvent(3, KeyAction.Press, "t", 170)
                    })), new TextAnalysisResult(1411, new[]
                    {
                        new KeyPair("", "T", "T", 10, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", "e", 30, KeyPairType.Correct, 0),

                        new KeyPair("", "T", "T", 60, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", "e", 30, KeyPairType.Correct, 0),

                        new KeyPair("e", "s", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0)
                    })),

                // Shift in the middle.
                new KeyValuePair<Input, TextAnalysisResult>(
                    new Input("Test On", MakeTextTypingResult(new[]
                    {
                        new KeyPressEvent(0, KeyAction.Press, "shift", 0),
                        new KeyPressEvent(0, KeyAction.Press, "T", 10),
                        new KeyPressEvent(1, KeyAction.Release, "T", 20),
                        new KeyPressEvent(1, KeyAction.Release, "shift", 30),
                        new KeyPressEvent(1, KeyAction.Press, "e", 40),
                        new KeyPressEvent(2, KeyAction.Release, "e", 50),
                        new KeyPressEvent(2, KeyAction.Press, "s", 60),
                        new KeyPressEvent(3, KeyAction.Release, "s", 70),
                        new KeyPressEvent(3, KeyAction.Press, "t", 80),
                        new KeyPressEvent(4, KeyAction.Release, "t", 90),
                        new KeyPressEvent(4, KeyAction.Press, " ", 100),
                        new KeyPressEvent(5, KeyAction.Press, "shift", 110),
                        new KeyPressEvent(5, KeyAction.Release, " ", 120),
                        new KeyPressEvent(5, KeyAction.Press, "O", 130),
                        new KeyPressEvent(6, KeyAction.Release, "shift", 140),
                        new KeyPressEvent(6, KeyAction.Press, "n", 150)
                    })), new TextAnalysisResult(2800, new[]
                    {
                        new KeyPair("", "T", "T", 10, KeyPairType.Correct, 10),
                        new KeyPair("T", "e", "e", 30, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", "s", 20, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0),
                        new KeyPair("t", " ", " ", 20, KeyPairType.Correct, 0),
                        new KeyPair(" ", "O", "O", 30, KeyPairType.Correct, 20),
                        new KeyPair("O", "n", "n", 20, KeyPairType.Correct, 0)
                    })),

                new KeyValuePair<Input, TextAnalysisResult>(
                    new Input("test", MakeTextTypingResult(new[]
                    {
                        new KeyPressEvent(0, KeyAction.Press, " ", 0),
                        new KeyPressEvent(1, KeyAction.Release, " ", 10),
                        new KeyPressEvent(1, KeyAction.Press, "e", 20),
                        new KeyPressEvent(2, KeyAction.Release, "e", 30),

                        new KeyPressEvent(2, KeyAction.Press, "backspace", 40),
                        new KeyPressEvent(1, KeyAction.Release, "backspace", 50),
                        new KeyPressEvent(1, KeyAction.Press, "backspace", 60),
                        new KeyPressEvent(0, KeyAction.Release, "backspace", 70),
                        new KeyPressEvent(0, KeyAction.Press, "t", 80),
                        new KeyPressEvent(1, KeyAction.Press, "e", 90),
                        new KeyPressEvent(2, KeyAction.Release, "t", 100),
                        new KeyPressEvent(2, KeyAction.Release, "e", 110),


                        new KeyPressEvent(2, KeyAction.Press, "s", 120),
                        new KeyPressEvent(3, KeyAction.Release, "s", 130),
                        new KeyPressEvent(3, KeyAction.Press, "t", 140)
                    })), new TextAnalysisResult(1714, new[]
                    {
                        new KeyPair("", " ", "t", 0, KeyPairType.Mistake, 0),
                        new KeyPair(" ", "backspace", "", 60, KeyPairType.Correction, 0),
                        new KeyPair("backspace", "t", "t", 20, KeyPairType.Correct, 0),
                        new KeyPair("t", "e", "e", 10, KeyPairType.Correct, 0),
                        new KeyPair("e", "s", "s", 30, KeyPairType.Correct, 0),
                        new KeyPair("s", "t", "t", 20, KeyPairType.Correct, 0)
                    }))
            };
        }

        private TextTypingResult MakeTextTypingResult(IEnumerable<KeyPressEvent> events)
        {
            return new TextTypingResult("", 1, DateTime.UtcNow, DateTime.UtcNow, events);
        }
    }
}
