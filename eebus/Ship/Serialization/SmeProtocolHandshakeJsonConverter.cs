namespace eebus.Ship.Serialization;

internal class SmeProtocolHandshakeJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        // Inspect a copy of the reader so we don't advance the original reader.
        var inspector = reader;
        if (!inspector.Read() || inspector.TokenType != JsonTokenType.PropertyName)
            throw new JsonException("Expected property.");

        var messagetype = inspector.GetString();
        if (messagetype == "messageProtocolHandshake")
        {
            var converter = new SmeProtocolHandshakeValueJsonConverter();
            return converter.Read(ref reader, typeof(SmeProtocolHandshakeValue), options);
        }
        else if (messagetype == "messageProtocolHandshakeError")
        {
            var converter = new SmeProtocolHandshakeErrorValueJsonConverter();
            return converter.Read(ref reader, typeof(SmeProtocolHandshakeErrorValue), options);
        }
        else
        {
            throw new JsonException($"Invalid message type: {messagetype}");
        }
    }
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        throw  new NotSupportedException("Serialization of SmeProtocolHandshakeValue is not supported. Use SmeProtocolHandshakeJsonConverter<SmeProtocolHandshakeValue> instead.");
    }
}