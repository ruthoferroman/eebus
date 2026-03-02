namespace eebus.Ship.Serialization;

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
