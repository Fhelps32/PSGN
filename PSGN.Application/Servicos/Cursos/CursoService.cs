using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Cursos
{
    public class CursoService
    {
        private readonly IExternoCurso _externoCurso;
        private readonly IPSGNDbContext _dbContext;

        public CursoService(IExternoCurso externoCurso, IPSGNDbContext dbContext)
        {
            _externoCurso = externoCurso;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Curso>> SincronizarCursosAsync()
        {
            var cursosExternos = await _externoCurso.ObterCursosPeloIdNumberAsync("EAD", 3);
            var cursosInternos = _dbContext.Cursos.ToList();
            var cursosParaAdicionar = cursosExternos
                .Where(cursoExterno => !cursosInternos.Any(cursoInterno => cursoInterno.IdMoodle == cursoExterno.Id))
                .Select(cursoExterno => new Curso(cursoExterno.Nome, string.Empty, null) { IdCurso = cursoExterno.Id })
                .ToList();
            if (cursosParaAdicionar.Any())
            {
                _dbContext.Cursos.AddRange(cursosParaAdicionar);
                await _dbContext.SaveChangesAsync();
            }
            return cursosParaAdicionar;
        }
    }
}
