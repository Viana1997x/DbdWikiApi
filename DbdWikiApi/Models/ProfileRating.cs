using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class ProfileRating
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string RaterId { get; set; } = null!;
    public int Score { get; set; }
}