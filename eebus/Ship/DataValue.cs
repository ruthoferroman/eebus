using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Ship;

internal record DataValue([property: JsonPropertyName("data")] DataType[] Data);
internal record DataType([property: JsonPropertyName("header")] HeaderType[] Header, [property: JsonPropertyName("payload")] byte[] Payload, [property: JsonPropertyName("extension")] ExtensionType[]? extension);

internal record HeaderType([property: JsonPropertyName("protocolId")] string ProtocolId);
internal record ExtensionType([property: JsonPropertyName("extensionId")] string? ExtensionId, [property: JsonPropertyName("binary")] byte[]? Binary, [property: JsonPropertyName("string")] string? String);

internal static class DataValueEncoder
{
    public static void Encode(this DataValue connectionHello, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Data);
        string json = JsonSerializer.Serialize(connectionHello);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }

    public static DataValue? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Data)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<DataValue>(json);
    }
}