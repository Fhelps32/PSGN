using Microsoft.AspNetCore.Mvc; // Controller, IActionResult, [HttpGet], Json
using Microsoft.AspNetCore.Mvc.Rendering; // SelectListItem, usado nos <select> da tela
using PSGN.Application.Servicos.Alunos; // AlunoService
using PSGN.Application.Servicos.Cursos; // CursoService
using PSGN.Application.Servicos.Modulos; // ModuloService
using PSGN.Domain; // AlunoModulo
using PSGN.Domain.Enum; // EnumSituacaoInscricao
using PSGN.Presentation.ViewModels; // DashBoardViewModel e as classes das seções

namespace PSGN.Presentation.Controllers // pasta dos controllers do MVC
{
    // REVISÃO: controller completado com base no DashBoardController do PSG. Os services devolvem entidades do domain (ou números)
    // e é AQUI que elas são transformadas no ViewModel, igual o HomeController faz com o HomeViewModel
    public class DashboardController : Controller // controller da tela de Dashboard
    {
        private const int MesesPadrao = 6; // quantos meses o gráfico em linha mostra ao abrir a página
        private const double NotaMinimaAprovacao = 7.0; // nota mínima para aprovação; ASSUMIDO 7.0, troque aqui se a regra da instituição for outra

        private readonly AlunoService _alunoService; // contagens de alunos e lista de inscrições
        private readonly CursoService _cursoService; // lista de cursos com alunos
        private readonly ModuloService _moduloService; // lista de módulos de um curso com alunos

        public DashboardController( // o ASP.NET entrega os services pelo construtor (registrados em DependencyInjection.cs)
            AlunoService alunoService, // parâmetro do service de alunos
            CursoService cursoService, // parâmetro do service de cursos
            ModuloService moduloService) // parâmetro do service de módulos
        {
            _alunoService = alunoService; // guarda para usar nas actions
            _cursoService = cursoService; // guarda para usar nas actions
            _moduloService = moduloService; // guarda para usar nas actions
        } // fim do construtor

        public async Task<IActionResult> Index() // monta a página inteira na primeira abertura
        {
            var cursos = await _cursoService.ListarCursosComAlunosAsync(); // cursos com a coleção de alunos carregada
            var cursosSelectList = cursos // a mesma lista de cursos alimenta os selects dos dois gráficos
                .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome }) // value = id (vai para o AJAX), texto = nome (aparece na tela)
                .ToList(); // fecha como lista

            var lineGraphValues = new List<ValueLineGraph>(); // pontos do gráfico em linha
            foreach (var mes in UltimosMeses(MesesPadrao)) // percorre os últimos 6 meses, do mais antigo para o atual
                lineGraphValues.Add(new ValueLineGraph // cria um ponto por mês
                { // inicializador do ponto
                    Data = mes, // eixo X: o mês
                    QuantidadeAlunos = await _alunoService.ContarAlunosCadastradosNoMesAsync(mes, null) // eixo Y: alunos cadastrados no mês, de todos os cursos (null)
                }); // fim do ponto

            var lineGraphSection = new LineGraphSection // seção do gráfico em linha
            { // inicializador da seção
                ValueLineGraph = lineGraphValues, // os pontos calculados acima
                Tempo = new List<SelectListItem> // opções do select de período
                { // as mesmas opções do PSG
                    new() { Value = "6", Text = "Últimos 6 meses", Selected = true }, // padrão ao abrir
                    new() { Value = "12", Text = "Últimos 12 meses" }, // um ano
                    new() { Value = "24", Text = "Últimos 24 meses" } // dois anos
                }, // fim das opções de período
                Cursos = cursosSelectList // opções do select de curso
            }; // fim da seção do gráfico em linha

            var sectorGraphSection = new SectorGraphSection // seção do gráfico de pizza
            { // inicializador da seção
                ValueSectorGraph = cursos // uma fatia por curso ao abrir a página
                    .Select(c => new ValueSectorGraph { Curso = c.Nome, QuantidadeAlunos = c.Alunos.Count }) // tamanho da fatia = alunos do curso
                    .ToList(), // fecha como lista
                Cursos = cursosSelectList // opções do select de curso da pizza
            }; // fim da seção do gráfico de pizza

            var inscricoes = await _alunoService.ListarInscricoesAsync(); // todas as inscrições (aluno x módulo) com aluno, módulo, curso e tentativas
            var situacaoInscricoes = MontarSituacaoPorCurso(inscricoes); // aprovados/em andamento/reprovados/cancelados por curso

            var viewModel = new DashBoardViewModel // objeto que a View vai exibir
            { // inicializador do ViewModel
                TotalAlunos = await _alunoService.ContarAlunosAsync(), // card "Total de Alunos" (pessoas distintas)
                TotalCursos = cursos.Count, // card "Total de Cursos" (a lista já está na memória, não precisa ir ao banco de novo)
                LineGraphSection = lineGraphSection, // gráfico em linha
                SectorGraphSection = sectorGraphSection, // gráfico de pizza
                SituacaoInscricoes = situacaoInscricoes, // tabela de situação por curso
                CursoMaiorCancelamento = MaiorTaxa(situacaoInscricoes, s => s.Cancelados), // destaque: curso com maior % de cancelados
                CursoMaiorReprovacao = MaiorTaxa(situacaoInscricoes, s => s.Reprovados), // destaque: curso com maior % de reprovados
                AlunosCancelados = inscricoes // lista "Alunos Cancelados Recentemente"
                    .Where(i => i.ObterSituacao(NotaMinimaAprovacao) == EnumSituacaoInscricao.Cancelado) // só as inscrições canceladas
                    .Select(i => new AlunoCanceladoItem // converte cada inscrição em um item da tela
                    { // inicializador do item
                        Nome = i.Aluno.Usuario.Nome, // nome do aluno
                        Curso = i.Modulo.Curso.Nome, // nome do curso
                        Modulo = i.Modulo.Nome, // nome do módulo
                        DataCancelamento = null // o domain ainda não guarda a data do cancelamento (ver comentário no ViewModel)
                    }) // fim do item
                    .ToList() // fecha como lista
            }; // fim do ViewModel
            return View(viewModel); // entrega o ViewModel à View Dashboard/Index
        } // fim da action Index

        [HttpGet] // chamado pelo JavaScript (fetch) quando o select da pizza muda
        public async Task<IActionResult> GetPieGraphData(int? cursoId) // cursoId vazio = todos os cursos
        {
            if (cursoId is null or 0) // nenhum curso escolhido
            {
                var cursos = await _cursoService.ListarCursosComAlunosAsync(); // busca os cursos com os alunos
                return Json(cursos.Select(c => new // devolve JSON no MESMO formato que o PSG, para o JavaScript ser igual
                { // objeto anônimo: vira { curso, modulo, quantidadeAlunos } no JSON
                    curso = c.Nome, // nome do curso
                    modulo = (string?)null, // sem módulo: a fatia é o curso inteiro
                    quantidadeAlunos = c.Alunos.Count // alunos do curso
                })); // fim do JSON por curso
            } // fim do caso "todos os cursos"

            var modulos = await _moduloService.ListarModulosDoCursoComAlunosAsync(cursoId.Value); // curso escolhido: busca os módulos dele com as inscrições
            return Json(modulos.Select(m => new // uma fatia por módulo
            { // mesmo formato do caso anterior
                curso = m.Curso.Nome, // nome do curso
                modulo = m.Nome, // nome do módulo (a legenda fica "Curso - Módulo")
                quantidadeAlunos = m.Alunos.Count // alunos inscritos no módulo
            })); // fim do JSON por módulo
        } // fim da action GetPieGraphData

        [HttpGet] // chamado pelo JavaScript (fetch) quando o período ou o curso do gráfico em linha muda
        public async Task<IActionResult> GetLineGraphData(int tempo, int? cursoId) // tempo = quantidade de meses; cursoId vazio = todos
        {
            var meses = tempo is > 0 and <= 60 ? tempo : MesesPadrao; // protege contra valores absurdos vindos da URL (no máximo 5 anos)

            var pontos = new List<object>(); // pontos que vão virar JSON
            foreach (var mes in UltimosMeses(meses)) // percorre os meses pedidos, do mais antigo para o atual
                pontos.Add(new // cria o ponto no MESMO formato do PSG: { totalAlunos, dataRegistro }
                { // objeto anônimo
                    totalAlunos = await _alunoService.ContarAlunosCadastradosNoMesAsync(mes, cursoId), // alunos cadastrados no mês (filtrando o curso, se houver)
                    dataRegistro = mes // o mês do ponto
                }); // fim do ponto
            return Json(pontos); // devolve a lista como JSON
        } // fim da action GetLineGraphData

        private static IEnumerable<DateTime> UltimosMeses(int quantidade) // gera os meses em ordem cronológica, terminando no mês atual
        {
            var mesAtual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // dia 1 do mês atual
            for (var i = quantidade - 1; i >= 0; i--) // conta de trás para frente, para o mais antigo sair primeiro
                yield return mesAtual.AddMonths(-i); // devolve um mês por vez (yield = vai entregando sem montar lista)
        } // fim do UltimosMeses

        private static List<SituacaoInscricoesItem> MontarSituacaoPorCurso(List<AlunoModulo> inscricoes) // agrupa as inscrições por curso e conta cada situação
        {
            return inscricoes // parte de todas as inscrições
                .GroupBy(i => i.Modulo.Curso.Nome) // um grupo por curso
                .Select(grupo => // transforma cada grupo em uma linha da tabela
                { // corpo do Select
                    var situacoes = grupo.Select(i => i.ObterSituacao(NotaMinimaAprovacao)).ToList(); // calcula a situação de cada inscrição UMA vez (regra fica no domain)
                    return new SituacaoInscricoesItem // linha da tabela
                    { // inicializador da linha
                        Curso = grupo.Key, // nome do curso (a chave do grupo)
                        Aprovados = situacoes.Count(s => s == EnumSituacaoInscricao.Aprovado), // quantos aprovados
                        EmAndamento = situacoes.Count(s => s == EnumSituacaoInscricao.EmAndamento), // quantos em andamento
                        Reprovados = situacoes.Count(s => s == EnumSituacaoInscricao.Reprovado), // quantos reprovados
                        Cancelados = situacoes.Count(s => s == EnumSituacaoInscricao.Cancelado) // quantos cancelados
                    }; // fim da linha (o Total é calculado sozinho no ViewModel)
                }) // fim do Select
                .OrderBy(s => s.Curso) // ordena pelo nome do curso
                .ToList(); // fecha como lista
        } // fim do MontarSituacaoPorCurso

        private static CursoImportanciaItem? MaiorTaxa(List<SituacaoInscricoesItem> situacoes, Func<SituacaoInscricoesItem, int> quantidade) // acha o curso com a maior % de uma situação (cancelados ou reprovados)
        {
            var maior = situacoes // parte da tabela de situação
                .Where(s => s.Total > 0 && quantidade(s) > 0) // ignora curso sem inscrição (divisão por zero) e curso sem nenhum caso
                .OrderByDescending(s => (double)quantidade(s) / s.Total) // ordena pela proporção, da maior para a menor
                .FirstOrDefault(); // pega a primeira (maior) ou null se nenhum curso tiver casos

            if (maior is null) // nenhum curso com casos
                return null; // a view mostra "Sem dados disponíveis"

            return new CursoImportanciaItem // monta o destaque
            { // inicializador do destaque
                Curso = maior.Curso, // nome do curso
                Taxa = (int)Math.Round(100.0 * quantidade(maior) / maior.Total), // porcentagem arredondada
                QuantidadeAlunosImportancia = quantidade(maior), // quantos cancelados/reprovados
                QuantidadeAlunosTotal = maior.Total // de quantas inscrições no total
            }; // fim do destaque
        } // fim do MaiorTaxa
    } // fim da classe
} // fim do namespace
