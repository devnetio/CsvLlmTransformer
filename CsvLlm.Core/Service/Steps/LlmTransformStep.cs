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

        foreach (var rowResult in result.Rows)
        {
            var row = context._rows.Find(r => r.Id == rowResult.RowId);
            if (row != null)
            {
                foreach (var kvp in rowResult.Values)
                {
                    row.Values[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
