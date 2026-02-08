namespace CsvLlm.Core.Model.Config;

public sealed class LlmConfig
{
    public string Provider { get; init; } = "openai";
    public string Model { get; init; } = string.Empty;
    public string? ApiKey { get; init; }

    /// <summary>
    /// Must be 0 for deterministic transformations.
    /// </summary>
    public double Temperature { get; init; } = 0;

    public int MaxTokens { get; init; } = 1024;

    public int BatchSize { get; init; } = 50;

    public RetryConfig Retry { get; init; } = new();
}
