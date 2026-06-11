namespace PortfolioRag.Api.Features.AskQuestion;

public sealed record AskQuestionResponse(IReadOnlyList<string> Sources, string Answer);