using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Cursos
{
    public class CursoService
    {
        private readonly IExternoCurso _externoCurso;
        private readonly IPSGNDbContext _dbContext;
        private readonly IExternoCursoNSG _externoCursoNSG;

        public CursoService(IExternoCurso externoCurso, IPSGNDbContext dbContext, IExternoCursoNSG externoCursoNSG)
        {
            _externoCurso = externoCurso;
            _dbContext = dbContext;
            _externoCursoNSG = externoCursoNSG;
        }

        public async Task<IEnumerable<Curso>> SincronizarCursosAsync(string idNumber, int profundidade)
        {
            var cursosExternos = await _externoCurso.ObterCursosPeloIdNumberAsync(idNumber, profundidade);

            //REVISÃO: antes comparava pelo Nome. Se alguém renomeasse a categoria no Moodle, o curso seria cadastrado de novo
            //(duplicado). O id da categoria no Moodle não muda, então é ele que identifica o curso. E, em vez de trazer a
            //tabela Cursos inteira pra memória (ToList), trago só os IdMoodle, que é tudo que a comparação precisa.
            var idsMoodleCadastrados = (await _dbContext.Cursos.Select(c => c.IdMoodle).ToListAsync()).ToHashSet();
            var cursosParaAdicionar = cursosExternos //todos os cursos novos que vão ser cadastrados
                .Where(cursoExterno => !idsMoodleCadastrados.Contains(cursoExterno.Id));

            var cursosAdicionados = new List<Curso>(); //lista pra retornar no final da função

            foreach (var curso in cursosParaAdicionar)
            {
                var cursoNSG = await _externoCursoNSG.ObterCursoAsync(curso.Nome); //busca no NSG o coordenador e o período letivo
                if (cursoNSG is null)
                    continue; //o NSG não conhece esse curso, então não temos coordenador pra ele: pula pro próximo

                //REVISÃO: os dois ramos do if/else antigo (coordenador já existe / não existe) faziam quase a mesma coisa, e
                //o "Any" seguido do "FirstOrDefault" ia duas vezes no banco. Agora é uma chamada só. O SaveChangesAsync que
                //existia no meio do loop também saiu: o ObterOuCriarUsuarioAsync enxerga usuários adicionados e ainda não
                //salvos, então dois cursos com o mesmo coordenador novo não criam o usuário duas vezes.
                var coordenadorNSG = cursoNSG.Value.Coordenador;
                var coordenador = await _dbContext.ObterOuCriarUsuarioAsync(coordenadorNSG.Matricula, coordenadorNSG.Nome);

                //REVISÃO: o período letivo agora vem do NSG (antes era o 51 fixo no Curso.cs).
                //A sigla continua recebendo o nome: nem o Moodle nem o NSG mandam uma sigla de verdade ainda.
                var novoCurso = new Curso(curso.Nome, curso.Nome, curso.Id, cursoNSG.Value.PerLetivo, coordenador);
                _dbContext.Cursos.Add(novoCurso);

                cursosAdicionados.Add(novoCurso); //adiciona na lista de pra retornar no final da função
            }
            await _dbContext.SaveChangesAsync(); //grava no banco todos os cursos (e coordenadores novos) do loop
            return cursosAdicionados;
        }

        //REVISÃO: método novo. A sincronização de módulos e alunos precisa rodar para TODOS os cursos cadastrados, e não só
        //para os criados agora (esse era o bug dos módulos). Também é usado pela tela inicial para listar os cursos.
        public async Task<List<Curso>> ListarCursosAsync()
        {
            return await _dbContext.Cursos
                .Include(c => c.Coordenador) //sem o Include, curso.Coordenador viria null (o EF não carrega navegações sozinho)
                .Include(c => c.Modulos)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
