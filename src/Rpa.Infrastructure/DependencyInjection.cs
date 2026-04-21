using Microsoft.Extensions.DependencyInjection;
using Rpa.Domain.Interfaces;
using Rpa.Infrastructure.Persistence;
using Rpa.Infrastructure.Resilience;
using Rpa.Infrastructure.Scrapers;

namespace Rpa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        
        services.AddSingleton<INewsRepository, InMemoryNewsRepository>();

       
        services.AddHttpClient<IScraper, HackerNewsScraper>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "RpaScraper/1.0");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddScraperResilience();

        return services;
    }
}
