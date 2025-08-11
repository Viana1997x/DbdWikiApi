using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbdWikiApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("role")]
    public string Role { get; set; } = null!;

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("bio")]
    public string Bio { get; set; } = string.Empty;

    // O novo campo que queremos usar.
    [BsonElement("profilePictureBase64")]
    [BsonIgnoreIfNull]
    public string ProfilePictureBase64 { get; set; } = string.Empty;

    // O campo antigo, para ler dados legados. Será ignorado ao guardar.
    [BsonElement("profilePictureUrl")]
    [BsonIgnoreIfNull]
    [BsonIgnoreIfDefault]
    public string ProfilePictureUrl { get; set; } = string.Empty;

    [BsonElement("favoriteKillers")]
    public List<CharacterBuild> FavoriteKillers { get; set; } = new List<CharacterBuild>();

    [BsonElement("favoriteSurvivors")]
    public List<CharacterBuild> FavoriteSurvivors { get; set; } = new List<CharacterBuild>();

    [BsonElement("ratings")]
    public List<ProfileRating> Ratings { get; set; } = new List<ProfileRating>();

    [BsonElement("comments")]
    public List<ProfileComment> Comments { get; set; } = new List<ProfileComment>();

    public User()
    {
        Role = "user";
        IsActive = true;
    }
}