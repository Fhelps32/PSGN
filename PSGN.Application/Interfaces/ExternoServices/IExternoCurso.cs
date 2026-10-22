using PSGN.Application.Servicos.Cursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoCurso
    {
        public Task<IEnumerable<CursoSaidaJsonMoodleCategoriasDto>> ObterCursosPeloIdNumberAsync(string idNumber, int depth);

        //REVISÃO: agora devolve null se o id não existir no Moodle (antes estourava ArgumentOutOfRangeException) e ganhou o
        //sufixo Async, como os outros métodos assíncronos. O ModuloService não usa mais esse método; mantive porque pode ser útil.
        public Task<CursoSaidaJsonMoodleCategoriasDto?> ObterCursoPeloIdMoodleAsync(int idCurso);
    }
}
