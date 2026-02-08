using CsvLlm.Core.Model.Enum;

namespace CsvLlm.Core.Model;

public sealed class RowData
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public Dictionary<string, object?> Values { get; } = new();
    public RowState State { get; set; } = RowState.Pending;
    public List<string> Errors { get; } = [];
}