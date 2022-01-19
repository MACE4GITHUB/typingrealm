using System;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Xunit2;
using TypingRealm.Library.Books;
using TypingRealm.Testing;

namespace TypingRealm.Library.Tests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute() : base(() => CreateFixture()) { }

        public static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new TextProcessing.Tests.DomainCustomization())
                .Customize(new DomainCustomization());
        }
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
        public static Book CreateBook(this IFixture fixture, Func<ICustomizationComposer<Book.State>, IPostprocessComposer<Book.State>> config)
        {
            var composer = fixture.Build<Book.State>();

            var postProcess = config(composer);

            var state = postProcess.Create();
            return Book.FromState(state);
        }
    }
}
