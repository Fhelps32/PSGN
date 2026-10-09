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
        public Task<CoordenadorCursoSaidaJsonDto> ObterCoordenadorDoCursoAsync(string nomeCurso);
    }
}
