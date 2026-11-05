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

            // O curso é identificado pelo IdMoodle da categoria (o nome pode mudar no Moodle)
            var idsMoodleCadastrados = (await _dbContext.Cursos.Select(c => c.IdMoodle).ToListAsync()).ToHashSet();
            var cursosParaAdicionar = cursosExternos //todos os cursos novos que vão ser cadastrados
                .Where(cursoExterno => !idsMoodleCadastrados.Contains(cursoExterno.Id));

            var cursosAdicionados = new List<Curso>(); //lista pra retornar no final da função

            foreach (var curso in cursosParaAdicionar)
            {
                var cursoNSG = await _externoCursoNSG.ObterCursoAsync(curso.Nome); //busca no NSG o coordenador e o período letivo
                if (cursoNSG is null)
                    continue; //o NSG não conhece esse curso, então não temos coordenador pra ele: pula pro próximo

                // Coordenador: busca ou cria pela matrícula (enxerga também os ainda não salvos)
                var coordenadorNSG = cursoNSG.Value.Coordenador;
                var coordenador = await _dbContext.ObterOuCriarUsuarioAsync(coordenadorNSG.Matricula, coordenadorNSG.Nome);

                // Período letivo vem do NSG; a sigla recebe o nome porque nenhuma das fontes manda sigla
                var novoCurso = new Curso(curso.Nome, curso.Nome, curso.Id, cursoNSG.Value.PerLetivo, coordenador);
                _dbContext.Cursos.Add(novoCurso);

                cursosAdicionados.Add(novoCurso); //adiciona na lista de pra retornar no final da função
            }
            await _dbContext.SaveChangesAsync(); //grava no banco todos os cursos (e coordenadores novos) do loop
            return cursosAdicionados;
        }

        // Todos os cursos com os módulos. Usado pela sincronização, pela tela de Cursos e pelos filtros da página de Alunos
        public async Task<List<Curso>> ListarCursosAsync()
        {
            return await _dbContext.Cursos
                .Include(c => c.Coordenador) //sem o Include, curso.Coordenador viria null (o EF não carrega navegações sozinho)
                .Include(c => c.Modulos)
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        // Cursos com alunos: filtros e gráfico de pizza do Dashboard
        public async Task<List<Curso>> ListarCursosComAlunosAsync()
        {
            return await _dbContext.Cursos
                .Include(c => c.Alunos)
                .OrderBy(c => c.Nome)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
