namespace CsvLlm.Core.Model.Config;

public sealed class FieldDefinition
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Supported types: string, number, boolean, date, enum
    /// </summary>
    public string Type { get; init; } = "string";

    public bool Required { get; init; } = false;

    /// <summary>
    /// Allowed values for enum type.
    /// </summary>
    public List<string>? Values { get; init; }

    /// <summary>
    /// Optional normalization rule (for example: uppercase, lowercase).
    /// </summary>
    public string? Normalize { get; init; }

    /// <summary>
    /// Optional format hint (for example: email, iso-date).
    /// </summary>
    public string? Format { get; init; }
}
