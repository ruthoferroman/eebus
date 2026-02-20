namespace eebus.Ship.Serialization;

internal class SmeProtocolHandshakeValueJsonConverter : JsonConverter<SmeProtocolHandshakeValue>
{
    static readonly JsonStringEnumConverter enumConverter = new(JsonNamingPolicy.CamelCase);

    public override SmeProtocolHandshakeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        ProtocolHandshakeTypeType? handshakeType = null;
        ushort? major = null;
        ushort? minor = null;
        List<string> formats = new();

        // Read start object
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "messageProtocolHandshake")
            throw new JsonException("Expected 'messageProtocolHandshake' property.");

        // Move to the value (should be StartArray)
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'messageProtocolHandshake'.");

        // Read array elements
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'messageProtocolHandshake' array.");

            // Read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in 'messageProtocolHandshake' object.");

                string propName = reader.GetString();
                // Move to property value
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading property value.");

                switch (propName)
                {
                    case "handshakeType":
                        var handshakeConverter = (JsonConverter<ProtocolHandshakeTypeType>)enumConverter.CreateConverter(typeof(ProtocolHandshakeTypeType), options);
                        handshakeType = handshakeConverter.Read(ref reader, typeof(ProtocolHandshakeTypeType), options);
                        break;

                    case "version":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'version'.");

                        // Read version array elements (objects with major/minor)
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException("Expected start of object in 'version' array.");

                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    throw new JsonException("Expected property name in 'version' object.");

                                var versionProp = reader.GetString();
                                if (!reader.Read())
                                    throw new JsonException("Unexpected end of JSON in 'version' object.");

                                if (versionProp == "major")
                                    major = reader.GetUInt16();
                                else if (versionProp == "minor")
                                    minor = reader.GetUInt16();
                            }
                        }
                        break;

                    case "formats":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'formats'.");

                        // Read formats array elements (objects with format -> array of string(s))
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException("Expected start of object in 'formats' array.");

                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    throw new JsonException("Expected property name in 'formats' object.");

                                var fmtProp = reader.GetString();

                                if (!reader.Read())
                                    throw new JsonException("Unexpected end of JSON in 'formats' object.");

                                if (fmtProp == "format")
                                {
                                    if (reader.TokenType != JsonTokenType.StartArray)
                                        throw new JsonException("Expected start of array for 'format'.");

                                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                    {
                                        if (reader.TokenType == JsonTokenType.String)
                                        {
                                            formats.Add(reader.GetString());
                                        }
                                        else
                                        {
                                            // skip unexpected tokens inside format array
                                        }
                                    }
                                }
                                else
                                {
                                    // Unexpected property inside formats object; skip its value by reading through
                                    // If it's an object/array we need to advance appropriately - simplest: throw
                                    throw new JsonException($"Unexpected property '{fmtProp}' in formats object.");
                                }
                            }
                        }
                        break;

                    default:
                        throw new JsonException($"Unexpected property '{propName}' in messageProtocolHandshake object.");
                }
            }
        }

        // Move past the EndObject of the root
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        if (handshakeType == null || major == null || minor == null)
            throw new JsonException("Missing required handshakeType or version fields.");

        var version = new MessageProtocolHandshakeTypeVersion(major.Value, minor.Value);
        var formatsType = new MessageProtocolFormatsType(formats);
        var handshake = new MessageProtocolHandshakeType(handshakeType.Value, version, formatsType);
        return new SmeProtocolHandshakeValue(handshake);
    }


    public override void Write(Utf8JsonWriter writer, SmeProtocolHandshakeValue value, JsonSerializerOptions options)
    {
        var handshakeTypeConverter = (JsonConverter<ProtocolHandshakeTypeType>)enumConverter.CreateConverter(typeof(ProtocolHandshakeTypeType), options);

        writer.WriteStartObject();
        writer.WritePropertyName("messageProtocolHandshake");
        writer.WriteStartArray();

        writer.WriteStartObject();
        writer.WritePropertyName("handshakeType");
        handshakeTypeConverter.Write(writer, value.MessageProtocolHandshake.HandshakeType, options);
        writer.WriteEndObject();

        //version
        writer.WriteStartObject();
        writer.WritePropertyName("version");
        writer.WriteStartArray();

        writer.WriteStartObject();
        writer.WriteNumber("major", value.MessageProtocolHandshake.Version.Major);
        writer.WriteEndObject();
        writer.WriteStartObject();
        writer.WriteNumber("minor", value.MessageProtocolHandshake.Version.Minor);
        writer.WriteEndObject();

        writer.WriteEndArray();
        writer.WriteEndObject();

        // formats
        writer.WriteStartObject();
        writer.WritePropertyName("formats");
        writer.WriteStartArray();
        foreach (var format in value.MessageProtocolHandshake.Formats.Formats)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("format");
            writer.WriteStartArray();
            writer.WriteStringValue(format);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}