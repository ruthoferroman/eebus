namespace eebus.Ship.Serialization;

internal class DataValueJsonConverter : JsonConverter<DataValue>
{
    public override DataValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of JSON object.");

        // move to first property
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON.");

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "data")
            throw new JsonException("Expected 'data' property.");

        // move to value (should be StartArray)
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for 'data'.");

        var dataList = new List<DataType>();

        // pending parts when header and payload are split into separate array elements
        HeaderType[]? pendingHeader = null;
        ExtensionType? pendingExtension = null;

        // read array elements
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object in 'data' array.");

            // local parts found in this element
            List<HeaderType>? headerListLocal = null;
            byte[]? payloadLocal = null;
            ExtensionType? extensionLocal = null;

            // read object properties
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected property name in data object.");

                string propName = reader.GetString()!;
                // move to property value
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading property value.");

                switch (propName)
                {
                    case "header":
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Expected start of array for 'header'.");

                        headerListLocal = new List<HeaderType>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException("Expected start of object in 'header' array.");

                            string? protocolId = null;
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    throw new JsonException("Expected property name in header object.");

                                var headerProp = reader.GetString()!;
                                if (!reader.Read())
                                    throw new JsonException("Unexpected end of JSON in header object.");

                                if (headerProp == "protocolId")
                                {
                                    if (reader.TokenType != JsonTokenType.String)
                                        throw new JsonException("Expected string value for 'protocolId'.");
                                    protocolId = reader.GetString();
                                }
                                else
                                {
                                    throw new JsonException($"Unexpected property '{headerProp}' in header object.");
                                }
                            }

                            if (protocolId == null)
                                throw new JsonException("Missing required 'protocolId' in header object.");

                            headerListLocal.Add(new HeaderType(protocolId));
                        }
                        break;

                    case "payload":
                        // payload may be a base64 string, null, or arbitrary JSON object/array.
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            var base64 = reader.GetString()!;
                            payloadLocal = Convert.FromBase64String(base64);
                        }
                        else if (reader.TokenType == JsonTokenType.Null)
                        {
                            payloadLocal = Array.Empty<byte>();
                        }
                        else
                        {
                            // consume arbitrary JSON value and store its raw JSON bytes
                            using var doc = JsonDocument.ParseValue(ref reader);
                            var raw = doc.RootElement.GetRawText();
                            payloadLocal = Encoding.UTF8.GetBytes(raw);
                        }
                        break;

                    case "extension":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            extensionLocal = null;
                        }
                        else
                        {
                            if (reader.TokenType != JsonTokenType.StartObject)
                                throw new JsonException("Expected start of object for 'extension'.");

                            string? extensionId = null;
                            byte[]? binary = null;
                            string? str = null;

                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                if (reader.TokenType != JsonTokenType.PropertyName)
                                    throw new JsonException("Expected property name in extension object.");

                                var extProp = reader.GetString()!;
                                if (!reader.Read())
                                    throw new JsonException("Unexpected end of JSON in extension object.");

                                switch (extProp)
                                {
                                    case "extensionId":
                                        extensionId = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                                        break;
                                    case "binary":
                                        if (reader.TokenType == JsonTokenType.String)
                                        {
                                            var b64 = reader.GetString()!;
                                            binary = Convert.FromBase64String(b64);
                                        }
                                        else if (reader.TokenType == JsonTokenType.Null)
                                        {
                                            binary = null;
                                        }
                                        else
                                        {
                                            throw new JsonException("Expected base64 string or null for 'binary'.");
                                        }
                                        break;
                                    case "string":
                                        str = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                                        break;
                                    default:
                                        throw new JsonException($"Unexpected property '{extProp}' in extension object.");
                                }
                            }

                            extensionLocal = new ExtensionType(extensionId, binary, str);
                        }
                        break;

                    default:
                        throw new JsonException($"Unexpected property '{propName}' in data object.");
                }
            } // end object properties

            // Decide how to combine local parts with pending ones.
            // If this element contains both header and payload -> create DataType immediately.
            if (headerListLocal != null && payloadLocal != null)
            {
                dataList.Add(new DataType(headerListLocal.ToArray(), payloadLocal, extensionLocal ?? pendingExtension));
                // clear any pending
                pendingHeader = null;
                pendingExtension = null;
            }
            // If this element contains only header -> store pending header (to be paired with next payload)
            else if (headerListLocal != null)
            {
                pendingHeader = headerListLocal.ToArray();
                // keep extension if provided
                pendingExtension = extensionLocal ?? pendingExtension;
            }
            // If this element contains only payload -> pair with pending header (if exists)
            else if (payloadLocal != null)
            {
                if (pendingHeader == null)
                    throw new JsonException("Payload found without preceding header.");
                dataList.Add(new DataType(pendingHeader, payloadLocal, extensionLocal ?? pendingExtension));
                pendingHeader = null;
                pendingExtension = null;
            }
            else if (extensionLocal != null)
            {
                // extension without header/payload: attach to pendingHeader if present, otherwise error
                if (pendingHeader != null)
                {
                    pendingExtension = extensionLocal;
                }
                else
                {
                    throw new JsonException("Extension found without header or payload context.");
                }
            }
            else
            {
                // empty object element — ignore or error
                throw new JsonException("Empty data array element encountered.");
            }
        } // end data array

        // move past EndObject of root
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Expected end of root JSON object.");

        if (pendingHeader != null)
            throw new JsonException("Header without payload at end of 'data' array.");

        return new DataValue(dataList.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, DataValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("data");
        writer.WriteStartArray();

        foreach (var dt in value.Data)
        {
            writer.WriteStartObject();

            // header
            writer.WritePropertyName("header");
            writer.WriteStartArray();
            foreach (var h in dt.Header)
            {
                writer.WriteStartObject();
                writer.WriteString("protocolId", h.ProtocolId);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            // payload as base64 string
            writer.WritePropertyName("payload");
            writer.WriteBase64StringValue(dt.Payload);

            // extension (optional - single object)
            if (dt.Extension != null)
            {
                var ext = dt.Extension;
                writer.WritePropertyName("extension");
                writer.WriteStartObject();
                if (ext.ExtensionId is not null)
                    writer.WriteString("extensionId", ext.ExtensionId);
                if (ext.Binary is not null)
                {
                    writer.WritePropertyName("binary");
                    writer.WriteBase64StringValue(ext.Binary);
                }
                if (ext.String is not null)
                    writer.WriteString("string", ext.String);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}