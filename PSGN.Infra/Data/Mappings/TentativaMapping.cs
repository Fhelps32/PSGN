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
    public class TentativaMapping : IEntityTypeConfiguration<Tentativa>
    {
        public void Configure(EntityTypeBuilder<Tentativa> builder)
        {
            builder.HasKey(t => t.IdTentativa);
            builder.HasOne(t => t.AlunoModulo)
                   .WithMany(am => am.Tentativas)
                   .HasForeignKey(t => t.IdAlunoModulo)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
