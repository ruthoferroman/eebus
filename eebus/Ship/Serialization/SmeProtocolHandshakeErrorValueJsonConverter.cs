namespace eebus.Ship.Serialization;

internal class SmeProtocolHandshakeErrorValueJsonConverter : JsonConverter<SmeProtocolHandshakeErrorValue>
{
    public override SmeProtocolHandshakeErrorValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        // Move to first property
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "messageProtocolHandshakeError")
            throw new JsonException("Expected 'messageProtocolHandshakeError' property.");

        // Move to start of array
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'messageProtocolHandshakeError'.");

        // Move into the array (either StartObject for the error object or EndArray if empty)
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON inside 'messageProtocolHandshakeError' array.");

        ProtocolHandshakeErrorType err = ProtocolHandshakeErrorType.RFU;

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Read the error object
            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON in error object.");

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "error")
                throw new JsonException("Expected 'error' property inside messageProtocolHandshakeError object.");

            // Move to number token
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException("Expected numeric 'error' value.");

            int errorValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(ProtocolHandshakeErrorType), errorValue))
                err = (ProtocolHandshakeErrorType)errorValue;
            else
                err = ProtocolHandshakeErrorType.RFU;

            // Move past the number to end object
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException("Expected end of error object.");

            // Move to next token which should be EndArray (or another array element)
            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON after error object.");

            // If there are additional array items, skip them until EndArray
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while skipping extra array items.");
            }
        }
        else if (reader.TokenType == JsonTokenType.EndArray)
        {
            // empty array, leave default err
        }
        else
        {
            throw new JsonException("Expected start of error object or end of array in 'messageProtocolHandshakeError'.");
        }

        // Move past EndArray to end object
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        return new SmeProtocolHandshakeErrorValue(new MessageProtocolHandshakeErrorType(err));
    }


    public override void Write(Utf8JsonWriter writer, SmeProtocolHandshakeErrorValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("messageProtocolHandshakeError");
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteNumber("error", (int)value.MessageProtocolHandshakeError.Error);
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();


    }
}