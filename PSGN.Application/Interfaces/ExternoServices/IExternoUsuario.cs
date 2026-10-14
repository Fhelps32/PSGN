using PSGN.Application.Servicos.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoUsuario
    {
        public Task<IEnumerable<UsuarioSaidaJsonMoodleDto>> ObterUsuariosPorModuloAsync(int idModuloMoodle);
    }
}
