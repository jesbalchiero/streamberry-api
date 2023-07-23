using System.ComponentModel.DataAnnotations;

namespace StreamberryApi.Models
{
    public class Avaliacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FilmeId { get; set; }

        [Range(1, 5)]
        public int Pontuacao { get; set; }

        public string Comentario { get; set; }
    }
}
