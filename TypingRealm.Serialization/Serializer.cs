using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypingRealm.Serialization;

public interface ISerializer
{
    T? Deserialize<T>(string json);
    string Serialize<T>(T value);
}

public sealed class Serializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public Serializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public T? Deserialize<T>(string json)
    {
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            return (T)Convert.ChangeType(json, typeof(T));

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}
