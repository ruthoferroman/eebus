namespace eebus.Ship.Serialization;

internal static class ConnectionPinStateEncoder 
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {   new ConnectionPinStateJsonConverter()
        }
    };

    public static void Encode(this ConnectionPinStateType msg, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShipMessageType.Control);
        string json = JsonSerializer.Serialize(msg, serializerOptions);
        binaryWriter.Write(Encoding.UTF8.GetBytes(json));
    }
}
