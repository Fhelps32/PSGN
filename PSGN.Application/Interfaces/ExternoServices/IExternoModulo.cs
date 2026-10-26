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
        public Task<ModuloQuestionarioSaidaJsonMoodleDto> ObterQuestionarioFinalPeloIdModulo(int idModuloMoodle);
    }
}
