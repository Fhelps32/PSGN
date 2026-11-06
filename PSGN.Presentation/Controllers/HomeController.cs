using Microsoft.AspNetCore.Mvc;
using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Historico;
using PSGN.Application.Servicos.Sincronizacao;
using PSGN.Presentation.ViewModels;
using System.Diagnostics;

namespace PSGN.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly CursoService _cursoService;
        private readonly SincronizacaoService _sincronizacaoService;
        private readonly ImportacaoHistoricoService _importacaoHistoricoService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger,
            CursoService cursoService,
            SincronizacaoService sincronizacaoService,
            ImportacaoHistoricoService importacaoHistoricoService
            )
        {
            _logger = logger;
            _cursoService = cursoService;
            _sincronizacaoService = sincronizacaoService;
            _importacaoHistoricoService = importacaoHistoricoService;
        }

        // Tela de Cursos: cursos e módulos sincronizados
        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ListarCursosAsync();

            var viewModel = new HomeViewModel
            {
                MensagemSincronizacao = TempData["MensagemSincronizacao"] as string,
                ErroSincronizacao = TempData["ErroSincronizacao"] as string,
                Cursos = cursos.Select(curso => new CursoItem
                {
                    Nome = curso.Nome,
                    Coordenador = curso.Coordenador.Nome,
                    Modulos = curso.Modulos
                        .OrderBy(modulo => modulo.Nome)
                        .Select(modulo => new ModulosItem { Nome = modulo.Nome, IdMoodle = modulo.IdMoodle })
                        .ToList()
                }).ToList()
            };
            return View(viewModel);
        }

        // Botão "Sincronizar com Moodle e NSG" da tela de Cursos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sincronizar()
        {
            // Falhas de rede (Moodle/NSG fora do ar, certificado, tempo esgotado) viram aviso na tela e vão para o log
            try
            {
                var cursos = await _sincronizacaoService.SincronizarTudoAsync("EAD", 3);

                TempData["MensagemSincronizacao"] = $"Sincronização concluída: {cursos.Count} curso(s) sincronizado(s) (módulos, alunos e tentativas incluídos).";
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Falha de conexão ao sincronizar com o Moodle/NSG.");
                var detalhe = ex.InnerException?.Message ?? ex.Message;
                TempData["ErroSincronizacao"] = $"Não foi possível conectar ao Moodle ou ao NSG. Tente novamente em alguns minutos. Detalhe técnico: {detalhe}";
            }

            return RedirectToAction(nameof(Index));
        }


        // Botão "Importar histórico" da tela de Cursos: cria alunos, inscrições e tentativas a partir das provas que já existem no Moodle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarHistorico()
        {
            try
            {
                var (importadas, semNome) = await _importacaoHistoricoService.ImportarHistoricoAsync();

                if (importadas.Count == 0)
                {
                    TempData["MensagemSincronizacao"] = "Nenhuma tentativa nova para importar: o histórico já estava importado ou os módulos não têm provas.";
                }
                else
                {
                    var alunos = importadas.Select(t => t.AlunoModulo.IdAluno).Distinct().Count();
                    var inscricoes = importadas.Select(t => t.AlunoModulo).Distinct().Count();
                    TempData["MensagemSincronizacao"] = $"Histórico importado: {importadas.Count} tentativa(s) de {alunos} aluno(s) em {inscricoes} inscrição(ões) de módulo.";
                }

                if (semNome.Count > 0)
                    TempData["ErroSincronizacao"] = $"{semNome.Count} aluno(s) ficaram com a matrícula no lugar do nome: o Moodle não liberou a consulta de usuários (core_user_get_users_by_field) para o token ou não encontrou o usuário. Libere a função e importe de novo para corrigir os nomes.";
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Falha de conexão ao importar o histórico do Moodle.");
                var detalhe = ex.InnerException?.Message ?? ex.Message;
                TempData["ErroSincronizacao"] = $"Não foi possível conectar ao Moodle. Tente novamente em alguns minutos. Detalhe técnico: {detalhe}";
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
