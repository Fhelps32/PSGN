using Microsoft.AspNetCore.Mvc; // Controller, IActionResult, [HttpPost], [ValidateAntiForgeryToken]
using PSGN.Application.Servicos.Cursos; // CursoService
using PSGN.Application.Servicos.Sincronizacao; // SincronizacaoService
using PSGN.Presentation.ViewModels; // HomeViewModel, CursoItem, ModuloItem, ErrorViewModel
using System.Diagnostics; // Activity, usado na action Error

namespace PSGN.Presentation.Controllers // pasta dos controllers do MVC
{
    public class HomeController : Controller // controller da página inicial
    {
        private readonly CursoService _cursoService; // usado para LER os cursos que aparecem na tela
        private readonly SincronizacaoService _sincronizacaoService; // usado pelo botão "Sincronizar"
        private readonly ILogger<HomeController> _logger; // log do ASP.NET (injetado, ainda não usado)

        public HomeController( // o ASP.NET entrega as dependências pelo construtor
            ILogger<HomeController> logger, // parâmetro do log
            CursoService cursoService, // parâmetro do serviço de cursos
            SincronizacaoService sincronizacaoService // parâmetro do serviço de sincronização
            ) // fim da lista de parâmetros
        {
            _logger = logger; // guarda o log
            _cursoService = cursoService; // guarda o serviço de cursos
            _sincronizacaoService = sincronizacaoService; // guarda o serviço de sincronização
        } // fim do construtor

        public async Task<IActionResult> Index() // action da página inicial; só LÊ do banco, não sincroniza
        {
            var cursos = await _cursoService.ListarCursosAsync(); // busca no banco os cursos já com coordenador e módulos

            var viewModel = new HomeViewModel // monta o objeto que a View (Razor) vai exibir
            { // inicializador de objeto
                MensagemSincronizacao = TempData["MensagemSincronizacao"] as string, // pega o aviso deixado pela action Sincronizar (null se não houver)
                Cursos = cursos.Select(curso => new CursoItem // converte cada Curso do domain em um CursoItem da tela
                { // inicializador do CursoItem
                    Nome = curso.Nome, // nome do curso
                    Coordenador = curso.Coordenador.Nome, // nome do coordenador
                    Modulos = curso.Modulos // parte da lista de módulos do curso
                        .OrderBy(modulo => modulo.Nome) // ordena alfabeticamente
                        .Select(modulo => new ModuloItem { Nome = modulo.Nome, IdMoodle = modulo.IdMoodle }) // converte cada Modulo em ModuloItem, mostrando o id do Moodle (não o id do nosso banco)
                        .ToList() // fecha como lista
                }).ToList() // fim do CursoItem e da lista de cursos
            }; // fim do HomeViewModel
            return View(viewModel); // entrega o viewModel à View Index
        } // fim da action Index

        [HttpPost] // só responde a POST, porque sincronizar altera dados e um GET pode ser disparado sozinho por navegador ou robô
        [ValidateAntiForgeryToken] // exige o token do formulário, impedindo que outro site dispare este POST
        public async Task<IActionResult> Sincronizar() // action chamada pelo botão "Sincronizar"
        {
            // REVISÃO: o service agora devolve List<Curso> (domain), então a mensagem usa a quantidade de cursos sincronizados
            var cursos = await _sincronizacaoService.SincronizarTudoAsync("EAD", 3); // roda cursos -> módulos -> alunos -> tentativas, usando a categoria raiz "EAD" e profundidade 3

            TempData["MensagemSincronizacao"] = $"Sincronização concluída: {cursos.Count} curso(s) sincronizado(s) (módulos, alunos e tentativas incluídos)."; // TempData sobrevive ao redirect, então o Index consegue mostrar o aviso

            return RedirectToAction(nameof(Index)); // redireciona para o Index (Post/Redirect/Get): um F5 depois não sincroniza de novo
        } // fim da action Sincronizar

        public IActionResult Privacy() // action da página de privacidade (padrão do template)
        {
            return View(); // só devolve a View
        } // fim da action Privacy

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // impede o navegador de guardar a página de erro em cache
        public IActionResult Error() // action da página de erro (padrão do template)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // envia o id da requisição para a View, para facilitar achar o erro no log
        } // fim da action Error
    } // fim da classe
} // fim do namespace
