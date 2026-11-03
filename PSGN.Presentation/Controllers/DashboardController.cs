using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using PSGN.Application.Servicos.Alunos; 
using PSGN.Application.Servicos.Cursos; 
using PSGN.Application.Servicos.Modulos; 
using PSGN.Domain; 
using PSGN.Domain.Enum; 
using PSGN.Presentation.ViewModels;

namespace PSGN.Presentation.Controllers
{
    public class DashboardController : Controller
    {
        private const int MesesPadrao = 6; 
        private const double NotaMinimaAprovacao = 7.0;

        private readonly AlunoService _alunoService; 
        private readonly CursoService _cursoService; 
        private readonly ModuloService _moduloService; 

        public DashboardController(
            AlunoService alunoService, 
            CursoService cursoService, 
            ModuloService moduloService) 
        {
            _alunoService = alunoService; 
            _cursoService = cursoService;
            _moduloService = moduloService; 
        } 

        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ListarCursosComAlunosAsync();
            var cursosSelectList = cursos
                .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome }) // value = id (vai para o AJAX), texto = nome (aparece na tela)
                .ToList();

            var lineGraphValues = new List<ValueLineGraph>(); 
            foreach (var mes in UltimosMeses(MesesPadrao)) // percorre os últimos 6 meses, do mais antigo para o atual
                lineGraphValues.Add(new ValueLineGraph // cria um ponto por mês
                {
                    Data = mes,
                    QuantidadeAlunos = await _alunoService.ContarAlunosCadastradosNoMesAsync(mes, null) // eixo Y: alunos cadastrados no mês, de todos os cursos (null)
                });

            var lineGraphSection = new LineGraphSection
            {
                ValueLineGraph = lineGraphValues, // os pontos calculados acima
                Tempo = new List<SelectListItem> // opções do select de período
                { 
                    new() { Value = "6", Text = "Últimos 6 meses", Selected = true }, // padrão ao abrir
                    new() { Value = "12", Text = "Últimos 12 meses" },
                    new() { Value = "24", Text = "Últimos 24 meses" }
                },
                Cursos = cursosSelectList // opções do select de curso
            };

            var sectorGraphSection = new SectorGraphSection
            {
                ValueSectorGraph = cursos
                    .Select(c => new ValueSectorGraph { Curso = c.Nome, QuantidadeAlunos = c.Alunos.Count })
                    .ToList(),
                Cursos = cursosSelectList
            };

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
