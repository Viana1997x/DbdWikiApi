using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models;

public class UpdateBioDto
{
    [MaxLength(500, ErrorMessage = "A biografia não pode ter mais de 500 caracteres.")]
    public string Bio { get; set; } = string.Empty;
}