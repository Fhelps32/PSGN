using Microsoft.AspNetCore.Mvc; // Controller, IActionResult, [HttpGet], [HttpPost], Json
using Microsoft.AspNetCore.Mvc.Rendering; // SelectListItem, usado nos <select> dos filtros
using Microsoft.AspNetCore.Routing; // REVISÃO: RouteValueDictionary, para voltar à tela com os mesmos filtros
using PSGN.Application.Servicos.Alunos; // AlunoService
using PSGN.Application.Servicos.Cursos; // CursoService
using PSGN.Application.Servicos.Tentativas; // REVISÃO: TentativaService, que executa as ações do administrador
using PSGN.Domain; // Aluno, AlunoModulo, Tentativa, Curso, Modulo
using PSGN.Presentation.ViewModels; // AlunosViewModel e os itens da tela

namespace PSGN.Presentation.Controllers // pasta dos controllers do MVC
{
    // REVISÃO: controller refeito para o layout mestre-detalhe e para as ações do administrador sobre as tentativas.
    // Segue a arquitetura do PSGN: os services devolvem entidades do domain, as regras ficam no domain, e é AQUI que as entidades viram ViewModel
    public class AlunosController : Controller // controller da tela de Alunos
    {
        private const int AlunosPorPagina = 25; // tamanho da página da lista

        private readonly AlunoService _alunoService; // lista os alunos e carrega o histórico do selecionado
        private readonly CursoService _cursoService; // fornece os cursos (e os módulos deles) para os selects dos filtros
        private readonly TentativaService _tentativaService; // REVISÃO: aprovar, reprovar e abrir tentativas

        public AlunosController(AlunoService alunoService, CursoService cursoService, TentativaService tentativaService) // o ASP.NET entrega os services pelo construtor (registrados em DependencyInjection.cs)
        {
            _alunoService = alunoService; // guarda para usar nas actions
            _cursoService = cursoService; // guarda para usar nas actions
            _tentativaService = tentativaService; // guarda para usar nas actions
        } // fim do construtor

        // Os parâmetros vêm da query string (GET): /Alunos?Nome=ana&CursoId=3&ModuloId=12&Pagina=2&AlunoId=40
        public async Task<IActionResult> Index(string? nome, int? cursoId, int? moduloId, int pagina = 1, int? alunoId = null) // sem alunoId, mostra o primeiro aluno da página
        {
            var resultado = await _alunoService.ListarAlunosAsync(nome, cursoId, moduloId, "nome", false, pagina, AlunosPorPagina); // entidades do domain (uma página), filtradas e em ordem alfabética
            var cursos = await _cursoService.ListarCursosAsync(); // cursos com os módulos carregados (os mesmos que a Home usa)

            // Aluno mostrado à direita: o pedido na URL, se estiver nesta página; senão o primeiro da página (senão o painel mostraria alguém que o filtro deixou de fora)
            var idSelecionado = resultado.Items.Any(a => a.IdAluno == alunoId) ? alunoId : resultado.Items.FirstOrDefault()?.IdAluno;
            var selecionado = idSelecionado.HasValue ? await _alunoService.ObterAlunoComHistoricoAsync(idSelecionado.Value) : null; // histórico completo só deste aluno

            var viewModel = new AlunosViewModel // objeto que a View vai exibir
            { // inicializador do ViewModel
                Nome = nome, // devolve o texto digitado para o campo continuar preenchido
                CursoId = cursoId, // devolve o curso escolhido para o select continuar marcado
                ModuloId = moduloId, // devolve o módulo escolhido para o select continuar marcado
                TotalAlunos = resultado.TotalItems, // total dinâmico: quantos alunos o filtro encontrou (em todas as páginas)
                Paginacao = new PaginacaoViewModel // dados da partial _Paginacao
                { // inicializador da paginação
                    PaginaAtual = resultado.PaginaAtual, // página mostrada (corrigida pelo service se a URL pedia uma inexistente)
                    TotalPaginas = resultado.TotalPaginas, // quantas páginas existem
                    TotalItens = resultado.TotalItems, // quantos alunos no total
                    ItensPorPagina = resultado.TamanhoPagina, // tamanho da página
                    Rotulo = "alunos", // "Mostrando 1–25 de 94 alunos"
                    Rota = RotaDosFiltros(nome, cursoId, moduloId) // os filtros acompanham os links de página
                }, // fim da paginação
                Cursos = cursos // opções do select de curso
                    .OrderBy(c => c.Nome) // ordem alfabética
                    .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome }) // value = id (vai para a query string), texto = nome
                    .ToList(), // fecha como lista
                Modulos = ModulosDoCurso(cursos, cursoId) // opções do select de módulo: só os do curso escolhido
                    .Select(m => new SelectListItem { Value = m.IdModulo.ToString(), Text = m.Nome }) // mesmo formato do select de curso
                    .ToList(), // fecha como lista
                Alunos = resultado.Items.Select(aluno => new AlunosAlunoItem // cada Aluno do domain vira um item da lista
                { // inicializador do item
                    IdAluno = aluno.IdAluno, // para o link de seleção
                    Matricula = aluno.Usuario.Matricula, // a matrícula fica no Usuario
                    Nome = aluno.Usuario.Nome, // o nome também
                    Curso = aluno.Curso.Nome, // nome do curso do aluno
                    ModuloAtual = aluno.ObterModuloAtual()?.Modulo.Nome ?? string.Empty, // regra no domain: o último módulo em que entrou
                    Selecionado = aluno.IdAluno == idSelecionado // destaca o aluno mostrado à direita
                }).ToList(), // fim da lista
                Selecionado = selecionado is null ? null : MontarDetalhe(selecionado) // painel da direita
            }; // fim do ViewModel

            ViewData["MensagemAluno"] = TempData["MensagemAluno"] as string; // aviso de sucesso deixado por uma ação (vira toast na view)
            ViewData["ErroAluno"] = TempData["ErroAluno"] as string; // aviso de erro deixado por uma ação (vira toast de erro)
            return View(viewModel); // entrega o ViewModel à View Alunos/Index
        } // fim da action Index

        // ===================== AÇÕES DO ADMINISTRADOR (POST) =====================
        // Todas recebem, além do id, os filtros e o aluno da tela, para voltar exatamente para onde o administrador estava

        [HttpPost, ValidateAntiForgeryToken] // POST com token: altera dados e não pode ser disparado por outro site
        public Task<IActionResult> Aprovar(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1) // nota >= 7
            => ExecutarAsync(() => _tentativaService.AprovarAsync(idTentativa), "Tentativa aprovada. O módulo foi concluído; o próximo módulo entra pela sincronização com o Moodle.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> Reprovar(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1) // nota < 7
            => ExecutarAsync(() => _tentativaService.ReprovarEAbrirNovaAsync(idTentativa), "Tentativa reprovada. Uma nova tentativa foi aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> ReprovarPorInatividade(int idTentativa, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1) // sem prova
            => ExecutarAsync(() => _tentativaService.ReprovarPorInatividadeEAbrirNovaAsync(idTentativa), "Tentativa reprovada por inatividade. Uma nova tentativa foi aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> AbrirTentativa(int idAlunoModulo, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina = 1) // abertura manual
            => ExecutarAsync(() => _tentativaService.AbrirNovaAsync(idAlunoModulo), "Nova tentativa aberta, com prazo de 40 dias.", alunoId, nome, cursoId, moduloId, pagina);

        // Executa a ação, guarda o aviso para a próxima tela e volta para a página de onde o administrador veio (Post/Redirect/Get)
        private async Task<IActionResult> ExecutarAsync(Func<Task<Tentativa>> acao, string mensagemSucesso, int alunoId, string? nome, int? cursoId, int? moduloId, int pagina)
        {
            try // a regra do domínio recusa a ação quando o estado não permite
            {
                await acao(); // chama o service (que chama a regra e grava)
                TempData["MensagemAluno"] = mensagemSucesso; // toast de sucesso
            } // fim do try
            catch (InvalidOperationException ex) // ação não permitida (ex.: tentativa já fechada por outra aba) ou id inexistente
            {
                TempData["ErroAluno"] = ex.Message; // a mensagem da regra, em português, vira toast de erro
            } // fim do catch

            var rota = RotaDosFiltros(nome, cursoId, moduloId); // os mesmos filtros da tela
            rota["Pagina"] = pagina.ToString(); // a mesma página
            rota["AlunoId"] = alunoId.ToString(); // o mesmo aluno selecionado
            return RedirectToAction(nameof(Index), new RouteValueDictionary(rota.ToDictionary(k => k.Key, v => (object?)v.Value))); // volta para a lista com tudo igual
        } // fim do ExecutarAsync

        [HttpGet] // chamado pelo JavaScript (fetch) quando o curso do filtro muda, para preencher o select de módulo
        public async Task<IActionResult> GetModulosPorCurso(int? cursoId) // cursoId vazio = nenhum módulo
        {
            var cursos = await _cursoService.ListarCursosAsync(); // cursos com os módulos carregados
            return Json(ModulosDoCurso(cursos, cursoId).Select(m => new { value = m.IdModulo, text = m.Nome })); // devolve [{ value, text }], o mesmo formato que o PSG usava na cascata
        } // fim da action GetModulosPorCurso

        // ===================== MONTAGEM DO VIEWMODEL =====================

        private static AlunosDetalheItem MontarDetalhe(Aluno aluno) // aluno do domain (com o histórico carregado) -> painel da direita
        {
            var atual = aluno.ObterModuloAtual(); // regra no domain: o último módulo em que entrou
            var historico = aluno.Modulos // todas as inscrições do aluno
                .OrderByDescending(am => am.DataInscricao) // do mais recente ao mais antigo
                .ThenByDescending(am => am.IdAlunoModulo) // desempate estável
                .Select(am => MontarModulo(am, am == atual)) // cada inscrição vira um item (marcando o atual)
                .ToList(); // fecha como lista

            return new AlunosDetalheItem // painel do aluno
            { // inicializador
                IdAluno = aluno.IdAluno, // id do aluno
                Matricula = aluno.Usuario.Matricula, // matrícula
                Nome = aluno.Usuario.Nome, // nome
                Curso = aluno.Curso.Nome, // curso
                ModulosConcluidos = aluno.Modulos.Count(am => am.Aprovado), // regra no domain: módulo com tentativa aprovada
                TotalModulosCurso = aluno.Curso.Modulos.Count, // quantos módulos o curso tem
                ModuloAtual = historico.FirstOrDefault(m => m.Atual), // o mesmo item do histórico, para não montar duas vezes
                Historico = historico // todos os módulos
            }; // fim do painel
        } // fim do MontarDetalhe

        private static AlunosModuloItem MontarModulo(AlunoModulo inscricao, bool atual) // uma inscrição do domain -> item do histórico
        {
            var hoje = DateTime.Today; // referência para "faltam N dias"
            return new AlunosModuloItem // item do módulo
            { // inicializador
                IdAlunoModulo = inscricao.IdAlunoModulo, // para "Abrir nova tentativa"
                Nome = inscricao.Modulo.Nome, // nome do módulo
                DataInscricao = inscricao.DataInscricao, // quando entrou
                DataFim = inscricao.DataConclusao, // quando concluiu (aprovação)
                NotaFinal = inscricao.ObterNotaFinal(), // regra no domain
                Aprovado = inscricao.Aprovado, // regra no domain
                Atual = atual, // é o módulo atual?
                PodeAbrirTentativa = inscricao.PodeAbrirTentativa, // regra no domain
                TentativasReprovadas = inscricao.Tentativas.Count(t => t.Situacao is Domain.Enum.EnumSituacaoTentativa.Reprovada or Domain.Enum.EnumSituacaoTentativa.ReprovadaPorInatividade), // quantas reprovações
                Tentativas = inscricao.TentativasEmOrdem.Select((t, i) => new AlunosTentativaItem // cada tentativa, na ordem em que foi aberta
                { // inicializador da tentativa
                    IdTentativa = t.IdTentativa, // para as ações
                    Numero = i + 1, // 1ª, 2ª, 3ª...
                    DataInicio = t.DataInicioTentativa, // abertura
                    DataLimite = t.DataLimite, // abertura + prazo (regra no domain)
                    DataFim = t.DataFimTentativa, // fechamento pelo administrador
                    Aberta = t.Aberta, // em curso?
                    PrazoVencido = t.PrazoVencido(DateTime.Now), // regra no domain (só um aviso)
                    DiasParaOPrazo = (t.DataLimite.Date - hoje).Days, // positivo = faltam; negativo = vencido há
                    DataProva = t.DataFimAttempt ?? t.DataInicioAttempt, // quando fez a prova (fim; se ainda em andamento, o início)
                    Nota = t.Nota, // nota da prova
                    Situacao = t.Situacao, // regra no domain
                    PodeAprovar = t.PodeAprovar, // ações permitidas (regra no domain)
                    PodeReprovar = t.PodeReprovar,
                    PodeReprovarPorInatividade = t.PodeReprovarPorInatividade
                }).ToList() // fim das tentativas
            }; // fim do item do módulo
        } // fim do MontarModulo

        private static Dictionary<string, string> RotaDosFiltros(string? nome, int? cursoId, int? moduloId) // filtros ativos, para links e redirecionamentos
        {
            var rota = new Dictionary<string, string>(); // só entra o que estiver preenchido
            if (!string.IsNullOrWhiteSpace(nome)) rota["Nome"] = nome; // texto do filtro
            if (cursoId.HasValue) rota["CursoId"] = cursoId.Value.ToString(); // curso do filtro
            if (moduloId.HasValue) rota["ModuloId"] = moduloId.Value.ToString(); // módulo do filtro
            return rota; // dicionário pronto para asp-all-route-data
        } // fim do RotaDosFiltros

        // Os módulos de um curso em ordem alfabética (lista vazia se nenhum curso foi escolhido)
        private static IEnumerable<Modulo> ModulosDoCurso(IEnumerable<Curso> cursos, int? cursoId) // reaproveitado pela tela e pelo AJAX
        {
            return cursos // parte de todos os cursos
                .Where(c => c.IdCurso == cursoId) // fica só com o curso escolhido (cursoId null não bate com nenhum)
                .SelectMany(c => c.Modulos) // junta os módulos dele
                .OrderBy(m => m.Nome); // ordem alfabética
        } // fim do ModulosDoCurso
    } // fim da classe
} // fim do namespace
