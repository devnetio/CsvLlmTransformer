using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Llm;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Steps;

public sealed class LlmTransformStep : IPipelineStep
{
    private readonly ILlmClient _llmClient;
    private readonly Dictionary<string, object?> _settings;

    public string Name => "LLM Transform";

    public LlmTransformStep(ILlmClient llmClient, Dictionary<string, object?> settings)
    {
        _llmClient = llmClient;
        _settings = settings;
    }

    public async Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("Executing LLM transformation...");

        string promptTemplate = _settings.TryGetValue("prompt_template", out var pt) ? pt?.ToString() ?? "" : "";

        var request = new LlmRequest
        {
            Rows = context.Rows,
            Schema = context.Schema,
            PromptTemplate = promptTemplate
        };

        var result = await _llmClient.TransformAsync(request, context.LlmConfig, cancellationToken);

        var newRows = new List<RowData>();

        foreach (var rowResult in result.Rows)
        {
            var originalRow = context._rows.FirstOrDefault(r => r.Id == rowResult.RowId);
            
            var newRow = new RowData
            {
                // If we found the original row, we could copy some state if needed, 
                // but usually LLM transform is a fresh start for values.
                State = Model.Enum.RowState.Transformed
            };

            foreach (var kvp in rowResult.Values)
            {
                newRow.Values[kvp.Key] = kvp.Value;
            }

            newRows.Add(newRow);
        }

        context._rows.Clear();
        context._rows.AddRange(newRows);
    }
}
