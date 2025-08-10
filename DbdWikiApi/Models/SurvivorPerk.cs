using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class SurvivorPerk
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("URIName")]
    public string URIName { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("content")]
    public string Content { get; set; } = null!;

    [BsonElement("contentText")]
    public string ContentText { get; set; } = null!;

    [BsonElement("characterName")]
    public string CharacterName { get; set; } = null!;

    [BsonElement("iconURL")]
    public string IconURL { get; set; } = null!;
}