using DbdWikiApi.Models;
using DbdWikiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DbdWikiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProfilesController : ControllerBase
    {
        private readonly UserService _userService;

        public ProfilesController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{username}")]
        [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { Message = "Perfil não encontrado ou inativo." });
            }

            // Mapeia o objeto User para o DTO de resposta pública
            var response = new UserProfileResponseDto
            {
                Id = user.Id!,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                ProfilePictureBase64 = user.ProfilePictureBase64, // <-- CORRIGIDO AQUI
                FavoriteKillers = user.FavoriteKillers,
                FavoriteSurvivors = user.FavoriteSurvivors,
                Comments = user.Comments
            };

            return Ok(response);
        }

        [HttpPost("{username}/comment")]
        [Authorize]
        public async Task<IActionResult> PostComment(string username, [FromBody] CommentDto dto)
        {
            var commenterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var commenterUser = await _userService.GetByIdAsync(commenterId!);
            if (commenterUser == null) return Unauthorized();

            var profileOwner = await _userService.GetByUsernameAsync(username);
            if (profileOwner == null || !profileOwner.IsActive) return NotFound(new { Message = "Perfil não encontrado." });

            var comment = new ProfileComment
            {
                CommenterId = commenterId!,
                CommenterDisplayName = commenterUser.DisplayName,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
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
            if (profileOwner == null || !profileOwner.IsActive) return NotFound(new { Message = "Perfil não encontrado." });
            if (profileOwner.Id == raterId) return BadRequest(new { Message = "Você não pode avaliar seu próprio perfil." });

            var (success, message) = await _userService.AddOrUpdateRatingAsync(profileOwner.Id!, raterId, dto.Score);
            if (!success) return BadRequest(new { Message = message });

            return Ok(new { Message = message });
        }
    }
}