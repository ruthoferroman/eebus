namespace eebus.Ship;

/// <summary>
/// Header contains the protocol identifier.
/// </summary>
internal record HeaderType([property: JsonPropertyName("protocolId")] string ProtocolId);

/// <summary>
/// Optional extension element. 'binary' is hexBinary in XSD — map to byte[].
/// </summary>
internal record ExtensionType(
    [property: JsonPropertyName("extensionId")] string? ExtensionId = null,
    [property: JsonPropertyName("binary")] byte[]? Binary = null,
    [property: JsonPropertyName("string")] string? String = null);

/// <summary>
/// Data element: header, payload (treated as bytes) and optional extension.
/// Matches XSD: header, payload (xs:anyType -> byte[] for binary payload), extension.
/// </summary>
internal record DataType(
    [property: JsonPropertyName("header")] HeaderType[] Header,
    [property: JsonPropertyName("payload")] byte[] Payload,
    [property: JsonPropertyName("extension")] ExtensionType? Extension = null);

/// <summary>
/// Root container for data messages.
/// </summary>
internal record DataValue([property: JsonPropertyName("data")] DataType[] Data);