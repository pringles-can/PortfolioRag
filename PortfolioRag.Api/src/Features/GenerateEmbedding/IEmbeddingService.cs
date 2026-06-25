namespace PortfolioRag.Api.Features.GenerateEmbedding;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(
        string text,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken);
}
