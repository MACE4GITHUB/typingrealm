using System;
using FluentValidation;

namespace TypingRealm.Hosting;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> MustCreate<T, TProperty, TDomainEntity>(this IRuleBuilder<T, TProperty> ruleBuilder, Func<TProperty, TDomainEntity> factory)
    {
        string CreateDomainEntityAndGetErrorMessage(TProperty property)
        {
            try
            {
                factory(property);
                return string.Empty;
            }
            catch (Exception exception) when (
                exception is ArgumentNullException
                || exception is ArgumentException
                || exception is InvalidOperationException
                || exception is NotSupportedException)
            {
                return exception.Message;
            }
        }

        return ruleBuilder.Must(x => CreateDomainEntityAndGetErrorMessage(x).Length == 0)
            .WithMessage((_, property) => CreateDomainEntityAndGetErrorMessage(property));
    }
}


