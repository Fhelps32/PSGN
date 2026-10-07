using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSGN.Infra.Migrations
{
    /// <inheritdoc />
    public partial class primeira_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    IdCurso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCoordenador = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sigla = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerLetivo = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.IdCurso);
                    table.ForeignKey(
                        name: "FK_Cursos_Usuarios_IdCoordenador",
                        column: x => x.IdCoordenador,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    IdAluno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    Egresso = table.Column<bool>(type: "bit", nullable: false),
                    DataMatricula = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alunos", x => x.IdAluno);
                    table.ForeignKey(
                        name: "FK_Alunos_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alunos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    IdModulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    IdnumberMoodle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdCmidMoodle = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.IdModulo);
                    table.ForeignKey(
                        name: "FK_Modulos_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlunosModulos",
                columns: table => new
                {
                    IdAlunoModulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAluno = table.Column<int>(type: "int", nullable: false),
                    IdModulo = table.Column<int>(type: "int", nullable: false),
                    DataInscricao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunosModulos", x => x.IdAlunoModulo);
                    table.ForeignKey(
                        name: "FK_AlunosModulos_Alunos_IdAluno",
                        column: x => x.IdAluno,
                        principalTable: "Alunos",
                        principalColumn: "IdAluno",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlunosModulos_Modulos_IdModulo",
                        column: x => x.IdModulo,
                        principalTable: "Modulos",
                        principalColumn: "IdModulo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessoresModulos",
                columns: table => new
                {
                    IdProfessorModulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdModulo = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ModuloIdModulo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessoresModulos", x => x.IdProfessorModulo);
                    table.ForeignKey(
                        name: "FK_ProfessoresModulos_Modulos_IdModulo",
                        column: x => x.IdModulo,
                        principalTable: "Modulos",
                        principalColumn: "IdModulo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessoresModulos_Modulos_ModuloIdModulo",
                        column: x => x.ModuloIdModulo,
                        principalTable: "Modulos",
                        principalColumn: "IdModulo");
                    table.ForeignKey(
                        name: "FK_ProfessoresModulos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tentativas",
                columns: table => new
                {
                    IdTentativa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlunoModulo = table.Column<int>(type: "int", nullable: false),
                    IdAttempt = table.Column<int>(type: "int", nullable: false),
                    DataInicioTentativa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFimTentativa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Prazo = table.Column<TimeSpan>(type: "time", nullable: false),
                    DataInicioAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataFimAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nota = table.Column<double>(type: "float", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tentativas", x => x.IdTentativa);
                    table.ForeignKey(
                        name: "FK_Tentativas_AlunosModulos_IdAlunoModulo",
                        column: x => x.IdAlunoModulo,
                        principalTable: "AlunosModulos",
                        principalColumn: "IdAlunoModulo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdCurso",
                table: "Alunos",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_IdUsuario",
                table: "Alunos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_AlunosModulos_IdAluno",
                table: "AlunosModulos",
                column: "IdAluno");

            migrationBuilder.CreateIndex(
                name: "IX_AlunosModulos_IdModulo",
                table: "AlunosModulos",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdCoordenador",
                table: "Cursos",
                column: "IdCoordenador");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_IdCurso",
                table: "Modulos",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_IdModulo",
                table: "ProfessoresModulos",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_IdUsuario",
                table: "ProfessoresModulos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresModulos_ModuloIdModulo",
                table: "ProfessoresModulos",
                column: "ModuloIdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_Tentativas_IdAlunoModulo",
                table: "Tentativas",
                column: "IdAlunoModulo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfessoresModulos");

            migrationBuilder.DropTable(
                name: "Tentativas");

            migrationBuilder.DropTable(
                name: "AlunosModulos");

            migrationBuilder.DropTable(
                name: "Alunos");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
