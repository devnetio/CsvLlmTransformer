using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Steps;

public sealed class MapColumnsStep : IPipelineStep
{
    private readonly Dictionary<string, object?> _settings;

    public string Name => "Map Columns";

    public MapColumnsStep(Dictionary<string, object?> settings)
    {
        _settings = settings;
    }

    public Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("Mapping columns...");

        if (!_settings.TryGetValue("mapping", out var mappingObj) || mappingObj is not Dictionary<object, object> mapping)
        {
            context.Logger.LogWarning("No mapping defined for MapColumnsStep");
            return Task.CompletedTask;
        }

        foreach (var row in context._rows)
        {
            foreach (var m in mapping)
            {
                var source = m.Key.ToString();
                var target = m.Value.ToString();

                if (source != null && target != null && row.Values.TryGetValue(source, out var value))
                {
                    row.Values[target] = value;
                }
            }
        }

        return Task.CompletedTask;
    }
}
