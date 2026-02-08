using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Enum;
using Microsoft.Extensions.Logging;

namespace CsvLlm.Core.Service.Steps;

public sealed class ValidateSchemaStep : IPipelineStep
{
    public string Name => "Validate Schema";

    public Task ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("Validating rows against schema...");

        foreach (var row in context._rows)
        {
            row.Errors.Clear();
            bool isValid = true;

            foreach (var field in context.Schema.Fields)
            {
                if (field.Required && (!row.Values.TryGetValue(field.Name, out var value) || value == null || string.IsNullOrWhiteSpace(value.ToString())))
                {
                    row.Errors.Add($"Field '{field.Name}' is required.");
                    isValid = false;
                }
                
                // Additional validation logic could go here (type checking, enum validation etc.)
                if (row.Values.TryGetValue(field.Name, out var val) && val != null && field.Type == "enum" && field.Values != null)
                {
                    if (!field.Values.Contains(val.ToString() ?? ""))
                    {
                        row.Errors.Add($"Field '{field.Name}' value '{val}' is not in allowed list.");
                        isValid = false;
                    }
                }
            }

            row.State = isValid ? RowState.Transformed : RowState.Invalid;
        }

        return Task.CompletedTask;
    }
}
