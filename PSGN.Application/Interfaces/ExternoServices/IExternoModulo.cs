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
        // Módulos ("courses" do Moodle) de uma categoria. Usado pelo ModuloService
        public Task<List<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoriaAsync(int idCategoriaMoodle);
        // Questionário "Avaliação Final do Módulo" (null se o módulo não tiver). Usado pelo ModuloService para o IdCmidMoodle
        public Task<ModuloQuestionarioSaidaJsonMoodleDto?> ObterQuestionarioFinalPeloIdModuloAsync(int idModuloMoodle);
    }
}
