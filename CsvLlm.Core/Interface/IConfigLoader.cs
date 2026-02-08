using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Interface;

public interface IConfigLoader
{
    PipelineConfig Load(string path);
}