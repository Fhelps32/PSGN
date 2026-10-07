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
                   .OnDelete(DeleteBehavior.Restrict); //tem algum bo rolando aq, mas to com preguissa de verificar
            builder.HasOne(am => am.Modulo)
                   .WithMany(m => m.Alunos)
                   .HasForeignKey(am => am.IdModulo)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
