using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Rpa.Domain.Entities;
using Rpa.Domain.Interfaces;

namespace Rpa.Infrastructure.Scrapers;

public sealed class HackerNewsScraper : IScraper
{
    private const string BaseUrl = "https://hn.algolia.com/api/v1/search?tags=front_page&hitsPerPage=30";
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsScraper> _logger;

    public string SourceName => "Hacker News";

    public HackerNewsScraper(HttpClient httpClient, ILogger<HackerNewsScraper> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NewsItem>> ScrapeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando a contagem de {Source}", SourceName);

        var response = await _httpClient.GetFromJsonAsync<HnResponse>(BaseUrl, cancellationToken);

        if (response?.Hits is null || response.Hits.Count == 0)
        {
            _logger.LogWarning("Nenhum item retornado de {Source}", SourceName);
            return Array.Empty<NewsItem>();
        }

        var items = response.Hits
            .Where(h => !string.IsNullOrWhiteSpace(h.Title) && !string.IsNullOrWhiteSpace(h.Url))
            .Select(h => NewsItem.Create(
                title: h.Title!,
                summary: $"Points: {h.Points} | Comments: {h.NumComments}",
                url: h.Url!,
                source: SourceName,
                publishedAt: h.CreatedAt))
            .ToList();

        _logger.LogInformation("Scraped {Count} items {Source}", items.Count, SourceName);
        return items;
    }


    private sealed record HnResponse(
        [property: JsonPropertyName("hits")] List<HnHit> Hits);

    private sealed record HnHit(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("points")] int Points,
        [property: JsonPropertyName("num_comments")] int NumComments,
        [property: JsonPropertyName("created_at")] DateTime? CreatedAt);
}
