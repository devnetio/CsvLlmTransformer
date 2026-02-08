namespace CsvLlm.Core.Model.Config;

public sealed class StepConfig
{
    /// <summary>
    /// Step type identifier (for example: normalize_headers, llm_transform).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Arbitrary step-specific settings.
    /// </summary>
    public Dictionary<string, object?> Settings { get; init; } = new();
}
