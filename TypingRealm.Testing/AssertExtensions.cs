using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TypingRealm.Testing
{
    public static class AssertExtensions
    {
        public static object AssertRegistered(
            this IServiceProvider provider,
            Type interfaceType,
            Type implementationType)
        {
            var implementation = provider.GetService(interfaceType);
            Assert.NotNull(implementation);
            Assert.IsType(implementationType, implementation);

            return implementation;
        }

        public static void AssertRegisteredTransient(
            this IServiceProvider provider,
            Type interfaceType,
            Type implementationType)
        {
            var i1 = provider.AssertRegistered(interfaceType, implementationType);
            var i2 = provider.AssertRegistered(interfaceType, implementationType);

            Assert.NotEqual(i1, i2);
        }

        public static void AssertRegisteredSingleton(
            this IServiceProvider provider,
            Type interfaceType,
            Type implementationType)
        {
            var i1 = provider.AssertRegistered(interfaceType, implementationType);
            var i2 = provider.AssertRegistered(interfaceType, implementationType);

            using var scope1 = provider.CreateScope();
            var i3 = scope1.ServiceProvider.AssertRegistered(interfaceType, implementationType);

            Assert.Equal(i1, i2);
            Assert.Equal(i1, i3);
        }

        public static void AssertRegisteredTransient<TInterface, TImplementation>(this IServiceProvider provider)
            => provider.AssertRegisteredTransient(typeof(TInterface), typeof(TImplementation));

        public static void AssertRegisteredSingleton<TInterface, TImplementation>(this IServiceProvider provider)
            => provider.AssertRegisteredSingleton(typeof(TInterface), typeof(TImplementation));
    }
}
