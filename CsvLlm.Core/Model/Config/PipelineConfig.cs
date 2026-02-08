namespace CsvLlm.Core.Model.Config;

public class PipelineConfig
{
    public InputConfig Input { get; init; } = default!;
    public OutputConfig Output { get; init; } = default!;
    public LlmConfig Llm { get; init; } = default!;
    public SchemaConfig Schema { get; init; } = default!;
    public List<StepConfig> Steps { get; init; } = [];
}