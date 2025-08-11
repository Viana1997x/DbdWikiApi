using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models
{
    public class CommentDto
    {
        [Required(ErrorMessage = "O texto do comentário não pode estar vazio.")]
        [MaxLength(1000, ErrorMessage = "O comentário não pode exceder 1000 caracteres.")]
        public string Text { get; set; } = null!;
    }
}