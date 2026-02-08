namespace CsvLlm.Core.Model.Llm;

public sealed class LlmRowResult
{
    public string RowId { get; init; } = string.Empty;
    public Dictionary<string, object?> Values { get; init; } = new();
}