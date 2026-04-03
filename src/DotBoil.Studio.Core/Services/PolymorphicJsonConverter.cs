using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotBoil.Studio.Core.Services;

/// <summary>
/// Generic polymorphic JSON converter that persists concrete type information via a "$type" field.
/// Supports any user-defined subclass without pre-registration.
/// </summary>
public class PolymorphicJsonConverter<T> : JsonConverter<T> where T : class
{
    private const string TypeDiscriminator = "$type";

    // Intercept only the abstract base type — prevents infinite recursion when serializing concrete types
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        if (!doc.RootElement.TryGetProperty(TypeDiscriminator, out var typeProp))
            throw new JsonException($"Missing '{TypeDiscriminator}' property.");

        var typeName = typeProp.GetString()
            ?? throw new JsonException($"'{TypeDiscriminator}' is null.");

        var concreteType = Type.GetType(typeName)
            ?? AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
                .FirstOrDefault(t => t.AssemblyQualifiedName == typeName || t.FullName == typeName)
            ?? throw new JsonException($"Cannot resolve type '{typeName}'.");

        // CanConvert(concreteType) == false  →  no recursion
        return (T?)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), concreteType, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var type = value.GetType();

        // Serialize the concrete instance. CanConvert(concreteType) == false → no recursion.
        // Nested List<T> members (e.g. ContainerComponent.Children) do go through this converter
        // because their declared element type is T (the abstract base).
        var json = JsonSerializer.Serialize(value, type, options);
        using var doc = JsonDocument.Parse(json);

        writer.WriteStartObject();
        writer.WriteString(TypeDiscriminator, type.FullName);
        foreach (var prop in doc.RootElement.EnumerateObject())
            prop.WriteTo(writer);
        writer.WriteEndObject();
    }
}

public class HttpMethodJsonConverter : JsonConverter<HttpMethod>
{
    public override HttpMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, HttpMethod value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Method);
}

/// <summary>
/// Skips serialization of System.Type properties (e.g. RendererType) since they are
/// computed from the concrete type and cannot be meaningfully round-tripped via JSON.
/// </summary>
public class TypeJsonConverter : JsonConverter<Type>
{
    public override bool CanConvert(Type typeToConvert) => typeof(Type).IsAssignableFrom(typeToConvert);

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        => writer.WriteNullValue();
}