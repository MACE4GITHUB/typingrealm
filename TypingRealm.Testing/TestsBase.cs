using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace TypingRealm.Testing;

/// <summary>
/// Test helper containing a bunch of methods to simplify unit testing.
/// </summary>
public abstract class TestsBase : IDisposable
{
    // Used for creating instances with custom ISpecimenBuilder.
    private readonly object _lock = new();

    protected TestsBase() : this(AutoMoqDataAttribute.CreateFixture()) { }
    protected TestsBase(IFixture fixture)
    {
        Fixture = fixture;
        Cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Fixture instance with auto Domain data customizations.
    /// </summary>
    protected IFixture Fixture { get; }

    /// <summary>
    /// Cancellation token source that is created separately per every test.
    /// </summary>
    protected CancellationTokenSource Cts { get; }

    void IDisposable.Dispose()
    {
        Cts.Dispose();
    }

    /// <summary>
    /// Waits for 100 ms.
    /// </summary>
    protected static ValueTask Wait() => Wait(100);
    protected static async ValueTask Wait(int milliseconds)
        => await Task.Delay(milliseconds).ConfigureAwait(false);

    protected T Create<T>() => Fixture.Create<T>();
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

    protected Mock<T> Freeze<T>() where T : class => Fixture.Freeze<Mock<T>>();

    /// <summary>
    /// Changes the value of a class that is inherited from <see cref="Primitive{TValue}"/>.
    /// </summary>
    protected static void SetPrimitiveValue<TValue>(object instance, string propertyName, TValue value)
    {
        var field = typeof(Primitive<TValue>).GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        field!.SetValue(instance, value);
    }

    protected static object? GetPrivateField(object instance, string fieldName) => instance
        .GetType()
        .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?
        .GetValue(instance);

    /// <summary>
    /// Gets new ServiceCollection with enabled Logging. Can be overridden to
    /// register additional dependencies by default.
    /// </summary>
    protected virtual IServiceCollection GetServiceCollection()
    {
        return new ServiceCollection()
            .AddLogging();
    }

    /// <summary>
    /// Gets new ServiceProvider using <see cref="GetServiceCollection"/> method.
    /// </summary>
    protected IServiceProvider GetServiceProvider() => GetServiceCollection().BuildServiceProvider();

    /// <summary>
    /// Creates new <see cref="TestException"/> instance.
    /// </summary>
    protected TestException NewTestException() => Fixture.Create<TestException>();

    /// <summary>
    /// Ensures that all public properties of a class can be read or assigned by
    /// serializer.
    /// </summary>
    protected void AssertSerializable<T>()
    {
        // This will use default parameterless constructor and assign all properties.
        var instance = Create<T>();

        // This will throw if there's no default parameterless constructor.
        var result = JsonSerializer.Deserialize<T>(
            JsonSerializer.Serialize(instance));

        AssertDeepEqual(instance!, result!);
    }

    protected void AssertDeepEqual(object leftInstance, object rightInstance)
    {
        var leftType = leftInstance.GetType();
        var rightType = rightInstance.GetType();

        if (leftType.IsValueType
            || leftType.IsPrimitive
            || leftType == typeof(string))
        {
            Assert.Equal(leftType, rightType);
            Assert.Equal(leftInstance, rightInstance);
            return;
        }

        if (leftType.GetInterface(nameof(IEnumerable)) != null)
        {
            // Do not compare types here, it can be different (mocked) collections.

            var leftItems = ((IEnumerable)leftInstance).Cast<object>().ToList();
            var rightItems = ((IEnumerable)rightInstance).Cast<object>().ToList();

            Assert.Equal(leftItems.Count, rightItems.Count);

            for (var i = 0; i < leftItems.Count; i++)
            {
                AssertDeepEqual(leftItems[i], rightItems[i]);
            }

            return;
        }

        foreach (var property in leftType.GetProperties())
        {
            Assert.Equal(leftType, rightType);

            AssertDeepEqual(
                property.GetValue(leftInstance)!,
                property.GetValue(rightInstance)!);
        }
    }

    protected static async ValueTask<TException> AssertThrowsAsync<TException>(Func<Task> taskFactory, TException exception)
        where TException : Exception
    {
        var thrown = await Assert.ThrowsAsync<TException>(() => taskFactory()).ConfigureAwait(false);
        Assert.Equal(exception, thrown);

        return thrown;
    }

    protected static async ValueTask<TException> AssertThrowsAsync<TException>(Func<ValueTask> vtFactory, TException exception)
        where TException : Exception
    {
        var thrown = await Assert.ThrowsAsync<TException>(() => vtFactory().AsTask()).ConfigureAwait(false);
        Assert.Equal(exception, thrown);

        return thrown;
    }

    protected static async ValueTask<TException> AssertThrowsAsync<TException>(Func<ValueTask> vtFactory)
        where TException : Exception
    {
        return await Assert.ThrowsAsync<TException>(() => vtFactory().AsTask()).ConfigureAwait(false);
    }

    protected static Task<Exception> SwallowAnyAsync(Task task)
        => Assert.ThrowsAnyAsync<Exception>(() => task);

    protected static void AssertSuccessful()
    {
        // This method does nothing, it is being ran at the end of unit tests to
        // indicate that test has been successfully completed without exceptions.
        // It's mainly used to get rid of analyzer warnings when it is intended.
    }
}
