using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;
using CsvLlm.Core.Model.Enum;

namespace CsvLlm.Core.Service.Csv;

public sealed class CsvWriter : ICsvWriter
{
    public async Task WriteAsync(
        string outputDirectory,
        IReadOnlyList<RowData> rows,
        SchemaConfig schema,
        OutputConfig outputConfig,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);

        var successfulRows = rows
            .Where(r => r.State == RowState.Transformed)
            .ToList();

        var errorRows = rows
            .Where(r => r.State == RowState.Invalid)
            .ToList();

        if (!string.IsNullOrWhiteSpace(outputConfig.SplitBy))
        {
            await WriteSplitAsync(
                outputDirectory,
                successfulRows,
                schema,
                outputConfig,
                cancellationToken);
        }
        else
        {
            var path = Path.Combine(
                outputDirectory,
                ResolveFileName(outputConfig.FilenamePattern, "output"));

            await WriteCsvAsync(
                path,
                successfulRows,
                schema,
                cancellationToken);
        }

        if (errorRows.Any())
        {
            var errorPath = Path.Combine(outputDirectory, "errors.csv");
            await WriteErrorsAsync(errorPath, errorRows, cancellationToken);
        }
    }

    private async Task WriteSplitAsync(
        string outputDirectory,
        IReadOnlyList<RowData> rows,
        SchemaConfig schema,
        OutputConfig outputConfig,
        CancellationToken cancellationToken)
    {
        var splitField = outputConfig.SplitBy!;

        var groups = rows
            .GroupBy(r =>
                r.Values.TryGetValue(splitField, out var value)
                    ? value?.ToString() ?? "unknown"
                    : "unknown");

        foreach (var group in groups)
        {
            var fileName = ResolveFileName(
                outputConfig.FilenamePattern,
                group.Key);

            var path = Path.Combine(outputDirectory, fileName);

            await WriteCsvAsync(
                path,
                group.ToList(),
                schema,
                cancellationToken);
        }
    }

    private async Task WriteCsvAsync(
        string path,
        IReadOnlyList<RowData> rows,
        SchemaConfig schema,
        CancellationToken cancellationToken)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        };

        await using var writer = new StreamWriter(path, false, Encoding.UTF8);
        await using var csv = new CsvHelper.CsvWriter(writer, csvConfig);

        // Write headers in schema order
        foreach (var field in schema.Fields)
        {
            csv.WriteField(field.Name);
        }

        await csv.NextRecordAsync();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var field in schema.Fields)
            {
                row.Values.TryGetValue(field.Name, out var value);
                csv.WriteField(value);
            }

            await csv.NextRecordAsync();
        }
    }

    private async Task WriteErrorsAsync(
        string path,
        IReadOnlyList<RowData> errorRows,
        CancellationToken cancellationToken)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        };

        await using var writer = new StreamWriter(path, false, Encoding.UTF8);
        await using var csv = new CsvHelper.CsvWriter(writer, csvConfig);

        csv.WriteField("row_id");
        csv.WriteField("errors");
        await csv.NextRecordAsync();

        foreach (var row in errorRows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(row.Id);
            csv.WriteField(string.Join("; ", row.Errors));
            await csv.NextRecordAsync();
        }
    }

    private static string ResolveFileName(string pattern, string value)
    {
        return pattern
            .Replace("{value}", SanitizeFileName(value))
            .Replace("{name}", "output");
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }

        return value;
    }
}