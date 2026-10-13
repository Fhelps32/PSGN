using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Modulos
{
    public class ModuloService
    {
        private readonly IExternoModulo _externoModulo;
        private readonly IExternoCurso _externoCurso;
        private readonly IPSGNDbContext _dbContext;

        public ModuloService(IExternoModulo externoModulo, IExternoCurso externoCurso, IPSGNDbContext pSGNDbContext)
        {
            _externoModulo = externoModulo;
            _dbContext = pSGNDbContext;
            _externoCurso = externoCurso;
        }

        public async Task<ICollection<Modulo>> SincronizarModulosPorCursoAsync(string idCategoriaMoodle)
        {
            var modulosMoodle = await _externoModulo.ObterModulosPeloIdCategoria(idCategoriaMoodle); 
            var modulosExistente = _dbContext.Modulos
                .Where(m => m.Curso.IdMoodle.ToString() == idCategoriaMoodle)
                .ToList();
            var modulosAdicionados = new List<Modulo>();

            foreach(var modulo in modulosMoodle)
            {
                if (!modulosExistente.Any(m => m.IdMoodle == modulo.IdMoodle))
                {
                    var cursoModuloDto = await _externoCurso.ObterCursoPeloIdMoodle(modulo.IdCursoMoodle);
                    var curso = _dbContext.Cursos.FirstOrDefault(c => c.IdMoodle == cursoModuloDto.Id)
                        ?? throw new Exception("Não existe um curso cadastrado para esse módulo");
                    var novoModulo = new Modulo(modulo.Nome, 67, modulo.IdMoodle, curso); //mudar essa parada de numero do curso aí kdlsklssdlk

                    modulosAdicionados.Add(novoModulo);
                    _dbContext.Modulos.Add(novoModulo);
                }
            }

            await _dbContext.SaveChangesAsync();
            return modulosAdicionados;
        }
    }
}
