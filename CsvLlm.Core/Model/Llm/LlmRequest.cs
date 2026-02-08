using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Model.Llm;

public sealed class LlmRequest
{
    public IReadOnlyList<RowData> Rows { get; init; } = [];
    public SchemaConfig Schema { get; init; } = default!;
    public string PromptTemplate { get; init; } = string.Empty;
}