using Microsoft.AspNetCore.Mvc;
using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Sincronizacao;
using PSGN.Presentation.ViewModels;
using System.Diagnostics;

namespace PSGN.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly CursoService _cursoService;
        private readonly SincronizacaoService _sincronizacaoService;
        private readonly ILogger<HomeController> _logger;

        //REVISÃO: o controller não recebe mais ModuloService e UsuarioService. Quem coordena a sincronização agora é o
        //SincronizacaoService (camada Application).
        public HomeController(
            ILogger<HomeController> logger,
            CursoService cursoService,
            SincronizacaoService sincronizacaoService
            )
        {
            _logger = logger;
            _cursoService = cursoService;
            _sincronizacaoService = sincronizacaoService;
        }

        //REVISÃO: o Index agora só LÊ do banco e mostra os cursos com seus módulos. Antes ele sincronizava tudo a cada
        //abertura da página: cada F5 fazia dezenas de chamadas ao Moodle, e a tela só mostrava o que tinha sido criado
        //naquela execução (por isso, depois da primeira vez, a lista vinha vazia).
        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ListarCursosAsync();

            var viewModel = new HomeViewModel
            {
                MensagemSincronizacao = TempData["MensagemSincronizacao"] as string,
                Cursos = cursos.Select(curso => new CursoItem
                {
                    Nome = curso.Nome,
                    Coordenador = curso.Coordenador.Nome,
                    Modulos = curso.Modulos
                        .OrderBy(modulo => modulo.Nome)
                        //REVISÃO: antes o campo "Id no Moodle" recebia modulo.IdModulo, que é o id do NOSSO banco
                        .Select(modulo => new ModuloItem { Nome = modulo.Nome, IdMoodle = modulo.IdMoodle })
                        .ToList()
                }).ToList()
            };
            return View(viewModel);
        }

        //REVISÃO: action nova, chamada pelo botão "Sincronizar". É POST porque altera dados: navegadores, pré-carregamento
        //de links e robôs podem disparar GETs sozinhos. O [ValidateAntiForgeryToken] impede que outro site dispare o POST.
        //Depois de sincronizar, redireciona para o Index (padrão Post/Redirect/Get), então um F5 não sincroniza de novo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sincronizar()
        {
            var resultado = await _sincronizacaoService.SincronizarTudoAsync("EAD", 3);

            //REVISÃO: TempData sobrevive a um redirect, então o Index consegue mostrar o resumo
            TempData["MensagemSincronizacao"] =
                $"Sincronização concluída: {resultado.CursosAdicionados} curso(s), " +
                $"{resultado.ModulosAdicionados} módulo(s) e {resultado.InscricoesAdicionadas} inscrição(ões) de aluno novos.";

            return RedirectToAction(nameof(Index));
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
