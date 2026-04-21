namespace Rpa.Domain.Entities;

public sealed class NewsItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime ScrapedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    private NewsItem() { }

    public static NewsItem Create(
        string title,
        string summary,
        string url,
        string source,
        DateTime? publishedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new NewsItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Summary = summary ?? string.Empty,
            Url = url,
            Source = source ?? "Unknown",
            ScrapedAt = DateTime.UtcNow,
            PublishedAt = publishedAt
        };
    }
}
