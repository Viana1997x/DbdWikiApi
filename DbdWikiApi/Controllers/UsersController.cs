using DbdWikiApi.Models;
using DbdWikiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DbdWikiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // --- ROTAS PÚBLICAS ---

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var (success, message) = await _userService.RegisterAsync(dto);
        if (!success) return BadRequest(new { Message = message });
        return Created("", new { Message = message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (success, tokenOrMessage) = await _userService.LoginAsync(dto);
        if (!success) return Unauthorized(new { Message = tokenOrMessage }); // 401 Unauthorized
        return Ok(new { Token = tokenOrMessage });
    }

    // --- ROTAS PROTEGIDAS (precisam de token JWT) ---

    /// <summary>
    /// Busca um usuário pelo seu ID. Requer autenticação.
    /// </summary>
    [HttpGet("id/{id:length(24)}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var response = new UserResponseDto // Usando o DTO de resposta
        {
            Id = user.Id!,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Busca um usuário pelo seu nome de usuário (username). Requer autenticação.
    /// </summary>
    [HttpGet("username/{username}")]
    [Authorize] // Protege a rota, só usuários logados podem buscar outros usuários
    public async Task<IActionResult> GetByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);
        if (user == null)
        {
            return NotFound(new { Message = "Usuário não encontrado." });
        }

        // Mapeia para o DTO seguro antes de retornar
        var response = new UserResponseDto
        {
            Id = user.Id!,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    // Outros GETs por username/email podem ser feitos de forma similar...

    /// <summary>
    /// Atualiza o nome de exibição do próprio usuário logado.
    /// </summary>
    [HttpPut("me/displayname")]
    [Authorize]
    public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _userService.UpdateDisplayNameAsync(userId, dto.NomeDeUsuario);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }

    /// <summary>
    /// Atualiza o e-mail do próprio usuário logado.
    /// </summary>
    [HttpPut("me/email")]
    [Authorize]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _userService.UpdateEmailAsync(userId, dto.Email);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }

    /// <summary>
    /// Atualiza a senha do próprio usuário logado.
    /// </summary>nao 
    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _userService.UpdatePasswordAsync(userId, dto.SenhaAtual, dto.NovaSenha);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }

    /// <summary>
    /// Desativa a conta do próprio usuário logado (Soft Delete).
    /// </summary>
    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _userService.DeactivateUserAsync(userId);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }

    /// <summary>
    /// [Admin] Rota de exemplo que só pode ser acessada por administradores.
    /// </summary>
    [HttpGet("admin/test")]
    [Authorize(Roles = "admin")] // Só usuários com a role "admin" podem acessar
    public IActionResult AdminOnlyRoute()
    {
        return Ok(new { Message = "Bem-vindo, Administrador!" });
    }
}