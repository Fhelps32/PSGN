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
        private readonly IExternoCursoNSG _externoCursoNSG; 

        public CursoService(IExternoCurso externoCurso, IPSGNDbContext dbContext, IExternoCursoNSG externoCursoNSG)
        {
            _externoCurso = externoCurso;
            _dbContext = dbContext;
            _externoCursoNSG = externoCursoNSG;
        }

        public async Task<IEnumerable<Curso>> SincronizarCursosAsync(string idNumber, int profundidade)
        {
            var cursosExternos = await _externoCurso.ObterCursosPeloIdNumberAsync(idNumber, profundidade);
            var cursosInternos = _dbContext.Cursos.ToList();
            var cursosParaAdicionar = cursosExternos
                .Where(cursoExterno => !cursosInternos.Any(cursoInterno => cursoInterno.Nome == cursoExterno.Nome));

            var cursosAdicionados = new List<Curso>();

            foreach (var curso in cursosParaAdicionar)
            {
                var coordenadorDto = await _externoCursoNSG.ObterCoordenadorDoCursoAsync(curso.Nome);
                if (_dbContext.Usuarios.Any(u => u.Matricula == coordenadorDto.Matricula))
                {
                    var usuarioCoordenador = _dbContext.Usuarios.FirstOrDefault(u => u.Matricula == coordenadorDto.Matricula);
                    var novoCurso = new Curso(curso.Nome, curso.Nome, usuarioCoordenador!);
                    _dbContext.Cursos.Add(novoCurso);

                    cursosAdicionados.Add(novoCurso);
                }
                else
                {
                    var novoUsuario = new Usuario(coordenadorDto.Matricula, coordenadorDto.Nome);
                    _dbContext.Usuarios.Add(novoUsuario);
                    await _dbContext.SaveChangesAsync();
                    var novoCurso = new Curso(curso.Nome, curso.Nome, novoUsuario!);
                    _dbContext.Cursos.Add(novoCurso);

                    cursosAdicionados.Add(novoCurso);
                }
            }
            return cursosAdicionados;
        }
    }
}
