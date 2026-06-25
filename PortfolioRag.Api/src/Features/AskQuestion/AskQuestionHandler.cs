using PortfolioRag.Api.Features.SearchKnowledge;
using PortfolioRag.Api.Infrastructure;

namespace PortfolioRag.Api.Features.AskQuestion;

public sealed class AskQuestionHandler
{
    private const int TopK = 5;

    private readonly ISearchKnowledgeService _searchKnowledgeService;
    private readonly IPortfolioAssistant _portfolioAssistant;

    public AskQuestionHandler(
        ISearchKnowledgeService searchKnowledgeService,
        IPortfolioAssistant portfolioAssistant)
    {
        _searchKnowledgeService = searchKnowledgeService;
        _portfolioAssistant = portfolioAssistant;
    }

    public async Task<AskQuestionResponse> Handle(
        AskQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var matches = await _searchKnowledgeService.SearchAsync(
            request.Question,
            TopK,
            cancellationToken);

        var context = string.Join(
            $"{Environment.NewLine}{Environment.NewLine}---{Environment.NewLine}{Environment.NewLine}",
            matches.Select(match => $"""
                                     Source: {match.Source}

                                     {match.Content}
                                     """));

        var answer = await _portfolioAssistant.AnswerAsync(
            request.Question,
            context,
            cancellationToken);

        return new AskQuestionResponse(
            Sources: matches
                .Select(match => match.Source)
                .Distinct()
                .ToList(),
            Answer: answer);
    }
}
