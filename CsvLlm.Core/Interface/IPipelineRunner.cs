using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Interface;

public interface IPipelineRunner
{
    Task RunAsync(
        string inputPath,
        string outputDirectory,
        PipelineConfig config,
        CancellationToken cancellationToken);

    Task RunAsync(
        string inputPath,
        string outputDirectory,
        string configPath,
        CancellationToken cancellationToken);
}