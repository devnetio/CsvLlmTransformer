namespace CsvLlm.Core.Model.Config;

public sealed class SchemaConfig
{
    /// <summary>
    /// Logical name of the target schema (for example: "customer").
    /// </summary>
    public string Target { get; init; } = string.Empty;

    public List<FieldDefinition> Fields { get; init; } = [];
}
