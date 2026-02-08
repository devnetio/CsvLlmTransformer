using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;

namespace CsvLlm.Core.Interface;

public interface ICsvReader
{
    Task<List<RowData>> ReadAsync(
        string path,
        InputConfig config,
        CancellationToken cancellationToken);
}