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
    public class ProfessorModuloMapping : IEntityTypeConfiguration<ProfessorModulo>
    {
        public void Configure(EntityTypeBuilder<ProfessorModulo> builder)
        {
            builder.HasKey(pm => pm.IdProfessorModulo);
            builder.HasOne(pm => pm.Usuario)
                   .WithMany()
                   .HasForeignKey(pm => pm.IdUsuario)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(pm => pm.Modulo)
                   //REVISÃO: antes era ".WithMany()" vazio. Como o Modulo tem a coleção "Professores", o EF achou que eram
                   //DUAS relações diferentes e criou uma segunda FK escondida na tabela (coluna "ModuloIdModulo"). Ligando
                   //explicitamente à coleção, vira uma relação só, usando o IdModulo.
                   .WithMany(m => m.Professores)
                   .HasForeignKey(pm => pm.IdModulo)
                   .OnDelete(DeleteBehavior.Restrict);

            //REVISÃO: o mesmo professor não é vinculado duas vezes ao mesmo módulo
            builder.HasIndex(pm => new { pm.IdUsuario, pm.IdModulo }).IsUnique();
        }
    }
}
