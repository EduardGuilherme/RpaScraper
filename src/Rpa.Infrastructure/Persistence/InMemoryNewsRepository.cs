using System.Collections.Concurrent;
using Rpa.Domain.Entities;
using Rpa.Domain.Interfaces;

namespace Rpa.Infrastructure.Persistence;

public sealed class InMemoryNewsRepository : INewsRepository
{
    private readonly ConcurrentDictionary<Guid, NewsItem> _store = new();

    public Task SaveAsync(IEnumerable<NewsItem> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
            _store.TryAdd(item.Id, item);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<NewsItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsItem> result = _store.Values
            .OrderByDescending(n => n.ScrapedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<NewsItem>> GetBySourceAsync(string source, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsItem> result = _store.Values
            .Where(n => n.Source.Equals(source, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.ScrapedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<NewsItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var item) ? item : null);

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Count);
}
