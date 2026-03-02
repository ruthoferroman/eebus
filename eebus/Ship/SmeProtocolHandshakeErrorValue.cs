namespace eebus.Ship;

/// <summary>
/// handshake error message
/// </summary>
internal record SmeProtocolHandshakeErrorValue([property: JsonPropertyName("messageProtocolHandshakeError")] MessageProtocolHandshakeErrorType MessageProtocolHandshakeError);
internal record MessageProtocolHandshakeErrorType([property: JsonPropertyName("error")] ProtocolHandshakeErrorType Error);

internal enum ProtocolHandshakeErrorType
{
    RFU = 0,
    Timeout = 1,
    UnexpectedMessage = 2,
    SelectionMismatch = 3
}
