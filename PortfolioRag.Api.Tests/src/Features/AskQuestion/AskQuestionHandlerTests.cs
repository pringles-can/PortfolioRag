using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PortfolioRag.Api.Features.AskQuestion;
using PortfolioRag.Api.Features.SearchKnowledge;
using PortfolioRag.Api.Infrastructure;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Features.AskQuestion;

[TestSubject(typeof(AskQuestionHandler))]
public sealed class AskQuestionHandlerTests
{
    [Fact]
    public async Task Handle_RetrievesMatches_AndBuildsContextForAssistant()
    {
        var search = new FakeSearchKnowledgeService(new[]
        {
            new KnowledgeMatch("resume.md", "Steve has .NET experience."),
            new KnowledgeMatch("roadmap.md", "Plans to add agents."),
        });
        var assistant = new FakePortfolioAssistant("Final answer.");

        var handler = new AskQuestionHandler(search, assistant);

        var response = await handler.Handle(
            new AskQuestionRequest("What experience does Steve have?"),
            CancellationToken.None);

        Assert.Equal("Final answer.", response.Answer);
        Assert.Equal("What experience does Steve have?", search.QueryReceived);

        Assert.Contains("Steve has .NET experience.", assistant.ContextReceived);
        Assert.Contains("roadmap.md", assistant.ContextReceived);

        Assert.Equal(new[] { "resume.md", "roadmap.md" }, response.Sources);
    }

    [Fact]
    public async Task Handle_DeduplicatesSources_WhenChunksShareADocument()
    {
        var search = new FakeSearchKnowledgeService(new[]
        {
            new KnowledgeMatch("resume.md", "Chunk one."),
            new KnowledgeMatch("resume.md", "Chunk two."),
        });
        var assistant = new FakePortfolioAssistant("Answer.");

        var handler = new AskQuestionHandler(search, assistant);

        var response = await handler.Handle(
            new AskQuestionRequest("Tell me about Steve."),
            CancellationToken.None);

        Assert.Equal(new[] { "resume.md" }, response.Sources);
    }

    [Fact]
    public async Task Handle_PassesQuestionToAssistant()
    {
        var search = new FakeSearchKnowledgeService(new[]
        {
            new KnowledgeMatch("resume.md", "content"),
        });
        var assistant = new FakePortfolioAssistant("Answer.");

        var handler = new AskQuestionHandler(search, assistant);

        await handler.Handle(
            new AskQuestionRequest("My question?"),
            CancellationToken.None);

        Assert.Equal("My question?", assistant.QuestionReceived);
    }

    private sealed class FakeSearchKnowledgeService : ISearchKnowledgeService
    {
        private readonly IReadOnlyList<KnowledgeMatch> _matches;

        public FakeSearchKnowledgeService(IReadOnlyList<KnowledgeMatch> matches) =>
            _matches = matches;

        public string QueryReceived { get; private set; }

        public Task<IReadOnlyList<KnowledgeMatch>> SearchAsync(
            string query,
            int topK,
            CancellationToken cancellationToken)
        {
            QueryReceived = query;
            return Task.FromResult(_matches);
        }
    }

    private sealed class FakePortfolioAssistant : IPortfolioAssistant
    {
        private readonly string _answer;

        public FakePortfolioAssistant(string answer) => _answer = answer;

        public string QuestionReceived { get; private set; }

        public string ContextReceived { get; private set; }

        public Task<string> AnswerAsync(
            string question,
            string context,
            CancellationToken cancellationToken)
        {
            QuestionReceived = question;
            ContextReceived = context;
            return Task.FromResult(_answer);
        }
    }
}
