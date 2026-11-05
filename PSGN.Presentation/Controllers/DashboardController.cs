using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using PSGN.Application.Servicos.Alunos; 
using PSGN.Application.Servicos.Cursos; 
using PSGN.Application.Servicos.Modulos; 
using PSGN.Presentation.ViewModels;

namespace PSGN.Presentation.Controllers
{
    public class DashboardController : Controller
    {
        private const int MesesPadrao = 6;

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

        // Dashboard: indicadores, filtros e os dados iniciais dos dois gráficos
        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ListarCursosComAlunosAsync();
            var cursosSelectList = cursos
                .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome })
                .ToList();

            var lineGraphValues = new List<ValueLineGraph>(); 
            foreach (var mes in UltimosMeses(MesesPadrao))
                lineGraphValues.Add(new ValueLineGraph
                {
                    Data = mes,
                    QuantidadeAlunos = await _alunoService.ContarAlunosCadastradosNoMesAsync(mes, null)
                });

            var lineGraphSection = new LineGraphSection
            {
                ValueLineGraph = lineGraphValues,
                Tempo = new List<SelectListItem>
                { 
                    new() { Value = "6", Text = "Últimos 6 meses", Selected = true },
                    new() { Value = "12", Text = "Últimos 12 meses" },
                    new() { Value = "24", Text = "Últimos 24 meses" }
                },
                Cursos = cursosSelectList
            };

            var sectorGraphSection = new SectorGraphSection
            {
                ValueSectorGraph = cursos
                    .Select(c => new ValueSectorGraph { Curso = c.Nome, QuantidadeAlunos = c.Alunos.Count })
                    .ToList(),
                Cursos = cursosSelectList
            };

            var viewModel = new DashboardViewModel
            {
                TotalAlunos = await _alunoService.ContarAlunosAsync(),
                TotalCursos = cursos.Count,
                LineGraphSection = lineGraphSection,
                SectorGraphSection = sectorGraphSection
            };
            return View(viewModel);
        }

        // Gráfico de pizza: alunos por curso ou, com um curso escolhido, por módulo (fetch do dashboard.js)
        [HttpGet]
        public async Task<IActionResult> GetPieGraphData(int? cursoId)
        {
            if (cursoId is null or 0)
            {
                var cursos = await _cursoService.ListarCursosComAlunosAsync();
                return Json(cursos.Select(c => new
                {
                    curso = c.Nome,
                    modulo = (string?)null,
                    quantidadeAlunos = c.Alunos.Count
                }));
            }

            var modulos = await _moduloService.ListarModulosDoCursoComAlunosAsync(cursoId.Value);
            return Json(modulos.Select(m => new
            {
                curso = m.Curso.Nome,
                modulo = m.Nome,
                quantidadeAlunos = m.Alunos.Count
            }));
        }

        // Gráfico de linha: alunos cadastrados por mês nos últimos N meses (fetch do dashboard.js)
        [HttpGet]
        public async Task<IActionResult> GetLineGraphData(int tempo, int? cursoId)
        {
            var meses = tempo is > 0 and <= 60 ? tempo : MesesPadrao;

            var pontos = new List<object>();
            foreach (var mes in UltimosMeses(meses))
                pontos.Add(new
                {
                    totalAlunos = await _alunoService.ContarAlunosCadastradosNoMesAsync(mes, cursoId),
                    dataRegistro = mes
                });
            return Json(pontos);
        }

        // Primeiro dia de cada um dos últimos N meses, do mais antigo ao atual
        private static IEnumerable<DateTime> UltimosMeses(int quantidade)
        {
            var mesAtual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            for (var i = quantidade - 1; i >= 0; i--)
                yield return mesAtual.AddMonths(-i);
        }
    }
}
