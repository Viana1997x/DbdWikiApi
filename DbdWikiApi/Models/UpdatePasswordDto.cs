using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdatePasswordDto
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string SenhaAtual { get; set; } = null!;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A 'Senha' deve ter no mínimo 6 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "A nova senha deve conter no mínimo uma letra maiúscula, uma minúscula, um número e um caractere especial (@$!%*?&).")]
    public string NovaSenha { get; set; } = null!;
}