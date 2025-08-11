using DbdWikiApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DbdWikiApi.Services;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IConfiguration _configuration;

    public UserService(IOptions<DbdWikiDatabaseSettings> dbdWikiDatabaseSettings, IConfiguration configuration)
    {
        _configuration = configuration;
        var mongoClient = new MongoClient(dbdWikiDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbdWikiDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>("users");
    }

    public async Task<User?> GetByIdAsync(string id) => await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    public async Task<User?> GetByUsernameAsync(string username) => await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto dto)
    {
        if (await _usersCollection.Find(u => u.Username == dto.Usuario && u.IsActive).AnyAsync())
            return (false, "Este nome de usuário já está em uso por uma conta ativa.");
        if (await _usersCollection.Find(u => u.Email == dto.Email && u.IsActive).AnyAsync())
            return (false, "Este e-mail já está cadastrado em uma conta ativa.");

        var newUser = new User
        {
            Username = dto.Usuario,
            DisplayName = dto.NomeDeUsuario,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };
        await _usersCollection.InsertOneAsync(newUser);
        return (true, "Usuário registrado com sucesso!");
    }

    public async Task<(bool Success, string TokenOrMessage)> LoginAsync(LoginDto dto)
    {
        var user = await GetByUsernameAsync(dto.Username);
        if (user == null || !user.IsActive) return (false, "Usuário ou senha inválidos.");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return (false, "Usuário ou senha inválidos.");
        return (true, GenerateJwtToken(user));
    }

    public async Task<(bool Success, string Message)> UpdateDisplayNameAsync(string userId, string newDisplayName)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");
        user.DisplayName = newDisplayName;
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Nome de usuário atualizado com sucesso.");
    }

    public async Task<(bool Success, string Message)> UpdateEmailAsync(string userId, string newEmail)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");

        if (await _usersCollection.Find(u => u.Email == newEmail && u.IsActive && u.Id != userId).AnyAsync())
        {
            return (false, "O novo e-mail já está em uso por outra conta.");
        }

        user.Email = newEmail;
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "E-mail atualizado com sucesso.");
    }

    public async Task<(bool Success, string Message)> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return (false, "A senha atual está incorreta.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Senha atualizada com sucesso.");
    }

    public async Task<(bool Success, string Message)> UpdateBioAsync(string userId, string newBio)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");
        user.Bio = newBio;
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Biografia atualizada com sucesso.");
    }

    public async Task<(bool Success, string Message)> UpdateProfilePictureAsync(string userId, string newUrl)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");
        user.ProfilePictureUrl = newUrl;
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Imagem de perfil atualizada com sucesso.");
    }

    public async Task<(bool Success, string Message)> DeactivateUserAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return (false, "Usuário não encontrado.");
        if (!user.IsActive) return (true, "Usuário já estava inativo.");
        user.IsActive = false;
        await _usersCollection.ReplaceOneAsync(u => u.Id == id, user);
        return (true, "Usuário desativado com sucesso.");
    }

    public async Task<(bool Success, string Message)> AddOrUpdateRatingAsync(string profileOwnerId, string raterId, int score)
    {
        var user = await GetByIdAsync(profileOwnerId);
        if (user == null) return (false, "Usuário não encontrado.");

        // Remove a avaliação antiga do rater, se houver
        user.Ratings.RemoveAll(r => r.RaterId == raterId);

        // Adiciona a nova avaliação
        user.Ratings.Add(new ProfileRating { RaterId = raterId, Score = score });

        await _usersCollection.ReplaceOneAsync(u => u.Id == profileOwnerId, user);
        return (true, "Avaliação registrada com sucesso.");
    }

    public async Task<(bool Success, string Message)> AddCommentAsync(string profileOwnerId, ProfileComment comment)
    {
        var user = await GetByIdAsync(profileOwnerId);
        if (user == null) return (false, "Usuário não encontrado.");

        user.Comments.Add(comment);
        await _usersCollection.ReplaceOneAsync(u => u.Id == profileOwnerId, user);
        return (true, "Comentário adicionado com sucesso.");
    }

    public async Task<(bool Success, string Message)> UpdateFavoritesAsync(string userId, List<CharacterBuild> killers, List<CharacterBuild> survivors)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return (false, "Usuário não encontrado.");

        user.FavoriteKillers = killers;
        user.FavoriteSurvivors = survivors;

        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Favoritos atualizados com sucesso.");
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}