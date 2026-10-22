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
    public class AlunoModuloMapping : IEntityTypeConfiguration<AlunoModulo>
    {
        public void Configure(EntityTypeBuilder<AlunoModulo> builder)
        {
            builder.HasKey(am => am.IdAlunoModulo);
            builder.HasOne(am => am.Aluno)
                   .WithMany(a => a.Modulos)
                   .HasForeignKey(am => am.IdAluno)
                   //REVISÃO: (sobre o "tem algum bo rolando aq") O Restrict aqui está CERTO e é necessário. Se fosse Cascade,
                   //apagar um Curso chegaria em AlunosModulos por dois caminhos (Curso -> Aluno -> AlunoModulo e
                   //Curso -> Modulo -> AlunoModulo), e o SQL Server recusa a migration com o erro "may cause cycles or
                   //multiple cascade paths". Com Restrict em um dos caminhos, o problema some.
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(am => am.Modulo)
                   .WithMany(m => m.Alunos)
                   .HasForeignKey(am => am.IdModulo)
                   .OnDelete(DeleteBehavior.Cascade);

            //REVISÃO: o aluno é inscrito em cada módulo uma vez só
            builder.HasIndex(am => new { am.IdAluno, am.IdModulo }).IsUnique();
        }
    }
}
