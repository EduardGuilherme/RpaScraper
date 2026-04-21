using Rpa.Domain.Entities;

namespace Rpa.Domain.Interfaces;

public interface INewsRepository
{
    Task SaveAsync(IEnumerable<NewsItem> items, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsItem>> GetBySourceAsync(string source, CancellationToken cancellationToken = default);
    Task<NewsItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
