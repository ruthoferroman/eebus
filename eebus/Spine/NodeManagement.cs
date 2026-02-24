csharp eebus\Spine\NodeManagementTypes.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eebus.Spine;

/// <summary>
/// NodeManagement: types adapted from the XSD. Fields that reference large or external
/// schema types are represented with existing records where available (e.g. FeatureAddressType)
/// or with JsonElement / JsonElement[] to remain tolerant to the full SPINE structure.
/// </summary>
internal record NodeManagementSpecificationVersionListType(
    [property: JsonPropertyName("specificationVersion")] JsonElement[]? SpecificationVersion = null
);

internal record NodeManagementDetailedDiscoveryDeviceInformationDescriptionType(
    [property: JsonPropertyName("deviceAddress")] DeviceAddressType? DeviceAddress = null,
    [property: JsonPropertyName("deviceType")] DeviceTypeType? DeviceType = null,
    [property: JsonPropertyName("networkManagementResponsibleAddress")] FeatureAddressType? NetworkManagementResponsibleAddress = null,
    [property: JsonPropertyName("nativeSetup")] JsonElement? NativeSetup = null,
    [property: JsonPropertyName("technologyAddress")] JsonElement? TechnologyAddress = null,
    [property: JsonPropertyName("communicationsTechnologyInformation")] JsonElement? CommunicationsTechnologyInformation = null,
    [property: JsonPropertyName("networkFeatureSet")] JsonElement? NetworkFeatureSet = null,
    [property: JsonPropertyName("lastStateChange")] JsonElement? LastStateChange = null,
    [property: JsonPropertyName("minimumTrustLevel")] JsonElement? MinimumTrustLevel = null,
    [property: JsonPropertyName("label")] LabelType? Label = null,
    [property: JsonPropertyName("description")] DescriptionType? Description = null
);

internal record NodeManagementDetailedDiscoveryDeviceInformationType(
    [property: JsonPropertyName("description")] NodeManagementDetailedDiscoveryDeviceInformationDescriptionType? Description = null
);

internal record NodeManagementDetailedDiscoveryEntityInformationDescriptionType(
    [property: JsonPropertyName("entityAddress")] EntityAddressType? EntityAddress = null,
    [property: JsonPropertyName("entityType")] EntityTypeType? EntityType = null,
    [property: JsonPropertyName("lastStateChange")] JsonElement? LastStateChange = null,
    [property: JsonPropertyName("minimumTrustLevel")] JsonElement? MinimumTrustLevel = null,
    [property: JsonPropertyName("label")] LabelType? Label = null,
    [property: JsonPropertyName("description")] DescriptionType? Description = null
);

internal record NodeManagementDetailedDiscoveryEntityInformationType(
    [property: JsonPropertyName("description")] NodeManagementDetailedDiscoveryEntityInformationDescriptionType? Description = null
);

internal record NodeManagementDetailedDiscoveryFeatureInformationDescriptionType(
    [property: JsonPropertyName("featureAddress")] FeatureAddressType? FeatureAddress = null,
    [property: JsonPropertyName("featureType")] FeatureTypeType? FeatureType = null,
    [property: JsonPropertyName("specificUsage")] JsonElement[]? SpecificUsage = null,
    [property: JsonPropertyName("featureGroup")] FeatureGroupType? FeatureGroup = null,
    [property: JsonPropertyName("role")] RoleType? Role = null,
    [property: JsonPropertyName("supportedFunction")] FunctionPropertyType[]? SupportedFunction = null,
    [property: JsonPropertyName("lastStateChange")] JsonElement? LastStateChange = null,
    [property: JsonPropertyName("minimumTrustLevel")] JsonElement? MinimumTrustLevel = null,
    [property: JsonPropertyName("label")] LabelType? Label = null,
    [property: JsonPropertyName("description")] DescriptionType? Description = null,
    [property: JsonPropertyName("maxResponseDelay")] JsonElement? MaxResponseDelay = null
);

internal record NodeManagementDetailedDiscoveryFeatureInformationType(
    [property: JsonPropertyName("description")] NodeManagementDetailedDiscoveryFeatureInformationDescriptionType? Description = null
);

internal record NodeManagementDetailedDiscoveryDataType(
    [property: JsonPropertyName("specificationVersionList")] NodeManagementSpecificationVersionListType? SpecificationVersionList = null,
    [property: JsonPropertyName("deviceInformation")] NodeManagementDetailedDiscoveryDeviceInformationType? DeviceInformation = null,
    [property: JsonPropertyName("entityInformation")] NodeManagementDetailedDiscoveryEntityInformationType[]? EntityInformation = null,
    [property: JsonPropertyName("featureInformation")] NodeManagementDetailedDiscoveryFeatureInformationType[]? FeatureInformation = null
);

internal record NodeManagementDetailedDiscoveryDataElementsType(
    [property: JsonPropertyName("specificationVersionList")] JsonElement? SpecificationVersionList = null,
    [property: JsonPropertyName("deviceInformation")] JsonElement? DeviceInformation = null,
    [property: JsonPropertyName("entityInformation")] JsonElement? EntityInformation = null,
    [property: JsonPropertyName("featureInformation")] JsonElement? FeatureInformation = null
);

internal record NodeManagementDetailedDiscoveryDataSelectorsType(
    [property: JsonPropertyName("deviceInformation")] JsonElement? DeviceInformation = null,
    [property: JsonPropertyName("entityInformation")] JsonElement? EntityInformation = null,
    [property: JsonPropertyName("featureInformation")] JsonElement? FeatureInformation = null
);

internal record NodeManagementBindingEntryType(
    [property: JsonPropertyName("bindingId")] JsonElement? BindingId = null,
    [property: JsonPropertyName("clientAddress")] FeatureAddressType? ClientAddress = null,
    [property: JsonPropertyName("serverAddress")] FeatureAddressType? ServerAddress = null,
    [property: JsonPropertyName("label")] LabelType? Label = null,
    [property: JsonPropertyName("description")] DescriptionType? Description = null
);

internal record NodeManagementBindingDataType(
    [property: JsonPropertyName("bindingEntry")] NodeManagementBindingEntryType[]? BindingEntry = null
);

internal record NodeManagementBindingDataElementsType(
    [property: JsonPropertyName("bindingEntry")] JsonElement[]? BindingEntry = null
);

internal record NodeManagementBindingDataSelectorsType(
    [property: JsonPropertyName("bindingEntry")] JsonElement[]? BindingEntry = null
);

internal record NodeManagementBindingRequestCallType(
    [property: JsonPropertyName("bindingRequest")] JsonElement? BindingRequest = null
);

internal record NodeManagementBindingRequestCallElementsType(
    [property: JsonPropertyName("bindingRequest")] JsonElement? BindingRequest = null
);

internal record NodeManagementBindingDeleteCallType(
    [property: JsonPropertyName("bindingDelete")] JsonElement? BindingDelete = null
);

internal record NodeManagementBindingDeleteCallElementsType(
    [property: JsonPropertyName("bindingDelete")] JsonElement? BindingDelete = null
);

internal record NodeManagementSubscriptionEntryType(
    [property: JsonPropertyName("subscriptionId")] JsonElement? SubscriptionId = null,
    [property: JsonPropertyName("clientAddress")] FeatureAddressType? ClientAddress = null,
    [property: JsonPropertyName("serverAddress")] FeatureAddressType? ServerAddress = null,
    [property: JsonPropertyName("label")] LabelType? Label = null,
    [property: JsonPropertyName("description")] DescriptionType? Description = null
);

internal record NodeManagementSubscriptionDataType(
    [property: JsonPropertyName("subscriptionEntry")] NodeManagementSubscriptionEntryType[]? SubscriptionEntry = null
);

internal record NodeManagementSubscriptionDataElementsType(
    [property: JsonPropertyName("subscriptionEntry")] JsonElement[]? SubscriptionEntry = null
);

internal record NodeManagementSubscriptionDataSelectorsType(
    [property: JsonPropertyName("subscriptionEntry")] JsonElement[]? SubscriptionEntry = null
);

internal record NodeManagementSubscriptionRequestCallType(
    [property: JsonPropertyName("subscriptionRequest")] JsonElement? SubscriptionRequest = null
);

internal record NodeManagementSubscriptionRequestCallElementsType(
    [property: JsonPropertyName("subscriptionRequest")] JsonElement? SubscriptionRequest = null
);

internal record NodeManagementSubscriptionDeleteCallType(
    [property: JsonPropertyName("subscriptionDelete")] JsonElement? SubscriptionDelete = null
);

internal record NodeManagementSubscriptionDeleteCallElementsType(
    [property: JsonPropertyName("subscriptionDelete")] JsonElement? SubscriptionDelete = null
);

internal record NodeManagementDestinationDeviceDescriptionType(
    [property: JsonPropertyName("deviceAddress")] DeviceAddressType? DeviceAddress = null,
    [property: JsonPropertyName("communicationsTechnologyInformation")] JsonElement? CommunicationsTechnologyInformation = null,
    [property: JsonPropertyName("networkFeatureSet")] JsonElement? NetworkFeatureSet = null,
    [property: JsonPropertyName("lastStateChange")] JsonElement? LastStateChange = null,
    [property: JsonPropertyName("label")] LabelType? Label = null
);

internal record NodeManagementDestinationDataType(
    [property: JsonPropertyName("deviceDescription")] NodeManagementDestinationDeviceDescriptionType? DeviceDescription = null
);

internal record NodeManagementDestinationDataElementsType(
    [property: JsonPropertyName("deviceDescription")] JsonElement? DeviceDescription = null
);

internal record NodeManagementDestinationListDataType(
    [property: JsonPropertyName("nodeManagementDestinationData")] NodeManagementDestinationDataType[]? Items = null
);

internal record NodeManagementDestinationListDataSelectorsType(
    [property: JsonPropertyName("deviceDescription")] JsonElement? DeviceDescription = null
);

internal record NodeManagementUseCaseSupportType(
    [property: JsonPropertyName("useCaseName")] JsonElement? UseCaseName = null,
    [property: JsonPropertyName("useCaseVersion")] SpecificationVersionType? UseCaseVersion = null,
    [property: JsonPropertyName("useCaseAvailable")] bool? UseCaseAvailable = null,
    [property: JsonPropertyName("scenarioSupport")] JsonElement[]? ScenarioSupport = null,
    [property: JsonPropertyName("useCaseDocumentSubRevision")] string? UseCaseDocumentSubRevision = null
);

internal record NodeManagementUseCaseInformationType(
    [property: JsonPropertyName("address")] FeatureAddressType? Address = null,
    [property: JsonPropertyName("actor")] JsonElement? Actor = null,
    [property: JsonPropertyName("useCaseSupport")] NodeManagementUseCaseSupportType[]? UseCaseSupport = null
);

internal record NodeManagementUseCaseDataType(
    [property: JsonPropertyName("useCaseInformation")] NodeManagementUseCaseInformationType[]? UseCaseInformation = null
);

internal record NodeManagementUseCaseDataElementsType(
    [property: JsonPropertyName("useCaseInformation")] JsonElement? UseCaseInformation = null
);

internal record NodeManagementUseCaseDataSelectorsType(
    [property: JsonPropertyName("useCaseInformation")] JsonElement? UseCaseInformation = null
);