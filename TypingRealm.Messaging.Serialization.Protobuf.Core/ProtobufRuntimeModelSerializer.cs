using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf.Meta;

namespace TypingRealm.Messaging.Serialization.Protobuf;

// Tested through ProtobufStreamSerializer & ProtobufMessageSerializer.
// TODO: Consider writing unit tests for this abstract class, or splitting the logic.
public abstract class ProtobufRuntimeModelSerializer
{
    protected RuntimeTypeModel Model { get; }

    protected ProtobufRuntimeModelSerializer(IEnumerable<Type> types, IDictionary<Type, IEnumerable<Type>>? subTypes = null)
    {
        Model = RuntimeTypeModel.Create();
        foreach (var type in types)
        {
            var metaType = Model.Add(type, false)
                .Add(type
                    .GetProperties()
                    .Select(property => property.Name)
                    .OrderBy(name => name)
                    .ToArray());

            if (subTypes != null && subTypes.ContainsKey(type))
            {
                var subFieldNumber = 100;
                foreach (var subType in subTypes[type].OrderBy(x => x.Name))
                {
                    metaType.AddSubType(subFieldNumber++, subType);
                }
            }
        }

        // TODO: uncomment this for better performance.
        ////Model.Compile();
    }
}
