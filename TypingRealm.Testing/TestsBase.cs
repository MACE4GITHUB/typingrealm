using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Xunit;

namespace TypingRealm.Testing
{
    public abstract class TestsBase : IDisposable
    {
        protected Fixture Fixture { get; } = AutoMoqDataAttribute.CreateFixture();

        protected T Create<T>() => Fixture.Create<T>();

        private readonly object _lock = new object();
        protected T Create<T>(params ISpecimenBuilder[] builders)
        {
            lock (_lock)
            {
                foreach (var builder in builders)
                {
                    Fixture.Customizations.Add(builder);
                }

                var result = Fixture.Create<T>();

                foreach (var builder in builders)
                {
                    Fixture.Customizations.Remove(builder);
                }

                return result;
            }
        }

        protected CancellationTokenSource Cts { get; } = new CancellationTokenSource();

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

        protected async ValueTask<TException> AssertThrowsAsync<TException>(Task task)
            where TException : Exception
        {
            return await Assert.ThrowsAsync<TException>(() => task).ConfigureAwait(false);
        }

        protected async ValueTask AssertThrowsAsync<TException>(ValueTask vt, TException exception)
            where TException : Exception
        {
            var thrown = await Assert.ThrowsAsync<TException>(() => vt.AsTask()).ConfigureAwait(false);
            Assert.Equal(exception, thrown);
        }

        protected async ValueTask<TException> AssertThrowsAsync<TException>(ValueTask vt)
            where TException : Exception
        {
            return await Assert.ThrowsAsync<TException>(() => vt.AsTask()).ConfigureAwait(false);
        }

        protected Task<Exception> SwallowAnyAsync(Task task)
            => Assert.ThrowsAnyAsync<Exception>(() => task);

        public void Dispose()
        {
            Cts.Cancel();
            Cts.Dispose();
        }
    }
}
