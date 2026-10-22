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

        //REVISÃO: removido o IExternoCurso (_extrernoCurso), que era injetado mas nunca usado
        public UsuarioService(IExternoUsuario externoUsuario, IPSGNDbContext dbContext)
        {
            _externoUsuario = externoUsuario;
            _dbContext = dbContext;
        }

        //REVISÃO: antes era CriarUsuarioAlunosPorIdCurso(int idCursoMoodle). Recebendo o Curso já carregado, não precisa
        //buscá-lo de novo no banco. "Sincronizar" porque agora pode rodar várias vezes sem duplicar nada.
        public async Task<List<AlunoModulo>> SincronizarAlunosDoCursoAsync(Curso curso)
        {
            //REVISÃO: antes tinha "ToList() ?? throw". O ToList() nunca devolve null (no máximo uma lista vazia), então
            //aquele throw nunca acontecia. Curso sem módulo simplesmente não entra no foreach.
            var modulos = await _dbContext.Modulos.Where(m => m.IdCurso == curso.IdCurso).ToListAsync();

            var alunosModulosCriados = new List<AlunoModulo>();

            foreach (var modulo in modulos)
            {
                //REVISÃO: BUG. Antes passava o idMoodle do CURSO, que no Moodle é uma CATEGORIA (ex.: 62). Só que o
                //core_enrol_get_enrolled_users espera o id de um "course" do Moodle, que no PSGN é o MÓDULO (ex.: 721).
                //Com o id da categoria o Moodle respondia [] e nenhum aluno era criado. (O "?? throw" que existia aqui
                //também nunca rodava: a implementação nunca devolve null.)
                var alunosMoodle = await _externoUsuario.ObterAlunosDoModuloAsync(modulo.IdMoodle);

                foreach (var alunoMoodle in alunosMoodle)
                {
                    //REVISÃO: BUG. Antes, se o usuário JÁ existisse no banco, o código não fazia nada, nem criava o vínculo
                    //com este módulo. Um aluno inscrito em 9 módulos só ganharia o primeiro. E se NÃO existisse, era criado
                    //de novo a cada módulo (o Any() não enxerga o que foi adicionado e ainda não salvo), duplicando usuário
                    //e aluno. Agora são 3 passos "busca ou cria", um para cada nível: Usuario -> Aluno -> AlunoModulo.
                    var usuario = await _dbContext.ObterOuCriarUsuarioAsync(alunoMoodle.Matricula, alunoMoodle.Nome);

                    //REVISÃO: 1 Aluno por (usuário, curso). Antes era criado um Aluno novo para cada módulo.
                    var aluno = await _dbContext.Alunos
                        .FirstOrDefaultAsync(a => a.IdUsuario == usuario.IdUsuario && a.IdCurso == curso.IdCurso);
                    if (aluno is null)
                    {
                        //REVISÃO: egresso = quem já concluiu o curso. Quem está inscrito num módulo agora não é egresso,
                        //então começa como false (antes era true fixo).
                        aluno = new Aluno(usuario, curso, egresso: false);
                        _dbContext.Alunos.Add(aluno);
                    }

                    //REVISÃO: 1 AlunoModulo por (aluno, módulo), para não duplicar quando a sincronização roda de novo.
                    //Se o aluno acabou de ser criado (IdAluno == 0), com certeza ainda não tem vínculo, nem precisa consultar.
                    var jaInscrito = aluno.IdAluno != 0 && await _dbContext.AlunosModulos
                        .AnyAsync(am => am.IdAluno == aluno.IdAluno && am.IdModulo == modulo.IdModulo);
                    if (!jaInscrito)
                    {
                        var alunoModuloNovo = new AlunoModulo(aluno, modulo);
                        _dbContext.AlunosModulos.Add(alunoModuloNovo);

                        alunosModulosCriados.Add(alunoModuloNovo);
                    }
                }

                //REVISÃO: salva ao fim de CADA módulo (antes era uma vez só, no final). Assim, quando o mesmo aluno aparecer
                //no próximo módulo, as consultas acima já o encontram no banco com IdUsuario/IdAluno preenchidos.
                await _dbContext.SaveChangesAsync();
            }

            return alunosModulosCriados;
        }
    }
}
