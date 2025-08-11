using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdateProfilePictureDto
{
    [Required]
    [Url(ErrorMessage = "A URL da imagem de perfil é inválida.")]
    public string Url { get; set; } = null!;
}