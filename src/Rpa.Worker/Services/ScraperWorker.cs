using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rpa.Domain.Interfaces;
using Rpa.Worker.Configuration;

namespace Rpa.Worker.Services;

public sealed class ScraperWorker : BackgroundService
{
    private readonly IScraper _scraper;
    private readonly INewsRepository _repository;
    private readonly ILogger<ScraperWorker> _logger;
    private readonly WorkerOptions _options;

    public ScraperWorker(
        IScraper scraper,
        INewsRepository repository,
        ILogger<ScraperWorker> logger,
        IOptions<WorkerOptions> options)
    {
        _scraper = scraper;
        _repository = repository;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ScraperWorker iniciado. Intervalo: {Interval} minutos",
            _options.IntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);

            await Task.Delay(
                TimeSpan.FromMinutes(_options.IntervalMinutes),
                stoppingToken);
        }
    }

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Ciclo de raspagem iniciando em {Time:O}", DateTime.UtcNow);

            var items = await _scraper.ScrapeAsync(cancellationToken);
            await _repository.SaveAsync(items, cancellationToken);

            var total = await _repository.GetCountAsync(cancellationToken);
            _logger.LogInformation(
                "Ciclo concluído. Itens salvos: {Novos}. Total na loja: {Total}",
                items.Count, total);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Ciclo de raspagem cancelado.");
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Exceção não tratada durante o ciclo de coleta de dados. Tentativa será feita novamente em {Minutes} minutos.",
                _options.IntervalMinutes);
        }
    }
}
