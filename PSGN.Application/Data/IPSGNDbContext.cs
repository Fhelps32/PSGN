using Microsoft.EntityFrameworkCore;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Data
{
    public interface IPSGNDbContext
    {
        DbSet<Usuario> Usuarios { get; set; }
        DbSet<Aluno> Alunos { get; set; }
        DbSet<Curso> Cursos { get; set; }
        DbSet<ProfessorModulo> ProfessoresModulos { get; set; }
        DbSet<Modulo> Modulos { get; set; }
        DbSet<AlunoModulo> AlunosModulos { get; set; }
        DbSet<Tentativa> Tentativas { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
