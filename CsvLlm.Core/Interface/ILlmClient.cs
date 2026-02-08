using CsvLlm.Core.Model.Config;
using CsvLlm.Core.Model.Llm;

namespace CsvLlm.Core.Interface;

public interface ILlmClient
{
    Task<LlmResult> TransformAsync(
        LlmRequest request,
        LlmConfig config,
        CancellationToken cancellationToken);
}