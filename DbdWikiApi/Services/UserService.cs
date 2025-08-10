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

    // --- Métodos de Busca (GET) ---
    public async Task<User?> GetByIdAsync(string id) => await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    public async Task<User?> GetByUsernameAsync(string username) => await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
    public async Task<User?> GetByEmailAsync(string email) => await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

    // --- Lógica de Registro (CREATE) ---
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto dto)
    {
        // Regra de negócio: só checar contra usuários ATIVOS
        if (await _usersCollection.Find(u => u.Username == dto.Usuario && u.IsActive).AnyAsync())
        {
            return (false, "Este nome de usuário já está em uso por uma conta ativa.");
        }

        if (await _usersCollection.Find(u => u.Email == dto.Email && u.IsActive).AnyAsync())
        {
            return (false, "Este e-mail já está cadastrado em uma conta ativa.");
        }

        var newUser = new User
        {
            Username = dto.Usuario,
            DisplayName = dto.NomeDeUsuario,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            // Role e IsActive já são definidos pelo construtor do Model
        };

        await _usersCollection.InsertOneAsync(newUser);
        return (true, "Usuário registrado com sucesso!");
    }

    // --- Lógica de Login (AUTENTICAÇÃO) ---
    public async Task<(bool Success, string TokenOrMessage)> LoginAsync(LoginDto dto)
    {
        var user = await GetByUsernameAsync(dto.Username);

        // O usuário não existe ou a conta está inativa?
        if (user == null || !user.IsActive)
        {
            return (false, "Usuário ou senha inválidos.");
        }

        // A senha está incorreta?
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return (false, "Usuário ou senha inválidos.");
        }

        // Se tudo estiver certo, gera o token JWT
        var token = GenerateJwtToken(user);
        return (true, token);
    }

    // --- Lógicas de Atualização Específicas ---

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

        // Checar se o novo email já não está em uso por outro usuário ATIVO
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

        // Verificar se a senha atual fornecida está correta
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return (false, "A senha atual está incorreta.");
        }

        // Se estiver correta, criptografar e salvar a nova senha
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return (true, "Senha atualizada com sucesso.");
    }

    // --- Lógica de Desativação (DELETE) ---
    public async Task<(bool Success, string Message)> DeactivateUserAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return (false, "Usuário não encontrado.");
        if (!user.IsActive) return (true, "Usuário já estava inativo.");

        user.IsActive = false;
        await _usersCollection.ReplaceOneAsync(u => u.Id == id, user);
        return (true, "Usuário desativado com sucesso.");
    }

    // --- Helper para Gerar o Token JWT ---
    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Claims são informações sobre o usuário que vão dentro do token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role), // Adiciona o Role como uma Claim
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8), // Duração do token
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}