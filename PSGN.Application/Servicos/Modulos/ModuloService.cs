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

        //REVISÃO: tirei o IExternoCurso daqui. Ele servia para perguntar ao Moodle, módulo por módulo, "de qual categoria
        //você é?". Mas a resposta já é conhecida: os módulos são buscados justamente PELA categoria do curso. Eram 9 chamadas
        //HTTP a mais por curso sem necessidade. (Também saiu o "using System.Security.Cryptography", que não era usado.)
        public ModuloService(IExternoModulo externoModulo, IPSGNDbContext dbContext)
        {
            _externoModulo = externoModulo;
            _dbContext = dbContext;
        }

        //REVISÃO: recebe o Curso (entidade) em vez de uma string com o id da categoria. Assim não precisa converter
        //int -> string -> int, nem buscar o curso de novo no banco para cada módulo (o antigo "?? throw" não é mais necessário).
        public async Task<List<Modulo>> SincronizarModulosDoCursoAsync(Curso curso)
        {
            var modulosMoodle = await _externoModulo.ObterModulosPeloIdCategoriaAsync(curso.IdMoodle);

            //REVISÃO: antes era "m.Curso.IdMoodle.ToString() == idCategoriaMoodle", que gera JOIN + conversão para texto no SQL,
            //e trazia os módulos inteiros. Agora busca só os IdMoodle que já existem. A busca é por TODOS os módulos (não só os
            //deste curso) porque, se um módulo for movido de categoria no Moodle, ele não pode ser cadastrado duas vezes.
            var idsMoodle = modulosMoodle.Select(m => m.IdMoodle).ToList();
            var idsMoodleCadastrados = (await _dbContext.Modulos
                .Where(m => idsMoodle.Contains(m.IdMoodle))
                .Select(m => m.IdMoodle)
                .ToListAsync()).ToHashSet();

            var modulosAdicionados = new List<Modulo>();

            foreach (var modulo in modulosMoodle.Where(m => !idsMoodleCadastrados.Contains(m.IdMoodle)))
            {
                //REVISÃO: o número do módulo continua fixo em 67 (seu TODO). O Moodle não informa a ordem do módulo no
                //curso de forma confiável; quando o NSG simulado tiver esse dado, é de lá que ele deve vir.

                var novoModulo = new Modulo(modulo.Nome, 67, modulo.IdMoodle, curso);
                var questionario = await _externoModulo.ObterQuestionarioFinalPeloIdModulo(modulo.IdMoodle);
                novoModulo.IdCmidMoodle = questionario.IdQuestionario; //bota o id do questionário junto
                modulosAdicionados.Add(novoModulo);
                _dbContext.Modulos.Add(novoModulo);
            }

            await _dbContext.SaveChangesAsync();
            return modulosAdicionados;
        }
    }
}
