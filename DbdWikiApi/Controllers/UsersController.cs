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
        if (!success) return Unauthorized(new { Message = tokenOrMessage });
        return Ok(new { Token = tokenOrMessage });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        var response = new UserProfileResponseDto
        {
            Id = user.Id!,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            FavoriteKillers = user.FavoriteKillers,
            FavoriteSurvivors = user.FavoriteSurvivors,
            Comments = user.Comments
        };
        return Ok(response);
    }

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

    [HttpPut("me/bio")]
    [Authorize]
    public async Task<IActionResult> UpdateBio([FromBody] UpdateBioDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var (success, message) = await _userService.UpdateBioAsync(userId, dto.Bio);
        if (!success) return BadRequest(new { Message = message });
        return Ok(new { Message = message });
    }

    [HttpPut("me/profile-picture")]
    [Authorize]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var (success, message) = await _userService.UpdateProfilePictureAsync(userId, dto.Url);
        if (!success) return BadRequest(new { Message = message });
        return Ok(new { Message = message });
    }

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

    // Endpoint para upload de foto de perfil
    [HttpPost("me/profile-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (file == null || file.Length == 0) return BadRequest("Nenhum arquivo enviado.");
        if (file.Length > 9 * 1024 * 1024) return BadRequest("O arquivo excede o tamanho máximo de 9MB.");

        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
        if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

        var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"{Request.Scheme}://{Request.Host}/images/profiles/{fileName}";

        var (success, message) = await _userService.UpdateProfilePictureAsync(userId, fileUrl);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message, Url = fileUrl });
    }

    // Endpoint para atualizar os favoritos
    [HttpPut("me/favorites")]
    [Authorize]
    public async Task<IActionResult> UpdateFavorites([FromBody] UpdateFavoritesDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _userService.UpdateFavoritesAsync(userId, dto.FavoriteKillers, dto.FavoriteSurvivors);
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