using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf;

// TODO: Unit test.
public abstract class ProtobufRuntimeModelSerializer
{
    protected RuntimeTypeModel Model { get; }

    protected ProtobufRuntimeModelSerializer(IEnumerable<Type> types)
    {
        Model = RuntimeTypeModel.Create();

        foreach (var type in types)
        {
            Model.Add(type, false)
                .Add(type
                    .GetProperties()
                    .Select(property => property.Name)
                    .OrderBy(name => name)
                    .ToArray());
        }

        // TODO: uncomment this for better performance.
        //Model.Compile();
    }
}
