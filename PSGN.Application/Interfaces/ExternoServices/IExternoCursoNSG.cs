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
        //REVISÃO: antes era ObterCoordenadorDoCursoAsync e devolvia só o coordenador, jogando fora o perLetivo que o NSG
        //também manda. Agora devolve o curso do NSG inteiro (coordenador + período letivo). null = o NSG não conhece o curso.
        public Task<CursoSaidaJsonNSGDto?> ObterCursoAsync(string nomeCurso);
    }
}
