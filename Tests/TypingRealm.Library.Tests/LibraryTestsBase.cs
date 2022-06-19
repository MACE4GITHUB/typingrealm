using System;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Library.Books;
using TypingRealm.Testing;

namespace TypingRealm.Library.Tests;

public abstract class LibraryTestsBase : TestsBase
{
    protected LibraryTestsBase() : base(AutoDomainDataAttribute.CreateFixture())
    {
    }

    protected override IServiceCollection GetServiceCollection()
    {
        return base.GetServiceCollection()
            .AddInMemoryInfrastructure();
    }
}

public class AutoDomainDataAttribute : AutoDataAttribute
{
    public AutoDomainDataAttribute() : base(() => CreateFixture()) { }

    public static IFixture CreateFixture()
    {
        var fixture = AutoMoqDataAttribute.CreateFixture()
            .Customize(new TextProcessing.Tests.DomainCustomization())
            .Customize(new DomainCustomization());

        fixture.Register(() => fixture.Build<Book.State>()
            .With(x => x.ProcessingStatus, ProcessingStatus.Processed)
            .Create());

        return fixture;
    }
}

public sealed class InlineDomainDataAttribute : InlineAutoDataAttribute
{
    public InlineDomainDataAttribute(params object[] objects)
        : base(new AutoDomainDataAttribute(), objects) { }
}

public class DomainCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Register(() => Book.FromState(fixture.Create<Book.State>()));
    }
}

public static class DomainExtensions
{
    public static Book CreateBook(this IFixture fixture, Func<IPostprocessComposer<Book.State>, IPostprocessComposer<Book.State>> config)
    {
        var composer = fixture.Build<Book.State>()
            .With(x => x.ProcessingStatus, ProcessingStatus.Processed);

        var postProcess = config(composer);

        var state = postProcess.Create();
        return Book.FromState(state);
    }
}
