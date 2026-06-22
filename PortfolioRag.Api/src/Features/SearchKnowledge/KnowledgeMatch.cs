namespace PortfolioRag.Api.Features.SearchKnowledge;

/// <summary>A chunk retrieved from the vector store for a query.</summary>
public sealed record KnowledgeMatch(string Source, string Content);
