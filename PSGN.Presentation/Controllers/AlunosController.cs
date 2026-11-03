using Microsoft.AspNetCore.Mvc; // Controller, IActionResult, [HttpGet], Json
using Microsoft.AspNetCore.Mvc.Rendering; // SelectListItem, usado nos <select> dos filtros
using PSGN.Application.Servicos.Alunos; // AlunoService
using PSGN.Application.Servicos.Cursos; // CursoService
using PSGN.Domain; // Curso, usado no método auxiliar
using PSGN.Presentation.ViewModels; // AlunosViewModel e os itens da tela

namespace PSGN.Presentation.Controllers // pasta dos controllers do MVC
{
    // REVISÃO: controller completado. Segue a arquitetura do PSGN: os services devolvem entidades do domain e é AQUI que elas viram ViewModel
    public class AlunosController : Controller // controller da tela de Alunos
    {
        private readonly AlunoService _alunoService; // lista os alunos já filtrados
        private readonly CursoService _cursoService; // fornece os cursos (e os módulos deles) para os selects dos filtros

        public AlunosController(AlunoService alunoService, CursoService cursoService) // o ASP.NET entrega os services pelo construtor (registrados em DependencyInjection.cs)
        {
            _alunoService = alunoService; // guarda para usar nas actions
            _cursoService = cursoService; // guarda para usar nas actions
        } // fim do construtor

        private const int AlunosPorPagina = 25; // REVISÃO: tamanho da página da tabela

        // Os parâmetros vêm da query string (GET): /Alunos?Nome=ana&CursoId=3&ModuloId=12&OrdenarPor=curso&Decrescente=true&Pagina=2
        public async Task<IActionResult> Index(string? nome, int? cursoId, int? moduloId, string? ordenarPor, bool decrescente, int pagina = 1) // sem parâmetros = primeira página, ordenada por nome
        {
            ordenarPor = string.Equals(ordenarPor, "curso", StringComparison.OrdinalIgnoreCase) ? "curso" : "nome"; // REVISÃO: só duas colunas são ordenáveis; qualquer outro valor da URL vira "nome"
            var resultado = await _alunoService.ListarAlunosAsync(nome, cursoId, moduloId, ordenarPor, decrescente, pagina, AlunosPorPagina); // REVISÃO: entidades do domain (uma página), já filtradas e ordenadas pelo service
            var alunos = resultado.Items; // REVISÃO: os alunos desta página
            var cursos = await _cursoService.ListarCursosAsync(); // cursos com os módulos carregados (os mesmos que a Home usa)

            var rota = new Dictionary<string, string>(); // REVISÃO: filtros e ordenação ativos; cada link da paginação os leva junto para não se perderem
            if (!string.IsNullOrWhiteSpace(nome)) rota["Nome"] = nome; // texto do filtro
            if (cursoId.HasValue) rota["CursoId"] = cursoId.Value.ToString(); // curso do filtro
            if (moduloId.HasValue) rota["ModuloId"] = moduloId.Value.ToString(); // módulo do filtro
            if (ordenarPor != "nome") rota["OrdenarPor"] = ordenarPor; // só vai na URL quando não é o padrão (mantém os links curtos)
            if (decrescente) rota["Decrescente"] = "true"; // sentido Z-A

            var viewModel = new AlunosViewModel // objeto que a View vai exibir
            { // inicializador do ViewModel
                Nome = nome, // devolve o texto digitado para o campo continuar preenchido
                CursoId = cursoId, // devolve o curso escolhido para o select continuar marcado
                ModuloId = moduloId, // devolve o módulo escolhido para o select continuar marcado
                OrdenarPor = ordenarPor, // REVISÃO: coluna ordenada, para a view marcar o cabeçalho
                Decrescente = decrescente, // REVISÃO: sentido da ordenação
                TotalAlunos = resultado.TotalItems, // total dinâmico: quantos alunos o filtro encontrou (em TODAS as páginas, não só nesta)
                Paginacao = new PaginacaoViewModel // REVISÃO: dados da partial _Paginacao
                { // inicializador da paginação
                    PaginaAtual = resultado.PaginaAtual, // página mostrada (já corrigida pelo service se a URL pedia uma inexistente)
                    TotalPaginas = resultado.TotalPaginas, // quantas páginas existem
                    TotalItens = resultado.TotalItems, // quantos alunos no total
                    ItensPorPagina = resultado.TamanhoPagina, // tamanho da página
                    Rotulo = "alunos", // "Mostrando 1–25 de 94 alunos"
                    Rota = rota // filtros e ordenação que acompanham os links
                }, // fim da paginação
                Cursos = cursos // opções do select de curso
                    .OrderBy(c => c.Nome) // ordem alfabética
                    .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome }) // value = id (vai para a query string), texto = nome
                    .ToList(), // fecha como lista
                Modulos = ModulosDoCurso(cursos, cursoId) // opções do select de módulo: só os do curso escolhido
                    .Select(m => new SelectListItem { Value = m.IdModulo.ToString(), Text = m.Nome }) // mesmo formato do select de curso
                    .ToList(), // fecha como lista
                Alunos = alunos.Select(aluno => new AlunosAlunoItem // converte cada Aluno do domain em um item da tabela
                { // inicializador do item
                    Matricula = aluno.Usuario.Matricula, // a matrícula fica no Usuario
                    Nome = aluno.Usuario.Nome, // o nome também
                    Curso = aluno.Curso.Nome, // nome do curso do aluno
                    ModuloAtual = aluno.ObterModuloAtual()?.Modulo.Nome ?? string.Empty, // regra no domain: inscrição mais recente sem DataConclusao (vazio se não houver)
                    Modulos = aluno.Modulos // parte das inscrições do aluno
                        .OrderBy(am => am.Modulo.Nome) // ordem alfabética dos módulos
                        .Select(am => new AlunosModuloItem // converte cada inscrição em um item da lista de módulos
                        { // inicializador do módulo
                            Nome = am.Modulo.Nome, // nome do módulo
                            DataInscricao = am.DataInscricao, // quando o aluno foi inscrito
                            DataFim = am.DataConclusao, // quando concluiu (null enquanto estiver em aberto)
                            NotaFinal = am.ObterNotaFinal() // regra no domain: nota do questionário final em Tentativa.Nota (null se não houver)
                        }) // fim do módulo
                        .ToList() // fecha como lista
                }).ToList() // fim do item e da lista de alunos
            }; // fim do ViewModel

            return View(viewModel); // entrega o ViewModel à View Alunos/Index
        } // fim da action Index

        [HttpGet] // chamado pelo JavaScript (fetch) quando o curso do filtro muda, para preencher o select de módulo
        public async Task<IActionResult> GetModulosPorCurso(int? cursoId) // cursoId vazio = nenhum módulo
        {
            var cursos = await _cursoService.ListarCursosAsync(); // cursos com os módulos carregados
            return Json(ModulosDoCurso(cursos, cursoId).Select(m => new { value = m.IdModulo, text = m.Nome })); // devolve [{ value, text }], o mesmo formato que o PSG usava na cascata
        } // fim da action GetModulosPorCurso

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
