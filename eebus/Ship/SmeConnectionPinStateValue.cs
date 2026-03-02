namespace eebus.Ship;

/// <summary>
/// Pin state values
/// </summary>
internal enum PinStateType
{
    [JsonStringEnumMemberName("required")]
    Required,
    [JsonStringEnumMemberName("optional")]
    Optional,
    [JsonStringEnumMemberName("pinOk")]
    PinOk,
    [JsonStringEnumMemberName("none")]
    None,
}

/// <summary>
/// Permission for pin input
/// </summary>
internal enum PinInputPermissionType
{
    [JsonStringEnumMemberName("busy")]
    Busy,
    [JsonStringEnumMemberName("ok")]
    Ok,
}

/// <summary>
/// Connection pin state message
/// </summary>
internal record ConnectionPinStateType(
    [property: JsonPropertyName("pinState")] PinStateType PinState,
    [property: JsonPropertyName("inputPermission")] PinInputPermissionType? InputPermission = null);

/// <summary>
/// Connection pin input message
/// </summary>
internal record ConnectionPinInputType([property: JsonPropertyName("pin")] string Pin);

/// <summary>
/// Connection pin error message
/// </summary>
internal record ConnectionPinErrorType([property: JsonPropertyName("error")] byte Error);

/// <summary>
/// Connection close phase
/// </summary>
internal enum ConnectionClosePhaseType
{
    [JsonStringEnumMemberName("announce")]
    Announce,
    [JsonStringEnumMemberName("confirm")]
    Confirm,
}

/// <summary>
/// Connection close reason
/// </summary>
internal enum ConnectionCloseReasonType
{
    [JsonStringEnumMemberName("unspecific")]
    Unspecific,
    [JsonStringEnumMemberName("removedConnection")]
    RemovedConnection,
}

/// <summary>
/// Connection close message
/// </summary>
internal record ConnectionCloseType(
    [property: JsonPropertyName("phase")] ConnectionClosePhaseType Phase,
    [property: JsonPropertyName("maxTime")] uint? MaxTime = null,
    [property: JsonPropertyName("reason")] ConnectionCloseReasonType? Reason = null);

