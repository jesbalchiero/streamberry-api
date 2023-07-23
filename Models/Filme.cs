using System.ComponentModel.DataAnnotations;

namespace StreamberryApi.Models
{
    public class Filme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public List<Streaming> Streamings { get; set; } = new List<Streaming>();
        public List<Genero> Generos { get; set; } = new List<Genero>();
        public List<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

        public DateTime DataLancamento { get; set; }
    }
}
