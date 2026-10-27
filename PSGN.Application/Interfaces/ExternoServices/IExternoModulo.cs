using PSGN.Application.Servicos.Modulos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoModulo
    {
        //REVISÃO: o id da categoria agora é int (antes era string, e o ModuloService tinha que converter de um pro outro)
        public Task<List<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoriaAsync(int idCategoriaMoodle);
        //REVISÃO: o retorno agora é anulável (?). O DTO é um struct, então o antigo FirstOrDefault devolvia um struct "zerado"
        //(IdQuestionario = 0) quando o módulo não tem "Avaliação Final do Módulo", e ninguém conseguia distinguir isso de um
        //questionário real. Agora devolve null nesse caso. Também ganhou o sufixo Async, como os outros métodos.
        public Task<ModuloQuestionarioSaidaJsonMoodleDto?> ObterQuestionarioFinalPeloIdModuloAsync(int idModuloMoodle);
    }
}
