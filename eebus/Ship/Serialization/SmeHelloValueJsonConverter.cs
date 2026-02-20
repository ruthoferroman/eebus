namespace eebus.Ship.Serialization;

// EEBus has a special way to convert json - so we need a custom converter to handle the SmeHelloValue type correctly. The JSON structure is a bit unusual, with an array of objects for the "connectionHello" property, so we need to read and write it manually in the converter.

internal class SmeHelloValueJsonConverter : JsonConverter<SmeHelloValue>
{
    static readonly JsonStringEnumConverter enumConverter = new(JsonNamingPolicy.CamelCase);

    public override SmeHelloValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        ConnectionHelloPhaseType? phase = null;
        uint? waiting = null;
        bool? prolongationRequest = null;

        // Read start object
        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "connectionHello")
            throw new JsonException("Expected 'connectionHello' property.");

        // Read property name
        reader.Read();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'connectionHello'.");

        // Read array elements
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'connectionHello' array.");

            // Read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in 'connectionHello' object.");

                string propName = reader.GetString();
                reader.Read();

                switch (propName)
                {
                    case "phase":
                        phase = Enum.Parse<ConnectionHelloPhaseType>(reader.GetString(), ignoreCase: true);
                        break;
                    case "waiting":
                        waiting = reader.GetUInt32();
                        break;
                    case "prolongationRequest":
                        prolongationRequest = reader.GetBoolean();
                        break;
                        // Add more cases if needed
                }
            }
        }

        // Read end object
        reader.Read();

        if (phase == null)
            throw new JsonException("Missing required 'phase' property.");

        var connectionHello = new ConnectionHelloType(phase.Value, waiting, prolongationRequest, null);
        return new SmeHelloValue(connectionHello);
    }


    public override void Write(Utf8JsonWriter writer, SmeHelloValue value, JsonSerializerOptions options)
    {
        var phaseConverter = (JsonConverter<ConnectionHelloPhaseType>)enumConverter.CreateConverter(typeof(ConnectionHelloPhaseType), options);

        writer.WriteStartObject();
        writer.WritePropertyName("connectionHello");
        writer.WriteStartArray();

        writer.WriteStartObject();
        writer.WritePropertyName("phase");
        phaseConverter.Write(writer, value.ConnectionHello.Phase, options);
        writer.WriteEndObject();

        if (value.ConnectionHello.Waiting.HasValue)
        {
            writer.WriteStartObject();
            writer.WriteNumber("waiting", value.ConnectionHello.Waiting.Value);
            writer.WriteEndObject();
        }
        if (value.ConnectionHello.ProlongationRequest.HasValue)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("prolongationRequest", value.ConnectionHello.ProlongationRequest.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}