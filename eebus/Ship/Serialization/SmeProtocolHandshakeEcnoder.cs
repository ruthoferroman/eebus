namespace eebus.Ship.Serialization;

internal static class SmeProtocolHandshakeEcnoder
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {   new SmeProtocolHandshakeJsonConverter()
        }
    };

    public static object? Decode(BinaryReader binaryReader, int msgLength)
    {
        var ctrl = binaryReader.ReadByte();
        if (ctrl != (byte)ShipMessageType.Control)
            throw new InvalidDataException($"Expected Control message type, but got {ctrl}");
        var json = Encoding.UTF8.GetString(binaryReader.ReadBytes(msgLength - 1));

        return JsonSerializer.Deserialize<object>(json, serializerOptions);
    }
}

