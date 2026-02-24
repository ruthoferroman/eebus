namespace eebus.Spine;

// Simple type aliases
using LabelType = System.String;
using DescriptionType = System.String;
using SpecificationVersionType = System.String;
using AbsoluteOrRelativeTimeType = System.String; // union of xs:duration | xs:dateTime - keep as string
using EnumExtendType = System.String;
using NumberType = System.Int64;
using ScaleType = System.Int16;
using AddressDeviceType = System.String;
using AddressEntityType = System.UInt32;
using AddressFeatureType = System.UInt32;
using FilterIdType = System.UInt32;
using MsgCounterType = System.UInt64;
using FilterId = System.UInt32;
using CurrencyType = System.String;
using FunctionType = System.String;
using CommodityTypeType = System.String;
using EnergyDirectionType = System.String;
using EnergyModeType = System.String;
using UnitOfMeasurementType = System.String;
using PageTimeType = System.String;

// Element tag is schema-empty marker; keep flexible by storing raw JSON if present.
internal record ElementTagType([property: JsonPropertyName("value")] JsonElement? Value = null);

// Small enumerations (serialized as strings by using JsonStringEnumConverter with camelCase)
internal enum RecurringIntervalEnumType
{
    Yearly,
    Monthly,
    Weekly,
    Daily,
    Hourly,
    EveryMinute,
    EverySecond
}

internal enum MonthType
{
    January,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
}

internal enum DayOfWeekType
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

internal enum OccurrenceEnumType
{
    First,
    Second,
    Third,
    Fourth,
    Last
}

// Complex types

internal record TimePeriodType(
    [property: JsonPropertyName("startTime")] AbsoluteOrRelativeTimeType? StartTime = null,
    [property: JsonPropertyName("endTime")] AbsoluteOrRelativeTimeType? EndTime = null
);

internal record TimePeriodElementsType(
    [property: JsonPropertyName("startTime")] ElementTagType? StartTime = null,
    [property: JsonPropertyName("endTime")] ElementTagType? EndTime = null
);

internal record TimestampIntervalType(
    [property: JsonPropertyName("startTime")] AbsoluteOrRelativeTimeType? StartTime = null,
    [property: JsonPropertyName("endTime")] AbsoluteOrRelativeTimeType? EndTime = null
);

internal record RecurringIntervalType([property: JsonPropertyName("value")] string? Value = null);

internal record DaysOfWeekType(
    [property: JsonPropertyName("monday")] ElementTagType? Monday = null,
    [property: JsonPropertyName("tuesday")] ElementTagType? Tuesday = null,
    [property: JsonPropertyName("wednesday")] ElementTagType? Wednesday = null,
    [property: JsonPropertyName("thursday")] ElementTagType? Thursday = null,
    [property: JsonPropertyName("friday")] ElementTagType? Friday = null,
    [property: JsonPropertyName("saturday")] ElementTagType? Saturday = null,
    [property: JsonPropertyName("sunday")] ElementTagType? Sunday = null
);

internal record AbsoluteOrRecurringTimeType(
    [property: JsonPropertyName("dateTime")] DateTimeOffset? DateTime = null,
    [property: JsonPropertyName("month")] MonthType? Month = null,
    [property: JsonPropertyName("dayOfMonth")] byte? DayOfMonth = null,
    [property: JsonPropertyName("calendarWeek")] byte? CalendarWeek = null,
    [property: JsonPropertyName("dayOfWeekOccurrence")] OccurrenceEnumType? DayOfWeekOccurrence = null,
    [property: JsonPropertyName("daysOfWeek")] DaysOfWeekType? DaysOfWeek = null,
    [property: JsonPropertyName("time")] string? Time = null, // xs:time as string
    [property: JsonPropertyName("relative")] string? Relative = null // xs:duration as string
);

internal record AbsoluteOrRecurringTimeElementsType(
    [property: JsonPropertyName("dateTime")] ElementTagType? DateTime = null,
    [property: JsonPropertyName("month")] ElementTagType? Month = null,
    [property: JsonPropertyName("dayOfMonth")] ElementTagType? DayOfMonth = null,
    [property: JsonPropertyName("calendarWeek")] ElementTagType? CalendarWeek = null,
    [property: JsonPropertyName("dayOfWeekOccurrence")] ElementTagType? DayOfWeekOccurrence = null,
    [property: JsonPropertyName("daysOfWeek")] ElementTagType? DaysOfWeek = null,
    [property: JsonPropertyName("time")] ElementTagType? Time = null,
    [property: JsonPropertyName("relative")] ElementTagType? Relative = null
);

internal record RecurrenceInformationType(
    [property: JsonPropertyName("recurringInterval")] string? RecurringInterval = null, // either enum or extended string
    [property: JsonPropertyName("recurringIntervalStep")] uint? RecurringIntervalStep = null,
    [property: JsonPropertyName("firstExecution")] DateTimeOffset? FirstExecution = null,
    [property: JsonPropertyName("executionCount")] uint? ExecutionCount = null,
    [property: JsonPropertyName("lastExecution")] DateTimeOffset? LastExecution = null
);

internal record RecurrenceInformationElementsType(
    [property: JsonPropertyName("recurringInterval")] ElementTagType? RecurringInterval = null,
    [property: JsonPropertyName("recurringIntervalStep")] ElementTagType? RecurringIntervalStep = null,
    [property: JsonPropertyName("firstExecution")] ElementTagType? FirstExecution = null,
    [property: JsonPropertyName("executionCount")] ElementTagType? ExecutionCount = null,
    [property: JsonPropertyName("lastExecution")] ElementTagType? LastExecution = null
);

internal record ScaledNumberType(
    [property: JsonPropertyName("number")] NumberType? Number = null,
    [property: JsonPropertyName("scale")] ScaleType? Scale = null
);

internal record ScaledNumberElementsType(
    [property: JsonPropertyName("number")] ElementTagType? Number = null,
    [property: JsonPropertyName("scale")] ElementTagType? Scale = null
);

internal record ScaledNumberRangeType(
    [property: JsonPropertyName("min")] ScaledNumberType? Min = null,
    [property: JsonPropertyName("max")] ScaledNumberType? Max = null
);

internal record ScaledNumberRangeElementsType(
    [property: JsonPropertyName("min")] ScaledNumberElementsType? Min = null,
    [property: JsonPropertyName("max")] ScaledNumberElementsType? Max = null
);

internal record ScaledNumberSetType(
    [property: JsonPropertyName("value")] ScaledNumberType[]? Value = null,
    [property: JsonPropertyName("range")] ScaledNumberRangeType[]? Range = null
);

internal record ScaledNumberSetElementsType(
    [property: JsonPropertyName("value")] ScaledNumberElementsType[]? Value = null,
    [property: JsonPropertyName("range")] ScaledNumberRangeElementsType[]? Range = null
);

internal record PossibleOperationsClassifierType(
    [property: JsonPropertyName("partial")] ElementTagType? Partial = null
);

internal record PossibleOperationsReadType(PossibleOperationsClassifierType? Base = null);
internal record PossibleOperationsWriteType(PossibleOperationsClassifierType? Base = null);

internal record PossibleOperationsType(
    [property: JsonPropertyName("read")] PossibleOperationsReadType? Read = null,
    [property: JsonPropertyName("write")] PossibleOperationsWriteType? Write = null
);

internal record FunctionPropertyType(
    [property: JsonPropertyName("function")] FunctionType? Function = null,
    [property: JsonPropertyName("possibleOperations")] PossibleOperationsType? PossibleOperations = null
);

internal record FunctionPropertyElementsType(
    [property: JsonPropertyName("function")] ElementTagType? Function = null,
    [property: JsonPropertyName("possibleOperations")] ElementTagType? PossibleOperations = null
);