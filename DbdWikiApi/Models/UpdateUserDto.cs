using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O 'NomeDeUsuario' deve ter no mínimo 3 caracteres.")]
    public string? NomeDeUsuario { get; set; }

    [EmailAddress(ErrorMessage = "O formato do 'Email' é inválido.")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "A 'Senha' deve ter no mínimo 6 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "A senha deve conter no mínimo uma letra maiúscula, uma minúscula, um número e um caractere especial (@$!%*?&).")]
    public string? Senha { get; set; }
}