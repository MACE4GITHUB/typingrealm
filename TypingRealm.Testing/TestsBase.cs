using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Moq.Language;
using Moq.Language.Flow;
using Xunit;

namespace TypingRealm.Testing
{
    public abstract class TestsBase
    {
        protected Fixture Fixture { get; } = AutoMoqDataAttribute.CreateFixture();

        protected T Create<T>() => Fixture.Create<T>();

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

        protected object? GetPrivateField(object instance, string fieldName)
            => instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance);

        protected async ValueTask AssertThrowsAsync<TException>(Task task, TException exception)
            where TException : Exception
        {
            var thrown = await Assert.ThrowsAsync<TException>(() => task).ConfigureAwait(false);
            Assert.Equal(exception, thrown);
        }

        protected async ValueTask AssertThrowsAsync<TException>(Task task)
            where TException : Exception
        {
            await Assert.ThrowsAsync<TException>(() => task).ConfigureAwait(false);
        }
    }
}
