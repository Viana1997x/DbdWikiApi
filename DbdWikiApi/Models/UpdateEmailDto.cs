using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdateEmailDto
{
    [Required(ErrorMessage = "O novo e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do 'Email' é inválido.")]
    public string Email { get; set; } = null!;
}