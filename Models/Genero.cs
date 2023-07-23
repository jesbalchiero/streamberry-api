using System.ComponentModel.DataAnnotations;

namespace StreamberryApi.Models
{
    public class Genero
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }
    }
}
