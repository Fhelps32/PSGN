using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Usuarios
{
    public class UsuarioService
    {
        private readonly IExternoUsuario _externoUsuario;
        private readonly IPSGNDbContext _dbContext;

        public UsuarioService(IExternoUsuario externoUsuario, IPSGNDbContext dbContext)
        {
            _externoUsuario = externoUsuario;
            _dbContext = dbContext;
        }

        // Cria usuários, alunos e inscrições a partir dos alunos de cada módulo do curso no Moodle. Pode rodar várias vezes sem duplicar. Usado pelo SincronizacaoService
        public async Task<List<AlunoModulo>> SincronizarAlunosDoCursoAsync(Curso curso)
        {
            var modulos = await _dbContext.Modulos.Where(m => m.IdCurso == curso.IdCurso).ToListAsync();

            var alunosModulosCriados = new List<AlunoModulo>();

            foreach (var modulo in modulos)
            {
                // O Moodle espera o id do MÓDULO ("course" do Moodle), não o da categoria do curso
                var alunosMoodle = await _externoUsuario.ObterAlunosDoModuloAsync(modulo.IdMoodle);

                foreach (var alunoMoodle in alunosMoodle)
                {
                    // Busca ou cria em três níveis: Usuario -> Aluno -> AlunoModulo
                    var usuario = await _dbContext.ObterOuCriarUsuarioAsync(alunoMoodle.Matricula, alunoMoodle.Nome);

                    // Um Aluno por (usuário, curso)
                    var aluno = await _dbContext.Alunos
                        .FirstOrDefaultAsync(a => a.IdUsuario == usuario.IdUsuario && a.IdCurso == curso.IdCurso);
                    if (aluno is null)
                    {
                        // Inscrito num módulo ainda não é egresso
                        aluno = new Aluno(usuario, curso, egresso: false);
                        _dbContext.Alunos.Add(aluno);
                    }

                    // Uma inscrição por (aluno, módulo); aluno recém-criado (IdAluno 0) com certeza ainda não tem
                    var jaInscrito = aluno.IdAluno != 0 && await _dbContext.AlunosModulos
                        .AnyAsync(am => am.IdAluno == aluno.IdAluno && am.IdModulo == modulo.IdModulo);
                    if (!jaInscrito)
                    {
                        var alunoModuloNovo = new AlunoModulo(aluno, modulo);
                        _dbContext.AlunosModulos.Add(alunoModuloNovo);

                        alunosModulosCriados.Add(alunoModuloNovo);
                    }
                }

                // Salva a cada módulo para o próximo módulo já encontrar o aluno no banco
                await _dbContext.SaveChangesAsync();
            }

            return alunosModulosCriados;
        }
    }
}
