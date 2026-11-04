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

            //REVISÃO: o Prazo era gravado como "time" (padrão do EF para TimeSpan), que no SQL Server só aceita menos de 24 horas.
            //O prazo da tentativa é de 40 dias, então agora é gravado em ticks (bigint). EXIGE MIGRATION: Add-Migration prazo_tentativa_em_ticks
            builder.Property(t => t.Prazo)
                   .HasConversion<long>();

            //REVISÃO: propriedades CALCULADAS da regra de tentativas (não são colunas): o EF não deve tentar mapeá-las
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
