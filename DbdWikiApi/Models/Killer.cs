using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class Killer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("URIName")]
    public string URIName { get; set; } = null!;

    [BsonElement("killerName")]
    public string KillerName { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("Bio")]
    public string Bio { get; set; } = null!;

    [BsonElement("Difficulty")]
    public string Difficulty { get; set; } = null!;

    [BsonElement("PowerName")]
    public string PowerName { get; set; } = null!;

    [BsonElement("PowerIconURL")]
    public string PowerIconURL { get; set; } = null!;

    [BsonElement("Role")]
    public string Role { get; set; } = null!;

    [BsonElement("iconURL")]
    public string IconURL { get; set; } = null!;

    [BsonElement("link")]
    public string Link { get; set; } = null!;
}