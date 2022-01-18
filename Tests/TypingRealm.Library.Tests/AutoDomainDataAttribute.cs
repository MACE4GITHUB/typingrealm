using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using TypingRealm.Common;
using TypingRealm.Library.Books;
using TypingRealm.Testing;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Tests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute() : base(() => CreateFixture()) { }

        public static IFixture CreateFixture()
        {
            return AutoMoqDataAttribute.CreateFixture()
                .Customize(new DomainCustomization());
        }
    }

    public class DomainCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new MaxLengthBuilder());
            fixture.Register(() => Constants.EnglishLanguage);
            fixture.Register(() => Book.FromState(fixture.Create<Book.State>()));
        }
    }

    // TODO: Move this builder to common testing, it works with any Primitives.
    public class MaxLengthBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is ParameterInfo pi
                && pi.ParameterType == typeof(string)
                && pi.Member.DeclaringType != null
                && pi.Member.DeclaringType.IsSubclassOf(typeof(Primitive<string>))
                && pi.Member.DeclaringType.GetField("MaxLength") != null)
            {
                var maxLength = Convert.ToInt32(pi.Member.DeclaringType.GetField("MaxLength")!.GetValue(null));

                var value = context.Create<string>();
                var length = Math.Min(maxLength, value.Length);

                return value[..length];
            }

            return new NoSpecimen();
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
