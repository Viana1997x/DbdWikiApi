namespace DbdWikiApi.Models;

public class UserProfileResponseDto
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Bio { get; set; } = string.Empty;

    // --- CAMPO ALTERADO ---
    public string ProfilePictureBase64 { get; set; } = string.Empty;

    public List<CharacterBuild> FavoriteKillers { get; set; } = new();
    public List<CharacterBuild> FavoriteSurvivors { get; set; } = new();
    public List<ProfileComment> Comments { get; set; } = new();
}