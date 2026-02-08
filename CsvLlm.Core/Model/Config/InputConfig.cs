namespace CsvLlm.Core.Model.Config;

public sealed class InputConfig
{
    /// <summary>
    /// CSV delimiter. Use "auto" to detect automatically.
    /// </summary>
    public string Delimiter { get; init; } = "auto";

    /// <summary>
    /// File encoding, for example: utf-8
    /// </summary>
    public string Encoding { get; init; } = "utf-8";

    /// <summary>
    /// Indicates whether the first row contains headers.
    /// </summary>
    public bool Header { get; init; } = true;

    /// <summary>
    /// Number of rows to skip before reading data.
    /// </summary>
    public int SkipRows { get; init; } = 0;
}
