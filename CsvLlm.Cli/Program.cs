using CsvLlm.Cli.Infrastructure;
using CsvLlm.Core.Interface;
using CsvLlm.Core.Service;
using CsvLlm.Core.Service.Csv;
using CsvLlm.Core.Service.Llm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace CsvLlm.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        var services = new ServiceCollection();
        
        // Register Core Services
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
        });

        services.AddHttpClient();
        services.AddSingleton<IConfigLoader, YamlConfigLoader>();
        services.AddSingleton<ICsvReader, CsvReader>();
        services.AddSingleton<ICsvWriter, CsvWriter>();
        services.AddSingleton<IPipelineRunner, PipelineRunner>();
        
        // Register ILlmClient
        services.AddSingleton<ILlmClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OpenAiClient>>();
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "MISSING_API_KEY";
            
            return new OpenAiClient(httpClient, apiKey, logger);
        });

        var registrar = new TypeRegistrar(services);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("csv-llm");
            config.AddCommand<RunCommand>("run")
                .WithDescription("Run CSV → LLM → CSV pipeline");
        });

        return app.Run(args);
    }
}