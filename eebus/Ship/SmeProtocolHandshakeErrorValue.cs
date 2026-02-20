using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Ship;


internal static class SmeProtocolHandshakeErrorValueEncoder
{
    public static void Encode(this SmeProtocolHandshakeErrorValue msg, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Control);
        string json = JsonSerializer.Serialize(msg);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }

    public static SmeProtocolHandshakeErrorValue? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Control)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<SmeProtocolHandshakeErrorValue>(json);
    }
}
/// <summary>
/// handshake error message
/// </summary>
internal record SmeProtocolHandshakeErrorValue([property: JsonPropertyName("messageProtocolHandshakeError")] MessageProtocolHandshakeErrorType[] MessageProtocolHandshakeError);
internal record MessageProtocolHandshakeErrorType([property: JsonPropertyName("error")] ProtocolHandshakeErrorType Error);

internal enum ProtocolHandshakeErrorType
{
    RFU = 0,
    Timeout = 1,
    UnexpectedMessage = 2,
    SelectionMismatch = 3
}
