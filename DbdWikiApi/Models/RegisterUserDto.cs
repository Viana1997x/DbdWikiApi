using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class RegisterUserDto
{
    [Required(ErrorMessage = "O campo 'Usuario' é obrigatório.")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "O 'Usuario' deve ter entre 4 e 30 caracteres.")]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "O 'Usuario' deve conter apenas letras minúsculas, números e o caractere '_'.")]
    public string Usuario { get; set; } = null!;

    [Required(ErrorMessage = "O campo 'NomeDeUsuario' é obrigatório.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O 'NomeDeUsuario' deve ter no mínimo 3 caracteres.")]
    public string NomeDeUsuario { get; set; } = null!;

    [Required(ErrorMessage = "O campo 'Email' é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do 'Email' é inválido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "O campo 'Senha' é obrigatório.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A 'Senha' deve ter no mínimo 6 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "A senha deve conter no mínimo uma letra maiúscula, uma minúscula, um número e um caractere especial (@$!%*?&).")]
    public string Senha { get; set; } = null!;
}