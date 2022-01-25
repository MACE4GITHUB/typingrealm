using System.Reflection;
using AutoFixture.Kernel;

namespace TypingRealm.Messaging.Tests.SpecimenBuilders;

public class ClientWithConnectionBuilder : ISpecimenBuilder
{
    private readonly IConnection _connection;

    public ClientWithConnectionBuilder(IConnection connection)
    {
        _connection = connection;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (!(request is ParameterInfo pi)
            || pi.ParameterType != typeof(IConnection))
            return new NoSpecimen();

        return _connection;
    }
}
