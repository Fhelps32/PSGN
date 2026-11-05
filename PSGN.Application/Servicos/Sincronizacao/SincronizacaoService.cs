using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Modulos;
using PSGN.Application.Servicos.Tentativas;
using PSGN.Application.Servicos.Usuarios;
using PSGN.Domain;

namespace PSGN.Application.Servicos.Sincronizacao
{
    public class SincronizacaoService
    {
        private readonly CursoService _cursoService;
        private readonly ModuloService _moduloService;
        private readonly UsuarioService _usuarioService;
        private readonly TentativaService _tentativaService;

        public SincronizacaoService(
            CursoService cursoService,
            ModuloService moduloService,
            UsuarioService usuarioService,
            TentativaService tentativaService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _usuarioService = usuarioService;
            _tentativaService = tentativaService;
        }

        // Sincroniza cursos, módulos, alunos e tentativas com o Moodle e o NSG. Chamado pelo botão da tela de Cursos (HomeController)
        public async Task<List<Curso>> SincronizarTudoAsync(string idNumberCategoriaRaiz, int profundidadeCursos)
        {
            await _cursoService.SincronizarCursosAsync(idNumberCategoriaRaiz, profundidadeCursos);

            var cursos = await _cursoService.ListarCursosAsync();

            foreach (var curso in cursos)
            {
                await _moduloService.SincronizarModulosDoCursoAsync(curso);
                await _usuarioService.SincronizarAlunosDoCursoAsync(curso);
                await _tentativaService.SincronizarTentativasDoCursoAsync(curso);
            }

            return cursos;
        }
    }
}
