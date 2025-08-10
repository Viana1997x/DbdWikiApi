using DbdWikiApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DbdWikiApi.Services;

public class DbdWikiService
{
    private readonly IMongoCollection<Killer> _killersCollection;
    private readonly IMongoCollection<Survivor> _survivorsCollection;
    private readonly IMongoCollection<Addon> _addonsCollection;
    private readonly IMongoCollection<KillerPerk> _killerPerksCollection;
    private readonly IMongoCollection<SurvivorPerk> _survivorPerksCollection;

    public DbdWikiService(IOptions<DbdWikiDatabaseSettings> dbdWikiDatabaseSettings)
    {
        var mongoClient = new MongoClient(dbdWikiDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbdWikiDatabaseSettings.Value.DatabaseName);

        _killersCollection = mongoDatabase.GetCollection<Killer>("killers");
        _survivorsCollection = mongoDatabase.GetCollection<Survivor>("survivors");
        _addonsCollection = mongoDatabase.GetCollection<Addon>("addons");
        _killerPerksCollection = mongoDatabase.GetCollection<KillerPerk>("killer_perks");
        _survivorPerksCollection = mongoDatabase.GetCollection<SurvivorPerk>("survivor_perks");
    }

    // Métodos para Killers
    public async Task<List<Killer>> GetKillersAsync() =>
        await _killersCollection.Find(_ => true).ToListAsync();

    public async Task<Killer?> GetKillerAsync(string id) =>
        await _killersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<Addon>> GetAddonsByKillerIdAsync(string killerId) =>
        await _addonsCollection.Find(x => x.KillerId == killerId).ToListAsync();

    public async Task<List<KillerPerk>> GetPerksByKillerNameAsync(string killerName) =>
        await _killerPerksCollection.Find(x => x.CharacterName == killerName).ToListAsync();

    // Métodos para Survivors
    public async Task<List<Survivor>> GetSurvivorsAsync() =>
        await _survivorsCollection.Find(_ => true).ToListAsync();

    public async Task<Survivor?> GetSurvivorAsync(string id) =>
        await _survivorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<SurvivorPerk>> GetPerksBySurvivorNameAsync(string survivorName) =>
        await _survivorPerksCollection.Find(x => x.CharacterName == survivorName).ToListAsync();
}

// Classe auxiliar para ler as configurações do appsettings.json
public class DbdWikiDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}