namespace CsvLlm.Core.Model.Config;

public sealed class OutputConfig
{
    /// <summary>
    /// Output format. Only "csv" is supported in v1.
    /// </summary>
    public string Format { get; init; } = "csv";

    /// <summary>
    /// Field name used to split output into multiple files.
    /// Null means no splitting.
    /// </summary>
    public string? SplitBy { get; init; }

    /// <summary>
    /// Output file name pattern.
    /// Example: "customers_{value}.csv"
    /// </summary>
    public string FilenamePattern { get; init; } = "{name}.csv";
}
