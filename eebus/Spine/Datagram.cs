using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Spine;

/// <summary>
/// SPINE feature address (partial, adapted from observed JSON).
/// The real schema (EEBus SPINE) contains more detailed definitions; keep fields optional to be tolerant.
/// </summary>
internal record FeatureAddressType(
    [property: JsonPropertyName("device")] string? Device = null,
    [property: JsonPropertyName("entity")] int[]? Entity = null,
    [property: JsonPropertyName("feature")] int? Feature = null
);

/// <summary>
/// SPINE header as used inside a datagram.
/// All fields are optional according to the XSD; types chosen to be tolerant for JSON payloads.
/// </summary>
internal record HeaderType(
    [property: JsonPropertyName("specificationVersion")] string? SpecificationVersion = null,
    [property: JsonPropertyName("addressSource")] FeatureAddressType? AddressSource = null,
    [property: JsonPropertyName("addressDestination")] FeatureAddressType? AddressDestination = null,
    [property: JsonPropertyName("addressOriginator")] FeatureAddressType? AddressOriginator = null,
    [property: JsonPropertyName("msgCounter")] uint? MsgCounter = null,
    [property: JsonPropertyName("msgCounterReference")] uint? MsgCounterReference = null,
    [property: JsonPropertyName("cmdClassifier")] string? CmdClassifier = null,
    [property: JsonPropertyName("ackRequest")] bool? AckRequest = null,
    [property: JsonPropertyName("timestamp")] JsonElement? Timestamp = null
);

/// <summary>
/// Payload contains zero or more cmd elements. CmdType is left as JsonElement to preserve arbitrary command-frame structure.
/// </summary>
internal record PayloadType(
    [property: JsonPropertyName("cmd")] CmdType[]? Cmd = null
);

/// <summary>
/// Datagram root type (header + payload)
/// </summary>
internal record DatagramType(
    [property: JsonPropertyName("header")] HeaderType Header,
    [property: JsonPropertyName("payload")] PayloadType Payload
);