using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Steps;

public sealed class NormalizeHeadersStep : IPipelineStep
{
    private readonly Dictionary<string, object?> _settings;

    public string Name => "Normalize Headers";

    public NormalizeHeadersStep(Dictionary<string, object?> settings)
    {
        _settings = settings;
    }

    public Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("Normalizing headers...");
        
        // Implementation: Trim and lowercase headers in each row's Values dictionary
        foreach (var row in context._rows)
        {
            var normalizedValues = new Dictionary<string, object?>();
            foreach (var kvp in row.Values)
            {
                normalizedValues[kvp.Key.Trim().ToLowerInvariant()] = kvp.Value;
            }
            
            row.Values.Clear();
            foreach (var kvp in normalizedValues)
            {
                row.Values[kvp.Key] = kvp.Value;
            }
        }

        return Task.CompletedTask;
    }
}
