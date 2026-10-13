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
        public Task<ICollection<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoria(string idCategoriaMoodle);
    }
}
