using Microsoft.EntityFrameworkCore;
using NSGSimulado.API.Models;

namespace NSGSimulado.API.Data
{
    public class NSGDbContext : DbContext
    {
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        public NSGDbContext(DbContextOptions<NSGDbContext> options)
        : base(options)
        {
        }
    }
}
