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
    public class ModuloMapping : IEntityTypeConfiguration<Modulo>
    {
        public void Configure(EntityTypeBuilder<Modulo> builder)
        {
            builder.HasKey(m => m.IdModulo);
            builder.HasOne(m => m.Curso)
                   .WithMany(c => c.Modulos)
                   .HasForeignKey(m => m.IdCurso)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
