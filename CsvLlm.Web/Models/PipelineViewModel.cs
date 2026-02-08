using System.ComponentModel.DataAnnotations;

namespace CsvLlm.Web.Models;

public class PipelineViewModel
{
    [Required]
    [Display(Name = "YAML Configuration")]
    public string YamlConfig { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Input CSV File")]
    public IFormFile? InputFile { get; set; }

    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }
}
