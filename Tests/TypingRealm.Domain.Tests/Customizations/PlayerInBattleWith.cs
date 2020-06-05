using System.Reflection;
using AutoFixture.Kernel;

namespace TypingRealm.Domain.Tests.Customizations
{
    public sealed class PlayerInBattleWith : ISpecimenBuilder
    {
        private readonly PlayerId _enemyId;

        public PlayerInBattleWith(PlayerId enemyId)
        {
            _enemyId = enemyId;
        }

        public object? Create(object request, ISpecimenContext context)
        {
            if (!(request is ParameterInfo pi)
                || pi.ParameterType != typeof(PlayerId)
                || pi.Name != "combatEnemyId")
                return new NoSpecimen();

            return _enemyId;
        }
    }
}
