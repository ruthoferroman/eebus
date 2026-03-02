namespace eebus.Ship.Serialization;

internal static class SmeProtocolHandshakeErrorValueEncoder
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {   new SmeProtocolHandshakeErrorValueJsonConverter()
        }
    };
    public static void Encode(this SmeProtocolHandshakeErrorValue msg, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Control);
        string json = JsonSerializer.Serialize(msg, serializerOptions);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }

    public static SmeProtocolHandshakeErrorValue? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Control)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<SmeProtocolHandshakeErrorValue>(json, serializerOptions);
    }
}
