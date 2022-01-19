using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using TypingRealm.Common;
using TypingRealm.Testing;

namespace TypingRealm.TextProcessing.Tests
{
    public sealed class AutoDomainDataAttribute : AutoDataAttribute
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
            fixture.Register(() => TextConstants.EnglishLanguage);
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
}
