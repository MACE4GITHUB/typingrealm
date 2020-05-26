using System.Reflection;
using AutoFixture.Kernel;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders
{
    public class ClientWithIdSpecimenBuilder : ISpecimenBuilder
    {
        private readonly string _clientId;

        public ClientWithIdSpecimenBuilder(string clientId)
        {
            _clientId = clientId;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is ParameterInfo pi)
                || pi.ParameterType != typeof(string)
                || pi.Name != "clientId")
                return new NoSpecimen();

            return _clientId;
        }
    }
}
