namespace PortfolioRag.Api.Infrastructure.OpenAI;

public sealed class OpenAiOptions
{
    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gpt-4.1-mini";
}