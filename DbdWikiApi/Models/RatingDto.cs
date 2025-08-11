using System.ComponentModel.DataAnnotations;

namespace DbdWikiApi.Models
{
    public class RatingDto
    {
        [Required(ErrorMessage = "A pontuação é obrigatória.")]
        [Range(1, 5, ErrorMessage = "A pontuação deve ser um valor entre 1 e 5.")]
        public int Score { get; set; }
    }
}