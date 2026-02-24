using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Spine;

/// <summary>
/// Represents control options for a command (delete/partial flags or tags).
/// ElementTagType in the schema can be arbitrary; keep flexible with JsonElement.
/// </summary>
internal record CmdControlType(
    [property: JsonPropertyName("delete")] JsonElement? Delete = null,
    [property: JsonPropertyName("partial")] JsonElement? Partial = null);

/// <summary>
/// Filter type: optional filterId, optional cmdControl and optional selector/element groups.
/// Unknown selector/element structures are preserved as JsonElement arrays for flexibility.
/// </summary>
internal record FilterType(
    [property: JsonPropertyName("filterId")] uint? FilterId = null,
    [property: JsonPropertyName("cmdControl")] CmdControlType? CmdControl = null,
    // Data selectors and data elements are schema-defined groups; keep as raw JSON to remain tolerant
    [property: JsonPropertyName("dataSelectors")] JsonElement[]? DataSelectors = null,
    [property: JsonPropertyName("dataElements")] JsonElement[]? DataElements = null);

/// <summary>
/// CmdType aggregates payload-contribution pieces:
/// - optional function (schema-defined FunctionType) preserved as JsonElement
/// - optional repeated filters
/// - optional manufacturerSpecificExtension (hexBinary -> byte[])
/// - optional lastUpdateAt (time type) preserved as JsonElement
/// 
/// This representation is intentionally tolerant: complex schema parts are captured as JsonElement
/// so that arbitrary command/frame content can be preserved and inspected at runtime.
/// </summary>
internal record CmdType(
    [property: JsonPropertyName("function")] JsonElement? Function = null,
    [property: JsonPropertyName("filter")] FilterType[]? Filter = null,
    [property: JsonPropertyName("manufacturerSpecificExtension")] byte[]? ManufacturerSpecificExtension = null,
    [property: JsonPropertyName("lastUpdateAt")] JsonElement? LastUpdateAt = null,
    // Additional arbitrary data selectors / elements that may appear inline with the command
    [property: JsonPropertyName("data")] JsonElement[]? Data = null);