using CsvLlm.Core.Model;

namespace CsvLlm.Core.Interface;

public interface IPipelineStep
{
    string Name { get; }
    Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken);
}
