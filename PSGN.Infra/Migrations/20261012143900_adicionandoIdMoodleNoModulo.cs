using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSGN.Infra.Migrations
{
    /// <inheritdoc />
    public partial class adicionandoIdMoodleNoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdMoodle",
                table: "Modulos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdMoodle",
                table: "Modulos");
        }
    }
}
