namespace eebus.Ship;

/// <summary>
/// Empty access methods request type (placeholder)
/// </summary>
internal record AccessMethodsRequestType();

/// <summary>
/// dnsSd_mDns placeholder type (empty complexType in XSD)
/// </summary>
internal record AccessMethodsDnsSdMdnsType();

/// <summary>
/// dns type containing a single uri
/// </summary>
internal record AccessMethodsDnsType([property: JsonPropertyName("uri")] string Uri);

/// <summary>
/// Access methods information
/// </summary>
internal record AccessMethodsType(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("dnsSd_mDns")] AccessMethodsDnsSdMdnsType? DnsSd_mDns = null,
    [property: JsonPropertyName("dns")] AccessMethodsDnsType? Dns = null);
