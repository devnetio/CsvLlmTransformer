using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Steps;

public sealed class ExportStep : IPipelineStep
{
    public string Name => "Export";

    public Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        // Export logic is usually handled by the CsvWriter after the pipeline finishes, 
        // but this step could be used to prepare data or log something.
        context.Logger.LogInformation("Export step reached.");
        return Task.CompletedTask;
    }
}
