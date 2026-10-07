using Microsoft.EntityFrameworkCore;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Infra.Data
{
    public class PSGNDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<ProfessorModulo> ProfessoresModulos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<AlunoModulo> AlunosModulos { get; set; }
        public DbSet<Tentativa> Tentativas { get; set; }

        public PSGNDbContext(DbContextOptions<PSGNDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PSGNDbContext).Assembly);
        }
    }
}
