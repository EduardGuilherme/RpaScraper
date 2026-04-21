using Microsoft.AspNetCore.Mvc;
using Rpa.Domain.Interfaces;

namespace Rpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class NewsController : ControllerBase
{
    private readonly INewsRepository _repository;

    public NewsController(INewsRepository repository)
        => _repository = repository;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("source/{source}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySource(string source, CancellationToken cancellationToken)
    {
        var items = await _repository.GetBySourceAsync(source, cancellationToken);
        return Ok(items);
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var count = await _repository.GetCountAsync(cancellationToken);
        return Ok(new { totalItems = count, generatedAt = DateTime.UtcNow });
    }
}
