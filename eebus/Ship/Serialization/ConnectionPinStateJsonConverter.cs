namespace eebus.Ship.Serialization;

internal class ConnectionPinStateJsonConverter : JsonConverter<ConnectionPinStateType>
{
    static readonly JsonStringEnumConverter enumConverter = new(JsonNamingPolicy.CamelCase);

    public override ConnectionPinStateType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        PinStateType? pinState = null;
        PinInputPermissionType? inputPermission = null;

        // Read start object
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "connectionPinState")
            throw new JsonException("Expected 'connectionPinState' property.");

        // Move to the value (should be StartArray)
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'connectionPinState'.");

        // Read array elements
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'connectionPinState' array.");

            // Read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in 'connectionPinState' object.");

                string propName = reader.GetString();

                // Move to property value
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading property value.");

                switch (propName)
                {
                    case "pinState":
                        var pinStateConverter = (JsonConverter<PinStateType>)enumConverter.CreateConverter(typeof(PinStateType), options);
                        pinState = pinStateConverter.Read(ref reader, typeof(PinStateType), options);
                        break;

                    case "inputPermission":
                        var inputPermissionConverter = (JsonConverter<PinInputPermissionType>)enumConverter.CreateConverter(typeof(PinInputPermissionType), options);
                        inputPermission = inputPermissionConverter.Read(ref reader, typeof(PinInputPermissionType), options);
                        break;

                    default:
                        throw new JsonException($"Unexpected property '{propName}' in connectionPinState object.");
                }
            }
        }

        // Move past EndObject of root
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        if (pinState == null)
            throw new JsonException("Missing required 'pinState' property.");

        return new ConnectionPinStateType(pinState.Value, inputPermission);
    }

    public override void Write(Utf8JsonWriter writer, ConnectionPinStateType value, JsonSerializerOptions options)
    {
        var pinStateConverter = (JsonConverter<PinStateType>)enumConverter.CreateConverter(typeof(PinStateType), options);

        writer.WriteStartObject();
        writer.WritePropertyName("connectionPinState");
        writer.WriteStartArray();

        writer.WriteStartObject();
        writer.WritePropertyName("pinState");
        pinStateConverter.Write(writer, value.PinState, options);
        writer.WriteEndObject();

        if ( value.InputPermission.HasValue)
        {
            var inputPermissionConverter = (JsonConverter<PinInputPermissionType>)enumConverter.CreateConverter(typeof(PinInputPermissionType), options);
            writer.WriteStartObject();
            writer.WritePropertyName("inputPermission");
            inputPermissionConverter.Write(writer, value.InputPermission.Value, options);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
