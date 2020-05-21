using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Xunit;

namespace TypingRealm.Testing
{
    public abstract class TestsBase
    {
        private readonly Fixture _fixture = AutoMoqDataAttribute.CreateFixture();

        protected T Create<T>()
        {
            return _fixture.Create<T>();
        }

        protected void AssertSerializable<T>()
        {
            var message = Create<T>();
            var result = JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(message));

            foreach (var property in typeof(T).GetProperties())
            {
                Assert.Equal(property.GetValue(message), property.GetValue(result));
            }
        }

        protected ValueTask Wait() => Wait(100);
        protected async ValueTask Wait(int milliseconds)
            => await Task.Delay(milliseconds).ConfigureAwait(false);
    }
}
