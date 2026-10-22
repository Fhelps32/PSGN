using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Infra.Data.Mappings
{
    public class AlunoMapping : IEntityTypeConfiguration<Aluno>
    {
        public void Configure(EntityTypeBuilder<Aluno> builder)
        {
            builder.HasKey(a => a.IdAluno);
            builder.HasOne(a => a.Usuario)
                   .WithMany()
                   .HasForeignKey(a => a.IdUsuario)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Curso)
                   .WithMany(c => c.Alunos)
                   .HasForeignKey(a => a.IdCurso)
                   .OnDelete(DeleteBehavior.Cascade);

            //REVISÃO: um usuário é aluno de um curso uma vez só (antes o UsuarioService criava um Aluno por módulo)
            builder.HasIndex(a => new { a.IdUsuario, a.IdCurso }).IsUnique();
        }
    }
}
