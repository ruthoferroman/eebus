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
    [property: JsonPropertyName("formats")] MessageProtocolFormatsType Formats);

public record MessageProtocolFormatsType(
    [property: JsonPropertyName("format")] IReadOnlyList<string> Formats
);

internal record MessageProtocolHandshakeTypeVersion([property: JsonPropertyName("major")] ushort Major, [property: JsonPropertyName("minor")] ushort Minor);


