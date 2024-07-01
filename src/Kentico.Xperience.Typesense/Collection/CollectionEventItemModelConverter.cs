using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kentico.Xperience.Typesense.Collection;
public class CollectionEventItemModelConverter : JsonConverter<ICollectionEventItemModel>
{
    public override ICollectionEventItemModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("serializedObject", out JsonElement typeElem))
            {
                throw new JsonException("Missing property 'serializedObject'");
            }

            string serializedObjectType = typeElem.GetString() ?? string.Empty;

            switch (serializedObjectType)
            {
                case nameof(CollectionEventWebPageItemModel):
                    return JsonSerializer.Deserialize<CollectionEventWebPageItemModel>(root.GetRawText(), options);
                case nameof(CollectionEventReusableItemModel):
                    return JsonSerializer.Deserialize<CollectionEventReusableItemModel>(root.GetRawText(), options);
                case nameof(EndOfRebuildItemModel):
                    return JsonSerializer.Deserialize<EndOfRebuildItemModel>(root.GetRawText(), options);
                default:
                    throw new NotSupportedException($"serializedObject '{serializedObjectType}' is not supported");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ICollectionEventItemModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Écrire les propriétés de l'objet
        foreach (var property in value.GetType().GetProperties())
        {
            var propertyValue = property.GetValue(value);
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
        }

        // Ajouter le champ serializedObject
        writer.WriteString("serializedObject", value.GetType().Name);

        writer.WriteEndObject();
    }
}

