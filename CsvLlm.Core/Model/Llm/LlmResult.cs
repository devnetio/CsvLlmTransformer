namespace CsvLlm.Core.Model.Llm;

public sealed class LlmResult
{
    public IReadOnlyList<LlmRowResult> Rows { get; init; } = [];
}