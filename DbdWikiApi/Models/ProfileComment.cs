using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class ProfileComment
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string CommenterId { get; set; } = null!;
    public string CommenterDisplayName { get; set; } = null!;
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}