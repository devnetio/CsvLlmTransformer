using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;
using CsvLlm.Core.Service.Steps;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service;

public sealed class PipelineRunner : IPipelineRunner
{
    private readonly IConfigLoader _configLoader;
    private readonly ICsvReader _csvReader;
    private readonly ICsvWriter _csvWriter;
    private readonly ILlmClient _llmClient;
    private readonly ILogger _logger;

    public PipelineRunner(
        IConfigLoader configLoader,
        ICsvReader csvReader,
        ICsvWriter csvWriter,
        ILlmClient llmClient,
        ILogger<PipelineRunner> logger)
    {
        _configLoader = configLoader;
        _csvReader = csvReader;
        _csvWriter = csvWriter;
        _llmClient = llmClient;
        _logger = logger;
    }

    // ------------------------------------------------------------
    // Entry point when config is already loaded
    // ------------------------------------------------------------
    public async Task RunAsync(
        string inputPath,
        string outputDirectory,
        PipelineConfig config,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pipeline started");

        ValidatePaths(inputPath, outputDirectory);
        ValidateConfig(config);

        // 1. Read CSV
        _logger.LogInformation("Reading input CSV");
        var rows = await _csvReader.ReadAsync(
            inputPath,
            config.Input,
            cancellationToken);

        _logger.LogInformation("Read {RowCount} rows", rows.Count);

        // 2. Create pipeline context
        var context = new PipelineContext(
            rows,
            config.Schema,
            config.Llm,
            _logger);

        // 3. Execute steps in order
        foreach (var stepConfig in config.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var step = CreateStep(stepConfig);

            _logger.LogInformation("Executing step: {Step}", step.Name);
            await step.ExecuteAsync(context, cancellationToken);
        }

        // 4. Write output
        _logger.LogInformation("Writing output CSV files");

        await _csvWriter.WriteAsync(
            outputDirectory,
            context.Rows,
            config.Schema,
            config.Output,
            cancellationToken);

        _logger.LogInformation("Pipeline finished successfully");
    }

    // ------------------------------------------------------------
    // Entry point when config path is provided
    // ------------------------------------------------------------
    public async Task RunAsync(
        string inputPath,
        string outputDirectory,
        string configPath,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading pipeline configuration");

        var config = _configLoader.Load(configPath);

        await RunAsync(
            inputPath,
            outputDirectory,
            config,
            cancellationToken);
    }

    // ------------------------------------------------------------
    // Step factory
    // ------------------------------------------------------------
    private IPipelineStep CreateStep(StepConfig config)
    {
        return config.Type switch
        {
            "normalize_headers" => new NormalizeHeadersStep(config.Settings),
            "map_columns"       => new MapColumnsStep(config.Settings),
            "llm_transform"     => new LlmTransformStep(
                                        _llmClient,
                                        config.Settings),
            "validate"          => new ValidateSchemaStep(),
            "export"            => new ExportStep(), // usually no-op here
            _ => throw new InvalidOperationException(
                    $"Unknown pipeline step: {config.Type}")
        };
    }

    // ------------------------------------------------------------
    // Validation
    // ------------------------------------------------------------
    private static void ValidatePaths(
        string inputPath,
        string outputDirectory)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException(
                $"Input CSV file not found: {inputPath}");
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
    }

    private static void ValidateConfig(PipelineConfig config)
    {
        if (config.Schema.Fields.Count == 0)
        {
            throw new InvalidOperationException(
                "Schema must define at least one field");
        }

        if (config.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                "Pipeline must contain at least one step");
        }
    }
}
