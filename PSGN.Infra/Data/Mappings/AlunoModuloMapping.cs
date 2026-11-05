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
                   // Restrict é necessário: com Cascade, apagar um Curso chegaria aqui por dois caminhos e o SQL Server recusa a migration
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(am => am.Modulo)
                   .WithMany(m => m.Alunos)
                   .HasForeignKey(am => am.IdModulo)
                   .OnDelete(DeleteBehavior.Cascade);

            // O aluno é inscrito em cada módulo uma vez só
            builder.HasIndex(am => new { am.IdAluno, am.IdModulo }).IsUnique();

            // Propriedades calculadas da regra de tentativas (não são colunas)
            builder.Ignore(am => am.TentativasEmOrdem);
            builder.Ignore(am => am.TentativaAberta);
            builder.Ignore(am => am.Aprovado);
            builder.Ignore(am => am.PodeAbrirTentativa);
        }
    }
}
