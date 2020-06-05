using System.Reflection;
using AutoFixture.Kernel;

namespace TypingRealm.Domain.Tests.Customizations
{
    public sealed class PlayerNotInBattle : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            if (!(request is ParameterInfo pi)
                || pi.ParameterType != typeof(PlayerId)
                || pi.Name != "combatEnemyId")
                return new NoSpecimen();

            return null;
        }
    }
}
