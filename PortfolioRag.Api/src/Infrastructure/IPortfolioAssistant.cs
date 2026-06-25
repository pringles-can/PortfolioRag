namespace PortfolioRag.Api.Infrastructure;

public interface IPortfolioAssistant
{
    Task<string> AnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken);
}