using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class Addon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("URIName")]
    public string URIName { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string Description { get; set; } = null!;

    [BsonElement("descriptionText")]
    public string DescriptionText { get; set; } = null!;

    [BsonElement("rarity")]
    public string Rarity { get; set; } = null!;

    [BsonElement("killer")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string KillerId { get; set; } = null!;

    [BsonElement("iconURL")]
    public string IconURL { get; set; } = null!;
}