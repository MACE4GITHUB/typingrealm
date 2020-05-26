using System.Reflection;
using AutoFixture.Kernel;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders
{

    public class ClientWithGroupBuilder : ISpecimenBuilder
    {
        private readonly string _group;

        public ClientWithGroupBuilder(string group)
        {
            _group = group;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is ParameterInfo pi)
                || pi.ParameterType != typeof(string)
                || pi.Name != "group")
                return new NoSpecimen();

            return _group;
        }
    }
}
