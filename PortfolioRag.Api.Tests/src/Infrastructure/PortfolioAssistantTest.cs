using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Moq;
using PortfolioRag.Api.Features.AskQuestion;
using PortfolioRag.Api.Infrastructure;
using PortfolioRag.Api.Infrastructure.OpenAI;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Infrastructure;

[TestSubject(typeof(IPortfolioAssistant))]
public class PortfolioAssistantTest
{
    private readonly Mock<IPortfolioAssistant> _aiAnswerServiceMock;

    public PortfolioAssistantTest()
    {
        _aiAnswerServiceMock = new Mock<IPortfolioAssistant>();
    }

    [Theory]
    [InlineData("What is AI?", "An intelligent machine context.")]
    [InlineData("Can you explain machine learning?", "Data-driven learning techniques.")]
    [InlineData("", "Empty question context.")]
    [InlineData("What is AI?", "")]
    [InlineData(null, "Null question context.")]
    [InlineData("What is AI?", null)]
    public async Task AnswerAsync_ShouldReturnExpectedResult(string question, string context)
    {
        // Arrange
        var expectedAnswer = "This is a mock answer.";
        _aiAnswerServiceMock
            .Setup(service => service.AnswerAsync(question, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnswer);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _aiAnswerServiceMock.Object.AnswerAsync(question, context, cancellationToken);

        // Assert
        Assert.Equal(expectedAnswer, result);
        _aiAnswerServiceMock.Verify(service =>
                service.AnswerAsync(question, context, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AnswerAsync_ShouldThrowException_IfCancellationRequested()
    {
        // Arrange
        var question = "What is AI?";
        var context = "Context data.";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Simulate cancellation.

        _aiAnswerServiceMock
            .Setup(service => service.AnswerAsync(question, context, cancellationTokenSource.Token))
            .ThrowsAsync(new TaskCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _aiAnswerServiceMock.Object.AnswerAsync(question, context, cancellationTokenSource.Token));

        _aiAnswerServiceMock.Verify(service =>
                service.AnswerAsync(question, context, cancellationTokenSource.Token),
            Times.Once);
    }

    [Fact]
    public async Task AnswerAsync_ShouldHandleNullResponse()
    {
        // Arrange
        var question = "What is AI?";
        var context = "Context of the question.";
        _aiAnswerServiceMock
            .Setup(service => service.AnswerAsync(question, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null!);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _aiAnswerServiceMock.Object.AnswerAsync(question, context, cancellationToken);

        // Assert
        Assert.Null(result);
        _aiAnswerServiceMock.Verify(service =>
                service.AnswerAsync(question, context, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AnswerAsync_ShouldThrowArgumentNullException_ForNullInputs()
    {
        // Arrange
        string question = null!;
        string context = null!;
        _aiAnswerServiceMock
            .Setup(service => service.AnswerAsync(question, context, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException());

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _aiAnswerServiceMock.Object.AnswerAsync(question, context, cancellationToken));

        _aiAnswerServiceMock.Verify(service =>
                service.AnswerAsync(question, context, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public sealed class AskQuestionHandlerTests
{
    [Fact]
    public async Task Handle_ReadsAllMarkdownDocs_AndReturnsSources()
    {
        var rootPath = CreateTempDocs(
            new Dictionary<string, string>
            {
                ["resume.md"] = "Steve has .NET experience.",
                ["work-order-tracker.md"] = "WorkOrderOperationsTracker uses vertical slices."
            });

        var assistant = new FakePortfolioAssistant("Test answer.");

        var handler = new AskQuestionHandler(
            new FakeWebHostEnvironment(rootPath),
            assistant);

        var response = await handler.Handle(
            new AskQuestionRequest("What experience does Steve have?"),
            CancellationToken.None);

        Assert.Equal("Test answer.", response.Answer);

        Assert.Contains("resume.md", response.Sources);
        Assert.Contains("work-order-tracker.md", response.Sources);

        Assert.Contains("Source: resume.md", assistant.ContextReceived);
        Assert.Contains("Steve has .NET experience.", assistant.ContextReceived);
        Assert.Contains("Source: work-order-tracker.md", assistant.ContextReceived);
        Assert.Contains("WorkOrderOperationsTracker uses vertical slices.", assistant.ContextReceived);
    }

    [Fact]
    public async Task Handle_PassesQuestionToAssistant()
    {
        var rootPath = CreateTempDocs(
            new Dictionary<string, string>
            {
                ["resume.md"] = "Steve is a backend engineer."
            });

        var assistant = new FakePortfolioAssistant("Answer.");

        var handler = new AskQuestionHandler(
            new FakeWebHostEnvironment(rootPath),
            assistant);

        await handler.Handle(
            new AskQuestionRequest("What is Steve's background?"),
            CancellationToken.None);

        Assert.Equal(
            "What is Steve's background?",
            assistant.QuestionReceived);
    }

    private static string CreateTempDocs(
        Dictionary<string, string> files)
    {
        var rootPath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"));

        var docsPath = Path.Combine(rootPath, "docs");

        Directory.CreateDirectory(docsPath);

        foreach (var file in files)
        {
            File.WriteAllText(
                Path.Combine(docsPath, file.Key),
                file.Value);
        }

        return rootPath;
    }

    private sealed class FakePortfolioAssistant : IPortfolioAssistant
    {
        private readonly string _answer;

        public FakePortfolioAssistant(string answer)
        {
            _answer = answer;
        }

        public string? QuestionReceived { get; private set; }

        public string? ContextReceived { get; private set; }

        public Task<string> AnswerQuestionAsync(
            string question,
            string context,
            CancellationToken cancellationToken)
        {
            QuestionReceived = question;
            ContextReceived = context;

            return Task.FromResult(_answer);
        }

        public Task<string> AnswerAsync(string question, string context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
        }

        public string ApplicationName { get; set; } = "PortfolioRag.Api.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; } = "Development";

        public string WebRootPath { get; set; }

        public IFileProvider WebRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}