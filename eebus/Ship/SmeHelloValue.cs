namespace eebus.Ship;

/// <summary>
/// hello message
/// </summary
internal record SmeHelloValue([property: JsonPropertyName("connectionHello")] ConnectionHelloType ConnectionHello);

internal enum ConnectionHelloPhaseType
{
    [JsonStringEnumMemberName("aborted")]
    Aborted,
    [JsonStringEnumMemberName("ready")]
    Ready,
    [JsonStringEnumMemberName("pending")]
    Pending,

}
/// <summary>
/// 
/// </summary>
/// <param name="Phase"></param>
/// <param name="Waiting"></param>
/// <param name="ProlongationRequest">true=request waiting prolongation</param>
/// <param name="KeyMaterialState">For SHIP versions ≥ 1.1, it is mandatory to send this field. For SHIP versions < 1.1, this field is 2118 undefined(thus "optional"). A recipient SHALL NOT consider it an error if the element is absent</param>
internal record ConnectionHelloType([property: JsonPropertyName("phase")] ConnectionHelloPhaseType Phase, 
    [property: JsonPropertyName("waiting")]  uint? Waiting = null, 
    [property: JsonPropertyName("prolongationRequest")] bool? ProlongationRequest = null, 
    [property: JsonPropertyName("keyMaterialState")] KeyMaterialState[]? KeyMaterialState = null);

internal record KeyMaterialState([property: JsonPropertyName("updateCounter")] ushort? UpdateCounter = null);
