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
                   .WithMany()
                   .HasForeignKey(pm => pm.IdModulo)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
