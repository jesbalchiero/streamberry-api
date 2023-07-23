using Microsoft.EntityFrameworkCore;
using StreamberryApi.Models;

namespace StreamberryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Filme> Filmes { get; set; }
        public DbSet<Streaming> Streamings { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
    }
}
