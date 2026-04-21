using Rpa.Domain.Entities;

namespace Rpa.Domain.Interfaces;

public interface IScraper
{
    string SourceName { get; }
    Task<IReadOnlyList<NewsItem>> ScrapeAsync(CancellationToken cancellationToken = default);
}
