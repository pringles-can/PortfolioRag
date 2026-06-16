using PortfolioRag.Api.Infrastructure;
using PortfolioRag.Api.Infrastructure.OpenAI;

namespace PortfolioRag.Api.Features.AskQuestion;

public sealed class AskQuestionHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly IPortfolioAssistant _portfolioAssistant;

    public AskQuestionHandler(
        IWebHostEnvironment environment,
        IPortfolioAssistant portfolioAssistant)
    {
        _environment = environment;
        _portfolioAssistant = portfolioAssistant;
    }

    public async Task<AskQuestionResponse> Handle(
        AskQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var docsPath = Path.Combine(
            _environment.ContentRootPath,
            "docs");

        var markdownFiles = Directory
            .GetFiles(docsPath, "*.md")
            .OrderBy(x => x)
            .ToList();

        var documentSections = new List<string>();

        foreach (var file in markdownFiles)
        {
            var fileName = Path.GetFileName(file);

            var text = await File.ReadAllTextAsync(
                file,
                cancellationToken);

            documentSections.Add($"""
                                  Source: {fileName}

                                  {text}
                                  """);
        }

        var context = string.Join(
            $"{Environment.NewLine}{Environment.NewLine}---{Environment.NewLine}{Environment.NewLine}",
            documentSections);

        var answer = await _portfolioAssistant.AnswerAsync(
            request.Question,
            context,
            cancellationToken);

        return new AskQuestionResponse(
            Answer: answer,
            Sources: markdownFiles
                .Select(Path.GetFileName)
                .Where(x => x is not null)
                .Cast<string>()
                .ToList());
    }
}