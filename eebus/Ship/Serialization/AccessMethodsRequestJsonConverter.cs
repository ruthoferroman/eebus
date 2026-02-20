namespace eebus.Ship.Serialization;

internal class AccessMethodsRequestJsonConverter : JsonConverter<AccessMethodsRequestType>
{
    public override AccessMethodsRequestType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        // Move to first property
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "accessMethodsRequest")
            throw new JsonException("Expected 'accessMethodsRequest' property.");

        // Move to start of array
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'accessMethodsRequest'.");

        // Skip any array elements if present (XSD defines an empty complexType, but be tolerant)
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Skip object contents
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) { }
            }
            // otherwise ignore unexpected tokens until EndArray
        }

        // Move past EndArray to end object
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        return new AccessMethodsRequestType();
    }

    public override void Write(Utf8JsonWriter writer, AccessMethodsRequestType value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("accessMethodsRequest");
        writer.WriteStartArray();
        // empty complex type -> write empty array
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}