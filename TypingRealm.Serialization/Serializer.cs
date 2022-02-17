using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypingRealm.Serialization;

public interface ISerializer
{
    T? Deserialize<T>(string json);
    object? Deserialize(string json, Type type);
    string Serialize<T>(T value);
}

// TODO: Consider not serializing properties that have default values (0, null, etc).
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
        if (typeof(T).IsPrimitive
            || (typeof(T) == typeof(string) && !json.StartsWith("\"", StringComparison.Ordinal)))
            return (T)Convert.ChangeType(json, typeof(T));

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public object? Deserialize(string json, Type type)
    {
        if (type.IsPrimitive
            || (type == typeof(string) && !json.StartsWith("\"", StringComparison.Ordinal)))
            return Convert.ChangeType(json, type);

        return JsonSerializer.Deserialize(json, type, _options);
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}
