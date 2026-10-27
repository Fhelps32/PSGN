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

            //REVISÃO: preenche o IdCmidMoodle dos módulos que JÁ estão no banco e ainda não têm esse valor (0). O cmid só era
            //preenchido na criação do módulo; todos os módulos cadastrados antes disso ficariam para sempre com 0, e como a
            //sincronização de tentativas depende do cmid, eles nunca teriam tentativas. Esta consulta roda ANTES de adicionar
            //os módulos novos abaixo, então traz apenas módulos antigos (os novos já nascem com o cmid).
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
                //REVISÃO: o número do módulo continua fixo em 67 (seu TODO). O Moodle não informa a ordem do módulo no
                //curso de forma confiável; quando o NSG simulado tiver esse dado, é de lá que ele deve vir.

                var novoModulo = new Modulo(modulo.Nome, 67, modulo.IdMoodle, curso);

                //REVISÃO: o método agora devolve null se o módulo não tem "Avaliação Final do Módulo". Nesse caso o IdCmidMoodle
                //fica em 0 (o valor padrão), e o módulo é simplesmente pulado na sincronização de tentativas. Antes um struct
                //zerado dava o mesmo 0, mas sem ninguém conseguir saber se foi "não achou" ou um valor de verdade.
                var questionario = await _externoModulo.ObterQuestionarioFinalPeloIdModuloAsync(modulo.IdMoodle);
                if (questionario is not null)
                    novoModulo.IdCmidMoodle = questionario.Value.IdQuestionario; //bota o id do questionário junto
                modulosAdicionados.Add(novoModulo);
                _dbContext.Modulos.Add(novoModulo);
            }

            await _dbContext.SaveChangesAsync();
            return modulosAdicionados;
        }
    }
}
