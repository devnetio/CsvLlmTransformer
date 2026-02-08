using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Interface;

public interface ICsvWriter
{
    Task WriteAsync(
        string outputDirectory,
        IReadOnlyList<RowData> rows,
        SchemaConfig schema,
        OutputConfig outputConfig,
        CancellationToken cancellationToken);
}