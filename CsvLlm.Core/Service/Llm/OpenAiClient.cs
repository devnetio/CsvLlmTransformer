using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;
using CsvLlm.Core.Model.Llm;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Llm;

public sealed class OpenAiClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _defaultApiKey;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(
        HttpClient httpClient,
        string apiKey,
        ILogger<OpenAiClient> logger)
    {
        _httpClient = httpClient;
        _defaultApiKey = apiKey;
        _logger = logger;
    }

    public async Task<LlmResult> TransformAsync(
        LlmRequest request,
        LlmConfig config,
        CancellationToken cancellationToken)
    {
        var apiKey = config.ApiKey ?? _defaultApiKey;

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "MISSING_API_KEY")
        {
            throw new InvalidOperationException(
                "OpenAI API key is missing. Provide it via config or environment variable (OPENAI_API_KEY).");
        }

        var prompt = BuildFlattenedPrompt(request);

        var apiRequest = new
        {
            model = config.Model,
            input = prompt,
            max_output_tokens = config.MaxTokens
        };

        var response = await ExecuteWithRetryAsync(async () =>
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            httpRequest.Content = JsonContent.Create(apiRequest);

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI error response: {Body}", errorBody);
            }

            httpResponse.EnsureSuccessStatusCode();

            return await httpResponse.Content.ReadFromJsonAsync<OpenAiResponse>(
                cancellationToken: cancellationToken);
        }, config, cancellationToken);

        if (response?.Output == null || response.Output.Length == 0)
        {
            throw new InvalidOperationException("OpenAI returned an empty response.");
        }

        var outputText = ExtractOutputText(response);

        return ParseResponse(outputText, request.Rows);
    }

    private static string BuildFlattenedPrompt(LlmRequest request)
    {
        var schemaFields = string.Join(
            ", ",
            request.Schema.Fields.Select(f => $"{f.Name} ({f.Type})"));

        var rowsData = JsonSerializer.Serialize(
            request.Rows.Select(r => new { r.Id, r.Values }));

        return $$"""
        You are a data transformation assistant.

        Task:
        Transform the provided rows into the target schema.

        Rules:
        - Respond ONLY with valid JSON
        - Do NOT include explanations, comments, or markdown
        - The JSON MUST match the output format exactly

        Target schema fields:
        {{schemaFields}}

        Instructions:
        {{request.PromptTemplate}}

        Data to transform:
        {{rowsData}}

        Output format (strict):
        {
          "results": [
            {
              "row_id": "...",
              "values": { ... }
            }
          ]
        }
        """;
    }

    private static string ExtractOutputText(OpenAiResponse response)
    {
        foreach (var output in response.Output)
        {
            foreach (var content in output.Content)
            {
                if (content.Type == "output_text" &&
                    !string.IsNullOrWhiteSpace(content.Text))
                {
                    return content.Text;
                }
            }
        }

        throw new InvalidOperationException(
            "No output_text content found in OpenAI response.");
    }

    private LlmResult ParseResponse(
        string content,
        IReadOnlyList<RowData> originalRows)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(content);
            var resultsArray = jsonDoc.RootElement.GetProperty("results");

            var rowResults = new List<LlmRowResult>();

            foreach (var item in resultsArray.EnumerateArray())
            {
                var rowId = item.GetProperty("row_id").GetString() ?? string.Empty;
                var valuesElement = item.GetProperty("values");

                var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    valuesElement.GetRawText());

                rowResults.Add(new LlmRowResult
                {
                    RowId = rowId,
                    Values = values ?? new Dictionary<string, object?>()
                });
            }

            return new LlmResult { Rows = rowResults };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response: {Content}", content);
            throw new InvalidOperationException(
                "Failed to parse LLM response into the expected format.", ex);
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> action,
        LlmConfig config,
        CancellationToken cancellationToken)
    {
        var attempts = 0;
        var maxAttempts = config.Retry?.MaxAttempts ?? 3;
        var delaySeconds = config.Retry?.BackoffSeconds ?? 2;

        while (true)
        {
            try
            {
                attempts++;
                return await action();
            }
            catch (Exception ex) when (attempts < maxAttempts &&
                                       !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "OpenAI API call failed. Attempt {Attempt} of {MaxAttempts}. Retrying in {Delay}s...",
                    attempts,
                    maxAttempts,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                delaySeconds *= 2;
            }
        }
    }

    #region DTOs

    private sealed class OpenAiResponse
    {
        [JsonPropertyName("output")]
        public OutputItem[] Output { get; set; } = Array.Empty<OutputItem>();
    }

    private sealed class OutputItem
    {
        [JsonPropertyName("content")]
        public ContentItem[] Content { get; set; } = Array.Empty<ContentItem>();
    }

    private sealed class ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    #endregion
}
