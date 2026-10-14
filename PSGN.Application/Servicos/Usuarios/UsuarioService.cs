using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Usuarios
{
    public class UsuarioService
    {
        private readonly IExternoUsuario _externoUsuario;
        private readonly IExternoCurso _extrernoCurso;
        private readonly IPSGNDbContext _dbContext;

        public UsuarioService(
            IExternoUsuario externoUsuario,
            IExternoCurso externoCurso,
            IPSGNDbContext dbContext
            )
        {
            _externoUsuario = externoUsuario;
            _extrernoCurso = externoCurso;
            _dbContext = dbContext;
        }

        public async Task<List<AlunoModulo>> CriarUsuarioAlunosPorIdCurso(int idCursoMoodle)
        {
            var curso = _dbContext.Cursos.FirstOrDefault(c => c.IdMoodle == idCursoMoodle)
                ?? throw new Exception($"Não existe nenhum curso cadastrado com o idMoodle: {idCursoMoodle}");

            var modulos = _dbContext.Modulos.Where(c => c.Curso.IdMoodle == idCursoMoodle).ToList()
                ?? throw new Exception($"Não exite nenhum módulo cadastrado para esse curso por enquanto");

            var alunosModulosCriados = new List<AlunoModulo>();

            foreach (var modulo in modulos)
            {
                var usuariosDto = await _externoUsuario.ObterUsuariosPorModuloAsync(idCursoMoodle)
                ?? throw new Exception("Não existe nenhum aluno inscrito nesse módulo");

                foreach (var usuario in usuariosDto)
                {
                    if (!_dbContext.Usuarios.Any(u => u.Matricula == usuario.Matricula))
                    {
                        var usuarioNovo = new Usuario(usuario.Matricula, usuario.Nome);
                        _dbContext.Usuarios.Add(usuarioNovo);

                        var alunoNovo = new Aluno(usuarioNovo, curso, true); //ajeitar esse egresso aq
                        _dbContext.Alunos.Add(alunoNovo);

                        var alunoModuloNovo = new AlunoModulo(alunoNovo, modulo);
                        _dbContext.AlunosModulos.Add(alunoModuloNovo);

                        alunosModulosCriados.Add(alunoModuloNovo);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return alunosModulosCriados;
        }
    }
}
