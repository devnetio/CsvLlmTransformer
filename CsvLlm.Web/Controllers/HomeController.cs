using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CsvLlm.Web.Models;
using CsvLlm.Core.Interface;
using CsvLlm.Core.Model.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ICSharpCode.SharpZipLib.Zip;

namespace CsvLlm.Web.Controllers;

public class HomeController(IPipelineRunner pipelineRunner, ILogger<HomeController> logger)
    : Controller
{
    public IActionResult Index()
    {
        var model = new PipelineViewModel
        {
            YamlConfig = GetDefaultYaml()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Run(PipelineViewModel model)
    {
        if (!ModelState.IsValid || model.InputFile == null)
        {
            return View("Index", model);
        }

        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        try
        {
            // 1. Save input file
            var inputFilePath = Path.Combine(tempPath, model.InputFile.FileName);
            await using (var stream = new FileStream(inputFilePath, FileMode.Create))
            {
                await model.InputFile.CopyToAsync(stream);
            }

            // 2. Parse YAML
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<PipelineConfig>(model.YamlConfig);

            // 3. Run Pipeline
            var outputDir = Path.Combine(tempPath, "output");
            Directory.CreateDirectory(outputDir);

            await pipelineRunner.RunAsync(inputFilePath, outputDir, config, CancellationToken.None);

            // 4. Zip output and download
            var zipFilePath = Path.Combine(tempPath, "results.zip");
            await using (var fsOut = new FileStream(zipFilePath, FileMode.Create))
            await using (var zipStream = new ZipOutputStream(fsOut))
            {
                zipStream.SetLevel(3);
                var files = Directory.GetFiles(outputDir);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var entry = new ZipEntry(fileInfo.Name)
                    {
                        DateTime = DateTime.Now,
                        Size = fileInfo.Length
                    };
                    await zipStream.PutNextEntryAsync(entry);
                    await using (var fsIn = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        await fsIn.CopyToAsync(zipStream);
                    }

                    zipStream.CloseEntry();
                }
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
            return File(bytes, "application/zip", "results.zip");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pipeline execution failed");
            model.ErrorMessage = ex.Message;
            return View("Index", model);
        }
        finally
        {
            try
            {
                Directory.Delete(tempPath, true);
            }
            catch
            {
                /* ignore */
            }
        }
    }

    private string GetDefaultYaml()
    {
        // Try to read from sample_config.yaml if exists
        if (System.IO.File.Exists("../sample_config.yaml"))
        {
            return System.IO.File.ReadAllText("../sample_config.yaml");
        }

        return @"input:
  delimiter: "",""
  encoding: ""utf-8""
  header: true

output:
  format: ""csv""
  filename_pattern: ""output.csv""

schema:
  target: ""user""
  fields:
    - name: ""full_name""
      type: ""string""
      required: true
    - name: ""email""
      type: ""string""
      required: true
    - name: ""age""
      type: ""number""
      required: false

llm:
  provider: ""openai""
  model: ""gpt-5-nano""
  api_key: ""YOUR_API_KEY_HERE"" # Recommended: Use OPENAI_API_KEY environment variable instead
  temperature: 0.3
  max_tokens: 2000

steps:
  - type: ""normalize_headers""
  - type: ""llm_transform""
    settings:
      prompt_template: ""Extract full name, email and age from the provided data.""
  - type: ""validate""
";
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}