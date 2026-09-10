using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RpBuddy.Inventory.Actions;

namespace RpBuddy.Utils;

public sealed class ItemActionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(IItemActionBase);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var actionType = value.GetType();
        var typeName = $"{actionType.FullName}, {actionType.Assembly.GetName().Name}";

        var innerSerializer = CreateInnerSerializer(serializer);

        var jo = JObject.FromObject(value, innerSerializer);
        jo.Remove("$type");
        jo.AddFirst(new JProperty("$type", typeName));
        jo.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var jo = JObject.Load(reader);

        var typeName = jo["$type"]?.Value<string>();

        Type targetType;

        if (!string.IsNullOrWhiteSpace(typeName))
        {
            targetType = Type.GetType(typeName)
                         ?? throw new JsonSerializationException($"Could not resolve action type '{typeName}'.");

            if (!typeof(IItemActionBase).IsAssignableFrom(targetType))
                throw new JsonSerializationException($"Type '{typeName}' does not implement {nameof(IItemActionBase)}.");

            jo.Remove("$type");
        }
        else
        {
            if (objectType.IsInterface || objectType.IsAbstract)
            {
                throw new JsonSerializationException(
                    $"Missing $type discriminator for {nameof(IItemActionBase)} entry at path '{reader.Path}'.");
            }

            targetType = objectType;
        }

        var innerSerializer = CreateInnerSerializer(serializer);

        using var subReader = jo.CreateReader();
        return innerSerializer.Deserialize(subReader, targetType);
    }

    private static JsonSerializer CreateInnerSerializer(JsonSerializer serializer)
    {
        var innerSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = serializer.NullValueHandling,
            DefaultValueHandling = serializer.DefaultValueHandling,
            ContractResolver = serializer.ContractResolver,
            Culture = serializer.Culture,
            DateFormatHandling = serializer.DateFormatHandling,
            DateParseHandling = serializer.DateParseHandling,
            DateTimeZoneHandling = serializer.DateTimeZoneHandling,
            Formatting = serializer.Formatting,
            ObjectCreationHandling = serializer.ObjectCreationHandling,
            ConstructorHandling = serializer.ConstructorHandling,
            MissingMemberHandling = serializer.MissingMemberHandling,
            ReferenceLoopHandling = serializer.ReferenceLoopHandling,
            PreserveReferencesHandling = serializer.PreserveReferencesHandling
        };

        foreach (var converter in serializer.Converters)
        {
            if (converter is not ItemActionConverter)
                innerSerializer.Converters.Add(converter);
        }

        // Exact-type CanConvert means this is only ever reached via ItemConverterType
        // on UseActions/ActionsToExecute - safe to leave unregistered here.
        return innerSerializer;
    }

    public override bool CanWrite => true;
    public override bool CanRead => true;
}