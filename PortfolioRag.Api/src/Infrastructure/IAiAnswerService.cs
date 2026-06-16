namespace PortfolioRag.Api.Infrastructure;

public interface IAiAnswerService
{
    Task<string> AnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken);
}