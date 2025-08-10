using DbdWikiApi.Models;
using DbdWikiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DbdWikiApi.Controllers;

/// <summary>
/// Acessa os dados do universo de Dead by Daylight.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DbdWikiController : ControllerBase
{
    private readonly DbdWikiService _dbdWikiService;

    public DbdWikiController(DbdWikiService dbdWikiService) =>
        _dbdWikiService = dbdWikiService;

    // --- Endpoints para Killers ---

    /// <summary>
    /// Retorna uma lista com todos os Killers.
    /// </summary>
    /// <returns>Uma lista de objetos Killer.</returns>
    [HttpGet("killers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<Killer>> GetKillers() =>
        await _dbdWikiService.GetKillersAsync();

    /// <summary>
    /// Busca um Killer específico pelo seu ID do MongoDB.
    /// </summary>
    /// <param name="id">O ID de 24 caracteres do Killer no banco de dados.</param>
    /// <returns>O objeto do Killer correspondente.</returns>
    /// <response code="200">Retorna o Killer encontrado.</response>
    /// <response code="404">Se nenhum Killer com o ID fornecido for encontrado.</response>
    [HttpGet("killers/{id:length(24)}")]
    [ProducesResponseType(typeof(Killer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Killer>> GetKiller(string id)
    {
        var killer = await _dbdWikiService.GetKillerAsync(id);
        if (killer is null)
        {
            return NotFound();
        }
        return killer;
    }

    /// <summary>
    /// Busca todos os Add-ons (complementos) de um Killer específico.
    /// </summary>
    /// <param name="killerId">O ID de 24 caracteres do Killer no banco de dados.</param>
    /// <returns>Uma lista de Addons para o Killer especificado.</returns>
    [HttpGet("killers/{killerId:length(24)}/addons")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<Addon>> GetAddonsForKiller(string killerId) =>
        await _dbdWikiService.GetAddonsByKillerIdAsync(killerId);

    /// <summary>
    /// Busca todas as Perks (vantagens) de um Killer específico pelo nome.
    /// </summary>
    /// <param name="killerName">O nome do Killer (ex: "The Trapper").</param>
    /// <returns>Uma lista de KillerPerks para o Killer especificado.</returns>
    [HttpGet("killers/perks/{killerName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<KillerPerk>> GetPerksForKiller(string killerName) =>
        await _dbdWikiService.GetPerksByKillerNameAsync(killerName);

    // --- Endpoints para Survivors ---

    /// <summary>
    /// Retorna uma lista com todos os Survivors.
    /// </summary>
    /// <returns>Uma lista de objetos Survivor.</returns>
    [HttpGet("survivors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<Survivor>> GetSurvivors() =>
        await _dbdWikiService.GetSurvivorsAsync();

    /// <summary>
    /// Busca um Survivor específico pelo seu ID do MongoDB.
    /// </summary>
    /// <param name="id">O ID de 24 caracteres do Survivor no banco de dados.</param>
    /// <returns>O objeto do Survivor correspondente.</returns>
    /// <response code="200">Retorna o Survivor encontrado.</response>
    /// <response code="404">Se nenhum Survivor com o ID fornecido for encontrado.</response>
    [HttpGet("survivors/{id:length(24)}")]
    [ProducesResponseType(typeof(Survivor), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Survivor>> GetSurvivor(string id)
    {
        var survivor = await _dbdWikiService.GetSurvivorAsync(id);
        if (survivor is null)
        {
            return NotFound();
        }
        return survivor;
    }

    /// <summary>
    /// Busca todas as Perks (vantagens) de um Survivor específico pelo nome.
    /// </summary>
    /// <param name="survivorName">O nome do Survivor (ex: "Dwight Fairfield").</param>
    /// <returns>Uma lista de SurvivorPerks para o Survivor especificado.</returns>
    [HttpGet("survivors/perks/{survivorName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SurvivorPerk>> GetPerksForSurvivor(string survivorName) =>
        await _dbdWikiService.GetPerksBySurvivorNameAsync(survivorName);
}