using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using PSGN.Application.Servicos.Alunos; 
using PSGN.Application.Servicos.Cursos; 
using PSGN.Application.Servicos.Modulos; 
using PSGN.Presentation.ViewModels; // REVISÃO: saíram os "using PSGN.Domain" e "PSGN.Domain.Enum", que só serviam à situação das inscrições

namespace PSGN.Presentation.Controllers
{
    public class DashboardController : Controller
    {
        private const int MesesPadrao = 6;
        // REVISÃO: removida a constante NotaMinimaAprovacao (7.0 assumido), que só existia para classificar aprovado/reprovado

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

            // REVISÃO: removidas daqui a busca das inscrições e tudo o que era calculado a partir delas (situação por curso, cursos de maior
            // cancelamento/reprovação e lista de alunos cancelados): a antiga situação das inscrições não existe mais no PSGN
            var viewModel = new DashboardViewModel // REVISÃO: nome da classe corrigido (era DashBoardViewModel); objeto que a View vai exibir
            { // inicializador do ViewModel
                TotalAlunos = await _alunoService.ContarAlunosAsync(), // card "Total de Alunos" (pessoas distintas)
                TotalCursos = cursos.Count, // card "Total de Cursos" (a lista já está na memória, não precisa ir ao banco de novo)
                LineGraphSection = lineGraphSection, // gráfico em linha
                SectorGraphSection = sectorGraphSection // gráfico de pizza
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
        // REVISÃO: removidos MontarSituacaoPorCurso e MaiorTaxa, que só existiam para a antiga situação das inscrições
    } // fim da classe
} // fim do namespace
