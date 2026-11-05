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

            // Prazo em ticks (bigint): a coluna "time" só aceita menos de 24h. Migration: prazo_tentativa_em_ticks
            builder.Property(t => t.Prazo)
                   .HasConversion<long>();

            // Propriedades calculadas da regra de tentativas (não são colunas)
            builder.Ignore(t => t.Aberta);
            builder.Ignore(t => t.TemProva);
            builder.Ignore(t => t.DataLimite);
            builder.Ignore(t => t.Situacao);
            builder.Ignore(t => t.PodeAprovar);
            builder.Ignore(t => t.PodeReprovar);
            builder.Ignore(t => t.PodeReprovarPorInatividade);
        }
    }
}
