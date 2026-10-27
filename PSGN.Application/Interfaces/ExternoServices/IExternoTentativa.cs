using PSGN.Application.Servicos.Tentativas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    //REVISÃO: o nome da interface (e do arquivo) estava com erro de digitação: IExternoTentaiva -> IExternoTentativa.
    public interface IExternoTentativa
    {
        //REVISÃO: antes era "ObterTodasTentativasPeloIdTentativaAsync(int IdTentativa)". O parâmetro NÃO é o id de uma
        //tentativa: o Moodle (função local_quizattempts_get_attempts) recebe o "cmid", o id do questionário "Avaliação Final
        //do Módulo" dentro do curso. No PSGN ele fica em Modulo.IdCmidMoodle. O nome antigo induzia a passar o id errado.
        public Task<List<TentativaSaidaJsonMoodleDto>> ObterTentativasDoQuestionarioAsync(int idCmidMoodle);
    }
}
