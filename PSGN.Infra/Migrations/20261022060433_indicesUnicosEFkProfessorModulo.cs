using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSGN.Infra.Migrations
{
    /// <inheritdoc />
    public partial class indicesUnicosEFkProfessorModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //REVISÃO: migration gerada pelo EF a partir das mudanças nos Mappings:
            // - remove a FK/coluna duplicada "ModuloIdModulo" de ProfessoresModulos (o .WithMany() vazio no mapeamento)
            // - Matricula passa de nvarchar(max) para nvarchar(50), porque o SQL Server não indexa nvarchar(max)
            // - cria índices únicos: Usuarios.Matricula, Cursos.IdMoodle, Modulos.IdMoodle, Alunos(IdUsuario, IdCurso),
            //   AlunosModulos(IdAluno, IdModulo) e ProfessoresModulos(IdUsuario, IdModulo)
            // - os índices simples IX_..._IdUsuario / IX_..._IdAluno são removidos porque os índices compostos novos começam
            //   pela mesma coluna e cobrem as mesmas buscas
            //O EF avisa "may result in the loss of data" por causa da coluna removida e da redução do tamanho da Matricula;
            //nos bancos atuais a tabela ProfessoresModulos está vazia e a maior matrícula tem 6 caracteres.
            migrationBuilder.DropForeignKey(
                name: "FK_ProfessoresModulos_Modulos_ModuloIdModulo",
                table: "ProfessoresModulos");

            migrationBuilder.DropIndex(
                name: "IX_ProfessoresModulos_IdUsuario",
                table: "ProfessoresModulos");

            migrationBuilder.DropIndex(
                name: "IX_ProfessoresModulos_ModuloIdModulo",
                table: "ProfessoresModulos");

            migrationBuilder.DropIndex(
                name: "IX_AlunosModulos_IdAluno",
                table: "AlunosModulos");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "ModuloIdModulo",
                table: "ProfessoresModulos");

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Matricula",
                table: "Usuarios",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_IdUsuario_IdModulo",
                table: "ProfessoresModulos",
                columns: new[] { "IdUsuario", "IdModulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_IdMoodle",
                table: "Modulos",
                column: "IdMoodle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdMoodle",
                table: "Cursos",
                column: "IdMoodle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlunosModulos_IdAluno_IdModulo",
                table: "AlunosModulos",
                columns: new[] { "IdAluno", "IdModulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdUsuario_IdCurso",
                table: "Alunos",
                columns: new[] { "IdUsuario", "IdCurso" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Matricula",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_ProfessoresModulos_IdUsuario_IdModulo",
                table: "ProfessoresModulos");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_IdMoodle",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdMoodle",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_AlunosModulos_IdAluno_IdModulo",
                table: "AlunosModulos");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_IdUsuario_IdCurso",
                table: "Alunos");

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "ModuloIdModulo",
                table: "ProfessoresModulos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_IdUsuario",
                table: "ProfessoresModulos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_ModuloIdModulo",
                table: "ProfessoresModulos",
                column: "ModuloIdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_AlunosModulos_IdAluno",
                table: "AlunosModulos",
                column: "IdAluno");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessoresModulos_Modulos_ModuloIdModulo",
                table: "ProfessoresModulos",
                column: "ModuloIdModulo",
                principalTable: "Modulos",
                principalColumn: "IdModulo");
        }
    }
}
