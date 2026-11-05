using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSGN.Infra.Migrations
{
    /// <summary>
    /// Troca Tentativas.Prazo de "time" (aceita menos de 24h) para "bigint" em ticks, que cabe o prazo de 40 dias.
    /// O SQL Server não converte time em bigint, então a coluna é recriada; as tentativas que já existirem recebem o prazo padrão (40 dias).
    /// </summary>
    public partial class prazo_tentativa_em_ticks : Migration
    {
        private const long QuarentaDiasEmTicks = 40L * 24 * 60 * 60 * TimeSpan.TicksPerSecond;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prazo",
                table: "Tentativas");

            migrationBuilder.AddColumn<long>(
                name: "Prazo",
                table: "Tentativas",
                type: "bigint",
                nullable: false,
                defaultValue: QuarentaDiasEmTicks);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prazo",
                table: "Tentativas");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Prazo",
                table: "Tentativas",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero);
        }
    }
}
