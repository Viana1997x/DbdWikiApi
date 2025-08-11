using DbdWikiApi.Models;
using DbdWikiApi.Services;
// Não precisamos mais do IFileService, então o using foi removido.
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

    // Construtor corrigido: Apenas um construtor, sem IFileService ou IWebHostEnvironment
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

        // Corrigido: Usa a nova propriedade ProfilePictureBase64
        var response = new UserProfileResponseDto
        {
            Id = user.Id!,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            Bio = user.Bio,
            ProfilePictureBase64 = user.ProfilePictureBase64, // Corrigido
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

    [HttpPost("me/profile-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (file == null || file.Length == 0) return BadRequest("Nenhum arquivo enviado.");
        if (file.Length > 9 * 1024 * 1024) return BadRequest("O arquivo excede o tamanho máximo de 9MB.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var base64String = Convert.ToBase64String(fileBytes);
        var fullBase64String = $"data:{file.ContentType};base64,{base64String}";

        var (success, message) = await _userService.UpdateProfilePictureAsync(userId, fullBase64String);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = "Imagem de perfil atualizada com sucesso.", ProfilePictureBase64 = fullBase64String });
    }

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

    [HttpGet("admin/test")]
    [Authorize(Roles = "admin")]
    public IActionResult AdminOnlyRoute()
    {
        return Ok(new { Message = "Bem-vindo, Administrador!" });
    }
}