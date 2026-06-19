using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using PortfolioRag.Api.Features.GenerateEmbedding;

namespace PortfolioRag.Api.Infrastructure.OpenAI;

public sealed class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;

    public OpenAiEmbeddingService(IOptions<OpenAiOptions> options)
    {
        _client = new EmbeddingClient(
            options.Value.EmbeddingModel,
            options.Value.ApiKey);
    }

    public async Task<float[]> EmbedAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var result = await _client.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return result.Value.ToFloats().ToArray();
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        var result = await _client.GenerateEmbeddingsAsync(
            texts,
            cancellationToken: cancellationToken);

        return result.Value
            .Select(embedding => embedding.ToFloats().ToArray())
            .ToList();
    }
}
