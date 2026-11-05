using PSGN.Application.Servicos.Cursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoCursoNSG
    {
        // Curso do NSG (coordenador e período letivo); null se o NSG não o conhece. Usado pelo CursoService
        public Task<CursoSaidaJsonNSGDto?> ObterCursoAsync(string nomeCurso);
    }
}
