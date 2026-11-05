using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Modulos
{
    public class ModuloService
    {
        private readonly IExternoModulo _externoModulo;
        private readonly IPSGNDbContext _dbContext;

        public ModuloService(IExternoModulo externoModulo, IPSGNDbContext dbContext)
        {
            _externoModulo = externoModulo;
            _dbContext = dbContext;
        }

        // Cadastra os módulos novos da categoria do curso e completa o IdCmidMoodle dos antigos. Usado pelo SincronizacaoService
        public async Task<List<Modulo>> SincronizarModulosDoCursoAsync(Curso curso)
        {
            var modulosMoodle = await _externoModulo.ObterModulosPeloIdCategoriaAsync(curso.IdMoodle);

            // Compara com TODOS os módulos (não só os do curso): um módulo movido de categoria não é cadastrado duas vezes
            var idsMoodle = modulosMoodle.Select(m => m.IdMoodle).ToList();
            var idsMoodleCadastrados = (await _dbContext.Modulos
                .Where(m => idsMoodle.Contains(m.IdMoodle))
                .Select(m => m.IdMoodle)
                .ToListAsync()).ToHashSet();

            // Módulos já cadastrados sem cmid (0) recebem o do questionário final; sem ele não há sincronização de tentativas
            var modulosSemCmid = await _dbContext.Modulos
                .Where(m => m.IdCurso == curso.IdCurso && m.IdCmidMoodle == 0)
                .ToListAsync();
            foreach (var moduloSemCmid in modulosSemCmid)
            {
                var questionarioExistente = await _externoModulo.ObterQuestionarioFinalPeloIdModuloAsync(moduloSemCmid.IdMoodle);
                if (questionarioExistente is not null)
                    moduloSemCmid.IdCmidMoodle = questionarioExistente.Value.IdQuestionario; //o EF detecta a mudança e grava no SaveChanges abaixo
            }

            var modulosAdicionados = new List<Modulo>();

            foreach (var modulo in modulosMoodle.Where(m => !idsMoodleCadastrados.Contains(m.IdMoodle)))
            {
                // Número do módulo ainda fixo (TODO): a ordem deve vir do NSG quando existir

                var novoModulo = new Modulo(modulo.Nome, 67, modulo.IdMoodle, curso);

                // Sem "Avaliação Final do Módulo" o IdCmidMoodle fica 0 e o módulo é pulado na sincronização de tentativas
                var questionario = await _externoModulo.ObterQuestionarioFinalPeloIdModuloAsync(modulo.IdMoodle);
                if (questionario is not null)
                    novoModulo.IdCmidMoodle = questionario.Value.IdQuestionario; //bota o id do questionário junto
                modulosAdicionados.Add(novoModulo);
                _dbContext.Modulos.Add(novoModulo);
            }

            await _dbContext.SaveChangesAsync();
            return modulosAdicionados;
        }

        // Módulos de um curso com os alunos: fatias do gráfico de pizza do Dashboard quando um curso é escolhido
        public async Task<List<Modulo>> ListarModulosDoCursoComAlunosAsync(int idCurso)
        {
            return await _dbContext.Modulos
                .Include(m => m.Curso)
                .Include(m => m.Alunos)
                .Where(m => m.IdCurso == idCurso)
                .OrderBy(m => m.Nome)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
