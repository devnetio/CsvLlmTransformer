using Spectre.Console.Cli;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using CsvLlm.Core.Interface;

namespace CsvLlm.Cli;

public sealed class RunCommand : AsyncCommand<RunCommand.Settings>
{
    private readonly IPipelineRunner _pipelineRunner;
    private readonly ILogger _logger;

    public RunCommand(IPipelineRunner pipelineRunner, ILogger<RunCommand> logger)
    {
        _pipelineRunner = pipelineRunner;
        _logger = logger;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--input <FILE>")]
        [Description("Path to input CSV file")]
        public FileInfo Input { get; init; } = default!;

        [CommandOption("--config <FILE>")]
        [Description("Path to pipeline YAML configuration")]
        public FileInfo Config { get; init; } = default!;

        [CommandOption("--output <DIR>")]
        [Description("Output directory")]
        public DirectoryInfo Output { get; init; } = default!;

        [CommandOption("--dry-run")]
        [Description("Run pipeline without calling LLMs or writing output")]
        public bool DryRun { get; init; }
    }

    private static void Validate(Settings settings)
    {
        if (!settings.Input.Exists)
            throw new FileNotFoundException($"Input file not found: {settings.Input.FullName}");

        if (!settings.Config.Exists)
            throw new FileNotFoundException($"Config file not found: {settings.Config.FullName}");

        if (!settings.Output.Exists)
            settings.Output.Create();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        Validate(settings);

        _logger.LogInformation("Starting pipeline");
        _logger.LogInformation("Input: {Input}", settings.Input.FullName);
        _logger.LogInformation("Config: {Config}", settings.Config.FullName);
        _logger.LogInformation("Output: {Output}", settings.Output.FullName);
        _logger.LogInformation("Dry run: {DryRun}", settings.DryRun);

        if (settings.DryRun)
        {
            _logger.LogInformation("Dry run requested, skipping execution.");
            return 0;
        }

        try
        {
            await _pipelineRunner.RunAsync(
                settings.Input.FullName,
                settings.Output.FullName,
                settings.Config.FullName,
                cancellationToken);
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline execution failed");
            return 1;
        }
    }
}
