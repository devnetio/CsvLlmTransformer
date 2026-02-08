using CsvLlm.Core.Interface;
using CsvLlm.Core.Service;
using CsvLlm.Core.Service.Csv;
using CsvLlm.Core.Service.Llm;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register CsvLlm Core Services
builder.Services.AddSingleton<IConfigLoader, YamlConfigLoader>();
builder.Services.AddSingleton<ICsvReader, CsvReader>();
builder.Services.AddSingleton<ICsvWriter, CsvWriter>();
builder.Services.AddSingleton<IPipelineRunner, PipelineRunner>();

// Default API Key from Environment or Config
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "MISSING_API_KEY";

builder.Services.AddHttpClient<ILlmClient, OpenAiClient>((sp, client) =>
{
    // Configure HttpClient if needed
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
.AddTypedClient<ILlmClient>((httpClient, sp) => 
{
    var logger = sp.GetRequiredService<ILogger<OpenAiClient>>();
    return new OpenAiClient(httpClient, openAiApiKey, logger);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
