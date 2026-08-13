using System.Collections.Concurrent;

namespace AuthenticationAuthorizationDocuments.Documents;

internal sealed record Document(Guid Id, string OwnerId, string Title, string Content);

internal sealed record CreateDocumentRequest(string Title, string Content);

internal sealed record DocumentResponse(Guid Id, string Title, string Content)
{
    public static DocumentResponse From(Document document) => new(document.Id, document.Title, document.Content);
}

internal interface IDocumentRepository
{
    Document? Find(Guid id);
    Document Add(string ownerId, string title, string content);
}

internal sealed class InMemoryDocumentRepository : IDocumentRepository
{
    public static readonly Guid AliceDocumentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly ConcurrentDictionary<Guid, Document> _documents = new();

    public InMemoryDocumentRepository()
    {
        _documents[AliceDocumentId] = new(AliceDocumentId, "alice", "Release plan", "Ship safely.");
    }

    public Document? Find(Guid id) => _documents.GetValueOrDefault(id);

    public Document Add(string ownerId, string title, string content)
    {
        var document = new Document(Guid.NewGuid(), ownerId, title, content);
        _documents[document.Id] = document;
        return document;
    }
}
