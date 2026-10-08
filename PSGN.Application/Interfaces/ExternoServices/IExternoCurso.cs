using PSGN;
using PSGN.Application.Servicos.Cursos;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoCurso
    {
        public Task<IEnumerable<CursoSaidaJsonDto>> ObterCursosPeloIdNumberAsync(string idNumber, int depth);
    }
}
