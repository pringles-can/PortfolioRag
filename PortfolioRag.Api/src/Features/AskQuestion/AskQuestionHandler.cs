using PortfolioRag.Api.Infrastructure;

namespace PortfolioRag.Api.Features.AskQuestion;

public sealed class AskQuestionHandler
{
    private readonly IAiAnswerService _aiAnswerService;
    private readonly IWebHostEnvironment _environment;

    public AskQuestionHandler(
        IAiAnswerService aiAnswerService,
        IWebHostEnvironment environment)
    {
        _aiAnswerService = aiAnswerService;
        _environment = environment;
    }

    public async Task<AskQuestionResponse> Handle(
        AskQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var resumePath = Path.Combine(
            _environment.ContentRootPath,
            "docs",
            "resume.md");

        var context = await File.ReadAllTextAsync(
            resumePath,
            cancellationToken);

        var answer = await _aiAnswerService.AnswerAsync(
            request.Question,
            context,
            cancellationToken);

        return new AskQuestionResponse(
            [answer],
            "resume.md");
    }
}