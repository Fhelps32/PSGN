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
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.IdUsuario);

            //REVISÃO: a matrícula identifica o usuário em toda a sincronização, então o banco passa a proibir duplicata.
            //O SQL Server não deixa criar índice em nvarchar(max), por isso o tamanho máximo de 50.
            builder.Property(u => u.Matricula).HasMaxLength(50);
            builder.HasIndex(u => u.Matricula).IsUnique();
        }
    }
}
