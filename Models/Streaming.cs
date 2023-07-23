using System.ComponentModel.DataAnnotations;

namespace StreamberryApi.Models
{
    public class Streaming
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }
    }
}
