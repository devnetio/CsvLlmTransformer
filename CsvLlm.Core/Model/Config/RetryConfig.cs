namespace CsvLlm.Core.Model.Config;

public sealed class RetryConfig
{
    public int MaxAttempts { get; init; } = 3;
    public int BackoffSeconds { get; init; } = 2;
}
