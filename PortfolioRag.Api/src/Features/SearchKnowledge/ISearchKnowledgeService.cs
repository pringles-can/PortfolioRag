namespace PortfolioRag.Api.Features.SearchKnowledge;

public interface ISearchKnowledgeService
{
    /// <summary>
    /// Embeds <paramref name="query"/> and returns the <paramref name="topK"/>
    /// most similar chunks from the vector store, nearest first.
    /// </summary>
    Task<IReadOnlyList<KnowledgeMatch>> SearchAsync(
        string query,
        int topK,
        CancellationToken cancellationToken);
}
