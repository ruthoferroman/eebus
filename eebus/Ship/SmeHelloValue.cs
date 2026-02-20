using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Ship;


/// <summary>
/// hello message
/// </summary
internal record SmeHelloValue([property: JsonPropertyName("connectionHello")] ConnectionHelloType ConnectionHello);

internal enum ConnectionHelloPhaseType
{
    [JsonStringEnumMemberName("aborted")]
    Aborted,
    [JsonStringEnumMemberName("ready")]
    Ready,
    [JsonStringEnumMemberName("pending")]
    Pending,

}
/// <summary>
/// 
/// </summary>
/// <param name="Phase"></param>
/// <param name="Waiting"></param>
/// <param name="ProlongationRequest">true=request waiting prolongation</param>
/// <param name="KeyMaterialState">For SHIP versions ≥ 1.1, it is mandatory to send this field. For SHIP versions < 1.1, this field is 2118 undefined(thus "optional"). A recipient SHALL NOT consider it an error if the element is absent</param>
internal record ConnectionHelloType([property: JsonPropertyName("phase")] ConnectionHelloPhaseType Phase, 
    [property: JsonPropertyName("waiting")]  uint? Waiting = null, 
    [property: JsonPropertyName("prolongationRequest")] bool? ProlongationRequest = null, 
    [property: JsonPropertyName("keyMaterialState")] KeyMaterialState[]? KeyMaterialState = null);

internal record KeyMaterialState([property: JsonPropertyName("updateCounter")] ushort? UpdateCounter = null);


internal static class SmeHelloValueEncoder
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new SmeHelloValueJsonConverter()
        }
    };
    public static void Encode(this SmeHelloValue connectionHello, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Control);
        string json = JsonSerializer.Serialize(connectionHello, serializerOptions);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }

    public static SmeHelloValue? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Control)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<SmeHelloValue>(json, serializerOptions);
    }
}

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