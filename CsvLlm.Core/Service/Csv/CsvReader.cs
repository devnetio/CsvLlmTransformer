using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Service.Csv;

public sealed class CsvReader : ICsvReader
{
    public async Task<List<RowData>> ReadAsync(
        string path,
        InputConfig config,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"CSV file not found: {path}");
        }

        var encoding = ResolveEncoding(config.Encoding);
        var delimiter = config.Delimiter == "auto"
            ? DetectDelimiter(path, encoding)
            : config.Delimiter;

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = config.Header,
            Delimiter = delimiter,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, encoding);
        using var csv = new CsvHelper.CsvReader(reader, csvConfig);

        var rows = new List<RowData>();
        var rowIndex = 0;

        if (config.SkipRows > 0)
        {
            for (var i = 0; i < config.SkipRows; i++)
            {
                if (!await csv.ReadAsync())
                {
                    break;
                }
            }
        }

        if (config.Header)
        {
            await csv.ReadAsync();
            csv.ReadHeader();
        }

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = new RowData
            {
                Id = rowIndex.ToString()
            };

            foreach (var header in csv.HeaderRecord ?? Array.Empty<string>())
            {
                row.Values[header] = csv.GetField(header);
            }

            rows.Add(row);
            rowIndex++;
        }

        return rows;
    }

    private static Encoding ResolveEncoding(string encodingName)
    {
        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch
        {
            throw new InvalidOperationException(
                $"Unsupported encoding: {encodingName}");
        }
    }

    private static string DetectDelimiter(string path, Encoding encoding)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, encoding);

        var sample = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(sample))
        {
            return ",";
        }

        var candidates = new[] { ",", ";", "\t", "|" };

        return candidates
            .Select(d => new
            {
                Delimiter = d,
                Count = sample.Count(c => c.ToString() == d)
            })
            .OrderByDescending(x => x.Count)
            .First().Delimiter;
    }
}