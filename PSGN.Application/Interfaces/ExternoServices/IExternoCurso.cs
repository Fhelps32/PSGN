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

        // Categoria do Moodle pelo id (null se não existir)
        public Task<CursoSaidaJsonMoodleCategoriasDto?> ObterCursoPeloIdMoodleAsync(int idCurso);
    }
}
