# CsvLlmTransformer

CsvLlmTransformer is a powerful tool designed to transform CSV data using Large Language Models (LLMs). It allows you to define complex transformation logic using natural language prompts, validate the output against a strict schema, and export the results back to CSV.

## Features

- **LLM-Powered Transformations**: Use OpenAI (or other providers) to extract, transform, and enrich data from CSV rows.
- **Strict Schema Validation**: Ensure the LLM output matches your expected data types (string, number, boolean, enum, etc.).
- **Pipeline-Based Architecture**: Configure multiple steps including header normalization, column mapping, LLM transformations, and validation.
- **CLI & Web Interface**: Run transformations from your terminal or via a user-friendly ASP.NET Core MVC web application.
- **Extensible**: Easily add new pipeline steps or LLM providers.

## Project Structure

- `CsvLlm.Core`: The heart of the application, containing the pipeline engine, LLM clients, and core logic.
- `CsvLlm.Cli`: A command-line interface for running pipelines.
- `CsvLlm.Web`: An ASP.NET Core MVC web application for a visual experience.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- OpenAI API Key

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/devnetio/CsvLlmTransformer.git
   cd CsvLlmTransformer
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

### Running the CLI

1. Set your OpenAI API key as an environment variable:
   ```bash
   export OPENAI_API_KEY="your_api_key_here"
   ```

2. Run a pipeline using the sample configuration and input:
   ```bash
   dotnet run --project CsvLlm.Cli/CsvLlm.Cli.csproj -- run --input sample_input.csv --config sample_config.yaml --output out
   ```

### Running the Web UI

1. Set your OpenAI API key:
   ```bash
   export OPENAI_API_KEY="your_api_key_here"
   ```

2. Start the web application:
   ```bash
   dotnet run --project CsvLlm.Web/CsvLlm.Web.csproj
   ```

3. Open your browser and navigate to `https://localhost:5001`.

## Configuration

The pipeline is configured using a YAML file. You can define:

- **Input**: Delimiter, encoding, and header presence.
- **Output**: Filename pattern and format.
- **Schema**: Target fields with types and validation rules.
- **LLM**: Provider, model, temperature, and API key settings.
- **Steps**: A sequence of operations (e.g., `normalize_headers`, `llm_transform`, `validate`).

See `sample_config.yaml` for a complete example.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
