using System.Reflection;
using AutoFixture.Kernel;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders
{
    public class ClientWithUpdateDetectorBuilder : ISpecimenBuilder
    {
        private readonly IUpdateDetector _updateDetector;

        public ClientWithUpdateDetectorBuilder(IUpdateDetector updateDetector)
        {
            _updateDetector = updateDetector;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is ParameterInfo pi)
                || pi.ParameterType != typeof(IUpdateDetector))
                return new NoSpecimen();

            return _updateDetector;
        }
    }

}
