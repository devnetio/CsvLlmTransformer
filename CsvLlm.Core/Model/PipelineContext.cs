using CsvLlm.Core.Model.Config;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Model;

public sealed class PipelineContext
{
    public IReadOnlyList<RowData> Rows => _rows;
    public SchemaConfig Schema { get; }
    public LlmConfig LlmConfig { get; }
    public ILogger Logger { get; }

    internal List<RowData> _rows;

    public PipelineContext(
        List<RowData> rows,
        SchemaConfig schema,
        LlmConfig llmConfig,
        ILogger logger)
    {
        _rows = rows;
        Schema = schema;
        LlmConfig = llmConfig;
        Logger = logger;
    }
}
