using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using PSGN.Application.Servicos.Alunos;
using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Tentativas;
using PSGN.Domain;
using PSGN.Presentation.ViewModels;

namespace PSGN.Presentation.Controllers
{
    // Página de Alunos: lista paginada, aluno selecionado (módulo atual, módulos e tentativas) e ações do administrador.
    // Os services devolvem entidades do domínio; aqui elas viram ViewModel
    public class AlunosController : Controller
    {
        private const int AlunosPorPagina = 25;

        private readonly AlunoService _alunoService;
        private readonly CursoService _cursoService;
        private readonly TentativaService _tentativaService;

        public AlunosController(AlunoService alunoService, CursoService cursoService, TentativaService tentativaService)
        {
            _alunoService = alunoService;
            _cursoService = cursoService;
            _tentativaService = tentativaService;
        }

        // GET /Alunos?Nome=&CursoId=&ModuloId=&Pagina=&AlunoId=
        public async Task<IActionResult> Index(string? nome, int? cursoId, int? moduloId, int pagina = 1, int? alunoId = null)
        {
            var resultado = await _alunoService.ListarAlunosAsync(nome, cursoId, moduloId, "nome", false, pagina, AlunosPorPagina);
            var cursos = await _cursoService.ListarCursosAsync();

            // Aluno do painel: o pedido, se estiver nesta página; senão o primeiro da página
            var idSelecionado = resultado.Items.Any(a => a.IdAluno == alunoId) ? alunoId : resultado.Items.FirstOrDefault()?.IdAluno;
            var selecionado = idSelecionado.HasValue ? await _alunoService.ObterAlunoComHistoricoAsync(idSelecionado.Value) : null;

            var viewModel = new AlunosViewModel
            {
                Nome = nome,
                CursoId = cursoId,
                ModuloId = moduloId,
                TotalAlunos = resultado.TotalItems,
                Paginacao = new PaginacaoViewModel
                {
                    PaginaAtual = resultado.PaginaAtual,
                    TotalPaginas = resultado.TotalPaginas,
                    TotalItens = resultado.TotalItems,
                    ItensPorPagina = resultado.TamanhoPagina,
                    Rotulo = "alunos",
                    Rota = RotaDosFiltros(nome, cursoId, moduloId)
                },
                Cursos = cursos
                    .OrderBy(c => c.Nome)
                    .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome })
                    .ToList(),
                Modulos = ModulosDoCurso(cursos, cursoId)
                    .Select(m => new SelectListItem { Value = m.IdModulo.ToString(), Text = m.Nome })
                    .ToList(),
                Alunos = resultado.Items.Select(aluno => new AlunosAlunoItem
                {
                    IdAluno = aluno.IdAluno,
                    Matricula = aluno.Usuario.Matricula,
                    Nome = aluno.Usuario.Nome,
                    Curso = aluno.Curso.Nome,
                    ModuloAtual = aluno.ObterModuloAtual()?.Modulo.Nome ?? string.Empty,
                    Selecionado = aluno.IdAluno == idSelecionado
                }).ToList(),
                Selecionado = selecionado is null ? null : MontarDetalhe(selecionado)
            };

            ViewData["MensagemAluno"] = TempData["MensagemAluno"] as string;
            ViewData["ErroAluno"] = TempData["ErroAluno"] as string;
            return View(viewModel);
        }

        // Ações sobre as tentativas (formulários do painel da direita). Recebem os filtros da tela para voltar ao mesmo lugar

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> Aprovar(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1)
            => ExecutarAsync(() => _tentativaService.AprovarAsync(idTentativa), "Tentativa aprovada. O módulo foi concluído; o próximo módulo entra pela sincronização com o Moodle.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> Reprovar(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1)
            => ExecutarAsync(() => _tentativaService.ReprovarEAbrirNovaAsync(idTentativa), "Tentativa reprovada. Uma nova tentativa foi aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> ReprovarPorInatividade(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1)
            => ExecutarAsync(() => _tentativaService.ReprovarPorInatividadeEAbrirNovaAsync(idTentativa), "Tentativa reprovada por inatividade. Uma nova tentativa foi aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> AbrirTentativa(int idAlunoModulo, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1)
            => ExecutarAsync(() => _tentativaService.AbrirNovaAsync(idAlunoModulo), "Nova tentativa aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        // Executa a ação, guarda o aviso (sucesso ou a regra que impediu) em TempData e volta para a mesma tela (Post/Redirect/Get)
        private async Task<IActionResult> ExecutarAsync(Func<Task<Tentativa>> acao, string mensagemSucesso, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina)
        {
            try
            {
                await acao();
                TempData["MensagemAluno"] = mensagemSucesso;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErroAluno"] = ex.Message;
            }

            var rota = RotaDosFiltros(nome, cursoId, moduloId);
            rota["Pagina"] = pagina.ToString();
            rota["AlunoId"] = alunoId.ToString();
            return RedirectToAction(nameof(Index), new RouteValueDictionary(rota.ToDictionary(k => k.Key, v => (object?)v.Value)));
        }

        // Opções do select de Módulo quando o Curso muda (cascata do alunos.js)
        [HttpGet]
        public async Task<IActionResult> GetModulosPorCurso(int? cursoId)
        {
            var cursos = await _cursoService.ListarCursosAsync();
            return Json(ModulosDoCurso(cursos, cursoId).Select(m => new { value = m.IdModulo, text = m.Nome }));
        }

        // Entidades do domínio -> ViewModel do painel da direita

        private static AlunosDetalheItem MontarDetalhe(Aluno aluno)
        {
            var atual = aluno.ObterModuloAtual();
            var historico = aluno.Modulos
                .OrderByDescending(am => am.DataInscricao)
                .ThenByDescending(am => am.IdAlunoModulo)
                .Select(am => MontarModulo(am, am == atual))
                .ToList();

            return new AlunosDetalheItem
            {
                IdAluno = aluno.IdAluno,
                Matricula = aluno.Usuario.Matricula,
                Nome = aluno.Usuario.Nome,
                Curso = aluno.Curso.Nome,
                ModulosConcluidos = aluno.Modulos.Count(am => am.Aprovado),
                TotalModulosCurso = aluno.Curso.Modulos.Count,
                ModuloAtual = historico.FirstOrDefault(m => m.Atual),
                Historico = historico
            };
        }

        private static AlunosModuloItem MontarModulo(AlunoModulo inscricao, bool atual)
        {
            var hoje = DateTime.Today;
            return new AlunosModuloItem
            {
                IdAlunoModulo = inscricao.IdAlunoModulo,
                Nome = inscricao.Modulo.Nome,
                DataInscricao = inscricao.DataInscricao,
                DataFim = inscricao.DataConclusao,
                NotaFinal = inscricao.ObterNotaFinal(),
                Aprovado = inscricao.Aprovado,
                Atual = atual,
                PodeAbrirTentativa = inscricao.PodeAbrirTentativa,
                TentativasReprovadas = inscricao.Tentativas.Count(t => t.Situacao is Domain.Enum.EnumSituacaoTentativa.Reprovada or Domain.Enum.EnumSituacaoTentativa.ReprovadaPorInatividade),
                Tentativas = inscricao.TentativasEmOrdem.Select((t, i) => new AlunosTentativaItem
                {
                    IdTentativa = t.IdTentativa,
                    Numero = i + 1,
                    DataInicio = t.DataInicioTentativa,
                    DataLimite = t.DataLimite,
                    DataFim = t.DataFimTentativa,
                    Aberta = t.Aberta,
                    PrazoVencido = t.PrazoVencido(DateTime.Now),
                    DiasParaOPrazo = (t.DataLimite.Date - hoje).Days,
                    DataProva = t.DataFimAttempt ?? t.DataInicioAttempt,
                    Nota = t.Nota,
                    Situacao = t.Situacao,
                    PodeAprovar = t.PodeAprovar,
                    PodeReprovar = t.PodeReprovar,
                    PodeReprovarPorInatividade = t.PodeReprovarPorInatividade
                }).ToList()
            };
        }

        // Filtros ativos como rota, para os links e redirecionamentos
        private static Dictionary<string, string> RotaDosFiltros(string? nome, int? cursoId, int? moduloId)
        {
            var rota = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(nome)) rota["Nome"] = nome;
            if (cursoId.HasValue) rota["CursoId"] = cursoId.Value.ToString();
            if (moduloId.HasValue) rota["ModuloId"] = moduloId.Value.ToString();
            return rota;
        }

        // Módulos de um curso em ordem alfabética (vazio sem curso). Usado no Index e no GetModulosPorCurso
        private static IEnumerable<Modulo> ModulosDoCurso(IEnumerable<Curso> cursos, int? cursoId)
        {
            return cursos
                .Where(c => c.IdCurso == cursoId)
                .SelectMany(c => c.Modulos)
                .OrderBy(m => m.Nome);
        }
    }
}
