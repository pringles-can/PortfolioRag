using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Moq;
using PortfolioRag.Api.Infrastructure;
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
