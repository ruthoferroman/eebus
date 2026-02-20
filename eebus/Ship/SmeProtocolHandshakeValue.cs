using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Ship;



/// <summary>
/// handshake message
/// </summary>
internal record SmeProtocolHandshakeValue([property: JsonPropertyName("messageProtocolHandshake")] MessageProtocolHandshakeType MessageProtocolHandshake);
internal enum ProtocolHandshakeTypeType
{
    [JsonStringEnumMemberName("announceMax")]
    AnnounceMax,
    [JsonStringEnumMemberName("select")]
    Select,
}

internal record MessageProtocolHandshakeType([property: JsonPropertyName("handshakeType")] ProtocolHandshakeTypeType HandshakeType,
    [property: JsonPropertyName("version")] MessageProtocolHandshakeTypeVersion Version,
    [property: JsonPropertyName("formats")] string[] Formats);

internal record MessageProtocolHandshakeTypeVersion([property: JsonPropertyName("major")] ushort Major, [property: JsonPropertyName("minor")] ushort Minor);

internal static class SmeProtocolHandshakeValueEncoder
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {   new SmeProtocolHandshakeValueJsonConverter()
        }
    };

    public static void Encode(this SmeProtocolHandshakeValue msg, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Control);
        string json = JsonSerializer.Serialize(msg, serializerOptions);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }

    public static SmeProtocolHandshakeValue? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Control)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<SmeProtocolHandshakeValue>(json, serializerOptions);
    }
}



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
        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "messageProtocolHandshake")
            throw new JsonException("Expected 'messageProtocolHandshake' property.");

        // Read property name
        reader.Read();

        if (reader.TokenType != JsonTokenType.StartArray)
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
                reader.Read();

                switch (propName)
                {
                    case "handshakeType":
                        var enumConverter = (JsonConverter<ProtocolHandshakeTypeType>)SmeProtocolHandshakeValueJsonConverter.enumConverter.CreateConverter(typeof(ProtocolHandshakeTypeType), options);
                        handshakeType = enumConverter.Read(ref reader, typeof(ProtocolHandshakeTypeType), options);
                        break;
                    case "version":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'version'.");
                        // Read major and minor
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName)
                            {
                                var versionProp = reader.GetString();
                                reader.Read();
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
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "format")
                            {
                                reader.Read();
                                formats.Add(reader.GetString());
                            }
                        }
                        break;
                }
            }
        }

        reader.Read(); // EndObject

        if (handshakeType == null || major == null || minor == null)
            throw new JsonException("Missing required handshakeType or version fields.");

        var version = new MessageProtocolHandshakeTypeVersion(major.Value, minor.Value);
        var handshake = new MessageProtocolHandshakeType(handshakeType.Value, version, formats.ToArray());
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

        writer.WriteStartObject();
        writer.WriteStartArray();
        writer.WriteNumber("major", value.MessageProtocolHandshake.Version.Major);
        writer.WriteNumber("minor", value.MessageProtocolHandshake.Version.Minor);
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.WritePropertyName("formats");
        writer.WriteStartArray();
        foreach (var format in value.MessageProtocolHandshake.Formats)
        {
            writer.WriteString("format",format);
        }
        writer.WriteEndArray();

        writer.WriteEndArray();
        writer.WriteEndObject();


      
    }
}