using CsvLlm.Core.Interface;
using CsvLlm.Core.Model;
using CsvLlm.Core.Model.Config;
using CsvLlm.Core.Model.Llm;
using CsvLlm.Core.Service.Steps;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CsvLlm.Tests;

public class StepTests
{
    [Fact]
    public async Task LlmTransformStep_ShouldHandleOneToManyTransformation()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var rowId = "1";
        var originalRows = new List<RowData> { new RowData { Id = rowId } };
        var context = new PipelineContext(originalRows, new SchemaConfig(), new LlmConfig(), NullLogger.Instance);
        
        var llmResult = new LlmResult
        {
            Rows = new List<LlmRowResult>
            {
                new LlmRowResult { RowId = rowId, Values = new Dictionary<string, object?> { ["name"] = "Split 1" } },
                new LlmRowResult { RowId = rowId, Values = new Dictionary<string, object?> { ["name"] = "Split 2" } }
            }
        };

        mockLlmClient.Setup(x => x.TransformAsync(It.IsAny<LlmRequest>(), It.IsAny<LlmConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResult);

        var settings = new Dictionary<string, object?> { ["prompt_template"] = "test" };
        var step = new LlmTransformStep(mockLlmClient.Object, settings);

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal(2, context.Rows.Count);
        Assert.Equal("Split 1", context.Rows[0].Values["name"]);
        Assert.Equal("Split 2", context.Rows[1].Values["name"]);
        Assert.All(context.Rows, r => Assert.Equal(CsvLlm.Core.Model.Enum.RowState.Transformed, r.State));
    }

    [Fact]
    public async Task LlmTransformStep_ShouldHandleOneToZeroTransformation()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var rowId = "1";
        var originalRows = new List<RowData> { new RowData { Id = rowId } };
        var context = new PipelineContext(originalRows, new SchemaConfig(), new LlmConfig(), NullLogger.Instance);
        
        var llmResult = new LlmResult { Rows = new List<LlmRowResult>() }; // Empty results

        mockLlmClient.Setup(x => x.TransformAsync(It.IsAny<LlmRequest>(), It.IsAny<LlmConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResult);

        var settings = new Dictionary<string, object?> { ["prompt_template"] = "test" };
        var step = new LlmTransformStep(mockLlmClient.Object, settings);

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Empty(context.Rows);
    }
    [Fact]
    public async Task NormalizeHeadersStep_ShouldTrimAndLowercaseHeaders()
    {
        // Arrange
        var row1 = new RowData { Id = "1" };
        row1.Values["Full_Name"] = "John Doe";
        row1.Values["EMAIL"] = "john@example.com";
        var rows = new List<RowData> { row1 };
        var context = new PipelineContext(rows, new SchemaConfig(), new LlmConfig(), NullLogger.Instance);
        var step = new NormalizeHeadersStep(new Dictionary<string, object?>());

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        var firstRow = context.Rows[0];
        Assert.True(firstRow.Values.ContainsKey("full_name"));
        Assert.True(firstRow.Values.ContainsKey("email"));
        Assert.Equal("John Doe", firstRow.Values["full_name"]);
    }

    [Fact]
    public async Task ValidateSchemaStep_ShouldMarkRequiredFieldsMissingAsInvalid()
    {
        // Arrange
        var schema = new SchemaConfig
        {
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "full_name", Required = true },
                new FieldDefinition { Name = "email", Required = true }
            }
        };
        var row1 = new RowData { Id = "1" };
        row1.Values["full_name"] = "John Doe";
        var rows = new List<RowData> { row1 };
        var context = new PipelineContext(rows, schema, new LlmConfig(), NullLogger.Instance);
        var step = new ValidateSchemaStep();

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal(CsvLlm.Core.Model.Enum.RowState.Invalid, context.Rows[0].State);
        Assert.Contains("Field 'email' is required.", context.Rows[0].Errors);
    }
}
