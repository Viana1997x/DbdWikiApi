using DbdWikiApi.Models;
using DbdWikiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DbdWikiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly UserService _userService;

    public ProfilesController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfileByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);
        if (user == null || !user.IsActive) return NotFound();

        // Mapeia para o DTO de resposta pública
        var response = new UserProfileResponseDto
        {
            // ... (mapeie os campos como fizemos no endpoint /me)
        };
        return Ok(response);
    }

    [HttpPost("{username}/comment")]
    [Authorize]
    public async Task<IActionResult> PostComment(string username, [FromBody] CommentDto dto)
    {
        var commenterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var commenterName = User.Identity?.Name;
        if (commenterId == null || commenterName == null) return Unauthorized();

        var profileOwner = await _userService.GetByUsernameAsync(username);
        if (profileOwner == null) return NotFound("Perfil não encontrado.");

        var comment = new ProfileComment
        {
            CommenterId = commenterId,
            CommenterDisplayName = commenterName, // Ou buscar o DisplayName do commenter
            Text = dto.Text
        };

        var (success, message) = await _userService.AddCommentAsync(profileOwner.Id!, comment);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }

    [HttpPost("{username}/rate")]
    [Authorize]
    public async Task<IActionResult> PostRating(string username, [FromBody] RatingDto dto)
    {
        var raterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (raterId == null) return Unauthorized();

        var profileOwner = await _userService.GetByUsernameAsync(username);
        if (profileOwner == null) return NotFound("Perfil não encontrado.");
        if (profileOwner.Id == raterId) return BadRequest("Você não pode avaliar seu próprio perfil.");

        var (success, message) = await _userService.AddOrUpdateRatingAsync(profileOwner.Id!, raterId, dto.Score);
        if (!success) return BadRequest(new { Message = message });

        return Ok(new { Message = message });
    }
}