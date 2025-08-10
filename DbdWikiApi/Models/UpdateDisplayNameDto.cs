using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdateDisplayNameDto
{
    [Required(ErrorMessage = "O novo nome de usuário é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O 'NomeDeUsuario' deve ter no mínimo 3 caracteres.")]
    public string NomeDeUsuario { get; set; } = null!;
}