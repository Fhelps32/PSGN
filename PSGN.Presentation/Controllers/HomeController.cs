using Microsoft.AspNetCore.Mvc;
using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Modulos;
using PSGN.Application.Servicos.Usuarios;
using PSGN.Domain;
using PSGN.Presentation.ViewModels;
using System.Diagnostics;

namespace PSGN.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly CursoService _cursoService;
        private readonly ModuloService _moduloService;
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger, 
            CursoService cursoService, 
            ModuloService moduloService,
            UsuarioService usuarioService
            )
        {
            _logger = logger;
            _cursoService = cursoService;
            _moduloService = moduloService;
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            var cursosAdicionados = await _cursoService.SincronizarCursosAsync("EAD", 3);
            var viewModel = new HomeViewModel() { };
            foreach (var curso in cursosAdicionados)
            {
                var cursoAdicionado = new CursoItem { Nome = curso.Nome, Coordenador = curso.Coordenador.Nome };
                var modulosAdicionados = await _moduloService.SincronizarModulosPorCursoAsync(curso.IdMoodle.ToString());
                var alunoModulosAdicionados = await _usuarioService.CriarUsuarioAlunosPorIdCurso(curso.IdMoodle);

                foreach (var modulo in modulosAdicionados)
                {
                    cursoAdicionado.Modulos.Add(new ModuloItem { Nome = modulo.Nome, IdMoodle = modulo.IdModulo.ToString()});
                }

                viewModel.Cursos.Add(cursoAdicionado);
            }
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
