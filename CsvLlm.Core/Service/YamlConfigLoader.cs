using CsvLlm.Core.Interface;
using CsvLlm.Core.Model.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public sealed class YamlConfigLoader : IConfigLoader
{
    public PipelineConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Pipeline configuration file not found: {path}");
        }

        var yaml = File.ReadAllText(path);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var config = deserializer.Deserialize<PipelineConfig>(yaml)
                     ?? throw new InvalidOperationException(
                         "Failed to deserialize pipeline configuration");

        Validate(config);
        return config;
    }

    private static void Validate(PipelineConfig config)
    {
        if (config.Input == null)
            throw new InvalidOperationException("Missing input configuration");

        if (config.Output == null)
            throw new InvalidOperationException("Missing output configuration");

        if (config.Schema == null || config.Schema.Fields.Count == 0)
            throw new InvalidOperationException(
                "Schema must define at least one field");

        if (config.Steps == null || config.Steps.Count == 0)
            throw new InvalidOperationException(
                "Pipeline must define at least one step");
    }
}