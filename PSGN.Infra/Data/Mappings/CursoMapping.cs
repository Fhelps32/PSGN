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
    public class CursoMapping : IEntityTypeConfiguration<Curso>
    {
        public void Configure(EntityTypeBuilder<Curso> builder)
        {
            builder.HasKey(c => c.IdCurso);
            builder.HasOne(c => c.Coordenador)
                   .WithMany()
                   .HasForeignKey(c => c.IdCoordenador)
                   .OnDelete(DeleteBehavior.Restrict);

            //REVISÃO: cada categoria do Moodle vira um único curso. O índice garante isso no banco e deixa a busca por IdMoodle rápida.
            builder.HasIndex(c => c.IdMoodle).IsUnique();
        }
    }
}
