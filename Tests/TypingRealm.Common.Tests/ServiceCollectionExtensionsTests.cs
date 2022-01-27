using System;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

public class ServiceCollectionExtensionsTests : TestsBase
{
    public interface IService
    {
    }

    public class Service : IService
    {
    }

    public class ServiceDecorator : IService
    {
        public ServiceDecorator(IService service)
        {
            Service = service;
        }

        public IService Service { get; }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    public void Decorate_ShouldThrow_WhenNoRegistrationsFound(ServiceLifetime? lifetime)
    {
        if (lifetime == null)
        {
            Assert.Throws<InvalidOperationException>(
                () => GetServiceCollection().Decorate<IService, Service>());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(
                () => GetServiceCollection().Decorate<IService, Service>(lifetime.Value));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    public void Decorate_ShouldThrow_WhenInvalidDecorator(ServiceLifetime? lifetime)
    {
        if (lifetime == null)
        {
            Assert.Throws<InvalidOperationException>(() => GetServiceCollection()
                .AddTransient<IService, Service>()
                .Decorate<IService, Service>());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => GetServiceCollection()
                .AddTransient<IService, Service>()
                .Decorate<IService, Service>(lifetime.Value));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    public void Decorate_ShouldDecorate(ServiceLifetime? lifetime)
    {
        var service = new Service();

        ServiceProvider serviceProvider;
        if (lifetime == null)
        {
            serviceProvider = GetServiceCollection()
                .AddSingleton<IService>(service)
                .Decorate<IService, ServiceDecorator>()
                .BuildServiceProvider();

            serviceProvider.AssertRegisteredSingleton<IService, ServiceDecorator>();
        }
        else
        {
            serviceProvider = GetServiceCollection()
                .AddSingleton<IService>(service)
                .Decorate<IService, ServiceDecorator>(lifetime.Value)
                .BuildServiceProvider();

            serviceProvider.AssertRegistered<IService, ServiceDecorator>(lifetime.Value);
        }

        var instance = serviceProvider.GetRequiredService<IService>();
        Assert.IsType<ServiceDecorator>(instance);
        Assert.Equal(service, ((ServiceDecorator)instance).Service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    public void Decorate_ShouldRespectLifetime(ServiceLifetime? lifetime)
    {
        if (lifetime == null)
        {
            var serviceProvider = GetServiceCollection()
                .AddTransient<IService, Service>()
                .Decorate<IService, ServiceDecorator>()
                .BuildServiceProvider();

            serviceProvider.AssertRegisteredTransient<IService, ServiceDecorator>();
        }
        else
        {
            var serviceProvider = GetServiceCollection()
                .AddTransient<IService, Service>()
                .Decorate<IService, ServiceDecorator>(lifetime.Value)
                .BuildServiceProvider();

            serviceProvider.AssertRegistered<IService, ServiceDecorator>(lifetime.Value);
        }

        if (lifetime == null)
        {
            var serviceProvider = GetServiceCollection()
                .AddScoped<IService, Service>()
                .Decorate<IService, ServiceDecorator>()
                .BuildServiceProvider();

            serviceProvider.AssertRegisteredScoped<IService, ServiceDecorator>();
        }
        else
        {
            var serviceProvider = GetServiceCollection()
                .AddScoped<IService, Service>()
                .Decorate<IService, ServiceDecorator>(lifetime.Value)
                .BuildServiceProvider();

            serviceProvider.AssertRegistered<IService, ServiceDecorator>(lifetime.Value);
        }
    }
}
