using PortfolioRag.Api.Features.IngestDocument;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Features.IngestDocument;

public sealed class IngestApiKeyTests
{
    [Fact]
    public void IsAuthorized_MatchingKey_ReturnsTrue()
    {
        Assert.True(IngestApiKey.IsAuthorized("super-secret", "super-secret"));
    }

    [Theory]
    [InlineData("wrong", "super-secret")]
    [InlineData("", "super-secret")]
    [InlineData(null, "super-secret")]
    [InlineData("Super-Secret", "super-secret")] // case-sensitive
    public void IsAuthorized_MismatchedOrMissingProvidedKey_ReturnsFalse(
        string provided,
        string configured)
    {
        Assert.False(IngestApiKey.IsAuthorized(provided, configured));
    }

    [Theory]
    [InlineData("anything", "")]
    [InlineData("anything", null)]
    [InlineData(null, null)]
    public void IsAuthorized_NoConfiguredKey_FailsClosed(
        string provided,
        string configured)
    {
        Assert.False(IngestApiKey.IsAuthorized(provided, configured));
    }
}
